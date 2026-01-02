# 🔐 SPRINT 10 - Admin Panel & Dealer Management

**Fecha:** 2 Enero 2026  
**Duración estimada:** 4 horas  
**Tokens estimados:** ~22,000  
**Prioridad:** 🟠 Alta

---

## 🎯 OBJETIVOS

1. Crear panel de administración para dealers
2. Implementar flujo de verificación de dealers
3. Sistema de aprobación de vehículos
4. Dashboard con métricas y estadísticas
5. Gestión de usuarios y permisos
6. Herramientas de moderación

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: Backend - AdminService (1.5 horas)

- [ ] 1.1. Crear AdminService microservicio
- [ ] 1.2. Implementar DealerVerification workflow
- [ ] 1.3. Crear VehicleApproval system
- [ ] 1.4. Implementar AdminController
- [ ] 1.5. Agregar audit logs

### Fase 2: Frontend - Admin Dashboard (2 horas)

- [ ] 2.1. Crear AdminLayout
- [ ] 2.2. Dashboard con estadísticas
- [ ] 2.3. Gestión de dealers
- [ ] 2.4. Aprobación de vehículos
- [ ] 2.5. Logs de auditoría

### Fase 3: Permisos y Roles (30 min)

- [ ] 3.1. Configurar roles de admin
- [ ] 3.2. Implementar middleware de autorización
- [ ] 3.3. Proteger rutas de admin

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Backend - AdminService Entities

**Archivo:** `backend/AdminService/AdminService.Domain/Entities/DealerVerification.cs`

```csharp
namespace AdminService.Domain.Entities;

public class DealerVerification
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid UserId { get; set; }
    
    public string CompanyName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string BusinessLicenseUrl { get; set; } = string.Empty;
    public string AddressProofUrl { get; set; } = string.Empty;
    
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public string? RejectionReason { get; set; }
    
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum VerificationStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    RequiresMoreInfo
}

public class VehicleApproval
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DealerId { get; set; }
    
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public string? RejectionReason { get; set; }
    
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}
```

---

### 2️⃣ Backend - Admin Controller

**Archivo:** `backend/AdminService/AdminService.Api/Controllers/AdminController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;

namespace AdminService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Dashboard Stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var query = new GetAdminStatsQuery();
        var result = await _mediator.Send(query);
        return Ok(result.Value);
    }

    // Dealer Verifications
    [HttpGet("dealer-verifications")]
    public async Task<IActionResult> GetDealerVerifications(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetDealerVerificationsQuery(status, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result.Value);
    }

    [HttpPost("dealer-verifications/{id}/approve")]
    public async Task<IActionResult> ApproveDealer(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        var command = new ApproveDealerCommand(id, adminId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("dealer-verifications/{id}/reject")]
    public async Task<IActionResult> RejectDealer(
        Guid id,
        [FromBody] RejectDealerRequest request)
    {
        var adminId = Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        var command = new RejectDealerCommand(id, adminId, request.Reason);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    // Vehicle Approvals
    [HttpGet("vehicle-approvals")]
    public async Task<IActionResult> GetVehicleApprovals(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetVehicleApprovalsQuery(status, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result.Value);
    }

    [HttpPost("vehicle-approvals/{id}/approve")]
    public async Task<IActionResult> ApproveVehicle(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        var command = new ApproveVehicleCommand(id, adminId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("vehicle-approvals/{id}/reject")]
    public async Task<IActionResult> RejectVehicle(
        Guid id,
        [FromBody] RejectVehicleRequest request)
    {
        var adminId = Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        var command = new RejectVehicleCommand(id, adminId, request.Reason);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    // Users Management
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetUsersQuery(search, role, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result.Value);
    }

    [HttpPut("users/{id}/suspend")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserRequest request)
    {
        var command = new SuspendUserCommand(id, request.Reason);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPut("users/{id}/activate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var command = new ActivateUserCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}

public record RejectDealerRequest(string Reason);
public record RejectVehicleRequest(string Reason);
public record SuspendUserRequest(string Reason);
```

---

### 3️⃣ Frontend - Admin Dashboard

**Archivo:** `frontend/web/original/src/pages/admin/AdminDashboard.tsx`

```typescript
import { type FC } from 'react';
import { useQuery } from '@tanstack/react-query';
import { 
  Users, 
  Car, 
  CheckCircle, 
  XCircle, 
  Clock,
  TrendingUp 
} from 'lucide-react';
import { api } from '@/services/api';

interface AdminStats {
  totalUsers: number;
  activeUsers: number;
  totalVehicles: number;
  pendingApprovals: number;
  approvedVehicles: number;
  rejectedVehicles: number;
  totalDealers: number;
  pendingVerifications: number;
  revenue: number;
  revenueGrowth: number;
}

export const AdminDashboard: FC = () => {
  const { data: stats, isLoading } = useQuery({
    queryKey: ['admin-stats'],
    queryFn: async () => {
      const response = await api.get<AdminStats>('/admin/stats');
      return response.data;
    },
  });

  if (isLoading) {
    return <div>Cargando...</div>;
  }

  const statCards = [
    {
      title: 'Total Usuarios',
      value: stats?.totalUsers || 0,
      change: '+12%',
      icon: Users,
      color: 'blue',
    },
    {
      title: 'Total Vehículos',
      value: stats?.totalVehicles || 0,
      change: '+8%',
      icon: Car,
      color: 'green',
    },
    {
      title: 'Aprobaciones Pendientes',
      value: stats?.pendingApprovals || 0,
      change: '',
      icon: Clock,
      color: 'yellow',
    },
    {
      title: 'Dealers Verificados',
      value: stats?.totalDealers || 0,
      change: '+5%',
      icon: CheckCircle,
      color: 'purple',
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">Panel de Administración</h1>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {statCards.map((stat) => (
          <div
            key={stat.title}
            className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-lg 
                     transition-shadow"
          >
            <div className="flex items-center justify-between mb-4">
              <div className={`p-3 bg-${stat.color}-100 rounded-lg`}>
                <stat.icon className={`w-6 h-6 text-${stat.color}-600`} />
              </div>
              {stat.change && (
                <span className="text-sm text-green-600 flex items-center gap-1">
                  <TrendingUp className="w-4 h-4" />
                  {stat.change}
                </span>
              )}
            </div>
            <h3 className="text-2xl font-bold text-gray-900 mb-1">
              {stat.value.toLocaleString()}
            </h3>
            <p className="text-sm text-gray-600">{stat.title}</p>
          </div>
        ))}
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Verificaciones Pendientes
          </h3>
          <div className="space-y-3">
            <QuickActionButton
              href="/admin/dealer-verifications?status=pending"
              count={stats?.pendingVerifications || 0}
              label="Dealers por verificar"
            />
            <QuickActionButton
              href="/admin/vehicle-approvals?status=pending"
              count={stats?.pendingApprovals || 0}
              label="Vehículos por aprobar"
            />
          </div>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Estadísticas de Aprobación
          </h3>
          <div className="space-y-4">
            <StatBar
              label="Aprobados"
              value={stats?.approvedVehicles || 0}
              total={stats?.totalVehicles || 1}
              color="green"
            />
            <StatBar
              label="Rechazados"
              value={stats?.rejectedVehicles || 0}
              total={stats?.totalVehicles || 1}
              color="red"
            />
          </div>
        </div>
      </div>
    </div>
  );
};

const QuickActionButton: FC<{ href: string; count: number; label: string }> = ({
  href,
  count,
  label,
}) => (
  <a
    href={href}
    className="flex items-center justify-between p-4 bg-gray-50 hover:bg-gray-100 
             rounded-lg transition-colors"
  >
    <span className="text-gray-700">{label}</span>
    <span className="px-3 py-1 bg-blue-600 text-white rounded-full text-sm font-medium">
      {count}
    </span>
  </a>
);

const StatBar: FC<{ label: string; value: number; total: number; color: string }> = ({
  label,
  value,
  total,
  color,
}) => {
  const percentage = (value / total) * 100;

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <span className="text-sm text-gray-600">{label}</span>
        <span className="text-sm font-medium text-gray-900">
          {value} ({percentage.toFixed(1)}%)
        </span>
      </div>
      <div className="w-full bg-gray-200 rounded-full h-2">
        <div
          className={`bg-${color}-600 h-2 rounded-full transition-all`}
          style={{ width: `${percentage}%` }}
        />
      </div>
    </div>
  );
};
```

---

### 4️⃣ Frontend - Dealer Verifications Page

**Archivo:** `frontend/web/original/src/pages/admin/DealerVerificationsPage.tsx`

```typescript
import { useState, type FC } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { CheckCircle, XCircle, Eye, FileText } from 'lucide-react';
import { api } from '@/services/api';
import toast from 'react-hot-toast';

interface DealerVerification {
  id: string;
  dealerId: string;
  userId: string;
  companyName: string;
  taxId: string;
  businessLicenseUrl: string;
  addressProofUrl: string;
  status: 'Pending' | 'UnderReview' | 'Approved' | 'Rejected';
  createdAt: string;
}

export const DealerVerificationsPage: FC = () => {
  const queryClient = useQueryClient();
  const [statusFilter, setStatusFilter] = useState<string>('Pending');

  const { data: verifications = [], isLoading } = useQuery({
    queryKey: ['dealer-verifications', statusFilter],
    queryFn: async () => {
      const response = await api.get<DealerVerification[]>(
        `/admin/dealer-verifications?status=${statusFilter}`
      );
      return response.data;
    },
  });

  const approveMutation = useMutation({
    mutationFn: (id: string) => api.post(`/admin/dealer-verifications/${id}/approve`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dealer-verifications'] });
      toast.success('Dealer aprobado');
    },
  });

  const rejectMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      api.post(`/admin/dealer-verifications/${id}/reject`, { reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dealer-verifications'] });
      toast.success('Dealer rechazado');
    },
  });

  const handleReject = (id: string) => {
    const reason = prompt('Motivo del rechazo:');
    if (reason) {
      rejectMutation.mutate({ id, reason });
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">Verificación de Dealers</h1>
        
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-lg"
        >
          <option value="Pending">Pendientes</option>
          <option value="UnderReview">En Revisión</option>
          <option value="Approved">Aprobados</option>
          <option value="Rejected">Rechazados</option>
        </select>
      </div>

      {isLoading ? (
        <div>Cargando...</div>
      ) : verifications.length === 0 ? (
        <div className="text-center py-12 text-gray-500">
          No hay verificaciones {statusFilter.toLowerCase()}
        </div>
      ) : (
        <div className="grid gap-6">
          {verifications.map((verification) => (
            <div
              key={verification.id}
              className="bg-white border border-gray-200 rounded-lg p-6"
            >
              <div className="flex items-start justify-between mb-4">
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 mb-2">
                    {verification.companyName}
                  </h3>
                  <p className="text-sm text-gray-600">
                    Tax ID: {verification.taxId}
                  </p>
                </div>
                <span
                  className={`px-3 py-1 rounded-full text-sm font-medium ${
                    verification.status === 'Approved'
                      ? 'bg-green-100 text-green-800'
                      : verification.status === 'Rejected'
                      ? 'bg-red-100 text-red-800'
                      : 'bg-yellow-100 text-yellow-800'
                  }`}
                >
                  {verification.status}
                </span>
              </div>

              <div className="flex gap-4 mb-4">
                <a
                  href={verification.businessLicenseUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 text-blue-600 hover:text-blue-700"
                >
                  <FileText className="w-4 h-4" />
                  Licencia de Negocio
                </a>
                <a
                  href={verification.addressProofUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 text-blue-600 hover:text-blue-700"
                >
                  <FileText className="w-4 h-4" />
                  Comprobante de Domicilio
                </a>
              </div>

              {verification.status === 'Pending' && (
                <div className="flex gap-3">
                  <button
                    onClick={() => approveMutation.mutate(verification.id)}
                    disabled={approveMutation.isPending}
                    className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white 
                             rounded-lg hover:bg-green-700 disabled:opacity-50"
                  >
                    <CheckCircle className="w-5 h-5" />
                    Aprobar
                  </button>
                  <button
                    onClick={() => handleReject(verification.id)}
                    disabled={rejectMutation.isPending}
                    className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white 
                             rounded-lg hover:bg-red-700 disabled:opacity-50"
                  >
                    <XCircle className="w-5 h-5" />
                    Rechazar
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

1. Admin puede ver estadísticas generales
2. Sistema de verificación de dealers funcional
3. Aprobación/rechazo de vehículos operacional
4. Solo admins pueden acceder al panel
5. Logs de auditoría registran todas las acciones

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| AdminService entities | 3,000 |
| Admin controller | 5,000 |
| Admin dashboard | 5,000 |
| Verifications page | 5,000 |
| Permissions | 2,000 |
| Testing | 2,000 |
| **TOTAL** | **~22,000** |

---

## ➡️ PRÓXIMO SPRINT

**Sprint 11:** Testing & QA

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026

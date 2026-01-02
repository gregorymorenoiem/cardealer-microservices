# 💾 SPRINT 9 - Saved Searches & Alerts

**Fecha:** 2 Enero 2026  
**Duración estimada:** 2-3 horas  
**Tokens estimados:** ~18,000  
**Prioridad:** 🟢 Baja

---

## 🎯 OBJETIVOS

1. Permitir guardar búsquedas personalizadas
2. Crear sistema de alertas automáticas
3. Notificar cuando hay nuevas coincidencias
4. Gestionar búsquedas guardadas
5. Implementar UI para gestión de alertas

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: Backend - Saved Searches (1 hora)

- [ ] 1.1. Crear entidad SavedSearch
- [ ] 1.2. Implementar SavedSearchController
- [ ] 1.3. Crear job para verificar coincidencias
- [ ] 1.4. Integrar con NotificationService

### Fase 2: Frontend - Saved Searches UI (1.5 horas)

- [ ] 2.1. Crear botón "Guardar búsqueda"
- [ ] 2.2. Implementar modal de configuración
- [ ] 2.3. Crear página de búsquedas guardadas
- [ ] 2.4. Agregar gestión de alertas

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Backend - SavedSearch Entity

**Archivo:** `backend/SearchService/SearchService.Domain/Entities/SavedSearch.cs`

```csharp
namespace SearchService.Domain.Entities;

public class SavedSearch
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public string? BodyType { get; set; }
    public string? Location { get; set; }
    
    public bool AlertsEnabled { get; set; }
    public AlertFrequency AlertFrequency { get; set; } = AlertFrequency.Daily;
    public DateTime? LastAlertSent { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum AlertFrequency
{
    Immediate,
    Daily,
    Weekly
}
```

---

### 2️⃣ Backend - SavedSearches Controller

**Archivo:** `backend/SearchService/SearchService.Api/Controllers/SavedSearchesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedSearchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SavedSearchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        var query = new GetSavedSearchesQuery(userId);
        var result = await _mediator.Send(query);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSavedSearchRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        
        var command = new CreateSavedSearchCommand
        {
            UserId = userId,
            Name = request.Name,
            Make = request.Make,
            Model = request.Model,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            MinYear = request.MinYear,
            MaxYear = request.MaxYear,
            AlertsEnabled = request.AlertsEnabled,
            AlertFrequency = request.AlertFrequency
        };

        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSavedSearchByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSavedSearchRequest request)
    {
        var command = new UpdateSavedSearchCommand
        {
            Id = id,
            Name = request.Name,
            AlertsEnabled = request.AlertsEnabled,
            AlertFrequency = request.AlertFrequency
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteSavedSearchCommand(id);
        await _mediator.Send(command);

        return NoContent();
    }

    [HttpGet("{id}/matches")]
    public async Task<IActionResult> GetMatches(Guid id)
    {
        var query = new GetSavedSearchMatchesQuery(id);
        var result = await _mediator.Send(query);

        return Ok(result.Value);
    }
}
```

---

### 3️⃣ Frontend - SavedSearches Page

**Archivo:** `frontend/web/original/src/pages/SavedSearchesPage.tsx`

```typescript
import { useState, type FC } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Bell, BellOff, Trash2, Edit2 } from 'lucide-react';
import { api } from '@/services/api';
import toast from 'react-hot-toast';

interface SavedSearch {
  id: string;
  name: string;
  make?: string;
  model?: string;
  minPrice?: number;
  maxPrice?: number;
  minYear?: number;
  maxYear?: number;
  alertsEnabled: boolean;
  alertFrequency: 'Immediate' | 'Daily' | 'Weekly';
  createdAt: string;
}

export const SavedSearchesPage: FC = () => {
  const queryClient = useQueryClient();

  const { data: searches = [], isLoading } = useQuery({
    queryKey: ['saved-searches'],
    queryFn: async () => {
      const response = await api.get<SavedSearch[]>('/saved-searches');
      return response.data;
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/saved-searches/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['saved-searches'] });
      toast.success('Búsqueda eliminada');
    },
  });

  const toggleAlertMutation = useMutation({
    mutationFn: ({ id, enabled }: { id: string; enabled: boolean }) =>
      api.put(`/saved-searches/${id}`, { alertsEnabled: enabled }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['saved-searches'] });
      toast.success('Alertas actualizadas');
    },
  });

  const getFilterSummary = (search: SavedSearch) => {
    const filters = [];
    if (search.make) filters.push(search.make);
    if (search.model) filters.push(search.model);
    if (search.minPrice || search.maxPrice) {
      const price = `$${search.minPrice || 0} - $${search.maxPrice || '∞'}`;
      filters.push(price);
    }
    if (search.minYear || search.maxYear) {
      const year = `${search.minYear || ''}-${search.maxYear || ''}`;
      filters.push(year);
    }
    return filters.join(' • ');
  };

  if (isLoading) {
    return <div>Cargando...</div>;
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Búsquedas Guardadas</h1>
        <button
          onClick={() => window.location.href = '/vehicles'}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white 
                   rounded-lg hover:bg-blue-700"
        >
          <Plus className="w-5 h-5" />
          Nueva Búsqueda
        </button>
      </div>

      {searches.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 mb-4">
            No tienes búsquedas guardadas aún
          </p>
          <button
            onClick={() => window.location.href = '/vehicles'}
            className="text-blue-600 hover:text-blue-700"
          >
            Explorar vehículos
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          {searches.map((search) => (
            <div
              key={search.id}
              className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-md 
                       transition-shadow"
            >
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="text-lg font-semibold text-gray-900">
                      {search.name}
                    </h3>
                    {search.alertsEnabled && (
                      <span className="px-2 py-1 bg-green-100 text-green-800 text-xs 
                                     rounded-full flex items-center gap-1">
                        <Bell className="w-3 h-3" />
                        Alertas activas
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-gray-600 mb-3">
                    {getFilterSummary(search)}
                  </p>
                  {search.alertsEnabled && (
                    <p className="text-xs text-gray-500">
                      Frecuencia: {search.alertFrequency === 'Immediate' ? 'Inmediata' : 
                                   search.alertFrequency === 'Daily' ? 'Diaria' : 'Semanal'}
                    </p>
                  )}
                </div>

                <div className="flex items-center gap-2">
                  <button
                    onClick={() => toggleAlertMutation.mutate({
                      id: search.id,
                      enabled: !search.alertsEnabled
                    })}
                    className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 
                             rounded-lg transition-colors"
                    title={search.alertsEnabled ? 'Desactivar alertas' : 'Activar alertas'}
                  >
                    {search.alertsEnabled ? (
                      <BellOff className="w-5 h-5" />
                    ) : (
                      <Bell className="w-5 h-5" />
                    )}
                  </button>

                  <button
                    onClick={() => deleteMutation.mutate(search.id)}
                    className="p-2 text-red-600 hover:text-red-700 hover:bg-red-50 
                             rounded-lg transition-colors"
                    title="Eliminar"
                  >
                    <Trash2 className="w-5 h-5" />
                  </button>
                </div>
              </div>

              <div className="mt-4 flex gap-2">
                <button
                  onClick={() => {
                    // Redirect to search with filters
                    const params = new URLSearchParams();
                    if (search.make) params.append('make', search.make);
                    if (search.model) params.append('model', search.model);
                    if (search.minPrice) params.append('minPrice', search.minPrice.toString());
                    if (search.maxPrice) params.append('maxPrice', search.maxPrice.toString());
                    window.location.href = `/vehicles?${params.toString()}`;
                  }}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
                           text-sm"
                >
                  Ver Resultados
                </button>
              </div>
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

1. Usuario puede guardar búsqueda actual
2. Búsquedas se listan correctamente
3. Alertas se activan/desactivan
4. Usuario recibe notificaciones de nuevos matches
5. Búsquedas pueden editarse y eliminarse

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| SavedSearch entity | 2,000 |
| SavedSearches controller | 4,000 |
| Alert job | 3,000 |
| Frontend page | 6,000 |
| Testing | 3,000 |
| **TOTAL** | **~18,000** |

---

## ➡️ PRÓXIMO SPRINT

**Sprint 10:** Admin Panel

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026

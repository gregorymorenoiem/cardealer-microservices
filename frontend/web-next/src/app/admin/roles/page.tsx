/**
 * Admin Roles Page
 *
 * Role and permission management
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  Shield,
  Users,
  Plus,
  Edit,
  Trash2,
  Check,
  X,
  ChevronDown,
  ChevronRight,
} from 'lucide-react';

const roles = [
  {
    id: '1',
    name: 'Super Admin',
    description: 'Acceso completo a todas las funcionalidades',
    users: 2,
    color: 'bg-purple-100 text-purple-700',
    permissions: ['all'],
    isSystem: true,
  },
  {
    id: '2',
    name: 'Admin',
    description: 'Gestión de usuarios, vehículos y dealers',
    users: 5,
    color: 'bg-blue-100 text-blue-700',
    permissions: [
      'users.read',
      'users.write',
      'vehicles.read',
      'vehicles.write',
      'dealers.read',
      'dealers.write',
      'moderation',
    ],
    isSystem: true,
  },
  {
    id: '3',
    name: 'Moderador',
    description: 'Revisión y aprobación de contenido',
    users: 8,
    color: 'bg-emerald-100 text-emerald-700',
    permissions: ['vehicles.read', 'vehicles.moderate', 'moderation', 'reports.read'],
    isSystem: false,
  },
  {
    id: '4',
    name: 'Soporte',
    description: 'Atención al cliente y soporte técnico',
    users: 12,
    color: 'bg-amber-100 text-amber-700',
    permissions: ['users.read', 'tickets.read', 'tickets.write', 'messages'],
    isSystem: false,
  },
  {
    id: '5',
    name: 'Analista',
    description: 'Acceso a reportes y analytics',
    users: 3,
    color: 'bg-gray-100 text-gray-700',
    permissions: ['analytics.read', 'reports.read'],
    isSystem: false,
  },
];

const permissionGroups = [
  {
    name: 'Usuarios',
    permissions: [
      { id: 'users.read', label: 'Ver usuarios', description: 'Ver lista y detalles de usuarios' },
      {
        id: 'users.write',
        label: 'Editar usuarios',
        description: 'Modificar información de usuarios',
      },
      {
        id: 'users.delete',
        label: 'Eliminar usuarios',
        description: 'Eliminar cuentas de usuarios',
      },
      { id: 'users.ban', label: 'Suspender usuarios', description: 'Suspender o banear usuarios' },
    ],
  },
  {
    name: 'Vehículos',
    permissions: [
      {
        id: 'vehicles.read',
        label: 'Ver vehículos',
        description: 'Ver lista y detalles de vehículos',
      },
      { id: 'vehicles.write', label: 'Editar vehículos', description: 'Modificar publicaciones' },
      { id: 'vehicles.delete', label: 'Eliminar vehículos', description: 'Eliminar publicaciones' },
      {
        id: 'vehicles.moderate',
        label: 'Moderar vehículos',
        description: 'Aprobar o rechazar publicaciones',
      },
    ],
  },
  {
    name: 'Dealers',
    permissions: [
      { id: 'dealers.read', label: 'Ver dealers', description: 'Ver lista y detalles de dealers' },
      {
        id: 'dealers.write',
        label: 'Editar dealers',
        description: 'Modificar información de dealers',
      },
      { id: 'dealers.verify', label: 'Verificar dealers', description: 'Aprobar nuevos dealers' },
    ],
  },
  {
    name: 'Reportes y Analytics',
    permissions: [
      { id: 'reports.read', label: 'Ver reportes', description: 'Ver reportes de usuarios' },
      {
        id: 'analytics.read',
        label: 'Ver analytics',
        description: 'Acceso al dashboard de analytics',
      },
    ],
  },
  {
    name: 'Configuración',
    permissions: [
      { id: 'settings.read', label: 'Ver configuración', description: 'Ver ajustes del sistema' },
      {
        id: 'settings.write',
        label: 'Editar configuración',
        description: 'Modificar ajustes del sistema',
      },
    ],
  },
];

export default function AdminRolesPage() {
  const [selectedRole, setSelectedRole] = useState<string | null>(null);
  const [expandedGroups, setExpandedGroups] = useState<string[]>(['Usuarios', 'Vehículos']);

  const toggleGroup = (groupName: string) => {
    setExpandedGroups(prev =>
      prev.includes(groupName) ? prev.filter(g => g !== groupName) : [...prev, groupName]
    );
  };

  const selectedRoleData = roles.find(r => r.id === selectedRole);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Roles y Permisos</h1>
          <p className="text-gray-600">Gestión de roles de acceso del sistema</p>
        </div>
        <Button className="bg-slate-900 hover:bg-slate-800">
          <Plus className="mr-2 h-4 w-4" />
          Nuevo Rol
        </Button>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Roles List */}
        <div className="space-y-4 lg:col-span-1">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-lg">
                <Shield className="h-5 w-5" />
                Roles ({roles.length})
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {roles.map(role => (
                <button
                  key={role.id}
                  onClick={() => setSelectedRole(role.id)}
                  className={`w-full rounded-lg p-3 text-left transition-colors ${
                    selectedRole === role.id ? 'bg-slate-900 text-white' : 'hover:bg-gray-50'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">{role.name}</p>
                      <p
                        className={`text-xs ${selectedRole === role.id ? 'text-gray-300' : 'text-gray-500'}`}
                      >
                        {role.users} usuarios
                      </p>
                    </div>
                    {role.isSystem && (
                      <Badge
                        variant={selectedRole === role.id ? 'secondary' : 'outline'}
                        className="text-xs"
                      >
                        Sistema
                      </Badge>
                    )}
                  </div>
                </button>
              ))}
            </CardContent>
          </Card>
        </div>

        {/* Role Details */}
        <div className="lg:col-span-2">
          {selectedRoleData ? (
            <Card>
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div>
                    <CardTitle className="flex items-center gap-2">
                      <Badge className={selectedRoleData.color}>{selectedRoleData.name}</Badge>
                      {selectedRoleData.isSystem && (
                        <Badge variant="outline" className="text-xs">
                          Sistema
                        </Badge>
                      )}
                    </CardTitle>
                    <CardDescription className="mt-1">
                      {selectedRoleData.description}
                    </CardDescription>
                  </div>
                  {!selectedRoleData.isSystem && (
                    <div className="flex gap-2">
                      <Button variant="outline" size="sm">
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        className="text-red-600 hover:text-red-700"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  )}
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center gap-2 text-sm text-gray-600">
                    <Users className="h-4 w-4" />
                    {selectedRoleData.users} usuarios asignados a este rol
                  </div>

                  <div className="border-t pt-4">
                    <h4 className="mb-4 font-medium">Permisos</h4>

                    {selectedRoleData.permissions.includes('all') ? (
                      <div className="rounded-lg border border-purple-200 bg-purple-50 p-4">
                        <p className="flex items-center gap-2 font-medium text-purple-800">
                          <Shield className="h-5 w-5" />
                          Acceso Completo
                        </p>
                        <p className="mt-1 text-sm text-purple-600">
                          Este rol tiene acceso a todas las funcionalidades del sistema
                        </p>
                      </div>
                    ) : (
                      <div className="space-y-2">
                        {permissionGroups.map(group => {
                          const groupPermissions = group.permissions.filter(p =>
                            selectedRoleData.permissions.includes(p.id)
                          );
                          const isExpanded = expandedGroups.includes(group.name);

                          if (groupPermissions.length === 0) return null;

                          return (
                            <div key={group.name} className="rounded-lg border">
                              <button
                                onClick={() => toggleGroup(group.name)}
                                className="flex w-full items-center justify-between p-3 hover:bg-gray-50"
                              >
                                <span className="font-medium">{group.name}</span>
                                <div className="flex items-center gap-2">
                                  <Badge variant="secondary">
                                    {groupPermissions.length}/{group.permissions.length}
                                  </Badge>
                                  {isExpanded ? (
                                    <ChevronDown className="h-4 w-4" />
                                  ) : (
                                    <ChevronRight className="h-4 w-4" />
                                  )}
                                </div>
                              </button>
                              {isExpanded && (
                                <div className="space-y-2 border-t px-3 py-2">
                                  {group.permissions.map(permission => {
                                    const hasPermission = selectedRoleData.permissions.includes(
                                      permission.id
                                    );
                                    return (
                                      <div
                                        key={permission.id}
                                        className={`flex items-center gap-3 rounded p-2 ${
                                          hasPermission ? 'bg-emerald-50' : 'bg-gray-50 opacity-50'
                                        }`}
                                      >
                                        {hasPermission ? (
                                          <Check className="h-4 w-4 text-emerald-600" />
                                        ) : (
                                          <X className="h-4 w-4 text-gray-400" />
                                        )}
                                        <div>
                                          <p className="text-sm font-medium">{permission.label}</p>
                                          <p className="text-xs text-gray-500">
                                            {permission.description}
                                          </p>
                                        </div>
                                      </div>
                                    );
                                  })}
                                </div>
                              )}
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12 text-center">
                <Shield className="mb-4 h-12 w-12 text-gray-300" />
                <p className="text-gray-500">Selecciona un rol para ver sus permisos</p>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

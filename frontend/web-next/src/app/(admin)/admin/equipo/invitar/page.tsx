/**
 * Staff Invitation Page
 *
 * Form to invite new staff members.
 * Only accessible by SuperAdmin and Admin roles.
 */

'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { ArrowLeft, Loader2, Mail, User, Shield, Building2, Briefcase, Send } from 'lucide-react';
import { toast } from 'sonner';
import {
  useCreateInvitation,
  useActiveDepartments,
  useActivePositions,
  useStaffMembers,
  type StaffRole,
} from '@/hooks/use-staff';

const ROLES: { value: StaffRole; label: string; description: string }[] = [
  { value: 'SuperAdmin', label: 'Super Admin', description: 'Acceso total al sistema' },
  { value: 'Admin', label: 'Admin', description: 'Gestión completa excepto SuperAdmin' },
  { value: 'Moderator', label: 'Moderador', description: 'Moderación de contenido y usuarios' },
  { value: 'Support', label: 'Soporte', description: 'Atención al cliente y soporte' },
  { value: 'Analyst', label: 'Analista', description: 'Acceso a reportes y analytics' },
  { value: 'Compliance', label: 'Compliance', description: 'Cumplimiento y regulaciones' },
];

export default function InviteStaffPage() {
  const router = useRouter();
  const createMutation = useCreateInvitation();
  const { data: departments } = useActiveDepartments();
  const [selectedDepartmentId, setSelectedDepartmentId] = useState<string>('');
  const { data: positions } = useActivePositions(selectedDepartmentId || undefined);
  const { data: staffData } = useStaffMembers({ status: 'Active', pageSize: 100 });

  const [formData, setFormData] = useState({
    email: '',
    role: '' as StaffRole | '',
    departmentId: '',
    positionId: '',
    supervisorId: '',
    message: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.email) {
      newErrors.email = 'El email es requerido';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Email inválido';
    }

    if (!formData.role) {
      newErrors.role = 'El rol es requerido';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) return;

    try {
      await createMutation.mutateAsync({
        email: formData.email,
        role: formData.role as StaffRole,
        departmentId: formData.departmentId || undefined,
        positionId: formData.positionId || undefined,
        supervisorId: formData.supervisorId || undefined,
        message: formData.message || undefined,
      });

      toast.success('Invitación enviada exitosamente');
      router.push('/admin/equipo/invitaciones');
    } catch (error: unknown) {
      const err = error as { message?: string };
      toast.error(err.message || 'Error al enviar invitación');
    }
  };

  const handleDepartmentChange = (value: string) => {
    setSelectedDepartmentId(value);
    setFormData(prev => ({
      ...prev,
      departmentId: value,
      positionId: '', // Reset position when department changes
    }));
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/admin/equipo">
            <ArrowLeft className="h-5 w-5" />
          </Link>
        </Button>
        <div>
          <h1 className="text-foreground text-3xl font-bold">Invitar Personal</h1>
          <p className="text-muted-foreground">Envía una invitación para unirse al equipo</p>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid gap-6 lg:grid-cols-3">
          {/* Main Form */}
          <div className="space-y-6 lg:col-span-2">
            {/* Personal Info */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <User className="h-5 w-5" />
                  Información Personal
                </CardTitle>
                <CardDescription>Datos básicos del nuevo miembro del equipo</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="email">Email *</Label>
                  <div className="relative">
                    <Mail className="text-muted-foreground absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2" />
                    <Input
                      id="email"
                      type="email"
                      placeholder="juan.perez@okla.com.do"
                      value={formData.email}
                      onChange={e => setFormData(prev => ({ ...prev, email: e.target.value }))}
                      className={`pl-10 ${errors.email ? 'border-red-500' : ''}`}
                    />
                  </div>
                  {errors.email && <p className="text-sm text-red-500">{errors.email}</p>}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="message">Mensaje (opcional)</Label>
                  <Input
                    id="message"
                    placeholder="Mensaje personalizado para la invitación..."
                    value={formData.message}
                    onChange={e => setFormData(prev => ({ ...prev, message: e.target.value }))}
                  />
                </div>
              </CardContent>
            </Card>

            {/* Role Selection */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Shield className="h-5 w-5" />
                  Rol y Permisos
                </CardTitle>
                <CardDescription>Define el nivel de acceso del usuario</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <Label>Rol *</Label>
                  <Select
                    value={formData.role}
                    onValueChange={value =>
                      setFormData(prev => ({ ...prev, role: value as StaffRole }))
                    }
                  >
                    <SelectTrigger className={errors.role ? 'border-red-500' : ''}>
                      <SelectValue placeholder="Selecciona un rol" />
                    </SelectTrigger>
                    <SelectContent>
                      {ROLES.map(role => (
                        <SelectItem key={role.value} value={role.value}>
                          <div className="flex flex-col">
                            <span>{role.label}</span>
                            <span className="text-muted-foreground text-xs">
                              {role.description}
                            </span>
                          </div>
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.role && <p className="text-sm text-red-500">{errors.role}</p>}
                </div>
              </CardContent>
            </Card>

            {/* Organization */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Building2 className="h-5 w-5" />
                  Organización
                </CardTitle>
                <CardDescription>
                  Asigna departamento, posición y supervisor (opcional)
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid gap-4 sm:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="department">Departamento</Label>
                    <Select value={formData.departmentId} onValueChange={handleDepartmentChange}>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecciona departamento" />
                      </SelectTrigger>
                      <SelectContent>
                        {departments?.map(dept => (
                          <SelectItem key={dept.id} value={dept.id}>
                            {dept.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="position">Posición</Label>
                    <Select
                      value={formData.positionId}
                      onValueChange={value => setFormData(prev => ({ ...prev, positionId: value }))}
                      disabled={!formData.departmentId}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Selecciona posición" />
                      </SelectTrigger>
                      <SelectContent>
                        {positions?.map(pos => (
                          <SelectItem key={pos.id} value={pos.id}>
                            {pos.title}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="supervisor">Supervisor</Label>
                  <Select
                    value={formData.supervisorId}
                    onValueChange={value => setFormData(prev => ({ ...prev, supervisorId: value }))}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Selecciona supervisor (opcional)" />
                    </SelectTrigger>
                    <SelectContent>
                      {staffData?.items?.map(staff => (
                        <SelectItem key={staff.id} value={staff.id}>
                          {staff.fullName} - {staff.role}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Actions */}
            <Card>
              <CardHeader>
                <CardTitle>Acciones</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <Button type="submit" className="w-full" disabled={createMutation.isPending}>
                  {createMutation.isPending ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Enviando...
                    </>
                  ) : (
                    <>
                      <Send className="mr-2 h-4 w-4" />
                      Enviar Invitación
                    </>
                  )}
                </Button>
                <Button type="button" variant="outline" className="w-full" asChild>
                  <Link href="/admin/equipo">Cancelar</Link>
                </Button>
              </CardContent>
            </Card>

            {/* Info Card */}
            <Card className="border-blue-200 bg-blue-50 dark:border-blue-800 dark:bg-blue-950/20">
              <CardContent className="pt-6">
                <div className="flex gap-3">
                  <Mail className="mt-0.5 h-5 w-5 flex-shrink-0 text-blue-600 dark:text-blue-400" />
                  <div className="space-y-1">
                    <p className="font-medium text-blue-900 dark:text-blue-100">¿Cómo funciona?</p>
                    <p className="text-sm text-blue-700 dark:text-blue-300">
                      Se enviará un email al invitado con un enlace para crear su cuenta. La
                      invitación expira en 7 días.
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Quick Links */}
            <Card>
              <CardHeader>
                <CardTitle className="text-sm">Enlaces Rápidos</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <Button variant="ghost" className="w-full justify-start" asChild>
                  <Link href="/admin/equipo/invitaciones">
                    <Mail className="mr-2 h-4 w-4" />
                    Ver invitaciones
                  </Link>
                </Button>
                <Button variant="ghost" className="w-full justify-start" asChild>
                  <Link href="/admin/equipo/departamentos">
                    <Building2 className="mr-2 h-4 w-4" />
                    Departamentos
                  </Link>
                </Button>
                <Button variant="ghost" className="w-full justify-start" asChild>
                  <Link href="/admin/equipo/posiciones">
                    <Briefcase className="mr-2 h-4 w-4" />
                    Posiciones
                  </Link>
                </Button>
              </CardContent>
            </Card>
          </div>
        </div>
      </form>
    </div>
  );
}

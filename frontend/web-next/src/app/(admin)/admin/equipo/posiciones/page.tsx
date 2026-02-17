/**
 * Positions Management Page
 *
 * CRUD for positions.
 */

'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Skeleton } from '@/components/ui/skeleton';
import { Switch } from '@/components/ui/switch';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Briefcase,
  Plus,
  ArrowLeft,
  RefreshCw,
  Loader2,
  Edit,
  Trash2,
  Users,
  Building2,
} from 'lucide-react';
import { toast } from 'sonner';
import {
  usePositions,
  useCreatePosition,
  useUpdatePosition,
  useDeletePosition,
  useActiveDepartments,
  type Position,
} from '@/hooks/use-staff';

// =============================================================================
// SKELETON
// =============================================================================

function PositionsSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-10 w-40" />
      </div>
      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nombre</TableHead>
                <TableHead>Departamento</TableHead>
                <TableHead>Nivel</TableHead>
                <TableHead>Personal</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {[1, 2, 3, 4].map(i => (
                <TableRow key={i}>
                  <TableCell>
                    <Skeleton className="h-4 w-32" />
                  </TableCell>
                  <TableCell>
                    <Skeleton className="h-4 w-24" />
                  </TableCell>
                  <TableCell>
                    <Skeleton className="h-4 w-12" />
                  </TableCell>
                  <TableCell>
                    <Skeleton className="h-4 w-12" />
                  </TableCell>
                  <TableCell>
                    <Skeleton className="h-6 w-16" />
                  </TableCell>
                  <TableCell>
                    <Skeleton className="h-8 w-20" />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function PositionsPage() {
  const [departmentFilter, setDepartmentFilter] = useState<string>('all');
  const { data: positionsData, isLoading, refetch } = usePositions();
  const { data: departments } = useActiveDepartments();
  const createMutation = useCreatePosition();
  const updateMutation = useUpdatePosition();
  const deleteMutation = useDeletePosition();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [editingPosition, setEditingPosition] = useState<Position | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    departmentId: '',
    level: 1,
  });

  const openCreateDialog = () => {
    setEditingPosition(null);
    setFormData({ title: '', description: '', departmentId: '', level: 1 });
    setDialogOpen(true);
  };

  const openEditDialog = (pos: Position) => {
    setEditingPosition(pos);
    setFormData({
      title: pos.title,
      description: pos.description || '',
      departmentId: pos.departmentId || '',
      level: pos.level,
    });
    setDialogOpen(true);
  };

  const openDeleteDialog = (id: string) => {
    setDeletingId(id);
    setDeleteDialogOpen(true);
  };

  const handleSubmit = async () => {
    if (!formData.title.trim()) {
      toast.error('El nombre es requerido');
      return;
    }
    if (!formData.departmentId) {
      toast.error('El departamento es requerido');
      return;
    }

    try {
      if (editingPosition) {
        await updateMutation.mutateAsync({
          id: editingPosition.id,
          data: {
            title: formData.title,
            description: formData.description || undefined,
            departmentId: formData.departmentId,
            level: formData.level,
          },
        });
        toast.success('Posición actualizada');
      } else {
        await createMutation.mutateAsync({
          title: formData.title,
          description: formData.description || undefined,
          departmentId: formData.departmentId,
          level: formData.level,
        });
        toast.success('Posición creada');
      }
      setDialogOpen(false);
    } catch {
      toast.error('Error al guardar posición');
    }
  };

  const handleToggleActive = async (pos: Position) => {
    try {
      await updateMutation.mutateAsync({
        id: pos.id,
        data: { isActive: !pos.isActive },
      });
      toast.success(pos.isActive ? 'Posición desactivada' : 'Posición activada');
    } catch {
      toast.error('Error al cambiar estado');
    }
  };

  const handleDelete = async () => {
    if (!deletingId) return;

    try {
      await deleteMutation.mutateAsync(deletingId);
      toast.success('Posición eliminada');
      setDeleteDialogOpen(false);
    } catch {
      toast.error('Error al eliminar posición');
    }
  };

  const handleFilterByDepartment = (deptId: string) => {
    setDepartmentFilter(deptId);
  };

  if (isLoading) {
    return <PositionsSkeleton />;
  }

  const allPositions: Position[] = Array.isArray(positionsData) ? positionsData : [];
  const positions =
    departmentFilter === 'all'
      ? allPositions
      : allPositions.filter(p => p.departmentId === departmentFilter);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/admin/equipo">
            <ArrowLeft className="h-5 w-5" />
          </Link>
        </Button>
        <div className="flex-1">
          <h1 className="text-foreground text-3xl font-bold">Posiciones</h1>
          <p className="text-muted-foreground">Gestiona las posiciones de la organización</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Actualizar
          </Button>
          <Button onClick={openCreateDialog}>
            <Plus className="mr-2 h-4 w-4" />
            Nueva Posición
          </Button>
        </div>
      </div>

      {/* Filter */}
      <Card>
        <CardContent className="p-4">
          <div className="flex items-center gap-4">
            <div className="w-64">
              <Label className="mb-1.5 block text-sm font-medium">Filtrar por departamento</Label>
              <Select value={departmentFilter} onValueChange={handleFilterByDepartment}>
                <SelectTrigger>
                  <SelectValue placeholder="Todos los departamentos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los departamentos</SelectItem>
                  {departments?.map(dept => (
                    <SelectItem key={dept.id} value={dept.id}>
                      {dept.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Table */}
      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nombre</TableHead>
                <TableHead>Departamento</TableHead>
                <TableHead className="text-center">Nivel</TableHead>
                <TableHead className="text-center">Personal</TableHead>
                <TableHead className="text-center">Estado</TableHead>
                <TableHead className="text-right">Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {positions.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="py-8 text-center">
                    <Briefcase className="text-muted-foreground/50 mx-auto mb-2 h-10 w-10" />
                    <p className="text-muted-foreground">No hay posiciones</p>
                    <Button className="mt-4" size="sm" onClick={openCreateDialog}>
                      <Plus className="mr-2 h-4 w-4" />
                      Crear posición
                    </Button>
                  </TableCell>
                </TableRow>
              ) : (
                positions.map(pos => (
                  <TableRow key={pos.id}>
                    <TableCell className="font-medium">
                      <div className="flex items-center gap-2">
                        <Briefcase className="text-muted-foreground h-4 w-4" />
                        <div>
                          {pos.title}
                          {pos.description && (
                            <p className="text-muted-foreground max-w-xs truncate text-xs">
                              {pos.description}
                            </p>
                          )}
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        <Building2 className="text-muted-foreground h-4 w-4" />
                        {pos.departmentName}
                      </div>
                    </TableCell>
                    <TableCell className="text-center">
                      <span className="rounded bg-slate-100 px-2 py-1 text-sm">
                        Nivel {pos.level}
                      </span>
                    </TableCell>
                    <TableCell className="text-center">
                      <div className="flex items-center justify-center gap-1">
                        <Users className="text-muted-foreground h-4 w-4" />
                        {pos.staffCount}
                      </div>
                    </TableCell>
                    <TableCell className="text-center">
                      <Switch
                        checked={pos.isActive}
                        onCheckedChange={() => handleToggleActive(pos)}
                        disabled={updateMutation.isPending}
                      />
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button variant="ghost" size="sm" onClick={() => openEditDialog(pos)}>
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => openDeleteDialog(pos.id)}
                          disabled={pos.staffCount > 0}
                        >
                          <Trash2 className="h-4 w-4 text-red-500" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editingPosition ? 'Editar Posición' : 'Nueva Posición'}</DialogTitle>
            <DialogDescription>
              {editingPosition
                ? 'Actualiza la información de la posición'
                : 'Crea una nueva posición en la organización'}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="title">Nombre *</Label>
              <Input
                id="title"
                value={formData.title}
                onChange={e => setFormData(prev => ({ ...prev, title: e.target.value }))}
                placeholder="Ej: Desarrollador Senior, Gerente..."
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="departmentId">Departamento *</Label>
              <Select
                value={formData.departmentId}
                onValueChange={value => setFormData(prev => ({ ...prev, departmentId: value }))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar departamento" />
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
              <Label htmlFor="level">Nivel</Label>
              <Select
                value={formData.level.toString()}
                onValueChange={value => setFormData(prev => ({ ...prev, level: parseInt(value) }))}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="1">Nivel 1 (Junior)</SelectItem>
                  <SelectItem value="2">Nivel 2 (Mid)</SelectItem>
                  <SelectItem value="3">Nivel 3 (Senior)</SelectItem>
                  <SelectItem value="4">Nivel 4 (Lead)</SelectItem>
                  <SelectItem value="5">Nivel 5 (Manager)</SelectItem>
                  <SelectItem value="6">Nivel 6 (Director)</SelectItem>
                  <SelectItem value="7">Nivel 7 (VP)</SelectItem>
                  <SelectItem value="8">Nivel 8 (C-Level)</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="description">Descripción</Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={e => setFormData(prev => ({ ...prev, description: e.target.value }))}
                placeholder="Descripción de la posición..."
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancelar
            </Button>
            <Button
              onClick={handleSubmit}
              disabled={createMutation.isPending || updateMutation.isPending}
            >
              {(createMutation.isPending || updateMutation.isPending) && (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              {editingPosition ? 'Guardar' : 'Crear'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Eliminar posición</AlertDialogTitle>
            <AlertDialogDescription>
              ¿Estás seguro de eliminar esta posición? Esta acción no se puede deshacer.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-red-600 hover:bg-red-700">
              {deleteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

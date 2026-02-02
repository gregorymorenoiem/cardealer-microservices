/**
 * Admin Reviews Moderation Page
 *
 * Moderate dealer and seller reviews
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
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
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  Search,
  Star,
  Flag,
  CheckCircle,
  XCircle,
  Clock,
  MessageSquare,
  AlertTriangle,
  Eye,
  Trash2,
  MoreVertical,
} from 'lucide-react';

const pendingReviews = [
  {
    id: '1',
    reviewer: 'Juan Martínez',
    reviewerAvatar: null,
    target: 'Auto Premium RD',
    targetType: 'dealer',
    rating: 5,
    title: 'Excelente servicio',
    content:
      'Muy profesionales, encontré mi auto ideal rápidamente. El proceso fue transparente y sin sorpresas.',
    createdAt: '2024-02-15T10:30:00Z',
    status: 'pending',
    flags: [],
  },
  {
    id: '2',
    reviewer: 'María García',
    reviewerAvatar: null,
    target: 'Carlos Vendedor',
    targetType: 'seller',
    rating: 1,
    title: 'Muy mala experiencia',
    content: 'El vehículo tenía problemas que no mencionaron. Perdí mi tiempo y dinero.',
    createdAt: '2024-02-15T09:15:00Z',
    status: 'pending',
    flags: ['reported'],
  },
  {
    id: '3',
    reviewer: 'Pedro López',
    reviewerAvatar: null,
    target: 'Auto Santo Domingo',
    targetType: 'dealer',
    rating: 4,
    title: 'Buen dealer',
    content: 'Buenos precios y variedad. El servicio post-venta puede mejorar.',
    createdAt: '2024-02-14T16:45:00Z',
    status: 'pending',
    flags: [],
  },
];

const reportedReviews = [
  {
    id: '4',
    reviewer: 'Carlos Sánchez',
    reviewerAvatar: null,
    target: 'Auto Premium RD',
    targetType: 'dealer',
    rating: 1,
    title: 'Eviten este lugar',
    content: 'Estafadores, no compren aquí. Me vendieron un carro con problemas ocultos.',
    createdAt: '2024-02-13T11:20:00Z',
    status: 'reported',
    reportReason: 'Contenido difamatorio',
    reportedBy: 'Auto Premium RD',
    reportedAt: '2024-02-14T08:00:00Z',
  },
];

const stats = {
  pending: 12,
  reported: 5,
  approvedToday: 34,
  removedToday: 3,
};

export default function ReviewsPage() {
  const [selectedReview, setSelectedReview] = useState<(typeof pendingReviews)[0] | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [activeTab, setActiveTab] = useState('pending');

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const renderStars = (rating: number) => {
    return (
      <div className="flex">
        {[1, 2, 3, 4, 5].map(star => (
          <Star
            key={star}
            className={`h-4 w-4 ${
              star <= rating ? 'fill-current text-yellow-400' : 'text-gray-600'
            }`}
          />
        ))}
      </div>
    );
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold">Moderación de Reviews</h1>
        <p className="text-gray-400">Aprueba, rechaza o elimina reviews de usuarios</p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-yellow-900 p-2">
                <Clock className="h-5 w-5 text-yellow-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Pendientes</p>
                <p className="text-2xl font-bold text-white">{stats.pending}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-red-900 p-2">
                <Flag className="h-5 w-5 text-red-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Reportados</p>
                <p className="text-2xl font-bold text-white">{stats.reported}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-emerald-900 p-2">
                <CheckCircle className="h-5 w-5 text-emerald-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Aprobados Hoy</p>
                <p className="text-2xl font-bold text-white">{stats.approvedToday}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-slate-700 bg-slate-800">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-purple-900 p-2">
                <Trash2 className="h-5 w-5 text-purple-400" />
              </div>
              <div>
                <p className="text-sm text-gray-400">Eliminados Hoy</p>
                <p className="text-2xl font-bold text-white">{stats.removedToday}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card className="border-slate-700 bg-slate-800">
        <CardContent className="py-4">
          <div className="flex flex-col gap-4 sm:flex-row">
            <div className="relative flex-1">
              <Search className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
              <Input
                placeholder="Buscar por contenido, reviewer o target..."
                className="border-slate-600 bg-slate-700 pl-10"
              />
            </div>
            <Select defaultValue="all">
              <SelectTrigger className="w-40 border-slate-600 bg-slate-700">
                <SelectValue placeholder="Rating" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                <SelectItem value="5">5 estrellas</SelectItem>
                <SelectItem value="4">4 estrellas</SelectItem>
                <SelectItem value="3">3 estrellas</SelectItem>
                <SelectItem value="2">2 estrellas</SelectItem>
                <SelectItem value="1">1 estrella</SelectItem>
              </SelectContent>
            </Select>
            <Select defaultValue="all">
              <SelectTrigger className="w-40 border-slate-600 bg-slate-700">
                <SelectValue placeholder="Tipo" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                <SelectItem value="dealer">Dealer</SelectItem>
                <SelectItem value="seller">Vendedor</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="border-slate-700 bg-slate-800">
          <TabsTrigger value="pending">
            Pendientes
            <Badge className="ml-2 bg-yellow-600">{stats.pending}</Badge>
          </TabsTrigger>
          <TabsTrigger value="reported">
            Reportados
            <Badge className="ml-2 bg-red-600">{stats.reported}</Badge>
          </TabsTrigger>
        </TabsList>

        <TabsContent value="pending" className="mt-6">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Reviews Pendientes</CardTitle>
              <CardDescription>Reviews esperando moderación</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {pendingReviews.map(review => (
                  <div key={review.id} className="rounded-lg bg-slate-700/50 p-4">
                    <div className="mb-3 flex items-start justify-between">
                      <div className="flex items-center gap-3">
                        <Avatar className="h-10 w-10">
                          <AvatarImage src={review.reviewerAvatar || undefined} />
                          <AvatarFallback className="bg-slate-600">
                            {review.reviewer
                              .split(' ')
                              .map(n => n[0])
                              .join('')}
                          </AvatarFallback>
                        </Avatar>
                        <div>
                          <p className="font-medium text-white">{review.reviewer}</p>
                          <p className="text-sm text-gray-400">
                            Review para: <span className="text-emerald-400">{review.target}</span>
                            <Badge variant="outline" className="ml-2 text-xs">
                              {review.targetType === 'dealer' ? 'Dealer' : 'Vendedor'}
                            </Badge>
                          </p>
                        </div>
                      </div>
                      <div className="text-right">
                        {renderStars(review.rating)}
                        <p className="mt-1 text-xs text-gray-500">{formatDate(review.createdAt)}</p>
                      </div>
                    </div>

                    <div className="mb-3">
                      <p className="mb-1 font-medium text-white">{review.title}</p>
                      <p className="text-sm text-gray-300">{review.content}</p>
                    </div>

                    {review.flags.includes('reported') && (
                      <div className="mb-3 rounded-lg border border-red-600/30 bg-red-900/20 p-2">
                        <p className="flex items-center gap-2 text-sm text-red-400">
                          <AlertTriangle className="h-4 w-4" />
                          Este review ha sido reportado
                        </p>
                      </div>
                    )}

                    <div className="flex justify-end gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => {
                          setSelectedReview(review);
                          setDialogOpen(true);
                        }}
                      >
                        <Eye className="mr-1 h-4 w-4" />
                        Ver Más
                      </Button>
                      <Button size="sm" className="bg-emerald-600 hover:bg-emerald-700">
                        <CheckCircle className="mr-1 h-4 w-4" />
                        Aprobar
                      </Button>
                      <Button variant="outline" size="sm" className="border-red-600 text-red-400">
                        <XCircle className="mr-1 h-4 w-4" />
                        Rechazar
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="reported" className="mt-6">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Reviews Reportados</CardTitle>
              <CardDescription>Reviews marcados como inapropiados</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {reportedReviews.map(review => (
                  <div
                    key={review.id}
                    className="rounded-lg border border-red-600/30 bg-slate-700/50 p-4"
                  >
                    <div className="mb-3 flex items-start justify-between">
                      <div className="flex items-center gap-3">
                        <Avatar className="h-10 w-10">
                          <AvatarImage src={review.reviewerAvatar || undefined} />
                          <AvatarFallback className="bg-slate-600">
                            {review.reviewer
                              .split(' ')
                              .map(n => n[0])
                              .join('')}
                          </AvatarFallback>
                        </Avatar>
                        <div>
                          <p className="font-medium text-white">{review.reviewer}</p>
                          <p className="text-sm text-gray-400">
                            Review para: <span className="text-emerald-400">{review.target}</span>
                          </p>
                        </div>
                      </div>
                      <div className="text-right">
                        {renderStars(review.rating)}
                        <p className="mt-1 text-xs text-gray-500">{formatDate(review.createdAt)}</p>
                      </div>
                    </div>

                    <div className="mb-3">
                      <p className="mb-1 font-medium text-white">{review.title}</p>
                      <p className="text-sm text-gray-300">{review.content}</p>
                    </div>

                    <div className="mb-3 rounded-lg border border-red-600/30 bg-red-900/20 p-3">
                      <p className="mb-1 text-sm font-medium text-red-400">
                        <Flag className="mr-1 inline h-4 w-4" />
                        Reportado por: {review.reportedBy}
                      </p>
                      <p className="text-sm text-gray-400">Razón: {review.reportReason}</p>
                      <p className="mt-1 text-xs text-gray-500">{formatDate(review.reportedAt)}</p>
                    </div>

                    <div className="flex justify-end gap-2">
                      <Button variant="outline" size="sm">
                        <MessageSquare className="mr-1 h-4 w-4" />
                        Contactar
                      </Button>
                      <Button size="sm" className="bg-emerald-600 hover:bg-emerald-700">
                        <CheckCircle className="mr-1 h-4 w-4" />
                        Mantener
                      </Button>
                      <Button variant="outline" size="sm" className="border-red-600 text-red-400">
                        <Trash2 className="mr-1 h-4 w-4" />
                        Eliminar
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Review Detail Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-2xl border-slate-700 bg-slate-800">
          <DialogHeader>
            <DialogTitle className="text-white">Detalle del Review</DialogTitle>
          </DialogHeader>

          {selectedReview && (
            <div className="space-y-4">
              <div className="flex items-center gap-3 border-b border-slate-700 pb-4">
                <Avatar className="h-12 w-12">
                  <AvatarImage src={selectedReview.reviewerAvatar || undefined} />
                  <AvatarFallback className="bg-slate-600">
                    {selectedReview.reviewer
                      .split(' ')
                      .map(n => n[0])
                      .join('')}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <p className="font-medium text-white">{selectedReview.reviewer}</p>
                  <div className="flex items-center gap-2">
                    {renderStars(selectedReview.rating)}
                    <span className="text-sm text-gray-400">
                      {formatDate(selectedReview.createdAt)}
                    </span>
                  </div>
                </div>
              </div>

              <div>
                <p className="mb-1 text-sm text-gray-400">Review para</p>
                <p className="font-medium text-white">{selectedReview.target}</p>
              </div>

              <div>
                <p className="mb-1 text-sm text-gray-400">Título</p>
                <p className="font-medium text-white">{selectedReview.title}</p>
              </div>

              <div>
                <p className="mb-1 text-sm text-gray-400">Contenido</p>
                <p className="text-gray-300">{selectedReview.content}</p>
              </div>

              <div>
                <p className="mb-2 text-sm text-gray-400">Nota de moderación (opcional)</p>
                <Textarea
                  placeholder="Agregar nota interna..."
                  className="border-slate-600 bg-slate-700"
                />
              </div>
            </div>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancelar
            </Button>
            <Button variant="outline" className="border-red-600 text-red-400">
              <Trash2 className="mr-2 h-4 w-4" />
              Eliminar
            </Button>
            <Button className="bg-emerald-600 hover:bg-emerald-700">
              <CheckCircle className="mr-2 h-4 w-4" />
              Aprobar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

/**
 * Admin Moderation Page
 *
 * Content moderation queue for administrators
 */

'use client';

import { useState } from 'react';
import Image from 'next/image';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Textarea } from '@/components/ui/textarea';
import {
  CheckCircle,
  XCircle,
  Clock,
  Eye,
  AlertTriangle,
  ChevronLeft,
  ChevronRight,
  Flag,
  User,
  Calendar,
  ImageIcon,
  FileText,
} from 'lucide-react';

// Mock moderation queue data
const moderationQueue = [
  {
    id: '1',
    type: 'vehicle',
    title: 'Toyota Camry 2023',
    description: 'Vehículo en perfectas condiciones, un solo dueño...',
    price: 1850000,
    images: [
      'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400',
      'https://images.unsplash.com/photo-1568844293986-8c1a5d7f7bc8?w=400',
    ],
    seller: 'Juan Pérez',
    sellerType: 'individual',
    submittedAt: '2024-02-12 14:30',
    reason: null,
    priority: 'normal',
  },
  {
    id: '2',
    type: 'vehicle',
    title: 'BMW X5 2022',
    description: 'Recién importado, papeles en regla, precio negociable...',
    price: 3500000,
    images: ['https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400'],
    seller: 'AutoMax Premium',
    sellerType: 'dealer',
    submittedAt: '2024-02-12 10:15',
    reason: 'Posible precio sospechoso',
    priority: 'high',
  },
  {
    id: '3',
    type: 'vehicle',
    title: 'Honda Civic 2021',
    description: 'Motor 2.0, aire acondicionado, sunroof...',
    price: 1200000,
    images: [
      'https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=400',
      'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=400',
    ],
    seller: 'María García',
    sellerType: 'individual',
    submittedAt: '2024-02-12 09:45',
    reason: null,
    priority: 'normal',
  },
];

const moderationStats = [
  { label: 'En Cola', value: 23, icon: Clock },
  { label: 'Alta Prioridad', value: 5, icon: AlertTriangle },
  { label: 'Hoy Revisados', value: 45, icon: CheckCircle },
  { label: 'Rechazados Hoy', value: 3, icon: XCircle },
];

const rejectionReasons = [
  'Fotos de baja calidad o incompletas',
  'Información del vehículo incorrecta o incompleta',
  'Precio fuera del rango de mercado',
  'Contenido inapropiado o engañoso',
  'Vehículo ya publicado anteriormente',
  'Documentos sospechosos o fraudulentos',
  'Otro (especificar)',
];

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    maximumFractionDigits: 0,
  }).format(price);
};

export default function AdminModerationPage() {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [selectedReason, setSelectedReason] = useState('');
  const [customReason, setCustomReason] = useState('');

  const currentItem = moderationQueue[currentIndex];

  const handleApprove = () => {
    // Approve and move to next
    if (currentIndex < moderationQueue.length - 1) {
      setCurrentIndex(currentIndex + 1);
    }
  };

  const handleReject = () => {
    setShowRejectModal(true);
  };

  const confirmReject = () => {
    // Reject and move to next
    setShowRejectModal(false);
    setSelectedReason('');
    setCustomReason('');
    if (currentIndex < moderationQueue.length - 1) {
      setCurrentIndex(currentIndex + 1);
    }
  };

  const handleNext = () => {
    if (currentIndex < moderationQueue.length - 1) {
      setCurrentIndex(currentIndex + 1);
    }
  };

  const handlePrev = () => {
    if (currentIndex > 0) {
      setCurrentIndex(currentIndex - 1);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Moderación</h1>
          <p className="text-gray-600">Cola de revisión de contenido</p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {moderationStats.map(stat => {
          const Icon = stat.icon;
          return (
            <Card key={stat.label}>
              <CardContent className="p-4">
                <div className="flex items-center gap-3">
                  <div className="rounded-lg bg-slate-100 p-2">
                    <Icon className="h-5 w-5 text-slate-600" />
                  </div>
                  <div>
                    <p className="text-2xl font-bold">{stat.value}</p>
                    <p className="text-xs text-gray-500">{stat.label}</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Main Moderation Interface */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Images Panel */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2 text-lg">
              <ImageIcon className="h-5 w-5" />
              Imágenes ({currentItem.images.length})
            </CardTitle>
            <Badge className={currentItem.priority === 'high' ? 'bg-red-100 text-red-700' : ''}>
              {currentItem.priority === 'high' ? 'Alta Prioridad' : 'Normal'}
            </Badge>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {/* Main Image */}
              <div className="relative aspect-video overflow-hidden rounded-lg bg-gray-100">
                <Image
                  src={currentItem.images[0]}
                  alt={currentItem.title}
                  fill
                  className="object-cover"
                />
              </div>

              {/* Thumbnails */}
              {currentItem.images.length > 1 && (
                <div className="flex gap-2">
                  {currentItem.images.map((img, idx) => (
                    <div
                      key={idx}
                      className="relative h-16 w-20 cursor-pointer overflow-hidden rounded-lg bg-gray-100 ring-emerald-500 hover:ring-2"
                    >
                      <Image
                        src={img}
                        alt={`${currentItem.title} ${idx + 1}`}
                        fill
                        className="object-cover"
                      />
                    </div>
                  ))}
                </div>
              )}

              {/* Alert if flagged */}
              {currentItem.reason && (
                <div className="flex items-start gap-2 rounded-lg border border-amber-200 bg-amber-50 p-3">
                  <Flag className="mt-0.5 h-5 w-5 flex-shrink-0 text-amber-600" />
                  <div>
                    <p className="font-medium text-amber-800">Marcado para revisión</p>
                    <p className="text-sm text-amber-700">{currentItem.reason}</p>
                  </div>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Details Panel */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              <FileText className="h-5 w-5" />
              Detalles del Listado
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* Title and Price */}
            <div>
              <h2 className="text-xl font-bold">{currentItem.title}</h2>
              <p className="text-2xl font-bold text-emerald-600">
                {formatPrice(currentItem.price)}
              </p>
            </div>

            {/* Description */}
            <div>
              <p className="mb-1 text-sm font-medium text-gray-600">Descripción</p>
              <p className="text-gray-800">{currentItem.description}</p>
            </div>

            {/* Seller Info */}
            <div className="rounded-lg bg-gray-50 p-4">
              <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gray-200">
                  <User className="h-5 w-5 text-gray-500" />
                </div>
                <div>
                  <p className="font-medium">{currentItem.seller}</p>
                  <p className="text-sm text-gray-500 capitalize">{currentItem.sellerType}</p>
                </div>
              </div>
            </div>

            {/* Metadata */}
            <div className="flex items-center gap-4 text-sm text-gray-500">
              <span className="flex items-center gap-1">
                <Calendar className="h-4 w-4" />
                {currentItem.submittedAt}
              </span>
            </div>

            {/* Action Buttons */}
            <div className="flex gap-3 pt-4">
              <Button
                className="flex-1 bg-emerald-600 hover:bg-emerald-700"
                onClick={handleApprove}
              >
                <CheckCircle className="mr-2 h-4 w-4" />
                Aprobar
              </Button>
              <Button variant="destructive" className="flex-1" onClick={handleReject}>
                <XCircle className="mr-2 h-4 w-4" />
                Rechazar
              </Button>
            </div>

            {/* Skip Button */}
            <Button variant="ghost" className="w-full" onClick={handleNext}>
              Saltar (revisar después)
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Navigation */}
      <div className="flex items-center justify-center gap-4">
        <Button variant="outline" onClick={handlePrev} disabled={currentIndex === 0}>
          <ChevronLeft className="mr-1 h-4 w-4" />
          Anterior
        </Button>
        <span className="text-sm text-gray-600">
          {currentIndex + 1} de {moderationQueue.length}
        </span>
        <Button
          variant="outline"
          onClick={handleNext}
          disabled={currentIndex === moderationQueue.length - 1}
        >
          Siguiente
          <ChevronRight className="ml-1 h-4 w-4" />
        </Button>
      </div>

      {/* Reject Modal */}
      {showRejectModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <Card className="w-full max-w-md">
            <CardHeader>
              <CardTitle>Razón del Rechazo</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                {rejectionReasons.map(reason => (
                  <label
                    key={reason}
                    className="flex cursor-pointer items-center gap-2 rounded-lg p-2 hover:bg-gray-50"
                  >
                    <input
                      type="radio"
                      name="reason"
                      value={reason}
                      checked={selectedReason === reason}
                      onChange={e => setSelectedReason(e.target.value)}
                      className="rounded-full"
                    />
                    <span className="text-sm">{reason}</span>
                  </label>
                ))}
              </div>

              {selectedReason === 'Otro (especificar)' && (
                <Textarea
                  placeholder="Describe la razón del rechazo..."
                  value={customReason}
                  onChange={e => setCustomReason(e.target.value)}
                />
              )}

              <div className="flex gap-3">
                <Button
                  variant="outline"
                  className="flex-1"
                  onClick={() => setShowRejectModal(false)}
                >
                  Cancelar
                </Button>
                <Button
                  variant="destructive"
                  className="flex-1"
                  onClick={confirmReject}
                  disabled={!selectedReason}
                >
                  Confirmar Rechazo
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}

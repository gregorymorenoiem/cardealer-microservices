/**
 * Admin KYC - UI Sub-components
 *
 * Extracted from the monolithic page for better code splitting and maintainability.
 */

'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { cn } from '@/lib/utils';
import {
  CheckCircle,
  XCircle,
  Eye,
  User,
  FileText,
  AlertTriangle,
  Loader2,
  ChevronRight,
  ImageIcon,
  ZoomIn,
  ZoomOut,
  X,
  Columns,
  CheckSquare,
  Square,
  Maximize2,
  RotateCw,
  Download,
  Activity,
  Cpu,
  TrendingUp,
} from 'lucide-react';
import { KYCStatusBadge } from '@/components/dashboard';
import { getDocumentFreshUrl, type BiometricScores } from '@/services/kyc';
import type { KYCProfileSummary } from '@/services/kyc';
import { getInitials, formatDate, statusToFilterKey, VERIFICATION_ITEMS } from './helpers';

// =============================================================================
// STAT CARD
// =============================================================================

interface StatCardProps {
  icon: React.ElementType;
  value: number;
  label: string;
  color: 'amber' | 'blue' | 'primary' | 'red';
  onClick?: () => void;
  isActive?: boolean;
}

export function StatCard({ icon: Icon, value, label, color, onClick, isActive }: StatCardProps) {
  const colorClasses = {
    amber: 'bg-amber-100 text-amber-600',
    blue: 'bg-blue-100 text-blue-600',
    primary: 'bg-primary/10 text-primary',
    red: 'bg-red-100 text-red-600',
  };

  return (
    <button
      onClick={onClick}
      className={cn(
        'flex items-center gap-3 rounded-lg p-3 text-left transition-all',
        isActive
          ? 'bg-primary/10 shadow-[0_0_0_2px_rgba(16,185,129,0.5)]'
          : 'bg-white hover:bg-gray-50'
      )}
    >
      <div className={cn('rounded-lg p-2', colorClasses[color])}>
        <Icon className="h-4 w-4" />
      </div>
      <div>
        <p className="text-xl font-bold">{value}</p>
        <p className="text-muted-foreground text-xs">{label}</p>
      </div>
    </button>
  );
}

// =============================================================================
// PROFILE LIST ITEM
// =============================================================================

interface ProfileListItemProps {
  profile: KYCProfileSummary;
  isSelected: boolean;
  onClick: () => void;
}

export function ProfileListItem({ profile, isSelected, onClick }: ProfileListItemProps) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'w-full border-b border-gray-100 p-3 text-left transition-all',
        isSelected ? 'bg-primary/10 shadow-[inset_3px_0_0_0_rgb(16,185,129)]' : 'hover:bg-white'
      )}
    >
      <div className="flex items-start gap-3">
        <Avatar className="h-10 w-10 flex-shrink-0">
          <AvatarFallback className="bg-muted text-muted-foreground text-sm">
            {getInitials(profile.fullName)}
          </AvatarFallback>
        </Avatar>
        <div className="min-w-0 flex-1">
          <div className="flex items-center justify-between gap-2">
            <p className="truncate font-medium">{profile.fullName || 'Sin nombre'}</p>
            <ChevronRight className="text-muted-foreground h-4 w-4 flex-shrink-0" />
          </div>
          <p className="text-muted-foreground truncate text-xs">
            {profile.documentNumber || 'Sin documento'}
          </p>
          <div className="mt-1.5 flex items-center justify-between gap-2">
            <span className="text-muted-foreground text-xs">{formatDate(profile.createdAt)}</span>
            <KYCStatusBadge status={statusToFilterKey(profile.status)} />
          </div>
          {profile.isPEP && (
            <Badge className="mt-1.5 bg-red-100 text-xs text-red-700">
              <AlertTriangle className="mr-1 h-3 w-3" />
              PEP
            </Badge>
          )}
        </div>
      </div>
    </button>
  );
}

// =============================================================================
// EMPTY & LOADING STATES
// =============================================================================

export function EmptyDetailPanel() {
  return (
    <div className="flex h-full flex-col items-center justify-center p-8 text-center">
      <div className="bg-muted/50 rounded-full p-6">
        <User className="text-muted-foreground h-12 w-12" />
      </div>
      <h3 className="text-muted-foreground mt-4 text-lg font-medium">Selecciona una solicitud</h3>
      <p className="text-muted-foreground mt-2 max-w-sm text-sm">
        Haz clic en una solicitud de la lista para ver los detalles completos y tomar acción.
      </p>
    </div>
  );
}

export function LoadingDetailPanel() {
  return (
    <div className="flex h-full items-center justify-center">
      <div className="text-center">
        <Loader2 className="mx-auto h-8 w-8 animate-spin text-primary" />
        <p className="text-muted-foreground mt-2">Cargando detalles...</p>
      </div>
    </div>
  );
}

// =============================================================================
// DOCUMENT IMAGE
// =============================================================================

interface DocumentImageProps {
  documentId: string;
  alt: string;
  className?: string;
}

export function DocumentImage({ documentId, alt, className }: DocumentImageProps) {
  const [imageUrl, setImageUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    let isMounted = true;
    // State is reset via key changes or initial render (loading=true by default)

    getDocumentFreshUrl(documentId)
      .then(response => {
        if (isMounted && response?.url) {
          setImageUrl(response.url);
        } else if (isMounted) {
          setError(true);
        }
      })
      .catch(() => {
        if (isMounted) {
          setError(true);
        }
      })
      .finally(() => {
        if (isMounted) {
          setLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [documentId]);

  if (loading) {
    return (
      <div className={cn('flex items-center justify-center bg-gray-100', className)}>
        <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
      </div>
    );
  }

  if (error || !imageUrl) {
    return (
      <div className={cn('flex flex-col items-center justify-center bg-gray-100', className)}>
        <ImageIcon className="h-6 w-6 text-gray-400" />
        <span className="mt-1 text-xs text-gray-400">Error</span>
      </div>
    );
  }

  return (
    <Image
      src={imageUrl}
      alt={alt}
      width={600}
      height={400}
      className={cn('object-cover', className)}
      onError={() => setError(true)}
      unoptimized
    />
  );
}

// =============================================================================
// VALIDATABLE FIELD
// =============================================================================

interface ValidatableFieldProps {
  icon: React.ElementType;
  label: string;
  value: string | null | undefined;
  checkKey: string;
  checked: boolean | undefined;
  onToggle: () => void;
  canValidate: boolean;
  mono?: boolean;
}

export function ValidatableField({
  icon: Icon,
  label,
  value,
  checked,
  onToggle,
  canValidate,
  mono,
}: ValidatableFieldProps) {
  return (
    <div className="flex items-center justify-between rounded-lg px-2 py-2 transition-colors hover:bg-gray-50">
      <div className="flex min-w-0 flex-1 items-start gap-3">
        <Icon className="text-muted-foreground mt-0.5 h-4 w-4 flex-shrink-0" />
        <div className="min-w-0 flex-1">
          <p className="text-muted-foreground text-xs">{label}</p>
          <p className={cn('truncate text-sm font-medium', mono && 'font-mono')}>
            {value || 'N/A'}
          </p>
        </div>
      </div>
      {canValidate && (
        <div className="ml-2 flex flex-shrink-0 gap-1">
          <Button
            size="sm"
            variant={checked === true ? 'default' : 'outline'}
            className={cn(
              'h-7 w-7 p-0',
              checked === true && 'bg-primary text-white hover:bg-primary/90'
            )}
            onClick={onToggle}
            title="Aprobar"
          >
            <CheckCircle className="h-3.5 w-3.5" />
          </Button>
          <Button
            size="sm"
            variant={checked === false ? 'default' : 'outline'}
            className={cn(
              'h-7 w-7 p-0',
              checked === false && 'bg-red-600 text-white hover:bg-red-700'
            )}
            onClick={() => {
              if (checked !== false) {
                onToggle();
                onToggle();
              }
            }}
            title="Rechazar"
          >
            <XCircle className="h-3.5 w-3.5" />
          </Button>
        </div>
      )}
      {!canValidate && checked !== undefined && (
        <div className="ml-2 flex-shrink-0">
          {checked ? (
            <CheckCircle className="h-4 w-4 text-primary" />
          ) : (
            <XCircle className="h-4 w-4 text-red-600" />
          )}
        </div>
      )}
    </div>
  );
}

// =============================================================================
// VERIFICATION CHECKLIST
// =============================================================================

interface VerificationChecklistProps {
  checks: Record<string, boolean>;
  onToggle: (key: string) => void;
}

export function VerificationChecklist({ checks, onToggle }: VerificationChecklistProps) {
  const categories = [...new Set(VERIFICATION_ITEMS.map(item => item.category))];
  const allChecked = VERIFICATION_ITEMS.every(item => checks[item.key]);
  const checkedCount = VERIFICATION_ITEMS.filter(item => checks[item.key]).length;

  return (
    <Card className="shadow-sm">
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2 text-base">
            <CheckSquare className="h-4 w-4" />
            Checklist de Verificación
          </CardTitle>
          <Badge
            className={cn(
              allChecked ? 'bg-primary/10 text-primary' : 'bg-amber-100 text-amber-700'
            )}
          >
            {checkedCount}/{VERIFICATION_ITEMS.length}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {categories.map(category => (
          <div key={category}>
            <p className="text-muted-foreground mb-1.5 text-xs font-medium tracking-wide uppercase">
              {category}
            </p>
            <div className="space-y-1">
              {VERIFICATION_ITEMS.filter(item => item.category === category).map(item => (
                <button
                  key={item.key}
                  onClick={() => onToggle(item.key)}
                  className={cn(
                    'flex w-full items-center gap-2 rounded-md p-2 text-left text-sm transition-colors',
                    checks[item.key] ? 'bg-primary/10 text-primary' : 'hover:bg-gray-50'
                  )}
                >
                  {checks[item.key] ? (
                    <CheckSquare className="h-4 w-4 flex-shrink-0 text-primary" />
                  ) : (
                    <Square className="text-muted-foreground h-4 w-4 flex-shrink-0" />
                  )}
                  <span>{item.label}</span>
                </button>
              ))}
            </div>
          </div>
        ))}
      </CardContent>
    </Card>
  );
}

// =============================================================================
// BIOMETRIC RESULTS CARD
// =============================================================================

interface ScoreBarProps {
  label: string;
  score: number;
  passed: boolean;
  icon: React.ElementType;
  provider?: string;
  verifiedAt?: string | null;
}

function ScoreBar({ label, score, passed, icon: Icon, provider, verifiedAt }: ScoreBarProps) {
  const getScoreColor = (s: number) => {
    if (s >= 90) return 'bg-primary/100';
    if (s >= 80) return 'bg-primary/60';
    if (s >= 70) return 'bg-amber-400';
    return 'bg-red-500';
  };

  const getScoreTextColor = (s: number) => {
    if (s >= 80) return 'text-primary';
    if (s >= 70) return 'text-amber-700';
    return 'text-red-700';
  };

  return (
    <div className="rounded-lg border p-3">
      <div className="mb-2 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Icon className="text-muted-foreground h-4 w-4" />
          <span className="text-sm font-medium">{label}</span>
        </div>
        <div className="flex items-center gap-2">
          <span className={cn('text-lg font-bold', getScoreTextColor(score))}>{score}%</span>
          {passed ? (
            <CheckCircle className="h-4 w-4 text-primary" />
          ) : (
            <XCircle className="h-4 w-4 text-red-500" />
          )}
        </div>
      </div>
      <div className="h-2 w-full rounded-full bg-gray-100">
        <div
          className={cn('h-2 rounded-full transition-all', getScoreColor(score))}
          style={{ width: `${Math.min(score, 100)}%` }}
        />
      </div>
      <div className="mt-1.5 flex items-center justify-between">
        {provider && (
          <span className="text-muted-foreground text-xs">
            vía{' '}
            {provider === 'AmazonRekognition'
              ? '🔵 Amazon Rekognition'
              : provider === 'Simulation'
                ? '⚪ Simulación'
                : provider}
          </span>
        )}
        {verifiedAt && (
          <span className="text-muted-foreground text-xs">
            {new Date(verifiedAt).toLocaleString('es-DO', {
              dateStyle: 'short',
              timeStyle: 'short',
            })}
          </span>
        )}
      </div>
    </div>
  );
}

interface BiometricResultsCardProps {
  scores: BiometricScores;
}

export function BiometricResultsCard({ scores }: BiometricResultsCardProps) {
  if (!scores.hasResults) {
    return (
      <Card className="border-dashed shadow-sm">
        <CardHeader className="pb-2">
          <CardTitle className="flex items-center gap-2 text-base">
            <Cpu className="h-4 w-4" />
            Reconocimiento Facial (IA)
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-4 text-center">
            <Activity className="text-muted-foreground mb-2 h-8 w-8" />
            <p className="text-muted-foreground text-sm">Sin resultados biométricos</p>
            <p className="text-muted-foreground mt-1 text-xs">
              El usuario aún no ha completado la verificación facial
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="border-blue-200 bg-blue-50/30 shadow-sm">
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2 text-base">
            <Cpu className="h-4 w-4 text-blue-600" />
            Reconocimiento Facial (IA)
          </CardTitle>
          {scores.overallScore !== null && (
            <Badge
              className={cn(
                scores.overallScore >= 80
                  ? 'bg-primary/10 text-primary'
                  : scores.overallScore >= 70
                    ? 'bg-amber-100 text-amber-700'
                    : 'bg-red-100 text-red-700'
              )}
            >
              <TrendingUp className="mr-1 h-3 w-3" />
              Score: {scores.overallScore}%
            </Badge>
          )}
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {scores.faceMatch && (
          <ScoreBar
            label="Coincidencia Facial"
            score={scores.faceMatch.score}
            passed={scores.faceMatch.passed}
            icon={User}
            provider={scores.faceMatch.provider}
            verifiedAt={scores.faceMatch.verifiedAt}
          />
        )}
        {scores.liveness && (
          <ScoreBar
            label="Prueba de Vida"
            score={scores.liveness.score}
            passed={scores.liveness.passed}
            icon={Eye}
            provider={scores.liveness.provider}
            verifiedAt={scores.liveness.verifiedAt}
          />
        )}
        {scores.documentOcr && (
          <ScoreBar
            label="OCR de Documento"
            score={scores.documentOcr.score}
            passed={scores.documentOcr.passed}
            icon={FileText}
            provider={scores.documentOcr.provider}
            verifiedAt={scores.documentOcr.verifiedAt}
          />
        )}
        <div className="mt-2 rounded-lg bg-white/70 p-3">
          <p className="text-muted-foreground text-xs">
            <strong>Resumen automático:</strong>{' '}
            {scores.faceMatch?.passed && scores.liveness?.passed
              ? '✅ La IA confirma que el rostro del documento coincide con el selfie y la prueba de vida fue exitosa.'
              : scores.faceMatch && !scores.faceMatch.passed
                ? '❌ La IA detectó que el rostro del documento NO coincide suficiente con el selfie. Revisar manualmente.'
                : scores.liveness && !scores.liveness.passed
                  ? '⚠️ La prueba de vida no fue superada. Posible intento de suplantación.'
                  : 'Resultados parciales disponibles. Verificación manual recomendada.'}
          </p>
        </div>
      </CardContent>
    </Card>
  );
}

// =============================================================================
// IMAGE VIEWER MODAL
// =============================================================================

interface ImageViewerProps {
  images: Array<{ id: string; url: string; name: string }>;
  currentIndex: number;
  onClose: () => void;
  onNavigate: (index: number) => void;
}

export function ImageViewer({ images, currentIndex, onClose, onNavigate }: ImageViewerProps) {
  const [zoom, setZoom] = useState(1);
  const [rotation, setRotation] = useState(0);
  const [compareMode, setCompareMode] = useState(false);
  const [compareIndex, setCompareIndex] = useState<number | null>(null);

  const currentImage = images[currentIndex];
  const compareImage = compareIndex !== null ? images[compareIndex] : null;

  const handleZoomIn = () => setZoom(prev => Math.min(prev + 0.25, 3));
  const handleZoomOut = () => setZoom(prev => Math.max(prev - 0.25, 0.5));
  const handleRotate = () => setRotation(prev => (prev + 90) % 360);
  const handleReset = () => {
    setZoom(1);
    setRotation(0);
  };

  const handleDownload = () => {
    if (!currentImage) return;
    const link = document.createElement('a');
    link.href = currentImage.url;
    link.download = currentImage.name || 'documento-kyc';
    link.target = '_blank';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowLeft' && currentIndex > 0) onNavigate(currentIndex - 1);
      if (e.key === 'ArrowRight' && currentIndex < images.length - 1) onNavigate(currentIndex + 1);
      if (e.key === '+' || e.key === '=') handleZoomIn();
      if (e.key === '-') handleZoomOut();
      if (e.key === 'd' || e.key === 'D') handleDownload();
      if (e.key === 'r' || e.key === 'R') handleRotate();
      if (e.key === '0') handleReset();
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentIndex, images.length, onClose, onNavigate]);

  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-black/95">
      {/* Toolbar */}
      <div className="flex items-center justify-between border-b border-white/10 bg-black/50 px-4 py-3">
        <div className="flex items-center gap-2">
          <span className="text-sm font-medium text-white">
            {currentImage?.name} ({currentIndex + 1}/{images.length})
          </span>
        </div>
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="icon"
            onClick={handleZoomOut}
            className="text-white hover:bg-white/10"
            title="Alejar (-)"
          >
            <ZoomOut className="h-4 w-4" />
          </Button>
          <span className="min-w-[50px] text-center text-xs text-white">
            {Math.round(zoom * 100)}%
          </span>
          <Button
            variant="ghost"
            size="icon"
            onClick={handleZoomIn}
            className="text-white hover:bg-white/10"
            title="Acercar (+)"
          >
            <ZoomIn className="h-4 w-4" />
          </Button>
          <div className="mx-2 h-4 w-px bg-white/20" />
          <Button
            variant="ghost"
            size="icon"
            onClick={handleRotate}
            className="text-white hover:bg-white/10"
            title="Rotar (R)"
          >
            <RotateCw className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            onClick={handleReset}
            className="text-white hover:bg-white/10"
            title="Restablecer vista"
          >
            <Maximize2 className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            onClick={handleDownload}
            className="text-white hover:bg-white/10"
            title="Descargar (D)"
          >
            <Download className="h-4 w-4" />
          </Button>
          <div className="mx-2 h-4 w-px bg-white/20" />
          <Button
            variant={compareMode ? 'secondary' : 'ghost'}
            size="sm"
            onClick={() => {
              setCompareMode(!compareMode);
              if (!compareMode && images.length > 1) {
                const selfieIdx = images.findIndex(
                  img =>
                    img.name.toLowerCase().includes('selfie') ||
                    img.name.toLowerCase().includes('liveness')
                );
                const otherIdx =
                  selfieIdx !== -1 && selfieIdx !== currentIndex
                    ? selfieIdx
                    : images.findIndex((_, idx) => idx !== currentIndex);
                setCompareIndex(otherIdx !== -1 ? otherIdx : null);
              } else {
                setCompareIndex(null);
              }
            }}
            className={cn(
              compareMode ? 'bg-primary text-white' : 'text-white hover:bg-white/10'
            )}
            title="Comparar lado a lado"
          >
            <Columns className="mr-1 h-4 w-4" />
            Comparar
          </Button>
          <div className="mx-2 h-4 w-px bg-white/20" />
          <Button
            variant="ghost"
            size="icon"
            onClick={onClose}
            className="text-white hover:bg-white/10"
            title="Cerrar (Esc)"
          >
            <X className="h-5 w-5" />
          </Button>
        </div>
      </div>

      {/* Image Area */}
      <div className="relative flex flex-1 overflow-hidden">
        {compareMode && compareImage ? (
          <>
            <div className="flex flex-1 flex-col border-r border-white/10">
              <div className="bg-white/5 px-3 py-1.5 text-center text-xs text-white/70">
                {currentImage?.name}
              </div>
              <div className="flex flex-1 items-center justify-center overflow-auto p-4">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={currentImage?.url}
                  alt={currentImage?.name}
                  className="max-h-full max-w-full object-contain transition-transform duration-200"
                  style={{ transform: `scale(${zoom}) rotate(${rotation}deg)` }}
                />
              </div>
            </div>
            <div className="flex flex-1 flex-col">
              <div className="flex items-center justify-center gap-2 bg-white/5 px-3 py-1.5">
                <select
                  value={compareIndex ?? ''}
                  onChange={e => setCompareIndex(Number(e.target.value))}
                  className="bg-transparent text-center text-xs text-white/70 focus:outline-none"
                >
                  {images.map(
                    (img, idx) =>
                      idx !== currentIndex && (
                        <option key={img.id} value={idx} className="bg-gray-900">
                          {img.name}
                        </option>
                      )
                  )}
                </select>
              </div>
              <div className="flex flex-1 items-center justify-center overflow-auto p-4">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={compareImage?.url}
                  alt={compareImage?.name}
                  className="max-h-full max-w-full object-contain transition-transform duration-200"
                  style={{ transform: `scale(${zoom}) rotate(${rotation}deg)` }}
                />
              </div>
            </div>
          </>
        ) : (
          <div className="flex flex-1 items-center justify-center overflow-auto p-4">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src={currentImage?.url}
              alt={currentImage?.name}
              className="max-h-full max-w-full object-contain transition-transform duration-200"
              style={{ transform: `scale(${zoom}) rotate(${rotation}deg)` }}
            />
          </div>
        )}

        {!compareMode && images.length > 1 && (
          <>
            {currentIndex > 0 && (
              <button
                onClick={() => onNavigate(currentIndex - 1)}
                className="absolute top-1/2 left-4 -translate-y-1/2 rounded-full bg-black/50 p-3 text-white transition-colors hover:bg-black/70"
              >
                <ChevronRight className="h-6 w-6 rotate-180" />
              </button>
            )}
            {currentIndex < images.length - 1 && (
              <button
                onClick={() => onNavigate(currentIndex + 1)}
                className="absolute top-1/2 right-4 -translate-y-1/2 rounded-full bg-black/50 p-3 text-white transition-colors hover:bg-black/70"
              >
                <ChevronRight className="h-6 w-6" />
              </button>
            )}
          </>
        )}
      </div>

      {/* Thumbnails */}
      {images.length > 1 && !compareMode && (
        <div className="flex justify-center gap-2 border-t border-white/10 bg-black/50 p-3">
          {images.map((img, idx) => (
            <button
              key={img.id}
              onClick={() => onNavigate(idx)}
              className={cn(
                'h-16 w-16 overflow-hidden rounded-md border-2 transition-all',
                idx === currentIndex
                  ? 'border-primary ring-2 ring-primary/50'
                  : 'border-white/20 opacity-60 hover:opacity-100'
              )}
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={img.url} alt={img.name} className="h-full w-full object-cover" />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

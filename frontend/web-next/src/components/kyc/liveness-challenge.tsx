/**
 * Liveness Challenge Component
 *
 * Implements liveness detection for identity verification.
 * User must complete facial challenges to prove they are a real person.
 *
 * Challenges:
 * - Blink (parpadear)
 * - Smile (sonreír)
 * - Turn head left/right (girar cabeza)
 *
 * The challenges help prevent photo/video spoofing attacks.
 */

'use client';

import * as React from 'react';
import Image from 'next/image';
import dynamic from 'next/dynamic';

// @ts-expect-error react-webcam module types incompatible with next/dynamic in Next.js 16
const Webcam = dynamic(() => import('react-webcam'), {
  ssr: false,
  loading: () => (
    <div className="bg-muted flex h-64 w-full items-center justify-center rounded-lg">
      <p className="text-muted-foreground text-sm">Cargando cámara...</p>
    </div>
  ),
});
import {
  CheckCircle,
  XCircle,
  AlertTriangle,
  Loader2,
  Eye,
  Smile,
  ArrowLeft,
  ArrowRight,
  ChevronDown,
  Camera,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { cn } from '@/lib/utils';
import type { LivenessData } from '@/services/kyc';

// =============================================================================
// TYPES
// =============================================================================

interface LivenessChallengeProps {
  requiredChallenges?: string[];
  onComplete: (selfie: Blob, livenessData: LivenessData) => Promise<void>;
  onError?: (error: string) => void;
  onCancel?: () => void;
  isProcessing?: boolean;
  className?: string;
}

interface ChallengeState {
  type: string;
  status: 'pending' | 'active' | 'completed' | 'failed';
  startedAt?: Date;
  completedAt?: Date;
  confidence?: number;
}

// =============================================================================
// HELPERS
// =============================================================================

function dataURLtoBlob(dataUrl: string): Blob {
  const arr = dataUrl.split(',');
  const mime = arr[0].match(/:(.*?);/)?.[1] || 'image/jpeg';
  const bstr = atob(arr[1]);
  let n = bstr.length;
  const u8arr = new Uint8Array(n);
  while (n--) {
    u8arr[n] = bstr.charCodeAt(n);
  }
  return new Blob([u8arr], { type: mime });
}

// =============================================================================
// CHALLENGE CONFIG
// =============================================================================

const ChallengeIcons: Record<string, React.ElementType> = {
  Blink: Eye,
  Smile: Smile,
  TurnLeft: ArrowLeft,
  TurnRight: ArrowRight,
  Nod: ChevronDown,
};

const ChallengeInstructions: Record<string, { title: string; instruction: string; tip: string }> = {
  Blink: {
    title: 'Parpadea',
    instruction: 'Parpadea 2 veces lentamente',
    tip: 'Cierra y abre los ojos de forma natural',
  },
  Smile: {
    title: 'Sonríe',
    instruction: 'Muestra una sonrisa natural',
    tip: 'Sonríe como si alguien te contara algo gracioso',
  },
  TurnLeft: {
    title: 'Gira a la izquierda',
    instruction: 'Gira tu cabeza hacia la izquierda',
    tip: 'Gira suavemente, no muy rápido',
  },
  TurnRight: {
    title: 'Gira a la derecha',
    instruction: 'Gira tu cabeza hacia la derecha',
    tip: 'Gira suavemente, no muy rápido',
  },
  Nod: {
    title: 'Asiente',
    instruction: 'Asiente con la cabeza (arriba-abajo)',
    tip: 'Mueve la cabeza como diciendo "sí"',
  },
};

const DEFAULT_CHALLENGES = ['Blink', 'Smile', 'TurnLeft'];

// =============================================================================
// COMPONENT
// =============================================================================

export function LivenessChallenge({
  requiredChallenges = DEFAULT_CHALLENGES,
  onComplete,
  onError,
  onCancel,
  isProcessing = false,
  className,
}: LivenessChallengeProps) {
  const webcamRef = React.useRef<Webcam>(null);

  const [cameraEnabled, setCameraEnabled] = React.useState(false); // Don't activate camera immediately
  const [hasCamera, setHasCamera] = React.useState(true);
  const [cameraReady, setCameraReady] = React.useState(false);
  const [challenges, setChallenges] = React.useState<ChallengeState[]>([]);
  const [currentChallengeIndex, setCurrentChallengeIndex] = React.useState(0);
  const [countdown, setCountdown] = React.useState<number | null>(null);
  const [challengeTimer, setChallengeTimer] = React.useState<number>(5);
  const [error, setError] = React.useState<string | null>(null);
  const [isCapturing, setIsCapturing] = React.useState(false);
  const [capturedFrames, setCapturedFrames] = React.useState<string[]>([]);
  const [finalSelfie, setFinalSelfie] = React.useState<string | null>(null);
  const [isComplete, setIsComplete] = React.useState(false);

  // Initialize challenges
  React.useEffect(() => {
    const initialChallenges: ChallengeState[] = requiredChallenges.map((type, index) => ({
      type,
      status: index === 0 ? 'pending' : 'pending',
    }));
    setChallenges(initialChallenges);
  }, [requiredChallenges]);

  // Video constraints
  const videoConstraints = {
    width: { ideal: 1280 },
    height: { ideal: 720 },
    facingMode: 'user',
    aspectRatio: { ideal: 4 / 3 },
  };

  // Check if camera permission is available (Permissions API)
  const checkCameraPermission = React.useCallback(async (): Promise<
    'granted' | 'denied' | 'prompt' | 'unknown'
  > => {
    try {
      // Permissions API is not available in all browsers
      if (!navigator.permissions || !navigator.permissions.query) {
        return 'unknown';
      }
      const result = await navigator.permissions.query({ name: 'camera' as PermissionName });
      return result.state;
    } catch {
      // Safari and some browsers don't support camera permission query
      return 'unknown';
    }
  }, []);

  // Request camera permission and enable camera
  const enableCamera = React.useCallback(async () => {
    try {
      // Check if mediaDevices is available (basic compatibility)
      if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        setHasCamera(false);
        setError(
          'Tu navegador no soporta acceso a la cámara. Por favor, usa Chrome, Firefox, Safari o Edge actualizado.'
        );
        onError?.('Camera not supported');
        return;
      }

      // Check permission status first (where available)
      const permissionStatus = await checkCameraPermission();
      if (permissionStatus === 'denied') {
        setHasCamera(false);
        setError(
          'El acceso a la cámara está bloqueado. Ve a la configuración de tu navegador para permitir el acceso a la cámara en este sitio.'
        );
        onError?.('Camera permission denied');
        return;
      }

      // Request camera access - use simple constraints first for better compatibility
      // Some browsers (especially mobile Safari) need simpler constraints initially
      let stream: MediaStream;
      try {
        stream = await navigator.mediaDevices.getUserMedia({
          video: { facingMode: 'user' },
        });
      } catch {
        // Fallback to even simpler constraints
        stream = await navigator.mediaDevices.getUserMedia({ video: true });
      }

      // Stop the test stream immediately
      stream.getTracks().forEach(track => track.stop());

      // Now enable the camera component
      setCameraEnabled(true);
      setError(null);
    } catch (err: unknown) {
      const error = err as Error;
      console.warn('Camera permission error:', error.name, error.message);

      setHasCamera(false);

      // Provide specific error messages based on the error type
      if (error.name === 'NotAllowedError' || error.name === 'PermissionDeniedError') {
        setError(
          'Acceso a la cámara denegado. Por favor, permite el acceso en la configuración de tu navegador y recarga la página.'
        );
      } else if (error.name === 'NotFoundError' || error.name === 'DevicesNotFoundError') {
        setError(
          'No se detectó ninguna cámara frontal. Por favor, verifica que tu dispositivo tenga cámara.'
        );
      } else if (error.name === 'NotReadableError' || error.name === 'TrackStartError') {
        setError(
          'La cámara está siendo usada por otra aplicación. Cierra otras apps que usen la cámara y vuelve a intentar.'
        );
      } else if (error.name === 'OverconstrainedError') {
        setError(
          'Tu cámara no cumple con los requisitos. Por favor, intenta desde otro dispositivo.'
        );
      } else if (error.name === 'SecurityError') {
        setError(
          'La cámara requiere una conexión segura (HTTPS). Por favor, accede desde una conexión segura.'
        );
      } else if (error.name === 'AbortError') {
        setError('Se canceló el acceso a la cámara. Por favor, intenta de nuevo.');
      } else if (error.name === 'TypeError') {
        setError('Error al configurar la cámara. Por favor, recarga la página e intenta de nuevo.');
      } else {
        setError(
          'No se pudo acceder a la cámara. Por favor, permite el acceso en tu navegador y recarga la página.'
        );
      }

      onError?.('Camera access denied');
    }
  }, [onError, checkCameraPermission]);

  // Handle camera errors from Webcam component
  const handleCameraError = React.useCallback(
    (err: string | DOMException) => {
      const errorName = typeof err === 'string' ? err : err.name;
      console.warn('Webcam component error:', errorName);

      setHasCamera(false);
      setCameraEnabled(false);

      // Try to give a helpful message
      if (errorName === 'NotAllowedError' || errorName.includes('Permission')) {
        setError(
          'Permiso de cámara denegado. Permite el acceso en la configuración del navegador.'
        );
      } else if (errorName === 'NotFoundError' || errorName.includes('NotFound')) {
        setError('No se encontró la cámara frontal.');
      } else {
        setError('Error al acceder a la cámara. Por favor, recarga la página.');
      }

      onError?.('Camera access denied');
    },
    [onError]
  );

  // Handle camera ready
  const handleUserMedia = React.useCallback(() => {
    setCameraReady(true);
    setError(null);
    // Start countdown
    setCountdown(3);
  }, []);

  // Capture a frame
  const captureFrame = React.useCallback(() => {
    if (!webcamRef.current) return;

    const frame = webcamRef.current.getScreenshot({
      width: 640,
      height: 480,
    });

    if (frame) {
      // Keep all frames for Rekognition analysis (up to 15 across all challenges)
      setCapturedFrames(prev => [...prev.slice(-14), frame]);
    }
  }, []);

  // Capture final selfie
  const captureFinalSelfie = React.useCallback(() => {
    if (!webcamRef.current) {
      setError('No se pudo capturar la selfie final');
      return;
    }

    const selfie = webcamRef.current.getScreenshot({
      width: 1280,
      height: 720,
    });

    if (!selfie) {
      setError('Error al capturar la selfie');
      return;
    }

    setFinalSelfie(selfie);
    setIsComplete(true);
  }, []);

  // Complete current challenge — NO hardcoded confidence.
  // The actual liveness confidence is determined by AWS Rekognition on the backend
  // when the captured frames are submitted via submitResults.
  const completeCurrentChallenge = React.useCallback(() => {
    setIsCapturing(false);

    // Mark challenge as completed with frames captured — real confidence comes from backend
    setChallenges(prev =>
      prev.map((c, i) =>
        i === currentChallengeIndex
          ? {
              ...c,
              status: 'completed',
              completedAt: new Date(),
              // No hardcoded confidence — backend will determine via AWS Rekognition
            }
          : c
      )
    );

    if (currentChallengeIndex < requiredChallenges.length - 1) {
      setCurrentChallengeIndex(prev => prev + 1);
      setChallengeTimer(5);
      setCountdown(2);
    } else {
      captureFinalSelfie();
    }
  }, [currentChallengeIndex, requiredChallenges.length, captureFinalSelfie]);

  // Start a challenge
  const startChallenge = React.useCallback(() => {
    setIsCapturing(true);
    setChallengeTimer(5);
    setChallenges(prev =>
      prev.map((c, i) =>
        i === currentChallengeIndex ? { ...c, status: 'active', startedAt: new Date() } : c
      )
    );
  }, [currentChallengeIndex]);

  // Countdown effect
  React.useEffect(() => {
    if (countdown === null) return;

    if (countdown > 0) {
      const timer = setTimeout(() => setCountdown(countdown - 1), 1000);
      return () => clearTimeout(timer);
    } else {
      setCountdown(null);
      startChallenge();
    }
  }, [countdown, startChallenge]);

  // Challenge timer effect — capture frames every second during challenge
  React.useEffect(() => {
    if (!isCapturing || challengeTimer <= 0) return;

    const timer = setInterval(() => {
      setChallengeTimer(prev => prev - 1);
      captureFrame();
    }, 1000);

    return () => clearInterval(timer);
  }, [isCapturing, challengeTimer, captureFrame]);

  // Handle challenge timer end — user must explicitly confirm
  // The challenge does NOT auto-pass; frames are captured and sent to backend for AWS Rekognition analysis
  React.useEffect(() => {
    if (challengeTimer === 0 && isCapturing) {
      // Timer expired — mark as needing confirmation
      // User must click "Listo, lo hice" to proceed
      // The captured frames will be sent to backend for real liveness verification
    }
  }, [challengeTimer, isCapturing]);

  // Manual confirm for challenge — user clicks "Listo, lo hice" after performing the action
  // Frames have been captured during the challenge timer; backend will validate via Rekognition
  const confirmChallenge = React.useCallback(() => {
    completeCurrentChallenge();
  }, [completeCurrentChallenge]);

  // Submit results — sends captured frames + selfie to backend
  // Backend will use AWS Rekognition to validate liveness from the video frames
  const submitResults = React.useCallback(async () => {
    if (!finalSelfie) return;

    try {
      const selfieBlob = dataURLtoBlob(finalSelfie);

      const livenessData: LivenessData = {
        challenges: challenges.map(c => ({
          type: c.type,
          passed: c.status === 'completed',
          timestamp: c.completedAt?.toISOString() || new Date().toISOString(),
          // confidence is NOT set here — backend determines it via AWS Rekognition
        })),
        // Send ALL captured frames for real Rekognition liveness analysis
        videoFrames: capturedFrames,
      };

      await onComplete(selfieBlob, livenessData);
    } catch (err) {
      console.error('Submit error:', err);
      setError('Error al enviar los datos. Por favor, intenta de nuevo.');
    }
  }, [finalSelfie, challenges, capturedFrames, onComplete]);

  // Retry all challenges
  const retryAll = React.useCallback(() => {
    setChallenges(requiredChallenges.map(type => ({ type, status: 'pending' })));
    setCurrentChallengeIndex(0);
    setCapturedFrames([]);
    setFinalSelfie(null);
    setIsComplete(false);
    setError(null);
    setCountdown(3);
  }, [requiredChallenges]);

  const currentChallenge = challenges[currentChallengeIndex];
  const currentChallengeInfo = currentChallenge
    ? ChallengeInstructions[currentChallenge.type]
    : null;
  const ChallengeIcon = currentChallenge ? ChallengeIcons[currentChallenge.type] || Eye : Eye;

  const completedCount = challenges.filter(c => c.status === 'completed').length;
  const progress = (completedCount / challenges.length) * 100;

  // =============================================================================
  // RENDER
  // =============================================================================

  return (
    <div className={cn('space-y-4', className)}>
      {/* Header */}
      <div className="text-center">
        <h3 className="text-foreground text-lg font-semibold">Verificación de Vida</h3>
        <p className="text-muted-foreground mt-1 text-sm">
          Completa los desafíos para verificar que eres una persona real
        </p>
      </div>

      {/* Progress */}
      <div className="space-y-2">
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Progreso</span>
          <span className="font-medium">
            {completedCount} de {challenges.length}
          </span>
        </div>
        <Progress value={progress} className="h-2" />

        {/* Challenge indicators */}
        <div className="mt-2 flex justify-center gap-2">
          {challenges.map((challenge, index) => {
            const Icon = ChallengeIcons[challenge.type] || Eye;
            return (
              <div
                key={index}
                className={cn(
                  'flex h-10 w-10 items-center justify-center rounded-full border-2 transition-all',
                  challenge.status === 'completed' && 'border-green-500 bg-green-500 text-white',
                  challenge.status === 'active' &&
                    'bg-primary border-primary text-primary-foreground animate-pulse',
                  challenge.status === 'failed' && 'border-red-500 bg-red-500 text-white',
                  challenge.status === 'pending' && 'bg-muted border-border text-muted-foreground'
                )}
              >
                {challenge.status === 'completed' ? (
                  <CheckCircle className="h-5 w-5" />
                ) : challenge.status === 'failed' ? (
                  <XCircle className="h-5 w-5" />
                ) : (
                  <Icon className="h-5 w-5" />
                )}
              </div>
            );
          })}
        </div>
      </div>

      {/* Camera Area */}
      <div className="relative aspect-[4/3] overflow-hidden rounded-xl bg-black">
        {isComplete && finalSelfie ? (
          // Show final selfie
          <Image src={finalSelfie} alt="Final selfie" fill className="object-cover" unoptimized />
        ) : !cameraEnabled ? (
          // Camera not activated yet - show activation button
          <div className="flex h-full flex-col items-center justify-center gap-4 p-8 text-white">
            <div className="rounded-full bg-white/10 p-6">
              <Camera className="h-16 w-16 text-white/80" />
            </div>
            <div className="text-center">
              <p className="text-lg font-medium">Verificación de vida</p>
              <p className="mt-1 text-sm text-gray-400">
                Activa la cámara para completar los desafíos de verificación
              </p>
            </div>
            <Button onClick={enableCamera} variant="secondary" size="lg" className="mt-2">
              <Camera className="mr-2 h-5 w-5" />
              Activar Cámara
            </Button>
          </div>
        ) : hasCamera ? (
          <>
            <Webcam
              ref={webcamRef}
              audio={false}
              screenshotFormat="image/jpeg"
              videoConstraints={videoConstraints}
              onUserMedia={handleUserMedia}
              onUserMediaError={handleCameraError}
              className="h-full w-full object-cover"
              mirrored
            />

            {/* Face outline guide */}
            {cameraReady && !isComplete && (
              <div className="pointer-events-none absolute inset-0">
                <div className="absolute top-1/2 left-1/2 h-64 w-48 -translate-x-1/2 -translate-y-1/2 transform rounded-full border-4 border-dashed border-white/50 md:h-80 md:w-64" />
              </div>
            )}

            {/* Countdown overlay */}
            {countdown !== null && countdown > 0 && (
              <div className="absolute inset-0 flex items-center justify-center bg-black/70">
                <div className="text-center text-white">
                  <div className="mb-4 text-7xl font-bold">{countdown}</div>
                  <p className="text-xl">Prepárate...</p>
                </div>
              </div>
            )}

            {/* Active challenge overlay */}
            {isCapturing && currentChallengeInfo && (
              <div className="absolute right-0 bottom-0 left-0 bg-gradient-to-t from-black/80 to-transparent p-6">
                <div className="text-center text-white">
                  <ChallengeIcon className="mx-auto mb-2 h-12 w-12" />
                  <h4 className="text-xl font-bold">{currentChallengeInfo.title}</h4>
                  <p className="mt-1 text-sm opacity-80">{currentChallengeInfo.instruction}</p>
                  <div className="mt-4">
                    <div className="h-2 overflow-hidden rounded-full bg-white/20">
                      <div
                        className="bg-primary h-full transition-all duration-1000"
                        style={{ width: `${(challengeTimer / 5) * 100}%` }}
                      />
                    </div>
                    <p className="mt-1 text-sm">{challengeTimer}s</p>
                  </div>
                </div>
              </div>
            )}
          </>
        ) : (
          <div className="flex h-full flex-col items-center justify-center text-white">
            <XCircle className="mb-4 h-16 w-16 text-red-400" />
            <p className="text-lg font-medium">Cámara no disponible</p>
            <p className="mt-2 text-sm text-gray-400">Se requiere cámara para la verificación</p>
          </div>
        )}

        {/* Processing overlay */}
        {isProcessing && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/70">
            <div className="text-center text-white">
              <Loader2 className="mx-auto mb-2 h-12 w-12 animate-spin" />
              <p>Verificando identidad...</p>
            </div>
          </div>
        )}
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-destructive/10 text-destructive flex items-center gap-2 rounded-lg p-3">
          <AlertTriangle className="h-5 w-5 flex-shrink-0" />
          <p className="text-sm">{error}</p>
        </div>
      )}

      {/* Instructions */}
      {cameraEnabled &&
        !isComplete &&
        currentChallengeInfo &&
        !isCapturing &&
        countdown === null && (
          <div className="bg-muted/50 rounded-lg p-4 text-center">
            <p className="text-muted-foreground text-sm">
              <strong>Siguiente:</strong> {currentChallengeInfo.instruction}
            </p>
            <p className="text-muted-foreground mt-1 text-xs">💡 {currentChallengeInfo.tip}</p>
          </div>
        )}

      {/* Action Buttons */}
      <div className="flex gap-3">
        {isComplete ? (
          <>
            <Button variant="outline" onClick={retryAll} disabled={isProcessing} className="flex-1">
              <Camera className="mr-2 h-4 w-4" />
              Volver a intentar
            </Button>
            <Button onClick={submitResults} disabled={isProcessing} className="flex-1">
              {isProcessing ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <CheckCircle className="mr-2 h-4 w-4" />
              )}
              Confirmar selfie
            </Button>
          </>
        ) : isCapturing ? (
          <Button onClick={confirmChallenge} className="w-full">
            <CheckCircle className="mr-2 h-4 w-4" />
            Listo, lo hice
          </Button>
        ) : !cameraEnabled ? (
          <>
            {onCancel && (
              <Button variant="outline" onClick={onCancel} className="flex-1">
                Cancelar
              </Button>
            )}
            <Button onClick={enableCamera} className="flex-1">
              <Camera className="mr-2 h-4 w-4" />
              Activar Cámara
            </Button>
          </>
        ) : (
          <>
            {onCancel && (
              <Button variant="outline" onClick={onCancel} className="flex-1">
                Cancelar
              </Button>
            )}
            {cameraReady && countdown === null && (
              <Button onClick={() => setCountdown(3)} className="flex-1">
                Comenzar verificación
              </Button>
            )}
          </>
        )}
      </div>
    </div>
  );
}

export default LivenessChallenge;

import React, { useRef, useState, useCallback, useEffect } from 'react';
import Webcam from 'react-webcam';
import {
  CheckCircle,
  XCircle,
  AlertTriangle,
  Loader2,
  Eye,
  Smile,
  RotateCcw,
  ArrowLeft,
  ArrowRight,
  ChevronDown,
} from 'lucide-react';
import { type LivenessData } from '../../services/identityVerificationService';

// ===== Types =====
interface LivenessChallengeProps {
  requiredChallenges: string[];
  onComplete: (selfie: Blob, livenessData: LivenessData) => Promise<void>;
  onError?: (error: string) => void;
  isProcessing?: boolean;
}

interface ChallengeState {
  type: string;
  status: 'pending' | 'active' | 'completed' | 'failed';
  startedAt?: Date;
  completedAt?: Date;
  confidence?: number;
}

// ===== Helper Functions =====
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

// ===== Challenge Icons =====
const ChallengeIcons: Record<string, React.FC<{ className?: string }>> = {
  Blink: Eye,
  Smile: Smile,
  TurnLeft: ArrowLeft,
  TurnRight: ArrowRight,
  Nod: ChevronDown,
  OpenMouth: () => (
    <svg
      className="w-12 h-12"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
    >
      <circle cx="12" cy="12" r="10" />
      <path d="M8 14s1.5 3 4 3 4-3 4-3" />
      <line x1="9" y1="9" x2="9.01" y2="9" />
      <line x1="15" y1="9" x2="15.01" y2="9" />
    </svg>
  ),
};

// ===== Challenge Instructions =====
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
  OpenMouth: {
    title: 'Abre la boca',
    instruction: 'Abre la boca como si dijeras "A"',
    tip: 'Abre la boca de forma natural',
  },
};

// ===== Component =====
export function LivenessChallenge({
  requiredChallenges,
  onComplete,
  onError,
  isProcessing = false,
}: LivenessChallengeProps) {
  const webcamRef = useRef<Webcam>(null);
  const [hasCamera, setHasCamera] = useState(true);
  const [cameraReady, setCameraReady] = useState(false);
  const [challenges, setChallenges] = useState<ChallengeState[]>([]);
  const [currentChallengeIndex, setCurrentChallengeIndex] = useState(0);
  const [countdown, setCountdown] = useState<number | null>(null);
  const [challengeTimer, setChallengeTimer] = useState<number>(5);
  const [error, setError] = useState<string | null>(null);
  const [isCapturing, setIsCapturing] = useState(false);
  const [capturedFrames, setCapturedFrames] = useState<string[]>([]);
  const [finalSelfie, setFinalSelfie] = useState<string | null>(null);
  const [isComplete, setIsComplete] = useState(false);

  // Initialize challenges
  useEffect(() => {
    const initialChallenges: ChallengeState[] = requiredChallenges.map((type, index) => ({
      type,
      status: index === 0 ? 'active' : 'pending',
    }));
    setChallenges(initialChallenges);
  }, [requiredChallenges]);

  // Video constraints optimized for face capture
  const videoConstraints = {
    width: { ideal: 1280 },
    height: { ideal: 720 },
    facingMode: 'user',
    aspectRatio: { ideal: 4 / 3 },
  };

  // Handle camera errors
  const handleCameraError = useCallback(
    (err: string | DOMException) => {
      console.error('Camera error:', err);
      setHasCamera(false);
      setError('No se pudo acceder a la cámara. Por favor, permite el acceso.');
      onError?.('Camera access denied');
    },
    [onError]
  );

  // Handle camera ready
  const handleUserMedia = useCallback(() => {
    setCameraReady(true);
    setError(null);
    // Start countdown before first challenge
    setCountdown(3);
  }, []);

  // Countdown effect
  useEffect(() => {
    if (countdown === null) return;

    if (countdown > 0) {
      const timer = setTimeout(() => setCountdown(countdown - 1), 1000);
      return () => clearTimeout(timer);
    } else {
      setCountdown(null);
      // Start the challenge
      startChallenge();
    }
  }, [countdown]);

  // Challenge timer effect
  useEffect(() => {
    if (!isCapturing || challengeTimer <= 0) return;

    const timer = setInterval(() => {
      setChallengeTimer((prev) => prev - 1);

      // Capture frame every second
      captureFrame();
    }, 1000);

    return () => clearInterval(timer);
  }, [isCapturing, challengeTimer]);

  // Handle challenge timer end
  useEffect(() => {
    if (challengeTimer === 0 && isCapturing) {
      completeCurrentChallenge(true);
    }
  }, [challengeTimer, isCapturing]);

  // Start a challenge
  const startChallenge = useCallback(() => {
    setIsCapturing(true);
    setChallengeTimer(5);
    setChallenges((prev) =>
      prev.map((c, i) =>
        i === currentChallengeIndex ? { ...c, status: 'active', startedAt: new Date() } : c
      )
    );
  }, [currentChallengeIndex]);

  // Capture a frame for liveness analysis
  const captureFrame = useCallback(() => {
    if (!webcamRef.current) return;

    const frame = webcamRef.current.getScreenshot({
      width: 640,
      height: 480,
    });

    if (frame) {
      setCapturedFrames((prev) => [...prev.slice(-4), frame]); // Keep last 5 frames
    }
  }, []);

  // Complete current challenge (simulated - in real app would use ML)
  const completeCurrentChallenge = useCallback(
    (passed: boolean) => {
      setIsCapturing(false);

      // Update challenge status
      setChallenges((prev) =>
        prev.map((c, i) =>
          i === currentChallengeIndex
            ? {
                ...c,
                status: passed ? 'completed' : 'failed',
                completedAt: new Date(),
                confidence: passed ? 0.95 : 0.3,
              }
            : c
        )
      );

      // Move to next challenge or complete
      if (currentChallengeIndex < requiredChallenges.length - 1) {
        setCurrentChallengeIndex((prev) => prev + 1);
        setChallenges((prev) =>
          prev.map((c, i) => (i === currentChallengeIndex + 1 ? { ...c, status: 'active' } : c))
        );
        setChallengeTimer(5);
        setCountdown(2); // Brief countdown between challenges
      } else {
        // All challenges complete - capture final selfie
        captureFinalSelfie();
      }
    },
    [currentChallengeIndex, requiredChallenges.length]
  );

  // Manual confirm for challenge (user indicates they completed the action)
  const confirmChallenge = useCallback(() => {
    completeCurrentChallenge(true);
  }, [completeCurrentChallenge]);

  // Capture final selfie
  const captureFinalSelfie = useCallback(() => {
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

  // Submit the results
  const submitResults = useCallback(async () => {
    if (!finalSelfie) return;

    try {
      const selfieBlob = dataURLtoBlob(finalSelfie);

      const livenessData: LivenessData = {
        challenges: challenges.map((c) => ({
          type: c.type,
          passed: c.status === 'completed',
          timestamp: c.completedAt?.toISOString() || new Date().toISOString(),
          confidence: c.confidence,
        })),
        videoFrames: capturedFrames.slice(0, 5), // Send up to 5 frames
      };

      await onComplete(selfieBlob, livenessData);
    } catch (err) {
      console.error('Submit error:', err);
      setError('Error al enviar los datos. Por favor, intenta de nuevo.');
    }
  }, [finalSelfie, challenges, capturedFrames, onComplete]);

  // Restart the process
  const restart = useCallback(() => {
    setChallenges(
      requiredChallenges.map((type, index) => ({
        type,
        status: index === 0 ? 'active' : 'pending',
      }))
    );
    setCurrentChallengeIndex(0);
    setChallengeTimer(5);
    setCapturedFrames([]);
    setFinalSelfie(null);
    setIsComplete(false);
    setError(null);
    setCountdown(3);
  }, [requiredChallenges]);

  // Get current challenge info
  const currentChallenge = challenges[currentChallengeIndex];
  const challengeInfo = currentChallenge ? ChallengeInstructions[currentChallenge.type] : null;
  const ChallengeIcon = currentChallenge ? ChallengeIcons[currentChallenge.type] : null;

  // Render progress bar
  const renderProgress = () => {
    return (
      <div className="flex items-center justify-center gap-2 mb-4">
        {challenges.map((challenge, index) => (
          <div key={index} className="flex items-center">
            <div
              className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium transition-all ${
                challenge.status === 'completed'
                  ? 'bg-green-500 text-white'
                  : challenge.status === 'failed'
                    ? 'bg-red-500 text-white'
                    : challenge.status === 'active'
                      ? 'bg-blue-500 text-white ring-4 ring-blue-200'
                      : 'bg-gray-200 text-gray-600'
              }`}
            >
              {challenge.status === 'completed' ? (
                <CheckCircle className="w-5 h-5" />
              ) : challenge.status === 'failed' ? (
                <XCircle className="w-5 h-5" />
              ) : (
                index + 1
              )}
            </div>
            {index < challenges.length - 1 && (
              <div
                className={`w-8 h-1 mx-1 ${
                  challenges[index].status === 'completed' ? 'bg-green-500' : 'bg-gray-200'
                }`}
              />
            )}
          </div>
        ))}
      </div>
    );
  };

  // Render face oval guide
  const renderFaceGuide = () => {
    return (
      <div className="absolute inset-0 pointer-events-none">
        {/* Semi-transparent overlay */}
        <div className="absolute inset-0 bg-black/30" />

        {/* Face oval cutout */}
        <div className="absolute inset-0 flex items-center justify-center">
          <div className="relative w-[60%] max-w-xs aspect-[3/4]">
            {/* Oval border */}
            <div className="absolute inset-0 border-4 border-white/80 rounded-[50%] shadow-lg">
              {/* Animated ring when active */}
              {isCapturing && (
                <div className="absolute inset-0 border-4 border-blue-400 rounded-[50%] animate-pulse" />
              )}
            </div>

            {/* Face position indicators */}
            <div className="absolute top-4 left-1/2 -translate-x-1/2 w-2 h-2 bg-white rounded-full" />
            <div className="absolute bottom-4 left-1/2 -translate-x-1/2 w-2 h-2 bg-white rounded-full" />
            <div className="absolute left-4 top-1/2 -translate-y-1/2 w-2 h-2 bg-white rounded-full" />
            <div className="absolute right-4 top-1/2 -translate-y-1/2 w-2 h-2 bg-white rounded-full" />
          </div>
        </div>

        {/* Challenge timer */}
        {isCapturing && (
          <div className="absolute bottom-4 left-1/2 -translate-x-1/2">
            <div className="bg-black/70 text-white px-4 py-2 rounded-full font-bold text-lg">
              {challengeTimer}s
            </div>
          </div>
        )}
      </div>
    );
  };

  // Render countdown overlay
  const renderCountdown = () => {
    if (countdown === null) return null;

    return (
      <div className="absolute inset-0 bg-black/60 flex items-center justify-center z-10">
        <div className="text-center">
          <div className="text-8xl font-bold text-white mb-4 animate-ping">{countdown}</div>
          <p className="text-white text-lg">Prepárate...</p>
        </div>
      </div>
    );
  };

  // Render completion screen
  if (isComplete && finalSelfie) {
    return (
      <div className="w-full max-w-2xl mx-auto">
        {/* Header */}
        <div className="mb-4 text-center">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <CheckCircle className="w-10 h-10 text-green-500" />
          </div>
          <h3 className="text-xl font-semibold text-gray-900">¡Verificación de vida completada!</h3>
          <p className="text-sm text-gray-600">Verifica que la foto sea clara</p>
        </div>

        {/* Results summary */}
        {renderProgress()}

        {/* Final selfie preview */}
        <div className="relative aspect-[4/3] bg-gray-900 rounded-xl overflow-hidden shadow-lg">
          <img
            src={finalSelfie}
            alt="Selfie final"
            className="w-full h-full object-cover"
            style={{ transform: 'scaleX(-1)' }}
          />

          {/* Processing overlay */}
          {isProcessing && (
            <div className="absolute inset-0 bg-black/50 flex flex-col items-center justify-center">
              <Loader2 className="w-12 h-12 text-white animate-spin mb-4" />
              <p className="text-white font-medium">Procesando verificación...</p>
              <p className="text-white/70 text-sm">Comparando rostro con documento</p>
            </div>
          )}
        </div>

        {/* Error message */}
        {error && (
          <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
            <XCircle className="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-700">{error}</p>
          </div>
        )}

        {/* Action buttons */}
        {!isProcessing && (
          <div className="mt-6 flex gap-4">
            <button
              onClick={restart}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg transition-colors"
            >
              <RotateCcw className="w-5 h-5" />
              Reiniciar
            </button>
            <button
              onClick={submitResults}
              disabled={isProcessing}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors disabled:opacity-50"
            >
              <CheckCircle className="w-5 h-5" />
              Continuar
            </button>
          </div>
        )}
      </div>
    );
  }

  // Render main challenge screen
  return (
    <div className="w-full max-w-2xl mx-auto">
      {/* Header */}
      <div className="mb-4 text-center">
        <h3 className="text-lg font-semibold text-gray-900">Verificación de vida</h3>
        <p className="text-sm text-gray-600">
          Sigue las instrucciones para verificar que eres una persona real
        </p>
      </div>

      {/* Progress */}
      {renderProgress()}

      {/* Current challenge instructions */}
      {challengeInfo && (
        <div className="mb-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
          <div className="flex items-center gap-3">
            {ChallengeIcon && (
              <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center text-blue-600">
                <ChallengeIcon className="w-6 h-6" />
              </div>
            )}
            <div>
              <h4 className="font-medium text-blue-900">{challengeInfo.title}</h4>
              <p className="text-sm text-blue-700">{challengeInfo.instruction}</p>
              <p className="text-xs text-blue-600 mt-1">💡 {challengeInfo.tip}</p>
            </div>
          </div>
        </div>
      )}

      {/* Camera view */}
      {hasCamera ? (
        <div className="relative aspect-[4/3] bg-gray-900 rounded-xl overflow-hidden shadow-lg">
          {/* Webcam */}
          <Webcam
            ref={webcamRef}
            audio={false}
            screenshotFormat="image/jpeg"
            screenshotQuality={0.9}
            videoConstraints={videoConstraints}
            onUserMedia={handleUserMedia}
            onUserMediaError={handleCameraError}
            className="w-full h-full object-cover"
            mirrored={true}
          />

          {/* Loading state */}
          {!cameraReady && (
            <div className="absolute inset-0 flex flex-col items-center justify-center bg-gray-900">
              <Loader2 className="w-12 h-12 text-white animate-spin mb-4" />
              <p className="text-white">Iniciando cámara...</p>
            </div>
          )}

          {/* Face guide overlay */}
          {cameraReady && !countdown && renderFaceGuide()}

          {/* Countdown overlay */}
          {renderCountdown()}

          {/* Challenge indicator */}
          {isCapturing && challengeInfo && ChallengeIcon && (
            <div className="absolute top-4 left-1/2 -translate-x-1/2 bg-black/70 text-white px-4 py-2 rounded-full flex items-center gap-2">
              <ChallengeIcon className="w-5 h-5" />
              <span className="font-medium">{challengeInfo.title}</span>
            </div>
          )}
        </div>
      ) : (
        <div className="aspect-[4/3] bg-gray-100 rounded-xl flex flex-col items-center justify-center p-8">
          <AlertTriangle className="w-16 h-16 text-red-400 mb-4" />
          <p className="text-gray-600 text-center mb-4">
            Se requiere acceso a la cámara para la verificación de vida.
          </p>
          <button
            onClick={() => window.location.reload()}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* Error message */}
      {error && (
        <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
          <XCircle className="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" />
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}

      {/* Confirm button during challenge */}
      {isCapturing && (
        <div className="mt-6">
          <button
            onClick={confirmChallenge}
            className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-green-600 hover:bg-green-700 text-white font-medium rounded-lg transition-colors"
          >
            <CheckCircle className="w-5 h-5" />
            Listo, completé la acción
          </button>
          <p className="text-xs text-gray-500 text-center mt-2">
            Presiona cuando hayas completado la acción, o espera el temporizador
          </p>
        </div>
      )}
    </div>
  );
}

export default LivenessChallenge;

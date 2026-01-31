import React, { useState, useEffect, useRef, useCallback } from 'react';
import { FiPlay, FiPause, FiMaximize2, FiMinimize2, FiRotateCw } from 'react-icons/fi';

export interface Spin360Data {
  vehicleId: string;
  frameUrls: string[];
  thumbnailUrl?: string;
  totalFrames: number;
}

interface AutoRotate360ViewerProps {
  spinData: Spin360Data;
  autoRotate?: boolean;
  rotationSpeed?: number; // segundos por vuelta completa
  onFrameChange?: (frameIndex: number) => void;
  onFullscreen?: () => void;
  className?: string;
}

export default function AutoRotate360Viewer({
  spinData,
  autoRotate = true,
  rotationSpeed = 10, // 10 segundos por vuelta completa
  onFrameChange,
  onFullscreen,
  className = '',
}: AutoRotate360ViewerProps) {
  const [currentFrame, setCurrentFrame] = useState(0);
  const [isPlaying, setIsPlaying] = useState(autoRotate);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [isDragging, setIsDragging] = useState(false);
  const [imagesLoaded, setImagesLoaded] = useState(false);
  const [loadedCount, setLoadedCount] = useState(0);

  const containerRef = useRef<HTMLDivElement>(null);
  const dragStartX = useRef(0);
  const lastFrameTime = useRef(0);
  const animationRef = useRef<number | null>(null);

  const frameCount = spinData.frameUrls.length;
  const frameInterval = (rotationSpeed * 1000) / frameCount;

  // Preload images
  useEffect(() => {
    let loaded = 0;
    const images: HTMLImageElement[] = [];

    spinData.frameUrls.forEach((url) => {
      const img = new Image();
      img.onload = () => {
        loaded++;
        setLoadedCount(loaded);
        if (loaded === frameCount) {
          setImagesLoaded(true);
        }
      };
      img.onerror = () => {
        loaded++;
        setLoadedCount(loaded);
      };
      img.src = url;
      images.push(img);
    });

    return () => {
      images.forEach((img) => {
        img.onload = null;
        img.onerror = null;
      });
    };
  }, [spinData.frameUrls, frameCount]);

  // Auto-rotation animation
  useEffect(() => {
    if (!isPlaying || !imagesLoaded || isDragging) {
      if (animationRef.current) {
        cancelAnimationFrame(animationRef.current);
      }
      return;
    }

    const animate = (timestamp: number) => {
      if (timestamp - lastFrameTime.current >= frameInterval) {
        setCurrentFrame((prev) => (prev + 1) % frameCount);
        lastFrameTime.current = timestamp;
      }
      animationRef.current = requestAnimationFrame(animate);
    };

    animationRef.current = requestAnimationFrame(animate);

    return () => {
      if (animationRef.current) {
        cancelAnimationFrame(animationRef.current);
      }
    };
  }, [isPlaying, imagesLoaded, isDragging, frameInterval, frameCount]);

  // Notify parent of frame changes
  useEffect(() => {
    onFrameChange?.(currentFrame);
  }, [currentFrame, onFrameChange]);

  // Handle mouse/touch drag for manual rotation
  const handleDragStart = useCallback((clientX: number) => {
    setIsDragging(true);
    setIsPlaying(false);
    dragStartX.current = clientX;
  }, []);

  const handleDragMove = useCallback(
    (clientX: number) => {
      if (!isDragging) return;

      const deltaX = clientX - dragStartX.current;
      const sensitivity = 5; // pixels per frame

      if (Math.abs(deltaX) >= sensitivity) {
        const frameDelta = Math.sign(deltaX);
        setCurrentFrame((prev) => (prev + frameDelta + frameCount) % frameCount);
        dragStartX.current = clientX;
      }
    },
    [isDragging, frameCount]
  );

  const handleDragEnd = useCallback(() => {
    setIsDragging(false);
  }, []);

  // Mouse events
  const handleMouseDown = (e: React.MouseEvent) => {
    e.preventDefault();
    handleDragStart(e.clientX);
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    handleDragMove(e.clientX);
  };

  const handleMouseUp = () => handleDragEnd();
  const handleMouseLeave = () => {
    if (isDragging) handleDragEnd();
  };

  // Touch events
  const handleTouchStart = (e: React.TouchEvent) => {
    handleDragStart(e.touches[0].clientX);
  };

  const handleTouchMove = (e: React.TouchEvent) => {
    handleDragMove(e.touches[0].clientX);
  };

  const handleTouchEnd = () => handleDragEnd();

  // Toggle play/pause
  const togglePlay = () => {
    setIsPlaying((prev) => !prev);
  };

  // Toggle fullscreen
  const toggleFullscreen = async () => {
    if (!containerRef.current) return;

    try {
      if (!document.fullscreenElement) {
        await containerRef.current.requestFullscreen();
        setIsFullscreen(true);
      } else {
        await document.exitFullscreen();
        setIsFullscreen(false);
      }
    } catch (err) {
      console.error('Fullscreen error:', err);
    }

    onFullscreen?.();
  };

  // Listen for fullscreen changes
  useEffect(() => {
    const handleFullscreenChange = () => {
      setIsFullscreen(!!document.fullscreenElement);
    };

    document.addEventListener('fullscreenchange', handleFullscreenChange);
    return () => document.removeEventListener('fullscreenchange', handleFullscreenChange);
  }, []);

  // Go to specific frame
  const goToFrame = (index: number) => {
    setCurrentFrame(index);
    setIsPlaying(false);
  };

  // Loading state
  if (!imagesLoaded) {
    return (
      <div className={`relative aspect-[16/9] bg-gray-100 rounded-xl overflow-hidden ${className}`}>
        <div className="absolute inset-0 flex flex-col items-center justify-center">
          <FiRotateCw className="w-12 h-12 text-blue-500 animate-spin mb-4" />
          <p className="text-gray-600 font-medium">Cargando vista 360°</p>
          <div className="w-48 h-2 bg-gray-200 rounded-full mt-3 overflow-hidden">
            <div
              className="h-full bg-blue-500 rounded-full transition-all duration-300"
              style={{ width: `${(loadedCount / frameCount) * 100}%` }}
            />
          </div>
          <p className="text-sm text-gray-500 mt-2">
            {loadedCount} / {frameCount} frames
          </p>
        </div>
      </div>
    );
  }

  return (
    <div
      ref={containerRef}
      className={`relative aspect-[16/9] bg-gradient-to-br from-gray-50 to-gray-100 rounded-xl overflow-hidden select-none ${className} ${isFullscreen ? 'rounded-none' : ''}`}
    >
      {/* Main Image */}
      <div
        className="absolute inset-0 cursor-grab active:cursor-grabbing"
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseLeave}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
      >
        {spinData.frameUrls.map((url, index) => (
          <img
            key={index}
            src={url}
            alt={`Vista 360° - Frame ${index + 1}`}
            className={`absolute inset-0 w-full h-full object-contain transition-opacity duration-100 ${
              index === currentFrame ? 'opacity-100' : 'opacity-0'
            }`}
            draggable={false}
          />
        ))}
      </div>

      {/* 360° Badge */}
      <div className="absolute top-4 left-4 flex items-center gap-2 px-3 py-1.5 bg-black/70 backdrop-blur-sm text-white rounded-full text-sm font-medium">
        <FiRotateCw
          className={`w-4 h-4 ${isPlaying ? 'animate-spin' : ''}`}
          style={{ animationDuration: `${rotationSpeed}s` }}
        />
        <span>360°</span>
      </div>

      {/* Controls */}
      <div className="absolute bottom-4 left-4 right-4 flex items-center justify-between">
        {/* Play/Pause + Info */}
        <div className="flex items-center gap-3">
          <button
            onClick={togglePlay}
            className="flex items-center gap-2 px-4 py-2 bg-black/70 backdrop-blur-sm text-white rounded-lg hover:bg-black/80 transition-colors"
          >
            {isPlaying ? (
              <>
                <FiPause className="w-4 h-4" />
                <span className="text-sm font-medium">Pausar</span>
              </>
            ) : (
              <>
                <FiPlay className="w-4 h-4" />
                <span className="text-sm font-medium">Rotar</span>
              </>
            )}
          </button>

          {!isPlaying && (
            <span className="text-sm text-white/80 bg-black/50 px-3 py-1 rounded-lg backdrop-blur-sm">
              Arrastra para girar
            </span>
          )}
        </div>

        {/* Fullscreen */}
        <button
          onClick={toggleFullscreen}
          className="p-2 bg-black/70 backdrop-blur-sm text-white rounded-lg hover:bg-black/80 transition-colors"
          aria-label={isFullscreen ? 'Salir de pantalla completa' : 'Pantalla completa'}
        >
          {isFullscreen ? <FiMinimize2 className="w-5 h-5" /> : <FiMaximize2 className="w-5 h-5" />}
        </button>
      </div>

      {/* Frame Indicators */}
      <div className="absolute bottom-16 left-1/2 -translate-x-1/2 flex items-center gap-1.5">
        {spinData.frameUrls.map((_, index) => (
          <button
            key={index}
            onClick={() => goToFrame(index)}
            className={`w-2 h-2 rounded-full transition-all duration-200 ${
              index === currentFrame ? 'bg-white w-4' : 'bg-white/50 hover:bg-white/75'
            }`}
            aria-label={`Ir al frame ${index + 1}`}
          />
        ))}
      </div>

      {/* Angle Labels (optional - shows on hover of frame indicators) */}
      <div className="absolute bottom-20 left-1/2 -translate-x-1/2">
        <span className="text-xs text-white/70 bg-black/40 px-2 py-0.5 rounded backdrop-blur-sm">
          {getAngleLabel(currentFrame, frameCount)}
        </span>
      </div>
    </div>
  );
}

// Helper function to get angle label
function getAngleLabel(frameIndex: number, totalFrames: number): string {
  const angle = Math.round((frameIndex / totalFrames) * 360);
  const labels: Record<number, string> = {
    0: 'Frontal',
    60: 'Frontal-Derecho',
    90: 'Lateral Derecho',
    120: 'Trasero-Derecho',
    180: 'Trasero',
    240: 'Trasero-Izquierdo',
    270: 'Lateral Izquierdo',
    300: 'Frontal-Izquierdo',
  };

  // Find closest angle
  const angles = Object.keys(labels).map(Number);
  const closest = angles.reduce((prev, curr) =>
    Math.abs(curr - angle) < Math.abs(prev - angle) ? curr : prev
  );

  return labels[closest] || `${angle}°`;
}

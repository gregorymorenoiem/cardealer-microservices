/**
 * Media360ViewerPage - Visor de fotos 360° de vehículos
 *
 * Permite visualizar vehículos con rotación 360° interactiva,
 * incluyendo vistas interiores y exteriores.
 *
 * Soporta dos modos:
 * - Option A: Embed Spyne viewer (spinViewerUrl) - iframe embed
 * - Option B: Custom viewer con extractedFrameUrls - rotación con imágenes
 *
 * @module pages/vehicles/Media360ViewerPage
 * @version 2.0.0
 * @since Enero 26, 2026
 */

import { useState, useRef, useEffect, useCallback } from 'react';
import { useParams, Link, useSearchParams } from 'react-router-dom';
import {
  FiRotateCw,
  FiZoomIn,
  FiZoomOut,
  FiMaximize2,
  FiMinimize2,
  FiArrowLeft,
  FiInfo,
  FiCamera,
  FiCircle,
  FiPlay,
  FiPause,
  FiRefreshCw,
  FiLoader,
  FiAlertCircle,
  FiExternalLink,
  FiLayers,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';
import {
  getVehicleViewer,
  getJobStatus,
  type Vehicle360ViewerData,
  type JobStatusResponse,
} from '../../services/vehicle360Service';

// Types - Usar tipo del servicio con alias local para compatibilidad
interface Video360SpinData {
  spinId: string;
  vehicleId: string;
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed';
  spinViewerUrl?: string;
  spinEmbedCode?: string;
  extractedFrameUrls: string[];
  extractedFrameCount: number;
  thumbnailUrl?: string;
  progressPercent: number;
  errorMessage?: string;
}

interface Hotspot {
  id: string;
  x: number;
  y: number;
  degrees: number; // Ángulo en el que aparece (0-360)
  label: string;
  description: string;
  type: 'feature' | 'damage' | 'upgrade' | 'info';
}

type ViewerMode = 'embed' | 'custom' | 'loading' | 'error';

const Media360ViewerPage = () => {
  const { slug } = useParams<{ slug: string }>();
  const [searchParams] = useSearchParams();
  const containerRef = useRef<HTMLDivElement>(null);
  const frameRefs = useRef<Map<number, HTMLImageElement>>(new Map());

  // Estado del visor
  const [viewerMode, setViewerMode] = useState<ViewerMode>('loading');
  const [spinData, setSpinData] = useState<Video360SpinData | null>(null);
  const [loadedFrames, setLoadedFrames] = useState<Set<number>>(new Set());
  const [preloadProgress, setPreloadProgress] = useState(0);

  // Estado de interacción
  const [currentFrame, setCurrentFrame] = useState(0);
  const [isAutoRotating, setIsAutoRotating] = useState(false);
  const [autoRotateSpeed, setAutoRotateSpeed] = useState(100); // ms per frame
  const [zoom, setZoom] = useState(1);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [isDragging, setIsDragging] = useState(false);
  const [startX, setStartX] = useState(0);
  const [showHotspots, setShowHotspots] = useState(true);
  const [activeHotspot, setActiveHotspot] = useState<Hotspot | null>(null);
  const [useEmbedViewer, setUseEmbedViewer] = useState(false);

  // Mock vehicle data (en producción vendría del API)
  const vehicle = {
    title: 'Toyota Camry 2023 XSE',
    slug: slug || 'toyota-camry-2023',
    price: 1850000,
    vehicleId: searchParams.get('vehicleId') || 'mock-vehicle-id',
  };

  // Mock hotspots (en producción vendrían del API junto con el spin)
  const hotspots: Hotspot[] = [
    {
      id: 'h1',
      x: 45,
      y: 60,
      degrees: 0,
      label: 'Faros LED',
      description: 'Faros LED adaptativos con luz diurna integrada',
      type: 'feature',
    },
    {
      id: 'h2',
      x: 75,
      y: 55,
      degrees: 90,
      label: 'Rines 19"',
      description: 'Rines de aleación de 19 pulgadas con diseño deportivo',
      type: 'feature',
    },
    {
      id: 'h3',
      x: 55,
      y: 35,
      degrees: 180,
      label: 'Techo solar',
      description: 'Techo panorámico de cristal con apertura eléctrica',
      type: 'upgrade',
    },
  ];

  const totalFrames = spinData?.extractedFrameCount || 36;
  const rotationAngle = Math.round((currentFrame / totalFrames) * 360);

  // Convertir Vehicle360ViewerData del servicio a formato local
  const mapViewerDataToSpinData = (data: Vehicle360ViewerData): Video360SpinData => ({
    spinId: data.viewId,
    vehicleId: data.vehicleId,
    status: data.status,
    spinViewerUrl: data.spinViewerUrl,
    spinEmbedCode: data.spinEmbedCode,
    extractedFrameUrls: data.extractedFrameUrls,
    extractedFrameCount: data.extractedFrameCount,
    thumbnailUrl: data.thumbnailUrl,
    progressPercent: data.progressPercent,
    errorMessage: data.errorMessage,
  });

  // Fetch spin data from API usando el servicio vehicle360Service
  useEffect(() => {
    const fetchSpinData = async () => {
      try {
        setViewerMode('loading');

        // Obtener datos del visor 360° usando el servicio correcto
        const viewerData = await getVehicleViewer(vehicle.vehicleId);
        const data = mapViewerDataToSpinData(viewerData);
        setSpinData(data);

        if (data.status === 'Completed') {
          // Decidir modo de visor
          if (data.extractedFrameUrls.length > 0) {
            setViewerMode('custom');
            // Precargar frames
            preloadFrames(data.extractedFrameUrls);
          } else if (data.spinViewerUrl) {
            setViewerMode('embed');
            setUseEmbedViewer(true);
          } else {
            setViewerMode('error');
          }
        } else if (data.status === 'Processing') {
          // Polling para status usando el servicio
          const pollInterval = setInterval(async () => {
            try {
              const statusData = await getJobStatus(data.spinId);
              // Actualizar progreso
              setSpinData((prev) =>
                prev
                  ? {
                      ...prev,
                      progressPercent: statusData.progress.percentage,
                      status: statusData.isComplete
                        ? 'Completed'
                        : statusData.isFailed
                          ? 'Failed'
                          : 'Processing',
                      errorMessage: statusData.errorMessage,
                    }
                  : null
              );

              if (statusData.isComplete) {
                clearInterval(pollInterval);
                // Recargar datos completos
                const updatedData = await getVehicleViewer(vehicle.vehicleId);
                const updated = mapViewerDataToSpinData(updatedData);
                setSpinData(updated);
                if (updated.extractedFrameUrls?.length > 0) {
                  setViewerMode('custom');
                  preloadFrames(updated.extractedFrameUrls);
                } else if (updated.spinViewerUrl) {
                  setViewerMode('embed');
                  setUseEmbedViewer(true);
                }
              } else if (statusData.isFailed) {
                clearInterval(pollInterval);
                setViewerMode('error');
              }
            } catch (pollError) {
              console.error('Error polling status:', pollError);
            }
          }, 3000);

          return () => clearInterval(pollInterval);
        } else if (data.status === 'Failed') {
          setViewerMode('error');
        }
      } catch (error) {
        console.error('Error fetching spin data:', error);
        // Fallback a mock data para demo
        loadMockData();
      }
    };

    fetchSpinData();
  }, [vehicle.vehicleId]);

  // Mock data para demo cuando no hay spin real
  const loadMockData = () => {
    const mockFrames = Array.from(
      { length: 36 },
      (_, i) =>
        `https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=1200&q=80&frame=${i}`
    );

    setSpinData({
      spinId: 'mock-spin',
      vehicleId: vehicle.vehicleId,
      status: 'Completed',
      extractedFrameUrls: mockFrames,
      extractedFrameCount: 36,
      thumbnailUrl: mockFrames[0],
      progressPercent: 100,
    });
    setViewerMode('custom');
    setPreloadProgress(100);
    setLoadedFrames(new Set(Array.from({ length: 36 }, (_, i) => i)));
  };

  // Preload frames for smooth rotation
  const preloadFrames = useCallback((urls: string[]) => {
    let loadedCount = 0;
    const newLoadedFrames = new Set<number>();

    urls.forEach((url, index) => {
      const img = new Image();
      img.onload = () => {
        loadedCount++;
        newLoadedFrames.add(index);
        setLoadedFrames(new Set(newLoadedFrames));
        setPreloadProgress(Math.round((loadedCount / urls.length) * 100));
        frameRefs.current.set(index, img);
      };
      img.onerror = () => {
        loadedCount++;
        setPreloadProgress(Math.round((loadedCount / urls.length) * 100));
      };
      img.src = url;
    });
  }, []);

  // Auto-rotation effect
  useEffect(() => {
    if (!isAutoRotating || viewerMode !== 'custom') return;

    const interval = setInterval(() => {
      setCurrentFrame((prev) => (prev + 1) % totalFrames);
    }, autoRotateSpeed);

    return () => clearInterval(interval);
  }, [isAutoRotating, totalFrames, autoRotateSpeed, viewerMode]);

  // Mouse/Touch handlers for rotation
  const handleMouseDown = (e: React.MouseEvent) => {
    if (viewerMode !== 'custom') return;
    setIsDragging(true);
    setStartX(e.clientX);
    setIsAutoRotating(false);
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isDragging || viewerMode !== 'custom') return;

    const deltaX = e.clientX - startX;
    const sensitivity = 5; // pixels per frame change
    if (Math.abs(deltaX) > sensitivity) {
      const frameChange = Math.sign(deltaX);
      setCurrentFrame((prev) => (prev + frameChange + totalFrames) % totalFrames);
      setStartX(e.clientX);
    }
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  const handleTouchStart = (e: React.TouchEvent) => {
    if (viewerMode !== 'custom') return;
    setIsDragging(true);
    setStartX(e.touches[0].clientX);
    setIsAutoRotating(false);
  };

  const handleTouchMove = (e: React.TouchEvent) => {
    if (!isDragging || viewerMode !== 'custom') return;

    const deltaX = e.touches[0].clientX - startX;
    if (Math.abs(deltaX) > 5) {
      const frameChange = Math.sign(deltaX);
      setCurrentFrame((prev) => (prev + frameChange + totalFrames) % totalFrames);
      setStartX(e.touches[0].clientX);
    }
  };

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (viewerMode !== 'custom') return;

      switch (e.key) {
        case 'ArrowLeft':
          setCurrentFrame((prev) => (prev - 1 + totalFrames) % totalFrames);
          setIsAutoRotating(false);
          break;
        case 'ArrowRight':
          setCurrentFrame((prev) => (prev + 1) % totalFrames);
          setIsAutoRotating(false);
          break;
        case ' ':
          e.preventDefault();
          setIsAutoRotating((prev) => !prev);
          break;
        case 'r':
        case 'R':
          resetView();
          break;
        case 'f':
        case 'F':
          toggleFullscreen();
          break;
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [viewerMode, totalFrames]);

  const handleZoomIn = () => setZoom((prev) => Math.min(prev + 0.25, 3));
  const handleZoomOut = () => setZoom((prev) => Math.max(prev - 0.25, 1));

  const toggleFullscreen = () => {
    if (!document.fullscreenElement) {
      containerRef.current?.requestFullscreen();
      setIsFullscreen(true);
    } else {
      document.exitFullscreen();
      setIsFullscreen(false);
    }
  };

  const resetView = () => {
    setCurrentFrame(0);
    setZoom(1);
    setIsAutoRotating(false);
  };

  // Filtrar hotspots visibles según el ángulo actual
  const visibleHotspots = hotspots.filter((h) => {
    const angleDiff = Math.abs(h.degrees - rotationAngle);
    return angleDiff < 30 || angleDiff > 330; // Visible ±30°
  });

  // Get current frame URL
  const currentFrameUrl = spinData?.extractedFrameUrls?.[currentFrame] || spinData?.thumbnailUrl;

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-900">
        {/* Header */}
        <div className="bg-gray-800 border-b border-gray-700 px-4 py-3">
          <div className="max-w-7xl mx-auto flex items-center justify-between">
            <div className="flex items-center">
              <Link
                to={`/vehicles/${vehicle.slug}`}
                className="text-gray-400 hover:text-white mr-4"
              >
                <FiArrowLeft className="w-5 h-5" />
              </Link>
              <div>
                <h1 className="text-white font-semibold">{vehicle.title}</h1>
                <p className="text-gray-400 text-sm">
                  Vista 360° interactiva
                  {spinData?.status === 'Processing' && (
                    <span className="ml-2 text-yellow-400">
                      <FiLoader className="inline animate-spin mr-1" />
                      Procesando...
                    </span>
                  )}
                </p>
              </div>
            </div>

            <div className="flex items-center space-x-3">
              {/* Viewer mode toggle */}
              {spinData?.spinViewerUrl && spinData?.extractedFrameUrls?.length > 0 && (
                <div className="flex bg-gray-700 rounded-lg p-1">
                  <button
                    onClick={() => setUseEmbedViewer(false)}
                    className={`px-3 py-1.5 rounded-md text-sm font-medium transition-colors flex items-center ${
                      !useEmbedViewer ? 'bg-blue-600 text-white' : 'text-gray-400 hover:text-white'
                    }`}
                    title="Visor personalizado con frames extraídos"
                  >
                    <FiLayers className="mr-1" />
                    Custom
                  </button>
                  <button
                    onClick={() => setUseEmbedViewer(true)}
                    className={`px-3 py-1.5 rounded-md text-sm font-medium transition-colors flex items-center ${
                      useEmbedViewer ? 'bg-blue-600 text-white' : 'text-gray-400 hover:text-white'
                    }`}
                    title="Visor embed de Spyne"
                  >
                    <FiExternalLink className="mr-1" />
                    Embed
                  </button>
                </div>
              )}

              <span className="text-gray-400 text-sm hidden sm:block">
                {viewerMode === 'custom' ? 'Arrastra para rotar • Flechas ← →' : ''}
              </span>
            </div>
          </div>
        </div>

        {/* Main Viewer */}
        <div
          ref={containerRef}
          className="relative w-full h-[calc(100vh-160px)] bg-gray-900 overflow-hidden select-none"
          onMouseDown={handleMouseDown}
          onMouseMove={handleMouseMove}
          onMouseUp={handleMouseUp}
          onMouseLeave={handleMouseUp}
          onTouchStart={handleTouchStart}
          onTouchMove={handleTouchMove}
          onTouchEnd={handleMouseUp}
          style={{
            cursor: viewerMode === 'custom' ? (isDragging ? 'grabbing' : 'grab') : 'default',
          }}
        >
          {/* Loading State */}
          {viewerMode === 'loading' && (
            <div className="absolute inset-0 flex items-center justify-center">
              <div className="text-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto mb-4" />
                <p className="text-gray-400">Cargando vista 360°...</p>
                {spinData?.status === 'Processing' && (
                  <div className="mt-4">
                    <div className="w-48 bg-gray-700 rounded-full h-2 mx-auto">
                      <div
                        className="bg-blue-500 h-2 rounded-full transition-all"
                        style={{ width: `${spinData.progressPercent}%` }}
                      />
                    </div>
                    <p className="text-gray-500 text-sm mt-2">
                      Procesando: {spinData.progressPercent}%
                    </p>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Error State */}
          {viewerMode === 'error' && (
            <div className="absolute inset-0 flex items-center justify-center">
              <div className="text-center max-w-md">
                <FiAlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
                <h3 className="text-white text-xl font-semibold mb-2">
                  Error al cargar vista 360°
                </h3>
                <p className="text-gray-400 mb-4">
                  {spinData?.errorMessage || 'No se pudo cargar la vista 360° del vehículo.'}
                </p>
                <button
                  onClick={() => window.location.reload()}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Reintentar
                </button>
              </div>
            </div>
          )}

          {/* Embed Viewer (Spyne iframe) */}
          {(viewerMode === 'embed' || useEmbedViewer) && spinData?.spinViewerUrl && (
            <div className="absolute inset-0">
              <iframe
                src={spinData.spinViewerUrl}
                className="w-full h-full border-0"
                title="360° Vehicle Viewer"
                allow="fullscreen; autoplay"
                loading="lazy"
              />
            </div>
          )}

          {/* Custom Frame Viewer */}
          {viewerMode === 'custom' && !useEmbedViewer && (
            <>
              {/* Preload Progress */}
              {preloadProgress < 100 && (
                <div className="absolute inset-0 flex items-center justify-center z-10 bg-gray-900/80">
                  <div className="text-center">
                    <div className="w-64 bg-gray-700 rounded-full h-2 mx-auto mb-3">
                      <div
                        className="bg-blue-500 h-2 rounded-full transition-all"
                        style={{ width: `${preloadProgress}%` }}
                      />
                    </div>
                    <p className="text-gray-400 text-sm">Cargando imágenes: {preloadProgress}%</p>
                  </div>
                </div>
              )}

              {/* 360 Frame Container */}
              <div
                className="absolute inset-0 flex items-center justify-center transition-transform duration-75"
                style={{ transform: `scale(${zoom})` }}
              >
                <div className="relative w-full max-w-5xl aspect-video bg-gray-800 rounded-lg overflow-hidden shadow-2xl">
                  {/* Current Frame Image */}
                  {currentFrameUrl && (
                    <img
                      src={currentFrameUrl}
                      alt={`${vehicle.title} - Vista 360° (${rotationAngle}°)`}
                      className="w-full h-full object-contain"
                      draggable={false}
                      loading="eager"
                    />
                  )}

                  {/* Rotation Compass Indicator */}
                  <div className="absolute bottom-4 right-4 pointer-events-none">
                    <div className="w-16 h-16 border-2 border-blue-500/40 rounded-full relative bg-gray-900/60">
                      <div className="absolute inset-0 flex items-center justify-center text-gray-400 text-xs font-mono">
                        {rotationAngle}°
                      </div>
                      <div
                        className="absolute w-1.5 h-6 bg-blue-500 rounded-full left-1/2 -ml-0.75"
                        style={{
                          transformOrigin: 'bottom center',
                          transform: `rotate(${rotationAngle}deg)`,
                          top: '4px',
                        }}
                      />
                    </div>
                  </div>

                  {/* Hotspots */}
                  {showHotspots &&
                    visibleHotspots.map((hotspot) => (
                      <button
                        key={hotspot.id}
                        className="absolute w-8 h-8 -ml-4 -mt-4 group z-20"
                        style={{ left: `${hotspot.x}%`, top: `${hotspot.y}%` }}
                        onClick={(e) => {
                          e.stopPropagation();
                          setActiveHotspot(activeHotspot?.id === hotspot.id ? null : hotspot);
                        }}
                      >
                        <span className="absolute inset-0 animate-ping bg-blue-500 rounded-full opacity-50" />
                        <span
                          className={`relative block w-8 h-8 rounded-full border-2 border-white shadow-lg flex items-center justify-center ${
                            hotspot.type === 'damage'
                              ? 'bg-red-600'
                              : hotspot.type === 'upgrade'
                                ? 'bg-green-600'
                                : 'bg-blue-600'
                          }`}
                        >
                          <FiInfo className="w-4 h-4 text-white" />
                        </span>
                      </button>
                    ))}
                </div>
              </div>

              {/* Hotspot Info Panel */}
              {activeHotspot && (
                <div className="absolute bottom-24 left-1/2 -translate-x-1/2 bg-white rounded-lg shadow-xl p-4 max-w-sm z-30 animate-in fade-in slide-in-from-bottom-4">
                  <div className="flex items-start">
                    <span
                      className={`w-3 h-3 rounded-full mr-2 mt-1 ${
                        activeHotspot.type === 'damage'
                          ? 'bg-red-500'
                          : activeHotspot.type === 'upgrade'
                            ? 'bg-green-500'
                            : 'bg-blue-500'
                      }`}
                    />
                    <div>
                      <h3 className="font-semibold text-gray-900">{activeHotspot.label}</h3>
                      <p className="text-sm text-gray-600 mt-1">{activeHotspot.description}</p>
                    </div>
                  </div>
                </div>
              )}

              {/* Frame Timeline (thumbnail strip) */}
              <div className="absolute bottom-20 left-1/2 -translate-x-1/2 bg-gray-800/90 backdrop-blur rounded-lg p-2 hidden lg:block">
                <div className="flex space-x-1 overflow-x-auto max-w-2xl">
                  {spinData?.extractedFrameUrls
                    ?.filter((_, i) => i % 4 === 0)
                    .map((url, idx) => {
                      const frameIndex = idx * 4;
                      return (
                        <button
                          key={frameIndex}
                          onClick={() => {
                            setCurrentFrame(frameIndex);
                            setIsAutoRotating(false);
                          }}
                          className={`flex-shrink-0 w-12 h-8 rounded overflow-hidden border-2 transition-all ${
                            Math.abs(currentFrame - frameIndex) < 2
                              ? 'border-blue-500 opacity-100'
                              : 'border-transparent opacity-60 hover:opacity-100'
                          }`}
                        >
                          <img src={url} alt="" className="w-full h-full object-cover" />
                        </button>
                      );
                    })}
                </div>
              </div>
            </>
          )}

          {/* Control Bar */}
          {viewerMode === 'custom' && !useEmbedViewer && (
            <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-gray-800/90 backdrop-blur rounded-lg px-4 py-3 flex items-center space-x-4">
              {/* Auto Rotate */}
              <button
                onClick={() => setIsAutoRotating(!isAutoRotating)}
                className={`p-2 rounded-lg transition-colors ${
                  isAutoRotating
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
                }`}
                title={isAutoRotating ? 'Detener (Espacio)' : 'Auto-rotar (Espacio)'}
              >
                {isAutoRotating ? <FiPause /> : <FiPlay />}
              </button>

              {/* Rotation Slider */}
              <div className="flex items-center space-x-2">
                <FiRotateCw className="text-gray-400" />
                <input
                  type="range"
                  min="0"
                  max={totalFrames - 1}
                  value={currentFrame}
                  onChange={(e) => {
                    setCurrentFrame(parseInt(e.target.value));
                    setIsAutoRotating(false);
                  }}
                  className="w-32 accent-blue-500"
                />
                <span className="text-gray-300 text-sm w-12">{rotationAngle}°</span>
              </div>

              {/* Speed Control */}
              <select
                value={autoRotateSpeed}
                onChange={(e) => setAutoRotateSpeed(parseInt(e.target.value))}
                className="bg-gray-700 text-gray-300 text-sm rounded-lg px-2 py-1.5 border-0"
                title="Velocidad de rotación"
              >
                <option value={200}>Lento</option>
                <option value={100}>Normal</option>
                <option value={50}>Rápido</option>
              </select>

              {/* Zoom Controls */}
              <div className="flex items-center space-x-1 border-l border-gray-600 pl-4">
                <button
                  onClick={handleZoomOut}
                  disabled={zoom <= 1}
                  className="p-2 rounded-lg bg-gray-700 text-gray-300 hover:bg-gray-600 disabled:opacity-50"
                  title="Alejar"
                >
                  <FiZoomOut />
                </button>
                <span className="text-gray-300 text-sm w-12 text-center">
                  {Math.round(zoom * 100)}%
                </span>
                <button
                  onClick={handleZoomIn}
                  disabled={zoom >= 3}
                  className="p-2 rounded-lg bg-gray-700 text-gray-300 hover:bg-gray-600 disabled:opacity-50"
                  title="Acercar"
                >
                  <FiZoomIn />
                </button>
              </div>

              {/* Hotspots Toggle */}
              <button
                onClick={() => setShowHotspots(!showHotspots)}
                className={`p-2 rounded-lg transition-colors border-l border-gray-600 pl-4 ${
                  showHotspots
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
                }`}
                title={showHotspots ? 'Ocultar hotspots' : 'Mostrar hotspots'}
              >
                <FiCircle />
              </button>

              {/* Reset */}
              <button
                onClick={resetView}
                className="p-2 rounded-lg bg-gray-700 text-gray-300 hover:bg-gray-600"
                title="Reiniciar (R)"
              >
                <FiRefreshCw />
              </button>

              {/* Fullscreen */}
              <button
                onClick={toggleFullscreen}
                className="p-2 rounded-lg bg-gray-700 text-gray-300 hover:bg-gray-600"
                title={isFullscreen ? 'Salir (F)' : 'Pantalla completa (F)'}
              >
                {isFullscreen ? <FiMinimize2 /> : <FiMaximize2 />}
              </button>
            </div>
          )}

          {/* Frame Counter */}
          {viewerMode === 'custom' && !useEmbedViewer && (
            <div className="absolute top-4 right-4 bg-gray-800/80 backdrop-blur rounded-lg px-3 py-2 text-gray-300 text-sm">
              Frame {currentFrame + 1} / {totalFrames}
            </div>
          )}
        </div>

        {/* Bottom Info Bar */}
        <div className="bg-gray-800 border-t border-gray-700 px-4 py-3">
          <div className="max-w-7xl mx-auto flex items-center justify-between">
            <div className="text-gray-400 text-sm flex items-center space-x-4">
              <span>
                <FiCamera className="inline mr-2" />
                {hotspots.length} puntos de interés
              </span>
              {spinData?.extractedFrameCount && (
                <span>
                  <FiLayers className="inline mr-2" />
                  {spinData.extractedFrameCount} imágenes
                </span>
              )}
            </div>
            <div className="flex items-center space-x-4">
              <Link
                to={`/vehicles/${vehicle.slug}/video`}
                className="text-blue-400 hover:text-blue-300 text-sm"
              >
                <FiPlay className="inline mr-1" />
                Ver video tour
              </Link>
              <Link
                to={`/vehicles/${vehicle.slug}`}
                className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 text-sm font-medium"
              >
                Ver detalles del vehículo
              </Link>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default Media360ViewerPage;

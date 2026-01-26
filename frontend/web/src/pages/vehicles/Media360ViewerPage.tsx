/**
 * Media360ViewerPage - Visor de fotos 360° de vehículos
 *
 * Permite visualizar vehículos con rotación 360° interactiva,
 * incluyendo vistas interiores y exteriores.
 *
 * Feature premium planificada para Q2 2026.
 *
 * @module pages/vehicles/Media360ViewerPage
 * @version 1.0.0
 * @since Enero 25, 2026
 */

import { useState, useRef, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import {
  FiRotateCw,
  FiZoomIn,
  FiZoomOut,
  FiMaximize2,
  FiMinimize2,
  FiArrowLeft,
  FiInfo,
  FiCamera,
  FiGrid,
  FiCircle,
  FiPlay,
  FiPause,
  FiRefreshCw,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

interface Hotspot {
  id: string;
  x: number;
  y: number;
  label: string;
  description: string;
}

interface View360 {
  id: string;
  name: string;
  type: 'exterior' | 'interior';
  frameCount: number;
  thumbnail: string;
  hotspots: Hotspot[];
}

const Media360ViewerPage = () => {
  const { slug } = useParams<{ slug: string }>();
  const containerRef = useRef<HTMLDivElement>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [currentView, setCurrentView] = useState<'exterior' | 'interior'>('exterior');
  const [currentFrame, setCurrentFrame] = useState(0);
  const [isAutoRotating, setIsAutoRotating] = useState(false);
  const [zoom, setZoom] = useState(1);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [isDragging, setIsDragging] = useState(false);
  const [startX, setStartX] = useState(0);
  const [showHotspots, setShowHotspots] = useState(true);
  const [activeHotspot, setActiveHotspot] = useState<Hotspot | null>(null);

  // Mock vehicle data
  const vehicle = {
    title: 'Toyota Camry 2023 XSE',
    slug: slug || 'toyota-camry-2023',
    price: 1850000,
  };

  // Mock 360 views
  const views: View360[] = [
    {
      id: '1',
      name: 'Exterior 360°',
      type: 'exterior',
      frameCount: 36, // 36 frames = 10° per frame
      thumbnail: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=200',
      hotspots: [
        {
          id: 'h1',
          x: 45,
          y: 60,
          label: 'Faros LED',
          description: 'Faros LED adaptativos con luz diurna integrada',
        },
        {
          id: 'h2',
          x: 75,
          y: 55,
          label: 'Rines 19"',
          description: 'Rines de aleación de 19 pulgadas con diseño deportivo',
        },
        {
          id: 'h3',
          x: 55,
          y: 35,
          label: 'Techo solar',
          description: 'Techo panorámico de cristal con apertura eléctrica',
        },
      ],
    },
    {
      id: '2',
      name: 'Interior 360°',
      type: 'interior',
      frameCount: 36,
      thumbnail: 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=200',
      hotspots: [
        {
          id: 'h4',
          x: 50,
          y: 45,
          label: 'Pantalla táctil',
          description: 'Pantalla táctil de 12.3" con Apple CarPlay y Android Auto',
        },
        {
          id: 'h5',
          x: 35,
          y: 50,
          label: 'Asientos de cuero',
          description: 'Asientos deportivos de cuero sintético con calefacción',
        },
        {
          id: 'h6',
          x: 65,
          y: 60,
          label: 'Consola central',
          description: 'Consola central con cargador inalámbrico y compartimentos',
        },
      ],
    },
  ];

  const currentViewData = views.find((v) => v.type === currentView) || views[0];
  const totalFrames = currentViewData.frameCount;

  // Simulate loading frames
  useEffect(() => {
    const timer = setTimeout(() => setIsLoading(false), 1000);
    return () => clearTimeout(timer);
  }, [currentView]);

  // Auto-rotation effect
  useEffect(() => {
    if (!isAutoRotating) return;

    const interval = setInterval(() => {
      setCurrentFrame((prev) => (prev + 1) % totalFrames);
    }, 100);

    return () => clearInterval(interval);
  }, [isAutoRotating, totalFrames]);

  // Mouse drag for rotation
  const handleMouseDown = (e: React.MouseEvent) => {
    setIsDragging(true);
    setStartX(e.clientX);
    setIsAutoRotating(false);
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isDragging) return;

    const deltaX = e.clientX - startX;
    if (Math.abs(deltaX) > 10) {
      const frameChange = deltaX > 0 ? 1 : -1;
      setCurrentFrame((prev) => (prev + frameChange + totalFrames) % totalFrames);
      setStartX(e.clientX);
    }
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  // Touch events for mobile
  const handleTouchStart = (e: React.TouchEvent) => {
    setIsDragging(true);
    setStartX(e.touches[0].clientX);
    setIsAutoRotating(false);
  };

  const handleTouchMove = (e: React.TouchEvent) => {
    if (!isDragging) return;

    const deltaX = e.touches[0].clientX - startX;
    if (Math.abs(deltaX) > 10) {
      const frameChange = deltaX > 0 ? 1 : -1;
      setCurrentFrame((prev) => (prev + frameChange + totalFrames) % totalFrames);
      setStartX(e.touches[0].clientX);
    }
  };

  const handleZoomIn = () => {
    setZoom((prev) => Math.min(prev + 0.25, 3));
  };

  const handleZoomOut = () => {
    setZoom((prev) => Math.max(prev - 0.25, 1));
  };

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

  // Calculate rotation angle for display
  const rotationAngle = Math.round((currentFrame / totalFrames) * 360);

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
                <p className="text-gray-400 text-sm">Vista 360° interactiva</p>
              </div>
            </div>
            <div className="flex items-center space-x-2">
              <span className="text-gray-400 text-sm hidden sm:block">Arrastra para rotar</span>
              <div className="flex bg-gray-700 rounded-lg p-1">
                <button
                  onClick={() => setCurrentView('exterior')}
                  className={`px-3 py-1.5 rounded-md text-sm font-medium transition-colors ${
                    currentView === 'exterior'
                      ? 'bg-blue-600 text-white'
                      : 'text-gray-400 hover:text-white'
                  }`}
                >
                  Exterior
                </button>
                <button
                  onClick={() => setCurrentView('interior')}
                  className={`px-3 py-1.5 rounded-md text-sm font-medium transition-colors ${
                    currentView === 'interior'
                      ? 'bg-blue-600 text-white'
                      : 'text-gray-400 hover:text-white'
                  }`}
                >
                  Interior
                </button>
              </div>
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
          style={{ cursor: isDragging ? 'grabbing' : 'grab' }}
        >
          {isLoading ? (
            <div className="absolute inset-0 flex items-center justify-center">
              <div className="text-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto mb-4" />
                <p className="text-gray-400">Cargando vista 360°...</p>
              </div>
            </div>
          ) : (
            <>
              {/* 360 Image Container */}
              <div
                className="absolute inset-0 flex items-center justify-center transition-transform duration-100"
                style={{
                  transform: `scale(${zoom})`,
                }}
              >
                {/* Placeholder for 360 frames - in production this would be actual images */}
                <div className="relative w-full max-w-4xl aspect-video bg-gray-800 rounded-lg overflow-hidden">
                  <img
                    src={currentViewData.thumbnail}
                    alt={`${vehicle.title} - ${currentView}`}
                    className="w-full h-full object-cover"
                    draggable={false}
                  />
                  {/* Rotation indicator overlay */}
                  <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <div className="w-24 h-24 border-4 border-blue-500/30 rounded-full relative">
                      <div
                        className="absolute w-2 h-2 bg-blue-500 rounded-full"
                        style={{
                          top: '50%',
                          left: '50%',
                          transform: `rotate(${rotationAngle}deg) translateY(-40px) translate(-50%, -50%)`,
                          transformOrigin: '0 0',
                        }}
                      />
                    </div>
                  </div>

                  {/* Hotspots */}
                  {showHotspots &&
                    currentViewData.hotspots.map((hotspot) => (
                      <button
                        key={hotspot.id}
                        className="absolute w-8 h-8 -ml-4 -mt-4 group"
                        style={{ left: `${hotspot.x}%`, top: `${hotspot.y}%` }}
                        onClick={(e) => {
                          e.stopPropagation();
                          setActiveHotspot(activeHotspot?.id === hotspot.id ? null : hotspot);
                        }}
                      >
                        <span className="absolute inset-0 animate-ping bg-blue-500 rounded-full opacity-50" />
                        <span className="relative block w-8 h-8 bg-blue-600 rounded-full border-2 border-white shadow-lg flex items-center justify-center">
                          <FiInfo className="w-4 h-4 text-white" />
                        </span>
                      </button>
                    ))}
                </div>
              </div>

              {/* Hotspot Info Panel */}
              {activeHotspot && (
                <div className="absolute bottom-24 left-1/2 -translate-x-1/2 bg-white rounded-lg shadow-xl p-4 max-w-sm animate-in fade-in slide-in-from-bottom-4">
                  <h3 className="font-semibold text-gray-900 mb-1">{activeHotspot.label}</h3>
                  <p className="text-sm text-gray-600">{activeHotspot.description}</p>
                </div>
              )}
            </>
          )}

          {/* Control Bar */}
          <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-gray-800/90 backdrop-blur rounded-lg px-4 py-3 flex items-center space-x-4">
            {/* Auto Rotate */}
            <button
              onClick={() => setIsAutoRotating(!isAutoRotating)}
              className={`p-2 rounded-lg transition-colors ${
                isAutoRotating
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
              }`}
              title={isAutoRotating ? 'Detener rotación' : 'Auto-rotar'}
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

            {/* Zoom Controls */}
            <div className="flex items-center space-x-1 border-l border-gray-600 pl-4">
              <button
                onClick={handleZoomOut}
                disabled={zoom <= 1}
                className="p-2 rounded-lg bg-gray-700 text-gray-300 hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <FiZoomOut />
              </button>
              <span className="text-gray-300 text-sm w-12 text-center">
                {Math.round(zoom * 100)}%
              </span>
              <button
                onClick={handleZoomIn}
                disabled={zoom >= 3}
                className="p-2 rounded-lg bg-gray-700 text-gray-300 hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed"
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
              title={showHotspots ? 'Ocultar puntos de interés' : 'Mostrar puntos de interés'}
            >
              <FiCircle />
            </button>

            {/* Reset */}
            <button
              onClick={resetView}
              className="p-2 rounded-lg bg-gray-700 text-gray-300 hover:bg-gray-600"
              title="Reiniciar vista"
            >
              <FiRefreshCw />
            </button>

            {/* Fullscreen */}
            <button
              onClick={toggleFullscreen}
              className="p-2 rounded-lg bg-gray-700 text-gray-300 hover:bg-gray-600"
              title={isFullscreen ? 'Salir de pantalla completa' : 'Pantalla completa'}
            >
              {isFullscreen ? <FiMinimize2 /> : <FiMaximize2 />}
            </button>
          </div>

          {/* Frame Counter */}
          <div className="absolute top-4 right-4 bg-gray-800/80 backdrop-blur rounded-lg px-3 py-2 text-gray-300 text-sm">
            Frame {currentFrame + 1} / {totalFrames}
          </div>

          {/* View Thumbnails */}
          <div className="absolute top-4 left-4 space-y-2">
            {views.map((view) => (
              <button
                key={view.id}
                onClick={() => setCurrentView(view.type)}
                className={`block w-20 h-14 rounded-lg overflow-hidden border-2 transition-all ${
                  currentView === view.type
                    ? 'border-blue-500 shadow-lg shadow-blue-500/30'
                    : 'border-transparent opacity-60 hover:opacity-100'
                }`}
              >
                <img src={view.thumbnail} alt={view.name} className="w-full h-full object-cover" />
              </button>
            ))}
          </div>
        </div>

        {/* Bottom Info Bar */}
        <div className="bg-gray-800 border-t border-gray-700 px-4 py-3">
          <div className="max-w-7xl mx-auto flex items-center justify-between">
            <div className="text-gray-400 text-sm">
              <FiCamera className="inline mr-2" />
              {currentViewData.hotspots.length} puntos de interés disponibles
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

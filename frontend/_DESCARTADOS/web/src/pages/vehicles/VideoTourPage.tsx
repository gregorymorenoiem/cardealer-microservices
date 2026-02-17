/**
 * VideoTourPage - Reproductor de video tour de vehículos
 *
 * Permite visualizar videos de walkaround grabados por vendedores/dealers,
 * incluyendo capítulos, hotspots temporales y controles avanzados.
 *
 * Feature premium planificada para Q2 2026.
 *
 * @module pages/vehicles/VideoTourPage
 * @version 1.0.0
 * @since Enero 25, 2026
 */

import { useState, useRef, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import {
  FiPlay,
  FiPause,
  FiVolume2,
  FiVolumeX,
  FiMaximize2,
  FiMinimize2,
  FiArrowLeft,
  FiSkipBack,
  FiSkipForward,
  FiSettings,
  FiShare2,
  FiHeart,
  FiMessageCircle,
  FiClock,
  FiUser,
  FiCheck,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

interface Chapter {
  id: string;
  title: string;
  startTime: number;
  endTime: number;
  thumbnail: string;
}

interface VideoData {
  id: string;
  url: string;
  poster: string;
  duration: number;
  views: number;
  uploadedAt: string;
  chapters: Chapter[];
  dealer: {
    name: string;
    avatar: string;
    verified: boolean;
  };
}

const VideoTourPage = () => {
  const { slug } = useParams<{ slug: string }>();
  const videoRef = useRef<HTMLVideoElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const progressRef = useRef<HTMLDivElement>(null);

  const [isPlaying, setIsPlaying] = useState(false);
  const [isMuted, setIsMuted] = useState(false);
  const [volume, setVolume] = useState(1);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [showControls, setShowControls] = useState(true);
  const [activeChapter, setActiveChapter] = useState<Chapter | null>(null);
  const [showSettings, setShowSettings] = useState(false);
  const [playbackSpeed, setPlaybackSpeed] = useState(1);
  const [quality, setQuality] = useState<'auto' | '1080p' | '720p' | '480p'>('auto');
  const [isFavorite, setIsFavorite] = useState(false);
  const [showChapters, setShowChapters] = useState(true);

  // Mock vehicle data
  const vehicle = {
    title: 'Toyota Camry 2023 XSE',
    slug: slug || 'toyota-camry-2023',
    price: 1850000,
    year: 2023,
  };

  // Mock video data
  const videoData: VideoData = {
    id: 'v1',
    url: 'https://sample-videos.com/video123/mp4/720/big_buck_bunny_720p_1mb.mp4',
    poster: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=1280',
    duration: 245, // 4:05
    views: 1234,
    uploadedAt: '2026-01-20T10:00:00',
    chapters: [
      {
        id: 'c1',
        title: 'Exterior frontal',
        startTime: 0,
        endTime: 45,
        thumbnail: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=200',
      },
      {
        id: 'c2',
        title: 'Lateral derecho',
        startTime: 45,
        endTime: 90,
        thumbnail: 'https://images.unsplash.com/photo-1606611013016-969c19ba27bb?w=200',
      },
      {
        id: 'c3',
        title: 'Parte trasera',
        startTime: 90,
        endTime: 120,
        thumbnail: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=200',
      },
      {
        id: 'c4',
        title: 'Lateral izquierdo',
        startTime: 120,
        endTime: 150,
        thumbnail: 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=200',
      },
      {
        id: 'c5',
        title: 'Interior - Dashboard',
        startTime: 150,
        endTime: 195,
        thumbnail: 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=200',
      },
      {
        id: 'c6',
        title: 'Interior - Asientos traseros',
        startTime: 195,
        endTime: 225,
        thumbnail: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=200',
      },
      {
        id: 'c7',
        title: 'Motor y maletero',
        startTime: 225,
        endTime: 245,
        thumbnail: 'https://images.unsplash.com/photo-1619682817481-e994891cd1f5?w=200',
      },
    ],
    dealer: {
      name: 'AutoMax RD',
      avatar: 'https://images.unsplash.com/photo-1560250097-0b93528c311a?w=100',
      verified: true,
    },
  };

  // Hide controls after inactivity
  useEffect(() => {
    let timeout: NodeJS.Timeout;

    const handleMouseMove = () => {
      setShowControls(true);
      clearTimeout(timeout);
      if (isPlaying) {
        timeout = setTimeout(() => setShowControls(false), 3000);
      }
    };

    const container = containerRef.current;
    container?.addEventListener('mousemove', handleMouseMove);

    return () => {
      container?.removeEventListener('mousemove', handleMouseMove);
      clearTimeout(timeout);
    };
  }, [isPlaying]);

  // Update active chapter based on current time
  useEffect(() => {
    const chapter = videoData.chapters.find(
      (c) => currentTime >= c.startTime && currentTime < c.endTime
    );
    setActiveChapter(chapter || null);
  }, [currentTime]);

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const handlePlayPause = () => {
    if (videoRef.current) {
      if (isPlaying) {
        videoRef.current.pause();
      } else {
        videoRef.current.play();
      }
      setIsPlaying(!isPlaying);
    }
  };

  const handleTimeUpdate = () => {
    if (videoRef.current) {
      setCurrentTime(videoRef.current.currentTime);
    }
  };

  const handleLoadedMetadata = () => {
    if (videoRef.current) {
      setDuration(videoRef.current.duration || videoData.duration);
    }
  };

  const handleProgressClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (progressRef.current && videoRef.current) {
      const rect = progressRef.current.getBoundingClientRect();
      const pos = (e.clientX - rect.left) / rect.width;
      const newTime = pos * (duration || videoData.duration);
      videoRef.current.currentTime = newTime;
      setCurrentTime(newTime);
    }
  };

  const handleVolumeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newVolume = parseFloat(e.target.value);
    setVolume(newVolume);
    if (videoRef.current) {
      videoRef.current.volume = newVolume;
    }
    setIsMuted(newVolume === 0);
  };

  const toggleMute = () => {
    if (videoRef.current) {
      videoRef.current.muted = !isMuted;
      setIsMuted(!isMuted);
    }
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

  const skipToChapter = (chapter: Chapter) => {
    if (videoRef.current) {
      videoRef.current.currentTime = chapter.startTime;
      setCurrentTime(chapter.startTime);
      if (!isPlaying) {
        videoRef.current.play();
        setIsPlaying(true);
      }
    }
  };

  const skip = (seconds: number) => {
    if (videoRef.current) {
      const newTime = Math.max(0, Math.min(duration, currentTime + seconds));
      videoRef.current.currentTime = newTime;
      setCurrentTime(newTime);
    }
  };

  const handleSpeedChange = (speed: number) => {
    setPlaybackSpeed(speed);
    if (videoRef.current) {
      videoRef.current.playbackRate = speed;
    }
    setShowSettings(false);
  };

  const progress = duration ? (currentTime / duration) * 100 : 0;

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
                <p className="text-gray-400 text-sm">Video Tour</p>
              </div>
            </div>
            <div className="flex items-center space-x-3">
              <button
                onClick={() => setIsFavorite(!isFavorite)}
                className={`p-2 rounded-lg transition-colors ${
                  isFavorite
                    ? 'bg-red-600 text-white'
                    : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
                }`}
              >
                <FiHeart className={isFavorite ? 'fill-current' : ''} />
              </button>
              <button className="p-2 rounded-lg bg-gray-700 text-gray-300 hover:bg-gray-600">
                <FiShare2 />
              </button>
              <Link
                to={`/contact/${vehicle.slug}`}
                className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 text-sm font-medium"
              >
                <FiMessageCircle className="mr-2" />
                Contactar
              </Link>
            </div>
          </div>
        </div>

        <div className="max-w-7xl mx-auto px-4 py-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Video Player */}
            <div className="lg:col-span-2">
              <div
                ref={containerRef}
                className="relative bg-black rounded-lg overflow-hidden aspect-video group"
              >
                {/* Video Element - Using poster for demo */}
                <div
                  className="absolute inset-0 bg-cover bg-center"
                  style={{ backgroundImage: `url(${videoData.poster})` }}
                >
                  {/* Demo overlay - in production this would be actual video */}
                  <div className="absolute inset-0 bg-black/20 flex items-center justify-center">
                    {!isPlaying && (
                      <button
                        onClick={handlePlayPause}
                        className="w-20 h-20 bg-white/20 backdrop-blur rounded-full flex items-center justify-center hover:bg-white/30 transition-colors"
                      >
                        <FiPlay className="w-8 h-8 text-white ml-1" />
                      </button>
                    )}
                  </div>
                </div>

                {/* Video Controls */}
                <div
                  className={`absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/80 to-transparent p-4 transition-opacity ${
                    showControls ? 'opacity-100' : 'opacity-0'
                  }`}
                >
                  {/* Progress Bar */}
                  <div
                    ref={progressRef}
                    className="h-1 bg-gray-600 rounded-full mb-4 cursor-pointer group/progress"
                    onClick={handleProgressClick}
                  >
                    <div
                      className="h-full bg-blue-500 rounded-full relative"
                      style={{ width: `${progress}%` }}
                    >
                      <div className="absolute right-0 top-1/2 -translate-y-1/2 w-3 h-3 bg-blue-500 rounded-full opacity-0 group-hover/progress:opacity-100" />
                    </div>
                    {/* Chapter markers */}
                    {videoData.chapters.map((chapter) => (
                      <div
                        key={chapter.id}
                        className="absolute top-0 w-0.5 h-full bg-white/50"
                        style={{ left: `${(chapter.startTime / duration) * 100}%` }}
                        title={chapter.title}
                      />
                    ))}
                  </div>

                  {/* Controls Row */}
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-4">
                      {/* Play/Pause */}
                      <button onClick={handlePlayPause} className="text-white hover:text-blue-400">
                        {isPlaying ? <FiPause size={24} /> : <FiPlay size={24} />}
                      </button>

                      {/* Skip Buttons */}
                      <button onClick={() => skip(-10)} className="text-white hover:text-blue-400">
                        <FiSkipBack size={20} />
                      </button>
                      <button onClick={() => skip(10)} className="text-white hover:text-blue-400">
                        <FiSkipForward size={20} />
                      </button>

                      {/* Volume */}
                      <div className="flex items-center space-x-2">
                        <button onClick={toggleMute} className="text-white hover:text-blue-400">
                          {isMuted || volume === 0 ? (
                            <FiVolumeX size={20} />
                          ) : (
                            <FiVolume2 size={20} />
                          )}
                        </button>
                        <input
                          type="range"
                          min="0"
                          max="1"
                          step="0.1"
                          value={isMuted ? 0 : volume}
                          onChange={handleVolumeChange}
                          className="w-20 accent-blue-500"
                        />
                      </div>

                      {/* Time */}
                      <span className="text-white text-sm">
                        {formatTime(currentTime)} / {formatTime(duration || videoData.duration)}
                      </span>
                    </div>

                    <div className="flex items-center space-x-3">
                      {/* Settings */}
                      <div className="relative">
                        <button
                          onClick={() => setShowSettings(!showSettings)}
                          className="text-white hover:text-blue-400"
                        >
                          <FiSettings size={20} />
                        </button>
                        {showSettings && (
                          <div className="absolute bottom-full right-0 mb-2 bg-gray-800 rounded-lg shadow-lg p-3 min-w-[160px]">
                            <div className="text-gray-400 text-xs uppercase mb-2">Velocidad</div>
                            {[0.5, 0.75, 1, 1.25, 1.5, 2].map((speed) => (
                              <button
                                key={speed}
                                onClick={() => handleSpeedChange(speed)}
                                className={`block w-full text-left px-2 py-1 text-sm rounded ${
                                  playbackSpeed === speed
                                    ? 'bg-blue-600 text-white'
                                    : 'text-gray-300 hover:bg-gray-700'
                                }`}
                              >
                                {speed}x
                              </button>
                            ))}
                            <div className="border-t border-gray-700 my-2" />
                            <div className="text-gray-400 text-xs uppercase mb-2">Calidad</div>
                            {['auto', '1080p', '720p', '480p'].map((q) => (
                              <button
                                key={q}
                                onClick={() => setQuality(q as typeof quality)}
                                className={`block w-full text-left px-2 py-1 text-sm rounded ${
                                  quality === q
                                    ? 'bg-blue-600 text-white'
                                    : 'text-gray-300 hover:bg-gray-700'
                                }`}
                              >
                                {q === 'auto' ? 'Auto' : q}
                              </button>
                            ))}
                          </div>
                        )}
                      </div>

                      {/* Fullscreen */}
                      <button onClick={toggleFullscreen} className="text-white hover:text-blue-400">
                        {isFullscreen ? <FiMinimize2 size={20} /> : <FiMaximize2 size={20} />}
                      </button>
                    </div>
                  </div>
                </div>

                {/* Current Chapter Indicator */}
                {activeChapter && (
                  <div className="absolute top-4 left-4 bg-black/60 backdrop-blur rounded-lg px-3 py-2 text-white text-sm">
                    {activeChapter.title}
                  </div>
                )}
              </div>

              {/* Video Info */}
              <div className="mt-4 bg-gray-800 rounded-lg p-4">
                <div className="flex items-center justify-between mb-4">
                  <div>
                    <h2 className="text-xl font-semibold text-white">{vehicle.title}</h2>
                    <div className="flex items-center space-x-4 mt-1 text-gray-400 text-sm">
                      <span>{videoData.views.toLocaleString()} vistas</span>
                      <span>•</span>
                      <span>{new Date(videoData.uploadedAt).toLocaleDateString('es-DO')}</span>
                    </div>
                  </div>
                  <div className="text-2xl font-bold text-blue-400">
                    RD${(vehicle.price / 1000).toFixed(0)}K
                  </div>
                </div>

                {/* Dealer Info */}
                <div className="flex items-center justify-between pt-4 border-t border-gray-700">
                  <div className="flex items-center">
                    <img
                      src={videoData.dealer.avatar}
                      alt={videoData.dealer.name}
                      className="w-10 h-10 rounded-full mr-3"
                    />
                    <div>
                      <div className="flex items-center">
                        <span className="text-white font-medium">{videoData.dealer.name}</span>
                        {videoData.dealer.verified && (
                          <span className="ml-1 w-4 h-4 bg-blue-500 rounded-full flex items-center justify-center">
                            <FiCheck className="w-3 h-3 text-white" />
                          </span>
                        )}
                      </div>
                      <span className="text-gray-400 text-sm">Dealer verificado</span>
                    </div>
                  </div>
                  <Link
                    to={`/dealers/${videoData.dealer.name.toLowerCase().replace(' ', '-')}`}
                    className="text-blue-400 hover:text-blue-300 text-sm"
                  >
                    Ver perfil
                  </Link>
                </div>
              </div>
            </div>

            {/* Chapters Sidebar */}
            <div className="lg:col-span-1">
              <div className="bg-gray-800 rounded-lg overflow-hidden">
                <div className="flex items-center justify-between px-4 py-3 border-b border-gray-700">
                  <h3 className="text-white font-semibold">Capítulos</h3>
                  <button
                    onClick={() => setShowChapters(!showChapters)}
                    className="text-gray-400 hover:text-white text-sm"
                  >
                    {showChapters ? 'Ocultar' : 'Mostrar'}
                  </button>
                </div>
                {showChapters && (
                  <div className="max-h-[500px] overflow-y-auto">
                    {videoData.chapters.map((chapter, index) => (
                      <button
                        key={chapter.id}
                        onClick={() => skipToChapter(chapter)}
                        className={`w-full flex items-start p-3 hover:bg-gray-700/50 transition-colors ${
                          activeChapter?.id === chapter.id ? 'bg-gray-700' : ''
                        }`}
                      >
                        <div className="relative flex-shrink-0 w-24 h-14 rounded overflow-hidden mr-3">
                          <img
                            src={chapter.thumbnail}
                            alt={chapter.title}
                            className="w-full h-full object-cover"
                          />
                          <div className="absolute bottom-1 right-1 bg-black/80 text-white text-xs px-1 rounded">
                            {formatTime(chapter.startTime)}
                          </div>
                          {activeChapter?.id === chapter.id && (
                            <div className="absolute inset-0 border-2 border-blue-500 rounded" />
                          )}
                        </div>
                        <div className="flex-1 text-left">
                          <div className="text-white text-sm font-medium line-clamp-2">
                            {index + 1}. {chapter.title}
                          </div>
                          <div className="text-gray-400 text-xs mt-1">
                            {formatTime(chapter.endTime - chapter.startTime)}
                          </div>
                        </div>
                      </button>
                    ))}
                  </div>
                )}
              </div>

              {/* Related Actions */}
              <div className="mt-4 space-y-3">
                <Link
                  to={`/vehicles/${vehicle.slug}/360`}
                  className="flex items-center justify-between w-full bg-gray-800 rounded-lg p-4 hover:bg-gray-700 transition-colors"
                >
                  <div className="flex items-center">
                    <div className="w-10 h-10 bg-purple-600 rounded-lg flex items-center justify-center mr-3">
                      <FiMaximize2 className="text-white" />
                    </div>
                    <div>
                      <div className="text-white font-medium">Vista 360°</div>
                      <div className="text-gray-400 text-sm">Explora el vehículo en 360°</div>
                    </div>
                  </div>
                  <FiArrowLeft className="text-gray-400 rotate-180" />
                </Link>

                <Link
                  to={`/vehicles/${vehicle.slug}`}
                  className="flex items-center justify-between w-full bg-gray-800 rounded-lg p-4 hover:bg-gray-700 transition-colors"
                >
                  <div className="flex items-center">
                    <div className="w-10 h-10 bg-blue-600 rounded-lg flex items-center justify-center mr-3">
                      <FiUser className="text-white" />
                    </div>
                    <div>
                      <div className="text-white font-medium">Ver detalles</div>
                      <div className="text-gray-400 text-sm">Especificaciones completas</div>
                    </div>
                  </div>
                  <FiArrowLeft className="text-gray-400 rotate-180" />
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default VideoTourPage;

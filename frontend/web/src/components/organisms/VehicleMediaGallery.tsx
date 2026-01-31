import React, { useState, useEffect } from 'react';
import {
  FiX,
  FiChevronLeft,
  FiChevronRight,
  FiRotateCw,
  FiHome,
  FiCamera,
  FiExternalLink,
} from 'react-icons/fi';
import { Link } from 'react-router-dom';
import AutoRotate360Viewer, { type Spin360Data } from './AutoRotate360Viewer';
import { getVehicleViewer, type Vehicle360ViewerData } from '@/services/vehicle360Service';

type MediaTab = 'exterior360' | 'interior' | 'photos';

interface VehicleMediaGalleryProps {
  vehicleId: string;
  vehicleSlug: string;
  images: string[];
  interiorImages?: string[];
  alt: string;
}

export default function VehicleMediaGallery({
  vehicleId,
  vehicleSlug,
  images,
  interiorImages = [],
  alt,
}: VehicleMediaGalleryProps) {
  const [activeTab, setActiveTab] = useState<MediaTab>('exterior360');
  const [spin360Data, setSpin360Data] = useState<Spin360Data | null>(null);
  const [isLoading360, setIsLoading360] = useState(true);
  const [has360View, setHas360View] = useState(false);

  // For traditional gallery
  const [selectedImage, setSelectedImage] = useState(0);
  const [lightboxOpen, setLightboxOpen] = useState(false);

  // Separate images by type (heuristic: first 5-6 are exterior, rest are interior if not provided separately)
  const exteriorImages = images.slice(0, Math.min(6, images.length));
  const derivedInteriorImages = interiorImages.length > 0 ? interiorImages : images.slice(6);

  // Fetch 360 data
  useEffect(() => {
    const fetch360Data = async () => {
      try {
        setIsLoading360(true);
        const response: Vehicle360ViewerData = await getVehicleViewer(vehicleId);

        if (response.extractedFrameUrls && response.extractedFrameUrls.length > 0) {
          setSpin360Data({
            vehicleId: response.vehicleId,
            frameUrls: response.extractedFrameUrls,
            thumbnailUrl: response.thumbnailUrl,
            totalFrames: response.extractedFrameCount,
          });
          setHas360View(true);
          setActiveTab('exterior360');
        } else {
          setHas360View(false);
          setActiveTab('photos');
        }
      } catch {
        // No 360 view available, fall back to photos
        setHas360View(false);
        setActiveTab('photos');
      } finally {
        setIsLoading360(false);
      }
    };

    fetch360Data();
  }, [vehicleId]);

  // Get current gallery images based on tab
  const currentGalleryImages = activeTab === 'interior' ? derivedInteriorImages : exteriorImages;

  // Navigation for traditional gallery
  const nextImage = () => {
    setSelectedImage((prev) => (prev + 1) % currentGalleryImages.length);
  };

  const previousImage = () => {
    setSelectedImage(
      (prev) => (prev - 1 + currentGalleryImages.length) % currentGalleryImages.length
    );
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') setLightboxOpen(false);
    if (e.key === 'ArrowRight') nextImage();
    if (e.key === 'ArrowLeft') previousImage();
  };

  // Tab definitions
  const tabs = [
    {
      id: 'exterior360' as MediaTab,
      label: '360° Exterior',
      icon: FiRotateCw,
      available: has360View,
      badge: has360View ? '360°' : null,
    },
    {
      id: 'interior' as MediaTab,
      label: 'Interior',
      icon: FiHome,
      available: derivedInteriorImages.length > 0,
      badge: derivedInteriorImages.length > 0 ? `${derivedInteriorImages.length}` : null,
    },
    {
      id: 'photos' as MediaTab,
      label: 'Fotos',
      icon: FiCamera,
      available: images.length > 0,
      badge: images.length > 0 ? `+${images.length}` : null,
    },
  ].filter((tab) => tab.available);

  return (
    <div className="space-y-4">
      {/* Tab Navigation */}
      <div className="flex items-center gap-2 overflow-x-auto pb-2">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => {
              setActiveTab(tab.id);
              setSelectedImage(0);
            }}
            className={`
              flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-all whitespace-nowrap
              ${
                activeTab === tab.id
                  ? 'bg-blue-600 text-white shadow-lg'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }
            `}
          >
            <tab.icon className="w-4 h-4" />
            <span>{tab.label}</span>
            {tab.badge && (
              <span
                className={`
                px-1.5 py-0.5 text-xs rounded-full
                ${activeTab === tab.id ? 'bg-white/20' : 'bg-gray-200'}
              `}
              >
                {tab.badge}
              </span>
            )}
          </button>
        ))}

        {/* Link to full 360 viewer */}
        {has360View && (
          <Link
            to={`/vehicles/${vehicleSlug}/360`}
            className="ml-auto flex items-center gap-2 px-4 py-2 text-blue-600 hover:text-blue-700 font-medium"
          >
            <FiExternalLink className="w-4 h-4" />
            <span className="hidden sm:inline">Ver 360° completo</span>
          </Link>
        )}
      </div>

      {/* Content Area */}
      <div className="relative">
        {/* 360° Exterior Tab */}
        {activeTab === 'exterior360' && (
          <>
            {isLoading360 ? (
              <div className="aspect-[16/9] bg-gray-100 rounded-xl flex items-center justify-center">
                <div className="text-center">
                  <FiRotateCw className="w-12 h-12 text-blue-500 animate-spin mx-auto mb-4" />
                  <p className="text-gray-600">Cargando vista 360°...</p>
                </div>
              </div>
            ) : spin360Data ? (
              <AutoRotate360Viewer spinData={spin360Data} autoRotate={true} rotationSpeed={10} />
            ) : (
              <div className="aspect-[16/9] bg-gray-100 rounded-xl flex items-center justify-center">
                <p className="text-gray-500">Vista 360° no disponible</p>
              </div>
            )}
          </>
        )}

        {/* Interior / Photos Tab - Traditional Gallery */}
        {(activeTab === 'interior' || activeTab === 'photos') && (
          <>
            {/* Main Image */}
            <div
              className="relative aspect-[16/9] bg-gray-200 rounded-xl overflow-hidden cursor-pointer group"
              onClick={() => setLightboxOpen(true)}
            >
              {currentGalleryImages.length > 0 ? (
                <>
                  <img
                    src={currentGalleryImages[selectedImage]}
                    alt={`${alt} - ${activeTab === 'interior' ? 'Interior' : 'Imagen'} ${selectedImage + 1}`}
                    className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
                  />
                  <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-10 transition-all duration-300 flex items-center justify-center">
                    <span className="opacity-0 group-hover:opacity-100 text-white text-lg font-medium transition-opacity duration-300">
                      Click para ampliar
                    </span>
                  </div>
                  {currentGalleryImages.length > 1 && (
                    <div className="absolute bottom-4 right-4 bg-black bg-opacity-75 text-white px-3 py-1 rounded-full text-sm">
                      {selectedImage + 1} / {currentGalleryImages.length}
                    </div>
                  )}
                </>
              ) : (
                <div className="flex items-center justify-center h-full">
                  <p className="text-gray-500">No hay imágenes disponibles</p>
                </div>
              )}
            </div>

            {/* Thumbnail Grid */}
            {currentGalleryImages.length > 1 && (
              <div className="grid grid-cols-4 sm:grid-cols-6 gap-2 mt-4">
                {currentGalleryImages.map((image, index) => (
                  <button
                    key={index}
                    onClick={() => setSelectedImage(index)}
                    className={`
                      relative aspect-square bg-gray-200 rounded-lg overflow-hidden
                      ${selectedImage === index ? 'ring-2 ring-blue-500' : 'hover:ring-2 hover:ring-gray-300'}
                      transition-all duration-200
                    `}
                  >
                    <img
                      src={image}
                      alt={`${alt} - Thumbnail ${index + 1}`}
                      className="w-full h-full object-cover"
                    />
                  </button>
                ))}
              </div>
            )}
          </>
        )}
      </div>

      {/* Lightbox Modal */}
      {lightboxOpen && (
        <div
          className="fixed inset-0 z-50 bg-black bg-opacity-95 flex items-center justify-center"
          onClick={() => setLightboxOpen(false)}
          onKeyDown={handleKeyDown}
          role="dialog"
          aria-modal="true"
          tabIndex={-1}
        >
          {/* Close Button */}
          <button
            onClick={(e) => {
              e.stopPropagation();
              setLightboxOpen(false);
            }}
            className="absolute top-4 right-4 z-10 p-2 bg-white bg-opacity-20 hover:bg-opacity-30 rounded-full transition-colors duration-200"
            aria-label="Cerrar"
          >
            <FiX size={24} className="text-white" />
          </button>

          {/* Previous Button */}
          {currentGalleryImages.length > 1 && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                previousImage();
              }}
              className="absolute left-4 z-10 p-3 bg-white bg-opacity-20 hover:bg-opacity-30 rounded-full transition-colors duration-200"
              aria-label="Imagen anterior"
            >
              <FiChevronLeft size={32} className="text-white" />
            </button>
          )}

          {/* Image */}
          <div className="max-w-7xl max-h-[90vh] mx-4" onClick={(e) => e.stopPropagation()}>
            <img
              src={currentGalleryImages[selectedImage]}
              alt={`${alt} - Full size ${selectedImage + 1}`}
              className="max-w-full max-h-[90vh] object-contain"
            />
            {currentGalleryImages.length > 1 && (
              <div className="text-center mt-4">
                <span className="text-white text-sm">
                  {selectedImage + 1} / {currentGalleryImages.length}
                </span>
              </div>
            )}
          </div>

          {/* Next Button */}
          {currentGalleryImages.length > 1 && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                nextImage();
              }}
              className="absolute right-4 z-10 p-3 bg-white bg-opacity-20 hover:bg-opacity-30 rounded-full transition-colors duration-200"
              aria-label="Siguiente imagen"
            >
              <FiChevronRight size={32} className="text-white" />
            </button>
          )}
        </div>
      )}

      {/* No 360° Available - CTA for dealers */}
      {!has360View && !isLoading360 && (
        <div className="mt-4 p-4 bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-100 rounded-xl">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-blue-100 rounded-full">
              <FiRotateCw className="w-6 h-6 text-blue-600" />
            </div>
            <div className="flex-1">
              <h4 className="font-semibold text-gray-900">¿Eres el vendedor?</h4>
              <p className="text-sm text-gray-600">
                Agrega una vista 360° para aumentar el engagement un 34%
              </p>
            </div>
            <Link
              to={`/dealer/inventory/${vehicleId}/edit?tab=media360`}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium text-sm whitespace-nowrap"
            >
              Activar 360°
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}

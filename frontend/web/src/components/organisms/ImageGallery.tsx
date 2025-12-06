import { useState } from 'react';
import { FiX, FiChevronLeft, FiChevronRight } from 'react-icons/fi';

interface ImageGalleryProps {
  images: string[];
  alt: string;
}

export default function ImageGallery({ images, alt }: ImageGalleryProps) {
  const [selectedImage, setSelectedImage] = useState(0);
  const [lightboxOpen, setLightboxOpen] = useState(false);

  const nextImage = () => {
    setSelectedImage((prev) => (prev + 1) % images.length);
  };

  const previousImage = () => {
    setSelectedImage((prev) => (prev - 1 + images.length) % images.length);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') setLightboxOpen(false);
    if (e.key === 'ArrowRight') nextImage();
    if (e.key === 'ArrowLeft') previousImage();
  };

  return (
    <div className="space-y-4">
      {/* Main Image */}
      <div
        className="relative aspect-[16/9] bg-gray-200 rounded-xl overflow-hidden cursor-pointer group"
        onClick={() => setLightboxOpen(true)}
      >
        <img
          src={images[selectedImage]}
          alt={`${alt} - Image ${selectedImage + 1}`}
          className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
        />
        <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-10 transition-all duration-300 flex items-center justify-center">
          <span className="opacity-0 group-hover:opacity-100 text-white text-lg font-medium transition-opacity duration-300">
            Click to enlarge
          </span>
        </div>
        {images.length > 1 && (
          <div className="absolute bottom-4 right-4 bg-black bg-opacity-75 text-white px-3 py-1 rounded-full text-sm">
            {selectedImage + 1} / {images.length}
          </div>
        )}
      </div>

      {/* Thumbnail Grid */}
      {images.length > 1 && (
        <div className="grid grid-cols-4 sm:grid-cols-6 gap-2">
          {images.map((image, index) => (
            <button
              key={index}
              onClick={() => setSelectedImage(index)}
              className={`
                relative aspect-square bg-gray-200 rounded-lg overflow-hidden
                ${selectedImage === index ? 'ring-2 ring-primary' : 'hover:ring-2 hover:ring-gray-300'}
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
            aria-label="Close lightbox"
          >
            <FiX size={24} className="text-white" />
          </button>

          {/* Previous Button */}
          {images.length > 1 && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                previousImage();
              }}
              className="absolute left-4 z-10 p-3 bg-white bg-opacity-20 hover:bg-opacity-30 rounded-full transition-colors duration-200"
              aria-label="Previous image"
            >
              <FiChevronLeft size={32} className="text-white" />
            </button>
          )}

          {/* Image */}
          <div
            className="max-w-7xl max-h-[90vh] mx-4"
            onClick={(e) => e.stopPropagation()}
          >
            <img
              src={images[selectedImage]}
              alt={`${alt} - Full size ${selectedImage + 1}`}
              className="max-w-full max-h-[90vh] object-contain"
            />
            {images.length > 1 && (
              <div className="text-center mt-4 text-white text-lg">
                {selectedImage + 1} / {images.length}
              </div>
            )}
          </div>

          {/* Next Button */}
          {images.length > 1 && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                nextImage();
              }}
              className="absolute right-4 z-10 p-3 bg-white bg-opacity-20 hover:bg-opacity-30 rounded-full transition-colors duration-200"
              aria-label="Next image"
            >
              <FiChevronRight size={32} className="text-white" />
            </button>
          )}
        </div>
      )}
    </div>
  );
}

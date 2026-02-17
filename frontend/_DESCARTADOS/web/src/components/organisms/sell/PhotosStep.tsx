import { useState, useRef } from 'react';
import Button from '@/components/atoms/Button';
import type { VehicleFormData } from '@/pages/vehicles/SellYourCarPage';
import { FiUpload, FiX, FiImage } from 'react-icons/fi';
import imageCompression from 'browser-image-compression';

interface PhotosStepProps {
  data: Partial<VehicleFormData>;
  onNext: (data: Partial<VehicleFormData>) => void;
  onBack: () => void;
}

export default function PhotosStep({ data, onNext, onBack }: PhotosStepProps) {
  const [images, setImages] = useState<File[]>(data.images || []);
  const [previews, setPreviews] = useState<string[]>([]);
  const [isDragging, setIsDragging] = useState(false);
  const [error, setError] = useState<string>('');
  const [isCompressing, setIsCompressing] = useState(false);
  const [compressionProgress, setCompressionProgress] = useState(0);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Generate previews when images change
  useState(() => {
    if (data.images && data.images.length > 0) {
      const previewUrls = data.images.map((file: File) => URL.createObjectURL(file));
      setPreviews(previewUrls);
      
      // Cleanup
      return () => {
        previewUrls.forEach((url: string) => URL.revokeObjectURL(url));
      };
    }
  });

  const validateFile = (file: File): boolean => {
    // Check file type
    if (!file.type.startsWith('image/')) {
      setError('Only image files are allowed');
      return false;
    }

    // Check file size (max 10MB before compression)
    if (file.size > 10 * 1024 * 1024) {
      setError('Image size must be less than 10MB');
      return false;
    }

    return true;
  };

  const compressImage = async (file: File): Promise<File> => {
    const options = {
      maxSizeMB: 1,
      maxWidthOrHeight: 1920,
      useWebWorker: true,
      onProgress: (progress: number) => {
        setCompressionProgress(progress);
      },
    };

    try {
      const compressedFile = await imageCompression(file, options);
      return compressedFile;
    } catch (error) {
      console.error('Error compressing image:', error);
      return file; // Return original if compression fails
    }
  };

  const handleFiles = async (files: FileList | null) => {
    if (!files) return;

    setError('');
    
    // Check total limit (max 10 images)
    if (images.length + files.length > 10) {
      setError('Maximum 10 images allowed');
      return;
    }

    const validFiles = Array.from(files).filter(validateFile);
    if (validFiles.length === 0) return;

    setIsCompressing(true);
    const newFiles: File[] = [];
    const newPreviews: string[] = [];

    for (let i = 0; i < validFiles.length; i++) {
      try {
        const compressedFile = await compressImage(validFiles[i]);
        newFiles.push(compressedFile);
        newPreviews.push(URL.createObjectURL(compressedFile));
      } catch (error) {
        console.error('Error processing image:', error);
      }
    }

    setImages([...images, ...newFiles]);
    setPreviews([...previews, ...newPreviews]);
    setIsCompressing(false);
    setCompressionProgress(0);
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(false);
    handleFiles(e.dataTransfer.files);
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    handleFiles(e.target.files);
  };

  const removeImage = (index: number) => {
    // Revoke URL to prevent memory leak
    URL.revokeObjectURL(previews[index]);

    setImages(images.filter((_, i) => i !== index));
    setPreviews(previews.filter((_, i) => i !== index));
    setError('');
  };

  const moveImage = (fromIndex: number, toIndex: number) => {
    const newImages = [...images];
    const newPreviews = [...previews];

    [newImages[fromIndex], newImages[toIndex]] = [newImages[toIndex], newImages[fromIndex]];
    [newPreviews[fromIndex], newPreviews[toIndex]] = [newPreviews[toIndex], newPreviews[fromIndex]];

    setImages(newImages);
    setPreviews(newPreviews);
  };

  const handleNext = () => {
    if (images.length === 0) {
      setError('Please upload at least one image');
      return;
    }

    onNext({ images });
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold font-heading text-gray-900 mb-2">
          Upload Photos
        </h2>
        <p className="text-gray-600">
          Add photos of your vehicle. The first image will be the main photo. (Min: 1, Max: 10)
        </p>
      </div>

      {/* Upload Area */}
      <div
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onClick={() => !isCompressing && fileInputRef.current?.click()}
        className={`
          border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-all
          ${
            isDragging
              ? 'border-primary bg-primary/5'
              : 'border-gray-300 hover:border-primary hover:bg-gray-50'
          }
          ${isCompressing ? 'opacity-50 cursor-not-allowed' : ''}
        `}
      >
        <FiUpload className="mx-auto text-4xl text-gray-400 mb-4" />
        <h3 className="text-lg font-semibold text-gray-900 mb-2">
          {isCompressing ? 'Compressing images...' : 'Drag and drop images here'}
        </h3>
        <p className="text-sm text-gray-600 mb-4">
          {isCompressing 
            ? `Processing... ${compressionProgress}%`
            : 'or click to browse from your computer'}
        </p>
        <p className="text-xs text-gray-500">
          Supported formats: JPG, PNG, GIF • Max size: 10MB per image
        </p>

        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          multiple
          onChange={handleFileInputChange}
          className="hidden"
          disabled={isCompressing}
        />
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-700 text-sm">{error}</p>
        </div>
      )}

      {/* Image Previews */}
      {previews.length > 0 && (
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Uploaded Images ({images.length}/10)
          </h3>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
            {previews.map((preview, index) => (
              <div
                key={index}
                className="relative group aspect-square rounded-lg overflow-hidden border-2 border-gray-200"
              >
                {/* Main Photo Badge */}
                {index === 0 && (
                  <div className="absolute top-2 left-2 bg-primary text-white px-2 py-1 rounded text-xs font-semibold z-10">
                    Main Photo
                  </div>
                )}

                {/* Image */}
                <img
                  src={preview}
                  alt={`Upload ${index + 1}`}
                  className="w-full h-full object-cover"
                />

                {/* Overlay with actions */}
                <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
                  {/* Move Left */}
                  {index > 0 && (
                    <button
                      type="button"
                      onClick={(e) => {
                        e.stopPropagation();
                        moveImage(index, index - 1);
                      }}
                      className="p-2 bg-white rounded-lg hover:bg-gray-100 transition-colors"
                      title="Move left"
                    >
                      ←
                    </button>
                  )}

                  {/* Remove */}
                  <button
                    type="button"
                    onClick={(e) => {
                      e.stopPropagation();
                      removeImage(index);
                    }}
                    className="p-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
                    title="Remove"
                  >
                    <FiX size={20} />
                  </button>

                  {/* Move Right */}
                  {index < previews.length - 1 && (
                    <button
                      type="button"
                      onClick={(e) => {
                        e.stopPropagation();
                        moveImage(index, index + 1);
                      }}
                      className="p-2 bg-white rounded-lg hover:bg-gray-100 transition-colors"
                      title="Move right"
                    >
                      →
                    </button>
                  )}
                </div>
              </div>
            ))}

            {/* Add More Button */}
            {images.length < 10 && (
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                className="aspect-square rounded-lg border-2 border-dashed border-gray-300 hover:border-primary hover:bg-gray-50 transition-all flex flex-col items-center justify-center gap-2 text-gray-500 hover:text-primary"
              >
                <FiImage size={32} />
                <span className="text-sm font-medium">Add More</span>
              </button>
            )}
          </div>

          {/* Tips */}
          <div className="mt-4 bg-blue-50 border border-blue-200 rounded-lg p-4">
            <h4 className="text-sm font-semibold text-blue-900 mb-2">Photography Tips:</h4>
            <ul className="text-xs text-blue-800 space-y-1">
              <li>• Take photos in good lighting conditions</li>
              <li>• Include exterior shots from all angles</li>
              <li>• Show interior, dashboard, and trunk</li>
              <li>• Capture any damage or special features</li>
              <li>• Clean your car before taking photos</li>
            </ul>
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="flex justify-between pt-6 border-t">
        <Button type="button" variant="outline" size="lg" onClick={onBack}>
          Back
        </Button>
        <Button type="button" variant="primary" size="lg" onClick={handleNext}>
          Next: Pricing & Details
        </Button>
      </div>
    </div>
  );
}

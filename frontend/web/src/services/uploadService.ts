import axios from 'axios';
import imageCompression from 'browser-image-compression';

// Use Gateway URL for all API calls - MediaService upload endpoint
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const MEDIA_UPLOAD_URL = `${API_BASE_URL}/api/media/upload`;

export interface UploadResponse {
  url: string;
  publicId: string;
  width: number;
  height: number;
  format: string;
  size: number;
}

export interface UploadProgress {
  loaded: number;
  total: number;
  percentage: number;
}

// Compression options for images
const compressionOptions = {
  maxSizeMB: 1,
  maxWidthOrHeight: 1920,
  useWebWorker: true,
  fileType: 'image/jpeg',
};

// Compress image before upload
export const compressImage = async (file: File): Promise<File> => {
  try {
    const compressedFile = await imageCompression(file, compressionOptions);
    return compressedFile;
  } catch (error) {
    console.error('Error compressing image:', error);
    // Return original file if compression fails
    return file;
  }
};

// Upload single image
export const uploadImage = async (
  file: File,
  folder: string = 'vehicles',
  onProgress?: (progress: UploadProgress) => void
): Promise<UploadResponse> => {
  try {
    // Compress image first
    const compressedFile = await compressImage(file);

    const formData = new FormData();
    formData.append('file', compressedFile);
    formData.append('folder', folder);

    const response = await axios.post(`${MEDIA_UPLOAD_URL}/image`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const loaded = progressEvent.loaded;
          const total = progressEvent.total;
          const percentage = Math.round((loaded * 100) / total);
          onProgress({ loaded, total, percentage });
        }
      },
    });

    return response.data;
  } catch (error) {
    console.error('Error uploading image:', error);
    throw new Error('Failed to upload image');
  }
};

// Upload multiple images
export const uploadMultipleImages = async (
  files: File[],
  folder: string = 'vehicles',
  onProgress?: (index: number, progress: UploadProgress) => void
): Promise<UploadResponse[]> => {
  try {
    const uploadPromises = files.map((file, index) =>
      uploadImage(file, folder, (progress) => {
        if (onProgress) {
          onProgress(index, progress);
        }
      })
    );

    const results = await Promise.all(uploadPromises);
    return results;
  } catch (error) {
    console.error('Error uploading multiple images:', error);
    throw new Error('Failed to upload images');
  }
};

// Delete image
export const deleteImage = async (publicId: string): Promise<void> => {
  try {
    await axios.delete(`${MEDIA_UPLOAD_URL}/image/${publicId}`);
  } catch (error) {
    console.error('Error deleting image:', error);
    throw new Error('Failed to delete image');
  }
};

// Upload profile picture
export const uploadProfilePicture = async (
  file: File,
  onProgress?: (progress: UploadProgress) => void
): Promise<UploadResponse> => {
  try {
    // Use smaller size for profile pictures
    const profileCompressionOptions = {
      maxSizeMB: 0.5,
      maxWidthOrHeight: 500,
      useWebWorker: true,
      fileType: 'image/jpeg',
    };

    const compressedFile = await imageCompression(file, profileCompressionOptions);

    const formData = new FormData();
    formData.append('file', compressedFile);
    formData.append('folder', 'profiles');

    const response = await axios.post(`${MEDIA_UPLOAD_URL}/profile`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const loaded = progressEvent.loaded;
          const total = progressEvent.total;
          const percentage = Math.round((loaded * 100) / total);
          onProgress({ loaded, total, percentage });
        }
      },
    });

    return response.data;
  } catch (error) {
    console.error('Error uploading profile picture:', error);
    throw new Error('Failed to upload profile picture');
  }
};

// Validate file type
export const isValidImageFile = (file: File): boolean => {
  const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
  return validTypes.includes(file.type);
};

// Validate file size (max 10MB)
export const isValidFileSize = (file: File, maxSizeMB: number = 10): boolean => {
  const maxSizeBytes = maxSizeMB * 1024 * 1024;
  return file.size <= maxSizeBytes;
};

// Validate image files
export const validateImageFiles = (files: File[]): { valid: boolean; errors: string[] } => {
  const errors: string[] = [];

  files.forEach((file, index) => {
    if (!isValidImageFile(file)) {
      errors.push(`File ${index + 1}: Invalid file type. Only JPEG, PNG, and WebP are allowed.`);
    }

    if (!isValidFileSize(file)) {
      errors.push(`File ${index + 1}: File size exceeds 10MB limit.`);
    }
  });

  return {
    valid: errors.length === 0,
    errors,
  };
};

// Get image dimensions
export const getImageDimensions = (file: File): Promise<{ width: number; height: number }> => {
  return new Promise((resolve, reject) => {
    const img = new Image();
    const url = URL.createObjectURL(file);

    img.onload = () => {
      URL.revokeObjectURL(url);
      resolve({
        width: img.width,
        height: img.height,
      });
    };

    img.onerror = () => {
      URL.revokeObjectURL(url);
      reject(new Error('Failed to load image'));
    };

    img.src = url;
  });
};

// Create thumbnail preview
export const createThumbnailPreview = (file: File): Promise<string> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = (e) => {
      if (e.target?.result) {
        resolve(e.target.result as string);
      } else {
        reject(new Error('Failed to create preview'));
      }
    };

    reader.onerror = () => {
      reject(new Error('Failed to read file'));
    };

    reader.readAsDataURL(file);
  });
};

// Upload document (for verification)
export const uploadDocument = async (
  file: File,
  documentType: 'id' | 'license' | 'registration' | 'other',
  onProgress?: (progress: UploadProgress) => void
): Promise<UploadResponse> => {
  try {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentType', documentType);
    formData.append('folder', 'documents');

    const response = await axios.post(`${MEDIA_UPLOAD_URL}/document`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const loaded = progressEvent.loaded;
          const total = progressEvent.total;
          const percentage = Math.round((loaded * 100) / total);
          onProgress({ loaded, total, percentage });
        }
      },
    });

    return response.data;
  } catch (error) {
    console.error('Error uploading document:', error);
    throw new Error('Failed to upload document');
  }
};

// Get upload statistics (admin)
export const getUploadStats = async (): Promise<{
  totalUploads: number;
  totalSize: number;
  storageUsed: string;
  uploadsByType: Record<string, number>;
}> => {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/media/admin/upload/stats`);
    return response.data;
  } catch (error) {
    console.error('Error fetching upload stats:', error);
    throw new Error('Failed to fetch upload statistics');
  }
};

export default {
  uploadImage,
  uploadMultipleImages,
  deleteImage,
  uploadProfilePicture,
  uploadDocument,
  compressImage,
  isValidImageFile,
  isValidFileSize,
  validateImageFiles,
  getImageDimensions,
  createThumbnailPreview,
  getUploadStats,
};

/**
 * Media Service - Handles image uploads to MediaService
 * Port: 15090
 */

import axios from 'axios';

const MEDIA_SERVICE_URL = import.meta.env.VITE_MEDIA_SERVICE_URL || 'http://localhost:15090';

export interface UploadResponse {
  id: string;
  url: string;
  thumbnailUrl?: string;
  fileName: string;
  fileSize: number;
  contentType: string;
}

/**
 * Upload a single vehicle image to MediaService
 * @param file - The image file to upload
 * @param onProgress - Optional callback for upload progress
 * @returns Promise with the uploaded image URL and metadata
 */
export const uploadVehicleImage = async (
  file: File,
  onProgress?: (progress: number) => void
): Promise<UploadResponse> => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('category', 'vehicles');
  formData.append('isPublic', 'true');

  try {
    const response = await axios.post<UploadResponse>(
      `${MEDIA_SERVICE_URL}/api/media/upload`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        onUploadProgress: (progressEvent) => {
          if (onProgress && progressEvent.total) {
            const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
            onProgress(percentCompleted);
          }
        },
      }
    );

    return response.data;
  } catch (error) {
    console.error('Error uploading image to MediaService:', error);
    throw new Error(`Failed to upload image: ${file.name}`);
  }
};

/**
 * Upload multiple vehicle images to MediaService (batch upload)
 * @param files - Array of image files to upload
 * @param onProgress - Optional callback for overall progress
 * @returns Promise with array of uploaded image URLs and metadata
 */
export const uploadVehicleImages = async (
  files: File[],
  onProgress?: (current: number, total: number, currentProgress: number) => void
): Promise<UploadResponse[]> => {
  const results: UploadResponse[] = [];
  const total = files.length;

  for (let i = 0; i < files.length; i++) {
    const file = files[i];
    
    try {
      const result = await uploadVehicleImage(file, (progress) => {
        if (onProgress) {
          onProgress(i + 1, total, progress);
        }
      });

      results.push(result);
    } catch (error) {
      console.error(`Failed to upload image ${i + 1}/${total}:`, error);
      // Continue with other uploads even if one fails
    }
  }

  return results;
};

/**
 * Delete an image from MediaService
 * @param imageId - The ID of the image to delete
 */
export const deleteVehicleImage = async (imageId: string): Promise<void> => {
  try {
    await axios.delete(`${MEDIA_SERVICE_URL}/api/media/${imageId}`);
  } catch (error) {
    console.error('Error deleting image from MediaService:', error);
    throw new Error(`Failed to delete image: ${imageId}`);
  }
};

export default {
  uploadVehicleImage,
  uploadVehicleImages,
  deleteVehicleImage,
};

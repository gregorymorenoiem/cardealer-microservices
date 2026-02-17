/**
 * User Profile Service
 *
 * Service for fetching user profile data from UserService.
 * Used to get complete user information including firstName, lastName, phone.
 */

import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const USERS_API_URL = `${API_BASE_URL}/api/users`;

export interface UserProfileDto {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  phoneNumber: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface UpdateUserProfileRequest {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
}

/**
 * User Profile Service
 */
export const userProfileService = {
  /**
   * Get user profile by ID
   * @param userId - The user's ID
   * @returns User profile data with firstName, lastName, phoneNumber
   */
  async getUserProfile(userId: string): Promise<UserProfileDto | null> {
    try {
      const token = localStorage.getItem('accessToken');

      const response = await axios.get<UserProfileDto>(`${USERS_API_URL}/${userId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        // Expected error states - don't log as errors
        // 404 = profile not found, 401 = not authenticated, 502/503 = service down
        const status = error.response?.status;
        if (status === 404 || status === 401 || status === 502 || status === 503) {
          // Service unavailable or auth issue - silently return null and use fallback
          return null;
        }
        console.error('Error fetching user profile:', error.response?.data || error.message);
      }
      return null;
    }
  },

  /**
   * Update user profile
   * @param userId - The user's ID
   * @param data - Profile data to update
   */
  async updateUserProfile(userId: string, data: UpdateUserProfileRequest): Promise<boolean> {
    try {
      const token = localStorage.getItem('accessToken');

      await axios.put(`${USERS_API_URL}/${userId}`, data, {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      return true;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        console.error('Error updating user profile:', error.response?.data || error.message);
      }
      return false;
    }
  },
};

export default userProfileService;

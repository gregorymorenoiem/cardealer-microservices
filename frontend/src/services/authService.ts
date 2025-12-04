import type { User } from '@/types';

// Mock users for testing
const MOCK_USERS = [
  {
    id: '1',
    email: 'demo@cardealer.com',
    password: 'demo123',
    username: 'Demo User',
    firstName: 'Demo',
    lastName: 'User',
    phone: '+1 (555) 123-4567',
    role: 'user' as const,
  },
  {
    id: '2',
    email: 'admin@cardealer.com',
    password: 'admin123',
    username: 'Admin',
    firstName: 'Admin',
    lastName: 'User',
    phone: '+1 (555) 987-6543',
    role: 'admin' as const,
  },
];

interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

interface LoginResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
}

/**
 * Mock login service - simulates API call
 */
export const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    // Simulate network delay
    await new Promise((resolve) => setTimeout(resolve, 800));

    const mockUser = MOCK_USERS.find(
      (u) => u.email === credentials.email && u.password === credentials.password
    );

    if (!mockUser) {
      throw new Error('Invalid email or password');
    }

    // Remove password from user object
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const { password, ...userWithoutPassword } = mockUser;

    return {
      user: {
        ...userWithoutPassword,
        isVerified: true,
        memberSince: new Date().toISOString(),
      } as User,
      accessToken: `mock-access-token-${mockUser.id}`,
      refreshToken: `mock-refresh-token-${mockUser.id}`,
    };
  },

  async register(data: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phone?: string;
  }): Promise<LoginResponse> {
    // Simulate network delay
    await new Promise((resolve) => setTimeout(resolve, 1000));

    // Check if email already exists
    const existingUser = MOCK_USERS.find((u) => u.email === data.email);
    if (existingUser) {
      throw new Error('Email already registered');
    }

    // Create new mock user
    const newUser: User = {
      id: Date.now().toString(),
      email: data.email,
      username: `${data.firstName} ${data.lastName}`,
      firstName: data.firstName,
      lastName: data.lastName,
      phone: data.phone || '',
      role: 'user',
      isVerified: true,
      memberSince: new Date().toISOString(),
    };

    return {
      user: newUser,
      accessToken: `mock-access-token-${newUser.id}`,
      refreshToken: `mock-refresh-token-${newUser.id}`,
    };
  },

  async logout(): Promise<void> {
    // Simulate network delay
    await new Promise((resolve) => setTimeout(resolve, 300));
    // Mock logout - just clear tokens
  },
};

// Admin Panel types for Sprint 8

export interface AdminStats {
  pendingApprovals: number;
  activeListings: number;
  totalUsers: number;
  totalRevenue: number;
  todayViews: number;
  todayInquiries: number;
}

export interface PendingVehicle {
  id: string;
  title: string;
  brand: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  images: string[];
  sellerId: string;
  sellerName: string;
  sellerEmail: string;
  submittedAt: string;
  status: 'pending' | 'approved' | 'rejected';
}

export interface AdminUser {
  id: string;
  username: string;
  email: string;
  role: 'buyer' | 'seller' | 'admin';
  status: 'active' | 'banned' | 'suspended';
  joinedAt: string;
  lastActive: string;
  listingsCount: number;
  purchasesCount: number;
  avatar?: string;
}

export interface ActivityLog {
  id: string;
  type: 'listing' | 'user' | 'report' | 'system';
  action: string;
  description: string;
  userId?: string;
  userName?: string;
  timestamp: string;
  severity: 'info' | 'warning' | 'error';
}

export interface Report {
  id: string;
  type: 'listing' | 'user' | 'message';
  targetId: string;
  targetTitle: string;
  reportedBy: string;
  reporterName: string;
  reason: string;
  description: string;
  status: 'open' | 'investigating' | 'resolved' | 'closed';
  createdAt: string;
  resolvedAt?: string;
}

export interface ApprovalAction {
  action: 'approve' | 'reject';
  vehicleId: string;
  reason?: string;
}

// Categories and Subcategories for Multi-Vertical Marketplace
export type VerticalType = 'vehicles' | 'properties';

export interface Category {
  id: string;
  name: string;
  slug: string;
  vertical: VerticalType;
  icon: string;
  description?: string;
  color: string;
  isActive: boolean;
  sortOrder: number;
  listingsCount: number;
  subcategories: Subcategory[];
  createdAt: string;
  updatedAt: string;
}

export interface Subcategory {
  id: string;
  categoryId: string;
  name: string;
  slug: string;
  icon?: string;
  description?: string;
  isActive: boolean;
  sortOrder: number;
  listingsCount: number;
  filters?: SubcategoryFilter[];
  createdAt: string;
  updatedAt: string;
}

export interface SubcategoryFilter {
  key: string;
  label: string;
  type: 'text' | 'number' | 'select' | 'range' | 'boolean';
  options?: string[];
  min?: number;
  max?: number;
  unit?: string;
}

export interface CategoryFormData {
  name: string;
  slug: string;
  vertical: VerticalType;
  icon: string;
  description: string;
  color: string;
  isActive: boolean;
  sortOrder: number;
}

export interface SubcategoryFormData {
  name: string;
  slug: string;
  icon: string;
  description: string;
  isActive: boolean;
  sortOrder: number;
}

/**
 * Featured Listings - Monetization Types
 * Sprint 1: Foundation & Data Model (Cardealer Design)
 */

// ============================================
// TIER TYPES
// ============================================

export type ListingTier = 'basic' | 'featured' | 'premium' | 'enterprise';
export type FeaturedBadge = 'destacado' | 'premium' | 'certificado' | 'top-dealer';
export type FeaturedPage = 'home' | 'browse' | 'detail';

// ============================================
// LISTING INTERFACE (Extended)
// ============================================

export interface FeaturedListing {
  // Base fields
  id: string;
  title: string;
  price: number;
  category: 'vehicles' | 'vehicle-rental' | 'properties' | 'lodging';
  location: string;
  images: string[];
  description: string;
  
  // Featured monetization fields
  tier: ListingTier;
  isFeatured: boolean;
  featuredUntil?: Date;
  featuredPosition?: number;
  featuredPages: FeaturedPage[];
  featuredBadge?: FeaturedBadge;
  
  // Ranking & Quality fields
  qualityScore: number; // 0-100
  engagementScore: number; // 0-1000
  conversionRate: number; // 0-100
  responseTime: number;
  
  // Dealer info
  dealerId: string;
  dealerName: string;
  dealerTier: DealerTier;
  dealerVerified: boolean;
  dealerRating: number;
  
  // Metadata
  createdAt: Date;
  updatedAt: Date;
  lastBoostedAt?: Date;
  expiresAt?: Date;
}

// ============================================
// DEALER TYPES
// ============================================

export type DealerTier = 'basic' | 'premium' | 'enterprise';

export interface Dealer {
  id: string;
  name: string;
  email: string;
  phone?: string;
  logo?: string;
  
  // Subscription
  tier: DealerTier;
  subscriptionType: 'free' | 'featured' | 'premium' | 'enterprise';
  subscriptionStart: Date;
  subscriptionEnd: Date;
  autoRenew: boolean;
  
  // Limits & Usage
  maxFeaturedListings: number;
  currentFeaturedListings: number;
  maxListings: number;
  currentListings: number;
  
  // Quality metrics
  verified: boolean;
  rating: number;
  totalReviews: number;
  totalSales: number;
  responseTime: number;
  responseRate: number;
  
  // Performance
  totalViews: number;
  totalLeads: number;
  conversionRate: number;
  
  // Billing
  monthlyBilling: number;
  totalSpent: number;
  
  // Metadata
  memberSince: Date;
  lastActive: Date;
}

// ============================================
// FEATURED POSITION ASSIGNMENT
// ============================================

export interface FeaturedPositionAssignment {
  id: string;
  listingId: string;
  dealerId: string;
  page: FeaturedPage;
  position: number;
  category?: string;
  startDate: Date;
  endDate: Date;
  impressions: number;
  clicks: number;
  ctr: number;
  cost: number;
}

// ============================================
// RANKING & SCORING
// ============================================

export interface RankingFactors {
  tierBoost: number;
  qualityScore: number;
  engagementScore: number;
  dealerScore: number;
  freshnessScore: number;
  relevanceScore: number;
  finalScore: number;
}

export interface FeaturedConfig {
  maxFeaturedRatio: number; // default: 0.40
  rotationInterval: number; // default: 24 hours
  minQualityScore: number; // default: 60
  boostMultiplier: {
    basic: number;
    featured: number;
    premium: number;
    enterprise: number;
  };
}

// ============================================
// ANALYTICS & METRICS
// ============================================

export interface FeaturedAnalytics {
  listingId: string;
  dealerId: string;
  period: 'daily' | 'weekly' | 'monthly';
  startDate: Date;
  endDate: Date;
  
  impressions: number;
  clicks: number;
  ctr: number;
  saves: number;
  shares: number;
  messages: number;
  calls: number;
  
  leads: number;
  testDrives: number;
  offers: number;
  sales: number;
  conversionRate: number;
  
  cost: number;
  revenue: number;
  roi: number;
}

// ============================================
// HELPER TYPES
// ============================================

export interface FeaturedListingFilters {
  tier?: ListingTier[];
  category?: string[];
  priceMin?: number;
  priceMax?: number;
  location?: string;
  dealerId?: string;
  verified?: boolean;
  page?: FeaturedPage;
}

export type ListingSortOption = 
  | 'rank'
  | 'price-asc'
  | 'price-desc'
  | 'newest'
  | 'popular'
  | 'relevant';

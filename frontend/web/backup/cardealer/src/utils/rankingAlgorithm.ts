/**
 * Ranking Algorithm for Featured Listings
 * Sprint 1: Foundation & Data Model (Cardealer - copied from original)
 */

import type { 
  ListingTier,
  FeaturedPage,
  FeaturedConfig
} from '../types/listing';

// Generic interface for rankable items (vehicles only in cardealer)
interface RankableItem {
  id: string;
  tier?: ListingTier;
  qualityScore?: number;
  engagementScore?: number;
  dealerId?: string;
  dealerTier?: 'basic' | 'premium' | 'enterprise';
  dealerVerified?: boolean;
  seller?: {
    rating?: number;
  };
  createdAt?: Date;
  year?: number;
  featuredPages?: FeaturedPage[];
}

// ============================================
// CONFIGURATION
// ============================================

export const DEFAULT_CONFIG: FeaturedConfig = {
  maxFeaturedRatio: 0.40, // Max 40% featured per viewport
  rotationInterval: 24, // Rotate every 24 hours
  minQualityScore: 60, // Minimum quality threshold
  boostMultiplier: {
    basic: 0,
    featured: 500,
    premium: 800,
    enterprise: 1200
  }
};

// ============================================
// SCORING FUNCTIONS
// ============================================

function calculateTierBoost(tier: ListingTier, config: FeaturedConfig = DEFAULT_CONFIG): number {
  return config.boostMultiplier[tier];
}

function calculateDealerScore(listing: RankableItem): number {
  let score = 0;
  
  if (listing.dealerTier === 'enterprise') score += 100;
  else if (listing.dealerTier === 'premium') score += 60;
  else if (listing.dealerTier === 'basic') score += 20;
  
  if (listing.dealerVerified) score += 40;
  const rating = listing.seller?.rating || 0;
  score += (rating / 5) * 60;
  
  return Math.min(score, 200);
}

function calculateFreshnessScore(item: RankableItem): number {
  if (!item.createdAt && item.year) {
    const currentYear = new Date().getFullYear();
    const age = currentYear - item.year;
    if (age === 0) return 100;
    if (age === 1) return 80;
    if (age === 2) return 60;
    return Math.max(10, 50 - (age * 5));
  }
  
  if (!item.createdAt) return 50;
  
  const now = new Date();
  const ageInDays = (now.getTime() - item.createdAt.getTime()) / (1000 * 60 * 60 * 24);
  
  if (ageInDays <= 7) return 100 - (ageInDays * 7);
  if (ageInDays <= 14) return 50 - ((ageInDays - 7) * 3.5);
  if (ageInDays <= 30) return 25 - ((ageInDays - 14) * 0.9);
  return 10;
}

// ============================================
// MAIN RANKING ALGORITHM
// ============================================

export function calculateItemScore<T extends RankableItem>(
  item: T,
  config: FeaturedConfig = DEFAULT_CONFIG
): number {
  const tier = item.tier || 'basic';
  const tierBoost = calculateTierBoost(tier, config);
  const qualityScore = item.qualityScore || 50;
  const engagementScore = item.engagementScore || 0;
  const dealerScore = calculateDealerScore(item);
  const freshnessScore = calculateFreshnessScore(item);
  
  const finalScore = 
    tierBoost +
    qualityScore +
    engagementScore +
    dealerScore +
    freshnessScore;
  
  return Math.round(finalScore);
}

// ============================================
// MIXING ALGORITHM (40% Rule)
// ============================================

export function mixFeaturedAndOrganic<T extends RankableItem>(
  items: T[],
  page: FeaturedPage = 'browse',
  config: FeaturedConfig = DEFAULT_CONFIG
): T[] {
  // Filter items by page if they have featuredPages
  const relevantItems = items.filter(item => 
    !item.featuredPages || item.featuredPages.includes(page)
  );

  // Sort by score
  const sortedItems = [...relevantItems].sort((a, b) => {
    const scoreA = calculateItemScore(a, config);
    const scoreB = calculateItemScore(b, config);
    return scoreB - scoreA;
  });

  // Apply 40% rule
  const featured = sortedItems.filter(item => item.tier && item.tier !== 'basic');
  const organic = sortedItems.filter(item => !item.tier || item.tier === 'basic');

  const totalSlots = sortedItems.length;
  const maxFeaturedSlots = Math.floor(totalSlots * config.maxFeaturedRatio);

  // Mix strategy for home page: Featured first, then organic
  if (page === 'home') {
    const allowedFeatured = featured.slice(0, maxFeaturedSlots);
    const remainingSlots = totalSlots - allowedFeatured.length;
    const selectedOrganic = organic.slice(0, remainingSlots);
    
    return [...allowedFeatured, ...selectedOrganic];
  }

  // Mix strategy for browse: Interleaved (2 featured, 3 organic pattern)
  if (page === 'browse') {
    const result: T[] = [];
    let fi = 0, oi = 0;
    let featuredCount = 0;

    while (result.length < totalSlots && (fi < featured.length || oi < organic.length)) {
      const canAddFeatured = featuredCount < maxFeaturedSlots && fi < featured.length;
      const needOrganic = result.length % 5 < 3;

      if (needOrganic && oi < organic.length) {
        result.push(organic[oi++]);
      } else if (canAddFeatured) {
        result.push(featured[fi++]);
        featuredCount++;
      } else if (oi < organic.length) {
        result.push(organic[oi++]);
      } else {
        break;
      }
    }

    return result;
  }

  // Default: Featured first
  return sortedItems.slice(0, totalSlots);
}

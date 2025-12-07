/**
 * Ranking Algorithm for Featured Listings
 * Sprint 1: Foundation & Data Model (Original Design)
 */

import type { 
  FeaturedListing, 
  RankingFactors, 
  FeaturedConfig,
  ListingTier 
} from '../types/listing';

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

function calculateDealerScore(listing: FeaturedListing): number {
  let score = 0;
  
  if (listing.dealerTier === 'enterprise') score += 100;
  else if (listing.dealerTier === 'premium') score += 60;
  else if (listing.dealerTier === 'basic') score += 20;
  
  if (listing.dealerVerified) score += 40;
  score += (listing.dealerRating / 5) * 60;
  
  return Math.min(score, 200);
}

function calculateFreshnessScore(createdAt: Date): number {
  const now = new Date();
  const ageInDays = (now.getTime() - createdAt.getTime()) / (1000 * 60 * 60 * 24);
  
  if (ageInDays <= 7) return 100 - (ageInDays * 7);
  if (ageInDays <= 14) return 50 - ((ageInDays - 7) * 3.5);
  if (ageInDays <= 30) return 25 - ((ageInDays - 14) * 0.9);
  return 10;
}

export function calculateRelevanceScore(
  listing: FeaturedListing, 
  searchQuery?: string
): number {
  if (!searchQuery) return 100;
  
  const query = searchQuery.toLowerCase();
  const title = listing.title.toLowerCase();
  const description = listing.description.toLowerCase();
  
  let score = 0;
  
  if (title.includes(query)) score += 100;
  else if (title.split(' ').some(word => word.includes(query))) score += 60;
  else if (description.includes(query)) score += 30;
  
  return Math.min(score, 100);
}

// ============================================
// MAIN RANKING ALGORITHM
// ============================================

export function calculateListingScore(
  listing: FeaturedListing,
  searchQuery?: string,
  config: FeaturedConfig = DEFAULT_CONFIG
): number {
  const factors: RankingFactors = {
    tierBoost: calculateTierBoost(listing.tier, config),
    qualityScore: listing.qualityScore,
    engagementScore: listing.engagementScore,
    dealerScore: calculateDealerScore(listing),
    freshnessScore: calculateFreshnessScore(listing.createdAt),
    relevanceScore: calculateRelevanceScore(listing, searchQuery),
    finalScore: 0
  };
  
  factors.finalScore = 
    factors.tierBoost +
    factors.qualityScore +
    factors.engagementScore +
    factors.dealerScore +
    factors.freshnessScore;
  
  if (searchQuery && factors.relevanceScore < 30) {
    factors.finalScore *= 0.5;
  }
  
  return Math.round(factors.finalScore);
}

// ============================================
// SORTING & FILTERING
// ============================================

export function sortListingsByRank(
  listings: FeaturedListing[],
  searchQuery?: string,
  config?: FeaturedConfig
): FeaturedListing[] {
  return [...listings].sort((a, b) => {
    const scoreA = calculateListingScore(a, searchQuery, config);
    const scoreB = calculateListingScore(b, searchQuery, config);
    return scoreB - scoreA;
  });
}

export function filterQualityListings(
  listings: FeaturedListing[],
  minQualityScore: number = DEFAULT_CONFIG.minQualityScore
): FeaturedListing[] {
  return listings.filter(listing => listing.qualityScore >= minQualityScore);
}

export function applyFairnessRules(
  listings: FeaturedListing[],
  maxFeaturedRatio: number = DEFAULT_CONFIG.maxFeaturedRatio
): FeaturedListing[] {
  const featured = listings.filter(l => l.tier !== 'basic');
  const organic = listings.filter(l => l.tier === 'basic');
  
  const totalSlots = listings.length;
  const maxFeaturedSlots = Math.floor(totalSlots * maxFeaturedRatio);
  
  const allowedFeatured = featured.slice(0, maxFeaturedSlots);
  const remainingSlots = totalSlots - allowedFeatured.length;
  const selectedOrganic = organic.slice(0, remainingSlots);
  
  return sortListingsByRank([...allowedFeatured, ...selectedOrganic]);
}

// ============================================
// POSITION ASSIGNMENT
// ============================================

export function mixFeaturedAndOrganic(
  featured: FeaturedListing[],
  organic: FeaturedListing[],
  pattern: 'home' | 'browse' | 'detail' = 'home'
): FeaturedListing[] {
  const result: FeaturedListing[] = [];
  
  switch (pattern) {
    case 'home':
      result.push(...featured.slice(0, 3));
      let fIndex = 3;
      let oIndex = 0;
      while (fIndex < featured.length || oIndex < organic.length) {
        if (fIndex < featured.length) result.push(featured[fIndex++]);
        if (oIndex < organic.length) result.push(organic[oIndex++]);
        if (oIndex < organic.length) result.push(organic[oIndex++]);
      }
      break;
      
    case 'browse':
      result.push(...featured.slice(0, 2));
      let fIdx = 2;
      let oIdx = 0;
      let counter = 0;
      while (fIdx < featured.length || oIdx < organic.length) {
        if (counter % 4 === 0 && fIdx < featured.length) {
          result.push(featured[fIdx++]);
        } else if (oIdx < organic.length) {
          result.push(organic[oIdx++]);
        }
        counter++;
      }
      break;
      
    case 'detail':
      result.push(...featured.slice(0, 2));
      result.push(...organic);
      break;
  }
  
  return result;
}

export function shuffleWithSeed(listings: FeaturedListing[], date: Date = new Date()): FeaturedListing[] {
  const seed = Math.floor(date.getTime() / (1000 * 60 * 60 * 24));
  
  const shuffled = [...listings];
  let currentIndex = shuffled.length;
  let randomValue = seed;
  
  while (currentIndex > 0) {
    randomValue = (randomValue * 1664525 + 1013904223) % 4294967296;
    const randomIndex = Math.floor((randomValue / 4294967296) * currentIndex);
    currentIndex--;
    
    [shuffled[currentIndex], shuffled[randomIndex]] = [shuffled[randomIndex], shuffled[currentIndex]];
  }
  
  return shuffled;
}

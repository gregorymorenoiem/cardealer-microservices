import { NextRequest, NextResponse } from 'next/server';

// =============================================================================
// BFF: Targeted Ad Rotation for High-Probability Leads
// =============================================================================
// Returns ads prioritized by:
// 1. Dealer spend (highest-paying dealers get priority)
// 2. Lead score (hot leads see premium placements)
// 3. Vehicle relevance (matched to lead's preferred makes/models/price range)
// =============================================================================

const API_URL =
  process.env.INTERNAL_API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:18443';

export async function GET(request: NextRequest) {
  const leadScore = parseInt(request.nextUrl.searchParams.get('leadScore') || '0', 10);
  const section = request.nextUrl.searchParams.get('section') || 'FeaturedSpot';
  const preferredMakes = request.nextUrl.searchParams.get('makes')?.split(',') || [];
  const priceMin = parseInt(request.nextUrl.searchParams.get('priceMin') || '0', 10);
  const priceMax = parseInt(request.nextUrl.searchParams.get('priceMax') || '99999999', 10);
  const limit = parseInt(request.nextUrl.searchParams.get('limit') || '6', 10);

  try {
    // Fetch active campaigns from backend
    const campaignsRes = await fetch(`${API_URL}/api/advertising/rotation/${section}`, {
      headers: { 'Content-Type': 'application/json' },
      next: { revalidate: 60 },
    });

    if (campaignsRes.ok) {
      const data = await campaignsRes.json();
      const campaigns = data.data || data || [];

      // Apply targeting logic
      const targeted = applyTargeting(campaigns, {
        leadScore,
        preferredMakes,
        priceRange: { min: priceMin, max: priceMax },
        limit,
      });

      return NextResponse.json({ success: true, data: targeted });
    }
  } catch (error) {
    console.error('[Targeted Ads] Backend fetch failed:', error);
  }

  // Fallback: demo targeted ads
  const demoAds = generateDemoTargetedAds(leadScore, preferredMakes, limit);
  return NextResponse.json({ success: true, data: demoAds, source: 'demo' });
}

// =============================================================================
// TARGETING ALGORITHM
// =============================================================================

interface TargetingParams {
  leadScore: number;
  preferredMakes: string[];
  priceRange: { min: number; max: number };
  limit: number;
}

interface CampaignAd {
  campaignId: string;
  vehicleId: string;
  vehicleTitle: string;
  vehicleImage?: string;
  vehiclePrice: number;
  vehicleMake?: string;
  dealerId: string;
  dealerName: string;
  placementType: string;
  totalBudget: number;
  dailyBudget?: number;
  spent: number;
  remainingBudget: number;
  qualityScore?: number;
  ctr?: number;
  // Computed during targeting
  targetingScore?: number;
  boostReason?: string;
}

function applyTargeting(campaigns: CampaignAd[], params: TargetingParams): CampaignAd[] {
  const { leadScore, preferredMakes, priceRange, limit } = params;

  // Is this a high-value lead?
  const isHotLead = leadScore >= 60;
  const isWarmLead = leadScore >= 35;

  // Score each campaign for targeting relevance
  const scored = campaigns.map((ad: CampaignAd) => {
    let targetingScore = 0;
    const reasons: string[] = [];

    // 1. DEALER SPEND PRIORITY (0-40 points)
    // Dealers spending more get higher priority, especially for hot leads
    const spendFactor = Math.min(40, (ad.totalBudget / 1000) * (isHotLead ? 2 : 1));
    targetingScore += spendFactor;
    if (ad.totalBudget >= 50000) reasons.push('top-spender');

    // 2. REMAINING BUDGET (0-15 points)
    // Campaigns with more remaining budget get priority
    const budgetRatio = ad.remainingBudget / Math.max(1, ad.totalBudget);
    targetingScore += budgetRatio * 15;

    // 3. VEHICLE RELEVANCE (0-25 points)
    // Match to lead's preferred makes
    if (ad.vehicleMake && preferredMakes.length > 0) {
      if (preferredMakes.some(m => m.toLowerCase() === ad.vehicleMake?.toLowerCase())) {
        targetingScore += 25;
        reasons.push('preferred-make');
      }
    }

    // Match price range
    if (ad.vehiclePrice >= priceRange.min && ad.vehiclePrice <= priceRange.max) {
      targetingScore += 10;
      reasons.push('price-match');
    }

    // 4. QUALITY SCORE (0-10 points)
    if (ad.qualityScore) {
      targetingScore += Math.min(10, ad.qualityScore * 2);
    }

    // 5. CTR BONUS (0-10 points)
    // Well-performing ads get a boost
    if (ad.ctr && ad.ctr > 2) {
      targetingScore += Math.min(10, ad.ctr * 2);
      reasons.push('high-ctr');
    }

    // 6. HOT LEAD PREMIUM BOOST
    // For hot leads, premium placements from highest spenders get extra weight
    if (isHotLead && ad.totalBudget >= 30000) {
      targetingScore *= 1.5;
      reasons.push('premium-for-hot-lead');
    } else if (isWarmLead && ad.totalBudget >= 15000) {
      targetingScore *= 1.2;
      reasons.push('boosted-for-warm-lead');
    }

    return {
      ...ad,
      targetingScore: Math.round(targetingScore),
      boostReason: reasons.join(', '),
    };
  });

  // Sort by targeting score (highest first)
  scored.sort((a: CampaignAd, b: CampaignAd) => (b.targetingScore || 0) - (a.targetingScore || 0));

  // Return top N
  return scored.slice(0, limit);
}

// =============================================================================
// DEMO DATA
// =============================================================================

function generateDemoTargetedAds(
  leadScore: number,
  preferredMakes: string[],
  limit: number
): CampaignAd[] {
  const ads: CampaignAd[] = [
    {
      campaignId: 'camp-t1',
      vehicleId: 'v-t1',
      vehicleTitle: 'Toyota RAV4 2024 — Disponible Inmediata',
      vehicleImage: '/images/demo/rav4.jpg',
      vehiclePrice: 2800000,
      vehicleMake: 'Toyota',
      dealerId: 'dealer-001',
      dealerName: 'AutoMax RD',
      placementType: 'FeaturedSpot',
      totalBudget: 80000,
      dailyBudget: 3000,
      spent: 25000,
      remainingBudget: 55000,
      qualityScore: 4.5,
      ctr: 3.2,
    },
    {
      campaignId: 'camp-t2',
      vehicleId: 'v-t2',
      vehicleTitle: 'Honda CR-V 2023 — Financiamiento Disponible',
      vehicleImage: '/images/demo/crv.jpg',
      vehiclePrice: 2500000,
      vehicleMake: 'Honda',
      dealerId: 'dealer-002',
      dealerName: 'Honda Plaza',
      placementType: 'FeaturedSpot',
      totalBudget: 60000,
      dailyBudget: 2500,
      spent: 18000,
      remainingBudget: 42000,
      qualityScore: 4.2,
      ctr: 2.8,
    },
    {
      campaignId: 'camp-t3',
      vehicleId: 'v-t3',
      vehicleTitle: 'Hyundai Tucson 2024 — 0 Kilómetros',
      vehicleImage: '/images/demo/tucson.jpg',
      vehiclePrice: 2200000,
      vehicleMake: 'Hyundai',
      dealerId: 'dealer-003',
      dealerName: 'Hyundai Motor',
      placementType: 'FeaturedSpot',
      totalBudget: 45000,
      dailyBudget: 2000,
      spent: 12000,
      remainingBudget: 33000,
      qualityScore: 3.8,
      ctr: 2.1,
    },
    {
      campaignId: 'camp-t4',
      vehicleId: 'v-t4',
      vehicleTitle: 'Mercedes-Benz GLC 2023 — Premium',
      vehicleImage: '/images/demo/glc.jpg',
      vehiclePrice: 4200000,
      vehicleMake: 'Mercedes-Benz',
      dealerId: 'dealer-004',
      dealerName: 'Star Motors RD',
      placementType: 'PremiumBanner',
      totalBudget: 120000,
      dailyBudget: 5000,
      spent: 35000,
      remainingBudget: 85000,
      qualityScore: 4.8,
      ctr: 4.1,
    },
    {
      campaignId: 'camp-t5',
      vehicleId: 'v-t5',
      vehicleTitle: 'Kia Sportage 2024 — Recién Llegado',
      vehicleImage: '/images/demo/sportage.jpg',
      vehiclePrice: 1900000,
      vehicleMake: 'Kia',
      dealerId: 'dealer-005',
      dealerName: 'KIA Santo Domingo',
      placementType: 'FeaturedSpot',
      totalBudget: 35000,
      dailyBudget: 1500,
      spent: 8000,
      remainingBudget: 27000,
      qualityScore: 3.5,
      ctr: 1.8,
    },
  ];

  // Apply targeting
  return applyTargeting(ads, {
    leadScore,
    preferredMakes,
    priceRange: { min: 0, max: 99999999 },
    limit,
  });
}

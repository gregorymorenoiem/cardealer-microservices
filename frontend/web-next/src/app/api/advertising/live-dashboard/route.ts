import { NextRequest, NextResponse } from 'next/server';

// =============================================================================
// BFF: Live Advertising Dashboard — Real-time campaign metrics
// =============================================================================

const API_URL =
  process.env.INTERNAL_API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:18443';

export async function GET(request: NextRequest) {
  const ownerId = request.nextUrl.searchParams.get('ownerId') || '';
  const ownerType = request.nextUrl.searchParams.get('ownerType') || 'dealer';

  try {
    const res = await fetch(
      `${API_URL}/api/advertising/live-dashboard?ownerId=${ownerId}&ownerType=${ownerType}`,
      {
        headers: {
          'Content-Type': 'application/json',
          ...(request.headers.get('Authorization')
            ? { Authorization: request.headers.get('Authorization')! }
            : {}),
        },
        next: { revalidate: 15 }, // Refresh every 15 seconds
      }
    );

    if (res.ok) {
      const data = await res.json();
      return NextResponse.json({ success: true, data: data.data || data });
    }
  } catch (error) {
    console.error('[Live Dashboard] Backend fetch failed:', error);
  }

  // Demo data fallback
  const demo = generateDemoLiveData(ownerId);
  return NextResponse.json({ success: true, data: demo, source: 'demo' });
}

// =============================================================================
// DEMO DATA
// =============================================================================

function generateDemoLiveData(ownerId: string) {
  const now = new Date();
  const todayStr = now.toISOString().split('T')[0];

  // Generate realistic hourly data for today
  const currentHour = now.getHours();
  const hourlyData = Array.from({ length: currentHour + 1 }, (_, i) => {
    const baseImpressions = Math.floor(50 + Math.random() * 150 * (i > 7 && i < 22 ? 1.5 : 0.4));
    const baseClicks = Math.floor(baseImpressions * (0.015 + Math.random() * 0.035));
    return {
      hour: i,
      label: `${i.toString().padStart(2, '0')}:00`,
      impressions: baseImpressions,
      clicks: baseClicks,
      ctr: (baseClicks / Math.max(1, baseImpressions)) * 100,
      spend: Math.floor(baseClicks * (15 + Math.random() * 25)),
      leads: Math.floor(baseClicks * (0.05 + Math.random() * 0.15)),
    };
  });

  const totalImpressions = hourlyData.reduce((s, h) => s + h.impressions, 0);
  const totalClicks = hourlyData.reduce((s, h) => s + h.clicks, 0);
  const totalSpend = hourlyData.reduce((s, h) => s + h.spend, 0);
  const totalLeads = hourlyData.reduce((s, h) => s + h.leads, 0);

  // Active campaigns with live metrics
  const campaigns = [
    {
      id: 'camp-live-1',
      name: 'Toyota RAV4 2024 — Campaña Impulsar',
      vehicleTitle: 'Toyota RAV4 2024 Limited',
      vehicleImage: '/images/demo/rav4.jpg',
      status: 'active' as const,
      placement: 'FeaturedSpot',
      budget: { total: 50000, spent: 22350, daily: 2500, spentToday: 1820 },
      metricsToday: {
        impressions: Math.floor(totalImpressions * 0.4),
        clicks: Math.floor(totalClicks * 0.35),
        ctr: 2.8,
        leads: Math.floor(totalLeads * 0.4),
        spend: Math.floor(totalSpend * 0.38),
      },
      metricsLifetime: { impressions: 12500, clicks: 350, ctr: 2.8, leads: 18, spend: 22350 },
      qualityScore: 4.2,
      lastClickAt: new Date(now.getTime() - Math.random() * 3600000).toISOString(),
      lastLeadAt: new Date(now.getTime() - Math.random() * 7200000).toISOString(),
    },
    {
      id: 'camp-live-2',
      name: 'Honda CR-V 2023 — Premium Banner',
      vehicleTitle: 'Honda CR-V EX-L 2023',
      vehicleImage: '/images/demo/crv.jpg',
      status: 'active' as const,
      placement: 'PremiumBanner',
      budget: { total: 35000, spent: 14200, daily: 1800, spentToday: 1250 },
      metricsToday: {
        impressions: Math.floor(totalImpressions * 0.3),
        clicks: Math.floor(totalClicks * 0.35),
        ctr: 3.1,
        leads: Math.floor(totalLeads * 0.35),
        spend: Math.floor(totalSpend * 0.32),
      },
      metricsLifetime: { impressions: 8900, clicks: 275, ctr: 3.1, leads: 14, spend: 14200 },
      qualityScore: 4.5,
      lastClickAt: new Date(now.getTime() - Math.random() * 1800000).toISOString(),
      lastLeadAt: new Date(now.getTime() - Math.random() * 5400000).toISOString(),
    },
    {
      id: 'camp-live-3',
      name: 'Hyundai Tucson 2024 — Search Top',
      vehicleTitle: 'Hyundai Tucson SEL 2024',
      vehicleImage: '/images/demo/tucson.jpg',
      status: 'active' as const,
      placement: 'SearchTop',
      budget: { total: 25000, spent: 8750, daily: 1200, spentToday: 890 },
      metricsToday: {
        impressions: Math.floor(totalImpressions * 0.3),
        clicks: Math.floor(totalClicks * 0.3),
        ctr: 2.4,
        leads: Math.floor(totalLeads * 0.25),
        spend: Math.floor(totalSpend * 0.3),
      },
      metricsLifetime: { impressions: 6200, clicks: 148, ctr: 2.4, leads: 8, spend: 8750 },
      qualityScore: 3.8,
      lastClickAt: new Date(now.getTime() - Math.random() * 5400000).toISOString(),
      lastLeadAt: new Date(now.getTime() - Math.random() * 10800000).toISOString(),
    },
  ];

  // Recent activity feed
  const recentActivity = [
    {
      type: 'click',
      message: 'Clic en Toyota RAV4 2024 desde búsqueda "SUV Toyota"',
      time: '2 min',
      campaign: 'camp-live-1',
    },
    {
      type: 'impression',
      message: '50 nuevas impresiones en Honda CR-V (PremiumBanner)',
      time: '5 min',
      campaign: 'camp-live-2',
    },
    {
      type: 'lead',
      message: '📞 Nuevo lead — llamada para Toyota RAV4 2024',
      time: '12 min',
      campaign: 'camp-live-1',
    },
    {
      type: 'click',
      message: 'Clic en Hyundai Tucson desde categoría SUV',
      time: '18 min',
      campaign: 'camp-live-3',
    },
    {
      type: 'impression',
      message: '30 nuevas impresiones en Toyota RAV4 (FeaturedSpot)',
      time: '22 min',
      campaign: 'camp-live-1',
    },
    {
      type: 'lead',
      message: '💬 WhatsApp para Honda CR-V 2023 (lead hot 🔥)',
      time: '35 min',
      campaign: 'camp-live-2',
    },
    {
      type: 'click',
      message: 'Clic en Honda CR-V desde homepage',
      time: '41 min',
      campaign: 'camp-live-2',
    },
    {
      type: 'milestone',
      message: '🎉 Toyota RAV4 superó las 12,000 impresiones totales',
      time: '1h',
      campaign: 'camp-live-1',
    },
  ];

  return {
    ownerId: ownerId || 'demo-dealer',
    ownerType: 'dealer',
    date: todayStr,
    lastUpdated: now.toISOString(),
    refreshIntervalMs: 30000,
    summary: {
      activeCampaigns: campaigns.length,
      totalBudget: campaigns.reduce((s, c) => s + c.budget.total, 0),
      totalSpent: campaigns.reduce((s, c) => s + c.budget.spent, 0),
      todayImpressions: totalImpressions,
      todayClicks: totalClicks,
      todayCTR: (totalClicks / Math.max(1, totalImpressions)) * 100,
      todaySpend: totalSpend,
      todayLeads: totalLeads,
      todayCostPerLead: totalSpend / Math.max(1, totalLeads),
      avgQualityScore: campaigns.reduce((s, c) => s + c.qualityScore, 0) / campaigns.length,
    },
    hourlyData,
    campaigns,
    recentActivity,
  };
}

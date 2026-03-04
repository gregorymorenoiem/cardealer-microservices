/**
 * Public banner API route — /api/banners?placement=search_leaderboard
 *
 * Proxies to AdminService's public endpoint (no auth required).
 * Used by /vehiculos to fetch configurable ad slots.
 */

import { NextRequest, NextResponse } from 'next/server';

const GATEWAY_URL = process.env.GATEWAY_INTERNAL_URL ?? 'http://gateway:8080';

export async function GET(request: NextRequest) {
  const { searchParams } = new URL(request.url);
  const placement = searchParams.get('placement') ?? 'search_leaderboard';

  try {
    const upstream = `${GATEWAY_URL}/api/content/banners?placement=${encodeURIComponent(placement)}`;
    const res = await fetch(upstream, {
      next: { revalidate: 60 }, // cache 60s — banners don't change often
    });

    if (!res.ok) {
      return NextResponse.json([], { status: 200 }); // graceful fallback
    }

    const data = await res.json();
    return NextResponse.json(Array.isArray(data) ? data : []);
  } catch {
    // Network error or AdminService unavailable — return empty (fallback to OKLA promo in UI)
    return NextResponse.json([]);
  }
}

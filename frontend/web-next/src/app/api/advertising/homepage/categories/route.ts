import { NextRequest, NextResponse } from 'next/server';

/**
 * GET /api/advertising/homepage/categories
 * BFF route for homepage category images.
 * Proxies to backend AdvertisingService with demo fallback.
 */

const API_URL =
  process.env.INTERNAL_API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:18443';

export async function GET(request: NextRequest) {
  const includeHidden = request.nextUrl.searchParams.get('includeHidden') || 'false';

  try {
    const backendUrl = `${API_URL}/api/advertising/homepage/categories?includeHidden=${includeHidden}`;
    const res = await fetch(backendUrl, {
      headers: { Accept: 'application/json' },
      next: { revalidate: 3600 }, // cache 1 hour — categories change rarely
    });

    if (res.ok) {
      const data = await res.json();
      return NextResponse.json(data);
    }
  } catch {
    // Backend unavailable — fall through to demo data
  }

  // Fallback: return demo category data
  const categories = getDemoCategories(includeHidden === 'true');
  return NextResponse.json({
    success: true,
    data: categories,
  });
}

function getDemoCategories(includeHidden: boolean) {
  const allCategories = [
    {
      id: 'cat-sedan',
      category: 'Sedán',
      slug: 'sedan',
      imageUrl: '/images/categories/sedan.webp',
      displayOrder: 1,
      isVisible: true,
      vehicleCount: 245,
    },
    {
      id: 'cat-suv',
      category: 'SUV',
      slug: 'suv',
      imageUrl: '/images/categories/suv.webp',
      displayOrder: 2,
      isVisible: true,
      vehicleCount: 312,
    },
    {
      id: 'cat-pickup',
      category: 'Pickup',
      slug: 'pickup',
      imageUrl: '/images/categories/pickup.webp',
      displayOrder: 3,
      isVisible: true,
      vehicleCount: 128,
    },
    {
      id: 'cat-hatchback',
      category: 'Hatchback',
      slug: 'hatchback',
      imageUrl: '/images/categories/hatchback.webp',
      displayOrder: 4,
      isVisible: true,
      vehicleCount: 89,
    },
    {
      id: 'cat-coupe',
      category: 'Coupé',
      slug: 'coupe',
      imageUrl: '/images/categories/coupe.webp',
      displayOrder: 5,
      isVisible: true,
      vehicleCount: 34,
    },
    {
      id: 'cat-van',
      category: 'Van / Minivan',
      slug: 'van',
      imageUrl: '/images/categories/van.webp',
      displayOrder: 6,
      isVisible: true,
      vehicleCount: 56,
    },
    {
      id: 'cat-convertible',
      category: 'Convertible',
      slug: 'convertible',
      imageUrl: '/images/categories/convertible.webp',
      displayOrder: 7,
      isVisible: true,
      vehicleCount: 12,
    },
    {
      id: 'cat-electric',
      category: 'Eléctrico / Híbrido',
      slug: 'electrico',
      imageUrl: '/images/categories/electric.webp',
      displayOrder: 8,
      isVisible: true,
      vehicleCount: 45,
    },
  ];

  if (includeHidden) {
    return allCategories;
  }
  return allCategories.filter(c => c.isVisible);
}

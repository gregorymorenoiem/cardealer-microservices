import { NextRequest, NextResponse } from 'next/server';

/**
 * GET /api/advertising/homepage/brands
 * BFF route for homepage brand logos/images.
 * Proxies to backend AdvertisingService with demo fallback.
 */

const API_URL =
  process.env.INTERNAL_API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:18443';

export async function GET(request: NextRequest) {
  const includeHidden = request.nextUrl.searchParams.get('includeHidden') || 'false';

  try {
    const backendUrl = `${API_URL}/api/advertising/homepage/brands?includeHidden=${includeHidden}`;
    const res = await fetch(backendUrl, {
      headers: { Accept: 'application/json' },
      next: { revalidate: 3600 }, // cache 1 hour
    });

    if (res.ok) {
      const data = await res.json();
      return NextResponse.json(data);
    }
  } catch {
    // Backend unavailable — fall through to demo data
  }

  // Fallback: return demo brand data
  const brands = getDemoBrands(includeHidden === 'true');
  return NextResponse.json({
    success: true,
    data: brands,
  });
}

function getDemoBrands(includeHidden: boolean) {
  const allBrands = [
    {
      id: 'brand-toyota',
      brand: 'Toyota',
      slug: 'toyota',
      logoUrl: '/images/brands/toyota.webp',
      displayOrder: 1,
      isVisible: true,
      vehicleCount: 198,
    },
    {
      id: 'brand-honda',
      brand: 'Honda',
      slug: 'honda',
      logoUrl: '/images/brands/honda.webp',
      displayOrder: 2,
      isVisible: true,
      vehicleCount: 156,
    },
    {
      id: 'brand-hyundai',
      brand: 'Hyundai',
      slug: 'hyundai',
      logoUrl: '/images/brands/hyundai.webp',
      displayOrder: 3,
      isVisible: true,
      vehicleCount: 134,
    },
    {
      id: 'brand-kia',
      brand: 'Kia',
      slug: 'kia',
      logoUrl: '/images/brands/kia.webp',
      displayOrder: 4,
      isVisible: true,
      vehicleCount: 112,
    },
    {
      id: 'brand-nissan',
      brand: 'Nissan',
      slug: 'nissan',
      logoUrl: '/images/brands/nissan.webp',
      displayOrder: 5,
      isVisible: true,
      vehicleCount: 98,
    },
    {
      id: 'brand-mercedes',
      brand: 'Mercedes-Benz',
      slug: 'mercedes-benz',
      logoUrl: '/images/brands/mercedes.webp',
      displayOrder: 6,
      isVisible: true,
      vehicleCount: 67,
    },
    {
      id: 'brand-bmw',
      brand: 'BMW',
      slug: 'bmw',
      logoUrl: '/images/brands/bmw.webp',
      displayOrder: 7,
      isVisible: true,
      vehicleCount: 54,
    },
    {
      id: 'brand-chevrolet',
      brand: 'Chevrolet',
      slug: 'chevrolet',
      logoUrl: '/images/brands/chevrolet.webp',
      displayOrder: 8,
      isVisible: true,
      vehicleCount: 87,
    },
    {
      id: 'brand-ford',
      brand: 'Ford',
      slug: 'ford',
      logoUrl: '/images/brands/ford.webp',
      displayOrder: 9,
      isVisible: true,
      vehicleCount: 76,
    },
    {
      id: 'brand-mitsubishi',
      brand: 'Mitsubishi',
      slug: 'mitsubishi',
      logoUrl: '/images/brands/mitsubishi.webp',
      displayOrder: 10,
      isVisible: true,
      vehicleCount: 65,
    },
  ];

  if (includeHidden) {
    return allBrands;
  }
  return allBrands.filter(b => b.isVisible);
}

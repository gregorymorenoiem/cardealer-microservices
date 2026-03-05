/**
 * Bulk Vehicle Import API Route (BFF)
 *
 * Accepts multiple vehicle listing texts and extracts structured data for each.
 * Supports:
 * - Facebook Marketplace (multiple listings pasted)
 * - Corotos.com.do
 * - SuperCarros.com
 * - MercadoLibre RD
 * - Any platform with vehicle listing text
 *
 * @route POST /api/import/bulk
 */

import { NextRequest, NextResponse } from 'next/server';

const INTERNAL_API_URL = process.env.INTERNAL_API_URL || 'http://gateway:8080';

interface BulkImportRequest {
  listings: Array<{
    text: string;
    source: string; // 'facebook' | 'corotos' | 'supercarros' | 'mercadolibre' | 'other'
  }>;
}

interface ExtractedVehicle {
  make: string;
  model: string;
  year: number;
  price: number;
  currency: string;
  mileage: number | null;
  transmission: string | null;
  fuelType: string | null;
  bodyType: string | null;
  condition: string;
  color: string | null;
  description: string;
  location: string | null;
  province: string | null;
  features: string[];
  engineSize: string | null;
  doors: number | null;
  driveType: string | null;
  confidence: number;
  rawSource: string;
}

interface BulkImportResult {
  index: number;
  success: boolean;
  data: ExtractedVehicle | null;
  error: string | null;
}

export async function POST(request: NextRequest) {
  try {
    const authHeader = request.headers.get('authorization');
    if (!authHeader) {
      return NextResponse.json(
        { success: false, error: 'Se requiere autenticación' },
        { status: 401 }
      );
    }

    const body = (await request.json()) as BulkImportRequest;

    if (!body.listings || !Array.isArray(body.listings) || body.listings.length === 0) {
      return NextResponse.json(
        { success: false, error: 'Se requiere al menos un anuncio para importar' },
        { status: 400 }
      );
    }

    if (body.listings.length > 50) {
      return NextResponse.json(
        { success: false, error: 'Máximo 50 anuncios por lote. Divide en múltiples solicitudes.' },
        { status: 400 }
      );
    }

    const results: BulkImportResult[] = [];

    // Process each listing sequentially to avoid overwhelming the AI service
    for (let i = 0; i < body.listings.length; i++) {
      const listing = body.listings[i];

      if (!listing.text || listing.text.length < 10) {
        results.push({
          index: i,
          success: false,
          data: null,
          error: `Anuncio ${i + 1}: Texto muy corto (mínimo 10 caracteres)`,
        });
        continue;
      }

      try {
        // Call the single import endpoint internally
        const singleResponse = await fetch(`${request.nextUrl.origin}/api/import/marketplace`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: authHeader,
          },
          body: JSON.stringify({
            text: listing.text,
            source: listing.source || 'other',
          }),
        });

        const singleResult = await singleResponse.json();

        if (singleResult.success && singleResult.data) {
          // Override source with the specified platform
          singleResult.data.rawSource = listing.source || 'other';
          results.push({
            index: i,
            success: true,
            data: singleResult.data,
            error: null,
          });
        } else {
          results.push({
            index: i,
            success: false,
            data: null,
            error: singleResult.error || `Error procesando anuncio ${i + 1}`,
          });
        }
      } catch {
        results.push({
          index: i,
          success: false,
          data: null,
          error: `Error interno procesando anuncio ${i + 1}`,
        });
      }

      // Small delay between requests to avoid rate limiting
      if (i < body.listings.length - 1) {
        await new Promise(resolve => setTimeout(resolve, 500));
      }
    }

    const successful = results.filter(r => r.success).length;
    const failed = results.filter(r => !r.success).length;

    return NextResponse.json({
      success: true,
      summary: {
        total: body.listings.length,
        successful,
        failed,
      },
      results,
      message: `${successful} de ${body.listings.length} anuncios procesados exitosamente.`,
    });
  } catch (error) {
    console.error('Bulk import error:', error);
    return NextResponse.json(
      { success: false, error: 'Error interno al procesar la importación masiva' },
      { status: 500 }
    );
  }
}

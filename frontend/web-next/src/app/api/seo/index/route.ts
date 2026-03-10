/**
 * Google Indexing API Route Handler
 *
 * POST /api/seo/index — Notify Google about new/updated vehicle URLs
 * DELETE /api/seo/index — Notify Google about removed vehicle URLs
 * GET /api/seo/index?slug=xxx — Check indexing status of a vehicle
 *
 * Called by the backend VehiclesSaleService webhook when a vehicle is
 * published, updated, or unpublished to ensure <48h Google indexing.
 *
 * Security: Requires X-SEO-Webhook-Secret header matching SEO_WEBHOOK_SECRET env.
 */

import { NextRequest, NextResponse } from 'next/server';
import {
  notifyGoogleUrlUpdated,
  notifyGoogleUrlDeleted,
  getUrlIndexingStatus,
  batchNotifyGoogleUrls,
} from '@/lib/google-indexing';

const WEBHOOK_SECRET = process.env.SEO_WEBHOOK_SECRET || '';

function validateAuth(request: NextRequest): boolean {
  if (!WEBHOOK_SECRET) return true; // Dev mode: no auth required
  const secret = request.headers.get('X-SEO-Webhook-Secret');
  return secret === WEBHOOK_SECRET;
}

// POST: Notify Google that a vehicle URL was created/updated
export async function POST(request: NextRequest) {
  if (!validateAuth(request)) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }

  try {
    const body = await request.json();

    // Single URL notification
    if (body.slug) {
      const result = await notifyGoogleUrlUpdated(body.slug);
      return NextResponse.json(result, { status: result.success ? 200 : 502 });
    }

    // Batch notification
    if (body.slugs && Array.isArray(body.slugs)) {
      const results = await batchNotifyGoogleUrls(body.slugs, 'URL_UPDATED');
      const successCount = results.filter(r => r.success).length;
      return NextResponse.json(
        {
          total: results.length,
          success: successCount,
          failed: results.length - successCount,
          results,
        },
        { status: successCount > 0 ? 200 : 502 }
      );
    }

    return NextResponse.json({ error: 'Missing slug or slugs in request body' }, { status: 400 });
  } catch (error) {
    console.error('[SEO Index API] Error:', error);
    return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
  }
}

// DELETE: Notify Google that a vehicle URL was removed
export async function DELETE(request: NextRequest) {
  if (!validateAuth(request)) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }

  try {
    const body = await request.json();

    if (body.slug) {
      const result = await notifyGoogleUrlDeleted(body.slug);
      return NextResponse.json(result, { status: result.success ? 200 : 502 });
    }

    if (body.slugs && Array.isArray(body.slugs)) {
      const results = await batchNotifyGoogleUrls(body.slugs, 'URL_DELETED');
      const successCount = results.filter(r => r.success).length;
      return NextResponse.json(
        {
          total: results.length,
          success: successCount,
          failed: results.length - successCount,
          results,
        },
        { status: successCount > 0 ? 200 : 502 }
      );
    }

    return NextResponse.json({ error: 'Missing slug or slugs in request body' }, { status: 400 });
  } catch (error) {
    console.error('[SEO Index API] Error:', error);
    return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
  }
}

// GET: Check indexing status of a vehicle
export async function GET(request: NextRequest) {
  if (!validateAuth(request)) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }

  const slug = request.nextUrl.searchParams.get('slug');
  if (!slug) {
    return NextResponse.json({ error: 'Missing slug query parameter' }, { status: 400 });
  }

  const status = await getUrlIndexingStatus(slug);
  if (!status) {
    return NextResponse.json({ error: 'Unable to retrieve indexing status' }, { status: 502 });
  }

  return NextResponse.json(status);
}

/**
 * Banner analytics tracking — POST /api/banners/[id]/click or /view
 * Proxies to AdminService to record impressions/clicks for configured banners.
 */

import { NextRequest, NextResponse } from 'next/server';

const GATEWAY_URL = process.env.GATEWAY_INTERNAL_URL ?? 'http://gateway:8080';

export async function POST(
  _request: NextRequest,
  { params }: { params: Promise<{ id: string; action: string }> }
) {
  const { id, action } = await params;

  if (!['click', 'view'].includes(action)) {
    return NextResponse.json({ error: 'Invalid action' }, { status: 400 });
  }

  try {
    await fetch(`${GATEWAY_URL}/api/content/banners/${id}/${action}`, {
      method: 'POST',
    });
  } catch {
    // Fire-and-forget — don't fail the user request if tracking fails
  }

  return NextResponse.json({ ok: true });
}

/**
 * AZUL Webhook API Route
 *
 * Handle AZUL payment webhooks (Dominican Republic payment processor).
 * Validates shared secret before forwarding to backend.
 */

import { NextRequest, NextResponse } from 'next/server';

const BACKEND_URL = process.env.BACKEND_API_URL || 'http://localhost:8080';
const AZUL_WEBHOOK_SECRET = process.env.AZUL_WEBHOOK_SECRET;

export async function POST(request: NextRequest) {
  try {
    // Verify webhook secret is configured
    if (!AZUL_WEBHOOK_SECRET) {
      console.error('[AZUL Webhook] AZUL_WEBHOOK_SECRET not configured — rejecting request');
      return NextResponse.json({ error: 'Webhook not configured' }, { status: 503 });
    }

    // Verify shared secret from header or query param
    const providedSecret =
      request.headers.get('x-azul-webhook-secret') ||
      new URL(request.url).searchParams.get('secret');

    if (providedSecret !== AZUL_WEBHOOK_SECRET) {
      console.error('[AZUL Webhook] Invalid or missing webhook secret');
      return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
    }

    const body = await request.json();

    // Forward to backend for processing (do NOT log full payload)
    const response = await fetch(`${BACKEND_URL}/api/webhooks/azul`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      console.error('[AZUL Webhook] Backend processing failed:', response.status);
      return NextResponse.json({ error: 'Webhook processing failed' }, { status: 500 });
    }

    return NextResponse.json({ received: true });
  } catch (error) {
    console.error('[AZUL Webhook] Error:', error instanceof Error ? error.message : error);
    return NextResponse.json({ error: 'Webhook handler failed' }, { status: 500 });
  }
}

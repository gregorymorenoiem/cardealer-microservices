/**
 * Stripe Webhook API Route
 *
 * Handle Stripe payment webhooks with signature verification.
 * The raw body is forwarded to the backend which performs full Stripe SDK validation.
 */

import { NextRequest, NextResponse } from 'next/server';

const STRIPE_WEBHOOK_SECRET = process.env.STRIPE_WEBHOOK_SECRET;
const BACKEND_URL = process.env.BACKEND_API_URL || 'http://localhost:8080';

export async function POST(request: NextRequest) {
  try {
    const body = await request.text();
    const signature = request.headers.get('stripe-signature');

    if (!signature) {
      return NextResponse.json({ error: 'Missing Stripe signature' }, { status: 400 });
    }

    // Verify webhook secret is configured
    if (!STRIPE_WEBHOOK_SECRET) {
      console.error('[Stripe Webhook] STRIPE_WEBHOOK_SECRET not configured — rejecting request');
      return NextResponse.json({ error: 'Webhook not configured' }, { status: 503 });
    }

    // Basic timing-based signature format check (full verification on backend with Stripe SDK)
    // Stripe signatures follow the format: t=<timestamp>,v1=<signature>[,v0=<test_signature>]
    if (!signature.startsWith('t=') || !signature.includes('v1=')) {
      return NextResponse.json({ error: 'Invalid signature format' }, { status: 400 });
    }

    // Forward to backend with raw body + signature for full SDK verification
    const response = await fetch(`${BACKEND_URL}/api/webhooks/stripe`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'stripe-signature': signature,
      },
      body,
    });

    if (!response.ok) {
      console.error('[Stripe Webhook] Backend processing failed:', response.status);
      return NextResponse.json({ error: 'Webhook processing failed' }, { status: 500 });
    }

    return NextResponse.json({ received: true });
  } catch (error) {
    console.error('[Stripe Webhook] Error:', error instanceof Error ? error.message : error);
    return NextResponse.json({ error: 'Webhook handler failed' }, { status: 500 });
  }
}

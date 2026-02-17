/**
 * AZUL Webhook API Route
 *
 * Handle AZUL payment webhooks (Dominican Republic payment processor)
 */

import { NextRequest, NextResponse } from 'next/server';

const BACKEND_URL = process.env.BACKEND_API_URL || 'http://localhost:8080';

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();

    // Log for debugging
    console.log('AZUL webhook received:', JSON.stringify(body, null, 2));

    // Forward to backend for processing
    const response = await fetch(`${BACKEND_URL}/api/webhooks/azul`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      console.error('Backend webhook processing failed:', response.status);
      return NextResponse.json({ error: 'Webhook processing failed' }, { status: 500 });
    }

    return NextResponse.json({ received: true });
  } catch (error) {
    console.error('AZUL webhook error:', error);
    return NextResponse.json({ error: 'Webhook handler failed' }, { status: 500 });
  }
}

/**
 * Admin Pricing API Route — BFF
 *
 * Proxies admin pricing updates to AdminService.
 * Requires an authenticated admin or super-admin session.
 *
 * PUT /api/admin/pricing → Forwards to AdminService PUT /api/admin/pricing
 */

import { cookies } from 'next/headers';
import { NextRequest, NextResponse } from 'next/server';

const API_URL =
  process.env.INTERNAL_API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:18443';

const AUTH_COOKIE_NAME = 'okla_access_token';

export async function PUT(req: NextRequest) {
  const cookieStore = await cookies();
  const authToken = cookieStore.get(AUTH_COOKIE_NAME)?.value;

  if (!authToken) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }

  try {
    const body = await req.json();

    const upstream = await fetch(`${API_URL}/api/admin/pricing`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${authToken}`,
      },
      body: JSON.stringify(body),
    });

    if (!upstream.ok) {
      const text = await upstream.text().catch(() => '');
      return NextResponse.json(
        { error: 'Upstream error', detail: text },
        { status: upstream.status }
      );
    }

    return NextResponse.json({ success: true }, { status: 200 });
  } catch (err) {
    console.error('[admin/pricing] PUT error:', err);
    return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
  }
}

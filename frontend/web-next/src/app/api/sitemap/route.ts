/**
 * Legacy Sitemap API Route — DEPRECATED
 *
 * SEO AUDIT FIX: This was a duplicate sitemap that conflicted with the
 * convention-based sitemap at src/app/sitemap.ts (which is more complete).
 * Now redirects to the canonical sitemap.xml.
 *
 * TODO: Remove this file entirely once confirmed no external tools depend on /api/sitemap
 */

import { NextResponse } from 'next/server';

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || 'https://okla.com.do';

export async function GET() {
  // 301 permanent redirect to the canonical sitemap
  return NextResponse.redirect(`${SITE_URL}/sitemap.xml`, { status: 301 });
}

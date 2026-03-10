/**
 * Twitter Card Image for Vehicle Detail Pages
 *
 * Re-exports the OpenGraph image generator for Twitter card compatibility.
 * Twitter uses summary_large_image card type with the same dimensions as OG.
 *
 * NOTE: `runtime`, `size`, and `contentType` MUST be declared directly here —
 * Next.js Turbopack cannot statically analyse re-exported route config fields.
 * See: https://nextjs.org/docs/app/api-reference/file-conventions/route-segment-config
 */

export { default } from './opengraph-image';

// Route segment config — must be declared inline, not re-exported
export const runtime = 'edge';
export const size = { width: 1200, height: 630 };
export const contentType = 'image/png';
export const alt = 'Vehículo en OKLA';

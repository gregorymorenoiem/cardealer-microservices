'use client';

import { useState, useEffect } from 'react';

/**
 * Returns the correct `maxItems` count based on the current viewport width.
 * Breakpoints mirror Tailwind defaults (same as CSS grid breakpoints used in
 * the card sections):
 *
 * | Name    | Width       |
 * |---------|-------------|
 * | mobile  | < 768px     |
 * | tablet  | 768–1023px  |
 * | desktop | 1024–1279px |
 * | xl      | ≥ 1280px    |
 *
 * SSR default = xl value (avoids layout shift on large-screen first paint).
 * After hydration the correct value for the real viewport is applied.
 */
export function useResponsiveMaxItems(
  mobile: number,
  tablet: number,
  desktop: number,
  xl: number
): number {
  // Default to xl so the server-rendered HTML matches wide-screen clients.
  const [value, setValue] = useState(xl);

  useEffect(() => {
    function update() {
      const w = window.innerWidth;
      if (w < 768) setValue(mobile);
      else if (w < 1024) setValue(tablet);
      else if (w < 1280) setValue(desktop);
      else setValue(xl);
    }

    update();
    window.addEventListener('resize', update);
    return () => window.removeEventListener('resize', update);
  }, [mobile, tablet, desktop, xl]);

  return value;
}

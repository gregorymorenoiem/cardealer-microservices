'use client';

import { useEffect, useRef, ReactNode } from 'react';
import { useRecordImpression } from '@/hooks/use-advertising';
import type { AdPlacementType } from '@/types/advertising';

interface AdImpressionTrackerProps {
  campaignId: string;
  vehicleId: string;
  placementType: AdPlacementType;
  children: ReactNode;
  className?: string;
}

/**
 * Wraps content and records an ad impression when it becomes visible.
 * Uses IntersectionObserver for viewport-based tracking.
 */
export default function AdImpressionTracker({
  campaignId,
  vehicleId,
  placementType,
  children,
  className,
}: AdImpressionTrackerProps) {
  const ref = useRef<HTMLDivElement>(null);
  const tracked = useRef(false);
  const recordImpression = useRecordImpression();

  useEffect(() => {
    if (!ref.current || tracked.current || !campaignId) return;

    const observer = new IntersectionObserver(
      entries => {
        entries.forEach(entry => {
          if (entry.isIntersecting && !tracked.current) {
            tracked.current = true;
            recordImpression.mutate({
              campaignId,
              vehicleId,
              section: placementType,
            });
            observer.disconnect();
          }
        });
      },
      { threshold: 0.5 }
    );

    observer.observe(ref.current);

    return () => observer.disconnect();
  }, [campaignId, vehicleId, placementType, recordImpression]);

  return (
    <div ref={ref} className={className}>
      {children}
    </div>
  );
}

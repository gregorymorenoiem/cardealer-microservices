'use client';

import { useMemo } from 'react';
import Lightbox from 'yet-another-react-lightbox';
import Zoom from 'yet-another-react-lightbox/plugins/zoom';
import Thumbnails from 'yet-another-react-lightbox/plugins/thumbnails';
import Counter from 'yet-another-react-lightbox/plugins/counter';
import 'yet-another-react-lightbox/styles.css';
import 'yet-another-react-lightbox/plugins/thumbnails.css';
import 'yet-another-react-lightbox/plugins/counter.css';
import type { PhotoItem } from './photo-card';

// ============================================================
// TYPES
// ============================================================

interface PhotoLightboxProps {
  photos: PhotoItem[];
  isOpen: boolean;
  initialIndex: number;
  onClose: () => void;
}

// ============================================================
// COMPONENT
// ============================================================

export function PhotoLightbox({ photos, isOpen, initialIndex, onClose }: PhotoLightboxProps) {
  const slides = useMemo(
    () =>
      photos.map(photo => ({
        src: photo.url,
        alt: photo.alt || `Foto ${photo.order + 1}`,
        width: photo.dimensions?.width ?? 1920,
        height: photo.dimensions?.height ?? 1440,
      })),
    [photos]
  );

  return (
    <Lightbox
      open={isOpen}
      close={onClose}
      slides={slides}
      index={initialIndex}
      plugins={[Zoom, Thumbnails, Counter]}
      zoom={{
        maxZoomPixelRatio: 3,
        scrollToZoom: true,
      }}
      thumbnails={{
        position: 'bottom',
        width: 80,
        height: 60,
        gap: 8,
      }}
      counter={{}}
      carousel={{
        finite: true,
      }}
      styles={{
        container: { backgroundColor: 'rgba(0, 0, 0, 0.9)' },
      }}
      labels={{
        Previous: 'Anterior',
        Next: 'Siguiente',
        Close: 'Cerrar',
      }}
    />
  );
}

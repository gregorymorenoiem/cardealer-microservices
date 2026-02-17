/**
 * Accessibility components and utilities
 * Ensures the application is usable by everyone
 * 
 * Features:
 * - Skip links for keyboard navigation
 * - Screen reader announcements
 * - Focus management
 * - ARIA live regions
 */

import React, { useEffect, useRef, useState } from 'react';

// ============================================
// Skip Links
// ============================================

interface SkipLinkProps {
  links?: Array<{ id: string; label: string }>;
}

export const SkipLinks: React.FC<SkipLinkProps> = ({
  links = [
    { id: 'main-content', label: 'Ir al contenido principal' },
    { id: 'main-nav', label: 'Ir a la navegación' },
    { id: 'footer', label: 'Ir al pie de página' },
  ],
}) => {
  return (
    <nav aria-label="Enlaces de acceso rápido" className="sr-only focus-within:not-sr-only">
      <ul className="fixed top-0 left-0 z-[9999] flex gap-2 p-2 bg-white shadow-lg">
        {links.map((link) => (
          <li key={link.id}>
            <a
              href={`#${link.id}`}
              className="block px-4 py-2 text-sm font-medium text-blue-600 bg-blue-50 rounded-lg hover:bg-blue-100 focus:ring-2 focus:ring-blue-500 focus:outline-none"
            >
              {link.label}
            </a>
          </li>
        ))}
      </ul>
    </nav>
  );
};

// ============================================
// Screen Reader Announcements
// ============================================

interface AnnouncerProps {
  message: string;
  assertive?: boolean;
}

export const ScreenReaderAnnouncer: React.FC<AnnouncerProps> = ({
  message,
  assertive = false,
}) => {
  const [announcement, setAnnouncement] = useState('');

  useEffect(() => {
    if (message) {
      // Clear and re-set to ensure screen readers announce the change
      setAnnouncement('');
      const timeout = setTimeout(() => setAnnouncement(message), 100);
      return () => clearTimeout(timeout);
    }
  }, [message]);

  return (
    <div
      role="status"
      aria-live={assertive ? 'assertive' : 'polite'}
      aria-atomic="true"
      className="sr-only"
    >
      {announcement}
    </div>
  );
};

// ============================================
// Focus Trap
// ============================================

interface FocusTrapProps {
  children: React.ReactNode;
  active?: boolean;
  onEscape?: () => void;
}

export const FocusTrap: React.FC<FocusTrapProps> = ({
  children,
  active = true,
  onEscape,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);

  useEffect(() => {
    if (!active) return;

    // Store current focus
    previousFocusRef.current = document.activeElement as HTMLElement;

    // Focus first focusable element
    const container = containerRef.current;
    if (!container) return;

    const focusableElements = container.querySelectorAll<HTMLElement>(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );

    if (focusableElements.length > 0) {
      focusableElements[0].focus();
    }

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && onEscape) {
        onEscape();
        return;
      }

      if (e.key !== 'Tab') return;

      const focusable = Array.from(focusableElements);
      const firstElement = focusable[0];
      const lastElement = focusable[focusable.length - 1];

      if (e.shiftKey && document.activeElement === firstElement) {
        e.preventDefault();
        lastElement?.focus();
      } else if (!e.shiftKey && document.activeElement === lastElement) {
        e.preventDefault();
        firstElement?.focus();
      }
    };

    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      // Restore focus
      previousFocusRef.current?.focus();
    };
  }, [active, onEscape]);

  return (
    <div ref={containerRef} role="dialog" aria-modal="true">
      {children}
    </div>
  );
};

// ============================================
// Visually Hidden
// ============================================

export const VisuallyHidden: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <span className="sr-only">{children}</span>
);

// ============================================
// Loading State Announcer
// ============================================

interface LoadingAnnouncerProps {
  isLoading: boolean;
  loadingMessage?: string;
  loadedMessage?: string;
}

export const LoadingAnnouncer: React.FC<LoadingAnnouncerProps> = ({
  isLoading,
  loadingMessage = 'Cargando...',
  loadedMessage = 'Contenido cargado',
}) => {
  const [message, setMessage] = useState('');

  useEffect(() => {
    if (isLoading) {
      setMessage(loadingMessage);
    } else {
      setMessage(loadedMessage);
    }
  }, [isLoading, loadingMessage, loadedMessage]);

  return <ScreenReaderAnnouncer message={message} />;
};

// ============================================
// Image with Alt Text Helper
// ============================================

interface AccessibleImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  decorative?: boolean;
}

export const AccessibleImage: React.FC<AccessibleImageProps> = ({
  alt,
  decorative = false,
  ...props
}) => {
  if (decorative) {
    return <img alt="" role="presentation" aria-hidden="true" {...props} />;
  }

  // Ensure alt text is provided for non-decorative images
  if (!alt) {
    console.warn('AccessibleImage: Non-decorative images should have alt text');
  }

  return <img alt={alt || ''} {...props} />;
};

// ============================================
// Keyboard Navigation Helper
// ============================================

export const useKeyboardNavigation = (
  items: HTMLElement[],
  options: {
    loop?: boolean;
    orientation?: 'horizontal' | 'vertical' | 'both';
    onSelect?: (index: number) => void;
  } = {}
) => {
  const { loop = true, orientation = 'horizontal', onSelect } = options;
  const [activeIndex, setActiveIndex] = useState(0);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    let newIndex = activeIndex;

    const nextKey = orientation === 'vertical' ? 'ArrowDown' : 'ArrowRight';
    const prevKey = orientation === 'vertical' ? 'ArrowUp' : 'ArrowLeft';

    switch (e.key) {
      case nextKey:
      case (orientation === 'both' ? 'ArrowDown' : ''):
      case (orientation === 'both' ? 'ArrowRight' : ''):
        e.preventDefault();
        newIndex = activeIndex + 1;
        if (newIndex >= items.length) {
          newIndex = loop ? 0 : items.length - 1;
        }
        break;

      case prevKey:
      case (orientation === 'both' ? 'ArrowUp' : ''):
      case (orientation === 'both' ? 'ArrowLeft' : ''):
        e.preventDefault();
        newIndex = activeIndex - 1;
        if (newIndex < 0) {
          newIndex = loop ? items.length - 1 : 0;
        }
        break;

      case 'Home':
        e.preventDefault();
        newIndex = 0;
        break;

      case 'End':
        e.preventDefault();
        newIndex = items.length - 1;
        break;

      case 'Enter':
      case ' ':
        e.preventDefault();
        onSelect?.(activeIndex);
        return;

      default:
        return;
    }

    setActiveIndex(newIndex);
    items[newIndex]?.focus();
  };

  return {
    activeIndex,
    setActiveIndex,
    handleKeyDown,
    getItemProps: (index: number) => ({
      tabIndex: index === activeIndex ? 0 : -1,
      'aria-selected': index === activeIndex,
    }),
  };
};

// ============================================
// Reduced Motion Hook
// ============================================

export const usePrefersReducedMotion = (): boolean => {
  const [prefersReducedMotion, setPrefersReducedMotion] = useState(false);

  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    setPrefersReducedMotion(mediaQuery.matches);

    const handler = (e: MediaQueryListEvent) => {
      setPrefersReducedMotion(e.matches);
    };

    mediaQuery.addEventListener('change', handler);
    return () => mediaQuery.removeEventListener('change', handler);
  }, []);

  return prefersReducedMotion;
};

// ============================================
// High Contrast Mode Detection
// ============================================

export const usePrefersHighContrast = (): boolean => {
  const [prefersHighContrast, setPrefersHighContrast] = useState(false);

  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-contrast: more)');
    setPrefersHighContrast(mediaQuery.matches);

    const handler = (e: MediaQueryListEvent) => {
      setPrefersHighContrast(e.matches);
    };

    mediaQuery.addEventListener('change', handler);
    return () => mediaQuery.removeEventListener('change', handler);
  }, []);

  return prefersHighContrast;
};

export default {
  SkipLinks,
  ScreenReaderAnnouncer,
  FocusTrap,
  VisuallyHidden,
  LoadingAnnouncer,
  AccessibleImage,
  useKeyboardNavigation,
  usePrefersReducedMotion,
  usePrefersHighContrast,
};

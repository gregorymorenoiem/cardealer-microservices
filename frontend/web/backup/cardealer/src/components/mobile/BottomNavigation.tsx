/**
 * BottomNavigation - Mobile bottom navigation component
 * Optimized for touch interaction and low-bandwidth scenarios
 * 
 * Features:
 * - Fixed bottom position on mobile
 * - Active state indicators with smooth animations
 * - Badge support for notifications
 * - Hide on scroll down, show on scroll up
 * - Haptic feedback on supported devices
 */

import React, { useState, useEffect, useCallback } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  FiHome, 
  FiHeart, 
  FiUser, 
  FiGrid,
  FiMessageCircle
} from 'react-icons/fi';
import { BsCarFront, BsBuilding } from 'react-icons/bs';

interface NavItem {
  path: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  badge?: number;
  activeColor?: string;
}

interface BottomNavigationProps {
  variant?: 'default' | 'marketplace';
  className?: string;
}

// Navigation items for different variants
const defaultNavItems: NavItem[] = [
  { path: '/', label: 'Inicio', icon: FiHome },
  { path: '/vehicles', label: 'Autos', icon: BsCarFront, activeColor: 'text-blue-500' },
  { path: '/properties', label: 'Inmuebles', icon: BsBuilding, activeColor: 'text-green-500' },
  { path: '/wishlist', label: 'Favoritos', icon: FiHeart },
  { path: '/profile', label: 'Perfil', icon: FiUser },
];

const marketplaceNavItems: NavItem[] = [
  { path: '/marketplace', label: 'Explorar', icon: FiGrid },
  { path: '/vehicles', label: 'Autos', icon: BsCarFront, activeColor: 'text-blue-500' },
  { path: '/properties', label: 'Inmuebles', icon: BsBuilding, activeColor: 'text-green-500' },
  { path: '/messages', label: 'Mensajes', icon: FiMessageCircle },
  { path: '/profile', label: 'Cuenta', icon: FiUser },
];

export const BottomNavigation: React.FC<BottomNavigationProps> = ({
  variant = 'default',
  className = '',
}) => {
  const location = useLocation();
  const [isVisible, setIsVisible] = useState(true);
  const [lastScrollY, setLastScrollY] = useState(0);
  const [badges, setBadges] = useState<Record<string, number>>({});

  const navItems = variant === 'marketplace' ? marketplaceNavItems : defaultNavItems;

  // Handle scroll to show/hide navigation
  const handleScroll = useCallback(() => {
    const currentScrollY = window.scrollY;
    const scrollDelta = currentScrollY - lastScrollY;

    // Only hide if scrolling down significantly and not at top
    if (scrollDelta > 10 && currentScrollY > 100) {
      setIsVisible(false);
    } else if (scrollDelta < -10 || currentScrollY < 100) {
      setIsVisible(true);
    }

    setLastScrollY(currentScrollY);
  }, [lastScrollY]);

  // Throttled scroll handler
  useEffect(() => {
    let ticking = false;

    const onScroll = () => {
      if (!ticking) {
        window.requestAnimationFrame(() => {
          handleScroll();
          ticking = false;
        });
        ticking = true;
      }
    };

    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, [handleScroll]);

  // Simulate badge updates (in real app, this would come from a context/store)
  useEffect(() => {
    // Example: Show badge for messages
    setBadges({
      '/messages': 3,
      '/wishlist': 5,
    });
  }, []);

  // Check if path is active (exact or starts with)
  const isActive = (path: string) => {
    if (path === '/') return location.pathname === '/';
    return location.pathname.startsWith(path);
  };

  // Trigger haptic feedback on supported devices
  const triggerHaptic = () => {
    if ('vibrate' in navigator) {
      navigator.vibrate(10);
    }
  };

  return (
    <>
      {/* Spacer to prevent content from being hidden behind nav */}
      <div className="h-16 md:hidden" />

      {/* Navigation */}
      <AnimatePresence>
        {isVisible && (
          <motion.nav
            initial={{ y: 100 }}
            animate={{ y: 0 }}
            exit={{ y: 100 }}
            transition={{ type: 'spring', damping: 25, stiffness: 300 }}
            className={`fixed bottom-0 left-0 right-0 z-50 md:hidden ${className}`}
          >
            {/* Background with blur */}
            <div className="absolute inset-0 bg-white/90 backdrop-blur-lg border-t border-gray-200" />

            {/* Safe area padding for devices with home indicator */}
            <div className="relative pb-safe">
              <div className="flex items-center justify-around px-2 h-16">
                {navItems.map((item) => {
                  const active = isActive(item.path);
                  const badge = badges[item.path];
                  const Icon = item.icon;

                  return (
                    <Link
                      key={item.path}
                      to={item.path}
                      onClick={triggerHaptic}
                      className="relative flex flex-col items-center justify-center flex-1 py-2 touch-manipulation"
                      aria-label={item.label}
                      aria-current={active ? 'page' : undefined}
                    >
                      {/* Active indicator */}
                      {active && (
                        <motion.div
                          layoutId="bottomNavIndicator"
                          className="absolute -top-0.5 w-8 h-1 bg-blue-500 rounded-full"
                          transition={{ type: 'spring', damping: 25, stiffness: 300 }}
                        />
                      )}

                      {/* Icon with badge */}
                      <div className="relative">
                        <motion.div
                          whileTap={{ scale: 0.9 }}
                          transition={{ duration: 0.1 }}
                        >
                          <Icon
                            className={`w-6 h-6 transition-colors duration-200 ${
                              active
                                ? item.activeColor || 'text-blue-500'
                                : 'text-gray-400'
                            }`}
                          />
                        </motion.div>

                        {/* Badge */}
                        {badge && badge > 0 && (
                          <motion.span
                            initial={{ scale: 0 }}
                            animate={{ scale: 1 }}
                            className="absolute -top-1.5 -right-1.5 min-w-[18px] h-[18px] flex items-center justify-center bg-red-500 text-white text-[10px] font-bold rounded-full px-1"
                          >
                            {badge > 99 ? '99+' : badge}
                          </motion.span>
                        )}
                      </div>

                      {/* Label */}
                      <span
                        className={`text-[10px] mt-1 font-medium transition-colors duration-200 ${
                          active
                            ? item.activeColor || 'text-blue-500'
                            : 'text-gray-500'
                        }`}
                      >
                        {item.label}
                      </span>
                    </Link>
                  );
                })}
              </div>
            </div>
          </motion.nav>
        )}
      </AnimatePresence>
    </>
  );
};

export default BottomNavigation;

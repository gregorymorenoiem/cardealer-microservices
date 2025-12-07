/**
 * Navbar - Optimized navigation with scalable design
 * Uses dropdown for categories to accommodate future growth
 */

import { useState, useEffect, useRef } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/hooks/useAuth';
import Button from '@/components/atoms/Button';
import NotificationDropdown from './NotificationDropdown';
import GlobalSearch from './GlobalSearch';
import { LanguageSwitcher } from '@/components/common';
import { 
  FiMenu, 
  FiX, 
  FiUser, 
  FiLogOut, 
  FiSettings, 
  FiMessageSquare, 
  FiShield,
  FiSearch,
  FiChevronDown,
  FiGrid,
  FiHeart
} from 'react-icons/fi';
import { FaCar, FaHome, FaKey, FaBed } from 'react-icons/fa';

export default function Navbar() {
  const { t } = useTranslation('common');
  const { isAuthenticated, user, logout } = useAuth();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isCategoryMenuOpen, setIsCategoryMenuOpen] = useState(false);
  const [isMobileSearchOpen, setIsMobileSearchOpen] = useState(false);
  const location = useLocation();
  const categoryMenuRef = useRef<HTMLDivElement>(null);

  // Categories configuration - easily extendable (using translation keys)
  const categories = [
    {
      id: 'vehicles',
      nameKey: 'nav.vehicles',
      descKey: 'nav.vehiclesDesc',
      icon: FaCar,
      href: '/vehicles',
      color: 'blue',
    },
    {
      id: 'vehicle-rental',
      nameKey: 'nav.vehicleRental',
      descKey: 'nav.vehicleRentalDesc',
      icon: FaKey,
      href: '/vehicle-rental',
      color: 'amber',
    },
    {
      id: 'properties',
      nameKey: 'nav.properties',
      descKey: 'nav.propertiesDesc',
      icon: FaHome,
      href: '/properties',
      color: 'emerald',
    },
    {
      id: 'lodging',
      nameKey: 'nav.lodging',
      descKey: 'nav.lodgingDesc',
      icon: FaBed,
      href: '/lodging',
      color: 'purple',
    },
  ];

  // Detect active category from URL
  const getActiveCategory = () => {
    const path = location.pathname;
    return categories.find(c => path.startsWith(c.href));
  };

  const activeCategory = getActiveCategory();

  // Close menus on route change
  useEffect(() => {
    setIsMobileMenuOpen(false);
    setIsCategoryMenuOpen(false);
    setIsUserMenuOpen(false);
    setIsMobileSearchOpen(false);
  }, [location.pathname]);

  // Close category menu on outside click
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (categoryMenuRef.current && !categoryMenuRef.current.contains(event.target as Node)) {
        setIsCategoryMenuOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleLogout = () => {
    logout();
    setIsUserMenuOpen(false);
  };

  const getCategoryStyles = (color: string) => {
    const styles: Record<string, { bg: string; text: string }> = {
      blue: { bg: 'bg-blue-50', text: 'text-blue-600' },
      amber: { bg: 'bg-amber-50', text: 'text-amber-600' },
      emerald: { bg: 'bg-emerald-50', text: 'text-emerald-600' },
      purple: { bg: 'bg-purple-50', text: 'text-purple-600' },
    };
    return styles[color] || styles.blue;
  };

  return (
    <nav className="bg-white shadow-sm border-b border-gray-200 sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2 flex-shrink-0">
            <div className="w-9 h-9 bg-gradient-to-br from-blue-600 to-emerald-500 rounded-xl flex items-center justify-center shadow-lg">
              <span className="text-white font-bold text-lg">O</span>
            </div>
            <div className="hidden sm:block">
              <span className="text-xl font-bold text-gray-900">Okla</span>
            </div>
          </Link>

          {/* Desktop Navigation - Compact */}
          <div className="hidden lg:flex items-center gap-1">
            {/* Categories Dropdown */}
            <div className="relative" ref={categoryMenuRef}>
              <button
                onClick={() => setIsCategoryMenuOpen(!isCategoryMenuOpen)}
                className={`flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors ${
                  activeCategory 
                    ? `${getCategoryStyles(activeCategory.color).bg} ${getCategoryStyles(activeCategory.color).text}`
                    : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                <FiGrid className="w-4 h-4" />
                <span>{activeCategory ? t(activeCategory.nameKey) : t('nav.categories')}</span>
                <FiChevronDown className={`w-4 h-4 transition-transform ${isCategoryMenuOpen ? 'rotate-180' : ''}`} />
              </button>

              {/* Dropdown Menu */}
              {isCategoryMenuOpen && (
                <>
                  <div className="fixed inset-0 z-10" onClick={() => setIsCategoryMenuOpen(false)} />
                  <div className="absolute left-0 mt-2 w-64 bg-white rounded-xl shadow-xl border border-gray-100 py-2 z-20">
                    <div className="px-4 py-2 border-b border-gray-100">
                      <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider">
                        {t('nav.explore')}
                      </p>
                    </div>
                    {categories.map((category) => {
                      const isActive = activeCategory?.id === category.id;
                      const styles = getCategoryStyles(category.color);
                      return (
                        <Link
                          key={category.id}
                          to={category.href}
                          className={`flex items-center gap-3 px-4 py-3 transition-colors ${
                            isActive ? styles.bg : 'hover:bg-gray-50'
                          }`}
                          onClick={() => setIsCategoryMenuOpen(false)}
                        >
                          <div className={`w-10 h-10 ${styles.bg} rounded-lg flex items-center justify-center`}>
                            <category.icon className={`w-5 h-5 ${styles.text}`} />
                          </div>
                          <div>
                            <p className={`font-medium ${isActive ? styles.text : 'text-gray-900'}`}>
                              {t(category.nameKey)}
                            </p>
                            <p className="text-xs text-gray-500">{t(category.descKey)}</p>
                          </div>
                        </Link>
                      );
                    })}
                  </div>
                </>
              )}
            </div>
          </div>

          {/* Global Search (Desktop) - More space */}
          <div className="hidden md:block flex-1 max-w-lg mx-6">
            <GlobalSearch placeholder={t('nav.searchPlaceholder')} />
          </div>

          {/* Right Side Actions */}
          <div className="flex items-center gap-1">
            {/* Mobile Search Toggle */}
            <button
              className="md:hidden p-2 rounded-lg hover:bg-gray-100 transition-colors"
              onClick={() => setIsMobileSearchOpen(!isMobileSearchOpen)}
              aria-label={t('buttons.search')}
            >
              <FiSearch className="w-5 h-5 text-gray-700" />
            </button>

            {/* Desktop Auth/User */}
            <div className="hidden md:flex items-center gap-1">
              {isAuthenticated && user ? (
                <>
                  {/* Wishlist */}
                  <Link
                    to="/wishlist"
                    className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
                    aria-label={t('nav.favorites')}
                  >
                    <FiHeart className="w-5 h-5" />
                  </Link>

                  {/* Notifications */}
                  <NotificationDropdown />
                  
                  {/* Messages */}
                  <Link
                    to="/messages"
                    className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
                    aria-label={t('nav.messages')}
                  >
                    <FiMessageSquare className="w-5 h-5" />
                  </Link>

                  {/* User Menu */}
                  <div className="relative ml-2">
                    <button
                      onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                      className="flex items-center gap-2 px-2 py-1.5 rounded-lg hover:bg-gray-100 transition-colors"
                    >
                      <div className="w-8 h-8 bg-gradient-to-br from-blue-500 to-emerald-500 rounded-full flex items-center justify-center">
                        <span className="text-white text-sm font-medium">
                          {(user.name || user.email || 'U').charAt(0).toUpperCase()}
                        </span>
                      </div>
                      <FiChevronDown className={`w-4 h-4 text-gray-500 transition-transform ${isUserMenuOpen ? 'rotate-180' : ''}`} />
                    </button>

                    {/* User Dropdown */}
                    {isUserMenuOpen && (
                      <>
                        <div className="fixed inset-0 z-10" onClick={() => setIsUserMenuOpen(false)} />
                        <div className="absolute right-0 mt-2 w-56 bg-white rounded-xl shadow-xl border border-gray-100 py-2 z-20">
                          <div className="px-4 py-3 border-b border-gray-100">
                            <p className="text-sm font-medium text-gray-900">{user.name || user.email}</p>
                            <p className="text-xs text-gray-500">{user.email}</p>
                          </div>
                          <div className="py-1">
                            <Link
                              to="/dashboard"
                              className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50"
                              onClick={() => setIsUserMenuOpen(false)}
                            >
                              <FiUser className="w-4 h-4" />
                              {t('nav.dashboard')}
                            </Link>
                            <Link
                              to="/profile"
                              className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50"
                              onClick={() => setIsUserMenuOpen(false)}
                            >
                              <FiSettings className="w-4 h-4" />
                              {t('nav.settings')}
                            </Link>
                            {(user.roles?.includes('admin') || user.accountType === 'admin') && (
                              <>
                                <hr className="my-1 border-gray-100" />
                                <Link
                                  to="/admin"
                                  className="flex items-center gap-3 px-4 py-2.5 text-sm text-purple-600 hover:bg-purple-50 font-medium"
                                  onClick={() => setIsUserMenuOpen(false)}
                                >
                                  <FiShield className="w-4 h-4" />
                                  {t('nav.adminPortal')}
                                </Link>
                              </>
                            )}
                            {(user.accountType === 'dealer' || user.accountType === 'dealer_employee') && (
                              <>
                                <hr className="my-1 border-gray-100" />
                                <Link
                                  to="/dealer"
                                  className="flex items-center gap-3 px-4 py-2.5 text-sm text-blue-600 hover:bg-blue-50 font-medium"
                                  onClick={() => setIsUserMenuOpen(false)}
                                >
                                  <FiUser className="w-4 h-4" />
                                  {t('nav.dealerPortal')}
                                </Link>
                              </>
                            )}
                          </div>
                          <div className="border-t border-gray-100 pt-1">
                            <button
                              onClick={handleLogout}
                              className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50"
                            >
                              <FiLogOut className="w-4 h-4" />
                              {t('nav.logout')}
                            </button>
                          </div>
                        </div>
                      </>
                    )}
                  </div>
                </>
              ) : (
                <div className="flex items-center gap-2">
                  <Link to="/login">
                    <Button variant="ghost" size="sm">
                      {t('nav.login')}
                    </Button>
                  </Link>
                  <Link to="/register">
                    <Button variant="primary" size="sm">
                      {t('nav.register')}
                    </Button>
                  </Link>
                </div>
              )}

              {/* Language Switcher */}
              <LanguageSwitcher variant="minimal" className="ml-2" />
            </div>

            {/* Mobile Menu Button */}
            <button
              className="lg:hidden p-2 rounded-lg hover:bg-gray-100 transition-colors ml-1"
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              aria-label="Menu"
            >
              {isMobileMenuOpen ? (
                <FiX className="w-6 h-6 text-gray-700" />
              ) : (
                <FiMenu className="w-6 h-6 text-gray-700" />
              )}
            </button>
          </div>
        </div>

        {/* Mobile Search Bar */}
        {isMobileSearchOpen && (
          <div className="md:hidden py-3 border-t border-gray-100">
            <GlobalSearch showOnMobile placeholder={t('nav.searchPlaceholderShort')} />
          </div>
        )}

        {/* Mobile Menu */}
        {isMobileMenuOpen && (
          <div className="lg:hidden py-4 border-t border-gray-100 max-h-[calc(100vh-4rem)] overflow-y-auto">
            {/* Categories */}
            <div className="px-2 mb-4">
              <p className="px-3 text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">
                {t('nav.categories')}
              </p>
              <div className="space-y-1">
                {categories.map((category) => {
                  const isActive = activeCategory?.id === category.id;
                  const styles = getCategoryStyles(category.color);
                  return (
                    <Link
                      key={category.id}
                      to={category.href}
                      className={`flex items-center gap-3 px-3 py-3 rounded-xl transition-colors ${
                        isActive ? styles.bg : 'hover:bg-gray-50'
                      }`}
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      <div className={`w-10 h-10 ${styles.bg} rounded-lg flex items-center justify-center`}>
                        <category.icon className={`w-5 h-5 ${styles.text}`} />
                      </div>
                      <div>
                        <p className={`font-medium ${isActive ? styles.text : 'text-gray-900'}`}>
                          {t(category.nameKey)}
                        </p>
                        <p className="text-xs text-gray-500">{t(category.descKey)}</p>
                      </div>
                    </Link>
                  );
                })}
              </div>
            </div>

            <hr className="my-3 border-gray-100" />

            {/* User Section */}
            {isAuthenticated && user ? (
              <>
                <div className="px-4 py-3 flex items-center gap-3 mb-2">
                  <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-emerald-500 rounded-full flex items-center justify-center">
                    <span className="text-white font-medium">
                      {(user.name || user.email || 'U').charAt(0).toUpperCase()}
                    </span>
                  </div>
                  <div>
                    <p className="font-medium text-gray-900">{user.name || user.email}</p>
                    <p className="text-xs text-gray-500">{user.email}</p>
                  </div>
                </div>

                <div className="px-2 space-y-1">
                  <Link
                    to="/dashboard"
                    className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-xl transition-colors"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <FiUser className="w-5 h-5" />
                    {t('nav.dashboard')}
                  </Link>
                  <Link
                    to="/wishlist"
                    className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-xl transition-colors"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <FiHeart className="w-5 h-5" />
                    {t('nav.favorites')}
                  </Link>
                  <Link
                    to="/messages"
                    className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-xl transition-colors"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <FiMessageSquare className="w-5 h-5" />
                    {t('nav.messages')}
                  </Link>
                  <Link
                    to="/profile"
                    className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-xl transition-colors"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <FiSettings className="w-5 h-5" />
                    {t('nav.settings')}
                  </Link>
                  {(user.roles?.includes('admin') || user.accountType === 'admin') && (
                    <Link
                      to="/admin"
                      className="flex items-center gap-3 px-4 py-3 text-purple-600 hover:bg-purple-50 rounded-xl transition-colors font-medium"
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      <FiShield className="w-5 h-5" />
                      {t('nav.adminPortal')}
                    </Link>
                  )}
                  {(user.accountType === 'dealer' || user.accountType === 'dealer_employee') && (
                    <Link
                      to="/dealer"
                      className="flex items-center gap-3 px-4 py-3 text-blue-600 hover:bg-blue-50 rounded-xl transition-colors font-medium"
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      <FiUser className="w-5 h-5" />
                      {t('nav.dealerPortal')}
                    </Link>
                  )}
                </div>

                <hr className="my-3 border-gray-100" />

                <div className="px-2">
                  <button
                    onClick={() => {
                      handleLogout();
                      setIsMobileMenuOpen(false);
                    }}
                    className="w-full flex items-center gap-3 px-4 py-3 text-red-600 hover:bg-red-50 rounded-xl transition-colors"
                  >
                    <FiLogOut className="w-5 h-5" />
                    {t('nav.logout')}
                  </button>
                </div>
              </>
            ) : (
              <div className="px-4 py-2 space-y-2">
                <Link
                  to="/login"
                  className="block w-full px-4 py-3 text-center text-gray-700 hover:bg-gray-50 rounded-xl transition-colors font-medium border border-gray-200"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  {t('nav.login')}
                </Link>
                <Link
                  to="/register"
                  className="block w-full px-4 py-3 text-center bg-blue-600 text-white hover:bg-blue-700 rounded-xl transition-colors font-medium"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  {t('nav.register')}
                </Link>
              </div>
            )}
          </div>
        )}
      </div>
    </nav>
  );
}

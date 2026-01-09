/**
 * Navbar - Professional navigation for vehicle marketplace
 * Clean, modern design with premium feel
 */

import { useState, useEffect } from 'react';
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
  FiHeart,
  FiPlusCircle,
  FiGrid,
  FiBriefcase,
  FiUserCheck,
  FiTarget,
  FiBarChart3,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';

export default function Navbar() {
  const { t } = useTranslation('common');
  const { isAuthenticated, user, logout } = useAuth();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isMobileSearchOpen, setIsMobileSearchOpen] = useState(false);
  const [isJoinMenuOpen, setIsJoinMenuOpen] = useState(false);
  const [isScrolled, setIsScrolled] = useState(false);
  const location = useLocation();

  // Handle scroll effect
  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 10);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Close menus on route change
  useEffect(() => {
    setIsMobileMenuOpen(false);
    setIsUserMenuOpen(false);
    setIsMobileSearchOpen(false);
    setIsJoinMenuOpen(false);
  }, [location.pathname]);

  const handleLogout = () => {
    logout();
    setIsUserMenuOpen(false);
  };

  const navLinks = [
    { href: '/vehicles', label: t('nav.vehicles'), icon: FaCar },
    { href: '/search', label: 'Buscar', icon: FiSearch },
    { href: '/dealer/landing', label: 'Para Dealers', icon: FiBriefcase },
  ];

  const userNavLinks = [
    { href: '/favorites', label: 'Favoritos', icon: FiHeart },
    { href: '/comparison', label: 'Comparar', icon: FiGrid },
    { href: '/alerts', label: 'Alertas', icon: FiBriefcase },
    { href: '/my-inquiries', label: 'Mis Consultas', icon: FiMessageSquare },
    { href: '/received-inquiries', label: 'Consultas Recibidas', icon: FiUserCheck },
    { href: '/dealer/leads', label: 'Mis Leads', icon: FiTarget }, // Sprint 11 - Lead Scoring
  ];

  // Dealer-specific navigation links (Sprint 12 - Advanced Dashboard)
  const dealerNavLinks = [
    { href: '/dealer/dashboard', label: 'Dashboard', icon: FiGrid },
    { href: '/dealer/inventory', label: 'Inventario', icon: FaCar },
    { href: '/dealer/analytics/advanced', label: 'Analytics Avanzado', icon: FiBarChart3 },
    { href: '/dealer/leads', label: 'Leads', icon: FiTarget },
  ];

  return (
    <nav className="bg-white border-b border-gray-200 shadow-md">
      {/* Top Bar - Contact/Info (hidden on tablet and mobile) */}
      <div className="hidden lg:block bg-gradient-to-r from-gray-900 via-gray-800 to-gray-900 text-gray-300">
        <div className="max-w-7xl mx-auto px-6 lg:px-8 py-2.5 flex justify-between items-center">
          <div className="flex items-center gap-8">
            <a
              href="tel:+15551234567"
              className="flex items-center gap-2 hover:text-white transition-colors duration-200"
            >
              <span>📞</span>
              <span className="text-sm font-medium">+1 (555) 123-4567</span>
            </a>
            <a
              href="mailto:ventas@okla.com"
              className="flex items-center gap-2 hover:text-white transition-colors duration-200"
            >
              <span>📧</span>
              <span className="text-sm font-medium">ventas@okla.com</span>
            </a>
          </div>
          <div className="flex items-center gap-6">
            <Link
              to="/about"
              className="text-sm font-medium hover:text-white transition-colors duration-200"
            >
              Acerca de
            </Link>
            <Link
              to="/contact"
              className="text-sm font-medium hover:text-white transition-colors duration-200"
            >
              Contacto
            </Link>
            <Link
              to="/help"
              className="text-sm font-medium hover:text-white transition-colors duration-200"
            >
              Ayuda
            </Link>
          </div>
        </div>
      </div>

      {/* Main Navbar */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16 sm:h-18 lg:h-20">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-3 flex-shrink-0 group">
            <div className="w-10 h-10 sm:w-11 sm:h-11 lg:w-12 lg:h-12 bg-gradient-to-br from-blue-600 via-blue-500 to-emerald-500 rounded-xl flex items-center justify-center shadow-lg group-hover:shadow-2xl group-hover:shadow-blue-500/30 group-hover:scale-105 transition-all duration-300">
              <span className="text-white font-bold text-xl sm:text-2xl">O</span>
            </div>
            <div className="flex flex-col">
              <span className="text-xl sm:text-2xl font-bold bg-gradient-to-r from-gray-900 via-gray-800 to-gray-700 bg-clip-text text-transparent leading-tight">
                Okla
              </span>
              <span className="hidden sm:block text-[10px] lg:text-xs text-gray-500 -mt-0.5 tracking-widest uppercase font-semibold">
                Marketplace
              </span>
            </div>
          </Link>

          {/* Desktop/Tablet Navigation */}
          <div className="hidden md:flex items-center gap-2 lg:gap-3">
            {navLinks.map((link) => (
              <Link
                key={link.href}
                to={link.href}
                className={`flex items-center gap-2 px-4 lg:px-5 py-2.5 rounded-xl font-semibold text-sm lg:text-base transition-all duration-200 ${
                  location.pathname === link.href ||
                  (link.href === '/vehicles' && location.pathname.startsWith('/vehicles'))
                    ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/40 scale-105'
                    : 'text-gray-700 hover:text-gray-900 hover:bg-gray-100 hover:scale-105'
                }`}
              >
                <link.icon className="w-4 h-4 lg:w-5 lg:h-5" />
                <span>{link.label}</span>
              </Link>
            ))}

            {/* User-only navigation links */}
            {isAuthenticated &&
              user &&
              userNavLinks.map((link) => (
                <Link
                  key={link.href}
                  to={link.href}
                  className={`flex items-center gap-2 px-4 lg:px-5 py-2.5 rounded-xl font-semibold text-sm lg:text-base transition-all duration-200 ${
                    location.pathname === link.href
                      ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/40 scale-105'
                      : 'text-gray-700 hover:text-gray-900 hover:bg-gray-100 hover:scale-105'
                  }`}
                >
                  <link.icon className="w-4 h-4 lg:w-5 lg:h-5" />
                  <span>{link.label}</span>
                </Link>
              ))}

            {/* Dealer-only navigation links (Sprint 12) */}
            {isAuthenticated &&
              user &&
              (user.accountType === 'dealer' || user.accountType === 'dealer_employee') &&
              dealerNavLinks.map((link) => (
                <Link
                  key={link.href}
                  to={link.href}
                  className={`flex items-center gap-2 px-4 lg:px-5 py-2.5 rounded-xl font-semibold text-sm lg:text-base transition-all duration-200 ${
                    location.pathname === link.href
                      ? 'bg-emerald-600 text-white shadow-lg shadow-emerald-600/40 scale-105'
                      : 'text-gray-700 hover:text-gray-900 hover:bg-emerald-50 hover:scale-105'
                  }`}
                >
                  <link.icon className="w-4 h-4 lg:w-5 lg:h-5" />
                  <span>{link.label}</span>
                </Link>
              ))}

            {/* Sell Button - CTA - Solo para usuarios autenticados */}
            {isAuthenticated && user && (
              <Link
                to="/sell"
                className="flex items-center gap-2 px-5 lg:px-6 py-2.5 ml-2 lg:ml-3 bg-gradient-to-r from-emerald-500 via-emerald-600 to-teal-500 text-white rounded-xl font-semibold text-sm lg:text-base shadow-lg shadow-emerald-500/40 hover:shadow-xl hover:shadow-emerald-500/50 hover:scale-105 transition-all duration-200"
              >
                <FiPlusCircle className="w-4 h-4 lg:w-5 lg:h-5" />
                <span>Vender</span>
              </Link>
            )}
          </div>

          {/* Global Search - Responsive */}
          <div className="hidden sm:block flex-1 max-w-[280px] md:max-w-sm lg:max-w-md xl:max-w-lg mx-4 lg:mx-6">
            <GlobalSearch placeholder={t('nav.searchPlaceholder')} />
          </div>

          {/* Right Side Actions */}
          <div className="flex items-center gap-1.5 sm:gap-2">
            {/* Mobile Search Toggle */}
            <button
              className="sm:hidden p-2.5 rounded-xl hover:bg-gray-100 transition-all duration-200"
              onClick={() => setIsMobileSearchOpen(!isMobileSearchOpen)}
              aria-label={t('buttons.search')}
            >
              <FiSearch className="w-5 h-5 text-gray-600" />
            </button>

            {/* Tablet/Desktop Auth/User */}
            <div className="hidden sm:flex items-center gap-2">
              {isAuthenticated && user ? (
                <>
                  {/* Wishlist */}
                  <Link
                    to="/wishlist"
                    className="p-2.5 lg:p-3 text-gray-600 hover:text-red-500 hover:bg-red-50 rounded-xl transition-all duration-200 relative group"
                    aria-label={t('nav.favorites')}
                  >
                    <FiHeart className="w-5 h-5 lg:w-5.5 lg:h-5.5 group-hover:scale-110 transition-transform" />
                  </Link>

                  {/* Notifications - Hidden on small tablets */}
                  <div className="hidden lg:block">
                    <NotificationDropdown />
                  </div>

                  {/* Messages - Hidden on small tablets */}
                  <Link
                    to="/messages"
                    className="hidden lg:flex p-2.5 lg:p-3 text-gray-600 hover:text-blue-600 hover:bg-blue-50 rounded-xl transition-all duration-200 group"
                    aria-label={t('nav.messages')}
                  >
                    <FiMessageSquare className="w-5 h-5 lg:w-5.5 lg:h-5.5 group-hover:scale-110 transition-transform" />
                  </Link>

                  {/* User Menu */}
                  <div className="relative ml-2 lg:ml-3">
                    <button
                      onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                      className="flex items-center gap-2.5 lg:gap-3 px-3 lg:px-4 py-2 lg:py-2.5 rounded-xl hover:bg-gray-50 transition-all duration-200 border-2 border-transparent hover:border-gray-200 hover:shadow-md"
                    >
                      <div className="w-9 h-9 lg:w-10 lg:h-10 bg-gradient-to-br from-blue-600 via-blue-500 to-emerald-500 rounded-full flex items-center justify-center ring-2 ring-white shadow-md">
                        <span className="text-white text-sm lg:text-base font-bold">
                          {(user.name || user.email || 'U').charAt(0).toUpperCase()}
                        </span>
                      </div>
                      <div className="hidden xl:block text-left">
                        <p className="text-sm font-semibold text-gray-900 leading-tight truncate max-w-[120px]">
                          {user.name || 'Usuario'}
                        </p>
                        <p className="text-xs text-gray-500 font-medium">Mi cuenta</p>
                      </div>
                      <FiChevronDown
                        className={`hidden lg:block w-4 h-4 text-gray-400 transition-transform duration-200 ${isUserMenuOpen ? 'rotate-180' : ''}`}
                      />
                    </button>

                    {/* User Dropdown */}
                    {isUserMenuOpen && (
                      <>
                        <div
                          className="fixed inset-0 z-10"
                          onClick={() => setIsUserMenuOpen(false)}
                        />
                        <div className="absolute right-0 mt-3 w-72 lg:w-80 bg-white rounded-2xl shadow-2xl border border-gray-200/50 py-2 z-20 animate-in fade-in slide-in-from-top-2 duration-200">
                          <div className="px-5 py-4 border-b border-gray-100 bg-gradient-to-br from-blue-50 via-blue-50 to-emerald-50 rounded-t-2xl">
                            <div className="flex items-center gap-4">
                              <div className="w-14 h-14 bg-gradient-to-br from-blue-600 via-blue-500 to-emerald-500 rounded-full flex items-center justify-center shadow-xl ring-2 ring-white">
                                <span className="text-white text-xl font-bold">
                                  {(user.name || user.email || 'U').charAt(0).toUpperCase()}
                                </span>
                              </div>
                              <div className="flex-1 min-w-0">
                                <p className="font-bold text-gray-900 truncate text-base">
                                  {user.name || user.email}
                                </p>
                                <p className="text-sm text-gray-600 truncate">{user.email}</p>
                              </div>
                            </div>
                          </div>
                          <div className="py-2">
                            <Link
                              to="/dashboard"
                              className="flex items-center gap-3 px-5 py-3.5 text-sm text-gray-700 hover:bg-blue-50 transition-colors group"
                              onClick={() => setIsUserMenuOpen(false)}
                            >
                              <div className="w-10 h-10 bg-blue-100 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                                <FiGrid className="w-5 h-5 text-blue-600" />
                              </div>
                              <div>
                                <p className="font-semibold text-gray-900">{t('nav.dashboard')}</p>
                                <p className="text-xs text-gray-500 mt-0.5">Panel de control</p>
                              </div>
                            </Link>

                            {/* Dealer Profile Link */}
                            {(user.accountType === 'dealer' ||
                              user.accountType === 'dealer_employee') && (
                              <Link
                                to="/dealer/profile"
                                className="flex items-center gap-3 px-5 py-3.5 text-sm text-gray-700 hover:bg-emerald-50 transition-colors group"
                                onClick={() => setIsUserMenuOpen(false)}
                              >
                                <div className="w-10 h-10 bg-emerald-100 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                                  <FiBriefcase className="w-5 h-5 text-emerald-600" />
                                </div>
                                <div>
                                  <p className="font-semibold text-emerald-700">Mi Dealer</p>
                                  <p className="text-xs text-emerald-600 mt-0.5">
                                    Gestión de negocio
                                  </p>
                                </div>
                              </Link>
                            )}

                            <Link
                              to="/profile"
                              className="flex items-center gap-3 px-5 py-3.5 text-sm text-gray-700 hover:bg-gray-50 transition-colors group"
                              onClick={() => setIsUserMenuOpen(false)}
                            >
                              <div className="w-10 h-10 bg-gray-100 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                                <FiUser className="w-5 h-5 text-gray-600" />
                              </div>
                              <div>
                                <p className="font-semibold text-gray-900">Mi Perfil</p>
                                <p className="text-xs text-gray-500 mt-0.5">Información personal</p>
                              </div>
                            </Link>
                            {(user.roles?.includes('admin') || user.accountType === 'admin') && (
                              <>
                                <hr className="my-2 border-gray-200" />
                                <Link
                                  to="/admin"
                                  className="flex items-center gap-3 px-5 py-3.5 text-sm hover:bg-purple-50 transition-colors group"
                                  onClick={() => setIsUserMenuOpen(false)}
                                >
                                  <div className="w-10 h-10 bg-purple-100 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                                    <FiShield className="w-5 h-5 text-purple-600" />
                                  </div>
                                  <div>
                                    <p className="font-semibold text-purple-700">
                                      {t('nav.adminPortal')}
                                    </p>
                                    <p className="text-xs text-purple-600 mt-0.5">Administración</p>
                                  </div>
                                </Link>
                              </>
                            )}
                            {(user.accountType === 'dealer' ||
                              user.accountType === 'dealer_employee') && (
                              <>
                                <hr className="my-2 border-gray-200" />
                                <Link
                                  to="/dealer"
                                  className="flex items-center gap-3 px-5 py-3.5 text-sm hover:bg-blue-50 transition-colors group"
                                  onClick={() => setIsUserMenuOpen(false)}
                                >
                                  <div className="w-10 h-10 bg-blue-100 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                                    <FaCar className="w-5 h-5 text-blue-600" />
                                  </div>
                                  <div>
                                    <p className="font-semibold text-blue-700">
                                      {t('nav.dealerPortal')}
                                    </p>
                                    <p className="text-xs text-blue-600 mt-0.5">Portal de ventas</p>
                                  </div>
                                </Link>
                              </>
                            )}
                          </div>
                          <div className="border-t border-gray-200 pt-2 px-2 pb-1">
                            <button
                              onClick={handleLogout}
                              className="w-full flex items-center justify-center gap-3 px-4 py-3.5 text-sm text-red-600 hover:bg-red-50 rounded-xl transition-all font-semibold hover:scale-[1.02]"
                            >
                              <FiLogOut className="w-5 h-5" />
                              <span>{t('nav.logout')}</span>
                            </button>
                          </div>
                        </div>
                      </>
                    )}
                  </div>
                </>
              ) : (
                <div className="flex items-center gap-3 lg:gap-4">
                  <Link
                    to="/login"
                    className="px-4 lg:px-5 py-2.5 text-sm lg:text-base font-semibold text-gray-700 hover:text-gray-900 hover:bg-gray-50 rounded-xl transition-all duration-200"
                  >
                    {t('nav.login')}
                  </Link>

                  {/* Dropdown Únete */}
                  <div className="relative">
                    <button
                      onClick={() => setIsJoinMenuOpen(!isJoinMenuOpen)}
                      className="flex items-center gap-2 px-5 lg:px-6 py-2.5 text-sm lg:text-base font-semibold text-white bg-gradient-to-r from-blue-600 via-blue-500 to-blue-600 rounded-xl shadow-lg shadow-blue-500/40 hover:shadow-xl hover:shadow-blue-500/50 hover:scale-105 transition-all duration-200"
                    >
                      <span>Únete</span>
                      <FiChevronDown
                        className={`w-4 h-4 transition-transform duration-200 ${isJoinMenuOpen ? 'rotate-180' : ''}`}
                      />
                    </button>

                    {/* Join Dropdown Menu */}
                    {isJoinMenuOpen && (
                      <>
                        <div
                          className="fixed inset-0 z-10"
                          onClick={() => setIsJoinMenuOpen(false)}
                        />
                        <div className="absolute right-0 mt-3 w-80 lg:w-96 bg-white rounded-2xl shadow-2xl border border-gray-200/50 py-2 z-20 animate-in fade-in slide-in-from-top-2 duration-200">
                          <div className="px-5 py-4 border-b border-gray-100">
                            <p className="text-base font-bold text-gray-900">
                              ¿Cómo quieres vender?
                            </p>
                            <p className="text-sm text-gray-600 mt-1">
                              Elige el tipo de cuenta que necesitas
                            </p>
                          </div>

                          <div className="py-2">
                            {/* Registrarse como Dealer */}
                            <Link
                              to="/dealer/onboarding"
                              className="flex items-start gap-4 px-5 py-4 hover:bg-emerald-50 transition-colors group"
                              onClick={() => setIsJoinMenuOpen(false)}
                            >
                              <div className="w-12 h-12 bg-emerald-100 rounded-xl flex items-center justify-center flex-shrink-0 group-hover:scale-110 transition-transform shadow-md">
                                <FiBriefcase className="w-6 h-6 text-emerald-600" />
                              </div>
                              <div className="flex-1">
                                <p className="font-bold text-emerald-700 text-base">
                                  Soy un Dealer
                                </p>
                                <p className="text-sm text-gray-600 mt-1 leading-relaxed">
                                  Agencia, concesionario o negocio de vehículos
                                </p>
                                <div className="flex flex-wrap gap-2 mt-2.5">
                                  <span className="text-xs px-3 py-1 bg-emerald-100 text-emerald-700 rounded-full font-medium">
                                    Múltiples listados
                                  </span>
                                  <span className="text-xs px-3 py-1 bg-emerald-100 text-emerald-700 rounded-full font-medium">
                                    Dashboard PRO
                                  </span>
                                </div>
                              </div>
                            </Link>

                            {/* Registrarse como Vendedor Individual */}
                            <Link
                              to="/seller/create"
                              className="flex items-start gap-4 px-5 py-4 hover:bg-purple-50 transition-colors group"
                              onClick={() => setIsJoinMenuOpen(false)}
                            >
                              <div className="w-12 h-12 bg-purple-100 rounded-xl flex items-center justify-center flex-shrink-0 group-hover:scale-110 transition-transform shadow-md">
                                <FiUserCheck className="w-6 h-6 text-purple-600" />
                              </div>
                              <div className="flex-1">
                                <p className="font-bold text-purple-700 text-base">
                                  Soy Vendedor Individual
                                </p>
                                <p className="text-sm text-gray-600 mt-1 leading-relaxed">
                                  Vendo mi vehículo personal u ocasional
                                </p>
                                <div className="flex flex-wrap gap-2 mt-2.5">
                                  <span className="text-xs px-3 py-1 bg-purple-100 text-purple-700 rounded-full font-medium">
                                    Gratis
                                  </span>
                                  <span className="text-xs px-3 py-1 bg-purple-100 text-purple-700 rounded-full font-medium">
                                    Fácil y rápido
                                  </span>
                                </div>
                              </div>
                            </Link>
                          </div>

                          <div className="px-5 py-4 border-t border-gray-100 bg-gray-50 rounded-b-2xl">
                            <p className="text-sm text-gray-600">
                              ¿Ya tienes cuenta?{' '}
                              <Link
                                to="/login"
                                className="text-blue-600 hover:text-blue-700 font-semibold"
                                onClick={() => setIsJoinMenuOpen(false)}
                              >
                                Inicia sesión
                              </Link>
                            </p>
                          </div>
                        </div>
                      </>
                    )}
                  </div>
                </div>
              )}

              {/* Language Switcher - Hidden on very small screens */}
              <div className="hidden lg:block ml-3 lg:ml-4 pl-3 lg:pl-4 border-l-2 border-gray-200">
                <LanguageSwitcher variant="minimal" />
              </div>
            </div>

            {/* Mobile Menu Button */}
            <button
              className="md:hidden p-2.5 rounded-xl hover:bg-gray-100 transition-all duration-200 ml-2"
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
          <div className="sm:hidden py-3 border-t border-gray-200">
            <GlobalSearch showOnMobile placeholder={t('nav.searchPlaceholderShort')} />
          </div>
        )}

        {/* Mobile/Tablet Menu */}
        {isMobileMenuOpen && (
          <div className="md:hidden py-4 border-t border-gray-200 max-h-[calc(100vh-5rem)] overflow-y-auto bg-white shadow-inner">
            {/* Navigation Links */}
            <div className="px-3 mb-4 space-y-2">
              {navLinks.map((link) => (
                <Link
                  key={link.href}
                  to={link.href}
                  className={`flex items-center gap-3 px-4 py-3.5 rounded-xl font-semibold transition-all duration-200 ${
                    location.pathname === link.href ||
                    (link.href === '/vehicles' && location.pathname.startsWith('/vehicles'))
                      ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/30'
                      : 'text-gray-700 hover:bg-gray-100 hover:scale-[1.02]'
                  }`}
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  <link.icon className="w-5 h-5" />
                  <span className="text-base">{link.label}</span>
                </Link>
              ))}

              {/* User-only navigation links */}
              {isAuthenticated &&
                user &&
                userNavLinks.map((link) => (
                  <Link
                    key={link.href}
                    to={link.href}
                    className={`flex items-center gap-3 px-4 py-3.5 rounded-xl font-semibold transition-all duration-200 ${
                      location.pathname === link.href
                        ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/30'
                        : 'text-gray-700 hover:bg-gray-100 hover:scale-[1.02]'
                    }`}
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <link.icon className="w-5 h-5" />
                    <span className="text-base">{link.label}</span>
                  </Link>
                ))}

              {/* Dealer-only navigation links (Sprint 12) */}
              {isAuthenticated &&
                user &&
                (user.accountType === 'dealer' || user.accountType === 'dealer_employee') &&
                dealerNavLinks.map((link) => (
                  <Link
                    key={link.href}
                    to={link.href}
                    className={`flex items-center gap-3 px-4 py-3.5 rounded-xl font-semibold transition-all duration-200 ${
                      location.pathname === link.href
                        ? 'bg-emerald-600 text-white shadow-lg shadow-emerald-600/30'
                        : 'text-gray-700 hover:bg-emerald-50 hover:scale-[1.02]'
                    }`}
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <link.icon className="w-5 h-5" />
                    <span className="text-base">{link.label}</span>
                  </Link>
                ))}

              {/* Sell CTA - Solo para usuarios autenticados */}
              {isAuthenticated && user && (
                <Link
                  to="/sell"
                  className="flex items-center gap-3 px-4 py-3.5 rounded-xl bg-gradient-to-r from-emerald-500 via-emerald-600 to-teal-500 text-white shadow-lg shadow-emerald-500/40 font-semibold hover:scale-[1.02] transition-all"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  <FiPlusCircle className="w-5 h-5" />
                  <span className="text-base">Vender mi Vehículo</span>
                </Link>
              )}
            </div>

            <hr className="my-4 border-gray-200" />

            {/* User Section */}
            {isAuthenticated && user ? (
              <>
                <div className="px-4 py-4 mx-3 flex items-center gap-4 mb-3 bg-gradient-to-br from-blue-50 via-blue-50 to-emerald-50 rounded-xl shadow-sm">
                  <div className="w-12 h-12 bg-gradient-to-br from-blue-600 via-blue-500 to-emerald-500 rounded-full flex items-center justify-center shadow-lg ring-2 ring-white">
                    <span className="text-white font-bold text-lg">
                      {(user.name || user.email || 'U').charAt(0).toUpperCase()}
                    </span>
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="font-bold text-gray-900 text-base truncate">
                      {user.name || user.email}
                    </p>
                    <p className="text-sm text-gray-600 truncate">{user.email}</p>
                  </div>
                </div>

                <div className="px-3 space-y-1.5">
                  <Link
                    to="/dashboard"
                    className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-blue-50 rounded-xl transition-all font-medium group"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <div className="w-9 h-9 bg-blue-100 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
                      <FiGrid className="w-5 h-5 text-blue-600" />
                    </div>
                    <span className="text-base">{t('nav.dashboard')}</span>
                  </Link>
                  <Link
                    to="/wishlist"
                    className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-red-50 rounded-xl transition-all font-medium group"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <div className="w-9 h-9 bg-red-100 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
                      <FiHeart className="w-5 h-5 text-red-500" />
                    </div>
                    <span className="text-base">{t('nav.favorites')}</span>
                  </Link>
                  <Link
                    to="/messages"
                    className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-blue-50 rounded-xl transition-all font-medium group"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <div className="w-9 h-9 bg-blue-100 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
                      <FiMessageSquare className="w-5 h-5 text-blue-600" />
                    </div>
                    <span className="text-base">{t('nav.messages')}</span>
                  </Link>
                  <Link
                    to="/profile"
                    className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-gray-100 rounded-xl transition-all font-medium group"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <div className="w-9 h-9 bg-gray-100 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
                      <FiSettings className="w-5 h-5 text-gray-600" />
                    </div>
                    <span className="text-base">{t('nav.settings')}</span>
                  </Link>
                  {(user.roles?.includes('admin') || user.accountType === 'admin') && (
                    <Link
                      to="/admin"
                      className="flex items-center gap-3 px-4 py-3 text-purple-700 hover:bg-purple-50 rounded-xl transition-all font-medium group"
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      <div className="w-9 h-9 bg-purple-100 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
                        <FiShield className="w-5 h-5 text-purple-600" />
                      </div>
                      <span className="text-base">{t('nav.adminPortal')}</span>
                    </Link>
                  )}
                  {(user.accountType === 'dealer' || user.accountType === 'dealer_employee') && (
                    <Link
                      to="/dealer"
                      className="flex items-center gap-3 px-4 py-3 text-blue-700 hover:bg-blue-50 rounded-xl transition-all font-medium group"
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      <div className="w-9 h-9 bg-blue-100 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
                        <FaCar className="w-5 h-5 text-blue-600" />
                      </div>
                      <span className="text-base">{t('nav.dealerPortal')}</span>
                    </Link>
                  )}
                </div>

                <hr className="my-4 border-gray-200" />

                <div className="px-3">
                  <button
                    onClick={() => {
                      handleLogout();
                      setIsMobileMenuOpen(false);
                    }}
                    className="w-full flex items-center justify-center gap-3 px-4 py-3.5 text-red-600 bg-red-50 hover:bg-red-100 rounded-xl transition-all font-semibold text-base hover:scale-[1.02]"
                  >
                    <FiLogOut className="w-5 h-5" />
                    {t('nav.logout')}
                  </button>
                </div>
              </>
            ) : (
              <div className="px-4 py-2 space-y-3">
                <Link
                  to="/login"
                  className="block w-full px-4 py-3.5 text-center text-gray-700 hover:bg-gray-50 rounded-xl transition-all font-semibold border-2 border-gray-200 text-base hover:scale-[1.02]"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  {t('nav.login')}
                </Link>
                <Link
                  to="/register"
                  className="block w-full px-4 py-3.5 text-center bg-gradient-to-r from-blue-600 via-blue-500 to-blue-600 text-white rounded-xl transition-all font-semibold shadow-lg shadow-blue-500/40 text-base hover:scale-[1.02]"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  {t('nav.register')}
                </Link>
              </div>
            )}

            {/* Language Switcher for Mobile */}
            <div className="px-4 py-4 mt-4 border-t border-gray-200">
              <LanguageSwitcher variant="minimal" />
            </div>
          </div>
        )}
      </div>
    </nav>
  );
}

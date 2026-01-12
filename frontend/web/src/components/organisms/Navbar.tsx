/**
 * Navbar Simplificado - Versión limpia y profesional
 * Menos elementos, mejor UX
 */

import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/hooks/useAuth';
import GlobalSearch from './GlobalSearch';
import UserDebugInfo from '@/components/debug/UserDebugInfo';
import {
  FiMenu,
  FiX,
  FiUser,
  FiLogOut,
  FiSettings,
  FiMessageSquare,
  FiSearch,
  FiChevronDown,
  FiHeart,
  FiPlusCircle,
  FiGrid,
  FiBriefcase,
  FiTarget,
  FiBarChart2,
  FiBell,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import NotificationDropdown from './NotificationDropdown';

export default function NavbarSimple() {
  const { t } = useTranslation('common');
  const { isAuthenticated, user, logout } = useAuth();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isMobileSearchOpen, setIsMobileSearchOpen] = useState(false);
  const location = useLocation();

  // Close menus on route change
  useEffect(() => {
    setIsMobileMenuOpen(false);
    setIsUserMenuOpen(false);
    setIsMobileSearchOpen(false);
  }, [location.pathname]);

  const handleLogout = () => {
    logout();
    setIsUserMenuOpen(false);
  };

  // Links principales - SOLO LO ESENCIAL
  // Link inteligente para dealers:
  // - No autenticado o no-dealer → Landing de marketing
  // - Dealer autenticado → Dashboard directo
  const dealerLink = (() => {
    if (
      isAuthenticated &&
      user &&
      (user.accountType === 'dealer' || user.accountType === 'dealer_employee')
    ) {
      return { href: '/dealer/dashboard', label: 'Mi Dashboard', icon: FiBriefcase };
    }
    return { href: '/dealer/landing', label: 'Para Dealers', icon: FiBriefcase };
  })();

  const mainNavLinks = [{ href: '/vehicles', label: 'Vehículos', icon: FaCar }, dealerLink];

  // Links del usuario - para el dropdown del perfil
  const userLinks = [
    { href: '/dashboard', label: 'Dashboard', icon: FiGrid },
    { href: '/favorites', label: 'Favoritos', icon: FiHeart },
    { href: '/messages', label: 'Mensajes', icon: FiMessageSquare },
    { href: '/profile', label: 'Perfil', icon: FiSettings },
  ];

  // Links para dealers - para el dropdown del perfil
  const dealerLinks = [
    { href: '/dealer/dashboard', label: 'Dashboard', icon: FiGrid },
    { href: '/dealer/inventory', label: 'Inventario', icon: FaCar },
    { href: '/dealer/crm', label: 'Leads', icon: FiTarget },
    { href: '/dealer/analytics', label: 'Analytics', icon: FiBarChart2 },
  ];

  return (
    <nav className="bg-white border-b border-gray-200 shadow-sm">
      {/* Main Navbar */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16 lg:h-18">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-3 flex-shrink-0 group">
            <div className="w-10 h-10 lg:w-11 lg:h-11 bg-gradient-to-br from-blue-600 via-blue-500 to-emerald-500 rounded-xl flex items-center justify-center shadow-lg group-hover:shadow-xl group-hover:scale-105 transition-all duration-300">
              <span className="text-white font-bold text-xl">O</span>
            </div>
            <div className="flex flex-col">
              <span className="text-xl lg:text-2xl font-bold bg-gradient-to-r from-gray-900 via-gray-800 to-gray-700 bg-clip-text text-transparent leading-tight">
                Okla
              </span>
              <span className="hidden sm:block text-[10px] text-gray-500 -mt-0.5 tracking-widest uppercase font-semibold">
                Marketplace
              </span>
            </div>
          </Link>

          {/* Desktop Navigation - LIMPIO */}
          <div className="hidden lg:flex items-center gap-4">
            {mainNavLinks.map((link) => {
              const isActive =
                location.pathname === link.href ||
                (link.href === '/vehicles' && location.pathname.startsWith('/vehicles'));

              return (
                <Link
                  key={link.href}
                  to={link.href}
                  className={`flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-sm transition-all duration-200 ${
                    isActive
                      ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/40'
                      : 'text-gray-700 hover:text-gray-900 hover:bg-gray-100'
                  }`}
                >
                  <link.icon className="w-4 h-4" />
                  <span>{link.label}</span>
                </Link>
              );
            })}
          </div>

          {/* Global Search - Responsive */}
          <div className="hidden sm:block flex-1 max-w-[300px] lg:max-w-md mx-6">
            <GlobalSearch placeholder={t('nav.searchPlaceholder')} />
          </div>

          {/* Right Side Actions - SIMPLIFICADO */}
          <div className="flex items-center gap-3">
            {/* Mobile Search Toggle */}
            <button
              className="sm:hidden p-2.5 rounded-xl hover:bg-gray-100 transition-all duration-200"
              onClick={() => setIsMobileSearchOpen(!isMobileSearchOpen)}
              aria-label="Buscar"
            >
              <FiSearch className="w-5 h-5 text-gray-600" />
            </button>

            {isAuthenticated && user ? (
              <>
                {/* CTA Vender - Prominente */}
                <Link
                  to="/sell"
                  className="hidden lg:flex items-center gap-2 px-4 py-2.5 bg-gradient-to-r from-emerald-500 via-emerald-600 to-teal-500 text-white rounded-xl font-semibold text-sm shadow-lg hover:scale-105 transition-all duration-200"
                >
                  <FiPlusCircle className="w-4 h-4" />
                  <span>Vender</span>
                </Link>

                {/* Notification Bell - Con dropdown */}
                <NotificationDropdown />

                {/* User Menu - Simplificado */}
                <div className="relative">
                  <button
                    onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                    className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-gray-50 transition-all duration-200 border border-gray-200 hover:border-gray-300"
                  >
                    <div className="w-9 h-9 bg-gradient-to-br from-blue-600 via-blue-500 to-emerald-500 rounded-full flex items-center justify-center shadow-md">
                      <span className="text-white text-sm font-bold">
                        {(user.name || user.email || 'U').charAt(0).toUpperCase()}
                      </span>
                    </div>
                    <div className="hidden lg:block text-left">
                      <p className="text-sm font-semibold text-gray-900 leading-tight truncate max-w-[100px]">
                        {user.name || 'Usuario'}
                      </p>
                      <p className="text-xs text-gray-500">Mi cuenta</p>
                    </div>
                    <FiChevronDown
                      className={`w-4 h-4 text-gray-400 transition-transform duration-200 ${isUserMenuOpen ? 'rotate-180' : ''}`}
                    />
                  </button>

                  {/* User Dropdown - Compacto */}
                  {isUserMenuOpen && (
                    <>
                      <div
                        className="fixed inset-0 z-10"
                        onClick={() => setIsUserMenuOpen(false)}
                      />
                      <div className="absolute right-0 mt-3 w-72 bg-white rounded-xl shadow-2xl border border-gray-200/50 py-2 z-20 animate-in fade-in slide-in-from-top-2 duration-200">
                        <div className="px-4 py-3 border-b border-gray-100">
                          <p className="font-semibold text-gray-900">{user.name || user.email}</p>
                          <div className="flex items-center gap-2 mt-1">
                            <p className="text-xs text-gray-500 truncate">{user.email}</p>
                            {(user.accountType === 'dealer' ||
                              user.accountType === 'dealer_employee') && (
                              <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-emerald-100 text-emerald-700 text-xs font-semibold rounded-full">
                                <FiBriefcase className="w-3 h-3" />
                                Dealer
                              </span>
                            )}
                          </div>
                        </div>

                        <div className="py-2">
                          {/* Links contextales según tipo de usuario */}
                          {user.accountType === 'dealer' || user.accountType === 'dealer_employee'
                            ? dealerLinks.map((link) => (
                                <Link
                                  key={link.href}
                                  to={link.href}
                                  onClick={() => setIsUserMenuOpen(false)}
                                  className="flex items-center gap-3 px-4 py-3 hover:bg-gray-50 transition-colors group"
                                >
                                  <div className="w-8 h-8 bg-emerald-100 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
                                    <link.icon className="w-4 h-4 text-emerald-600" />
                                  </div>
                                  <span className="font-medium text-gray-900 text-sm">
                                    {link.label}
                                  </span>
                                </Link>
                              ))
                            : userLinks.map((link) => (
                                <Link
                                  key={link.href}
                                  to={link.href}
                                  onClick={() => setIsUserMenuOpen(false)}
                                  className="flex items-center gap-3 px-4 py-3 hover:bg-gray-50 transition-colors group"
                                >
                                  <div className="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
                                    <link.icon className="w-4 h-4 text-blue-600" />
                                  </div>
                                  <span className="font-medium text-gray-900 text-sm">
                                    {link.label}
                                  </span>
                                </Link>
                              ))}
                        </div>

                        <div className="border-t border-gray-200 pt-2">
                          <button
                            onClick={handleLogout}
                            className="w-full flex items-center justify-center gap-2 px-4 py-3 text-red-600 hover:bg-red-50 transition-all font-medium"
                          >
                            <FiLogOut className="w-4 h-4" />
                            <span>Cerrar sesión</span>
                          </button>
                        </div>
                      </div>
                    </>
                  )}
                </div>
              </>
            ) : (
              /* Login/Register for non-authenticated users */
              <div className="hidden sm:flex items-center gap-3">
                <Link
                  to="/login"
                  className="px-4 py-2.5 text-sm font-semibold text-gray-700 hover:text-gray-900 hover:bg-gray-50 rounded-xl transition-all duration-200"
                >
                  Iniciar sesión
                </Link>
                <Link
                  to="/register"
                  className="px-4 py-2.5 text-sm font-semibold text-white bg-gradient-to-r from-blue-600 via-blue-500 to-blue-600 rounded-xl shadow-lg hover:scale-105 transition-all duration-200"
                >
                  Registrarse
                </Link>
              </div>
            )}

            {/* Mobile Menu Button */}
            <button
              className="lg:hidden p-2.5 rounded-xl hover:bg-gray-100 transition-all duration-200"
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
            <GlobalSearch showOnMobile placeholder="Buscar vehículos..." />
          </div>
        )}

        {/* Mobile Menu - Simplificado */}
        {isMobileMenuOpen && (
          <div className="lg:hidden py-4 border-t border-gray-200 max-h-[calc(100vh-4rem)] overflow-y-auto bg-white">
            {/* Navigation Links */}
            <div className="px-3 mb-4 space-y-2">
              {mainNavLinks.map((link) => {
                const isActive =
                  location.pathname === link.href ||
                  (link.href === '/vehicles' && location.pathname.startsWith('/vehicles'));

                return (
                  <Link
                    key={link.href}
                    to={link.href}
                    className={`flex items-center gap-3 px-4 py-3.5 rounded-xl font-semibold transition-all duration-200 ${
                      isActive
                        ? 'bg-blue-600 text-white shadow-lg'
                        : 'text-gray-700 hover:bg-gray-100'
                    }`}
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <link.icon className="w-5 h-5" />
                    <span className="text-base">{link.label}</span>
                  </Link>
                );
              })}

              {/* CTA Vender Mobile */}
              {isAuthenticated && user && (
                <Link
                  to="/sell"
                  className="flex items-center gap-3 px-4 py-3.5 rounded-xl bg-gradient-to-r from-emerald-500 via-emerald-600 to-teal-500 text-white shadow-lg font-semibold"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  <FiPlusCircle className="w-5 h-5" />
                  <span className="text-base">Vender mi Vehículo</span>
                </Link>
              )}
            </div>

            {/* User Section Mobile */}
            {isAuthenticated && user ? (
              <>
                <hr className="my-4 border-gray-200" />
                <div className="px-3 space-y-2">
                  {user.accountType === 'dealer' || user.accountType === 'dealer_employee'
                    ? dealerLinks.map((link) => (
                        <Link
                          key={link.href}
                          to={link.href}
                          className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-emerald-50 rounded-xl transition-all font-medium"
                          onClick={() => setIsMobileMenuOpen(false)}
                        >
                          <div className="w-8 h-8 bg-emerald-100 rounded-lg flex items-center justify-center">
                            <link.icon className="w-4 h-4 text-emerald-600" />
                          </div>
                          <span className="text-base">{link.label}</span>
                        </Link>
                      ))
                    : userLinks.map((link) => (
                        <Link
                          key={link.href}
                          to={link.href}
                          className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-blue-50 rounded-xl transition-all font-medium"
                          onClick={() => setIsMobileMenuOpen(false)}
                        >
                          <div className="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center">
                            <link.icon className="w-4 h-4 text-blue-600" />
                          </div>
                          <span className="text-base">{link.label}</span>
                        </Link>
                      ))}

                  <button
                    onClick={() => {
                      handleLogout();
                      setIsMobileMenuOpen(false);
                    }}
                    className="w-full flex items-center justify-center gap-3 px-4 py-3.5 text-red-600 bg-red-50 hover:bg-red-100 rounded-xl transition-all font-semibold mt-4"
                  >
                    <FiLogOut className="w-5 h-5" />
                    Cerrar sesión
                  </button>
                </div>
              </>
            ) : (
              <div className="px-4 py-2 space-y-3">
                <Link
                  to="/login"
                  className="block w-full px-4 py-3.5 text-center text-gray-700 hover:bg-gray-50 rounded-xl transition-all font-semibold border-2 border-gray-200"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  Iniciar sesión
                </Link>
                <Link
                  to="/register"
                  className="block w-full px-4 py-3.5 text-center bg-gradient-to-r from-blue-600 via-blue-500 to-blue-600 text-white rounded-xl transition-all font-semibold shadow-lg"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  Registrarse
                </Link>
              </div>
            )}
          </div>
        )}
      </div>

      {/* DEBUG COMPONENT - TEMPORAL */}
      <UserDebugInfo />
    </nav>
  );
}

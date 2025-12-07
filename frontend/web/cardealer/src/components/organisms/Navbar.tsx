/**
 * Navbar - CarGurus-style clean and professional navigation
 * Focused on car dealership functionality
 */

import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { 
  FiMenu, 
  FiX, 
  FiUser, 
  FiLogOut, 
  FiSettings, 
  FiMessageSquare, 
  FiHeart,
  FiChevronDown
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';

export default function Navbar() {
  const { isAuthenticated, user, logout } = useAuth();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const location = useLocation();

  // Close menus on route change
  useEffect(() => {
    setIsMobileMenuOpen(false);
    setIsUserMenuOpen(false);
  }, [location.pathname]);

  const handleLogout = () => {
    logout();
    setIsUserMenuOpen(false);
  };

  const navLinks = [
    { to: '/browse', label: 'Buscar Autos' },
    { to: '/sell-your-car', label: 'Vender mi Auto' },
    { to: '/compare', label: 'Comparar' },
    { to: '/about', label: 'Nosotros' },
  ];

  return (
    <nav className="bg-white border-b border-gray-200 sticky top-0 z-50">
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

          {/* Desktop Navigation */}
          <div className="hidden lg:flex items-center gap-6">
            {navLinks.map((link) => (
              <Link
                key={link.to}
                to={link.to}
                className={`text-sm font-medium transition-colors ${
                  location.pathname === link.to
                    ? 'text-blue-600'
                    : 'text-gray-700 hover:text-blue-600'
                }`}
              >
                {link.label}
              </Link>
            ))}
          </div>

          {/* Right Side Actions */}
          <div className="flex items-center gap-3">
            {isAuthenticated && user ? (
              <>
                {/* Desktop User Menu */}
                <div className="hidden md:flex items-center gap-2">
                  <Link
                    to="/wishlist"
                    className="p-2 text-gray-600 hover:text-blue-600 transition-colors"
                    title="Favoritos"
                  >
                    <FiHeart className="w-5 h-5" />
                  </Link>
                  
                  <Link
                    to="/messages"
                    className="p-2 text-gray-600 hover:text-blue-600 transition-colors"
                    title="Mensajes"
                  >
                    <FiMessageSquare className="w-5 h-5" />
                  </Link>

                  {/* User Menu Dropdown */}
                  <div className="relative">
                    <button
                      onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                      className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-gray-100 transition-colors"
                    >
                      <div className="w-8 h-8 bg-blue-600 rounded-full flex items-center justify-center">
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
                        <div className="absolute right-0 mt-2 w-56 bg-white rounded-lg shadow-lg border border-gray-200 py-2 z-20">
                          <div className="px-4 py-3 border-b border-gray-200">
                            <p className="text-sm font-medium text-gray-900">{user.name || 'Usuario'}</p>
                            <p className="text-xs text-gray-500 truncate">{user.email}</p>
                          </div>
                          <div className="py-1">
                            <Link
                              to="/dashboard"
                              className="flex items-center gap-3 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                              onClick={() => setIsUserMenuOpen(false)}
                            >
                              <FiUser className="w-4 h-4" />
                              Mi Cuenta
                            </Link>
                            <Link
                              to="/profile"
                              className="flex items-center gap-3 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                              onClick={() => setIsUserMenuOpen(false)}
                            >
                              <FiSettings className="w-4 h-4" />
                              Configuración
                            </Link>
                            
                            {/* Dealer Portal */}
                            {(user.accountType === 'dealer' || user.accountType === 'dealer_employee') && (
                              <>
                                <div className="border-t border-gray-100 my-1" />
                                <Link
                                  to="/dealer"
                                  className="flex items-center gap-3 px-4 py-2 text-sm text-blue-600 hover:bg-blue-50 font-medium"
                                  onClick={() => setIsUserMenuOpen(false)}
                                >
                                  <FaCar className="w-4 h-4" />
                                  Portal Dealer
                                </Link>
                              </>
                            )}

                            {/* Admin Portal */}
                            {(user.roles?.includes('admin') || user.accountType === 'admin') && (
                              <>
                                <div className="border-t border-gray-100 my-1" />
                                <Link
                                  to="/admin"
                                  className="flex items-center gap-3 px-4 py-2 text-sm text-purple-600 hover:bg-purple-50 font-medium"
                                  onClick={() => setIsUserMenuOpen(false)}
                                >
                                  <FiSettings className="w-4 h-4" />
                                  Administración
                                </Link>
                              </>
                            )}
                          </div>
                          <div className="border-t border-gray-200 pt-1">
                            <button
                              onClick={handleLogout}
                              className="w-full flex items-center gap-3 px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                            >
                              <FiLogOut className="w-4 h-4" />
                              Cerrar Sesión
                            </button>
                          </div>
                        </div>
                      </>
                    )}
                  </div>
                </div>
              </>
            ) : (
              <div className="hidden md:flex items-center gap-2">
                <Link
                  to="/login"
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:text-blue-600 transition-colors"
                >
                  Iniciar Sesión
                </Link>
                <Link
                  to="/register"
                  className="px-4 py-2 text-sm font-medium bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                >
                  Registrarse
                </Link>
              </div>
            )}

            {/* Mobile Menu Button */}
            <button
              className="lg:hidden p-2 rounded-lg hover:bg-gray-100 transition-colors"
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

        {/* Mobile Menu */}
        {isMobileMenuOpen && (
          <div className="lg:hidden py-4 border-t border-gray-200">
            {/* Navigation Links */}
            <div className="space-y-1 mb-4">
              {navLinks.map((link) => (
                <Link
                  key={link.to}
                  to={link.to}
                  className={`block px-4 py-3 rounded-lg font-medium transition-colors ${
                    location.pathname === link.to
                      ? 'bg-blue-50 text-blue-600'
                      : 'text-gray-700 hover:bg-gray-50'
                  }`}
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  {link.label}
                </Link>
              ))}
            </div>

            {/* User Section */}
            {isAuthenticated && user ? (
              <>
                <div className="border-t border-gray-200 pt-4 mb-4">
                  <div className="px-4 py-2 flex items-center gap-3 mb-3">
                    <div className="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center">
                      <span className="text-white font-medium">
                        {(user.name || user.email || 'U').charAt(0).toUpperCase()}
                      </span>
                    </div>
                    <div>
                      <p className="font-medium text-gray-900">{user.name || 'Usuario'}</p>
                      <p className="text-xs text-gray-500">{user.email}</p>
                    </div>
                  </div>

                  <div className="space-y-1">
                    <Link
                      to="/dashboard"
                      className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-lg"
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      <FiUser className="w-5 h-5" />
                      Mi Cuenta
                    </Link>
                    <Link
                      to="/wishlist"
                      className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-lg"
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      <FiHeart className="w-5 h-5" />
                      Favoritos
                    </Link>
                    <Link
                      to="/messages"
                      className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-lg"
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      <FiMessageSquare className="w-5 h-5" />
                      Mensajes
                    </Link>
                    <Link
                      to="/profile"
                      className="flex items-center gap-3 px-4 py-3 text-gray-700 hover:bg-gray-50 rounded-lg"
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      <FiSettings className="w-5 h-5" />
                      Configuración
                    </Link>

                    {/* Dealer Portal */}
                    {(user.accountType === 'dealer' || user.accountType === 'dealer_employee') && (
                      <Link
                        to="/dealer"
                        className="flex items-center gap-3 px-4 py-3 text-blue-600 hover:bg-blue-50 rounded-lg font-medium"
                        onClick={() => setIsMobileMenuOpen(false)}
                      >
                        <FaCar className="w-5 h-5" />
                        Portal Dealer
                      </Link>
                    )}

                    {/* Admin Portal */}
                    {(user.roles?.includes('admin') || user.accountType === 'admin') && (
                      <Link
                        to="/admin"
                        className="flex items-center gap-3 px-4 py-3 text-purple-600 hover:bg-purple-50 rounded-lg font-medium"
                        onClick={() => setIsMobileMenuOpen(false)}
                      >
                        <FiSettings className="w-5 h-5" />
                        Administración
                      </Link>
                    )}
                  </div>
                </div>

                <div className="border-t border-gray-200 pt-4">
                  <button
                    onClick={() => {
                      handleLogout();
                      setIsMobileMenuOpen(false);
                    }}
                    className="w-full flex items-center justify-center gap-2 px-4 py-3 text-red-600 hover:bg-red-50 rounded-lg font-medium"
                  >
                    <FiLogOut className="w-5 h-5" />
                    Cerrar Sesión
                  </button>
                </div>
              </>
            ) : (
              <div className="border-t border-gray-200 pt-4 space-y-2">
                <Link
                  to="/login"
                  className="block w-full px-4 py-3 text-center text-gray-700 hover:bg-gray-50 rounded-lg font-medium border border-gray-300"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  Iniciar Sesión
                </Link>
                <Link
                  to="/register"
                  className="block w-full px-4 py-3 text-center bg-blue-600 text-white hover:bg-blue-700 rounded-lg font-medium"
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  Registrarse
                </Link>
              </div>
            )}
          </div>
        )}
      </div>
    </nav>
  );
}

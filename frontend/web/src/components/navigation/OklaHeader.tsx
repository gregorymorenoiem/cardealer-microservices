import React, { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { clsx } from 'clsx';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Search,
  Menu,
  X,
  Heart,
  Bell,
  ChevronDown,
  LogIn,
  Settings,
  LogOut,
  Package,
  MessageSquare,
  Sun,
  Moon,
} from 'lucide-react';
import { OklaButton } from '../atoms/okla/OklaButton';

/**
 * OKLA Header Component
 * 
 * Header profesional y elegante con navegación,
 * búsqueda y acciones de usuario.
 */

export interface OklaHeaderProps {
  isLoggedIn?: boolean;
  user?: {
    name: string;
    email: string;
    avatar?: string;
  };
  onLogin?: () => void;
  onLogout?: () => void;
  onSearch?: (query: string) => void;
  notificationCount?: number;
  favoriteCount?: number;
  darkMode?: boolean;
  onToggleDarkMode?: () => void;
}

const navigationLinks = [
  { href: '/', label: 'Inicio' },
  { href: '/browse', label: 'Explorar' },
  { href: '/categories', label: 'Categorías', hasSubmenu: true },
  { href: '/sell', label: 'Vender' },
  { href: '/about', label: 'Nosotros' },
];

const categorySubmenu = [
  { href: '/vehicles', label: 'Vehículos', icon: '🚗' },
  { href: '/properties', label: 'Propiedades', icon: '🏠' },
  { href: '/electronics', label: 'Electrónica', icon: '📱' },
  { href: '/fashion', label: 'Moda', icon: '👔' },
];

export const OklaHeader: React.FC<OklaHeaderProps> = ({
  isLoggedIn = false,
  user,
  onLogin,
  onLogout,
  onSearch,
  notificationCount = 0,
  favoriteCount = 0,
  darkMode = false,
  onToggleDarkMode,
}) => {
  const [isScrolled, setIsScrolled] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isCategoryMenuOpen, setIsCategoryMenuOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  
  const location = useLocation();

  // Handle scroll effect
  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
    };
    
    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Close menus when route changes
  useEffect(() => {
    setIsMobileMenuOpen(false);
    setIsSearchOpen(false);
    setIsUserMenuOpen(false);
    setIsCategoryMenuOpen(false);
  }, [location]);

  // Handle search submit
  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      onSearch?.(searchQuery);
      setIsSearchOpen(false);
    }
  };

  return (
    <>
      <header
        className={clsx(
          'fixed top-0 left-0 right-0 z-sticky',
          'transition-all duration-300 ease-elegant',
          
          isScrolled ? [
            'py-2',
            'bg-white/95 dark:bg-gray-900/95',
            'backdrop-blur-md',
            'shadow-elegant',
          ] : [
            'py-4',
            'bg-transparent',
          ]
        )}
      >
        <div className="container mx-auto px-4 lg:px-8">
          <div className="flex items-center justify-between gap-4">
            {/* Logo */}
            <Link 
              to="/" 
              className="flex items-center gap-2 flex-shrink-0"
            >
              <div className="relative">
                {/* Placeholder for logo - will be replaced with actual logo */}
                <div 
                  className={clsx(
                    'w-10 h-10 rounded-lg',
                    'bg-gradient-to-br from-gold-400 to-gold-600',
                    'flex items-center justify-center',
                    'transition-transform duration-200',
                    'hover:scale-105'
                  )}
                >
                  <span className="text-lg font-bold text-gray-900">O</span>
                </div>
              </div>
              <span 
                className={clsx(
                  'text-2xl font-heading font-bold',
                  'text-gray-900 dark:text-white',
                  'tracking-tight'
                )}
              >
                OKLA
              </span>
            </Link>

            {/* Desktop Navigation */}
            <nav className="hidden lg:flex items-center gap-1">
              {navigationLinks.map((link) => (
                <div key={link.href} className="relative">
                  {link.hasSubmenu ? (
                    <button
                      onClick={() => setIsCategoryMenuOpen(!isCategoryMenuOpen)}
                      className={clsx(
                        'flex items-center gap-1',
                        'px-4 py-2 rounded-lg',
                        'text-sm font-medium',
                        'text-gray-600 dark:text-gray-300',
                        'hover:text-gray-900 dark:hover:text-white',
                        'hover:bg-gray-100/50 dark:hover:bg-gray-800/50',
                        'transition-colors duration-200'
                      )}
                    >
                      {link.label}
                      <ChevronDown 
                        className={clsx(
                          'w-4 h-4 transition-transform duration-200',
                          isCategoryMenuOpen && 'rotate-180'
                        )} 
                      />
                    </button>
                  ) : (
                    <Link
                      to={link.href}
                      className={clsx(
                        'px-4 py-2 rounded-lg',
                        'text-sm font-medium',
                        'transition-colors duration-200',
                        location.pathname === link.href
                          ? 'text-gold-600 dark:text-gold-400'
                          : 'text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100/50 dark:hover:bg-gray-800/50'
                      )}
                    >
                      {link.label}
                    </Link>
                  )}

                  {/* Category Submenu */}
                  {link.hasSubmenu && (
                    <AnimatePresence>
                      {isCategoryMenuOpen && (
                        <motion.div
                          initial={{ opacity: 0, y: 10 }}
                          animate={{ opacity: 1, y: 0 }}
                          exit={{ opacity: 0, y: 10 }}
                          transition={{ duration: 0.2 }}
                          className={clsx(
                            'absolute top-full left-0 mt-2',
                            'w-56 py-2',
                            'bg-white dark:bg-gray-900',
                            'rounded-xl shadow-elegant-xl',
                            'border border-gray-100 dark:border-gray-800'
                          )}
                        >
                          {categorySubmenu.map((item) => (
                            <Link
                              key={item.href}
                              to={item.href}
                              className={clsx(
                                'flex items-center gap-3',
                                'px-4 py-2.5',
                                'text-sm font-medium',
                                'text-gray-600 dark:text-gray-300',
                                'hover:text-gray-900 dark:hover:text-white',
                                'hover:bg-gray-50 dark:hover:bg-gray-800',
                                'transition-colors duration-150'
                              )}
                              onClick={() => setIsCategoryMenuOpen(false)}
                            >
                              <span className="text-lg">{item.icon}</span>
                              {item.label}
                            </Link>
                          ))}
                        </motion.div>
                      )}
                    </AnimatePresence>
                  )}
                </div>
              ))}
            </nav>

            {/* Search Bar - Desktop */}
            <div className="hidden md:flex flex-1 max-w-md mx-4">
              <form onSubmit={handleSearchSubmit} className="w-full relative">
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Buscar productos, marcas..."
                  className={clsx(
                    'w-full h-10',
                    'pl-10 pr-4',
                    'text-sm',
                    'bg-gray-100 dark:bg-gray-800',
                    'border-0 rounded-full',
                    'text-gray-900 dark:text-white',
                    'placeholder:text-gray-400',
                    'focus:outline-none focus:ring-2 focus:ring-gold-500/30',
                    'transition-all duration-200'
                  )}
                />
                <Search 
                  className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" 
                />
              </form>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-2">
              {/* Mobile Search Toggle */}
              <button
                onClick={() => setIsSearchOpen(!isSearchOpen)}
                className={clsx(
                  'md:hidden',
                  'p-2 rounded-full',
                  'text-gray-600 dark:text-gray-300',
                  'hover:bg-gray-100 dark:hover:bg-gray-800',
                  'transition-colors duration-200'
                )}
              >
                <Search className="w-5 h-5" />
              </button>

              {/* Dark Mode Toggle */}
              <button
                onClick={onToggleDarkMode}
                className={clsx(
                  'hidden sm:flex',
                  'p-2 rounded-full',
                  'text-gray-600 dark:text-gray-300',
                  'hover:bg-gray-100 dark:hover:bg-gray-800',
                  'transition-colors duration-200'
                )}
                aria-label="Toggle dark mode"
              >
                {darkMode ? (
                  <Sun className="w-5 h-5" />
                ) : (
                  <Moon className="w-5 h-5" />
                )}
              </button>

              {/* Favorites */}
              <Link
                to="/favorites"
                className={clsx(
                  'hidden sm:flex',
                  'relative p-2 rounded-full',
                  'text-gray-600 dark:text-gray-300',
                  'hover:bg-gray-100 dark:hover:bg-gray-800',
                  'transition-colors duration-200'
                )}
              >
                <Heart className="w-5 h-5" />
                {favoriteCount > 0 && (
                  <span 
                    className={clsx(
                      'absolute -top-0.5 -right-0.5',
                      'min-w-[18px] h-[18px]',
                      'flex items-center justify-center',
                      'bg-gold-500 text-white',
                      'text-[10px] font-bold',
                      'rounded-full'
                    )}
                  >
                    {favoriteCount > 99 ? '99+' : favoriteCount}
                  </span>
                )}
              </Link>

              {/* Notifications */}
              {isLoggedIn && (
                <button
                  className={clsx(
                    'hidden sm:flex',
                    'relative p-2 rounded-full',
                    'text-gray-600 dark:text-gray-300',
                    'hover:bg-gray-100 dark:hover:bg-gray-800',
                    'transition-colors duration-200'
                  )}
                >
                  <Bell className="w-5 h-5" />
                  {notificationCount > 0 && (
                    <span 
                      className={clsx(
                        'absolute -top-0.5 -right-0.5',
                        'min-w-[18px] h-[18px]',
                        'flex items-center justify-center',
                        'bg-red-500 text-white',
                        'text-[10px] font-bold',
                        'rounded-full'
                      )}
                    >
                      {notificationCount > 99 ? '99+' : notificationCount}
                    </span>
                  )}
                </button>
              )}

              {/* User Menu / Login */}
              {isLoggedIn && user ? (
                <div className="relative">
                  <button
                    onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                    className={clsx(
                      'flex items-center gap-2',
                      'pl-2 pr-3 py-1.5 rounded-full',
                      'bg-gray-100 dark:bg-gray-800',
                      'hover:bg-gray-200 dark:hover:bg-gray-700',
                      'transition-colors duration-200'
                    )}
                  >
                    {user.avatar ? (
                      <img 
                        src={user.avatar} 
                        alt={user.name}
                        className="w-7 h-7 rounded-full object-cover"
                      />
                    ) : (
                      <div 
                        className={clsx(
                          'w-7 h-7 rounded-full',
                          'bg-gold-500 text-white',
                          'flex items-center justify-center',
                          'text-sm font-semibold'
                        )}
                      >
                        {user.name.charAt(0).toUpperCase()}
                      </div>
                    )}
                    <span className="hidden sm:block text-sm font-medium text-gray-700 dark:text-gray-200">
                      {user.name.split(' ')[0]}
                    </span>
                    <ChevronDown 
                      className={clsx(
                        'w-4 h-4 text-gray-400',
                        'transition-transform duration-200',
                        isUserMenuOpen && 'rotate-180'
                      )} 
                    />
                  </button>

                  {/* User Dropdown */}
                  <AnimatePresence>
                    {isUserMenuOpen && (
                      <motion.div
                        initial={{ opacity: 0, y: 10, scale: 0.95 }}
                        animate={{ opacity: 1, y: 0, scale: 1 }}
                        exit={{ opacity: 0, y: 10, scale: 0.95 }}
                        transition={{ duration: 0.2 }}
                        className={clsx(
                          'absolute top-full right-0 mt-2',
                          'w-64 py-2',
                          'bg-white dark:bg-gray-900',
                          'rounded-xl shadow-elegant-xl',
                          'border border-gray-100 dark:border-gray-800'
                        )}
                      >
                        {/* User Info */}
                        <div className="px-4 py-3 border-b border-gray-100 dark:border-gray-800">
                          <p className="text-sm font-semibold text-gray-900 dark:text-white">
                            {user.name}
                          </p>
                          <p className="text-xs text-gray-500 dark:text-gray-400 truncate">
                            {user.email}
                          </p>
                        </div>

                        {/* Menu Items */}
                        <div className="py-2">
                          <Link
                            to="/dashboard"
                            className={clsx(
                              'flex items-center gap-3 px-4 py-2',
                              'text-sm text-gray-600 dark:text-gray-300',
                              'hover:bg-gray-50 dark:hover:bg-gray-800',
                              'transition-colors duration-150'
                            )}
                            onClick={() => setIsUserMenuOpen(false)}
                          >
                            <Package className="w-4 h-4" />
                            Mis Publicaciones
                          </Link>
                          <Link
                            to="/messages"
                            className={clsx(
                              'flex items-center gap-3 px-4 py-2',
                              'text-sm text-gray-600 dark:text-gray-300',
                              'hover:bg-gray-50 dark:hover:bg-gray-800',
                              'transition-colors duration-150'
                            )}
                            onClick={() => setIsUserMenuOpen(false)}
                          >
                            <MessageSquare className="w-4 h-4" />
                            Mensajes
                          </Link>
                          <Link
                            to="/settings"
                            className={clsx(
                              'flex items-center gap-3 px-4 py-2',
                              'text-sm text-gray-600 dark:text-gray-300',
                              'hover:bg-gray-50 dark:hover:bg-gray-800',
                              'transition-colors duration-150'
                            )}
                            onClick={() => setIsUserMenuOpen(false)}
                          >
                            <Settings className="w-4 h-4" />
                            Configuración
                          </Link>
                        </div>

                        {/* Logout */}
                        <div className="pt-2 border-t border-gray-100 dark:border-gray-800">
                          <button
                            onClick={() => {
                              setIsUserMenuOpen(false);
                              onLogout?.();
                            }}
                            className={clsx(
                              'flex items-center gap-3 w-full px-4 py-2',
                              'text-sm text-red-600',
                              'hover:bg-red-50 dark:hover:bg-red-900/20',
                              'transition-colors duration-150'
                            )}
                          >
                            <LogOut className="w-4 h-4" />
                            Cerrar Sesión
                          </button>
                        </div>
                      </motion.div>
                    )}
                  </AnimatePresence>
                </div>
              ) : (
                <div className="hidden sm:flex items-center gap-2">
                  <OklaButton
                    variant="ghost"
                    size="sm"
                    onClick={onLogin}
                    leftIcon={<LogIn className="w-4 h-4" />}
                  >
                    Ingresar
                  </OklaButton>
                  <OklaButton
                    variant="gold"
                    size="sm"
                  >
                    Publicar
                  </OklaButton>
                </div>
              )}

              {/* Mobile Menu Toggle */}
              <button
                onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
                className={clsx(
                  'lg:hidden',
                  'p-2 rounded-lg',
                  'text-gray-600 dark:text-gray-300',
                  'hover:bg-gray-100 dark:hover:bg-gray-800',
                  'transition-colors duration-200'
                )}
              >
                {isMobileMenuOpen ? (
                  <X className="w-6 h-6" />
                ) : (
                  <Menu className="w-6 h-6" />
                )}
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Mobile Search Overlay */}
      <AnimatePresence>
        {isSearchOpen && (
          <motion.div
            initial={{ opacity: 0, y: -20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            className={clsx(
              'fixed top-16 left-0 right-0 z-fixed',
              'bg-white dark:bg-gray-900',
              'border-b border-gray-200 dark:border-gray-800',
              'p-4 md:hidden'
            )}
          >
            <form onSubmit={handleSearchSubmit} className="relative">
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Buscar productos, marcas..."
                autoFocus
                className={clsx(
                  'w-full h-12',
                  'pl-12 pr-4',
                  'text-base',
                  'bg-gray-100 dark:bg-gray-800',
                  'border-0 rounded-xl',
                  'text-gray-900 dark:text-white',
                  'placeholder:text-gray-400',
                  'focus:outline-none focus:ring-2 focus:ring-gold-500/30'
                )}
              />
              <Search 
                className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" 
              />
            </form>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Mobile Menu */}
      <AnimatePresence>
        {isMobileMenuOpen && (
          <>
            {/* Backdrop */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 z-modal-backdrop bg-black/50 lg:hidden"
              onClick={() => setIsMobileMenuOpen(false)}
            />
            
            {/* Menu Panel */}
            <motion.div
              initial={{ x: '100%' }}
              animate={{ x: 0 }}
              exit={{ x: '100%' }}
              transition={{ type: 'tween', duration: 0.3 }}
              className={clsx(
                'fixed top-0 right-0 bottom-0 z-modal',
                'w-80 max-w-[calc(100vw-3rem)]',
                'bg-white dark:bg-gray-900',
                'shadow-2xl',
                'overflow-y-auto',
                'lg:hidden'
              )}
            >
              {/* Header */}
              <div className="flex items-center justify-between p-4 border-b border-gray-100 dark:border-gray-800">
                <span className="text-lg font-heading font-bold text-gray-900 dark:text-white">
                  Menú
                </span>
                <button
                  onClick={() => setIsMobileMenuOpen(false)}
                  className={clsx(
                    'p-2 rounded-lg',
                    'text-gray-600 dark:text-gray-300',
                    'hover:bg-gray-100 dark:hover:bg-gray-800'
                  )}
                >
                  <X className="w-5 h-5" />
                </button>
              </div>

              {/* User Info (if logged in) */}
              {isLoggedIn && user && (
                <div className="p-4 border-b border-gray-100 dark:border-gray-800">
                  <div className="flex items-center gap-3">
                    {user.avatar ? (
                      <img 
                        src={user.avatar} 
                        alt={user.name}
                        className="w-12 h-12 rounded-full object-cover"
                      />
                    ) : (
                      <div 
                        className={clsx(
                          'w-12 h-12 rounded-full',
                          'bg-gold-500 text-white',
                          'flex items-center justify-center',
                          'text-lg font-semibold'
                        )}
                      >
                        {user.name.charAt(0).toUpperCase()}
                      </div>
                    )}
                    <div>
                      <p className="font-semibold text-gray-900 dark:text-white">
                        {user.name}
                      </p>
                      <p className="text-sm text-gray-500 dark:text-gray-400 truncate">
                        {user.email}
                      </p>
                    </div>
                  </div>
                </div>
              )}

              {/* Navigation */}
              <nav className="p-4">
                {navigationLinks.map((link) => (
                  <Link
                    key={link.href}
                    to={link.href}
                    className={clsx(
                      'block px-4 py-3 rounded-lg',
                      'text-base font-medium',
                      'transition-colors duration-150',
                      location.pathname === link.href
                        ? 'bg-gold-50 text-gold-600 dark:bg-gold-900/20 dark:text-gold-400'
                        : 'text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800'
                    )}
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    {link.label}
                  </Link>
                ))}
              </nav>

              {/* Categories */}
              <div className="px-4 pb-4">
                <p className="px-4 mb-2 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                  Categorías
                </p>
                {categorySubmenu.map((item) => (
                  <Link
                    key={item.href}
                    to={item.href}
                    className={clsx(
                      'flex items-center gap-3 px-4 py-2.5 rounded-lg',
                      'text-sm text-gray-600 dark:text-gray-300',
                      'hover:bg-gray-50 dark:hover:bg-gray-800',
                      'transition-colors duration-150'
                    )}
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    <span className="text-lg">{item.icon}</span>
                    {item.label}
                  </Link>
                ))}
              </div>

              {/* Actions */}
              <div className="p-4 border-t border-gray-100 dark:border-gray-800">
                {isLoggedIn ? (
                  <button
                    onClick={() => {
                      setIsMobileMenuOpen(false);
                      onLogout?.();
                    }}
                    className={clsx(
                      'flex items-center gap-3 w-full px-4 py-3 rounded-lg',
                      'text-base font-medium text-red-600',
                      'hover:bg-red-50 dark:hover:bg-red-900/20',
                      'transition-colors duration-150'
                    )}
                  >
                    <LogOut className="w-5 h-5" />
                    Cerrar Sesión
                  </button>
                ) : (
                  <div className="space-y-2">
                    <OklaButton
                      variant="outline"
                      fullWidth
                      onClick={() => {
                        setIsMobileMenuOpen(false);
                        onLogin?.();
                      }}
                    >
                      Ingresar
                    </OklaButton>
                    <OklaButton
                      variant="gold"
                      fullWidth
                    >
                      Publicar Gratis
                    </OklaButton>
                  </div>
                )}
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>

      {/* Spacer to prevent content from going under fixed header */}
      <div className={clsx(isScrolled ? 'h-16' : 'h-20', 'transition-all duration-300')} />
    </>
  );
};

export default OklaHeader;


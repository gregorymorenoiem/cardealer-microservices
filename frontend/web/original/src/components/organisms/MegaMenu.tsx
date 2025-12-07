/**
 * MegaMenu - Multi-vertical navigation menu
 * Displays categories for Vehicles and Real Estate with subcategories
 */

import React, { useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  FiChevronDown, 
  FiTruck, 
  FiHome, 
  FiStar,
  FiArrowRight,
  FiZap,
  FiMapPin,
  FiGrid
} from 'react-icons/fi';

interface MegaMenuProps {
  isOpen: boolean;
  onClose: () => void;
  activeCategory?: 'vehicles' | 'properties' | null;
}

interface CategoryItem {
  label: string;
  href: string;
  icon?: React.ReactNode;
  description?: string;
  isNew?: boolean;
  isPopular?: boolean;
}

interface CategorySection {
  title: string;
  items: CategoryItem[];
}

// Vehicle categories
const vehicleCategories: CategorySection[] = [
  {
    title: 'Por Tipo',
    items: [
      { label: 'Sedanes', href: '/vehicles?type=sedan', icon: '🚗' },
      { label: 'SUVs', href: '/vehicles?type=suv', icon: '🚙' },
      { label: 'Pickups', href: '/vehicles?type=pickup', icon: '🛻' },
      { label: 'Deportivos', href: '/vehicles?type=sports', icon: '🏎️' },
      { label: 'Eléctricos', href: '/vehicles?type=electric', icon: '⚡', isNew: true },
      { label: 'Híbridos', href: '/vehicles?type=hybrid', icon: '🔋' },
    ],
  },
  {
    title: 'Por Precio',
    items: [
      { label: 'Hasta $200,000', href: '/vehicles?maxPrice=200000' },
      { label: '$200,000 - $500,000', href: '/vehicles?minPrice=200000&maxPrice=500000' },
      { label: '$500,000 - $1,000,000', href: '/vehicles?minPrice=500000&maxPrice=1000000' },
      { label: 'Más de $1,000,000', href: '/vehicles?minPrice=1000000' },
    ],
  },
];

// Property categories
const propertyCategories: CategorySection[] = [
  {
    title: 'Por Tipo',
    items: [
      { label: 'Casas', href: '/properties?type=house', icon: '🏠' },
      { label: 'Apartamentos', href: '/properties?type=apartment', icon: '🏢' },
      { label: 'Terrenos', href: '/properties?type=land', icon: '🌳' },
      { label: 'Locales Comerciales', href: '/properties?type=commercial', icon: '🏪' },
      { label: 'Oficinas', href: '/properties?type=office', icon: '🏛️' },
      { label: 'Bodegas', href: '/properties?type=warehouse', icon: '📦', isNew: true },
    ],
  },
  {
    title: 'Por Operación',
    items: [
      { label: 'En Venta', href: '/properties?listingType=sale', isPopular: true },
      { label: 'En Renta', href: '/properties?listingType=rent' },
      { label: 'Preventa', href: '/properties?listingType=presale', isNew: true },
    ],
  },
];

// Featured items for each vertical
const vehicleFeatured: CategoryItem[] = [
  { label: 'Más vendidos', href: '/vehicles?sort=popular', icon: <FiStar className="w-4 h-4" />, isPopular: true },
  { label: 'Recién llegados', href: '/vehicles?sort=newest', icon: <FiZap className="w-4 h-4" />, isNew: true },
  { label: 'Ofertas especiales', href: '/vehicles?hasDiscount=true', icon: '💰' },
];

const propertyFeatured: CategoryItem[] = [
  { label: 'Más buscadas', href: '/properties?sort=popular', icon: <FiStar className="w-4 h-4" />, isPopular: true },
  { label: 'Nuevas publicaciones', href: '/properties?sort=newest', icon: <FiZap className="w-4 h-4" />, isNew: true },
  { label: 'Cerca de ti', href: '/properties?nearMe=true', icon: <FiMapPin className="w-4 h-4" /> },
];

const MegaMenu: React.FC<MegaMenuProps> = ({ isOpen, onClose, activeCategory }) => {
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen, onClose]);

  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener('keydown', handleEscape);
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
    };
  }, [isOpen, onClose]);

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/20 z-40"
            onClick={onClose}
          />
          
          {/* Menu Panel */}
          <motion.div
            ref={menuRef}
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            transition={{ duration: 0.2 }}
            className="absolute left-0 right-0 top-full bg-white shadow-2xl border-t border-gray-200 z-50"
          >
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
              <div className="grid grid-cols-2 gap-12">
                {/* Vehicles Column */}
                <div className={`transition-opacity ${activeCategory === 'properties' ? 'opacity-50' : ''}`}>
                  <div className="flex items-center gap-3 mb-6">
                    <div className="w-10 h-10 bg-blue-100 rounded-xl flex items-center justify-center">
                      <FiTruck className="w-5 h-5 text-blue-600" />
                    </div>
                    <div>
                      <h3 className="text-lg font-bold text-gray-900">Vehículos</h3>
                      <p className="text-sm text-gray-500">Encuentra tu próximo auto</p>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-8">
                    {vehicleCategories.map((section) => (
                      <div key={section.title}>
                        <h4 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">
                          {section.title}
                        </h4>
                        <ul className="space-y-2">
                          {section.items.map((item) => (
                            <li key={item.label}>
                              <Link
                                to={item.href}
                                onClick={onClose}
                                className="flex items-center gap-2 text-gray-700 hover:text-blue-600 transition-colors group"
                              >
                                {item.icon && <span className="text-lg">{item.icon}</span>}
                                <span className="group-hover:translate-x-0.5 transition-transform">{item.label}</span>
                                {item.isNew && (
                                  <span className="px-1.5 py-0.5 bg-green-100 text-green-600 text-[10px] font-bold rounded">
                                    NUEVO
                                  </span>
                                )}
                                {item.isPopular && (
                                  <span className="px-1.5 py-0.5 bg-amber-100 text-amber-600 text-[10px] font-bold rounded">
                                    POPULAR
                                  </span>
                                )}
                              </Link>
                            </li>
                          ))}
                        </ul>
                      </div>
                    ))}
                  </div>

                  {/* Featured Section */}
                  <div className="mt-6 pt-6 border-t border-gray-100">
                    <h4 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3 flex items-center gap-2">
                      <FiStar className="w-3 h-3" />
                      Destacados
                    </h4>
                    <div className="flex flex-wrap gap-3">
                      {vehicleFeatured.map((item) => (
                        <Link
                          key={item.label}
                          to={item.href}
                          onClick={onClose}
                          className="inline-flex items-center gap-2 px-3 py-1.5 bg-gray-100 hover:bg-blue-100 rounded-full text-sm text-gray-700 hover:text-blue-600 transition-colors"
                        >
                          {typeof item.icon === 'string' ? <span>{item.icon}</span> : item.icon}
                          {item.label}
                        </Link>
                      ))}
                    </div>
                  </div>

                  {/* View All Link */}
                  <Link
                    to="/vehicles"
                    onClick={onClose}
                    className="inline-flex items-center gap-2 mt-4 text-blue-600 font-medium hover:gap-3 transition-all"
                  >
                    Ver todos los vehículos
                    <FiArrowRight className="w-4 h-4" />
                  </Link>
                </div>

                {/* Properties Column */}
                <div className={`transition-opacity ${activeCategory === 'vehicles' ? 'opacity-50' : ''}`}>
                  <div className="flex items-center gap-3 mb-6">
                    <div className="w-10 h-10 bg-emerald-100 rounded-xl flex items-center justify-center">
                      <FiHome className="w-5 h-5 text-emerald-600" />
                    </div>
                    <div>
                      <h3 className="text-lg font-bold text-gray-900">Inmuebles</h3>
                      <p className="text-sm text-gray-500">Encuentra tu hogar ideal</p>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-8">
                    {propertyCategories.map((section) => (
                      <div key={section.title}>
                        <h4 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">
                          {section.title}
                        </h4>
                        <ul className="space-y-2">
                          {section.items.map((item) => (
                            <li key={item.label}>
                              <Link
                                to={item.href}
                                onClick={onClose}
                                className="flex items-center gap-2 text-gray-700 hover:text-emerald-600 transition-colors group"
                              >
                                {item.icon && <span className="text-lg">{item.icon}</span>}
                                <span className="group-hover:translate-x-0.5 transition-transform">{item.label}</span>
                                {item.isNew && (
                                  <span className="px-1.5 py-0.5 bg-green-100 text-green-600 text-[10px] font-bold rounded">
                                    NUEVO
                                  </span>
                                )}
                                {item.isPopular && (
                                  <span className="px-1.5 py-0.5 bg-amber-100 text-amber-600 text-[10px] font-bold rounded">
                                    POPULAR
                                  </span>
                                )}
                              </Link>
                            </li>
                          ))}
                        </ul>
                      </div>
                    ))}
                  </div>

                  {/* Featured Section */}
                  <div className="mt-6 pt-6 border-t border-gray-100">
                    <h4 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3 flex items-center gap-2">
                      <FiStar className="w-3 h-3" />
                      Destacados
                    </h4>
                    <div className="flex flex-wrap gap-3">
                      {propertyFeatured.map((item) => (
                        <Link
                          key={item.label}
                          to={item.href}
                          onClick={onClose}
                          className="inline-flex items-center gap-2 px-3 py-1.5 bg-gray-100 hover:bg-emerald-100 rounded-full text-sm text-gray-700 hover:text-emerald-600 transition-colors"
                        >
                          {typeof item.icon === 'string' ? <span>{item.icon}</span> : item.icon}
                          {item.label}
                        </Link>
                      ))}
                    </div>
                  </div>

                  {/* View All Link */}
                  <Link
                    to="/properties"
                    onClick={onClose}
                    className="inline-flex items-center gap-2 mt-4 text-emerald-600 font-medium hover:gap-3 transition-all"
                  >
                    Ver todos los inmuebles
                    <FiArrowRight className="w-4 h-4" />
                  </Link>
                </div>
              </div>
            </div>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
};

// Navigation button that triggers the mega menu
export const MegaMenuTrigger: React.FC<{
  isOpen: boolean;
  onClick: () => void;
  activeCategory?: 'vehicles' | 'properties' | null;
}> = ({ isOpen, onClick, activeCategory }) => {
  const getActiveLabel = () => {
    if (activeCategory === 'vehicles') return 'Vehículos';
    if (activeCategory === 'properties') return 'Inmuebles';
    return 'Explorar';
  };

  const getActiveIcon = () => {
    if (activeCategory === 'vehicles') return <FiTruck className="w-4 h-4" />;
    if (activeCategory === 'properties') return <FiHome className="w-4 h-4" />;
    return <FiGrid className="w-4 h-4" />;
  };

  const getActiveColor = () => {
    if (activeCategory === 'vehicles') return 'text-blue-600';
    if (activeCategory === 'properties') return 'text-emerald-600';
    return 'text-gray-700';
  };

  return (
    <button
      onClick={onClick}
      className={`flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-gray-100 transition-colors font-medium ${getActiveColor()}`}
    >
      {getActiveIcon()}
      <span>{getActiveLabel()}</span>
      <FiChevronDown 
        className={`w-4 h-4 transition-transform ${isOpen ? 'rotate-180' : ''}`} 
      />
    </button>
  );
};

export default MegaMenu;

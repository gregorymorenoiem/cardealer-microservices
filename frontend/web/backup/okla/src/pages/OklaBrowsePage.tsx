import { useState, useMemo } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Filter, X } from 'lucide-react';
import { OklaLayout } from '../layouts/OklaLayout';
import { 
  OklaSearchBar,
  OklaFilterSidebar,
  OklaResultsHeader,
  OklaPagination,
  OklaActiveFilters,
  OklaNoResults
} from '../components/okla/search';
import type { FilterSection } from '../components/okla/search';
import { OklaListingCard } from '../components/okla/cards/OklaListingCard';
import { FadeIn } from '../components/okla/animations/FadeIn';
import { StaggerContainer, StaggerItem } from '../components/okla/animations/StaggerContainer';

// Mock data for listings
const mockListings = Array.from({ length: 24 }, (_, i) => ({
  id: `listing-${i + 1}`,
  title: [
    'Mercedes-Benz Clase C 300',
    'BMW Serie 5 530i',
    'Audi A6 Premium Plus',
    'Porsche Cayenne S',
    'Range Rover Sport HSE',
    'Tesla Model S Plaid',
    'Lexus ES 350 F Sport',
    'Volvo XC90 T6 Inscription',
  ][i % 8],
  price: 45000 + Math.floor(Math.random() * 80000),
  location: ['San Juan', 'Bayamón', 'Caguas', 'Ponce', 'Mayagüez'][i % 5],
  year: 2020 + (i % 4),
  mileage: 10000 + Math.floor(Math.random() * 50000),
  image: `https://images.unsplash.com/photo-${[
    '1549399542-7e8ee8c6e7a0',
    '1555215695-3004980ad54e',
    '1494976388531-d1058494cdd8',
    '1503376780353-7e6692767b70',
    '1519641471654-76ce0107ad1b',
    '1552519507-da3b142c6e3d',
    '1580414057403-c5f451f30e1c',
    '1606664515524-ed2f786a0bd6',
  ][i % 8]}?w=400&h=300&fit=crop`,
  isNew: i < 6,
  isFeatured: i % 4 === 0,
  isVerified: i % 3 === 0,
  views: 100 + Math.floor(Math.random() * 500),
  daysOnMarket: Math.floor(Math.random() * 30),
}));

// Filter sections configuration
const filterSections: FilterSection[] = [
  {
    id: 'priceRange',
    label: 'Rango de Precio',
    type: 'range',
    min: 0,
    max: 200000,
    step: 5000,
    prefix: '$',
  },
  {
    id: 'year',
    label: 'Año',
    type: 'checkbox',
    options: [
      { id: '2024', label: '2024', count: 45 },
      { id: '2023', label: '2023', count: 89 },
      { id: '2022', label: '2022', count: 124 },
      { id: '2021', label: '2021', count: 98 },
      { id: '2020', label: '2020', count: 67 },
    ],
  },
  {
    id: 'make',
    label: 'Marca',
    type: 'checkbox',
    options: [
      { id: 'mercedes', label: 'Mercedes-Benz', count: 156 },
      { id: 'bmw', label: 'BMW', count: 142 },
      { id: 'audi', label: 'Audi', count: 98 },
      { id: 'porsche', label: 'Porsche', count: 45 },
      { id: 'tesla', label: 'Tesla', count: 67 },
      { id: 'lexus', label: 'Lexus', count: 89 },
      { id: 'volvo', label: 'Volvo', count: 34 },
      { id: 'range-rover', label: 'Range Rover', count: 28 },
    ],
    defaultOpen: true,
  },
  {
    id: 'bodyType',
    label: 'Tipo de Carrocería',
    type: 'checkbox',
    options: [
      { id: 'sedan', label: 'Sedán', count: 234 },
      { id: 'suv', label: 'SUV', count: 189 },
      { id: 'coupe', label: 'Coupé', count: 67 },
      { id: 'convertible', label: 'Convertible', count: 34 },
      { id: 'hatchback', label: 'Hatchback', count: 56 },
      { id: 'wagon', label: 'Wagon', count: 23 },
    ],
  },
  {
    id: 'transmission',
    label: 'Transmisión',
    type: 'checkbox',
    options: [
      { id: 'automatic', label: 'Automática', count: 456 },
      { id: 'manual', label: 'Manual', count: 87 },
    ],
  },
  {
    id: 'fuelType',
    label: 'Tipo de Combustible',
    type: 'checkbox',
    options: [
      { id: 'gasoline', label: 'Gasolina', count: 345 },
      { id: 'diesel', label: 'Diésel', count: 89 },
      { id: 'electric', label: 'Eléctrico', count: 67 },
      { id: 'hybrid', label: 'Híbrido', count: 112 },
    ],
  },
  {
    id: 'condition',
    label: 'Condición',
    type: 'checkbox',
    options: [
      { id: 'new', label: 'Nuevo', count: 156 },
      { id: 'certified', label: 'Certificado', count: 234 },
      { id: 'used', label: 'Usado', count: 345 },
    ],
  },
  {
    id: 'features',
    label: 'Características',
    type: 'checkbox',
    options: [
      { id: 'sunroof', label: 'Techo Solar', count: 234 },
      { id: 'leather', label: 'Asientos de Cuero', count: 345 },
      { id: 'navigation', label: 'Navegación', count: 289 },
      { id: 'bluetooth', label: 'Bluetooth', count: 456 },
      { id: 'backup-camera', label: 'Cámara de Reversa', count: 389 },
      { id: 'heated-seats', label: 'Asientos Calefaccionados', count: 234 },
      { id: 'apple-carplay', label: 'Apple CarPlay', count: 312 },
      { id: 'parking-sensors', label: 'Sensores de Estacionamiento', count: 267 },
    ],
  },
  {
    id: 'sellerType',
    label: 'Tipo de Vendedor',
    type: 'checkbox',
    options: [
      { id: 'dealer', label: 'Dealer', count: 456 },
      { id: 'private', label: 'Particular', count: 234 },
      { id: 'certified-dealer', label: 'Dealer Certificado', count: 123 },
    ],
  },
];

const sortOptions = [
  { id: 'relevance', label: 'Más relevante' },
  { id: 'price-low', label: 'Precio: menor a mayor' },
  { id: 'price-high', label: 'Precio: mayor a menor' },
  { id: 'newest', label: 'Más reciente' },
  { id: 'oldest', label: 'Más antiguo' },
];

export const OklaBrowsePage = () => {
  // State
  const [searchQuery, setSearchQuery] = useState('');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [sortBy, setSortBy] = useState<string>('relevance');
  const [currentPage, setCurrentPage] = useState(1);
  const [activeFilters, setActiveFilters] = useState<Record<string, string | string[] | [number, number]>>({});
  const [priceRange, setPriceRange] = useState<{ min: number; max: number } | null>(null);
  const [isMobileFiltersOpen, setIsMobileFiltersOpen] = useState(false);

  const itemsPerPage = 12;

  // Calculate active filter tags
  const activeFilterTags = useMemo(() => {
    const tags: { id: string; sectionId: string; label: string; value: string }[] = [];

    Object.entries(activeFilters).forEach(([sectionId, values]) => {
      const section = filterSections.find((s) => s.id === sectionId);
      if (section && section.options) {
        const valuesArray = Array.isArray(values) ? values : [values];
        valuesArray.forEach((value) => {
          if (typeof value === 'string') {
            const option = section.options?.find((o) => o.id === value);
            if (option) {
              tags.push({
                id: `${sectionId}-${value}`,
                sectionId,
                label: section.label,
                value: option.label,
              });
            }
          }
        });
      }
    });

    if (priceRange) {
      tags.push({
        id: 'price-range',
        sectionId: 'priceRange',
        label: 'Precio',
        value: `$${priceRange.min.toLocaleString()} - $${priceRange.max.toLocaleString()}`,
      });
    }

    return tags;
  }, [activeFilters, priceRange]);

  // Filter handlers
  const handleFilterChange = (sectionId: string, value: string | string[] | [number, number]) => {
    if (sectionId === 'priceRange' && Array.isArray(value) && value.length === 2 && typeof value[0] === 'number') {
      setPriceRange({ min: value[0] as number, max: value[1] as number });
    } else {
      setActiveFilters((prev) => ({
        ...prev,
        [sectionId]: value,
      }));
    }
    setCurrentPage(1);
  };

  const handleRemoveFilter = (sectionId: string, value: string) => {
    if (sectionId === 'priceRange') {
      setPriceRange(null);
    } else {
      setActiveFilters((prev) => {
        const section = filterSections.find((s) => s.id === sectionId);
        const option = section?.options?.find((o) => o.label === value);
        const currentValue = prev[sectionId];
        if (Array.isArray(currentValue)) {
          return {
            ...prev,
            [sectionId]: (currentValue as string[]).filter((v) => v !== option?.id),
          };
        }
        return prev;
      });
    }
  };

  const handleClearAllFilters = () => {
    setActiveFilters({});
    setPriceRange(null);
    setSearchQuery('');
    setCurrentPage(1);
  };

  // Filter and sort listings
  const filteredListings = useMemo(() => {
    let results = [...mockListings];

    // Apply search
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      results = results.filter(
        (listing) =>
          listing.title.toLowerCase().includes(query) ||
          listing.location.toLowerCase().includes(query)
      );
    }

    // Apply price range
    if (priceRange) {
      results = results.filter(
        (listing) => listing.price >= priceRange.min && listing.price <= priceRange.max
      );
    }

    // Apply sorting
    switch (sortBy) {
      case 'price-low':
        results.sort((a, b) => a.price - b.price);
        break;
      case 'price-high':
        results.sort((a, b) => b.price - a.price);
        break;
      case 'newest':
        results.sort((a, b) => b.year - a.year);
        break;
      case 'oldest':
        results.sort((a, b) => a.year - b.year);
        break;
      default:
        // relevance - featured first
        results.sort((a, b) => (b.isFeatured ? 1 : 0) - (a.isFeatured ? 1 : 0));
    }

    return results;
  }, [searchQuery, priceRange, sortBy]);

  // Pagination
  const totalPages = Math.ceil(filteredListings.length / itemsPerPage);
  const paginatedListings = filteredListings.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );

  const hasActiveFilters = activeFilterTags.length > 0 || searchQuery;

  return (
    <OklaLayout>
      {/* Hero Search Section */}
      <section className="bg-gradient-to-b from-okla-navy to-okla-navy/95 py-12">
        <div className="container mx-auto px-4">
          <FadeIn>
            <h1 className="text-3xl md:text-4xl font-display font-bold text-white text-center mb-2">
              Explora Nuestro Catálogo
            </h1>
            <p className="text-okla-cream/80 text-center mb-8 max-w-2xl mx-auto">
              Encuentra el vehículo perfecto entre nuestra selección de autos premium
            </p>
          </FadeIn>

          <FadeIn delay={0.2}>
            <OklaSearchBar
              value={searchQuery}
              onChange={setSearchQuery}
              onSearch={() => console.log('Search:', searchQuery)}
            />
          </FadeIn>
        </div>
      </section>

      {/* Main Content */}
      <section className="py-8 bg-okla-cream/30">
        <div className="container mx-auto px-4">
          <div className="flex gap-8">
            {/* Desktop Sidebar */}
            <aside className="hidden lg:block w-80 flex-shrink-0">
              <div className="sticky top-24">
                <OklaFilterSidebar
                  sections={filterSections}
                  activeFilters={activeFilters}
                  onFilterChange={handleFilterChange}
                  onClearFilters={handleClearAllFilters}
                />
              </div>
            </aside>

            {/* Mobile Filter Overlay */}
            <AnimatePresence>
              {isMobileFiltersOpen && (
                <>
                  {/* Backdrop */}
                  <motion.div
                    className="fixed inset-0 bg-black/50 z-40 lg:hidden"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    onClick={() => setIsMobileFiltersOpen(false)}
                  />

                  {/* Filter Panel */}
                  <motion.div
                    className="fixed inset-y-0 left-0 w-full max-w-sm bg-white z-50 lg:hidden overflow-y-auto"
                    initial={{ x: '-100%' }}
                    animate={{ x: 0 }}
                    exit={{ x: '-100%' }}
                    transition={{ type: 'spring', damping: 25, stiffness: 300 }}
                  >
                    <div className="sticky top-0 bg-white border-b px-4 py-4 flex items-center justify-between z-10">
                      <h2 className="text-lg font-display font-bold text-okla-navy flex items-center gap-2">
                        <Filter className="w-5 h-5" />
                        Filtros
                      </h2>
                      <button
                        onClick={() => setIsMobileFiltersOpen(false)}
                        className="p-2 hover:bg-okla-cream rounded-lg transition-colors"
                      >
                        <X className="w-5 h-5" />
                      </button>
                    </div>
                    <div className="p-4">
                      <OklaFilterSidebar
                        sections={filterSections}
                        activeFilters={activeFilters}
                        onFilterChange={handleFilterChange}
                        onClearFilters={handleClearAllFilters}
                      />
                    </div>
                  </motion.div>
                </>
              )}
            </AnimatePresence>

            {/* Results */}
            <div className="flex-1 min-w-0">
              {/* Active Filters */}
              <OklaActiveFilters
                filters={activeFilterTags}
                onRemove={handleRemoveFilter}
                onClearAll={handleClearAllFilters}
              />

              {/* Results Header */}
              <OklaResultsHeader
                totalResults={filteredListings.length}
                currentPage={currentPage}
                itemsPerPage={itemsPerPage}
                viewMode={viewMode}
                sortBy={sortBy}
                sortOptions={sortOptions}
                onViewModeChange={setViewMode}
                onSortChange={(sortId) => setSortBy(sortId)}
              />

              {/* Results Grid/List */}
              {paginatedListings.length > 0 ? (
                <>
                  <StaggerContainer
                    className={
                      viewMode === 'grid'
                        ? 'grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6'
                        : 'flex flex-col gap-4'
                    }
                  >
                    {paginatedListings.map((listing) => (
                      <StaggerItem key={listing.id}>
                        <OklaListingCard
                          id={listing.id}
                          title={listing.title}
                          price={listing.price}
                          location={listing.location}
                          category="Vehículos"
                          image={listing.image}
                          isNew={listing.isNew}
                          featured={listing.isFeatured}
                          verified={listing.isVerified}
                          views={listing.views}
                          specs={[
                            { label: 'Año', value: String(listing.year) },
                            { label: 'Millaje', value: `${listing.mileage.toLocaleString()} mi` },
                          ]}
                          variant={viewMode === 'list' ? 'horizontal' : 'default'}
                          onFavorite={() => console.log('Favorite:', listing.id)}
                        />
                      </StaggerItem>
                    ))}
                  </StaggerContainer>

                  {/* Pagination */}
                  <div className="mt-10">
                    <OklaPagination
                      currentPage={currentPage}
                      totalPages={totalPages}
                      onPageChange={setCurrentPage}
                    />
                  </div>
                </>
              ) : (
                <OklaNoResults
                  searchQuery={searchQuery}
                  onClearFilters={hasActiveFilters ? handleClearAllFilters : undefined}
                  onBrowseAll={() => {
                    handleClearAllFilters();
                  }}
                />
              )}
            </div>
          </div>
        </div>
      </section>
    </OklaLayout>
  );
};

export default OklaBrowsePage;

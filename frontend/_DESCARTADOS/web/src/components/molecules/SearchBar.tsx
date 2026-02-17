import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Button from '@/components/atoms/Button';
import { FiSearch } from 'react-icons/fi';

interface SearchBarProps {
  onSearch?: (filters: SearchFilters) => void;
  className?: string;
}

export interface SearchFilters {
  make?: string;
  model?: string;
  yearMin?: number;
  yearMax?: number;
  priceMin?: number;
  priceMax?: number;
}

export default function SearchBar({ onSearch, className = '' }: SearchBarProps) {
  const { t } = useTranslation(['vehicles', 'common']);
  const [filters, setFilters] = useState<SearchFilters>({
    make: '',
    model: '',
    yearMin: undefined,
    yearMax: undefined,
    priceMin: undefined,
    priceMax: undefined,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (onSearch) {
      // Filter out empty values
      const cleanFilters = Object.fromEntries(
        Object.entries(filters).filter(([, value]) => value !== '' && value !== undefined)
      );
      onSearch(cleanFilters);
    }
  };

  const currentYear = new Date().getFullYear();

  return (
    <form 
      onSubmit={handleSubmit}
      className={`bg-white rounded-xl shadow-lg p-6 ${className}`}
    >
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-4">
        {/* Make */}
        <div>
          <label htmlFor="make" className="block text-sm font-medium text-gray-700 mb-2">
            {t('vehicles:filters.brand')}
          </label>
          <select
            id="make"
            value={filters.make}
            onChange={(e) => setFilters({ ...filters, make: e.target.value })}
            className="w-full rounded-lg border border-gray-300 px-4 py-2.5 text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          >
            <option value="">{t('common:filters.all')}</option>
            <option value="Toyota">Toyota</option>
            <option value="Honda">Honda</option>
            <option value="Ford">Ford</option>
            <option value="Chevrolet">Chevrolet</option>
            <option value="BMW">BMW</option>
            <option value="Mercedes-Benz">Mercedes-Benz</option>
            <option value="Audi">Audi</option>
            <option value="Tesla">Tesla</option>
            <option value="Nissan">Nissan</option>
            <option value="Hyundai">Hyundai</option>
          </select>
        </div>

        {/* Model */}
        <div>
          <label htmlFor="model" className="block text-sm font-medium text-gray-700 mb-2">
            {t('vehicles:filters.model')}
          </label>
          <input
            type="text"
            id="model"
            placeholder={t('common:filters.any')}
            value={filters.model}
            onChange={(e) => setFilters({ ...filters, model: e.target.value })}
            className="w-full rounded-lg border border-gray-300 px-4 py-2.5 text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          />
        </div>

        {/* Year Range */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            {t('vehicles:filters.year')}
          </label>
          <div className="flex gap-2">
            <select
              value={filters.yearMin || ''}
              onChange={(e) => setFilters({ ...filters, yearMin: e.target.value ? parseInt(e.target.value) : undefined })}
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">{t('common:filters.min')}</option>
              {Array.from({ length: 30 }, (_, i) => currentYear - i).map(year => (
                <option key={year} value={year}>{year}</option>
              ))}
            </select>
            <select
              value={filters.yearMax || ''}
              onChange={(e) => setFilters({ ...filters, yearMax: e.target.value ? parseInt(e.target.value) : undefined })}
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">{t('common:filters.max')}</option>
              {Array.from({ length: 30 }, (_, i) => currentYear - i).map(year => (
                <option key={year} value={year}>{year}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Price Range */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            {t('vehicles:filters.price')}
          </label>
          <div className="flex gap-2">
            <select
              value={filters.priceMin || ''}
              onChange={(e) => setFilters({ ...filters, priceMin: e.target.value ? parseInt(e.target.value) : undefined })}
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">{t('common:filters.min')}</option>
              <option value="5000">$5,000</option>
              <option value="10000">$10,000</option>
              <option value="15000">$15,000</option>
              <option value="20000">$20,000</option>
              <option value="25000">$25,000</option>
              <option value="30000">$30,000</option>
              <option value="40000">$40,000</option>
              <option value="50000">$50,000</option>
            </select>
            <select
              value={filters.priceMax || ''}
              onChange={(e) => setFilters({ ...filters, priceMax: e.target.value ? parseInt(e.target.value) : undefined })}
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">{t('common:filters.max')}</option>
              <option value="10000">$10,000</option>
              <option value="20000">$20,000</option>
              <option value="30000">$30,000</option>
              <option value="40000">$40,000</option>
              <option value="50000">$50,000</option>
              <option value="75000">$75,000</option>
              <option value="100000">$100,000</option>
              <option value="150000">$150,000+</option>
            </select>
          </div>
        </div>
      </div>

      {/* Search Button */}
      <Button
        type="submit"
        variant="primary"
        size="lg"
        fullWidth
        leftIcon={<FiSearch size={20} />}
      >
        {t('common:buttons.search')}
      </Button>
    </form>
  );
}

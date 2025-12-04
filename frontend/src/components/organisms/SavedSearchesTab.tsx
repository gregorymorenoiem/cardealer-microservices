import { useState } from 'react';
import { Link } from 'react-router-dom';
import { FiSearch, FiTrash2, FiPlay, FiBell } from 'react-icons/fi';

interface SavedSearch {
  id: string;
  name: string;
  filters: {
    make?: string;
    model?: string;
    minPrice?: number;
    maxPrice?: number;
    minYear?: number;
    maxYear?: number;
  };
  resultsCount: number;
  createdAt: string;
  notificationsEnabled: boolean;
}

// Mock data - replace with real data from API/localStorage
const mockSearches: SavedSearch[] = [
  {
    id: '1',
    name: 'Tesla under $50k',
    filters: {
      make: 'Tesla',
      maxPrice: 50000,
    },
    resultsCount: 8,
    createdAt: '2024-11-20',
    notificationsEnabled: true,
  },
  {
    id: '2',
    name: 'BMW 2020-2023',
    filters: {
      make: 'BMW',
      minYear: 2020,
      maxYear: 2023,
    },
    resultsCount: 15,
    createdAt: '2024-11-18',
    notificationsEnabled: false,
  },
];

export default function SavedSearchesTab() {
  const [searches, setSearches] = useState<SavedSearch[]>(mockSearches);

  const handleDelete = (id: string) => {
    if (window.confirm('Delete this saved search?')) {
      setSearches((prev) => prev.filter((s) => s.id !== id));
    }
  };

  const toggleNotifications = (id: string) => {
    setSearches((prev) =>
      prev.map((s) =>
        s.id === id ? { ...s, notificationsEnabled: !s.notificationsEnabled } : s
      )
    );
  };

  const buildSearchUrl = (filters: SavedSearch['filters']) => {
    const params = new URLSearchParams();
    if (filters.make) params.set('make', filters.make);
    if (filters.model) params.set('model', filters.model);
    if (filters.minPrice) params.set('minPrice', filters.minPrice.toString());
    if (filters.maxPrice) params.set('maxPrice', filters.maxPrice.toString());
    if (filters.minYear) params.set('minYear', filters.minYear.toString());
    if (filters.maxYear) params.set('maxYear', filters.maxYear.toString());
    return `/browse?${params.toString()}`;
  };

  const formatFilters = (filters: SavedSearch['filters']) => {
    const parts: string[] = [];
    if (filters.make) parts.push(filters.make);
    if (filters.model) parts.push(filters.model);
    if (filters.minYear && filters.maxYear) {
      parts.push(`${filters.minYear}-${filters.maxYear}`);
    } else if (filters.minYear) {
      parts.push(`From ${filters.minYear}`);
    } else if (filters.maxYear) {
      parts.push(`Up to ${filters.maxYear}`);
    }
    if (filters.minPrice && filters.maxPrice) {
      parts.push(`$${filters.minPrice.toLocaleString()}-$${filters.maxPrice.toLocaleString()}`);
    } else if (filters.maxPrice) {
      parts.push(`Under $${filters.maxPrice.toLocaleString()}`);
    } else if (filters.minPrice) {
      parts.push(`Over $${filters.minPrice.toLocaleString()}`);
    }
    return parts.join(' • ');
  };

  if (searches.length === 0) {
    return (
      <div className="bg-white rounded-xl shadow-card p-12 text-center">
        <div className="flex justify-center mb-4">
          <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
            <FiSearch size={32} className="text-gray-400" />
          </div>
        </div>
        <h3 className="text-xl font-bold font-heading text-gray-900 mb-2">
          No saved searches
        </h3>
        <p className="text-gray-600 mb-6 max-w-md mx-auto">
          Save your searches to quickly find vehicles that match your criteria.
        </p>
        <Link
          to="/browse"
          className="inline-block px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors duration-200 font-medium"
        >
          Browse Vehicles
        </Link>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-card p-6">
      <div className="mb-6">
        <h2 className="text-2xl font-bold font-heading text-gray-900">
          Saved Searches
        </h2>
        <p className="text-gray-600 mt-1">
          {searches.length} {searches.length === 1 ? 'search' : 'searches'} saved
        </p>
      </div>

      <div className="space-y-4">
        {searches.map((search) => (
          <div
            key={search.id}
            className="border border-gray-200 rounded-lg p-4 hover:border-primary transition-colors duration-200"
          >
            <div className="flex items-start justify-between gap-4 mb-3">
              <div className="flex-1">
                <h3 className="text-lg font-semibold text-gray-900 mb-1">
                  {search.name}
                </h3>
                <p className="text-sm text-gray-600">
                  {formatFilters(search.filters)}
                </p>
              </div>
              <button
                onClick={() => handleDelete(search.id)}
                className="text-gray-400 hover:text-red-600 transition-colors duration-200"
                title="Delete search"
              >
                <FiTrash2 size={18} />
              </button>
            </div>

            <div className="flex items-center justify-between">
              <div className="flex items-center gap-4 text-sm text-gray-600">
                <span>{search.resultsCount} results</span>
                <span>•</span>
                <span>Saved {new Date(search.createdAt).toLocaleDateString()}</span>
              </div>

              <div className="flex items-center gap-2">
                <button
                  onClick={() => toggleNotifications(search.id)}
                  className={`
                    px-3 py-1.5 text-sm rounded-lg font-medium inline-flex items-center gap-1 transition-colors duration-200
                    ${
                      search.notificationsEnabled
                        ? 'bg-primary text-white hover:bg-primary-600'
                        : 'border border-gray-300 text-gray-700 hover:bg-gray-50'
                    }
                  `}
                  title={search.notificationsEnabled ? 'Notifications on' : 'Notifications off'}
                >
                  <FiBell size={14} />
                  {search.notificationsEnabled ? 'On' : 'Off'}
                </button>
                <Link
                  to={buildSearchUrl(search.filters)}
                  className="px-3 py-1.5 text-sm bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors duration-200 font-medium inline-flex items-center gap-1"
                >
                  <FiPlay size={14} />
                  Run Search
                </Link>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

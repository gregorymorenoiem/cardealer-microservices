import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { FiSearch, FiTrash2, FiPlay, FiBell } from 'react-icons/fi';
import {
  getSavedSearches,
  deleteSavedSearch,
  toggleNotifications,
  buildSearchUrl,
  formatFilters,
  type SavedSearch,
} from '@/services/savedSearchService';

export default function SavedSearchesTab() {
  const [searches, setSearches] = useState<SavedSearch[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Load saved searches on mount
  useEffect(() => {
    const loadSearches = async () => {
      try {
        setIsLoading(true);
        const data = await getSavedSearches();
        setSearches(data);
      } catch (err) {
        setError('Failed to load saved searches');
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };
    loadSearches();
  }, []);

  const handleDelete = async (id: string) => {
    if (!window.confirm('Delete this saved search?')) return;

    try {
      await deleteSavedSearch(id);
      setSearches((prev) => prev.filter((s) => s.id !== id));
    } catch (err) {
      alert('Failed to delete saved search');
      console.error(err);
    }
  };

  const handleToggleNotifications = async (id: string, currentState: boolean) => {
    try {
      await toggleNotifications(id, !currentState);
      setSearches((prev) =>
        prev.map((s) =>
          s.id === id ? { ...s, notificationsEnabled: !currentState } : s
        )
      );
    } catch (err) {
      alert('Failed to toggle notifications');
      console.error(err);
    }
  };

  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-card p-12 text-center">
        <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full mx-auto"></div>
        <p className="text-gray-600 mt-4">Loading saved searches...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-white rounded-xl shadow-card p-12 text-center">
        <p className="text-red-600">{error}</p>
      </div>
    );
  }

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
                  onClick={() => handleToggleNotifications(search.id, search.notificationsEnabled)}
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

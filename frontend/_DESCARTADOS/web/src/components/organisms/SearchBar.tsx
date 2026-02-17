import { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSearchBar } from '@/hooks/useSearch';
import { FiSearch, FiX, FiClock, FiTrendingUp, FiTrash2 } from 'react-icons/fi';

interface SearchBarProps {
  placeholder?: string;
  className?: string;
  onSearch?: (query: string) => void;
  showDropdown?: boolean;
  autoFocus?: boolean;
}

export default function SearchBar({
  placeholder = 'Buscar vehículos...',
  className = '',
  onSearch,
  showDropdown = true,
  autoFocus = false,
}: SearchBarProps) {
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [isOpen, setIsOpen] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const {
    suggestions,
    suggestionsLoading,
    popularSearches,
    recentSearches,
    trackSearch,
    clearRecentSearches,
  } = useSearchBar(query);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node) &&
        inputRef.current &&
        !inputRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSearch = (searchQuery: string) => {
    if (!searchQuery.trim()) return;

    // Track search in recent searches
    trackSearch(searchQuery.trim());

    // Callback or navigate
    if (onSearch) {
      onSearch(searchQuery.trim());
    } else {
      navigate(`/vehicles?search=${encodeURIComponent(searchQuery.trim())}`);
    }

    setIsOpen(false);
    setQuery('');
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    handleSearch(query);
  };

  const handleClearRecent = () => {
    clearRecentSearches();
  };

  const showSuggestions = showDropdown && isOpen && query.length >= 2 && suggestions.length > 0;
  const showRecent = showDropdown && isOpen && query.length < 2 && recentSearches.length > 0;
  const showPopular = showDropdown && isOpen && query.length < 2 && popularSearches.length > 0;

  return (
    <div className={`relative ${className}`}>
      <form onSubmit={handleSubmit} className="relative">
        <div className="relative">
          <FiSearch className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 h-5 w-5" />
          <input
            ref={inputRef}
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onFocus={() => setIsOpen(true)}
            placeholder={placeholder}
            autoFocus={autoFocus}
            className="w-full pl-12 pr-12 py-3 bg-white border border-gray-200 rounded-xl 
                     focus:ring-2 focus:ring-primary/20 focus:border-primary 
                     transition-all duration-200 text-gray-900 placeholder-gray-500"
          />
          {query && (
            <button
              type="button"
              onClick={() => setQuery('')}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
            >
              <FiX className="h-5 w-5" />
            </button>
          )}
        </div>
      </form>

      {/* Dropdown */}
      {showDropdown && isOpen && (showSuggestions || showRecent || showPopular) && (
        <div
          ref={dropdownRef}
          className="absolute z-50 w-full mt-2 bg-white rounded-xl shadow-lg border border-gray-100 overflow-hidden"
        >
          {/* Autocomplete Suggestions */}
          {showSuggestions && (
            <div className="py-2">
              <div className="px-4 py-2 text-xs font-medium text-gray-500 uppercase tracking-wider">
                Sugerencias
              </div>
              {suggestionsLoading ? (
                <div className="px-4 py-3 text-gray-500 text-sm">Buscando...</div>
              ) : (
                suggestions.map((suggestion, index) => (
                  <button
                    key={index}
                    onClick={() => handleSearch(suggestion)}
                    className="w-full px-4 py-3 text-left hover:bg-gray-50 transition-colors flex items-center gap-3"
                  >
                    <FiSearch className="h-4 w-4 text-gray-400" />
                    <span className="text-gray-900">{suggestion}</span>
                  </button>
                ))
              )}
            </div>
          )}

          {/* Recent Searches */}
          {showRecent && (
            <div className="py-2 border-b border-gray-100">
              <div className="px-4 py-2 flex items-center justify-between">
                <span className="text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Búsquedas recientes
                </span>
                <button
                  onClick={handleClearRecent}
                  className="text-xs text-gray-400 hover:text-red-500 transition-colors flex items-center gap-1"
                >
                  <FiTrash2 className="h-3 w-3" />
                  Limpiar
                </button>
              </div>
              {recentSearches.slice(0, 5).map((recent, index) => (
                  <button
                    key={index}
                    onClick={() => handleSearch(recent.query)}
                    className="w-full px-4 py-3 text-left hover:bg-gray-50 transition-colors flex items-center gap-3"
                  >
                    <FiClock className="h-4 w-4 text-gray-400" />
                    <span className="text-gray-900">{recent.query}</span>
                  </button>
                ))}
            </div>
          )}

          {/* Popular Searches */}
          {showPopular && (
            <div className="py-2">
              <div className="px-4 py-2 text-xs font-medium text-gray-500 uppercase tracking-wider">
                Búsquedas populares
              </div>
              {popularSearches.slice(0, 5).map((popular, index) => (
                  <button
                    key={index}
                    onClick={() => handleSearch(popular)}
                    className="w-full px-4 py-3 text-left hover:bg-gray-50 transition-colors flex items-center gap-3"
                  >
                    <FiTrendingUp className="h-4 w-4 text-primary" />
                    <span className="text-gray-900">{popular}</span>
                  </button>
                ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

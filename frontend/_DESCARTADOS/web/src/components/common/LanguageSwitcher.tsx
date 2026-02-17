/**
 * Language Switcher Component
 * 
 * Allows users to switch between Spanish and English
 * Persists selection in localStorage
 */

import { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Globe, Check, ChevronDown } from 'lucide-react';
import { 
  supportedLanguages, 
  languageLabels, 
  languageFlags,
  type SupportedLanguage 
} from '@/i18n';

interface LanguageSwitcherProps {
  variant?: 'dropdown' | 'inline' | 'minimal';
  className?: string;
}

export const LanguageSwitcher = ({ 
  variant = 'dropdown',
  className = '' 
}: LanguageSwitcherProps) => {
  const { i18n } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  
  // Normalize language code (e.g., 'es-DO' -> 'es')
  const currentLanguage = (i18n.language?.split('-')[0] || 'es') as SupportedLanguage;

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const changeLanguage = (lang: SupportedLanguage) => {
    i18n.changeLanguage(lang);
    document.documentElement.lang = lang;
    localStorage.setItem('i18nextLng', lang);
    setIsOpen(false);
  };

  // Minimal variant - single button with dropdown
  if (variant === 'minimal') {
    return (
      <div className={`relative ${className}`} ref={dropdownRef}>
        <button
          onClick={() => setIsOpen(!isOpen)}
          className="p-2 rounded-xl text-xl hover:bg-gray-100 transition-all duration-200"
          title={languageLabels[currentLanguage]}
          aria-label={`Current language: ${languageLabels[currentLanguage]}`}
          aria-expanded={isOpen}
        >
          {languageFlags[currentLanguage]}
        </button>

        {isOpen && (
          <div 
            className="
              absolute right-0 mt-2 w-36 py-1
              bg-white rounded-xl shadow-xl border border-gray-100
              z-50 animate-in fade-in slide-in-from-top-2 duration-200
            "
            role="listbox"
          >
            {supportedLanguages.map((lang) => (
              <button
                key={lang}
                onClick={() => changeLanguage(lang)}
                className={`
                  w-full flex items-center gap-3 px-3 py-2.5 text-sm
                  ${currentLanguage === lang 
                    ? 'bg-blue-50 text-blue-700' 
                    : 'text-gray-700 hover:bg-gray-50'
                  }
                `}
                role="option"
                aria-selected={currentLanguage === lang}
              >
                <span className="text-lg">{languageFlags[lang]}</span>
                <span className="flex-1 text-left font-medium">{languageLabels[lang]}</span>
                {currentLanguage === lang && (
                  <Check className="h-4 w-4 text-blue-600" />
                )}
              </button>
            ))}
          </div>
        )}
      </div>
    );
  }

  // Inline variant - horizontal buttons
  if (variant === 'inline') {
    return (
      <div className={`flex items-center gap-2 ${className}`}>
        {supportedLanguages.map((lang) => (
          <button
            key={lang}
            onClick={() => changeLanguage(lang)}
            className={`
              flex items-center gap-1.5 px-3 py-1.5 rounded-md text-sm font-medium transition-all
              ${currentLanguage === lang 
                ? 'bg-primary-500 text-white' 
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }
            `}
          >
            <span>{languageFlags[lang]}</span>
            <span>{languageLabels[lang]}</span>
          </button>
        ))}
      </div>
    );
  }

  // Dropdown variant (default)
  return (
    <div className={`relative ${className}`} ref={dropdownRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="
          flex items-center gap-2 px-3 py-2 rounded-lg
          bg-white border border-gray-200 
          hover:bg-gray-50 hover:border-gray-300
          transition-all text-sm font-medium text-gray-700
        "
        aria-expanded={isOpen}
        aria-haspopup="listbox"
      >
        <Globe className="h-4 w-4 text-gray-500" />
        <span>{languageFlags[currentLanguage]}</span>
        <span className="hidden sm:inline">{languageLabels[currentLanguage]}</span>
        <ChevronDown className={`h-4 w-4 text-gray-400 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
      </button>

      {isOpen && (
        <div 
          className="
            absolute right-0 mt-2 w-40 py-1
            bg-white rounded-lg shadow-lg border border-gray-200
            z-50 animate-in fade-in slide-in-from-top-2 duration-200
          "
          role="listbox"
        >
          {supportedLanguages.map((lang) => (
            <button
              key={lang}
              onClick={() => changeLanguage(lang)}
              className={`
                w-full flex items-center gap-3 px-4 py-2 text-sm
                ${currentLanguage === lang 
                  ? 'bg-primary-50 text-primary-700' 
                  : 'text-gray-700 hover:bg-gray-50'
                }
              `}
              role="option"
              aria-selected={currentLanguage === lang}
            >
              <span className="text-lg">{languageFlags[lang]}</span>
              <span className="flex-1 text-left">{languageLabels[lang]}</span>
              {currentLanguage === lang && (
                <Check className="h-4 w-4 text-primary-600" />
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

export default LanguageSwitcher;

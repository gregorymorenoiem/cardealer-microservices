/**
 * Combobox - Dropdown con capacidad de escribir/buscar
 *
 * Un componente híbrido entre select y input que permite:
 * - Seleccionar de una lista de opciones
 * - Escribir libremente para filtrar o ingresar valores personalizados
 */

import { useState, useRef, useEffect, forwardRef, useImperativeHandle } from 'react';
import { FiChevronDown, FiX, FiLoader } from 'react-icons/fi';

export interface ComboboxOption {
  value: string;
  label: string;
  sublabel?: string;
}

// Acepta tanto strings como objetos ComboboxOption
export type ComboboxOptionInput = string | ComboboxOption;

export interface ComboboxProps {
  options: ComboboxOptionInput[];
  value?: string;
  onChange?: (value: string) => void;
  onInputChange?: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  loading?: boolean;
  error?: string;
  label?: string;
  required?: boolean;
  allowCustom?: boolean; // Permite valores que no están en la lista
  name?: string;
  className?: string;
}

// Helper para normalizar opciones
function normalizeOption(option: ComboboxOptionInput): ComboboxOption {
  if (typeof option === 'string') {
    return { value: option, label: option };
  }
  return option;
}

const Combobox = forwardRef<HTMLInputElement, ComboboxProps>(
  (
    {
      options,
      value = '',
      onChange,
      onInputChange,
      placeholder = 'Select or type...',
      disabled = false,
      loading = false,
      error,
      label,
      required = false,
      allowCustom = true,
      name,
      className = '',
    },
    ref
  ) => {
    const [isOpen, setIsOpen] = useState(false);
    const [inputValue, setInputValue] = useState(value);
    const [highlightedIndex, setHighlightedIndex] = useState(-1);
    const containerRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);
    const listRef = useRef<HTMLUListElement>(null);

    // Normalize all options to ComboboxOption format
    const normalizedOptions = options.map(normalizeOption);

    // Expose input ref to parent
    useImperativeHandle(ref, () => inputRef.current as HTMLInputElement);

    // Sync inputValue with value prop
    useEffect(() => {
      setInputValue(value);
    }, [value]);

    // Filter options based on input (with null safety)
    const filteredOptions = normalizedOptions.filter((option) => {
      const searchValue = (inputValue || '').toLowerCase();
      return (
        (option.label || '').toLowerCase().includes(searchValue) ||
        (option.value || '').toLowerCase().includes(searchValue) ||
        (option.sublabel || '').toLowerCase().includes(searchValue)
      );
    });

    // Close dropdown when clicking outside
    useEffect(() => {
      const handleClickOutside = (event: MouseEvent) => {
        if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
          setIsOpen(false);
          // If allowCustom is false and value doesn't match any option, clear it
          if (!allowCustom) {
            const matchingOption = normalizedOptions.find(
              (o) =>
                (o.label || '').toLowerCase() === (inputValue || '').toLowerCase() ||
                o.value === inputValue
            );
            if (!matchingOption && inputValue) {
              setInputValue(value); // Reset to original value
            }
          }
        }
      };

      document.addEventListener('mousedown', handleClickOutside);
      return () => document.removeEventListener('mousedown', handleClickOutside);
    }, [allowCustom, inputValue, normalizedOptions, value]);

    // Scroll highlighted option into view
    useEffect(() => {
      if (highlightedIndex >= 0 && listRef.current) {
        const highlightedElement = listRef.current.children[highlightedIndex] as HTMLElement;
        if (highlightedElement) {
          highlightedElement.scrollIntoView({ block: 'nearest' });
        }
      }
    }, [highlightedIndex]);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const newValue = e.target.value;
      setInputValue(newValue);
      setIsOpen(true);
      setHighlightedIndex(-1);
      onInputChange?.(newValue);

      // If allowCustom, also call onChange with the typed value
      if (allowCustom) {
        onChange?.(newValue);
      }
    };

    const handleOptionSelect = (option: ComboboxOption) => {
      setInputValue(option.label);
      onChange?.(option.value);
      setIsOpen(false);
      setHighlightedIndex(-1);
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
      if (disabled) return;

      switch (e.key) {
        case 'ArrowDown':
          e.preventDefault();
          if (!isOpen) {
            setIsOpen(true);
          } else {
            setHighlightedIndex((prev) => (prev < filteredOptions.length - 1 ? prev + 1 : prev));
          }
          break;

        case 'ArrowUp':
          e.preventDefault();
          setHighlightedIndex((prev) => (prev > 0 ? prev - 1 : prev));
          break;

        case 'Enter':
          e.preventDefault();
          if (highlightedIndex >= 0 && filteredOptions[highlightedIndex]) {
            handleOptionSelect(filteredOptions[highlightedIndex]);
          } else if (allowCustom && inputValue) {
            onChange?.(inputValue);
            setIsOpen(false);
          }
          break;

        case 'Escape':
          setIsOpen(false);
          setHighlightedIndex(-1);
          break;

        case 'Tab':
          setIsOpen(false);
          break;
      }
    };

    const handleClear = () => {
      setInputValue('');
      onChange?.('');
      inputRef.current?.focus();
    };

    const handleToggle = () => {
      if (!disabled) {
        setIsOpen(!isOpen);
        if (!isOpen) {
          inputRef.current?.focus();
        }
      }
    };

    return (
      <div className={`relative ${className}`} ref={containerRef}>
        {label && (
          <label className="block text-sm font-medium text-gray-700 mb-1">
            {label} {required && <span className="text-red-500">*</span>}
          </label>
        )}

        <div className="relative">
          <input
            ref={inputRef}
            type="text"
            name={name}
            value={inputValue}
            onChange={handleInputChange}
            onFocus={() => setIsOpen(true)}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            disabled={disabled}
            className={`
              w-full px-3 py-2 pr-16 
              border rounded-lg 
              focus:ring-2 focus:ring-primary focus:border-transparent
              disabled:bg-gray-100 disabled:cursor-not-allowed
              ${error ? 'border-red-500' : 'border-gray-300'}
            `}
            autoComplete="off"
          />

          {/* Right side icons */}
          <div className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-1">
            {loading && <FiLoader className="w-4 h-4 text-gray-400 animate-spin" />}

            {inputValue && !disabled && (
              <button
                type="button"
                onClick={handleClear}
                className="p-1 text-gray-400 hover:text-gray-600 rounded"
              >
                <FiX className="w-4 h-4" />
              </button>
            )}

            <button
              type="button"
              onClick={handleToggle}
              disabled={disabled}
              className="p-1 text-gray-400 hover:text-gray-600 disabled:cursor-not-allowed"
            >
              <FiChevronDown
                className={`w-4 h-4 transition-transform ${isOpen ? 'rotate-180' : ''}`}
              />
            </button>
          </div>
        </div>

        {/* Dropdown list */}
        {isOpen && !disabled && (
          <ul
            ref={listRef}
            className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg max-h-60 overflow-auto"
          >
            {filteredOptions.length === 0 ? (
              <li className="px-3 py-2 text-sm text-gray-500 italic">
                {allowCustom ? 'No matches - type to add custom value' : 'No options available'}
              </li>
            ) : (
              filteredOptions.map((option, index) => (
                <li
                  key={option.value}
                  onClick={() => handleOptionSelect(option)}
                  onMouseEnter={() => setHighlightedIndex(index)}
                  className={`
                    px-3 py-2 cursor-pointer text-sm
                    ${highlightedIndex === index ? 'bg-primary/10 text-primary' : 'hover:bg-gray-50'}
                    ${option.value === value ? 'bg-primary/5 font-medium' : ''}
                  `}
                >
                  <div className="flex items-center justify-between">
                    <span>{option.label}</span>
                    {option.sublabel && (
                      <span className="text-xs text-gray-400">{option.sublabel}</span>
                    )}
                  </div>
                </li>
              ))
            )}
          </ul>
        )}

        {error && <p className="text-red-500 text-sm mt-1">{error}</p>}
      </div>
    );
  }
);

Combobox.displayName = 'Combobox';

export default Combobox;

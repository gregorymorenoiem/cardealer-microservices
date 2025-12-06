import { useState, useEffect } from 'react';

const COMPARE_KEY = 'cardealer_compare';
const MAX_COMPARE_ITEMS = 3;

export function useCompare() {
  const loadCompareItems = (): string[] => {
    try {
      const stored = localStorage.getItem(COMPARE_KEY);
      return stored ? JSON.parse(stored) : [];
    } catch (error) {
      console.error('Error loading compare items:', error);
      return [];
    }
  };

  const [compareItems, setCompareItems] = useState<string[]>(loadCompareItems);

  // Save to localStorage whenever compareItems changes
  useEffect(() => {
    try {
      localStorage.setItem(COMPARE_KEY, JSON.stringify(compareItems));
    } catch (error) {
      console.error('Error saving compare items:', error);
    }
  }, [compareItems]);

  const addToCompare = (vehicleId: string) => {
    setCompareItems((prev) => {
      if (prev.includes(vehicleId)) {
        return prev; // Already in compare
      }
      if (prev.length >= MAX_COMPARE_ITEMS) {
        alert(`You can only compare up to ${MAX_COMPARE_ITEMS} vehicles at a time`);
        return prev;
      }
      return [...prev, vehicleId];
    });
  };

  const removeFromCompare = (vehicleId: string) => {
    setCompareItems((prev) => prev.filter((id) => id !== vehicleId));
  };

  const clearCompare = () => {
    setCompareItems([]);
  };

  const isInCompare = (vehicleId: string) => {
    return compareItems.includes(vehicleId);
  };

  const canAddMore = () => {
    return compareItems.length < MAX_COMPARE_ITEMS;
  };

  return {
    compareItems,
    addToCompare,
    removeFromCompare,
    clearCompare,
    isInCompare,
    canAddMore,
    count: compareItems.length,
    maxItems: MAX_COMPARE_ITEMS,
  };
}

import { useState, useEffect, useCallback, useSyncExternalStore } from 'react';

const COMPARE_KEY = 'cardealer_compare';
const MAX_COMPARE_ITEMS = 3;

// Global store for compare items
let compareStore: string[] = [];
let listeners: Set<() => void> = new Set();

// Initialize from localStorage
try {
  const stored = localStorage.getItem(COMPARE_KEY);
  compareStore = stored ? JSON.parse(stored) : [];
} catch {
  compareStore = [];
}

function getSnapshot(): string[] {
  return compareStore;
}

function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

function notifyListeners() {
  listeners.forEach(listener => listener());
}

function setCompareStore(newItems: string[]) {
  compareStore = newItems;
  try {
    localStorage.setItem(COMPARE_KEY, JSON.stringify(newItems));
  } catch (error) {
    console.error('Error saving compare items:', error);
  }
  notifyListeners();
}

// Listen for localStorage changes from other tabs
if (typeof window !== 'undefined') {
  window.addEventListener('storage', (e) => {
    if (e.key === COMPARE_KEY) {
      try {
        compareStore = e.newValue ? JSON.parse(e.newValue) : [];
        notifyListeners();
      } catch {
        // Ignore parse errors
      }
    }
  });
}

export function useCompare() {
  const compareItems = useSyncExternalStore(subscribe, getSnapshot, getSnapshot);

  const addToCompare = useCallback((vehicleId: string) => {
    if (compareStore.includes(vehicleId)) {
      return; // Already in compare
    }
    if (compareStore.length >= MAX_COMPARE_ITEMS) {
      alert(`You can only compare up to ${MAX_COMPARE_ITEMS} vehicles at a time`);
      return;
    }
    setCompareStore([...compareStore, vehicleId]);
  }, []);

  const removeFromCompare = useCallback((vehicleId: string) => {
    setCompareStore(compareStore.filter((id) => id !== vehicleId));
  }, []);

  const clearCompare = useCallback(() => {
    setCompareStore([]);
  }, []);

  const isInCompare = useCallback((vehicleId: string) => {
    return compareItems.includes(vehicleId);
  }, [compareItems]);

  const canAddMore = useCallback(() => {
    return compareItems.length < MAX_COMPARE_ITEMS;
  }, [compareItems.length]);

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

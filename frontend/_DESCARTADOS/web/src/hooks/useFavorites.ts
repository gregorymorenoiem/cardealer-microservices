import { useState, useEffect } from 'react';

const FAVORITES_KEY = 'cardealer_favorites';

// Load initial favorites from localStorage
const loadFavorites = (): string[] => {
  try {
    const stored = localStorage.getItem(FAVORITES_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch (error) {
    console.error('Error loading favorites:', error);
    return [];
  }
};

export function useFavorites() {
  const [favorites, setFavorites] = useState<string[]>(loadFavorites);

  // Save favorites to localStorage whenever they change
  useEffect(() => {
    localStorage.setItem(FAVORITES_KEY, JSON.stringify(favorites));
  }, [favorites]);

  const addFavorite = (vehicleId: string) => {
    setFavorites((prev) => {
      if (prev.includes(vehicleId)) {
        return prev;
      }
      return [...prev, vehicleId];
    });
  };

  const removeFavorite = (vehicleId: string) => {
    setFavorites((prev) => prev.filter((id) => id !== vehicleId));
  };

  const toggleFavorite = (vehicleId: string) => {
    if (favorites.includes(vehicleId)) {
      removeFavorite(vehicleId);
    } else {
      addFavorite(vehicleId);
    }
  };

  const isFavorite = (vehicleId: string) => {
    return favorites.includes(vehicleId);
  };

  return {
    favorites,
    addFavorite,
    removeFavorite,
    toggleFavorite,
    isFavorite,
  };
}

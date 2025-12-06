import React, { useState, useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { clsx } from 'clsx';
import { OklaHeader } from '../components/navigation/OklaHeader';
import { OklaFooter } from '../components/organisms/OklaFooter';

/**
 * OKLA Main Layout
 * 
 * Layout principal que envuelve todas las páginas públicas
 * con el header y footer premium de OKLA.
 */

export interface OklaLayoutProps {
  children?: React.ReactNode;
}

export const OklaLayout: React.FC<OklaLayoutProps> = ({ children }) => {
  const [darkMode, setDarkMode] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  
  // Check for dark mode preference
  useEffect(() => {
    const isDark = localStorage.getItem('okla-dark-mode') === 'true' ||
      (!localStorage.getItem('okla-dark-mode') && 
       window.matchMedia('(prefers-color-scheme: dark)').matches);
    
    setDarkMode(isDark);
    if (isDark) {
      document.documentElement.setAttribute('data-theme', 'dark');
      document.documentElement.classList.add('dark');
    }
  }, []);

  const toggleDarkMode = () => {
    const newValue = !darkMode;
    setDarkMode(newValue);
    localStorage.setItem('okla-dark-mode', String(newValue));
    
    if (newValue) {
      document.documentElement.setAttribute('data-theme', 'dark');
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.removeAttribute('data-theme');
      document.documentElement.classList.remove('dark');
    }
  };

  const handleLogin = () => {
    // Navigate to login page or open modal
    window.location.href = '/login';
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    // Clear auth state
  };

  const handleSearch = (query: string) => {
    window.location.href = `/browse?q=${encodeURIComponent(query)}`;
  };

  // Mock user for demonstration
  const mockUser = isLoggedIn ? {
    name: 'Usuario Demo',
    email: 'demo@okla.com',
  } : undefined;

  return (
    <div 
      className={clsx(
        'min-h-screen flex flex-col',
        'bg-white dark:bg-gray-950',
        'transition-colors duration-300'
      )}
    >
      <OklaHeader
        isLoggedIn={isLoggedIn}
        user={mockUser}
        onLogin={handleLogin}
        onLogout={handleLogout}
        onSearch={handleSearch}
        notificationCount={3}
        favoriteCount={5}
        darkMode={darkMode}
        onToggleDarkMode={toggleDarkMode}
      />
      
      <main className="flex-1">
        {children || <Outlet />}
      </main>
      
      <OklaFooter />
    </div>
  );
};

export default OklaLayout;

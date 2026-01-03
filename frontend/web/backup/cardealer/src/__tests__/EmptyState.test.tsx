import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import EmptyState from '../components/organisms/EmptyState';

const renderWithRouter = (ui: React.ReactElement) => {
  return render(<BrowserRouter>{ui}</BrowserRouter>);
};

describe('EmptyState', () => {
  describe('No Results Type', () => {
    it('should render no-results state with default content', () => {
      const onAction = vi.fn();
      renderWithRouter(<EmptyState type="no-results" onAction={onAction} />);
      
      expect(screen.getByText('No vehicles found')).toBeInTheDocument();
      expect(screen.getByText(/Try adjusting your filters/i)).toBeInTheDocument();
    });

    it('should show suggestions for no-results', () => {
      renderWithRouter(<EmptyState type="no-results" />);
      
      expect(screen.getByText('Suggestions:')).toBeInTheDocument();
      expect(screen.getByText(/Check your spelling/i)).toBeInTheDocument();
      expect(screen.getByText(/Try more general keywords/i)).toBeInTheDocument();
    });

    it('should call onAction when button is clicked', () => {
      const onAction = vi.fn();
      renderWithRouter(<EmptyState type="no-results" onAction={onAction} />);
      
      const button = screen.getByRole('button', { name: /clear filters/i });
      fireEvent.click(button);
      
      expect(onAction).toHaveBeenCalledTimes(1);
    });
  });

  describe('No Favorites Type', () => {
    it('should render no-favorites state', () => {
      renderWithRouter(<EmptyState type="no-favorites" />);
      
      expect(screen.getByText('No favorites yet')).toBeInTheDocument();
      expect(screen.getByText(/Start exploring/i)).toBeInTheDocument();
    });

    it('should have link to browse page', () => {
      renderWithRouter(<EmptyState type="no-favorites" />);
      
      const link = screen.getByRole('link', { name: /browse vehicles/i });
      expect(link).toHaveAttribute('href', '/browse');
    });
  });

  describe('Error Type', () => {
    it('should render error state', () => {
      renderWithRouter(<EmptyState type="error" />);
      
      expect(screen.getByText('Something went wrong')).toBeInTheDocument();
      expect(screen.getByText(/couldn't load the data/i)).toBeInTheDocument();
    });

    it('should have try again button', () => {
      const onAction = vi.fn();
      renderWithRouter(<EmptyState type="error" onAction={onAction} />);
      
      const button = screen.getByRole('button', { name: /try again/i });
      expect(button).toBeInTheDocument();
    });
  });

  describe('No Listings Type', () => {
    it('should render no-listings state', () => {
      renderWithRouter(<EmptyState type="no-listings" />);
      
      expect(screen.getByText('No listings yet')).toBeInTheDocument();
      expect(screen.getByText(/haven't created any vehicle listings/i)).toBeInTheDocument();
    });

    it('should have link to sell page', () => {
      renderWithRouter(<EmptyState type="no-listings" />);
      
      const link = screen.getByRole('link', { name: /create listing/i });
      expect(link).toHaveAttribute('href', '/sell');
    });
  });

  describe('Inbox Type', () => {
    it('should render inbox empty state', () => {
      renderWithRouter(<EmptyState type="inbox" />);
      
      expect(screen.getByText('No messages')).toBeInTheDocument();
      expect(screen.getByText(/don't have any messages yet/i)).toBeInTheDocument();
    });

    it('should not have action button for inbox', () => {
      renderWithRouter(<EmptyState type="inbox" />);
      
      const buttons = screen.queryAllByRole('button');
      expect(buttons.length).toBe(0);
    });
  });

  describe('Custom Content', () => {
    it('should allow custom title', () => {
      renderWithRouter(<EmptyState type="no-results" title="Custom Title" />);
      
      expect(screen.getByText('Custom Title')).toBeInTheDocument();
    });

    it('should allow custom message', () => {
      renderWithRouter(<EmptyState type="no-results" message="Custom message here" />);
      
      expect(screen.getByText('Custom message here')).toBeInTheDocument();
    });

    it('should allow custom action label', () => {
      renderWithRouter(<EmptyState type="no-results" actionLabel="Custom Action" onAction={vi.fn()} />);
      
      expect(screen.getByRole('button', { name: 'Custom Action' })).toBeInTheDocument();
    });

    it('should allow custom action href', () => {
      renderWithRouter(<EmptyState type="no-results" actionLabel="Go Home" actionHref="/" />);
      
      const link = screen.getByRole('link', { name: 'Go Home' });
      expect(link).toHaveAttribute('href', '/');
    });
  });

  describe('Icon Display', () => {
    it('should display icon for each type', () => {
      const { container: noResults } = renderWithRouter(<EmptyState type="no-results" />);
      expect(noResults.querySelector('svg')).toBeInTheDocument();

      const { container: noFavorites } = renderWithRouter(<EmptyState type="no-favorites" />);
      expect(noFavorites.querySelector('svg')).toBeInTheDocument();

      const { container: error } = renderWithRouter(<EmptyState type="error" />);
      expect(error.querySelector('svg')).toBeInTheDocument();
    });
  });
});

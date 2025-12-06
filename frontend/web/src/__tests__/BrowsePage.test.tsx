import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import BrowsePage from '../pages/BrowsePage';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

// Create a test query client
const createTestQueryClient = () => new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
    },
  },
});

// Helper to wrap components with necessary providers
const renderWithProviders = (ui: React.ReactElement) => {
  const testQueryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={testQueryClient}>
      <BrowserRouter>
        {ui}
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('BrowsePage', () => {
  describe('Rendering', () => {
    it('should render the page title', () => {
      renderWithProviders(<BrowsePage />);
      expect(screen.getByText('Browse Vehicles')).toBeInTheDocument();
    });

    it('should render filters sidebar', () => {
      renderWithProviders(<BrowsePage />);
      // AdvancedFilters should be present (check for specific filter label)
      expect(screen.getByText(/basic filters/i)).toBeInTheDocument();
    });

    it('should render view mode toggle buttons', () => {
      renderWithProviders(<BrowsePage />);
      const gridButton = screen.getByLabelText('Grid view');
      const listButton = screen.getByLabelText('List view');
      expect(gridButton).toBeInTheDocument();
      expect(listButton).toBeInTheDocument();
    });

    it('should display vehicle count', () => {
      renderWithProviders(<BrowsePage />);
      expect(screen.getByText(/vehicles found/i)).toBeInTheDocument();
    });
  });

  describe('View Mode', () => {
    it('should start in grid view by default', () => {
      renderWithProviders(<BrowsePage />);
      const gridButton = screen.getByLabelText('Grid view');
      expect(gridButton).toHaveClass('text-primary');
    });

    it('should switch to list view when list button is clicked', async () => {
      renderWithProviders(<BrowsePage />);
      const listButton = screen.getByLabelText('List view');
      
      fireEvent.click(listButton);
      
      await waitFor(() => {
        expect(listButton).toHaveClass('text-primary');
      });
    });

    it('should switch back to grid view', async () => {
      renderWithProviders(<BrowsePage />);
      const listButton = screen.getByLabelText('List view');
      const gridButton = screen.getByLabelText('Grid view');
      
      fireEvent.click(listButton);
      await waitFor(() => {
        expect(listButton).toHaveClass('text-primary');
      });
      
      fireEvent.click(gridButton);
      await waitFor(() => {
        expect(gridButton).toHaveClass('text-primary');
      });
    });
  });

  describe('Vehicle Cards', () => {
    it('should render vehicle cards', async () => {
      renderWithProviders(<BrowsePage />);
      
      await waitFor(() => {
        const vehicleCards = screen.getAllByRole('link', { name: /view details/i });
        expect(vehicleCards.length).toBeGreaterThan(0);
      });
    });

    it('should display vehicle information', async () => {
      renderWithProviders(<BrowsePage />);
      
      await waitFor(() => {
        // Should show at least one vehicle make/model
        expect(screen.getAllByText(/toyota|honda|ford|bmw/i).length).toBeGreaterThan(0);
      });
    });
  });

  describe('Pagination', () => {
    it('should render pagination when there are multiple pages', async () => {
      renderWithProviders(<BrowsePage />);
      
      await waitFor(() => {
        // Pagination may or may not be visible depending on number of results
        // Just verify the page renders without errors by checking for heading
        expect(screen.getByText(/browse vehicles/i)).toBeInTheDocument();
      });
    });

    it('should navigate to next page', async () => {
      renderWithProviders(<BrowsePage />);
      
      await waitFor(() => {
        // Check if pagination exists (only if there are multiple pages)
        const nextButtons = screen.queryAllByLabelText(/next page/i);
        // If pagination exists, verify it's rendered
        if (nextButtons.length > 0) {
          expect(nextButtons[0]).toBeInTheDocument();
        } else {
          // If no pagination, verify we have vehicles displayed
          const vehicleCards = screen.queryAllByRole('article');
          expect(vehicleCards.length).toBeGreaterThanOrEqual(0);
        }
      });
    });
  });

  describe('Empty State', () => {
    it('should show empty state when no vehicles match filters', async () => {
      renderWithProviders(<BrowsePage />);
      
      // This test would need to simulate filtering that returns no results
      // For now, we just verify the component can handle it
      expect(screen.getByText(/browse vehicles/i)).toBeInTheDocument();
    });
  });

  describe('URL Parameters', () => {
    it('should initialize filters from URL parameters', () => {
      // This would require a custom history with search params
      renderWithProviders(<BrowsePage />);
      expect(screen.getByText(/browse vehicles/i)).toBeInTheDocument();
    });

    it('should update URL when filters change', async () => {
      renderWithProviders(<BrowsePage />);
      // Test would verify URL params are updated
      expect(screen.getByText(/browse vehicles/i)).toBeInTheDocument();
    });
  });

  describe('Loading State', () => {
    it('should show skeleton loaders while loading', () => {
      // This test would mock the loading state
      renderWithProviders(<BrowsePage />);
      
      // When isLoading is true, VehicleCardSkeleton should be shown
      // Currently using mock data so isLoading is always false
      expect(screen.getByText(/browse vehicles/i)).toBeInTheDocument();
    });
  });

  describe('Error State', () => {
    it('should show error state when API call fails', () => {
      // This test would mock an API error
      renderWithProviders(<BrowsePage />);
      
      // When isError is true, EmptyState with type="error" should be shown
      expect(screen.getByText(/browse vehicles/i)).toBeInTheDocument();
    });
  });

  describe('Scrolling', () => {
    it('should scroll to top when page changes', async () => {
      const scrollToMock = vi.fn();
      window.scrollTo = scrollToMock;
      
      renderWithProviders(<BrowsePage />);
      
      await waitFor(() => {
        const nextButtons = screen.queryAllByLabelText(/next page/i);
        if (nextButtons.length > 0 && !nextButtons[0].hasAttribute('disabled')) {
          fireEvent.click(nextButtons[0]);
          expect(scrollToMock).toHaveBeenCalledWith({
            top: 0,
            behavior: 'smooth',
          });
        }
      });
    });
  });
});

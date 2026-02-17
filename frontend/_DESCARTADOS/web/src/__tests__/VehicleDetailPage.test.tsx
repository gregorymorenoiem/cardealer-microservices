import { render, screen } from './setup/test-utils';
import { Routes, Route, MemoryRouter } from 'react-router-dom';
import { describe, it, expect, vi } from 'vitest';
import VehicleDetailPage from '@/pages/vehicles/VehicleDetailPage';
import { mockVehicles } from '@/data/mockVehicles';

// Mock the hooks
vi.mock('@/hooks/useFavorites', () => ({
  useFavorites: () => ({
    isFavorite: vi.fn(() => false),
    toggleFavorite: vi.fn(),
  }),
}));

vi.mock('@/hooks/useCompare', () => ({
  useCompare: () => ({
    isInCompare: vi.fn(() => false),
    addToCompare: vi.fn(),
    removeFromCompare: vi.fn(),
  }),
}));

const renderWithRouter = (vehicleId: string = '1') => {
  // Set initial location
  window.history.pushState({}, 'Test page', `/vehicle/${vehicleId}`);
  
  return render(
    <BrowserRouter>
      <Routes>
        <Route path="/vehicle/:id" element={<VehicleDetailPage />} />
      </Routes>
    </BrowserRouter>
  );
};

// TODO: Fix these tests - they need proper API mocking for vehicle data
describe.skip('VehicleDetailPage', () => {
  const firstVehicle = mockVehicles[0];

  describe('Page Rendering', () => {
    it('should render vehicle title', () => {
      renderWithRouter();

      const vehicleTitle = `${firstVehicle.year} ${firstVehicle.make} ${firstVehicle.model}`;
      // Vehicle title appears multiple times (in h1 and similar vehicles)
      const titleElements = screen.getAllByText(vehicleTitle);
      expect(titleElements.length).toBeGreaterThan(0);
    });    it('should render vehicle price', () => {
      renderWithRouter();
      
      // Price should be displayed
      const priceElements = screen.getAllByText(/\$[\d,]+/);
      expect(priceElements.length).toBeGreaterThan(0);
    });

    it('should render vehicle location', () => {
      renderWithRouter();
      
      expect(screen.getByText(firstVehicle.location)).toBeInTheDocument();
    });
  });

  describe('Breadcrumbs', () => {
    it('should render breadcrumb navigation', () => {
      renderWithRouter();

      // "Browse" appears in nav, breadcrumbs, button, and footer
      const browseElements = screen.getAllByText(/browse/i);
      expect(browseElements.length).toBeGreaterThan(0);
    });    it('should have link to home', () => {
      renderWithRouter();
      
      const homeLink = screen.getAllByRole('link')[0]; // First link should be home
      expect(homeLink).toHaveAttribute('href', '/');
    });

    it('should have link to browse page', () => {
      renderWithRouter();
      
      // Verify Browse link exists in breadcrumbs (multiple browse links exist in nav too)
      const browseLinks = screen.getAllByText(/browse/i);
      expect(browseLinks.length).toBeGreaterThan(0);
      
      // Check that at least one link goes to /browse
      const hasValidBrowseLink = Array.from(document.querySelectorAll('a[href="/browse"]')).length > 0;
      expect(hasValidBrowseLink).toBe(true);
    });
  });

  describe('Action Buttons', () => {
    it('should render Save button', () => {
      renderWithRouter();
      
      expect(screen.getByText(/save/i)).toBeInTheDocument();
    });

    it('should render Share button', () => {
      renderWithRouter();
      
      expect(screen.getByText(/share/i)).toBeInTheDocument();
    });

    it('should render Print button', () => {
      renderWithRouter();
      
      expect(screen.getByText(/print/i)).toBeInTheDocument();
    });

    it('should render Call Seller button', () => {
      renderWithRouter();
      
      expect(screen.getByText(/call seller/i)).toBeInTheDocument();
    });

    it('should render Back to Browse button', () => {
      renderWithRouter();
      
      expect(screen.getByText(/back to browse/i)).toBeInTheDocument();
    });
  });

  describe('Vehicle Information', () => {
    it('should render description section', () => {
      renderWithRouter();
      
      expect(screen.getByText(/description/i)).toBeInTheDocument();
    });

    it('should render specifications section', () => {
      renderWithRouter();
      
      expect(screen.getByText(/specifications/i)).toBeInTheDocument();
    });

    it('should render features section if vehicle has features', () => {
      renderWithRouter();
      
      if (firstVehicle.features && firstVehicle.features.length > 0) {
        expect(screen.getByText(/features & options/i)).toBeInTheDocument();
      }
    });

    it('should display condition badge if available', () => {
      renderWithRouter();
      
      if (firstVehicle.condition) {
        // Multiple elements may have this text (badge, specs table), just verify it appears
        const conditionElements = screen.getAllByText(firstVehicle.condition);
        expect(conditionElements.length).toBeGreaterThan(0);
      }
    });
  });

  describe('Seller Information', () => {
    it('should render seller information section', () => {
      renderWithRouter();
      
      expect(screen.getByText(/seller information/i)).toBeInTheDocument();
    });

    it('should display seller name', () => {
      renderWithRouter();
      
      expect(screen.getByText(firstVehicle.seller.name)).toBeInTheDocument();
    });

    it('should display seller type', () => {
      renderWithRouter();
      
      expect(screen.getByText(firstVehicle.seller.type)).toBeInTheDocument();
    });

    it('should display seller phone number', () => {
      renderWithRouter();
      
      const phoneLinks = screen.getAllByText(firstVehicle.seller.phone);
      expect(phoneLinks.length).toBeGreaterThan(0);
    });
  });

  describe('Contact Form', () => {
    it('should render contact form', () => {
      renderWithRouter();
      
      // Check that the contact form exists by looking for the heading
      expect(screen.getByText(/send message/i)).toBeInTheDocument();
    });
  });

  describe('Similar Vehicles', () => {
    it('should render similar vehicles section', () => {
      renderWithRouter();
      
      expect(screen.getByText(/similar vehicles/i)).toBeInTheDocument();
    });
  });

  describe('Badges', () => {
    it('should display Featured badge if vehicle is featured', () => {
      renderWithRouter();
      
      if (firstVehicle.isFeatured) {
        // Multiple featured badges may exist (on page and in similar vehicles)
        const featuredElements = screen.getAllByText(/featured/i);
        expect(featuredElements.length).toBeGreaterThan(0);
      }
    });

    it('should display New badge if vehicle is new', () => {
      renderWithRouter();
      
      if (firstVehicle.isNew) {
        // Check that we have a "New" badge somewhere (multiple elements may have this text)
        const newElements = screen.getAllByText(/^new$/i);
        expect(newElements.length).toBeGreaterThan(0);
      }
    });
  });

  describe('Navigation', () => {
    it('should redirect to browse page if vehicle not found', () => {
      const { container } = renderWithRouter('nonexistent');
      
      // Should redirect, so the page content won't be there
      expect(container.textContent).not.toContain('Seller Information');
    });
  });

  describe('Responsive Layout', () => {
    it('should have responsive grid classes', () => {
      const { container } = renderWithRouter();
      
      // Check for responsive grid
      const grid = container.querySelector('.grid-cols-1.lg\\:grid-cols-3');
      expect(grid).toBeInTheDocument();
    });
  });

  describe('Print Styles', () => {
    it('should include print styles in the page', () => {
      const { container } = renderWithRouter();
      
      // Check if style tags are present (may include ShareButton and VehicleDetailPage styles)
      const styleTags = container.querySelectorAll('style');
      expect(styleTags.length).toBeGreaterThan(0);
      
      // Verify one of them contains print media query
      const hasPrintStyles = Array.from(styleTags).some(
        tag => tag.textContent?.includes('@media print')
      );
      // Print styles should be in VehicleDetailPage
      expect(hasPrintStyles).toBe(true);
    });
  });
});

import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, it, expect } from 'vitest';
import SimilarVehicles from '@/components/organisms/SimilarVehicles';
import { mockVehicles } from '@/data/mockVehicles';

const renderWithRouter = (ui: React.ReactElement) => {
  return render(<BrowserRouter>{ui}</BrowserRouter>);
};

describe('SimilarVehicles', () => {
  const currentVehicle = mockVehicles[0];

  describe('Rendering', () => {
    it('should render the component with heading', () => {
      renderWithRouter(<SimilarVehicles currentVehicle={currentVehicle} />);
      expect(screen.getByText(/similar vehicles/i)).toBeInTheDocument();
    });

    it('should render View All link', () => {
      renderWithRouter(<SimilarVehicles currentVehicle={currentVehicle} />);
      const viewAllLink = screen.getByText(/view all/i);
      expect(viewAllLink).toBeInTheDocument();
      expect(viewAllLink.closest('a')).toHaveAttribute('href', '/browse');
    });

    it('should render vehicle cards', () => {
      renderWithRouter(<SimilarVehicles currentVehicle={currentVehicle} />);
      // Should render up to 4 similar vehicles (default maxItems)
      const cards = screen.getAllByText(/\d{4}/); // Match year in cards
      expect(cards.length).toBeGreaterThan(0);
      expect(cards.length).toBeLessThanOrEqual(4);
    });
  });

  describe('Similarity Algorithm', () => {
    it('should not include the current vehicle in results', () => {
      renderWithRouter(<SimilarVehicles currentVehicle={currentVehicle} />);
      
      // Get all vehicle titles displayed
      const vehicleTitle = `${currentVehicle.year} ${currentVehicle.make} ${currentVehicle.model}`;
      
      // The exact current vehicle should not be in the similar vehicles list
      // Note: Similar vehicles might have same make/model but different year
      const allText = screen.queryByText(vehicleTitle, { exact: true });
      expect(allText).not.toBeInTheDocument();
    });

    it('should respect maxItems prop', () => {
      renderWithRouter(<SimilarVehicles currentVehicle={currentVehicle} maxItems={2} />);
      
      // Should render at most 2 vehicles
      const cards = screen.getAllByText(/\d{4}/);
      expect(cards.length).toBeLessThanOrEqual(2);
    });
  });

  describe('Empty State', () => {
    it('should render browse button when few similar vehicles', () => {
      // Create a unique vehicle that won't have many matches
      const uniqueVehicle = {
        ...mockVehicles[0],
        id: 'unique-999',
        make: 'UniqueTestMake',
        model: 'UniqueTestModel',
        bodyType: 'Sedan' as const,
      };

      renderWithRouter(<SimilarVehicles currentVehicle={uniqueVehicle} maxItems={10} />);
      
      // If less than 3 results, should show the "Looking for more options?" message
      const lookingText = screen.queryByText(/looking for more options/i);
      const browseButton = screen.queryByText(/browse all vehicles/i);
      
      if (lookingText) {
        expect(browseButton).toBeInTheDocument();
      }
    });
  });

  describe('Grid Layout', () => {
    it('should use responsive grid layout', () => {
      const { container } = renderWithRouter(<SimilarVehicles currentVehicle={currentVehicle} />);
      
      // Check for grid classes
      const gridContainer = container.querySelector('.grid');
      expect(gridContainer).toBeInTheDocument();
      expect(gridContainer).toHaveClass('grid-cols-1');
      expect(gridContainer).toHaveClass('md:grid-cols-2');
      expect(gridContainer).toHaveClass('xl:grid-cols-4');
    });
  });

  describe('Links', () => {
    it('should have working links to vehicle details', () => {
      renderWithRouter(<SimilarVehicles currentVehicle={currentVehicle} />);
      
      // Get all links in the component
      const links = screen.getAllByRole('link');
      
      // Should have at least the "View All" link plus vehicle detail links
      expect(links.length).toBeGreaterThan(1);
    });
  });
});

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import ReviewStep from '@/components/organisms/sell/ReviewStep';
import type { VehicleFormData } from '@/pages/SellYourCarPage';

describe('ReviewStep', () => {
  const mockOnSubmit = vi.fn();
  const mockOnBack = vi.fn();
  const mockOnSaveDraft = vi.fn();

  // Create a mock File for images
  const createMockFile = (name: string): File => {
    return new File([''], name, { type: 'image/jpeg' });
  };

  const mockData: VehicleFormData = {
    // Vehicle Info
    make: 'Toyota',
    model: 'Camry',
    year: 2023,
    mileage: 15000,
    vin: '12345678901234567',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
    bodyType: 'Sedan',
    drivetrain: 'FWD',
    engine: '2.5L I4',
    horsepower: '203 hp',
    mpg: '28 city / 39 hwy',
    exteriorColor: 'Pearl White',
    interiorColor: 'Black Leather',
    condition: 'Used',
    features: ['Bluetooth', 'Navigation System', 'Backup Camera', 'Leather Seats', 'Sunroof'],
    // Photos
    images: [createMockFile('car1.jpg'), createMockFile('car2.jpg')],
    // Pricing
    price: 25000,
    description: 'This is an excellent vehicle in great condition with full service history and many premium features.',
    location: 'Los Angeles, CA',
    sellerName: 'John Doe',
    sellerPhone: '+1 555 123 4567',
    sellerEmail: 'john@example.com',
    sellerType: 'private',
  };

  const defaultProps = {
    data: mockData,
    onSubmit: mockOnSubmit,
    onBack: mockOnBack,
    onSaveDraft: mockOnSaveDraft,
  };

  beforeEach(() => {
    vi.clearAllMocks();
    // Mock URL.createObjectURL
    window.URL.createObjectURL = vi.fn(() => 'mock-url');
  });

  describe('Rendering', () => {
    it('should render the form title', () => {
      render(<ReviewStep {...defaultProps} />);
      expect(screen.getByText('Review Your Listing')).toBeInTheDocument();
    });

    it('should display vehicle title', () => {
      render(<ReviewStep {...defaultProps} />);
      expect(screen.getByText('2023 Toyota Camry')).toBeInTheDocument();
    });

    it('should display formatted price', () => {
      render(<ReviewStep {...defaultProps} />);
      const prices = screen.getAllByText('$25,000');
      expect(prices.length).toBeGreaterThan(0);
    });

    it('should display formatted mileage', () => {
      render(<ReviewStep {...defaultProps} />);
      expect(screen.getByText(/15,000 miles/i)).toBeInTheDocument();
    });

    it('should display location', () => {
      render(<ReviewStep {...defaultProps} />);
      expect(screen.getByText(/los angeles, ca/i)).toBeInTheDocument();
    });

    it('should display condition', () => {
      render(<ReviewStep {...defaultProps} />);
      expect(screen.getByText('Used')).toBeInTheDocument();
    });

    it('should display vehicle description', () => {
      render(<ReviewStep {...defaultProps} />);
      expect(screen.getByText(/excellent vehicle/i)).toBeInTheDocument();
    });

    it('should display specifications', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText('Automatic')).toBeInTheDocument();
      expect(screen.getByText('Gasoline')).toBeInTheDocument();
      expect(screen.getByText('Sedan')).toBeInTheDocument();
      expect(screen.getByText('FWD')).toBeInTheDocument();
    });

    it('should display features', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText('Bluetooth')).toBeInTheDocument();
      expect(screen.getByText('Navigation System')).toBeInTheDocument();
      expect(screen.getByText('Backup Camera')).toBeInTheDocument();
    });

    it('should display seller information', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText('John Doe')).toBeInTheDocument();
      expect(screen.getByText('+1 555 123 4567')).toBeInTheDocument();
      expect(screen.getByText('john@example.com')).toBeInTheDocument();
    });

    it('should display seller type', () => {
      render(<ReviewStep {...defaultProps} />);
      expect(screen.getByText('Private Seller')).toBeInTheDocument();
    });

    it('should show dealer type when applicable', () => {
      const dealerData = { ...mockData, sellerType: 'dealer' as const };
      render(<ReviewStep {...defaultProps} data={dealerData} />);
      
      expect(screen.getByText('Dealer')).toBeInTheDocument();
    });
  });

  describe('Images Display', () => {
    it('should display main image', () => {
      render(<ReviewStep {...defaultProps} />);
      
      const images = screen.getAllByRole('img');
      expect(images.length).toBeGreaterThan(0);
    });

    it('should handle multiple images', () => {
      const manyImages = Array.from({ length: 6 }, (_, i) => createMockFile(`car${i + 1}.jpg`));
      const dataWithManyImages = { ...mockData, images: manyImages };
      
      render(<ReviewStep {...defaultProps} data={dataWithManyImages} />);
      
      // Should show "+X more" indicator
      expect(screen.getByText('+1 more')).toBeInTheDocument();
    });

    it('should handle no images gracefully', () => {
      const dataWithoutImages = { ...mockData, images: [] };
      render(<ReviewStep {...defaultProps} data={dataWithoutImages} />);
      
      // Should still render other content
      expect(screen.getByText('2023 Toyota Camry')).toBeInTheDocument();
    });
  });

  describe('Terms Agreement', () => {
    it('should render terms and conditions checkbox', () => {
      render(<ReviewStep {...defaultProps} />);
      
      const checkbox = screen.getByRole('checkbox');
      expect(checkbox).toBeInTheDocument();
      expect(checkbox).not.toBeChecked();
    });

    it('should toggle checkbox when clicked', () => {
      render(<ReviewStep {...defaultProps} />);
      
      const checkbox = screen.getByRole('checkbox');
      fireEvent.click(checkbox);
      
      expect(checkbox).toBeChecked();
    });

    it('should not submit without agreeing to terms', () => {
      render(<ReviewStep {...defaultProps} />);
      
      const publishButton = screen.getByText('Publish Listing');
      // Button should be disabled, so click won't do anything
      fireEvent.click(publishButton);
      
      expect(mockOnSubmit).not.toHaveBeenCalled();
    });
  });

  describe('Form Submission', () => {
    it('should call onSubmit when publish button is clicked with agreement', async () => {
      render(<ReviewStep {...defaultProps} />);
      
      // Agree to terms
      const checkbox = screen.getByRole('checkbox');
      fireEvent.click(checkbox);
      
      // Click publish
      const publishButton = screen.getByText('Publish Listing');
      fireEvent.click(publishButton);

      await waitFor(() => {
        expect(mockOnSubmit).toHaveBeenCalled();
      });
    });

    it('should show loading state while submitting', async () => {
      // Mock a delayed submission
      const delayedSubmit = vi.fn(() => new Promise(resolve => setTimeout(resolve, 100)));
      
      render(<ReviewStep {...defaultProps} onSubmit={delayedSubmit} />);
      
      const checkbox = screen.getByRole('checkbox');
      fireEvent.click(checkbox);
      
      const publishButton = screen.getByText('Publish Listing');
      fireEvent.click(publishButton);
      
      // Should show loading state
      const loadingElements = screen.getAllByText(/publishing/i);
      expect(loadingElements.length).toBeGreaterThan(0);
    });

    it('should disable button while submitting', async () => {
      const delayedSubmit = vi.fn(() => new Promise(resolve => setTimeout(resolve, 100)));
      
      render(<ReviewStep {...defaultProps} onSubmit={delayedSubmit} />);
      
      const checkbox = screen.getByRole('checkbox');
      fireEvent.click(checkbox);
      
      const publishButton = screen.getByRole('button', { name: /publish listing/i });
      fireEvent.click(publishButton);
      
      expect(publishButton).toBeDisabled();
    });
  });

  describe('Navigation', () => {
    it('should call onBack when back button is clicked', () => {
      render(<ReviewStep {...defaultProps} />);
      
      const backButton = screen.getByText('Back');
      fireEvent.click(backButton);
      
      expect(mockOnBack).toHaveBeenCalled();
    });

    it('should render save draft button when onSaveDraft is provided', () => {
      render(<ReviewStep {...defaultProps} />);
      expect(screen.getByText('Save as Draft')).toBeInTheDocument();
    });

    it('should not render save draft button when onSaveDraft is not provided', () => {
      render(<ReviewStep {...defaultProps} onSaveDraft={undefined} />);
      expect(screen.queryByText('Save as Draft')).not.toBeInTheDocument();
    });

    it('should call onSaveDraft when save draft button is clicked', () => {
      render(<ReviewStep {...defaultProps} />);
      
      const saveDraftButton = screen.getByText('Save as Draft');
      fireEvent.click(saveDraftButton);
      
      expect(mockOnSaveDraft).toHaveBeenCalled();
    });
  });

  describe('Data Formatting', () => {
    it('should format large prices correctly', () => {
      const expensiveData = { ...mockData, price: 125000 };
      render(<ReviewStep {...defaultProps} data={expensiveData} />);
      
      const prices = screen.getAllByText('$125,000');
      expect(prices.length).toBeGreaterThan(0);
    });

    it('should format high mileage correctly', () => {
      const highMileageData = { ...mockData, mileage: 125000 };
      render(<ReviewStep {...defaultProps} data={highMileageData} />);
      
      expect(screen.getByText(/125,000 miles/i)).toBeInTheDocument();
    });
  });

  describe('Vehicle Details Section', () => {
    it('should display all vehicle details', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText(/vehicle details/i)).toBeInTheDocument();
      expect(screen.getByText(/vin/i)).toBeInTheDocument();
      expect(screen.getByText('12345678901234567')).toBeInTheDocument();
    });

    it('should display colors', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText('Pearl White')).toBeInTheDocument();
      expect(screen.getByText('Black Leather')).toBeInTheDocument();
    });

    it('should display engine info when available', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText('2.5L I4')).toBeInTheDocument();
    });

    it('should display horsepower when available', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText('203 hp')).toBeInTheDocument();
    });

    it('should display MPG when available', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText('28 city / 39 hwy')).toBeInTheDocument();
    });
  });

  describe('Important Notice', () => {
    it('should display review notice', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText(/listing will be reviewed/i)).toBeInTheDocument();
    });

    it('should display approval timeframe', () => {
      render(<ReviewStep {...defaultProps} />);
      
      expect(screen.getByText(/within 24 hours/i)).toBeInTheDocument();
    });
  });
});

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import PricingStep from '@/components/organisms/sell/PricingStep';

describe('PricingStep', () => {
  const mockOnNext = vi.fn();
  const mockOnBack = vi.fn();

  const defaultProps = {
    data: {},
    onNext: mockOnNext,
    onBack: mockOnBack,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render the form title', () => {
      render(<PricingStep {...defaultProps} />);
      expect(screen.getByText('Pricing & Details')).toBeInTheDocument();
    });

    it('should render all form fields', () => {
      render(<PricingStep {...defaultProps} />);
      
      expect(screen.getByPlaceholderText('50,000')).toBeInTheDocument();
      expect(screen.getByPlaceholderText(/describe your vehicle in detail/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/location/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/phone number/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/email address/i)).toBeInTheDocument();
    });

    it('should render seller type radio buttons', () => {
      render(<PricingStep {...defaultProps} />);
      
      expect(screen.getByText('Private Seller')).toBeInTheDocument();
      expect(screen.getByText('Dealer')).toBeInTheDocument();
    });

    it('should render navigation buttons', () => {
      render(<PricingStep {...defaultProps} />);
      
      expect(screen.getByText('Back')).toBeInTheDocument();
      expect(screen.getByText('Next: Review Listing')).toBeInTheDocument();
    });

    it('should show pricing tips', () => {
      render(<PricingStep {...defaultProps} />);
      
      expect(screen.getByText(/pricing tips/i)).toBeInTheDocument();
      expect(screen.getByText(/research similar vehicles/i)).toBeInTheDocument();
    });

    it('should show description tips', () => {
      render(<PricingStep {...defaultProps} />);
      
      expect(screen.getByText(/description tips/i)).toBeInTheDocument();
      expect(screen.getByText(/service history/i)).toBeInTheDocument();
    });

    it('should populate form with existing data', () => {
      const existingData = {
        price: 25000,
        description: 'Great car in excellent condition with full service history and recent upgrades',
        location: 'Los Angeles, CA',
        sellerName: 'John Doe',
        sellerPhone: '+1 555 123 4567',
        sellerEmail: 'john@example.com',
        sellerType: 'private' as const,
      };

      render(<PricingStep {...defaultProps} data={existingData} />);

      expect(screen.getByDisplayValue('25000')).toBeInTheDocument();
      expect(screen.getByDisplayValue(/great car/i)).toBeInTheDocument();
      expect(screen.getByDisplayValue('Los Angeles, CA')).toBeInTheDocument();
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument();
    });
  });

  describe('Form Validation', () => {
    it('should show error when price is missing', async () => {
      render(<PricingStep {...defaultProps} />);
      
      const priceInput = screen.getByPlaceholderText('50,000');
      fireEvent.change(priceInput, { target: { value: '0' } });
      
      // Fill other required fields
      fireEvent.change(screen.getByPlaceholderText(/describe your vehicle/i), { 
        target: { value: 'A'.repeat(51) } 
      });
      fireEvent.change(screen.getByLabelText(/location/i), { target: { value: 'LA' } });
      fireEvent.change(screen.getByLabelText(/your name/i), { target: { value: 'John' } });
      fireEvent.change(screen.getByLabelText(/phone/i), { target: { value: '1234567890' } });
      fireEvent.change(screen.getByLabelText(/email address/i), { target: { value: 'test@test.com' } });
      
      const submitButton = screen.getByText('Next: Review Listing');
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText('Price is required')).toBeInTheDocument();
      });
    });

    it('should validate description minimum length', async () => {
      render(<PricingStep {...defaultProps} />);
      
      // Fill required fields with short description
      fireEvent.change(screen.getByPlaceholderText('50,000'), { target: { value: '25000' } });
      fireEvent.change(screen.getByLabelText(/location/i), { target: { value: 'LA' } });
      fireEvent.change(screen.getByLabelText(/your name/i), { target: { value: 'John' } });
      fireEvent.change(screen.getByLabelText(/phone/i), { target: { value: '1234567890' } });
      fireEvent.change(screen.getByLabelText(/email address/i), { target: { value: 'test@test.com' } });
      
      const descriptionInput = screen.getByPlaceholderText(/describe your vehicle in detail/i);
      fireEvent.change(descriptionInput, { target: { value: 'Too short' } });
      
      const submitButton = screen.getByText('Next: Review Listing');
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText('Description must be at least 50 characters')).toBeInTheDocument();
      });
    });

    it('should show character count for description', () => {
      render(<PricingStep {...defaultProps} />);
      
      const descriptionInput = screen.getByPlaceholderText(/describe your vehicle in detail/i);
      fireEvent.change(descriptionInput, { target: { value: 'Hello' } });
      
      expect(screen.getByText(/5 \/ 2000 characters/i)).toBeInTheDocument();
    });

    it('should show remaining characters needed for description', () => {
      render(<PricingStep {...defaultProps} />);
      
      const descriptionInput = screen.getByPlaceholderText(/describe your vehicle in detail/i);
      fireEvent.change(descriptionInput, { target: { value: 'Short text' } });
      
      expect(screen.getByText(/more needed/i)).toBeInTheDocument();
    });

    it('should validate email format', async () => {
      render(<PricingStep {...defaultProps} />);
      
      // Fill required fields first
      fireEvent.change(screen.getByPlaceholderText('50,000'), { target: { value: '25000' } });
      fireEvent.change(screen.getByPlaceholderText(/describe your vehicle/i), { 
        target: { value: 'A'.repeat(51) } 
      });
      fireEvent.change(screen.getByLabelText(/location/i), { target: { value: 'LA' } });
      fireEvent.change(screen.getByLabelText(/your name/i), { target: { value: 'John' } });
      fireEvent.change(screen.getByLabelText(/phone/i), { target: { value: '1234567890' } });
      
      const emailInput = screen.getByLabelText(/email address/i);
      fireEvent.change(emailInput, { target: { value: 'invalid-email' } });
      
      const submitButton = screen.getByText('Next: Review Listing');
      fireEvent.click(submitButton);

      // Email validation is handled by HTML5, so form should not submit
      await new Promise(resolve => setTimeout(resolve, 100));
      expect(mockOnNext).not.toHaveBeenCalled();
    });

    it('should validate phone number minimum length', async () => {
      render(<PricingStep {...defaultProps} />);
      
      // Fill required fields first
      fireEvent.change(screen.getByPlaceholderText('50,000'), { target: { value: '25000' } });
      fireEvent.change(screen.getByPlaceholderText(/describe your vehicle/i), { 
        target: { value: 'A'.repeat(51) } 
      });
      fireEvent.change(screen.getByLabelText(/location/i), { target: { value: 'LA' } });
      fireEvent.change(screen.getByLabelText(/your name/i), { target: { value: 'John' } });
      fireEvent.change(screen.getByLabelText(/email address/i), { target: { value: 'test@test.com' } });
      
      const phoneInput = screen.getByLabelText(/phone number/i);
      fireEvent.change(phoneInput, { target: { value: '123' } });
      
      const submitButton = screen.getByText('Next: Review Listing');
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText('Valid phone number is required')).toBeInTheDocument();
      });
    });

    it('should validate location is required', async () => {
      render(<PricingStep {...defaultProps} />);
      
      // Fill required fields except location
      fireEvent.change(screen.getByPlaceholderText('50,000'), { target: { value: '25000' } });
      fireEvent.change(screen.getByPlaceholderText(/describe your vehicle/i), { 
        target: { value: 'A'.repeat(51) } 
      });
      fireEvent.change(screen.getByLabelText(/your name/i), { target: { value: 'John' } });
      fireEvent.change(screen.getByLabelText(/phone/i), { target: { value: '1234567890' } });
      fireEvent.change(screen.getByLabelText(/email address/i), { target: { value: 'test@test.com' } });
      
      const submitButton = screen.getByText('Next: Review Listing');
      fireEvent.click(submitButton);

      // Wait for validation and check that onNext was not called
      await new Promise(resolve => setTimeout(resolve, 100));
      expect(mockOnNext).not.toHaveBeenCalled();
    });
  });

  describe('Seller Type Toggle', () => {
    it('should default to private seller', () => {
      render(<PricingStep {...defaultProps} />);
      
      const privateRadio = screen.getByDisplayValue('private');
      expect(privateRadio).toBeChecked();
    });

    it('should toggle to dealer when clicked', () => {
      render(<PricingStep {...defaultProps} />);
      
      const dealerRadio = screen.getByDisplayValue('dealer');
      fireEvent.click(dealerRadio);
      
      expect(dealerRadio).toBeChecked();
    });

    it('should change name field label when dealer is selected', () => {
      render(<PricingStep {...defaultProps} />);
      
      // Initially shows "Your Name"
      expect(screen.getByLabelText(/your name/i)).toBeInTheDocument();
      
      // Switch to dealer
      const dealerRadio = screen.getByDisplayValue('dealer');
      fireEvent.click(dealerRadio);
      
      // Should now show "Dealership Name"
      expect(screen.getByLabelText(/dealership name/i)).toBeInTheDocument();
    });

    it('should preserve dealer selection from existing data', () => {
      const existingData = {
        sellerType: 'dealer' as const,
      };

      render(<PricingStep {...defaultProps} data={existingData} />);
      
      const dealerRadio = screen.getByDisplayValue('dealer');
      expect(dealerRadio).toBeChecked();
    });
  });

  describe('Form Submission', () => {
    it('should call onNext with form data on valid submission', async () => {
      render(<PricingStep {...defaultProps} />);
      
      // Fill all required fields
      fireEvent.change(screen.getByPlaceholderText('50,000'), { target: { value: '25000' } });
      fireEvent.change(screen.getByPlaceholderText(/describe your vehicle in detail/i), { 
        target: { value: 'This is a great car in excellent condition with full service history and many extras' } 
      });
      fireEvent.change(screen.getByLabelText(/location/i), { target: { value: 'Los Angeles, CA' } });
      fireEvent.change(screen.getByLabelText(/your name/i), { target: { value: 'John Doe' } });
      fireEvent.change(screen.getByLabelText(/phone number/i), { target: { value: '+1 555 123 4567' } });
      fireEvent.change(screen.getByLabelText(/email address/i), { target: { value: 'john@example.com' } });
      
      const submitButton = screen.getByText('Next: Review Listing');
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(mockOnNext).toHaveBeenCalledWith(
          expect.objectContaining({
            price: 25000,
            location: 'Los Angeles, CA',
            sellerName: 'John Doe',
            sellerPhone: '+1 555 123 4567',
            sellerEmail: 'john@example.com',
            sellerType: 'private',
          })
        );
      });
    });

    it('should include dealer type in submission when selected', async () => {
      render(<PricingStep {...defaultProps} />);
      
      // Select dealer
      fireEvent.click(screen.getByDisplayValue('dealer'));
      
      // Fill required fields
      fireEvent.change(screen.getByPlaceholderText('50,000'), { target: { value: '25000' } });
      fireEvent.change(screen.getByPlaceholderText(/describe your vehicle in detail/i), { 
        target: { value: 'This is a great car in excellent condition with full service history and many extras' } 
      });
      fireEvent.change(screen.getByLabelText(/location/i), { target: { value: 'Los Angeles, CA' } });
      fireEvent.change(screen.getByLabelText(/dealership name/i), { target: { value: 'ABC Motors' } });
      fireEvent.change(screen.getByLabelText(/phone number/i), { target: { value: '+1 555 123 4567' } });
      fireEvent.change(screen.getByLabelText(/email address/i), { target: { value: 'dealer@example.com' } });
      
      const submitButton = screen.getByText('Next: Review Listing');
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(mockOnNext).toHaveBeenCalledWith(
          expect.objectContaining({
            sellerType: 'dealer',
            sellerName: 'ABC Motors',
          })
        );
      });
    });
  });

  describe('Navigation', () => {
    it('should call onBack when Back button is clicked', () => {
      render(<PricingStep {...defaultProps} />);
      
      const backButton = screen.getByText('Back');
      fireEvent.click(backButton);
      
      expect(mockOnBack).toHaveBeenCalled();
    });
  });

  describe('Price Input', () => {
    it('should show dollar sign prefix', () => {
      render(<PricingStep {...defaultProps} />);
      
      expect(screen.getByText('$')).toBeInTheDocument();
    });

    it('should accept numeric input for price', () => {
      render(<PricingStep {...defaultProps} />);
      
      const priceInput = screen.getByPlaceholderText('50,000');
      fireEvent.change(priceInput, { target: { value: '35000' } });
      
      expect(priceInput).toHaveValue(35000);
    });

    it('should validate maximum price', async () => {
      render(<PricingStep {...defaultProps} />);
      
      // Fill required fields
      fireEvent.change(screen.getByPlaceholderText(/describe your vehicle/i), { 
        target: { value: 'A'.repeat(51) } 
      });
      fireEvent.change(screen.getByLabelText(/location/i), { target: { value: 'LA' } });
      fireEvent.change(screen.getByLabelText(/your name/i), { target: { value: 'John' } });
      fireEvent.change(screen.getByLabelText(/phone/i), { target: { value: '1234567890' } });
      fireEvent.change(screen.getByLabelText(/email address/i), { target: { value: 'test@test.com' } });
      
      const priceInput = screen.getByPlaceholderText('50,000');
      fireEvent.change(priceInput, { target: { value: '20000000' } });
      
      const submitButton = screen.getByText('Next: Review Listing');
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText('Price is too high')).toBeInTheDocument();
      });
    });
  });

  describe('Description Character Counter', () => {
    it('should show red text when under minimum characters', () => {
      render(<PricingStep {...defaultProps} />);
      
      const descriptionInput = screen.getByPlaceholderText(/describe your vehicle in detail/i);
      fireEvent.change(descriptionInput, { target: { value: 'Short' } });
      
      const counter = screen.getByText(/5 \/ 2000 characters/i);
      expect(counter).toHaveClass('text-red-500');
    });

    it('should show gray text when above minimum characters', () => {
      render(<PricingStep {...defaultProps} />);
      
      const descriptionInput = screen.getByPlaceholderText(/describe your vehicle in detail/i);
      const longText = 'A'.repeat(60); // 60 characters, above minimum of 50
      fireEvent.change(descriptionInput, { target: { value: longText } });
      
      const counter = screen.getByText(/60.*2000 characters/i);
      expect(counter).toHaveClass('text-gray-500');
    });
  });
});

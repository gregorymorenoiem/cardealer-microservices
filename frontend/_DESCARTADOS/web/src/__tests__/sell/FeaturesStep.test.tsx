import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import FeaturesStep from '../../components/organisms/sell/FeaturesStep';

describe('FeaturesStep', () => {
  const mockOnNext = vi.fn();
  const mockOnBack = vi.fn();
  const defaultProps = {
    data: { features: [] },
    onNext: mockOnNext,
    onBack: mockOnBack,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render the component', () => {
      render(<FeaturesStep {...defaultProps} />);
      expect(screen.getByText(/Features & Options/i)).toBeInTheDocument();
    });

    it('should display all feature categories', () => {
      render(<FeaturesStep {...defaultProps} />);
      
      expect(screen.getByText(/🪑 Comfort/i)).toBeInTheDocument();
      expect(screen.getByText(/🎵 Entertainment/i)).toBeInTheDocument();
      expect(screen.getByText(/🛡️ Safety/i)).toBeInTheDocument();
      expect(screen.getByText(/🔧 Convenience/i)).toBeInTheDocument();
    });

    it('should display comfort features', () => {
      render(<FeaturesStep {...defaultProps} />);
      
      expect(screen.getByText('Leather Seats')).toBeInTheDocument();
      expect(screen.getByText('Heated Seats')).toBeInTheDocument();
      expect(screen.getByText('Ventilated Seats')).toBeInTheDocument();
    });

    it('should display entertainment features', () => {
      render(<FeaturesStep {...defaultProps} />);
      
      expect(screen.getByText('Premium Sound System')).toBeInTheDocument();
      expect(screen.getByText('Navigation System')).toBeInTheDocument();
      expect(screen.getByText('Apple CarPlay')).toBeInTheDocument();
    });

    it('should display safety features', () => {
      render(<FeaturesStep {...defaultProps} />);
      
      expect(screen.getByText('Backup Camera')).toBeInTheDocument();
      expect(screen.getByText('Blind Spot Monitor')).toBeInTheDocument();
      expect(screen.getByText('Lane Departure Warning')).toBeInTheDocument();
    });

    it('should display convenience features', () => {
      render(<FeaturesStep {...defaultProps} />);
      
      expect(screen.getByText('Keyless Entry')).toBeInTheDocument();
      expect(screen.getByText('Remote Start')).toBeInTheDocument();
      expect(screen.getByText('Power Liftgate')).toBeInTheDocument();
    });

    it('should render navigation buttons', () => {
      render(<FeaturesStep {...defaultProps} />);
      
      expect(screen.getByRole('button', { name: /Back/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /Next/i })).toBeInTheDocument();
    });
  });

  describe('Feature Selection', () => {
    it('should select a feature when checkbox is clicked', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const checkbox = screen.getByLabelText('Leather Seats');
      await user.click(checkbox);
      
      expect(checkbox).toBeChecked();
    });

    it('should deselect a feature when clicked again', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const checkbox = screen.getByLabelText('Leather Seats');
      await user.click(checkbox);
      expect(checkbox).toBeChecked();
      
      await user.click(checkbox);
      expect(checkbox).not.toBeChecked();
    });

    it('should update selected features count', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      expect(screen.getByText('0 features selected')).toBeInTheDocument();
      
      await user.click(screen.getByLabelText('Leather Seats'));
      expect(screen.getByText('1 feature selected')).toBeInTheDocument();
      
      await user.click(screen.getByLabelText('Heated Seats'));
      expect(screen.getByText('2 features selected')).toBeInTheDocument();
    });

    it('should load previously selected features', () => {
      const props = {
        ...defaultProps,
        data: { features: ['Leather Seats', 'GPS Navigation'] },
      };
      render(<FeaturesStep {...props} />);
      
      expect(screen.getByLabelText('Leather Seats')).toBeChecked();
      expect(screen.getByText('2 features selected')).toBeInTheDocument();
    });

    it('should select multiple features', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      await user.click(screen.getByLabelText('Leather Seats'));
      await user.click(screen.getByLabelText('Heated Seats'));
      await user.click(screen.getByLabelText('Premium Sound System'));
      
      expect(screen.getByText('3 features selected')).toBeInTheDocument();
    });
  });

  describe('Custom Features', () => {
    it('should display custom feature input', () => {
      render(<FeaturesStep {...defaultProps} />);
      
      expect(screen.getByPlaceholderText(/e.g., Tow Package/i)).toBeInTheDocument();
    });

    it('should add a custom feature', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const input = screen.getByPlaceholderText(/e.g., Tow Package/i);
      await user.type(input, 'Electric Sunroof');
      
      const addButton = screen.getByRole('button', { name: /Add/i });
      await user.click(addButton);
      
      const customFeatures = screen.getAllByText('Electric Sunroof');
      expect(customFeatures.length).toBeGreaterThan(0);
      expect(input).toHaveValue('');
    });

    it('should add custom feature on Enter key', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const input = screen.getByPlaceholderText(/e.g., Tow Package/i);
      await user.type(input, 'Electric Sunroof{Enter}');
      
      const customFeatures = screen.getAllByText('Electric Sunroof');
      expect(customFeatures.length).toBeGreaterThan(0);
    });

    it('should not add empty custom feature', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const addButton = screen.getByRole('button', { name: /Add/i });
      await user.click(addButton);
      
      // Should not have custom features if no features added
      expect(screen.queryByText('Electric Sunroof')).not.toBeInTheDocument();
    });

    it('should trim whitespace from custom features', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const input = screen.getByPlaceholderText(/e.g., Tow Package/i);
      await user.type(input, '  Electric Sunroof  ');
      
      const addButton = screen.getByRole('button', { name: /Add/i });
      await user.click(addButton);
      
      const customFeatures = screen.getAllByText('Electric Sunroof');
      expect(customFeatures.length).toBeGreaterThan(0);
    });

    it('should not add duplicate custom features', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const input = screen.getByPlaceholderText(/e.g., Tow Package/i);
      const addButton = screen.getByRole('button', { name: /Add/i });
      
      await user.type(input, 'Electric Sunroof');
      await user.click(addButton);
      
      // Count before adding duplicate
      const countBefore = screen.getAllByText('Electric Sunroof').length;
      
      await user.type(input, 'Electric Sunroof');
      await user.click(addButton);
      
      // Count should remain the same
      const countAfter = screen.getAllByText('Electric Sunroof').length;
      expect(countAfter).toBe(countBefore);
    });

    it('should remove custom feature', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const input = screen.getByPlaceholderText(/e.g., Tow Package/i);
      await user.type(input, 'Electric Sunroof');
      
      const addButton = screen.getByRole('button', { name: /Add/i });
      await user.click(addButton);
      
      // Find remove button with aria-label
      await waitFor(() => {
        const customFeatures = screen.getAllByText('Electric Sunroof');
        expect(customFeatures.length).toBeGreaterThan(0);
      });
      
      const removeButton = screen.getByLabelText('Remove Electric Sunroof');
      await user.click(removeButton);
      
      await waitFor(() => {
        const buttons = screen.queryAllByLabelText('Remove Electric Sunroof');
        expect(buttons.length).toBe(0);
      });
    });

    it('should include custom features in selected count', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      await user.click(screen.getByLabelText('Leather Seats'));
      expect(screen.getByText('1 feature selected')).toBeInTheDocument();
      
      const input = screen.getByPlaceholderText(/e.g., Tow Package/i);
      await user.type(input, 'Electric Sunroof');
      await user.click(screen.getByRole('button', { name: /Add/i }));
      
      expect(screen.getByText('2 features selected')).toBeInTheDocument();
    });
  });

  describe('Navigation', () => {
    it('should call onBack when Back button is clicked', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const backButton = screen.getByRole('button', { name: /Back/i });
      await user.click(backButton);
      
      expect(mockOnBack).toHaveBeenCalledTimes(1);
    });

    it('should call onNext with selected features', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      await user.click(screen.getByLabelText('Leather Seats'));
      await user.click(screen.getByLabelText('Heated Seats'));
      
      const nextButton = screen.getByRole('button', { name: /Next/i });
      await user.click(nextButton);
      
      expect(mockOnNext).toHaveBeenCalledWith({
        features: ['Leather Seats', 'Heated Seats'],
      });
    });

    it('should include custom features when calling onNext', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      await user.click(screen.getByLabelText('Leather Seats'));
      
      const input = screen.getByPlaceholderText(/e.g., Tow Package/i);
      await user.type(input, 'Electric Sunroof');
      await user.click(screen.getByRole('button', { name: /Add/i }));
      
      const nextButton = screen.getByRole('button', { name: /Next/i });
      await user.click(nextButton);
      
      expect(mockOnNext).toHaveBeenCalledWith({
        features: ['Leather Seats', 'Electric Sunroof'],
      });
    });

    it('should allow proceeding with no features selected', async () => {
      const user = userEvent.setup();
      render(<FeaturesStep {...defaultProps} />);
      
      const nextButton = screen.getByRole('button', { name: /Next/i });
      await user.click(nextButton);
      
      expect(mockOnNext).toHaveBeenCalledWith({ features: [] });
    });
  });

  describe('UI Interactions', () => {
    it('should have hover effect on feature labels', () => {
      render(<FeaturesStep {...defaultProps} />);
      
      const label = screen.getByText('Leather Seats').closest('label');
      expect(label).toHaveClass('cursor-pointer');
    });

    it('should display checkboxes for all features', () => {
      render(<FeaturesStep {...defaultProps} />);
      
      const checkboxes = screen.getAllByRole('checkbox');
      // 36 predefined features (8+8+10+10)
      expect(checkboxes.length).toBeGreaterThanOrEqual(36);
    });
  });
});

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import VehicleInfoStep from '@/components/organisms/sell/VehicleInfoStep';

describe('VehicleInfoStep', () => {
  const mockOnNext = vi.fn();
  const mockOnBack = vi.fn();

  const defaultProps = {
    data: {},
    onNext: mockOnNext,
    onBack: mockOnBack,
  };

  // Helper to get select by name
  const getSelectByName = (name: string) => {
    const selects = screen.getAllByRole('combobox');
    return selects.find(s => s.getAttribute('name') === name);
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render the form title', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      expect(screen.getByText('Vehicle Information')).toBeInTheDocument();
    });

    it('should render all required fields', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      // Use getSelectByName for select fields without proper label association
      expect(getSelectByName('make')).toBeInTheDocument();
      expect(screen.getByLabelText(/model/i)).toBeInTheDocument();
      expect(getSelectByName('year')).toBeInTheDocument();
      expect(screen.getByLabelText(/mileage/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/vin/i)).toBeInTheDocument();
      expect(getSelectByName('transmission')).toBeInTheDocument();
      expect(getSelectByName('fuelType')).toBeInTheDocument();
      expect(getSelectByName('bodyType')).toBeInTheDocument();
      expect(getSelectByName('drivetrain')).toBeInTheDocument();
      expect(screen.getByLabelText(/engine/i)).toBeInTheDocument();
    });

    it('should render color fields', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      expect(screen.getByLabelText(/exterior color/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/interior color/i)).toBeInTheDocument();
    });

    it('should render features section', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      expect(screen.getByText('Features')).toBeInTheDocument();
      expect(screen.getByText('Bluetooth')).toBeInTheDocument();
      expect(screen.getByText('Navigation System')).toBeInTheDocument();
      expect(screen.getByText('Leather Seats')).toBeInTheDocument();
    });

    it('should render submit button', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      expect(screen.getByText('Next: Upload Photos')).toBeInTheDocument();
    });

    it('should populate form with existing data', () => {
      const existingData = {
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
        exteriorColor: 'White',
        interiorColor: 'Black',
        condition: 'Used',
        features: ['Bluetooth', 'Backup Camera'],
      };

      render(<VehicleInfoStep {...defaultProps} data={existingData} />);

      expect(screen.getByDisplayValue('Toyota')).toBeInTheDocument();
      expect(screen.getByDisplayValue('Camry')).toBeInTheDocument();
      expect(screen.getByDisplayValue('15000')).toBeInTheDocument();
    });
  });

  describe('Form Validation', () => {
    it('should not submit empty form', async () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      const submitButton = screen.getByText('Next: Upload Photos');
      fireEvent.click(submitButton);

      await new Promise(resolve => setTimeout(resolve, 100));
      expect(mockOnNext).not.toHaveBeenCalled();
    });

    it('should not submit with invalid VIN length', async () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      const vinInput = screen.getByLabelText(/vin/i);
      fireEvent.change(vinInput, { target: { value: '123' } });
      
      const submitButton = screen.getByText('Next: Upload Photos');
      fireEvent.click(submitButton);

      await new Promise(resolve => setTimeout(resolve, 100));
      expect(mockOnNext).not.toHaveBeenCalled();
    });

    it('should not submit with negative mileage', async () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      const mileageInput = screen.getByLabelText(/mileage/i);
      fireEvent.change(mileageInput, { target: { value: '-100' } });
      
      const submitButton = screen.getByText('Next: Upload Photos');
      fireEvent.click(submitButton);

      await new Promise(resolve => setTimeout(resolve, 100));
      expect(mockOnNext).not.toHaveBeenCalled();
    });

    it('should not submit with future year', async () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      const yearSelect = getSelectByName('year');
      const futureYear = new Date().getFullYear() + 5;
      fireEvent.change(yearSelect!, { target: { value: futureYear.toString() } });
      
      const submitButton = screen.getByText('Next: Upload Photos');
      fireEvent.click(submitButton);

      await new Promise(resolve => setTimeout(resolve, 100));
      expect(mockOnNext).not.toHaveBeenCalled();
    });
  });

  describe('Features Selection', () => {
    it('should toggle feature selection', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      const bluetoothButton = screen.getByText('Bluetooth');
      
      // Initially not selected
      expect(bluetoothButton).not.toHaveClass('bg-primary');
      
      // Click to select
      fireEvent.click(bluetoothButton);
      expect(bluetoothButton).toHaveClass('bg-primary');
      
      // Click again to deselect
      fireEvent.click(bluetoothButton);
      expect(bluetoothButton).not.toHaveClass('bg-primary');
    });

    it('should allow multiple features to be selected', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      const bluetooth = screen.getByText('Bluetooth');
      const navigation = screen.getByText('Navigation System');
      const leather = screen.getByText('Leather Seats');
      
      fireEvent.click(bluetooth);
      fireEvent.click(navigation);
      fireEvent.click(leather);
      
      expect(bluetooth).toHaveClass('bg-primary');
      expect(navigation).toHaveClass('bg-primary');
      expect(leather).toHaveClass('bg-primary');
    });

    it('should preserve selected features from existing data', () => {
      const existingData = {
        features: ['Bluetooth', 'Backup Camera'],
      };

      render(<VehicleInfoStep {...defaultProps} data={existingData} />);
      
      const bluetooth = screen.getByText('Bluetooth');
      const backupCamera = screen.getByText('Backup Camera');
      
      expect(bluetooth).toHaveClass('bg-primary');
      expect(backupCamera).toHaveClass('bg-primary');
    });
  });

  describe('Form Submission', () => {
    it('should call onNext with form data on valid submission', async () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      // Fill required fields
      fireEvent.change(getSelectByName('make')!, { target: { value: 'Toyota' } });
      fireEvent.change(screen.getByLabelText(/model/i), { target: { value: 'Camry' } });
      fireEvent.change(getSelectByName('year')!, { target: { value: '2023' } });
      fireEvent.change(screen.getByLabelText(/mileage/i), { target: { value: '15000' } });
      fireEvent.change(screen.getByLabelText(/vin/i), { target: { value: '12345678901234567' } });
      fireEvent.change(getSelectByName('transmission')!, { target: { value: 'Automatic' } });
      fireEvent.change(getSelectByName('fuelType')!, { target: { value: 'Gasoline' } });
      fireEvent.change(getSelectByName('bodyType')!, { target: { value: 'Sedan' } });
      fireEvent.change(getSelectByName('drivetrain')!, { target: { value: 'FWD' } });
      fireEvent.change(screen.getByLabelText(/engine/i), { target: { value: '2.5L I4' } });
      fireEvent.change(screen.getByLabelText(/exterior color/i), { target: { value: 'White' } });
      fireEvent.change(screen.getByLabelText(/interior color/i), { target: { value: 'Black' } });
      fireEvent.change(getSelectByName('condition')!, { target: { value: 'Used' } });
      
      const submitButton = screen.getByText('Next: Upload Photos');
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(mockOnNext).toHaveBeenCalledWith(
          expect.objectContaining({
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
            exteriorColor: 'White',
            interiorColor: 'Black',
            condition: 'Used',
          })
        );
      });
    });

    it('should include selected features in submission', async () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      // Fill required fields (simplified for this test)
      fireEvent.change(getSelectByName('make')!, { target: { value: 'Toyota' } });
      fireEvent.change(screen.getByLabelText(/model/i), { target: { value: 'Camry' } });
      fireEvent.change(getSelectByName('year')!, { target: { value: '2023' } });
      fireEvent.change(screen.getByLabelText(/mileage/i), { target: { value: '15000' } });
      fireEvent.change(screen.getByLabelText(/vin/i), { target: { value: '12345678901234567' } });
      fireEvent.change(getSelectByName('transmission')!, { target: { value: 'Automatic' } });
      fireEvent.change(getSelectByName('fuelType')!, { target: { value: 'Gasoline' } });
      fireEvent.change(getSelectByName('bodyType')!, { target: { value: 'Sedan' } });
      fireEvent.change(getSelectByName('drivetrain')!, { target: { value: 'FWD' } });
      fireEvent.change(screen.getByLabelText(/engine/i), { target: { value: '2.5L I4' } });
      fireEvent.change(screen.getByLabelText(/exterior color/i), { target: { value: 'White' } });
      fireEvent.change(screen.getByLabelText(/interior color/i), { target: { value: 'Black' } });
      fireEvent.change(getSelectByName('condition')!, { target: { value: 'Used' } });
      
      // Select features
      fireEvent.click(screen.getByText('Bluetooth'));
      fireEvent.click(screen.getByText('Navigation System'));
      
      const submitButton = screen.getByText('Next: Upload Photos');
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(mockOnNext).toHaveBeenCalledWith(
          expect.objectContaining({
            features: expect.arrayContaining(['Bluetooth', 'Navigation System']),
          })
        );
      });
    });
  });

  describe('Select Options', () => {
    it('should have all makes available', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      expect(screen.getByText('Tesla')).toBeInTheDocument();
      expect(screen.getByText('Toyota')).toBeInTheDocument();
      expect(screen.getByText('BMW')).toBeInTheDocument();
    });

    it('should have transmission options', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      expect(screen.getByText('Automatic')).toBeInTheDocument();
      expect(screen.getByText('Manual')).toBeInTheDocument();
      expect(screen.getByText('CVT')).toBeInTheDocument();
    });

    it('should have fuel type options', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      expect(screen.getByText('Gasoline')).toBeInTheDocument();
      expect(screen.getByText('Diesel')).toBeInTheDocument();
      expect(screen.getByText('Electric')).toBeInTheDocument();
      expect(screen.getByText('Hybrid')).toBeInTheDocument();
    });

    it('should have body type options', () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      expect(screen.getByText('Sedan')).toBeInTheDocument();
      expect(screen.getByText('SUV')).toBeInTheDocument();
      expect(screen.getByText('Truck')).toBeInTheDocument();
      expect(screen.getByText('Coupe')).toBeInTheDocument();
    });
  });

  describe('Optional Fields', () => {
    it('should allow submission without optional horsepower field', async () => {
      render(<VehicleInfoStep {...defaultProps} />);
      
      // Fill only required fields using helper
      fireEvent.change(getSelectByName('make')!, { target: { value: 'Toyota' } });
      fireEvent.change(screen.getByLabelText(/model/i), { target: { value: 'Camry' } });
      fireEvent.change(getSelectByName('year')!, { target: { value: '2023' } });
      fireEvent.change(screen.getByLabelText(/mileage/i), { target: { value: '15000' } });
      fireEvent.change(screen.getByLabelText(/vin/i), { target: { value: '12345678901234567' } });
      fireEvent.change(getSelectByName('transmission')!, { target: { value: 'Automatic' } });
      fireEvent.change(getSelectByName('fuelType')!, { target: { value: 'Gasoline' } });
      fireEvent.change(getSelectByName('bodyType')!, { target: { value: 'Sedan' } });
      fireEvent.change(getSelectByName('drivetrain')!, { target: { value: 'FWD' } });
      fireEvent.change(screen.getByLabelText(/engine/i), { target: { value: '2.5L I4' } });
      fireEvent.change(screen.getByLabelText(/exterior color/i), { target: { value: 'White' } });
      fireEvent.change(screen.getByLabelText(/interior color/i), { target: { value: 'Black' } });
      fireEvent.change(getSelectByName('condition')!, { target: { value: 'Used' } });
      
      // Leave horsepower and mpg empty
      
      const submitButton = screen.getByText('Next: Upload Photos');
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(mockOnNext).toHaveBeenCalled();
      });
    });
  });
});

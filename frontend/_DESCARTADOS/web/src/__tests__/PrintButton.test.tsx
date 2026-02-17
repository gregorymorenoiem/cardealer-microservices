import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import PrintButton from '@/components/atoms/PrintButton';

// Mock window.print
const mockPrint = vi.fn();
window.print = mockPrint;

describe('PrintButton', () => {
  describe('Rendering', () => {
    it('should render print button', () => {
      render(<PrintButton />);
      expect(screen.getByText(/print/i)).toBeInTheDocument();
    });

    it('should have print icon', () => {
      render(<PrintButton />);
      const button = screen.getByRole('button', { name: /print vehicle details/i });
      expect(button).toBeInTheDocument();
    });

    it('should have proper accessibility attributes', () => {
      render(<PrintButton />);
      const button = screen.getByRole('button', { name: /print vehicle details/i });
      expect(button).toHaveAttribute('aria-label', 'Print vehicle details');
    });
  });

  describe('Functionality', () => {
    it('should call window.print when clicked', () => {
      render(<PrintButton />);
      
      const button = screen.getByText(/print/i);
      fireEvent.click(button);
      
      expect(mockPrint).toHaveBeenCalledTimes(1);
    });

    it('should have print:hidden class for print media', () => {
      render(<PrintButton />);
      
      const button = screen.getByText(/print/i);
      expect(button).toHaveClass('print:hidden');
    });
  });

  describe('Styling', () => {
    it('should have proper button styling', () => {
      render(<PrintButton />);
      
      const button = screen.getByText(/print/i);
      expect(button).toHaveClass('flex', 'items-center', 'gap-2');
      expect(button).toHaveClass('px-4', 'py-2');
      expect(button).toHaveClass('border-2', 'border-gray-300');
      expect(button).toHaveClass('rounded-lg');
    });

    it('should have hover effects', () => {
      render(<PrintButton />);
      
      const button = screen.getByText(/print/i);
      expect(button).toHaveClass('hover:border-primary', 'hover:text-primary');
    });
  });
});

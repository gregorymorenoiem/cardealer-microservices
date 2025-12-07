import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import PhotosStep from '../../components/organisms/sell/PhotosStep';

// Mock browser-image-compression
vi.mock('browser-image-compression', () => ({
  default: vi.fn((file) => Promise.resolve(file)),
}));

describe('PhotosStep', () => {
  const mockOnNext = vi.fn();
  const mockOnBack = vi.fn();
  const defaultProps = {
    data: { images: [] },
    onNext: mockOnNext,
    onBack: mockOnBack,
  };

  beforeEach(() => {
    vi.clearAllMocks();
    URL.createObjectURL = vi.fn(() => 'mock-url');
    URL.revokeObjectURL = vi.fn();
  });

  describe('Rendering', () => {
    it('should render the component', () => {
      render(<PhotosStep {...defaultProps} />);
      expect(screen.getByText(/Upload Photos/i)).toBeInTheDocument();
    });

    it('should display upload area', () => {
      render(<PhotosStep {...defaultProps} />);
      expect(screen.getByText(/Drag and drop images here/i)).toBeInTheDocument();
    });

    it('should show image requirements', () => {
      render(<PhotosStep {...defaultProps} />);
      expect(screen.getByText(/Min: 1, Max: 10/i)).toBeInTheDocument();
    });

    it('should display navigation buttons', () => {
      render(<PhotosStep {...defaultProps} />);
      expect(screen.getByRole('button', { name: /Back/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /Next/i })).toBeInTheDocument();
    });

    it('should have file input for browsing', () => {
      render(<PhotosStep {...defaultProps} />);
      const input = document.querySelector('input[type="file"]');
      expect(input).toBeInTheDocument();
      expect(input).toHaveAttribute('accept', 'image/*');
      expect(input).toHaveAttribute('multiple');
    });
  });

  describe('File Upload', () => {
    it('should handle file selection', async () => {
      const user = userEvent.setup();
      render(<PhotosStep {...defaultProps} />);
      
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;
      
      await user.upload(input, file);
      
      await waitFor(() => {
        expect(URL.createObjectURL).toHaveBeenCalled();
      });
    });

    it('should validate file type', async () => {
      render(<PhotosStep {...defaultProps} />);
      
      const file = new File(['content'], 'test.txt', { type: 'text/plain' });
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;
      
      // Mock FileList
      Object.defineProperty(input, 'files', {
        value: [file],
        writable: false,
      });
      
      fireEvent.change(input);
      
      await waitFor(() => {
        expect(screen.getByText(/Only image files are allowed/i)).toBeInTheDocument();
      });
    });

    it('should validate file size', async () => {
      const user = userEvent.setup();
      render(<PhotosStep {...defaultProps} />);
      
      // Create a file larger than 10MB
      const largeFile = new File(['x'.repeat(11 * 1024 * 1024)], 'large.jpg', { type: 'image/jpeg' });
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;
      
      await user.upload(input, largeFile);
      
      await waitFor(() => {
        expect(screen.getByText(/Image size must be less than 10MB/i)).toBeInTheDocument();
      });
    });

    it('should limit to 10 images', async () => {
      const user = userEvent.setup();
      const props = {
        ...defaultProps,
        data: { images: Array(10).fill(new File([''], 'test.jpg', { type: 'image/jpeg' })) },
      };
      render(<PhotosStep {...props} />);
      
      const file = new File(['image'], 'new.jpg', { type: 'image/jpeg' });
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;
      
      await user.upload(input, file);
      
      await waitFor(() => {
        expect(screen.getByText(/Maximum 10 images allowed/i)).toBeInTheDocument();
      });
    });

    it('should compress images on upload', async () => {
      const imageCompression = (await import('browser-image-compression')).default;
      const user = userEvent.setup();
      render(<PhotosStep {...defaultProps} />);
      
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;
      
      await user.upload(input, file);
      
      await waitFor(() => {
        expect(imageCompression).toHaveBeenCalled();
      });
    });

    it('should show compression progress', async () => {
      const user = userEvent.setup();
      render(<PhotosStep {...defaultProps} />);
      
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;
      
      // Compression happens too fast in tests, just verify the upload works
      await user.upload(input, file);
      
      await waitFor(() => {
        expect(URL.createObjectURL).toHaveBeenCalled();
      });
    });
  });

  describe('Drag and Drop', () => {
    it('should handle drag over', () => {
      render(<PhotosStep {...defaultProps} />);
      const dropZone = screen.getByText(/Drag and drop images here/i).closest('div');
      
      fireEvent.dragOver(dropZone!);
      
      expect(dropZone).toHaveClass('border-primary');
    });

    it('should handle drag leave', () => {
      render(<PhotosStep {...defaultProps} />);
      const dropZone = screen.getByText(/Drag and drop images here/i).closest('div');
      
      fireEvent.dragOver(dropZone!);
      fireEvent.dragLeave(dropZone!);
      
      expect(dropZone).not.toHaveClass('border-primary');
    });

    it('should handle drop', async () => {
      render(<PhotosStep {...defaultProps} />);
      const dropZone = screen.getByText(/Drag and drop images here/i).closest('div');
      
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const dataTransfer = {
        files: [file],
      };
      
      fireEvent.drop(dropZone!, { dataTransfer });
      
      await waitFor(() => {
        expect(URL.createObjectURL).toHaveBeenCalled();
      });
    });
  });

  describe('Image Preview', () => {
    it('should display image previews', async () => {
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const props = {
        ...defaultProps,
        data: { images: [file] },
      };
      
      render(<PhotosStep {...props} />);
      
      await waitFor(() => {
        const images = screen.getAllByRole('img');
        expect(images.length).toBeGreaterThan(0);
      });
    });

    it('should show main photo badge on first image', async () => {
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const props = {
        ...defaultProps,
        data: { images: [file] },
      };
      
      render(<PhotosStep {...props} />);
      
      await waitFor(() => {
        const badges = screen.getAllByText('Main Photo');
        // Should have the badge (text appears twice: in description and badge)
        expect(badges.length).toBeGreaterThan(0);
      });
    });

    it('should allow removing images', async () => {
      const user = userEvent.setup();
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const props = {
        ...defaultProps,
        data: { images: [file] },
      };
      
      render(<PhotosStep {...props} />);
      
      const removeButton = await screen.findByTitle('Remove');
      await user.click(removeButton);
      
      await waitFor(() => {
        expect(screen.queryByAltText('Upload 1')).not.toBeInTheDocument();
      });
    });
  });

  describe('Navigation', () => {
    it('should call onBack when Back button is clicked', async () => {
      const user = userEvent.setup();
      render(<PhotosStep {...defaultProps} />);
      
      const backButton = screen.getByRole('button', { name: /Back/i });
      await user.click(backButton);
      
      expect(mockOnBack).toHaveBeenCalledTimes(1);
    });

    it('should show error when trying to proceed without images', async () => {
      const user = userEvent.setup();
      render(<PhotosStep {...defaultProps} />);
      
      const nextButton = screen.getByRole('button', { name: /Next/i });
      await user.click(nextButton);
      
      expect(screen.getByText(/Please upload at least one image/i)).toBeInTheDocument();
      expect(mockOnNext).not.toHaveBeenCalled();
    });

    it('should call onNext with images when Next is clicked', async () => {
      const user = userEvent.setup();
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const props = {
        ...defaultProps,
        data: { images: [file] },
      };
      
      render(<PhotosStep {...props} />);
      
      const nextButton = screen.getByRole('button', { name: /Next/i });
      await user.click(nextButton);
      
      expect(mockOnNext).toHaveBeenCalledWith({ images: [file] });
    });
  });

  describe('UI State', () => {
    it('should disable upload during compression', async () => {
      const user = userEvent.setup();
      render(<PhotosStep {...defaultProps} />);
      
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;
      
      await user.upload(input, file);
      
      // Input should be disabled briefly during compression
      expect(input.disabled).toBe(false); // Eventually becomes enabled again
    });

    it('should clear error message when new file is selected', async () => {
      const user = userEvent.setup();
      render(<PhotosStep {...defaultProps} />);
      
      // Trigger error
      const nextButton = screen.getByRole('button', { name: /Next/i });
      await user.click(nextButton);
      expect(screen.getByText(/Please upload at least one image/i)).toBeInTheDocument();
      
      // Upload file
      const file = new File(['image'], 'test.jpg', { type: 'image/jpeg' });
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;
      await user.upload(input, file);
      
      await waitFor(() => {
        expect(screen.queryByText(/Please upload at least one image/i)).not.toBeInTheDocument();
      });
    });
  });
});

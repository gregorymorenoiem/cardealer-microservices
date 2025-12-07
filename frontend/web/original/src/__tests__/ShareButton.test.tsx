import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import ShareButton from '@/components/molecules/ShareButton';

// Mock window.open
const mockOpen = vi.fn();
window.open = mockOpen;

// Mock clipboard API
Object.assign(navigator, {
  clipboard: {
    writeText: vi.fn(() => Promise.resolve()),
  },
});

describe('ShareButton', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render share button', () => {
      render(<ShareButton title="Test Vehicle" />);
      expect(screen.getByText(/share/i)).toBeInTheDocument();
    });

    it('should not show modal initially', () => {
      render(<ShareButton title="Test Vehicle" />);
      expect(screen.queryByText(/share vehicle/i)).not.toBeInTheDocument();
    });
  });

  describe('Modal Interaction', () => {
    it('should open modal when button is clicked', () => {
      render(<ShareButton title="Test Vehicle" />);
      
      const shareButton = screen.getByText(/share/i);
      fireEvent.click(shareButton);
      
      expect(screen.getByText(/share vehicle/i)).toBeInTheDocument();
    });

    it('should close modal when X button is clicked', () => {
      render(<ShareButton title="Test Vehicle" />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      expect(screen.getByText(/share vehicle/i)).toBeInTheDocument();
      
      // Close modal
      const closeButton = screen.getByRole('button', { name: '' }); // X button has no text
      fireEvent.click(closeButton);
      
      waitFor(() => {
        expect(screen.queryByText(/share vehicle/i)).not.toBeInTheDocument();
      });
    });
  });

  describe('Share Options', () => {
    it('should display all share options', () => {
      render(<ShareButton title="Test Vehicle" />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      
      // Check for all share options
      expect(screen.getByText(/share on facebook/i)).toBeInTheDocument();
      expect(screen.getByText(/share on twitter/i)).toBeInTheDocument();
      expect(screen.getByText(/share on whatsapp/i)).toBeInTheDocument();
      expect(screen.getByText(/share on linkedin/i)).toBeInTheDocument();
      expect(screen.getByText(/share on email/i)).toBeInTheDocument();
    });

    it('should open Facebook share when clicked', () => {
      render(<ShareButton title="Test Vehicle" />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      
      // Click Facebook share
      const facebookButton = screen.getByText(/share on facebook/i);
      fireEvent.click(facebookButton);
      
      expect(mockOpen).toHaveBeenCalledWith(
        expect.stringContaining('facebook.com'),
        '_blank',
        'width=600,height=400'
      );
    });

    it('should open Twitter share when clicked', () => {
      render(<ShareButton title="Test Vehicle" />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      
      // Click Twitter share
      const twitterButton = screen.getByText(/share on twitter/i);
      fireEvent.click(twitterButton);
      
      expect(mockOpen).toHaveBeenCalledWith(
        expect.stringContaining('twitter.com'),
        '_blank',
        'width=600,height=400'
      );
    });

    it('should open WhatsApp share when clicked', () => {
      render(<ShareButton title="Test Vehicle" />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      
      // Click WhatsApp share
      const whatsappButton = screen.getByText(/share on whatsapp/i);
      fireEvent.click(whatsappButton);
      
      expect(mockOpen).toHaveBeenCalledWith(
        expect.stringContaining('wa.me'),
        '_blank',
        'width=600,height=400'
      );
    });
  });

  describe('Copy Link Functionality', () => {
    it('should display copy link input', () => {
      render(<ShareButton title="Test Vehicle" />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      
      expect(screen.getByText(/or copy link/i)).toBeInTheDocument();
      expect(screen.getByDisplayValue(window.location.href)).toBeInTheDocument();
    });

    it('should copy link to clipboard when copy button is clicked', async () => {
      render(<ShareButton title="Test Vehicle" />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      
      // Click copy button
      const copyButton = screen.getByText(/^copy$/i);
      fireEvent.click(copyButton);
      
      await waitFor(() => {
        expect(navigator.clipboard.writeText).toHaveBeenCalledWith(window.location.href);
      });
    });

    it('should show "Copied!" message after successful copy', async () => {
      render(<ShareButton title="Test Vehicle" />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      
      // Click copy button
      const copyButton = screen.getByText(/^copy$/i);
      fireEvent.click(copyButton);
      
      await waitFor(() => {
        expect(screen.getByText(/copied!/i)).toBeInTheDocument();
      });
    });

    it('should use custom URL if provided', () => {
      const customUrl = 'https://example.com/vehicle/123';
      render(<ShareButton title="Test Vehicle" url={customUrl} />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      
      expect(screen.getByDisplayValue(customUrl)).toBeInTheDocument();
    });

    it('should use custom description if provided', () => {
      const customDescription = 'Check out this amazing car!';
      render(<ShareButton title="Test Vehicle" description={customDescription} />);
      
      // Open modal
      fireEvent.click(screen.getByText(/share/i));
      
      // Click Twitter to verify description is used
      const twitterButton = screen.getByText(/share on twitter/i);
      fireEvent.click(twitterButton);
      
      expect(mockOpen).toHaveBeenCalledWith(
        expect.stringContaining(encodeURIComponent(customDescription)),
        '_blank',
        'width=600,height=400'
      );
    });
  });
});

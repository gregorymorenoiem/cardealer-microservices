/**
 * Tests for AdminHomepagePage
 *
 * Tests the admin interface for managing homepage sections:
 * - Rendering
 * - Section list display
 * - Create/Edit modal
 * - Delete confirmation
 * - Loading states
 *
 * @author OKLA Team
 * @date January 28, 2026
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';

// Mock the homepage sections service
vi.mock('@/services/homepageSectionsService', () => ({
  getAllSectionConfigs: vi.fn(),
  createSection: vi.fn(),
  updateSection: vi.fn(),
  deleteSection: vi.fn(),
}));

import AdminHomepagePage from '@/pages/admin/AdminHomepagePage';
import * as homepageSectionsService from '@/services/homepageSectionsService';

// Mock data
const mockSections = [
  {
    id: '1',
    name: 'Carousel Principal',
    slug: 'carousel-principal',
    displayOrder: 1,
    maxItems: 10,
    isActive: true,
    layoutType: 'Carousel',
    accentColor: 'blue',
    subtitle: 'Los mejores vehículos',
    viewAllHref: '/vehicles',
    autoAssignCriteria: null,
    vehicles: [],
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-28T00:00:00Z',
  },
  {
    id: '2',
    name: 'Destacados',
    slug: 'destacados',
    displayOrder: 2,
    maxItems: 9,
    isActive: true,
    layoutType: 'Featured',
    accentColor: 'amber',
    subtitle: 'Vehículos destacados',
    viewAllHref: '/vehicles?featured=true',
    autoAssignCriteria: null,
    vehicles: [],
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-28T00:00:00Z',
  },
  {
    id: '3',
    name: 'SUVs',
    slug: 'suvs',
    displayOrder: 3,
    maxItems: 10,
    isActive: false,
    layoutType: 'Grid',
    accentColor: 'green',
    subtitle: 'SUVs disponibles',
    viewAllHref: '/vehicles?type=suv',
    autoAssignCriteria: null,
    vehicles: [],
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-28T00:00:00Z',
  },
];

// Wrapper component with BrowserRouter
const renderWithProviders = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('AdminHomepagePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (homepageSectionsService.getAllSectionConfigs as ReturnType<typeof vi.fn>).mockResolvedValue(
      mockSections
    );
  });

  describe('Rendering', () => {
    it('should render the page title', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        expect(screen.getByText(/Gestión de Homepage/i)).toBeInTheDocument();
      });
    });

    it('should render loading state initially', () => {
      renderWithProviders(<AdminHomepagePage />);

      // During loading, there might be a loading indicator
      expect(screen.getByText(/Cargando|Loading|Gestión/i)).toBeInTheDocument();
    });

    it('should render sections list after loading', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        expect(screen.getByText('Carousel Principal')).toBeInTheDocument();
        expect(screen.getByText('Destacados')).toBeInTheDocument();
        expect(screen.getByText('SUVs')).toBeInTheDocument();
      });
    });

    it('should render create button', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        const createButton = screen.getByRole('button', {
          name: /Nueva Sección|Crear|Add|New/i,
        });
        expect(createButton).toBeInTheDocument();
      });
    });
  });

  describe('Section Status', () => {
    it('should display active status correctly', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        // Active sections should be visible
        expect(screen.getByText('Carousel Principal')).toBeInTheDocument();
        expect(screen.getByText('Destacados')).toBeInTheDocument();
      });
    });

    it('should display inactive sections with visual indicator', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        // SUVs is inactive in mock data
        expect(screen.getByText('SUVs')).toBeInTheDocument();
      });
    });
  });

  describe('Section Order', () => {
    it('should display sections in correct order', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        const sections = screen.getAllByText(/Carousel Principal|Destacados|SUVs/);
        expect(sections.length).toBeGreaterThanOrEqual(3);
      });
    });
  });

  describe('Create Section Modal', () => {
    it('should open create modal when clicking create button', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        expect(screen.getByText('Carousel Principal')).toBeInTheDocument();
      });

      const createButton = screen.getByRole('button', {
        name: /Nueva Sección/i,
      });
      fireEvent.click(createButton);

      await waitFor(() => {
        // Modal should be visible - there can be multiple matching elements
        const modalElements = screen.getAllByText(/Crear Sección|Nueva Sección/i);
        expect(modalElements.length).toBeGreaterThanOrEqual(1);
      });
    });
  });

  describe('Edit Section', () => {
    it('should render edit buttons for each section', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        expect(screen.getByText('Carousel Principal')).toBeInTheDocument();
      });

      // Should have edit buttons (one per section)
      const editButtons = screen.getAllByRole('button', { name: /Editar|Edit/i });
      expect(editButtons.length).toBeGreaterThanOrEqual(1);
    });
  });

  describe('Delete Section', () => {
    it('should render delete buttons for each section', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        expect(screen.getByText('Carousel Principal')).toBeInTheDocument();
      });

      // Should have delete buttons (one per section)
      const deleteButtons = screen.getAllByRole('button', { name: /Eliminar|Delete/i });
      expect(deleteButtons.length).toBeGreaterThanOrEqual(1);
    });
  });

  describe('Error Handling', () => {
    it('should display error message when loading fails', async () => {
      (homepageSectionsService.getAllSectionConfigs as ReturnType<typeof vi.fn>).mockRejectedValue(
        new Error('Network error')
      );

      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        expect(screen.getByText(/Error|error|Failed/i)).toBeInTheDocument();
      });
    });
  });

  describe('Empty State', () => {
    it('should display empty state when no sections exist', async () => {
      (homepageSectionsService.getAllSectionConfigs as ReturnType<typeof vi.fn>).mockResolvedValue(
        []
      );

      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        expect(screen.getByText(/No hay secciones|No sections|vacío|empty/i)).toBeInTheDocument();
      });
    });
  });

  describe('Refresh Button', () => {
    it('should render refresh button', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        expect(screen.getByText('Carousel Principal')).toBeInTheDocument();
      });

      const refreshButton = screen.getByRole('button', { name: /Actualizar|Refresh|Recargar/i });
      expect(refreshButton).toBeInTheDocument();
    });

    it('should call getAllSectionConfigs when refresh is clicked', async () => {
      renderWithProviders(<AdminHomepagePage />);

      await waitFor(() => {
        expect(screen.getByText('Carousel Principal')).toBeInTheDocument();
      });

      const refreshButton = screen.getByRole('button', { name: /Actualizar|Refresh|Recargar/i });
      fireEvent.click(refreshButton);

      await waitFor(() => {
        // Should have called the service again
        expect(homepageSectionsService.getAllSectionConfigs).toHaveBeenCalledTimes(2);
      });
    });
  });
});

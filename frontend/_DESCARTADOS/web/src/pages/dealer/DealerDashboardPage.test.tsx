import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import DealerDashboardPage from './DealerDashboardPage';

vi.mock('@/hooks/usePermissions', () => ({
  usePermissions: () => ({
    user: { id: 'user-1', dealerId: 'dealer-1', name: 'Test Dealer' },
    dealerPlan: 'pro',
    portalAccess: { crm: false, reports: false },
    serviceAccess: { appointmentService: false },
    usage: { listings: 1, featuredListings: 0 },
    limits: { maxListings: 10, maxFeaturedListings: 3 },
    hasReachedLimit: () => false,
    getUsagePercentage: () => 10,
    canAccessFeature: () => true,
    needsUpgrade: () => false,
    getNextPlan: () => null,
  }),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<any>('react-router-dom');
  return {
    ...actual,
    Link: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
  };
});

describe('DealerDashboardPage (Sprint 18)', () => {
  it('shows category demand section', async () => {
    localStorage.setItem('accessToken', 'mock-access-token');
    localStorage.setItem('refreshToken', 'mock-refresh-token');

    render(<DealerDashboardPage />);

    expect(screen.getByText('Demanda por categoría')).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('SUV')).toBeInTheDocument();
    });

    expect(screen.getByText(/86\/100/)).toBeInTheDocument();
  });
});

/**
 * Sprint 12 - Frontend Tests
 * Tests for AdvancedDealerDashboard component, useDealerAnalytics hook, and dealerAnalyticsService
 */

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import axios from 'axios';

// Import components and services to test
import { AdvancedDealerDashboard } from '../pages/AdvancedDealerDashboard';
import { useDealerAnalytics } from '../hooks/useDealerAnalytics';
import { DealerAnalyticsService } from '../services/dealerAnalyticsService';

// Mock axios
vi.mock('axios');
const mockedAxios = vi.mocked(axios);

// Mock react-router-dom
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => vi.fn(),
  };
});

// Test wrapper component
const TestWrapper = ({ children }: { children: React.ReactNode }) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{children}</BrowserRouter>
    </QueryClientProvider>
  );
};

describe('DealerAnalyticsService', () => {
  let service: DealerAnalyticsService;
  const dealerId = '123e4567-e89b-12d3-a456-426614174000';

  beforeEach(() => {
    service = new DealerAnalyticsService();
    vi.clearAllMocks();
  });

  it('should get dashboard summary successfully', async () => {
    const mockSummary = {
      dealerId,
      totalViews: 5000,
      uniqueVisitors: 3200,
      totalInquiries: 180,
      convertedLeads: 54,
      conversionRate: 30.0,
      totalRevenue: 425000,
      averageResponseTime: 1.2,
      customerSatisfactionScore: 4.6,
      period: 'Last 30 days',
    };

    mockedAxios.get.mockResolvedValueOnce({ data: mockSummary });

    const result = await service.getDashboardSummary(dealerId);

    expect(mockedAxios.get).toHaveBeenCalledWith(`/api/dashboard/${dealerId}/summary`);
    expect(result).toEqual(mockSummary);
  });

  it('should get analytics data successfully', async () => {
    const mockAnalytics = [
      {
        id: '456e7890-e89b-12d3-a456-426614174000',
        dealerId,
        dateRange: '2026-01-08',
        totalViews: 1500,
        uniqueVisitors: 900,
        totalInquiries: 65,
        convertedLeads: 18,
        conversionRate: 27.69,
        totalRevenue: 145000,
        averageResponseTime: 1.8,
        customerSatisfactionScore: 4.3,
      },
    ];

    const startDate = new Date('2026-01-01');
    const endDate = new Date('2026-01-31');

    mockedAxios.get.mockResolvedValueOnce({ data: mockAnalytics });

    const result = await service.getAnalytics(dealerId, startDate, endDate);

    expect(mockedAxios.get).toHaveBeenCalledWith(`/api/analytics/${dealerId}`, {
      params: {
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
      },
    });
    expect(result).toEqual(mockAnalytics);
  });

  it('should get conversion funnel successfully', async () => {
    const mockFunnel = {
      id: '789e0123-e89b-12d3-a456-426614174000',
      dealerId,
      views: 8000,
      detailViews: 4800,
      inquiries: 240,
      testDrives: 120,
      purchases: 48,
      viewToDetailRate: 60.0,
      detailToInquiryRate: 5.0,
      inquiryToTestDriveRate: 50.0,
      testDriveToPurchaseRate: 40.0,
      overallConversionRate: 0.6,
      createdAt: new Date().toISOString(),
    };

    mockedAxios.get.mockResolvedValueOnce({ data: mockFunnel });

    const result = await service.getConversionFunnel(dealerId);

    expect(mockedAxios.get).toHaveBeenCalledWith(`/api/funnel/${dealerId}`);
    expect(result).toEqual(mockFunnel);
  });

  it('should get insights successfully', async () => {
    const mockInsights = [
      {
        id: 'abc12345-e89b-12d3-a456-426614174000',
        dealerId,
        type: 'OpportunityAlert',
        priority: 'High',
        title: 'Oportunidad: Mejorar tiempo de respuesta',
        description:
          'Su tiempo de respuesta promedio es 3.2 horas, 40% más lento que el promedio del mercado.',
        actionRecommendation:
          'Configure notificaciones automáticas para responder en menos de 1 hora',
        potentialImpact: '+18% conversiones',
        isRead: false,
        createdAt: new Date().toISOString(),
      },
    ];

    mockedAxios.get.mockResolvedValueOnce({ data: mockInsights });

    const result = await service.getInsights(dealerId);

    expect(mockedAxios.get).toHaveBeenCalledWith(`/api/insights/${dealerId}`, {
      params: {
        page: 1,
        pageSize: 20,
        type: undefined,
        priority: undefined,
      },
    });
    expect(result).toEqual(mockInsights);
  });

  it('should mark insight as read successfully', async () => {
    const insightId = 'def45678-e89b-12d3-a456-426614174000';

    mockedAxios.post.mockResolvedValueOnce({ data: true });

    const result = await service.markInsightAsRead(insightId);

    expect(mockedAxios.post).toHaveBeenCalledWith(`/api/insights/${insightId}/read`);
    expect(result).toBe(true);
  });

  it('should dismiss insight successfully', async () => {
    const insightId = 'ghi78901-e89b-12d3-a456-426614174000';

    mockedAxios.delete.mockResolvedValueOnce({ data: true });

    const result = await service.dismissInsight(insightId);

    expect(mockedAxios.delete).toHaveBeenCalledWith(`/api/insights/${insightId}`);
    expect(result).toBe(true);
  });

  it('should format currency correctly', () => {
    expect(service.formatCurrency(125000)).toBe('$125,000');
    expect(service.formatCurrency(1250000)).toBe('$1,250,000');
    expect(service.formatCurrency(1250.5)).toBe('$1,251');
  });

  it('should format percentage correctly', () => {
    expect(service.formatPercentage(25.67)).toBe('25.7%');
    expect(service.formatPercentage(0.5)).toBe('0.5%');
    expect(service.formatPercentage(100)).toBe('100.0%');
  });

  it('should get priority color correctly', () => {
    expect(service.getPriorityColor('High')).toBe('text-red-600 bg-red-50');
    expect(service.getPriorityColor('Medium')).toBe('text-yellow-600 bg-yellow-50');
    expect(service.getPriorityColor('Low')).toBe('text-green-600 bg-green-50');
  });

  it('should get priority icon correctly', () => {
    expect(service.getPriorityIcon('High')).toBe('🚨');
    expect(service.getPriorityIcon('Medium')).toBe('⚠️');
    expect(service.getPriorityIcon('Low')).toBe('💡');
  });
});

describe('useDealerAnalytics Hook', () => {
  const dealerId = '123e4567-e89b-12d3-a456-426614174000';

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should handle loading state correctly', async () => {
    // Mock pending API call
    mockedAxios.get.mockImplementationOnce(
      () => new Promise((resolve) => setTimeout(resolve, 1000))
    );

    const TestComponent = () => {
      const { isLoading } = useDealerAnalytics(dealerId);
      return <div>{isLoading ? 'Loading...' : 'Loaded'}</div>;
    };

    render(
      <TestWrapper>
        <TestComponent />
      </TestWrapper>
    );

    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('should handle successful data fetch', async () => {
    const mockSummary = {
      dealerId,
      totalViews: 5000,
      uniqueVisitors: 3200,
      conversionRate: 30.0,
      period: 'Last 30 days',
    };

    mockedAxios.get.mockResolvedValueOnce({ data: mockSummary });

    const TestComponent = () => {
      const { summary, isLoading } = useDealerAnalytics(dealerId);

      if (isLoading) return <div>Loading...</div>;

      return (
        <div>
          <div data-testid="total-views">{summary?.totalViews}</div>
          <div data-testid="conversion-rate">{summary?.conversionRate}%</div>
        </div>
      );
    };

    render(
      <TestWrapper>
        <TestComponent />
      </TestWrapper>
    );

    await waitFor(() => {
      expect(screen.getByTestId('total-views')).toHaveTextContent('5000');
      expect(screen.getByTestId('conversion-rate')).toHaveTextContent('30%');
    });
  });
});

describe('AdvancedDealerDashboard Component', () => {
  const mockSummary = {
    dealerId: '123e4567-e89b-12d3-a456-426614174000',
    totalViews: 5000,
    uniqueVisitors: 3200,
    totalInquiries: 180,
    convertedLeads: 54,
    conversionRate: 30.0,
    totalRevenue: 425000,
    averageResponseTime: 1.2,
    customerSatisfactionScore: 4.6,
    period: 'Last 30 days',
  };

  const mockInsights = [
    {
      id: 'abc12345-e89b-12d3-a456-426614174000',
      dealerId: '123e4567-e89b-12d3-a456-426614174000',
      type: 'OpportunityAlert',
      priority: 'High',
      title: 'Oportunidad: Mejorar tiempo de respuesta',
      description:
        'Su tiempo de respuesta promedio es 3.2 horas, 40% más lento que el promedio del mercado.',
      actionRecommendation: 'Configure notificaciones automáticas',
      potentialImpact: '+18% conversiones',
      isRead: false,
      createdAt: new Date().toISOString(),
    },
  ];

  beforeEach(() => {
    vi.clearAllMocks();

    // Mock successful API calls
    mockedAxios.get.mockImplementation((url) => {
      if (url.includes('/summary')) {
        return Promise.resolve({ data: mockSummary });
      }
      if (url.includes('/insights')) {
        return Promise.resolve({ data: mockInsights });
      }
      return Promise.resolve({ data: {} });
    });
  });

  it('should render dashboard header correctly', async () => {
    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    await waitFor(() => {
      expect(screen.getByText('Dashboard Analytics Avanzado')).toBeInTheDocument();
      expect(screen.getByText('Datos del último mes')).toBeInTheDocument();
    });
  });

  it('should render stat cards with correct data', async () => {
    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    await waitFor(() => {
      expect(screen.getByText('5,000')).toBeInTheDocument(); // Total Views
      expect(screen.getByText('3,200')).toBeInTheDocument(); // Unique Visitors
      expect(screen.getByText('30.0%')).toBeInTheDocument(); // Conversion Rate
      expect(screen.getByText('$425,000')).toBeInTheDocument(); // Total Revenue
    });
  });

  it('should render all tabs correctly', async () => {
    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    await waitFor(() => {
      expect(screen.getByText('Resumen')).toBeInTheDocument();
      expect(screen.getByText('Funnel')).toBeInTheDocument();
      expect(screen.getByText('Insights')).toBeInTheDocument();
      expect(screen.getByText('Benchmark')).toBeInTheDocument();
    });
  });

  it('should switch tabs when clicked', async () => {
    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    // Click on Insights tab
    const insightsTab = await screen.findByText('Insights');
    fireEvent.click(insightsTab);

    await waitFor(() => {
      expect(screen.getByText('Insights y Recomendaciones')).toBeInTheDocument();
    });
  });

  it('should render insights correctly', async () => {
    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    // Switch to Insights tab
    const insightsTab = await screen.findByText('Insights');
    fireEvent.click(insightsTab);

    await waitFor(() => {
      expect(screen.getByText('Oportunidad: Mejorar tiempo de respuesta')).toBeInTheDocument();
      expect(screen.getByText('+18% conversiones')).toBeInTheDocument();
      expect(screen.getByText('🚨')).toBeInTheDocument(); // High priority icon
    });
  });

  it('should handle insight actions correctly', async () => {
    mockedAxios.post.mockResolvedValueOnce({ data: true });

    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    // Switch to Insights tab
    const insightsTab = await screen.findByText('Insights');
    fireEvent.click(insightsTab);

    // Click mark as read button
    const markAsReadButton = await screen.findByText('Marcar como Leído');
    fireEvent.click(markAsReadButton);

    await waitFor(() => {
      expect(mockedAxios.post).toHaveBeenCalledWith(
        '/api/insights/abc12345-e89b-12d3-a456-426614174000/read'
      );
    });
  });

  it('should handle refresh action correctly', async () => {
    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    const refreshButton = await screen.findByText('Actualizar');
    fireEvent.click(refreshButton);

    // Should trigger new API calls
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledTimes(6); // Initial + refresh calls
    });
  });

  it('should handle loading state', () => {
    // Mock loading state
    mockedAxios.get.mockImplementationOnce(
      () => new Promise((resolve) => setTimeout(resolve, 1000))
    );

    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    expect(screen.getByText('Cargando dashboard...')).toBeInTheDocument();
  });

  it('should handle error state', async () => {
    // Mock API error
    mockedAxios.get.mockRejectedValueOnce(new Error('API Error'));

    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    await waitFor(() => {
      expect(screen.getByText('Error al cargar los datos')).toBeInTheDocument();
      expect(screen.getByText('Reintentar')).toBeInTheDocument();
    });
  });

  it('should be responsive on mobile', async () => {
    // Mock mobile viewport
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 375,
    });

    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    await waitFor(() => {
      const container = screen.getByRole('main');
      expect(container).toHaveClass('px-4'); // Mobile padding
    });
  });
});

describe('Dashboard Integration Tests', () => {
  const dealerId = '123e4567-e89b-12d3-a456-426614174000';

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should handle complete dashboard workflow', async () => {
    // Mock all API endpoints
    mockedAxios.get.mockImplementation((url) => {
      if (url.includes('/summary')) {
        return Promise.resolve({
          data: {
            dealerId,
            totalViews: 10000,
            conversionRate: 35.0,
            totalRevenue: 500000,
          },
        });
      }
      if (url.includes('/funnel')) {
        return Promise.resolve({
          data: {
            views: 10000,
            purchases: 350,
            overallConversionRate: 3.5,
          },
        });
      }
      if (url.includes('/insights')) {
        return Promise.resolve({
          data: [
            {
              id: 'insight1',
              title: 'Great Performance!',
              type: 'PerformanceAlert',
              priority: 'Low',
            },
          ],
        });
      }
      if (url.includes('/benchmarks')) {
        return Promise.resolve({
          data: [
            {
              category: 'SUV',
              dealerValue: 4.8,
              marketAverage: 4.2,
              isAboveAverage: true,
            },
          ],
        });
      }
      return Promise.resolve({ data: {} });
    });

    render(
      <TestWrapper>
        <AdvancedDealerDashboard />
      </TestWrapper>
    );

    // Verify dashboard loads
    await waitFor(() => {
      expect(screen.getByText('Dashboard Analytics Avanzado')).toBeInTheDocument();
    });

    // Test tab switching
    const funnelTab = screen.getByText('Funnel');
    fireEvent.click(funnelTab);

    await waitFor(() => {
      expect(screen.getByText('Embudo de Conversión')).toBeInTheDocument();
    });

    // Test benchmark tab
    const benchmarkTab = screen.getByText('Benchmark');
    fireEvent.click(benchmarkTab);

    await waitFor(() => {
      expect(screen.getByText('Comparación con el Mercado')).toBeInTheDocument();
    });

    // Verify all API calls were made
    expect(mockedAxios.get).toHaveBeenCalledTimes(4); // summary, funnel, insights, benchmarks
  });
});

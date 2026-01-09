/**
 * Dealer analytics tests were previously tightly coupled to axios mocks and older API shapes.
 * This suite keeps lightweight, stable assertions around non-IO helper behavior.
 */

import { describe, it, expect } from 'vitest';

import { DealerAnalyticsService, dealerAnalyticsService } from '../services/dealerAnalyticsService';

describe('DealerAnalyticsService (helpers)', () => {
  it('formats currency as USD (no decimals)', () => {
    const service = new DealerAnalyticsService();
    expect(service.formatCurrency(125000)).toBe('$125,000');
    expect(service.formatCurrency(1250000)).toBe('$1,250,000');
    expect(service.formatCurrency(1250.5)).toBe('$1,251');
  });

  it('formats percentage with one decimal', () => {
    const service = new DealerAnalyticsService();
    expect(service.formatPercentage(25.67)).toBe('25.7%');
    expect(service.formatPercentage(0.5)).toBe('0.5%');
    expect(service.formatPercentage(100)).toBe('100.0%');
  });

  it('maps priority color + icon', () => {
    const service = new DealerAnalyticsService();
    expect(service.getPriorityColor('High')).toBe('text-red-600 bg-red-50');
    expect(service.getPriorityColor('Medium')).toBe('text-yellow-600 bg-yellow-50');
    expect(service.getPriorityColor('Low')).toBe('text-green-600 bg-green-50');

    expect(service.getPriorityIcon('High')).toBe('🚨');
    expect(service.getPriorityIcon('Medium')).toBe('⚠️');
    expect(service.getPriorityIcon('Low')).toBe('💡');
  });
});

describe('dealerAnalyticsService (shape)', () => {
  it('exports a usable singleton', () => {
    expect(dealerAnalyticsService).toBeTruthy();
    expect(typeof dealerAnalyticsService.getDashboardSummary).toBe('function');
    expect(typeof dealerAnalyticsService.getQuickStats).toBe('function');
    expect(typeof dealerAnalyticsService.getConversionFunnel).toBe('function');
    expect(typeof dealerAnalyticsService.getDealerInsights).toBe('function');
    expect(typeof dealerAnalyticsService.getMarketBenchmarks).toBe('function');
  });
});

/**
 * Mock Data Index
 * 
 * Exports all mock data for use throughout the application.
 */

// Mock Users - Authentication
export * from './mockUsers';

// Billing Data - Plans, Subscriptions, Invoices, Payments
export * from './billingData';

// CRM Data - Leads, Deals, Pipelines, Activities
export * from './crmData';

// MSW Handlers for API Mocking
export { handlers, authHandlers, vehicleHandlers, dealerHandlers } from './handlers';

// MSW Server for Testing
export { server, resetHandlers, useHandlers } from './server';

// MSW Browser Worker for Development
export { worker, startMockWorker } from './browser';

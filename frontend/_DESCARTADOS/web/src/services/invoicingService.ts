/**
 * Invoicing Service - API Client
 *
 * Servicio de facturación electrónica con NCF para OKLA
 * Cumplimiento DGII - Ley 253-12, Norma 06-2018
 */

import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'https://api.okla.com.do';

// ============================================================================
// TYPES & ENUMS
// ============================================================================

export enum InvoiceType {
  ConsumidorFinal = 1, // B01 - Factura de Consumo
  CreditoFiscal = 2, // B02 - Factura de Crédito Fiscal
  NotaDebito = 3, // B03 - Nota de Débito
  NotaCredito = 4, // B04 - Nota de Crédito
  Gubernamental = 14, // B14 - Gubernamental
  RegimenEspecial = 15, // B15 - Régimen Especial
}

export enum InvoiceStatus {
  Draft = 'Draft',
  Issued = 'Issued',
  Sent = 'Sent',
  Paid = 'Paid',
  PartiallyPaid = 'PartiallyPaid',
  Overdue = 'Overdue',
  Voided = 'Voided',
  Cancelled = 'Cancelled',
}

export interface InvoiceItem {
  id: string;
  description: string;
  productCode?: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  taxRate: number; // 0.18 = 18%
  taxAmount: number;
  total: number;
}

export interface Invoice {
  id: string;
  invoiceNumber: string; // OKLA-2026-00001
  ncf: string; // B0100000001
  type: InvoiceType;
  status: InvoiceStatus;

  // Referencias
  paymentId?: string;
  subscriptionId?: string;
  orderId?: string;

  // Emisor (OKLA)
  issuerId: string;
  issuerRnc: string;
  issuerName: string;
  issuerAddress: string;

  // Receptor (Cliente)
  customerId?: string;
  customerRnc?: string;
  customerName: string;
  customerEmail: string;
  customerAddress?: string;
  customerPhone?: string;

  // Montos
  currency: string;
  subtotal: number;
  taxAmount: number; // ITBIS 18%
  discountAmount: number;
  total: number;

  // Items
  items: InvoiceItem[];

  // Fechas
  issueDate: string;
  dueDate?: string;
  paidAt?: string;

  // PDF
  pdfUrl?: string;

  // DGII
  reportedToDgii: boolean;
  reportedAt?: string;

  // Timestamps
  createdAt: string;
  voidedAt?: string;
  voidReason?: string;
}

export interface CreditNote {
  id: string;
  creditNoteNumber: string;
  ncf: string; // B0400000001
  originalInvoiceId: string;
  originalNcf: string;
  reason: string;
  amount: number;
  taxAmount: number;
  total: number;
  status: 'Draft' | 'Issued' | 'Applied';
  createdAt: string;
}

export interface NCFSequence {
  id: string;
  type: InvoiceType;
  prefix: string; // B01, B02, etc.
  currentNumber: number;
  startNumber: number;
  endNumber: number;
  authorizationDate: string;
  expirationDate: string;
  isActive: boolean;
  usagePercentage: number;
}

export interface DGIIReport {
  id: string;
  type: '606' | '607';
  period: string; // 202601
  status: 'Pending' | 'Generated' | 'Submitted';
  recordCount: number;
  totalAmount: number;
  totalTax: number;
  fileUrl?: string;
  generatedAt?: string;
  submittedAt?: string;
}

export interface RNCValidation {
  rnc: string;
  isValid: boolean;
  name?: string;
  status?: 'Active' | 'Inactive' | 'Suspended';
  taxRegime?: string;
}

// ============================================================================
// API CLIENT
// ============================================================================

const apiClient = axios.create({
  baseURL: `${API_URL}/api/invoices`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token interceptor
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ============================================================================
// INVOICES
// ============================================================================

export interface GetInvoicesParams {
  page?: number;
  pageSize?: number;
  status?: InvoiceStatus;
  type?: InvoiceType;
  startDate?: string;
  endDate?: string;
  search?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export async function getInvoices(
  params: GetInvoicesParams = {}
): Promise<PaginatedResponse<Invoice>> {
  const response = await apiClient.get<PaginatedResponse<Invoice>>('/', { params });
  return response.data;
}

export async function getInvoice(id: string): Promise<Invoice> {
  const response = await apiClient.get<Invoice>(`/${id}`);
  return response.data;
}

export async function getInvoiceByNCF(ncf: string): Promise<Invoice> {
  const response = await apiClient.get<Invoice>(`/ncf/${ncf}`);
  return response.data;
}

export async function downloadInvoicePDF(id: string): Promise<Blob> {
  const response = await apiClient.get(`/${id}/pdf`, {
    responseType: 'blob',
  });
  return response.data;
}

export async function sendInvoiceByEmail(id: string, email?: string): Promise<void> {
  await apiClient.post(`/${id}/send`, { email });
}

export async function voidInvoice(id: string, reason: string): Promise<Invoice> {
  const response = await apiClient.post<Invoice>(`/${id}/void`, { reason });
  return response.data;
}

// ============================================================================
// CREDIT NOTES
// ============================================================================

export async function getCreditNotes(
  params: { page?: number; pageSize?: number } = {}
): Promise<PaginatedResponse<CreditNote>> {
  const response = await apiClient.get<PaginatedResponse<CreditNote>>('/credit-notes', {
    params,
  });
  return response.data;
}

export async function getCreditNote(id: string): Promise<CreditNote> {
  const response = await apiClient.get<CreditNote>(`/credit-notes/${id}`);
  return response.data;
}

export async function createCreditNote(
  originalInvoiceId: string,
  reason: string,
  amount?: number // Partial credit note
): Promise<CreditNote> {
  const response = await apiClient.post<CreditNote>('/credit-notes', {
    originalInvoiceId,
    reason,
    amount,
  });
  return response.data;
}

// ============================================================================
// NCF SEQUENCES (Admin)
// ============================================================================

export async function getNCFSequences(): Promise<NCFSequence[]> {
  const response = await apiClient.get<NCFSequence[]>('/ncf-sequences');
  return response.data;
}

export async function createNCFSequence(
  type: InvoiceType,
  startNumber: number,
  endNumber: number,
  expirationDate: string
): Promise<NCFSequence> {
  const response = await apiClient.post<NCFSequence>('/ncf-sequences', {
    type,
    startNumber,
    endNumber,
    expirationDate,
  });
  return response.data;
}

export async function activateNCFSequence(id: string): Promise<NCFSequence> {
  const response = await apiClient.post<NCFSequence>(`/ncf-sequences/${id}/activate`);
  return response.data;
}

// ============================================================================
// DGII REPORTS (Admin)
// ============================================================================

export async function getDGIIReports(
  params: { year?: number; month?: number } = {}
): Promise<DGIIReport[]> {
  const response = await apiClient.get<DGIIReport[]>('/dgii/reports', { params });
  return response.data;
}

export async function generateDGIIReport(
  type: '606' | '607',
  year: number,
  month: number
): Promise<DGIIReport> {
  const response = await apiClient.post<DGIIReport>('/dgii/reports', {
    type,
    year,
    month,
  });
  return response.data;
}

export async function downloadDGIIReport(id: string): Promise<Blob> {
  const response = await apiClient.get(`/dgii/reports/${id}/download`, {
    responseType: 'blob',
  });
  return response.data;
}

export async function submitDGIIReport(id: string): Promise<DGIIReport> {
  const response = await apiClient.post<DGIIReport>(`/dgii/reports/${id}/submit`);
  return response.data;
}

// ============================================================================
// RNC VALIDATION
// ============================================================================

export async function validateRNC(rnc: string): Promise<RNCValidation> {
  const response = await apiClient.get<RNCValidation>(`/dgii/validate-rnc/${rnc}`);
  return response.data;
}

export async function validateNCF(ncf: string): Promise<{ isValid: boolean; message?: string }> {
  const response = await apiClient.get(`/dgii/validate-ncf/${ncf}`);
  return response.data;
}

// ============================================================================
// HELPERS
// ============================================================================

export function getInvoiceTypeLabel(type: InvoiceType): string {
  const labels: Record<InvoiceType, string> = {
    [InvoiceType.ConsumidorFinal]: 'B01 - Consumidor Final',
    [InvoiceType.CreditoFiscal]: 'B02 - Crédito Fiscal',
    [InvoiceType.NotaDebito]: 'B03 - Nota de Débito',
    [InvoiceType.NotaCredito]: 'B04 - Nota de Crédito',
    [InvoiceType.Gubernamental]: 'B14 - Gubernamental',
    [InvoiceType.RegimenEspecial]: 'B15 - Régimen Especial',
  };
  return labels[type] || `Tipo ${type}`;
}

export function getInvoiceStatusLabel(status: InvoiceStatus): string {
  const labels: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: 'Borrador',
    [InvoiceStatus.Issued]: 'Emitida',
    [InvoiceStatus.Sent]: 'Enviada',
    [InvoiceStatus.Paid]: 'Pagada',
    [InvoiceStatus.PartiallyPaid]: 'Pago Parcial',
    [InvoiceStatus.Overdue]: 'Vencida',
    [InvoiceStatus.Voided]: 'Anulada',
    [InvoiceStatus.Cancelled]: 'Cancelada',
  };
  return labels[status] || status;
}

export function getInvoiceStatusColor(status: InvoiceStatus): string {
  const colors: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: 'gray',
    [InvoiceStatus.Issued]: 'blue',
    [InvoiceStatus.Sent]: 'indigo',
    [InvoiceStatus.Paid]: 'green',
    [InvoiceStatus.PartiallyPaid]: 'yellow',
    [InvoiceStatus.Overdue]: 'red',
    [InvoiceStatus.Voided]: 'gray',
    [InvoiceStatus.Cancelled]: 'red',
  };
  return colors[status] || 'gray';
}

export function formatNCF(ncf: string): string {
  // Format: B0100000001 → B01-00000001
  if (ncf.length === 11) {
    return `${ncf.slice(0, 3)}-${ncf.slice(3)}`;
  }
  return ncf;
}

export function getNCFPrefix(type: InvoiceType): string {
  const prefixes: Record<InvoiceType, string> = {
    [InvoiceType.ConsumidorFinal]: 'B01',
    [InvoiceType.CreditoFiscal]: 'B02',
    [InvoiceType.NotaDebito]: 'B03',
    [InvoiceType.NotaCredito]: 'B04',
    [InvoiceType.Gubernamental]: 'B14',
    [InvoiceType.RegimenEspecial]: 'B15',
  };
  return prefixes[type] || 'B01';
}

export function formatCurrency(amount: number, currency = 'DOP'): string {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
  }).format(amount);
}

export default {
  // Invoices
  getInvoices,
  getInvoice,
  getInvoiceByNCF,
  downloadInvoicePDF,
  sendInvoiceByEmail,
  voidInvoice,
  // Credit Notes
  getCreditNotes,
  getCreditNote,
  createCreditNote,
  // NCF Sequences
  getNCFSequences,
  createNCFSequence,
  activateNCFSequence,
  // DGII Reports
  getDGIIReports,
  generateDGIIReport,
  downloadDGIIReport,
  submitDGIIReport,
  // Validation
  validateRNC,
  validateNCF,
  // Helpers
  getInvoiceTypeLabel,
  getInvoiceStatusLabel,
  getInvoiceStatusColor,
  formatNCF,
  getNCFPrefix,
  formatCurrency,
};

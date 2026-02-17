/**
 * Invoice Hooks - React Query
 *
 * Hooks para facturas, notas de crédito y reportes DGII
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import invoicingService, {
  Invoice,
  InvoiceStatus,
  InvoiceType,
  CreditNote,
  NCFSequence,
  DGIIReport,
  RNCValidation,
  GetInvoicesParams,
  PaginatedResponse,
} from '@/services/invoicingService';

// ============================================================================
// QUERY KEYS
// ============================================================================

export const invoiceKeys = {
  all: ['invoices'] as const,
  lists: () => [...invoiceKeys.all, 'list'] as const,
  list: (params: GetInvoicesParams) => [...invoiceKeys.lists(), params] as const,
  details: () => [...invoiceKeys.all, 'detail'] as const,
  detail: (id: string) => [...invoiceKeys.details(), id] as const,
  byNcf: (ncf: string) => [...invoiceKeys.all, 'ncf', ncf] as const,
  creditNotes: () => [...invoiceKeys.all, 'credit-notes'] as const,
  creditNote: (id: string) => [...invoiceKeys.creditNotes(), id] as const,
  ncfSequences: () => [...invoiceKeys.all, 'ncf-sequences'] as const,
  dgiiReports: (params?: { year?: number; month?: number }) =>
    [...invoiceKeys.all, 'dgii-reports', params] as const,
};

// ============================================================================
// INVOICES
// ============================================================================

/**
 * Get invoices list with filters
 */
export function useInvoices(params: GetInvoicesParams = {}) {
  return useQuery<PaginatedResponse<Invoice>, Error>({
    queryKey: invoiceKeys.list(params),
    queryFn: () => invoicingService.getInvoices(params),
  });
}

/**
 * Get single invoice by ID
 */
export function useInvoice(id: string) {
  return useQuery<Invoice, Error>({
    queryKey: invoiceKeys.detail(id),
    queryFn: () => invoicingService.getInvoice(id),
    enabled: !!id,
  });
}

/**
 * Get invoice by NCF
 */
export function useInvoiceByNCF(ncf: string) {
  return useQuery<Invoice, Error>({
    queryKey: invoiceKeys.byNcf(ncf),
    queryFn: () => invoicingService.getInvoiceByNCF(ncf),
    enabled: !!ncf,
  });
}

/**
 * Download invoice PDF
 */
export function useDownloadInvoicePDF() {
  return useMutation<Blob, Error, string>({
    mutationFn: invoicingService.downloadInvoicePDF,
    onSuccess: (blob, id) => {
      // Create download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `factura-${id}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    },
  });
}

/**
 * Send invoice by email
 */
export function useSendInvoiceByEmail() {
  const queryClient = useQueryClient();

  return useMutation<void, Error, { id: string; email?: string }>({
    mutationFn: ({ id, email }) => invoicingService.sendInvoiceByEmail(id, email),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.detail(id) });
    },
  });
}

/**
 * Void invoice (Admin)
 */
export function useVoidInvoice() {
  const queryClient = useQueryClient();

  return useMutation<Invoice, Error, { id: string; reason: string }>({
    mutationFn: ({ id, reason }) => invoicingService.voidInvoice(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.lists() });
    },
  });
}

// ============================================================================
// CREDIT NOTES
// ============================================================================

/**
 * Get credit notes list
 */
export function useCreditNotes(page = 1, pageSize = 20) {
  return useQuery<PaginatedResponse<CreditNote>, Error>({
    queryKey: invoiceKeys.creditNotes(),
    queryFn: () => invoicingService.getCreditNotes({ page, pageSize }),
  });
}

/**
 * Get single credit note
 */
export function useCreditNote(id: string) {
  return useQuery<CreditNote, Error>({
    queryKey: invoiceKeys.creditNote(id),
    queryFn: () => invoicingService.getCreditNote(id),
    enabled: !!id,
  });
}

/**
 * Create credit note (Admin)
 */
export function useCreateCreditNote() {
  const queryClient = useQueryClient();

  return useMutation<
    CreditNote,
    Error,
    { originalInvoiceId: string; reason: string; amount?: number }
  >({
    mutationFn: ({ originalInvoiceId, reason, amount }) =>
      invoicingService.createCreditNote(originalInvoiceId, reason, amount),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.creditNotes() });
      queryClient.invalidateQueries({ queryKey: invoiceKeys.lists() });
    },
  });
}

// ============================================================================
// NCF SEQUENCES (Admin)
// ============================================================================

/**
 * Get NCF sequences
 */
export function useNCFSequences() {
  return useQuery<NCFSequence[], Error>({
    queryKey: invoiceKeys.ncfSequences(),
    queryFn: invoicingService.getNCFSequences,
  });
}

/**
 * Create NCF sequence (Admin)
 */
export function useCreateNCFSequence() {
  const queryClient = useQueryClient();

  return useMutation<
    NCFSequence,
    Error,
    {
      type: InvoiceType;
      startNumber: number;
      endNumber: number;
      expirationDate: string;
    }
  >({
    mutationFn: ({ type, startNumber, endNumber, expirationDate }) =>
      invoicingService.createNCFSequence(type, startNumber, endNumber, expirationDate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.ncfSequences() });
    },
  });
}

/**
 * Activate NCF sequence (Admin)
 */
export function useActivateNCFSequence() {
  const queryClient = useQueryClient();

  return useMutation<NCFSequence, Error, string>({
    mutationFn: invoicingService.activateNCFSequence,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.ncfSequences() });
    },
  });
}

// ============================================================================
// DGII REPORTS (Admin)
// ============================================================================

/**
 * Get DGII reports
 */
export function useDGIIReports(year?: number, month?: number) {
  return useQuery<DGIIReport[], Error>({
    queryKey: invoiceKeys.dgiiReports({ year, month }),
    queryFn: () => invoicingService.getDGIIReports({ year, month }),
  });
}

/**
 * Generate DGII report (Admin)
 */
export function useGenerateDGIIReport() {
  const queryClient = useQueryClient();

  return useMutation<DGIIReport, Error, { type: '606' | '607'; year: number; month: number }>({
    mutationFn: ({ type, year, month }) => invoicingService.generateDGIIReport(type, year, month),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.dgiiReports() });
    },
  });
}

/**
 * Download DGII report
 */
export function useDownloadDGIIReport() {
  return useMutation<Blob, Error, string>({
    mutationFn: invoicingService.downloadDGIIReport,
    onSuccess: (blob, id) => {
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `dgii-report-${id}.txt`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    },
  });
}

/**
 * Submit DGII report (Admin)
 */
export function useSubmitDGIIReport() {
  const queryClient = useQueryClient();

  return useMutation<DGIIReport, Error, string>({
    mutationFn: invoicingService.submitDGIIReport,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: invoiceKeys.dgiiReports() });
    },
  });
}

// ============================================================================
// VALIDATION
// ============================================================================

/**
 * Validate RNC
 */
export function useValidateRNC(rnc: string) {
  return useQuery<RNCValidation, Error>({
    queryKey: ['validate-rnc', rnc],
    queryFn: () => invoicingService.validateRNC(rnc),
    enabled: rnc.length >= 9,
  });
}

/**
 * Validate NCF
 */
export function useValidateNCF(ncf: string) {
  return useQuery<{ isValid: boolean; message?: string }, Error>({
    queryKey: ['validate-ncf', ncf],
    queryFn: () => invoicingService.validateNCF(ncf),
    enabled: ncf.length === 11,
  });
}

// ============================================================================
// HELPERS
// ============================================================================

/**
 * Get invoice type label
 */
export function useInvoiceTypeLabel(type: InvoiceType) {
  return invoicingService.getInvoiceTypeLabel(type);
}

/**
 * Get invoice status label
 */
export function useInvoiceStatusLabel(status: InvoiceStatus) {
  return invoicingService.getInvoiceStatusLabel(status);
}

/**
 * Get invoice status color
 */
export function useInvoiceStatusColor(status: InvoiceStatus) {
  return invoicingService.getInvoiceStatusColor(status);
}

/**
 * Format NCF
 */
export function useFormatNCF(ncf: string) {
  return invoicingService.formatNCF(ncf);
}

/**
 * Format currency
 */
export function useFormatCurrency(amount: number, currency = 'DOP') {
  return invoicingService.formatCurrency(amount, currency);
}

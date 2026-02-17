// Interfaces TypeScript que mapean DTOs del backend
export interface ReviewDto {
  id: string;
  sellerId: string;
  buyerId: string;
  vehicleId: string;
  orderId?: string;
  rating: number;
  title: string;
  comment: string;
  isApproved: boolean;
  isVerifiedPurchase: boolean;
  sellerResponse?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateReviewDto {
  sellerId: string;
  vehicleId: string;
  orderId?: string;
  rating: number;
  title: string;
  comment: string;
}

export interface ReviewResponseDto {
  id: string;
  reviewId: string;
  sellerId: string;
  responseText: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ReviewSummaryDto {
  id: string;
  sellerId: string;
  totalReviews: number;
  averageRating: number;
  fiveStarReviews: number;
  fourStarReviews: number;
  threeStarReviews: number;
  twoStarReviews: number;
  oneStarReviews: number;
  lastReviewDate?: string;
  positivePercentage: number;
  verifiedPurchaseReviews: number;
}

export interface RatingDistribution {
  [rating: number]: number;
}

export interface SellerReviewsResponse {
  reviews: ReviewDto[];
  summary: ReviewSummaryDto;
  totalCount: number;
  currentPage: number;
  totalPages: number;
}

export interface CreateReviewRequest {
  sellerId: string;
  vehicleId: string;
  orderId?: string;
  rating: number;
  title: string;
  comment: string;
}

export type ReviewSortBy = 'date_desc' | 'date_asc' | 'rating_desc' | 'rating_asc' | 'helpful';

// ========================================
// SPRINT 15 - DTOs AVANZADOS
// ========================================

export interface VoteResultDto {
  success: boolean;
  message: string;
  totalHelpfulVotes: number;
  totalVotes: number;
  helpfulPercentage: number;
  userVotedHelpful: boolean;
}

export interface VoteStatsDto {
  reviewId: string;
  helpfulVotes: number;
  totalVotes: number;
  helpfulPercentage: number;
  currentUserVotedHelpful: boolean | null;
}

export interface SellerBadgeDto {
  id: string;
  sellerId: string;
  badgeType: string;
  title: string;
  description: string;
  icon: string;
  color: string;
  isActive: boolean;
  earnedAt: string;
  expiresAt?: string;
}

export interface BadgeUpdateResultDto {
  sellerId: string;
  badgesAdded: SellerBadgeDto[];
  badgesRemoved: string[];
  totalActiveBadges: number;
}

export interface ReviewRequestDto {
  id: string;
  buyerId: string;
  sellerId: string;
  transactionId: string;
  vehicleId?: string;
  status: ReviewRequestStatus;
  sentAt: string;
  expiresAt: string;
  completedAt?: string;
  reviewId?: string;
}

export type ReviewRequestStatus = 'Sent' | 'Viewed' | 'Completed' | 'Expired' | 'Cancelled';

export interface GetReviewsRequest {
  sellerId: string;
  vehicleId?: string;
  page?: number;
  pageSize?: number;
  sortBy?: ReviewSortBy;
  minRating?: number;
  maxRating?: number;
  verifiedOnly?: boolean;
}

/**
 * Servicio para interactuar con el ReviewService API
 * Maneja todas las operaciones CRUD de reviews y resúmenes
 */
export class ReviewService {
  private baseUrl: string;

  constructor() {
    // Usar URL del environment o fallback a localhost
    this.baseUrl =
      process.env.NODE_ENV === 'production' ? 'https://api.okla.com.do' : 'http://localhost:18443';
  }

  private getAuthHeaders(): Record<string, string> {
    const token = localStorage.getItem('jwt_token');
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const error = await response.text();
      throw new Error(`HTTP ${response.status}: ${error}`);
    }
    return response.json();
  }

  /**
   * Obtener reviews de un vendedor con paginación
   */
  async getSellerReviews(
    sellerId: string,
    page: number = 1,
    pageSize: number = 10,
    onlyApproved: boolean = true
  ): Promise<SellerReviewsResponse> {
    const params = new URLSearchParams({
      sellerId,
      page: page.toString(),
      pageSize: pageSize.toString(),
      onlyApproved: onlyApproved.toString(),
    });

    const response = await fetch(`${this.baseUrl}/api/reviews/seller?${params}`, {
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<SellerReviewsResponse>(response);
  }

  /**
   * Obtener una review específica por ID
   */
  async getReviewById(reviewId: string): Promise<ReviewDto> {
    const response = await fetch(`${this.baseUrl}/api/reviews/${reviewId}`, {
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<ReviewDto>(response);
  }

  /**
   * Obtener resumen de reviews de un vendedor
   */
  async getSellerSummary(sellerId: string): Promise<ReviewSummaryDto> {
    const response = await fetch(`${this.baseUrl}/api/reviews/seller/${sellerId}/summary`, {
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<ReviewSummaryDto>(response);
  }

  /**
   * Crear una nueva review
   * Requiere autenticación
   */
  async createReview(request: CreateReviewRequest): Promise<ReviewDto> {
    const response = await fetch(`${this.baseUrl}/api/reviews`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(request),
    });

    return this.handleResponse<ReviewDto>(response);
  }

  /**
   * Actualizar una review existente
   * Solo el autor puede actualizar su review
   */
  async updateReview(reviewId: string, request: Partial<CreateReviewRequest>): Promise<ReviewDto> {
    const response = await fetch(`${this.baseUrl}/api/reviews/${reviewId}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(request),
    });

    return this.handleResponse<ReviewDto>(response);
  }

  /**
   * Eliminar una review
   * Solo el autor o admin pueden eliminar
   */
  async deleteReview(reviewId: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/api/reviews/${reviewId}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(`HTTP ${response.status}: ${error}`);
    }
  }

  /**
   * Aprobar/rechazar una review (solo admins)
   */
  async moderateReview(
    reviewId: string,
    isApproved: boolean,
    moderatorNote?: string
  ): Promise<ReviewDto> {
    const response = await fetch(`${this.baseUrl}/api/reviews/${reviewId}/moderate`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({
        isApproved,
        moderatorNote,
      }),
    });

    return this.handleResponse<ReviewDto>(response);
  }

  // ========================================
  // SPRINT 15 - FUNCIONALIDADES AVANZADAS
  // ========================================

  /**
   * Votar si una review fue útil o no
   * Requiere autenticación
   */
  async voteReview(reviewId: string, isHelpful: boolean): Promise<VoteResultDto> {
    const response = await fetch(`${this.baseUrl}/api/reviews/${reviewId}/vote`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ isHelpful }),
    });

    return this.handleResponse<VoteResultDto>(response);
  }

  /**
   * Obtener estadísticas de votos de una review
   */
  async getVoteStats(reviewId: string): Promise<VoteStatsDto> {
    const response = await fetch(`${this.baseUrl}/api/reviews/${reviewId}/vote-stats`, {
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<VoteStatsDto>(response);
  }

  /**
   * Obtener badges de un vendedor
   */
  async getSellerBadges(sellerId: string): Promise<SellerBadgeDto[]> {
    const response = await fetch(`${this.baseUrl}/api/reviews/seller/${sellerId}/badges`, {
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<SellerBadgeDto[]>(response);
  }

  /**
   * Recalcular badges de un vendedor (solo admins)
   */
  async recalculateBadges(sellerId: string): Promise<BadgeUpdateResultDto> {
    const response = await fetch(
      `${this.baseUrl}/api/reviews/seller/${sellerId}/badges/recalculate`,
      {
        method: 'POST',
        headers: this.getAuthHeaders(),
      }
    );

    return this.handleResponse<BadgeUpdateResultDto>(response);
  }

  /**
   * Enviar solicitud de review post-compra (admin/system)
   */
  async sendReviewRequest(
    buyerId: string,
    sellerId: string,
    transactionId: string
  ): Promise<ReviewRequestDto> {
    const response = await fetch(`${this.baseUrl}/api/reviews/requests`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ buyerId, sellerId, transactionId }),
    });

    return this.handleResponse<ReviewRequestDto>(response);
  }

  /**
   * Obtener reviews pendientes para un comprador
   * Requiere autenticación
   */
  async getPendingReviewRequests(buyerId: string): Promise<ReviewRequestDto[]> {
    const response = await fetch(`${this.baseUrl}/api/reviews/requests/buyer/${buyerId}`, {
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<ReviewRequestDto[]>(response);
  }

  /**
   * Obtener mis reviews pendientes
   * Requiere autenticación
   */
  async getMyPendingRequests(): Promise<ReviewRequestDto[]> {
    const response = await fetch(`${this.baseUrl}/api/reviews/requests/mine`, {
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<ReviewRequestDto[]>(response);
  }

  /**
   * Responder a una review como vendedor
   * Requiere autenticación como vendedor
   */
  async respondToReview(reviewId: string, responseText: string): Promise<ReviewDto> {
    const response = await fetch(`${this.baseUrl}/api/reviews/${reviewId}/respond`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ responseText }),
    });

    return this.handleResponse<ReviewDto>(response);
  }

  // ========================================
  // MÉTODOS DE UTILIDAD
  // ========================================

  /**
   * Calcular distribución de ratings de un resumen
   */
  getRatingDistribution(summary: ReviewSummaryDto): RatingDistribution {
    return {
      5: summary.fiveStarReviews,
      4: summary.fourStarReviews,
      3: summary.threeStarReviews,
      2: summary.twoStarReviews,
      1: summary.oneStarReviews,
    };
  }

  /**
   * Formatear rating con estrellas
   */
  formatRating(rating: number): string {
    return (
      '★'.repeat(Math.floor(rating)) +
      (rating % 1 !== 0 ? '☆' : '') +
      '☆'.repeat(5 - Math.ceil(rating))
    );
  }

  /**
   * Obtener color basado en rating
   */
  getRatingColor(rating: number): string {
    if (rating >= 4.5) return 'text-green-600';
    if (rating >= 3.5) return 'text-yellow-600';
    if (rating >= 2.5) return 'text-orange-600';
    return 'text-red-600';
  }

  /**
   * Calcular tiempo transcurrido desde la review
   */
  getTimeAgo(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMs = now.getTime() - date.getTime();
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));

    if (diffInDays === 0) {
      const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
      if (diffInHours === 0) {
        const diffInMinutes = Math.floor(diffInMs / (1000 * 60));
        return `hace ${diffInMinutes} min${diffInMinutes !== 1 ? 's' : ''}`;
      }
      return `hace ${diffInHours} hora${diffInHours !== 1 ? 's' : ''}`;
    } else if (diffInDays < 30) {
      return `hace ${diffInDays} día${diffInDays !== 1 ? 's' : ''}`;
    } else if (diffInDays < 365) {
      const diffInMonths = Math.floor(diffInDays / 30);
      return `hace ${diffInMonths} mes${diffInMonths !== 1 ? 'es' : ''}`;
    } else {
      const diffInYears = Math.floor(diffInDays / 365);
      return `hace ${diffInYears} año${diffInYears !== 1 ? 's' : ''}`;
    }
  }

  /**
   * Validar datos de review antes de enviar
   */
  validateReviewData(data: CreateReviewRequest): string[] {
    const errors: string[] = [];

    if (!data.sellerId) {
      errors.push('ID del vendedor es requerido');
    }

    if (!data.vehicleId) {
      errors.push('ID del vehículo es requerido');
    }

    if (!data.rating || data.rating < 1 || data.rating > 5) {
      errors.push('Rating debe ser entre 1 y 5 estrellas');
    }

    if (!data.title || data.title.trim().length < 5) {
      errors.push('Título debe tener al menos 5 caracteres');
    }

    if (!data.comment || data.comment.trim().length < 10) {
      errors.push('Comentario debe tener al menos 10 caracteres');
    }

    if (data.title && data.title.length > 100) {
      errors.push('Título no debe exceder 100 caracteres');
    }

    if (data.comment && data.comment.length > 1000) {
      errors.push('Comentario no debe exceder 1000 caracteres');
    }

    return errors;
  }
}

// Exportar instancia singleton
export const reviewService = new ReviewService();

export interface Review {
  id: string;
  vehicleId: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  rating: number; // 1-5
  title: string;
  comment: string;
  photos?: string[];
  pros?: string[];
  cons?: string[];
  verifiedPurchase?: boolean;
  helpful: number;
  date: string;
}

export interface ReviewStats {
  averageRating: number;
  totalReviews: number;
  distribution: {
    5: number;
    4: number;
    3: number;
    2: number;
    1: number;
  };
}

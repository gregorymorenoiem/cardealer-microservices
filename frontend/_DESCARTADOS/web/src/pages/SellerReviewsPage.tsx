import React from 'react';
import { useParams } from 'react-router-dom';
import { ReviewsSection } from '../components/reviews';
import MainLayout from '../layouts/MainLayout';

/**
 * Página de reseñas de un vendedor específico
 * Muestra el sistema completo de reviews para un seller
 */
const SellerReviewsPage: React.FC = () => {
  const { sellerId } = useParams<{ sellerId: string }>();

  if (!sellerId) {
    return (
      <MainLayout>
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Vendedor no encontrado</h1>
            <p className="text-gray-600">No se pudo encontrar el ID del vendedor en la URL</p>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <div className="bg-white shadow-sm border-b">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="py-6">
              <div className="flex items-center space-x-3">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 bg-blue-600 rounded-full flex items-center justify-center">
                    <svg
                      className="w-6 h-6 text-white"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                      />
                    </svg>
                  </div>
                </div>
                <div>
                  <h1 className="text-2xl font-bold text-gray-900">Reseñas del Vendedor</h1>
                  <p className="text-sm text-gray-500">ID: {sellerId}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Content */}
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <ReviewsSection sellerUserId={sellerId} allowReviews={true} className="max-w-none" />
        </div>
      </div>
    </MainLayout>
  );
};

export default SellerReviewsPage;

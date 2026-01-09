import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ReviewForm } from '../components/reviews';
import MainLayout from '../layouts/MainLayout';
import { useAuth } from '../hooks/useAuth';

/**
 * Página para escribir una nueva reseña
 * Formulario standalone para crear reseñas
 */
const WriteReviewPage: React.FC = () => {
  const { sellerId, vehicleId } = useParams<{ sellerId: string; vehicleId?: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [submitted, setSubmitted] = useState(false);

  if (!sellerId) {
    return (
      <MainLayout>
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Error en los parámetros</h1>
            <p className="text-gray-600">No se pudo encontrar la información del vendedor</p>
          </div>
        </div>
      </MainLayout>
    );
  }

  const handleReviewSubmitted = () => {
    setSubmitted(true);
    setTimeout(() => {
      navigate(`/sellers/${sellerId}/reviews`);
    }, 2000);
  };

  const handleCancel = () => {
    navigate(-1); // Go back to previous page
  };

  if (submitted) {
    return (
      <MainLayout>
        <div className="min-h-screen flex items-center justify-center bg-gray-50">
          <div className="max-w-md mx-auto text-center">
            <div className="bg-white rounded-lg shadow-lg p-8">
              <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-green-100 flex items-center justify-center">
                <svg
                  className="w-8 h-8 text-green-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M5 13l4 4L19 7"
                  />
                </svg>
              </div>

              <h2 className="text-2xl font-bold text-gray-900 mb-2">¡Reseña Enviada!</h2>

              <p className="text-gray-600 mb-6">
                Tu reseña ha sido enviada exitosamente. Será revisada por nuestro equipo antes de
                publicarse.
              </p>

              <div className="text-sm text-gray-500">Redirigiendo en unos segundos...</div>
            </div>
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
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-3">
                  <button
                    onClick={handleCancel}
                    className="p-2 text-gray-400 hover:text-gray-600 rounded-full hover:bg-gray-100"
                  >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M15 19l-7-7 7-7"
                      />
                    </svg>
                  </button>

                  <div>
                    <h1 className="text-2xl font-bold text-gray-900">Escribir Reseña</h1>
                    <p className="text-sm text-gray-500">
                      Comparte tu experiencia con este vendedor
                    </p>
                  </div>
                </div>

                <div className="hidden sm:flex items-center space-x-2 text-sm text-gray-500">
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                  <span>Tu reseña será moderada antes de publicarse</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Content */}
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Main form */}
            <div className="lg:col-span-2">
              <ReviewForm
                sellerUserId={sellerId}
                vehicleId={vehicleId}
                buyerUserId={user?.id}
                onReviewSubmitted={handleReviewSubmitted}
                onCancel={handleCancel}
              />
            </div>

            {/* Sidebar with tips */}
            <div className="lg:col-span-1">
              <div className="bg-white rounded-lg border p-6 sticky top-8">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Consejos para una buena reseña
                </h3>

                <div className="space-y-4">
                  <div className="flex items-start space-x-3">
                    <div className="flex-shrink-0 w-6 h-6 bg-blue-100 rounded-full flex items-center justify-center">
                      <svg
                        className="w-3 h-3 text-blue-600"
                        fill="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                    </div>
                    <div>
                      <h4 className="text-sm font-medium text-gray-900">Sé específico</h4>
                      <p className="text-sm text-gray-600">
                        Describe detalles concretos de tu experiencia
                      </p>
                    </div>
                  </div>

                  <div className="flex items-start space-x-3">
                    <div className="flex-shrink-0 w-6 h-6 bg-green-100 rounded-full flex items-center justify-center">
                      <svg
                        className="w-3 h-3 text-green-600"
                        fill="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                    </div>
                    <div>
                      <h4 className="text-sm font-medium text-gray-900">Sé honesto</h4>
                      <p className="text-sm text-gray-600">
                        Comparte tanto lo positivo como áreas de mejora
                      </p>
                    </div>
                  </div>

                  <div className="flex items-start space-x-3">
                    <div className="flex-shrink-0 w-6 h-6 bg-yellow-100 rounded-full flex items-center justify-center">
                      <svg
                        className="w-3 h-3 text-yellow-600"
                        fill="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                    </div>
                    <div>
                      <h4 className="text-sm font-medium text-gray-900">Sé constructivo</h4>
                      <p className="text-sm text-gray-600">
                        Ofrece retroalimentación útil para otros compradores
                      </p>
                    </div>
                  </div>

                  <div className="flex items-start space-x-3">
                    <div className="flex-shrink-0 w-6 h-6 bg-purple-100 rounded-full flex items-center justify-center">
                      <svg
                        className="w-3 h-3 text-purple-600"
                        fill="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                    </div>
                    <div>
                      <h4 className="text-sm font-medium text-gray-900">
                        Evita información personal
                      </h4>
                      <p className="text-sm text-gray-600">
                        No incluyas datos de contacto o información privada
                      </p>
                    </div>
                  </div>
                </div>

                <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                  <div className="flex items-center space-x-2">
                    <svg
                      className="w-5 h-5 text-blue-600"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                      />
                    </svg>
                    <span className="text-sm font-medium text-blue-900">Proceso de revisión</span>
                  </div>
                  <p className="text-sm text-blue-800 mt-2">
                    Tu reseña será revisada por nuestro equipo en un plazo de 24-48 horas antes de
                    ser publicada.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default WriteReviewPage;

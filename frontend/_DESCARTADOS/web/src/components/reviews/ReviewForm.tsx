import React, { useState } from 'react';
import { StarRating } from './StarRating';
import { reviewService } from '../../services/reviewService';
import type { CreateReviewRequest } from '../../services/reviewService';

interface ReviewFormProps {
  sellerUserId: string;
  vehicleId?: string;
  buyerUserId?: string;
  onReviewSubmitted?: () => void;
  onCancel?: () => void;
  className?: string;
}

/**
 * Formulario para que los usuarios escriban reseñas
 * Incluye validaciones, preview y manejo de estados
 */
export const ReviewForm: React.FC<ReviewFormProps> = ({
  sellerUserId,
  vehicleId,
  buyerUserId,
  onReviewSubmitted,
  onCancel,
  className = '',
}) => {
  const [rating, setRating] = useState<number>(0);
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [isVerifiedPurchase, setIsVerifiedPurchase] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (rating === 0) {
      newErrors.rating = 'Debes seleccionar una calificación';
    }

    if (!title.trim()) {
      newErrors.title = 'El título es requerido';
    } else if (title.length > 100) {
      newErrors.title = 'El título no puede exceder 100 caracteres';
    }

    if (!content.trim()) {
      newErrors.content = 'El comentario es requerido';
    } else if (content.length < 10) {
      newErrors.content = 'El comentario debe tener al menos 10 caracteres';
    } else if (content.length > 1000) {
      newErrors.content = 'El comentario no puede exceder 1000 caracteres';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);

    try {
      const request: CreateReviewRequest = {
        sellerUserId,
        vehicleId,
        buyerUserId,
        rating,
        title: title.trim(),
        content: content.trim(),
        isVerifiedPurchase,
      };

      const result = await reviewService.createReview(request);

      if (result.success) {
        // Reset form
        setRating(0);
        setTitle('');
        setContent('');
        setIsVerifiedPurchase(false);
        setErrors({});

        // Notify parent
        onReviewSubmitted?.();
      } else {
        setErrors({ submit: result.error || 'Error al enviar la reseña' });
      }
    } catch (error) {
      setErrors({ submit: 'Error inesperado al enviar la reseña' });
    } finally {
      setIsSubmitting(false);
    }
  };

  const getRatingDescription = (rating: number): string => {
    switch (rating) {
      case 1:
        return 'Muy malo';
      case 2:
        return 'Malo';
      case 3:
        return 'Regular';
      case 4:
        return 'Bueno';
      case 5:
        return 'Excelente';
      default:
        return 'Selecciona una calificación';
    }
  };

  return (
    <form onSubmit={handleSubmit} className={`bg-white rounded-lg border p-6 ${className}`}>
      {/* Header */}
      <div className="mb-6">
        <h3 className="text-xl font-semibold text-gray-900 mb-2">Escribir Reseña</h3>
        <p className="text-sm text-gray-600">
          Comparte tu experiencia para ayudar a otros compradores
        </p>
      </div>

      {/* Rating */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">Calificación *</label>
        <div className="flex items-center space-x-4">
          <StarRating rating={rating} onRatingChange={setRating} interactive={true} size="lg" />
          <span className="text-sm text-gray-600 font-medium">{getRatingDescription(rating)}</span>
        </div>
        {errors.rating && <p className="mt-1 text-sm text-red-600">{errors.rating}</p>}
      </div>

      {/* Title */}
      <div className="mb-4">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Título de la reseña *
        </label>
        <input
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Resume tu experiencia en una frase..."
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          maxLength={100}
        />
        <div className="flex justify-between items-center mt-1">
          {errors.title ? (
            <p className="text-sm text-red-600">{errors.title}</p>
          ) : (
            <span className="text-xs text-gray-500">Máximo 100 caracteres</span>
          )}
          <span className="text-xs text-gray-400">{title.length}/100</span>
        </div>
      </div>

      {/* Content */}
      <div className="mb-4">
        <label className="block text-sm font-medium text-gray-700 mb-2">Tu reseña *</label>
        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder="Describe tu experiencia con este vendedor. ¿Qué fue lo que más te gustó? ¿Hay algo que mejorarías?"
          rows={4}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-vertical"
          maxLength={1000}
        />
        <div className="flex justify-between items-center mt-1">
          {errors.content ? (
            <p className="text-sm text-red-600">{errors.content}</p>
          ) : (
            <span className="text-xs text-gray-500">Mínimo 10 caracteres, máximo 1000</span>
          )}
          <span className="text-xs text-gray-400">{content.length}/1000</span>
        </div>
      </div>

      {/* Verified Purchase */}
      <div className="mb-6">
        <label className="flex items-center">
          <input
            type="checkbox"
            checked={isVerifiedPurchase}
            onChange={(e) => setIsVerifiedPurchase(e.target.checked)}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
          />
          <span className="ml-2 text-sm text-gray-700">
            Esta es una compra verificada
            <span className="ml-1 text-xs text-gray-500">(opcional)</span>
          </span>
        </label>
        <p className="mt-1 text-xs text-gray-500 ml-6">
          Marca esta casilla si compraste este vehículo a través de OKLA
        </p>
      </div>

      {/* Submit Error */}
      {errors.submit && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
          <p className="text-sm text-red-600">{errors.submit}</p>
        </div>
      )}

      {/* Actions */}
      <div className="flex justify-end space-x-3 pt-4 border-t">
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            disabled={isSubmitting}
          >
            Cancelar
          </button>
        )}

        <button
          type="submit"
          disabled={isSubmitting || rating === 0}
          className="px-6 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isSubmitting ? (
            <div className="flex items-center space-x-2">
              <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  strokeWidth="2"
                  className="opacity-25"
                />
                <path
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                  className="opacity-75"
                />
              </svg>
              <span>Enviando...</span>
            </div>
          ) : (
            'Publicar Reseña'
          )}
        </button>
      </div>

      {/* Guidelines */}
      <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-md">
        <h4 className="text-sm font-medium text-blue-900 mb-1">Pautas para reseñas</h4>
        <ul className="text-xs text-blue-800 space-y-1">
          <li>• Sé honesto y constructivo en tus comentarios</li>
          <li>• Enfócate en tu experiencia con el vendedor y el vehículo</li>
          <li>• Evita información personal o datos de contacto</li>
          <li>• Las reseñas son moderadas antes de publicarse</li>
        </ul>
      </div>
    </form>
  );
};

export default ReviewForm;

import React, { useState } from 'react';
import { X, Mail, Phone, MessageSquare, User, Car } from 'lucide-react';

interface ContactSellerModalProps {
  isOpen: boolean;
  onClose: () => void;
  vehicle: {
    id: string;
    title: string;
    price: number;
    imageUrl?: string;
  };
  seller: {
    id: string;
    name: string;
    phoneNumber?: string;
    email: string;
    responseRate?: string;
    responseTime?: string;
  };
  onSubmit: (data: ContactFormData) => Promise<void>;
}

interface ContactFormData {
  vehicleId: string;
  sellerId: string;
  subject: string;
  buyerName: string;
  buyerEmail: string;
  buyerPhone?: string;
  message: string;
}

export const ContactSellerModal: React.FC<ContactSellerModalProps> = ({
  isOpen,
  onClose,
  vehicle,
  seller,
  onSubmit,
}) => {
  const [formData, setFormData] = useState<ContactFormData>({
    vehicleId: vehicle.id,
    sellerId: seller.id,
    subject: `Interés en ${vehicle.title}`,
    buyerName: '',
    buyerEmail: '',
    buyerPhone: '',
    message: `Hola ${seller.name},\n\nEstoy interesado en su vehículo ${vehicle.title} con precio de ${vehicle.price.toLocaleString('es-DO', { style: 'currency', currency: 'DOP' })}.\n\n¿Podríamos coordinar una cita para verlo?\n\nGracias.`,
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.buyerName.trim()) {
      newErrors.buyerName = 'El nombre es requerido';
    }

    if (!formData.buyerEmail.trim()) {
      newErrors.buyerEmail = 'El email es requerido';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.buyerEmail)) {
      newErrors.buyerEmail = 'Email inválido';
    }

    if (!formData.message.trim()) {
      newErrors.message = 'El mensaje es requerido';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) return;

    setIsSubmitting(true);
    try {
      await onSubmit(formData);
      onClose();
    } catch (error) {
      console.error('Error sending message:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        <div className="fixed inset-0 bg-black bg-opacity-50" onClick={onClose} />

        <div className="relative w-full max-w-2xl bg-white rounded-lg shadow-xl">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b">
            <div className="flex items-center space-x-3">
              <MessageSquare className="h-6 w-6 text-blue-600" />
              <h2 className="text-xl font-semibold text-gray-900">Contactar Vendedor</h2>
            </div>
            <button
              onClick={onClose}
              className="p-2 hover:bg-gray-100 rounded-full transition-colors"
            >
              <X className="h-5 w-5" />
            </button>
          </div>

          {/* Vehicle Info */}
          <div className="p-6 bg-gray-50 border-b">
            <div className="flex items-center space-x-4">
              {vehicle.imageUrl && (
                <img
                  src={vehicle.imageUrl}
                  alt={vehicle.title}
                  className="w-16 h-16 rounded-lg object-cover"
                />
              )}
              <div>
                <h3 className="font-semibold text-gray-900">{vehicle.title}</h3>
                <p className="text-lg font-bold text-blue-600">
                  {vehicle.price.toLocaleString('es-DO', { style: 'currency', currency: 'DOP' })}
                </p>
              </div>
            </div>
          </div>

          {/* Seller Info */}
          <div className="p-6 border-b">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-3">
                <div className="w-12 h-12 bg-gray-200 rounded-full flex items-center justify-center">
                  <User className="h-6 w-6 text-gray-600" />
                </div>
                <div>
                  <p className="font-semibold text-gray-900">{seller.name}</p>
                  <div className="flex items-center space-x-4 text-sm text-gray-600">
                    {seller.responseRate && <span>Responde {seller.responseRate}</span>}
                    {seller.responseTime && <span>{seller.responseTime}</span>}
                  </div>
                </div>
              </div>

              <div className="flex space-x-2">
                {seller.phoneNumber && (
                  <a
                    href={`tel:${seller.phoneNumber}`}
                    className="p-2 text-green-600 hover:bg-green-50 rounded-full transition-colors"
                    title="Llamar"
                  >
                    <Phone className="h-5 w-5" />
                  </a>
                )}
                <a
                  href={`mailto:${seller.email}`}
                  className="p-2 text-blue-600 hover:bg-blue-50 rounded-full transition-colors"
                  title="Email"
                >
                  <Mail className="h-5 w-5" />
                </a>
              </div>
            </div>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="p-6 space-y-4">
            {/* Subject */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Asunto</label>
              <input
                type="text"
                value={formData.subject}
                onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Asunto del mensaje"
              />
            </div>

            {/* Buyer Info */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Tu nombre *</label>
                <input
                  type="text"
                  value={formData.buyerName}
                  onChange={(e) => setFormData({ ...formData, buyerName: e.target.value })}
                  className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                    errors.buyerName ? 'border-red-500' : 'border-gray-300'
                  }`}
                  placeholder="Nombre completo"
                />
                {errors.buyerName && (
                  <p className="text-red-500 text-sm mt-1">{errors.buyerName}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Tu email *</label>
                <input
                  type="email"
                  value={formData.buyerEmail}
                  onChange={(e) => setFormData({ ...formData, buyerEmail: e.target.value })}
                  className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                    errors.buyerEmail ? 'border-red-500' : 'border-gray-300'
                  }`}
                  placeholder="tu@email.com"
                />
                {errors.buyerEmail && (
                  <p className="text-red-500 text-sm mt-1">{errors.buyerEmail}</p>
                )}
              </div>
            </div>

            {/* Phone (optional) */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Tu teléfono (opcional)
              </label>
              <input
                type="tel"
                value={formData.buyerPhone}
                onChange={(e) => setFormData({ ...formData, buyerPhone: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="(809) 123-4567"
              />
            </div>

            {/* Message */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Mensaje *</label>
              <textarea
                value={formData.message}
                onChange={(e) => setFormData({ ...formData, message: e.target.value })}
                rows={6}
                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                  errors.message ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="Escribe tu mensaje aquí..."
              />
              {errors.message && <p className="text-red-500 text-sm mt-1">{errors.message}</p>}
            </div>

            {/* Actions */}
            <div className="flex justify-end space-x-3 pt-4">
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 text-gray-700 bg-gray-200 rounded-md hover:bg-gray-300 transition-colors"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center space-x-2"
              >
                {isSubmitting ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white" />
                    <span>Enviando...</span>
                  </>
                ) : (
                  <>
                    <MessageSquare className="h-4 w-4" />
                    <span>Enviar Mensaje</span>
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

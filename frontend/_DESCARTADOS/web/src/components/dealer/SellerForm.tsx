/**
 * Seller Form Component
 * Form for creating and editing seller profiles
 */

import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import {
  User,
  Phone,
  Mail,
  MapPin,
  MessageCircle,
  Facebook,
  Instagram,
  Twitter,
  Loader2,
  Check,
  Languages,
  Heart,
} from 'lucide-react';
import type { SellerProfile, CreateSellerRequest } from '@/types/dealer';

interface SellerFormProps {
  seller?: SellerProfile;
  userId?: string;
  onSubmit: (data: CreateSellerRequest) => Promise<void>;
  isLoading?: boolean;
  mode: 'create' | 'edit';
}

export const SellerForm: React.FC<SellerFormProps> = ({
  seller,
  userId,
  onSubmit,
  isLoading = false,
  mode,
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    reset,
    watch,
  } = useForm<CreateSellerRequest>({
    defaultValues: seller || {
      userId: userId || '',
      country: 'República Dominicana',
      preferredContactMethod: 'phone',
    },
  });

  useEffect(() => {
    if (seller) {
      reset(seller);
    }
  }, [seller, reset]);

  const onFormSubmit = async (data: CreateSellerRequest) => {
    await onSubmit(data);
  };

  const preferredContact = watch('preferredContactMethod');

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-8">
      {/* Personal Information */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <User className="h-5 w-5 text-blue-600" />
          Información Personal
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Nombre para Mostrar *
            </label>
            <input
              type="text"
              {...register('displayName', { required: 'El nombre es requerido' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Cómo quieres que te conozcan"
            />
            {errors.displayName && (
              <p className="mt-1 text-sm text-red-600">{errors.displayName.message}</p>
            )}
          </div>

          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Biografía
            </label>
            <textarea
              {...register('bio')}
              rows={3}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Cuéntale a los compradores sobre ti..."
            />
          </div>
        </div>
      </div>

      {/* Contact Information */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Phone className="h-5 w-5 text-blue-600" />
          Información de Contacto
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Correo Electrónico *
            </label>
            <div className="relative">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
              <input
                type="email"
                {...register('email', {
                  required: 'El correo electrónico es requerido',
                  pattern: {
                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                    message: 'Correo electrónico inválido',
                  },
                })}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="tu@email.com"
              />
            </div>
            {errors.email && (
              <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Teléfono
            </label>
            <div className="relative">
              <Phone className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
              <input
                type="tel"
                {...register('phone')}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="+1 (809) 555-1234"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              WhatsApp
            </label>
            <div className="relative">
              <MessageCircle className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
              <input
                type="tel"
                {...register('whatsApp')}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="+1 (829) 555-1234"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Método de Contacto Preferido *
            </label>
            <div className="flex gap-4">
              {[{ value: 'phone', label: 'Teléfono' }, { value: 'email', label: 'Email' }, { value: 'whatsapp', label: 'WhatsApp' }, { value: 'chat', label: 'Chat' }].map(({ value, label }) => (
                <label
                  key={value}
                  className={`flex-1 flex items-center justify-center gap-2 p-3 border rounded-lg cursor-pointer transition-colors ${
                    preferredContact === value
                      ? 'border-blue-500 bg-blue-50 text-blue-600'
                      : 'border-gray-200 hover:bg-gray-50'
                  }`}
                >
                  <input
                    type="radio"
                    {...register('preferredContactMethod')}
                    value={value}
                    className="sr-only"
                  />
                  <span className="text-sm font-medium">{label}</span>
                </label>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Location */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <MapPin className="h-5 w-5 text-blue-600" />
          Ubicación
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Municipio/Ciudad *
            </label>
            <input
              type="text"
              {...register('city', { required: 'El municipio es requerido' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Santo Domingo"
            />
            {errors.city && (
              <p className="mt-1 text-sm text-red-600">{errors.city.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Provincia *
            </label>
            <select
              {...register('state', { required: 'La provincia es requerida' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">Seleccione una provincia</option>
              <option value="Distrito Nacional">Distrito Nacional</option>
              <option value="Santo Domingo">Santo Domingo</option>
              <option value="Santiago">Santiago</option>
              <option value="La Altagracia">La Altagracia</option>
              <option value="Puerto Plata">Puerto Plata</option>
              <option value="La Romana">La Romana</option>
              <option value="San Cristóbal">San Cristóbal</option>
              <option value="Duarte">Duarte</option>
              <option value="La Vega">La Vega</option>
              <option value="Espaillat">Espaillat</option>
              <option value="Azua">Azua</option>
              <option value="Barahona">Barahona</option>
              <option value="San Pedro de Macorís">San Pedro de Macorís</option>
              <option value="María Trinidad Sánchez">María Trinidad Sánchez</option>
              <option value="Monseñor Nouel">Monseñor Nouel</option>
              <option value="Sánchez Ramírez">Sánchez Ramírez</option>
              <option value="Peravia">Peravia</option>
              <option value="San Juan">San Juan</option>
              <option value="Elías Piña">Elías Piña</option>
              <option value="Baoruco">Baoruco</option>
              <option value="El Seibo">El Seibo</option>
              <option value="Hato Mayor">Hato Mayor</option>
              <option value="Independencia">Independencia</option>
              <option value="Pedernales">Pedernales</option>
              <option value="Samaná">Samaná</option>
              <option value="Monte Cristi">Monte Cristi</option>
              <option value="Dajabón">Dajabón</option>
              <option value="Santiago Rodríguez">Santiago Rodríguez</option>
              <option value="Valverde">Valverde</option>
              <option value="Monte Plata">Monte Plata</option>
              <option value="Hermanas Mirabal">Hermanas Mirabal</option>
              <option value="San José de Ocoa">San José de Ocoa</option>
            </select>
            {errors.state && (
              <p className="mt-1 text-sm text-red-600">{errors.state.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              País *
            </label>
            <input
              type="text"
              {...register('country', { required: 'El país es requerido' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="República Dominicana"
              readOnly
            />
          </div>
        </div>
      </div>

      {/* Social Media */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Redes Sociales</h3>
        <p className="text-sm text-gray-500 mb-4">
          Agrega tus perfiles de redes sociales para generar confianza con potenciales compradores
        </p>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Facebook
            </label>
            <div className="relative">
              <Facebook className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-blue-600" />
              <input
                type="url"
                {...register('facebookUrl')}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="https://facebook.com/..."
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Instagram
            </label>
            <div className="relative">
              <Instagram className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-pink-600" />
              <input
                type="url"
                {...register('instagramUrl')}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="https://instagram.com/..."
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Twitter
            </label>
            <div className="relative">
              <Twitter className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-sky-500" />
              <input
                type="url"
                {...register('twitterUrl')}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="https://twitter.com/..."
              />
            </div>
          </div>
        </div>
      </div>

      {/* Additional Information */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Additional Information</h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-2">
              <Languages className="h-4 w-4" />
              Languages Spoken
            </label>
            <input
              type="text"
              {...register('spokenLanguages')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="English, Spanish, Portuguese..."
            />
            <p className="mt-1 text-xs text-gray-500">Separate languages with commas</p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-2">
              <Heart className="h-4 w-4" />
              Specialization
            </label>
            <input
              type="text"
              {...register('specialization')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Sports cars, Family vehicles, Classic cars..."
            />
          </div>
        </div>
      </div>

      {/* Submit Button */}
      <div className="flex justify-end gap-4">
        <button
          type="button"
          onClick={() => reset()}
          className="px-6 py-3 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-50 transition-colors"
          disabled={isLoading}
        >
          Reset
        </button>
        <button
          type="submit"
          disabled={isLoading || (!isDirty && mode === 'edit')}
          className="px-6 py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-medium rounded-lg transition-colors flex items-center gap-2"
        >
          {isLoading ? (
            <>
              <Loader2 className="h-5 w-5 animate-spin" />
              {mode === 'create' ? 'Creating...' : 'Saving...'}
            </>
          ) : (
            <>
              <Check className="h-5 w-5" />
              {mode === 'create' ? 'Create Profile' : 'Save Changes'}
            </>
          )}
        </button>
      </div>
    </form>
  );
};

export default SellerForm;

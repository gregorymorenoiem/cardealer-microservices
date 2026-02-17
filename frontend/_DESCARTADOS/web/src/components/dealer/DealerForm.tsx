/**
 * Dealer Form Component
 * Form for creating and editing dealer profiles
 */

import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import {
  Building2,
  MapPin,
  Phone,
  Mail,
  Globe,
  FileText,
  CreditCard,
  Truck,
  Shield,
  Car,
  Upload,
  Loader2,
  Check,
} from 'lucide-react';
import type { Dealer, CreateDealerRequest } from '@/types/dealer';
import { DealerType, DealerTypeLabels } from '@/types/dealer';
import { GoogleMapComponent } from '@/components/shared/GoogleMap';

interface DealerFormProps {
  dealer?: Dealer;
  ownerUserId?: string;
  onSubmit: (data: CreateDealerRequest) => Promise<void>;
  isLoading?: boolean;
  mode: 'create' | 'edit';
}

export const DealerForm: React.FC<DealerFormProps> = ({
  dealer,
  ownerUserId,
  onSubmit,
  isLoading = false,
  mode,
}) => {
  const [latitude, setLatitude] = useState<number>(dealer?.latitude || 18.4861);
  const [longitude, setLongitude] = useState<number>(dealer?.longitude || -69.9312);
  const [mapAddress, setMapAddress] = useState<string>(dealer?.address || '');

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isDirty },
    reset,
    setValue,
  } = useForm<CreateDealerRequest>({
    defaultValues: dealer || {
      ownerUserId: ownerUserId || '',
      dealerType: DealerType.INDEPENDENT,
      country: 'República Dominicana',
      acceptsFinancing: false,
      acceptsTradeIn: false,
      offersWarranty: false,
      homeDelivery: false,
    },
  });

  useEffect(() => {
    if (dealer) {
      reset(dealer);
    }
  }, [dealer, reset]);

  const onFormSubmit = async (data: CreateDealerRequest) => {
    await onSubmit(data);
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-8">
      {/* Business Information */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Building2 className="h-5 w-5 text-blue-600" />
          Información del Negocio
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Nombre del Negocio *
            </label>
            <input
              type="text"
              {...register('businessName', { required: 'El nombre del negocio es requerido' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Autos Premium RD"
            />
            {errors.businessName && (
              <p className="mt-1 text-sm text-red-600">{errors.businessName.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Nombre Comercial
            </label>
            <input
              type="text"
              {...register('tradeName')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Auto Ventas Santo Domingo"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tipo de Dealer *
            </label>
            <select
              {...register('dealerType', { valueAsNumber: true })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              {Object.entries(DealerTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </div>

          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Descripción
            </label>
            <textarea
              {...register('description')}
              rows={3}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Cuéntales a los clientes sobre tu negocio..."
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
                placeholder="contacto@tudealership.com"
              />
            </div>
            {errors.email && (
              <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Teléfono *
            </label>
            <div className="relative">
              <Phone className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
              <input
                type="tel"
                {...register('phone', { required: 'El teléfono es requerido' })}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="+1 (809) 555-1234"
              />
            </div>
            {errors.phone && (
              <p className="mt-1 text-sm text-red-600">{errors.phone.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              WhatsApp
            </label>
            <input
              type="tel"
              {...register('whatsApp')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="+1 (829) 555-1234"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Sitio Web
            </label>
            <div className="relative">
              <Globe className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
              <input
                type="url"
                {...register('website')}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="https://www.tudealership.com"
              />
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

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Dirección *
            </label>
            <input
              type="text"
              {...register('address', { required: 'La dirección es requerida' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Av. Winston Churchill #123"
            />
            {errors.address && (
              <p className="mt-1 text-sm text-red-600">{errors.address.message}</p>
            )}
          </div>

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
              Código Postal
            </label>
            <input
              type="text"
              {...register('zipCode')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="10101"
            />
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

        {/* Mapa de Geolocalización */}
        <div className="mt-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            📍 Ubicación en el Mapa
          </label>
          
          <GoogleMapComponent
            latitude={latitude}
            longitude={longitude}
            onLocationChange={(lat, lng) => {
              setLatitude(lat);
              setLongitude(lng);
              setValue('latitude', lat);
              setValue('longitude', lng);
            }}
            address={mapAddress}
          />
          
          <div className="mt-4 grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">
                Latitud
              </label>
              <input
                type="number"
                step="0.000001"
                value={latitude}
                onChange={(e) => {
                  const lat = parseFloat(e.target.value);
                  setLatitude(lat);
                  setValue('latitude', lat);
                }}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="18.4861"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">
                Longitud
              </label>
              <input
                type="number"
                step="0.000001"
                value={longitude}
                onChange={(e) => {
                  const lng = parseFloat(e.target.value);
                  setLongitude(lng);
                  setValue('longitude', lng);
                }}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="-69.9312"
              />
            </div>
          </div>
        </div>
      </div>

      {/* Legal Information */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <FileText className="h-5 w-5 text-blue-600" />
          Información Legal
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              RNC (Registro Nacional de Contribuyentes) *
            </label>
            <input
              type="text"
              {...register('taxId', { 
                pattern: {
                  value: /^\d{9}$/,
                  message: 'RNC debe tener 9 dígitos'
                }
              })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="123456789"
              maxLength={9}
            />
            {errors.taxId && (
              <p className="mt-1 text-sm text-red-600">{errors.taxId.message}</p>
            )}
            <p className="mt-1 text-xs text-gray-500">9 dígitos sin guiones</p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Número de Registro Mercantil
            </label>
            <input
              type="text"
              {...register('businessRegistrationNumber')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Registro Mercantil"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Licencia de Operación
            </label>
            <input
              type="text"
              {...register('dealerLicenseNumber')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Número de Licencia"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Fecha de Vencimiento de Licencia
            </label>
            <input
              type="date"
              {...register('licenseExpiryDate')}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
      </div>

      {/* Services Offered */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Servicios Ofrecidos</h3>

        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <label className="flex items-center gap-3 p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50 transition-colors">
            <input
              type="checkbox"
              {...register('acceptsFinancing')}
              className="h-5 w-5 text-blue-600 rounded focus:ring-blue-500"
            />
            <div className="flex items-center gap-2">
              <CreditCard className="h-5 w-5 text-green-600" />
              <span className="text-sm font-medium">Financiamiento</span>
            </div>
          </label>

          <label className="flex items-center gap-3 p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50 transition-colors">
            <input
              type="checkbox"
              {...register('acceptsTradeIn')}
              className="h-5 w-5 text-blue-600 rounded focus:ring-blue-500"
            />
            <div className="flex items-center gap-2">
              <Car className="h-5 w-5 text-blue-600" />
              <span className="text-sm font-medium">Recibo Vehículo</span>
            </div>
          </label>

          <label className="flex items-center gap-3 p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50 transition-colors">
            <input
              type="checkbox"
              {...register('offersWarranty')}
              className="h-5 w-5 text-blue-600 rounded focus:ring-blue-500"
            />
            <div className="flex items-center gap-2">
              <Shield className="h-5 w-5 text-purple-600" />
              <span className="text-sm font-medium">Garantía</span>
            </div>
          </label>

          <label className="flex items-center gap-3 p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50 transition-colors">
            <input
              type="checkbox"
              {...register('homeDelivery')}
              className="h-5 w-5 text-blue-600 rounded focus:ring-blue-500"
            />
            <div className="flex items-center gap-2">
              <Truck className="h-5 w-5 text-amber-600" />
              <span className="text-sm font-medium">Entrega a Domicilio</span>
            </div>
          </label>
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
              {mode === 'create' ? 'Create Dealer' : 'Save Changes'}
            </>
          )}
        </button>
      </div>
    </form>
  );
};

export default DealerForm;

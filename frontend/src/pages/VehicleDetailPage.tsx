import { useParams, Link, Navigate } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import ImageGallery from '@/components/organisms/ImageGallery';
import VehicleSpecs from '@/components/organisms/VehicleSpecs';
import ContactSellerForm from '@/components/organisms/ContactSellerForm';
import { mockVehicles } from '@/data/mockVehicles';
import { formatPrice } from '@/utils/formatters';
import { FiHome, FiChevronRight, FiStar, FiMapPin, FiPhone, FiUser } from 'react-icons/fi';

export default function VehicleDetailPage() {
  const { id } = useParams<{ id: string }>();
  const vehicle = mockVehicles.find((v) => v.id === id);

  if (!vehicle) {
    return <Navigate to="/browse" replace />;
  }

  const vehicleTitle = `${vehicle.year} ${vehicle.make} ${vehicle.model}`;

  return (
    <MainLayout>
      <div className="bg-gray-50 min-h-screen py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Breadcrumbs */}
          <nav className="flex items-center gap-2 text-sm text-gray-600 mb-6">
            <Link to="/" className="hover:text-primary transition-colors duration-200">
              <FiHome size={16} />
            </Link>
            <FiChevronRight size={16} />
            <Link to="/browse" className="hover:text-primary transition-colors duration-200">
              Browse
            </Link>
            <FiChevronRight size={16} />
            <span className="text-gray-900 font-medium">{vehicleTitle}</span>
          </nav>

          {/* Title & Price */}
          <div className="mb-8">
            <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4 mb-4">
              <div>
                <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-2">
                  {vehicleTitle}
                </h1>
                <div className="flex items-center gap-4 text-gray-600">
                  <span className="flex items-center gap-1">
                    <FiMapPin size={16} />
                    {vehicle.location}
                  </span>
                  {vehicle.condition && (
                    <span className="px-3 py-1 bg-blue-100 text-blue-800 text-sm font-medium rounded-full">
                      {vehicle.condition}
                    </span>
                  )}
                </div>
              </div>
              <div className="text-right">
                <p className="text-4xl font-bold text-primary">
                  {formatPrice(vehicle.price)}
                </p>
              </div>
            </div>

            {/* Badges */}
            <div className="flex flex-wrap gap-2">
              {vehicle.isFeatured && (
                <span className="px-3 py-1 bg-accent text-white text-sm font-medium rounded-full">
                  Featured
                </span>
              )}
              {vehicle.isNew && (
                <span className="px-3 py-1 bg-green-500 text-white text-sm font-medium rounded-full">
                  New
                </span>
              )}
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Left Column - Images & Details */}
            <div className="lg:col-span-2 space-y-8">
              {/* Image Gallery */}
              <ImageGallery images={vehicle.images} alt={vehicleTitle} />

              {/* Description */}
              <div className="bg-white rounded-xl shadow-card p-6">
                <h2 className="text-2xl font-bold font-heading text-gray-900 mb-4">
                  Description
                </h2>
                <p className="text-gray-700 leading-relaxed whitespace-pre-line">
                  {vehicle.description}
                </p>
              </div>

              {/* Specifications */}
              <VehicleSpecs vehicle={vehicle} />

              {/* Features */}
              {vehicle.features && vehicle.features.length > 0 && (
                <div className="bg-white rounded-xl shadow-card p-6">
                  <h2 className="text-2xl font-bold font-heading text-gray-900 mb-4">
                    Features & Options
                  </h2>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    {vehicle.features.map((feature, index) => (
                      <div
                        key={index}
                        className="flex items-center gap-2 text-gray-700"
                      >
                        <div className="w-2 h-2 bg-primary rounded-full flex-shrink-0" />
                        <span>{feature}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Right Column - Seller Info & Contact */}
            <div className="space-y-6">
              {/* Seller Info */}
              <div className="bg-white rounded-xl shadow-card p-6 sticky top-24">
                <h3 className="text-xl font-bold font-heading text-gray-900 mb-4">
                  Seller Information
                </h3>
                
                <div className="space-y-4 mb-6">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 bg-primary/10 rounded-full flex items-center justify-center">
                      <FiUser size={24} className="text-primary" />
                    </div>
                    <div>
                      <p className="font-semibold text-gray-900">{vehicle.seller.name}</p>
                      <p className="text-sm text-gray-600">{vehicle.seller.type}</p>
                    </div>
                  </div>

                  {vehicle.seller.rating && (
                    <div className="flex items-center gap-2">
                      <FiStar size={20} className="text-yellow-400 fill-yellow-400" />
                      <span className="font-semibold text-gray-900">{vehicle.seller.rating}</span>
                      <span className="text-sm text-gray-600">rating</span>
                    </div>
                  )}

                  <div className="flex items-center gap-2 text-gray-700">
                    <FiPhone size={18} />
                    <a
                      href={`tel:${vehicle.seller.phone}`}
                      className="hover:text-primary transition-colors duration-200"
                    >
                      {vehicle.seller.phone}
                    </a>
                  </div>
                </div>

                <Link to={`tel:${vehicle.seller.phone}`}>
                  <button className="w-full py-3 px-4 bg-secondary text-white rounded-lg hover:bg-secondary-600 transition-colors duration-200 font-medium mb-3">
                    Call Seller
                  </button>
                </Link>

                <Link to="/browse">
                  <button className="w-full py-3 px-4 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors duration-200 font-medium">
                    Back to Browse
                  </button>
                </Link>
              </div>

              {/* Contact Form */}
              <ContactSellerForm
                vehicleName={vehicleTitle}
                sellerName={vehicle.seller.name}
                sellerPhone={vehicle.seller.phone}
              />
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}

import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { FiSearch, FiMessageSquare, FiCheckCircle, FiWifi, FiWifiOff } from 'react-icons/fi';
import Button from '@/components/atoms/Button';
import MainLayout from '@/layouts/MainLayout';
import SearchBar from '@/components/molecules/SearchBar';
import VehicleCard from '@/components/organisms/VehicleCard';
import VehicleCardSkeleton from '@/components/organisms/VehicleCardSkeleton';
import { getFeaturedVehicles } from '@/services/vehicleService';
import { mockVehicles } from '@/data/mockVehicles';

function HomePage() {
  const { t } = useTranslation(['vehicles', 'common']);
  const handleSearch = (filters: unknown) => {
    console.log('Search filters:', filters);
    // TODO: Navigate to browse page with filters
  };

  // Fetch featured vehicles from API
  const {
    data: apiVehicles,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ['featured-vehicles', 8],
    queryFn: () => getFeaturedVehicles(8),
    staleTime: 5 * 60 * 1000,
    retry: 1,
  });

  // Use API data or fallback to mock
  const isUsingMockData = isError || !apiVehicles?.length;
  const featuredVehicles = !isUsingMockData
    ? apiVehicles
    : mockVehicles.filter((v) => v.isFeatured).slice(0, 8);

  return (
    <MainLayout>
      {/* Hero Section */}
      <div className="relative bg-gradient-to-br from-primary via-primary-600 to-secondary overflow-hidden">
        {/* Background Pattern */}
        <div className="absolute inset-0 opacity-10">
          <div
            className="absolute inset-0"
            style={{
              backgroundImage:
                'url("data:image/svg+xml,%3Csvg width="60" height="60" viewBox="0 0 60 60" xmlns="http://www.w3.org/2000/svg"%3E%3Cg fill="none" fill-rule="evenodd"%3E%3Cg fill="%23ffffff" fill-opacity="0.4"%3E%3Cpath d="M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")',
            }}
          ></div>
        </div>

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20 lg:py-28">
          <div className="text-center mb-12">
            <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold font-heading text-white mb-6">
              {t('vehicles:home.heroTitle')}
            </h1>
            <p className="text-xl sm:text-2xl text-white/90 max-w-3xl mx-auto mb-8">
              {t('vehicles:home.heroSubtitle')}
            </p>

            <div className="flex flex-col sm:flex-row gap-4 justify-center mb-12">
              <Link to="/browse">
                <Button variant="secondary" size="lg" className="shadow-lg">
                  <FiSearch size={20} className="mr-2" />
                  {t('vehicles:home.browseCars')}
                </Button>
              </Link>
              <Link to="/sell">
                <Button
                  variant="outline"
                  size="lg"
                  className="bg-white/10 text-white border-white hover:bg-white hover:text-primary shadow-lg"
                >
                  {t('vehicles:home.sellYourCar')}
                </Button>
              </Link>
            </div>
          </div>

          {/* Search Bar */}
          <div className="max-w-5xl mx-auto">
            <SearchBar onSearch={handleSearch} />
          </div>
        </div>
      </div>

      {/* Stats Section */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
            <div>
              <p className="text-4xl font-bold text-primary mb-2">15,000+</p>
              <p className="text-gray-600">{t('vehicles:home.vehiclesListed')}</p>
            </div>
            <div>
              <p className="text-4xl font-bold text-primary mb-2">8,500+</p>
              <p className="text-gray-600">{t('vehicles:home.happyCustomers')}</p>
            </div>
            <div>
              <p className="text-4xl font-bold text-primary mb-2">250+</p>
              <p className="text-gray-600">{t('vehicles:home.verifiedDealers')}</p>
            </div>
            <div>
              <p className="text-4xl font-bold text-primary mb-2">50+</p>
              <p className="text-gray-600">{t('vehicles:home.citiesCovered')}</p>
            </div>
          </div>
        </div>
      </div>

      {/* How It Works Section */}
      <div className="bg-gray-50 py-16 lg:py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-4">
              {t('vehicles:home.howItWorks')}
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              {t('vehicles:home.howItWorksSubtitle')}
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {/* Step 1 */}
            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiSearch className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                {t('vehicles:home.step1Title')}
              </h3>
              <p className="text-gray-600">{t('vehicles:home.step1Desc')}</p>
            </div>

            {/* Step 2 */}
            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiMessageSquare className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                {t('vehicles:home.step2Title')}
              </h3>
              <p className="text-gray-600">{t('vehicles:home.step2Desc')}</p>
            </div>

            {/* Step 3 */}
            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiCheckCircle className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                {t('vehicles:home.step3Title')}
              </h3>
              <p className="text-gray-600">{t('vehicles:home.step3Desc')}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Featured Listings Section */}
      <div className="bg-white py-16 lg:py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between mb-10">
            <div>
              <div className="flex items-center gap-2 mb-2">
                <h2 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900">
                  {t('vehicles:home.featuredListings')}
                </h2>
                {/* Data source indicator */}
                <span
                  className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${
                    isUsingMockData ? 'bg-amber-100 text-amber-700' : 'bg-green-100 text-green-700'
                  }`}
                  title={isUsingMockData ? 'Using demo data' : 'Live data from ProductService'}
                >
                  {isUsingMockData ? <FiWifiOff size={12} /> : <FiWifi size={12} />}
                  {isUsingMockData ? 'Demo' : 'Live'}
                </span>
              </div>
              <p className="text-gray-600">{t('vehicles:home.featuredSubtitle')}</p>
            </div>
            <Link
              to="/browse"
              className="hidden sm:block text-primary hover:text-primary-600 font-medium"
            >
              {t('vehicles:home.viewAll')} →
            </Link>
          </div>

          {/* Vehicle Cards Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {isLoading
              ? // Show skeletons while loading
                Array.from({ length: 8 }).map((_, i) => <VehicleCardSkeleton key={i} />)
              : featuredVehicles.map((vehicle) => (
                  <VehicleCard
                    key={vehicle.id}
                    id={vehicle.id}
                    make={vehicle.make}
                    model={vehicle.model}
                    year={vehicle.year}
                    price={vehicle.price}
                    mileage={vehicle.mileage}
                    location={vehicle.location || ''}
                    imageUrl={vehicle.images?.[0] || '/placeholder-car.jpg'}
                    isFeatured={vehicle.isFeatured}
                    isNew={vehicle.isNew}
                    transmission={vehicle.transmission}
                    fuelType={vehicle.fuelType}
                  />
                ))}
          </div>

          <div className="text-center mt-10">
            <Link to="/browse">
              <Button variant="outline" size="lg">
                {t('vehicles:home.viewAllVehicles')}
              </Button>
            </Link>
          </div>
        </div>
      </div>

      {/* CTA Section */}
      <div className="bg-gradient-to-r from-primary to-secondary py-16 lg:py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl sm:text-4xl font-bold font-heading text-white mb-6">
            {t('vehicles:home.readyToSell')}
          </h2>
          <p className="text-xl text-white/90 mb-8 max-w-2xl mx-auto">
            {t('vehicles:home.sellSubtitle')}
          </p>
          <Link to="/sell">
            <Button variant="secondary" size="lg" className="shadow-lg">
              {t('vehicles:home.startSellingNow')}
            </Button>
          </Link>
        </div>
      </div>
    </MainLayout>
  );
}

export default HomePage;

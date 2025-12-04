import { Link } from 'react-router-dom';
import { FiSearch, FiMessageSquare, FiCheckCircle } from 'react-icons/fi';
import Button from '@/components/atoms/Button';
import MainLayout from '@/layouts/MainLayout';
import SearchBar from '@/components/molecules/SearchBar';
import VehicleCard from '@/components/organisms/VehicleCard';
import { mockVehicles } from '@/data/mockVehicles';

function HomePage() {
  const handleSearch = (filters: unknown) => {
    console.log('Search filters:', filters);
    // TODO: Navigate to browse page with filters
  };

  // Get featured vehicles for display
  const featuredVehicles = mockVehicles.filter(v => v.isFeatured).slice(0, 8);

  return (
    <MainLayout>
      {/* Hero Section */}
      <div className="relative bg-gradient-to-br from-primary via-primary-600 to-secondary overflow-hidden">
        {/* Background Pattern */}
        <div className="absolute inset-0 opacity-10">
          <div className="absolute inset-0" style={{
            backgroundImage: 'url("data:image/svg+xml,%3Csvg width="60" height="60" viewBox="0 0 60 60" xmlns="http://www.w3.org/2000/svg"%3E%3Cg fill="none" fill-rule="evenodd"%3E%3Cg fill="%23ffffff" fill-opacity="0.4"%3E%3Cpath d="M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")',
          }}></div>
        </div>

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20 lg:py-28">
          <div className="text-center mb-12">
            <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold font-heading text-white mb-6">
              Find Your Dream Car
            </h1>
            <p className="text-xl sm:text-2xl text-white/90 max-w-3xl mx-auto mb-8">
              Browse thousands of verified listings from trusted sellers across the country
            </p>
            
            <div className="flex flex-col sm:flex-row gap-4 justify-center mb-12">
              <Link to="/browse">
                <Button variant="secondary" size="lg" className="shadow-lg">
                  <FiSearch size={20} className="mr-2" />
                  Browse Cars
                </Button>
              </Link>
              <Link to="/sell">
                <Button variant="outline" size="lg" className="bg-white/10 text-white border-white hover:bg-white hover:text-primary shadow-lg">
                  Sell Your Car
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
              <p className="text-gray-600">Vehicles Listed</p>
            </div>
            <div>
              <p className="text-4xl font-bold text-primary mb-2">8,500+</p>
              <p className="text-gray-600">Happy Customers</p>
            </div>
            <div>
              <p className="text-4xl font-bold text-primary mb-2">250+</p>
              <p className="text-gray-600">Verified Dealers</p>
            </div>
            <div>
              <p className="text-4xl font-bold text-primary mb-2">50+</p>
              <p className="text-gray-600">Cities Covered</p>
            </div>
          </div>
        </div>
      </div>

      {/* How It Works Section */}
      <div className="bg-gray-50 py-16 lg:py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-4">
              How It Works
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Buy or sell your car in three simple steps
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {/* Step 1 */}
            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiSearch className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                1. Search & Browse
              </h3>
              <p className="text-gray-600">
                Use our advanced filters to find exactly what you're looking for. Browse thousands of verified listings.
              </p>
            </div>

            {/* Step 2 */}
            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiMessageSquare className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                2. Contact Seller
              </h3>
              <p className="text-gray-600">
                Connect directly with sellers through our secure messaging system. Ask questions and schedule viewings.
              </p>
            </div>

            {/* Step 3 */}
            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiCheckCircle className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                3. Complete Deal
              </h3>
              <p className="text-gray-600">
                Finalize your purchase with confidence. Our platform ensures secure transactions for peace of mind.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Featured Listings Section */}
      <div className="bg-white py-16 lg:py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between mb-10">
            <div>
              <h2 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-2">
                Featured Listings
              </h2>
              <p className="text-gray-600">
                Handpicked vehicles from our premium sellers
              </p>
            </div>
            <Link to="/browse" className="hidden sm:block text-primary hover:text-primary-600 font-medium">
              View All →
            </Link>
          </div>

          {/* Vehicle Cards Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {featuredVehicles.map((vehicle) => (
              <VehicleCard
                key={vehicle.id}
                id={vehicle.id}
                make={vehicle.make}
                model={vehicle.model}
                year={vehicle.year}
                price={vehicle.price}
                mileage={vehicle.mileage}
                location={vehicle.location}
                imageUrl={vehicle.images[0]}
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
                View All Vehicles
              </Button>
            </Link>
          </div>
        </div>
      </div>

      {/* CTA Section */}
      <div className="bg-gradient-to-r from-primary to-secondary py-16 lg:py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl sm:text-4xl font-bold font-heading text-white mb-6">
            Ready to Sell Your Car?
          </h2>
          <p className="text-xl text-white/90 mb-8 max-w-2xl mx-auto">
            List your vehicle for free and reach thousands of potential buyers
          </p>
          <Link to="/sell">
            <Button variant="secondary" size="lg" className="shadow-lg">
              Start Selling Now
            </Button>
          </Link>
        </div>
      </div>
    </MainLayout>
  );
}

export default HomePage;

import { Link } from 'react-router-dom';
import { FiSearch, FiMessageSquare, FiCheckCircle } from 'react-icons/fi';
import Button from '@/components/atoms/Button';
import MainLayout from '@/layouts/MainLayout';
import SearchBar from '@/components/molecules/SearchBar';
import VehicleCard from '@/components/organisms/VehicleCard';

function HomePage() {
  const handleSearch = (filters: unknown) => {
    console.log('Search filters:', filters);
    // TODO: Navigate to browse page with filters
  };

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
            {/* Mock Data - Will be replaced with real data */}
            {mockVehicles.map((vehicle) => (
              <VehicleCard key={vehicle.id} {...vehicle} />
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

// Mock data for featured vehicles
const mockVehicles = [
  {
    id: '1',
    make: 'Tesla',
    model: 'Model 3',
    year: 2023,
    price: 42990,
    mileage: 5200,
    location: 'Los Angeles, CA',
    imageUrl: 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=400&h=300&fit=crop',
    isFeatured: true,
    isNew: true,
    transmission: 'Automatic',
    fuelType: 'Electric',
  },
  {
    id: '2',
    make: 'BMW',
    model: '3 Series',
    year: 2022,
    price: 38500,
    mileage: 12000,
    location: 'Miami, FL',
    imageUrl: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400&h=300&fit=crop',
    isFeatured: true,
    transmission: 'Automatic',
    fuelType: 'Gasoline',
  },
  {
    id: '3',
    make: 'Toyota',
    model: 'Camry',
    year: 2021,
    price: 24900,
    mileage: 28000,
    location: 'Houston, TX',
    imageUrl: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400&h=300&fit=crop',
    transmission: 'Automatic',
    fuelType: 'Hybrid',
  },
  {
    id: '4',
    make: 'Ford',
    model: 'Mustang',
    year: 2023,
    price: 35990,
    mileage: 8500,
    location: 'Dallas, TX',
    imageUrl: 'https://images.unsplash.com/photo-1584345604476-8ec5f5c4c728?w=400&h=300&fit=crop',
    isFeatured: true,
    transmission: 'Manual',
    fuelType: 'Gasoline',
  },
  {
    id: '5',
    make: 'Honda',
    model: 'Accord',
    year: 2022,
    price: 27500,
    mileage: 15000,
    location: 'Chicago, IL',
    imageUrl: 'https://images.unsplash.com/photo-1590362891991-f776e747a588?w=400&h=300&fit=crop',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
  },
  {
    id: '6',
    make: 'Audi',
    model: 'A4',
    year: 2023,
    price: 41200,
    mileage: 6800,
    location: 'New York, NY',
    imageUrl: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400&h=300&fit=crop',
    isFeatured: true,
    transmission: 'Automatic',
    fuelType: 'Gasoline',
  },
  {
    id: '7',
    make: 'Mercedes-Benz',
    model: 'C-Class',
    year: 2022,
    price: 43900,
    mileage: 11000,
    location: 'Seattle, WA',
    imageUrl: 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=400&h=300&fit=crop',
    isFeatured: true,
    transmission: 'Automatic',
    fuelType: 'Gasoline',
  },
  {
    id: '8',
    make: 'Chevrolet',
    model: 'Silverado',
    year: 2021,
    price: 39500,
    mileage: 22000,
    location: 'Denver, CO',
    imageUrl: 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=400&h=300&fit=crop',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
  },
];

export default HomePage;

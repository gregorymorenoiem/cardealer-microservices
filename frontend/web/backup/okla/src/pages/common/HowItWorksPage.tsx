import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import { Link } from 'react-router-dom';
import { FiSearch, FiMessageSquare, FiCheckCircle, FiFileText, FiShield, FiDollarSign } from 'react-icons/fi';

export default function HowItWorksPage() {
  return (
    <MainLayout>
      {/* Hero Section */}
      <div className="relative bg-gradient-to-br from-primary via-primary-600 to-secondary text-white py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-4xl sm:text-5xl font-bold font-heading mb-6">
              How It Works
            </h1>
            <p className="text-xl text-white/90 max-w-3xl mx-auto">
              Buy or sell your vehicle in three simple steps
            </p>
          </div>
        </div>
      </div>

      {/* For Buyers Section */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">For Buyers</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Find your perfect vehicle with our simple and secure process
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8 lg:gap-12">
            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiSearch className="text-primary" size={32} />
              </div>
              <div className="bg-primary text-white w-8 h-8 rounded-full flex items-center justify-center mx-auto mb-4 font-bold">
                1
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                Search & Browse
              </h3>
              <p className="text-gray-600">
                Use our advanced filters to find vehicles that match your preferences. 
                Browse thousands of verified listings with detailed photos and specifications.
              </p>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiMessageSquare className="text-primary" size={32} />
              </div>
              <div className="bg-primary text-white w-8 h-8 rounded-full flex items-center justify-center mx-auto mb-4 font-bold">
                2
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                Contact Seller
              </h3>
              <p className="text-gray-600">
                Connect directly with sellers through our secure messaging system. 
                Ask questions, request additional photos, and schedule test drives.
              </p>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiCheckCircle className="text-primary" size={32} />
              </div>
              <div className="bg-primary text-white w-8 h-8 rounded-full flex items-center justify-center mx-auto mb-4 font-bold">
                3
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                Complete Purchase
              </h3>
              <p className="text-gray-600">
                Finalize your purchase with confidence. Meet in safe locations, 
                conduct thorough inspections, and complete the transaction securely.
              </p>
            </div>
          </div>

          <div className="text-center mt-12">
            <Link to="/browse">
              <Button variant="primary" size="lg">
                Start Browsing
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* For Sellers Section */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">For Sellers</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              List your vehicle and reach thousands of potential buyers
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8 lg:gap-12">
            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiFileText className="text-primary" size={32} />
              </div>
              <div className="bg-primary text-white w-8 h-8 rounded-full flex items-center justify-center mx-auto mb-4 font-bold">
                1
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                Create Listing
              </h3>
              <p className="text-gray-600">
                Fill out our simple form with your vehicle details. Upload high-quality 
                photos and provide accurate information to attract serious buyers.
              </p>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiShield className="text-primary" size={32} />
              </div>
              <div className="bg-primary text-white w-8 h-8 rounded-full flex items-center justify-center mx-auto mb-4 font-bold">
                2
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                Get Verified
              </h3>
              <p className="text-gray-600">
                Our team reviews your listing to ensure quality and accuracy. 
                Verified listings get more visibility and build buyer confidence.
              </p>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiDollarSign className="text-primary" size={32} />
              </div>
              <div className="bg-primary text-white w-8 h-8 rounded-full flex items-center justify-center mx-auto mb-4 font-bold">
                3
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">
                Sell Your Car
              </h3>
              <p className="text-gray-600">
                Receive inquiries from interested buyers. Communicate securely, 
                arrange viewings, and close the deal on your terms.
              </p>
            </div>
          </div>

          <div className="text-center mt-12">
            <Link to="/sell">
              <Button variant="primary" size="lg">
                List Your Vehicle
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* Verification Process */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">Our Verification Process</h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              We ensure every listing meets our quality standards
            </p>
          </div>

          <div className="max-w-3xl mx-auto space-y-6">
            <div className="flex gap-4">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-green-100 text-green-600 rounded-full flex items-center justify-center font-bold">
                  ✓
                </div>
              </div>
              <div>
                <h4 className="font-semibold text-gray-900 mb-2">Identity Verification</h4>
                <p className="text-gray-600">
                  All sellers must verify their identity to ensure trust and security
                </p>
              </div>
            </div>

            <div className="flex gap-4">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-green-100 text-green-600 rounded-full flex items-center justify-center font-bold">
                  ✓
                </div>
              </div>
              <div>
                <h4 className="font-semibold text-gray-900 mb-2">Listing Review</h4>
                <p className="text-gray-600">
                  Our team reviews photos, descriptions, and pricing for accuracy
                </p>
              </div>
            </div>

            <div className="flex gap-4">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-green-100 text-green-600 rounded-full flex items-center justify-center font-bold">
                  ✓
                </div>
              </div>
              <div>
                <h4 className="font-semibold text-gray-900 mb-2">Quality Assurance</h4>
                <p className="text-gray-600">
                  Listings must meet our standards for photos, information, and presentation
                </p>
              </div>
            </div>

            <div className="flex gap-4">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-green-100 text-green-600 rounded-full flex items-center justify-center font-bold">
                  ✓
                </div>
              </div>
              <div>
                <h4 className="font-semibold text-gray-900 mb-2">Ongoing Monitoring</h4>
                <p className="text-gray-600">
                  We continuously monitor listings and respond to user reports
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* FAQ Preview */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">Quick Questions</h2>
            <p className="text-gray-600">Common questions about our process</p>
          </div>

          <div className="max-w-3xl mx-auto space-y-4">
            <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200">
              <h4 className="font-semibold text-gray-900 mb-2">How long does verification take?</h4>
              <p className="text-gray-600">
                Most listings are reviewed and verified within 24 hours of submission.
              </p>
            </div>

            <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200">
              <h4 className="font-semibold text-gray-900 mb-2">Is it free to list a vehicle?</h4>
              <p className="text-gray-600">
                We offer both free and premium listing options. Premium listings get more visibility.
              </p>
            </div>

            <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200">
              <h4 className="font-semibold text-gray-900 mb-2">How do I contact a seller?</h4>
              <p className="text-gray-600">
                Simply click the "Contact Seller" button on any listing to start a secure conversation.
              </p>
            </div>
          </div>

          <div className="text-center mt-8">
            <Link to="/faq" className="text-primary hover:text-primary-600 font-medium">
              View All FAQs →
            </Link>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="bg-gradient-to-r from-primary to-secondary py-16 text-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl sm:text-4xl font-bold font-heading mb-6">
            Ready to Get Started?
          </h2>
          <p className="text-xl text-white/90 mb-8 max-w-2xl mx-auto">
            Whether you're buying or selling, we're here to help every step of the way
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link to="/browse">
              <Button variant="secondary" size="lg">
                Browse Cars
              </Button>
            </Link>
            <Link to="/sell">
              <Button variant="outline" size="lg" className="bg-white/10 border-white text-white hover:bg-white hover:text-primary">
                Sell Your Car
              </Button>
            </Link>
          </div>
        </div>
      </section>
    </MainLayout>
  );
}


import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import { Link } from 'react-router-dom';
import { FiCheckCircle, FiShield, FiUsers, FiHeart } from 'react-icons/fi';

export default function AboutPage() {
  return (
    <MainLayout>
      {/* Hero Section */}
      <div className="relative bg-gradient-to-br from-primary via-primary-600 to-secondary text-white py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-4xl sm:text-5xl font-bold font-heading mb-6">
              About CarDealer
            </h1>
            <p className="text-xl text-white/90 max-w-3xl mx-auto">
              Your trusted automotive marketplace connecting buyers and sellers nationwide
            </p>
          </div>
        </div>
      </div>

      {/* Mission Section */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="max-w-3xl mx-auto text-center">
            <h2 className="text-3xl font-bold text-gray-900 mb-6">Our Mission</h2>
            <p className="text-lg text-gray-600 mb-4">
              At CarDealer, we're revolutionizing the way people buy and sell vehicles. 
              Our mission is to create a transparent, secure, and efficient marketplace 
              that empowers both buyers and sellers.
            </p>
            <p className="text-lg text-gray-600">
              We believe that purchasing or selling a vehicle should be a straightforward 
              and enjoyable experience, free from complexity and uncertainty.
            </p>
          </div>
        </div>
      </section>

      {/* Values Section */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-gray-900 text-center mb-12">Our Values</h2>
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                <FiShield className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">Trust</h3>
              <p className="text-gray-600">
                Every listing is verified to ensure authenticity and reliability
              </p>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                <FiCheckCircle className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">Quality</h3>
              <p className="text-gray-600">
                We maintain high standards for all vehicles listed on our platform
              </p>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                <FiUsers className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">Community</h3>
              <p className="text-gray-600">
                Building lasting relationships between buyers, sellers, and dealers
              </p>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                <FiHeart className="text-primary" size={32} />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 mb-3">Service</h3>
              <p className="text-gray-600">
                Dedicated support team ready to assist you every step of the way
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-gray-900 text-center mb-12">Our Impact</h2>
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
      </section>

      {/* Story Section */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="max-w-3xl mx-auto">
            <h2 className="text-3xl font-bold text-gray-900 mb-6 text-center">Our Story</h2>
            <div className="space-y-4 text-gray-600">
              <p>
                Founded in 2020, CarDealer was born from a simple idea: buying and selling 
                vehicles should be easier, more transparent, and more secure. Our founders, 
                frustrated with the complexity of traditional car marketplaces, set out to 
                create a better solution.
              </p>
              <p>
                What started as a small team with a big vision has grown into one of the 
                most trusted automotive marketplaces in the country. We've helped thousands 
                of people find their dream vehicles and sellers reach qualified buyers.
              </p>
              <p>
                Today, we continue to innovate and improve our platform, always keeping our 
                users' needs at the forefront of everything we do. Our commitment to excellence 
                and customer satisfaction drives us forward every day.
              </p>
            </div>
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
            Join thousands of satisfied customers who trust CarDealer for their automotive needs
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


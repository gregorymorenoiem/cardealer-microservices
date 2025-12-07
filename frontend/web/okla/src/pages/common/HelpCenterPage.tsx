import MainLayout from '@/layouts/MainLayout';
import { Link } from 'react-router-dom';
import { FiSearch, FiBook, FiShoppingCart, FiDollarSign, FiShield, FiSettings, FiMessageSquare } from 'react-icons/fi';

export default function HelpCenterPage() {
  const categories = [
    {
      icon: FiShoppingCart,
      title: 'Buying a Vehicle',
      description: 'Learn how to search, contact sellers, and complete your purchase',
      articles: [
        'How to search for vehicles',
        'Understanding vehicle listings',
        'Contacting sellers',
        'Scheduling test drives',
        'Pre-purchase inspections',
      ],
    },
    {
      icon: FiDollarSign,
      title: 'Selling a Vehicle',
      description: 'Everything you need to know about listing your vehicle',
      articles: [
        'Creating your first listing',
        'Taking great photos',
        'Pricing your vehicle',
        'Managing inquiries',
        'Completing the sale',
      ],
    },
    {
      icon: FiSettings,
      title: 'Account Management',
      description: 'Manage your profile, preferences, and settings',
      articles: [
        'Creating an account',
        'Password reset',
        'Profile settings',
        'Email notifications',
        'Deleting your account',
      ],
    },
    {
      icon: FiShield,
      title: 'Trust & Safety',
      description: 'Stay safe while buying or selling',
      articles: [
        'Avoiding scams',
        'Meeting safely',
        'Reporting suspicious activity',
        'Payment best practices',
        'Vehicle history checks',
      ],
    },
    {
      icon: FiBook,
      title: 'Policies & Guidelines',
      description: 'Understanding our rules and regulations',
      articles: [
        'Terms of service',
        'Privacy policy',
        'Listing guidelines',
        'Prohibited items',
        'Dispute resolution',
      ],
    },
    {
      icon: FiMessageSquare,
      title: 'Messaging & Communication',
      description: 'Using our secure messaging system',
      articles: [
        'Sending messages',
        'Message etiquette',
        'Blocking users',
        'Notification settings',
        'Message history',
      ],
    },
  ];

  return (
    <MainLayout>
      {/* Hero Section */}
      <div className="relative bg-gradient-to-br from-primary via-primary-600 to-secondary text-white py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-4xl sm:text-5xl font-bold font-heading mb-6">
              Help Center
            </h1>
            <p className="text-xl text-white/90 max-w-3xl mx-auto mb-8">
              Find answers and get help with CarDealer
            </p>

            {/* Search Bar */}
            <div className="max-w-2xl mx-auto">
              <div className="relative">
                <FiSearch className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
                <input
                  type="text"
                  placeholder="Search help articles..."
                  className="w-full pl-12 pr-4 py-4 rounded-lg text-gray-900 focus:ring-2 focus:ring-white focus:outline-none"
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Categories Grid */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-gray-900 text-center mb-12">
            Browse by Category
          </h2>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {categories.map((category, index) => (
              <div
                key={index}
                className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow"
              >
                <div className="w-12 h-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
                  <category.icon className="text-primary" size={24} />
                </div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                  {category.title}
                </h3>
                <p className="text-gray-600 mb-4">{category.description}</p>
                <ul className="space-y-2">
                  {category.articles.map((article, articleIndex) => (
                    <li key={articleIndex}>
                      <a
                        href="#"
                        className="text-sm text-gray-600 hover:text-primary transition-colors"
                      >
                        {article}
                      </a>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Popular Articles */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-gray-900 text-center mb-12">
            Popular Articles
          </h2>

          <div className="max-w-3xl mx-auto space-y-4">
            {[
              'How do I verify a vehicle\'s history?',
              'What payment methods are safe to use?',
              'How long does listing approval take?',
              'Can I edit my listing after it\'s published?',
              'How do I report a suspicious listing?',
              'What should I bring to a test drive?',
              'How do I delete my account?',
              'What fees do you charge?',
            ].map((article, index) => (
              <a
                key={index}
                href="#"
                className="block bg-gray-50 hover:bg-gray-100 rounded-lg p-4 transition-colors border border-gray-200"
              >
                <div className="flex items-center justify-between">
                  <span className="text-gray-900 font-medium">{article}</span>
                  <span className="text-gray-400">→</span>
                </div>
              </a>
            ))}
          </div>
        </div>
      </section>

      {/* Still Need Help */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl font-bold text-gray-900 mb-4">
            Still Need Help?
          </h2>
          <p className="text-lg text-gray-600 mb-8">
            Our support team is available Monday through Friday, 9 AM - 6 PM
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              to="/contact"
              className="inline-block px-6 py-3 bg-primary text-white font-medium rounded-lg hover:bg-primary-600 transition-colors"
            >
              Contact Support
            </Link>
            <Link
              to="/faq"
              className="inline-block px-6 py-3 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors"
            >
              View FAQs
            </Link>
          </div>
        </div>
      </section>
    </MainLayout>
  );
}


import { useState } from 'react';
import MainLayout from '@/layouts/MainLayout';
import { FiChevronDown, FiSearch } from 'react-icons/fi';

interface FAQItem {
  question: string;
  answer: string;
  category: string;
}

const faqs: FAQItem[] = [
  // Buying
  {
    category: 'Buying',
    question: 'How do I search for vehicles?',
    answer: 'Use our search bar on the homepage or browse page. You can filter by make, model, year, price range, mileage, location, and more to find exactly what you\'re looking for.',
  },
  {
    category: 'Buying',
    question: 'Are all listings verified?',
    answer: 'Yes, our team reviews every listing before it goes live. We verify seller identity, check photos for quality, and ensure all information is accurate.',
  },
  {
    category: 'Buying',
    question: 'How do I contact a seller?',
    answer: 'Simply click the "Contact Seller" button on any listing. You\'ll need to create a free account to send messages. All communication happens through our secure messaging system.',
  },
  {
    category: 'Buying',
    question: 'Can I schedule a test drive?',
    answer: 'Absolutely! Use our messaging system to arrange a test drive with the seller. We recommend meeting in safe, public locations.',
  },
  {
    category: 'Buying',
    question: 'What should I check before buying?',
    answer: 'Always inspect the vehicle in person, take it for a test drive, check the VIN history report, verify ownership documents, and consider getting a pre-purchase inspection from a mechanic.',
  },
  
  // Selling
  {
    category: 'Selling',
    question: 'How do I list my vehicle?',
    answer: 'Click "Sell Your Car" in the navigation menu, create an account if you haven\'t already, and fill out our simple listing form with your vehicle details and photos.',
  },
  {
    category: 'Selling',
    question: 'How long does it take to get approved?',
    answer: 'Most listings are reviewed and approved within 24 hours. You\'ll receive an email notification once your listing is live.',
  },
  {
    category: 'Selling',
    question: 'How much does it cost to list?',
    answer: 'We offer a free Basic plan for individual sellers. Premium plans starting at $49 offer enhanced visibility and additional features. Check our Pricing page for details.',
  },
  {
    category: 'Selling',
    question: 'What photos should I include?',
    answer: 'Include clear photos of the exterior (all angles), interior, engine bay, dashboard showing mileage, and any notable features or imperfections. More photos lead to more inquiries!',
  },
  {
    category: 'Selling',
    question: 'Can I edit my listing after it\'s published?',
    answer: 'Yes! You can edit your listing details, update photos, or change the price at any time from your dashboard.',
  },
  {
    category: 'Selling',
    question: 'How long will my listing stay active?',
    answer: 'Listing duration depends on your plan: 30 days for Basic, 60 days for Premium, and 90 days for Dealer plans. You can renew anytime.',
  },

  // Account
  {
    category: 'Account',
    question: 'How do I create an account?',
    answer: 'Click "Sign Up" in the top right corner, enter your email, create a password, and verify your email address. It takes less than a minute!',
  },
  {
    category: 'Account',
    question: 'I forgot my password. What do I do?',
    answer: 'Click "Forgot Password" on the login page, enter your email, and we\'ll send you a reset link.',
  },
  {
    category: 'Account',
    question: 'Can I delete my account?',
    answer: 'Yes, you can delete your account from your profile settings. Note that this will permanently remove all your data and active listings.',
  },
  {
    category: 'Account',
    question: 'How do I save my favorite vehicles?',
    answer: 'Click the heart icon on any vehicle listing to add it to your wishlist. You can view all saved vehicles in your dashboard.',
  },

  // Payment
  {
    category: 'Payment',
    question: 'What payment methods do you accept?',
    answer: 'We accept all major credit cards (Visa, MasterCard, American Express), PayPal, and bank transfers for Dealer plans.',
  },
  {
    category: 'Payment',
    question: 'Is my payment information secure?',
    answer: 'Yes, we use industry-standard encryption and never store your full payment details. All transactions are processed through secure payment gateways.',
  },
  {
    category: 'Payment',
    question: 'Can I get a refund?',
    answer: 'We offer a 7-day money-back guarantee for Premium and Dealer plans. If you\'re not satisfied, contact support for a full refund.',
  },
  {
    category: 'Payment',
    question: 'Do you charge transaction fees?',
    answer: 'No, we don\'t charge any transaction fees between buyers and sellers. You only pay for listing plans if you choose a premium option.',
  },

  // Trust & Safety
  {
    category: 'Trust & Safety',
    question: 'How do I avoid scams?',
    answer: 'Never send money before seeing the vehicle. Meet in public places, verify seller identity, check vehicle history, and trust your instincts. Report suspicious listings immediately.',
  },
  {
    category: 'Trust & Safety',
    question: 'What if a listing seems suspicious?',
    answer: 'Use the "Report" button on any listing you find suspicious. Our team will investigate and take appropriate action.',
  },
  {
    category: 'Trust & Safety',
    question: 'Do you offer buyer protection?',
    answer: 'While we verify listings, transactions happen directly between buyers and sellers. We recommend thorough inspections, vehicle history checks, and meeting in safe locations.',
  },
];

export default function FAQPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('All');
  const [openIndex, setOpenIndex] = useState<number | null>(null);

  const categories = ['All', 'Buying', 'Selling', 'Account', 'Payment', 'Trust & Safety'];

  const filteredFaqs = faqs.filter((faq) => {
    const matchesCategory = selectedCategory === 'All' || faq.category === selectedCategory;
    const matchesSearch =
      searchQuery === '' ||
      faq.question.toLowerCase().includes(searchQuery.toLowerCase()) ||
      faq.answer.toLowerCase().includes(searchQuery.toLowerCase());
    return matchesCategory && matchesSearch;
  });

  return (
    <MainLayout>
      {/* Hero Section */}
      <div className="relative bg-gradient-to-br from-primary via-primary-600 to-secondary text-white py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-4xl sm:text-5xl font-bold font-heading mb-6">
              Frequently Asked Questions
            </h1>
            <p className="text-xl text-white/90 max-w-3xl mx-auto">
              Find answers to common questions about buying and selling vehicles
            </p>
          </div>
        </div>
      </div>

      {/* Search and Filter */}
      <section className="py-12 bg-white border-b border-gray-200">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Search Bar */}
          <div className="relative mb-6">
            <FiSearch className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
            <input
              type="text"
              placeholder="Search questions..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-12 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
            />
          </div>

          {/* Category Filters */}
          <div className="flex flex-wrap gap-2">
            {categories.map((category) => (
              <button
                key={category}
                onClick={() => setSelectedCategory(category)}
                className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                  selectedCategory === category
                    ? 'bg-primary text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                {category}
              </button>
            ))}
          </div>
        </div>
      </section>

      {/* FAQ List */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          {filteredFaqs.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-600 text-lg">
                No questions found matching your search.
              </p>
            </div>
          ) : (
            <div className="space-y-3">
              {filteredFaqs.map((faq, index) => (
                <div
                  key={index}
                  className="bg-white rounded-lg border border-gray-200 overflow-hidden"
                >
                  <button
                    onClick={() => setOpenIndex(openIndex === index ? null : index)}
                    className="w-full px-6 py-4 flex items-center justify-between text-left hover:bg-gray-50 transition-colors"
                  >
                    <div className="flex-1">
                      <span className="inline-block px-2 py-1 text-xs font-semibold text-primary bg-primary/10 rounded mr-3">
                        {faq.category}
                      </span>
                      <span className="font-semibold text-gray-900">{faq.question}</span>
                    </div>
                    <FiChevronDown
                      className={`flex-shrink-0 text-gray-400 transition-transform ${
                        openIndex === index ? 'transform rotate-180' : ''
                      }`}
                      size={20}
                    />
                  </button>
                  {openIndex === index && (
                    <div className="px-6 pb-4 pt-2">
                      <p className="text-gray-600 leading-relaxed">{faq.answer}</p>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </section>

      {/* Still Have Questions */}
      <section className="py-16 bg-white">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl font-bold text-gray-900 mb-4">
            Still Have Questions?
          </h2>
          <p className="text-lg text-gray-600 mb-8">
            Can't find what you're looking for? Our support team is here to help.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <a
              href="/contact"
              className="inline-block px-6 py-3 bg-primary text-white font-medium rounded-lg hover:bg-primary-600 transition-colors"
            >
              Contact Support
            </a>
            <a
              href="/help"
              className="inline-block px-6 py-3 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors"
            >
              Visit Help Center
            </a>
          </div>
        </div>
      </section>
    </MainLayout>
  );
}


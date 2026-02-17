import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import { Link } from 'react-router-dom';
import { FiCheck, FiStar } from 'react-icons/fi';

export default function PricingPage() {
  const plans = [
    {
      name: 'Basic',
      price: 0,
      period: 'Free',
      description: 'Perfect for individual sellers',
      features: [
        '1 active listing',
        '30 days duration',
        'Up to 5 photos',
        'Basic support',
        'Standard visibility',
      ],
      notIncluded: [
        'Featured badge',
        'Priority placement',
        'Analytics dashboard',
        'Dedicated support',
      ],
      cta: 'Get Started',
      popular: false,
    },
    {
      name: 'Premium',
      price: 49,
      period: 'per listing',
      description: 'Most popular for serious sellers',
      features: [
        '5 active listings',
        '60 days duration',
        'Up to 20 photos per listing',
        'Featured badge',
        'Priority placement',
        'Priority support',
        'Analytics dashboard',
        'Social media sharing',
      ],
      notIncluded: [
        'Dedicated account manager',
        'API access',
      ],
      cta: 'Start Premium',
      popular: true,
    },
    {
      name: 'Dealer',
      price: 199,
      period: 'per month',
      description: 'For professional dealers',
      features: [
        'Unlimited listings',
        '90 days duration',
        'Unlimited photos',
        'Featured badge on all listings',
        'Top priority placement',
        'Dedicated support',
        'Advanced analytics',
        'Social media integration',
        'API access',
        'Custom branding',
        'Bulk upload tools',
      ],
      notIncluded: [],
      cta: 'Contact Sales',
      popular: false,
    },
  ];

  return (
    <MainLayout>
      {/* Hero Section */}
      <div className="relative bg-gradient-to-br from-primary via-primary-600 to-secondary text-white py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-4xl sm:text-5xl font-bold font-heading mb-6">
              Simple, Transparent Pricing
            </h1>
            <p className="text-xl text-white/90 max-w-3xl mx-auto">
              Choose the plan that works best for you. No hidden fees, cancel anytime.
            </p>
          </div>
        </div>
      </div>

      {/* Pricing Cards */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid md:grid-cols-3 gap-8 lg:gap-12">
            {plans.map((plan) => (
              <div
                key={plan.name}
                className={`bg-white rounded-2xl shadow-lg overflow-hidden ${
                  plan.popular ? 'ring-2 ring-primary transform lg:scale-105' : ''
                }`}
              >
                {plan.popular && (
                  <div className="bg-primary text-white text-center py-2 font-semibold flex items-center justify-center gap-2">
                    <FiStar size={16} />
                    Most Popular
                  </div>
                )}

                <div className="p-8">
                  <h3 className="text-2xl font-bold text-gray-900 mb-2">{plan.name}</h3>
                  <p className="text-gray-600 mb-6">{plan.description}</p>

                  <div className="mb-6">
                    <span className="text-5xl font-bold text-gray-900">${plan.price}</span>
                    {plan.price > 0 && (
                      <span className="text-gray-600 ml-2">/{plan.period}</span>
                    )}
                    {plan.price === 0 && (
                      <span className="text-gray-600 ml-2">Forever</span>
                    )}
                  </div>

                  <Link to="/sell">
                    <Button
                      variant={plan.popular ? 'primary' : 'outline'}
                      size="lg"
                      className="w-full mb-6"
                    >
                      {plan.cta}
                    </Button>
                  </Link>

                  <div className="space-y-4">
                    <p className="font-semibold text-gray-900 text-sm uppercase tracking-wide">
                      What's included:
                    </p>
                    {plan.features.map((feature, index) => (
                      <div key={index} className="flex items-start gap-3">
                        <div className="flex-shrink-0">
                          <div className="w-5 h-5 bg-green-100 text-green-600 rounded-full flex items-center justify-center">
                            <FiCheck size={14} />
                          </div>
                        </div>
                        <span className="text-gray-700 text-sm">{feature}</span>
                      </div>
                    ))}

                    {plan.notIncluded.length > 0 && (
                      <>
                        <div className="pt-4 border-t border-gray-200">
                          <p className="font-semibold text-gray-900 text-sm uppercase tracking-wide mb-4">
                            Not included:
                          </p>
                          {plan.notIncluded.map((feature, index) => (
                            <div key={index} className="flex items-start gap-3 mb-3 opacity-50">
                              <div className="flex-shrink-0">
                                <div className="w-5 h-5 border-2 border-gray-300 rounded-full"></div>
                              </div>
                              <span className="text-gray-500 text-sm line-through">{feature}</span>
                            </div>
                          ))}
                        </div>
                      </>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Features Comparison */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-gray-900 text-center mb-12">
            Compare All Features
          </h2>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="text-left py-4 px-4 font-semibold text-gray-900">Feature</th>
                  <th className="text-center py-4 px-4 font-semibold text-gray-900">Basic</th>
                  <th className="text-center py-4 px-4 font-semibold text-gray-900">Premium</th>
                  <th className="text-center py-4 px-4 font-semibold text-gray-900">Dealer</th>
                </tr>
              </thead>
              <tbody>
                <tr className="border-b border-gray-100">
                  <td className="py-4 px-4 text-gray-700">Active Listings</td>
                  <td className="py-4 px-4 text-center text-gray-700">1</td>
                  <td className="py-4 px-4 text-center text-gray-700">5</td>
                  <td className="py-4 px-4 text-center text-gray-700">Unlimited</td>
                </tr>
                <tr className="border-b border-gray-100">
                  <td className="py-4 px-4 text-gray-700">Listing Duration</td>
                  <td className="py-4 px-4 text-center text-gray-700">30 days</td>
                  <td className="py-4 px-4 text-center text-gray-700">60 days</td>
                  <td className="py-4 px-4 text-center text-gray-700">90 days</td>
                </tr>
                <tr className="border-b border-gray-100">
                  <td className="py-4 px-4 text-gray-700">Photos per Listing</td>
                  <td className="py-4 px-4 text-center text-gray-700">5</td>
                  <td className="py-4 px-4 text-center text-gray-700">20</td>
                  <td className="py-4 px-4 text-center text-gray-700">Unlimited</td>
                </tr>
                <tr className="border-b border-gray-100">
                  <td className="py-4 px-4 text-gray-700">Featured Badge</td>
                  <td className="py-4 px-4 text-center">-</td>
                  <td className="py-4 px-4 text-center text-green-600">
                    <FiCheck className="inline" />
                  </td>
                  <td className="py-4 px-4 text-center text-green-600">
                    <FiCheck className="inline" />
                  </td>
                </tr>
                <tr className="border-b border-gray-100">
                  <td className="py-4 px-4 text-gray-700">Priority Placement</td>
                  <td className="py-4 px-4 text-center">-</td>
                  <td className="py-4 px-4 text-center text-green-600">
                    <FiCheck className="inline" />
                  </td>
                  <td className="py-4 px-4 text-center text-green-600">
                    <FiCheck className="inline" />
                  </td>
                </tr>
                <tr className="border-b border-gray-100">
                  <td className="py-4 px-4 text-gray-700">Analytics Dashboard</td>
                  <td className="py-4 px-4 text-center">-</td>
                  <td className="py-4 px-4 text-center text-green-600">
                    <FiCheck className="inline" />
                  </td>
                  <td className="py-4 px-4 text-center text-green-600">
                    <FiCheck className="inline" />
                  </td>
                </tr>
                <tr className="border-b border-gray-100">
                  <td className="py-4 px-4 text-gray-700">API Access</td>
                  <td className="py-4 px-4 text-center">-</td>
                  <td className="py-4 px-4 text-center">-</td>
                  <td className="py-4 px-4 text-center text-green-600">
                    <FiCheck className="inline" />
                  </td>
                </tr>
                <tr className="border-b border-gray-100">
                  <td className="py-4 px-4 text-gray-700">Dedicated Support</td>
                  <td className="py-4 px-4 text-center">-</td>
                  <td className="py-4 px-4 text-center">-</td>
                  <td className="py-4 px-4 text-center text-green-600">
                    <FiCheck className="inline" />
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </section>

      {/* FAQ Section */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-gray-900 text-center mb-12">
            Pricing FAQs
          </h2>

          <div className="space-y-6">
            <div>
              <h4 className="font-semibold text-gray-900 mb-2">Can I upgrade my plan later?</h4>
              <p className="text-gray-600">
                Yes! You can upgrade to a higher tier plan at any time. The difference will be prorated.
              </p>
            </div>

            <div>
              <h4 className="font-semibold text-gray-900 mb-2">What payment methods do you accept?</h4>
              <p className="text-gray-600">
                We accept all major credit cards, PayPal, and bank transfers for Dealer plans.
              </p>
            </div>

            <div>
              <h4 className="font-semibold text-gray-900 mb-2">Is there a refund policy?</h4>
              <p className="text-gray-600">
                We offer a 7-day money-back guarantee for Premium and Dealer plans if you're not satisfied.
              </p>
            </div>

            <div>
              <h4 className="font-semibold text-gray-900 mb-2">Do listings expire?</h4>
              <p className="text-gray-600">
                Yes, listings expire based on your plan duration. You can renew or relist at any time.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="bg-gradient-to-r from-primary to-secondary py-16 text-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl sm:text-4xl font-bold font-heading mb-6">
            Ready to List Your Vehicle?
          </h2>
          <p className="text-xl text-white/90 mb-8 max-w-2xl mx-auto">
            Choose your plan and start reaching thousands of potential buyers today
          </p>
          <Link to="/sell">
            <Button variant="secondary" size="lg">
              Get Started Now
            </Button>
          </Link>
        </div>
      </section>
    </MainLayout>
  );
}


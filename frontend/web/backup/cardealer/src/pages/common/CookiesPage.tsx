import MainLayout from '@/layouts/MainLayout';
import { Link } from 'react-router-dom';
import { useState } from 'react';

export default function CookiesPage() {
  const [cookiePreferences, setCookiePreferences] = useState({
    necessary: true,
    functional: true,
    analytics: true,
    advertising: false,
  });

  const handleToggle = (category: keyof typeof cookiePreferences) => {
    if (category === 'necessary') return; // Necessary cookies cannot be disabled
    setCookiePreferences((prev) => ({
      ...prev,
      [category]: !prev[category],
    }));
  };

  const handleSavePreferences = () => {
    console.log('Saving cookie preferences:', cookiePreferences);
    alert('Your cookie preferences have been saved!');
  };

  return (
    <MainLayout>
      {/* Hero */}
      <div className="bg-gradient-to-br from-primary via-primary-600 to-secondary text-white py-12">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <h1 className="text-4xl font-bold font-heading mb-4">Cookie Policy</h1>
          <p className="text-lg text-white/90">
            Last updated: {new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}
          </p>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="prose prose-lg max-w-none">
          {/* Introduction */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">1. What Are Cookies?</h2>
            <p className="text-gray-600 leading-relaxed">
              Cookies are small text files that are placed on your device when you visit our website. They help us provide you with a better experience by remembering your preferences, analyzing how you use our Service, and personalizing content.
            </p>
            <p className="text-gray-600 leading-relaxed mt-4">
              This Cookie Policy explains what cookies are, how we use them, and how you can control them.
            </p>
          </section>

          {/* Types of Cookies */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">2. Types of Cookies We Use</h2>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">2.1 Necessary Cookies</h3>
            <p className="text-gray-600 leading-relaxed mb-4">
              These cookies are essential for the website to function properly. They enable core functionality such as security, authentication, and accessibility. The website cannot function properly without these cookies.
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Authentication and session management</li>
              <li>Security and fraud prevention</li>
              <li>Remembering your cookie preferences</li>
              <li>Load balancing</li>
            </ul>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">2.2 Functional Cookies</h3>
            <p className="text-gray-600 leading-relaxed mb-4">
              These cookies enable enhanced functionality and personalization, such as remembering your preferences and settings.
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Language preferences</li>
              <li>Location settings</li>
              <li>Recently viewed listings</li>
              <li>Saved searches and filters</li>
            </ul>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">2.3 Analytics Cookies</h3>
            <p className="text-gray-600 leading-relaxed mb-4">
              These cookies help us understand how visitors interact with our website by collecting and reporting information anonymously.
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Google Analytics</li>
              <li>Page views and navigation patterns</li>
              <li>Session duration and bounce rate</li>
              <li>Device and browser information</li>
            </ul>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">2.4 Advertising Cookies</h3>
            <p className="text-gray-600 leading-relaxed mb-4">
              These cookies are used to deliver relevant advertisements and track advertising campaign performance.
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Google Ads and remarketing</li>
              <li>Facebook Pixel</li>
              <li>Interest-based advertising</li>
              <li>Conversion tracking</li>
            </ul>
          </section>

          {/* First-Party vs Third-Party */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">3. First-Party vs. Third-Party Cookies</h2>
            
            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">3.1 First-Party Cookies</h3>
            <p className="text-gray-600 leading-relaxed">
              These are cookies set by CarDealer directly. They are used to provide core functionality and improve your experience on our website.
            </p>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">3.2 Third-Party Cookies</h3>
            <p className="text-gray-600 leading-relaxed">
              These are cookies set by third-party services we use, such as Google Analytics, social media platforms, and advertising networks. These cookies are subject to the respective privacy policies of these external services.
            </p>
          </section>

          {/* Cookie Duration */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">4. Cookie Duration</h2>
            
            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">4.1 Session Cookies</h3>
            <p className="text-gray-600 leading-relaxed">
              These temporary cookies are deleted when you close your browser. They are used to maintain your session as you navigate through the website.
            </p>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">4.2 Persistent Cookies</h3>
            <p className="text-gray-600 leading-relaxed">
              These cookies remain on your device for a set period or until you manually delete them. They help us remember your preferences and settings across multiple visits.
            </p>
          </section>

          {/* Specific Cookies */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">5. Specific Cookies We Use</h2>
            <div className="overflow-x-auto">
              <table className="min-w-full bg-white border border-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-gray-900 border-b">Cookie Name</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-gray-900 border-b">Purpose</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-gray-900 border-b">Type</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-gray-900 border-b">Duration</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  <tr>
                    <td className="px-4 py-3 text-sm text-gray-900">_session</td>
                    <td className="px-4 py-3 text-sm text-gray-600">User authentication</td>
                    <td className="px-4 py-3 text-sm text-gray-600">Necessary</td>
                    <td className="px-4 py-3 text-sm text-gray-600">Session</td>
                  </tr>
                  <tr>
                    <td className="px-4 py-3 text-sm text-gray-900">_preferences</td>
                    <td className="px-4 py-3 text-sm text-gray-600">User settings</td>
                    <td className="px-4 py-3 text-sm text-gray-600">Functional</td>
                    <td className="px-4 py-3 text-sm text-gray-600">1 year</td>
                  </tr>
                  <tr>
                    <td className="px-4 py-3 text-sm text-gray-900">_ga</td>
                    <td className="px-4 py-3 text-sm text-gray-600">Google Analytics</td>
                    <td className="px-4 py-3 text-sm text-gray-600">Analytics</td>
                    <td className="px-4 py-3 text-sm text-gray-600">2 years</td>
                  </tr>
                  <tr>
                    <td className="px-4 py-3 text-sm text-gray-900">_fbp</td>
                    <td className="px-4 py-3 text-sm text-gray-600">Facebook Pixel</td>
                    <td className="px-4 py-3 text-sm text-gray-600">Advertising</td>
                    <td className="px-4 py-3 text-sm text-gray-600">3 months</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          {/* Cookie Consent */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">6. Your Cookie Choices</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              You have the right to accept or reject cookies. You can manage your cookie preferences using the controls below or through your browser settings.
            </p>

            {/* Cookie Preferences Panel */}
            <div className="bg-gray-50 rounded-xl p-6 border border-gray-200 my-8">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Manage Cookie Preferences</h3>
              
              <div className="space-y-4">
                {/* Necessary Cookies */}
                <div className="flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200">
                  <div className="flex-1">
                    <h4 className="font-medium text-gray-900">Necessary Cookies</h4>
                    <p className="text-sm text-gray-600">Required for the website to function</p>
                  </div>
                  <div className="text-sm text-gray-500">Always Active</div>
                </div>

                {/* Functional Cookies */}
                <div className="flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200">
                  <div className="flex-1">
                    <h4 className="font-medium text-gray-900">Functional Cookies</h4>
                    <p className="text-sm text-gray-600">Remember your preferences</p>
                  </div>
                  <button
                    onClick={() => handleToggle('functional')}
                    className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                      cookiePreferences.functional ? 'bg-primary' : 'bg-gray-300'
                    }`}
                  >
                    <span
                      className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                        cookiePreferences.functional ? 'translate-x-6' : 'translate-x-1'
                      }`}
                    />
                  </button>
                </div>

                {/* Analytics Cookies */}
                <div className="flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200">
                  <div className="flex-1">
                    <h4 className="font-medium text-gray-900">Analytics Cookies</h4>
                    <p className="text-sm text-gray-600">Help us improve our service</p>
                  </div>
                  <button
                    onClick={() => handleToggle('analytics')}
                    className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                      cookiePreferences.analytics ? 'bg-primary' : 'bg-gray-300'
                    }`}
                  >
                    <span
                      className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                        cookiePreferences.analytics ? 'translate-x-6' : 'translate-x-1'
                      }`}
                    />
                  </button>
                </div>

                {/* Advertising Cookies */}
                <div className="flex items-center justify-between p-4 bg-white rounded-lg border border-gray-200">
                  <div className="flex-1">
                    <h4 className="font-medium text-gray-900">Advertising Cookies</h4>
                    <p className="text-sm text-gray-600">Show you relevant ads</p>
                  </div>
                  <button
                    onClick={() => handleToggle('advertising')}
                    className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                      cookiePreferences.advertising ? 'bg-primary' : 'bg-gray-300'
                    }`}
                  >
                    <span
                      className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                        cookiePreferences.advertising ? 'translate-x-6' : 'translate-x-1'
                      }`}
                    />
                  </button>
                </div>
              </div>

              <button
                onClick={handleSavePreferences}
                className="mt-6 w-full px-6 py-3 bg-primary text-white font-medium rounded-lg hover:bg-primary-600 transition-colors"
              >
                Save Preferences
              </button>
            </div>
          </section>

          {/* Browser Controls */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">7. Browser Controls</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              Most web browsers allow you to control cookies through their settings. You can:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Block all cookies</li>
              <li>Block third-party cookies</li>
              <li>Delete cookies after each session</li>
              <li>View and delete specific cookies</li>
            </ul>
            <p className="text-gray-600 leading-relaxed mt-4">
              Please note that blocking or deleting cookies may impact your experience and some features may not function properly.
            </p>
            <p className="text-gray-600 leading-relaxed mt-4">
              For instructions on managing cookies in your browser:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600 mt-2">
              <li><a href="https://support.google.com/chrome/answer/95647" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">Chrome</a></li>
              <li><a href="https://support.mozilla.org/en-US/kb/cookies-information-websites-store-on-your-computer" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">Firefox</a></li>
              <li><a href="https://support.apple.com/guide/safari/manage-cookies-sfri11471/mac" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">Safari</a></li>
              <li><a href="https://support.microsoft.com/en-us/microsoft-edge/delete-cookies-in-microsoft-edge-63947406-40ac-c3b8-57b9-2a946a29ae09" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">Edge</a></li>
            </ul>
          </section>

          {/* Do Not Track */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">8. Do Not Track Signals</h2>
            <p className="text-gray-600 leading-relaxed">
              Some browsers offer a "Do Not Track" (DNT) signal. Currently, there is no universal standard for how to respond to DNT signals. We do not currently respond to DNT signals, but we provide you with tools to manage your cookie preferences.
            </p>
          </section>

          {/* Changes */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">9. Changes to This Policy</h2>
            <p className="text-gray-600 leading-relaxed">
              We may update this Cookie Policy from time to time. Any changes will be posted on this page with an updated "Last updated" date.
            </p>
          </section>

          {/* Contact */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">10. Contact Us</h2>
            <p className="text-gray-600 leading-relaxed">
              If you have questions about our use of cookies, please contact us:
            </p>
            <ul className="list-none space-y-2 text-gray-600 mt-4">
              <li><strong>Email:</strong> privacy@cardealer.com</li>
              <li><strong>Phone:</strong> 1-800-CAR-DEAL</li>
              <li><strong>Address:</strong> 123 Auto Plaza, Los Angeles, CA 90001</li>
            </ul>
          </section>
        </div>

        {/* CTA Section */}
        <div className="mt-16 bg-gray-50 rounded-xl p-8 text-center">
          <h3 className="text-2xl font-bold text-gray-900 mb-4">
            Learn More About Your Privacy
          </h3>
          <p className="text-gray-600 mb-6">
            Read our comprehensive privacy documentation
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              to="/privacy"
              className="inline-block px-6 py-3 bg-primary text-white font-medium rounded-lg hover:bg-primary-600 transition-colors"
            >
              Privacy Policy
            </Link>
            <Link
              to="/contact"
              className="inline-block px-6 py-3 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors"
            >
              Contact Us
            </Link>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}


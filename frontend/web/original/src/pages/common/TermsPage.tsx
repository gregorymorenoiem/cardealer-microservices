import MainLayout from '@/layouts/MainLayout';
import { Link } from 'react-router-dom';

export default function TermsPage() {
  return (
    <MainLayout>
      {/* Hero */}
      <div className="bg-gradient-to-br from-primary via-primary-600 to-secondary text-white py-12">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <h1 className="text-4xl font-bold font-heading mb-4">Terms of Service</h1>
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
            <h2 className="text-2xl font-bold text-gray-900 mb-4">1. Introduction</h2>
            <p className="text-gray-600 leading-relaxed">
              Welcome to CarDealer ("we," "our," or "us"). These Terms of Service ("Terms") govern your access to and use of our website, mobile application, and services (collectively, the "Service"). By accessing or using the Service, you agree to be bound by these Terms.
            </p>
            <p className="text-gray-600 leading-relaxed mt-4">
              If you do not agree to these Terms, you may not access or use the Service. We reserve the right to modify these Terms at any time, and your continued use of the Service constitutes acceptance of any changes.
            </p>
          </section>

          {/* Account Registration */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">2. Account Registration</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              To use certain features of the Service, you must register for an account. When you register, you agree to:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Provide accurate, current, and complete information</li>
              <li>Maintain and promptly update your account information</li>
              <li>Maintain the security of your password and account</li>
              <li>Accept responsibility for all activities that occur under your account</li>
              <li>Notify us immediately of any unauthorized use of your account</li>
            </ul>
            <p className="text-gray-600 leading-relaxed mt-4">
              You must be at least 18 years old to create an account. We reserve the right to refuse service, terminate accounts, or remove content at our sole discretion.
            </p>
          </section>

          {/* User Conduct */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">3. User Conduct</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              You agree not to use the Service to:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Post false, inaccurate, misleading, or fraudulent content</li>
              <li>Impersonate any person or entity</li>
              <li>Harass, threaten, or harm other users</li>
              <li>Violate any applicable laws or regulations</li>
              <li>Infringe upon intellectual property rights</li>
              <li>Transmit viruses, malware, or malicious code</li>
              <li>Attempt to gain unauthorized access to the Service</li>
              <li>Use automated systems (bots, scrapers) without permission</li>
              <li>Interfere with or disrupt the Service</li>
            </ul>
          </section>

          {/* Listings */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">4. Vehicle Listings</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              When you create a vehicle listing, you agree that:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>You have the legal right to sell the vehicle</li>
              <li>All information provided is accurate and complete</li>
              <li>Photos are of the actual vehicle being sold</li>
              <li>The vehicle is not stolen or subject to any liens (unless disclosed)</li>
              <li>You will comply with all applicable laws regarding the sale</li>
              <li>You will respond promptly to legitimate inquiries</li>
            </ul>
            <p className="text-gray-600 leading-relaxed mt-4">
              We reserve the right to remove any listing that violates these Terms or our listing guidelines. We do not guarantee the accuracy of listings and are not responsible for disputes between buyers and sellers.
            </p>
          </section>

          {/* Transactions */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">5. Transactions</h2>
            <p className="text-gray-600 leading-relaxed">
              CarDealer is a platform that connects buyers and sellers. We are not a party to any transaction between users. You are solely responsible for:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600 mt-4">
              <li>Verifying the identity of other users</li>
              <li>Inspecting vehicles before purchase</li>
              <li>Negotiating terms of sale</li>
              <li>Completing payment and transfer of ownership</li>
              <li>Complying with all applicable laws and regulations</li>
            </ul>
            <p className="text-gray-600 leading-relaxed mt-4">
              We strongly recommend conducting transactions in person and using secure payment methods. Never wire money to strangers or share sensitive financial information.
            </p>
          </section>

          {/* Fees */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">6. Fees and Payment</h2>
            <p className="text-gray-600 leading-relaxed">
              Certain features of the Service may require payment of fees. By purchasing a subscription or service, you agree to pay all applicable fees. Fees are non-refundable except as required by law or as explicitly stated in these Terms.
            </p>
            <p className="text-gray-600 leading-relaxed mt-4">
              We reserve the right to change our fees at any time. Any fee changes will be communicated to you in advance and will apply to future billing periods.
            </p>
          </section>

          {/* Intellectual Property */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">7. Intellectual Property</h2>
            <p className="text-gray-600 leading-relaxed">
              The Service and its original content, features, and functionality are owned by CarDealer and are protected by international copyright, trademark, patent, trade secret, and other intellectual property laws.
            </p>
            <p className="text-gray-600 leading-relaxed mt-4">
              You retain all rights to content you post on the Service. By posting content, you grant us a worldwide, non-exclusive, royalty-free license to use, reproduce, modify, and display such content in connection with the Service.
            </p>
          </section>

          {/* Disclaimers */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">8. Disclaimers</h2>
            <p className="text-gray-600 leading-relaxed">
              THE SERVICE IS PROVIDED "AS IS" AND "AS AVAILABLE" WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED. WE DO NOT WARRANT THAT THE SERVICE WILL BE UNINTERRUPTED, SECURE, OR ERROR-FREE.
            </p>
            <p className="text-gray-600 leading-relaxed mt-4">
              WE DO NOT VERIFY THE ACCURACY OF USER-GENERATED CONTENT AND MAKE NO REPRESENTATIONS OR WARRANTIES REGARDING THE QUALITY, SAFETY, OR LEGALITY OF VEHICLES LISTED ON THE SERVICE.
            </p>
          </section>

          {/* Limitation of Liability */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">9. Limitation of Liability</h2>
            <p className="text-gray-600 leading-relaxed">
              TO THE MAXIMUM EXTENT PERMITTED BY LAW, CARDEALER SHALL NOT BE LIABLE FOR ANY INDIRECT, INCIDENTAL, SPECIAL, CONSEQUENTIAL, OR PUNITIVE DAMAGES, OR ANY LOSS OF PROFITS OR REVENUES, WHETHER INCURRED DIRECTLY OR INDIRECTLY.
            </p>
            <p className="text-gray-600 leading-relaxed mt-4">
              OUR TOTAL LIABILITY FOR ALL CLAIMS RELATED TO THE SERVICE SHALL NOT EXCEED THE AMOUNT YOU PAID US IN THE TWELVE MONTHS PRIOR TO THE EVENT GIVING RISE TO LIABILITY.
            </p>
          </section>

          {/* Indemnification */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">10. Indemnification</h2>
            <p className="text-gray-600 leading-relaxed">
              You agree to indemnify and hold harmless CarDealer and its officers, directors, employees, and agents from any claims, losses, damages, liabilities, and expenses (including attorneys' fees) arising out of your use of the Service, your violation of these Terms, or your violation of any rights of another.
            </p>
          </section>

          {/* Dispute Resolution */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">11. Dispute Resolution</h2>
            <p className="text-gray-600 leading-relaxed">
              Any disputes arising from these Terms or your use of the Service shall be resolved through binding arbitration in accordance with the rules of the American Arbitration Association. You waive any right to participate in a class action lawsuit or class-wide arbitration.
            </p>
          </section>

          {/* Termination */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">12. Termination</h2>
            <p className="text-gray-600 leading-relaxed">
              We may terminate or suspend your account and access to the Service immediately, without prior notice or liability, for any reason, including if you breach these Terms. Upon termination, your right to use the Service will cease immediately.
            </p>
          </section>

          {/* Governing Law */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">13. Governing Law</h2>
            <p className="text-gray-600 leading-relaxed">
              These Terms shall be governed by and construed in accordance with the laws of the State of California, without regard to its conflict of law provisions.
            </p>
          </section>

          {/* Contact */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">14. Contact Information</h2>
            <p className="text-gray-600 leading-relaxed">
              If you have any questions about these Terms, please contact us at:
            </p>
            <ul className="list-none space-y-2 text-gray-600 mt-4">
              <li><strong>Email:</strong> legal@cardealer.com</li>
              <li><strong>Phone:</strong> 1-800-CAR-DEAL</li>
              <li><strong>Address:</strong> 123 Auto Plaza, Los Angeles, CA 90001</li>
            </ul>
          </section>
        </div>

        {/* CTA Section */}
        <div className="mt-16 bg-gray-50 rounded-xl p-8 text-center">
          <h3 className="text-2xl font-bold text-gray-900 mb-4">
            Have Questions?
          </h3>
          <p className="text-gray-600 mb-6">
            Our legal team is here to help clarify any concerns
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              to="/contact"
              className="inline-block px-6 py-3 bg-primary text-white font-medium rounded-lg hover:bg-primary-600 transition-colors"
            >
              Contact Legal Team
            </Link>
            <Link
              to="/privacy"
              className="inline-block px-6 py-3 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors"
            >
              Privacy Policy
            </Link>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}


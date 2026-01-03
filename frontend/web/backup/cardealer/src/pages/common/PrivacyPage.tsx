import MainLayout from '@/layouts/MainLayout';
import { Link } from 'react-router-dom';

export default function PrivacyPage() {
  return (
    <MainLayout>
      {/* Hero */}
      <div className="bg-gradient-to-br from-primary via-primary-600 to-secondary text-white py-12">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <h1 className="text-4xl font-bold font-heading mb-4">Privacy Policy</h1>
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
              CarDealer ("we," "our," or "us") is committed to protecting your privacy. This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you use our website, mobile application, and services (collectively, the "Service").
            </p>
            <p className="text-gray-600 leading-relaxed mt-4">
              By using the Service, you consent to the practices described in this Privacy Policy. If you do not agree with our policies and practices, please do not use the Service.
            </p>
          </section>

          {/* Information We Collect */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">2. Information We Collect</h2>
            
            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">2.1 Information You Provide</h3>
            <p className="text-gray-600 leading-relaxed mb-4">
              We collect information you provide directly to us, including:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Account registration information (name, email, password, phone number)</li>
              <li>Profile information (photo, location, preferences)</li>
              <li>Vehicle listing information (descriptions, photos, specifications)</li>
              <li>Messages and communications with other users</li>
              <li>Payment and billing information</li>
              <li>Identity verification documents</li>
              <li>Customer support inquiries and correspondence</li>
            </ul>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">2.2 Information Collected Automatically</h3>
            <p className="text-gray-600 leading-relaxed mb-4">
              When you use the Service, we automatically collect certain information, including:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Device information (IP address, browser type, operating system)</li>
              <li>Usage data (pages visited, time spent, features used)</li>
              <li>Location information (if you grant permission)</li>
              <li>Cookies and similar tracking technologies</li>
              <li>Log data (access times, error logs)</li>
            </ul>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">2.3 Information from Third Parties</h3>
            <p className="text-gray-600 leading-relaxed">
              We may receive information from third parties, such as social media platforms (if you connect your account), identity verification services, payment processors, and analytics providers.
            </p>
          </section>

          {/* How We Use Your Information */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">3. How We Use Your Information</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              We use the information we collect to:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Provide, maintain, and improve the Service</li>
              <li>Process transactions and send related information</li>
              <li>Create and manage your account</li>
              <li>Facilitate communication between buyers and sellers</li>
              <li>Send you technical notices and support messages</li>
              <li>Respond to your inquiries and provide customer support</li>
              <li>Monitor and analyze usage patterns and trends</li>
              <li>Detect, prevent, and address fraud and security issues</li>
              <li>Comply with legal obligations</li>
              <li>Send you promotional communications (with your consent)</li>
              <li>Personalize your experience and show relevant content</li>
            </ul>
          </section>

          {/* How We Share Your Information */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">4. How We Share Your Information</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              We may share your information in the following circumstances:
            </p>
            
            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">4.1 With Other Users</h3>
            <p className="text-gray-600 leading-relaxed">
              When you create a listing or communicate with other users, certain information (such as your name, profile photo, and listing details) will be visible to other users.
            </p>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">4.2 With Service Providers</h3>
            <p className="text-gray-600 leading-relaxed">
              We share information with third-party vendors who perform services on our behalf, such as payment processing, data analysis, email delivery, hosting services, and customer support.
            </p>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">4.3 For Legal Reasons</h3>
            <p className="text-gray-600 leading-relaxed">
              We may disclose information if required by law or in response to legal processes, to protect our rights, or to ensure the safety of our users.
            </p>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">4.4 Business Transfers</h3>
            <p className="text-gray-600 leading-relaxed">
              If we are involved in a merger, acquisition, or sale of assets, your information may be transferred as part of that transaction.
            </p>

            <h3 className="text-xl font-semibold text-gray-900 mb-3 mt-6">4.5 With Your Consent</h3>
            <p className="text-gray-600 leading-relaxed">
              We may share information with third parties when you give us permission to do so.
            </p>
          </section>

          {/* Your Rights and Choices */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">5. Your Rights and Choices</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              You have certain rights regarding your personal information:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li><strong>Access:</strong> You can request a copy of the personal information we hold about you</li>
              <li><strong>Correction:</strong> You can update or correct your account information at any time</li>
              <li><strong>Deletion:</strong> You can request deletion of your account and personal information</li>
              <li><strong>Opt-Out:</strong> You can opt out of marketing communications by following unsubscribe links</li>
              <li><strong>Cookies:</strong> You can control cookies through your browser settings</li>
              <li><strong>Location Data:</strong> You can disable location services in your device settings</li>
            </ul>
            <p className="text-gray-600 leading-relaxed mt-4">
              To exercise these rights, please contact us at privacy@cardealer.com. We will respond to your request within 30 days.
            </p>
          </section>

          {/* Data Security */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">6. Data Security</h2>
            <p className="text-gray-600 leading-relaxed">
              We implement appropriate technical and organizational measures to protect your personal information against unauthorized access, alteration, disclosure, or destruction. These measures include:
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600 mt-4">
              <li>Encryption of data in transit and at rest</li>
              <li>Regular security assessments and penetration testing</li>
              <li>Access controls and authentication mechanisms</li>
              <li>Employee training on data protection</li>
              <li>Incident response procedures</li>
            </ul>
            <p className="text-gray-600 leading-relaxed mt-4">
              However, no method of transmission over the internet is 100% secure. While we strive to protect your information, we cannot guarantee absolute security.
            </p>
          </section>

          {/* Data Retention */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">7. Data Retention</h2>
            <p className="text-gray-600 leading-relaxed">
              We retain your personal information for as long as necessary to provide the Service and fulfill the purposes described in this Privacy Policy. When you delete your account, we will delete or anonymize your information, except where we are required to retain it for legal or legitimate business purposes.
            </p>
          </section>

          {/* Children's Privacy */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">8. Children's Privacy</h2>
            <p className="text-gray-600 leading-relaxed">
              The Service is not intended for children under 18. We do not knowingly collect personal information from children. If you believe we have collected information from a child, please contact us immediately.
            </p>
          </section>

          {/* International Transfers */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">9. International Data Transfers</h2>
            <p className="text-gray-600 leading-relaxed">
              Your information may be transferred to and processed in countries other than your own. These countries may have different data protection laws. By using the Service, you consent to such transfers.
            </p>
          </section>

          {/* GDPR Rights */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">10. GDPR Rights (European Users)</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              If you are located in the European Economic Area, you have additional rights under the General Data Protection Regulation (GDPR):
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Right to access your personal data</li>
              <li>Right to rectification of inaccurate data</li>
              <li>Right to erasure ("right to be forgotten")</li>
              <li>Right to restrict processing</li>
              <li>Right to data portability</li>
              <li>Right to object to processing</li>
              <li>Right to withdraw consent</li>
              <li>Right to lodge a complaint with a supervisory authority</li>
            </ul>
          </section>

          {/* CCPA Rights */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">11. CCPA Rights (California Users)</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              If you are a California resident, you have additional rights under the California Consumer Privacy Act (CCPA):
            </p>
            <ul className="list-disc pl-6 space-y-2 text-gray-600">
              <li>Right to know what personal information is collected</li>
              <li>Right to know if personal information is sold or disclosed</li>
              <li>Right to opt-out of the sale of personal information</li>
              <li>Right to deletion of personal information</li>
              <li>Right to non-discrimination for exercising CCPA rights</li>
            </ul>
            <p className="text-gray-600 leading-relaxed mt-4">
              We do not sell your personal information to third parties.
            </p>
          </section>

          {/* Changes to Privacy Policy */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">12. Changes to This Privacy Policy</h2>
            <p className="text-gray-600 leading-relaxed">
              We may update this Privacy Policy from time to time. We will notify you of any material changes by posting the new Privacy Policy on this page and updating the "Last updated" date. You are advised to review this Privacy Policy periodically.
            </p>
          </section>

          {/* Contact */}
          <section className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">13. Contact Us</h2>
            <p className="text-gray-600 leading-relaxed">
              If you have questions or concerns about this Privacy Policy, please contact us:
            </p>
            <ul className="list-none space-y-2 text-gray-600 mt-4">
              <li><strong>Email:</strong> privacy@cardealer.com</li>
              <li><strong>Phone:</strong> 1-800-CAR-DEAL</li>
              <li><strong>Address:</strong> 123 Auto Plaza, Los Angeles, CA 90001</li>
              <li><strong>Data Protection Officer:</strong> dpo@cardealer.com</li>
            </ul>
          </section>
        </div>

        {/* CTA Section */}
        <div className="mt-16 bg-gray-50 rounded-xl p-8 text-center">
          <h3 className="text-2xl font-bold text-gray-900 mb-4">
            Questions About Your Privacy?
          </h3>
          <p className="text-gray-600 mb-6">
            We're here to help you understand how we protect your data
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              to="/contact"
              className="inline-block px-6 py-3 bg-primary text-white font-medium rounded-lg hover:bg-primary-600 transition-colors"
            >
              Contact Privacy Team
            </Link>
            <Link
              to="/cookies"
              className="inline-block px-6 py-3 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors"
            >
              Cookie Policy
            </Link>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}


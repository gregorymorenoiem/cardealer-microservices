import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { FiFacebook, FiTwitter, FiInstagram, FiLinkedin, FiMail, FiPhone, FiMapPin } from 'react-icons/fi';
import { LanguageSwitcher } from '@/components/common';

export default function Footer() {
  const { t } = useTranslation('common');
  const currentYear = new Date().getFullYear();

  return (
    <footer className="bg-gray-900 text-gray-300">
      {/* Main Footer Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
          {/* About Section */}
          <div>
            <div className="flex items-center gap-2 mb-4">
              <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-xl">M</span>
              </div>
              <span className="text-xl font-bold font-heading text-white">Marketplace</span>
            </div>
            <p className="text-sm text-gray-400 mb-4">
              {t('footer.description')}
            </p>
            <div className="flex gap-3">
              <a
                href="https://facebook.com"
                target="_blank"
                rel="noopener noreferrer"
                className="w-9 h-9 bg-gray-800 hover:bg-primary rounded-lg flex items-center justify-center transition-colors"
                aria-label="Facebook"
              >
                <FiFacebook size={18} />
              </a>
              <a
                href="https://twitter.com"
                target="_blank"
                rel="noopener noreferrer"
                className="w-9 h-9 bg-gray-800 hover:bg-primary rounded-lg flex items-center justify-center transition-colors"
                aria-label="Twitter"
              >
                <FiTwitter size={18} />
              </a>
              <a
                href="https://instagram.com"
                target="_blank"
                rel="noopener noreferrer"
                className="w-9 h-9 bg-gray-800 hover:bg-primary rounded-lg flex items-center justify-center transition-colors"
                aria-label="Instagram"
              >
                <FiInstagram size={18} />
              </a>
              <a
                href="https://linkedin.com"
                target="_blank"
                rel="noopener noreferrer"
                className="w-9 h-9 bg-gray-800 hover:bg-primary rounded-lg flex items-center justify-center transition-colors"
                aria-label="LinkedIn"
              >
                <FiLinkedin size={18} />
              </a>
            </div>
          </div>

          {/* Quick Links */}
          <div>
            <h3 className="text-white font-semibold mb-4">{t('footer.quickLinks')}</h3>
            <ul className="space-y-2">
              <li>
                <Link to="/vehicles" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.browseVehicles')}
                </Link>
              </li>
              <li>
                <Link to="/properties" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.browseProperties')}
                </Link>
              </li>
              <li>
                <Link to="/sell" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.sell')}
                </Link>
              </li>
              <li>
                <Link to="/about" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.about')}
                </Link>
              </li>
              <li>
                <Link to="/how-it-works" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.howItWorks')}
                </Link>
              </li>
              <li>
                <Link to="/faq" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.faq')}
                </Link>
              </li>
            </ul>
          </div>

          {/* Support */}
          <div>
            <h3 className="text-white font-semibold mb-4">{t('footer.support')}</h3>
            <ul className="space-y-2">
              <li>
                <Link to="/contact" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.contact')}
                </Link>
              </li>
              <li>
                <Link to="/help" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.helpCenter')}
                </Link>
              </li>
              <li>
                <Link to="/terms" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.terms')}
                </Link>
              </li>
              <li>
                <Link to="/privacy" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.privacy')}
                </Link>
              </li>
              <li>
                <Link to="/cookies" className="text-sm hover:text-white transition-colors">
                  {t('footer.links.cookies')}
                </Link>
              </li>
            </ul>
          </div>

          {/* Contact Info */}
          <div>
            <h3 className="text-white font-semibold mb-4">{t('footer.contactInfo')}</h3>
            <ul className="space-y-3">
              <li className="flex items-start gap-3">
                <FiMapPin className="flex-shrink-0 mt-1 text-primary" size={18} />
                <span className="text-sm">
                  Santo Domingo, República Dominicana
                </span>
              </li>
              <li className="flex items-center gap-3">
                <FiPhone className="flex-shrink-0 text-primary" size={18} />
                <a href="tel:+18095551234" className="text-sm hover:text-white transition-colors">
                  +1 (809) 555-1234
                </a>
              </li>
              <li className="flex items-center gap-3">
                <FiMail className="flex-shrink-0 text-primary" size={18} />
                <a href="mailto:soporte@marketplace.do" className="text-sm hover:text-white transition-colors">
                  soporte@marketplace.do
                </a>
              </li>
            </ul>
            <div className="mt-4">
              <p className="text-xs text-gray-400">
                {t('footer.schedule.weekdays')}<br />
                {t('footer.schedule.saturday')}<br />
                {t('footer.schedule.sunday')}
              </p>
            </div>
            
            {/* Language Switcher in Footer */}
            <div className="mt-4 pt-4 border-t border-gray-800">
              <p className="text-xs text-gray-500 mb-2">{t('footer.language')}</p>
              <LanguageSwitcher variant="inline" />
            </div>
          </div>
        </div>
      </div>

      {/* Bottom Bar */}
      <div className="border-t border-gray-800">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex flex-col md:flex-row justify-between items-center gap-4">
            <p className="text-sm text-gray-400">
              © {currentYear} Marketplace. {t('footer.allRightsReserved')}
            </p>
            <div className="flex gap-6">
              <Link to="/sitemap" className="text-xs text-gray-400 hover:text-white transition-colors">
                {t('footer.links.sitemap')}
              </Link>
              <Link to="/accessibility" className="text-xs text-gray-400 hover:text-white transition-colors">
                {t('footer.links.accessibility')}
              </Link>
              <Link to="/careers" className="text-xs text-gray-400 hover:text-white transition-colors">
                {t('footer.links.careers')}
              </Link>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}

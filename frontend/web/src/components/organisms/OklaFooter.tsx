import React from 'react';
import { Link } from 'react-router-dom';
import { clsx } from 'clsx';
import {
  Facebook,
  Instagram,
  Twitter,
  Youtube,
  Linkedin,
  Mail,
  Phone,
  Shield,
  Award,
} from 'lucide-react';
import { OklaButton } from '../atoms/okla/OklaButton';

/**
 * OKLA Footer Component
 * 
 * Footer profesional multi-columna con información de contacto,
 * navegación, newsletter y elementos de confianza.
 */

interface FooterLink {
  label: string;
  href: string;
  external?: boolean;
}

interface FooterSection {
  title: string;
  links: FooterLink[];
}

const footerSections: FooterSection[] = [
  {
    title: 'Marketplace',
    links: [
      { label: 'Explorar Todo', href: '/browse' },
      { label: 'Vehículos', href: '/vehicles' },
      { label: 'Propiedades', href: '/properties' },
      { label: 'Ofertas del Día', href: '/deals' },
      { label: 'Nuevos Anuncios', href: '/new' },
    ],
  },
  {
    title: 'Vendedor',
    links: [
      { label: 'Publicar Anuncio', href: '/sell' },
      { label: 'Planes y Precios', href: '/pricing' },
      { label: 'Guía del Vendedor', href: '/seller-guide' },
      { label: 'Panel de Control', href: '/dashboard' },
      { label: 'Estadísticas', href: '/analytics' },
    ],
  },
  {
    title: 'Soporte',
    links: [
      { label: 'Centro de Ayuda', href: '/help' },
      { label: 'Contacto', href: '/contact' },
      { label: 'FAQ', href: '/faq' },
      { label: 'Reportar Problema', href: '/report' },
      { label: 'Sugerencias', href: '/feedback' },
    ],
  },
  {
    title: 'Legal',
    links: [
      { label: 'Términos de Uso', href: '/terms' },
      { label: 'Política de Privacidad', href: '/privacy' },
      { label: 'Política de Cookies', href: '/cookies' },
      { label: 'Licencias', href: '/licenses' },
    ],
  },
];

const socialLinks = [
  { icon: Facebook, href: 'https://facebook.com/okla', label: 'Facebook' },
  { icon: Instagram, href: 'https://instagram.com/okla', label: 'Instagram' },
  { icon: Twitter, href: 'https://twitter.com/okla', label: 'Twitter' },
  { icon: Youtube, href: 'https://youtube.com/okla', label: 'YouTube' },
  { icon: Linkedin, href: 'https://linkedin.com/company/okla', label: 'LinkedIn' },
];

const paymentMethods = [
  { name: 'Visa', icon: '💳' },
  { name: 'Mastercard', icon: '💳' },
  { name: 'PayPal', icon: '💳' },
  { name: 'Apple Pay', icon: '🍎' },
  { name: 'Google Pay', icon: '📱' },
];

export interface OklaFooterProps {
  className?: string;
}

export const OklaFooter: React.FC<OklaFooterProps> = ({ className }) => {
  const [email, setEmail] = React.useState('');
  const currentYear = new Date().getFullYear();

  const handleNewsletterSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // Handle newsletter subscription
    console.log('Newsletter subscription:', email);
    setEmail('');
  };

  return (
    <footer 
      className={clsx(
        'bg-gray-900 text-white',
        'dark:bg-gray-950',
        className
      )}
    >
      {/* Main Footer Content */}
      <div className="container mx-auto px-4 lg:px-8 py-16">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-10">
          {/* Brand & Newsletter */}
          <div className="lg:col-span-2">
            {/* Logo */}
            <Link to="/" className="inline-flex items-center gap-2 mb-6">
              <div 
                className={clsx(
                  'w-10 h-10 rounded-lg',
                  'bg-gradient-to-br from-gold-400 to-gold-600',
                  'flex items-center justify-center'
                )}
              >
                <span className="text-lg font-bold text-gray-900">O</span>
              </div>
              <span className="text-2xl font-heading font-bold">
                OKLA
              </span>
            </Link>

            <p className="text-gray-300 dark:text-gray-400 mb-6 max-w-xs">
              El marketplace premium donde compradores y vendedores se conectan con confianza y elegancia.
            </p>

            {/* Newsletter */}
            <div className="mb-8">
              <h4 className="text-sm font-semibold uppercase tracking-wider mb-3 text-gold-400">
                Newsletter
              </h4>
              <p className="text-sm text-gray-300 dark:text-gray-400 mb-4">
                Recibe las mejores ofertas y novedades directamente en tu correo.
              </p>
              <form onSubmit={handleNewsletterSubmit} className="flex gap-2">
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="tu@email.com"
                  required
                  className={clsx(
                    'flex-1 h-11 px-4',
                    'bg-white/10 border-0 rounded-lg',
                    'text-white placeholder:text-gray-400',
                    'focus:outline-none focus:ring-2 focus:ring-gold-500/50'
                  )}
                />
                <OklaButton type="submit" variant="gold" size="md">
                  Suscribir
                </OklaButton>
              </form>
            </div>

            {/* Social Links */}
            <div>
              <h4 className="text-sm font-semibold uppercase tracking-wider mb-3 text-gold-400">
                Síguenos
              </h4>
              <div className="flex items-center gap-3">
                {socialLinks.map((social) => (
                  <a
                    key={social.label}
                    href={social.href}
                    target="_blank"
                    rel="noopener noreferrer"
                    aria-label={social.label}
                    className={clsx(
                      'w-10 h-10 rounded-full',
                      'bg-white/10',
                      'flex items-center justify-center',
                      'text-gray-300 hover:text-white',
                      'hover:bg-gold-500',
                      'transition-all duration-200'
                    )}
                  >
                    <social.icon className="w-5 h-5" />
                  </a>
                ))}
              </div>
            </div>
          </div>

          {/* Footer Links */}
          {footerSections.map((section) => (
            <div key={section.title}>
              <h4 className="text-sm font-semibold uppercase tracking-wider mb-4 text-gold-400">
                {section.title}
              </h4>
              <ul className="space-y-3">
                {section.links.map((link) => (
                  <li key={link.label}>
                    {link.external ? (
                      <a
                        href={link.href}
                        target="_blank"
                        rel="noopener noreferrer"
                        className={clsx(
                          'text-sm text-gray-300 dark:text-gray-400',
                          'hover:text-gold-400',
                          'transition-colors duration-150'
                        )}
                      >
                        {link.label}
                      </a>
                    ) : (
                      <Link
                        to={link.href}
                        className={clsx(
                          'text-sm text-gray-300 dark:text-gray-400',
                          'hover:text-gold-400',
                          'transition-colors duration-150'
                        )}
                      >
                        {link.label}
                      </Link>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>

      {/* Trust Section */}
      <div className="border-t border-white/10">
        <div className="container mx-auto px-4 lg:px-8 py-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {/* Trust Badges */}
            <div className="flex flex-col items-center md:items-start">
              <h4 className="text-sm font-semibold uppercase tracking-wider mb-4 text-gold-400">
                Compra Segura
              </h4>
              <div className="flex items-center gap-4">
                <div className="flex items-center gap-2 text-gray-300">
                  <Shield className="w-5 h-5 text-gold-400" />
                  <span className="text-sm">SSL Seguro</span>
                </div>
                <div className="flex items-center gap-2 text-gray-300">
                  <Award className="w-5 h-5 text-gold-400" />
                  <span className="text-sm">Verificado</span>
                </div>
              </div>
            </div>

            {/* Payment Methods */}
            <div className="flex flex-col items-center">
              <h4 className="text-sm font-semibold uppercase tracking-wider mb-4 text-gold-400">
                Métodos de Pago
              </h4>
              <div className="flex items-center gap-2">
                {paymentMethods.map((method) => (
                  <div
                    key={method.name}
                    className={clsx(
                      'w-12 h-8 rounded',
                      'bg-white/10',
                      'flex items-center justify-center',
                      'text-lg'
                    )}
                    title={method.name}
                  >
                    {method.icon}
                  </div>
                ))}
              </div>
            </div>

            {/* Contact Info */}
            <div className="flex flex-col items-center md:items-end">
              <h4 className="text-sm font-semibold uppercase tracking-wider mb-4 text-gold-400">
                Contacto
              </h4>
              <div className="space-y-2 text-right">
                <a 
                  href="mailto:info@okla.com"
                  className="flex items-center gap-2 text-sm text-gray-300 hover:text-gold-400 transition-colors"
                >
                  <Mail className="w-4 h-4" />
                  info@okla.com
                </a>
                <a 
                  href="tel:+1234567890"
                  className="flex items-center gap-2 text-sm text-gray-300 hover:text-gold-400 transition-colors"
                >
                  <Phone className="w-4 h-4" />
                  +1 (234) 567-890
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Bottom Bar */}
      <div className="border-t border-white/10">
        <div className="container mx-auto px-4 lg:px-8 py-6">
          <div className="flex flex-col md:flex-row items-center justify-between gap-4">
            <p className="text-sm text-gray-400">
              © {currentYear} OKLA. Todos los derechos reservados.
            </p>
            <div className="flex items-center gap-6">
              <Link 
                to="/terms" 
                className="text-sm text-gray-400 hover:text-gold-400 transition-colors"
              >
                Términos
              </Link>
              <Link 
                to="/privacy" 
                className="text-sm text-gray-400 hover:text-gold-400 transition-colors"
              >
                Privacidad
              </Link>
              <Link 
                to="/cookies" 
                className="text-sm text-gray-400 hover:text-gold-400 transition-colors"
              >
                Cookies
              </Link>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default OklaFooter;


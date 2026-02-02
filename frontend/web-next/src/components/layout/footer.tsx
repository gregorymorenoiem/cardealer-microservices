import * as React from 'react';
import Link from 'next/link';
import { Facebook, Instagram, Twitter, Youtube, Mail, Phone, MapPin } from 'lucide-react';

const footerLinks = {
  marketplace: {
    title: 'Marketplace',
    links: [
      { href: '/vehiculos', label: 'Buscar Vehículos' },
      { href: '/vender', label: 'Vender tu Vehículo' },
      { href: '/dealers', label: 'Para Dealers' },
      { href: '/precios', label: 'Guía de Precios' },
      { href: '/comparar', label: 'Comparar Vehículos' },
    ],
  },
  company: {
    title: 'Compañía',
    links: [
      { href: '/nosotros', label: 'Sobre OKLA' },
      { href: '/contacto', label: 'Contacto' },
      { href: '/blog', label: 'Blog' },
      { href: '/prensa', label: 'Prensa' },
      { href: '/empleos', label: 'Empleos' },
    ],
  },
  legal: {
    title: 'Legal',
    links: [
      { href: '/terminos', label: 'Términos de Servicio' },
      { href: '/privacidad', label: 'Política de Privacidad' },
      { href: '/cookies', label: 'Política de Cookies' },
      { href: '/seguridad', label: 'Seguridad' },
    ],
  },
  support: {
    title: 'Soporte',
    links: [
      { href: '/ayuda', label: 'Centro de Ayuda' },
      { href: '/faq', label: 'Preguntas Frecuentes' },
      { href: '/guias', label: 'Guías de Compra' },
      { href: '/reportar', label: 'Reportar Fraude' },
    ],
  },
};

const socialLinks = [
  { href: 'https://facebook.com/okla', icon: Facebook, label: 'Facebook' },
  { href: 'https://instagram.com/okla', icon: Instagram, label: 'Instagram' },
  { href: 'https://twitter.com/okla', icon: Twitter, label: 'Twitter' },
  { href: 'https://youtube.com/okla', icon: Youtube, label: 'YouTube' },
];

export function Footer() {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="border-t border-gray-200 bg-gray-50">
      {/* Main Footer */}
      <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid grid-cols-2 gap-8 md:grid-cols-4 lg:grid-cols-5">
          {/* Brand Column */}
          <div className="col-span-2 md:col-span-4 lg:col-span-1">
            <Link href="/" className="flex items-center gap-2">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-[#00A870]">
                <span className="text-xl font-bold text-white">O</span>
              </div>
              <span className="text-2xl font-bold text-gray-900">OKLA</span>
            </Link>
            <p className="mt-4 text-sm text-gray-600">
              El marketplace de vehículos #1 de República Dominicana. Compra y vende con confianza.
            </p>

            {/* Contact Info */}
            <div className="mt-6 space-y-2">
              <a
                href="mailto:info@okla.com.do"
                className="flex items-center gap-2 text-sm text-gray-600 hover:text-[#00A870]"
              >
                <Mail className="h-4 w-4" />
                info@okla.com.do
              </a>
              <a
                href="tel:+18095551234"
                className="flex items-center gap-2 text-sm text-gray-600 hover:text-[#00A870]"
              >
                <Phone className="h-4 w-4" />
                (809) 555-1234
              </a>
              <p className="flex items-center gap-2 text-sm text-gray-600">
                <MapPin className="h-4 w-4" />
                Santo Domingo, RD
              </p>
            </div>

            {/* Social Links */}
            <div className="mt-6 flex gap-3">
              {socialLinks.map(social => (
                <a
                  key={social.label}
                  href={social.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex h-9 w-9 items-center justify-center rounded-lg bg-gray-200 text-gray-600 transition-colors hover:bg-[#00A870] hover:text-white"
                  aria-label={social.label}
                >
                  <social.icon className="h-4 w-4" />
                </a>
              ))}
            </div>
          </div>

          {/* Links Columns */}
          {Object.entries(footerLinks).map(([key, section]) => (
            <div key={key}>
              <h3 className="text-sm font-semibold text-gray-900">{section.title}</h3>
              <ul className="mt-4 space-y-2">
                {section.links.map(link => (
                  <li key={link.href}>
                    <Link
                      href={link.href}
                      className="text-sm text-gray-600 transition-colors hover:text-[#00A870]"
                    >
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>

      {/* Bottom Bar */}
      <div className="border-t border-gray-200 bg-gray-100">
        <div className="mx-auto flex max-w-7xl flex-col items-center justify-between gap-4 px-4 py-6 sm:flex-row sm:px-6 lg:px-8">
          <p className="text-sm text-gray-600">
            © {currentYear} OKLA. Todos los derechos reservados.
          </p>
          <div className="flex gap-6">
            <Link href="/terminos" className="text-sm text-gray-600 hover:text-[#00A870]">
              Términos
            </Link>
            <Link href="/privacidad" className="text-sm text-gray-600 hover:text-[#00A870]">
              Privacidad
            </Link>
            <Link href="/cookies" className="text-sm text-gray-600 hover:text-[#00A870]">
              Cookies
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
}

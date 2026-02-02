'use client';

import Link from 'next/link';
import { ArrowRight } from 'lucide-react';

interface HeroStaticProps {
  title?: string;
  highlight?: string;
  description?: string;
  primaryCTA?: {
    label: string;
    href: string;
  };
  secondaryCTA?: {
    label: string;
    href: string;
  };
}

const defaultProps: Required<HeroStaticProps> = {
  title: 'Tu próximo vehículo está en',
  highlight: 'OKLA',
  description:
    'El marketplace de vehículos #1 de República Dominicana. Compra y vende carros nuevos y usados con total confianza.',
  primaryCTA: {
    label: 'Explorar Vehículos',
    href: '/vehiculos',
  },
  secondaryCTA: {
    label: 'Vender mi Vehículo',
    href: '/vender',
  },
};

export function HeroStatic({
  title = defaultProps.title,
  highlight = defaultProps.highlight,
  description = defaultProps.description,
  primaryCTA = defaultProps.primaryCTA,
  secondaryCTA = defaultProps.secondaryCTA,
}: HeroStaticProps) {
  return (
    <section className="relative h-[calc(100vh-4rem)] overflow-hidden bg-gradient-to-br from-gray-900 to-gray-800">
      {/* Background pattern */}
      <div className="absolute inset-0 bg-[url('/hero-pattern.svg')] opacity-10" />

      {/* Content */}
      <div className="relative mx-auto flex h-full max-w-7xl items-center px-4 py-16 sm:px-6 lg:px-8">
        <div className="max-w-2xl">
          {/* Title */}
          <h1 className="text-4xl font-bold tracking-tight text-white sm:text-5xl lg:text-7xl">
            {title} <span className="text-[#00A870]">{highlight}</span>
          </h1>

          {/* Description */}
          <p className="mt-6 text-lg text-gray-300 sm:text-xl">{description}</p>

          {/* CTAs */}
          <div className="mt-8 flex flex-col gap-4 sm:flex-row">
            <Link
              href={primaryCTA.href}
              className="inline-flex h-14 items-center justify-center gap-2 rounded-lg bg-[#00A870] px-8 text-lg font-semibold text-white shadow-lg transition-all hover:bg-[#009663] hover:shadow-xl"
            >
              {primaryCTA.label}
              <ArrowRight className="h-5 w-5" />
            </Link>
            <Link
              href={secondaryCTA.href}
              className="inline-flex h-14 items-center justify-center gap-2 rounded-lg border-2 border-white bg-transparent px-8 text-lg font-semibold text-white transition-all hover:bg-white hover:text-gray-900"
            >
              {secondaryCTA.label}
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}

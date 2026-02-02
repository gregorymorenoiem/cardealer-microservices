'use client';

import Link from 'next/link';
import { motion } from 'framer-motion';
import { ArrowRight, Car, Camera, Shield, TrendingUp } from 'lucide-react';
import { cn } from '@/lib/utils';

interface CTAButton {
  label: string;
  href: string;
  variant?: 'primary' | 'secondary';
}

interface CTASectionProps {
  title: string;
  subtitle?: string;
  primaryButton: CTAButton;
  secondaryButton?: CTAButton;
  className?: string;
  variant?: 'simple' | 'premium';
}

// =============================================================================
// PREMIUM VARIANT - WITH VISUAL ELEMENTS
// =============================================================================

function PremiumCTA({
  title,
  subtitle,
  primaryButton,
  secondaryButton,
  className,
}: CTASectionProps) {
  const benefits = [
    { icon: Camera, text: 'Fotos profesionales gratis' },
    { icon: TrendingUp, text: 'Precio de mercado sugerido' },
    { icon: Shield, text: 'Vendedores verificados' },
  ];

  return (
    <section
      className={cn(
        'relative overflow-hidden bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 py-20 lg:py-28',
        className
      )}
    >
      {/* Animated Background */}
      <div className="absolute inset-0 overflow-hidden">
        <motion.div
          className="absolute -top-40 -right-40 h-96 w-96 rounded-full bg-[#00A870]/20 blur-3xl"
          animate={{ scale: [1, 1.2, 1], opacity: [0.3, 0.5, 0.3] }}
          transition={{ duration: 8, repeat: Infinity, ease: 'easeInOut' }}
        />
        <motion.div
          className="absolute -bottom-40 -left-40 h-80 w-80 rounded-full bg-[#00A870]/10 blur-3xl"
          animate={{ scale: [1.2, 1, 1.2], opacity: [0.2, 0.4, 0.2] }}
          transition={{ duration: 10, repeat: Infinity, ease: 'easeInOut' }}
        />
        {/* Grid Pattern */}
        <div
          className="absolute inset-0 opacity-[0.03]"
          style={{
            backgroundImage: `
              linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px),
              linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px)
            `,
            backgroundSize: '60px 60px',
          }}
        />
      </div>

      <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center gap-12 lg:flex-row lg:gap-20">
          {/* Content */}
          <div className="flex-1 text-center lg:text-left">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
            >
              {/* Badge */}
              <span className="mb-6 inline-flex items-center gap-2 rounded-full bg-[#00A870]/20 px-4 py-1.5 text-sm font-semibold tracking-wide text-[#00A870]">
                <Car className="h-4 w-4" />
                Publica Gratis
              </span>

              <h2 className="mb-6 text-4xl leading-tight font-bold tracking-tight text-white lg:text-5xl">
                {title}
              </h2>

              {subtitle && (
                <p className="mx-auto mb-8 max-w-lg text-lg leading-relaxed text-white/80 lg:mx-0">
                  {subtitle}
                </p>
              )}

              {/* Benefits */}
              <div className="mb-8 flex flex-wrap justify-center gap-4 lg:justify-start">
                {benefits.map((benefit, index) => (
                  <motion.div
                    key={benefit.text}
                    initial={{ opacity: 0, x: -20 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: index * 0.1 }}
                    className="flex items-center gap-2 text-white/90"
                  >
                    <benefit.icon className="h-5 w-5 text-[#00A870]" />
                    <span className="text-sm font-medium">{benefit.text}</span>
                  </motion.div>
                ))}
              </div>

              {/* Buttons */}
              <div className="flex flex-col items-center justify-center gap-4 sm:flex-row lg:justify-start">
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Link
                    href={primaryButton.href}
                    className="inline-flex h-14 items-center justify-center gap-2 rounded-xl bg-[#00A870] px-8 text-lg font-semibold tracking-wide text-white shadow-lg shadow-[#00A870]/30 transition-all hover:bg-[#009663]"
                  >
                    {primaryButton.label}
                    <ArrowRight className="h-5 w-5" />
                  </Link>
                </motion.div>

                {secondaryButton && (
                  <Link
                    href={secondaryButton.href}
                    className="inline-flex h-14 items-center justify-center gap-2 rounded-xl border-2 border-white/30 bg-transparent px-8 text-lg font-semibold tracking-wide text-white transition-all hover:border-white"
                  >
                    {secondaryButton.label}
                  </Link>
                )}
              </div>
            </motion.div>
          </div>

          {/* Visual Element */}
          <motion.div
            initial={{ opacity: 0, x: 40 }}
            whileInView={{ opacity: 1, x: 0 }}
            viewport={{ once: true }}
            className="hidden flex-1 justify-center lg:flex"
          >
            <div className="relative">
              {/* Car Illustration / Stats Card */}
              <div className="rounded-3xl border border-white/10 bg-white/10 p-8 backdrop-blur-sm">
                <div className="mb-6 text-center">
                  <div className="mx-auto mb-4 flex h-20 w-20 items-center justify-center rounded-2xl bg-[#00A870]/20">
                    <Car className="h-10 w-10 text-[#00A870]" />
                  </div>
                  <h3 className="mb-2 text-2xl font-bold tracking-tight text-white">
                    Publica en 5 minutos
                  </h3>
                  <p className="leading-relaxed text-white/70">
                    Sube fotos, describe tu vehículo y recibe ofertas
                  </p>
                </div>

                {/* Quick Stats */}
                <div className="grid grid-cols-2 gap-4">
                  {[
                    { value: '5 min', label: 'Para publicar' },
                    { value: '100%', label: 'Gratuito' },
                    { value: '24/7', label: 'Disponible' },
                    { value: '0%', label: 'Comisión' },
                  ].map(stat => (
                    <div key={stat.label} className="rounded-xl bg-white/5 p-3 text-center">
                      <div className="text-xl font-bold tracking-tight text-[#00A870]">
                        {stat.value}
                      </div>
                      <div className="text-xs font-medium text-white/70">{stat.label}</div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Floating Elements */}
              <motion.div
                animate={{ y: [-10, 10, -10] }}
                transition={{ duration: 4, repeat: Infinity }}
                className="absolute -top-4 -right-4 rounded-xl bg-white p-3 shadow-xl"
              >
                <div className="font-bold text-[#00A870]">✓ Verificado</div>
              </motion.div>
            </div>
          </motion.div>
        </div>
      </div>
    </section>
  );
}

// =============================================================================
// SIMPLE VARIANT - ORIGINAL DESIGN
// =============================================================================

function SimpleCTA({
  title,
  subtitle,
  primaryButton,
  secondaryButton,
  className,
}: CTASectionProps) {
  return (
    <section className={cn('bg-[#00A870] py-16 sm:py-24', className)}>
      <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
        <h2 className="text-3xl leading-tight font-bold tracking-tight text-white sm:text-4xl">
          {title}
        </h2>

        {subtitle && (
          <p className="mx-auto mt-4 max-w-2xl text-lg leading-relaxed text-white/95">{subtitle}</p>
        )}

        <div className="mt-8 flex flex-col items-center justify-center gap-4 sm:flex-row">
          <Link
            href={primaryButton.href}
            className="inline-flex h-14 items-center justify-center gap-2 rounded-lg border-2 border-white bg-transparent px-8 text-lg font-semibold tracking-wide text-white transition-all hover:bg-white hover:text-[#00A870]"
          >
            {primaryButton.label}
            <ArrowRight className="h-5 w-5" />
          </Link>

          {secondaryButton && (
            <Link
              href={secondaryButton.href}
              className="inline-flex h-14 items-center justify-center gap-2 rounded-lg bg-white px-8 text-lg font-semibold tracking-wide text-[#00A870] transition-all hover:bg-slate-100"
            >
              {secondaryButton.label}
            </Link>
          )}
        </div>
      </div>
    </section>
  );
}

// =============================================================================
// MAIN EXPORT
// =============================================================================

export function CTASection(props: CTASectionProps) {
  const { variant = 'premium' } = props;

  return variant === 'premium' ? <PremiumCTA {...props} /> : <SimpleCTA {...props} />;
}

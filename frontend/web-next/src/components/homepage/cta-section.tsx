'use client';

import Link from 'next/link';
import { ArrowRight, Car, Camera, Shield, TrendingUp } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useInView } from '@/hooks/use-in-view';

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
  const { ref: contentRef, inView } = useInView();
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
        <div className="bg-primary/20 animate-pulse-orb absolute -top-40 -right-40 h-96 w-96 rounded-full blur-3xl" />
        <div
          className="bg-primary/10 animate-pulse-orb absolute -bottom-40 -left-40 h-80 w-80 rounded-full blur-3xl"
          style={{ animationDelay: '2s' }}
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
            <div
              ref={contentRef}
              className={cn(
                'transition-all duration-700',
                inView ? 'translate-y-0 opacity-100' : 'translate-y-5 opacity-0'
              )}
            >
              {/* Badge */}
              <span className="bg-primary/20 text-primary mb-6 inline-flex items-center gap-2 rounded-full px-4 py-1.5 text-sm font-semibold tracking-wide">
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
                  <div
                    key={benefit.text}
                    className={cn(
                      'flex items-center gap-2 text-white/90 transition-all duration-500',
                      inView ? 'translate-x-0 opacity-100' : '-translate-x-5 opacity-0'
                    )}
                    style={{ transitionDelay: `${index * 100}ms` }}
                  >
                    <benefit.icon className="text-primary h-5 w-5" />
                    <span className="text-sm font-medium">{benefit.text}</span>
                  </div>
                ))}
              </div>

              {/* Buttons */}
              <div className="flex flex-col items-center justify-center gap-4 sm:flex-row lg:justify-start">
                <div className="transition-transform duration-200 hover:scale-[1.02] active:scale-[0.98]">
                  <Link
                    href={primaryButton.href}
                    className="bg-primary text-primary-foreground shadow-primary/30 hover:bg-primary/90 inline-flex h-14 items-center justify-center gap-2 rounded-xl px-8 text-lg font-semibold tracking-wide shadow-lg transition-all"
                  >
                    {primaryButton.label}
                    <ArrowRight className="h-5 w-5" />
                  </Link>
                </div>

                {secondaryButton && (
                  <Link
                    href={secondaryButton.href}
                    className="inline-flex h-14 items-center justify-center gap-2 rounded-xl border-2 border-white/30 bg-transparent px-8 text-lg font-semibold tracking-wide text-white transition-all hover:border-white"
                  >
                    {secondaryButton.label}
                  </Link>
                )}
              </div>
            </div>
          </div>

          {/* Visual Element */}
          <div
            className={cn(
              'hidden flex-1 justify-center transition-all duration-700 lg:flex',
              inView ? 'translate-x-0 opacity-100' : 'translate-x-10 opacity-0'
            )}
            style={{ transitionDelay: '200ms' }}
          >
            <div className="relative">
              {/* Car Illustration / Stats Card */}
              <div className="rounded-3xl border border-white/10 bg-white/10 p-8 backdrop-blur-sm">
                <div className="mb-6 text-center">
                  <div className="bg-primary/20 mx-auto mb-4 flex h-20 w-20 items-center justify-center rounded-2xl">
                    <Car className="text-primary h-10 w-10" />
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
                      <div className="text-primary text-xl font-bold tracking-tight">
                        {stat.value}
                      </div>
                      <div className="text-xs font-medium text-white/70">{stat.label}</div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Floating Elements */}
              <div className="bg-card animate-float-y absolute -top-4 -right-4 rounded-xl p-3 shadow-xl">
                <div className="text-primary font-bold">✓ Verificado</div>
              </div>
            </div>
          </div>
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
    <section className={cn('bg-primary py-16 sm:py-24', className)}>
      <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
        <h2 className="text-primary-foreground text-3xl leading-tight font-bold tracking-tight sm:text-4xl">
          {title}
        </h2>

        {subtitle && (
          <p className="text-primary-foreground/95 mx-auto mt-4 max-w-2xl text-lg leading-relaxed">
            {subtitle}
          </p>
        )}

        <div className="mt-8 flex flex-col items-center justify-center gap-4 sm:flex-row">
          <Link
            href={primaryButton.href}
            className="border-primary-foreground text-primary-foreground hover:bg-primary-foreground hover:text-primary inline-flex h-14 items-center justify-center gap-2 rounded-lg border-2 bg-transparent px-8 text-lg font-semibold tracking-wide transition-all"
          >
            {primaryButton.label}
            <ArrowRight className="h-5 w-5" />
          </Link>

          {secondaryButton && (
            <Link
              href={secondaryButton.href}
              className="bg-card text-primary hover:bg-muted inline-flex h-14 items-center justify-center gap-2 rounded-lg px-8 text-lg font-semibold tracking-wide transition-all"
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

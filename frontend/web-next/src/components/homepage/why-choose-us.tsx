/**
 * Why Choose OKLA Section
 *
 * Professional value proposition section with icons and animations
 * Highlights key differentiators of the platform
 */

'use client';

import { motion } from 'framer-motion';
import {
  Shield,
  Search,
  TrendingUp,
  MessageCircle,
  Camera,
  CreditCard,
  Clock,
  Users,
  CheckCircle2,
  Award,
  Zap,
  HeartHandshake,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { LucideIcon } from 'lucide-react';

// =============================================================================
// TYPES
// =============================================================================

interface ValueProp {
  icon: LucideIcon;
  title: string;
  description: string;
  highlight?: string;
  color: string;
}

interface WhyChooseUsProps {
  className?: string;
  variant?: 'grid' | 'alternating' | 'cards';
}

// =============================================================================
// VALUE PROPOSITIONS
// =============================================================================

const VALUE_PROPS: ValueProp[] = [
  {
    icon: Shield,
    title: 'Vendedores Verificados',
    description:
      'Todos nuestros dealers y vendedores pasan por un proceso de verificación riguroso. Tu seguridad es nuestra prioridad.',
    highlight: '100% Verificados',
    color: 'from-blue-500 to-blue-600',
  },
  {
    icon: Camera,
    title: 'Fotos Profesionales',
    description:
      'Tecnología de inteligencia artificial para mejorar las fotos de tu vehículo y atraer más compradores.',
    highlight: 'AI-Powered',
    color: 'from-purple-500 to-purple-600',
  },
  {
    icon: TrendingUp,
    title: 'Mejor Precio del Mercado',
    description:
      'Análisis de mercado en tiempo real para que obtengas el mejor precio, ya sea que compres o vendas.',
    highlight: 'Precios Justos',
    color: 'from-emerald-500 to-emerald-600',
  },
  {
    icon: MessageCircle,
    title: 'Contacto Directo',
    description:
      'Habla directamente con vendedores sin intermediarios. Sin comisiones ocultas ni sorpresas.',
    highlight: '0% Comisión',
    color: 'from-amber-500 to-orange-600',
  },
  {
    icon: Clock,
    title: 'Vende en 7 Días',
    description:
      'Nuestros usuarios venden sus vehículos en promedio en solo 7 días. El marketplace más activo de RD.',
    highlight: '7 Días Promedio',
    color: 'from-pink-500 to-rose-600',
  },
  {
    icon: CreditCard,
    title: 'Pagos Seguros',
    description:
      'Integración con los principales bancos dominicanos y procesadores de pago internacionales.',
    highlight: 'Banco Popular & Stripe',
    color: 'from-cyan-500 to-blue-600',
  },
];

// =============================================================================
// GRID VARIANT
// =============================================================================

function GridVariant({ className }: { className?: string }) {
  return (
    <div className={cn('grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3 lg:gap-8', className)}>
      {VALUE_PROPS.map((prop, index) => (
        <motion.div
          key={prop.title}
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-50px' }}
          transition={{ delay: index * 0.1, duration: 0.5 }}
          className="group relative overflow-hidden rounded-2xl border border-gray-100 bg-white p-6 shadow-sm transition-all duration-300 hover:border-transparent hover:shadow-xl lg:p-8"
        >
          {/* Background Gradient on Hover */}
          <div
            className={cn(
              'absolute inset-0 bg-gradient-to-br opacity-0 transition-opacity duration-300 group-hover:opacity-5',
              prop.color
            )}
          />

          {/* Icon */}
          <div
            className={cn(
              'mb-5 flex h-14 w-14 items-center justify-center rounded-xl bg-gradient-to-br shadow-lg transition-transform duration-300 group-hover:scale-110',
              prop.color
            )}
          >
            <prop.icon className="h-7 w-7 text-white" />
          </div>

          {/* Highlight Badge */}
          {prop.highlight && (
            <span className="mb-3 inline-block rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold tracking-wide text-slate-600">
              {prop.highlight}
            </span>
          )}

          {/* Title */}
          <h3 className="mb-3 text-xl leading-snug font-bold tracking-tight text-slate-900 transition-colors group-hover:text-[#00A870]">
            {prop.title}
          </h3>

          {/* Description */}
          <p className="leading-relaxed text-slate-600">{prop.description}</p>

          {/* Decorative Corner */}
          <div
            className={cn(
              'absolute -right-2 -bottom-2 h-20 w-20 rounded-full bg-gradient-to-br opacity-10 blur-xl',
              prop.color
            )}
          />
        </motion.div>
      ))}
    </div>
  );
}

// =============================================================================
// ALTERNATING VARIANT
// =============================================================================

function AlternatingVariant({ className }: { className?: string }) {
  return (
    <div className={cn('space-y-16 lg:space-y-24', className)}>
      {VALUE_PROPS.slice(0, 4).map((prop, index) => (
        <motion.div
          key={prop.title}
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-100px' }}
          transition={{ duration: 0.6 }}
          className={cn(
            'flex flex-col items-center gap-8 lg:flex-row lg:gap-16',
            index % 2 === 1 && 'lg:flex-row-reverse'
          )}
        >
          {/* Visual */}
          <div className="relative flex-1">
            <div
              className={cn(
                'mx-auto aspect-square w-full max-w-md rounded-3xl bg-gradient-to-br p-1',
                prop.color
              )}
            >
              <div className="flex h-full w-full items-center justify-center rounded-3xl bg-white">
                <prop.icon
                  className={cn(
                    'h-32 w-32 bg-gradient-to-br bg-clip-text',
                    prop.color.replace('from-', 'text-').replace(/to-.+/, '')
                  )}
                />
              </div>
            </div>
            {/* Decorative Dots */}
            <div className="absolute top-8 left-8 -z-10 h-full w-full rounded-3xl bg-gray-100" />
          </div>

          {/* Content */}
          <div className="flex-1 text-center lg:text-left">
            {prop.highlight && (
              <span
                className={cn(
                  'mb-4 inline-block rounded-full bg-gradient-to-r px-4 py-1.5 text-sm font-semibold text-white',
                  prop.color
                )}
              >
                {prop.highlight}
              </span>
            )}
            <h3 className="mb-4 text-3xl font-bold text-gray-900 lg:text-4xl">{prop.title}</h3>
            <p className="mx-auto max-w-lg text-lg leading-relaxed text-gray-600 lg:mx-0">
              {prop.description}
            </p>
          </div>
        </motion.div>
      ))}
    </div>
  );
}

// =============================================================================
// CARDS VARIANT (COMPACT)
// =============================================================================

function CardsVariant({ className }: { className?: string }) {
  return (
    <div className={cn('grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-6', className)}>
      {VALUE_PROPS.map((prop, index) => (
        <motion.div
          key={prop.title}
          initial={{ opacity: 0, scale: 0.9 }}
          whileInView={{ opacity: 1, scale: 1 }}
          viewport={{ once: true }}
          transition={{ delay: index * 0.05 }}
          whileHover={{ y: -8 }}
          className="group flex flex-col items-center rounded-2xl border border-slate-100 bg-white p-4 text-center shadow-sm transition-all duration-300 hover:shadow-lg lg:p-6"
        >
          {/* Icon */}
          <div
            className={cn(
              'mb-3 flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br transition-transform group-hover:scale-110',
              prop.color
            )}
          >
            <prop.icon className="h-6 w-6 text-white" />
          </div>

          {/* Title */}
          <h4 className="mb-1 text-sm leading-snug font-semibold tracking-tight text-slate-900 transition-colors group-hover:text-[#00A870] lg:text-base">
            {prop.title}
          </h4>

          {/* Highlight */}
          {prop.highlight && (
            <span className="text-xs font-medium text-slate-500">{prop.highlight}</span>
          )}
        </motion.div>
      ))}
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function WhyChooseUs({ className, variant = 'grid' }: WhyChooseUsProps) {
  const VariantComponent = {
    grid: GridVariant,
    alternating: AlternatingVariant,
    cards: CardsVariant,
  }[variant];

  return (
    <section className={cn('py-16 lg:py-24', className)}>
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="mb-12 text-center lg:mb-16"
        >
          <span className="mb-4 inline-block rounded-full bg-[#00A870]/10 px-4 py-1.5 text-sm font-semibold tracking-wide text-[#00A870]">
            ¿Por qué OKLA?
          </span>
          <h2 className="mb-4 text-3xl leading-tight font-bold tracking-tight text-slate-900 lg:text-5xl">
            La forma más inteligente de
            <br />
            <span className="text-[#00A870]">comprar y vender</span> vehículos
          </h2>
          <p className="mx-auto max-w-2xl text-lg leading-relaxed text-slate-600">
            Miles de dominicanos confían en OKLA para encontrar su próximo vehículo o vender el suyo
            de forma rápida y segura.
          </p>
        </motion.div>

        {/* Content */}
        <VariantComponent className={className} />
      </div>
    </section>
  );
}

export default WhyChooseUs;

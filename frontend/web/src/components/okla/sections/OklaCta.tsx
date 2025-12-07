import { motion } from 'framer-motion';
import { ArrowRight, Sparkles } from 'lucide-react';
import { FadeIn } from '../animations/FadeIn';
import { OklaButton } from '../../atoms/okla/OklaButton';

interface OklaCtaProps {
  title?: string;
  subtitle?: string;
  primaryAction?: {
    label: string;
    onClick?: () => void;
    href?: string;
  };
  secondaryAction?: {
    label: string;
    onClick?: () => void;
    href?: string;
  };
  variant?: 'gradient' | 'image' | 'minimal';
  backgroundImage?: string;
}

export const OklaCta = ({
  title = '¿Listo para Comenzar?',
  subtitle = 'Descubre una plataforma segura y profesional para tus operaciones',
  primaryAction = { label: 'Comenzar Ahora' },
  secondaryAction,
  variant = 'gradient',
  backgroundImage,
}: OklaCtaProps) => {
  const getBackgroundStyles = () => {
    switch (variant) {
      case 'gradient':
        return 'bg-gradient-to-br from-okla-navy via-okla-charcoal to-okla-navy';
      case 'image':
        return 'relative';
      case 'minimal':
        return 'bg-okla-cream';
      default:
        return 'bg-okla-navy';
    }
  };

  const isLight = variant === 'minimal';

  return (
    <section className={`py-24 px-6 overflow-hidden ${getBackgroundStyles()}`}>
      {/* Background Image for 'image' variant */}
      {variant === 'image' && backgroundImage && (
        <>
          <div
            className="absolute inset-0 bg-cover bg-center"
            style={{ backgroundImage: `url(${backgroundImage})` }}
          />
          <div className="absolute inset-0 bg-okla-navy/80" />
        </>
      )}

      {/* Animated Background Elements */}
      {variant === 'gradient' && (
        <div className="absolute inset-0 overflow-hidden pointer-events-none">
          <motion.div
            className="absolute -top-20 -left-20 w-80 h-80 bg-okla-gold/10 rounded-full blur-3xl"
            animate={{
              scale: [1, 1.3, 1],
              x: [0, 50, 0],
              y: [0, 30, 0],
            }}
            transition={{ duration: 15, repeat: Infinity }}
          />
          <motion.div
            className="absolute -bottom-20 -right-20 w-96 h-96 bg-okla-gold/10 rounded-full blur-3xl"
            animate={{
              scale: [1.2, 1, 1.2],
              x: [0, -40, 0],
              y: [0, -40, 0],
            }}
            transition={{ duration: 18, repeat: Infinity }}
          />
        </div>
      )}

      <div className="max-w-4xl mx-auto text-center relative z-10">
        {/* Icon */}
        <FadeIn>
          <div className="flex justify-center mb-6">
            <motion.div
              className={`w-16 h-16 rounded-2xl flex items-center justify-center ${
                isLight ? 'bg-okla-gold/20' : 'bg-okla-gold/20'
              }`}
              animate={{ rotate: [0, 5, -5, 0] }}
              transition={{ duration: 4, repeat: Infinity }}
            >
              <Sparkles className="w-8 h-8 text-okla-gold" />
            </motion.div>
          </div>
        </FadeIn>

        {/* Title */}
        <FadeIn delay={0.1}>
          <h2
            className={`text-4xl md:text-5xl lg:text-6xl font-playfair font-bold leading-tight ${
              isLight ? 'text-okla-navy' : 'text-white'
            }`}
          >
            {title}
          </h2>
        </FadeIn>

        {/* Subtitle */}
        <FadeIn delay={0.2}>
          <p
            className={`text-lg md:text-xl mt-6 max-w-2xl mx-auto ${
              isLight ? 'text-okla-slate' : 'text-okla-cream/80'
            }`}
          >
            {subtitle}
          </p>
        </FadeIn>

        {/* Actions */}
        <FadeIn delay={0.3}>
          <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mt-10">
            <OklaButton
              variant="primary"
              size="lg"
              onClick={primaryAction.onClick}
              rightIcon={<ArrowRight className="w-5 h-5" />}
              className="min-w-[200px]"
            >
              {primaryAction.label}
            </OklaButton>
            {secondaryAction && (
              <OklaButton
                variant={isLight ? 'outline' : 'ghost'}
                size="lg"
                onClick={secondaryAction.onClick}
                className={`min-w-[200px] ${!isLight && 'text-white border-white/30 hover:bg-white/10'}`}
              >
                {secondaryAction.label}
              </OklaButton>
            )}
          </div>
        </FadeIn>

        {/* Trust Text */}
        <FadeIn delay={0.4}>
          <p
            className={`text-sm mt-8 ${
              isLight ? 'text-okla-slate/60' : 'text-okla-cream/50'
            }`}
          >
            Sin compromisos • Cancela cuando quieras • Soporte 24/7
          </p>
        </FadeIn>
      </div>
    </section>
  );
};

export default OklaCta;

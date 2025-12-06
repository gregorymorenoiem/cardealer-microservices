import { motion } from 'framer-motion';
import type { ReactNode } from 'react';
import { FadeIn } from '../animations/FadeIn';
import { StaggerContainer, StaggerItem } from '../animations/StaggerContainer';

interface Feature {
  icon: ReactNode;
  title: string;
  description: string;
}

interface OklaHowItWorksProps {
  title?: string;
  subtitle?: string;
  features: Feature[];
  variant?: 'steps' | 'grid' | 'cards';
}

export const OklaHowItWorks = ({
  title = 'Cómo Funciona',
  subtitle = 'Tres simples pasos para encontrar lo que buscas',
  features,
  variant = 'steps',
}: OklaHowItWorksProps) => {
  if (variant === 'grid') {
    return (
      <section className="py-20 px-6 bg-okla-cream">
        <div className="max-w-7xl mx-auto">
          <FadeIn>
            <div className="text-center mb-16">
              <span className="text-okla-gold font-medium tracking-wider text-sm uppercase">
                Proceso
              </span>
              <h2 className="text-4xl md:text-5xl font-playfair font-bold text-okla-navy mt-2">
                {title}
              </h2>
              <p className="text-okla-slate mt-4 max-w-xl mx-auto">{subtitle}</p>
            </div>
          </FadeIn>

          <StaggerContainer className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            {features.map((feature, index) => (
              <StaggerItem key={index}>
                <div className="bg-white rounded-2xl p-8 shadow-sm hover:shadow-lg transition-shadow">
                  <div className="w-14 h-14 rounded-xl bg-okla-gold/10 flex items-center justify-center text-okla-gold mb-6">
                    {feature.icon}
                  </div>
                  <h3 className="text-xl font-semibold text-okla-navy mb-3">
                    {feature.title}
                  </h3>
                  <p className="text-okla-slate">{feature.description}</p>
                </div>
              </StaggerItem>
            ))}
          </StaggerContainer>
        </div>
      </section>
    );
  }

  if (variant === 'cards') {
    return (
      <section className="py-20 px-6 bg-white">
        <div className="max-w-7xl mx-auto">
          <FadeIn>
            <div className="text-center mb-16">
              <span className="text-okla-gold font-medium tracking-wider text-sm uppercase">
                Proceso
              </span>
              <h2 className="text-4xl md:text-5xl font-playfair font-bold text-okla-navy mt-2">
                {title}
              </h2>
              <p className="text-okla-slate mt-4 max-w-xl mx-auto">{subtitle}</p>
            </div>
          </FadeIn>

          <StaggerContainer className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {features.map((feature, index) => (
              <StaggerItem key={index}>
                <motion.div
                  className="relative bg-okla-cream rounded-2xl p-8 text-center group"
                  whileHover={{ y: -10 }}
                  transition={{ duration: 0.3 }}
                >
                  <div className="absolute -top-4 left-1/2 -translate-x-1/2 w-8 h-8 rounded-full bg-okla-gold text-white font-bold flex items-center justify-center text-sm">
                    {index + 1}
                  </div>
                  <div className="w-20 h-20 mx-auto rounded-2xl bg-white flex items-center justify-center text-okla-navy mb-6 shadow-sm group-hover:bg-okla-navy group-hover:text-okla-gold transition-colors">
                    {feature.icon}
                  </div>
                  <h3 className="text-xl font-semibold text-okla-navy mb-3">
                    {feature.title}
                  </h3>
                  <p className="text-okla-slate">{feature.description}</p>
                </motion.div>
              </StaggerItem>
            ))}
          </StaggerContainer>
        </div>
      </section>
    );
  }

  // Default: steps variant with connecting lines
  return (
    <section className="py-20 px-6 bg-white overflow-hidden">
      <div className="max-w-6xl mx-auto">
        <FadeIn>
          <div className="text-center mb-16">
            <span className="text-okla-gold font-medium tracking-wider text-sm uppercase">
              Proceso
            </span>
            <h2 className="text-4xl md:text-5xl font-playfair font-bold text-okla-navy mt-2">
              {title}
            </h2>
            <p className="text-okla-slate mt-4 max-w-xl mx-auto">{subtitle}</p>
          </div>
        </FadeIn>

        <div className="relative">
          {/* Connecting Line */}
          <div className="hidden md:block absolute top-16 left-[10%] right-[10%] h-0.5 bg-okla-cream">
            <motion.div
              className="absolute inset-0 bg-okla-gold origin-left"
              initial={{ scaleX: 0 }}
              whileInView={{ scaleX: 1 }}
              viewport={{ once: true }}
              transition={{ duration: 1.5, delay: 0.5 }}
            />
          </div>

          <StaggerContainer className="grid grid-cols-1 md:grid-cols-3 gap-12 md:gap-8 relative">
            {features.map((feature, index) => (
              <StaggerItem key={index}>
                <div className="flex flex-col items-center text-center">
                  {/* Step Circle */}
                  <motion.div
                    className="relative z-10 w-32 h-32 rounded-full bg-white border-4 border-okla-cream flex items-center justify-center mb-8 shadow-lg"
                    whileHover={{ scale: 1.05, borderColor: '#C5A572' }}
                    transition={{ duration: 0.3 }}
                  >
                    <div className="absolute -top-2 -right-2 w-8 h-8 rounded-full bg-okla-gold text-white font-bold flex items-center justify-center text-sm shadow-md">
                      {index + 1}
                    </div>
                    <div className="text-okla-navy">{feature.icon}</div>
                  </motion.div>

                  {/* Content */}
                  <h3 className="text-xl font-semibold text-okla-navy mb-3">
                    {feature.title}
                  </h3>
                  <p className="text-okla-slate max-w-xs">{feature.description}</p>
                </div>
              </StaggerItem>
            ))}
          </StaggerContainer>
        </div>
      </div>
    </section>
  );
};

export default OklaHowItWorks;

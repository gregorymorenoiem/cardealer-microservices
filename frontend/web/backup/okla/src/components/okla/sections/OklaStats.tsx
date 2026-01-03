import { motion, useMotionValue, animate } from 'framer-motion';
import { useEffect, useState } from 'react';
import { FadeIn } from '../animations/FadeIn';
import { StaggerContainer, StaggerItem } from '../animations/StaggerContainer';

interface StatItem {
  value: number;
  label: string;
  suffix?: string;
  prefix?: string;
}

interface OklaStatsProps {
  title?: string;
  subtitle?: string;
  stats: StatItem[];
  darkMode?: boolean;
}

const AnimatedCounter = ({ 
  value, 
  suffix = '', 
  prefix = '',
  darkMode = false 
}: { 
  value: number; 
  suffix?: string; 
  prefix?: string;
  darkMode?: boolean;
}) => {
  const count = useMotionValue(0);
  const [displayValue, setDisplayValue] = useState(0);

  useEffect(() => {
    const controls = animate(count, value, {
      duration: 2,
      ease: 'easeOut',
      onUpdate: (latest) => {
        setDisplayValue(Math.round(latest));
      },
    });

    return controls.stop;
  }, [value, count]);

  return (
    <span className={`text-5xl md:text-6xl font-playfair font-bold ${darkMode ? 'text-white' : 'text-okla-navy'}`}>
      {prefix}
      {displayValue.toLocaleString()}
      {suffix}
    </span>
  );
};

export const OklaStats = ({
  title = 'Números que Hablan',
  subtitle = 'Nuestro compromiso con la seguridad y la calidad',
  stats,
  darkMode = false,
}: OklaStatsProps) => {
  return (
    <section className={`py-24 px-6 ${darkMode ? 'bg-okla-navy' : 'bg-white'}`}>
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <FadeIn>
          <div className="text-center mb-16">
            <span className="text-okla-gold font-medium tracking-wider text-sm uppercase">
              Estadísticas
            </span>
            <h2 className={`text-4xl md:text-5xl font-playfair font-bold mt-2 ${darkMode ? 'text-white' : 'text-okla-navy'}`}>
              {title}
            </h2>
            <p className={`mt-4 max-w-xl mx-auto ${darkMode ? 'text-okla-cream/70' : 'text-okla-slate'}`}>
              {subtitle}
            </p>
          </div>
        </FadeIn>

        {/* Stats Grid */}
        <StaggerContainer className="grid grid-cols-2 lg:grid-cols-4 gap-8 md:gap-12">
          {stats.map((stat, index) => (
            <StaggerItem key={index}>
              <motion.div
                className="text-center"
                whileInView="visible"
                viewport={{ once: true }}
              >
                <motion.div
                  initial={{ opacity: 0, scale: 0.5 }}
                  whileInView={{ opacity: 1, scale: 1 }}
                  viewport={{ once: true }}
                  transition={{ delay: index * 0.1, duration: 0.5 }}
                >
                  <AnimatedCounter
                    value={stat.value}
                    suffix={stat.suffix}
                    prefix={stat.prefix}
                    darkMode={darkMode}
                  />
                </motion.div>
                <motion.div
                  initial={{ opacity: 0, y: 10 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  transition={{ delay: 0.3 + index * 0.1 }}
                  className="mt-2"
                >
                  <span className={`text-sm font-medium tracking-wide uppercase ${darkMode ? 'text-okla-gold' : 'text-okla-gold'}`}>
                    {stat.label}
                  </span>
                </motion.div>
                {/* Decorative Line */}
                <motion.div
                  className="h-1 w-12 bg-okla-gold/30 mx-auto mt-4 rounded-full"
                  initial={{ width: 0 }}
                  whileInView={{ width: 48 }}
                  viewport={{ once: true }}
                  transition={{ delay: 0.5 + index * 0.1, duration: 0.5 }}
                />
              </motion.div>
            </StaggerItem>
          ))}
        </StaggerContainer>
      </div>
    </section>
  );
};

export default OklaStats;

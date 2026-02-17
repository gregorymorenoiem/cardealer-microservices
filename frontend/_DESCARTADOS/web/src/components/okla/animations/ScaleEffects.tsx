import { motion } from 'framer-motion';
import type { ReactNode } from 'react';

interface ScaleInProps {
  children: ReactNode;
  delay?: number;
  duration?: number;
  className?: string;
  scale?: number;
}

export const ScaleIn = ({
  children,
  delay = 0,
  duration = 0.5,
  className = '',
  scale = 0.9,
}: ScaleInProps) => {
  return (
    <motion.div
      className={className}
      initial={{ opacity: 0, scale }}
      whileInView={{ opacity: 1, scale: 1 }}
      viewport={{ once: true, margin: '-50px' }}
      transition={{
        duration,
        delay,
        ease: [0.25, 0.46, 0.45, 0.94],
      }}
    >
      {children}
    </motion.div>
  );
};

interface HoverScaleProps {
  children: ReactNode;
  scale?: number;
  className?: string;
}

export const HoverScale = ({
  children,
  scale = 1.02,
  className = '',
}: HoverScaleProps) => {
  return (
    <motion.div
      className={className}
      whileHover={{ scale }}
      whileTap={{ scale: 0.98 }}
      transition={{ duration: 0.2 }}
    >
      {children}
    </motion.div>
  );
};

interface HoverLiftProps {
  children: ReactNode;
  className?: string;
  lift?: number;
}

export const HoverLift = ({
  children,
  className = '',
  lift = -8,
}: HoverLiftProps) => {
  return (
    <motion.div
      className={className}
      whileHover={{ y: lift }}
      transition={{ duration: 0.3, ease: 'easeOut' }}
    >
      {children}
    </motion.div>
  );
};

export default ScaleIn;

import { motion, useScroll, useTransform } from 'framer-motion';
import type { ReactNode } from 'react';
import { useRef } from 'react';

interface ParallaxProps {
  children: ReactNode;
  speed?: number;
  className?: string;
}

export const Parallax = ({
  children,
  speed = 0.5,
  className = '',
}: ParallaxProps) => {
  const ref = useRef<HTMLDivElement>(null);
  const { scrollYProgress } = useScroll({
    target: ref,
    offset: ['start end', 'end start'],
  });

  const y = useTransform(scrollYProgress, [0, 1], [0, 100 * speed]);

  return (
    <motion.div ref={ref} style={{ y }} className={className}>
      {children}
    </motion.div>
  );
};

interface ParallaxBackgroundProps {
  imageUrl: string;
  speed?: number;
  className?: string;
  overlay?: boolean;
  overlayOpacity?: number;
}

export const ParallaxBackground = ({
  imageUrl,
  speed = 0.3,
  className = '',
  overlay = true,
  overlayOpacity = 0.4,
}: ParallaxBackgroundProps) => {
  const ref = useRef<HTMLDivElement>(null);
  const { scrollYProgress } = useScroll({
    target: ref,
    offset: ['start end', 'end start'],
  });

  const y = useTransform(scrollYProgress, [0, 1], ['0%', `${speed * 100}%`]);

  return (
    <div ref={ref} className={`relative overflow-hidden ${className}`}>
      <motion.div
        className="absolute inset-0 w-full h-[120%] -top-[10%]"
        style={{
          y,
          backgroundImage: `url(${imageUrl})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
        }}
      />
      {overlay && (
        <div
          className="absolute inset-0 bg-okla-navy"
          style={{ opacity: overlayOpacity }}
        />
      )}
    </div>
  );
};

export default Parallax;

import { motion } from 'framer-motion';
import type { ReactNode } from 'react';

interface TextRevealProps {
  children: string;
  className?: string;
  delay?: number;
  staggerDelay?: number;
}

export const TextReveal = ({
  children,
  className = '',
  delay = 0,
  staggerDelay = 0.03,
}: TextRevealProps) => {
  const words = children.split(' ');

  return (
    <motion.span
      className={`inline-flex flex-wrap ${className}`}
      initial="hidden"
      whileInView="visible"
      viewport={{ once: true }}
    >
      {words.map((word, wordIndex) => (
        <span key={wordIndex} className="inline-block mr-[0.25em]">
          {word.split('').map((char, charIndex) => (
            <motion.span
              key={charIndex}
              className="inline-block"
              variants={{
                hidden: { opacity: 0, y: 20 },
                visible: {
                  opacity: 1,
                  y: 0,
                  transition: {
                    delay: delay + (wordIndex * word.length + charIndex) * staggerDelay,
                    duration: 0.4,
                    ease: [0.25, 0.46, 0.45, 0.94],
                  },
                },
              }}
            >
              {char}
            </motion.span>
          ))}
        </span>
      ))}
    </motion.span>
  );
};

interface TypewriterProps {
  text: string;
  className?: string;
  speed?: number;
  delay?: number;
  cursor?: boolean;
}

export const Typewriter = ({
  text,
  className = '',
  speed = 0.05,
  delay = 0,
  cursor = true,
}: TypewriterProps) => {
  return (
    <motion.span className={`inline-block ${className}`}>
      {text.split('').map((char, index) => (
        <motion.span
          key={index}
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: delay + index * speed }}
        >
          {char}
        </motion.span>
      ))}
      {cursor && (
        <motion.span
          className="inline-block w-[2px] h-[1em] bg-current ml-1"
          animate={{ opacity: [1, 0] }}
          transition={{ duration: 0.8, repeat: Infinity }}
        />
      )}
    </motion.span>
  );
};

interface GlowTextProps {
  children: ReactNode;
  className?: string;
  color?: string;
}

export const GlowText = ({
  children,
  className = '',
  color = '#C5A572',
}: GlowTextProps) => {
  return (
    <motion.span
      className={`relative inline-block ${className}`}
      whileHover="hover"
    >
      <motion.span
        className="absolute inset-0 blur-lg opacity-50"
        style={{ color }}
        variants={{
          hover: { opacity: 0.8 },
        }}
      >
        {children}
      </motion.span>
      <span className="relative">{children}</span>
    </motion.span>
  );
};

export default TextReveal;

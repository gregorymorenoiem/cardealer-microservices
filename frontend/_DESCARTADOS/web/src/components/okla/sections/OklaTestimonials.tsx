import { motion } from 'framer-motion';
import { Star, Quote } from 'lucide-react';
import { useState } from 'react';
import { FadeIn } from '../animations/FadeIn';

interface Testimonial {
  id: string;
  name: string;
  role?: string;
  avatar?: string;
  content: string;
  rating: number;
  date?: string;
}

interface OklaTestimonialsProps {
  title?: string;
  subtitle?: string;
  testimonials: Testimonial[];
}

export const OklaTestimonials = ({
  title = 'Lo que Dicen Nuestros Clientes',
  subtitle = 'Historias de éxito que nos inspiran a mejorar cada día',
  testimonials,
}: OklaTestimonialsProps) => {
  const [activeIndex, setActiveIndex] = useState(0);

  const activeTestimonial = testimonials[activeIndex];

  return (
    <section className="py-24 px-6 bg-gradient-to-br from-okla-navy via-okla-charcoal to-okla-navy relative overflow-hidden">
      {/* Background Decorations */}
      <div className="absolute inset-0 pointer-events-none">
        <motion.div
          className="absolute top-0 left-1/4 w-96 h-96 bg-okla-gold/5 rounded-full blur-3xl"
          animate={{
            scale: [1, 1.2, 1],
            opacity: [0.3, 0.5, 0.3],
          }}
          transition={{ duration: 10, repeat: Infinity }}
        />
        <motion.div
          className="absolute bottom-0 right-1/4 w-80 h-80 bg-okla-gold/5 rounded-full blur-3xl"
          animate={{
            scale: [1.2, 1, 1.2],
            opacity: [0.3, 0.5, 0.3],
          }}
          transition={{ duration: 12, repeat: Infinity }}
        />
      </div>

      <div className="max-w-5xl mx-auto relative z-10">
        {/* Header */}
        <FadeIn>
          <div className="text-center mb-16">
            <span className="text-okla-gold font-medium tracking-wider text-sm uppercase">
              Testimonios
            </span>
            <h2 className="text-4xl md:text-5xl font-playfair font-bold text-white mt-2">
              {title}
            </h2>
            <p className="text-okla-cream/70 mt-4 max-w-xl mx-auto">
              {subtitle}
            </p>
          </div>
        </FadeIn>

        {/* Main Testimonial */}
        <FadeIn delay={0.2}>
          <motion.div
            key={activeTestimonial.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            className="text-center"
          >
            {/* Quote Icon */}
            <div className="flex justify-center mb-8">
              <div className="w-16 h-16 bg-okla-gold/20 rounded-full flex items-center justify-center">
                <Quote className="w-8 h-8 text-okla-gold" />
              </div>
            </div>

            {/* Rating */}
            <div className="flex justify-center gap-1 mb-6">
              {Array.from({ length: 5 }).map((_, i) => (
                <Star
                  key={i}
                  className={`w-6 h-6 ${
                    i < activeTestimonial.rating
                      ? 'text-okla-gold'
                      : 'text-okla-slate/30'
                  }`}
                  fill={i < activeTestimonial.rating ? 'currentColor' : 'none'}
                />
              ))}
            </div>

            {/* Quote Text */}
            <blockquote className="text-2xl md:text-3xl font-playfair text-white leading-relaxed max-w-3xl mx-auto mb-8">
              "{activeTestimonial.content}"
            </blockquote>

            {/* Author */}
            <div className="flex flex-col items-center">
              {activeTestimonial.avatar ? (
                <img
                  src={activeTestimonial.avatar}
                  alt={activeTestimonial.name}
                  className="w-16 h-16 rounded-full object-cover border-2 border-okla-gold mb-4"
                />
              ) : (
                <div className="w-16 h-16 rounded-full bg-okla-gold/20 flex items-center justify-center text-okla-gold text-xl font-bold mb-4">
                  {activeTestimonial.name.charAt(0)}
                </div>
              )}
              <div className="text-white font-semibold text-lg">
                {activeTestimonial.name}
              </div>
              {activeTestimonial.role && (
                <div className="text-okla-cream/60 text-sm mt-1">
                  {activeTestimonial.role}
                </div>
              )}
            </div>
          </motion.div>
        </FadeIn>

        {/* Navigation Dots */}
        <div className="flex justify-center gap-3 mt-12">
          {testimonials.map((_, index) => (
            <motion.button
              key={index}
              onClick={() => setActiveIndex(index)}
              className={`h-3 rounded-full transition-all duration-300 ${
                index === activeIndex
                  ? 'w-8 bg-okla-gold'
                  : 'w-3 bg-okla-gold/30 hover:bg-okla-gold/50'
              }`}
              whileHover={{ scale: 1.2 }}
              whileTap={{ scale: 0.9 }}
            />
          ))}
        </div>

        {/* Thumbnail Navigation */}
        <div className="flex justify-center gap-4 mt-8">
          {testimonials.map((testimonial, index) => (
            <motion.button
              key={testimonial.id}
              onClick={() => setActiveIndex(index)}
              className={`relative transition-all duration-300 ${
                index === activeIndex ? 'scale-110' : 'opacity-50 hover:opacity-80'
              }`}
              whileHover={{ scale: 1.1 }}
              whileTap={{ scale: 0.95 }}
            >
              {testimonial.avatar ? (
                <img
                  src={testimonial.avatar}
                  alt={testimonial.name}
                  className={`w-12 h-12 rounded-full object-cover border-2 ${
                    index === activeIndex ? 'border-okla-gold' : 'border-transparent'
                  }`}
                />
              ) : (
                <div
                  className={`w-12 h-12 rounded-full bg-okla-charcoal flex items-center justify-center text-okla-gold font-bold border-2 ${
                    index === activeIndex ? 'border-okla-gold' : 'border-transparent'
                  }`}
                >
                  {testimonial.name.charAt(0)}
                </div>
              )}
            </motion.button>
          ))}
        </div>
      </div>
    </section>
  );
};

export default OklaTestimonials;

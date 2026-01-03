import { useState } from 'react';
import { motion } from 'framer-motion';
import { MessageSquare, Send, Check } from 'lucide-react';
import { OklaButton } from '../../atoms/okla/OklaButton';
import { OklaInput } from '../../atoms/okla/OklaInput';

interface OklaContactFormProps {
  listingTitle?: string;
  sellerName?: string;
  onSubmit?: (data: ContactFormData) => void;
  variant?: 'full' | 'compact' | 'modal';
}

interface ContactFormData {
  name: string;
  email: string;
  phone: string;
  message: string;
}

const predefinedMessages = [
  '¿Está disponible todavía?',
  '¿Cuál es el precio final?',
  '¿Acepta intercambio?',
  '¿Cuándo puedo verlo?',
];

export const OklaContactForm = ({
  listingTitle,
  sellerName,
  onSubmit,
  variant = 'full',
}: OklaContactFormProps) => {
  const [formData, setFormData] = useState<ContactFormData>({
    name: '',
    email: '',
    phone: '',
    message: listingTitle ? `Hola, me interesa "${listingTitle}". ` : '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);

  const handleChange = (field: keyof ContactFormData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleQuickMessage = (message: string) => {
    setFormData((prev) => ({
      ...prev,
      message: prev.message + message + ' ',
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 1500));

    onSubmit?.(formData);
    setIsSubmitting(false);
    setIsSubmitted(true);

    // Reset after showing success
    setTimeout(() => {
      setIsSubmitted(false);
      setFormData({ name: '', email: '', phone: '', message: '' });
    }, 3000);
  };

  if (isSubmitted) {
    return (
      <motion.div
        className="bg-white rounded-2xl border border-okla-cream p-8 text-center"
        initial={{ scale: 0.9, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
      >
        <motion.div
          className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4"
          initial={{ scale: 0 }}
          animate={{ scale: 1 }}
          transition={{ delay: 0.2, type: 'spring' }}
        >
          <Check className="w-8 h-8 text-green-600" />
        </motion.div>
        <h3 className="text-xl font-display font-bold text-okla-navy mb-2">
          ¡Mensaje enviado!
        </h3>
        <p className="text-okla-slate">
          {sellerName ? `${sellerName} recibirá tu mensaje pronto.` : 'El vendedor recibirá tu mensaje pronto.'}
        </p>
      </motion.div>
    );
  }

  if (variant === 'compact') {
    return (
      <motion.div
        className="bg-white rounded-2xl border border-okla-cream p-6"
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
      >
        <h3 className="font-display font-bold text-lg text-okla-navy mb-4">
          Contactar al vendedor
        </h3>
        <form onSubmit={handleSubmit} className="space-y-4">
          <OklaInput
            placeholder="Tu mensaje..."
            value={formData.message}
            onChange={(e) => handleChange('message', e.target.value)}
            className="min-h-[80px]"
          />
          <OklaButton type="submit" className="w-full" isLoading={isSubmitting}>
            <Send className="w-4 h-4 mr-2" />
            Enviar mensaje
          </OklaButton>
        </form>
      </motion.div>
    );
  }

  return (
    <motion.div
      className="bg-white rounded-2xl border border-okla-cream p-6 shadow-sm"
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4 }}
    >
      {/* Header */}
      <div className="mb-6">
        <h3 className="font-display font-bold text-xl text-okla-navy mb-1">
          Contactar al vendedor
        </h3>
        {sellerName && (
          <p className="text-okla-slate text-sm">
            Envía un mensaje a {sellerName}
          </p>
        )}
      </div>

      {/* Quick Messages */}
      <div className="mb-6">
        <p className="text-sm text-okla-slate mb-3">Mensajes rápidos:</p>
        <div className="flex flex-wrap gap-2">
          {predefinedMessages.map((msg, index) => (
            <motion.button
              key={index}
              type="button"
              onClick={() => handleQuickMessage(msg)}
              className="px-3 py-1.5 bg-okla-cream/50 hover:bg-okla-cream rounded-full text-sm text-okla-navy transition-colors"
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
            >
              {msg}
            </motion.button>
          ))}
        </div>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <OklaInput
            label="Nombre"
            placeholder="Tu nombre"
            value={formData.name}
            onChange={(e) => handleChange('name', e.target.value)}
            required
          />
          <OklaInput
            label="Email"
            type="email"
            placeholder="tu@email.com"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            required
          />
        </div>

        <OklaInput
          label="Teléfono (opcional)"
          type="tel"
          placeholder="(787) 000-0000"
          value={formData.phone}
          onChange={(e) => handleChange('phone', e.target.value)}
        />

        <div>
          <label className="block text-sm font-medium text-okla-navy mb-2">
            <MessageSquare className="w-4 h-4 inline-block mr-1" />
            Mensaje
          </label>
          <textarea
            value={formData.message}
            onChange={(e) => handleChange('message', e.target.value)}
            placeholder="Escribe tu mensaje aquí..."
            rows={4}
            required
            className="w-full px-4 py-3 border border-okla-cream rounded-xl focus:outline-none focus:ring-2 focus:ring-okla-gold/50 focus:border-okla-gold resize-none transition-all"
          />
        </div>

        <OklaButton type="submit" className="w-full" size="lg" isLoading={isSubmitting}>
          <Send className="w-4 h-4 mr-2" />
          Enviar mensaje
        </OklaButton>

        <p className="text-xs text-okla-slate text-center">
          Al enviar este mensaje, aceptas nuestros{' '}
          <a href="/terms" className="text-okla-gold hover:underline">
            Términos de uso
          </a>{' '}
          y{' '}
          <a href="/privacy" className="text-okla-gold hover:underline">
            Política de privacidad
          </a>
          .
        </p>
      </form>
    </motion.div>
  );
};

export default OklaContactForm;

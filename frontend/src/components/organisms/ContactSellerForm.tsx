import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useState } from 'react';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import { FiUser, FiMail, FiPhone, FiMessageSquare, FiCheck } from 'react-icons/fi';

const contactSchema = z.object({
  name: z.string().min(2, 'Name must be at least 2 characters'),
  email: z.string().email('Invalid email address'),
  phone: z.string().min(10, 'Phone number must be at least 10 digits'),
  message: z.string().min(10, 'Message must be at least 10 characters'),
});

type ContactFormData = z.infer<typeof contactSchema>;

interface ContactSellerFormProps {
  vehicleName: string;
  sellerName: string;
  sellerPhone: string;
}

export default function ContactSellerForm({ vehicleName, sellerName, sellerPhone }: ContactSellerFormProps) {
  const [submitted, setSubmitted] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<ContactFormData>({
    resolver: zodResolver(contactSchema),
    defaultValues: {
      message: `Hi, I'm interested in the ${vehicleName}. Is it still available?`,
    },
  });

  const onSubmit = async (data: ContactFormData) => {
    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 1000));
    console.log('Contact form submitted:', data);
    setSubmitted(true);
    setTimeout(() => {
      setSubmitted(false);
      reset();
    }, 3000);
  };

  if (submitted) {
    return (
      <div className="bg-green-50 border-2 border-green-200 rounded-xl p-8 text-center">
        <div className="w-16 h-16 bg-green-500 rounded-full flex items-center justify-center mx-auto mb-4">
          <FiCheck size={32} className="text-white" />
        </div>
        <h3 className="text-xl font-bold text-gray-900 mb-2">
          Message Sent!
        </h3>
        <p className="text-gray-600 mb-4">
          Your message has been sent to {sellerName}. They will contact you soon at the provided email or phone number.
        </p>
        <p className="text-sm text-gray-500">
          You can also contact them directly at {sellerPhone}
        </p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-card p-6">
      <div className="mb-6">
        <h2 className="text-2xl font-bold font-heading text-gray-900 mb-2">
          Contact Seller
        </h2>
        <p className="text-gray-600">
          Interested in this vehicle? Send a message to {sellerName}
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input
          label="Your Name"
          type="text"
          placeholder="John Doe"
          error={errors.name?.message}
          leftIcon={<FiUser />}
          {...register('name')}
        />

        <Input
          label="Email Address"
          type="email"
          placeholder="john@example.com"
          error={errors.email?.message}
          leftIcon={<FiMail />}
          {...register('email')}
        />

        <Input
          label="Phone Number"
          type="tel"
          placeholder="(555) 123-4567"
          error={errors.phone?.message}
          leftIcon={<FiPhone />}
          {...register('phone')}
        />

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Message
          </label>
          <div className="relative">
            <div className="absolute top-3 left-3 text-gray-400">
              <FiMessageSquare size={20} />
            </div>
            <textarea
              {...register('message')}
              rows={5}
              className={`
                w-full pl-10 pr-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent transition-all duration-200
                ${errors.message ? 'border-red-500' : 'border-gray-300'}
              `}
              placeholder="Enter your message..."
            />
          </div>
          {errors.message && (
            <p className="mt-1 text-sm text-red-600">{errors.message.message}</p>
          )}
        </div>

        <Button
          type="submit"
          variant="primary"
          size="lg"
          className="w-full"
          isLoading={isSubmitting}
        >
          Send Message
        </Button>

        <p className="text-xs text-gray-500 text-center">
          By submitting this form, you agree to be contacted by the seller regarding this vehicle.
        </p>
      </form>
    </div>
  );
}

/**
 * Contact Page
 *
 * Contact form and information
 *
 * Route: /contacto
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import { Mail, Phone, MapPin, Clock, Send, CheckCircle, MessageCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

// =============================================================================
// TYPES
// =============================================================================

interface ContactFormData {
  name: string;
  email: string;
  phone: string;
  subject: string;
  message: string;
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function ContactoPage() {
  const [formData, setFormData] = React.useState<ContactFormData>({
    name: '',
    email: '',
    phone: '',
    subject: '',
    message: '',
  });
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  const [isSubmitted, setIsSubmitted] = React.useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    // TODO: Implement API call to send message
    await new Promise(resolve => setTimeout(resolve, 1500));

    setIsSubmitting(false);
    setIsSubmitted(true);
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  if (isSubmitted) {
    return (
      <div className="flex min-h-[70vh] items-center justify-center px-4">
        <div className="text-center">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-green-100">
            <CheckCircle className="h-8 w-8 text-green-600" />
          </div>
          <h1 className="mt-6 text-2xl font-bold text-gray-900">¡Mensaje enviado!</h1>
          <p className="mt-2 max-w-md text-gray-600">
            Gracias por contactarnos. Hemos recibido tu mensaje y te responderemos a la brevedad
            posible.
          </p>
          <div className="mt-8 flex flex-col items-center gap-4 sm:flex-row sm:justify-center">
            <Button asChild className="gap-2 bg-[#00A870] hover:bg-[#009663]">
              <Link href="/">Volver al inicio</Link>
            </Button>
            <Button asChild variant="outline">
              <Link href="/vehiculos">Ver vehículos</Link>
            </Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen">
      {/* Hero */}
      <section className="bg-gradient-to-br from-[#00A870] to-[#007850] py-16 text-white">
        <div className="container mx-auto px-4">
          <div className="mx-auto max-w-2xl text-center">
            <h1 className="text-3xl font-bold md:text-4xl">Contáctanos</h1>
            <p className="mt-4 text-white/90">
              ¿Tienes preguntas, sugerencias o necesitas ayuda? Estamos aquí para ti.
            </p>
          </div>
        </div>
      </section>

      {/* Main Content */}
      <section className="py-12">
        <div className="container mx-auto px-4">
          <div className="grid gap-8 lg:grid-cols-3">
            {/* Contact Info */}
            <div className="lg:col-span-1">
              <h2 className="text-xl font-semibold text-gray-900">Información de contacto</h2>
              <p className="mt-2 text-gray-600">
                Puedes comunicarte con nosotros a través de cualquiera de estos canales.
              </p>

              <div className="mt-8 space-y-6">
                <div className="flex gap-4">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-[#00A870]/10">
                    <Mail className="h-5 w-5 text-[#00A870]" />
                  </div>
                  <div>
                    <div className="font-medium text-gray-900">Correo electrónico</div>
                    <a href="mailto:soporte@okla.com.do" className="text-[#00A870] hover:underline">
                      soporte@okla.com.do
                    </a>
                  </div>
                </div>

                <div className="flex gap-4">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-[#00A870]/10">
                    <Phone className="h-5 w-5 text-[#00A870]" />
                  </div>
                  <div>
                    <div className="font-medium text-gray-900">Teléfono</div>
                    <a href="tel:+18095551234" className="text-[#00A870] hover:underline">
                      +1 (809) 555-1234
                    </a>
                  </div>
                </div>

                <div className="flex gap-4">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-[#00A870]/10">
                    <MessageCircle className="h-5 w-5 text-[#00A870]" />
                  </div>
                  <div>
                    <div className="font-medium text-gray-900">WhatsApp</div>
                    <a
                      href="https://wa.me/18095551234"
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-[#00A870] hover:underline"
                    >
                      +1 (809) 555-1234
                    </a>
                  </div>
                </div>

                <div className="flex gap-4">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-[#00A870]/10">
                    <MapPin className="h-5 w-5 text-[#00A870]" />
                  </div>
                  <div>
                    <div className="font-medium text-gray-900">Ubicación</div>
                    <div className="text-gray-600">
                      Av. Winston Churchill #1099
                      <br />
                      Torre Acrópolis, Piso 10
                      <br />
                      Santo Domingo, RD
                    </div>
                  </div>
                </div>

                <div className="flex gap-4">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-[#00A870]/10">
                    <Clock className="h-5 w-5 text-[#00A870]" />
                  </div>
                  <div>
                    <div className="font-medium text-gray-900">Horario de atención</div>
                    <div className="text-gray-600">
                      Lunes a Viernes: 8:00 AM - 6:00 PM
                      <br />
                      Sábados: 9:00 AM - 1:00 PM
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Contact Form */}
            <div className="lg:col-span-2">
              <Card>
                <CardContent className="p-6">
                  <h2 className="text-xl font-semibold text-gray-900">Envíanos un mensaje</h2>
                  <p className="mt-2 text-gray-600">
                    Completa el formulario y te responderemos en menos de 24 horas.
                  </p>

                  <form onSubmit={handleSubmit} className="mt-8 space-y-6">
                    <div className="grid gap-6 sm:grid-cols-2">
                      <div className="space-y-2">
                        <Label htmlFor="name">Nombre completo *</Label>
                        <Input
                          id="name"
                          name="name"
                          value={formData.name}
                          onChange={handleChange}
                          placeholder="Tu nombre"
                          required
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="email">Correo electrónico *</Label>
                        <Input
                          id="email"
                          name="email"
                          type="email"
                          value={formData.email}
                          onChange={handleChange}
                          placeholder="tu@email.com"
                          required
                        />
                      </div>
                    </div>

                    <div className="grid gap-6 sm:grid-cols-2">
                      <div className="space-y-2">
                        <Label htmlFor="phone">Teléfono</Label>
                        <Input
                          id="phone"
                          name="phone"
                          type="tel"
                          value={formData.phone}
                          onChange={handleChange}
                          placeholder="(809) 555-1234"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="subject">Asunto *</Label>
                        <select
                          id="subject"
                          name="subject"
                          value={formData.subject}
                          onChange={handleChange}
                          className="border-input bg-background ring-offset-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none"
                          required
                        >
                          <option value="">Seleccionar...</option>
                          <option value="general">Consulta general</option>
                          <option value="support">Soporte técnico</option>
                          <option value="sales">Información de ventas</option>
                          <option value="dealer">Quiero ser dealer</option>
                          <option value="complaint">Queja o reclamo</option>
                          <option value="suggestion">Sugerencia</option>
                        </select>
                      </div>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="message">Mensaje *</Label>
                      <textarea
                        id="message"
                        name="message"
                        value={formData.message}
                        onChange={handleChange}
                        placeholder="Escribe tu mensaje aquí..."
                        rows={5}
                        className="border-input bg-background ring-offset-background placeholder:text-muted-foreground focus-visible:ring-ring flex w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:outline-none"
                        required
                      />
                    </div>

                    <Button
                      type="submit"
                      size="lg"
                      className="w-full gap-2 bg-[#00A870] hover:bg-[#009663] sm:w-auto"
                      disabled={isSubmitting}
                    >
                      {isSubmitting ? (
                        <>
                          <div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                          Enviando...
                        </>
                      ) : (
                        <>
                          <Send className="h-4 w-4" />
                          Enviar mensaje
                        </>
                      )}
                    </Button>
                  </form>
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      </section>

      {/* FAQ CTA */}
      <section className="bg-gray-50 py-12">
        <div className="container mx-auto px-4 text-center">
          <h2 className="text-2xl font-bold text-gray-900">¿Tienes preguntas frecuentes?</h2>
          <p className="mt-2 text-gray-600">
            Revisa nuestra sección de ayuda donde respondemos las dudas más comunes.
          </p>
          <Button asChild variant="outline" className="mt-6">
            <Link href="/ayuda">Ver preguntas frecuentes</Link>
          </Button>
        </div>
      </section>
    </div>
  );
}

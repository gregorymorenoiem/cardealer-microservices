import type { Metadata } from 'next';
import { Card, CardContent } from '@/components/ui/card';
import {
  AlertTriangle,
  ArrowRight,
  CheckCircle2,
  Clock,
  DollarSign,
  Mail,
  Shield,
} from 'lucide-react';

export const metadata: Metadata = {
  title: 'Política de Reembolso',
  description:
    'Política de reembolso de OKLA conforme a la Ley 126-02. Información sobre el derecho de retracto, condiciones y proceso de solicitud.',
};

export default function PoliticaReembolsoPage() {
  return (
    <div className="mx-auto max-w-4xl px-4 py-12">
      {/* Header */}
      <div className="mb-10 space-y-3">
        <h1 className="text-3xl font-bold text-foreground">Política de Reembolso</h1>
        <p className="text-lg text-muted-foreground">
          En cumplimiento de la <strong>Ley 126-02</strong> de Comercio Electrónico y la{' '}
          <strong>Ley 358-05</strong> de Protección al Consumidor de República Dominicana.
        </p>
        <p className="text-sm text-muted-foreground">
          Última actualización: 1 de marzo de 2026
        </p>
      </div>

      {/* Derecho de retracto */}
      <section className="mb-10">
        <div className="mb-4 flex items-center gap-2">
          <Shield className="h-6 w-6 text-primary" />
          <h2 className="text-2xl font-bold text-foreground">Derecho de Retracto</h2>
        </div>
        <Card>
          <CardContent className="space-y-4 p-5">
            <p className="text-sm text-muted-foreground">
              De conformidad con la <strong>Ley 126-02</strong> de Comercio Electrónico, Documentos
              y Firmas Digitales, los usuarios de OKLA tienen el derecho de retracto dentro de los{' '}
              <strong>siete (7) días calendario</strong> siguientes a la adquisición de cualquier
              servicio pagado en la plataforma.
            </p>
            <Card className="border-primary/20 bg-primary/5">
              <CardContent className="flex items-start gap-3 p-4">
                <Clock className="mt-0.5 h-5 w-5 shrink-0 text-primary" />
                <div>
                  <p className="text-sm font-semibold text-foreground">
                    7 días calendario para solicitar reembolso
                  </p>
                  <p className="mt-1 text-xs text-muted-foreground">
                    A partir de la fecha de compra o contratación del servicio.
                  </p>
                </div>
              </CardContent>
            </Card>
          </CardContent>
        </Card>
      </section>

      {/* Servicios aplicables */}
      <section className="mb-10">
        <div className="mb-4 flex items-center gap-2">
          <DollarSign className="h-6 w-6 text-primary" />
          <h2 className="text-2xl font-bold text-foreground">Servicios Aplicables</h2>
        </div>
        <Card>
          <CardContent className="space-y-4 p-5">
            <div>
              <h3 className="font-semibold text-foreground">Publicación de vehículos ($29/listado)</h3>
              <ul className="mt-2 space-y-2 text-sm text-muted-foreground">
                <li className="flex items-start gap-2">
                  <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-green-500" />
                  Reembolso completo si la publicación no ha sido activada o no ha recibido
                  visualizaciones.
                </li>
                <li className="flex items-start gap-2">
                  <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0 text-amber-500" />
                  No aplica reembolso si el listado ya fue publicado y recibió interacciones
                  (visualizaciones, contactos o favoritos).
                </li>
              </ul>
            </div>

            <div className="border-t border-border pt-4">
              <h3 className="font-semibold text-foreground">
                Suscripciones de dealer ($49–$299/mes)
              </h3>
              <ul className="mt-2 space-y-2 text-sm text-muted-foreground">
                <li className="flex items-start gap-2">
                  <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-green-500" />
                  Reembolso prorrateado por los días no utilizados si se solicita dentro del
                  período de retracto de 7 días.
                </li>
                <li className="flex items-start gap-2">
                  <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-green-500" />
                  La suscripción se cancelará inmediatamente al procesar el reembolso.
                </li>
                <li className="flex items-start gap-2">
                  <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0 text-amber-500" />
                  Después del período de 7 días, la cancelación surtirá efecto al final del ciclo
                  de facturación actual.
                </li>
              </ul>
            </div>

            <div className="border-t border-border pt-4">
              <h3 className="font-semibold text-foreground">Servicios de impulso/promoción</h3>
              <ul className="mt-2 space-y-2 text-sm text-muted-foreground">
                <li className="flex items-start gap-2">
                  <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-green-500" />
                  Reembolso completo si el servicio de impulso no ha sido ejecutado.
                </li>
                <li className="flex items-start gap-2">
                  <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0 text-amber-500" />
                  Reembolso parcial prorrateado si el período de impulso está en curso.
                </li>
              </ul>
            </div>
          </CardContent>
        </Card>
      </section>

      {/* Cómo solicitar */}
      <section className="mb-10">
        <div className="mb-4 flex items-center gap-2">
          <ArrowRight className="h-6 w-6 text-primary" />
          <h2 className="text-2xl font-bold text-foreground">Cómo Solicitar un Reembolso</h2>
        </div>
        <Card>
          <CardContent className="p-5">
            <div className="space-y-4">
              <div className="flex gap-4">
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-bold text-primary-foreground">
                  1
                </div>
                <div>
                  <h3 className="font-semibold text-foreground">Envía tu solicitud</h3>
                  <p className="mt-1 text-sm text-muted-foreground">
                    Escribe a{' '}
                    <a
                      href="mailto:reembolsos@okla.com.do"
                      className="text-primary hover:underline"
                    >
                      reembolsos@okla.com.do
                    </a>{' '}
                    con el asunto &ldquo;Solicitud de Reembolso&rdquo; e incluye: tu nombre completo,
                    email de la cuenta, número de transacción/recibo y motivo de la solicitud.
                  </p>
                </div>
              </div>

              <div className="flex gap-4">
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-bold text-primary-foreground">
                  2
                </div>
                <div>
                  <h3 className="font-semibold text-foreground">Confirmación</h3>
                  <p className="mt-1 text-sm text-muted-foreground">
                    Recibirás una confirmación de recepción dentro de 2 días hábiles con un número
                    de caso para seguimiento.
                  </p>
                </div>
              </div>

              <div className="flex gap-4">
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-bold text-primary-foreground">
                  3
                </div>
                <div>
                  <h3 className="font-semibold text-foreground">Evaluación</h3>
                  <p className="mt-1 text-sm text-muted-foreground">
                    Nuestro equipo evaluará tu solicitud y verificará que cumple con las condiciones
                    aplicables.
                  </p>
                </div>
              </div>

              <div className="flex gap-4">
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-bold text-primary-foreground">
                  4
                </div>
                <div>
                  <h3 className="font-semibold text-foreground">Procesamiento</h3>
                  <p className="mt-1 text-sm text-muted-foreground">
                    Si procede, el reembolso se realizará al mismo método de pago original dentro
                    de <strong>15 días hábiles</strong>. Recibirás una notificación cuando se
                    procese.
                  </p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </section>

      {/* Timeline */}
      <section className="mb-10">
        <Card className="border-primary/20 bg-primary/5">
          <CardContent className="flex items-start gap-3 p-5">
            <Clock className="mt-0.5 h-5 w-5 shrink-0 text-primary" />
            <div>
              <h3 className="font-semibold text-foreground">Plazo de procesamiento</h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Los reembolsos aprobados serán procesados dentro de un plazo máximo de{' '}
                <strong>15 días hábiles</strong>. El tiempo de reflejo en tu cuenta puede variar
                según tu banco o proveedor de pago (generalmente 3-5 días hábiles adicionales).
              </p>
            </div>
          </CardContent>
        </Card>
      </section>

      {/* Contact */}
      <section>
        <Card>
          <CardContent className="p-5">
            <h3 className="mb-3 font-semibold text-foreground">Contacto para Reembolsos</h3>
            <div className="space-y-2 text-sm text-muted-foreground">
              <p className="flex items-center gap-2">
                <Mail className="h-4 w-4 text-primary" />
                Email:{' '}
                <a
                  href="mailto:reembolsos@okla.com.do"
                  className="text-primary hover:underline"
                >
                  reembolsos@okla.com.do
                </a>
              </p>
              <p className="mt-3 text-xs">
                Si no estás satisfecho con la resolución, puedes presentar una reclamación a
                través de nuestro{' '}
                <a href="/reclamaciones" className="text-primary hover:underline">
                  Sistema de Reclamaciones
                </a>{' '}
                o contactar a ProConsumidor (Tel. 809-567-7755).
              </p>
            </div>
          </CardContent>
        </Card>
      </section>
    </div>
  );
}

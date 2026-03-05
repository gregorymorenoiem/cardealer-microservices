import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Reportar Contenido',
  description:
    'Reporta contenido que infringe derechos de propiedad intelectual o viola los términos de uso de OKLA. Conforme a la Ley 65-00 de Derecho de Autor.',
};

export default function ReportarContenidoLayout({ children }: { children: React.ReactNode }) {
  return <>{children}</>;
}

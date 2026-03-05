import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Sistema de Reclamaciones',
  description:
    'Presenta una reclamación sobre productos o servicios en OKLA. Conforme a la Ley 358-05 de Protección al Consumidor de República Dominicana.',
};

export default function ReclamacionesLayout({ children }: { children: React.ReactNode }) {
  return <>{children}</>;
}

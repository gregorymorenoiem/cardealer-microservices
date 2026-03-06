import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Herramientas para Vehículos | OKLA',
  description:
    'Herramientas gratuitas para comprar, vender o importar vehículos en República Dominicana. Calculadoras, comparador y más.',
  keywords: [
    'herramientas vehiculares',
    'calculadora vehículo RD',
    'importar carro dominicana',
    'financiamiento vehicular',
  ],
};

export default function HerramientasLayout({ children }: { children: React.ReactNode }) {
  return <>{children}</>;
}

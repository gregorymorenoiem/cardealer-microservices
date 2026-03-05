import { Metadata } from 'next';

/**
 * Account pages should not be indexed by search engines.
 */
export const metadata: Metadata = {
  robots: {
    index: false,
    follow: false,
  },
};

export default function CuentaTemplate({ children }: { children: React.ReactNode }) {
  return <>{children}</>;
}

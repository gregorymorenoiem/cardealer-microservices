/**
 * Auth template — adds noindex meta tag to prevent search engines from indexing auth pages.
 * Using template.tsx because layout.tsx is 'use client' and cannot export metadata.
 */
export default function AuthTemplate({ children }: { children: React.ReactNode }) {
  return (
    <>
      <meta name="robots" content="noindex, nofollow" />
      {children}
    </>
  );
}

/**
 * Reset Password Redirect Page
 *
 * This page redirects from /reset-password to /restablecer-contrasena
 * to maintain consistency with Spanish URLs while supporting email links
 * that use English route names.
 */

import { redirect } from 'next/navigation';

interface ResetPasswordPageProps {
  searchParams: Promise<{ token?: string }>;
}

export default async function ResetPasswordPage({ searchParams }: ResetPasswordPageProps) {
  const params = await searchParams;
  const token = params.token;

  // Redirect to the Spanish version with the token
  if (token) {
    redirect(`/restablecer-contrasena?token=${encodeURIComponent(token)}`);
  }

  // If no token, redirect to the password recovery page
  redirect('/recuperar-contrasena');
}

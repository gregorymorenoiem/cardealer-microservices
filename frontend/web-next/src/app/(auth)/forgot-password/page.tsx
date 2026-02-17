/**
 * Forgot Password Redirect Page
 *
 * This page redirects from /forgot-password to /recuperar-contrasena
 * to maintain consistency with Spanish URLs while supporting email links
 * that use English route names (e.g., security alert emails).
 */

import { redirect } from 'next/navigation';

export default async function ForgotPasswordPage() {
  redirect('/recuperar-contrasena');
}

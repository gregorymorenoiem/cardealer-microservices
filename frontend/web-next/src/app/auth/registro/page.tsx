/**
 * Orphan Registration Page - Redirects to the real registration page
 *
 * This page existed as a duplicate with a fake submit handler.
 * Now redirects to the actual /registro route.
 */

import { redirect } from 'next/navigation';

export default function OrphanRegisterPage() {
  redirect('/registro');
}

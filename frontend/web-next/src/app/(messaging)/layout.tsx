/**
 * Messaging Layout - Centered like other account pages
 *
 * Uses same container width as navbar (max-w-7xl) for consistency
 * No sidebar - full width messaging experience
 *
 * Pattern: Navbar → Centered container → Full-width content
 *
 * Security:
 * - Middleware handles server-side route protection (first layer)
 * - AuthGuard provides client-side defense-in-depth (second layer)
 */

'use client';

import * as React from 'react';
import { Navbar } from '@/components/layout';
import { AuthGuard } from '@/components/auth/auth-guard';

interface MessagingLayoutProps {
  children: React.ReactNode;
}

export default function MessagingLayout({ children }: MessagingLayoutProps) {
  return (
    <AuthGuard loginPath="/login">
      <div className="bg-muted/50 flex h-screen flex-col">
        {/* Navbar - Fixed at top */}
        <Navbar />

        {/* Centered container with same max-width as navbar - fills remaining height */}
        <main className="mx-auto flex w-full max-w-7xl flex-1 flex-col px-4 py-6 sm:px-6 lg:px-8">
          {children}
        </main>
      </div>
    </AuthGuard>
  );
}

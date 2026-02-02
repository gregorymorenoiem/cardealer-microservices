/**
 * Auth Layout
 * Shared layout for authentication pages (login, register, forgot password)
 */

import Image from 'next/image';
import Link from 'next/link';

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen">
      {/* Left side - Hero image (hidden on mobile) */}
      <div className="bg-primary relative hidden lg:flex lg:w-1/2">
        {/* Background Image */}
        <Image
          src="/auth-hero.jpg"
          alt="Vehículo premium"
          fill
          priority
          className="object-cover opacity-80"
          sizes="50vw"
        />

        {/* Overlay */}
        <div className="from-primary/90 to-primary/70 absolute inset-0 bg-gradient-to-br" />

        {/* Content */}
        <div className="relative z-10 flex flex-col justify-between p-12 text-white">
          {/* Logo */}
          <Link href="/" className="flex items-center gap-2">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-white">
              <span className="text-primary text-xl font-bold">O</span>
            </div>
            <span className="text-2xl font-bold">OKLA</span>
          </Link>

          {/* Main content */}
          <div className="space-y-6">
            <h1 className="text-4xl leading-tight font-bold">
              El marketplace #1 de vehículos en República Dominicana
            </h1>
            <p className="text-lg text-white/90">
              Miles de vehículos te esperan. Encuentra tu próximo carro, jeepeta o camioneta.
            </p>

            {/* Stats */}
            <div className="flex gap-8 pt-4">
              <div>
                <div className="text-3xl font-bold">15K+</div>
                <div className="text-sm text-white/80">Vehículos</div>
              </div>
              <div>
                <div className="text-3xl font-bold">500+</div>
                <div className="text-sm text-white/80">Dealers</div>
              </div>
              <div>
                <div className="text-3xl font-bold">50K+</div>
                <div className="text-sm text-white/80">Usuarios</div>
              </div>
            </div>
          </div>

          {/* Footer */}
          <div className="text-sm text-white/70">
            © {new Date().getFullYear()} OKLA. Todos los derechos reservados.
          </div>
        </div>
      </div>

      {/* Right side - Auth form */}
      <div className="flex flex-1 items-center justify-center bg-gray-50 p-6 sm:p-12">
        <div className="w-full max-w-md">
          {/* Mobile logo */}
          <div className="mb-8 text-center lg:hidden">
            <Link href="/" className="inline-flex items-center gap-2">
              <div className="bg-primary flex h-10 w-10 items-center justify-center rounded-lg">
                <span className="text-xl font-bold text-white">O</span>
              </div>
              <span className="text-2xl font-bold text-gray-900">OKLA</span>
            </Link>
          </div>

          {children}
        </div>
      </div>
    </div>
  );
}

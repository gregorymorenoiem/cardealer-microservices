import { Loader2 } from 'lucide-react';

export default function Loading() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50">
      <div className="text-center">
        <div className="relative">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-emerald-100">
            <Loader2 className="h-8 w-8 animate-spin text-emerald-600" />
          </div>
        </div>

        <p className="mt-4 text-sm text-gray-600">Cargando...</p>
      </div>
    </div>
  );
}

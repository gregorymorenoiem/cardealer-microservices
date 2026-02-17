import { Loader2 } from 'lucide-react';

export default function Loading() {
  return (
    <div className="bg-background flex min-h-screen items-center justify-center">
      <div className="text-center" role="status" aria-live="polite">
        <div className="relative">
          <div className="bg-primary/10 mx-auto flex h-16 w-16 items-center justify-center rounded-full">
            <Loader2 className="text-primary h-8 w-8 animate-spin" />
          </div>
        </div>

        <p className="text-muted-foreground mt-4 text-sm">Cargando...</p>
      </div>
    </div>
  );
}

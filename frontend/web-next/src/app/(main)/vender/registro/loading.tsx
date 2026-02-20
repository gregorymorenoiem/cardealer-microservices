import { Loader2 } from 'lucide-react';

export default function Loading() {
  return (
    <div className="flex min-h-[60vh] items-center justify-center">
      <div className="text-center">
        <Loader2 className="mx-auto h-8 w-8 animate-spin text-[#00A870]" />
        <p className="mt-4 text-gray-500">Preparando registro...</p>
      </div>
    </div>
  );
}

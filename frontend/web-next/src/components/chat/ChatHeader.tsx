'use client';

import { X, RotateCcw, PhoneCall, Minus } from 'lucide-react';

interface ChatHeaderProps {
  botName: string;
  isConnected: boolean;
  onClose: () => void;
  onMinimize: () => void;
  onReset: () => void;
  onTransfer: () => void;
}

export function ChatHeader({
  botName,
  isConnected,
  onClose,
  onMinimize,
  onReset,
  onTransfer,
}: ChatHeaderProps) {
  return (
    <div className="flex items-center justify-between rounded-t-2xl bg-gradient-to-r from-[#00A870] to-[#009663] px-4 py-3 text-white">
      {/* Left — Bot info */}
      <div className="flex items-center gap-3">
        <div className="flex h-9 w-9 items-center justify-center rounded-full bg-white/20">
          <span className="text-lg font-bold">O</span>
        </div>
        <div>
          <h3 className="text-sm leading-tight font-semibold">{botName}</h3>
          <div className="flex items-center gap-1.5">
            <span
              className={`inline-block h-2 w-2 rounded-full ${
                isConnected ? 'bg-green-300' : 'bg-gray-300'
              }`}
            />
            <span className="text-[11px] text-white/80">
              {isConnected ? 'En línea' : 'Conectando...'}
            </span>
          </div>
        </div>
      </div>

      {/* Right — Actions */}
      <div className="flex items-center gap-1">
        <button
          onClick={onTransfer}
          className="rounded-lg p-1.5 transition-colors hover:bg-white/20"
          title="Hablar con un agente"
          aria-label="Transferir a agente"
        >
          <PhoneCall className="h-4 w-4" />
        </button>
        <button
          onClick={onReset}
          className="rounded-lg p-1.5 transition-colors hover:bg-white/20"
          title="Nuevo chat"
          aria-label="Reiniciar chat"
        >
          <RotateCcw className="h-4 w-4" />
        </button>
        <button
          onClick={onMinimize}
          className="rounded-lg p-1.5 transition-colors hover:bg-white/20"
          title="Minimizar"
          aria-label="Minimizar chat"
        >
          <Minus className="h-4 w-4" />
        </button>
        <button
          onClick={onClose}
          className="rounded-lg p-1.5 transition-colors hover:bg-white/20"
          title="Cerrar chat"
          aria-label="Cerrar chat"
        >
          <X className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}

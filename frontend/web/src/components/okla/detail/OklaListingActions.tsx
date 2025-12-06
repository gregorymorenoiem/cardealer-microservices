import { motion } from 'framer-motion';
import { Heart, Share2, Bookmark, Flag, Printer, Copy, Check } from 'lucide-react';
import { useState } from 'react';

interface OklaListingActionsProps {
  listingId: string;
  isFavorite?: boolean;
  isSaved?: boolean;
  onFavorite?: () => void;
  onSave?: () => void;
  onShare?: () => void;
  onPrint?: () => void;
  onReport?: () => void;
  variant?: 'horizontal' | 'vertical' | 'icons-only';
  shareUrl?: string;
}

export const OklaListingActions = ({
  listingId,
  isFavorite = false,
  isSaved = false,
  onFavorite,
  onSave,
  onShare,
  onPrint,
  onReport,
  variant = 'horizontal',
  shareUrl,
}: OklaListingActionsProps) => {
  const [copied, setCopied] = useState(false);
  const [isShareOpen, setIsShareOpen] = useState(false);

  const handleCopyLink = async () => {
    const url = shareUrl || window.location.href;
    await navigator.clipboard.writeText(url);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const shareOptions = [
    {
      name: 'Facebook',
      icon: (
        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
          <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
        </svg>
      ),
      color: 'bg-[#1877f2]',
      onClick: () => window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(shareUrl || window.location.href)}`, '_blank'),
    },
    {
      name: 'WhatsApp',
      icon: (
        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
          <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z" />
        </svg>
      ),
      color: 'bg-[#25d366]',
      onClick: () => window.open(`https://wa.me/?text=${encodeURIComponent(shareUrl || window.location.href)}`, '_blank'),
    },
    {
      name: 'Twitter',
      icon: (
        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
          <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z" />
        </svg>
      ),
      color: 'bg-black',
      onClick: () => window.open(`https://twitter.com/intent/tweet?url=${encodeURIComponent(shareUrl || window.location.href)}`, '_blank'),
    },
    {
      name: 'Email',
      icon: (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
        </svg>
      ),
      color: 'bg-gray-600',
      onClick: () => window.open(`mailto:?body=${encodeURIComponent(shareUrl || window.location.href)}`, '_blank'),
    },
  ];

  const ActionButton = ({
    icon,
    label,
    onClick,
    active = false,
    activeColor = 'text-red-500',
  }: {
    icon: React.ReactNode;
    label: string;
    onClick?: () => void;
    active?: boolean;
    activeColor?: string;
  }) => {
    if (variant === 'icons-only') {
      return (
        <motion.button
          onClick={onClick}
          className={`w-10 h-10 rounded-full border flex items-center justify-center transition-colors ${
            active
              ? `${activeColor} border-current bg-current/10`
              : 'text-okla-slate border-okla-cream hover:border-okla-navy hover:text-okla-navy'
          }`}
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
        >
          {icon}
        </motion.button>
      );
    }

    return (
      <motion.button
        onClick={onClick}
        className={`flex items-center gap-2 px-4 py-2 rounded-lg border transition-colors ${
          active
            ? `${activeColor} border-current bg-current/10`
            : 'text-okla-slate border-okla-cream hover:border-okla-navy hover:text-okla-navy'
        }`}
        whileHover={{ scale: 1.02 }}
        whileTap={{ scale: 0.98 }}
      >
        {icon}
        <span className="text-sm font-medium">{label}</span>
      </motion.button>
    );
  };

  return (
    <div className="relative">
      <div
        className={`flex ${
          variant === 'vertical' ? 'flex-col gap-2' : 'flex-wrap gap-3'
        }`}
      >
        {/* Favorite */}
        <ActionButton
          icon={
            <Heart
              className={`w-5 h-5 ${isFavorite ? 'fill-current' : ''}`}
            />
          }
          label={isFavorite ? 'Guardado' : 'Favorito'}
          onClick={onFavorite}
          active={isFavorite}
          activeColor="text-red-500"
        />

        {/* Save */}
        <ActionButton
          icon={
            <Bookmark
              className={`w-5 h-5 ${isSaved ? 'fill-current' : ''}`}
            />
          }
          label={isSaved ? 'Guardado' : 'Guardar'}
          onClick={onSave}
          active={isSaved}
          activeColor="text-okla-gold"
        />

        {/* Share */}
        <div className="relative">
          <ActionButton
            icon={<Share2 className="w-5 h-5" />}
            label="Compartir"
            onClick={() => setIsShareOpen(!isShareOpen)}
          />

          {/* Share Dropdown */}
          {isShareOpen && (
            <motion.div
              className="absolute top-full mt-2 right-0 bg-white rounded-xl shadow-lg border border-okla-cream p-4 z-20 min-w-[200px]"
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
            >
              <p className="text-sm font-semibold text-okla-navy mb-3">Compartir en:</p>
              <div className="flex gap-2 mb-4">
                {shareOptions.map((option) => (
                  <motion.button
                    key={option.name}
                    onClick={() => {
                      option.onClick();
                      setIsShareOpen(false);
                    }}
                    className={`w-10 h-10 rounded-full ${option.color} text-white flex items-center justify-center`}
                    whileHover={{ scale: 1.1 }}
                    whileTap={{ scale: 0.95 }}
                    title={option.name}
                  >
                    {option.icon}
                  </motion.button>
                ))}
              </div>
              <button
                onClick={handleCopyLink}
                className="w-full flex items-center justify-center gap-2 py-2 px-3 bg-okla-cream/50 hover:bg-okla-cream rounded-lg text-sm text-okla-navy transition-colors"
              >
                {copied ? (
                  <>
                    <Check className="w-4 h-4 text-green-600" />
                    ¡Copiado!
                  </>
                ) : (
                  <>
                    <Copy className="w-4 h-4" />
                    Copiar enlace
                  </>
                )}
              </button>
            </motion.div>
          )}
        </div>

        {/* Print */}
        {onPrint && (
          <ActionButton
            icon={<Printer className="w-5 h-5" />}
            label="Imprimir"
            onClick={onPrint}
          />
        )}

        {/* Report */}
        {onReport && (
          <ActionButton
            icon={<Flag className="w-5 h-5" />}
            label="Reportar"
            onClick={onReport}
          />
        )}
      </div>

      {/* Overlay to close share menu */}
      {isShareOpen && (
        <div
          className="fixed inset-0 z-10"
          onClick={() => setIsShareOpen(false)}
        />
      )}
    </div>
  );
};

export default OklaListingActions;

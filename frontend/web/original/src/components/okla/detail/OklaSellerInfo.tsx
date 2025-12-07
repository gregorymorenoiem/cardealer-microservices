import { motion } from 'framer-motion';
import { Star, Shield, MapPin, Calendar, MessageCircle, Phone, Mail, ExternalLink } from 'lucide-react';
import { OklaButton } from '../../atoms/okla/OklaButton';
import { OklaBadge } from '../../atoms/okla/OklaBadge';

interface SellerInfo {
  id: string;
  name: string;
  type: 'dealer' | 'private' | 'certified';
  avatar?: string;
  verified: boolean;
  rating?: number;
  reviewCount?: number;
  memberSince: string;
  location: string;
  responseRate?: number;
  responseTime?: string;
  listingsCount?: number;
}

interface OklaSellerInfoProps {
  seller: SellerInfo;
  onContact?: () => void;
  onCall?: () => void;
  onMessage?: () => void;
  onViewProfile?: () => void;
  variant?: 'card' | 'inline' | 'compact';
}

const getSellerTypeLabel = (type: SellerInfo['type']) => {
  switch (type) {
    case 'dealer':
      return 'Dealer';
    case 'certified':
      return 'Dealer Certificado';
    case 'private':
      return 'Vendedor Particular';
    default:
      return 'Vendedor';
  }
};

export const OklaSellerInfo = ({
  seller,
  onContact,
  onCall,
  onMessage,
  onViewProfile,
  variant = 'card',
}: OklaSellerInfoProps) => {
  const renderRating = () => {
    if (!seller.rating) return null;
    return (
      <div className="flex items-center gap-1">
        <Star className="w-4 h-4 text-okla-gold fill-okla-gold" />
        <span className="font-semibold text-okla-navy">{seller.rating.toFixed(1)}</span>
        {seller.reviewCount && (
          <span className="text-okla-slate text-sm">({seller.reviewCount} reseñas)</span>
        )}
      </div>
    );
  };

  if (variant === 'compact') {
    return (
      <motion.div
        className="flex items-center gap-3"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
      >
        {/* Avatar */}
        <div className="relative">
          <div className="w-12 h-12 rounded-full bg-okla-cream overflow-hidden">
            {seller.avatar ? (
              <img src={seller.avatar} alt={seller.name} className="w-full h-full object-cover" />
            ) : (
              <div className="w-full h-full flex items-center justify-center bg-okla-navy text-white font-bold">
                {seller.name.charAt(0)}
              </div>
            )}
          </div>
          {seller.verified && (
            <div className="absolute -bottom-1 -right-1 w-5 h-5 bg-blue-500 rounded-full flex items-center justify-center">
              <Shield className="w-3 h-3 text-white" />
            </div>
          )}
        </div>

        {/* Info */}
        <div className="flex-1 min-w-0">
          <p className="font-semibold text-okla-navy truncate">{seller.name}</p>
          <p className="text-xs text-okla-slate">{getSellerTypeLabel(seller.type)}</p>
        </div>

        {/* Rating */}
        {seller.rating && (
          <div className="flex items-center gap-1">
            <Star className="w-4 h-4 text-okla-gold fill-okla-gold" />
            <span className="font-medium text-okla-navy">{seller.rating.toFixed(1)}</span>
          </div>
        )}
      </motion.div>
    );
  }

  if (variant === 'inline') {
    return (
      <motion.div
        className="flex items-center justify-between p-4 bg-white rounded-xl border border-okla-cream"
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
      >
        <div className="flex items-center gap-4">
          {/* Avatar */}
          <div className="relative">
            <div className="w-14 h-14 rounded-full bg-okla-cream overflow-hidden">
              {seller.avatar ? (
                <img src={seller.avatar} alt={seller.name} className="w-full h-full object-cover" />
              ) : (
                <div className="w-full h-full flex items-center justify-center bg-okla-navy text-white text-xl font-bold">
                  {seller.name.charAt(0)}
                </div>
              )}
            </div>
            {seller.verified && (
              <div className="absolute -bottom-1 -right-1 w-6 h-6 bg-blue-500 rounded-full flex items-center justify-center border-2 border-white">
                <Shield className="w-3 h-3 text-white" />
              </div>
            )}
          </div>

          {/* Info */}
          <div>
            <div className="flex items-center gap-2 mb-1">
              <p className="font-display font-bold text-okla-navy">{seller.name}</p>
              <OklaBadge variant={seller.type === 'certified' ? 'solid' : 'outline'} size="sm">
                {getSellerTypeLabel(seller.type)}
              </OklaBadge>
            </div>
            <div className="flex items-center gap-4 text-sm text-okla-slate">
              {renderRating()}
              <span className="flex items-center gap-1">
                <MapPin className="w-3.5 h-3.5" />
                {seller.location}
              </span>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex items-center gap-2">
          {onMessage && (
            <OklaButton variant="outline" size="sm" onClick={onMessage}>
              <MessageCircle className="w-4 h-4 mr-1" />
              Mensaje
            </OklaButton>
          )}
          {onCall && (
            <OklaButton size="sm" onClick={onCall}>
              <Phone className="w-4 h-4 mr-1" />
              Llamar
            </OklaButton>
          )}
        </div>
      </motion.div>
    );
  }

  // Card variant (default)
  return (
    <motion.div
      className="bg-white rounded-2xl border border-okla-cream p-6 shadow-sm"
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4 }}
    >
      {/* Header */}
      <div className="flex items-start gap-4 mb-6">
        {/* Avatar */}
        <div className="relative">
          <div className="w-16 h-16 rounded-xl bg-okla-cream overflow-hidden">
            {seller.avatar ? (
              <img src={seller.avatar} alt={seller.name} className="w-full h-full object-cover" />
            ) : (
              <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-okla-navy to-okla-navy/80 text-white text-2xl font-bold">
                {seller.name.charAt(0)}
              </div>
            )}
          </div>
          {seller.verified && (
            <div className="absolute -bottom-2 -right-2 w-7 h-7 bg-blue-500 rounded-full flex items-center justify-center border-2 border-white shadow-sm">
              <Shield className="w-4 h-4 text-white" />
            </div>
          )}
        </div>

        {/* Info */}
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-1">
            <h3 className="font-display font-bold text-xl text-okla-navy">{seller.name}</h3>
          </div>
          <OklaBadge variant={seller.type === 'certified' ? 'solid' : 'outline'}>
            {getSellerTypeLabel(seller.type)}
          </OklaBadge>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 mb-6">
        {seller.rating && (
          <div className="bg-okla-cream/50 rounded-xl p-3 text-center">
            <div className="flex items-center justify-center gap-1 mb-1">
              <Star className="w-5 h-5 text-okla-gold fill-okla-gold" />
              <span className="text-xl font-bold text-okla-navy">{seller.rating.toFixed(1)}</span>
            </div>
            <p className="text-xs text-okla-slate">
              {seller.reviewCount} reseñas
            </p>
          </div>
        )}
        {seller.responseRate && (
          <div className="bg-okla-cream/50 rounded-xl p-3 text-center">
            <p className="text-xl font-bold text-okla-navy">{seller.responseRate}%</p>
            <p className="text-xs text-okla-slate">Tasa de respuesta</p>
          </div>
        )}
      </div>

      {/* Details */}
      <div className="space-y-3 mb-6 text-sm">
        <div className="flex items-center gap-3 text-okla-slate">
          <MapPin className="w-4 h-4 text-okla-gold" />
          <span>{seller.location}</span>
        </div>
        <div className="flex items-center gap-3 text-okla-slate">
          <Calendar className="w-4 h-4 text-okla-gold" />
          <span>Miembro desde {seller.memberSince}</span>
        </div>
        {seller.responseTime && (
          <div className="flex items-center gap-3 text-okla-slate">
            <MessageCircle className="w-4 h-4 text-okla-gold" />
            <span>Responde en {seller.responseTime}</span>
          </div>
        )}
        {seller.listingsCount && (
          <div className="flex items-center gap-3 text-okla-slate">
            <ExternalLink className="w-4 h-4 text-okla-gold" />
            <span>{seller.listingsCount} publicaciones activas</span>
          </div>
        )}
      </div>

      {/* Actions */}
      <div className="space-y-3">
        {onContact && (
          <OklaButton className="w-full" onClick={onContact}>
            <Mail className="w-4 h-4 mr-2" />
            Contactar vendedor
          </OklaButton>
        )}
        <div className="grid grid-cols-2 gap-3">
          {onCall && (
            <OklaButton variant="outline" onClick={onCall}>
              <Phone className="w-4 h-4 mr-2" />
              Llamar
            </OklaButton>
          )}
          {onMessage && (
            <OklaButton variant="outline" onClick={onMessage}>
              <MessageCircle className="w-4 h-4 mr-2" />
              Mensaje
            </OklaButton>
          )}
        </div>
        {onViewProfile && (
          <button
            onClick={onViewProfile}
            className="w-full text-center text-sm text-okla-gold hover:underline font-medium"
          >
            Ver perfil completo
          </button>
        )}
      </div>
    </motion.div>
  );
};

export default OklaSellerInfo;

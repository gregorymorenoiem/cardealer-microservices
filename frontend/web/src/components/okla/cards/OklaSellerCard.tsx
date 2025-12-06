import { motion } from 'framer-motion';
import { Star, MapPin, Shield, Award, MessageCircle } from 'lucide-react';
import { OklaButton } from '../../atoms/okla/OklaButton';
import { OklaBadge } from '../../atoms/okla/OklaBadge';

interface OklaSellerCardProps {
  id: string;
  name: string;
  avatar?: string;
  coverImage?: string;
  rating: number;
  reviews: number;
  listings: number;
  location: string;
  verified?: boolean;
  premium?: boolean;
  memberSince?: string;
  responseTime?: string;
  onClick?: () => void;
  onContact?: () => void;
  variant?: 'default' | 'compact' | 'featured';
}

export const OklaSellerCard = ({
  id,
  name,
  avatar,
  coverImage,
  rating,
  reviews,
  listings,
  location,
  verified = false,
  premium = false,
  memberSince,
  responseTime,
  onClick,
  onContact,
  variant = 'default',
}: OklaSellerCardProps) => {
  if (variant === 'compact') {
    return (
      <motion.div
        className="flex items-center gap-4 p-4 bg-white rounded-xl shadow-sm hover:shadow-md transition-shadow cursor-pointer"
        onClick={onClick}
        whileHover={{ y: -2 }}
      >
        <div className="relative">
          {avatar ? (
            <img
              src={avatar}
              alt={name}
              className="w-14 h-14 rounded-full object-cover border-2 border-okla-cream"
            />
          ) : (
            <div className="w-14 h-14 rounded-full bg-okla-gold/20 flex items-center justify-center text-okla-gold text-xl font-bold">
              {name.charAt(0)}
            </div>
          )}
          {verified && (
            <div className="absolute -bottom-1 -right-1 w-5 h-5 bg-green-500 rounded-full flex items-center justify-center">
              <Shield className="w-3 h-3 text-white" />
            </div>
          )}
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="font-semibold text-okla-navy truncate">{name}</h3>
          <div className="flex items-center gap-2 text-sm text-okla-slate">
            <Star className="w-4 h-4 text-okla-gold" fill="currentColor" />
            {rating.toFixed(1)} ({reviews})
          </div>
        </div>
      </motion.div>
    );
  }

  if (variant === 'featured') {
    return (
      <motion.article
        className="bg-white rounded-2xl overflow-hidden shadow-lg cursor-pointer group"
        onClick={onClick}
        whileHover={{ y: -8 }}
        transition={{ duration: 0.3 }}
      >
        {/* Cover Image */}
        <div className="relative h-32 overflow-hidden">
          {coverImage ? (
            <motion.img
              src={coverImage}
              alt=""
              className="w-full h-full object-cover"
              whileHover={{ scale: 1.05 }}
              transition={{ duration: 0.6 }}
            />
          ) : (
            <div className="w-full h-full bg-gradient-to-br from-okla-navy to-okla-charcoal" />
          )}
          <div className="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent" />
          {premium && (
            <div className="absolute top-3 right-3">
              <OklaBadge variant="premium">
                <Award className="w-3 h-3 mr-1" />
                Premium
              </OklaBadge>
            </div>
          )}
        </div>

        {/* Avatar */}
        <div className="relative -mt-12 px-6">
          <div className="relative inline-block">
            {avatar ? (
              <img
                src={avatar}
                alt={name}
                className="w-24 h-24 rounded-2xl object-cover border-4 border-white shadow-lg"
              />
            ) : (
              <div className="w-24 h-24 rounded-2xl bg-okla-gold flex items-center justify-center text-white text-3xl font-bold border-4 border-white shadow-lg">
                {name.charAt(0)}
              </div>
            )}
            {verified && (
              <div className="absolute -bottom-2 -right-2 w-8 h-8 bg-green-500 rounded-full flex items-center justify-center shadow-md">
                <Shield className="w-4 h-4 text-white" />
              </div>
            )}
          </div>
        </div>

        {/* Content */}
        <div className="p-6 pt-4">
          <h3 className="text-xl font-semibold text-okla-navy group-hover:text-okla-gold transition-colors">
            {name}
          </h3>
          <div className="flex items-center gap-2 mt-2 text-okla-slate text-sm">
            <MapPin className="w-4 h-4" />
            {location}
          </div>

          {/* Stats */}
          <div className="grid grid-cols-3 gap-4 mt-6 py-4 border-t border-b border-okla-cream">
            <div className="text-center">
              <div className="flex items-center justify-center gap-1">
                <Star className="w-5 h-5 text-okla-gold" fill="currentColor" />
                <span className="text-lg font-bold text-okla-navy">{rating.toFixed(1)}</span>
              </div>
              <span className="text-xs text-okla-slate">Rating</span>
            </div>
            <div className="text-center">
              <span className="text-lg font-bold text-okla-navy">{reviews}</span>
              <span className="text-xs text-okla-slate block">Reseñas</span>
            </div>
            <div className="text-center">
              <span className="text-lg font-bold text-okla-navy">{listings}</span>
              <span className="text-xs text-okla-slate block">Anuncios</span>
            </div>
          </div>

          {/* Meta Info */}
          <div className="flex flex-wrap gap-4 mt-4 text-sm text-okla-slate">
            {memberSince && (
              <span>Miembro desde {memberSince}</span>
            )}
            {responseTime && (
              <span>Responde en {responseTime}</span>
            )}
          </div>

          {/* Actions */}
          <div className="mt-6">
            <OklaButton
              variant="primary"
              className="w-full"
              leftIcon={<MessageCircle className="w-4 h-4" />}
              onClick={(e) => {
                e.stopPropagation();
                onContact?.();
              }}
            >
              Contactar
            </OklaButton>
          </div>
        </div>
      </motion.article>
    );
  }

  // Default variant
  return (
    <motion.article
      className="bg-white rounded-2xl p-6 shadow-sm hover:shadow-lg transition-shadow cursor-pointer group text-center"
      onClick={onClick}
      whileHover={{ y: -4 }}
      transition={{ duration: 0.3 }}
    >
      {/* Avatar */}
      <div className="relative inline-block mb-4">
        {avatar ? (
          <img
            src={avatar}
            alt={name}
            className="w-20 h-20 rounded-full object-cover border-2 border-okla-cream"
          />
        ) : (
          <div className="w-20 h-20 rounded-full bg-okla-gold/20 flex items-center justify-center text-okla-gold text-2xl font-bold">
            {name.charAt(0)}
          </div>
        )}
        {verified && (
          <div className="absolute -bottom-1 -right-1 w-6 h-6 bg-green-500 rounded-full flex items-center justify-center shadow-md">
            <Shield className="w-3.5 h-3.5 text-white" />
          </div>
        )}
        {premium && (
          <div className="absolute -top-1 -right-1 w-6 h-6 bg-okla-gold rounded-full flex items-center justify-center shadow-md">
            <Award className="w-3.5 h-3.5 text-white" />
          </div>
        )}
      </div>

      <h3 className="text-lg font-semibold text-okla-navy group-hover:text-okla-gold transition-colors">
        {name}
      </h3>
      
      <div className="flex items-center justify-center gap-1 mt-2 text-okla-slate text-sm">
        <MapPin className="w-4 h-4" />
        {location}
      </div>

      <div className="flex items-center justify-center gap-4 mt-4 text-sm">
        <div className="flex items-center gap-1">
          <Star className="w-4 h-4 text-okla-gold" fill="currentColor" />
          <span className="font-medium text-okla-navy">{rating.toFixed(1)}</span>
          <span className="text-okla-slate">({reviews})</span>
        </div>
        <span className="text-okla-slate">•</span>
        <span className="text-okla-slate">{listings} anuncios</span>
      </div>

      <OklaButton
        variant="outline"
        size="sm"
        className="mt-4"
        onClick={(e) => {
          e.stopPropagation();
          onContact?.();
        }}
      >
        Ver Perfil
      </OklaButton>
    </motion.article>
  );
};

export default OklaSellerCard;

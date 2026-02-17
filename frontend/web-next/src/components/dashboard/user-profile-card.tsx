/**
 * User Profile Card Component
 *
 * Reusable component for displaying user profile information
 * across admin views, user listings, and profile sections.
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import {
  Mail,
  Phone,
  MapPin,
  Calendar,
  Shield,
  ShieldCheck,
  ShieldAlert,
  Building2,
  Car,
  Star,
  ExternalLink,
} from 'lucide-react';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { cn, formatDate, formatRelativeTime } from '@/lib/utils';
import type { User as UserType } from '@/types';

// =============================================================================
// TYPES
// =============================================================================

export type UserProfileVariant = 'full' | 'compact' | 'minimal' | 'horizontal';
export type AccountTypeBadge =
  | 'admin'
  | 'platform_employee'
  | 'dealer'
  | 'dealer_employee'
  | 'buyer'
  | 'seller';

export interface UserProfileCardProps {
  /** User data */
  user: Partial<UserType> & {
    id: string;
    email: string;
    firstName?: string;
    lastName?: string;
    avatarUrl?: string | null;
    accountType?: string;
    phone?: string;
    location?: string | { city?: string; state?: string };
    isVerified?: boolean;
    isEmailVerified?: boolean;
    createdAt?: string | Date;
    lastActiveAt?: string | Date;
    listingsCount?: number;
    rating?: number;
    reviewsCount?: number;
  };
  /** Display variant */
  variant?: UserProfileVariant;
  /** Whether to show verification badge */
  showVerification?: boolean;
  /** Whether to show last active time */
  showLastActive?: boolean;
  /** Whether to show member since date */
  showMemberSince?: boolean;
  /** Whether to show listing count (for sellers) */
  showListings?: boolean;
  /** Whether to show rating */
  showRating?: boolean;
  /** Link to view full profile */
  profileLink?: string;
  /** Additional actions */
  actions?: React.ReactNode;
  /** Additional CSS classes */
  className?: string;
}

// =============================================================================
// HELPER FUNCTIONS
// =============================================================================

function getInitials(firstName?: string, lastName?: string, email?: string): string {
  if (firstName && lastName) {
    return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
  }
  if (firstName) {
    return firstName.charAt(0).toUpperCase();
  }
  if (email) {
    return email.charAt(0).toUpperCase();
  }
  return 'U';
}

function getDisplayName(firstName?: string, lastName?: string, email?: string): string {
  if (firstName && lastName) {
    return `${firstName} ${lastName}`;
  }
  if (firstName) {
    return firstName;
  }
  if (email) {
    return email.split('@')[0];
  }
  return 'Usuario';
}

function getAccountTypeBadge(
  accountType?: string,
  listingsCount?: number
): { label: string; className: string; icon: React.ReactNode } | null {
  if (accountType === 'admin') {
    return {
      label: 'Admin',
      className: 'bg-red-100 text-red-700 border-red-200',
      icon: <Shield className="mr-1 h-3 w-3" />,
    };
  }
  if (accountType === 'platform_employee') {
    return {
      label: 'Plataforma',
      className: 'bg-red-100 text-red-700 border-red-200',
      icon: <Shield className="mr-1 h-3 w-3" />,
    };
  }
  if (accountType === 'dealer') {
    return {
      label: 'Dealer',
      className: 'bg-blue-100 text-blue-700 border-blue-200',
      icon: <Building2 className="mr-1 h-3 w-3" />,
    };
  }
  if (accountType === 'dealer_employee') {
    return {
      label: 'Empleado Dealer',
      className: 'bg-blue-100 text-blue-700 border-blue-200',
      icon: <Building2 className="mr-1 h-3 w-3" />,
    };
  }
  if (accountType === 'seller') {
    return {
      label: 'Vendedor',
      className: 'bg-green-100 text-green-700 border-green-200',
      icon: <Car className="mr-1 h-3 w-3" />,
    };
  }
  if ((listingsCount ?? 0) > 0) {
    return {
      label: 'Vendedor',
      className: 'bg-green-100 text-green-700 border-green-200',
      icon: <Car className="mr-1 h-3 w-3" />,
    };
  }
  return null;
}

function getLocationString(location?: string | { city?: string; state?: string }): string | null {
  if (!location) return null;
  if (typeof location === 'string') return location;
  const parts = [location.city, location.state].filter(Boolean);
  return parts.length > 0 ? parts.join(', ') : null;
}

// =============================================================================
// VERIFICATION BADGE COMPONENT
// =============================================================================

function VerificationBadge({ isVerified }: { isVerified?: boolean }) {
  if (isVerified) {
    return (
      <Badge variant="outline" className="border-green-200 bg-green-50 text-green-700">
        <ShieldCheck className="mr-1 h-3 w-3" />
        Verificado
      </Badge>
    );
  }
  return (
    <Badge variant="outline" className="border-amber-200 bg-amber-50 text-amber-700">
      <ShieldAlert className="mr-1 h-3 w-3" />
      Sin verificar
    </Badge>
  );
}

// =============================================================================
// RATING COMPONENT
// =============================================================================

function RatingDisplay({ rating, reviewsCount }: { rating?: number; reviewsCount?: number }) {
  if (!rating) return null;
  return (
    <div className="flex items-center gap-1 text-sm">
      <Star className="h-4 w-4 fill-amber-400 text-amber-400" />
      <span className="font-medium">{rating.toFixed(1)}</span>
      {reviewsCount !== undefined && (
        <span className="text-muted-foreground">({reviewsCount} reseñas)</span>
      )}
    </div>
  );
}

// =============================================================================
// COMPACT VARIANT
// =============================================================================

function CompactProfile({ user, showVerification, profileLink, className }: UserProfileCardProps) {
  const initials = getInitials(user.firstName, user.lastName, user.email);
  const displayName = getDisplayName(user.firstName, user.lastName, user.email);
  const accountBadge = getAccountTypeBadge(user.accountType, user.listingsCount);

  const content = (
    <div className={cn('flex items-center gap-3', className)}>
      <Avatar className="h-10 w-10">
        <AvatarImage src={user.avatarUrl || undefined} alt={displayName} />
        <AvatarFallback className="bg-primary/10 text-primary">{initials}</AvatarFallback>
      </Avatar>
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <p className="truncate font-medium">{displayName}</p>
          {accountBadge && (
            <Badge variant="outline" className={cn('text-xs', accountBadge.className)}>
              {accountBadge.icon}
              {accountBadge.label}
            </Badge>
          )}
          {showVerification && user.isVerified && (
            <ShieldCheck className="h-4 w-4 text-green-600" />
          )}
        </div>
        <p className="text-muted-foreground truncate text-sm">{user.email}</p>
      </div>
    </div>
  );

  if (profileLink) {
    return (
      <Link href={profileLink} className="block transition-opacity hover:opacity-80">
        {content}
      </Link>
    );
  }

  return content;
}

// =============================================================================
// MINIMAL VARIANT
// =============================================================================

function MinimalProfile({ user, className }: UserProfileCardProps) {
  const initials = getInitials(user.firstName, user.lastName, user.email);
  const displayName = getDisplayName(user.firstName, user.lastName, user.email);

  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Avatar className="h-8 w-8">
        <AvatarImage src={user.avatarUrl || undefined} alt={displayName} />
        <AvatarFallback className="bg-primary/10 text-primary text-xs">{initials}</AvatarFallback>
      </Avatar>
      <span className="truncate text-sm font-medium">{displayName}</span>
    </div>
  );
}

// =============================================================================
// HORIZONTAL VARIANT
// =============================================================================

function HorizontalProfile({
  user,
  showVerification,
  showLastActive,
  showListings,
  showRating,
  profileLink,
  actions,
  className,
}: UserProfileCardProps) {
  const initials = getInitials(user.firstName, user.lastName, user.email);
  const displayName = getDisplayName(user.firstName, user.lastName, user.email);
  const accountBadge = getAccountTypeBadge(user.accountType, user.listingsCount);
  const locationStr = getLocationString(user.location);

  return (
    <div className={cn('bg-card flex items-center gap-4 rounded-lg border p-4', className)}>
      <Avatar className="h-14 w-14">
        <AvatarImage src={user.avatarUrl || undefined} alt={displayName} />
        <AvatarFallback className="bg-primary/10 text-primary text-lg">{initials}</AvatarFallback>
      </Avatar>

      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <h3 className="truncate font-semibold">{displayName}</h3>
          {accountBadge && (
            <Badge variant="outline" className={cn('text-xs', accountBadge.className)}>
              {accountBadge.icon}
              {accountBadge.label}
            </Badge>
          )}
          {showVerification && <VerificationBadge isVerified={user.isVerified} />}
        </div>

        <div className="text-muted-foreground mt-1 flex flex-wrap items-center gap-x-4 gap-y-1 text-sm">
          <span className="flex items-center gap-1">
            <Mail className="h-3.5 w-3.5" />
            {user.email}
          </span>
          {user.phone && (
            <span className="flex items-center gap-1">
              <Phone className="h-3.5 w-3.5" />
              {user.phone}
            </span>
          )}
          {locationStr && (
            <span className="flex items-center gap-1">
              <MapPin className="h-3.5 w-3.5" />
              {locationStr}
            </span>
          )}
          {showLastActive && user.lastActiveAt && (
            <span className="flex items-center gap-1">
              <Calendar className="h-3.5 w-3.5" />
              Activo {formatRelativeTime(user.lastActiveAt)}
            </span>
          )}
          {showListings && user.listingsCount !== undefined && (
            <span className="flex items-center gap-1">
              <Car className="h-3.5 w-3.5" />
              {user.listingsCount} vehículos
            </span>
          )}
        </div>

        {showRating && user.rating && (
          <div className="mt-2">
            <RatingDisplay rating={user.rating} reviewsCount={user.reviewsCount} />
          </div>
        )}
      </div>

      <div className="flex items-center gap-2">
        {profileLink && (
          <Button variant="outline" size="sm" asChild>
            <Link href={profileLink}>
              <ExternalLink className="mr-2 h-4 w-4" />
              Ver perfil
            </Link>
          </Button>
        )}
        {actions}
      </div>
    </div>
  );
}

// =============================================================================
// FULL VARIANT (CARD)
// =============================================================================

function FullProfile({
  user,
  showVerification = true,
  showLastActive = true,
  showMemberSince = true,
  showListings = true,
  showRating = true,
  profileLink,
  actions,
  className,
}: UserProfileCardProps) {
  const initials = getInitials(user.firstName, user.lastName, user.email);
  const displayName = getDisplayName(user.firstName, user.lastName, user.email);
  const accountBadge = getAccountTypeBadge(user.accountType, user.listingsCount);
  const locationStr = getLocationString(user.location);

  return (
    <Card className={className}>
      <CardHeader className="pb-4">
        <div className="flex items-start gap-4">
          <Avatar className="h-16 w-16">
            <AvatarImage src={user.avatarUrl || undefined} alt={displayName} />
            <AvatarFallback className="bg-primary/10 text-primary text-xl">
              {initials}
            </AvatarFallback>
          </Avatar>

          <div className="min-w-0 flex-1">
            <div className="flex flex-wrap items-center gap-2">
              <h3 className="text-lg font-semibold">{displayName}</h3>
              {accountBadge && (
                <Badge variant="outline" className={accountBadge.className}>
                  {accountBadge.icon}
                  {accountBadge.label}
                </Badge>
              )}
            </div>
            {showVerification && (
              <div className="mt-1">
                <VerificationBadge isVerified={user.isVerified} />
              </div>
            )}
            {showRating && user.rating && (
              <div className="mt-2">
                <RatingDisplay rating={user.rating} reviewsCount={user.reviewsCount} />
              </div>
            )}
          </div>

          {profileLink && (
            <Button variant="ghost" size="sm" asChild>
              <Link href={profileLink}>
                <ExternalLink className="h-4 w-4" />
              </Link>
            </Button>
          )}
        </div>
      </CardHeader>

      <CardContent className="space-y-3">
        <div className="text-muted-foreground space-y-2 text-sm">
          <div className="flex items-center gap-2">
            <Mail className="h-4 w-4" />
            <span>{user.email}</span>
          </div>
          {user.phone && (
            <div className="flex items-center gap-2">
              <Phone className="h-4 w-4" />
              <span>{user.phone}</span>
            </div>
          )}
          {locationStr && (
            <div className="flex items-center gap-2">
              <MapPin className="h-4 w-4" />
              <span>{locationStr}</span>
            </div>
          )}
          {showListings && user.listingsCount !== undefined && (
            <div className="flex items-center gap-2">
              <Car className="h-4 w-4" />
              <span>{user.listingsCount} vehículos publicados</span>
            </div>
          )}
        </div>

        <div className="text-muted-foreground border-border flex flex-wrap gap-4 border-t pt-3 text-xs">
          {showMemberSince && user.createdAt && (
            <span>Miembro desde {formatDate(user.createdAt)}</span>
          )}
          {showLastActive && user.lastActiveAt && (
            <span>Activo {formatRelativeTime(user.lastActiveAt)}</span>
          )}
        </div>

        {actions && <div className="flex gap-2 pt-2">{actions}</div>}
      </CardContent>
    </Card>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function UserProfileCard(props: UserProfileCardProps) {
  const { variant = 'full' } = props;

  switch (variant) {
    case 'compact':
      return <CompactProfile {...props} />;
    case 'minimal':
      return <MinimalProfile {...props} />;
    case 'horizontal':
      return <HorizontalProfile {...props} />;
    case 'full':
    default:
      return <FullProfile {...props} />;
  }
}

// =============================================================================
// EXPORTS
// =============================================================================

export { VerificationBadge, RatingDisplay, getInitials, getDisplayName };

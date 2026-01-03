import { type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { 
  FiAlertCircle, 
  FiSearch, 
  FiHeart, 
  FiShoppingBag,
  FiInbox
} from 'react-icons/fi';

export type EmptyStateType = 'no-results' | 'no-favorites' | 'error' | 'no-listings' | 'inbox';

interface EmptyStateProps {
  type?: EmptyStateType;
  title?: string;
  message?: string;
  icon?: ReactNode;
  actionLabel?: string;
  actionHref?: string;
  onAction?: () => void;
}

const presets: Record<EmptyStateType, { icon: ReactNode; title: string; message: string; actionLabel?: string; actionHref?: string }> = {
  'no-results': {
    icon: <FiSearch className="text-gray-400" size={64} />,
    title: 'No vehicles found',
    message: 'Try adjusting your filters or search criteria to find what you\'re looking for.',
    actionLabel: 'Clear Filters',
  },
  'no-favorites': {
    icon: <FiHeart className="text-gray-400" size={64} />,
    title: 'No favorites yet',
    message: 'Start exploring and save your favorite vehicles to view them here.',
    actionLabel: 'Browse Vehicles',
    actionHref: '/browse',
  },
  'error': {
    icon: <FiAlertCircle className="text-red-400" size={64} />,
    title: 'Something went wrong',
    message: 'We couldn\'t load the data. Please try again later.',
    actionLabel: 'Try Again',
  },
  'no-listings': {
    icon: <FiShoppingBag className="text-gray-400" size={64} />,
    title: 'No listings yet',
    message: 'You haven\'t created any vehicle listings. Start selling your car today!',
    actionLabel: 'Create Listing',
    actionHref: '/sell',
  },
  'inbox': {
    icon: <FiInbox className="text-gray-400" size={64} />,
    title: 'No messages',
    message: 'You don\'t have any messages yet. Start a conversation with sellers or buyers.',
  },
};

export default function EmptyState({
  type = 'no-results',
  title: customTitle,
  message: customMessage,
  icon: customIcon,
  actionLabel: customActionLabel,
  actionHref: customActionHref,
  onAction,
}: EmptyStateProps) {
  const preset = presets[type];
  
  const icon = customIcon || preset.icon;
  const title = customTitle || preset.title;
  const message = customMessage || preset.message;
  const actionLabel = customActionLabel || preset.actionLabel;
  const actionHref = customActionHref || preset.actionHref;

  const renderAction = () => {
    if (!actionLabel) return null;

    const buttonClasses = `
      px-6 py-3 rounded-lg font-medium transition-all duration-200
      ${type === 'error' 
        ? 'bg-red-600 text-white hover:bg-red-700 hover:shadow-lg' 
        : 'bg-primary text-white hover:bg-primary-600 hover:shadow-lg'
      }
    `;

    if (actionHref) {
      return (
        <Link to={actionHref} className={buttonClasses}>
          {actionLabel}
        </Link>
      );
    }

    if (onAction) {
      return (
        <button onClick={onAction} className={buttonClasses}>
          {actionLabel}
        </button>
      );
    }

    return null;
  };

  return (
    <div className="bg-white rounded-xl shadow-card p-12 text-center">
      {/* Icon */}
      <div className="flex justify-center mb-6">
        {icon}
      </div>

      {/* Title */}
      <h3 className="text-2xl font-bold text-gray-900 mb-3">
        {title}
      </h3>

      {/* Message */}
      <p className="text-gray-600 max-w-md mx-auto mb-8">
        {message}
      </p>

      {/* Action Button */}
      {renderAction()}

      {/* Suggestions for no-results */}
      {type === 'no-results' && (
        <div className="mt-10 pt-8 border-t border-gray-200">
          <p className="text-sm font-semibold text-gray-700 mb-4">
            Suggestions:
          </p>
          <ul className="text-sm text-gray-600 space-y-2">
            <li>• Check your spelling</li>
            <li>• Try more general keywords</li>
            <li>• Expand your price range</li>
            <li>• Remove some filters</li>
          </ul>
        </div>
      )}
    </div>
  );
}

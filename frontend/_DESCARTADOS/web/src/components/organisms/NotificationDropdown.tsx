import { useState, useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { FiBell, FiX, FiLoader } from 'react-icons/fi';
import { useNotificationCenter } from '@/hooks/useNotifications';

// Local notification type for display
interface DisplayNotification {
  id: string;
  type: string;
  title: string;
  message: string;
  createdAt: string;
  isRead: boolean;
  link?: string;
  icon?: string;
}

const NotificationDropdown = () => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { notifications, unreadCount, isLoading, markAsRead, markAllAsRead, isMarkingAllAsRead } =
    useNotificationCenter(1, 10);

  const handleBellClick = () => {
    setIsOpen(!isOpen);
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  const handleNotificationClick = (notificationId: string, actionUrl?: string) => {
    markAsRead(notificationId);
    if (actionUrl) {
      setIsOpen(false);
    }
  };

  return (
    <div className="relative" ref={dropdownRef}>
      {/* Notification Bell Button */}
      <button
        onClick={handleBellClick}
        className="relative p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
        aria-label="Notifications"
      >
        <FiBell className="w-6 h-6" />
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs font-bold rounded-full w-5 h-5 flex items-center justify-center">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {/* Dropdown */}
      {isOpen && (
        <div
          className="absolute right-0 mt-2 w-96 max-w-[calc(100vw-32px)] bg-white rounded-lg shadow-xl border border-gray-200 z-50"
          style={{ marginRight: 'max(0px, calc((100% + 384px) - 100vw + 16px))' }}
        >
          {/* Header */}
          <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200">
            <h3 className="font-semibold text-gray-900">
              Notifications {unreadCount > 0 && `(${unreadCount})`}
            </h3>
            <button
              onClick={() => setIsOpen(false)}
              className="text-gray-400 hover:text-gray-600 transition-colors"
            >
              <FiX className="w-5 h-5" />
            </button>
          </div>

          {/* Notifications List */}
          <div className="max-h-96 overflow-y-auto">
            {isLoading ? (
              <div className="px-4 py-8 text-center text-gray-500">
                <FiLoader className="w-8 h-8 animate-spin mx-auto text-gray-400" />
                <p className="text-sm mt-2">Loading notifications...</p>
              </div>
            ) : notifications.length === 0 ? (
              <div className="px-4 py-8 text-center text-gray-500">
                <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-3">
                  <FiBell className="w-8 h-8 text-gray-400" />
                </div>
                <p className="font-medium">No notifications</p>
                <p className="text-sm mt-1">You're all caught up!</p>
              </div>
            ) : (
              notifications.map((notification) => (
                <div
                  key={notification.id}
                  className={`px-4 py-3 border-b border-gray-100 hover:bg-gray-50 transition-colors cursor-pointer ${
                    !notification.isRead ? 'bg-blue-50' : ''
                  }`}
                >
                  {notification.link ? (
                    <Link
                      to={notification.link}
                      onClick={() => handleNotificationClick(notification.id, notification.link)}
                      className="block"
                    >
                      <NotificationContent notification={notification} />
                    </Link>
                  ) : (
                    <div onClick={() => handleNotificationClick(notification.id)}>
                      <NotificationContent notification={notification} />
                    </div>
                  )}
                </div>
              ))
            )}
          </div>

          {/* Footer */}
          {notifications.length > 0 && (
            <div className="px-4 py-3 bg-gray-50 border-t border-gray-200 flex gap-2">
              <button
                onClick={() => markAllAsRead()}
                disabled={isMarkingAllAsRead || unreadCount === 0}
                className="flex-1 text-sm text-primary hover:text-primary-dark font-medium transition-colors disabled:opacity-50"
              >
                {isMarkingAllAsRead ? 'Marking...' : 'Mark all as read'}
              </button>
              <Link
                to="/notifications"
                onClick={() => setIsOpen(false)}
                className="flex-1 text-sm text-gray-600 hover:text-gray-900 font-medium transition-colors text-right"
              >
                View All
              </Link>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

// Notification Content Component
const NotificationContent = ({ notification }: { notification: DisplayNotification }) => {
  const getNotificationIcon = (type: string, icon?: string) => {
    if (icon) return icon;
    switch (type) {
      case 'message':
        return '💬';
      case 'approval':
      case 'listing':
        return '✅';
      case 'favorite':
        return '❤️';
      case 'sale':
        return '🎉';
      case 'vehicle':
        return '🚗';
      case 'system':
        return '💰';
      default:
        return '📢';
    }
  };

  return (
    <div className="flex gap-3">
      <div className="text-2xl flex-shrink-0">
        {getNotificationIcon(notification.type, notification.icon)}
      </div>
      <div className="flex-1 min-w-0">
        <p className="font-medium text-gray-900 text-sm">{notification.title}</p>
        <p className="text-sm text-gray-600 mt-0.5 line-clamp-2">{notification.message}</p>
        <p className="text-xs text-gray-400 mt-1">
          {new Date(notification.createdAt).toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
          })}
        </p>
      </div>
      {!notification.isRead && (
        <div className="w-2 h-2 bg-blue-500 rounded-full flex-shrink-0 mt-1" />
      )}
    </div>
  );
};

export default NotificationDropdown;

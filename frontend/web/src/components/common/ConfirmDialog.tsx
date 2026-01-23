import { Fragment, useEffect, useRef } from 'react';
import { FiAlertTriangle, FiX, FiCheck, FiInfo, FiAlertCircle } from 'react-icons/fi';

export type ConfirmDialogVariant = 'danger' | 'warning' | 'info' | 'success';

interface ConfirmDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  message: string | React.ReactNode;
  confirmText?: string;
  cancelText?: string;
  variant?: ConfirmDialogVariant;
  isLoading?: boolean;
  /** Additional info text shown below the main message */
  infoText?: string;
}

const variantStyles: Record<
  ConfirmDialogVariant,
  {
    iconBg: string;
    icon: typeof FiAlertTriangle;
    iconColor: string;
    buttonBg: string;
    buttonHover: string;
  }
> = {
  danger: {
    iconBg: 'bg-red-100',
    icon: FiAlertTriangle,
    iconColor: 'text-red-600',
    buttonBg: 'bg-red-600',
    buttonHover: 'hover:bg-red-700',
  },
  warning: {
    iconBg: 'bg-yellow-100',
    icon: FiAlertCircle,
    iconColor: 'text-yellow-600',
    buttonBg: 'bg-yellow-600',
    buttonHover: 'hover:bg-yellow-700',
  },
  info: {
    iconBg: 'bg-blue-100',
    icon: FiInfo,
    iconColor: 'text-blue-600',
    buttonBg: 'bg-blue-600',
    buttonHover: 'hover:bg-blue-700',
  },
  success: {
    iconBg: 'bg-green-100',
    icon: FiCheck,
    iconColor: 'text-green-600',
    buttonBg: 'bg-green-600',
    buttonHover: 'hover:bg-green-700',
  },
};

export default function ConfirmDialog({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  variant = 'danger',
  isLoading = false,
  infoText,
}: ConfirmDialogProps) {
  const cancelButtonRef = useRef<HTMLButtonElement>(null);
  const style = variantStyles[variant];
  const IconComponent = style.icon;

  // Handle escape key
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen && !isLoading) {
        onClose();
      }
    };
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen, isLoading, onClose]);

  // Focus trap and initial focus
  useEffect(() => {
    if (isOpen) {
      cancelButtonRef.current?.focus();
    }
  }, [isOpen]);

  // Prevent body scroll when modal is open
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <Fragment>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 z-50 transition-opacity duration-200"
        onClick={!isLoading ? onClose : undefined}
        aria-hidden="true"
      />

      {/* Dialog */}
      <div className="fixed inset-0 z-50 overflow-y-auto">
        <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
          <div
            className="relative transform overflow-hidden rounded-xl bg-white text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg animate-in fade-in zoom-in-95 duration-200"
            role="dialog"
            aria-modal="true"
            aria-labelledby="modal-title"
          >
            {/* Close button */}
            <button
              onClick={onClose}
              disabled={isLoading}
              className="absolute right-4 top-4 p-1 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors disabled:opacity-50"
              aria-label="Close"
            >
              <FiX size={20} />
            </button>

            <div className="bg-white px-4 pb-4 pt-5 sm:p-6 sm:pb-4">
              <div className="sm:flex sm:items-start">
                {/* Icon */}
                <div
                  className={`mx-auto flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full ${style.iconBg} sm:mx-0 sm:h-10 sm:w-10`}
                >
                  <IconComponent className={style.iconColor} size={24} aria-hidden="true" />
                </div>

                {/* Content */}
                <div className="mt-3 text-center sm:ml-4 sm:mt-0 sm:text-left flex-1">
                  <h3
                    className="text-lg font-semibold leading-6 text-gray-900 pr-8"
                    id="modal-title"
                  >
                    {title}
                  </h3>
                  <div className="mt-2">
                    {typeof message === 'string' ? (
                      <p className="text-sm text-gray-600">{message}</p>
                    ) : (
                      message
                    )}
                  </div>
                  {infoText && (
                    <div className="mt-3 p-3 bg-gray-50 rounded-lg border border-gray-200">
                      <p className="text-xs text-gray-500 flex items-start gap-2">
                        <FiInfo className="flex-shrink-0 mt-0.5" size={14} />
                        {infoText}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6 gap-3">
              <button
                type="button"
                onClick={onConfirm}
                disabled={isLoading}
                className={`inline-flex w-full justify-center items-center rounded-lg px-4 py-2.5 text-sm font-semibold text-white shadow-sm ${style.buttonBg} ${style.buttonHover} focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-${variant === 'danger' ? 'red' : variant === 'warning' ? 'yellow' : variant === 'success' ? 'green' : 'blue'}-500 disabled:opacity-50 disabled:cursor-not-allowed sm:w-auto transition-colors`}
              >
                {isLoading ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent mr-2" />
                    Processing...
                  </>
                ) : (
                  confirmText
                )}
              </button>
              <button
                type="button"
                ref={cancelButtonRef}
                onClick={onClose}
                disabled={isLoading}
                className="mt-3 inline-flex w-full justify-center rounded-lg bg-white px-4 py-2.5 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:mt-0 sm:w-auto disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {cancelText}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Fragment>
  );
}

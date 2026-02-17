/**
 * Confirm Dialog Component
 *
 * Reusable confirmation dialog built on top of AlertDialog.
 * Supports different variants: danger, warning, info, success.
 */

'use client';

import * as React from 'react';
import { AlertTriangle, Trash2, Info, CheckCircle, Loader2 } from 'lucide-react';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { cn } from '@/lib/utils';

// =============================================================================
// TYPES
// =============================================================================

export type ConfirmDialogVariant = 'danger' | 'warning' | 'info' | 'success';

export interface ConfirmDialogProps {
  /** Whether the dialog is open */
  open: boolean;
  /** Callback when open state changes */
  onOpenChange: (open: boolean) => void;
  /** Dialog title */
  title: string;
  /** Dialog description/message */
  description: string;
  /** Confirm button text */
  confirmText?: string;
  /** Cancel button text */
  cancelText?: string;
  /** Callback when confirm is clicked */
  onConfirm: () => void | Promise<void>;
  /** Visual variant */
  variant?: ConfirmDialogVariant;
  /** Whether to show loading state on confirm */
  isLoading?: boolean;
  /** Custom icon to display */
  icon?: React.ReactNode;
}

// =============================================================================
// VARIANT CONFIGURATIONS
// =============================================================================

const variantConfig: Record<
  ConfirmDialogVariant,
  {
    icon: React.ReactNode;
    iconBg: string;
    iconColor: string;
    confirmBtnClass: string;
  }
> = {
  danger: {
    icon: <Trash2 className="h-6 w-6" />,
    iconBg: 'bg-red-100',
    iconColor: 'text-red-600',
    confirmBtnClass: 'bg-red-600 hover:bg-red-700 focus:ring-red-500 text-white',
  },
  warning: {
    icon: <AlertTriangle className="h-6 w-6" />,
    iconBg: 'bg-amber-100',
    iconColor: 'text-amber-600',
    confirmBtnClass: 'bg-amber-600 hover:bg-amber-700 focus:ring-amber-500 text-white',
  },
  info: {
    icon: <Info className="h-6 w-6" />,
    iconBg: 'bg-blue-100',
    iconColor: 'text-blue-600',
    confirmBtnClass: 'bg-blue-600 hover:bg-blue-700 focus:ring-blue-500 text-white',
  },
  success: {
    icon: <CheckCircle className="h-6 w-6" />,
    iconBg: 'bg-green-100',
    iconColor: 'text-green-600',
    confirmBtnClass: 'bg-green-600 hover:bg-green-700 focus:ring-green-500 text-white',
  },
};

// =============================================================================
// COMPONENT
// =============================================================================

export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmText = 'Confirmar',
  cancelText = 'Cancelar',
  onConfirm,
  variant = 'danger',
  isLoading = false,
  icon,
}: ConfirmDialogProps) {
  const config = variantConfig[variant];
  const displayIcon = icon || config.icon;

  const handleConfirm = async () => {
    await onConfirm();
  };

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <div className="flex items-start gap-4">
            <div
              className={cn(
                'flex h-12 w-12 items-center justify-center rounded-full',
                config.iconBg
              )}
            >
              <span className={config.iconColor}>{displayIcon}</span>
            </div>
            <div className="flex-1">
              <AlertDialogTitle className="text-lg">{title}</AlertDialogTitle>
              <AlertDialogDescription className="mt-2">{description}</AlertDialogDescription>
            </div>
          </div>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isLoading}>{cancelText}</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={isLoading}
            className={cn(config.confirmBtnClass, 'min-w-[100px]')}
          >
            {isLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Procesando...
              </>
            ) : (
              confirmText
            )}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

// =============================================================================
// HOOK FOR PROGRAMMATIC USAGE
// =============================================================================

interface ConfirmState {
  open: boolean;
  title: string;
  description: string;
  onConfirm: () => void | Promise<void>;
  variant: ConfirmDialogVariant;
  confirmText: string;
  cancelText: string;
  isLoading: boolean;
}

const defaultState: ConfirmState = {
  open: false,
  title: '',
  description: '',
  onConfirm: () => {},
  variant: 'danger',
  confirmText: 'Confirmar',
  cancelText: 'Cancelar',
  isLoading: false,
};

/**
 * Hook for programmatic confirm dialogs
 *
 * @example
 * ```tsx
 * const { confirm, ConfirmDialogComponent } = useConfirmDialog();
 *
 * const handleDelete = async () => {
 *   const confirmed = await confirm({
 *     title: '¿Eliminar usuario?',
 *     description: 'Esta acción no se puede deshacer.',
 *     variant: 'danger',
 *   });
 *   if (confirmed) {
 *     await deleteUser(id);
 *   }
 * };
 *
 * return (
 *   <>
 *     <button onClick={handleDelete}>Eliminar</button>
 *     <ConfirmDialogComponent />
 *   </>
 * );
 * ```
 */
export function useConfirmDialog() {
  const [state, setState] = React.useState<ConfirmState>(defaultState);
  const resolveRef = React.useRef<((value: boolean) => void) | null>(null);

  const confirm = React.useCallback(
    (options: {
      title: string;
      description: string;
      variant?: ConfirmDialogVariant;
      confirmText?: string;
      cancelText?: string;
    }): Promise<boolean> => {
      return new Promise(resolve => {
        resolveRef.current = resolve;
        setState({
          open: true,
          title: options.title,
          description: options.description,
          variant: options.variant || 'danger',
          confirmText: options.confirmText || 'Confirmar',
          cancelText: options.cancelText || 'Cancelar',
          onConfirm: () => {},
          isLoading: false,
        });
      });
    },
    []
  );

  const handleConfirm = React.useCallback(() => {
    resolveRef.current?.(true);
    setState(defaultState);
  }, []);

  const handleCancel = React.useCallback((open: boolean) => {
    if (!open) {
      resolveRef.current?.(false);
      setState(defaultState);
    }
  }, []);

  const ConfirmDialogComponent = React.useCallback(
    () => (
      <ConfirmDialog
        open={state.open}
        onOpenChange={handleCancel}
        title={state.title}
        description={state.description}
        variant={state.variant}
        confirmText={state.confirmText}
        cancelText={state.cancelText}
        onConfirm={handleConfirm}
        isLoading={state.isLoading}
      />
    ),
    [state, handleConfirm, handleCancel]
  );

  return { confirm, ConfirmDialogComponent };
}

// =============================================================================
// PRE-CONFIGURED DIALOGS
// =============================================================================

/** Delete confirmation dialog */
export function DeleteConfirmDialog({
  open,
  onOpenChange,
  itemName,
  onConfirm,
  isLoading,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  itemName: string;
  onConfirm: () => void | Promise<void>;
  isLoading?: boolean;
}) {
  return (
    <ConfirmDialog
      open={open}
      onOpenChange={onOpenChange}
      title={`¿Eliminar ${itemName}?`}
      description="Esta acción no se puede deshacer. Todos los datos asociados serán eliminados permanentemente."
      confirmText="Eliminar"
      variant="danger"
      onConfirm={onConfirm}
      isLoading={isLoading}
    />
  );
}

/** Logout confirmation dialog */
export function LogoutConfirmDialog({
  open,
  onOpenChange,
  onConfirm,
  isLoading,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void | Promise<void>;
  isLoading?: boolean;
}) {
  return (
    <ConfirmDialog
      open={open}
      onOpenChange={onOpenChange}
      title="¿Cerrar sesión?"
      description="Tendrás que volver a iniciar sesión para acceder a tu cuenta."
      confirmText="Cerrar sesión"
      variant="warning"
      onConfirm={onConfirm}
      isLoading={isLoading}
    />
  );
}

/** Discard changes confirmation dialog */
export function DiscardChangesDialog({
  open,
  onOpenChange,
  onConfirm,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
}) {
  return (
    <ConfirmDialog
      open={open}
      onOpenChange={onOpenChange}
      title="¿Descartar cambios?"
      description="Tienes cambios sin guardar. ¿Estás seguro de que quieres salir? Los cambios se perderán."
      confirmText="Descartar"
      cancelText="Seguir editando"
      variant="warning"
      onConfirm={onConfirm}
    />
  );
}

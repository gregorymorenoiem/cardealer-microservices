/**
 * Common Components Index
 *
 * Exports reusable components used across the application
 */

export { LanguageSwitcher } from './LanguageSwitcher';
export { LocalizedContent, LanguageBadge, useLocalizedContent } from './LocalizedContent';
export type { MultiLangText } from './LocalizedContent';

// Media upload components
export { ImageDropZone } from './ImageDropZone';
export type { ImageDropZoneProps, ImagePreview } from './ImageDropZone';

// Dialog components
export { default as ConfirmDialog } from './ConfirmDialog';
export type { ConfirmDialogVariant } from './ConfirmDialog';

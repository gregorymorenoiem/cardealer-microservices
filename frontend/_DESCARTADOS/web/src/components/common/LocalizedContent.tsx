import { useTranslation } from 'react-i18next';
import { FiGlobe } from 'react-icons/fi';

export interface MultiLangText {
  es?: string;
  en?: string;
  originalLang?: 'es' | 'en';
}

interface LocalizedContentProps {
  /** Content object with language keys */
  content: MultiLangText | string;
  /** Show language badge when content is in different language */
  showBadge?: boolean;
  /** Badge position */
  badgePosition?: 'inline' | 'top-right';
  /** Custom class for the container */
  className?: string;
  /** Render as specific element */
  as?: 'span' | 'p' | 'div' | 'h1' | 'h2' | 'h3' | 'h4';
  /** Truncate text after n lines */
  lineClamp?: number;
}

/**
 * LocalizedContent - Displays user-generated content with language indicator
 * 
 * Shows content in user's preferred language if available,
 * otherwise shows original content with a subtle badge indicating the language.
 * 
 * @example
 * // Simple string (legacy support)
 * <LocalizedContent content="Toyota RAV4 2024" />
 * 
 * // Multi-language object
 * <LocalizedContent 
 *   content={{ es: "Excelente estado", en: "Excellent condition" }}
 *   showBadge
 * />
 */
export function LocalizedContent({
  content,
  showBadge = true,
  badgePosition = 'inline',
  className = '',
  as: Component = 'span',
  lineClamp,
}: LocalizedContentProps) {
  const { i18n, t } = useTranslation('common');
  const currentLang = i18n.language as 'es' | 'en';

  // Handle simple string (legacy/non-translated content)
  if (typeof content === 'string') {
    return <Component className={className}>{content}</Component>;
  }

  // Get content in preferred language or fallback
  const preferredContent = content[currentLang];
  const fallbackLang = currentLang === 'es' ? 'en' : 'es';
  const fallbackContent = content[fallbackLang];

  // Determine what to display
  const displayContent = preferredContent || fallbackContent || '';
  const isInDifferentLanguage = !preferredContent && fallbackContent;
  const displayLang = isInDifferentLanguage ? fallbackLang : currentLang;

  // Language labels
  const langLabels: Record<string, string> = {
    es: 'ES',
    en: 'EN',
  };

  const lineClampClass = lineClamp 
    ? `line-clamp-${lineClamp}` 
    : '';

  if (!showBadge || !isInDifferentLanguage) {
    return (
      <Component className={`${className} ${lineClampClass}`}>
        {displayContent}
      </Component>
    );
  }

  // Inline badge
  if (badgePosition === 'inline') {
    return (
      <Component className={`${className} ${lineClampClass}`}>
        {displayContent}
        <span 
          className="inline-flex items-center ml-1.5 px-1.5 py-0.5 text-[10px] font-medium bg-gray-100 text-gray-500 rounded align-middle"
          title={t('content.originalLanguage', { lang: langLabels[displayLang] })}
        >
          <FiGlobe className="w-2.5 h-2.5 mr-0.5" />
          {langLabels[displayLang]}
        </span>
      </Component>
    );
  }

  // Top-right badge
  return (
    <div className="relative">
      <Component className={`${className} ${lineClampClass}`}>
        {displayContent}
      </Component>
      <span 
        className="absolute -top-1 -right-1 inline-flex items-center px-1.5 py-0.5 text-[10px] font-medium bg-gray-100 text-gray-500 rounded"
        title={t('content.originalLanguage', { lang: langLabels[displayLang] })}
      >
        <FiGlobe className="w-2.5 h-2.5 mr-0.5" />
        {langLabels[displayLang]}
      </span>
    </div>
  );
}

/**
 * Hook to get localized content from a multi-lang object
 */
export function useLocalizedContent(content: MultiLangText | string): {
  text: string;
  isTranslated: boolean;
  originalLang: 'es' | 'en';
} {
  const { i18n } = useTranslation();
  const currentLang = i18n.language as 'es' | 'en';

  if (typeof content === 'string') {
    return { text: content, isTranslated: true, originalLang: 'es' };
  }

  const preferredContent = content[currentLang];
  const fallbackLang = currentLang === 'es' ? 'en' : 'es';
  const fallbackContent = content[fallbackLang];
  const originalLang = content.originalLang || (content.es ? 'es' : 'en');

  return {
    text: preferredContent || fallbackContent || '',
    isTranslated: !!preferredContent,
    originalLang,
  };
}

/**
 * Simple language badge component for custom layouts
 */
export function LanguageBadge({ 
  lang, 
  className = '' 
}: { 
  lang: 'es' | 'en'; 
  className?: string;
}) {
  const { t } = useTranslation('common');
  const langLabels: Record<string, string> = {
    es: 'Español',
    en: 'English',
  };

  return (
    <span 
      className={`inline-flex items-center px-1.5 py-0.5 text-[10px] font-medium bg-gray-100 text-gray-500 rounded ${className}`}
      title={t('content.originalLanguage', { lang: langLabels[lang] })}
    >
      <FiGlobe className="w-2.5 h-2.5 mr-0.5" />
      {lang.toUpperCase()}
    </span>
  );
}

export default LocalizedContent;

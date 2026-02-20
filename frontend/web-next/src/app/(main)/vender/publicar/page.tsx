import { redirect } from 'next/navigation';

/**
 * Legacy route redirect — /vender/publicar → /publicar
 * The canonical publishing wizard is at /publicar (SmartPublishWizard).
 */
export default function VenderPublicarRedirect() {
  redirect('/publicar');
}

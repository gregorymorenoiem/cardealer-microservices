/**
 * Dealer Panel — Contextual Video Tutorial Configuration
 *
 * Maps each dealer panel section to a short tutorial video (< 90s, Dominican Spanish).
 * Videos are self-hosted on DigitalOcean Spaces CDN for < 2s load time (no YouTube).
 *
 * Video naming convention: /videos/tutoriales/dealer/{section-slug}.mp4
 * Recommended format: MP4 H.264, 720p, ~2MB per 60s, with Spanish (DR) audio.
 */

export interface DealerTutorial {
  /** Unique section key matching the route */
  sectionKey: string;
  /** Short title for the dialog header */
  title: string;
  /** One-line description of what the video covers */
  description: string;
  /** CDN URL to the MP4 video file */
  videoUrl: string;
  /** Poster/thumbnail image URL */
  posterUrl?: string;
  /** Duration in seconds (for display) */
  durationSeconds: number;
  /** Optional list of key topics covered */
  topics?: string[];
}

const CDN_BASE = process.env.NEXT_PUBLIC_CDN_URL || 'https://cdn.okla.com.do';
const TUTORIAL_PATH = `${CDN_BASE}/videos/tutoriales/dealer`;

export const dealerTutorials: Record<string, DealerTutorial> = {
  inventario: {
    sectionKey: 'inventario',
    title: 'Cómo manejar tu inventario',
    description: 'Aprende a publicar, editar y organizar tus vehículos en OKLA.',
    videoUrl: `${TUTORIAL_PATH}/inventario.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/inventario.jpg`,
    durationSeconds: 75,
    topics: [
      'Publicar un vehículo nuevo',
      'Editar precio y detalles',
      'Activar y pausar publicaciones',
      'Importar inventario masivo',
    ],
  },
  leads: {
    sectionKey: 'leads',
    title: 'Cómo gestionar tus leads',
    description: 'Domina el CRM de OKLA para convertir consultas en ventas.',
    videoUrl: `${TUTORIAL_PATH}/leads.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/leads.jpg`,
    durationSeconds: 80,
    topics: [
      'Ver y responder consultas',
      'Clasificar leads por prioridad',
      'Seguimiento de oportunidades',
      'Exportar datos a CSV',
    ],
  },
  publicidad: {
    sectionKey: 'publicidad',
    title: 'Cómo promocionar tus vehículos',
    description: 'Aprende a usar las herramientas de publicidad para vender más rápido.',
    videoUrl: `${TUTORIAL_PATH}/publicidad.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/publicidad.jpg`,
    durationSeconds: 85,
    topics: [
      'Crear una campaña destacada',
      'Elegir el paquete de boost ideal',
      'Ver estadísticas de rendimiento',
      'Calcular ROI de tus campañas',
    ],
  },
  analytics: {
    sectionKey: 'analytics',
    title: 'Cómo leer tus analytics',
    description: 'Entiende las métricas clave de tu negocio en OKLA.',
    videoUrl: `${TUTORIAL_PATH}/analytics.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/analytics.jpg`,
    durationSeconds: 70,
    topics: [
      'Vistas e impresiones',
      'Tasa de conversión de leads',
      'Rendimiento por vehículo',
      'Comparar períodos',
    ],
  },
  citas: {
    sectionKey: 'citas',
    title: 'Cómo gestionar tus citas',
    description: 'Organiza pruebas de manejo y visitas con compradores.',
    videoUrl: `${TUTORIAL_PATH}/citas.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/citas.jpg`,
    durationSeconds: 60,
    topics: [
      'Ver citas programadas',
      'Confirmar o reagendar',
      'Usar el calendario',
      'Notificaciones automáticas',
    ],
  },
  mensajes: {
    sectionKey: 'mensajes',
    title: 'Cómo usar el chat de OKLA',
    description: 'Comunícate directamente con compradores interesados.',
    videoUrl: `${TUTORIAL_PATH}/mensajes.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/mensajes.jpg`,
    durationSeconds: 55,
    topics: [
      'Responder mensajes rápido',
      'Ver historial de conversaciones',
      'Adjuntar fotos adicionales',
    ],
  },
  resenas: {
    sectionKey: 'resenas',
    title: 'Cómo gestionar tus reseñas',
    description: 'Construye la confianza de tu concesionario con buenas reseñas.',
    videoUrl: `${TUTORIAL_PATH}/resenas.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/resenas.jpg`,
    durationSeconds: 50,
    topics: ['Ver reseñas de compradores', 'Responder reseñas', 'Mejorar tu puntuación'],
  },
  empleados: {
    sectionKey: 'empleados',
    title: 'Cómo manejar tu equipo',
    description: 'Agrega y gestiona los empleados de tu concesionario.',
    videoUrl: `${TUTORIAL_PATH}/empleados.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/empleados.jpg`,
    durationSeconds: 45,
    topics: ['Invitar un nuevo empleado', 'Asignar roles y permisos', 'Desactivar accesos'],
  },
  pricing: {
    sectionKey: 'pricing',
    title: 'Cómo usar el Pricing IA',
    description: 'Deja que la inteligencia artificial te ayude a poner el precio correcto.',
    videoUrl: `${TUTORIAL_PATH}/pricing.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/pricing.jpg`,
    durationSeconds: 65,
    topics: [
      'Análisis de mercado automático',
      'Precio sugerido por IA',
      'Comparar con competencia',
    ],
  },
  reportes: {
    sectionKey: 'reportes',
    title: 'Cómo generar reportes',
    description: 'Descarga reportes detallados de tu actividad en OKLA.',
    videoUrl: `${TUTORIAL_PATH}/reportes.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/reportes.jpg`,
    durationSeconds: 50,
    topics: ['Reporte de ventas', 'Reporte de leads', 'Exportar a Excel/PDF'],
  },
  perfil: {
    sectionKey: 'perfil',
    title: 'Cómo configurar tu perfil',
    description: 'Completa tu perfil de concesionario para generar más confianza.',
    videoUrl: `${TUTORIAL_PATH}/perfil.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/perfil.jpg`,
    durationSeconds: 55,
    topics: [
      'Logo y fotos del dealer',
      'Horarios de atención',
      'Información de contacto',
      'Descripción del negocio',
    ],
  },
  facturacion: {
    sectionKey: 'facturacion',
    title: 'Cómo funciona la facturación',
    description: 'Entiende tu plan, pagos y facturas en OKLA.',
    videoUrl: `${TUTORIAL_PATH}/facturacion.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/facturacion.jpg`,
    durationSeconds: 60,
    topics: [
      'Ver tu plan actual',
      'Historial de pagos',
      'Descargar facturas',
      'Cambiar método de pago',
    ],
  },
  configuracion: {
    sectionKey: 'configuracion',
    title: 'Configuración del dealer',
    description: 'Personaliza las opciones de tu concesionario.',
    videoUrl: `${TUTORIAL_PATH}/configuracion.mp4`,
    posterUrl: `${TUTORIAL_PATH}/posters/configuracion.jpg`,
    durationSeconds: 45,
    topics: ['Notificaciones', 'Preferencias de contacto', 'Ubicaciones'],
  },
};

/**
 * Get tutorial config for a specific dealer section.
 * Returns undefined if no tutorial is configured for that section.
 */
export function getDealerTutorial(sectionKey: string): DealerTutorial | undefined {
  return dealerTutorials[sectionKey];
}

/**
 * Format duration for display (e.g., "1:15")
 */
export function formatTutorialDuration(seconds: number): string {
  const mins = Math.floor(seconds / 60);
  const secs = seconds % 60;
  return `${mins}:${secs.toString().padStart(2, '0')}`;
}

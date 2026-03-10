/**
 * Metric Humanizer — Traduce métricas técnicas a frases accionables
 * en español dominicano para dealers del Segmento B sin experiencia digital.
 *
 * Principio: Cada número debe responder "¿y qué hago con eso?"
 *
 * Ejemplo ANTES:  "CTR: 2.3% WoW +40%"
 * Ejemplo DESPUÉS: "De cada 100 personas que ven tu anuncio, 2 tocan para saber más.
 *                   Eso es un 40% mejor que la semana pasada. 🎉"
 */

// =============================================================================
// CORE FORMATTERS
// =============================================================================

const fmtNum = (n: number) => new Intl.NumberFormat('es-DO').format(Math.round(n));
const fmtPct = (n: number) => `${n.toFixed(1)}%`;
const fmtDOP = (n: number) =>
  new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    maximumFractionDigits: 0,
  }).format(n);

// =============================================================================
// CHANGE DESCRIPTORS — How to describe % changes in plain language
// =============================================================================

/**
 * Converts a percentage change into a human-readable Dominican Spanish phrase.
 * e.g. +40 → "un 40% más que antes 🎉"
 * e.g. -15 → "un 15% menos que antes"
 * e.g. 0  → "igual que antes"
 */
export function describeChange(change: number): string {
  if (change === 0) return 'igual que antes';
  const abs = Math.abs(change);
  if (change > 0) {
    if (abs >= 50) return `un ${fmtPct(abs)} más que antes 🚀`;
    if (abs >= 20) return `un ${fmtPct(abs)} más que antes 🎉`;
    return `un ${fmtPct(abs)} más que antes`;
  }
  if (abs >= 30) return `un ${fmtPct(abs)} menos que antes ⚠️`;
  return `un ${fmtPct(abs)} menos que antes`;
}

/**
 * Describes a change with context period
 */
export function describeChangeWithPeriod(change: number, period: string): string {
  if (change === 0) return `Igual que ${period}`;
  const abs = Math.abs(change);
  if (change > 0) {
    const emoji = abs >= 20 ? ' 🎉' : '';
    return `${fmtPct(abs)} más que ${period}${emoji}`;
  }
  const emoji = abs >= 30 ? ' ⚠️' : '';
  return `${fmtPct(abs)} menos que ${period}${emoji}`;
}

// =============================================================================
// METRIC LABELS — Human-friendly labels for technical terms
// =============================================================================

/** Replaces "Leads Generados" everywhere with something a dealer understands */
export const LABELS = {
  // — Core metrics —
  leads: 'Personas interesadas',
  leadsGenerated: 'Personas que te contactaron',
  leadsShort: 'Interesados',
  totalViews: 'Personas que vieron tus publicaciones',
  viewsShort: 'Vistas',
  contacts: 'Personas que te escribieron',
  favorites: 'Personas que guardaron tu vehículo',
  activeListings: 'Vehículos publicados',
  soldThisMonth: 'Vehículos vendidos',

  // — Conversion —
  conversionRate: '¿Cuántos compran?',
  conversionRateDesc: 'De cada 100 interesados, cuántos terminan comprando',
  conversionFunnel: '¿Cómo llegan a comprarte?',

  // — Performance —
  responseRate: 'Rapidez de respuesta',
  responseRateDesc: 'Porcentaje de mensajes que respondes',
  avgDaysOnMarket: 'Tiempo promedio para vender',
  revenueThisMonth: 'Lo que has ganado este mes',

  // — Engagement —
  engagement: 'Interacción',
  engagementScore: 'Nivel de interés',
  engagementMetrics: 'Cómo interactúa la gente con tus publicaciones',

  // — Advertising —
  impressions: 'Veces que apareció tu anuncio',
  impressionsShort: 'Apariciones',
  clicks: 'Personas que tocaron tu anuncio',
  clicksShort: 'Toques',
  ctr: 'Efectividad del anuncio',
  ctrDesc: 'De cada 100 personas que ven tu anuncio, cuántas tocan para saber más',
  cpc: 'Costo por cada toque',
  cpcDesc: 'Lo que te cuesta cada persona que toca tu anuncio',
  cpm: 'Costo por 1,000 apariciones',
  cpmDesc: 'Lo que cuesta que 1,000 personas vean tu anuncio',
  cpl: 'Costo por cada interesado',
  cplDesc: 'Lo que te cuesta cada persona que te contacta',
  cac: 'Costo por cada venta',
  cacDesc: 'Lo que te cuesta lograr cada venta',
  qualityScore: 'Calidad del anuncio',
  qualityScoreDesc: 'Qué tan bien está hecho tu anuncio (1-10)',
  budgetUtilization: 'Cuánto de tu presupuesto has usado',
  roi: 'Retorno de inversión',
  roiDesc: '¿Cuánto gano por cada peso que invierto?',
  revenue: 'Ganancias',
  estimatedLeads: 'Interesados esperados',

  // — Traffic —
  trafficSources: 'De dónde vienen tus visitantes',
  bounceRate: 'Personas que se van rápido',

  // — Tab names —
  tabOverview: 'Resumen',
  tabVehicles: 'Vehículos',
  tabTraffic: 'Visitantes',
  tabLeads: 'Interesados',
  tabEngagement: 'Interacción',
  tabInventory: 'Inventario',

  // — Campaign status (translated from English) —
  statusActive: 'Activa',
  statusPaused: 'Pausada',
  statusCompleted: 'Finalizada',
  statusCancelled: 'Cancelada',
  statusExpired: 'Vencida',
  statusPendingPayment: 'Pago pendiente',
} as const;

/** Translates campaign status from English backend values */
export function translateCampaignStatus(status: string): string {
  const map: Record<string, string> = {
    Active: LABELS.statusActive,
    Paused: LABELS.statusPaused,
    Completed: LABELS.statusCompleted,
    Cancelled: LABELS.statusCancelled,
    Expired: LABELS.statusExpired,
    PendingPayment: LABELS.statusPendingPayment,
  };
  return map[status] || status;
}

// =============================================================================
// ACTIONABLE PHRASES — Convert raw numbers to "so what?" sentences
// =============================================================================

/**
 * Generates an actionable phrase for total views with comparison.
 * "312 personas vieron tus publicaciones esta semana. Eso es 40% más que la semana pasada."
 */
export function humanizeViews(views: number, change?: number, period = 'este mes'): string {
  const base = `${fmtNum(views)} personas vieron tus publicaciones ${period}`;
  if (change === undefined || change === 0) return `${base}.`;
  return `${base}. Eso es ${describeChange(change)}.`;
}

/**
 * Generates an actionable phrase for leads/inquiries.
 * "8 personas preguntaron por tus vehículos este mes."
 */
export function humanizeLeads(leads: number, change?: number, period = 'este mes'): string {
  const base = `${fmtNum(leads)} personas preguntaron por tus vehículos ${period}`;
  if (change === undefined || change === 0) return `${base}.`;
  return `${base}. Eso es ${describeChange(change)}.`;
}

/**
 * Generates an actionable phrase for response rate.
 * "Respondes al 85% de los mensajes que recibes. ¡Buen trabajo!"
 */
export function humanizeResponseRate(rate: number): string {
  if (rate >= 90) return `Respondes al ${fmtPct(rate)} de los mensajes. ¡Excelente! 🏆`;
  if (rate >= 70) return `Respondes al ${fmtPct(rate)} de los mensajes. ¡Sigue así! 💪`;
  if (rate >= 50)
    return `Solo respondes al ${fmtPct(rate)} de los mensajes. Intenta responder más rápido.`;
  return `Solo respondes al ${fmtPct(rate)}. Responder rápido te ayuda a vender más. ⚠️`;
}

/**
 * Generates a CTR explanation in plain language.
 * "De cada 100 personas que ven tu anuncio, 5 tocan para ver más."
 */
export function humanizeCtr(ctrPercent: number): string {
  const perHundred = Math.round(ctrPercent);
  if (perHundred <= 0) return 'Todavía nadie ha tocado tu anuncio.';
  if (perHundred >= 10)
    return `De cada 100 personas que ven tu anuncio, ${perHundred} tocan para ver más. ¡Excelente! 🎯`;
  if (perHundred >= 5)
    return `De cada 100 personas que ven tu anuncio, ${perHundred} tocan para ver más. ¡Bien!`;
  return `De cada 100 personas que ven tu anuncio, ${perHundred} tocan para ver más. Prueba mejorar las fotos.`;
}

/**
 * Generates ROI explanation.
 * "Por cada RD$1 que inviertes, recibes RD$3.50 de ganancia."
 */
export function humanizeRoi(roiPercent: number, returnPerDollar: number): string {
  if (roiPercent <= 0) return 'Tu inversión aún no genera ganancias. Revisa tus campañas.';
  return `Por cada RD$1 que inviertes en publicidad, recibes ${fmtDOP(returnPerDollar)} de ganancia.`;
}

/**
 * Generates conversion rate explanation.
 * "De cada 100 personas interesadas, 8 terminan comprando."
 */
export function humanizeConversionRate(rate: number): string {
  const per100 = Math.round(rate);
  if (per100 <= 0) return 'Todavía no hay ventas registradas.';
  if (per100 >= 20)
    return `De cada 100 personas interesadas, ${per100} terminan comprando. ¡Excelente! 🏆`;
  if (per100 >= 10)
    return `De cada 100 personas interesadas, ${per100} terminan comprando. ¡Buen ritmo!`;
  return `De cada 100 personas interesadas, ${per100} terminan comprando.`;
}

/**
 * Generates avg days on market explanation.
 */
export function humanizeDaysOnMarket(days: number): string {
  if (days <= 7)
    return `Tus vehículos se venden en promedio en ${Math.round(days)} días. ¡Rapidísimo! 🚀`;
  if (days <= 30) return `Tus vehículos tardan en promedio ${Math.round(days)} días en venderse.`;
  if (days <= 60)
    return `Tus vehículos tardan ${Math.round(days)} días en venderse. Prueba bajar el precio o mejorar las fotos.`;
  return `Tus vehículos llevan ${Math.round(days)} días promedio sin venderse. ⚠️ Considera ajustar los precios.`;
}

/**
 * Generates vehicle performance assessment.
 */
export function humanizeVehiclePerformance(
  views: number,
  contacts: number,
  _daysOnMarket?: number
): string {
  if (views > 200 && contacts > 5)
    return 'Este vehículo está muy bien. Mucha gente lo ve y te contactan. 🔥';
  if (views > 100) return 'Tiene buena visibilidad. Sigue así y los contactos llegarán.';
  if (views > 30) return 'Va bien pero puede mejorar. Agrega más fotos y mejora la descripción.';
  return 'Pocas personas lo han visto. Considera promocionarlo o mejorar las fotos. 📸';
}

/**
 * Generates quality score explanation.
 */
export function humanizeQualityScore(score: number): string {
  if (score >= 8) return `Calidad: ${score}/10 — Tu anuncio está muy bien hecho 🌟`;
  if (score >= 5) return `Calidad: ${score}/10 — Puedes mejorar agregando mejores fotos`;
  return `Calidad: ${score}/10 — Mejora el anuncio para que más personas lo vean`;
}

// =============================================================================
// TOOLTIP TEXTS — Extended explanations for "?" icons
// =============================================================================

export const TOOLTIPS = {
  impressions:
    'Cada vez que tu anuncio aparece en la pantalla de alguien, cuenta como una aparición. Más apariciones = más personas saben que existes.',
  clicks: 'Cuando alguien ve tu anuncio y toca para ver más detalles. Significa que les interesó.',
  ctr: 'Es el porcentaje de personas que tocan tu anuncio después de verlo. Si es alto, tu anuncio es llamativo. Si es bajo, prueba con mejores fotos.',
  cpc: 'Lo que te cuesta cada vez que alguien toca tu anuncio. Mientras más bajo, mejor.',
  cpm: 'Lo que pagas para que tu anuncio aparezca 1,000 veces. Es una forma de medir si tu publicidad es costosa o barata.',
  leads:
    'Personas que vieron tu vehículo y te contactaron (por WhatsApp, llamada o mensaje). Son posibles compradores.',
  conversionRate:
    'De todas las personas que te contactan, este porcentaje termina comprando. Si es bajo, revisa cómo atiendes a los interesados.',
  responseRate:
    'El porcentaje de mensajes que respondes. Los dealers que responden rápido venden más.',
  daysOnMarket:
    'Cuántos días pasan desde que publicas un vehículo hasta que lo vendes. Menos días = mejor.',
  qualityScore:
    'OKLA evalúa la calidad de tu anuncio del 1 al 10. Buenas fotos, descripción completa y precio justo suben tu puntaje.',
  roi: 'Retorno de inversión: por cada peso que inviertes en publicidad, cuánto ganas de vuelta. Si es positivo, tu publicidad está funcionando.',
  engagement:
    'Mide cuánto interactúan las personas con tus publicaciones: cuántas ven, guardan, o te contactan.',
} as const;

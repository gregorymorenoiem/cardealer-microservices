/**
 * Guide Content Data
 *
 * Static content for buying guides.
 * Each guide has a slug, title, and full markdown-like content.
 */

export interface Guide {
  slug: string;
  title: string;
  description: string;
  icon: string;
  readTime: string;
  topics: string[];
  content: string;
  lastUpdated: string;
}

export const guides: Guide[] = [
  {
    slug: 'como-buscar-vehiculo',
    title: 'Cómo Buscar tu Vehículo Ideal',
    description:
      'Aprende a usar los filtros de OKLA para encontrar exactamente lo que buscas según tu presupuesto y necesidades.',
    icon: 'Search',
    readTime: '5 min',
    topics: ['Filtros de búsqueda', 'Comparar modelos', 'Alertas de precio'],
    lastUpdated: '2026-02-15',
    content: `## Cómo encontrar tu vehículo ideal en OKLA

Comprar un vehículo es una de las decisiones financieras más importantes. En OKLA te damos las herramientas para que encuentres exactamente lo que necesitas.

### 1. Define tu presupuesto

Antes de buscar, establece un rango de precio realista. Recuerda incluir:

- **Precio del vehículo** — el costo de adquisición
- **ITBIS (18%)** — si aplica al tipo de transacción
- **Traspaso** — generalmente RD$3,000-5,000
- **Seguro obligatorio** — varía según el vehículo
- **Mantenimiento inicial** — cambio de aceite, revisión general

### 2. Usa los filtros inteligentes

OKLA ofrece filtros avanzados para afinar tu búsqueda:

| Filtro | Para qué sirve |
|--------|----------------|
| Marca y modelo | Buscar vehículos específicos |
| Rango de precio | Ajustar a tu presupuesto |
| Año | Encontrar modelos recientes o económicos |
| Kilometraje | Evaluar el uso del vehículo |
| Ubicación | Buscar cerca de tu zona |
| Tipo de combustible | Gasolina, diesel, híbrido, eléctrico |

### 3. Búsqueda con IA

Escribe en lenguaje natural lo que buscas. Por ejemplo: "SUV familiar económico en Santo Domingo" y nuestro sistema de IA encontrará las mejores opciones.

### 4. Compara antes de decidir

Usa la herramienta de comparación para evaluar hasta 3 vehículos lado a lado. Compara especificaciones, precios y valoración de mercado.

### 5. Activa alertas de precio

¿No encuentras lo que buscas hoy? Crea una alerta y te notificaremos cuando un vehículo que coincida con tus criterios sea publicado.

**Consejo Pro:** Guarda tus búsquedas frecuentes para acceder rápidamente desde "Búsquedas Guardadas" en tu perfil.`,
  },
  {
    slug: 'verificacion-documentos',
    title: 'Verificación de Documentos',
    description:
      'Todo lo que necesitas verificar antes de comprar: título, matrícula, historial del vehículo y deudas pendientes.',
    icon: 'FileCheck',
    readTime: '8 min',
    topics: ['Verificar título', 'Historial de VIN', 'Deudas en DGII'],
    lastUpdated: '2026-02-10',
    content: `## Verificación de documentos antes de comprar

La documentación es el aspecto más crítico de cualquier compra de vehículo en República Dominicana. Un error aquí puede costarte miles de pesos.

### Documentos esenciales a verificar

**1. Certificado de Título de Propiedad**

El título es el documento más importante. Verifica:

- Que el nombre del vendedor coincida con el titular
- Que no tenga gravámenes o prendas bancarias
- Que el número de chasis (VIN) coincida con el vehículo
- Que esté vigente y sin alteraciones visibles

**2. Matrícula del vehículo**

La matrícula debe estar al día. Revisa:

- Fecha de vencimiento — no debe estar expirada
- Que los datos coincidan con el título
- Tipo de placa (particular, oficial, diplomática)

**3. Verificación en DGII**

Antes de comprar, consulta en la DGII:

| Consulta | Qué revela |
|----------|-----------|
| Estado del vehículo | Si tiene deudas de impuestos |
| Historial de traspasos | Cuántas veces ha cambiado de dueño |
| Valor fiscal | Base para calcular impuestos |
| Restricciones | Si tiene embargos o medidas judiciales |

### Verificación del VIN

El Número de Identificación del Vehículo (VIN) es único para cada unidad. Compara el VIN en:

- El título de propiedad
- La placa metálica en el tablero (visible desde afuera)
- La etiqueta en la puerta del conductor
- El motor (grabado)

**Si algún VIN no coincide, NO compres el vehículo.**

### Señales de alerta

- Vendedor que presiona para cerrar rápido
- Documentos con correcciones o tachaduras
- Precio muy por debajo del mercado
- Vendedor que no es el titular del título

**Consejo Pro:** Usa el servicio de verificación de OKLA para validar la documentación antes de la compra.`,
  },
  {
    slug: 'inspeccion-vehiculo',
    title: 'Inspección del Vehículo',
    description:
      'Guía paso a paso para inspeccionar un vehículo usado antes de comprarlo. Qué revisar en carrocería, motor e interior.',
    icon: 'Car',
    readTime: '10 min',
    topics: ['Inspección visual', 'Prueba de manejo', 'Mecánico de confianza'],
    lastUpdated: '2026-02-08',
    content: `## Guía completa de inspección de vehículos

Nunca compres un vehículo sin inspeccionarlo personalmente. Esta guía te enseña qué revisar paso a paso.

### Inspección exterior

**Carrocería:**
- Revisa toda la superficie buscando abolladuras, óxido o repintado
- Pasa la mano por los paneles — diferencias de textura indican reparación
- Verifica que las brechas entre paneles sean uniformes
- Revisa debajo del vehículo buscando óxido o fugas

**Luces y cristales:**
- Todas las luces deben funcionar (altas, bajas, direccionales, frenos, reversa)
- Los cristales no deben tener grietas
- Los limpiaparabrisas deben funcionar correctamente

**Neumáticos:**
- Desgaste uniforme indica buena alineación
- Desgaste desigual sugiere problemas de suspensión
- Revisa la profundidad del dibujo (mínimo 1.6mm)
- Verifica la fecha de fabricación (no mayor a 5 años)

### Inspección del motor

| Qué revisar | Qué buscar |
|-------------|-----------|
| Nivel de aceite | Color oscuro excesivo = falta de mantenimiento |
| Refrigerante | Nivel correcto, sin mezcla con aceite |
| Batería | Terminales limpios, sin corrosión |
| Correas | Sin grietas ni desgaste visible |
| Fugas | Manchas de aceite o líquido debajo del motor |

### Prueba de manejo

La prueba de manejo es obligatoria. Durante el recorrido verifica:

- **Arranque en frío** — debe encender sin problemas
- **Transmisión** — cambios suaves, sin ruidos extraños
- **Frenos** — frenado recto, sin vibraciones
- **Dirección** — sin juego excesivo, alineada
- **Suspensión** — sin golpes al pasar baches
- **Aire acondicionado** — enfría correctamente
- **Ruidos** — escucha atentamente con la radio apagada

### Lleva a un mecánico

Invierte RD$2,000-5,000 en una inspección mecánica profesional. Un buen mecánico puede detectar:

- Problemas de compresión del motor
- Estado de la transmisión
- Desgaste de componentes de suspensión
- Fugas ocultas
- Historial de reparaciones

**Consejo Pro:** Si el vendedor se niega a que un mecánico inspeccione el vehículo, es una señal de alerta importante.`,
  },
  {
    slug: 'financiamiento-pagos',
    title: 'Financiamiento y Pagos',
    description:
      'Opciones de financiamiento disponibles en RD, tasas de interés bancarias y cómo calcular el pago mensual.',
    icon: 'DollarSign',
    readTime: '7 min',
    topics: ['Bancos locales', 'Cuota inicial', 'Tasas de interés'],
    lastUpdated: '2026-02-12',
    content: `## Guía de financiamiento vehicular en República Dominicana

Aproximadamente el 60-70% de las compras de vehículos en RD se realizan con financiamiento. Aquí te explicamos tus opciones.

### Tipos de financiamiento

**1. Préstamo bancario tradicional**

La opción más común. Los bancos principales ofrecen:

| Banco | Tasa aprox. | Plazo máximo | Inicial mínima |
|-------|------------|-------------|----------------|
| Banco Popular | 10-14% | 72 meses | 20% |
| Banreservas | 11-15% | 60 meses | 25% |
| BHD León | 10-13% | 72 meses | 20% |
| Scotiabank | 11-14% | 60 meses | 20% |

**2. Financiamiento del dealer**

Algunos dealers ofrecen financiamiento directo con:
- Aprobación más rápida
- Menos requisitos documentales
- Tasas generalmente más altas (15-24%)

**3. Cooperativas de ahorro y crédito**

Una alternativa con tasas competitivas para sus miembros.

### Cómo calcular tu cuota

Usa nuestra calculadora de financiamiento en /herramientas/calculadora-financiamiento para obtener un estimado preciso.

La fórmula básica es: **Cuota = P × [r(1+r)^n] / [(1+r)^n – 1]**

Donde:
- **P** = Monto a financiar (precio - inicial)
- **r** = Tasa de interés mensual (tasa anual / 12)
- **n** = Número de cuotas (plazo en meses)

### Requisitos típicos

Para solicitar un préstamo vehicular necesitas:

- Cédula de identidad vigente
- Comprobante de ingresos (últimos 3 meses)
- Historial crediticio limpio
- Cuota inicial (mínimo 20% del valor)
- Seguro del vehículo
- Tasación del vehículo (si es usado)

### Consejos para obtener mejor tasa

- **Mejora tu score crediticio** antes de solicitar
- **Compara entre 3+ instituciones** antes de decidir
- **Negocia la tasa** — siempre hay margen
- **Aporta más inicial** — reduce la tasa y el monto de intereses
- **Elige el plazo más corto** que puedas pagar cómodamente

**Consejo Pro:** Usa la calculadora de OKLA para simular diferentes escenarios antes de visitar el banco.`,
  },
  {
    slug: 'compra-segura',
    title: 'Compra Segura: Evitar Fraudes',
    description:
      'Cómo identificar anuncios fraudulentos, qué señales de alerta buscar y cómo protegerte durante la transacción.',
    icon: 'Shield',
    readTime: '6 min',
    topics: ['Señales de fraude', 'Pago seguro', 'Verificar vendedor'],
    lastUpdated: '2026-02-05',
    content: `## Cómo comprar de forma segura y evitar fraudes

El mercado de vehículos usados puede tener riesgos. Aquí te enseñamos a protegerte.

### Señales de alerta en anuncios

**🚩 Precio demasiado bajo:**
Si el precio es significativamente menor al valor de mercado, desconfía. Usa el OKLA Score para comparar con precios reales.

**🚩 Fotos genéricas o de catálogo:**
Las fotos reales del vehículo deben mostrar el entorno real, no fotos de estudio o descargadas de internet.

**🚩 Vendedor que presiona:**
"Tengo otro comprador interesado" o "El precio solo es válido hoy" son tácticas de presión.

**🚩 Comunicación fuera de la plataforma:**
Si el vendedor insiste en comunicarse por WhatsApp o email directamente, puede ser para evitar la trazabilidad de OKLA.

### Verificación del vendedor

Antes de reunirte con el vendedor:

| Verificación | Cómo hacerla |
|-------------|-------------|
| Perfil en OKLA | Revisa antigüedad, calificaciones, ventas previas |
| Identidad | Pide cédula y compara con el título del vehículo |
| Vendedor verificado | Busca el sello de verificación KYC en OKLA |
| Referencias | Lee los comentarios de compradores anteriores |

### Proceso de pago seguro

**Nunca pagues antes de ver el vehículo y verificar los documentos.**

Pasos recomendados:

1. **Inspecciona el vehículo** en persona con mecánico
2. **Verifica toda la documentación** (título, matrícula, DGII)
3. **Acuerda el precio** y forma de pago
4. **Realiza el traspaso** en la DGII antes del pago final
5. **Paga con método trazable** (transferencia bancaria, cheque certificado)

### Qué hacer si detectas fraude

- **Reporta en OKLA** usando el botón "Reportar" en el anuncio
- **No envíes dinero** bajo ninguna circunstancia
- **Contacta a las autoridades** si ya fuiste víctima
- **Guarda todas las conversaciones** como evidencia

**Consejo Pro:** Los vendedores verificados con el sello KYC de OKLA han pasado por un proceso de verificación de identidad. Prioriza comprar a vendedores verificados.`,
  },
  {
    slug: 'traspaso-documentacion',
    title: 'Traspaso y Documentación',
    description:
      'Proceso completo para el traspaso del vehículo en República Dominicana: DGII, placa, seguro obligatorio.',
    icon: 'BookOpen',
    readTime: '9 min',
    topics: ['Proceso DGII', 'Costo de traspaso', 'Seguro obligatorio'],
    lastUpdated: '2026-02-01',
    content: `## Guía completa de traspaso vehicular en RD

El traspaso es el proceso legal para cambiar la titularidad de un vehículo. Es obligatorio y protege tanto al comprador como al vendedor.

### Documentos necesarios

**Del comprador:**
- Cédula de identidad vigente
- Copia de la cédula

**Del vendedor:**
- Cédula de identidad vigente
- Certificado de título original
- Matrícula vigente
- Certificación de no adeudo vehicular (DGII)

### Proceso paso a paso

**Paso 1: Verificación previa**
- Consulta en la DGII que el vehículo no tenga deudas
- Verifica que no existan gravámenes o embargos
- Confirma que el vendedor es el titular legítimo

**Paso 2: Ir a la DGII**
Ambas partes (comprador y vendedor) deben presentarse en la DGII con los documentos.

**Paso 3: Pago de impuestos**

| Concepto | Costo aproximado |
|----------|-----------------|
| Impuesto de traspaso | 2% del valor fiscal |
| Placas nuevas (si aplica) | RD$3,500-5,000 |
| Marbete | Varía según cilindrada |
| Seguro obligatorio | RD$2,000-4,000 anual |

**Paso 4: Emisión del nuevo título**
La DGII emitirá un nuevo certificado de título a nombre del comprador. El proceso toma entre 5-15 días hábiles.

**Paso 5: Seguro obligatorio**
Es requisito legal tener seguro de responsabilidad civil. Puedes contratarlo con cualquier aseguradora autorizada.

### Costos totales estimados

Para un vehículo con valor fiscal de RD$800,000:

- Impuesto de traspaso (2%): **RD$16,000**
- Placas: **RD$4,000**
- Seguro obligatorio: **RD$3,000**
- Marbete: **RD$2,500**
- **Total aproximado: RD$25,500**

### Errores comunes a evitar

- **No traspasar** — circular con título ajeno es ilegal
- **Traspasar sin verificar deudas** — el nuevo dueño hereda las deudas
- **No exigir el título original** — las copias no son válidas para traspaso
- **Firmar documentos en blanco** — nunca lo hagas

**Consejo Pro:** Realiza el traspaso ANTES de entregar el pago total. Esto te protege legalmente como comprador.`,
  },
];

export function getGuideBySlug(slug: string): Guide | undefined {
  return guides.find(g => g.slug === slug);
}

export function getAllGuideSlugs(): string[] {
  return guides.map(g => g.slug);
}

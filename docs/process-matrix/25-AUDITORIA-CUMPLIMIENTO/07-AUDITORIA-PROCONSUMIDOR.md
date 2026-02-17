# ⚖️ Auditoría Pro Consumidor - Protección al Consumidor

> **Propósito:** Verificar cumplimiento de Ley 358-05 de Protección al Consumidor  
> **Regulador:** Pro Consumidor  
> **Última actualización:** Enero 25, 2026

---

## 📋 INFORMACIÓN GENERAL

### Datos del Proveedor

| Campo                 | Valor                      | Estado        |
| --------------------- | -------------------------- | ------------- |
| **Razón Social**      | OKLA SRL                   | ✅            |
| **RNC**               | 1-32-XXXXX-X               | ⚠️ Verificar  |
| **Nombre Comercial**  | OKLA                       | ✅            |
| **Actividad**         | Marketplace de vehículos   | ✅            |
| **Dirección Física**  | Av. Winston Churchill #XXX | ⚠️ Verificar  |
| **Teléfono Atención** | 809-XXX-XXXX               | 🔴 Establecer |
| **Email Atención**    | soporte@okla.com.do        | ✅            |
| **Horario Atención**  | Lun-Vie 9am-6pm            | 🔴 Establecer |

### Tipo de Relación con Consumidor

| Rol de OKLA                    | Descripción                                | Responsabilidad   |
| ------------------------------ | ------------------------------------------ | ----------------- |
| **Como Marketplace**           | Facilitador entre vendedores y compradores | Intermediario     |
| **Como Proveedor de Servicio** | Suscripciones, boosts, publicaciones       | Proveedor directo |

---

## 📊 ESTADO DE CUMPLIMIENTO

### Obligaciones Generales

| Obligación                | Base Legal | Estado | Evidencia                |
| ------------------------- | ---------- | ------ | ------------------------ |
| Información clara y veraz | Art. 8     | ✅     | Listados                 |
| Precios visibles          | Art. 11    | ✅     | UI                       |
| Términos y condiciones    | Art. 6     | ✅     | /terms                   |
| Política de devoluciones  | Art. 44    | 🟡     | En términos              |
| Canal de quejas accesible | Art. 83    | 🔴     | Pendiente                |
| Libro de reclamaciones    | Art. 84    | 🔴     | Pendiente                |
| Respuesta a quejas        | Art. 85    | 🔴     | Pendiente                |
| Garantías informadas      | Art. 41    | N/A    | No vendemos directamente |

---

## 📝 INFORMACIÓN AL CONSUMIDOR

### Requisitos de Información

| Elemento                         | Requerido | Implementado | Ubicación          |
| -------------------------------- | --------- | ------------ | ------------------ |
| **Identidad del proveedor**      | ✅        | ✅           | Footer, /about     |
| **RNC**                          | ✅        | 🟡           | Footer (verificar) |
| **Dirección física**             | ✅        | 🟡           | Footer (verificar) |
| **Teléfono de contacto**         | ✅        | 🟡           | Footer             |
| **Email de contacto**            | ✅        | ✅           | Footer             |
| **Características del servicio** | ✅        | ✅           | Descripciones      |
| **Precio total con impuestos**   | ✅        | ✅           | Checkout           |
| **Forma de pago**                | ✅        | ✅           | Checkout           |
| **Tiempo de entrega**            | N/A       | N/A          | Servicio digital   |
| **Política de devoluciones**     | ✅        | 🟡           | Términos           |

### Información en Anuncios de Vehículos

| Elemento                    | Obligatorio | Estado               |
| --------------------------- | ----------- | -------------------- |
| Marca y modelo              | ✅          | ✅ Campos requeridos |
| Año                         | ✅          | ✅ Campo requerido   |
| Kilometraje                 | ✅          | ✅ Campo requerido   |
| Condición (nuevo/usado)     | ✅          | ✅ Campo requerido   |
| Precio                      | ✅          | ✅ Campo requerido   |
| Identificación del vendedor | ✅          | 🟡 Nombre visible    |
| Ubicación                   | ✅          | ✅ Ciudad/Provincia  |

---

## 🔄 DERECHO DE RETRACTO

### Aplicabilidad en OKLA

| Servicio                   | Aplica Retracto  | Plazo    | Estado                  |
| -------------------------- | ---------------- | -------- | ----------------------- |
| **Suscripción Dealer**     | ✅               | 7 días   | 🔴 No implementado      |
| **Boost de publicación**   | ❓               | Analizar | 🔴 No definido          |
| **Publicación individual** | ✅               | 7 días   | 🔴 No implementado      |
| **Compra de vehículo**     | Depende vendedor | Variable | 🟡 Informar en términos |

### Proceso de Retracto (A implementar)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  FLUJO DE DERECHO DE RETRACTO                                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  COMPRA REALIZADA                                                       │
│  └── Usuario paga suscripción/boost/publicación                        │
│                                                                         │
│  PLAZO DE RETRACTO: 7 DÍAS                                              │
│  ├── Usuario puede solicitar retracto sin justificación               │
│  ├── Vía: Email, formulario web, teléfono                              │
│  └── Sin penalidad                                                     │
│                                                                         │
│  PROCESAMIENTO (MAX 15 DÍAS)                                            │
│  ├── Verificar que está dentro del plazo                               │
│  ├── Verificar que el servicio no fue consumido                        │
│  ├── Cancelar suscripción/boost si activo                              │
│  ├── Procesar reembolso completo                                       │
│  └── Emitir Nota de Crédito (B04)                                      │
│                                                                         │
│  EXCEPCIONES (NO APLICA RETRACTO)                                       │
│  ├── Servicio ya consumido completamente                               │
│  ├── Contenido digital personalizado                                   │
│  └── Si el usuario renunció expresamente al retracto                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Información sobre Retracto

**Texto requerido en checkout:**

> "Usted tiene derecho a retractarse de esta compra dentro de los 7 días siguientes a la contratación, sin necesidad de justificación y sin penalidad. Para ejercer este derecho, contacte a soporte@okla.com.do o llame al 809-XXX-XXXX."

---

## 📢 SISTEMA DE QUEJAS Y RECLAMACIONES

### Estado Actual

| Componente             | Requerido | Estado | Acción           |
| ---------------------- | --------- | ------ | ---------------- |
| Canal de quejas        | ✅        | 🔴     | Crear formulario |
| Libro de reclamaciones | ✅        | 🔴     | Implementar      |
| Proceso de gestión     | ✅        | 🔴     | Documentar       |
| Respuesta en plazo     | ✅        | 🔴     | Automatizar      |
| Archivo de quejas      | ✅        | 🔴     | Base de datos    |

### Proceso de Quejas (A implementar)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  FLUJO DE GESTIÓN DE QUEJAS                                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  RECEPCIÓN (Día 0)                                                      │
│  ├── Usuario presenta queja por:                                       │
│  │   ├── Formulario web (/support/complaint)                           │
│  │   ├── Email (quejas@okla.com.do)                                    │
│  │   ├── Teléfono (809-XXX-XXXX)                                       │
│  │   └── Libro de reclamaciones (físico/digital)                       │
│  ├── Sistema asigna número único                                       │
│  └── Confirma recepción al usuario                                     │
│                                                                         │
│  CLASIFICACIÓN (Día 1)                                                  │
│  ├── Tipo de queja:                                                    │
│  │   ├── Servicio de OKLA (suscripción, facturación)                   │
│  │   ├── Vendedor de vehículo                                          │
│  │   ├── Calidad de listado                                            │
│  │   └── Otro                                                          │
│  └── Asignar a responsable                                             │
│                                                                         │
│  INVESTIGACIÓN (Días 2-10)                                              │
│  ├── Recopilar información                                             │
│  ├── Contactar partes involucradas                                     │
│  └── Determinar solución                                               │
│                                                                         │
│  RESPUESTA (Máximo 15 días)                                             │
│  ├── Comunicar resolución al usuario                                   │
│  ├── Implementar solución acordada                                     │
│  └── Archivar caso                                                     │
│                                                                         │
│  SEGUIMIENTO                                                            │
│  ├── Verificar satisfacción                                            │
│  └── Actualizar métricas                                               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Categorías de Quejas

| Categoría       | Descripción                     | Responsable     | SLA     |
| --------------- | ------------------------------- | --------------- | ------- |
| **Facturación** | Cobros incorrectos, doble cobro | BillingService  | 3 días  |
| **Servicio**    | Funcionalidad no disponible     | Soporte técnico | 5 días  |
| **Vendedor**    | Problemas con un vendedor       | Mediación       | 10 días |
| **Contenido**   | Anuncio engañoso                | Moderación      | 2 días  |
| **Reembolso**   | Solicitud de devolución         | BillingService  | 5 días  |
| **Cuenta**      | Acceso, verificación            | UserService     | 3 días  |

---

## 📚 LIBRO DE RECLAMACIONES

### Requisitos

| Requisito                  | Estado | Acción                 |
| -------------------------- | ------ | ---------------------- |
| Formato físico disponible  | 🔴     | Adquirir libro oficial |
| Formato digital disponible | 🔴     | Implementar en web     |
| Visible y accesible        | 🔴     | Link en footer         |
| Foliado consecutivamente   | 🔴     | Sistema de tickets     |
| Copia al consumidor        | 🔴     | Email automático       |
| Conservar 2 años           | 🔴     | Base de datos          |

### Campos del Libro de Reclamaciones

```yaml
reclamacion:
  numero: "REC-2026-001"
  fecha: "2026-01-25"
  consumidor:
    nombre: "Juan Pérez"
    cedula: "001-0000000-0"
    direccion: "..."
    telefono: "809-000-0000"
    email: "juan@email.com"
  proveedor:
    nombre: "OKLA SRL"
    rnc: "1-32-XXXXX-X"
    direccion: "..."
  tipo: "queja" | "reclamacion"
  detalle: "Descripción detallada del problema..."
  pedido: "Lo que solicita el consumidor..."
  respuesta:
    fecha: "2026-01-30"
    detalle: "Respuesta de OKLA..."
  firma_consumidor: "..."
  firma_proveedor: "..."
```

---

## 💰 GARANTÍAS Y DEVOLUCIONES

### Servicios de OKLA

| Servicio                   | Garantía                  | Política Devolución        | Estado         |
| -------------------------- | ------------------------- | -------------------------- | -------------- |
| **Suscripción Dealer**     | Disponibilidad 99.9%      | Prorrateo si falla         | 🟡 En términos |
| **Boost**                  | Visualización garantizada | Reembolso si no se muestra | 🔴 No definido |
| **Publicación Individual** | 30 días activa            | Reembolso primeros 7 días  | 🔴 No definido |

### Vehículos (Responsabilidad del Vendedor)

OKLA como marketplace NO es responsable de:

- Garantías del vehículo
- Condiciones mecánicas
- Historial del vehículo
- Vicios ocultos

**Sin embargo, OKLA debe:**

- Informar claramente esta limitación
- Facilitar contacto entre partes
- Actuar en casos de fraude evidente
- Moderar anuncios engañosos

---

## ⚠️ PRÁCTICAS COMERCIALES PROHIBIDAS

### Verificación de Cumplimiento

| Práctica Prohibida  | Art. | Estado OKLA | Evidencia                 |
| ------------------- | ---- | ----------- | ------------------------- |
| Publicidad engañosa | 83   | ✅ Cumple   | Política de moderación    |
| Información falsa   | 8    | ✅ Cumple   | Verificación de datos     |
| Precios ocultos     | 11   | ✅ Cumple   | Precios visibles          |
| Cargos sorpresa     | 12   | ✅ Cumple   | Checkout claro            |
| Ventas forzadas     | 24   | ✅ Cumple   | Sin prácticas coercitivas |
| Discriminación      | 3    | ✅ Cumple   | Acceso universal          |
| Cláusulas abusivas  | 92   | 🟡 Revisar  | Revisar términos          |

### Cláusulas Abusivas a Evitar

```
CLÁUSULAS A REVISAR EN TÉRMINOS Y CONDICIONES:

❌ NO INCLUIR:
- Exoneración total de responsabilidad
- Renuncia a derechos irrenunciables
- Modificación unilateral sin aviso
- Prórroga automática sin consentimiento
- Penalidades desproporcionadas
- Inversión de carga de la prueba

✅ SÍ INCLUIR:
- Limitación razonable de responsabilidad
- Procedimiento claro de terminación
- Aviso previo de cambios (30 días)
- Mecanismos de solución de controversias
- Derechos del consumidor expresos
```

---

## 📊 MÉTRICAS DE ATENCIÓN AL CONSUMIDOR

### KPIs Requeridos

| Métrica                       | Meta      | Estado Actual | Acción                |
| ----------------------------- | --------- | ------------- | --------------------- |
| Tiempo respuesta inicial      | < 24h     | N/A           | Implementar tracking  |
| Tiempo resolución promedio    | < 15 días | N/A           | Implementar tracking  |
| Tasa de resolución            | > 90%     | N/A           | Implementar tracking  |
| Satisfacción del cliente      | > 4/5     | N/A           | Implementar encuestas |
| Quejas por 1000 transacciones | < 5       | N/A           | Medir                 |

### Dashboard de Quejas (A implementar)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  DASHBOARD DE QUEJAS - Enero 2026                                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  RESUMEN                                                                │
│  ├── Total quejas recibidas: XX                                        │
│  ├── Resueltas: XX (XX%)                                               │
│  ├── Pendientes: XX                                                    │
│  └── Tiempo promedio resolución: XX días                               │
│                                                                         │
│  POR CATEGORÍA                                                          │
│  ├── Facturación: XX (XX%)                                             │
│  ├── Servicio: XX (XX%)                                                │
│  ├── Vendedor: XX (XX%)                                                │
│  └── Otro: XX (XX%)                                                    │
│                                                                         │
│  TENDENCIA                                                              │
│  ├── vs. mes anterior: +XX% / -XX%                                     │
│  └── Quejas recurrentes: [Lista de problemas frecuentes]              │
│                                                                         │
│  ALERTAS                                                                │
│  ├── 🔴 X quejas próximas a vencer SLA                                 │
│  └── 🟡 X quejas sin asignar                                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🔍 CHECKLIST DE AUDITORÍA PRO CONSUMIDOR

### Información y Transparencia

```
□ Identidad del proveedor visible en sitio web
□ RNC visible y verificable
□ Dirección física publicada
□ Teléfono de atención publicado
□ Email de contacto publicado
□ Horario de atención publicado
□ Términos y condiciones accesibles
□ Política de privacidad accesible
□ Política de devoluciones clara
□ Precios incluyen ITBIS
□ Sin cargos ocultos en checkout
```

### Derechos del Consumidor

```
□ Derecho de retracto informado
□ Proceso de retracto documentado
□ Plazo de retracto respetado (7 días)
□ Reembolsos en tiempo (15 días)
□ Garantías informadas
□ Canales de queja accesibles
□ Libro de reclamaciones disponible
□ Respuestas en plazo (15 días)
```

### Prácticas Comerciales

```
□ Publicidad veraz
□ Precios claros y visibles
□ Sin ventas forzadas
□ Sin discriminación
□ Contratos sin cláusulas abusivas
□ Confirmación de pedido
□ Facturas/comprobantes emitidos
```

### Registros

```
□ Registro de quejas
□ Registro de devoluciones
□ Registro de reembolsos
□ Estadísticas de atención
□ Conservación 2 años mínimo
```

---

## 📞 CONTACTO PRO CONSUMIDOR

| Concepto        | Contacto                       |
| --------------- | ------------------------------ |
| Pro Consumidor  | 809-200-1110                   |
| Web             | proconsumidor.gob.do           |
| Email           | info@proconsumidor.gob.do      |
| Denuncia online | proconsumidor.gob.do/denuncias |

---

## 🔗 INTEGRACIÓN CON MICROSERVICIOS

### SupportService (Puerto 5063)

| Endpoint                                     | Descripción             |
| -------------------------------------------- | ----------------------- |
| `POST /api/support/complaints`               | Crear queja/reclamación |
| `GET /api/support/complaints`                | Listar quejas           |
| `GET /api/support/complaints/{id}`           | Detalle de queja        |
| `PUT /api/support/complaints/{id}`           | Actualizar estado       |
| `POST /api/support/complaints/{id}/response` | Responder queja         |
| `GET /api/support/complaints/stats`          | Estadísticas            |
| `GET /api/support/book`                      | Libro de reclamaciones  |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Trimestral  
**Responsable:** Gerente de Atención al Cliente

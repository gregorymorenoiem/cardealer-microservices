# 🔙 Derecho de Retracto - Devoluciones - Matriz de Procesos

> **Marco Legal:** Ley 358-05 - Protección al Consumidor  
> **Regulador:** Pro Consumidor  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** 🔴 10% Backend | 🔴 0% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                       | Backend      | UI Access | Observación    |
| ----------------------------- | ------------ | --------- | -------------- |
| RETRACT-POLICY-001 Política   | 🟡 Parcial   | 🟡 /terms | En términos    |
| RETRACT-REQUEST-001 Solicitud | 🔴 Pendiente | 🔴 Falta  | Sin formulario |
| RETRACT-REFUND-001 Reembolso  | 🔴 Pendiente | 🔴 Falta  | Sin proceso    |
| RETRACT-VEHICLE-001 Vehículos | 🔴 Pendiente | 🔴 Falta  | Caso especial  |

### Rutas UI Existentes ✅

- `/terms` → Política de devoluciones mencionada

### Rutas UI Faltantes 🔴

- `/refund-policy` → Política de devoluciones detallada
- `/refund/request` → Solicitar devolución
- `/refund/my` → Mis solicitudes de devolución
- `/refund/:id` → Estado de devolución

---

## 📊 Resumen de Implementación

| Componente                         | Total | Implementado | Pendiente | Estado         |
| ---------------------------------- | ----- | ------------ | --------- | -------------- |
| **RETRACT-POLICY-\*** (Política)   | 3     | 1            | 2         | 🟡 Parcial     |
| **RETRACT-REQUEST-\*** (Solicitud) | 4     | 0            | 4         | 🔴 Pendiente   |
| **RETRACT-REFUND-\*** (Reembolso)  | 4     | 0            | 4         | 🔴 Pendiente   |
| **RETRACT-VEHICLE-\*** (Vehículos) | 4     | 0            | 4         | 🔴 Pendiente   |
| **Tests**                          | 12    | 1            | 11        | 🔴 Pendiente   |
| **TOTAL**                          | 27    | 2            | 25        | 🔴 10% Backend |

---

## 1. Información General

### 1.1 Marco Legal

La Ley 358-05 establece el derecho de retracto para compras a distancia (comercio electrónico):

| Aspecto         | Regulación                         |
| --------------- | ---------------------------------- |
| **Plazo**       | 48 horas desde la entrega          |
| **Aplica a**    | Servicios contratados a distancia  |
| **Excepciones** | Bienes personalizados, perecederos |
| **Reembolso**   | Total, incluyendo gastos de envío  |

### 1.2 Aplicación en OKLA

| Producto/Servicio      | Aplica Retracto | Plazo       | Condiciones        |
| ---------------------- | --------------- | ----------- | ------------------ |
| **Suscripción Dealer** | ✅ Sí           | 48 horas    | Antes de publicar  |
| **Boost/Destacado**    | ✅ Sí           | 48 horas    | Antes de inicio    |
| **Comisión de venta**  | ❌ No           | -           | Servicio ejecutado |
| **Vehículo (P2P)**     | ⚠️ Especial     | 24-48 horas | Según acuerdo      |
| **Vehículo (Dealer)**  | ⚠️ Especial     | 3-7 días    | Según dealer       |

---

## 2. Servicios de OKLA con Retracto

### 2.1 Suscripciones de Dealer

| Plan       | Precio   | Retracto | Condición                |
| ---------- | -------- | -------- | ------------------------ |
| Starter    | $49/mes  | ✅ 48h   | Sin vehículos publicados |
| Pro        | $129/mes | ✅ 48h   | Sin vehículos publicados |
| Enterprise | $299/mes | ✅ 48h   | Sin vehículos publicados |

**Proceso de Retracto:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    RETRACTO DE SUSCRIPCIÓN                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1️⃣ Dealer contrata suscripción                                        │
│   └── Pago procesado, cuenta activada                                  │
│                                                                         │
│   2️⃣ Dentro de 48 horas                                                 │
│   ├── Dealer solicita cancelación/retracto                             │
│   └── Sistema verifica:                                                │
│       ├── ✅ < 48 horas desde contratación                             │
│       └── ✅ No ha publicado vehículos                                  │
│                                                                         │
│   3️⃣ Si cumple condiciones                                              │
│   ├── Cancelar suscripción inmediatamente                              │
│   ├── Procesar reembolso completo (100%)                               │
│   └── Enviar confirmación por email                                    │
│                                                                         │
│   4️⃣ Si NO cumple (>48h o publicó)                                      │
│   ├── Informar que no aplica retracto                                  │
│   └── Ofrecer cancelación al final del período                         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Boost/Destacados

| Servicio          | Precio | Retracto | Condición           |
| ----------------- | ------ | -------- | ------------------- |
| Boost 7 días      | $15    | ✅ 48h   | Antes de activación |
| Destacado 30 días | $29    | ✅ 48h   | Antes de activación |
| Premium 30 días   | $49    | ✅ 48h   | Antes de activación |

**Nota:** Si el boost ya se activó (vehículo ya está destacado), no aplica retracto pero se puede ofrecer crédito proporcional.

---

## 3. Compra de Vehículos

### 3.1 Venta Entre Particulares (P2P)

OKLA facilita la transacción pero el vehículo es vendido por un particular:

| Escenario                 | Responsabilidad | Retracto             |
| ------------------------- | --------------- | -------------------- |
| Vehículo como se describe | Vendedor        | ❌ No obligatorio    |
| Vehículo diferente        | Vendedor        | ⚠️ Negociable        |
| Vicios ocultos            | Vendedor        | ✅ 30 días (Art. 39) |
| Fraude                    | OKLA + Vendedor | ✅ Completo          |

### 3.2 Venta por Dealers

Los dealers profesionales tienen mayor responsabilidad:

| Garantía              | Plazo          | Cobertura              |
| --------------------- | -------------- | ---------------------- |
| **Retracto**          | 48 horas       | Devolución completa    |
| **Garantía mecánica** | 30 días mínimo | Fallas mecánicas       |
| **Vicios ocultos**    | 6 meses        | Defectos no declarados |

### 3.3 Programa OKLA Certified

Para vehículos certificados por OKLA:

| Garantía               | Plazo           | Cobertura           |
| ---------------------- | --------------- | ------------------- |
| **Período de prueba**  | 7 días / 500 km | Devolución completa |
| **Garantía mecánica**  | 90 días         | Motor, transmisión  |
| **Inspección fallida** | 30 días         | Reembolso completo  |

---

## 4. Procesos de Implementación

### 4.1 RETRACT-REQUEST: Solicitud de Retracto

#### RETRACT-REQUEST-001: Formulario de Solicitud

| Campo       | Valor               |
| ----------- | ------------------- |
| **Proceso** | RETRACT-REQUEST-001 |
| **Ruta**    | `/refund/request`   |
| **Estado**  | 🔴 Pendiente        |

**UI Propuesta:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  🔙 SOLICITAR DEVOLUCIÓN / RETRACTO                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Selecciona la compra a devolver:                                       │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ (•) Suscripción Pro - $129/mes                                   │  │
│  │     Contratada: 24/01/2026 (hace 8 horas)                        │  │
│  │     ✅ Aplica retracto                                           │  │
│  │                                                                   │  │
│  │ ( ) Boost Premium - $49                                          │  │
│  │     Comprado: 20/01/2026 (hace 5 días)                          │  │
│  │     ❌ Fuera de plazo (48 horas)                                 │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  Motivo de la devolución:                                               │
│  [▼ Seleccionar motivo                                            ]    │
│                                                                         │
│  Comentarios adicionales:                                               │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                                                                   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  📋 RESUMEN DE REEMBOLSO                                                │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Monto original:           $129.00                                │  │
│  │ ITBIS pagado:              $23.22                                │  │
│  │ ─────────────────────────────────                                │  │
│  │ Total a reembolsar:       $152.22                                │  │
│  │                                                                   │  │
│  │ Método de reembolso: Tarjeta ****4532                            │  │
│  │ Tiempo estimado: 5-10 días hábiles                               │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [Cancelar]                          [Solicitar Devolución]            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### RETRACT-REQUEST-002: Validación Automática

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **Proceso** | RETRACT-REQUEST-002        |
| **Nombre**  | Validación de Elegibilidad |
| **Estado**  | 🔴 Pendiente               |

**Validaciones:**

| Check            | Descripción              | Automático |
| ---------------- | ------------------------ | ---------- |
| Plazo            | Menos de 48 horas        | ✅         |
| Uso del servicio | No usado/activado        | ✅         |
| Tipo de compra   | Elegible para retracto   | ✅         |
| Estado de pago   | Pago confirmado          | ✅         |
| Fraude           | Sin indicadores de abuso | ✅         |

---

### 4.2 RETRACT-REFUND: Proceso de Reembolso

#### RETRACT-REFUND-001: Procesamiento

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **Proceso** | RETRACT-REFUND-001         |
| **Nombre**  | Procesamiento de Reembolso |
| **Estado**  | 🔴 Pendiente               |

**Flujo:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    PROCESAMIENTO DE REEMBOLSO                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1️⃣ Solicitud aprobada                                                 │
│   └── Sistema valida elegibilidad automáticamente                      │
│                                                                         │
│   2️⃣ Cancelar servicio                                                  │
│   ├── Desactivar suscripción/boost                                     │
│   ├── Remover acceso a features                                        │
│   └── Log de cancelación                                               │
│                                                                         │
│   3️⃣ Iniciar reembolso                                                  │
│   ├── Stripe: refund API                                               │
│   ├── AZUL: proceso manual o API                                       │
│   └── Monto: 100% del cargo                                            │
│                                                                         │
│   4️⃣ Notificaciones                                                     │
│   ├── Email: Confirmación de reembolso                                 │
│   ├── Estado: "Reembolso en proceso"                                   │
│   └── Timeline: 5-10 días hábiles                                      │
│                                                                         │
│   5️⃣ Confirmación bancaria                                              │
│   ├── Webhook de Stripe/AZUL                                           │
│   ├── Email: Reembolso completado                                      │
│   └── Estado: "Reembolso completado"                                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### RETRACT-REFUND-002: Nota de Crédito

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **Proceso** | RETRACT-REFUND-002         |
| **Nombre**  | Emisión de Nota de Crédito |
| **NCF**     | B04                        |
| **Estado**  | 🔴 Pendiente               |

Para cumplimiento DGII, cada reembolso debe generar una Nota de Crédito (NCF B04) que anule la factura original.

---

### 4.3 RETRACT-VEHICLE: Devolución de Vehículos

#### RETRACT-VEHICLE-001: Política de Dealers

| Campo       | Valor                             |
| ----------- | --------------------------------- |
| **Proceso** | RETRACT-VEHICLE-001               |
| **Nombre**  | Política de Devolución de Dealers |
| **Estado**  | 🔴 Pendiente                      |

**Configuración por Dealer:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ⚙️ POLÍTICA DE DEVOLUCIÓN (Config Dealer)                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Período de devolución                                                  │
│  [▼ 7 días        ] (Mínimo legal: 48 horas)                           │
│                                                                         │
│  Límite de kilómetros                                                   │
│  [    500    ] km adicionales permitidos                               │
│                                                                         │
│  Condiciones para aceptar devolución:                                   │
│  [✓] Vehículo en mismas condiciones                                    │
│  [✓] Sin daños adicionales                                             │
│  [✓] Documentación completa                                            │
│  [ ] Inspección satisfactoria                                          │
│                                                                         │
│  Cargos por devolución:                                                 │
│  ( ) Sin cargos (100% reembolso)                                       │
│  (•) Cargo por uso: RD$ [  5,000  ] o [ 2 ]% del precio                │
│  ( ) Cargo fijo: RD$ [         ]                                       │
│                                                                         │
│  [Guardar Política]                                                     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### RETRACT-VEHICLE-002: Proceso de Devolución

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **Proceso** | RETRACT-VEHICLE-002             |
| **Nombre**  | Flujo de Devolución de Vehículo |
| **Estado**  | 🔴 Pendiente                    |

**Pasos:**

1. Comprador solicita devolución (dentro del plazo)
2. Dealer revisa solicitud
3. Agendar inspección del vehículo
4. Inspección (mismo estado que entrega)
5. Aprobación/Rechazo de devolución
6. Proceso de reembolso (menos cargos si aplican)
7. Transferencia de propiedad reversa

---

## 5. Endpoints API

### 5.1 RefundController

| Método | Endpoint                 | Descripción         | Auth | Estado |
| ------ | ------------------------ | ------------------- | ---- | ------ |
| `GET`  | `/api/refund/eligible`   | Compras elegibles   | ✅   | 🔴     |
| `POST` | `/api/refund/request`    | Solicitar reembolso | ✅   | 🔴     |
| `GET`  | `/api/refund/my`         | Mis solicitudes     | ✅   | 🔴     |
| `GET`  | `/api/refund/:id`        | Estado de solicitud | ✅   | 🔴     |
| `POST` | `/api/refund/:id/cancel` | Cancelar solicitud  | ✅   | 🔴     |

### 5.2 Admin RefundController

| Método | Endpoint                         | Descripción        | Auth  | Estado |
| ------ | -------------------------------- | ------------------ | ----- | ------ |
| `GET`  | `/api/admin/refunds`             | Listar todas       | Admin | 🔴     |
| `PUT`  | `/api/admin/refunds/:id/approve` | Aprobar manual     | Admin | 🔴     |
| `PUT`  | `/api/admin/refunds/:id/reject`  | Rechazar           | Admin | 🔴     |
| `POST` | `/api/admin/refunds/:id/process` | Procesar reembolso | Admin | 🔴     |

### 5.3 Dealer RefundController

| Método | Endpoint                          | Descripción           | Auth   | Estado |
| ------ | --------------------------------- | --------------------- | ------ | ------ |
| `GET`  | `/api/dealer/refund-policy`       | Ver mi política       | Dealer | 🔴     |
| `PUT`  | `/api/dealer/refund-policy`       | Actualizar política   | Dealer | 🔴     |
| `GET`  | `/api/dealer/refund-requests`     | Solicitudes recibidas | Dealer | 🔴     |
| `PUT`  | `/api/dealer/refund-requests/:id` | Responder solicitud   | Dealer | 🔴     |

---

## 6. Página de Política Pública

### 6.1 Ruta: `/refund-policy`

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📋 POLÍTICA DE DEVOLUCIONES Y RETRACTO                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  En OKLA respetamos tu derecho de retracto según la Ley 358-05.        │
│                                                                         │
│  SERVICIOS DE OKLA                                                      │
│  ─────────────────                                                      │
│                                                                         │
│  ✅ Suscripciones de Dealer                                             │
│  • Plazo: 48 horas desde la contratación                               │
│  • Condición: No haber publicado vehículos                             │
│  • Reembolso: 100% del monto pagado                                    │
│                                                                         │
│  ✅ Boost y Destacados                                                  │
│  • Plazo: 48 horas desde la compra                                     │
│  • Condición: Antes de que se active el servicio                       │
│  • Reembolso: 100% del monto pagado                                    │
│                                                                         │
│  ❌ Comisiones por venta exitosa                                        │
│  • No aplica retracto (servicio ya ejecutado)                          │
│                                                                         │
│  COMPRA DE VEHÍCULOS                                                    │
│  ───────────────────                                                    │
│                                                                         │
│  La política de devolución de vehículos depende del vendedor:          │
│                                                                         │
│  🏪 Dealers Profesionales                                               │
│  • Mínimo 48 horas de retracto                                         │
│  • Muchos ofrecen 7 días o más                                         │
│  • Ver política específica en cada anuncio                             │
│                                                                         │
│  👤 Vendedores Particulares                                             │
│  • Negociable entre las partes                                         │
│  • Garantía de vicios ocultos: 30 días                                 │
│                                                                         │
│  🏆 OKLA Certified                                                      │
│  • 7 días o 500 km de prueba                                           │
│  • Garantía mecánica de 90 días                                        │
│  • Devolución sin preguntas                                            │
│                                                                         │
│  ¿Cómo solicitar una devolución?                                        │
│  1. Ve a "Mis Compras" en tu perfil                                    │
│  2. Selecciona la compra a devolver                                    │
│  3. Haz clic en "Solicitar Devolución"                                 │
│  4. Completa el formulario                                             │
│  5. Espera la confirmación                                             │
│                                                                         │
│  [Ir a Mis Compras]                                                     │
│                                                                         │
│  ¿Preguntas? Contacta a soporte@okla.com.do                            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 7. Notificaciones

| Evento               | Destinatario | Template               |
| -------------------- | ------------ | ---------------------- |
| Solicitud creada     | Usuario      | refund-requested       |
| Reembolso aprobado   | Usuario      | refund-approved        |
| Reembolso rechazado  | Usuario      | refund-rejected        |
| Reembolso procesado  | Usuario      | refund-processed       |
| Reembolso completado | Usuario      | refund-completed       |
| Devolución vehículo  | Dealer       | vehicle-return-request |

---

## 8. Cronograma de Implementación

### Fase 1: Q1 2026 - Servicios OKLA 🔴

- [ ] Validación de elegibilidad
- [ ] Formulario de solicitud
- [ ] Integración con Stripe refunds
- [ ] Emisión de Nota de Crédito

### Fase 2: Q2 2026 - Vehículos 🔴

- [ ] Configuración de política por dealer
- [ ] Flujo de devolución de vehículos
- [ ] Inspección y aprobación
- [ ] Proceso de reembolso

### Fase 3: Q2 2026 - OKLA Certified 🔴

- [ ] Período de prueba de 7 días
- [ ] Proceso automático de devolución
- [ ] Garantía mecánica integrada

---

## 9. Referencias

| Documento                    | Ubicación              |
| ---------------------------- | ---------------------- |
| Ley 358-05                   | congreso.gob.do        |
| Términos OKLA                | /terms                 |
| 04-proconsumidor.md          | 08-COMPLIANCE-LEGAL-RD |
| 03-devolucion-cancelacion.md | 15-CONFIANZA-SEGURIDAD |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Febrero 25, 2026  
**Responsable:** Equipo de Desarrollo + Legal OKLA  
**Prioridad:** 🟡 MEDIA (Obligación legal pero bajo volumen inicial)

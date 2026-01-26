# 🌐 Ley 126-02 - Comercio Electrónico - Matriz de Procesos

> **Marco Legal:** Ley 126-02 sobre Comercio Electrónico, Documentos y Firmas Digitales  
> **Regulador:** INDOTEL  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟢 ACTIVO (Obligatorio)  
> **Estado de Implementación:** 🟡 70% Backend | ✅ 80% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                      | Backend         | UI Access     | Observación             |
| ---------------------------- | --------------- | ------------- | ----------------------- |
| ECOM-INFO-001 Identificación | ✅ Completo     | ✅ FooterPage | Datos en footer         |
| ECOM-TOS-001 Términos        | ✅ Completo     | ✅ /terms     | Términos publicados     |
| ECOM-PRIV-001 Privacidad     | ✅ Completo     | ✅ /privacy   | Política publicada      |
| ECOM-CONF-001 Confirmación   | ✅ EmailService | ✅ Email      | Confirmación automática |
| ECOM-SIGN-001 Firma Digital  | 🔴 Pendiente    | 🔴 Falta      | No implementado         |
| ECOM-CERT-001 Certificados   | 🔴 Pendiente    | 🔴 Falta      | Sin integración         |

### Rutas UI Existentes ✅

- `/terms` → Términos y condiciones
- `/privacy` → Política de privacidad
- Footer → Datos de la empresa

### Rutas UI Faltantes 🔴

- `/contracts/sign` → Firma digital de contratos
- `/documents/verify` → Verificar autenticidad de documentos

**Verificación Backend:** Cumplimiento básico de información ✅, Firma digital pendiente 🔴

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado         |
| -------------------------------- | ----- | ------------ | --------- | -------------- |
| **ECOM-INFO-\*** (Información)   | 4     | 4            | 0         | ✅ Completo    |
| **ECOM-TOS-\*** (Términos)       | 3     | 3            | 0         | ✅ Completo    |
| **ECOM-CONF-\*** (Confirmación)  | 3     | 3            | 0         | ✅ Completo    |
| **ECOM-SIGN-\*** (Firma Digital) | 4     | 0            | 4         | 🔴 Pendiente   |
| **ECOM-CERT-\*** (Certificados)  | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                        | 15    | 10           | 5         | 🟡 Parcial     |
| **TOTAL**                        | 32    | 20           | 12        | 🟡 70% Backend |

---

## 1. Información General

### 1.1 Descripción

La Ley 126-02 establece el marco jurídico para el comercio electrónico en República Dominicana, otorgando validez legal a los documentos y firmas digitales, y estableciendo obligaciones para proveedores de servicios electrónicos.

### 1.2 Ámbito de Aplicación

| Aspecto                  | Aplica a OKLA    |
| ------------------------ | ---------------- |
| Comercio electrónico B2C | ✅ Sí            |
| Comercio electrónico B2B | ✅ Sí (Dealers)  |
| Documentos electrónicos  | ✅ Sí            |
| Firma digital            | 🟡 Parcial       |
| Facturación electrónica  | ✅ Sí (con DGII) |

### 1.3 Regulador

| Entidad                | Rol                                                     |
| ---------------------- | ------------------------------------------------------- |
| **INDOTEL**            | Regulación de telecomunicaciones y comercio electrónico |
| **DGII**               | Facturación electrónica (e-CF)                          |
| **Cámara de Comercio** | Certificación de firmas digitales                       |

---

## 2. Obligaciones del Proveedor (OKLA)

### 2.1 Información Obligatoria

OKLA debe publicar claramente:

| Información                  | Ubicación       | Estado |
| ---------------------------- | --------------- | ------ |
| Nombre legal completo        | Footer, About   | ✅     |
| RNC de la empresa            | Footer          | ✅     |
| Domicilio físico             | Footer, About   | ✅     |
| Correo electrónico           | Footer, Contact | ✅     |
| Teléfono de contacto         | Footer, Contact | ✅     |
| Número de registro mercantil | About           | 🟡     |

### 2.2 Proceso de Contratación Electrónica

```
┌─────────────────────────────────────────────────────────────────────────┐
│              PROCESO DE CONTRATACIÓN ELECTRÓNICA                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1️⃣ INFORMACIÓN PREVIA                                                 │
│   ├── Descripción completa del producto/servicio                        │
│   ├── Precio total (incluidos impuestos)                                │
│   ├── Gastos de entrega (si aplica)                                     │
│   └── Condiciones de pago                                               │
│                                                                         │
│   2️⃣ CONSENTIMIENTO                                                     │
│   ├── Aceptación expresa de términos                                    │
│   ├── Checkbox de condiciones                                           │
│   └── Registro de la aceptación                                         │
│                                                                         │
│   3️⃣ CONFIRMACIÓN                                                       │
│   ├── Email de confirmación inmediato                                   │
│   ├── Resumen de la transacción                                         │
│   ├── Número de orden/referencia                                        │
│   └── Datos de contacto para consultas                                  │
│                                                                         │
│   4️⃣ EJECUCIÓN                                                          │
│   ├── Cumplimiento del contrato                                         │
│   ├── Actualizaciones de estado                                         │
│   └── Comprobante final                                                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Procesos de Implementación

### 3.1 ECOM-INFO: Información del Proveedor

#### ECOM-INFO-001: Publicación de Datos Empresa

| Campo           | Valor                                               |
| --------------- | --------------------------------------------------- |
| **Proceso**     | ECOM-INFO-001                                       |
| **Nombre**      | Publicación de Datos de Empresa                     |
| **Descripción** | Mostrar datos legales de OKLA en toda la plataforma |
| **Estado**      | ✅ Implementado                                     |

**Implementación:**

```typescript
// Footer.tsx
const companyInfo = {
  name: "OKLA SRL",
  rnc: "1-31-XXXXX-X",
  address: "Av. Winston Churchill #123, Santo Domingo, RD",
  email: "info@okla.com.do",
  phone: "+1 (809) 555-0100",
  registry: "Cámara de Comercio SD #12345",
};
```

#### ECOM-INFO-002: Página About

| Campo       | Valor           |
| ----------- | --------------- |
| **Proceso** | ECOM-INFO-002   |
| **Ruta**    | `/about`        |
| **Estado**  | ✅ Implementado |

---

### 3.2 ECOM-TOS: Términos y Condiciones

#### ECOM-TOS-001: Términos de Uso

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **Proceso** | ECOM-TOS-001                  |
| **Nombre**  | Términos y Condiciones de Uso |
| **Ruta**    | `/terms`                      |
| **Estado**  | ✅ Implementado               |

**Contenido Requerido por Ley:**

| Sección                      | Descripción                     | Estado |
| ---------------------------- | ------------------------------- | ------ |
| Identificación del proveedor | Datos legales de OKLA           | ✅     |
| Descripción del servicio     | Qué ofrece la plataforma        | ✅     |
| Proceso de contratación      | Cómo funciona la compra         | ✅     |
| Precios y pagos              | Comisiones, métodos de pago     | ✅     |
| Derecho de desistimiento     | Cancelación de servicios        | ✅     |
| Garantías                    | Responsabilidades               | ✅     |
| Resolución de disputas       | Proceso de mediación            | ✅     |
| Jurisdicción                 | Tribunales de RD                | ✅     |
| Modificaciones               | Cómo se actualizan los términos | ✅     |

#### ECOM-TOS-002: Aceptación de Términos

| Campo         | Valor              |
| ------------- | ------------------ |
| **Proceso**   | ECOM-TOS-002       |
| **Ubicación** | Registro, Checkout |
| **Estado**    | ✅ Implementado    |

```typescript
// RegisterForm.tsx
<Checkbox
  id="terms"
  required
  checked={acceptedTerms}
  onChange={setAcceptedTerms}
/>
<label htmlFor="terms">
  Acepto los <Link to="/terms">Términos y Condiciones</Link> y la{" "}
  <Link to="/privacy">Política de Privacidad</Link>
</label>
```

---

### 3.3 ECOM-CONF: Confirmación de Transacciones

#### ECOM-CONF-001: Email de Confirmación

| Campo        | Valor                  |
| ------------ | ---------------------- |
| **Proceso**  | ECOM-CONF-001          |
| **Nombre**   | Confirmación por Email |
| **Servicio** | NotificationService    |
| **Estado**   | ✅ Implementado        |

**Contenido del Email:**

| Campo                             | Obligatorio | Incluido |
| --------------------------------- | ----------- | -------- |
| Número de orden                   | ✅          | ✅       |
| Fecha y hora                      | ✅          | ✅       |
| Descripción del servicio/producto | ✅          | ✅       |
| Precio total                      | ✅          | ✅       |
| Datos del vendedor                | ✅          | ✅       |
| Datos del comprador               | ✅          | ✅       |
| Condiciones de la transacción     | ✅          | ✅       |
| Información de contacto           | ✅          | ✅       |

#### ECOM-CONF-002: Recibo Digital

| Campo       | Valor           |
| ----------- | --------------- |
| **Proceso** | ECOM-CONF-002   |
| **Formato** | PDF, Email      |
| **Estado**  | ✅ Implementado |

---

### 3.4 ECOM-SIGN: Firma Digital (PENDIENTE)

#### ECOM-SIGN-001: Integración Firma Digital

| Campo         | Valor                        |
| ------------- | ---------------------------- |
| **Proceso**   | ECOM-SIGN-001                |
| **Nombre**    | Integración de Firma Digital |
| **Proveedor** | Cámara de Comercio / INDOTEL |
| **Estado**    | 🔴 Pendiente                 |

**Requisitos:**

| Componente                | Descripción                      | Estado |
| ------------------------- | -------------------------------- | ------ |
| Proveedor de certificados | Entidad certificadora autorizada | 🔴     |
| SDK de firma              | Integración con API de firma     | 🔴     |
| Almacenamiento seguro     | Certificados en HSM              | 🔴     |
| Verificación              | Validar firmas                   | 🔴     |

#### ECOM-SIGN-002: Firma de Contratos

| Campo       | Valor                                 |
| ----------- | ------------------------------------- |
| **Proceso** | ECOM-SIGN-002                         |
| **Uso**     | Contratos de compraventa de vehículos |
| **Estado**  | 🔴 Pendiente                          |

**Flujo Propuesto:**

```
1. Usuario crea contrato de compraventa
2. Sistema genera PDF del contrato
3. Comprador firma digitalmente
4. Vendedor firma digitalmente
5. Sistema sella con timestamp
6. Ambas partes reciben copia firmada
7. Contrato almacenado con hash de verificación
```

#### ECOM-SIGN-003: Verificación de Firmas

| Campo       | Valor               |
| ----------- | ------------------- |
| **Proceso** | ECOM-SIGN-003       |
| **Ruta**    | `/documents/verify` |
| **Estado**  | 🔴 Pendiente        |

---

## 4. Validez de Documentos Electrónicos

### 4.1 Documentos con Validez Legal

Según la Ley 126-02, estos documentos electrónicos tienen la misma validez que los físicos:

| Documento                | Validez           | Requisitos                | Estado OKLA |
| ------------------------ | ----------------- | ------------------------- | ----------- |
| Contrato de suscripción  | ✅ Legal          | Aceptación electrónica    | ✅          |
| Factura electrónica      | ✅ Legal          | NCF + e-CF                | 🟡          |
| Contrato de compraventa  | ⚠️ Requiere firma | Firma digital certificada | 🔴          |
| Recibos de pago          | ✅ Legal          | Timestamp + referencia    | ✅          |
| Comunicaciones oficiales | ✅ Legal          | Registro de envío         | ✅          |

### 4.2 Requisitos para Validez Legal

| Requisito    | Descripción              | Estado                    |
| ------------ | ------------------------ | ------------------------- |
| Integridad   | Documento no alterado    | ✅ Hash SHA-256           |
| Autenticidad | Identidad verificable    | 🟡 Parcial                |
| No repudio   | No negar la firma        | 🔴 Requiere firma digital |
| Timestamp    | Fecha y hora certificada | 🟡 Servidor               |
| Conservación | 10 años mínimo           | ✅ S3 + Glacier           |

---

## 5. Endpoints API

### 5.1 ECommerceController

| Método | Endpoint                               | Descripción                    | Auth | Estado |
| ------ | -------------------------------------- | ------------------------------ | ---- | ------ |
| `GET`  | `/api/legal/terms`                     | Obtener términos vigentes      | ❌   | ✅     |
| `GET`  | `/api/legal/privacy`                   | Obtener política de privacidad | ❌   | ✅     |
| `GET`  | `/api/legal/company-info`              | Datos de la empresa            | ❌   | ✅     |
| `POST` | `/api/legal/terms/accept`              | Registrar aceptación           | ✅   | ✅     |
| `GET`  | `/api/legal/terms/acceptance/{userId}` | Ver historial de aceptaciones  | ✅   | ✅     |

### 5.2 DocumentSigningController (Futuro)

| Método | Endpoint                       | Descripción                 | Auth | Estado |
| ------ | ------------------------------ | --------------------------- | ---- | ------ |
| `POST` | `/api/documents/sign/request`  | Solicitar firma             | ✅   | 🔴     |
| `POST` | `/api/documents/sign/complete` | Completar firma             | ✅   | 🔴     |
| `GET`  | `/api/documents/verify/{hash}` | Verificar documento         | ❌   | 🔴     |
| `GET`  | `/api/documents/signed/{id}`   | Descargar documento firmado | ✅   | 🔴     |

---

## 6. Componentes UI

### 6.1 Existentes

| Componente    | Ruta       | Descripción            |
| ------------- | ---------- | ---------------------- |
| TermsPage     | `/terms`   | Términos y condiciones |
| PrivacyPage   | `/privacy` | Política de privacidad |
| Footer        | (global)   | Información de empresa |
| AboutPage     | `/about`   | Acerca de OKLA         |
| TermsCheckbox | Registro   | Aceptación de términos |

### 6.2 Pendientes

| Componente           | Ruta                | Prioridad |
| -------------------- | ------------------- | --------- |
| DocumentSigningPage  | `/contracts/sign`   | Media     |
| DocumentVerifyPage   | `/documents/verify` | Media     |
| SignatureWidget      | (componente)        | Media     |
| CertificateInfoModal | (modal)             | Baja      |

---

## 7. Cronograma de Implementación

### Fase 1: Completado ✅

- Información de empresa en footer
- Página de términos y condiciones
- Página de política de privacidad
- Checkbox de aceptación en registro
- Email de confirmación de transacciones

### Fase 2: Q1 2026 🟡

- Mejorar trazabilidad de aceptaciones
- Versionado de términos
- Notificación de cambios a usuarios

### Fase 3: Q2 2026 (Firma Digital) 🔴

- Seleccionar proveedor de certificados
- Integrar SDK de firma digital
- UI de firma de contratos
- Verificación de documentos

---

## 8. Sanciones por Incumplimiento

| Infracción                              | Sanción                 |
| --------------------------------------- | ----------------------- |
| No publicar información del proveedor   | 10-50 salarios mínimos  |
| No entregar confirmación de transacción | 5-25 salarios mínimos   |
| Publicidad engañosa                     | 20-100 salarios mínimos |
| Incumplimiento de contrato electrónico  | Responsabilidad civil   |

---

## 9. Referencias

| Documento                    | Ubicación            |
| ---------------------------- | -------------------- |
| Ley 126-02                   | congreso.gob.do      |
| Reglamento de aplicación     | indotel.gob.do       |
| Guía de comercio electrónico | proconsumidor.gob.do |
| Términos OKLA                | `/terms`             |
| Política de Privacidad       | `/privacy`           |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Abril 25, 2026  
**Responsable:** Equipo Legal OKLA

# 🔍 KYC Service - Matriz de Procesos

> **Servicio:** KYCService  
> **Puerto:** 5020  
> **Última actualización:** Enero 23, 2026  
> **Estado:** 🟢 ACTIVO  
> **Regulación:** Ley 155-17 (Prevención Lavado de Activos)

---

## 📊 Estado de Implementación

### Leyenda de Estados

| Icono | Estado                   | Descripción                             |
| ----- | ------------------------ | --------------------------------------- |
| ✅    | **Implementado**         | Código completo y funcional             |
| 🧪    | **Probado**              | Implementado + Tests pasando            |
| 🚀    | **En Producción**        | Desplegado en DOKS                      |
| 🔄    | **En Progreso**          | Parcialmente implementado               |
| ⏳    | **Pendiente**            | No iniciado                             |
| 🔌    | **Requiere Integración** | Necesita servicio externo (Azure, etc.) |

---

### 📁 Backend - KYCService (Puerto 5020)

#### Controllers

| Componente                     | Estado | Archivo                                         | Notas                                 |
| ------------------------------ | ------ | ----------------------------------------------- | ------------------------------------- |
| KYCProfilesController          | ✅     | `Controllers/KYCProfilesController.cs`          | CRUD de perfiles KYC                  |
| KYCDocumentsController         | ✅     | `Controllers/KYCDocumentsController.cs`         | Gestión de documentos                 |
| IdentityVerificationController | ✅     | `Controllers/IdentityVerificationController.cs` | Verificación biométrica               |
| WatchlistController            | ✅     | `Controllers/WatchlistController.cs`            | Lista de vigilancia                   |
| STRController                  | ✅     | `Controllers/STRController.cs`                  | Reportes de transacciones sospechosas |

#### Domain Layer

| Componente                   | Estado | Archivo                                           | Notas                                |
| ---------------------------- | ------ | ------------------------------------------------- | ------------------------------------ |
| KYCEntities                  | ✅     | `Domain/Entities/KYCEntities.cs`                  | KYCProfile, KYCDocument, etc.        |
| IdentityVerificationEntities | ✅     | `Domain/Entities/IdentityVerificationEntities.cs` | Sesiones de verificación biométrica  |
| CedulaValidator              | ✅     | `Domain/Validators/CedulaValidator.cs`            | Validación Modulo 10 para cédulas RD |

#### Application Layer

| Componente                   | Estado | Archivo                                                | Notas                            |
| ---------------------------- | ------ | ------------------------------------------------------ | -------------------------------- |
| KYCCommands                  | ✅     | `Application/Commands/KYCCommands.cs`                  | CreateProfile, Approve, Reject   |
| IdentityVerificationCommands | ✅     | `Application/Commands/IdentityVerificationCommands.cs` | Start, ProcessDoc, ProcessSelfie |
| IdentityVerificationQueries  | ✅     | `Application/Queries/IdentityVerificationQueries.cs`   | GetSession, GetHistory           |
| IdentityVerificationHandlers | ✅ 🔌  | `Application/Handlers/IdentityVerificationHandlers.cs` | Handlers con TODOs para Azure AI |
| IdentityVerificationDtos     | ✅     | `Application/DTOs/IdentityVerificationDtos.cs`         | Request/Response DTOs            |

#### Infrastructure Layer

| Componente                  | Estado | Notas                             |
| --------------------------- | ------ | --------------------------------- |
| KYCDbContext                | ✅     | EF Core DbContext                 |
| Repositories                | ✅     | Patrón Repository implementado    |
| Azure Computer Vision (OCR) | ⏳ 🔌  | TODO: Integrar OCR real           |
| Azure Face API              | ⏳ 🔌  | TODO: Integrar comparación facial |
| RabbitMQ Events             | ⏳     | TODO: Publicar eventos            |

---

### 🌐 Frontend - React/TypeScript

#### Servicios

| Componente                     | Estado | Archivo                                   | Notas                                   |
| ------------------------------ | ------ | ----------------------------------------- | --------------------------------------- |
| kycService.ts                  | ✅     | `services/kycService.ts`                  | API client para perfiles KYC            |
| identityVerificationService.ts | ✅     | `services/identityVerificationService.ts` | API client para verificación biométrica |

#### Páginas

| Componente                | Estado | Archivo                                   | Notas                                  |
| ------------------------- | ------ | ----------------------------------------- | -------------------------------------- |
| KYCVerificationPage       | ✅     | `pages/kyc/KYCVerificationPage.tsx`       | Verificación básica (subir documentos) |
| KYCStatusPage             | ✅     | `pages/kyc/KYCStatusPage.tsx`             | Estado del KYC del usuario             |
| BiometricVerificationPage | ✅     | `pages/kyc/BiometricVerificationPage.tsx` | Wizard de verificación biométrica      |

#### Componentes

| Componente        | Estado | Archivo                                | Notas                                     |
| ----------------- | ------ | -------------------------------------- | ----------------------------------------- |
| DocumentCapture   | ✅     | `components/kyc/DocumentCapture.tsx`   | Captura de cédula con cámara              |
| LivenessChallenge | ✅     | `components/kyc/LivenessChallenge.tsx` | Desafíos de liveness (parpadear, sonreír) |
| index.ts (barrel) | ✅     | `components/kyc/index.ts`              | Exports del módulo                        |

#### Rutas

| Ruta                    | Estado | Componente                | Auth           |
| ----------------------- | ------ | ------------------------- | -------------- |
| `/kyc/verify`           | ✅     | KYCVerificationPage       | ProtectedRoute |
| `/kyc/status`           | ✅     | KYCStatusPage             | ProtectedRoute |
| `/kyc/biometric-verify` | ✅     | BiometricVerificationPage | ProtectedRoute |
| `/admin/kyc`            | ✅     | KYCAdminReviewPage        | Admin          |

---

### 📋 Procesos por Estado

#### ✅ Implementado (Backend + Frontend)

| ID           | Proceso                          | Backend | Frontend | Tests |
| ------------ | -------------------------------- | ------- | -------- | ----- |
| KYC-BIO-001  | Verificación Biométrica Completa | ✅      | ✅       | ⏳    |
| KYC-PROF-001 | Crear Perfil KYC                 | ✅      | ✅       | ⏳    |
| KYC-DOC-001  | Subir Documento KYC              | ✅      | ✅       | ⏳    |

#### 🔄 En Progreso (Requiere Integración Externa)

| ID  | Proceso            | Backend | Frontend | Pendiente                 |
| --- | ------------------ | ------- | -------- | ------------------------- |
| -   | OCR de Documentos  | 🔌      | ✅       | Azure Computer Vision     |
| -   | Comparación Facial | 🔌      | ✅       | Azure Face API            |
| -   | Liveness Detection | 🔌      | ✅       | Azure Face API (Liveness) |

#### ⏳ Pendiente

| ID          | Proceso                 | Descripción             | Prioridad |
| ----------- | ----------------------- | ----------------------- | --------- |
| KYC-REV-001 | Aprobar Perfil KYC      | Dashboard de compliance | Alta      |
| KYC-REV-002 | Rechazar Perfil KYC     | Dashboard de compliance | Alta      |
| KYC-MON-001 | Monitoreo de Expiración | Job programado          | Media     |
| -           | Integración JCE         | Validación contra JCE   | Baja      |
| -           | Integración UAF/PEP     | Listas PEP/Sanciones    | Alta      |
| -           | RabbitMQ Events         | Publicar eventos        | Media     |

---

### 🧪 Estado de Tests

| Componente                     | Unit Tests | Integration Tests | E2E |
| ------------------------------ | ---------- | ----------------- | --- |
| KYCProfilesController          | ⏳         | ⏳                | ⏳  |
| IdentityVerificationController | ⏳         | ⏳                | ⏳  |
| CedulaValidator                | ⏳         | N/A               | N/A |
| Frontend Components            | ⏳         | ⏳                | ⏳  |

---

### 🚀 Estado de Despliegue

| Ambiente          | Estado | Notas                    |
| ----------------- | ------ | ------------------------ |
| Desarrollo Local  | ✅     | docker-compose funcional |
| Staging           | ⏳     | No configurado           |
| Producción (DOKS) | ⏳     | Pendiente despliegue     |

---

### 📅 Próximos Pasos

1. **Alta Prioridad:**
   - [ ] Integrar Azure Computer Vision para OCR real
   - [ ] Integrar Azure Face API para comparación facial
   - [ ] Crear tests unitarios para CedulaValidator
   - [ ] Implementar dashboard de compliance (Admin)

2. **Media Prioridad:**
   - [ ] Implementar eventos RabbitMQ
   - [ ] Crear job de monitoreo de expiración
   - [ ] Agregar métricas Prometheus

3. **Baja Prioridad:**
   - [ ] Integración con JCE (si API disponible)
   - [ ] Integración con listas UAF/PEP
   - [ ] World Check integration

---

## 📊 Diagrama de Flujo de Datos Completo

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                           FLUJO DE DATOS KYC - ARQUITECTURA COMPLETA                                │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘

                                    ┌─────────────────┐
                                    │    USUARIO      │
                                    │   (Mobile/Web)  │
                                    └────────┬────────┘
                                             │
         ┌───────────────────────────────────┼───────────────────────────────────┐
         │                                   │                                   │
         ▼                                   ▼                                   ▼
┌─────────────────┐              ┌─────────────────────┐              ┌─────────────────┐
│  1. REGISTRO    │              │  2. VERIFICACIÓN    │              │  3. CONSULTA    │
│  PERFIL KYC     │              │     BIOMÉTRICA      │              │    ESTADO       │
└────────┬────────┘              └──────────┬──────────┘              └────────┬────────┘
         │                                  │                                   │
         ▼                                  ▼                                   ▼
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│                                    API GATEWAY                                            │
│                              (https://api.okla.com.do)                                    │
│  ┌────────────────────────────────────────────────────────────────────────────────────┐  │
│  │  Routes:                                                                            │  │
│  │  /api/kyc/profiles/* → KYCService:5020                                             │  │
│  │  /api/kyc/identity-verification/* → KYCService:5020                                │  │
│  │  /api/kyc/documents/* → KYCService:5020                                            │  │
│  └────────────────────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────────────────────┘
                                            │
                                            ▼
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│                                    KYC SERVICE                                            │
│                                    (Puerto: 5020)                                         │
│                                                                                           │
│  ┌─────────────────┐  ┌─────────────────────────┐  ┌─────────────────────────────────┐   │
│  │  CONTROLLERS    │  │      APPLICATION        │  │         DOMAIN                  │   │
│  │                 │  │                         │  │                                 │   │
│  │ • KYCProfiles   │  │  Commands:              │  │  Entities:                      │   │
│  │ • Identity      │  │  • CreateKYCProfile     │  │  • KYCProfile                   │   │
│  │   Verification  │  │  • StartVerification    │  │  • IdentityVerificationSession  │   │
│  │ • KYCDocuments  │  │  • ProcessDocument      │  │  • KYCDocument                  │   │
│  │ • Watchlist     │  │  • ProcessSelfie        │  │  • KYCVerification              │   │
│  │ • STR           │  │  • CompleteVerification │  │  • WatchlistEntry               │   │
│  │                 │  │                         │  │  • SuspiciousTransactionReport  │   │
│  │                 │  │  Queries:               │  │                                 │   │
│  │                 │  │  • GetProfile           │  │  Validators:                    │   │
│  │                 │  │  • GetVerificationStatus│  │  • CedulaValidator              │   │
│  │                 │  │  • GetPendingProfiles   │  │  • DocumentValidator            │   │
│  └─────────────────┘  └─────────────────────────┘  └─────────────────────────────────┘   │
│                                                                                           │
│  ┌────────────────────────────────────────────────────────────────────────────────────┐  │
│  │                           INFRASTRUCTURE                                            │  │
│  │                                                                                     │  │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────────────────┐  │  │
│  │  │   PostgreSQL     │  │   Azure AI       │  │        RabbitMQ                  │  │  │
│  │  │   Database       │  │   Services       │  │        Events                    │  │  │
│  │  │                  │  │                  │  │                                  │  │  │
│  │  │ • kyc_profiles   │  │ • Computer Vision│  │ • kyc.profile.created            │  │  │
│  │  │ • identity_      │  │   (OCR)          │  │ • kyc.verification.started       │  │  │
│  │  │   verification_  │  │ • Face API       │  │ • kyc.verification.completed     │  │  │
│  │  │   sessions       │  │   (Compare)      │  │ • kyc.verification.failed        │  │  │
│  │  │ • kyc_documents  │  │ • Liveness       │  │ • kyc.document.uploaded          │  │  │
│  │  │ • watchlist      │  │   Detection      │  │ • kyc.pep.detected               │  │  │
│  │  └──────────────────┘  └──────────────────┘  └──────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────────────────────┘
         │                          │                              │
         ▼                          ▼                              ▼
┌─────────────────┐      ┌─────────────────────┐      ┌─────────────────────────┐
│  MediaService   │      │ ComplianceService   │      │ NotificationService     │
│  (S3 Storage)   │      │ (PEP/Sanciones)     │      │ (Email/SMS/Push)        │
│                 │      │                     │      │                         │
│ Almacena:       │      │ Verifica:           │      │ Envía:                  │
│ • Documentos    │      │ • Lista PEP UAF     │      │ • Verificación OK       │
│ • Selfies       │      │ • OFAC Sanctions    │      │ • Documentos pendientes │
│ • Evidencias    │      │ • UN/EU Lists       │      │ • KYC por expirar       │
└─────────────────┘      └─────────────────────┘      └─────────────────────────┘
```

---

## 🔄 Flujos de Proceso Detallados

### Flujo 1: Verificación Biométrica Completa (Estilo Qik)

```
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO PASO A PASO: VERIFICACIÓN BIOMÉTRICA                                    │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘

PASO 1: INICIAR SESIÓN DE VERIFICACIÓN
═══════════════════════════════════════
Usuario                          Frontend                         Backend (KYCService)
  │                                  │                                    │
  │  Click "Verificar Identidad"     │                                    │
  │────────────────────────────────>│                                    │
  │                                  │  POST /api/kyc/identity-           │
  │                                  │  verification/start                │
  │                                  │  {documentType, deviceInfo}        │
  │                                  │───────────────────────────────────>│
  │                                  │                                    │
  │                                  │                                    │ 1. Crear sesión UUID
  │                                  │                                    │ 2. Generar challenges
  │                                  │                                    │ 3. Calcular expiración
  │                                  │                                    │
  │                                  │   {sessionId, challenges,          │
  │                                  │    expiresAt, instructions}        │
  │                                  │<───────────────────────────────────│
  │   Mostrar pantalla de captura    │                                    │
  │<────────────────────────────────│                                    │
  │                                  │                                    │

PASO 2: CAPTURA FRENTE DE DOCUMENTO
════════════════════════════════════
Usuario                          Frontend                         Backend                    Azure AI
  │                                  │                                    │                      │
  │  Alinea cédula en cámara         │                                    │                      │
  │────────────────────────────────>│                                    │                      │
  │                                  │ Detectar bordes automático         │                      │
  │                                  │ Validar calidad imagen             │                      │
  │                                  │ Auto-capture o manual              │                      │
  │                                  │                                    │                      │
  │                                  │  POST /api/kyc/identity-           │                      │
  │                                  │  verification/document             │                      │
  │                                  │  {sessionId, side:"Front", image}  │                      │
  │                                  │───────────────────────────────────>│                      │
  │                                  │                                    │                      │
  │                                  │                                    │  OCR Request         │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │  Extraer:            │
  │                                  │                                    │  • Nombre            │
  │                                  │                                    │  • Cédula            │
  │                                  │                                    │  • Fecha Nac         │
  │                                  │                                    │  • Foto              │
  │                                  │                                    │<─────────────────────│
  │                                  │                                    │                      │
  │                                  │                                    │ Validar formato cédula│
  │                                  │                                    │ Validar checksum     │
  │                                  │                                    │ Guardar en S3        │
  │                                  │                                    │                      │
  │                                  │   {ocrResult, documentValidation,  │                      │
  │                                  │    nextStep: "CAPTURE_BACK"}       │                      │
  │                                  │<───────────────────────────────────│                      │
  │                                  │                                    │                      │
  │  "Ahora voltea la cédula"        │                                    │                      │
  │<────────────────────────────────│                                    │                      │
  │                                  │                                    │                      │

PASO 3: CAPTURA REVERSO DE DOCUMENTO
═════════════════════════════════════
Usuario                          Frontend                         Backend                    Azure AI
  │                                  │                                    │                      │
  │  Alinea reverso de cédula        │                                    │                      │
  │────────────────────────────────>│                                    │                      │
  │                                  │                                    │                      │
  │                                  │  POST /document {side:"Back"}      │                      │
  │                                  │───────────────────────────────────>│                      │
  │                                  │                                    │  OCR (MRZ zone)      │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │<─────────────────────│
  │                                  │                                    │                      │
  │                                  │                                    │ Validar consistencia │
  │                                  │                                    │ frente vs reverso    │
  │                                  │                                    │                      │
  │                                  │   {status:"AwaitingSelfie"}        │                      │
  │                                  │<───────────────────────────────────│                      │
  │  "Ahora tu selfie"               │                                    │                      │
  │<────────────────────────────────│                                    │                      │

PASO 4: LIVENESS DETECTION + SELFIE
════════════════════════════════════
Usuario                          Frontend                         Backend                    Azure Face
  │                                  │                                    │                      │
  │  Cámara frontal activa           │                                    │                      │
  │────────────────────────────────>│                                    │                      │
  │                                  │ Detectar rostro en óvalo           │                      │
  │                                  │                                    │                      │
  │  "Gira cabeza a la izquierda"    │                                    │                      │
  │<────────────────────────────────│                                    │                      │
  │  *Gira la cabeza*                │                                    │                      │
  │────────────────────────────────>│ Detectar movimiento                 │                      │
  │                                  │ Challenge 1/3 ✓                    │                      │
  │                                  │                                    │                      │
  │  "Ahora sonríe"                  │                                    │                      │
  │<────────────────────────────────│                                    │                      │
  │  *Sonríe*                        │                                    │                      │
  │────────────────────────────────>│ Detectar expresión                  │                      │
  │                                  │ Challenge 2/3 ✓                    │                      │
  │                                  │                                    │                      │
  │  "Parpadea 2 veces"              │                                    │                      │
  │<────────────────────────────────│                                    │                      │
  │  *Parpadea*                      │                                    │                      │
  │────────────────────────────────>│ Detectar parpadeos                  │                      │
  │                                  │ Challenge 3/3 ✓                    │                      │
  │                                  │                                    │                      │
  │                                  │  POST /selfie                      │                      │
  │                                  │  {sessionId, selfieImage,          │                      │
  │                                  │   livenessData: challenges}        │                      │
  │                                  │───────────────────────────────────>│                      │
  │                                  │                                    │  Liveness Check      │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │  Score: 88%          │
  │                                  │                                    │<─────────────────────│
  │                                  │                                    │                      │
  │                                  │                                    │  Face Compare        │
  │                                  │                                    │  (doc vs selfie)     │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │  Match: 94.5%        │
  │                                  │                                    │<─────────────────────│
  │                                  │                                    │                      │
  │                                  │   {livenessScore, faceMatchScore}  │                      │
  │                                  │<───────────────────────────────────│                      │
  │  "Procesando..."                 │                                    │                      │
  │<────────────────────────────────│                                    │                      │

PASO 5: COMPLETAR VERIFICACIÓN
═══════════════════════════════
Usuario                          Frontend                         Backend                    Services
  │                                  │                                    │                      │
  │                                  │  POST /complete {sessionId}        │                      │
  │                                  │───────────────────────────────────>│                      │
  │                                  │                                    │                      │
  │                                  │                                    │ 1. Validar todas     │
  │                                  │                                    │    las verificaciones│
  │                                  │                                    │                      │
  │                                  │                                    │ 2. Crear/Actualizar  │
  │                                  │                                    │    KYCProfile        │
  │                                  │                                    │                      │
  │                                  │                                    │ 3. Verificar PEP     │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │    Compliance        │
  │                                  │                                    │<─────────────────────│
  │                                  │                                    │                      │
  │                                  │                                    │ 4. Calcular Risk     │
  │                                  │                                    │    Score             │
  │                                  │                                    │                      │
  │                                  │                                    │ 5. Publicar evento   │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │    RabbitMQ          │
  │                                  │                                    │                      │
  │                                  │                                    │ 6. Notificar usuario │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │    Notification      │
  │                                  │                                    │                      │
  │                                  │   {verified: true,                 │                      │
  │                                  │    overallScore: 92.5,             │                      │
  │                                  │    kycStatus: "PendingReview",     │                      │
  │                                  │    extractedProfile: {...}}        │                      │
  │                                  │<───────────────────────────────────│                      │
  │                                  │                                    │                      │
  │  "¡Verificación Exitosa!"        │                                    │                      │
  │<────────────────────────────────│                                    │                      │
```

### Flujo 2: Aprobación de Perfil KYC (Compliance)

```
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO PASO A PASO: APROBACIÓN KYC (COMPLIANCE)                                │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘

Oficial Compliance              Dashboard Admin                    Backend                    Servicios
  │                                  │                                    │                      │
  │  Accede a dashboard              │                                    │                      │
  │────────────────────────────────>│                                    │                      │
  │                                  │  GET /api/kyc/profiles/pending     │                      │
  │                                  │───────────────────────────────────>│                      │
  │                                  │                                    │                      │
  │                                  │   [lista de perfiles pendientes]   │                      │
  │                                  │<───────────────────────────────────│                      │
  │  Ver lista de pendientes         │                                    │                      │
  │<────────────────────────────────│                                    │                      │
  │                                  │                                    │                      │
  │  Selecciona perfil               │                                    │                      │
  │────────────────────────────────>│                                    │                      │
  │                                  │  GET /api/kyc/profiles/{id}        │                      │
  │                                  │───────────────────────────────────>│                      │
  │                                  │                                    │                      │
  │                                  │   {profile + documents + scores}   │                      │
  │                                  │<───────────────────────────────────│                      │
  │                                  │                                    │                      │
  │  Revisa:                         │                                    │                      │
  │  • Fotos de documentos           │                                    │                      │
  │  • Datos OCR extraídos           │                                    │                      │
  │  • Selfie vs documento           │                                    │                      │
  │  • Face match score              │                                    │                      │
  │  • Alertas PEP/Sanciones         │                                    │                      │
  │                                  │                                    │                      │
  │  Click "Aprobar"                 │                                    │                      │
  │────────────────────────────────>│                                    │                      │
  │                                  │  POST /api/kyc/profiles/{id}/      │                      │
  │                                  │  approve {approvedBy, notes}       │                      │
  │                                  │───────────────────────────────────>│                      │
  │                                  │                                    │                      │
  │                                  │                                    │ 1. Actualizar status │
  │                                  │                                    │    = Approved        │
  │                                  │                                    │                      │
  │                                  │                                    │ 2. Calcular expiry   │
  │                                  │                                    │    (según riesgo)    │
  │                                  │                                    │                      │
  │                                  │                                    │ 3. Actualizar User   │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │    UserService       │
  │                                  │                                    │    isKYCVerified=true│
  │                                  │                                    │                      │
  │                                  │                                    │ 4. Publicar evento   │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │    kyc.approved      │
  │                                  │                                    │                      │
  │                                  │                                    │ 5. Notificar usuario │
  │                                  │                                    │─────────────────────>│
  │                                  │                                    │    Email + Push      │
  │                                  │                                    │                      │
  │                                  │   {status: "Approved",             │                      │
  │                                  │    expiresAt: "2027-01-23"}        │                      │
  │                                  │<───────────────────────────────────│                      │
  │  "Perfil aprobado"               │                                    │                      │
  │<────────────────────────────────│                                    │                      │
```

### Flujo 3: Rechazo y Reintento

```
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO: VERIFICACIÓN FALLIDA → REINTENTO                                       │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘

                          ┌─────────────────────────────────────┐
                          │    VERIFICACIÓN BIOMÉTRICA FALLA    │
                          └──────────────────┬──────────────────┘
                                             │
                  ┌──────────────────────────┼──────────────────────────┐
                  │                          │                          │
                  ▼                          ▼                          ▼
     ┌────────────────────┐    ┌────────────────────┐    ┌────────────────────┐
     │ DocumentBlurry (1) │    │ FaceMismatch (7)   │    │LivenessFailed (8)  │
     │ DocumentCutOff (2) │    │ Score < 80%        │    │ Anti-spoof falló   │
     │ DocumentGlare (3)  │    │                    │    │                    │
     │ OCRFailed (11)     │    │                    │    │                    │
     └─────────┬──────────┘    └─────────┬──────────┘    └─────────┬──────────┘
               │                         │                          │
               ▼                         ▼                          ▼
     ┌─────────────────────────────────────────────────────────────────────┐
     │                    RESPUESTA DE ERROR                               │
     │  {                                                                  │
     │    "status": "Failed",                                              │
     │    "failureReason": "FaceMismatch",                                 │
     │    "failureDetails": "La selfie no coincide con el documento",      │
     │    "attemptsRemaining": 2,                                          │
     │    "canRetry": true,                                                │
     │    "suggestions": [                                                 │
     │      "Asegúrate que el documento sea tuyo",                         │
     │      "Mejora la iluminación",                                       │
     │      "Quita lentes o accesorios"                                    │
     │    ]                                                                │
     │  }                                                                  │
     └────────────────────────────────────┬────────────────────────────────┘
                                          │
                    ┌─────────────────────┴─────────────────────┐
                    │                                           │
                    ▼                                           ▼
          ┌─────────────────┐                        ┌─────────────────┐
          │  REINTENTAR     │                        │  CONTACTAR      │
          │  (Si intentos   │                        │  SOPORTE        │
          │  restantes > 0) │                        │  (Si agotó      │
          │                 │                        │  intentos)      │
          └────────┬────────┘                        └────────┬────────┘
                   │                                          │
                   ▼                                          ▼
     ┌────────────────────────┐                 ┌────────────────────────┐
     │ POST /retry            │                 │ Crear ticket soporte   │
     │ {sessionId}            │                 │ Revisión manual        │
     │                        │                 │ +1 809-555-0000        │
     │ Volver a PASO 1        │                 │ soporte@okla.com.do    │
     └────────────────────────┘                 └────────────────────────┘
```

---

## 1. Información General

### 1.1 Descripción

Sistema de Know Your Customer (KYC) para verificación de identidad de usuarios y dealers en OKLA. Cumple con la Ley 155-17 de Prevención de Lavado de Activos de República Dominicana y normativas de la Unidad de Análisis Financiero (UAF).

### 1.2 Dependencias

| Servicio            | Propósito                    |
| ------------------- | ---------------------------- |
| UserService         | Información de usuarios      |
| ComplianceService   | Verificaciones de compliance |
| MediaService        | Almacenamiento de documentos |
| NotificationService | Alertas de estado KYC        |

### 1.3 Componentes

- **KYCProfilesController**: Gestión de perfiles KYC
- **KYCDocumentsController**: Gestión de documentos

---

## 2. Endpoints API

### 2.1 KYCProfilesController ✅

| Método | Endpoint                                     | Descripción                 | Auth | Roles               | Estado |
| ------ | -------------------------------------------- | --------------------------- | ---- | ------------------- | ------ |
| `GET`  | `/api/kycprofiles`                           | Listar perfiles con filtros | ✅   | Admin, Compliance   | ✅     |
| `GET`  | `/api/kycprofiles/{id}`                      | Obtener perfil por ID       | ✅   | User (owner), Admin | ✅     |
| `GET`  | `/api/kycprofiles/user/{userId}`             | Obtener por User ID         | ✅   | User (owner), Admin | ✅     |
| `GET`  | `/api/kycprofiles/document/{documentNumber}` | Buscar por documento        | ✅   | Admin, Compliance   | ✅     |
| `POST` | `/api/kycprofiles`                           | Crear perfil KYC            | ✅   | User                | ✅     |
| `PUT`  | `/api/kycprofiles/{id}`                      | Actualizar perfil           | ✅   | User (owner), Admin | ✅     |
| `POST` | `/api/kycprofiles/{id}/approve`              | Aprobar perfil              | ✅   | Admin, Compliance   | ✅     |
| `POST` | `/api/kycprofiles/{id}/reject`               | Rechazar perfil             | ✅   | Admin, Compliance   | ✅     |
| `GET`  | `/api/kycprofiles/pending`                   | Perfiles pendientes         | ✅   | Admin, Compliance   | ✅     |
| `GET`  | `/api/kycprofiles/expiring`                  | Perfiles próximos a expirar | ✅   | Admin, Compliance   | ✅     |
| `GET`  | `/api/kycprofiles/statistics`                | Estadísticas KYC            | ✅   | Admin, Compliance   | ✅     |

### 2.2 KYCDocumentsController ✅

| Método   | Endpoint                                | Descripción             | Auth | Roles               | Estado |
| -------- | --------------------------------------- | ----------------------- | ---- | ------------------- | ------ |
| `GET`    | `/api/kycdocuments/profile/{profileId}` | Documentos de un perfil | ✅   | User, Admin         | ✅     |
| `POST`   | `/api/kycdocuments`                     | Subir documento         | ✅   | User                | ✅     |
| `PUT`    | `/api/kycdocuments/{id}/verify`         | Verificar documento     | ✅   | Admin, Compliance   | ✅     |
| `DELETE` | `/api/kycdocuments/{id}`                | Eliminar documento      | ✅   | User (owner), Admin | ✅     |

### 2.3 IdentityVerificationController ✅

| Método   | Endpoint                                     | Descripción                    | Auth | Estado |
| -------- | -------------------------------------------- | ------------------------------ | ---- | ------ |
| `POST`   | `/api/kyc/identity-verification/start`       | Iniciar sesión de verificación | ✅   | ✅     |
| `POST`   | `/api/kyc/identity-verification/document`    | Subir foto de documento        | ✅   | ✅ 🔌  |
| `POST`   | `/api/kyc/identity-verification/selfie`      | Subir selfie con liveness      | ✅   | ✅ 🔌  |
| `POST`   | `/api/kyc/identity-verification/complete`    | Completar verificación         | ✅   | ✅     |
| `GET`    | `/api/kyc/identity-verification/{sessionId}` | Obtener estado de verificación | ✅   | ✅     |
| `GET`    | `/api/kyc/identity-verification/active`      | Obtener sesión activa          | ✅   | ✅     |
| `POST`   | `/api/kyc/identity-verification/retry`       | Reintentar verificación        | ✅   | ✅     |
| `DELETE` | `/api/kyc/identity-verification/{sessionId}` | Cancelar sesión                | ✅   | ✅     |
| `GET`    | `/api/kyc/identity-verification/history`     | Historial de verificaciones    | ✅   | ✅     |
| `GET`    | `/api/kyc/identity-verification/can-start`   | Verificar si puede iniciar     | ✅   | ✅     |

> 🔌 **Nota:** Los endpoints de `document` y `selfie` están implementados con respuestas simuladas. Requieren integración con Azure AI Services para OCR y comparación facial real.

---

## 3. Entidades y Enums

### 3.1 KYCStatus (Enum)

```csharp
public enum KYCStatus
{
    NotStarted = 0,        // Usuario no ha iniciado KYC
    InProgress = 1,        // Documentos en proceso de subida
    PendingReview = 2,     // Esperando revisión de compliance
    UnderReview = 3,       // En revisión activa
    Approved = 4,          // KYC aprobado
    Rejected = 5,          // KYC rechazado
    Expired = 6,           // KYC expirado (requiere renovación)
    Suspended = 7          // Suspendido por investigación
}
```

### 3.2 RiskLevel (Enum)

```csharp
public enum RiskLevel
{
    Low = 0,               // Bajo riesgo - verificación estándar
    Medium = 1,            // Riesgo medio - revisión adicional
    High = 2,              // Alto riesgo - due diligence reforzada
    Critical = 3           // Crítico - requiere escalamiento
}
```

### 3.3 DocumentType (Enum)

```csharp
public enum DocumentType
{
    // Documentos de identidad
    Cedula = 0,            // Cédula dominicana
    Passport = 1,          // Pasaporte
    DriverLicense = 2,     // Licencia de conducir

    // Comprobantes de dirección
    UtilityBill = 10,      // Factura de servicios
    BankStatement = 11,    // Estado de cuenta bancario
    LeaseAgreement = 12,   // Contrato de alquiler

    // Documentos de negocio (Dealers)
    RNC = 20,              // Registro Nacional Contribuyente
    MercantileRegistry = 21,// Registro Mercantil
    BusinessLicense = 22,  // Licencia comercial
    TaxCertificate = 23,   // Certificación DGII

    // Documentos financieros
    IncomeProof = 30,      // Comprobante de ingresos
    TaxReturn = 31,        // Declaración de impuestos

    // Selfie/Verificación
    Selfie = 40,           // Foto selfie
    SelfieWithDocument = 41 // Selfie con documento
}
```

### 3.4 KYCProfile (Entidad Principal)

```csharp
public class KYCProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Información Personal
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DocumentNumber { get; set; }      // Cédula/Pasaporte
    public DocumentType DocumentType { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Nationality { get; set; }

    // Información de Contacto
    public string Address { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string PhoneNumber { get; set; }

    // Estado y Riesgo
    public KYCStatus Status { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public decimal RiskScore { get; set; }          // 0-100

    // PEP (Persona Expuesta Políticamente)
    public bool IsPEP { get; set; }
    public string? PEPPosition { get; set; }
    public string? PEPRelationship { get; set; }

    // Fuente de Fondos
    public string SourceOfFunds { get; set; }
    public string Occupation { get; set; }
    public decimal? ExpectedMonthlyTransaction { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Documentos
    public List<KYCDocument> Documents { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.0 🆕 KYC-BIO-001: Verificación Biométrica con Selfie + Documento (Estilo Qik)

| Campo       | Valor                                              |
| ----------- | -------------------------------------------------- |
| **ID**      | KYC-BIO-001                                        |
| **Nombre**  | Verificación de Identidad con Foto y Documento     |
| **Actor**   | Usuario registrado                                 |
| **Trigger** | POST /api/kyc/identity-verification/start          |
| **Ref**     | Similar a proceso de verificación de Qik (Popular) |

#### Descripción

Proceso de verificación de identidad en tiempo real donde el usuario:

1. Captura foto del documento de identidad (frente y reverso)
2. Se toma una selfie en tiempo real con detección de vida (liveness)
3. El sistema compara la foto del documento con la selfie
4. Extrae datos del documento mediante OCR
5. Valida la autenticidad del documento

Este proceso es similar al usado por **Qik (Banco Popular)**, **AZUL**, y otros servicios financieros en RD.

#### Arquitectura del Flujo

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE VERIFICACIÓN BIOMÉTRICA                                 │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  PASO 1: Captura de Documento                                                       │
│  ┌─────────────────────────────────────────────────────────────────────────────┐   │
│  │  📄 CÉDULA FRENTE                    📄 CÉDULA REVERSO                      │   │
│  │  ┌───────────────────┐              ┌───────────────────┐                   │   │
│  │  │  ┌─────┐          │              │  ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ │                   │   │
│  │  │  │FOTO │ Nombre   │              │  ▓▓ MRZ Zone  ▓▓ │                   │   │
│  │  │  └─────┘ Apellido │              │  ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ │                   │   │
│  │  │  001-0000000-0    │              │  Firma            │                   │   │
│  │  │  Fecha Nac        │              │  ───────────      │                   │   │
│  │  └───────────────────┘              └───────────────────┘                   │   │
│  │  ✅ Detectar bordes automáticos                                              │   │
│  │  ✅ Validar calidad de imagen (blur, luz, recorte)                           │   │
│  │  ✅ Extraer datos OCR                                                        │   │
│  └─────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                     │
│  PASO 2: Liveness Detection + Selfie                                                │
│  ┌─────────────────────────────────────────────────────────────────────────────┐   │
│  │  🎥 CÁMARA FRONTAL EN TIEMPO REAL                                           │   │
│  │  ┌───────────────────────────────────────┐                                   │   │
│  │  │           ┌─────────────┐              │                                   │
│  │  │           │   😊        │              │                                   │
│  │  │           │   USER      │              │  "Gira la cabeza a la izquierda" │   │
│  │  │           │   FACE      │              │  "Ahora sonríe"                   │   │
│  │  │           └─────────────┘              │  "Parpadea 2 veces"               │   │
│  │  │         [ OVAL GUIDE ]                 │                                   │   │
│  │  └───────────────────────────────────────┘                                   │   │
│  │  ✅ Detección de vida (anti-spoofing)                                        │   │
│  │  ✅ Verificar que es persona real (no foto de foto)                          │   │
│  │  ✅ Capturar múltiples ángulos                                               │   │
│  └─────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                     │
│  PASO 3: Comparación Facial                                                         │
│  ┌─────────────────────────────────────────────────────────────────────────────┐   │
│  │  📄 Foto del Documento          🤳 Selfie Capturada                         │   │
│  │  ┌─────────────┐                 ┌─────────────┐                             │   │
│  │  │   ┌───┐     │     ═════>      │   ┌───┐     │                             │   │
│  │  │   │🧑│     │   COMPARAR       │   │🧑│     │                             │   │
│  │  │   └───┘     │   ════>         │   └───┘     │                             │   │
│  │  └─────────────┘                 └─────────────┘                             │   │
│  │           │                             │                                     │   │
│  │           └─────────────┬───────────────┘                                     │   │
│  │                         ▼                                                     │   │
│  │              ┌──────────────────┐                                             │   │
│  │              │  MATCH SCORE     │                                             │   │
│  │              │  ✅ 94.5%        │  (Umbral mínimo: 80%)                       │   │
│  │              └──────────────────┘                                             │   │
│  └─────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                     │
│  PASO 4: Validación de Documento                                                    │
│  ┌─────────────────────────────────────────────────────────────────────────────┐   │
│  │  ✅ Formato de cédula válido (001-0000000-0)                                 │   │
│  │  ✅ Dígito verificador correcto                                              │   │
│  │  ✅ Fecha de nacimiento coherente (>18 años)                                 │   │
│  │  ✅ Documento no expirado                                                    │   │
│  │  ⚠️  Opcional: Validar contra JCE (si API disponible)                       │   │
│  │  ⚠️  Opcional: Verificar contra listas de fraude internas                   │   │
│  └─────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                     │
│  RESULTADO FINAL:                                                                   │
│  ┌─────────────────────────────────────────────────────────────────────────────┐   │
│  │  🟢 VERIFICACIÓN EXITOSA                                                     │   │
│  │  ─────────────────────────                                                   │   │
│  │  • Identidad confirmada con 94.5% de confianza                               │   │
│  │  • Documento auténtico                                                       │   │
│  │  • Liveness detection: PASSED                                                │   │
│  │  • Status KYC: PendingReview (para aprobación final)                         │   │
│  │                                                                              │   │
│  │  🔴 VERIFICACIÓN FALLIDA                                                     │   │
│  │  ─────────────────────────                                                   │   │
│  │  • Razón: "La foto no coincide con el documento" / "Documento borroso"       │   │
│  │  • Intentos restantes: 2                                                     │   │
│  │  • Acción: Reintentar o contactar soporte                                    │   │
│  └─────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

#### Nuevos Endpoints Requeridos

| Método | Endpoint                                     | Descripción                       | Auth |
| ------ | -------------------------------------------- | --------------------------------- | ---- |
| `POST` | `/api/kyc/identity-verification/start`       | Iniciar sesión de verificación    | ✅   |
| `POST` | `/api/kyc/identity-verification/document`    | Subir foto de documento           | ✅   |
| `POST` | `/api/kyc/identity-verification/selfie`      | Subir selfie con liveness         | ✅   |
| `POST` | `/api/kyc/identity-verification/complete`    | Completar y procesar verificación | ✅   |
| `GET`  | `/api/kyc/identity-verification/{sessionId}` | Obtener estado de la verificación | ✅   |
| `POST` | `/api/kyc/identity-verification/retry`       | Reintentar verificación fallida   | ✅   |

#### Nuevos Enums

```csharp
public enum VerificationSessionStatus
{
    Started = 1,              // Sesión iniciada
    DocumentFrontCaptured = 2, // Frente del documento capturado
    DocumentBackCaptured = 3,  // Reverso capturado
    DocumentProcessing = 4,    // Procesando OCR
    AwaitingSelfie = 5,        // Esperando selfie
    SelfieCaptured = 6,        // Selfie capturada
    ProcessingBiometrics = 7,  // Procesando comparación facial
    Completed = 8,             // Verificación completada exitosamente
    Failed = 9,                // Verificación fallida
    Expired = 10               // Sesión expirada (30 min timeout)
}

public enum LivenessChallenge
{
    Blink = 1,          // Parpadear
    Smile = 2,          // Sonreír
    TurnLeft = 3,       // Girar cabeza izquierda
    TurnRight = 4,      // Girar cabeza derecha
    Nod = 5,            // Asentir
    OpenMouth = 6       // Abrir boca
}

public enum DocumentSide
{
    Front = 1,
    Back = 2
}

public enum VerificationFailureReason
{
    None = 0,
    DocumentBlurry = 1,           // Documento borroso
    DocumentCutOff = 2,           // Documento cortado
    DocumentGlare = 3,            // Reflejo en documento
    DocumentExpired = 4,          // Documento expirado
    DocumentFake = 5,             // Documento falso detectado
    FaceNotDetected = 6,          // No se detectó rostro
    FaceMismatch = 7,             // Rostro no coincide
    LivenessCheckFailed = 8,      // Falló detección de vida
    MultipleAttemptsFailed = 9,   // Múltiples intentos fallidos
    SessionExpired = 10,          // Sesión expirada
    OCRFailed = 11,               // Error en extracción OCR
    InvalidDocumentNumber = 12    // Número de documento inválido
}
```

#### Nueva Entidad: IdentityVerificationSession

```csharp
public class IdentityVerificationSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? KYCProfileId { get; set; }

    // Estado de la sesión
    public VerificationSessionStatus Status { get; set; } = VerificationSessionStatus.Started;
    public VerificationFailureReason? FailureReason { get; set; }
    public string? FailureDetails { get; set; }

    // Documento
    public DocumentType DocumentType { get; set; } = DocumentType.Cedula;
    public string? DocumentFrontUrl { get; set; }
    public string? DocumentBackUrl { get; set; }
    public bool DocumentFrontProcessed { get; set; }
    public bool DocumentBackProcessed { get; set; }

    // Datos OCR extraídos
    public string? ExtractedFullName { get; set; }
    public string? ExtractedDocumentNumber { get; set; }
    public DateTime? ExtractedDateOfBirth { get; set; }
    public DateTime? ExtractedExpiryDate { get; set; }
    public string? ExtractedNationality { get; set; }
    public string? ExtractedGender { get; set; }
    public string? ExtractedAddress { get; set; }

    // Selfie y biometría
    public string? SelfieUrl { get; set; }
    public List<LivenessChallenge> LivenessChallenges { get; set; } = new();
    public bool LivenessCheckPassed { get; set; }
    public decimal? LivenessScore { get; set; } // 0-100

    // Comparación facial
    public decimal? FaceMatchScore { get; set; } // 0-100
    public bool FaceMatchPassed { get; set; }
    public decimal FaceMatchThreshold { get; set; } = 80.0m; // 80% mínimo

    // Validación de documento
    public bool DocumentValidationPassed { get; set; }
    public List<string> DocumentValidationErrors { get; set; } = new();

    // Intentos y límites
    public int AttemptNumber { get; set; } = 1;
    public int MaxAttempts { get; set; } = 3;

    // Metadatos del dispositivo
    public string? DeviceInfo { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DocumentCapturedAt { get; set; }
    public DateTime? SelfieCapturedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime ExpiresAt { get; set; } // 30 minutos desde creación

    // Provider de verificación externo (si se usa)
    public string? ExternalProvider { get; set; } // "Azure", "AWS", "Jumio", "Onfido"
    public string? ExternalSessionId { get; set; }
    public string? ExternalResponse { get; set; } // JSON raw response
}
```

#### Flujo Detallado del Proceso

| Paso | Acción del Usuario                   | Sistema                       | Validación                           |
| ---- | ------------------------------------ | ----------------------------- | ------------------------------------ |
| 1    | Click "Verificar Identidad"          | POST /start → Crear sesión    | Usuario autenticado                  |
| 2    | Permite acceso a cámara              | Frontend solicita permisos    | Camera permission granted            |
| 3    | Captura frente del documento         | Detectar bordes, auto-crop    | Imagen clara, 4 esquinas visibles    |
| 4    | POST imagen frente                   | OCR + validación inicial      | Formato cédula válido                |
| 5    | Captura reverso del documento        | Detectar bordes, auto-crop    | MRZ legible (si aplica)              |
| 6    | POST imagen reverso                  | OCR + extraer datos completos | Datos coherentes con frente          |
| 7    | Liveness: "Gira la cabeza izquierda" | Detectar movimiento           | Movimiento natural detectado         |
| 8    | Liveness: "Sonríe"                   | Detectar expresión            | Cambio facial detectado              |
| 9    | Liveness: "Parpadea 2 veces"         | Detectar parpadeos            | Anti-spoofing passed                 |
| 10   | Captura selfie final                 | Imagen clara del rostro       | Un solo rostro, bien iluminado       |
| 11   | POST selfie                          | Comparación facial            | Match score >= 80%                   |
| 12   | POST /complete                       | Consolidar resultados         | Todas las validaciones pasaron       |
| 13   | Actualizar KYCProfile                | Status = PendingReview        | Agregar documentos al perfil         |
| 14   | Notificar usuario                    | Push + Email                  | "Verificación enviada para revisión" |
| 15   | Notificar compliance                 | Dashboard + Email             | Nueva verificación pendiente         |

#### Request: Iniciar Verificación

```json
POST /api/kyc/identity-verification/start
{
  "documentType": "Cedula",
  "deviceInfo": {
    "platform": "iOS",
    "version": "17.2",
    "model": "iPhone 15",
    "appVersion": "1.2.0"
  },
  "location": {
    "latitude": 18.4861,
    "longitude": -69.9312
  }
}
```

#### Response: Sesión Iniciada

```json
{
  "sessionId": "uuid-session-id",
  "status": "Started",
  "documentType": "Cedula",
  "expiresAt": "2026-01-21T10:30:00Z",
  "expiresInSeconds": 1800,
  "nextStep": "CAPTURE_DOCUMENT_FRONT",
  "instructions": {
    "title": "Captura el frente de tu cédula",
    "steps": [
      "Coloca tu cédula sobre una superficie plana",
      "Asegúrate de que haya buena iluminación",
      "Alinea el documento dentro del marco",
      "Mantén la cámara estable"
    ],
    "tips": [
      "Evita reflejos y sombras",
      "Asegúrate que las 4 esquinas sean visibles",
      "El texto debe ser legible"
    ]
  },
  "requiredChallenges": ["TurnLeft", "Smile", "Blink"]
}
```

#### Request: Subir Documento

```json
POST /api/kyc/identity-verification/document
Content-Type: multipart/form-data

sessionId: uuid-session-id
side: Front
image: [binary - base64 o file upload]
```

#### Response: Documento Procesado

```json
{
  "sessionId": "uuid-session-id",
  "side": "Front",
  "status": "DocumentFrontCaptured",
  "ocrResult": {
    "success": true,
    "extractedData": {
      "fullName": "JUAN ANTONIO PEREZ MARTINEZ",
      "documentNumber": "001-1234567-8",
      "dateOfBirth": "1985-06-15",
      "expiryDate": "2028-06-15",
      "nationality": "DOMINICANA"
    },
    "confidence": 0.95
  },
  "documentValidation": {
    "formatValid": true,
    "checksumValid": true,
    "notExpired": true,
    "issues": []
  },
  "nextStep": "CAPTURE_DOCUMENT_BACK",
  "instructions": {
    "title": "Ahora captura el reverso de tu cédula",
    "steps": [
      "Voltea tu cédula",
      "Captura el reverso siguiendo las mismas instrucciones"
    ]
  }
}
```

#### Request: Selfie con Liveness

```json
POST /api/kyc/identity-verification/selfie
Content-Type: multipart/form-data

sessionId: uuid-session-id
selfieImage: [binary]
livenessData: {
  "challenges": [
    {"type": "TurnLeft", "passed": true, "timestamp": "2026-01-21T10:15:00Z"},
    {"type": "Smile", "passed": true, "timestamp": "2026-01-21T10:15:05Z"},
    {"type": "Blink", "passed": true, "timestamp": "2026-01-21T10:15:10Z"}
  ],
  "videoFrames": ["base64...", "base64...", "base64..."],  // Opcional: frames del video
  "deviceGyroscope": {...}  // Datos de movimiento del dispositivo
}
```

#### Response: Verificación Completada

```json
{
  "sessionId": "uuid-session-id",
  "status": "Completed",
  "result": {
    "verified": true,
    "overallScore": 92.5,
    "details": {
      "documentAuthenticity": {
        "passed": true,
        "score": 95.0,
        "checks": ["format", "checksum", "expiry", "tampering"]
      },
      "livenessDetection": {
        "passed": true,
        "score": 88.0,
        "challengesPassed": 3,
        "challengesTotal": 3
      },
      "faceMatch": {
        "passed": true,
        "score": 94.5,
        "threshold": 80.0
      },
      "ocrAccuracy": {
        "confidence": 95.0,
        "fieldsExtracted": 6,
        "fieldsTotal": 6
      }
    }
  },
  "extractedProfile": {
    "fullName": "JUAN ANTONIO PEREZ MARTINEZ",
    "documentNumber": "001-1234567-8",
    "documentType": "Cedula",
    "dateOfBirth": "1985-06-15",
    "nationality": "DOMINICANA",
    "gender": "M",
    "address": "CALLE PRINCIPAL #123, SANTO DOMINGO"
  },
  "kycProfileId": "uuid-kyc-profile",
  "kycStatus": "PendingReview",
  "message": "¡Verificación exitosa! Tu identidad ha sido confirmada y está pendiente de revisión final."
}
```

#### Response: Verificación Fallida

```json
{
  "sessionId": "uuid-session-id",
  "status": "Failed",
  "result": {
    "verified": false,
    "failureReason": "FaceMismatch",
    "failureDetails": "La foto de la selfie no coincide con la foto del documento de identidad.",
    "scores": {
      "faceMatch": {
        "score": 45.2,
        "threshold": 80.0,
        "passed": false
      }
    }
  },
  "attemptsRemaining": 2,
  "canRetry": true,
  "retryInstructions": {
    "title": "La verificación no fue exitosa",
    "reason": "El rostro capturado no coincide con el documento",
    "suggestions": [
      "Asegúrate de que la foto del documento sea clara",
      "Toma la selfie con buena iluminación",
      "Mira directamente a la cámara",
      "No uses lentes de sol o accesorios que cubran tu rostro"
    ]
  },
  "supportContact": {
    "email": "soporte@okla.com.do",
    "phone": "+1 809-555-0000",
    "whatsapp": "+1 809-555-0001"
  }
}
```

#### Integración con Proveedores Externos

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                     OPCIONES DE PROVEEDORES DE VERIFICACIÓN                  │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  OPCIÓN 1: Azure AI Services (Recomendado para MVP)                          │
│  ───────────────────────────────────────────────────                          │
│  • Azure Computer Vision - OCR de documentos                                 │
│  • Azure Face API - Comparación facial + liveness                            │
│  • Costo: ~$0.001 por transacción                                            │
│  • Latencia: <2 segundos                                                     │
│                                                                              │
│  OPCIÓN 2: AWS Rekognition                                                   │
│  ─────────────────────────                                                   │
│  • Amazon Textract - OCR de documentos                                       │
│  • Amazon Rekognition - Face compare + liveness                              │
│  • Costo: ~$0.001 por imagen                                                 │
│                                                                              │
│  OPCIÓN 3: Jumio (Enterprise - Mayor precisión)                              │
│  ──────────────────────────────────────────────                              │
│  • Especializado en verificación de identidad                                │
│  • Soporte para documentos dominicanos                                       │
│  • Costo: ~$2-5 por verificación                                             │
│  • Cumplimiento regulatorio certificado                                      │
│                                                                              │
│  OPCIÓN 4: Onfido (Similar a Jumio)                                          │
│  ─────────────────────────────────                                           │
│  • SDK móvil robusto                                                         │
│  • Detección de documentos latinoamericanos                                  │
│  • Costo: ~$2-4 por verificación                                             │
│                                                                              │
│  OPCIÓN 5: Híbrido (Recomendado para Producción)                             │
│  ───────────────────────────────────────────────                             │
│  • OCR interno + Azure/AWS para liveness                                     │
│  • Validación de cédula contra formato JCE                                   │
│  • Menor costo, control total de datos                                       │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

#### Configuración del Servicio

```json
{
  "IdentityVerification": {
    "Enabled": true,
    "Provider": "Azure",
    "SessionTimeoutMinutes": 30,
    "MaxAttempts": 3,
    "CooldownMinutesAfterMaxAttempts": 1440,

    "FaceMatch": {
      "MinimumScore": 80.0,
      "HighConfidenceScore": 95.0
    },

    "Liveness": {
      "Enabled": true,
      "ChallengesRequired": 3,
      "AvailableChallenges": ["Blink", "Smile", "TurnLeft", "TurnRight"],
      "MinimumScore": 70.0
    },

    "Document": {
      "AllowedTypes": ["Cedula", "Passport"],
      "RequireBothSides": true,
      "OCRConfidenceThreshold": 0.8,
      "MaxFileSizeMB": 10,
      "AllowedFormats": ["jpg", "jpeg", "png", "heic"]
    },

    "CedulaValidation": {
      "ValidateChecksum": true,
      "ValidateFormat": true,
      "ValidateExpiry": true,
      "MinimumAge": 18,
      "JCEIntegrationEnabled": false
    },

    "Azure": {
      "Endpoint": "https://okla-vision.cognitiveservices.azure.com/",
      "FaceEndpoint": "https://okla-face.cognitiveservices.azure.com/",
      "ApiKeySecretName": "azure-cognitive-key"
    }
  }
}
```

#### Eventos RabbitMQ

| Evento                                | Exchange     | Descripción                   |
| ------------------------------------- | ------------ | ----------------------------- |
| `kyc.identity.verification.started`   | `kyc.events` | Sesión de verificación creada |
| `kyc.identity.verification.completed` | `kyc.events` | Verificación exitosa          |
| `kyc.identity.verification.failed`    | `kyc.events` | Verificación fallida          |
| `kyc.identity.document.captured`      | `kyc.events` | Documento capturado           |
| `kyc.identity.selfie.captured`        | `kyc.events` | Selfie capturada              |
| `kyc.identity.liveness.passed`        | `kyc.events` | Liveness detection pasó       |
| `kyc.identity.face.matched`           | `kyc.events` | Comparación facial exitosa    |

#### Métricas Prometheus

```yaml
# Sesiones de verificación
kyc_identity_sessions_total{status="started|completed|failed|expired"}
kyc_identity_sessions_duration_seconds_histogram

# Comparación facial
kyc_face_match_score_histogram
kyc_face_match_result{result="passed|failed"}

# Liveness
kyc_liveness_score_histogram
kyc_liveness_challenges_passed_total{challenge="blink|smile|turn"}

# OCR
kyc_ocr_confidence_histogram
kyc_ocr_fields_extracted_total{field="name|document|dob|expiry"}

# Errores
kyc_verification_errors_total{reason="blur|mismatch|liveness|expired"}

# Tiempos
kyc_document_processing_seconds_histogram
kyc_selfie_processing_seconds_histogram
kyc_total_verification_seconds_histogram
```

---

### 4.1 KYC-PROF-001: Crear Perfil KYC

| Campo       | Valor                    |
| ----------- | ------------------------ |
| **ID**      | KYC-PROF-001             |
| **Nombre**  | Iniciar Verificación KYC |
| **Actor**   | Usuario registrado       |
| **Trigger** | POST /api/kycprofiles    |

#### Flujo del Proceso

| Paso | Acción                     | Sistema           | Validación              |
| ---- | -------------------------- | ----------------- | ----------------------- |
| 1    | Usuario inicia KYC         | Frontend          | Usuario autenticado     |
| 2    | Verificar perfil no existe | KYCService        | UserId único            |
| 3    | Validar datos personales   | KYCService        | Formato cédula RD       |
| 4    | Verificar cédula en JCE    | External API      | Opcional, si disponible |
| 5    | Calcular RiskScore inicial | KYCService        | Algoritmo interno       |
| 6    | Verificar lista PEP        | ComplianceService | Contra base datos UAF   |
| 7    | Verificar sanciones        | ComplianceService | OFAC, UN, EU lists      |
| 8    | Crear perfil               | Database          | Status = InProgress     |
| 9    | Publicar evento            | RabbitMQ          | KYCProfileCreated       |

#### Request

```json
{
  "userId": "uuid",
  "firstName": "Juan",
  "lastName": "Pérez",
  "documentNumber": "001-0000000-0",
  "documentType": "Cedula",
  "dateOfBirth": "1985-06-15",
  "nationality": "Dominicana",
  "address": "Calle Principal #123",
  "city": "Santo Domingo",
  "province": "Distrito Nacional",
  "phoneNumber": "+1 809-555-1234",
  "sourceOfFunds": "Salary",
  "occupation": "Engineer",
  "expectedMonthlyTransaction": 50000.0
}
```

#### Response

```json
{
  "id": "uuid",
  "userId": "uuid",
  "status": "InProgress",
  "riskLevel": "Low",
  "riskScore": 25.5,
  "isPEP": false,
  "requiredDocuments": ["Cedula", "UtilityBill", "SelfieWithDocument"],
  "createdAt": "2026-01-21T10:00:00Z"
}
```

---

### 4.2 KYC-DOC-001: Subir Documento KYC

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **ID**      | KYC-DOC-001                     |
| **Nombre**  | Subir Documento de Verificación |
| **Actor**   | Usuario con perfil KYC          |
| **Trigger** | POST /api/kycdocuments          |

#### Flujo del Proceso

| Paso | Acción                     | Sistema      | Validación            |
| ---- | -------------------------- | ------------ | --------------------- |
| 1    | Usuario sube documento     | Frontend     | Imagen/PDF            |
| 2    | Validar tipo de archivo    | KYCService   | jpg, png, pdf         |
| 3    | Validar tamaño             | KYCService   | Max 10MB              |
| 4    | Escanear malware           | MediaService | ClamAV                |
| 5    | Verificar calidad imagen   | KYCService   | Min 300 DPI           |
| 6    | OCR extracción datos       | KYCService   | Tesseract/Azure       |
| 7    | Validar datos vs perfil    | KYCService   | Nombre, cédula match  |
| 8    | Almacenar encriptado       | MediaService | S3 + encryption       |
| 9    | Actualizar perfil          | Database     | Documento agregado    |
| 10   | Verificar completitud      | KYCService   | Todos docs requeridos |
| 11   | Cambiar status si completo | Database     | PendingReview         |

#### Request (multipart/form-data)

```
profileId: uuid
documentType: Cedula
file: [binary]
side: Front  // Front, Back (para cédula)
```

---

### 4.3 KYC-REV-001: Aprobar Perfil KYC

| Campo       | Valor                              |
| ----------- | ---------------------------------- |
| **ID**      | KYC-REV-001                        |
| **Nombre**  | Aprobar Verificación KYC           |
| **Actor**   | Oficial de Compliance              |
| **Trigger** | POST /api/kycprofiles/{id}/approve |

#### Flujo del Proceso

| Paso | Acción                        | Sistema             | Validación                |
| ---- | ----------------------------- | ------------------- | ------------------------- |
| 1    | Compliance revisa perfil      | Dashboard           | Documentos visibles       |
| 2    | Verificar todos documentos OK | KYCService          | Cada doc verificado       |
| 3    | Revisar alertas PEP/Sanciones | ComplianceService   | Ninguna pendiente         |
| 4    | Aprobar perfil                | KYCService          | Con comentarios           |
| 5    | Calcular fecha expiración     | KYCService          | +1 año para Low risk      |
| 6    | Actualizar status             | Database            | Approved                  |
| 7    | Actualizar UserService        | HTTP                | user.IsKYCVerified = true |
| 8    | Publicar evento               | RabbitMQ            | KYCApproved               |
| 9    | Notificar usuario             | NotificationService | Email + Push              |

#### Request

```json
{
  "id": "uuid",
  "approvedBy": "compliance@okla.com.do",
  "comments": "All documents verified. Identity confirmed.",
  "expiresAt": "2027-01-21T00:00:00Z"
}
```

---

### 4.4 KYC-REV-002: Rechazar Perfil KYC

| Campo       | Valor                             |
| ----------- | --------------------------------- |
| **ID**      | KYC-REV-002                       |
| **Nombre**  | Rechazar Verificación KYC         |
| **Actor**   | Oficial de Compliance             |
| **Trigger** | POST /api/kycprofiles/{id}/reject |

#### Flujo del Proceso

| Paso | Acción                         | Sistema             | Validación               |
| ---- | ------------------------------ | ------------------- | ------------------------ |
| 1    | Compliance identifica problema | Dashboard           | Documento inválido, etc. |
| 2    | Seleccionar razón de rechazo   | Frontend            | Lista predefinida        |
| 3    | Agregar comentarios            | Frontend            | Detalles específicos     |
| 4    | Rechazar perfil                | KYCService          | Con razón obligatoria    |
| 5    | Actualizar status              | Database            | Rejected                 |
| 6    | Publicar evento                | RabbitMQ            | KYCRejected              |
| 7    | Notificar usuario              | NotificationService | Con razón y pasos        |

#### Request

```json
{
  "id": "uuid",
  "rejectionReason": "DocumentExpired",
  "comments": "La cédula presentada está expirada. Por favor suba un documento vigente.",
  "canRetry": true
}
```

---

### 4.5 KYC-MON-001: Monitoreo de Expiración

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **ID**      | KYC-MON-001                   |
| **Nombre**  | Monitoreo de KYC por Expirar  |
| **Actor**   | Sistema (Scheduled Job)       |
| **Trigger** | GET /api/kycprofiles/expiring |

#### Flujo del Proceso

| Paso | Acción                      | Sistema             | Validación                |
| ---- | --------------------------- | ------------------- | ------------------------- |
| 1    | Job diario ejecuta          | SchedulerService    | 6:00 AM                   |
| 2    | Buscar perfiles por expirar | KYCService          | ExpiresAt < Now + 30 días |
| 3    | Por cada perfil             | Loop                | Procesar                  |
| 4    | Enviar recordatorio         | NotificationService | Email + Push              |
| 5    | Si expirado                 | KYCService          | Status = Expired          |
| 6    | Restringir funcionalidades  | UserService         | Limitar transacciones     |
| 7    | Generar reporte             | ReportingService    | Para compliance           |

---

## 5. Reglas de Negocio

### 5.1 Documentos Requeridos por Tipo de Usuario

| Tipo Usuario           | Documentos Requeridos                                                         |
| ---------------------- | ----------------------------------------------------------------------------- |
| Individual (Comprador) | Cédula, UtilityBill                                                           |
| Individual (Vendedor)  | Cédula, UtilityBill, SelfieWithDocument                                       |
| Dealer                 | RNC, MercantileRegistry, BusinessLicense, TaxCertificate, Cédula (rep. legal) |

### 5.2 Cálculo de RiskScore

| Factor                  | Peso | Descripción                    |
| ----------------------- | ---- | ------------------------------ |
| Nacionalidad            | 20%  | RD=bajo, otros=variable        |
| Ocupación               | 15%  | Alto riesgo: cambista, casino  |
| PEP                     | 25%  | +50 puntos si es PEP           |
| Fuente de fondos        | 20%  | Salary=bajo, Investments=medio |
| Transacciones esperadas | 20%  | >$100K/mes = alto              |

### 5.3 Vigencia KYC

| Risk Level | Vigencia | Renovación    |
| ---------- | -------- | ------------- |
| Low        | 2 años   | 30 días antes |
| Medium     | 1 año    | 45 días antes |
| High       | 6 meses  | 60 días antes |
| Critical   | 3 meses  | 90 días antes |

### 5.4 Límites por Estado KYC

| KYC Status      | Límite Transacción | Funcionalidades |
| --------------- | ------------------ | --------------- |
| NotStarted      | $0                 | Solo navegación |
| InProgress      | $0                 | Solo navegación |
| Approved (Low)  | $500,000/mes       | Todas           |
| Approved (High) | $100,000/mes       | Con monitoreo   |
| Expired         | $0                 | Bloqueado       |

---

## 6. Manejo de Errores

| Código | Error           | Mensaje                        | Acción                |
| ------ | --------------- | ------------------------------ | --------------------- |
| 400    | InvalidCedula   | "Formato de cédula inválido"   | Verificar formato     |
| 400    | DocumentExpired | "El documento está expirado"   | Subir vigente         |
| 400    | LowQualityImage | "Imagen de baja calidad"       | Tomar mejor foto      |
| 400    | DataMismatch    | "Los datos no coinciden"       | Verificar información |
| 404    | ProfileNotFound | "Perfil KYC no encontrado"     | Crear perfil primero  |
| 409    | ProfileExists   | "Ya existe un perfil KYC"      | Usar existente        |
| 409    | DocumentExists  | "Este documento ya fue subido" | No duplicar           |

---

## 7. Eventos RabbitMQ

| Evento                  | Exchange     | Descripción          | Payload                         |
| ----------------------- | ------------ | -------------------- | ------------------------------- |
| `kyc.profile.created`   | `kyc.events` | Perfil creado        | `{ profileId, userId, status }` |
| `kyc.profile.updated`   | `kyc.events` | Perfil actualizado   | `{ profileId, changes }`        |
| `kyc.profile.approved`  | `kyc.events` | Perfil aprobado      | `{ profileId, approvedBy }`     |
| `kyc.profile.rejected`  | `kyc.events` | Perfil rechazado     | `{ profileId, reason }`         |
| `kyc.profile.expired`   | `kyc.events` | Perfil expirado      | `{ profileId, expiresAt }`      |
| `kyc.document.uploaded` | `kyc.events` | Documento subido     | `{ docId, type, profileId }`    |
| `kyc.document.verified` | `kyc.events` | Documento verificado | `{ docId, verifiedBy }`         |
| `kyc.pep.detected`      | `kyc.events` | PEP detectado        | `{ profileId, pepInfo }`        |

---

## 8. Integración con Compliance (Ley 155-17)

### 8.1 Verificaciones Automáticas

```
┌──────────────┐     ┌──────────────┐     ┌───────────────┐
│  KYCService  │────>│ Compliance   │────>│  UAF Listas   │
│              │     │   Service    │     │  PEP/Sanciones│
└──────────────┘     └──────────────┘     └───────────────┘
       │                    │                     │
       │                    ▼                     │
       │            ┌──────────────┐              │
       │            │    OFAC      │<─────────────┤
       │            │  Sanctions   │              │
       │            └──────────────┘              │
       │                                          │
       ▼                                          ▼
┌──────────────┐                         ┌───────────────┐
│  JCE (Cédula)│                         │  World Check  │
│  Validation  │                         │  (optional)   │
└──────────────┘                         └───────────────┘
```

### 8.2 Reportes UAF Requeridos

| Reporte              | Frecuencia | Contenido               |
| -------------------- | ---------- | ----------------------- |
| ROS                  | Inmediato  | Operaciones sospechosas |
| Transacciones > $10K | Mensual    | Todas las transacciones |
| PEP Activos          | Trimestral | Lista de PEPs           |
| Estadísticas KYC     | Mensual    | Aprobados/Rechazados    |

---

## 9. Métricas y Dashboard

### 9.1 KPIs Principales

```
# Perfiles por estado
kyc_profiles_by_status{status="approved|pending|rejected"}

# Tiempo promedio de aprobación
kyc_approval_time_seconds_avg

# Documentos procesados
kyc_documents_processed_total{type="cedula|passport"}

# PEPs detectados
kyc_pep_detected_total

# Verificaciones por día
kyc_verifications_daily
```

### 9.2 Alertas

| Alerta           | Condición           | Severidad |
| ---------------- | ------------------- | --------- |
| HighPendingQueue | >50 pendientes      | Warning   |
| PEPDetected      | Nuevo PEP           | Critical  |
| SanctionMatch    | Match en OFAC       | Critical  |
| ExpiringSoon     | >20 por expirar hoy | Warning   |

---

## 10. Configuración

### 10.1 appsettings.json

```json
{
  "KYC": {
    "ExpirationDays": {
      "Low": 730,
      "Medium": 365,
      "High": 180,
      "Critical": 90
    },
    "ReminderDays": [30, 14, 7, 1],
    "MaxDocumentSizeMB": 10,
    "AllowedFileTypes": ["jpg", "jpeg", "png", "pdf"],
    "OCREnabled": true,
    "AutoApprovalEnabled": false
  },
  "Compliance": {
    "PEPCheckEnabled": true,
    "OFACCheckEnabled": true,
    "JCEValidationEnabled": false
  }
}
```

---

## 📚 Referencias

- [Ley 155-17](https://uaf.gob.do/ley-155-17/) - Prevención Lavado de Activos
- [01-compliance-service.md](../08-COMPLIANCE-LEGAL-RD/01-compliance-service.md) - Compliance general
- [OFAC Sanctions Lists](https://sanctionssearch.ofac.treas.gov/)

---

## 11. 🆕 UI/UX para Verificación Biométrica (Estilo Qik)

### 11.1 Componentes Frontend Requeridos

#### Estructura de Archivos

```
frontend/web/src/
├── pages/
│   └── kyc/
│       ├── IdentityVerificationPage.tsx      # Página principal de verificación
│       ├── VerificationSuccessPage.tsx       # Página de éxito
│       └── VerificationFailedPage.tsx        # Página de error
├── components/
│   └── kyc/
│       ├── DocumentCapture.tsx               # Captura de documento
│       ├── DocumentPreview.tsx               # Preview con overlay
│       ├── SelfieCapture.tsx                 # Captura de selfie
│       ├── LivenessChallenge.tsx             # Retos de liveness
│       ├── FaceOvalGuide.tsx                 # Guía oval para rostro
│       ├── VerificationProgress.tsx          # Progress stepper
│       ├── VerificationResult.tsx            # Resultado final
│       └── CameraPermissionRequest.tsx       # Solicitar permisos
├── hooks/
│   └── useCamera.ts                          # Hook para cámara
│   └── useIdentityVerification.ts            # Hook para API de verificación
└── services/
    └── identityVerificationService.ts        # API client
```

### 11.2 Flujo de Pantallas

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                           FLUJO DE VERIFICACIÓN - UI                                │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  PANTALLA 1: Introducción                                                           │
│  ┌───────────────────────────────────────────────────────────────────────────────┐ │
│  │  ┌─────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                         🛡️ Verifica tu Identidad                        │  │ │
│  │  │                                                                         │  │ │
│  │  │     Para tu seguridad, necesitamos verificar que eres tú.               │  │ │
│  │  │                                                                         │  │ │
│  │  │     Necesitarás:                                                        │  │ │
│  │  │     ✓ Tu cédula de identidad vigente                                    │  │ │
│  │  │     ✓ Acceso a la cámara de tu dispositivo                              │  │ │
│  │  │     ✓ 3-5 minutos de tu tiempo                                          │  │ │
│  │  │                                                                         │  │ │
│  │  │     [ Comenzar Verificación ]                                           │  │ │
│  │  │                                                                         │  │ │
│  │  │     Este proceso es seguro y cumple con la Ley 155-17                   │  │ │
│  │  └─────────────────────────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  PANTALLA 2: Captura Frente de Cédula                                               │
│  ┌───────────────────────────────────────────────────────────────────────────────┐ │
│  │  ┌─────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │  PASO 1/4: Frente de tu Cédula                                          │  │ │
│  │  │  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━              │  │ │
│  │  │                                                                         │  │ │
│  │  │  ┌─────────────────────────────────────────────────────────────────┐    │  │ │
│  │  │  │ ┌───────────────────────────────────────────────────────────┐   │    │  │ │
│  │  │  │ │                                                           │   │    │  │ │
│  │  │  │ │          [ VISTA DE CÁMARA EN VIVO ]                      │   │    │  │ │
│  │  │  │ │                                                           │   │    │  │ │
│  │  │  │ │    ┌─────────────────────────────────────────────┐        │   │    │  │ │
│  │  │  │ │    │         MARCO DE DETECCIÓN                   │        │   │    │  │ │
│  │  │  │ │    │         (se pone verde cuando detecta)       │        │   │    │  │ │
│  │  │  │ │    └─────────────────────────────────────────────┘        │   │    │  │ │
│  │  │  │ │                                                           │   │    │  │ │
│  │  │  │ └───────────────────────────────────────────────────────────┘   │    │  │ │
│  │  │  └─────────────────────────────────────────────────────────────────┘    │  │ │
│  │  │                                                                         │  │ │
│  │  │  💡 Consejos:                                                           │  │ │
│  │  │  • Coloca la cédula en una superficie plana                             │  │ │
│  │  │  • Asegúrate de buena iluminación                                       │  │ │
│  │  │  • Evita sombras y reflejos                                             │  │ │
│  │  │                                                                         │  │ │
│  │  │  [ ⏺ Capturar Foto ]                                                   │  │ │
│  │  └─────────────────────────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  PANTALLA 3: Captura Reverso de Cédula                                              │
│  ┌───────────────────────────────────────────────────────────────────────────────┐ │
│  │  ┌─────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │  PASO 2/4: Reverso de tu Cédula                                         │  │ │
│  │  │  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━              │  │ │
│  │  │                                                                         │  │ │
│  │  │  ┌─────────────────────────────────────────────────────────────────┐    │  │ │
│  │  │  │              [ VISTA DE CÁMARA EN VIVO ]                        │    │  │ │
│  │  │  │    ┌─────────────────────────────────────────────┐              │    │  │ │
│  │  │  │    │         Voltea tu cédula                     │              │    │  │ │
│  │  │  │    │         y alinea el reverso                  │              │    │  │ │
│  │  │  │    └─────────────────────────────────────────────┘              │    │  │ │
│  │  │  └─────────────────────────────────────────────────────────────────┘    │  │ │
│  │  │                                                                         │  │ │
│  │  │  ✅ Frente capturado correctamente                                      │  │ │
│  │  │                                                                         │  │ │
│  │  │  [ ⏺ Capturar Reverso ]                                                │  │ │
│  │  └─────────────────────────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  PANTALLA 4: Verificación de Vida (Liveness)                                        │
│  ┌───────────────────────────────────────────────────────────────────────────────┐ │
│  │  ┌─────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │  PASO 3/4: Verificación de Identidad en Vivo                            │  │ │
│  │  │  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━              │  │ │
│  │  │                                                                         │  │ │
│  │  │  ┌─────────────────────────────────────────────────────────────────┐    │  │ │
│  │  │  │                     ┌───────────┐                                │    │  │ │
│  │  │  │                     │           │                                │    │  │ │
│  │  │  │                     │    😊     │  ← Tu rostro aquí              │    │  │ │
│  │  │  │                     │           │                                │    │  │ │
│  │  │  │                     └───────────┘                                │    │  │ │
│  │  │  │                     (  ÓVALO  )                                  │    │  │ │
│  │  │  │                                                                  │    │  │ │
│  │  │  └─────────────────────────────────────────────────────────────────┘    │  │ │
│  │  │                                                                         │  │ │
│  │  │  ┌─────────────────────────────────────────────────────────────────┐    │  │ │
│  │  │  │         👈 GIRA LA CABEZA A LA IZQUIERDA                        │    │  │ │
│  │  │  │                                                                  │    │  │ │
│  │  │  │         [ ███████████░░░░░░░░░ ] 65%                             │    │  │ │
│  │  │  └─────────────────────────────────────────────────────────────────┘    │  │ │
│  │  │                                                                         │  │ │
│  │  │  Reto 1/3: Gira lentamente la cabeza hacia la izquierda                 │  │ │
│  │  └─────────────────────────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  PANTALLA 5: Procesando                                                             │
│  ┌───────────────────────────────────────────────────────────────────────────────┐ │
│  │  ┌─────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                                                                         │  │ │
│  │  │                         ⏳ Procesando...                                │  │ │
│  │  │                                                                         │  │ │
│  │  │              ┌────────────────────────────────────┐                     │  │ │
│  │  │              │  ████████████████████░░░░░ 78%    │                     │  │ │
│  │  │              └────────────────────────────────────┘                     │  │ │
│  │  │                                                                         │  │ │
│  │  │              ✅ Documento analizado                                     │  │ │
│  │  │              ✅ Datos extraídos                                         │  │ │
│  │  │              ✅ Verificación de vida completada                         │  │ │
│  │  │              ⏳ Comparando rostros...                                   │  │ │
│  │  │              ⬜ Validando información                                   │  │ │
│  │  │                                                                         │  │ │
│  │  │              Esto solo tomará unos segundos...                          │  │ │
│  │  └─────────────────────────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  PANTALLA 6A: Éxito                                                                 │
│  ┌───────────────────────────────────────────────────────────────────────────────┐ │
│  │  ┌─────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                                                                         │  │ │
│  │  │                         ✅ ¡Verificación Exitosa!                       │  │ │
│  │  │                                                                         │  │ │
│  │  │              ┌────────────────────────────────────┐                     │  │ │
│  │  │              │        🎉                          │                     │  │ │
│  │  │              │                                    │                     │  │ │
│  │  │              │   Hola, Juan Pérez                 │                     │  │ │
│  │  │              │                                    │                     │  │ │
│  │  │              │   Tu identidad ha sido verificada  │                     │  │ │
│  │  │              │   con un 94.5% de confianza        │                     │  │ │
│  │  │              │                                    │                     │  │ │
│  │  │              └────────────────────────────────────┘                     │  │ │
│  │  │                                                                         │  │ │
│  │  │              📋 Datos verificados:                                      │  │ │
│  │  │              • Nombre: Juan Antonio Pérez Martínez                      │  │ │
│  │  │              • Cédula: 001-1234567-8                                    │  │ │
│  │  │              • Fecha Nac: 15/06/1985                                    │  │ │
│  │  │                                                                         │  │ │
│  │  │              Tu perfil está pendiente de revisión final.                │  │ │
│  │  │              Te notificaremos en 24-48 horas.                           │  │ │
│  │  │                                                                         │  │ │
│  │  │              [ Continuar al Dashboard ]                                 │  │ │
│  │  └─────────────────────────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
│  PANTALLA 6B: Error (Reintentar)                                                    │
│  ┌───────────────────────────────────────────────────────────────────────────────┐ │
│  │  ┌─────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                                                                         │  │ │
│  │  │                         ❌ Verificación No Exitosa                       │  │ │
│  │  │                                                                         │  │ │
│  │  │              ┌────────────────────────────────────┐                     │  │ │
│  │  │              │                                    │                     │  │ │
│  │  │              │   No pudimos verificar tu identidad │                     │  │ │
│  │  │              │                                    │                     │  │ │
│  │  │              │   Razón: La foto no coincide con   │                     │  │ │
│  │  │              │   el documento de identidad        │                     │  │ │
│  │  │              │                                    │                     │  │ │
│  │  │              └────────────────────────────────────┘                     │  │ │
│  │  │                                                                         │  │ │
│  │  │              💡 Sugerencias:                                            │  │ │
│  │  │              • Asegúrate que el documento sea tuyo                      │  │ │
│  │  │              • Mejora la iluminación                                    │  │ │
│  │  │              • Quita lentes o accesorios del rostro                     │  │ │
│  │  │              • Mira directamente a la cámara                            │  │ │
│  │  │                                                                         │  │ │
│  │  │              Intentos restantes: 2                                      │  │ │
│  │  │                                                                         │  │ │
│  │  │              [ 🔄 Intentar de Nuevo ]                                   │  │ │
│  │  │              [ 📞 Contactar Soporte ]                                   │  │ │
│  │  └─────────────────────────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### 11.3 Componentes React Principales

#### IdentityVerificationPage.tsx (Estructura)

```typescript
// Estados del wizard
type VerificationStep =
  | "intro" // Pantalla de introducción
  | "permission" // Solicitar permisos de cámara
  | "document-front" // Capturar frente de cédula
  | "document-back" // Capturar reverso
  | "liveness" // Prueba de vida
  | "selfie" // Capturar selfie final
  | "processing" // Procesando verificación
  | "success" // Verificación exitosa
  | "failed"; // Verificación fallida

interface VerificationState {
  sessionId: string | null;
  step: VerificationStep;
  documentFrontImage: string | null;
  documentBackImage: string | null;
  selfieImage: string | null;
  livenessCompleted: boolean;
  currentChallenge: LivenessChallenge | null;
  challengesPassed: number;
  error: string | null;
  attemptsRemaining: number;
  result: VerificationResult | null;
}
```

#### DocumentCapture.tsx (Estructura)

```typescript
interface DocumentCaptureProps {
  side: "front" | "back";
  onCapture: (imageData: string) => void;
  onError: (error: string) => void;
  documentType: "cedula" | "passport";
}

// Features:
// - Auto-detect document edges
// - Real-time quality feedback
// - Auto-capture when aligned
// - Manual capture button fallback
// - Overlay guide for document placement
```

#### LivenessChallenge.tsx (Estructura)

```typescript
interface LivenessChallengeProps {
  challenges: LivenessChallenge[];
  onComplete: (results: ChallengeResult[]) => void;
  onFail: (reason: string) => void;
}

interface ChallengeResult {
  type: LivenessChallenge;
  passed: boolean;
  timestamp: Date;
  confidence: number;
}

// Features:
// - Real-time face detection
// - Challenge instructions with animations
// - Progress indicator per challenge
// - Anti-spoofing detection
// - Face position feedback
```

### 11.4 Rutas Frontend

```typescript
// App.tsx
<Route path="/kyc/verify" element={<IdentityVerificationPage />} />
<Route path="/kyc/verify/success" element={<VerificationSuccessPage />} />
<Route path="/kyc/verify/failed" element={<VerificationFailedPage />} />
<Route path="/kyc/status" element={<KYCStatusPage />} />
```

### 11.5 Librerías Recomendadas para Frontend

| Librería                      | Uso                                | NPM                         |
| ----------------------------- | ---------------------------------- | --------------------------- |
| **react-webcam**              | Acceso a cámara                    | `react-webcam`              |
| **face-api.js**               | Detección facial en cliente        | `face-api.js`               |
| **tesseract.js**              | OCR en cliente (opcional)          | `tesseract.js`              |
| **framer-motion**             | Animaciones de UI                  | `framer-motion`             |
| **@mediapipe/face_detection** | Detección de rostro en tiempo real | `@mediapipe/face_detection` |

### 11.6 Consideraciones de Seguridad para UI

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                          SEGURIDAD EN FRONTEND                                      │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  1. NUNCA almacenar imágenes de documentos en localStorage/sessionStorage           │
│  2. Las imágenes deben enviarse directamente al backend                             │
│  3. Usar HTTPS para todas las comunicaciones                                        │
│  4. Implementar timeout de sesión (30 min)                                          │
│  5. Limpiar cámara y streams al salir del componente                                │
│  6. No mostrar datos sensibles en logs de consola                                   │
│  7. Implementar rate limiting en intentos                                           │
│  8. Capturar y reportar intentos de spoofing                                        │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 12. Validación de Cédula Dominicana

### 12.1 Formato de Cédula

```
Formato: XXX-XXXXXXX-X
         │   │       │
         │   │       └── Dígito verificador (0-9)
         │   └────────── Número único (7 dígitos)
         └────────────── Municipio (3 dígitos: 001-044)

Ejemplos válidos:
- 001-1234567-8
- 402-2345678-9
- 031-0000001-0
```

### 12.2 Algoritmo de Validación (Dígito Verificador)

```csharp
public static class CedulaValidator
{
    /// <summary>
    /// Valida el formato y dígito verificador de una cédula dominicana
    /// </summary>
    public static (bool isValid, string? error) ValidateCedula(string cedula)
    {
        // Remover guiones y espacios
        var cleaned = cedula.Replace("-", "").Replace(" ", "").Trim();

        // Validar longitud
        if (cleaned.Length != 11)
            return (false, "La cédula debe tener 11 dígitos");

        // Validar que solo contenga números
        if (!cleaned.All(char.IsDigit))
            return (false, "La cédula solo debe contener números");

        // Validar municipio (primeros 3 dígitos)
        var municipio = int.Parse(cleaned.Substring(0, 3));
        if (municipio < 1 || municipio > 44)
            return (false, "Código de municipio inválido");

        // Calcular dígito verificador
        int[] weights = { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };
        int sum = 0;

        for (int i = 0; i < 10; i++)
        {
            int digit = int.Parse(cleaned[i].ToString());
            int product = digit * weights[i];

            // Si el producto es >= 10, sumar sus dígitos
            sum += product >= 10 ? (product / 10) + (product % 10) : product;
        }

        // El dígito verificador es (10 - (suma mod 10)) mod 10
        int expectedCheckDigit = (10 - (sum % 10)) % 10;
        int actualCheckDigit = int.Parse(cleaned[10].ToString());

        if (expectedCheckDigit != actualCheckDigit)
            return (false, "Dígito verificador inválido");

        return (true, null);
    }

    /// <summary>
    /// Formatea una cédula al formato estándar XXX-XXXXXXX-X
    /// </summary>
    public static string FormatCedula(string cedula)
    {
        var cleaned = cedula.Replace("-", "").Replace(" ", "").Trim();
        if (cleaned.Length != 11) return cedula;

        return $"{cleaned.Substring(0, 3)}-{cleaned.Substring(3, 7)}-{cleaned.Substring(10, 1)}";
    }
}
```

### 12.3 Códigos de Municipio Válidos

| Código | Provincia/Municipio  |
| ------ | -------------------- |
| 001    | Distrito Nacional    |
| 002    | Santiago             |
| 003    | La Vega              |
| 004    | San Cristóbal        |
| 005    | Puerto Plata         |
| 006    | San Pedro de Macorís |
| 007    | Duarte               |
| 008    | La Romana            |
| 009    | Espaillat            |
| 010    | San Juan             |
| ...    | (continúa hasta 044) |

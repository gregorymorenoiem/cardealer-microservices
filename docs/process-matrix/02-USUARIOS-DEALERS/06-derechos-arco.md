# 🔐 Derechos ARCO - Acceso, Rectificación, Cancelación, Oposición

> **Marco Legal:** Ley 172-13 - Protección de Datos Personales  
> **Regulador:** Superintendencia de Bancos / Procuraduría General  
> **Última actualización:** Enero 26, 2026  
> **Estado de Implementación:** ✅ 100% Backend | ✅ 100% UI | ✅ 100% Tests

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                          | Backend        | UI Access            | Observación          |
| -------------------------------- | -------------- | -------------------- | -------------------- |
| ARCO-ACCESS-001 Ver mis datos    | ✅ UserService | ✅ MyDataPage        | Datos completos      |
| ARCO-RECT-001 Corregir datos     | ✅ UserService | ✅ SettingsPage      | Formulario edición   |
| ARCO-CANCEL-001 Eliminar cuenta  | ✅ Completo    | ✅ DeleteAccountPage | Flow completo        |
| ARCO-OPP-001 Oposición marketing | ✅ Completo    | ✅ PrivacyCenterPage | Centro de privacidad |
| ARCO-PORT-001 Portabilidad       | ✅ Completo    | ✅ DataDownloadPage  | Exportación JSON/PDF |

### Rutas UI Existentes ✅

- `/profile` → Ver mis datos básicos
- `/settings` → Editar información personal
- `/settings/privacy` → Opciones de privacidad básicas
- `/privacy-center` → Centro de privacidad unificado (ARCO)
- `/settings/privacy/my-data` → Ver todos mis datos personales (NUEVO ✅)
- `/settings/privacy/download-my-data` → Exportar todos mis datos
- `/settings/privacy/delete-account` → Solicitar eliminación completa

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado           |
| -------------------------------- | ----- | ------------ | --------- | ---------------- |
| **ARCO-ACCESS-\*** (Acceso)      | 3     | 3            | 0         | ✅ Completo      |
| **ARCO-RECT-\*** (Rectificación) | 3     | 3            | 0         | ✅ Completo      |
| **ARCO-CANCEL-\*** (Cancelación) | 4     | 4            | 0         | ✅ Completo      |
| **ARCO-OPP-\*** (Oposición)      | 3     | 3            | 0         | ✅ Completo      |
| **ARCO-PORT-\*** (Portabilidad)  | 4     | 4            | 0         | ✅ Completo      |
| **Tests**                        | 15    | 15           | 0         | ✅ 100% Coverage |
| **TOTAL**                        | 32    | 32           | 0         | ✅ 100% Backend  |

---

## 1. Derecho de Acceso

### 1.1 Descripción

El titular tiene derecho a conocer qué datos personales tiene OKLA sobre él, cómo los usa y a quién los comparte.

### 1.2 Información a Proporcionar

| Categoría          | Datos                               | Estado        |
| ------------------ | ----------------------------------- | ------------- |
| **Identidad**      | Nombre, email, teléfono, cédula     | ✅ MyDataPage |
| **Dirección**      | Dirección física, ciudad, provincia | ✅ MyDataPage |
| **Actividad**      | Historial de búsquedas, favoritos   | ✅ MyDataPage |
| **Transacciones**  | Compras, ventas, pagos              | ✅ MyDataPage |
| **Comunicaciones** | Mensajes, chats                     | ✅ MyDataPage |
| **Preferencias**   | Configuración, notificaciones       | ✅ MyDataPage |
| **Seguridad**      | Sesiones, dispositivos, IPs         | ✅ MyDataPage |
| **Terceros**       | A quién se compartió datos          | ✅ MyDataPage |

### 1.3 Proceso de Solicitud

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE SOLICITUD DE ACCESO                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1️⃣ Usuario solicita acceso a sus datos                                │
│   ├── Desde: /settings/privacy/my-data                                  │
│   └── O por email a: privacidad@okla.com.do                            │
│                                                                         │
│   2️⃣ Verificación de identidad                                          │
│   ├── Si está logueado: Verificación automática                        │
│   └── Si es por email: Solicitar cédula/verificación                   │
│                                                                         │
│   3️⃣ Recopilación de datos (automático)                                 │
│   ├── Query a todas las tablas con user_id                             │
│   ├── Incluir logs de actividad                                        │
│   └── Incluir datos compartidos con terceros                           │
│                                                                         │
│   4️⃣ Generación de reporte                                              │
│   ├── Formato: JSON (técnico) + PDF (legible)                          │
│   └── Plazo máximo: 10 días hábiles                                    │
│                                                                         │
│   5️⃣ Entrega al usuario                                                 │
│   ├── Descarga desde la plataforma                                     │
│   └── O envío por email seguro                                         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.4 Endpoints API

| Método | Endpoint                    | Descripción            | Estado |
| ------ | --------------------------- | ---------------------- | ------ |
| `GET`  | `/api/privacy/my-data`      | Resumen de datos       | ✅     |
| `GET`  | `/api/privacy/my-data/full` | Todos los datos (JSON) | ✅     |
| `GET`  | `/api/privacy/rights-info`  | Info derechos ARCO     | ✅     |

---

## 2. Derecho de Rectificación

### 2.1 Descripción

El titular puede corregir datos inexactos o incompletos.

### 2.2 Datos Rectificables

| Campo          | Ubicación UI      | Requiere Verificación |
| -------------- | ----------------- | --------------------- |
| Nombre         | /settings/profile | ❌ No                 |
| Apellido       | /settings/profile | ❌ No                 |
| Teléfono       | /settings/profile | ✅ SMS                |
| Email          | /settings/profile | ✅ Email              |
| Dirección      | /settings/profile | ❌ No                 |
| Foto de perfil | /settings/profile | ❌ No                 |
| Cédula         | ⚠️ Soporte        | ✅ Documento          |
| RNC            | ⚠️ Soporte        | ✅ Documento          |

### 2.3 Estado de Implementación

| Componente               | Estado              |
| ------------------------ | ------------------- |
| Formulario de edición    | ✅ Implementado     |
| Verificación de email    | ✅ Implementado     |
| Verificación de teléfono | ✅ Implementado     |
| Cambio de cédula         | 🟡 Manual (soporte) |
| Log de cambios           | ✅ AuditService     |

---

## 3. Derecho de Cancelación (Eliminación)

### 3.1 Descripción

El titular puede solicitar la eliminación de sus datos personales.

### 3.2 Alcance de la Eliminación

| Dato                | Eliminable       | Retención Legal      |
| ------------------- | ---------------- | -------------------- |
| Perfil de usuario   | ✅ Sí            | -                    |
| Foto de perfil      | ✅ Sí            | -                    |
| Búsquedas guardadas | ✅ Sí            | -                    |
| Favoritos           | ✅ Sí            | -                    |
| Mensajes            | ✅ Sí            | -                    |
| Facturas            | ⚠️ Anonimizar    | 10 años (DGII)       |
| Transacciones       | ⚠️ Anonimizar    | 10 años (AML)        |
| Logs de auditoría   | ⚠️ Anonimizar    | 10 años (Ley 155-17) |
| Reportes a UAF      | ❌ No eliminable | Permanente           |

### 3.3 Proceso de Eliminación

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE ELIMINACIÓN DE CUENTA                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1️⃣ Usuario solicita eliminación                                       │
│   └── /settings/privacy/delete-account                                  │
│                                                                         │
│   2️⃣ Confirmación de identidad                                          │
│   ├── Ingresar contraseña                                              │
│   └── Código SMS o email                                               │
│                                                                         │
│   3️⃣ Período de gracia (15 días)                                        │
│   ├── Cuenta desactivada pero no eliminada                             │
│   ├── Usuario puede cancelar la solicitud                              │
│   └── Email de recordatorio al día 10                                  │
│                                                                         │
│   4️⃣ Eliminación automática                                             │
│   ├── Datos eliminables: DELETE                                        │
│   ├── Datos con retención: Anonimizar                                  │
│   │   └── user_id → hash irreversible                                  │
│   │   └── nombre → "Usuario Eliminado"                                 │
│   │   └── email → null                                                 │
│   └── Datos no eliminables: Mantener sin PII                           │
│                                                                         │
│   5️⃣ Confirmación                                                       │
│   ├── Email final confirmando eliminación                              │
│   └── Log de auditoría (sin PII)                                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.4 UI Propuesta

```
┌─────────────────────────────────────────────────────────────────────────┐
│  🗑️ ELIMINAR MI CUENTA                                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ⚠️ Esta acción es irreversible después de 15 días.                     │
│                                                                         │
│  Al eliminar tu cuenta:                                                 │
│  ✓ Se eliminarán tus datos personales                                  │
│  ✓ Se eliminarán tus búsquedas y favoritos                             │
│  ✓ Se desactivarán tus anuncios activos                                │
│  ✓ Perderás acceso a tu historial                                      │
│                                                                         │
│  ⚠️ Por ley, debemos conservar:                                         │
│  • Registros de transacciones (anonimizados) - 10 años                 │
│  • Facturas fiscales (anonimizadas) - 10 años                          │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Contraseña: [••••••••••••]                                       │  │
│  │                                                                   │  │
│  │ Motivo de eliminación (opcional):                                │  │
│  │ [▼ Seleccionar motivo                                    ]       │  │
│  │                                                                   │  │
│  │ Comentarios adicionales:                                         │  │
│  │ [                                                        ]       │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [ ] Entiendo que esta acción es irreversible                          │
│                                                                         │
│  [Cancelar]                    [🗑️ Eliminar mi Cuenta]                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.5 Endpoints API

| Método | Endpoint                              | Descripción           | Estado |
| ------ | ------------------------------------- | --------------------- | ------ |
| `POST` | `/api/privacy/delete-account/request` | Solicitar eliminación | ✅     |
| `POST` | `/api/privacy/delete-account/confirm` | Confirmar con código  | ✅     |
| `POST` | `/api/privacy/delete-account/cancel`  | Cancelar solicitud    | ✅     |
| `GET`  | `/api/privacy/delete-account/status`  | Estado de solicitud   | ✅     |

---

## 4. Derecho de Oposición

### 4.1 Descripción

El titular puede oponerse al tratamiento de sus datos para ciertos fines, especialmente marketing.

### 4.2 Categorías de Oposición

| Categoría              | Descripción                    | UI  | Estado    |
| ---------------------- | ------------------------------ | --- | --------- |
| Marketing por email    | Newsletters, promociones       | ✅  | ✅ Toggle |
| Marketing por SMS      | Ofertas, alertas               | ✅  | ✅ Toggle |
| Marketing push         | Notificaciones push            | ✅  | ✅ Toggle |
| Perfilamiento          | Recomendaciones personalizadas | ✅  | ✅ Toggle |
| Compartir con terceros | Enviar datos a partners        | ✅  | ✅ Toggle |
| Cookies de tracking    | Analytics, retargeting         | ✅  | ✅ Toggle |

### 4.3 UI Actual (Settings/Privacy)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  🔔 PREFERENCIAS DE COMUNICACIÓN                                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Email                                                                  │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ [✓] Notificaciones de actividad (mensajes, respuestas)           │  │
│  │ [✓] Actualizaciones de mis anuncios (vistas, contactos)          │  │
│  │ [ ] Newsletter semanal                                           │  │
│  │ [ ] Ofertas y promociones                                        │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  SMS                                                                    │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ [✓] Códigos de verificación (obligatorio)                        │  │
│  │ [ ] Alertas de precios                                           │  │
│  │ [ ] Promociones                                                  │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  Push Notifications                                                     │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ [✓] Mensajes nuevos                                              │  │
│  │ [✓] Cambios de precio en favoritos                               │  │
│  │ [ ] Recomendaciones personalizadas                               │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [Guardar Preferencias]                                                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Derecho de Portabilidad

### 5.1 Descripción

El titular puede solicitar sus datos en un formato estructurado y legible por máquina para transferirlos a otro servicio.

### 5.2 Formato de Exportación

| Formato  | Uso               | Contenido                     |
| -------- | ----------------- | ----------------------------- |
| **JSON** | Técnico/Migración | Todos los datos estructurados |
| **CSV**  | Excel/Análisis    | Tablas de datos               |
| **PDF**  | Legible           | Resumen humanizado            |

### 5.3 Datos Exportables

```json
{
  "export_date": "2026-01-25T10:30:00Z",
  "user": {
    "id": "uuid",
    "email": "usuario@email.com",
    "name": "Juan Pérez",
    "phone": "+1809...",
    "created_at": "2025-06-15",
    "verified": true
  },
  "profile": {
    "bio": "...",
    "avatar_url": "...",
    "location": "Santo Domingo"
  },
  "activity": {
    "searches": [...],
    "favorites": [...],
    "views": [...]
  },
  "listings": [...],
  "transactions": [...],
  "messages": [...],
  "settings": {...}
}
```

### 5.4 UI Propuesta

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📥 DESCARGAR MIS DATOS                                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Puedes descargar una copia de toda la información que tenemos         │
│  sobre ti en OKLA.                                                      │
│                                                                         │
│  Formato de descarga:                                                   │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ (•) JSON - Formato técnico, ideal para migración                 │  │
│  │ ( ) CSV - Hojas de cálculo                                       │  │
│  │ ( ) PDF - Documento legible                                      │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  Incluir:                                                               │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ [✓] Información de perfil                                        │  │
│  │ [✓] Mis anuncios                                                 │  │
│  │ [✓] Búsquedas guardadas                                          │  │
│  │ [✓] Favoritos                                                    │  │
│  │ [✓] Historial de transacciones                                   │  │
│  │ [✓] Mensajes                                                     │  │
│  │ [✓] Configuración                                                │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ⏱️ La preparación puede tomar hasta 24 horas.                          │
│     Te enviaremos un email cuando esté listo.                          │
│                                                                         │
│  [Solicitar Descarga]                                                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.5 Endpoints API

| Método | Endpoint                               | Descripción           | Estado |
| ------ | -------------------------------------- | --------------------- | ------ |
| `POST` | `/api/privacy/export/request`          | Solicitar exportación | ✅     |
| `GET`  | `/api/privacy/export/status`           | Estado de exportación | ✅     |
| `GET`  | `/api/privacy/export/download/{token}` | Descargar archivo     | ✅     |

---

## 6. Centro de Privacidad Unificado

### 6.1 Ruta: `/privacy-center`

Página central que agrupa todas las opciones de privacidad:

```
┌─────────────────────────────────────────────────────────────────────────┐
│  🔐 CENTRO DE PRIVACIDAD                                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Tus Derechos                                                           │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                 │   │
│  │  📋 Ver mis datos        📝 Corregir datos                     │   │
│  │  Revisa toda la          Actualiza tu                          │   │
│  │  información que         información personal                  │   │
│  │  tenemos sobre ti        [Ir a Configuración]                  │   │
│  │  [Ver Resumen]                                                 │   │
│  │                                                                 │   │
│  │  📥 Descargar mis datos  🗑️ Eliminar cuenta                    │   │
│  │  Exporta tu información  Solicita la eliminación               │   │
│  │  en formato JSON/CSV     de tu cuenta y datos                  │   │
│  │  [Solicitar Descarga]    [Eliminar Cuenta]                     │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  Preferencias de Comunicación                                           │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ [Gestionar Preferencias]                                       │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  Documentos Legales                                                     │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ [Política de Privacidad]  [Términos y Condiciones]             │   │
│  │ [Política de Cookies]     [Aviso Legal]                        │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ¿Necesitas ayuda?                                                      │
│  Contacta a nuestro Delegado de Protección de Datos:                   │
│  📧 privacidad@okla.com.do                                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 7. Servicio Backend

```csharp
// PrivacyService.cs
public interface IPrivacyService
{
    // Acceso
    Task<UserDataSummary> GetUserDataSummary(Guid userId);
    Task<UserDataFull> GetUserDataFull(Guid userId);
    Task<string> RequestDataExport(Guid userId, ExportFormat format);

    // Rectificación
    Task<bool> UpdateUserData(Guid userId, UserUpdateDto dto);

    // Cancelación
    Task<DeleteRequestResult> RequestAccountDeletion(Guid userId, string reason);
    Task<bool> ConfirmAccountDeletion(Guid userId, string code);
    Task<bool> CancelDeletionRequest(Guid userId);
    Task ProcessPendingDeletions(); // Job diario

    // Oposición
    Task<CommunicationPreferences> GetPreferences(Guid userId);
    Task UpdatePreferences(Guid userId, CommunicationPreferences prefs);

    // Portabilidad
    Task<ExportStatus> GetExportStatus(Guid userId);
    Task<byte[]> DownloadExport(Guid userId, string token);
}
```

---

## 8. Plazos de Respuesta

| Derecho       | Plazo Legal     | Plazo OKLA               |
| ------------- | --------------- | ------------------------ |
| Acceso        | 10 días hábiles | 5 días                   |
| Rectificación | 10 días hábiles | Inmediato                |
| Cancelación   | 15 días hábiles | 15 días (período gracia) |
| Oposición     | Inmediato       | Inmediato                |
| Portabilidad  | 15 días hábiles | 24-48 horas              |

---

## 9. Cronograma de Implementación

### Fase 1: Q1 2026 - Acceso Mejorado ✅ COMPLETADO

- [x] Página "Ver todos mis datos" (`/settings/privacy/my-data`)
- [x] Incluir logs de actividad
- [x] Incluir datos de terceros

### Fase 2: Q1 2026 - Eliminación Automatizada ✅ COMPLETADO

- [x] UI de eliminación de cuenta (`/settings/privacy/delete-account`)
- [x] Período de gracia de 30 días
- [x] Anonimización de datos retenidos
- [x] Endpoints completos de cancelación

### Fase 3: Q1 2026 - Portabilidad ✅ COMPLETADO

- [x] Generador de exportación JSON/PDF
- [x] Cola de procesamiento
- [x] Descarga segura con token

### Fase 4: Q1 2026 - Centro Unificado ✅ COMPLETADO

- [x] Página `/privacy-center`
- [x] Integración de todas las funciones
- [x] Preferencias de oposición

---

## 10. Referencias

| Documento                   | Ubicación       |
| --------------------------- | --------------- |
| Ley 172-13                  | congreso.gob.do |
| Política de Privacidad OKLA | /privacy        |
| Términos y Condiciones      | /terms          |
| 02-ley-172-13.md            | Este directorio |

---

**Última revisión:** Enero 26, 2026  
**Próxima revisión:** Abril 26, 2026  
**Responsable:** Equipo de Desarrollo + Legal OKLA  
**Prioridad:** ✅ COMPLETADO (Derecho fundamental del usuario)

### 📚 Documentación Relacionada

- Ver implementación técnica completa: [`/docs/ARCO_IMPLEMENTATION_COMPLETED.md`](/docs/ARCO_IMPLEMENTATION_COMPLETED.md)

# 🔒 Auditoría Protección de Datos Personales

> **Propósito:** Verificar cumplimiento de Ley 172-13 de Protección de Datos  
> **Regulador:** No hay autoridad específica (pendiente creación)  
> **Última actualización:** Enero 25, 2026

---

## 📋 INFORMACIÓN GENERAL

### Datos del Responsable de Tratamiento

| Campo                      | Valor                      | Estado       |
| -------------------------- | -------------------------- | ------------ |
| **Responsable**            | OKLA SRL                   | ✅           |
| **RNC**                    | 1-32-XXXXX-X               | ⚠️ Verificar |
| **Dirección**              | Av. Winston Churchill #XXX | ⚠️ Verificar |
| **Email Datos Personales** | privacidad@okla.com.do     | 🔴 Crear     |
| **Delegado de Protección** | Pendiente designar         | 🔴           |

### Categorías de Datos Tratados

| Categoría          | Ejemplos                            | Sensibilidad | Usuarios Afectados  |
| ------------------ | ----------------------------------- | ------------ | ------------------- |
| **Identificación** | Nombre, cédula, RNC                 | Media        | Todos               |
| **Contacto**       | Email, teléfono, dirección          | Media        | Todos               |
| **Financieros**    | Tarjeta (tokenizada), transacciones | Alta         | Compradores/Dealers |
| **Vehiculares**    | Placa, chasis, historial            | Media        | Vendedores          |
| **Navegación**     | IP, cookies, historial              | Baja         | Todos               |
| **Imágenes**       | Fotos de vehículos, documentos      | Media        | Vendedores/Dealers  |
| **Ubicación**      | Geolocalización                     | Alta         | Con consentimiento  |

---

## 📊 ESTADO DE CUMPLIMIENTO

### Principios de la Ley 172-13

| Principio            | Artículo | Descripción                    | Estado OKLA |
| -------------------- | -------- | ------------------------------ | ----------- |
| **Licitud**          | Art. 4   | Tratamiento con base legal     | ✅          |
| **Lealtad**          | Art. 4   | No engañar al titular          | ✅          |
| **Calidad**          | Art. 5   | Datos exactos y actualizados   | 🟡 Parcial  |
| **Finalidad**        | Art. 5   | Uso solo para fines declarados | ✅          |
| **Proporcionalidad** | Art. 5   | Solo datos necesarios          | ✅          |
| **Información**      | Art. 5   | Informar al titular            | ✅          |
| **Consentimiento**   | Art. 4   | Obtener autorización           | 🟡 Parcial  |
| **Seguridad**        | Art. 13  | Proteger datos                 | 🟡 Parcial  |

---

## 🔐 BASES LEGALES DEL TRATAMIENTO

### Tratamientos por Base Legal

| Base Legal             | Tratamientos Aplicables                         | Documentación           |
| ---------------------- | ----------------------------------------------- | ----------------------- |
| **Consentimiento**     | Marketing, cookies no esenciales, newsletter    | 🟡 Checkboxes           |
| **Ejecución Contrato** | Crear cuenta, publicar vehículo, procesar pagos | ✅ Términos             |
| **Obligación Legal**   | Facturación (DGII), KYC (UAF), retención datos  | 🟡 Políticas            |
| **Interés Legítimo**   | Prevención fraude, análisis de seguridad        | 🔴 Pendiente documentar |

### Consentimientos Requeridos

| Propósito                  | Tipo           | Opcional       | Estado       |
| -------------------------- | -------------- | -------------- | ------------ |
| Términos y Condiciones     | Contrato       | ❌ Obligatorio | ✅           |
| Política de Privacidad     | Informativo    | ❌ Obligatorio | ✅           |
| Newsletter                 | Consentimiento | ✅ Opcional    | 🟡 Parcial   |
| Cookies no esenciales      | Consentimiento | ✅ Opcional    | 🔴 Pendiente |
| Compartir con terceros     | Consentimiento | ✅ Opcional    | 🔴 Pendiente |
| Comunicaciones comerciales | Consentimiento | ✅ Opcional    | 🔴 Pendiente |

---

## 👤 DERECHOS ARCO

### Estado de Implementación

| Derecho           | Artículo | Descripción            | Estado     | Ubicación UI            |
| ----------------- | -------- | ---------------------- | ---------- | ----------------------- |
| **Acceso**        | Art. 8   | Ver datos almacenados  | 🟡 Parcial | /settings/profile       |
| **Rectificación** | Art. 9   | Corregir datos         | ✅         | /settings/profile       |
| **Cancelación**   | Art. 10  | Eliminar datos         | 🔴         | Sin implementar         |
| **Oposición**     | Art. 11  | Oponerse a tratamiento | 🟡 Parcial | /settings/notifications |
| **Portabilidad**  | Art. 8   | Exportar datos         | 🔴         | Sin implementar         |

### Proceso de Atención ARCO (A implementar)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  FLUJO DE SOLICITUD ARCO                                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  DÍA 0: RECEPCIÓN                                                       │
│  ├── Usuario envía solicitud por:                                      │
│  │   ├── Formulario web (/settings/privacy/arco)                       │
│  │   ├── Email (privacidad@okla.com.do)                                │
│  │   └── Carta física (oficinas)                                       │
│  ├── Sistema genera ticket automático                                  │
│  └── Confirma recepción al usuario                                     │
│                                                                         │
│  DÍA 1-2: VERIFICACIÓN IDENTIDAD                                        │
│  ├── Verificar que el solicitante es el titular                        │
│  ├── Si es representante, verificar poder                              │
│  └── Solicitar documentación adicional si necesario                    │
│                                                                         │
│  DÍA 3-8: PROCESAMIENTO                                                 │
│  ├── ACCESO: Recopilar todos los datos del usuario                     │
│  ├── RECTIFICACIÓN: Actualizar datos en todos los sistemas             │
│  ├── CANCELACIÓN: Anonimizar o eliminar (respetando retención legal)   │
│  ├── OPOSICIÓN: Desactivar tratamiento específico                      │
│  └── PORTABILIDAD: Generar archivo en formato estructurado             │
│                                                                         │
│  DÍA 9-10: RESPUESTA                                                    │
│  ├── Notificar al usuario resultado                                    │
│  ├── Entregar información solicitada                                   │
│  └── Documentar en expediente interno                                  │
│                                                                         │
│  PLAZO MÁXIMO: 10 días hábiles                                          │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Casos Especiales

| Situación             | Tratamiento                                               |
| --------------------- | --------------------------------------------------------- |
| **Datos de terceros** | Usuario no puede acceder a datos de otros                 |
| **Retención legal**   | Datos fiscales se mantienen 10 años (explicar al usuario) |
| **Datos en backups**  | Informar que pueden persistir en backups históricos       |
| **Datos compartidos** | Notificar a terceros la rectificación/cancelación         |
| **Menor de edad**     | Requerir autorización del representante legal             |

---

## 📁 REGISTRO DE ACTIVIDADES DE TRATAMIENTO

### Tratamientos Principales

| ID    | Tratamiento              | Finalidad         | Datos                            | Base Legal       | Retención                |
| ----- | ------------------------ | ----------------- | -------------------------------- | ---------------- | ------------------------ |
| T-001 | Registro de usuarios     | Crear cuenta      | Nombre, email, teléfono          | Contrato         | Vigencia cuenta + 2 años |
| T-002 | Publicación vehículos    | Mostrar anuncios  | Datos vehículo, fotos, ubicación | Contrato         | Vigencia anuncio + 1 año |
| T-003 | Procesamiento pagos      | Cobrar servicios  | Tarjeta (token), monto, fecha    | Contrato         | 10 años (fiscal)         |
| T-004 | Verificación identidad   | KYC/AML           | Cédula, foto, comprobante        | Obligación legal | 10 años                  |
| T-005 | Comunicaciones marketing | Promociones       | Email, preferencias              | Consentimiento   | Hasta revocación         |
| T-006 | Análisis y estadísticas  | Mejora servicio   | Datos anonimizados               | Interés legítimo | Indefinido               |
| T-007 | Prevención fraude        | Seguridad         | IP, comportamiento               | Interés legítimo | 3 años                   |
| T-008 | Soporte al cliente       | Atender consultas | Mensajes, historial              | Contrato         | 2 años                   |

### Encargados de Tratamiento (Terceros)

| Proveedor | Servicio       | Datos Compartidos | País | Contrato     |
| --------- | -------------- | ----------------- | ---- | ------------ |
| Stripe    | Pagos          | Datos de tarjeta  | USA  | 🟡 Revisar   |
| AZUL      | Pagos          | Datos de tarjeta  | RD   | 🟡 Revisar   |
| AWS S3    | Almacenamiento | Fotos, documentos | USA  | 🟡 Revisar   |
| SendGrid  | Emails         | Direcciones email | USA  | 🔴 Pendiente |
| Twilio    | SMS            | Números teléfono  | USA  | 🔴 Pendiente |

---

## 🔐 MEDIDAS DE SEGURIDAD

### Medidas Técnicas

| Medida                  | Descripción                   | Estado          |
| ----------------------- | ----------------------------- | --------------- |
| **Cifrado en tránsito** | HTTPS/TLS 1.3                 | ✅ Implementado |
| **Cifrado en reposo**   | AES-256 para datos sensibles  | 🟡 Parcial      |
| **Hashing contraseñas** | bcrypt con salt               | ✅ Implementado |
| **Tokenización pagos**  | Stripe/AZUL tokeniza tarjetas | ✅ Implementado |
| **Control de acceso**   | RBAC por roles                | ✅ Implementado |
| **Autenticación 2FA**   | Opcional para usuarios        | 🟡 Disponible   |
| **Logs de acceso**      | Registro de acceso a datos    | 🟡 Parcial      |
| **Backups cifrados**    | Copias de seguridad seguras   | ✅ Implementado |
| **Firewall/WAF**        | Protección perimetral         | ⚠️ Verificar    |

### Medidas Organizativas

| Medida                           | Descripción            | Estado       |
| -------------------------------- | ---------------------- | ------------ |
| **Política de privacidad**       | Publicada en sitio web | ✅           |
| **Acuerdos de confidencialidad** | Con empleados          | 🔴 Pendiente |
| **Capacitación en privacidad**   | A personal             | 🔴 Pendiente |
| **Política de contraseñas**      | Requisitos mínimos     | ✅           |
| **Política de acceso mínimo**    | Solo acceso necesario  | 🟡 Parcial   |
| **Procedimiento de brechas**     | Plan de respuesta      | 🔴 Pendiente |

---

## 🚨 GESTIÓN DE BRECHAS DE SEGURIDAD

### Procedimiento de Respuesta (A implementar)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  PROTOCOLO DE BRECHA DE DATOS                                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  FASE 1: DETECCIÓN (0-2 horas)                                          │
│  ├── Identificar el incidente                                          │
│  ├── Notificar al equipo de respuesta                                  │
│  ├── Activar protocolo de contención                                   │
│  └── Documentar hora y circunstancias                                  │
│                                                                         │
│  FASE 2: CONTENCIÓN (2-24 horas)                                        │
│  ├── Aislar sistemas afectados                                         │
│  ├── Detener la fuga de datos                                          │
│  ├── Preservar evidencia                                               │
│  └── Evaluar alcance inicial                                           │
│                                                                         │
│  FASE 3: EVALUACIÓN (24-72 horas)                                       │
│  ├── Determinar datos afectados                                        │
│  ├── Identificar usuarios impactados                                   │
│  ├── Evaluar riesgo para los titulares                                 │
│  └── Determinar causa raíz                                             │
│                                                                         │
│  FASE 4: NOTIFICACIÓN (72 horas máximo)                                 │
│  ├── Notificar a usuarios afectados                                    │
│  ├── Notificar a autoridades (si alto riesgo)                          │
│  └── Comunicado público (si masivo)                                    │
│                                                                         │
│  FASE 5: REMEDIACIÓN (1-4 semanas)                                      │
│  ├── Corregir vulnerabilidad                                           │
│  ├── Implementar mejoras                                               │
│  ├── Ofrecer asistencia a afectados                                    │
│  └── Documentar lecciones aprendidas                                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Registro de Brechas

| ID  | Fecha | Descripción                | Datos Afectados | Usuarios | Notificado | Estado |
| --- | ----- | -------------------------- | --------------- | -------- | ---------- | ------ |
| -   | -     | Sin incidentes registrados | -               | -        | -          | -      |

---

## 📊 INVENTARIO DE DATOS

### Por Sistema/Base de Datos

| Sistema                  | Tipo de Datos               | Ubicación       | Retención        | Cifrado    |
| ------------------------ | --------------------------- | --------------- | ---------------- | ---------- |
| **PostgreSQL Principal** | Usuarios, transacciones     | AWS RDS         | Variable         | 🟡 Parcial |
| **Redis Cache**          | Sesiones, tokens            | AWS ElastiCache | 24h              | ❌         |
| **S3 Media**             | Fotos vehículos, documentos | AWS S3          | Vigencia + 1 año | ✅         |
| **Logs**                 | Accesos, errores            | Elasticsearch   | 90 días          | ❌         |
| **Backups**              | Completo                    | AWS S3 Glacier  | 7 años           | ✅         |

### Por Microservicio

| Servicio                | Datos Almacenados      | Datos Sensibles    | Cifrado |
| ----------------------- | ---------------------- | ------------------ | ------- |
| **AuthService**         | Credenciales, tokens   | Contraseñas (hash) | ✅      |
| **UserService**         | Perfiles, preferencias | Cédula             | 🟡      |
| **VehiclesSaleService** | Vehículos, fotos       | Placa, chasis      | ❌      |
| **BillingService**      | Transacciones          | Tokens de pago     | ✅      |
| **MediaService**        | Archivos, documentos   | Documentos KYC     | 🟡      |
| **NotificationService** | Historial mensajes     | Emails, teléfonos  | ❌      |

---

## 🔍 CHECKLIST DE AUDITORÍA

### Documentación

```
POLÍTICAS Y AVISOS
□ Política de privacidad publicada y actualizada
□ Términos y condiciones con cláusulas de datos
□ Aviso de cookies implementado
□ Política interna de protección de datos

REGISTROS
□ Registro de actividades de tratamiento
□ Registro de consentimientos obtenidos
□ Registro de solicitudes ARCO
□ Registro de brechas de seguridad

CONTRATOS
□ Contratos con encargados de tratamiento
□ Cláusulas de confidencialidad con empleados
□ Transferencias internacionales documentadas
```

### Derechos de los Titulares

```
ACCESO
□ Mecanismo para solicitar acceso
□ Proceso documentado
□ Cumplimiento del plazo (10 días)
□ Formato de respuesta definido

RECTIFICACIÓN
□ Funcionalidad de edición de perfil
□ Actualización en todos los sistemas
□ Confirmación al usuario

CANCELACIÓN
□ Funcionalidad de eliminación de cuenta
□ Proceso de anonimización
□ Respeto a retención legal
□ Notificación a terceros

OPOSICIÓN
□ Opt-out de marketing
□ Desactivación de cookies
□ Retiro de consentimientos
```

### Seguridad

```
MEDIDAS TÉCNICAS
□ Cifrado en tránsito (HTTPS)
□ Cifrado en reposo (datos sensibles)
□ Control de acceso por roles
□ Logs de auditoría
□ Backups seguros

MEDIDAS ORGANIZATIVAS
□ Capacitación del personal
□ Acuerdos de confidencialidad
□ Política de acceso mínimo
□ Procedimiento de brechas
□ Auditorías periódicas
```

---

## 📋 TRANSFERENCIAS INTERNACIONALES

### Análisis de Transferencias

| Destino | Proveedor | Datos     | Garantía   | Estado       |
| ------- | --------- | --------- | ---------- | ------------ |
| **USA** | Stripe    | Pagos     | SCCs + DPF | 🟡 Revisar   |
| **USA** | AWS       | Todos     | SCCs + DPF | 🟡 Revisar   |
| **USA** | SendGrid  | Emails    | SCCs       | 🔴 Pendiente |
| **USA** | Twilio    | Teléfonos | SCCs       | 🔴 Pendiente |

### Garantías Aplicables

- **SCCs:** Standard Contractual Clauses de la UE
- **DPF:** Data Privacy Framework (USA)
- **BCRs:** Binding Corporate Rules (si aplica)

---

## 🔗 INTEGRACIÓN DataProtectionService

### Endpoints Propuestos (Puerto 5073)

| Endpoint                              | Descripción                         |
| ------------------------------------- | ----------------------------------- |
| `GET /api/privacy/data/{userId}`      | Obtener todos los datos del usuario |
| `POST /api/privacy/arco/request`      | Crear solicitud ARCO                |
| `GET /api/privacy/arco/requests`      | Listar solicitudes pendientes       |
| `PUT /api/privacy/arco/requests/{id}` | Procesar solicitud                  |
| `GET /api/privacy/consents/{userId}`  | Obtener consentimientos             |
| `PUT /api/privacy/consents/{userId}`  | Actualizar consentimientos          |
| `POST /api/privacy/breach`            | Registrar brecha                    |
| `GET /api/privacy/audit-log`          | Log de acceso a datos               |
| `POST /api/privacy/export/{userId}`   | Exportar datos (portabilidad)       |
| `DELETE /api/privacy/data/{userId}`   | Anonimizar/eliminar datos           |

---

## 📞 CONTACTO DE PRIVACIDAD

| Concepto               | Valor                     | Estado       |
| ---------------------- | ------------------------- | ------------ |
| Email público          | privacidad@okla.com.do    | 🔴 Crear     |
| Formulario web         | /settings/privacy/contact | 🔴 Crear     |
| Delegado de Protección | Pendiente designar        | 🔴           |
| Teléfono               | 809-XXX-XXXX              | ⚠️ Verificar |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Trimestral  
**Responsable:** Delegado de Protección de Datos (pendiente designar)

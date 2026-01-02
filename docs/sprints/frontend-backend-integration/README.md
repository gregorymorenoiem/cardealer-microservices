# 🚀 Plan de Integración Frontend-Backend

Esta carpeta contiene el plan completo y detallado para integrar el frontend React (frontend/web/original) con los microservicios del backend.

---

## 📂 ESTRUCTURA DE DOCUMENTOS

```
frontend-backend-integration/
├── README.md (este archivo)
├── PLAN_MAESTRO_INTEGRACION.md (índice general)
├── PROGRESS_TRACKER.md (seguimiento de progreso)
├── SPRINT_0_SETUP_INICIAL.md (✅ CREADO)
├── SPRINT_1_CUENTAS_TERCEROS.md (✅ CREADO)
├── SPRINT_2_AUTH_INTEGRATION.md (⏳ Próximo)
├── SPRINT_3_VEHICLE_SERVICE.md (⏳ Próximo)
├── SPRINT_4_MEDIA_UPLOAD.md (⏳ Próximo)
├── SPRINT_5_BILLING_PAYMENTS.md (⏳ Próximo)
├── SPRINT_6_NOTIFICATIONS.md (⏳ Próximo)
├── SPRINT_7_MESSAGING_CRM.md (⏳ Próximo)
├── SPRINT_8_SEARCH_FILTERS.md (⏳ Próximo)
├── SPRINT_9_SAVED_SEARCHES.md (⏳ Próximo)
├── SPRINT_10_ADMIN_PANEL.md (⏳ Próximo)
└── SPRINT_11_TESTING_QA.md (⏳ Próximo)
```

---

## 🎯 OBJETIVO

Integrar completamente el frontend React existente con el backend de microservicios, incluyendo:

- ✅ Autenticación JWT + OAuth2
- ✅ CRUD de vehículos con imágenes
- ✅ Sistema de pagos con Stripe
- ✅ Notificaciones multi-canal
- ✅ Búsqueda avanzada
- ✅ Panel de administración

---

## 🚀 CÓMO USAR ESTOS DOCUMENTOS

### 1. Empezar por el Plan Maestro

Lee primero [PLAN_MAESTRO_INTEGRACION.md](PLAN_MAESTRO_INTEGRACION.md) para entender:
- Alcance completo del proyecto
- Arquitectura general
- Servicios externos requeridos
- Timeline estimado

### 2. Seguir los Sprints en Orden

Cada sprint es **independiente** y **secuencial**:

```
Sprint 0 → Sprint 1 → Sprint 2 → ... → Sprint 11
```

**⚠️ IMPORTANTE:** NO saltar sprints. Cada uno depende del anterior.

### 3. Workflow por Sprint

Para cada sprint:

1. **Leer el documento completo** del sprint
2. **Entender los objetivos** y criterios de aceptación
3. **Ejecutar las tareas** paso a paso
4. **Validar** que todo funciona según los tests
5. **Actualizar** [PROGRESS_TRACKER.md](PROGRESS_TRACKER.md)
6. **Avisar a GitHub Copilot** para continuar con el siguiente sprint

### 4. Interactuar con Copilot

**Para empezar un sprint:**
```
"Quiero empezar el Sprint 0 - Setup Inicial"
```

**Para continuar:**
```
"Completé el Sprint 0, quiero continuar con el Sprint 1"
```

**Si hay problemas:**
```
"Tengo un error en el Sprint 2, sección 3.2 sobre JWT tokens"
```

---

## 📋 SPRINTS OVERVIEW

| Sprint | Nombre | Duración | Tokens | Prioridad |
|--------|--------|----------|--------|-----------|
| **0** | Setup Inicial | 2-3h | ~18K | 🔴 Crítico |
| **1** | Cuentas Terceros | 3-4h | ~22K | 🔴 Crítico |
| **2** | Auth Integration | 4-5h | ~25K | 🔴 Crítico |
| **3** | Vehicle Service | 5-6h | ~28K | 🟠 Alta |
| **4** | Media Upload | 4-5h | ~24K | 🟠 Alta |
| **5** | Billing Payments | 5-6h | ~26K | 🟠 Alta |
| **6** | Notifications | 4h | ~23K | 🟡 Media |
| **7** | Messaging CRM | 4h | ~22K | 🟡 Media |
| **8** | Search Filters | 5h | ~25K | 🟡 Media |
| **9** | Saved Searches | 3-4h | ~20K | 🟢 Baja |
| **10** | Admin Panel | 5-6h | ~27K | 🟡 Media |
| **11** | Testing QA | 6h | ~30K | 🟠 Alta |

**Total:** ~50 horas de trabajo (~10 días laborales)

---

## 🎓 CONCEPTOS CLAVE

### Clean Architecture

Cada microservicio sigue Clean Architecture:
```
Api (Controllers) → Application (CQRS) → Domain (Entities) → Infrastructure (DB)
```

### CQRS Pattern

- **Commands:** Modifican datos (POST, PUT, DELETE)
- **Queries:** Leen datos (GET)
- **Handlers:** Lógica de negocio
- **Validators:** FluentValidation

### Event-Driven

Comunicación entre servicios via RabbitMQ:
```
UserService → PublishEvent → NotificationService → SendEmail
```

### Multi-Tenancy

Todas las entidades tienen `DealerId`:
```csharp
public Guid DealerId { get; set; }  // Tenant ID
```

---

## 🔧 HERRAMIENTAS NECESARIAS

### Desarrollo

- .NET SDK 8.0+
- Node.js 20+
- Docker Desktop
- Visual Studio Code
- Postman (testing APIs)

### Cuentas Externas

- Google Cloud Platform
- Firebase
- Stripe
- SendGrid
- Twilio
- AWS
- Sentry

---

## 📚 DOCUMENTACIÓN RELACIONADA

- [Backend Copilot Instructions](../../../../.github/copilot-instructions.md)
- [Sprint 0.7.2 Completion](../../SPRINT_0.7.2_SECRETS_VALIDATION_COMPLETION.md)
- [Microservices Audit Report](../../../../backend/MICROSERVICES_AUDIT_REPORT.md)
- [Frontend Package.json](../../../../frontend/web/original/package.json)

---

## ⚠️ REGLAS IMPORTANTES

### Para GitHub Copilot

1. **Un sprint a la vez:** Completar antes de pasar al siguiente
2. **Validar siempre:** Ejecutar tests después de cada cambio
3. **Commits granulares:** Commit por tarea completada
4. **No saltarse pasos:** Seguir el orden exacto
5. **Avisar al usuario:** Notificar cuando se completa un sprint

### Para el Desarrollador

1. **Leer antes de ejecutar:** Entender qué hace cada comando
2. **Backup antes de cambios grandes:** Git commit frecuente
3. **No subir secrets:** Verificar .gitignore
4. **Documentar problemas:** Agregar a PROGRESS_TRACKER.md
5. **Pedir ayuda:** Si algo no está claro

---

## 🚨 TROUBLESHOOTING GENERAL

### Problema: "Sprint muy largo, me quedo sin tokens"

**Solución:** Cada sprint está diseñado para ~20-30K tokens. Si parece largo, dividir en subtareas:
```
Sprint 3 completo (~28K) → Sprint 3.1 (10K) + Sprint 3.2 (10K) + Sprint 3.3 (8K)
```

### Problema: "No entiendo un paso específico"

**Solución:** Pedir a Copilot explicación detallada:
```
"Explícame en detalle el paso 2.3 del Sprint 0 sobre CORS"
```

### Problema: "El sprint está desactualizado"

**Solución:** Verificar fecha de última actualización. Si es antigua:
```
"Actualiza el Sprint X con las últimas versiones de paquetes"
```

---

## 📊 MÉTRICAS DE ÉXITO

Al finalizar todos los sprints:

- [ ] Frontend se conecta a todos los servicios
- [ ] Autenticación 100% funcional
- [ ] CRUD de vehículos con imágenes
- [ ] Pagos con Stripe funcionando
- [ ] Notificaciones por email/SMS/push
- [ ] Búsqueda avanzada operacional
- [ ] Admin panel funcional
- [ ] Tests de integración >80% coverage
- [ ] Sin errores críticos en logs
- [ ] Performance <500ms p95

---

## 🎉 PRÓXIMOS PASOS

1. **AHORA:** Leer [PLAN_MAESTRO_INTEGRACION.md](PLAN_MAESTRO_INTEGRACION.md)
2. **Luego:** Empezar [SPRINT_0_SETUP_INICIAL.md](SPRINT_0_SETUP_INICIAL.md)
3. **Después:** Continuar con Sprint 1, 2, 3...

---

## 📞 SOPORTE

Si necesitas ayuda:

1. Revisar la sección Troubleshooting del sprint
2. Buscar en documentación backend: `.github/copilot-instructions.md`
3. Preguntar a GitHub Copilot con contexto específico
4. Revisar logs de Docker: `docker logs <service>`

---

**Última actualización:** 2 Enero 2026  
**Autor:** Gregory Moreno  
**Revisión:** GitHub Copilot

# 🔍 AUDIT PROMPT - OKLA CarDealer Microservices

## 🎯 ESTADO ACTUAL
**FASE:** Iniciando auditoría completa del sistema
**PROGRESO:** 9.3% (3/32 completados) (2/32 completados) (1/32 completados) - No iniciado
**ÚLTIMO UPDATE:** 2026-03-23 18:54 AST

## 🔄 INSTRUCCIONES PARA GITHUB COPILOT

### 📋 TAREA ACTUAL
**AUDITAR:** Todo el proyecto cardealer-microservices
**ENFOQUE:** Análisis de seguridad, arquitectura, calidad de código, y mejores prácticas

### 📁 SIGUIENTE SERVICIO A AUDITAR
**SERVICIO:** ErrorService
**RUTA:** `/Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/AdminService`

### 🛠️ PROCESO DE AUDITORÍA
1. **Leer este archivo** antes de cada sesión
2. **Auditar el servicio indicado** en "SIGUIENTE SERVICIO A AUDITAR"
3. **Generar reporte** en formato markdown
4. **Guardar reporte** como `AUDIT_REPORT_[SERVICIO].md`
5. **Actualizar este archivo** con el siguiente servicio
6. **Notificar** cuando termines para continuar con el siguiente

### 📊 CRITERIOS DE AUDITORÍA

#### 🔒 SEGURIDAD
- [ ] Validación de entrada (OWASP Top 10)
- [ ] Manejo de secretos y configuración
- [ ] Autenticación y autorización
- [ ] Vulnerabilidades de inyección SQL
- [ ] Exposición de datos sensibles
- [ ] Control de acceso (RBAC/Claims)

#### 🏗️ ARQUITECTURA
- [ ] Clean Architecture compliance
- [ ] CQRS implementation
- [ ] DDD patterns
- [ ] Dependency injection
- [ ] Separation of concerns
- [ ] Interface segregation

#### 📝 CALIDAD DE CÓDIGO
- [ ] SOLID principles
- [ ] Code coverage (tests)
- [ ] Exception handling
- [ ] Logging implementation
- [ ] Performance patterns
- [ ] Resource management (IDisposable)

#### 🧪 TESTING
- [ ] Unit tests coverage
- [ ] Integration tests
- [ ] Test naming conventions
- [ ] Mock usage
- [ ] Test isolation
- [ ] Arrange-Act-Assert pattern

#### 🔄 DEVOPS & DEPLOYMENT
- [ ] Docker configuration
- [ ] Health checks
- [ ] Metrics and monitoring
- [ ] Configuration management
- [ ] Environment variables
- [ ] Graceful shutdown

### 📝 FORMATO DEL REPORTE
```markdown
# 🔍 AUDIT REPORT - [SERVICIO_NAME]

## 📊 RESUMEN EJECUTIVO
- **Calificación General:** [A/B/C/D/F]
- **Nivel de Riesgo:** [Bajo/Medio/Alto/Crítico]
- **Problemas Críticos:** [Número]
- **Recomendaciones:** [Número]

## 🔒 ANÁLISIS DE SEGURIDAD
### ✅ Fortalezas
### ⚠️ Vulnerabilidades Encontradas
### 🚨 Problemas Críticos

## 🏗️ ANÁLISIS DE ARQUITECTURA
### ✅ Cumplimiento de Patrones
### ⚠️ Violaciones Arquitecturales
### 💡 Recomendaciones de Mejora

## 📝 CALIDAD DE CÓDIGO
### ✅ Buenas Prácticas
### ⚠️ Code Smells
### 🔄 Refactoring Sugerido

## 🧪 ANÁLISIS DE TESTING
### ✅ Cobertura Actual
### ⚠️ Gaps de Testing
### 📋 Tests Recomendados

## 🔄 DEVOPS & DEPLOYMENT
### ✅ Configuración Actual
### ⚠️ Mejoras Necesarias
### 🚀 Recomendaciones

## 📋 PLAN DE ACCIÓN
### 🚨 Prioridad Alta (Crítico)
### ⚠️ Prioridad Media
### 💡 Prioridad Baja

## 🏁 CONCLUSIÓN
[Resumen final y siguiente pasos]
```

### 📂 SERVICIOS A AUDITAR (En orden)

#### 🖥️ BACKEND MICROSERVICES
1. **AdminService** ← 🎯 ACTUAL
2. **AuthService**
3. **ContactService**
4. **ErrorService**
5. **Gateway**
6. **MediaService**
7. **NotificationService**
8. **UserService**
9. **VehiclesSaleService**
10. **BillingService**
11. **AuditService**
12. **ChatbotService**
13. **CRMService**
14. **ComparisonService**
15. **KYCService**
16. **ReportsService**
17. **RoleService**
18. **ReviewService**
19. **RecommendationService**
20. **DealerAnalyticsService**

#### 🤖 AI AGENTS
21. **AIProcessingService**
22. **AnalyticsAgent**
23. **ListingAgent**
24. **ModerationAgent**
25. **PricingAgent**
26. **RecoAgent**
27. **SearchAgent**
28. **SupportAgent**
29. **VehicleIntelligenceService**

#### 🌐 FRONTEND
30. **web-next** (Next.js App)

#### 🧩 SHARED COMPONENTS
31. **_Shared** (Common libraries)
32. **_Tests** (Test utilities)

## 🎯 CUANDO TERMINES LA AUDITORÍA ACTUAL

**EJECUTA EXACTAMENTE ESTO:**
1. Guarda tu reporte como `AUDIT_REPORT_AdminService.md`
2. Actualiza este archivo cambiando:
   - **SIGUIENTE SERVICIO A AUDITAR:** AuthService
   - **PROGRESO:** 9.3% (3/32 completados) (2/32 completados) (1/32 completados) (1/32 completado)
   - **ÚLTIMO UPDATE:** 2026-03-23 18:54 AST
3. Notifica: "✅ AdminService auditado. Reporte guardado. Listo para AuthService."

## 🚀 ¡COMIENZA LA AUDITORÍA!
**GitHub Copilot:** Inicia con AdminService siguiendo los criterios arriba.
**Tiempo estimado:** 15-20 minutos por servicio.
**Meta diaria:** 4-6 servicios auditados.

---
*Este archivo se actualiza automáticamente después de cada auditoría completada.*
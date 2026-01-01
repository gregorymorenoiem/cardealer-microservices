# 📋 SPRINT 0.7.1 - GESTIÓN DE SECRETOS
**Fecha:** 31 Diciembre 2025  
**Estado:** 🟡 EN PROGRESO

---

## 🎯 Objetivo
Remover todos los secretos hardcodeados de `compose.yaml` y reemplazarlos con variables de entorno, permitiendo configuración flexible entre ambientes (dev/staging/prod).

---

## 🔍 Secretos Detectados en compose.yaml

### 🔴 CRÍTICO - JWT Keys (12 ocurrencias)
**Valor actual:** `"clave-super-secreta-desarrollo-32-caracteres-aaa"`  
**Ubicaciones (líneas):** 48, 147, 258, 375, 448, 521, 572, 644, 762, 831, 903, 1098  
**Servicios afectados:**
- authservice
- userservice
- productservice
- mediaservice
- errorservice
- notificationservice
- billingservice
- crmservice
- adminservice
- roleservice
- appointmentservice
- marketingservice

**Acción requerida:**
```yaml
# Antes:
Jwt__Key: "clave-super-secreta-desarrollo-32-caracteres-aaa"

# Después:
Jwt__Key: "${JWT__KEY:-clave-super-secreta-desarrollo-32-caracteres-aaa}"
```

### 🔴 CRÍTICO - SendGrid API Key (1 ocurrencia)
**Valor actual:** `"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"`  
**Ubicación:** línea 261 (NotificationService)  
**Riesgo:** **ALTO** - API key de producción expuesta

**Acción requerida:**
```yaml
# Antes:
NotificationSettings__SendGrid__ApiKey: "SG.Iuj5GOJjSc-d7GyWBUgJsw..."

# Después:
NotificationSettings__SendGrid__ApiKey: "${NOTIFICATIONSETTINGS__SENDGRID__APIKEY:-SG.demo_key_for_dev}"
```

### 🔴 CRÍTICO - Twilio Credentials (2 ocurrencias)
**Account SID:** `"AC19fec9dd3df70a34f6252c9ef649a532"`  
**Auth Token:** `"2221beebc69b7251062f2b10d7ed75e6"`  
**Ubicación:** líneas 264-265 (NotificationService)  
**Riesgo:** **ALTO** - Credenciales de producción expuestas

**Acción requerida:**
```yaml
# Antes:
NotificationSettings__Twilio__AccountSid: "AC19fec9dd3df70a34f6252c9ef649a532"
NotificationSettings__Twilio__AuthToken: "2221beebc69b7251062f2b10d7ed75e6"

# Después:
NotificationSettings__Twilio__AccountSid: "${NOTIFICATIONSETTINGS__TWILIO__ACCOUNTSID:-AC_demo_account_sid}"
NotificationSettings__Twilio__AuthToken: "${NOTIFICATIONSETTINGS__TWILIO__AUTHTOKEN:-demo_auth_token}"
```

### 🟡 MEDIO - Stripe Secret Key (1 ocurrencia)
**Valor actual:** `"sk_test_demo_key_for_development"`  
**Ubicación:** línea 836 (BillingService)  
**Riesgo:** MEDIO - Es clave de test pero debe ser configurable

**Acción requerida:**
```yaml
# Antes:
Stripe__SecretKey: "sk_test_demo_key_for_development"

# Después:
Stripe__SecretKey: "${STRIPE__SECRETKEY:-sk_test_demo_key_for_development}"
```

### 🟢 BAJO - PostgreSQL Passwords
**Valor actual:** `"password"` (múltiples bases de datos)  
**Riesgo:** BAJO en desarrollo, CRÍTICO en producción

**Acción requerida:**
```yaml
# Antes:
POSTGRES_PASSWORD: password

# Después:
POSTGRES_PASSWORD: "${POSTGRES_PASSWORD:-password}"
```

---

## 📝 Plan de Acción

### Fase 1: Reemplazo de Secretos (ESTE SPRINT)
- [ ] Reemplazar 12 ocurrencias de JWT__Key con variable `${JWT__KEY:-valor-default}`
- [ ] Reemplazar SendGrid API Key con variable `${SENDGRID_APIKEY:-valor-default}`
- [ ] Reemplazar Twilio AccountSid con variable `${TWILIO_ACCOUNTSID:-valor-default}`
- [ ] Reemplazar Twilio AuthToken con variable `${TWILIO_AUTHTOKEN:-valor-default}`
- [ ] Reemplazar Stripe SecretKey con variable `${STRIPE_SECRETKEY:-valor-default}`
- [ ] Reemplazar PostgreSQL passwords con variable `${POSTGRES_PASSWORD:-password}`

### Fase 2: Documentación (ESTE SPRINT)
- [x] .env.example ya existe y está actualizado
- [x] compose.secrets.example.yaml ya existe
- [ ] Actualizar README.md con instrucciones de configuración de secretos
- [ ] Crear script de validación de secretos (.env.validator.sh)

### Fase 3: Validación (Sprint 0.7.2)
- [ ] Probar servicios con .env configurado
- [ ] Probar servicios con valores default (modo desarrollo)
- [ ] Verificar que NO haya errores de autenticación por variables faltantes
- [ ] Documentar secretos obligatorios vs opcionales

---

## 🔧 Sintaxis de Variables de Entorno en Docker Compose

```yaml
# Sintaxis: ${VARIABLE:-valor_default}
# 
# Si VARIABLE existe en .env → usa ese valor
# Si VARIABLE NO existe → usa valor_default
# 
# Ejemplo:
environment:
  JWT__KEY: "${JWT__KEY:-clave-desarrollo-insegura}"
```

**Ventajas:**
- ✅ Permite desarrollo sin configurar .env (usa defaults)
- ✅ Permite producción con secretos seguros
- ✅ No rompe servicios existentes
- ✅ Facilita CI/CD con variables de entorno del sistema

---

## ⚠️ Secretos que DEBEN rotarse INMEDIATAMENTE

### SendGrid API Key
```
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
```
**Acción:** 
1. Ir a SendGrid Dashboard
2. Revocar esta API key
3. Generar nueva key
4. Agregar a .env como NOTIFICATIONSETTINGS__SENDGRID__APIKEY

### Twilio Credentials
```
AccountSid: AC19fec9dd3df70a34f6252c9ef649a532
AuthToken: 2221beebc69b7251062f2b10d7ed75e6
```
**Acción:**
1. Ir a Twilio Console
2. Verificar si estas credenciales están activas
3. Si están activas, revocarlas
4. Generar nuevas credenciales
5. Agregar a .env

### JWT Keys
```
clave-super-secreta-desarrollo-32-caracteres-aaa
```
**Acción:**
1. Generar nueva key: `openssl rand -base64 32`
2. Agregar a .env como JWT__KEY
3. Reiniciar todos los servicios para que usen la nueva key

---

## 🎯 Criterios de Aceptación

- [ ] 0 secretos hardcodeados en compose.yaml (todos reemplazados con variables)
- [ ] Servicios funcionan sin .env (con valores default de desarrollo)
- [ ] Servicios funcionan con .env configurado (con valores reales)
- [ ] .gitignore incluye .env y compose.secrets.yaml
- [ ] Documentación actualizada con instrucciones de configuración

---

## 📊 Impacto Estimado

| Cambio | Archivos | Líneas | Riesgo |
|--------|----------|--------|--------|
| JWT Keys | compose.yaml | 12 | Bajo (compatibilidad hacia atrás con default) |
| SendGrid | compose.yaml | 1 | Medio (requiere key válida para emails) |
| Twilio | compose.yaml | 2 | Medio (requiere credenciales para SMS) |
| Stripe | compose.yaml | 1 | Bajo (test key funcional) |
| PostgreSQL | compose.yaml | 13 | Bajo (default "password" OK para dev) |

**Total:** ~29 líneas a modificar en compose.yaml

---

## 🔄 Próximos Pasos

1. ✅ Identificar secretos hardcodeados - COMPLETADO
2. 🟡 Reemplazar con variables de entorno - EN PROGRESO
3. ⏳ Validar configuración con/sin .env
4. ⏳ Actualizar documentación
5. ⏳ Probar despliegue en ambiente limpio

---

**Sprint Status:** 🟡 EN PROGRESO  
**Tiempo estimado:** 1.5 horas  
**Progreso:** 30%

# Sprint 0.7.2 - Secrets Validation Plan

**Estado:** 🔄 EN PROGRESO  
**Fecha Inicio:** 01 Enero 2026

---

## 🎯 Objetivos

1. ✅ Levantar TODOS los 35 microservicios con variables de entorno
2. ⏳ Verificar health checks de cada servicio
3. ⏳ Identificar servicios con dependencias de secretos opcionales
4. ⏳ Documentar matriz "Servicio → Secretos Requeridos"
5. ⏳ Probar escenarios de configuración

---

## 📋 Checklist de Validación

### Servicios Core (7)
- [ ] AuthService - Puerto 15085
- [ ] Gateway - Puerto 18443
- [ ] ErrorService - Puerto 15083
- [ ] NotificationService - Puerto 15084
- [ ] UserService - Puerto 15100
- [ ] RoleService - Puerto 15101
- [ ] ProductService - Puerto 15006

### Servicios Infraestructura (11)
- [ ] ServiceDiscovery - Puerto 15005
- [ ] ConfigurationService - Puerto 15007
- [ ] HealthCheckService - Puerto 15008
- [ ] LoggingService - Puerto 15009
- [ ] TracingService - Puerto 15010
- [ ] CacheService - Puerto 15011
- [ ] MessageBusService - Puerto 15012
- [ ] SchedulerService - Puerto 15013
- [ ] SearchService - Puerto 15014
- [ ] FeatureToggleService - Puerto 15015
- [ ] IdempotencyService - Puerto 15016

### Servicios Negocio (9)
- [ ] MediaService - Puerto 15017
- [ ] BillingService - Puerto 15018
- [ ] CRMService - Puerto 15019
- [ ] AuditService - Puerto 15020
- [ ] ReportsService - Puerto 15021
- [ ] MarketingService - Puerto 15022
- [ ] IntegrationService - Puerto 15023
- [ ] FinanceService - Puerto 15024
- [ ] InvoicingService - Puerto 15025

### Servicios Especializados (8)
- [ ] AdminService - Puerto 24037
- [ ] ApiDocsService - Puerto 15027
- [ ] RateLimitingService - Puerto 15028
- [ ] ContactService - Puerto 15029
- [ ] AppointmentService - Puerto 15030
- [ ] BackupDRService - Puerto 15031
- [ ] FileStorageService - Puerto 15032
- [ ] RealEstateService - Puerto 15033

---

## 🧪 Escenarios de Prueba

### Escenario 1: Sin .env (Defaults) ✅ EN PROGRESO
**Comando:**
```powershell
# Sin archivo .env, solo valores por defecto de compose.yaml
docker-compose up -d
```

**Validación:**
- Todos los servicios deben iniciar
- JWT funciona con clave de desarrollo
- PostgreSQL usa password=password
- RabbitMQ usa guest/guest

### Escenario 2: Con .env Personalizado (PENDIENTE)
**Comando:**
```powershell
# Crear .env con valores custom
echo 'JWT__KEY=mi-super-secreto-personalizado' > .env
docker-compose up -d --force-recreate
```

**Validación:**
- AuthService debe usar el JWT key del .env
- Login debe generar tokens con la nueva clave

### Escenario 3: Secretos Inválidos (PENDIENTE)
**Comando:**
```powershell
# .env con secreto JWT inválido (muy corto)
echo 'JWT__KEY=abc' > .env
docker-compose up -d --force-recreate
```

**Validación:**
- AuthService debe fallar o advertir sobre clave insegura
- Logs deben indicar el problema claramente

---

## 📊 Matriz de Dependencias de Secretos

| Servicio | JWT__KEY | POSTGRES_PASSWORD | SENDGRID | TWILIO | STRIPE | RABBITMQ |
|----------|:--------:|:-----------------:|:--------:|:------:|:------:|:--------:|
| AuthService | ✅ CRÍTICO | ✅ CRÍTICO | ❌ | ❌ | ❌ | ✅ CRÍTICO |
| UserService | ✅ CRÍTICO | ✅ CRÍTICO | ❌ | ❌ | ❌ | ⚪ |
| RoleService | ✅ CRÍTICO | ✅ CRÍTICO | ❌ | ❌ | ❌ | ⚪ |
| NotificationService | ✅ CRÍTICO | ✅ CRÍTICO | ⚪ OPCIONAL | ⚪ OPCIONAL | ❌ | ✅ CRÍTICO |
| BillingService | ✅ CRÍTICO | ✅ CRÍTICO | ❌ | ❌ | ⚪ OPCIONAL | ⚪ |
| ... | TBD | TBD | TBD | TBD | TBD | TBD |

**Leyenda:**
- ✅ CRÍTICO: Servicio NO inicia sin este secreto
- ⚪ OPCIONAL: Funcionalidad degradada pero servicio inicia
- ❌ NO USA: Servicio no requiere este secreto

---

## 🔍 Comandos de Validación

### 1. Verificar todos los contenedores corriendo
```powershell
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | Select-String "Up"
```

**Esperado:** 48 contenedores (35 servicios + 13 bases de datos)

### 2. Health Check Masivo
```powershell
$services = @(
    @{Name="AuthService"; Port=15085},
    @{Name="Gateway"; Port=18443},
    @{Name="ErrorService"; Port=15083}
    # ... agregar todos
)

foreach ($svc in $services) {
    try {
        $response = Invoke-WebRequest "http://localhost:$($svc.Port)/health" -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $($svc.Name): OK" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "❌ $($svc.Name): FAILED" -ForegroundColor Red
    }
}
```

### 3. Verificar Variables de Entorno
```powershell
docker exec authservice printenv | Select-String "JWT__KEY"
```

**Esperado:**
```
JWT__KEY=clave-super-secreta-desarrollo-32-caracteres-aaa
```

### 4. Test de Login con JWT
```powershell
$body = @{
    email = "test@example.com"
    password = "Admin123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:15085/api/Auth/login" `
    -Method POST -Body $body -ContentType "application/json"

Write-Host "Token generado: $($response.token.Substring(0,20))..."
```

---

## 📈 Métricas de Éxito

| Métrica | Target | Actual |
|---------|:------:|:------:|
| **Servicios Compilados** | 35/35 | 🔄 TBD |
| **Contenedores Corriendo** | 48/48 | 🔄 TBD |
| **Health Checks OK** | 35/35 | 🔄 TBD |
| **Servicios con Secretos Hardcoded** | 0/35 | ✅ 0/35 |
| **Tiempo de Startup Total** | <5 min | 🔄 TBD |

---

## 🐛 Problemas Conocidos

### 1. Compilación Lenta
**Observado:** Servicios tardan ~2-3 minutos en compilar  
**Causa:** Primera compilación después de cambios en compose.yaml  
**Impacto:** Solo afecta al primer `docker-compose up`

### 2. RabbitMQ Connection Delays
**Observado:** Algunos servicios intentan conectar antes de que RabbitMQ esté listo  
**Solución:** Dependencia `depends_on: rabbitmq: condition: service_healthy`

---

## 📝 Próximas Acciones

1. ⏳ Esperar finalización de compilación (~5 minutos restantes)
2. ⏳ Ejecutar health check masivo
3. ⏳ Identificar servicios fallidos (si los hay)
4. ⏳ Corregir problemas de configuración
5. ⏳ Documentar matriz completa de dependencias
6. ⏳ Probar Escenario 2 (con .env personalizado)
7. ⏳ Completar reporte de Sprint 0.7.2

---

**Última Actualización:** 01 Enero 2026 00:20 GMT  
**Estado:** 🔄 Compilación en Progreso (26/39 etapas completadas)

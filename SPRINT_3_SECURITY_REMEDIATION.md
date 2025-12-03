# 🔒 Sprint 3 - Security Remediation
**Fecha de inicio**: 3 de diciembre de 2025  
**Duración estimada**: 4-6 horas  
**Objetivo**: Remediar vulnerabilidades críticas y altas identificadas en el escaneo de seguridad de Sprint 1

---

## 📊 Contexto

En Sprint 1 (US-1.7) se identificaron **54 vulnerabilidades** en las 6 imágenes Docker:
- **6 CRITICAL**: CVE-2023-45853 (zlib overflow) - `will_not_fix` por Debian
- **48 HIGH**: Git, OpenLDAP, Linux-PAM

Este sprint se enfoca en **reducir la superficie de ataque** implementando las acciones del plan de remediación.

---

## 🎯 Objetivos del Sprint

### Métricas de Éxito
- ✅ Reducir vulnerabilidades HIGH de 48 a ≤20
- ✅ Optimizar tamaño de imágenes Docker (reducción ≥30%)
- ✅ Implementar security contexts en contenedores
- ✅ Escaneo final con Trivy mostrando mejoras cuantificables

---

## 📋 User Stories

### US-3.1: Optimización de Imágenes Docker
**Prioridad**: CRÍTICA  
**Tiempo estimado**: 90 minutos

**Como** DevOps Engineer  
**Quiero** optimizar las imágenes Docker removiendo componentes innecesarios  
**Para** reducir la superficie de ataque y el tamaño de las imágenes

**Tareas**:
1. ✅ Remover Git de 4 imágenes (AuthService, Gateway, ErrorService, NotificationService)
   - Modificar Dockerfiles para eliminar instalación de Git
   - Verificar que servicios no requieren Git en runtime
   - **Beneficio esperado**: -4 vulnerabilidades HIGH por imagen (-16 total)

2. ✅ Implementar multi-stage builds en todos los Dockerfiles
   - Separar etapa de build de runtime
   - Copiar solo binarios necesarios
   - **Beneficio esperado**: Reducción 40-60% en tamaño

3. ✅ Migrar a imagen base Alpine para servicios pequeños
   - ConfigurationService: `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`
   - MessageBusService: `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`
   - **Beneficio esperado**: -50% vulnerabilidades, -40% tamaño

4. ✅ Remover bibliotecas innecesarias (libldap si no se usa)
   - Analizar dependencias de cada servicio
   - Agregar `RUN apt-get remove -y libldap-2.5-0 && apt-get autoremove -y`
   - **Beneficio esperado**: -2 vulnerabilidades HIGH por imagen

**Acceptance Criteria**:
- [ ] Las 6 imágenes se construyen sin errores
- [ ] AuthService, Gateway, ErrorService, NotificationService sin Git
- [ ] ConfigurationService y MessageBusService usan Alpine
- [ ] Tamaño promedio de imágenes reducido ≥30%
- [ ] Servicios desplegados funcionan correctamente

**Dockerfile Ejemplo Optimizado**:
```dockerfile
# ============ BUILD STAGE ============
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo archivos de proyecto primero (mejor cache)
COPY ["AuthService.Api/AuthService.Api.csproj", "AuthService.Api/"]
COPY ["AuthService.Application/AuthService.Application.csproj", "AuthService.Application/"]
COPY ["AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService.Infrastructure/"]

RUN dotnet restore "AuthService.Api/AuthService.Api.csproj"

# Copiar código fuente
COPY . .
WORKDIR "/src/AuthService.Api"
RUN dotnet build "AuthService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "AuthService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============ RUNTIME STAGE ============
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

# Crear usuario no-root
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Remover paquetes innecesarios para reducir vulnerabilidades
RUN apt-get update && \
    apt-get remove -y git git-man && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copiar binarios del build stage
COPY --from=build /app/publish .

# Cambiar a usuario no-root
USER appuser

EXPOSE 5000
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
```

---

### US-3.2: Implementar Security Contexts
**Prioridad**: ALTA  
**Tiempo estimado**: 60 minutos

**Como** Security Engineer  
**Quiero** configurar security contexts en los contenedores  
**Para** aplicar el principio de privilegio mínimo y hardening

**Tareas**:
1. ✅ Crear usuarios no-root en Dockerfiles
   ```dockerfile
   RUN groupadd -r appuser && useradd -r -g appuser appuser
   USER appuser
   ```

2. ✅ Actualizar docker-compose.yml con security options
   ```yaml
   security_opt:
     - no-new-privileges:true
   read_only: true
   tmpfs:
     - /tmp
     - /var/tmp
   cap_drop:
     - ALL
   cap_add:
     - NET_BIND_SERVICE
   ```

3. ✅ Configurar volúmenes de solo lectura donde sea posible
   ```yaml
   volumes:
     - ./config:/app/config:ro
   ```

4. ✅ Implementar health checks robustos
   ```yaml
   healthcheck:
     test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
     interval: 30s
     timeout: 10s
     retries: 3
     start_period: 40s
   ```

**Acceptance Criteria**:
- [ ] Todos los contenedores corren como usuarios no-root
- [ ] Security options aplicados en docker-compose.yml
- [ ] Health checks configurados y funcionando
- [ ] Servicios desplegados pasan validación de seguridad

---

### US-3.3: Escaneo y Validación de Dependencias .NET
**Prioridad**: ALTA  
**Tiempo estimado**: 45 minutos

**Como** Developer  
**Quiero** identificar y actualizar paquetes NuGet vulnerables  
**Para** eliminar vulnerabilidades conocidas en dependencias

**Tareas**:
1. ✅ Escanear dependencias de todos los servicios
   ```powershell
   dotnet list package --vulnerable --include-transitive
   ```

2. ✅ Actualizar paquetes con vulnerabilidades conocidas
   ```powershell
   dotnet add package <PackageName> --version <SafeVersion>
   ```

3. ✅ Configurar Central Package Management
   - Crear/actualizar `Directory.Packages.props`
   - Centralizar versiones de paquetes
   - Facilitar actualizaciones futuras

4. ✅ Agregar escaneo automatizado al pipeline
   ```yaml
   - task: DotNetCoreCLI@2
     displayName: 'Check vulnerable packages'
     inputs:
       command: 'custom'
       custom: 'list'
       arguments: 'package --vulnerable'
   ```

**Acceptance Criteria**:
- [ ] Todos los servicios escaneados
- [ ] Paquetes vulnerables actualizados o documentada justificación
- [ ] Directory.Packages.props configurado
- [ ] Documentación de dependencias actualizada

---

### US-3.4: Actualización de Imagen Base
**Prioridad**: MEDIA  
**Tiempo estimado**: 60 minutos

**Como** DevOps Engineer  
**Quiero** actualizar a imágenes base más recientes y seguras  
**Para** obtener parches de seguridad del sistema operativo

**Tareas**:
1. ✅ Evaluar opciones de imagen base
   - Debian 12 (bookworm) latest
   - Alpine 3.19 (menor superficie de ataque)
   - Ubuntu 24.04 (LTS)

2. ✅ Actualizar Dockerfiles con nueva imagen base
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.19
   # o
   FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim
   ```

3. ✅ Probar compatibilidad de servicios
   - Verificar que globalization funciona (ICU en Alpine)
   - Validar SSL/TLS
   - Confirmar conectividad a bases de datos

4. ✅ Documentar decisiones de imagen base
   - Crear matriz de compatibilidad
   - Justificar elección por servicio

**Acceptance Criteria**:
- [ ] Imagen base evaluada y seleccionada
- [ ] Dockerfiles actualizados
- [ ] Servicios funcionan correctamente con nueva base
- [ ] Tests de integración pasan

**Matriz de Recomendación**:
| Servicio | Imagen Base Recomendada | Justificación |
|----------|-------------------------|---------------|
| AuthService | aspnet:8.0-bookworm-slim | Requiere bibliotecas completas |
| Gateway | aspnet:8.0-bookworm-slim | Requiere bibliotecas completas |
| ErrorService | aspnet:8.0-alpine3.19 | Servicio simple, Alpine suficiente |
| NotificationService | aspnet:8.0-alpine3.19 | Servicio simple |
| ConfigurationService | aspnet:8.0-alpine3.19 | Ya optimizado |
| MessageBusService | aspnet:8.0-alpine3.19 | Ya optimizado |

---

### US-3.5: Configuración de Runtime Security
**Prioridad**: MEDIA  
**Tiempo estimado**: 45 minutos

**Como** Security Engineer  
**Quiero** implementar controles de seguridad en runtime  
**Para** detectar y prevenir comportamientos anómalos

**Tareas**:
1. ✅ Configurar AppArmor profiles (Linux)
   - Crear profiles restrictivos por servicio
   - Limitar acceso a filesystem
   - Restringir capabilities

2. ✅ Implementar resource limits
   ```yaml
   deploy:
     resources:
       limits:
         cpus: '0.5'
         memory: 512M
       reservations:
         cpus: '0.25'
         memory: 256M
   ```

3. ✅ Configurar logging de seguridad
   - Logs de acceso fallidos
   - Intentos de escalación de privilegios
   - Cambios en archivos críticos

4. ✅ Documentar políticas de seguridad
   - Crear SECURITY_POLICIES.md
   - Definir respuesta a incidentes
   - Establecer rotación de secretos

**Acceptance Criteria**:
- [ ] AppArmor profiles aplicados
- [ ] Resource limits configurados
- [ ] Logging de seguridad funcional
- [ ] SECURITY_POLICIES.md creado

---

### US-3.6: Escaneo Final y Validación
**Prioridad**: CRÍTICA  
**Tiempo estimado**: 30 minutos

**Como** DevOps Engineer  
**Quiero** ejecutar escaneo de seguridad final  
**Para** validar que las remediaciones fueron efectivas

**Tareas**:
1. ✅ Reconstruir todas las imágenes Docker
   ```powershell
   docker-compose -f docker-compose.yml build --no-cache
   ```

2. ✅ Ejecutar Trivy en todas las imágenes
   ```powershell
   foreach ($img in $images) {
       trivy image --severity HIGH,CRITICAL $img
   }
   ```

3. ✅ Generar reporte comparativo
   - Sprint 1 vs Sprint 3
   - Gráficas de reducción de vulnerabilidades
   - Análisis de mejoras

4. ✅ Actualizar documentación
   - SECURITY_SCAN_REPORT.md (nuevo reporte)
   - SPRINT3_COMPLETION_REPORT.md

**Acceptance Criteria**:
- [ ] Todas las imágenes escaneadas
- [ ] Vulnerabilidades HIGH reducidas ≥50%
- [ ] Reporte comparativo generado
- [ ] Documentación actualizada

**Formato de Reporte Comparativo**:
```markdown
## Sprint 1 vs Sprint 3 - Security Improvements

| Imagen | Sprint 1 (HIGH) | Sprint 3 (HIGH) | Reducción |
|--------|-----------------|-----------------|-----------|
| authservice | 9 | X | -Y% |
| gateway | 9 | X | -Y% |
| errorservice | 9 | X | -Y% |
| notificationservice | 9 | X | -Y% |
| configurationservice | 6 | X | -Y% |
| messagebusservice | 6 | X | -Y% |
| **TOTAL** | **48** | **X** | **-Y%** |

### Tamaño de Imágenes
| Imagen | Sprint 1 | Sprint 3 | Reducción |
|--------|----------|----------|-----------|
| authservice | 4.91GB | XGB | -Y% |
| gateway | 4.98GB | XGB | -Y% |
| ... | ... | ... | ... |
```

---

## 📊 Cronograma de Ejecución

### Sesión 1: Optimización de Imágenes (2 horas)
- **00:00 - 01:30**: US-3.1 - Optimización de Dockerfiles
  - Remover Git
  - Implementar multi-stage builds
  - Migrar a Alpine
- **01:30 - 02:00**: US-3.2 - Security Contexts
  - Crear usuarios no-root
  - Configurar docker-compose.yml

### Sesión 2: Dependencias y Base Image (1.5 horas)
- **00:00 - 00:45**: US-3.3 - Escaneo de dependencias .NET
  - Ejecutar `dotnet list package --vulnerable`
  - Actualizar paquetes
- **00:45 - 01:30**: US-3.4 - Actualizar imagen base
  - Evaluar opciones
  - Actualizar Dockerfiles
  - Probar compatibilidad

### Sesión 3: Runtime Security y Validación (1.5 horas)
- **00:00 - 00:45**: US-3.5 - Runtime security
  - AppArmor profiles
  - Resource limits
  - SECURITY_POLICIES.md
- **00:45 - 01:15**: US-3.6 - Escaneo final
  - Reconstruir imágenes
  - Ejecutar Trivy
- **01:15 - 01:30**: Generación de reportes
  - Reporte comparativo
  - Actualizar documentación

---

## ✅ Definition of Done

### Para cada User Story:
- [ ] Código implementado y testeado
- [ ] Builds exitosos sin warnings
- [ ] Servicios desplegados y funcionando
- [ ] Documentación actualizada
- [ ] Code review completado (si aplica)

### Para el Sprint Completo:
- [ ] Vulnerabilidades HIGH reducidas ≥50%
- [ ] Tamaño de imágenes reducido ≥30%
- [ ] Todos los servicios pasan health checks
- [ ] Security contexts implementados
- [ ] Escaneo final de Trivy completado
- [ ] Reporte comparativo Sprint 1 vs 3 generado
- [ ] SPRINT3_COMPLETION_REPORT.md creado
- [ ] Git commits y push realizados

---

## 🎯 Métricas de Éxito

### Vulnerabilidades
- **Objetivo**: Reducir vulnerabilidades HIGH de 48 a ≤20 (≥58% reducción)
- **Meta stretch**: Reducir a ≤15 (≥69% reducción)
- **CRITICAL**: Documentar CVE-2023-45853 (zlib) y plan de monitoreo

### Tamaño de Imágenes
- **Objetivo**: Reducir tamaño promedio ≥30%
- **Meta stretch**: Reducir ≥40%
- **Baseline Sprint 1**: 2.75GB promedio

### Seguridad
- [ ] 100% de contenedores con usuarios no-root
- [ ] 100% de servicios con security contexts
- [ ] 100% de dependencias .NET escaneadas
- [ ] 0 vulnerabilidades CRITICAL sin documentar

---

## 📁 Entregables

1. **Dockerfiles optimizados** (6 archivos)
   - `backend/AuthService/AuthService.Api/Dockerfile`
   - `backend/Gateway/Gateway.Api/Dockerfile`
   - `backend/ErrorService/ErrorService.Api/Dockerfile`
   - `backend/NotificationService/NotificationService.Api/Dockerfile`
   - `backend/ConfigurationService/ConfigurationService.Api/Dockerfile`
   - `backend/MessageBusService/MessageBusService.Api/Dockerfile`

2. **docker-compose.yml actualizado** con security options

3. **Directory.Packages.props** centralizado

4. **SECURITY_POLICIES.md** - Políticas de seguridad

5. **SECURITY_SCAN_REPORT_SPRINT3.md** - Reporte final de escaneo

6. **SPRINT3_COMPLETION_REPORT.md** - Reporte de completitud

7. **SECURITY_IMPROVEMENTS_COMPARISON.md** - Reporte comparativo Sprint 1 vs 3

---

## 🔄 Siguientes Pasos (Post-Sprint 3)

1. **Sprint 4**: CI/CD Pipeline con security scanning automatizado
2. **Sprint 5**: Implementar Vault integration en todos los servicios
3. **Sprint 6**: Runtime security monitoring (Falco/Sysdig)
4. **Sprint 7**: Penetration testing y auditoría externa

---

## 📚 Referencias

- [Docker Security Best Practices](https://docs.docker.com/engine/security/)
- [OWASP Container Security](https://owasp.org/www-project-docker-top-10/)
- [CIS Docker Benchmark](https://www.cisecurity.org/benchmark/docker)
- [Trivy Documentation](https://aquasecurity.github.io/trivy/)
- [.NET Security Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/security/)

---

**Última actualización**: 3 de diciembre de 2025  
**Próxima revisión**: Al completar Sprint 3

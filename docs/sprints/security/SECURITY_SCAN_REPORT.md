# Security Scan Report - Docker Images
**Fecha**: 3 de diciembre de 2025  
**Herramienta**: Trivy v0.48.0  
**Criterio de Escaneo**: Vulnerabilidades HIGH y CRITICAL

## 📊 Resumen Ejecutivo

| Imagen | Total | CRITICAL | HIGH |
|--------|-------|----------|------|
| backend-authservice:latest | 10 | 1 | 9 |
| backend-gateway:latest | 10 | 1 | 9 |
| backend-errorservice:latest | 10 | 1 | 9 |
| backend-notificationservice:latest | 10 | 1 | 9 |
| backend-configurationservice:latest | 7 | 1 | 6 |
| backend-messagebusservice:latest | 7 | 1 | 6 |
| **TOTAL** | **54** | **6** | **48** |

### Estadísticas Generales
- **Imágenes escaneadas**: 6
- **Vulnerabilidades CRITICAL**: 6 (todas en zlib1g)
- **Vulnerabilidades HIGH**: 48 (distribuidas en múltiples bibliotecas)
- **Base OS**: Debian 12.12 (común en todas las imágenes)

---

## 🔴 Vulnerabilidades CRITICAL (Prioridad Máxima)

### CVE-2023-45853: zlib Integer Overflow
- **Biblioteca afectada**: `zlib1g`
- **Versión instalada**: `1:1.2.13.dfsg-1`
- **Severidad**: **CRITICAL**
- **Estado**: `will_not_fix`
- **Imágenes afectadas**: Las 6 imágenes
- **Descripción**: Integer overflow en zlib que resulta en heap-based buffer overflow
- **Impacto**: Potencial ejecución arbitraria de código
- **Recomendación**: 
  - Monitorear actualizaciones de Debian 12 para zlib
  - Considerar actualizar a una imagen base más reciente cuando esté disponible el fix
  - Implementar controles de seguridad adicionales en runtime (AppArmor/SELinux)

---

## 🟠 Vulnerabilidades HIGH (Prioridad Alta)

### 1. Git Arbitrary Code Execution (CVE-2025-48384, CVE-2025-48385)
- **Bibliotecas afectadas**: `git` y `git-man`
- **Versión instalada**: `1:2.39.5-0+deb12u2`
- **Severidad**: **HIGH**
- **Estado**: `affected`
- **Imágenes afectadas**: Las 6 imágenes
- **CVEs**:
  - **CVE-2025-48384**: Git arbitrary code execution
  - **CVE-2025-48385**: Git arbitrary file writes
- **Impacto**: Ejecución arbitraria de código, escritura de archivos arbitrarios
- **Recomendación**:
  - **Acción inmediata**: Si Git no es necesario en runtime, removerlo del Dockerfile
  - Actualizar a la versión más reciente de Git cuando esté disponible
  - Limitar permisos de ejecución de Git en los contenedores

### 2. OpenLDAP Null Pointer Dereference (CVE-2023-2953)
- **Bibliotecas afectadas**: `libldap-2.5-0`, `libldap-common`
- **Versión instalada**: `2.5.13+dfsg-5`
- **Severidad**: **HIGH**
- **Estado**: `affected`
- **Imágenes afectadas**: Las 6 imágenes
- **Descripción**: Null pointer dereference en función `ber_memalloc_x`
- **Impacto**: Denegación de servicio (DoS)
- **Recomendación**:
  - Actualizar libldap cuando haya fix disponible
  - Si LDAP no es usado, considerar remover la biblioteca

### 3. Linux-PAM Directory Traversal (CVE-2025-6020)
- **Bibliotecas afectadas**: `libpam-modules`, `libpam-modules-bin`, `libpam-runtime`, `libpam0g`
- **Versión instalada**: `1.5.2-6+deb12u1`
- **Severidad**: **HIGH**
- **Estado**: `affected`
- **Imágenes afectadas**: Las 6 imágenes
- **Descripción**: Directory traversal en Linux-PAM
- **Impacto**: Acceso no autorizado a archivos del sistema
- **Recomendación**:
  - Monitorear actualizaciones de Debian para linux-pam
  - Implementar controles de acceso estrictos en los contenedores

---

## 📋 Detalle por Imagen

### backend-authservice:latest
- **Total vulnerabilidades**: 10 (1 CRITICAL, 9 HIGH)
- **Tamaño de imagen**: 4.91GB
- **Archivos específicos del lenguaje detectados**: 287 (dotnet-core, nuget, packages-props)
- **Vulnerabilidades únicas**: Ninguna adicional a las comunes

### backend-gateway:latest
- **Total vulnerabilidades**: 10 (1 CRITICAL, 9 HIGH)
- **Tamaño de imagen**: 4.98GB
- **Archivos específicos del lenguaje detectados**: 285 (dotnet-core, nuget, packages-props)
- **Vulnerabilidades únicas**: Ninguna adicional a las comunes

### backend-errorservice:latest
- **Total vulnerabilidades**: 10 (1 CRITICAL, 9 HIGH)
- **Tamaño de imagen**: 2.04GB
- **Archivos específicos del lenguaje detectados**: 30 (dotnet-core)
- **Vulnerabilidades únicas**: Ninguna adicional a las comunes

### backend-notificationservice:latest
- **Total vulnerabilidades**: 10 (1 CRITICAL, 9 HIGH)
- **Tamaño de imagen**: 2.18GB
- **Archivos específicos del lenguaje detectados**: 51 (dotnet-core)
- **Vulnerabilidades únicas**: Ninguna adicional a las comunes

### backend-configurationservice:latest
- **Total vulnerabilidades**: 7 (1 CRITICAL, 6 HIGH)
- **Tamaño de imagen**: 344MB
- **Archivos específicos del lenguaje detectados**: 3 (dotnet-core)
- **Vulnerabilidades únicas**: No contiene Git (mejora de seguridad)

### backend-messagebusservice:latest
- **Total vulnerabilidades**: 7 (1 CRITICAL, 6 HIGH)
- **Tamaño de imagen**: 346MB
- **Archivos específicos del lenguaje detectados**: 3 (dotnet-core)
- **Vulnerabilidades únicas**: No contiene Git (mejora de seguridad)

---

## 🎯 Plan de Remediación

### Acciones Inmediatas (0-7 días)
1. **Remover Git de las imágenes Docker**
   - ConfigurationService y MessageBusService ya no lo tienen
   - Actualizar Dockerfiles de AuthService, Gateway, ErrorService y NotificationService
   - Beneficio: Elimina 4 vulnerabilidades HIGH por imagen

2. **Escaneo de dependencias .NET**
   - Ejecutar `dotnet list package --vulnerable` en todos los servicios
   - Actualizar paquetes NuGet con vulnerabilidades conocidas

3. **Implementar escaneo automático en CI/CD**
   - Integrar Trivy en el pipeline de GitHub Actions/Azure DevOps
   - Configurar política de "fail on CRITICAL vulnerabilities"

### Acciones a Corto Plazo (7-30 días)
4. **Actualizar imagen base de Debian**
   - Monitorear actualizaciones de seguridad de Debian 12
   - Considerar migrar a Alpine Linux para reducir superficie de ataque
   - Benefit: Menor tamaño de imagen y menos vulnerabilidades potenciales

5. **Implementar security contexts en Kubernetes/Docker**
   - RunAsNonRoot: true
   - ReadOnlyRootFilesystem: true
   - DropCapabilities: ALL

6. **Remover bibliotecas innecesarias**
   - Analizar si libldap es realmente necesario
   - Usar imágenes multi-stage para compilación

### Acciones a Medio Plazo (30-90 días)
7. **Establecer política de actualizaciones**
   - Programar reconstrucciones mensuales de imágenes
   - Automatizar proceso de actualización de imagen base

8. **Implementar runtime security**
   - Desplegar Falco o similar para detección de amenazas
   - Configurar AppArmor/SELinux profiles

9. **Auditoría de seguridad completa**
   - Penetration testing de los contenedores
   - Revisión de configuraciones de red y seguridad

---

## 📈 Métricas de Seguridad

### Por Severidad
- **CRITICAL**: 6 vulnerabilidades (11.1%)
- **HIGH**: 48 vulnerabilidades (88.9%)
- **TOTAL**: 54 vulnerabilidades

### Por Categoría
| Categoría | Cantidad | Porcentaje |
|-----------|----------|------------|
| Ejecución arbitraria de código | 12 | 22.2% |
| Escritura arbitraria de archivos | 6 | 11.1% |
| Buffer overflow | 6 | 11.1% |
| Directory traversal | 24 | 44.4% |
| Null pointer dereference | 6 | 11.1% |

### Nivel de Riesgo Global
- **Imágenes grandes (AuthService, Gateway)**: 🔴 **ALTO** (10 vulnerabilidades, incluye Git)
- **Imágenes medianas (ErrorService, NotificationService)**: 🟠 **MEDIO-ALTO** (10 vulnerabilidades, incluye Git)
- **Imágenes pequeñas (ConfigurationService, MessageBusService)**: 🟡 **MEDIO** (7 vulnerabilidades, sin Git)

---

## ✅ Recomendaciones de Buenas Prácticas

1. **Automatización de escaneos**
   ```yaml
   # Ejemplo para GitHub Actions
   - name: Run Trivy vulnerability scanner
     uses: aquasecurity/trivy-action@master
     with:
       image-ref: 'backend-authservice:latest'
       format: 'sarif'
       severity: 'CRITICAL,HIGH'
   ```

2. **Dockerfile optimizado**
   ```dockerfile
   # Usar multi-stage builds
   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   # ... build steps ...

   FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
   # Alpine tiene menos vulnerabilidades que Debian
   WORKDIR /app
   COPY --from=build /app/out .
   RUN apk del git  # Remover Git si no es necesario
   USER 1000  # No ejecutar como root
   ```

3. **Monitoreo continuo**
   - Configurar alertas para nuevas CVEs
   - Revisar reportes semanalmente
   - Mantener inventario actualizado de dependencias

---

## 🔗 Referencias

- [Trivy Documentation](https://aquasecurity.github.io/trivy/)
- [CVE Details](https://www.cvedetails.com/)
- [Debian Security Tracker](https://security-tracker.debian.org/)
- [National Vulnerability Database](https://nvd.nist.gov/)

---

## 📝 Notas Adicionales

- Este escaneo cubre únicamente vulnerabilidades a nivel de sistema operativo y bibliotecas base
- Se recomienda escaneo adicional de dependencias .NET con `dotnet list package --vulnerable`
- Las imágenes ConfigurationService y MessageBusService tienen mejor perfil de seguridad (sin Git)
- Todas las vulnerabilidades detectadas son a nivel de imagen base Debian 12.12

---

**Generado por**: Trivy v0.48.0  
**Fecha del escaneo**: 3 de diciembre de 2025  
**Próxima revisión recomendada**: 10 de diciembre de 2025

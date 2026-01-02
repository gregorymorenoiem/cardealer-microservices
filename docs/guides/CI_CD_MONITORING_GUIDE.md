# 🚀 Guía de Monitoreo CI/CD - CarDealer Microservices

> **Última actualización**: 4 de Diciembre 2025  
> **Push realizado**: `7686006..8473e7f main -> main`  
> **Commits subidos**: 5 commits (Sprints 13-17)

---

## 📋 Resumen del Push

### Commits Incluidos

| Commit | Sprint | Descripción |
|--------|--------|-------------|
| `5067c61` | Sprint 13 | Seguridad & Autorización |
| `8870aa0` | Sprint 14 | Test Coverage (43 nuevos tests) |
| `71f98e3` | Sprint 15 | Jobs & Automatización |
| `b3ed295` | Sprint 16 | Integración & Contratos |
| `8473e7f` | Sprint 17 | ClamAV Integration |

---

## 🔄 ¿Qué Pasa Después del Push?

### 1️⃣ GitHub Recibe el Push (Inmediato)

```
Tu máquina local → GitHub Repository (gmorenotrade/cardealer-microservices)
```

- GitHub actualiza la rama `main`
- Se disparan los **webhooks** configurados
- Se inician los **GitHub Actions workflows**

### 2️⃣ GitHub Actions se Activan (0-30 segundos)

Los workflows definidos en `.github/workflows/` se ejecutan automáticamente:

| Workflow | Trigger | Descripción |
|----------|---------|-------------|
| `ci.yml` | push a main | Build + Tests de todos los microservicios |
| `docker-build.yml` | push a main | Construcción de imágenes Docker |
| `security-scan.yml` | push a main | Escaneo de vulnerabilidades (Trivy) |

### 3️⃣ Pipeline de CI (2-15 minutos)

```
┌─────────────────────────────────────────────────────────────────┐
│                      GITHUB ACTIONS PIPELINE                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐      │
│  │ Checkout │ → │ Restore  │ → │  Build   │ → │  Test    │      │
│  │   Code   │   │ Packages │   │ Solution │   │  1483+   │      │
│  └──────────┘   └──────────┘   └──────────┘   └──────────┘      │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                    MATRIX BUILD (25 servicios)            │   │
│  │  ErrorService | AuthService | NotificationService | ...   │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐                     │
│  │  Docker  │ → │  Trivy   │ → │  Report  │                     │
│  │  Build   │   │  Scan    │   │ Generate │                     │
│  └──────────┘   └──────────┘   └──────────┘                     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 4️⃣ Resultados Posibles

| Estado | Significado | Acción |
|--------|-------------|--------|
| ✅ **Success** | Todo pasó correctamente | Listo para deploy |
| ❌ **Failure** | Algo falló (build/tests) | Revisar logs |
| ⚠️ **Warning** | Vulnerabilidades encontradas | Revisar security report |

---

## 📊 Cómo Monitorear el Pipeline

### Opción 1: GitHub Web UI (Recomendado)

1. **Ir a GitHub Actions**:
   ```
   https://github.com/gmorenotrade/cardealer-microservices/actions
   ```

2. **Ver el workflow en ejecución**:
   - Buscar el commit más reciente (`8473e7f`)
   - Click en el workflow para ver detalles

3. **Ver logs en tiempo real**:
   - Expandir cada job para ver los steps
   - Los logs se actualizan automáticamente

### Opción 2: GitHub CLI (Terminal)

```powershell
# Ver workflows recientes
gh run list --limit 5

# Ver estado del workflow más reciente
gh run view

# Ver logs de un workflow específico
gh run view <run-id> --log

# Monitorear en tiempo real
gh run watch
```

### Opción 3: API de GitHub

```powershell
# Obtener estado de workflows
Invoke-RestMethod -Uri "https://api.github.com/repos/gmorenotrade/cardealer-microservices/actions/runs?per_page=5" `
  -Headers @{Authorization = "Bearer $env:GITHUB_TOKEN"}
```

---

## 🔍 Monitoreo Detallado por Etapa

### Build Stage

**Qué buscar**:
- `dotnet restore` exitoso
- `dotnet build` sin errores
- Tiempo de compilación < 5 minutos

**Posibles errores**:
```
error CS0234: The type or namespace name 'X' does not exist
error CS1061: 'Type' does not contain a definition for 'Method'
```

### Test Stage

**Qué buscar**:
- Todos los 1,483+ tests pasando
- Coverage reports generados
- Sin tests skipped sin razón

**Comando para ver resultado**:
```
Total tests: 1483
     Passed: 1483
     Failed: 0
     Skipped: 0
```

### Docker Build Stage

**Qué buscar**:
- 25 imágenes construidas exitosamente
- Tamaño de imágenes < 500MB cada una
- Tags correctos aplicados

**Servicios en matrix build**:
```
errorservice, authservice, notificationservice, auditservice,
mediaservice, filesstorageservice, schedulerservice, searchservice,
healthcheckservice, featuretoggleservice, configurationservice,
cacheservice, messagebusservice, servicediscovery, gateway,
loggingservice, tracingservice, idempotencyservice, ratelimitingservice,
backupdrservice, adminservice, roleservice, userservice,
contactservice
```

### Security Scan Stage

**Qué buscar**:
- Sin vulnerabilidades CRITICAL
- Mínimas vulnerabilidades HIGH
- Report generado en artifacts

---

## 📈 Métricas a Monitorear

### Tiempos Esperados

| Etapa | Tiempo Normal | Alerta Si > |
|-------|---------------|-------------|
| Checkout | 5-10s | 30s |
| Restore | 30-60s | 2min |
| Build | 2-4min | 8min |
| Tests | 3-6min | 10min |
| Docker Build | 5-10min | 15min |
| Security Scan | 2-5min | 10min |
| **Total** | **12-20min** | **30min** |

### Indicadores de Salud

| Métrica | Saludable | Preocupante |
|---------|-----------|-------------|
| Build Success Rate | > 95% | < 90% |
| Test Pass Rate | 100% | < 100% |
| Avg Pipeline Time | < 15min | > 25min |
| Critical Vulns | 0 | > 0 |

---

## 🔔 Configurar Notificaciones

### Email Notifications

GitHub envía notificaciones automáticas a tu email cuando:
- Un workflow falla
- Un workflow se recupera de un fallo

**Configurar en**: Settings → Notifications → Actions

### Slack Integration (Opcional)

Agregar a `.github/workflows/ci.yml`:
```yaml
- name: Notify Slack
  if: failure()
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

### Teams Integration (Opcional)

```yaml
- name: Notify Teams
  if: always()
  uses: jdcargile/ms-teams-notification@v1
  with:
    webhook_url: ${{ secrets.TEAMS_WEBHOOK }}
```

---

## 🛠️ Troubleshooting

### El Pipeline no se Dispara

1. Verificar que el workflow tiene el trigger correcto:
   ```yaml
   on:
     push:
       branches: [main]
   ```

2. Verificar que no hay errores de sintaxis en el YAML

3. Verificar permisos del repositorio

### Build Falla

1. **Revisar logs del step fallido**
2. **Reproducir localmente**:
   ```powershell
   cd backend
   dotnet restore
   dotnet build CarDealer.sln
   ```

3. **Verificar dependencias**:
   ```powershell
   dotnet list package --outdated
   ```

### Tests Fallan

1. **Identificar tests fallidos** en el log
2. **Ejecutar localmente**:
   ```powershell
   dotnet test CarDealer.sln --filter "FullyQualifiedName~TestName"
   ```

3. **Verificar si es flaky test** (intermitente)

### Docker Build Falla

1. **Revisar Dockerfile** del servicio afectado
2. **Probar localmente**:
   ```powershell
   docker-compose -f backend/docker-compose.yml build <service-name>
   ```

---

## 📱 Dashboard Rápido

### URLs Importantes

| Recurso | URL |
|---------|-----|
| **GitHub Actions** | https://github.com/gmorenotrade/cardealer-microservices/actions |
| **Pull Requests** | https://github.com/gmorenotrade/cardealer-microservices/pulls |
| **Security Advisories** | https://github.com/gmorenotrade/cardealer-microservices/security |
| **Insights** | https://github.com/gmorenotrade/cardealer-microservices/pulse |

### Comandos PowerShell Útiles

```powershell
# Ver estado actual del repo
git log --oneline -5
git status

# Ver workflows recientes (requiere gh CLI)
gh run list

# Ver workflow actual
gh run watch

# Descargar artifacts
gh run download <run-id>
```

---

## ✅ Checklist Post-Push

- [ ] Verificar que GitHub Actions se disparó
- [ ] Monitorear build stage (2-4 min)
- [ ] Verificar que todos los tests pasan
- [ ] Revisar Docker build de los 25 servicios
- [ ] Verificar security scan sin CRITICAL
- [ ] Confirmar que todo está ✅ verde

---

## 📞 Próximos Pasos

Una vez que el pipeline termine exitosamente:

1. **Las imágenes Docker están listas** para deployment
2. **Puedes hacer deploy** a ambiente de staging/producción
3. **Los tests confirman** que los cambios no rompen nada

### Para Deploy Manual:
```powershell
cd backend
docker-compose pull
docker-compose up -d
```

### Para Deploy Automatizado:
Configurar CD pipeline adicional que:
1. Push imágenes a container registry
2. Actualiza Kubernetes/Docker Swarm
3. Ejecuta health checks post-deploy

---

*Documento generado automáticamente - CarDealer Microservices CI/CD*

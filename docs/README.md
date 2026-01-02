# 📚 Documentación CarDealer Microservices

**Última actualización:** 2 Enero 2026

---

## 📁 Estructura de Carpetas

```
docs/
├── architecture/     # Diseño y arquitectura del sistema (6 docs)
├── guides/          # Tutoriales y guías de implementación (8 docs)
├── reports/         # Reportes generales y resúmenes (4 docs)
├── sprints/         # Documentación de sprints por área
│   ├── audit/       # Auditoría de microservicios Q1 2026 (24 docs) ⭐
│   ├── backend/     # Sprints de backend .NET (21 docs)
│   ├── frontend/    # Sprints de frontend React (22 docs)
│   ├── mobile/      # Sprints de mobile Flutter (4 docs)
│   └── security/    # Seguridad y vulnerabilidades (5 docs)
└── legacy/          # Documentos obsoletos
```

**Total:** 94 documentos organizados

---

## 🎯 Documentos Más Importantes

### Para Nuevos Desarrolladores
| Documento | Ubicación | Descripción |
|-----------|-----------|-------------|
| Arquitectura General | [architecture/ARQUITECTURA_MICROSERVICIOS.md](architecture/ARQUITECTURA_MICROSERVICIOS.md) | Visión general del sistema |
| Guía Docker | [guides/DOCKER_DESKTOP_INSTALLATION_GUIDE.md](guides/DOCKER_DESKTOP_INSTALLATION_GUIDE.md) | Setup del entorno |
| Deploy Guide | [guides/DEPLOY_DIGITALOCEAN_GUIDE.md](guides/DEPLOY_DIGITALOCEAN_GUIDE.md) | Despliegue en producción |

### Para el Equipo de DevOps
| Documento | Ubicación | Descripción |
|-----------|-----------|-------------|
| CI/CD Guide | [guides/CI_CD_MONITORING_GUIDE.md](guides/CI_CD_MONITORING_GUIDE.md) | Pipelines y monitoreo |
| Vault Setup | [guides/VAULT_INTEGRATION_GUIDE.md](guides/VAULT_INTEGRATION_GUIDE.md) | Gestión de secretos |
| Plan Q1 2026 | [sprints/audit/Q1_2026_PRODUCTION_SPRINT_PLAN.md](sprints/audit/Q1_2026_PRODUCTION_SPRINT_PLAN.md) | Roadmap a producción |

### Estado Actual del Proyecto
| Documento | Ubicación | Descripción |
|-----------|-----------|-------------|
| Reporte Final Auditoría | [sprints/audit/MICROSERVICES_AUDIT_FINAL_REPORT.md](sprints/audit/MICROSERVICES_AUDIT_FINAL_REPORT.md) | Estado de 35 servicios |
| Plan Remediación | [sprints/audit/SPRINT_8.2_REMEDIATION_PLAN.md](sprints/audit/SPRINT_8.2_REMEDIATION_PLAN.md) | Acciones correctivas |
| Sprint Plan Auditoría | [sprints/audit/MICROSERVICES_AUDIT_SPRINT_PLAN.md](sprints/audit/MICROSERVICES_AUDIT_SPRINT_PLAN.md) | Plan maestro completado |

---

## 📂 Detalle por Carpeta

### 🏛️ architecture/ (6 documentos)
Diseño de alto nivel, decisiones arquitectónicas, patrones.
- Arquitectura de microservicios
- Sistema multi-tenant
- Marketplace multi-vertical
- SaaS ERP design

### 📖 guides/ (8 documentos)
Tutoriales paso a paso para tareas específicas.
- Docker setup
- Deploy a DigitalOcean
- Multi-database configuration
- CI/CD pipelines
- Vault para secretos

### 📊 reports/ (4 documentos)
Reportes de estado y análisis general.
- Overview de sprints
- Tareas pendientes
- Performance reports

### 🔍 sprints/audit/ (24 documentos) ⭐ PRINCIPAL
Auditoría completa de microservicios (Dic 2025 - Ene 2026).
- **MICROSERVICES_AUDIT_SPRINT_PLAN.md** - Plan maestro (100% completado)
- **Q1_2026_PRODUCTION_SPRINT_PLAN.md** - Roadmap a producción
- Reportes de fases 0-8
- Documentación de fixes aplicados

### ⚙️ sprints/backend/ (21 documentos)
Sprints de desarrollo backend .NET.
- Mejoras de endpoints
- Refactoring plans
- Implementaciones multi-tenant

### 🖥️ sprints/frontend/ (22 documentos)
Sprints de desarrollo frontend React.
- Home redesign
- Monetización
- i18n
- Social features

### 📱 sprints/mobile/ (4 documentos)
Sprints de desarrollo mobile Flutter.
- App development plan
- Mobile sprints completion

### 🔐 sprints/security/ (5 documentos)
Seguridad, vulnerabilidades y remediación.
- Security policies
- Scan reports
- Vulnerability fixes

---

## 🔍 Búsqueda Rápida

### Por Tema

| Tema | Buscar en |
|------|-----------|
| Docker/Containers | `guides/`, `sprints/audit/` |
| Autenticación/JWT | `sprints/backend/`, `sprints/security/` |
| Base de datos | `guides/GUIA_MULTI_DATABASE_CONFIGURATION.md` |
| Kubernetes | `sprints/audit/Q1_2026_PRODUCTION_SPRINT_PLAN.md` |
| Frontend React | `sprints/frontend/` |
| Mobile Flutter | `sprints/mobile/` |
| Seguridad | `sprints/security/` |

### Por Sprint Number

Los sprints están nombrados con el patrón `SPRINT{N}_*.md` o `SPRINT_{N}*.md`.

---

## 📝 Convenciones de Nomenclatura

| Prefijo | Significado |
|---------|-------------|
| `SPRINT_` | Sprint de auditoría/mejora |
| `SPRINT{N}_` | Sprint histórico de desarrollo |
| `*_GUIDE.md` | Tutorial/guía paso a paso |
| `*_PLAN.md` | Plan de trabajo |
| `*_REPORT.md` | Reporte de estado |
| `*_COMPLETION*.md` | Sprint completado |

---

*Documentación organizada: 2 Enero 2026*

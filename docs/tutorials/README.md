# 📚 Tutoriales - Deploy en Digital Ocean DOKS

Guía completa para aprender a desplegar microservicios .NET + React en Kubernetes (Digital Ocean).

---

## 🎯 Índice de Tutoriales

### Nivel 1: Fundamentos (Principiante)
| # | Tutorial | Duración | Estado |
|---|----------|----------|--------|
| 1 | [kubectl Básico para DOKS](./01-kubectl-basico.md) | 30 min | ⬜ Pendiente |
| 2 | [Gestión de Pods y Deployments](./02-pods-deployments.md) | 45 min | ⬜ Pendiente |
| 3 | [ConfigMaps y Secrets](./03-configmaps-secrets.md) | 40 min | ⬜ Pendiente |
| 4 | [Logs y Debugging](./04-logs-debugging.md) | 35 min | ⬜ Pendiente |

### Nivel 2: Infraestructura (Intermedio)
| # | Tutorial | Duración | Estado |
|---|----------|----------|--------|
| 5 | [DNS y SSL con Let's Encrypt](./05-dns-ssl-letsencrypt.md) | 50 min | ⬜ Pendiente |
| 6 | [Load Balancer y Services](./06-loadbalancer-services.md) | 45 min | ⬜ Pendiente |
| 7 | [Digital Ocean Container Registry](./07-docr-registry.md) | 40 min | ⬜ Pendiente |
| 8 | [PostgreSQL en Kubernetes](./08-postgresql-kubernetes.md) | 55 min | ⬜ Pendiente |

### Nivel 3: API Gateway y Microservicios (Avanzado)
| # | Tutorial | Duración | Estado |
|---|----------|----------|--------|
| 9 | [Ocelot API Gateway](./09-ocelot-gateway.md) | 60 min | ⬜ Pendiente |
| 10 | [Troubleshooting de Rutas](./10-troubleshooting-rutas.md) | 45 min | ⬜ Pendiente |
| 11 | [Actualizaciones sin Downtime](./11-zero-downtime.md) | 40 min | ⬜ Pendiente |

### Nivel 4: CI/CD y Automatización (Experto)
| # | Tutorial | Duración | Estado |
|---|----------|----------|--------|
| 12 | [GitHub Actions para DOKS](./12-github-actions-doks.md) | 70 min | ⬜ Pendiente |
| 13 | [Pipeline CI/CD Completo](./13-pipeline-cicd-completo.md) | 90 min | ⬜ Pendiente |
| 14 | [Monitoreo y Rollbacks](./14-monitoreo-rollbacks.md) | 45 min | ⬜ Pendiente |

### Nivel 5: Proyecto Completo (Masterclass)
| # | Tutorial | Duración | Estado |
|---|----------|----------|--------|
| 15 | [Deploy Completo: Código a Producción](./15-masterclass-deploy-completo.md) | 3-4 hrs | ⬜ Pendiente |

---

## 🚀 Orden Recomendado

```
Nivel 1 (1-4) → Nivel 2 (5-8) → Nivel 3 (9-11) → Nivel 4 (12-14) → Nivel 5 (15)
```

## 📋 Requisitos Previos

- Cuenta en Digital Ocean
- Docker instalado localmente
- kubectl instalado
- doctl (CLI de Digital Ocean) instalado
- GitHub account con acceso a Actions

## 🔧 Proyecto de Referencia

Todos los tutoriales usan como ejemplo el proyecto **cardealer-microservices**:
- Backend: .NET 8 Microservicios
- Frontend: React 19 + Vite
- Base de datos: PostgreSQL
- API Gateway: Ocelot
- Cluster: Digital Ocean Kubernetes (DOKS)

---

*Última actualización: Enero 2026*

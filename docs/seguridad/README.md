# 🔒 Documentación de Seguridad — OKLA

Índice centralizado de toda la documentación de seguridad del proyecto OKLA.

**Última actualización:** Febrero 14, 2026

---

## 📁 Documentos en esta carpeta

| #   | Documento                                                  | Descripción                                                              | Fecha    |
| --- | ---------------------------------------------------------- | ------------------------------------------------------------------------ | -------- |
| 1   | [SERVER_ACTIONS_SECURITY.md](./SERVER_ACTIONS_SECURITY.md) | Server Actions de Next.js para proteger endpoints críticos en el browser | Feb 2026 |
| 2   | [BFF_PATTERN.md](./BFF_PATTERN.md)                         | Backend for Frontend — Gateway sin IP pública                            | Feb 2026 |
| 3   | [NETWORK_ISOLATION.md](./NETWORK_ISOLATION.md)             | NetworkPolicies de Kubernetes y aislamiento de red                       | Feb 2026 |

---

## 🏗️ Arquitectura de seguridad (3 capas)

```
┌─────────────────────────────────────────────────────────────────┐
│                    CAPA 3: Server Actions                       │
│  Mutaciones críticas (auth, pagos, KYC) se ejecutan en el      │
│  servidor Next.js. El browser NO ve endpoints ni datos.         │
│  📄 Docs: SERVER_ACTIONS_SECURITY.md                           │
├─────────────────────────────────────────────────────────────────┤
│                    CAPA 2: BFF Pattern                          │
│  Gateway no tiene IP pública. Next.js proxea /api/* al          │
│  Gateway por red interna de Kubernetes.                         │
│  📄 Docs: BFF_PATTERN.md                                      │
├─────────────────────────────────────────────────────────────────┤
│                    CAPA 1: Network Isolation                    │
│  Kubernetes NetworkPolicies con zero-trust. Solo frontend-web   │
│  puede hablar con Gateway. Solo Gateway con microservicios.     │
│  📄 Docs: NETWORK_ISOLATION.md                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📚 Documentos relacionados (fuera de esta carpeta)

| Documento                                  | Ubicación                                        | Relevancia                                       |
| ------------------------------------------ | ------------------------------------------------ | ------------------------------------------------ |
| Copilot Instructions (reglas de seguridad) | `/.github/copilot-instructions.md`               | Reglas obligatorias: NoSqlInjection, NoXss, CSRF |
| Auth Security Implementation               | `/docs/AUTH_SECURITY_IMPLEMENTATION_COMPLETE.md` | JWT, 2FA, refresh tokens                         |
| Architecture Audit Remediation             | `/docs/ARCHITECTURE_AUDIT_REMEDIATION.md`        | Auditoría de ciberseguridad (5 fases)            |
| Plan Compliance RD                         | `/docs/PLAN_COMPLIANCE_AUDITABILIDAD_RD.md`      | Compliance regulatorio dominicano                |
| ARCO Implementation                        | `/docs/ARCO_IMPLEMENTATION_COMPLETED.md`         | Derechos ARCO (eliminación de cuenta)            |
| Normativas RD                              | `/docs/NORMATIVAS_RD_OKLA.md`                    | Leyes aplicables (155-17, 172-13)                |

---

_Equipo de Seguridad — OKLA — Febrero 2026_

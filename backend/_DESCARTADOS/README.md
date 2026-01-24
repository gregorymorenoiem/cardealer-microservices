# 🗑️ Microservicios Descartados

> **Última actualización:** Enero 23, 2026  
> **Razón principal:** Corrección del modelo de negocio de OKLA

---

## ⚠️ IMPORTANTE

Los microservicios en esta carpeta fueron **planificados incorrectamente** asumiendo que OKLA sería un **marketplace transaccional** donde la plataforma procesaría pagos de vehículos.

### Modelo Correcto de OKLA

```
┌────────────────────────────────────────────────────────────────────────┐
│              OKLA ES PLATAFORMA DE PUBLICIDAD                          │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│   ✅ Dealers PAGAN a OKLA: Suscripción mensual RD$2,900-14,900        │
│   ✅ Sellers PAGAN a OKLA: Publicación única RD$1,500                  │
│                                                                        │
│   ❌ OKLA NO procesa pagos de vehículos                                │
│   ❌ OKLA NO retiene dinero de compradores                             │
│   ❌ OKLA NO transfiere dinero a vendedores                            │
│   ❌ OKLA NO cobra comisión por ventas                                 │
│                                                                        │
│   LA TRANSACCIÓN DEL VEHÍCULO OCURRE DIRECTAMENTE:                     │
│   Comprador ───[Paga en efectivo/banco]───> Vendedor                  │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 📋 Microservicios Descartados

### 🔐 EscrowService

| Campo                  | Valor                              |
| ---------------------- | ---------------------------------- |
| **Puerto Planificado** | 5047                               |
| **Base de Datos**      | escrow_db                          |
| **Fecha de Descarte**  | Enero 23, 2026                     |
| **Razón**              | OKLA no procesa pagos de vehículos |

**Descripción Original:**  
Sistema de pagos en garantía (escrow) para transacciones de alto valor entre compradores y vendedores.

**Por qué NO aplica:**

- OKLA no recibe dinero de compradores
- OKLA no transfiere dinero a vendedores
- Las transacciones de vehículos son externas a la plataforma

**Estructura del Servicio:**

```
EscrowService/
├── EscrowService.sln
├── Dockerfile
├── EscrowService.Api/
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── EscrowService.Application/
│   ├── Commands/
│   ├── DTOs/
│   ├── Handlers/
│   └── Validators/
├── EscrowService.Domain/
│   ├── Entities/
│   └── Interfaces/
├── EscrowService.Infrastructure/
│   └── Persistence/
└── EscrowService.Tests/
```

---

## ⚠️ NO USAR ESTOS SERVICIOS

Estos microservicios:

1. ❌ NO deben ser desplegados
2. ❌ NO deben ser referenciados en el código activo
3. ❌ NO deben ser incluidos en docker-compose
4. ❌ NO deben ser incluidos en CI/CD

Se mantienen archivados para:

1. ✅ Referencia histórica
2. ✅ Evitar que se vuelvan a proponer
3. ✅ Posible reutilización de código para otros propósitos

---

## 📚 Documentación Relacionada

- [Documentación descartada](../docs/_DESCARTADOS/README.md)
- [Modelo de negocio correcto](../docs/process-matrix/02-USUARIOS-DEALERS/04-dealer-onboarding.md)
- [Integración de cobros Azul](../docs/process-matrix/05-PAGOS-FACTURACION/03-azul-payment.md)

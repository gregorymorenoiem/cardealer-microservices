# 🇩🇴 APIs República Dominicana - Marketplace OKLA

**Categoría:** Dominican Republic Marketplace APIs  
**Uso:** Verificación, financiamiento, seguros, comunicación  
**Última Actualización:** Enero 15, 2026

---

## 📋 Índice de APIs

### 🚗 Sector Vehículos
| # | API | Archivo | Prioridad |
|---|-----|---------|-----------|
| 1 | DGII Consulta Placa | [DGII_VEHICULOS_API.md](./DGII_VEHICULOS_API.md) | ⭐⭐⭐⭐⭐ |
| 2 | INTRANT | [INTRANT_API.md](./INTRANT_API.md) | ⭐⭐⭐⭐⭐ |
| 3 | AMET | [AMET_API.md](./AMET_API.md) | ⭐⭐⭐⭐ |

### 👤 Verificación de Identidad
| # | API | Archivo | Prioridad |
|---|-----|---------|-----------|
| 4 | JCE Cédula | [JCE_CEDULA_API.md](./JCE_CEDULA_API.md) | ⭐⭐⭐⭐⭐ |
| 5 | Data Crédito | [DATACREDITO_API.md](./DATACREDITO_API.md) | ⭐⭐⭐⭐⭐ |

### 🏦 Financiamiento
| # | API | Archivo | Prioridad |
|---|-----|---------|-----------|
| 6 | Banco Popular Auto | [BANCO_POPULAR_AUTO_API.md](./BANCO_POPULAR_AUTO_API.md) | ⭐⭐⭐⭐⭐ |
| 7 | Asociaciones (APAP) | [ASOCIACIONES_AHORROS_API.md](./ASOCIACIONES_AHORROS_API.md) | ⭐⭐⭐⭐ |

### 🛡️ Seguros
| # | API | Archivo | Prioridad |
|---|-----|---------|-----------|
| 8 | Seguros Reservas | [SEGUROS_RESERVAS_API.md](./SEGUROS_RESERVAS_API.md) | ⭐⭐⭐⭐⭐ |
| 9 | Otras Aseguradoras | [ASEGURADORAS_API.md](./ASEGURADORAS_API.md) | ⭐⭐⭐⭐ |

### 📱 Comunicación
| # | API | Archivo | Prioridad |
|---|-----|---------|-----------|
| 10 | WhatsApp Business | [WHATSAPP_BUSINESS_API.md](./WHATSAPP_BUSINESS_API.md) | ⭐⭐⭐⭐⭐ |
| 11 | SMS Gateways | [SMS_GATEWAYS_API.md](./SMS_GATEWAYS_API.md) | ⭐⭐⭐⭐ |

### 📍 Geolocalización
| # | API | Archivo | Prioridad |
|---|-----|---------|-----------|
| 12 | Google Maps RD | [GOOGLE_MAPS_API.md](./GOOGLE_MAPS_API.md) | ⭐⭐⭐⭐ |
| 13 | ONE Estadísticas | [ONE_ESTADISTICAS_API.md](./ONE_ESTADISTICAS_API.md) | ⭐⭐⭐ |

### 🔧 Servicios Auxiliares
| # | API | Archivo | Prioridad |
|---|-----|---------|-----------|
| 14 | Inspección Vehicular | [INSPECCION_VEHICULAR_API.md](./INSPECCION_VEHICULAR_API.md) | ⭐⭐⭐⭐ |
| 15 | Servicios de Grúa | [GRUAS_API.md](./GRUAS_API.md) | ⭐⭐⭐ |

---

## 🏗️ Arquitectura de Integración

```
┌─────────────────────────────────────────────────────────────────────┐
│                         OKLA FRONTEND                               │
│                    (React 19 + TypeScript)                          │
└──────────────────────────┬──────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      OKLA API GATEWAY                               │
│                    (Ocelot + .NET 8)                                │
└──────────────────────────┬──────────────────────────────────────────┘
                           │
           ┌───────────────┼───────────────┐
           ▼               ▼               ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ VehicleVerifi-  │ │ FinancingService│ │ InsuranceService│
│ cationService   │ │ (Financiamiento)│ │ (Seguros)       │
│ Puerto: 5070    │ │ Puerto: 5071    │ │ Puerto: 5072    │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ • DGII Placa    │ │ • Banco Popular │ │ • Seg. Reservas │
│ • INTRANT       │ │ • APAP          │ │ • Universal     │
│ • AMET          │ │ • Data Crédito  │ │ • Mapfre        │
│ • JCE           │ │ • Asociaciones  │ │ • Sura          │
└─────────────────┘ └─────────────────┘ └─────────────────┘

┌─────────────────┐ ┌─────────────────┐
│ Communication-  │ │ LocationService │
│ Service         │ │ (Ubicación)     │
│ Puerto: 5073    │ │ Puerto: 5074    │
└────────┬────────┘ └────────┬────────┘
         │                   │
         ▼                   ▼
┌─────────────────┐ ┌─────────────────┐
│ • WhatsApp API  │ │ • Google Maps   │
│ • SMS Claro     │ │ • ONE Stats     │
│ • SMS Altice    │ │ • Geocoding     │
└─────────────────┘ └─────────────────┘
```

---

## 🔧 Microservicios Nuevos Requeridos

| Servicio | Puerto | Responsabilidad |
|----------|--------|-----------------|
| **VehicleVerificationService** | 5070 | DGII, INTRANT, AMET, JCE |
| **FinancingService** | 5071 | Banco Popular, APAP, Data Crédito |
| **InsuranceService** | 5072 | Seguros Reservas, Universal, Mapfre |
| **CommunicationService** | 5073 | WhatsApp, SMS |
| **LocationService** | 5074 | Google Maps, ONE |

---

## 📦 Paquetes NuGet Compartidos

```xml
<!-- Todas las integraciones RD -->
<PackageReference Include="HtmlAgilityPack" Version="1.11.57" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="8.0.0" />
<PackageReference Include="Polly" Version="8.2.0" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
```

---

## 🔐 Convenios Requeridos

| Entidad | Tipo de Acceso | Contacto | Estado |
|---------|---------------|----------|--------|
| DGII | Scraping/Convenio | 809-689-3444 | ⏳ Pendiente |
| JCE | Convenio formal | 809-539-2522 | ⏳ Pendiente |
| Data Crédito | Comercial | 809-567-4100 | ⏳ Pendiente |
| Banco Popular | Alianza | 809-544-5555 | ⏳ Pendiente |
| Seguros Reservas | API Partner | 809-476-4000 | ⏳ Pendiente |
| WhatsApp/Meta | BSP Partner | business.whatsapp.com | ⏳ Pendiente |

---

## 💰 Costos Estimados Mensuales

| Servicio | Modelo | Costo Estimado |
|----------|--------|----------------|
| Data Crédito | Por consulta | $0.50-1.00 USD |
| WhatsApp Business | Por mensaje | $0.05-0.10 USD |
| SMS Claro/Altice | Por SMS | RD$0.40-0.50 |
| Google Maps | Por request | $0.005-0.007 USD |
| Seguros APIs | Gratis (comisión) | 0 |
| Financiamiento APIs | Gratis (comisión) | 0 |

**Estimado mensual (10,000 transacciones):** ~$2,000-3,000 USD

---

## 📋 Plan de Implementación

### Fase 1: Verificaciones (Semanas 1-3)
- [x] DGII Consulta Placa
- [x] JCE Cédula
- [x] INTRANT Historial
- [x] AMET Multas

### Fase 2: Financiamiento (Semanas 4-6)
- [ ] Data Crédito Score
- [ ] Banco Popular Pre-aprobación
- [ ] APAP Integración

### Fase 3: Seguros (Semanas 7-8)
- [ ] Seguros Reservas Cotización
- [ ] Multi-aseguradora

### Fase 4: Comunicación (Semanas 9-10)
- [ ] WhatsApp Business
- [ ] SMS Gateways

### Fase 5: Optimización (Semanas 11-12)
- [ ] Google Maps
- [ ] ONE Estadísticas
- [ ] Servicios auxiliares

---

**Documentación relacionada:**
- [accounting-tax/](../accounting-tax/) - APIs de contabilidad e impuestos
- [README.md](../README.md) - Índice principal de APIs

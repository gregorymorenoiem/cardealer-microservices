# 🇩🇴 APIs Adicionales República Dominicana para OKLA Marketplace

**Fecha:** Enero 15, 2026  
**Objetivo:** Identificar APIs que brinden ventajas competitivas en el sector vehículos

---

## 📊 Resumen de APIs Recomendadas

| Categoría             | API                 | Prioridad  | Ventaja Competitiva                           |
| --------------------- | ------------------- | ---------- | --------------------------------------------- |
| 🚗 **Vehículos**      | DGII Consulta Placa | ⭐⭐⭐⭐⭐ | Verificar historial fiscal del vehículo       |
| 🚗 **Vehículos**      | INTRANT             | ⭐⭐⭐⭐⭐ | Multas, revisión técnica, historial           |
| 🚗 **Vehículos**      | AMET                | ⭐⭐⭐⭐   | Historial de accidentes/multas                |
| 👤 **Identidad**      | JCE Cédula          | ⭐⭐⭐⭐⭐ | Verificar identidad de compradores/vendedores |
| 💳 **Crédito**        | Data Crédito        | ⭐⭐⭐⭐⭐ | Pre-aprobación de financiamiento              |
| 💳 **Crédito**        | TransUnion RD       | ⭐⭐⭐⭐   | Score crediticio alternativo                  |
| 🏦 **Financiamiento** | Banco Popular Auto  | ⭐⭐⭐⭐⭐ | Financiamiento integrado                      |
| 🏦 **Financiamiento** | APAP/AAyP           | ⭐⭐⭐⭐   | Préstamos vehículos                           |
| 🛡️ **Seguros**        | Seguros Reservas    | ⭐⭐⭐⭐⭐ | Cotización instantánea                        |
| 🛡️ **Seguros**        | SISALRIL            | ⭐⭐⭐     | Validar cobertura de salud                    |
| 📱 **Comunicación**   | WhatsApp Business   | ⭐⭐⭐⭐⭐ | Contacto directo comprador-vendedor           |
| 📱 **Comunicación**   | Claro/Altice SMS    | ⭐⭐⭐⭐   | Notificaciones SMS                            |
| 📍 **Ubicación**      | Google Maps RD      | ⭐⭐⭐⭐   | Ubicación de dealers                          |
| 📍 **Ubicación**      | ONE (Estadísticas)  | ⭐⭐⭐     | Datos demográficos por zona                   |
| 🚚 **Logística**      | Grúas RD            | ⭐⭐⭐     | Servicio de traslado                          |
| 🔧 **Inspección**     | Centros INTRANT     | ⭐⭐⭐⭐   | Inspección pre-compra                         |

---

## 🚗 1. SECTOR VEHÍCULOS (CRÍTICO)

### 1.1 DGII - Consulta de Vehículos por Placa

**¿Qué es?** Permite verificar el estado fiscal de un vehículo (impuestos pagados, embargos, etc.)

```http
# Consulta web (scraping necesario)
GET https://dgii.gov.do/servicios/consultaVehiculo.aspx?placa=A123456

# Datos que se obtienen:
- Placa
- Marca/Modelo
- Año
- Tipo de vehículo
- Estado fiscal (al día/pendiente)
- Monto adeudado
- Última fecha de pago de marbete
```

**Ventaja Competitiva:**

- ✅ Mostrar badge "VERIFICADO DGII ✓" en listings
- ✅ Alertar si vehículo tiene deudas fiscales
- ✅ Generar confianza en compradores

**Implementación:**

```csharp
public record VehicleFiscalStatus(
    string Placa,
    string Marca,
    string Modelo,
    int Ano,
    bool ImpuestosAlDia,
    decimal MontoAdeudado,
    DateTime? UltimoPagoMarbete
);

public interface IVehicleFiscalService
{
    Task<VehicleFiscalStatus?> GetByPlacaAsync(string placa);
}
```

---

### 1.2 INTRANT - Instituto Nacional de Tránsito

**¿Qué es?** Autoridad de tránsito que maneja licencias, revisión técnica, y registro de vehículos.

**Website:** [intrant.gob.do](https://intrant.gob.do)

```http
# Consultas disponibles:
- Estado de revisión técnica vehicular
- Historial de inspecciones
- Verificar si licencia de conducir es válida
- Consulta de multas de tránsito
```

**Datos Obtenibles:**

| Dato                      | Uso en OKLA                 |
| ------------------------- | --------------------------- |
| Revisión técnica vigente  | Badge "INSPECCIÓN AL DÍA ✓" |
| Fecha próxima inspección  | Alertar al comprador        |
| Historial de inspecciones | Mostrar mantenimiento       |
| Multas pendientes         | Alertar deudas ocultas      |

**Ventaja Competitiva:**

- 🏆 **ÚNICO en RD** si mostramos historial de inspecciones
- 🏆 Compradores confían más en vehículos verificados
- 🏆 Dealers verificados se destacan

---

### 1.3 AMET - Autoridad Metropolitana de Transporte

**¿Qué es?** Policía de tránsito del Gran Santo Domingo.

```http
# Consultas posibles:
- Historial de multas por placa
- Accidentes reportados
- Estado de pago de multas
```

**Implementación:**

```csharp
public record TrafficHistory(
    string Placa,
    int TotalMultas,
    int MultasPendientes,
    decimal MontoAdeudado,
    int AccidentesReportados,
    List<TrafficIncident> Incidentes
);

public record TrafficIncident(
    DateTime Fecha,
    string Tipo, // "Multa", "Accidente"
    string Descripcion,
    decimal Monto,
    bool Pagado
);
```

**Badge en UI:**

```
🟢 Sin multas pendientes
🟡 2 multas menores pendientes
🔴 Multas graves o accidentes
```

---

## 👤 2. VERIFICACIÓN DE IDENTIDAD

### 2.1 JCE - Junta Central Electoral (Cédula)

**¿Qué es?** Validar que la cédula de un usuario es real y obtener datos básicos.

**Endpoint (no oficial, requiere convenio):**

```http
POST https://api.jce.gob.do/consulta/cedula
{
  "cedula": "00100000001"
}

# Response:
{
  "valido": true,
  "nombres": "JUAN CARLOS",
  "apellidos": "PEREZ GARCIA",
  "fechaNacimiento": "1990-05-15",
  "sexo": "M",
  "estado": "VIGENTE"
}
```

**Alternativa (Scraping):**

```
https://servicios.jce.gob.do/consultapadron/
```

**Uso en OKLA:**

- ✅ Verificar identidad de vendedores → Badge "VENDEDOR VERIFICADO ✓"
- ✅ Verificar compradores antes de mostrar contacto
- ✅ Reducir fraudes (perfiles falsos)

**Ventaja Competitiva:**

- 🏆 **CONFIANZA** - Usuarios verificados generan más ventas
- 🏆 Reducir scams y perfiles falsos
- 🏆 Diferenciador vs. competencia (Corotos, etc.)

---

### 2.2 Data Crédito (TransUnion RD)

**¿Qué es?** Buró de crédito principal de República Dominicana.

**Website:** [datacredito.com.do](https://datacredito.com.do)

**API (requiere convenio comercial):**

```http
# Consulta de score crediticio
POST https://api.datacredito.com.do/v1/score
Authorization: Bearer {token}
{
  "cedula": "00100000001",
  "tipoConsulta": "SOFT" // No afecta el score
}

# Response:
{
  "score": 720,
  "rango": "BUENO", // EXCELENTE, BUENO, REGULAR, DEFICIENTE
  "capacidadEndeudamiento": 500000,
  "deudaActual": 150000,
  "historialMorosidad": false
}
```

**Uso en OKLA:**

- ✅ **Pre-aprobación de financiamiento** instantánea
- ✅ Mostrar "Elegible para financiamiento hasta RD$500,000"
- ✅ Conectar con bancos partner para préstamos

**Ventaja Competitiva:**

- 🏆 **GAME CHANGER** - Financiamiento integrado en el marketplace
- 🏆 Aumenta conversión (comprador sabe si puede pagar)
- 🏆 Comisión por referidos a bancos

---

## 🏦 3. FINANCIAMIENTO DE VEHÍCULOS

### 3.1 Banco Popular - Auto Fácil

**¿Qué es?** Préstamos para vehículos del banco más grande de RD.

**Programa:** Auto Fácil Popular

```http
# API de pre-aprobación (requiere convenio)
POST https://api.popularenlinea.com/auto/preaprobacion
{
  "cedula": "00100000001",
  "montoSolicitado": 800000,
  "plazoMeses": 60,
  "ingresoMensual": 50000
}

# Response:
{
  "aprobado": true,
  "montoAprobado": 750000,
  "tasaAnual": 12.5,
  "cuotaMensual": 16875,
  "requisitosPendientes": ["Carta de trabajo", "Estados de cuenta"]
}
```

**Integración en OKLA:**

```tsx
// En la página de detalle del vehículo
<FinancingCalculator
  vehiclePrice={850000}
  onPreApproval={(result) => {
    // Mostrar cuota mensual estimada
    // Botón "Solicitar Financiamiento"
  }}
/>
```

**Ventaja Competitiva:**

- 🏆 **Aumenta conversión 3x** cuando el comprador ve la cuota mensual
- 🏆 Comisión por cada préstamo referido (~1-2%)
- 🏆 Dealers prefieren OKLA porque venden más rápido

---

### 3.2 Asociaciones de Ahorros y Préstamos

| Entidad              | API           | Tasas  | Especialidad       |
| -------------------- | ------------- | ------ | ------------------ |
| **APAP**             | ✅ (convenio) | 10-14% | Préstamos rápidos  |
| **ALNAP**            | ⚠️ Limitada   | 11-15% | Empleados públicos |
| **La Nacional**      | ✅ (convenio) | 12-16% | Vehículos usados   |
| **Asociación Cibao** | ⚠️ Limitada   | 10-13% | Región Cibao       |

**Modelo de Negocio:**

```
Usuario solicita financiamiento en OKLA
    ↓
OKLA envía a 3-5 entidades simultáneamente
    ↓
Usuario recibe mejores ofertas en 24-48h
    ↓
OKLA cobra comisión por referido exitoso
```

---

## 🛡️ 4. SEGUROS DE VEHÍCULOS

### 4.1 Seguros Reservas

**¿Qué es?** Aseguradora más grande de RD, subsidiaria del Banco de Reservas.

**API de Cotización:**

```http
POST https://api.segurosreservas.com/vehiculos/cotizar
{
  "marca": "Toyota",
  "modelo": "Corolla",
  "ano": 2022,
  "valor": 1200000,
  "uso": "PARTICULAR",
  "zona": "SANTO_DOMINGO",
  "coberturas": ["RESPONSABILIDAD_CIVIL", "COBERTURA_AMPLIA", "ROBO"]
}

# Response:
{
  "cotizacionId": "COT-2026-12345",
  "primaAnual": 45000,
  "primaMensual": 4125,
  "coberturas": [
    {"nombre": "Responsabilidad Civil", "limite": 500000},
    {"nombre": "Daños Propios", "deducible": 15000},
    {"nombre": "Robo Total", "limite": 1200000}
  ],
  "validezHasta": "2026-01-22"
}
```

**Integración en OKLA:**

```tsx
// Widget de seguro en detalle de vehículo
<InsuranceQuoteWidget
  vehicle={vehicle}
  onQuote={(quote) => {
    // Mostrar: "Asegura este vehículo desde RD$4,125/mes"
  }}
/>
```

**Otras Aseguradoras:**

| Aseguradora             | API | Especialidad       |
| ----------------------- | --- | ------------------ |
| **Seguros Universal**   | ✅  | Vehículos de lujo  |
| **Seguros Banreservas** | ✅  | Clientes banco     |
| **Mapfre BHD**          | ✅  | Flotas/dealers     |
| **Seguros Sura**        | ✅  | Coberturas premium |

**Ventaja Competitiva:**

- 🏆 Comprador puede asegurar **al momento de la compra**
- 🏆 Comisión por póliza vendida (~5-10%)
- 🏆 Dealers ofrecen "paquete completo" (vehículo + seguro + financiamiento)

---

## 📱 5. COMUNICACIÓN

### 5.1 WhatsApp Business API

**¿Qué es?** API oficial de WhatsApp para comunicación empresarial.

**Proveedores en RD:**

- **Twilio** (internacional)
- **360Dialog** (más económico)
- **Gupshup** (especializado en LATAM)

```http
# Enviar mensaje
POST https://api.360dialog.com/messages
{
  "to": "18091234567",
  "type": "template",
  "template": {
    "name": "nuevo_vehiculo_interes",
    "language": "es",
    "components": [
      {
        "type": "body",
        "parameters": [
          {"type": "text", "text": "Toyota Corolla 2024"},
          {"type": "text", "text": "RD$1,200,000"}
        ]
      }
    ]
  }
}
```

**Uso en OKLA:**

- ✅ Notificaciones de nuevos vehículos que coinciden con búsqueda
- ✅ Alertas de baja de precio
- ✅ Chat directo comprador-vendedor
- ✅ Confirmación de citas para ver vehículos

**Costo:** ~$0.05-0.10 USD por mensaje

---

### 5.2 SMS Gateways Locales

| Proveedor    | Costo/SMS  | API     |
| ------------ | ---------- | ------- |
| **Claro RD** | RD$0.50    | ✅ SMPP |
| **Altice**   | RD$0.45    | ✅ HTTP |
| **Viva**     | RD$0.40    | ✅ HTTP |
| **Twilio**   | $0.075 USD | ✅ REST |

**Uso:** OTP, verificación de teléfono, alertas críticas

---

## 📍 6. GEOLOCALIZACIÓN Y DATOS

### 6.1 Google Maps Platform (RD)

**APIs Útiles:**

```http
# Geocoding - Dirección a coordenadas
GET https://maps.googleapis.com/maps/api/geocode/json
    ?address=Av.+Winston+Churchill,+Santo+Domingo
    &key=API_KEY

# Distance Matrix - Distancia comprador-vendedor
GET https://maps.googleapis.com/maps/api/distancematrix/json
    ?origins=18.4861,-69.9312
    &destinations=18.5001,-69.8500
    &key=API_KEY

# Places - Buscar dealers cercanos
GET https://maps.googleapis.com/maps/api/place/nearbysearch/json
    ?location=18.4861,-69.9312
    &radius=5000
    &type=car_dealer
    &key=API_KEY
```

**Uso en OKLA:**

- ✅ Mapa de ubicación de dealers
- ✅ "Vehículos cerca de ti"
- ✅ Calcular distancia a cada vehículo
- ✅ Rutas para test drives

---

### 6.2 ONE - Oficina Nacional de Estadística

**¿Qué es?** Datos demográficos y estadísticos de RD.

**Datos útiles:**

- Ingreso promedio por zona
- Población por municipio
- Índice de motorización por provincia

**Uso en OKLA:**

- ✅ Pricing intelligence por zona
- ✅ Identificar mercados desatendidos
- ✅ Segmentación de marketing

---

## 🔧 7. SERVICIOS AUXILIARES

### 7.1 Inspección Vehicular Pre-Compra

**Servicios en RD:**

- **INTRANT Centros de Inspección** - Oficial
- **AutoCheck RD** - Privado
- **Inspección Express** - A domicilio

**Integración:**

```tsx
// Botón en detalle de vehículo
<button onClick={scheduleInspection}>
  📋 Solicitar Inspección Pre-Compra (RD$2,500)
</button>
```

**Ventaja:**

- 🏆 Genera confianza
- 🏆 Ingreso adicional por referido
- 🏆 Diferenciador vs. competencia

---

### 7.2 Servicios de Grúa

**Proveedores:**

- **Grúas del Caribe** - Nacional
- **Asistencia Vial Popular** - Clientes banco
- **SOS Grúas** - 24/7

**Uso:**

- Traslado de vehículo vendido
- Asistencia incluida en paquetes premium

---

## 💡 8. RECOMENDACIONES DE IMPLEMENTACIÓN

### Prioridad ALTA (Implementar primero)

| API                     | Por qué                               | ROI Estimado |
| ----------------------- | ------------------------------------- | ------------ |
| **DGII Consulta Placa** | Verificación básica, genera confianza | Alto         |
| **JCE Cédula**          | Reducir fraudes, verificar usuarios   | Alto         |
| **Data Crédito**        | Financiamiento = más ventas           | Muy Alto     |
| **Banco Popular Auto**  | Financiamiento integrado              | Muy Alto     |
| **WhatsApp Business**   | Canal de comunicación #1 en RD        | Alto         |
| **Seguros Reservas**    | Ingresos adicionales, valor agregado  | Medio-Alto   |

### Prioridad MEDIA

| API                    | Por qué                   | ROI Estimado |
| ---------------------- | ------------------------- | ------------ |
| **INTRANT**            | Historial de inspecciones | Medio        |
| **AMET**               | Historial de multas       | Medio        |
| **Google Maps**        | UX mejorada               | Medio        |
| **Otras aseguradoras** | Más opciones              | Medio        |

### Prioridad BAJA (Nice to have)

| API                  | Por qué             |
| -------------------- | ------------------- |
| **ONE Estadísticas** | Analytics avanzados |
| **Grúas**            | Servicio adicional  |
| **Inspección**       | Valor agregado      |

---

## 💰 9. MODELO DE MONETIZACIÓN

### Ingresos Potenciales por APIs

| Fuente               | Modelo                     | Ingreso Estimado          |
| -------------------- | -------------------------- | ------------------------- |
| **Financiamiento**   | Comisión 1-2% por préstamo | RD$8,000-16,000/vehículo  |
| **Seguros**          | Comisión 5-10% por póliza  | RD$2,500-5,000/póliza     |
| **Verificaciones**   | Cobrar al vendedor         | RD$500-1,000/verificación |
| **Inspecciones**     | Comisión por referido      | RD$500/inspección         |
| **Premium Listings** | Incluir verificaciones     | RD$2,000/mes extra        |

### Proyección Mensual (1,000 transacciones)

```
Financiamiento (30% de ventas): 300 × RD$10,000 = RD$3,000,000
Seguros (50% compran):          500 × RD$3,000  = RD$1,500,000
Verificaciones premium:         200 × RD$1,000  = RD$200,000
Inspecciones:                   100 × RD$500    = RD$50,000
─────────────────────────────────────────────────────────────
TOTAL MENSUAL ADICIONAL:                          RD$4,750,000
                                                  (~USD $80,000)
```

---

## 🏆 10. VENTAJAS COMPETITIVAS VS. COMPETENCIA

| Feature                  | OKLA | Corotos | Mercado Libre | Facebook |
| ------------------------ | ---- | ------- | ------------- | -------- |
| Verificación DGII        | ✅   | ❌      | ❌            | ❌       |
| Verificación Identidad   | ✅   | ❌      | ⚠️            | ⚠️       |
| Historial Vehículo       | ✅   | ❌      | ❌            | ❌       |
| Financiamiento Integrado | ✅   | ❌      | ❌            | ❌       |
| Cotización Seguro        | ✅   | ❌      | ❌            | ❌       |
| Pre-aprobación Crédito   | ✅   | ❌      | ❌            | ❌       |
| WhatsApp Integrado       | ✅   | ❌      | ❌            | ⚠️       |
| Inspección Pre-Compra    | ✅   | ❌      | ❌            | ❌       |

**Mensaje de Marketing:**

> _"En OKLA, cada vehículo está verificado. Conoce su historial, obtén financiamiento al instante, y asegúralo con un clic. La forma más segura de comprar vehículos en República Dominicana."_

---

## 📋 11. PLAN DE IMPLEMENTACIÓN

### Fase 1: Fundamentos (Semanas 1-4)

- [ ] DGII Consulta Placa
- [ ] JCE Verificación Cédula
- [ ] WhatsApp Business básico

### Fase 2: Monetización (Semanas 5-8)

- [ ] Data Crédito integración
- [ ] Banco Popular convenio
- [ ] Seguros Reservas API

### Fase 3: Diferenciación (Semanas 9-12)

- [ ] INTRANT historial
- [ ] AMET multas
- [ ] Más bancos y aseguradoras

### Fase 4: Optimización (Mes 4+)

- [ ] Analytics con ONE
- [ ] Inspecciones
- [ ] Servicios de grúa

---

## 📞 12. CONTACTOS PARA CONVENIOS

| Entidad              | Departamento          | Teléfono                                               |
| -------------------- | --------------------- | ------------------------------------------------------ |
| **DGII**             | Servicios Digitales   | 809-689-3444                                           |
| **JCE**              | Tecnología            | 809-539-2522                                           |
| **Data Crédito**     | Comercial             | 809-567-4100                                           |
| **Banco Popular**    | Alianzas Estratégicas | 809-544-5555                                           |
| **Seguros Reservas** | Canales Digitales     | 809-476-4000                                           |
| **INTRANT**          | Servicios             | 809-920-0065                                           |
| **WhatsApp/Meta**    | Business Partners     | [business.whatsapp.com](https://business.whatsapp.com) |

---

**Conclusión:** Estas APIs posicionarían a OKLA como el marketplace de vehículos **MÁS COMPLETO Y SEGURO** de República Dominicana, con ventajas competitivas que ningún competidor actual ofrece.

---

**Relacionado:**

- [accounting-tax/README.md](./accounting-tax/README.md) - APIs de contabilidad e impuestos
- [README.md](./README.md) - Índice principal de APIs

# 📊 Google Analytics API - Documentación Completa

**API:** Google Analytics 4 (GA4)  
**Versión:** GA4 / gtag.js  
**Proveedor:** Google LLC  
**Estado:** ✅ En Producción  
**Última actualización:** Enero 15, 2026

---

## 📋 Tabla de Contenidos

1. [Introducción](#introducción)
2. [Características Principales](#características-principales)
3. [Casos de Uso en OKLA](#casos-de-uso-en-okla)
4. [Configuración e Instalación](#configuración-e-instalación)
5. [Web Vitals Tracking](#web-vitals-tracking)
6. [Eventos Personalizados](#eventos-personalizados)
7. [Ejemplos de Código](#ejemplos-de-código)
8. [Dashboards y Reportes](#dashboards-y-reportes)
9. [E-commerce Tracking](#e-commerce-tracking)
10. [Conversiones y Goals](#conversiones-y-goals)
11. [Privacidad y GDPR](#privacidad-y-gdpr)
12. [Mejores Prácticas](#mejores-prácticas)
13. [Troubleshooting](#troubleshooting)
14. [Costos y Límites](#costos-y-límites)
15. [Recursos Adicionales](#recursos-adicionales)

---

## 🎯 Introducción

**Google Analytics 4 (GA4)** es la plataforma de analítica web y mobile de Google. En OKLA, se utiliza para rastrear comportamiento de usuarios, performance del sitio (Web Vitals), conversiones y métricas de negocio.

### ¿Por qué Google Analytics 4 en OKLA?

- ✅ **FREE:** Sin costo hasta 10M eventos/mes
- ✅ **Web Vitals:** Tracking automático de performance
- ✅ **User Journey:** Análisis completo del funnel
- ✅ **Machine Learning:** Predicciones automáticas
- ✅ **Cross-Platform:** Web + Mobile unificado
- ✅ **Privacy-First:** Compatible con GDPR

---

## ✨ Características Principales

### Capacidades Core de GA4

| Característica           | Descripción             | Uso en OKLA                             |
| ------------------------ | ----------------------- | --------------------------------------- |
| **Event-Based Model**    | Todo es un evento       | Clicks, views, conversions              |
| **Automatic Events**     | 35+ eventos automáticos | Page views, scrolls, file downloads     |
| **Enhanced Measurement** | 0-config tracking       | Outbound clicks, site search, video     |
| **Web Vitals**           | Performance metrics     | CLS, FID, LCP, FCP, TTFB                |
| **User Properties**      | Atributos de usuario    | Plan (Free/Starter/Pro), AccountType    |
| **Custom Dimensions**    | Datos personalizados    | Vehicle ID, Dealer ID, City             |
| **Predictive Metrics**   | ML predictions          | Purchase probability, churn probability |
| **BigQuery Export**      | Data warehouse          | Análisis avanzado offline               |

### GA4 vs Universal Analytics

| Aspecto              | Universal Analytics (UA) | Google Analytics 4 (GA4) |
| -------------------- | ------------------------ | ------------------------ |
| **Modelo de Datos**  | Sessions + Pageviews     | Events                   |
| **Tracking**         | ga.js / analytics.js     | gtag.js / Firebase SDK   |
| **Cross-Platform**   | ❌ No                    | ✅ Sí (Web + Mobile)     |
| **Machine Learning** | Básico                   | Avanzado                 |
| **Privacy**          | Cookies required         | Cookieless options       |
| **BigQuery**         | Pago                     | FREE (con límites)       |
| **Fin de Soporte**   | Julio 1, 2023            | Activo                   |

---

## 🎨 Casos de Uso en OKLA

### 1. Web Vitals Tracking (Implementado ✅)

**Ubicación:** `frontend/web/src/lib/webVitals.ts`

```
User loads page
      ↓
Web Vitals library measures performance
      ↓
onCLS, onFID, onLCP, onFCP, onTTFB callbacks
      ↓
sendToGoogleAnalytics(metric)
      ↓
gtag('event', metric.name, {...})
      ↓
Google Analytics 4 dashboard
```

**Métricas trackeadas:**

- ✅ **CLS** - Cumulative Layout Shift (estabilidad visual)
- ✅ **FID** - First Input Delay (interactividad)
- ✅ **LCP** - Largest Contentful Paint (velocidad de carga)
- ✅ **FCP** - First Contentful Paint (primer render)
- ✅ **TTFB** - Time to First Byte (respuesta del servidor)

### 2. User Journey Analysis

**Funnel Completo:**

```
Landing Page (/)
    ↓ 10,000 users/month
Search Page (/search)
    ↓ 7,000 (70% conversion)
Vehicle Detail (/vehicles/{slug})
    ↓ 4,500 (64% conversion)
Contact Seller (WhatsApp/Phone)
    ↓ 900 (20% conversion)
Purchase/Sale
    ↓ 180 (20% conversion)
```

**Eventos principales:**

- `page_view` - Vista de página
- `search` - Búsqueda de vehículos
- `view_item` - Ver detalle de vehículo
- `add_to_cart` - Agregar a favoritos
- `begin_checkout` - Contactar vendedor
- `purchase` - Venta completada

### 3. E-commerce Tracking

**Transacciones:**

- Listados creados (Dealers)
- Suscripciones (Starter, Pro, Enterprise)
- Pagos procesados (Stripe/AZUL)
- Revenue por fuente de tráfico

### 4. Marketing Attribution

**Canales:**

- Organic Search (Google)
- Paid Search (Google Ads)
- Social Media (Facebook, Instagram)
- Direct Traffic
- Referral (Clasificados RD, otros sitios)

### 5. Audience Segmentation

**Segmentos:**

- Compradores vs Vendedores
- Free vs Paid Dealers
- Mobile vs Desktop
- New vs Returning Users
- High Intent (multiple searches)

---

## ⚙️ Configuración e Instalación

### Paso 1: Crear Propiedad GA4

1. **Ir a Google Analytics:** https://analytics.google.com/
2. **Crear cuenta:** "OKLA Marketplace"
3. **Crear propiedad:**
   - Nombre: "OKLA - Production"
   - Zona horaria: Atlantic Standard Time (República Dominicana)
   - Moneda: DOP (Peso Dominicano)
4. **Configurar Data Stream:**
   - Tipo: Web
   - URL: https://okla.com.do
   - Stream name: "OKLA Web"
5. **Copiar Measurement ID:** `G-XXXXXXXXXX`

### Paso 2: Instalar gtag.js en Frontend

**Opción A: Directamente en HTML (Recomendado)**

```html
<!-- public/index.html -->
<!DOCTYPE html>
<html lang="es">
  <head>
    <meta charset="UTF-8" />
    <title>OKLA - Marketplace de Vehículos</title>

    <!-- Google Analytics GA4 -->
    <script
      async
      src="https://www.googletagmanager.com/gtag/js?id=G-XXXXXXXXXX"
    ></script>
    <script>
      window.dataLayer = window.dataLayer || [];
      function gtag() {
        dataLayer.push(arguments);
      }
      gtag("js", new Date());

      gtag("config", "G-XXXXXXXXXX", {
        send_page_view: false, // Manual pageview tracking
        cookie_flags: "SameSite=None;Secure",
      });
    </script>
  </head>
  <body>
    <div id="root"></div>
  </body>
</html>
```

**Opción B: React Component**

```tsx
// src/components/GoogleAnalytics.tsx
import { useEffect } from "react";
import { useLocation } from "react-router-dom";

const GA_MEASUREMENT_ID = import.meta.env.VITE_GA_MEASUREMENT_ID;

export const GoogleAnalytics = () => {
  const location = useLocation();

  useEffect(() => {
    // Track pageview on route change
    if (window.gtag) {
      window.gtag("config", GA_MEASUREMENT_ID, {
        page_path: location.pathname + location.search,
      });
    }
  }, [location]);

  return null;
};
```

**Uso en App.tsx:**

```tsx
import { GoogleAnalytics } from "./components/GoogleAnalytics";

function App() {
  return (
    <>
      <GoogleAnalytics />
      <Routes>{/* ... routes */}</Routes>
    </>
  );
}
```

### Paso 3: Variables de Entorno

**`.env.development`:**

```env
VITE_GA_MEASUREMENT_ID=G-XXXXXXXXXX
VITE_GA_DEBUG=true
```

**`.env.production`:**

```env
VITE_GA_MEASUREMENT_ID=G-YYYYYYYYYY
VITE_GA_DEBUG=false
```

**Acceso en código:**

```typescript
const GA_ID = import.meta.env.VITE_GA_MEASUREMENT_ID;
const GA_DEBUG = import.meta.env.VITE_GA_DEBUG === "true";
```

### Paso 4: TypeScript Declarations

```typescript
// src/types/gtag.d.ts
declare global {
  interface Window {
    gtag: (
      command: "config" | "event" | "set",
      targetId: string,
      config?: Record<string, any>
    ) => void;
    dataLayer: any[];
  }
}

export {};
```

---

## 📊 Web Vitals Tracking

### Implementación Actual

**Archivo:** `frontend/web/src/lib/webVitals.ts`

```typescript
import { onCLS, onFID, onFCP, onLCP, onTTFB, Metric } from "web-vitals";

const sendToGoogleAnalytics = (metric: Metric) => {
  if (window.gtag) {
    // Send to Google Analytics 4
    window.gtag("event", metric.name, {
      value: Math.round(
        metric.name === "CLS" ? metric.value * 1000 : metric.value
      ),
      event_category: "Web Vitals",
      event_label: metric.id,
      non_interaction: true,
    });
  }
};

export const reportWebVitals = () => {
  onCLS(sendToGoogleAnalytics); // Cumulative Layout Shift
  onFID(sendToGoogleAnalytics); // First Input Delay
  onFCP(sendToGoogleAnalytics); // First Contentful Paint
  onLCP(sendToGoogleAnalytics); // Largest Contentful Paint
  onTTFB(sendToGoogleAnalytics); // Time to First Byte
};
```

**Uso en main.tsx:**

```typescript
import { reportWebVitals } from "./lib/webVitals";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);

// Report Web Vitals
reportWebVitals();
```

### Métricas Web Vitals

| Métrica  | Descripción        | Bueno   | Necesita Mejora | Pobre    |
| -------- | ------------------ | ------- | --------------- | -------- |
| **CLS**  | Estabilidad visual | ≤ 0.1   | 0.1 - 0.25      | > 0.25   |
| **FID**  | Interactividad     | ≤ 100ms | 100-300ms       | > 300ms  |
| **LCP**  | Velocidad de carga | ≤ 2.5s  | 2.5-4s          | > 4s     |
| **FCP**  | Primer render      | ≤ 1.8s  | 1.8-3s          | > 3s     |
| **TTFB** | Respuesta servidor | ≤ 800ms | 800-1800ms      | > 1800ms |

### Ver Web Vitals en GA4

1. **Navegación:**

   - Reports → Engagement → Events
   - Filtrar por: `event_name` = `CLS`, `FID`, `LCP`, etc.

2. **Custom Report:**

   - Explorations → Create exploration
   - Dimensions: `event_name`, `page_path`
   - Metrics: `event_value` (avg)
   - Segment: Web Vitals events

3. **Alertas:**
   - Admin → Custom Alerts
   - Condition: `LCP > 4000ms` (4 seconds)
   - Email: dev@okla.com.do

---

## 🎯 Eventos Personalizados

### Eventos Recomendados para OKLA

#### 1. Búsqueda de Vehículos

```typescript
// Cuando usuario busca
export const trackSearch = (searchTerm: string, filters: any) => {
  window.gtag("event", "search", {
    search_term: searchTerm,
    filters: JSON.stringify(filters),
    item_count: filters.resultCount || 0,
  });
};

// Uso
trackSearch("Toyota Corolla", {
  make: "Toyota",
  model: "Corolla",
  yearFrom: 2020,
  yearTo: 2024,
  resultCount: 25,
});
```

#### 2. Ver Vehículo

```typescript
export const trackViewVehicle = (vehicle: Vehicle) => {
  window.gtag("event", "view_item", {
    currency: "DOP",
    value: vehicle.price,
    items: [
      {
        item_id: vehicle.id,
        item_name: vehicle.title,
        item_brand: vehicle.make,
        item_category: vehicle.bodyType,
        price: vehicle.price,
        quantity: 1,
      },
    ],
  });
};
```

#### 3. Agregar a Favoritos

```typescript
export const trackAddToFavorites = (vehicle: Vehicle) => {
  window.gtag("event", "add_to_cart", {
    currency: "DOP",
    value: vehicle.price,
    items: [
      {
        item_id: vehicle.id,
        item_name: vehicle.title,
        item_brand: vehicle.make,
        price: vehicle.price,
      },
    ],
  });
};
```

#### 4. Contactar Vendedor

```typescript
export const trackContactSeller = (
  vehicle: Vehicle,
  method: "whatsapp" | "phone" | "email"
) => {
  window.gtag("event", "begin_checkout", {
    currency: "DOP",
    value: vehicle.price,
    contact_method: method,
    items: [
      {
        item_id: vehicle.id,
        item_name: vehicle.title,
        price: vehicle.price,
      },
    ],
  });
};
```

#### 5. Suscripción de Dealer

```typescript
export const trackDealerSubscription = (
  plan: "Starter" | "Pro" | "Enterprise",
  price: number
) => {
  window.gtag("event", "purchase", {
    transaction_id: generateTransactionId(),
    currency: "DOP",
    value: price,
    items: [
      {
        item_id: `plan_${plan.toLowerCase()}`,
        item_name: `Dealer ${plan} Plan`,
        item_category: "Subscription",
        price: price,
        quantity: 1,
      },
    ],
  });
};
```

#### 6. Publicar Vehículo (Seller)

```typescript
export const trackPublishVehicle = (vehicle: Vehicle) => {
  window.gtag("event", "create_listing", {
    item_id: vehicle.id,
    item_name: vehicle.title,
    item_brand: vehicle.make,
    price: vehicle.price,
    user_type: "seller",
  });
};
```

### Servicio Centralizado

```typescript
// src/services/analyticsService.ts
class AnalyticsService {
  private measurementId: string;
  private debug: boolean;

  constructor() {
    this.measurementId = import.meta.env.VITE_GA_MEASUREMENT_ID;
    this.debug = import.meta.env.VITE_GA_DEBUG === "true";
  }

  private gtag(...args: any[]) {
    if (this.debug) {
      console.log("[GA4]", ...args);
    }
    if (window.gtag) {
      window.gtag(...args);
    }
  }

  trackPageView(path: string, title?: string) {
    this.gtag("config", this.measurementId, {
      page_path: path,
      page_title: title,
    });
  }

  trackEvent(eventName: string, params?: Record<string, any>) {
    this.gtag("event", eventName, params);
  }

  setUserProperties(properties: Record<string, any>) {
    this.gtag("set", "user_properties", properties);
  }

  trackSearch(searchTerm: string, filters: any) {
    this.trackEvent("search", {
      search_term: searchTerm,
      filters: JSON.stringify(filters),
    });
  }

  trackViewVehicle(vehicle: any) {
    this.trackEvent("view_item", {
      currency: "DOP",
      value: vehicle.price,
      items: [
        {
          item_id: vehicle.id,
          item_name: vehicle.title,
          item_brand: vehicle.make,
          price: vehicle.price,
        },
      ],
    });
  }

  trackAddToFavorites(vehicle: any) {
    this.trackEvent("add_to_cart", {
      currency: "DOP",
      value: vehicle.price,
      items: [
        {
          item_id: vehicle.id,
          item_name: vehicle.title,
          price: vehicle.price,
        },
      ],
    });
  }

  trackContactSeller(vehicle: any, method: string) {
    this.trackEvent("begin_checkout", {
      currency: "DOP",
      value: vehicle.price,
      contact_method: method,
      items: [
        {
          item_id: vehicle.id,
          item_name: vehicle.title,
          price: vehicle.price,
        },
      ],
    });
  }

  trackPurchase(transactionId: string, value: number, items: any[]) {
    this.trackEvent("purchase", {
      transaction_id: transactionId,
      currency: "DOP",
      value,
      items,
    });
  }
}

export const analyticsService = new AnalyticsService();
```

---

## 💻 Ejemplos de Código

### Uso en Componentes React

#### SearchPage.tsx

```tsx
import { useEffect } from "react";
import { analyticsService } from "@/services/analyticsService";

export const SearchPage = () => {
  const [filters, setFilters] = useState({});
  const [results, setResults] = useState([]);

  const handleSearch = async (newFilters: any) => {
    setFilters(newFilters);
    const data = await vehicleService.search(newFilters);
    setResults(data.results);

    // Track search
    analyticsService.trackSearch(newFilters.searchTerm || "", {
      ...newFilters,
      resultCount: data.total,
    });
  };

  return (
    <div>
      <SearchFilters onSearch={handleSearch} />
      <SearchResults results={results} />
    </div>
  );
};
```

#### VehicleDetailPage.tsx

```tsx
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import { analyticsService } from "@/services/analyticsService";

export const VehicleDetailPage = () => {
  const { slug } = useParams();
  const [vehicle, setVehicle] = useState(null);

  useEffect(() => {
    const loadVehicle = async () => {
      const data = await vehicleService.getBySlug(slug);
      setVehicle(data);

      // Track view
      analyticsService.trackViewVehicle(data);
    };

    loadVehicle();
  }, [slug]);

  const handleAddToFavorites = async () => {
    await favoriteService.add(vehicle.id);

    // Track favorite
    analyticsService.trackAddToFavorites(vehicle);
  };

  const handleContactSeller = (method: "whatsapp" | "phone") => {
    // Track contact
    analyticsService.trackContactSeller(vehicle, method);

    // Open WhatsApp/Phone
    if (method === "whatsapp") {
      window.open(`https://wa.me/${vehicle.seller.phone}`);
    } else {
      window.location.href = `tel:${vehicle.seller.phone}`;
    }
  };

  return (
    <div>
      <VehicleDetails vehicle={vehicle} />
      <Button onClick={handleAddToFavorites}>❤️ Favorito</Button>
      <Button onClick={() => handleContactSeller("whatsapp")}>
        💬 WhatsApp
      </Button>
    </div>
  );
};
```

#### DealerCheckoutPage.tsx

```tsx
export const DealerCheckoutPage = () => {
  const [plan, setPlan] = useState("Pro");

  const handleSubscribe = async () => {
    const payment = await billingService.subscribe(plan);

    if (payment.success) {
      // Track subscription
      analyticsService.trackPurchase(payment.transactionId, payment.amount, [
        {
          item_id: `plan_${plan.toLowerCase()}`,
          item_name: `Dealer ${plan} Plan`,
          item_category: "Subscription",
          price: payment.amount,
          quantity: 1,
        },
      ]);

      // Redirect to dashboard
      navigate("/dealer/dashboard");
    }
  };

  return (
    <div>
      <PlanSelector selected={plan} onChange={setPlan} />
      <Button onClick={handleSubscribe}>Suscribirse</Button>
    </div>
  );
};
```

### User Properties

```typescript
// Set user properties after login
useEffect(() => {
  if (user) {
    analyticsService.setUserProperties({
      user_id: user.id,
      account_type: user.accountType, // 'Individual', 'Dealer', 'Admin'
      dealer_plan: user.dealerPlan || "Free", // 'Starter', 'Pro', 'Enterprise'
      is_verified: user.isVerified,
      city: user.city,
      signup_date: user.createdAt,
    });
  }
}, [user]);
```

---

## 📈 Dashboards y Reportes

### Reportes Pre-construidos en GA4

#### 1. Overview Dashboard

**Métricas principales:**

- Users (últimos 7/30 días)
- Sessions
- Bounce Rate
- Average Session Duration
- Conversions

**Navegación:** Reports → Life Cycle → Overview

#### 2. Acquisition Report

**De dónde vienen los usuarios:**

- Organic Search
- Direct
- Referral
- Social
- Paid Search

**Navegación:** Reports → Life Cycle → Acquisition → Traffic acquisition

#### 3. Engagement Report

**Qué hacen los usuarios:**

- Most viewed pages
- Most triggered events
- Average engagement time

**Navegación:** Reports → Life Cycle → Engagement → Pages and screens

#### 4. E-commerce Report

**Transacciones:**

- Total revenue
- Purchases
- Average order value
- Top selling items

**Navegación:** Reports → Life Cycle → Monetization → Ecommerce purchases

### Custom Dashboards Recomendados

#### Dashboard 1: OKLA Performance

**Widgets:**

1. **Real-time Users** (Scorecard)
2. **Web Vitals** (Table: CLS, FID, LCP avg values)
3. **Top Vehicles Viewed** (Table: item_name, view_item count)
4. **Search to View Rate** (Funnel: search → view_item)
5. **Contact Conversion** (Funnel: view_item → begin_checkout)

**Cómo crear:**

```
1. Explorations → Create exploration
2. Technique: Free form
3. Add dimensions: page_path, event_name, item_name
4. Add metrics: event_count, total_users, event_value
5. Add segments: Mobile vs Desktop
```

#### Dashboard 2: Dealer Success

**Widgets:**

1. **New Dealer Registrations** (Line chart)
2. **Plan Distribution** (Pie chart: Starter, Pro, Enterprise)
3. **Listings Created** (Table: dealer_id, create_listing count)
4. **Revenue by Plan** (Bar chart)

#### Dashboard 3: User Journey

**Widgets:**

1. **Path Exploration** (Path analysis from landing)
2. **Drop-off Funnel** (Landing → Search → View → Contact → Purchase)
3. **Top Exit Pages** (Table)
4. **Time to Conversion** (Histogram)

### Explorations

**Funnel Analysis:**

```
1. Explorations → Funnel exploration
2. Steps:
   - Step 1: page_view (path: /)
   - Step 2: search
   - Step 3: view_item
   - Step 4: begin_checkout
   - Step 5: purchase
3. Breakdown: device_category, user_type
```

**Path Exploration:**

```
1. Explorations → Path exploration
2. Starting point: page_view (path: /)
3. Steps: +3 steps forward
4. View: Tree graph
```

---

## 🛒 E-commerce Tracking

### Standard Events

#### View Item List (Search Results)

```typescript
window.gtag("event", "view_item_list", {
  item_list_id: "search_results",
  item_list_name: "Search Results",
  items: vehicles.map((v, index) => ({
    item_id: v.id,
    item_name: v.title,
    item_brand: v.make,
    item_category: v.bodyType,
    price: v.price,
    index: index,
  })),
});
```

#### Select Item (Click from List)

```typescript
window.gtag("event", "select_item", {
  item_list_id: "search_results",
  item_list_name: "Search Results",
  items: [
    {
      item_id: vehicle.id,
      item_name: vehicle.title,
      item_brand: vehicle.make,
      price: vehicle.price,
      index: position,
    },
  ],
});
```

#### View Item (Detail Page)

```typescript
window.gtag("event", "view_item", {
  currency: "DOP",
  value: vehicle.price,
  items: [
    {
      item_id: vehicle.id,
      item_name: vehicle.title,
      item_brand: vehicle.make,
      item_category: vehicle.bodyType,
      item_category2: vehicle.year,
      price: vehicle.price,
      quantity: 1,
    },
  ],
});
```

#### Add to Wishlist (Favorites)

```typescript
window.gtag("event", "add_to_wishlist", {
  currency: "DOP",
  value: vehicle.price,
  items: [
    {
      item_id: vehicle.id,
      item_name: vehicle.title,
      price: vehicle.price,
    },
  ],
});
```

#### Begin Checkout (Contact Seller)

```typescript
window.gtag("event", "begin_checkout", {
  currency: "DOP",
  value: vehicle.price,
  items: [
    {
      item_id: vehicle.id,
      item_name: vehicle.title,
      price: vehicle.price,
    },
  ],
});
```

#### Purchase (Sale Completed)

```typescript
window.gtag("event", "purchase", {
  transaction_id: "T12345",
  affiliation: "OKLA Marketplace",
  value: vehicle.price,
  tax: 0,
  shipping: 0,
  currency: "DOP",
  items: [
    {
      item_id: vehicle.id,
      item_name: vehicle.title,
      item_brand: vehicle.make,
      price: vehicle.price,
      quantity: 1,
    },
  ],
});
```

### Enhanced E-commerce

#### Promotions

```typescript
// View promotion
window.gtag("event", "view_promotion", {
  creative_name: "Early Bird Banner",
  creative_slot: "header",
  promotion_id: "EARLYBIRD2026",
  promotion_name: "3 Months Free",
  items: [
    {
      item_id: "plan_pro",
      item_name: "Dealer Pro Plan",
      discount: 0.2,
    },
  ],
});

// Select promotion
window.gtag("event", "select_promotion", {
  creative_name: "Early Bird Banner",
  creative_slot: "header",
  promotion_id: "EARLYBIRD2026",
  promotion_name: "3 Months Free",
});
```

---

## 🎯 Conversiones y Goals

### Key Conversions para OKLA

| Conversión              | Evento                              | Descripción               | Meta Mensual |
| ----------------------- | ----------------------------------- | ------------------------- | ------------ |
| **Lead Generation**     | `begin_checkout`                    | Usuario contacta vendedor | 500          |
| **Dealer Registration** | `sign_up` (user_type: dealer)       | Nuevo dealer se registra  | 20           |
| **Subscription**        | `purchase` (category: Subscription) | Dealer se suscribe a plan | 15           |
| **Listing Created**     | `create_listing`                    | Seller publica vehículo   | 100          |
| **Favorite Added**      | `add_to_wishlist`                   | Usuario guarda favorito   | 1,000        |

### Configurar Conversiones en GA4

```
1. Admin → Events
2. Click "Mark as conversion" para estos eventos:
   - begin_checkout
   - sign_up
   - purchase
   - create_listing
3. Configure conversion value:
   - Usar event parameter "value"
   - O set default value (Ej: $100 por lead)
```

### Conversión Value

```typescript
// Lead with value
window.gtag("event", "begin_checkout", {
  value: 100, // $100 estimated value per lead
  currency: "USD",
  // ... items
});

// Subscription with actual value
window.gtag("event", "purchase", {
  value: plan === "Pro" ? 103 : plan === "Enterprise" ? 239 : 39,
  currency: "DOP",
  // ... transaction details
});
```

---

## 🔒 Privacidad y GDPR

### Consentimiento de Cookies

#### Cookie Consent Banner

```tsx
// src/components/CookieConsent.tsx
import { useState, useEffect } from "react";

export const CookieConsent = () => {
  const [showBanner, setShowBanner] = useState(false);

  useEffect(() => {
    const consent = localStorage.getItem("cookie_consent");
    if (!consent) {
      setShowBanner(true);
    } else if (consent === "accepted") {
      // Initialize GA4
      window.gtag("consent", "update", {
        analytics_storage: "granted",
      });
    }
  }, []);

  const acceptCookies = () => {
    localStorage.setItem("cookie_consent", "accepted");
    setShowBanner(false);

    // Update consent
    window.gtag("consent", "update", {
      analytics_storage: "granted",
    });

    // Initialize GA4
    window.gtag("config", GA_MEASUREMENT_ID);
  };

  const rejectCookies = () => {
    localStorage.setItem("cookie_consent", "rejected");
    setShowBanner(false);

    // Deny consent
    window.gtag("consent", "update", {
      analytics_storage: "denied",
    });
  };

  if (!showBanner) return null;

  return (
    <div className="fixed bottom-0 left-0 right-0 bg-gray-900 text-white p-4 z-50">
      <div className="container mx-auto flex flex-col md:flex-row items-center justify-between gap-4">
        <p className="text-sm">
          🍪 Usamos cookies para mejorar tu experiencia. Al continuar navegando,
          aceptas nuestra{" "}
          <a href="/privacy" className="underline">
            Política de Privacidad
          </a>
          .
        </p>
        <div className="flex gap-2">
          <button
            onClick={acceptCookies}
            className="px-4 py-2 bg-blue-600 rounded hover:bg-blue-700"
          >
            Aceptar
          </button>
          <button
            onClick={rejectCookies}
            className="px-4 py-2 bg-gray-700 rounded hover:bg-gray-600"
          >
            Rechazar
          </button>
        </div>
      </div>
    </div>
  );
};
```

### Consent Mode

```html
<!-- public/index.html -->
<script>
  // Default consent state (before user choice)
  window.dataLayer = window.dataLayer || [];
  function gtag() {
    dataLayer.push(arguments);
  }

  gtag("consent", "default", {
    analytics_storage: "denied",
    ad_storage: "denied",
    wait_for_update: 500,
  });

  gtag("js", new Date());
  gtag("config", "G-XXXXXXXXXX");
</script>
```

### IP Anonymization

GA4 anonymizes IP addresses by default. No additional configuration needed.

### User Data Deletion

```typescript
// Allow users to request data deletion
export const requestDataDeletion = async (userId: string) => {
  // 1. Backend: Mark user for deletion
  await api.post("/users/delete-data", { userId });

  // 2. Clear GA4 user_id
  window.gtag("config", GA_MEASUREMENT_ID, {
    user_id: undefined,
  });

  // 3. Clear local storage
  localStorage.clear();
  sessionStorage.clear();
};
```

### Data Retention

**Configurar en GA4:**

```
1. Admin → Data Settings → Data Retention
2. Event data retention: 14 months (recommended)
3. Reset user data on new activity: ON
```

---

## 🎯 Mejores Prácticas

### 1. Event Naming

✅ **DO:**

- Usar snake_case: `begin_checkout`, `view_item`
- Nombres descriptivos: `dealer_registration` en vez de `reg`
- Consistentes con GA4 standard events

❌ **DON'T:**

- No usar camelCase: `beginCheckout`
- No usar espacios: `begin checkout`
- No crear eventos redundantes

### 2. Parameters

✅ **DO:**

- Máximo 25 parámetros por evento
- Nombres cortos y claros
- Usar tipos correctos (string, number, boolean)

❌ **DON'T:**

- No pasar objetos complejos (serializar a JSON)
- No incluir PII (nombres, emails, teléfonos)
- No usar parámetros dinámicos (crear custom dimensions)

### 3. User Properties

✅ **DO:**

```typescript
// Good
analyticsService.setUserProperties({
  account_type: "Dealer",
  plan: "Pro",
  city: "Santo Domingo",
});
```

❌ **DON'T:**

```typescript
// Bad - PII
analyticsService.setUserProperties({
  email: user.email, // ❌ No!
  full_name: user.name, // ❌ No!
  phone: user.phone, // ❌ No!
});
```

### 4. Performance

✅ **DO:**

- Lazy load analytics code
- Usar `send_page_view: false` y track manualmente
- Batch events cuando sea posible

❌ **DON'T:**

- No bloquear rendering
- No trackear en loops
- No enviar eventos cada 100ms

---

## 🔧 Troubleshooting

### Problema: Eventos No Aparecen en GA4

**Verificación:**

```javascript
// 1. Check if gtag is loaded
console.log(window.gtag); // Should be a function

// 2. Check dataLayer
console.log(window.dataLayer); // Should be an array with events

// 3. Enable debug mode
window.gtag("config", GA_MEASUREMENT_ID, {
  debug_mode: true,
});

// 4. Use GA4 DebugView
// Admin → DebugView (requires debug_mode: true)
```

**Soluciones:**

- Verificar Measurement ID correcto
- Verificar que script gtag.js cargó
- Desactivar ad blockers temporalmente
- Revisar console para errores

### Problema: Web Vitals No Se Reportan

**Debug:**

```typescript
// Add console.log to sendToGoogleAnalytics
const sendToGoogleAnalytics = (metric: Metric) => {
  console.log('[Web Vitals]', metric.name, metric.value);

  if (!window.gtag) {
    console.error('gtag not loaded!');
    return;
  }

  window.gtag('event', metric.name, {...});
};
```

**Soluciones:**

- Asegurar que `web-vitals` package está instalado
- Verificar que `reportWebVitals()` se llama en main.tsx
- Esperar a que métricas se generen (pueden tardar segundos)

### Problema: Conversiones No Se Marcan

**Verificación:**

```
1. Admin → Events
2. Buscar evento (Ej: "begin_checkout")
3. Verificar que esté marcado como "Conversion"
4. Si no existe, crear manualmente
```

### Problema: Real-Time No Funciona

**Causas comunes:**

- Filtros de IP bloqueando tráfico interno
- Ad blockers activos
- Consent mode denegado
- Measurement ID incorrecto

**Solución:**

```typescript
// Test real-time tracking
window.gtag("event", "test_event", {
  debug: true,
  timestamp: Date.now(),
});

// Luego ir a: Reports → Realtime → Event count by Event name
// Buscar "test_event"
```

---

## 💰 Costos y Límites

### Google Analytics 4 - FREE Tier

| Límite                | Valor          | Notas            |
| --------------------- | -------------- | ---------------- |
| **Eventos/mes**       | 10M            | FREE             |
| **Conversiones**      | Ilimitadas     | FREE             |
| **Custom dimensions** | 50             | FREE             |
| **Custom metrics**    | 50             | FREE             |
| **Audiences**         | 100            | FREE             |
| **Data retention**    | 14 meses       | Configurable     |
| **BigQuery export**   | 1M eventos/día | FREE (con cuota) |

### Google Analytics 360 (Pagado)

| Característica     | GA4 Free  | GA4 360     |
| ------------------ | --------- | ----------- |
| **Costo**          | FREE      | $150K/año   |
| **Eventos/mes**    | 10M       | Ilimitado   |
| **Data freshness** | 24-48h    | Tiempo real |
| **SLA**            | No        | 99.9%       |
| **Support**        | Community | Dedicated   |
| **BigQuery**       | 1M/día    | Ilimitado   |

### Para OKLA (Estimación)

**Volumen mensual esperado:**

- Users: 50,000
- Sessions: 150,000
- Events: ~3M (promedio 20 eventos/sesión)

**Conclusión:** ✅ FREE tier es suficiente (< 10M eventos/mes)

---

## 📚 Recursos Adicionales

### Documentación Oficial

- **GA4 Home:** https://support.google.com/analytics/answer/10089681
- **gtag.js Reference:** https://developers.google.com/tag-platform/gtagjs/reference
- **Events Reference:** https://developers.google.com/analytics/devguides/collection/ga4/reference/events
- **E-commerce:** https://developers.google.com/analytics/devguides/collection/ga4/ecommerce

### Herramientas

- **GA Debugger (Chrome Extension):** Debug events en browser
- **Tag Assistant:** https://tagassistant.google.com/
- **GA4 Query Explorer:** https://ga-dev-tools.google/ga4/query-explorer/

### Cursos

- **Google Analytics Academy:** https://analytics.google.com/analytics/academy/
- **GA4 for Beginners:** Curso gratuito oficial
- **Advanced GA4:** Para análisis profundo

### Comunidad

- **Google Analytics Community:** https://support.google.com/analytics/community
- **Reddit:** r/GoogleAnalytics
- **YouTube:** Official Google Analytics channel

---

## 📝 Changelog

| Versión   | Fecha          | Cambios                        |
| --------- | -------------- | ------------------------------ |
| **1.0.0** | Enero 15, 2026 | Documentación inicial completa |

---

## 👥 Contacto y Soporte

**Equipo de Desarrollo OKLA:**

- **Email:** dev@okla.com.do
- **Slack:** #analytics-support
- **GitHub:** gregorymorenoiem/cardealer-microservices

**Google Support:**

- **Community:** https://support.google.com/analytics/community
- **Enterprise (GA360):** Contact Google sales

---

**✅ Documentación completada - Ready for Production**

_Esta documentación es parte del proyecto OKLA Marketplace. Para más información sobre otras APIs, consulta [API_MASTER_INDEX.md](../API_MASTER_INDEX.md)._

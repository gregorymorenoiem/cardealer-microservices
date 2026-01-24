# 📚 Documentación de APIs Externas - OKLA Marketplace

**Plataforma:** OKLA (CarDealer Microservices)  
**Tipo:** APIs de Terceros (Third-Party APIs)  
**Última actualización:** Enero 15, 2026

---

## 🎯 ¿Qué encontrarás aquí?

Esta carpeta contiene **documentación completa** de todas las **APIs externas** (de terceros) utilizadas en la plataforma OKLA. Cada API tiene:

✅ **Documentación técnica completa**  
✅ **Roadmap de evolución**  
✅ **Ejemplos de código C#/.NET**  
✅ **Casos de uso en OKLA**  
✅ **Troubleshooting y mejores prácticas**

---

## 📋 Índice Rápido

### 🚀 Comienza Aquí

| Documento                                                                        | Descripción                                               |
| -------------------------------------------------------------------------------- | --------------------------------------------------------- |
| **[API_MASTER_INDEX.md](API_MASTER_INDEX.md)**                                   | 📊 Índice maestro de TODAS las APIs (externas + internas) |
| **[ROADMAP_CONSOLIDADO_APIS_EXTERNAS.md](ROADMAP_CONSOLIDADO_APIS_EXTERNAS.md)** | 🗓️ Roadmap unificado 2025-2027                            |

---

## 💳 APIs de Pagos

### AZUL (Banco Popular RD)

Pasarela de pagos para **tarjetas locales dominicanas**.

| Documento                                                                    | Descripción                         |
| ---------------------------------------------------------------------------- | ----------------------------------- |
| **[payments/AZUL_API_DOCUMENTATION.md](payments/AZUL_API_DOCUMENTATION.md)** | Documentación completa de AZUL API  |
| **[payments/AZUL_ROADMAP.md](payments/AZUL_ROADMAP.md)**                     | Roadmap AZUL: 5 fases hasta Q3 2026 |

**Estado:** ✅ En Producción  
**Casos de uso:**

- Pagos de dealers (plan mensual)
- Pagos de compradores (listings promocionados)
- Refunds

**Comisión:** ~2.5% por transacción  
**Depósito:** 24-48 horas

---

### Stripe

Pasarela de pagos para **tarjetas internacionales** + **subscripciones**.

| Documento                                                                        | Descripción                           |
| -------------------------------------------------------------------------------- | ------------------------------------- |
| **[payments/STRIPE_API_DOCUMENTATION.md](payments/STRIPE_API_DOCUMENTATION.md)** | Documentación completa de Stripe API  |
| **[payments/STRIPE_ROADMAP.md](payments/STRIPE_ROADMAP.md)**                     | Roadmap Stripe: 5 fases hasta Q3 2026 |

**Estado:** ✅ En Producción  
**Casos de uso:**

- Subscripciones mensuales de dealers (Starter, Pro, Enterprise)
- Pagos internacionales
- Apple Pay / Google Pay (planificado Q2 2026)

**Comisión:** ~3.5% por transacción  
**Depósito:** 7 días

**🎯 Early Bird Program activo hasta 31 enero 2026:**

- 3 MESES GRATIS
- 20% descuento de por vida
- Badge "Miembro Fundador"

---

## ☁️ Storage & Media

### Amazon S3 / DigitalOcean Spaces

Almacenamiento de archivos e imágenes (compatible S3).

| Documento                                                              | Descripción                             |
| ---------------------------------------------------------------------- | --------------------------------------- |
| **[storage/S3_API_DOCUMENTATION.md](storage/S3_API_DOCUMENTATION.md)** | Documentación completa de S3/Spaces API |
| **[storage/S3_ROADMAP.md](storage/S3_ROADMAP.md)**                     | Roadmap S3: 5 fases hasta Q3 2026       |

**Estado:** ✅ En Producción  
**Bucket:** `okla-media` (región nyc3)  
**CDN:** ✅ Habilitado

**Casos de uso:**

- Imágenes de vehículos (múltiples tamaños)
- Avatares de usuarios
- Documentos de dealers (RNC, licencias)
- Videos (planificado Q3 2026)

**Costo:** $5/mes (250 GB storage + 1 TB bandwidth incluido)

---

## 🗄️ Bases de Datos & Cache

### PostgreSQL

Base de datos relacional principal.

| Documento                                                                                            | Descripción                          |
| ---------------------------------------------------------------------------------------------------- | ------------------------------------ |
| **[infrastructure/POSTGRESQL_API_DOCUMENTATION.md](infrastructure/POSTGRESQL_API_DOCUMENTATION.md)** | Documentación completa de PostgreSQL |

**Estado:** ✅ En Producción  
**Versión:** 16  
**Databases:** 16 bases de datos (una por microservicio)

**Tamaño total:** ~8 GB (Enero 2026)

**Casos de uso:**

- Persistencia de todos los datos
- Transacciones ACID
- Full-text search
- JSON columns para datos flexibles

---

### Redis

Cache distribuido y session store.

| Documento                                                                                  | Descripción                     |
| ------------------------------------------------------------------------------------------ | ------------------------------- |
| **[infrastructure/REDIS_API_DOCUMENTATION.md](infrastructure/REDIS_API_DOCUMENTATION.md)** | Documentación completa de Redis |

**Estado:** ✅ En Producción  
**Versión:** 7

**Casos de uso:**

- Cache de vehículos populares
- JWT refresh tokens
- Rate limiting (100 requests/min)
- Idempotency keys de pagos
- Session storage

**Performance:** <1ms latencia promedio

---

### RabbitMQ

Message broker para comunicación asíncrona.

| Documento                                                                                        | Descripción                        |
| ------------------------------------------------------------------------------------------------ | ---------------------------------- |
| **[infrastructure/RABBITMQ_API_DOCUMENTATION.md](infrastructure/RABBITMQ_API_DOCUMENTATION.md)** | Documentación completa de RabbitMQ |

**Estado:** ✅ En Producción  
**Versión:** 3.12

**Casos de uso:**

- Eventos de vehículos (created, updated, deleted)
- Jobs de procesamiento de imágenes
- Cola de notificaciones (email, SMS, push)
- Errores centralizados
- Webhooks de pagos

**Management UI:** http://localhost:15672

---

## 📧 Notificaciones y Comunicación

### SendGrid Email API

Servicio de email transaccional con templates avanzados.

| Documento                                                                                      | Descripción                         |
| ---------------------------------------------------------------------------------------------- | ----------------------------------- |
| **[notifications/SENDGRID_API_DOCUMENTATION.md](notifications/SENDGRID_API_DOCUMENTATION.md)** | Documentación completa de SendGrid  |
| **[notifications/SENDGRID_ROADMAP.md](notifications/SENDGRID_ROADMAP.md)**                     | Roadmap: 25+ templates, A/B testing |

**Estado:** ✅ En Producción  
**Delivery Rate:** 99.2%+ | **Open Rate:** 22%+

**Casos de uso:**

- Confirmación de registro
- Reset de password
- Notificaciones de vehículos
- Invoices de pagos
- Alertas de precio

**Costo:** $0/mes (free tier, bajo volumen)

---

### Twilio SMS API

Servicio SMS para notificaciones críticas.

| Documento                                                                                  | Descripción                      |
| ------------------------------------------------------------------------------------------ | -------------------------------- |
| **[notifications/TWILIO_API_DOCUMENTATION.md](notifications/TWILIO_API_DOCUMENTATION.md)** | Documentación completa de Twilio |

**Estado:** 🚧 En Configuración (Q1 2026)  
**Delivery Rate:** 99.8%

**Casos de uso:**

- OTP para login
- Alertas de precio críticas
- Recordatorios urgentes
- Notificaciones a dealers

**Costo:** $0.0075/SMS (~$10/mes)

---

### Firebase Cloud Messaging (FCM)

Push notifications para app móvil Flutter.

| Documento                                                                            | Descripción                |
| ------------------------------------------------------------------------------------ | -------------------------- |
| **[notifications/FCM_API_DOCUMENTATION.md](notifications/FCM_API_DOCUMENTATION.md)** | Documentación FCM completa |

**Estado:** 📝 Planificado (Q3 2026)

**Casos de uso:**

- Notificaciones push en app
- Alertas en tiempo real
- Mensajes de chat
- Updates de vehículos

**Costo:** ✅ **GRATUITO** (sin límites)

---

## 🗺️ Geolocalización

### Google Maps API

Mapas, direcciones y geolocalización.

| Documento                                                                                        | Descripción               |
| ------------------------------------------------------------------------------------------------ | ------------------------- |
| **[geolocation/GOOGLE_MAPS_API_DOCUMENTATION.md](geolocation/GOOGLE_MAPS_API_DOCUMENTATION.md)** | Documentación Google Maps |

**Estado:** 🚧 En Configuración (Q1 2026)

**Casos de uso:**

- Mostrar ubicación de vehículos
- Buscar dealers cercanos
- Autocomplete de direcciones
- Calcular distancias

**Costo:** Mostly free (dentro de free tier)

---

## 💬 Mensajería Empresarial

### WhatsApp Business API

Mensajería para clientes y dealers.

| Documento                                                                                                | Descripción                     |
| -------------------------------------------------------------------------------------------------------- | ------------------------------- |
| **[messaging/WHATSAPP_BUSINESS_API_DOCUMENTATION.md](messaging/WHATSAPP_BUSINESS_API_DOCUMENTATION.md)** | Documentación WhatsApp Business |

**Estado:** 🚧 Planificado (Q2 2026)

**Casos de uso:**

- Confirmaciones de orden
- Notificaciones de entrega
- Consultas de soporte
- Alertas de vehículos
- Mensajes de marketing (templates)

**Ventajas:**

- Open rate: 98% (vs 25% email)
- Click rate: 15% (vs 5% email)
- Cobertura muy alta en RD

**Costo:** $0.005-$0.008 por mensaje

---

## 🤖 Inteligencia Artificial

### OpenAI API (GPT-4, GPT-3.5)

Chatbot inteligente y análisis de leads.

| Documento                                                            | Descripción              |
| -------------------------------------------------------------------- | ------------------------ |
| **[ai/OPENAI_API_DOCUMENTATION.md](ai/OPENAI_API_DOCUMENTATION.md)** | Documentación OpenAI API |

**Estado:** 🚧 Planificado (Q3 2026)

**Casos de uso:**

- Chatbot en homepage
- Recomendaciones personalizadas
- Lead scoring automático
- Generación de descripciones
- Análisis de inquietudes

**Costo:** ~$100/mes (estimado)

---

### Zoho Mail API (Alternativa)

Servicio de email transaccional (backup).

**Estado:** 📝 Evaluando (Q2 2026)

**Casos de uso:**

- Email alternativo a SendGrid
- Automaciones de CRM
- Integración con Zoho Suite

---

## 📊 Estadísticas Generales

### APIs en Producción y Configuración

| API                   | Estado          | Versión    | Costo Mensual |
| --------------------- | --------------- | ---------- | ------------- |
| **AZUL**              | ✅ Producción   | 2.0        | $0\*          |
| **Stripe**            | ✅ Producción   | 2024-01-15 | $50           |
| **S3/Spaces**         | ✅ Producción   | AWS SDK v3 | $5            |
| **PostgreSQL**        | ✅ Producción   | 16         | $0\*\*        |
| **Redis**             | ✅ Producción   | 7          | $0\*\*        |
| **RabbitMQ**          | ✅ Producción   | 3.12       | $0\*\*        |
| **SendGrid**          | ✅ Producción   | v3         | $0\*\*\*      |
| **Twilio**            | 🚧 Configurando | v1         | $10\*\*\*     |
| **Google Maps**       | 🚧 Configurando | v3         | $0\*\*\*\*    |
| **Firebase FCM**      | 📝 Q3 2026      | v1         | $0            |
| **WhatsApp Business** | 🚧 Q2 2026      | v18        | ~$20          |
| **OpenAI**            | 🚧 Q3 2026      | v1         | ~$100         |
| **Zoho Mail**         | 📝 Q2 2026      | API v1     | $0**\***      |

\* Comisión por transacción (~2.5%)  
\*\* Incluido en cluster DOKS  
\*\*\* Free tier (bajo volumen)  
\*\*\*\* Mostly within free tier  
\***\*** Plan gratuito inicial

**Costo actual (Enero 2026):** ~$65/mes  
**Costo proyectado (Q4 2026):** ~$235/mes  
**Total anual 2026:** ~$1,500

---

## 🗓️ Roadmap 2026

### Q1 2026 (Enero-Marzo) - CONSOLIDACIÓN ✅ 75%

**Objetivo:** Estabilizar APIs en producción

- ✅ AZUL pagos básicos (DEFAULT para tarjetas dominicanas)
- ✅ Stripe subscriptions activas (backup internacional)
- ✅ S3 con CDN
- ✅ ~~Stripe Connect~~ DESCARTADO (no aplica al modelo)
- 🚧 AZUL 3D Secure

> **NOTA:** Stripe Connect fue descartado. OKLA es plataforma de publicidad,
> no marketplace transaccional. Los dealers PAGAN a OKLA por suscripciones.

**Hitos:**

- ✅ 23 dealers suscritos
- ✅ 45GB storage usado
- 🎯 50 dealers (fin Q1)

---

### Q2 2026 (Abril-Junio) - ADVANCED FEATURES

**Objetivo:** Features avanzadas para escalar

- ✅ ~~Stripe Connect producción~~ DESCARTADO
- 🚧 AZUL webhooks + recurring payments
- 🚧 S3 video upload + transcoding
- 📝 RabbitMQ quorum queues
- 📝 PostgreSQL read replicas

**Hitos:**

- 🎯 100+ dealers suscritos
- 🎯 $15K MRR
- 🎯 Video features activas

---

### Q3 2026 (Julio-Septiembre) - SCALABILITY

**Objetivo:** Preparar para crecimiento exponencial

- Apple Pay + Google Pay (Stripe)
- S3 live streaming
- Redis cluster mode
- PostgreSQL partitioning
- Firebase FCM integration

**Hitos:**

- 🎯 250+ dealers suscritos
- 🎯 $40K MRR
- 🎯 1M+ operations/día

---

### Q4 2026 (Octubre-Diciembre) - OPTIMIZATION

**Objetivo:** Optimizar costos y performance

- Cost optimization audit
- Security audit completo
- Disaster recovery drills
- Performance profiling
- Nuevas integraciones (OpenAI, WhatsApp)

**Hitos:**

- 🎯 500+ dealers suscritos
- 🎯 $80K MRR
- 🎯 99.99% uptime

---

## 🛠️ Guías de Uso

### Para Desarrolladores

1. **Empezar con una API:**
   - Leer documentación técnica en `{api}/API_DOCUMENTATION.md`
   - Revisar ejemplos de código C#/.NET
   - Consultar casos de uso en OKLA
   - Implementar siguiendo best practices

2. **Entender el roadmap:**
   - Leer roadmap específico en `{api}/ROADMAP.md`
   - Ver qué features están disponibles ahora
   - Planificar features futuras

3. **Troubleshooting:**
   - Consultar sección "Manejo de errores" en documentación
   - Revisar logs en Kubernetes (`kubectl logs`)
   - Verificar health checks

### Para Product Managers

1. **Planificación de features:**
   - Consultar [ROADMAP_CONSOLIDADO_APIS_EXTERNAS.md](ROADMAP_CONSOLIDADO_APIS_EXTERNAS.md)
   - Ver dependencias entre APIs
   - Estimar costos y tiempos

2. **Tracking de progreso:**
   - Revisar estado actual de cada API
   - Ver KPIs y métricas
   - Identificar blockers

### Para DevOps

1. **Deployment:**
   - Verificar configuración en `appsettings.json`
   - Configurar secrets en Kubernetes
   - Monitorear health checks

2. **Monitoring:**
   - Configurar alertas por API
   - Dashboard de métricas
   - Logs centralizados

---

## 📞 Soporte y Contacto

### Soporte Interno (OKLA)

- **Email:** dev@okla.com.do
- **Slack:** #api-support
- **Jira:** OKLA Project

### Soporte de Providers

| Provider         | Email                    | SLA |
| ---------------- | ------------------------ | --- |
| **AZUL**         | soporte@azul.com.do      | 48h |
| **Stripe**       | support@stripe.com       | 24h |
| **DigitalOcean** | support@digitalocean.com | 4h  |

---

## 🔗 Enlaces Útiles

### Documentación Externa

- [AZUL Developers](https://desarrolladores.azul.com.do)
- [Stripe Docs](https://stripe.com/docs/api)
- [DigitalOcean Spaces](https://docs.digitalocean.com/products/spaces/)
- [PostgreSQL Docs](https://www.postgresql.org/docs/)
- [Redis Docs](https://redis.io/docs/)
- [RabbitMQ Docs](https://www.rabbitmq.com/docs)

### Implementación OKLA

- [Copilot Instructions](../.github/copilot-instructions.md)
- [Sprint Plans](../sprints/)
- [Architecture Docs](../architecture/)

---

## ⚠️ Notas Importantes

### Seguridad

- ❌ **NUNCA** commitear API keys o secrets en el código
- ✅ Usar Kubernetes Secrets o environment variables
- ✅ Rotar credentials cada 90 días
- ✅ Auditar accesos regularmente

### Performance

- ✅ Siempre usar cache (Redis) para datos frecuentes
- ✅ Connection pooling para PostgreSQL
- ✅ Batch operations en RabbitMQ
- ✅ CDN para archivos estáticos (S3)

### Costos

- 📊 Monitorear uso mensual de cada API
- 🚨 Configurar alertas al 80% del límite
- 💰 Optimizar queries y storage regularmente
- 📈 Revisar costos vs beneficios trimestralmente

---

## 🎓 Recursos de Aprendizaje

### Tutoriales OKLA

- [Tutorial 1: Setup AZUL](../tutorials/01-setup-azul.md)
- [Tutorial 2: Stripe Subscriptions](../tutorials/02-stripe-subscriptions.md)
- [Tutorial 3: S3 File Upload](../tutorials/03-s3-upload.md)
- [Tutorial 4: Redis Caching](../tutorials/04-redis-cache.md)

### Cursos Recomendados

- [Stripe for Developers](https://stripe.com/docs/development)
- [AWS S3 Masterclass](https://aws.amazon.com/s3/getting-started/)
- [PostgreSQL Performance Tuning](https://www.postgresql.org/docs/current/performance-tips.html)
- [Redis University](https://university.redis.com/)

---

## 📝 Changelog

### Enero 15, 2026

- ✅ Creada documentación completa de APIs externas
- ✅ Roadmaps individuales para cada API
- ✅ Roadmap consolidado 2025-2027
- ✅ Ejemplos de código C#/.NET
- ✅ Casos de uso en OKLA

---

## 🙋 FAQ

### ¿Por qué usar AZUL y Stripe?

**AZUL** tiene mejor conversión con tarjetas dominicanas (comisión más baja, depósito más rápido). **Stripe** es backup para tarjetas internacionales.

### ¿OKLA cobra comisión por venta de vehículos?

**NO.** OKLA es una plataforma de **publicidad**, no un marketplace transaccional. Los dealers pagan una suscripción mensual (RD$2,900-14,900) por publicar vehículos. La venta del vehículo ocurre **fuera de OKLA** (directamente entre dealer y comprador). OKLA no participa ni cobra comisión en esas transacciones.

### ¿Se usará Stripe Connect?

**NO.** Stripe Connect es para marketplaces donde la plataforma cobra comisión por transacciones. Como OKLA no procesa pagos de vehículos, Stripe Connect no aplica. Los dealers son **clientes** de OKLA (pagan suscripción), no comerciantes que reciben pagos a través de la plataforma.

### ¿Por qué DigitalOcean Spaces y no AWS S3?

Misma API (compatible S3), pero más económico y con CDN incluido. Perfecto para startups.

### ¿Necesitamos Redis en producción?

Sí, **crítico**. Reduce carga en PostgreSQL ~80% y mejora latencia de API ~60%. Sin Redis, el sistema no escala.

### ¿Cuándo usar RabbitMQ vs API directa?

**RabbitMQ** para operaciones asíncronas (emails, procesamiento de imágenes). **API directa** para operaciones síncronas (login, búsquedas).

---

**¿Preguntas?** Abre un issue en GitHub o contacta al equipo de arquitectura.

---

**Mantenido por:** Equipo de Arquitectura OKLA  
**Última revisión:** Enero 15, 2026  
**Próxima revisión:** Abril 1, 2026

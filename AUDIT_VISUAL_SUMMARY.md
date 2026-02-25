# 🎯 AUDIT VISUAL SUMMARY - SellerProfile

**Fecha:** 24 de febrero de 2026  
**Propósito:** Resumen visual para presentación al equipo

---

## 📊 El Problema en 1 Página

```
┌─────────────────────────────────────────────────────────────┐
│           USUARIO REGISTRA COMO VENDEDOR                    │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  STEP 1: Crear Cuenta                                        │
│  ├─ firstName: "Juan"                ✓ Se captura           │
│  ├─ lastName: "Pérez"                ✓ Se captura           │
│  ├─ email: "juan@example.com"        ✓ Se captura           │
│  ├─ phone: "+1-809-555-0123"         ✓ Se captura           │
│  └─ password: "xxx"                  ✓ Se captura           │
│                                                              │
│  STEP 2: Perfil de Vendedor                                  │
│  ├─ displayName: "Juan Vende Autos"  ✓ Se captura           │
│  ├─ businessName: "JPA Auto Sales"   ✓ Se captura           │
│  ├─ description: "Vendo de todo..."  ✓ Se captura           │
│  ├─ phone: "+1-809-555-0123"         ⚠️  DUPLICADO!         │
│  ├─ location: "Santo Domingo, DN"    ✓ Se captura           │
│  └─ specialties: ["Sedanes", ...]    ✓ Se captura           │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│           BACKEND PROCESA DATOS                              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Mapeo: Frontend → SellerProfile Entity                      │
│  ├─ displayName → FullName           ⚠️  NO CLARO           │
│  ├─ description → Bio                ✓ OK                   │
│  ├─ phone → Phone                    ✓ OK                   │
│  ├─ location → ???                   ❌ PROBLEMA #1          │
│  │   Expected: City, State, Address, ZipCode               │
│  │   Got: "Santo Domingo, DN"                              │
│  │   Result: Address=vacio, City=parcial                   │
│  └─ specialties → ???                ❌ PROBLEMA #2          │
│      Expected: Specialties[] column                        │
│      Got: campo no existe                                  │
│      Result: DATOS PERDIDOS                                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│           BASE DE DATOS ALMACENA                             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  seller_profiles table:                                      │
│  ├─ id: UUID ✓                                              │
│  ├─ fullName: "Juan Pérez"           ✓                      │
│  ├─ phone: "+1-809-555-0123"         ✓                      │
│  ├─ email: "juan@example.com"        ✓                      │
│  ├─ bio: "Vendo de todo..."          ✓                      │
│  ├─ city: "Santo Domingo" (parcial)  ⚠️  INCOMPLETO         │
│  ├─ state: NULL                      ❌ VACÍO               │
│  ├─ address: NULL                    ❌ VACÍO               │
│  ├─ zipCode: NULL                    ❌ VACÍO               │
│  ├─ specialties: <no existe>         ❌ NO EXISTE           │
│  └─ businessName: <no existe>        ❌ NO PERSISTE         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│           USUARIO VE SU PERFIL                               │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  /cuenta/perfil:                                             │
│  ├─ Nombre del negocio: "JPA Auto Sales"                    │
│  ├─ Nombre público: "Juan Vende Autos"  ✓ Aparece OK       │
│  ├─ Descripción: "Vendo de todo..."     ✓ Aparece OK       │
│  ├─ Teléfono: "+1-809-555-0123"         ✓ Aparece          │
│  ├─ Ubicación: "Santo Domingo"          ⚠️  Incompleta      │
│  │   (Falta State, Address, ZipCode)                       │
│  └─ Especialidades: (vacío)             ❌ SE PERDIERON      │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔴 5 Problemas Identificados

```
┌──────────────────────────────────────┐
│ PROBLEMA #1: TELÉFONO DUPLICADO     │ 🔴 CRÍTICO
├──────────────────────────────────────┤
│ Se pide en AMBOS steps                │
│ ├─ Step 1: Teléfono personal         │
│ └─ Step 2: Teléfono de contacto(?)   │
│                                      │
│ Usuario confundido: ¿Cuál es cuál?   │
│ Backend recibe 2 teléfonos            │
│ ¿Cuál guardar?                        │
│                                      │
│ ✅ SOLUCIÓN:                         │
│    Remover teléfono de Step 2        │
│    Usar solo del Step 1              │
└──────────────────────────────────────┘

┌──────────────────────────────────────┐
│ PROBLEMA #2: ESPECIALIDADES PERDIDAS│ 🔴 CRÍTICO
├──────────────────────────────────────┤
│ Se capturan: ✓                       │
│ Se validan: ✓                        │
│ Se envían al API: ✓                  │
│ Se guardan en BD: ❌ NO              │
│                                      │
│ Campo no existe en entity             │
│ Datos se pierden                      │
│                                      │
│ ✅ SOLUCIÓN:                         │
│    1. Migration: Agregar columna     │
│    2. Entity: Agregar property       │
│    3. Handlers: Mapear correctamente │
│    4. DTOs: Incluir en response      │
└──────────────────────────────────────┘

┌──────────────────────────────────────┐
│ PROBLEMA #3: UBICACIÓN INCOMPLETA   │ 🟠 ALTO
├──────────────────────────────────────┤
│ Se captura: "Santo Domingo, DN"     │
│ BD espera:                            │
│  ├─ City: "Santo Domingo" ✓         │
│  ├─ State: "Distrito Nacional" ❌   │
│  ├─ Address: (específica) ❌         │
│  └─ ZipCode: (postal) ❌             │
│                                      │
│ Datos se mapean incorrectamente       │
│ Búsqueda por ubicación no funciona   │
│                                      │
│ ✅ SOLUCIÓN:                         │
│    Frontend: 4 inputs (no 1)         │
│    Provincia dropdown                 │
│    Ciudad dropdown                    │
│    Dirección input                    │
│    Código postal input                │
└──────────────────────────────────────┘

┌──────────────────────────────────────┐
│ PROBLEMA #4: displayName vs FullName│ 🟡 MEDIO
├──────────────────────────────────────┤
│ Frontend envía: displayName           │
│ Backend espera: FullName              │
│ ¿Son lo mismo?                        │
│ ¿Cómo se mapean?                      │
│                                      │
│ Ambiguedad causa bugs                │
│                                      │
│ ✅ SOLUCIÓN:                         │
│    Documentar claramente              │
│    Mapeo explícito en handlers       │
│    Tests que validen                 │
└──────────────────────────────────────┘

┌──────────────────────────────────────┐
│ PROBLEMA #5: CAMPOS FALTANTES       │ 🟡 MEDIO
├──────────────────────────────────────┤
│ Existen en BD:                        │
│  ├─ DateOfBirth (no se captura)     │
│  ├─ Nationality (no se captura)     │
│  ├─ AlternatePhone (no se captura)  │
│  ├─ WhatsApp (no se captura)        │
│  └─ AvatarUrl (no se captura)       │
│                                      │
│ Datos subutilizados                  │
│                                      │
│ ✅ SOLUCIÓN:                         │
│    Agregar en Settings               │
│    (No en registro)                  │
└──────────────────────────────────────┘
```

---

## 📊 Impacto en Números

```
┌─────────────────────────────────────────┐
│ COMPONENTES AFECTADOS: 20+              │
├─────────────────────────────────────────┤
│ Frontend (8)                            │
│  ├─ /vender/registro                   │
│  ├─ /cuenta/perfil                     │
│  ├─ /publicar                          │
│  ├─ /cuenta                            │
│  ├─ ProfileStep                        │
│  ├─ ProfilePage                        │
│  ├─ seller-card                        │
│  └─ dashboard                          │
│                                         │
│ Backend (7)                             │
│  ├─ SellerProfileController            │
│  ├─ CreateSellerProfileHandler         │
│  ├─ UpdateSellerProfileHandler         │
│  ├─ GetSellerProfileHandler            │
│  ├─ CreateSellerProfileRequest DTO     │
│  ├─ UpdateSellerProfileRequest DTO     │
│  └─ SellerProfileDto                   │
│                                         │
│ Base de Datos (1)                       │
│  └─ seller_profiles table              │
│                                         │
│ RabbitMQ Events (3)                     │
│  ├─ VehiclesSaleService                │
│  ├─ ReviewService                      │
│  └─ NotificationService                │
│                                         │
│ Otros (1+)                              │
│  └─ Search, Filters, Integrations      │
│                                         │
└─────────────────────────────────────────┘
```

---

## 🎯 Plan de Solución (3 FASES)

```
FASE 1: ESPECIALIDADES (5 días)
┌──────────────────────────────────┐
│ Riesgo: 🟢 BAJO                  │
│ Impacto: 🟡 MEDIO                │
│ Cambio: ADITIVO (no rompe nada)  │
├──────────────────────────────────┤
│ 1. Migration: Agregar columna     │
│ 2. Entity: Property specialties   │
│ 3. DTOs: Incluir en request/resp  │
│ 4. Handlers: Mapear datos         │
│ 5. Tests: Validar roundtrip       │
│ 6. QA: Registro + Edit            │
│                                   │
│ ✅ Resultado: Datos persisten     │
└──────────────────────────────────┘
              ↓
FASE 2: UBICACIÓN EXPANDIDA (7 días)
┌──────────────────────────────────┐
│ Riesgo: 🟠 MEDIO                 │
│ Impacto: 🟠 ALTO                 │
│ Cambio: ESTRUCTURAL (breaking)   │
├──────────────────────────────────┤
│ 1. DTOs: location → city/state    │
│ 2. Handlers: Mapeo 4 campos       │
│ 3. Frontend: 4 inputs en UI       │
│ 4. Validations: Actualizar        │
│ 5. Tests: API + Components        │
│ 6. QA: Búsqueda + Filtros         │
│                                   │
│ ✅ Resultado: Ubicación completa  │
└──────────────────────────────────┘
              ↓
FASE 3: REMOVER PHONE DUPLICADO (3 días)
┌──────────────────────────────────┐
│ Riesgo: 🔴 ALTO                  │
│ Impacto: 🟡 MEDIO                │
│ Cambio: VALIDACIÓN (necesario)   │
├──────────────────────────────────┤
│ 1. Frontend: Remover input Step 2 │
│ 2. Handlers: Validar source único │
│ 3. Tests: Manual QA               │
│ 4. Docs: Clarificar flujo         │
│                                   │
│ ✅ Resultado: Sin confusión       │
└──────────────────────────────────┘

TOTAL: 2-3 SPRINTS
```

---

## 📈 Timeline Recomendado

```
Semana 1-2: FASE 1 (Especialidades)
├─ Lunes: Planning + Setup
├─ Martes-Jueves: Implementation
├─ Viernes: Testing + QA
├─ Fin Semana: Monitoring
└─ ✅ DEPLOY

Semana 3-4: FASE 2 (Ubicación)
├─ Lunes: Planning + Setup
├─ Martes-Jueves: Implementation
├─ Viernes: Testing + QA
├─ Fin Semana: Monitoring
└─ ✅ DEPLOY

Semana 5: FASE 3 (Phone)
├─ Lunes-Martes: Implementation
├─ Miércoles-Jueves: Testing + QA
├─ Viernes: Monitoring
└─ ✅ DEPLOY

Semana 6: Buffer + Issues
```

---

## ✅ Resultados Esperados

```
┌─────────────────────────────────────┐
│ DESPUÉS DE IMPLEMENTACIÓN           │
├─────────────────────────────────────┤
│                                     │
│ ✅ Especialidades se guardan        │
│ ✅ Ubicación completa en BD         │
│ ✅ Sin teléfono duplicado           │
│ ✅ Datos consistentes (UI ↔ BD)     │
│ ✅ Tests 100% passing               │
│ ✅ 0 data loss                      │
│ ✅ Performance OK                   │
│ ✅ Users happy                      │
│                                     │
└─────────────────────────────────────┘
```

---

## 🚨 Riesgos Principales

```
┌────────────────────────────┐
│ RIESGO #1: Datos Huérfanos│ 🔴
├────────────────────────────┤
│ Especialidades actuales    │
│ se pueden perder           │
│                            │
│ Solución:                  │
│ • Backup antes             │
│ • Migration script         │
│ • Notificar usuarios       │
└────────────────────────────┘

┌────────────────────────────┐
│ RIESGO #2: API Mismatch   │ 🔴
├────────────────────────────┤
│ Frontend vs Backend        │
│ desincronizados            │
│                            │
│ Solución:                  │
│ • Changes simultáneos      │
│ • Integration tests        │
│ • Type safety              │
└────────────────────────────┘

┌────────────────────────────┐
│ RIESGO #3: Event Sync     │ 🟠
├────────────────────────────┤
│ RabbitMQ consumers         │
│ reciben datos incompletos  │
│                            │
│ Solución:                  │
│ • Versioning de eventos    │
│ • Defensive code           │
│ • Tests de integración     │
└────────────────────────────┘
```

---

## 🎓 Key Takeaways

```
┌───────────────────────────────────────┐
│ PARA EL EQUIPO                        │
├───────────────────────────────────────┤
│                                       │
│ 1. Frontend types ↔ Backend DTOs      │
│    DEBEN estar sincronizados          │
│    NO es opcional                     │
│                                       │
│ 2. No duplicar campos en UI           │
│    (phone en 2 steps = confusión)     │
│                                       │
│ 3. RabbitMQ events son críticos       │
│    Cambios deben ser versionados      │
│                                       │
│ 4. Database migrations                │
│    SIEMPRE plan rollback              │
│                                       │
│ 5. Tests, tests, tests                │
│    Unit + Integration + Manual        │
│                                       │
└───────────────────────────────────────┘
```

---

## 📋 Next Steps

```
☐ 1. REVIEW: Equipo lee documentación
   └─ Tiempo: 2 horas

☐ 2. MEETING: Discutir plan
   └─ Tiempo: 1 hora

☐ 3. APROBACIÓN: Stakeholders aprueban
   └─ Tiempo: 24 horas

☐ 4. PREPARACIÓN: Backups, scripts
   └─ Tiempo: 1 día

☐ 5. IMPLEMENTACIÓN: FASE 1
   └─ Tiempo: 5 días

☐ 6. MONITOREO: 24 horas post-deploy
   └─ Tiempo: ongoing

☐ 7. DOCUMENTACIÓN: Actualizar docs
   └─ Tiempo: 1 día
```

---

## 🏁 Conclusión

✅ **Problemas identificados claramente**  
✅ **Riesgos documentados**  
✅ **Plan de solución detallado**  
✅ **Listo para aprobación y ejecución**

👉 **Próximo paso:** Aprobación del plan para empezar FASE 1

---

**Presentación Visual Completada**  
**Documentación Técnica Disponible en Archivos Markdown**  
**Listo para Team Review**

# 📋 Registro de Evidencias y Controles

> **Propósito:** Documentar todas las evidencias que demuestran cumplimiento regulatorio  
> **Uso:** Presentar ante auditores y reguladores como prueba de cumplimiento  
> **Última actualización:** Enero 25, 2026

---

## 📊 RESUMEN DE EVIDENCIAS

### Por Regulador

| Regulador      | Total Evidencias | Disponibles  | Parciales    | Faltantes    |
| -------------- | ---------------- | ------------ | ------------ | ------------ |
| DGII           | 25               | 5            | 8            | 12           |
| UAF            | 20               | 0            | 3            | 17           |
| Ley 172-13     | 15               | 4            | 5            | 6            |
| Pro Consumidor | 12               | 3            | 3            | 6            |
| **TOTAL**      | **72**           | **12 (17%)** | **19 (26%)** | **41 (57%)** |

---

## 🗂️ CATÁLOGO DE EVIDENCIAS

### 1. EVIDENCIAS DGII

#### 1.1 Registro Fiscal

| ID        | Evidencia         | Descripción                     | Estado | Ubicación      | Frecuencia    |
| --------- | ----------------- | ------------------------------- | ------ | -------------- | ------------- |
| DGII-E001 | Certificado RNC   | Registro Nacional Contribuyente | ⚠️     | Archivo físico | Única         |
| DGII-E002 | Autorización NCF  | Secuencias autorizadas          | ✅     | DGII OV        | Por solicitud |
| DGII-E003 | e-CF Habilitación | Certificado e-factura           | 🔴     | Pendiente      | Única         |

#### 1.2 Declaraciones y Pagos

| ID        | Evidencia           | Descripción                   | Estado | Ubicación | Frecuencia |
| --------- | ------------------- | ----------------------------- | ------ | --------- | ---------- |
| DGII-E004 | IT-1 Declaraciones  | Declaraciones ITBIS           | 🔴     | DGII OV   | Mensual    |
| DGII-E005 | IT-1 Recibos Pago   | Comprobantes de pago ITBIS    | 🔴     | DGII OV   | Mensual    |
| DGII-E006 | IR-17 Declaraciones | Retenciones                   | 🔴     | DGII OV   | Mensual    |
| DGII-E007 | IR-17 Recibos Pago  | Comprobantes pago retenciones | 🔴     | DGII OV   | Mensual    |
| DGII-E008 | IR-2 Declaración    | ISR Anual                     | 🔴     | DGII OV   | Anual      |

#### 1.3 Formatos Informativos

| ID        | Evidencia   | Descripción          | Estado | Ubicación | Frecuencia |
| --------- | ----------- | -------------------- | ------ | --------- | ---------- |
| DGII-E009 | Formato 606 | Archivo + Acuse DGII | 🔴     | S3/DGII   | Mensual    |
| DGII-E010 | Formato 607 | Archivo + Acuse DGII | 🔴     | S3/DGII   | Mensual    |
| DGII-E011 | Formato 608 | Archivo + Acuse DGII | 🔴     | S3/DGII   | Mensual    |

#### 1.4 Libros y Registros

| ID        | Evidencia         | Descripción                  | Estado | Ubicación     | Frecuencia      |
| --------- | ----------------- | ---------------------------- | ------ | ------------- | --------------- |
| DGII-E012 | Libro de Ventas   | Registro de todas las ventas | ✅     | BD PostgreSQL | Continuo        |
| DGII-E013 | Libro de Compras  | Registro de gastos           | 🟡     | BD PostgreSQL | Continuo        |
| DGII-E014 | Secuencias NCF    | Control de numeración        | ✅     | BD PostgreSQL | Continuo        |
| DGII-E015 | Facturas Emitidas | Copias de facturas           | ✅     | BD + S3       | Por transacción |

---

### 2. EVIDENCIAS UAF

#### 2.1 Estructura de Cumplimiento

| ID       | Evidencia                | Descripción                       | Estado | Ubicación | Frecuencia |
| -------- | ------------------------ | --------------------------------- | ------ | --------- | ---------- |
| UAF-E001 | Registro Sujeto Obligado | Constancia de registro UAF        | 🔴     | Pendiente | Única      |
| UAF-E002 | Acta Designación OC      | Oficial de Cumplimiento designado | 🔴     | Pendiente | Por cambio |
| UAF-E003 | Notificación a UAF       | Comunicación de designación       | 🔴     | Pendiente | Por cambio |
| UAF-E004 | Organigrama Cumplimiento | Estructura organizacional         | 🔴     | Pendiente | Anual      |

#### 2.2 Políticas y Manuales

| ID       | Evidencia               | Descripción              | Estado | Ubicación | Frecuencia     |
| -------- | ----------------------- | ------------------------ | ------ | --------- | -------------- |
| UAF-E005 | Manual Prevención LA/FT | Documento de políticas   | 🔴     | Pendiente | Anual revisión |
| UAF-E006 | Política KYC            | Conocimiento del cliente | 🟡     | Código    | Anual revisión |
| UAF-E007 | Matriz de Riesgo        | Evaluación de riesgos    | 🔴     | Pendiente | Anual          |
| UAF-E008 | Código de Conducta      | Ética y cumplimiento     | 🔴     | Pendiente | Anual revisión |

#### 2.3 Debida Diligencia

| ID       | Evidencia                 | Descripción                  | Estado | Ubicación | Frecuencia  |
| -------- | ------------------------- | ---------------------------- | ------ | --------- | ----------- |
| UAF-E009 | Expedientes KYC           | Documentos de clientes       | 🟡     | BD + S3   | Por cliente |
| UAF-E010 | Verificaciones Realizadas | Log de verificaciones        | 🟡     | BD        | Por cliente |
| UAF-E011 | Lista de Alto Riesgo      | Clientes clasificados        | 🔴     | Pendiente | Continuo    |
| UAF-E012 | Revisiones Periódicas     | Actualización de expedientes | 🔴     | Pendiente | Anual       |

#### 2.4 Monitoreo y Reportes

| ID       | Evidencia           | Descripción                | Estado | Ubicación    | Frecuencia |
| -------- | ------------------- | -------------------------- | ------ | ------------ | ---------- |
| UAF-E013 | Alertas Generadas   | Log de alertas del sistema | 🔴     | Pendiente    | Continuo   |
| UAF-E014 | Análisis de Alertas | Resolución de alertas      | 🔴     | Pendiente    | Por alerta |
| UAF-E015 | ROS Enviados        | Reportes a UAF             | 🔴     | Confidencial | Por evento |
| UAF-E016 | Acuses ROS          | Confirmación de recepción  | 🔴     | Confidencial | Por evento |

#### 2.5 Capacitación

| ID       | Evidencia            | Descripción               | Estado | Ubicación | Frecuencia |
| -------- | -------------------- | ------------------------- | ------ | --------- | ---------- |
| UAF-E017 | Plan de Capacitación | Programa anual            | 🔴     | Pendiente | Anual      |
| UAF-E018 | Listas de Asistencia | Registro de participantes | 🔴     | Pendiente | Por sesión |
| UAF-E019 | Materiales de Curso  | Presentaciones, guías     | 🔴     | Pendiente | Por sesión |
| UAF-E020 | Evaluaciones         | Exámenes y resultados     | 🔴     | Pendiente | Por sesión |

---

### 3. EVIDENCIAS PROTECCIÓN DE DATOS

#### 3.1 Información y Consentimiento

| ID      | Evidencia                   | Descripción            | Estado | Ubicación | Frecuencia     |
| ------- | --------------------------- | ---------------------- | ------ | --------- | -------------- |
| PD-E001 | Política de Privacidad      | Documento publicado    | ✅     | /privacy  | Anual revisión |
| PD-E002 | Términos y Condiciones      | Con cláusulas de datos | ✅     | /terms    | Anual revisión |
| PD-E003 | Registros de Consentimiento | Log de aceptaciones    | 🟡     | BD        | Por usuario    |
| PD-E004 | Aviso de Cookies            | Banner y preferencias  | 🔴     | Pendiente | Continuo       |

#### 3.2 Derechos ARCO

| ID      | Evidencia           | Descripción                | Estado | Ubicación | Frecuencia    |
| ------- | ------------------- | -------------------------- | ------ | --------- | ------------- |
| PD-E005 | Solicitudes ARCO    | Registro de solicitudes    | 🔴     | Pendiente | Por solicitud |
| PD-E006 | Respuestas ARCO     | Comunicaciones a titulares | 🔴     | Pendiente | Por solicitud |
| PD-E007 | Tiempo de Respuesta | Métricas de cumplimiento   | 🔴     | Pendiente | Mensual       |

#### 3.3 Seguridad

| ID      | Evidencia            | Descripción                | Estado         | Ubicación     | Frecuencia   |
| ------- | -------------------- | -------------------------- | -------------- | ------------- | ------------ |
| PD-E008 | Medidas de Seguridad | Documentación técnica      | 🟡             | Docs técnicos | Anual        |
| PD-E009 | Logs de Acceso       | Registro de acceso a datos | 🟡             | Elasticsearch | Continuo     |
| PD-E010 | Registro de Brechas  | Incidentes de seguridad    | ✅ (0 brechas) | N/A           | Por evento   |
| PD-E011 | Contratos Encargados | Acuerdos con terceros      | 🔴             | Pendiente     | Por contrato |

---

### 4. EVIDENCIAS PRO CONSUMIDOR

#### 4.1 Información al Consumidor

| ID      | Evidencia               | Descripción              | Estado | Ubicación   | Frecuencia |
| ------- | ----------------------- | ------------------------ | ------ | ----------- | ---------- |
| PC-E001 | Identificación Visible  | RNC, dirección, contacto | 🟡     | Footer web  | Continuo   |
| PC-E002 | Precios Claros          | Sin cargos ocultos       | ✅     | UI Checkout | Continuo   |
| PC-E003 | Información de Retracto | Derecho de 7 días        | 🔴     | Pendiente   | Continuo   |

#### 4.2 Quejas y Reclamaciones

| ID      | Evidencia              | Descripción             | Estado | Ubicación | Frecuencia |
| ------- | ---------------------- | ----------------------- | ------ | --------- | ---------- |
| PC-E004 | Libro de Reclamaciones | Físico y/o digital      | 🔴     | Pendiente | Continuo   |
| PC-E005 | Registro de Quejas     | Base de datos de quejas | 🔴     | Pendiente | Por queja  |
| PC-E006 | Respuestas a Quejas    | Comunicaciones enviadas | 🔴     | Pendiente | Por queja  |
| PC-E007 | Métricas de Atención   | Tiempos de resolución   | 🔴     | Pendiente | Mensual    |

#### 4.3 Devoluciones

| ID      | Evidencia               | Descripción                | Estado | Ubicación      | Frecuencia    |
| ------- | ----------------------- | -------------------------- | ------ | -------------- | ------------- |
| PC-E008 | Solicitudes de Retracto | Registro de solicitudes    | 🔴     | Pendiente      | Por solicitud |
| PC-E009 | Reembolsos Procesados   | Comprobantes de devolución | 🟡     | BillingService | Por reembolso |
| PC-E010 | Notas de Crédito (B04)  | NCF de devolución          | 🔴     | Pendiente      | Por reembolso |

---

## 📁 CONTROLES IMPLEMENTADOS

### Controles Técnicos

| ID     | Control           | Descripción                  | Estado | Evidencia          |
| ------ | ----------------- | ---------------------------- | ------ | ------------------ |
| CT-001 | Autenticación     | Login seguro con JWT         | ✅     | Código AuthService |
| CT-002 | Cifrado HTTPS     | TLS 1.3 en tránsito          | ✅     | Certificado SSL    |
| CT-003 | Cifrado en Reposo | AES-256 para datos sensibles | 🟡     | Parcial            |
| CT-004 | Control de Acceso | RBAC por roles               | ✅     | RoleService        |
| CT-005 | Logs de Auditoría | Registro de acciones         | 🟡     | Elasticsearch      |
| CT-006 | Backups           | Copias de seguridad          | ✅     | AWS Backup         |
| CT-007 | Firewall          | WAF y reglas de red          | ⚠️     | Verificar          |
| CT-008 | 2FA               | Autenticación dos factores   | 🟡     | Disponible         |

### Controles Organizativos

| ID     | Control                     | Descripción               | Estado | Evidencia |
| ------ | --------------------------- | ------------------------- | ------ | --------- |
| CO-001 | Política de Privacidad      | Publicada y actualizada   | ✅     | /privacy  |
| CO-002 | Términos de Servicio        | Publicados y actualizados | ✅     | /terms    |
| CO-003 | Acuerdos Confidencialidad   | Con empleados             | 🔴     | Pendiente |
| CO-004 | Capacitación Seguridad      | Para personal             | 🔴     | Pendiente |
| CO-005 | Oficial de Cumplimiento     | Designado                 | 🔴     | Pendiente |
| CO-006 | Procedimiento de Incidentes | Documentado               | 🔴     | Pendiente |
| CO-007 | Revisión de Proveedores     | Evaluación de terceros    | 🔴     | Pendiente |
| CO-008 | Auditorías Internas         | Programa de auditoría     | 🔴     | Pendiente |

### Controles de Proceso

| ID     | Control              | Descripción                 | Estado | Evidencia      |
| ------ | -------------------- | --------------------------- | ------ | -------------- |
| CP-001 | Verificación KYC     | Proceso de identificación   | 🟡     | UserService    |
| CP-002 | Moderación Contenido | Revisión de publicaciones   | ✅     | Workflow       |
| CP-003 | Aprobación Pagos     | Validación de transacciones | ✅     | BillingService |
| CP-004 | Gestión de Quejas    | Proceso de atención         | 🔴     | Pendiente      |
| CP-005 | Generación NCF       | Comprobantes fiscales       | ✅     | BillingService |
| CP-006 | Retención Documentos | Conservación 10 años        | 🟡     | S3 + Glacier   |

---

## 📊 MATRIZ DE CONTROLES POR RIESGO

### Riesgos Identificados vs Controles

| Riesgo               | Impacto | Probabilidad | Controles              | Estado |
| -------------------- | ------- | ------------ | ---------------------- | ------ |
| Fraude Fiscal        | Alto    | Medio        | CT-005, CP-005         | 🟡     |
| Lavado de Activos    | Crítico | Medio        | CP-001, CO-005         | 🔴     |
| Brecha de Datos      | Alto    | Bajo         | CT-002, CT-003, CO-006 | 🟡     |
| Incumplimiento DGII  | Alto    | Alto         | CP-005, DGII-E009-11   | 🔴     |
| Quejas Sin Atender   | Medio   | Alto         | CP-004, CO-004         | 🔴     |
| Acceso No Autorizado | Alto    | Bajo         | CT-001, CT-004, CT-008 | ✅     |

---

## 🔍 PROCEDIMIENTO DE AUDITORÍA

### Preparación de Evidencias

```
┌─────────────────────────────────────────────────────────────────────────┐
│  PROCEDIMIENTO: PREPARAR EVIDENCIAS PARA AUDITORÍA                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1. RECIBIR NOTIFICACIÓN DE AUDITORÍA                                   │
│     ├── Identificar regulador (DGII, UAF, etc.)                        │
│     ├── Identificar alcance y período                                  │
│     └── Asignar responsable interno                                    │
│                                                                         │
│  2. RECOPILAR EVIDENCIAS (3-5 días antes)                               │
│     ├── Consultar catálogo de evidencias aplicables                    │
│     ├── Exportar datos de sistemas                                     │
│     ├── Generar reportes requeridos                                    │
│     ├── Obtener documentos físicos                                     │
│     └── Verificar completitud                                          │
│                                                                         │
│  3. ORGANIZAR DOCUMENTACIÓN                                             │
│     ├── Crear carpeta por área de auditoría                            │
│     ├── Nombrar archivos descriptivamente                              │
│     ├── Crear índice de documentos                                     │
│     └── Preparar copias (no entregar originales)                       │
│                                                                         │
│  4. REVISIÓN PREVIA                                                     │
│     ├── Verificar que evidencias son suficientes                       │
│     ├── Identificar gaps y preparar explicaciones                      │
│     ├── Revisar con asesor legal/fiscal si necesario                   │
│     └── Preparar a personal que atenderá                               │
│                                                                         │
│  5. DURANTE LA AUDITORÍA                                                │
│     ├── Proporcionar acceso controlado                                 │
│     ├── Documentar toda solicitud                                      │
│     ├── Mantener registro de documentos entregados                     │
│     └── Tomar nota de observaciones                                    │
│                                                                         │
│  6. POST-AUDITORÍA                                                      │
│     ├── Solicitar informe de hallazgos                                 │
│     ├── Preparar plan de acción                                        │
│     ├── Implementar correcciones                                       │
│     └── Documentar lecciones aprendidas                                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Checklist Pre-Auditoría

```
DOCUMENTOS CORPORATIVOS
□ Acta Constitutiva (copia certificada)
□ Certificado RNC
□ Registro Mercantil actualizado
□ Patente Municipal vigente
□ Poderes de representación

DOCUMENTOS FISCALES (por período)
□ Declaraciones IT-1 con acuse
□ Comprobantes de pago ITBIS
□ Declaraciones IR-17 con acuse
□ Comprobantes de pago retenciones
□ Formatos 606/607/608 con acuse
□ Control de secuencias NCF

DOCUMENTOS AML (si aplica)
□ Registro UAF
□ Manual de Prevención
□ Expedientes KYC de alto riesgo
□ Registro de capacitaciones
□ Copia de ROS enviados (confidencial)

DOCUMENTOS DATOS PERSONALES
□ Política de privacidad vigente
□ Registro de consentimientos
□ Registro de solicitudes ARCO
□ Contratos con encargados

DOCUMENTOS CONSUMIDOR
□ Libro de reclamaciones
□ Registro de quejas
□ Registro de devoluciones
```

---

## 💾 ALMACENAMIENTO DE EVIDENCIAS

### Estructura de Archivo

```
/evidencias/
├── 2026/
│   ├── DGII/
│   │   ├── 01-Enero/
│   │   │   ├── 606-202601.txt
│   │   │   ├── 606-202601-acuse.pdf
│   │   │   ├── 607-202601.txt
│   │   │   ├── 607-202601-acuse.pdf
│   │   │   ├── IT1-202601.pdf
│   │   │   └── IT1-202601-pago.pdf
│   │   └── ...
│   ├── UAF/
│   │   ├── registro/
│   │   ├── manual/
│   │   ├── capacitaciones/
│   │   └── ros/  (confidencial)
│   ├── Datos_Personales/
│   │   ├── politicas/
│   │   ├── consentimientos/
│   │   └── arco/
│   └── ProConsumidor/
│       ├── quejas/
│       └── devoluciones/
└── corporativos/
    ├── acta_constitutiva.pdf
    ├── rnc_certificado.pdf
    └── ...
```

### Política de Retención

| Tipo de Evidencia   | Retención Mínima      | Ubicación     |
| ------------------- | --------------------- | ------------- |
| Documentos Fiscales | 10 años               | S3 Glacier    |
| Documentos AML      | 10 años               | S3 Glacier    |
| Expedientes KYC     | 10 años post-relación | S3 Glacier    |
| Logs de Sistema     | 2 años                | Elasticsearch |
| Consentimientos     | Vigencia + 5 años     | S3 Standard   |
| Quejas              | 2 años                | S3 Standard   |
| Corporativos        | Permanente            | S3 Standard   |

---

## 🔗 INTEGRACIÓN CON AuditService

### Endpoints Propuestos (Puerto 5070)

| Endpoint                                     | Descripción                 |
| -------------------------------------------- | --------------------------- |
| `GET /api/audit/evidences`                   | Listar todas las evidencias |
| `GET /api/audit/evidences/{type}`            | Evidencias por tipo         |
| `GET /api/audit/evidences/{id}/download`     | Descargar evidencia         |
| `POST /api/audit/evidences`                  | Registrar nueva evidencia   |
| `GET /api/audit/controls`                    | Listar controles            |
| `GET /api/audit/controls/{id}/effectiveness` | Evaluar control             |
| `POST /api/audit/prepare/{regulador}`        | Preparar para auditoría     |
| `GET /api/audit/gaps`                        | Identificar brechas         |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Mensual  
**Responsable:** Oficial de Cumplimiento (pendiente designar)

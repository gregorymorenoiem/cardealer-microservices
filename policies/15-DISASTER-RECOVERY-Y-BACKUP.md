# POLÍTICA 15: DISASTER RECOVERY Y BACKUP

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los sistemas críticos deben tener estrategias de Backup y Disaster Recovery (DR) probadas y documentadas. Se establecen objetivos de RPO (Recovery Point Objective) de 15 minutos y RTO (Recovery Time Objective) de 4 horas para servicios Tier 1.

**Objetivo**: Garantizar la continuidad del negocio ante fallos catastróficos, pérdida de datos, ciberataques o desastres naturales, minimizando el tiempo de inactividad y la pérdida de información.

**Alcance**: Aplica a TODOS los servicios, bases de datos e infraestructura del ecosistema CarDealer.

---

## 🎯 OBJETIVOS DE RECUPERACIÓN (RPO/RTO)

### Clasificación de Servicios

| Tier | Descripción | Servicios | RPO (Datos) | RTO (Tiempo) | Backup Freq |
|------|-------------|-----------|-------------|--------------|-------------|
| **Tier 1** | Críticos para el negocio | Auth, Gateway, ErrorService | 15 min | 4 horas | Cada 15 min (Log) / Diario (Full) |
| **Tier 2** | Importantes | Notification, Audit | 1 hora | 8 horas | Cada 1 hora (Log) / Diario (Full) |
| **Tier 3** | Soporte / Internos | Admin, Reporting | 24 horas | 24 horas | Diario (Full) |
| **Tier 4** | No críticos | Dev/Test envs | Best effort | Best effort | Semanal |

**Definiciones**:
- **RPO (Recovery Point Objective)**: Máxima cantidad de datos que se pueden perder (medido en tiempo).
- **RTO (Recovery Time Objective)**: Máximo tiempo permitido para restaurar el servicio.

---

## 💾 ESTRATEGIA DE BACKUP

### 1. Base de Datos (PostgreSQL)

#### Full Backup (Diario)
```bash
# Script: backup-full.sh
#!/bin/bash
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backups/postgres/full"
CONTAINER_NAME="postgres-db"
DB_USER="postgres"

# Crear backup
docker exec -t $CONTAINER_NAME pg_dumpall -c -U $DB_USER > $BACKUP_DIR/full_backup_$TIMESTAMP.sql

# Comprimir
gzip $BACKUP_DIR/full_backup_$TIMESTAMP.sql

# Subir a Azure Blob Storage (Off-site)
az storage blob upload \
  --container-name db-backups \
  --file $BACKUP_DIR/full_backup_$TIMESTAMP.sql.gz \
  --name full/full_backup_$TIMESTAMP.sql.gz \
  --account-name cardealerbackups

# Retención: 30 días
find $BACKUP_DIR -name "*.gz" -mtime +30 -delete
```

#### WAL Archiving (Continuo - Point-in-Time Recovery)
```bash
# postgresql.conf
wal_level = replica
archive_mode = on
archive_command = 'test ! -f /mnt/server/archivedir/%f && cp %p /mnt/server/archivedir/%f'
archive_timeout = 900  # 15 minutos (RPO Tier 1)
```

---

### 2. Volúmenes Persistentes (Kubernetes)

#### Velero Backup
```bash
# Instalar Velero CLI
velero install \
    --provider azure \
    --plugins velero/velero-plugin-for-microsoft-azure:v1.5.0 \
    --bucket velero \
    --secret-file ./credentials-velero \
    --use-volume-snapshots=true

# Crear Schedule Diario
velero schedule create daily-full-backup \
    --schedule="0 1 * * *" \
    --include-namespaces cardealer \
    --ttl 720h  # 30 días

# Backup Manual (antes de cambios grandes)
velero backup create pre-upgrade-backup \
    --include-namespaces cardealer \
    --wait
```

---

### 3. Configuración e Infraestructura (IaC)

- **Repositorio Git**: Todo el código de infraestructura (Terraform, K8s manifests) está versionado.
- **Secrets**: Backups de Azure Key Vault o Sealed Secrets master key.

```bash
# Backup de Sealed Secrets Master Key
kubectl get secret -n kube-system -l sealedsecrets.bitnami.com/sealed-secrets-key -o yaml > master-key-backup.yaml
# Guardar en lugar SEGURO y OFFLINE (ej. caja fuerte física o password manager corporativo)
```

---

## 🔄 DISASTER RECOVERY PLAN (DRP)

### Escenario 1: Fallo de Base de Datos

**Síntomas**: Errores de conexión a DB, corrupción de datos.
**Acción**: Restaurar desde último backup válido.

1. **Detener servicios afectados**:
   ```bash
   kubectl scale deployment errorservice --replicas=0 -n cardealer
   ```

2. **Restaurar DB**:
   ```bash
   # Bajar último backup
   az storage blob download ... --file backup.sql.gz
   gunzip backup.sql.gz
   
   # Restaurar
   cat backup.sql | docker exec -i postgres-db psql -U postgres
   ```

3. **Verificar datos**:
   ```sql
   SELECT count(*) FROM error_logs;
   ```

4. **Reiniciar servicios**:
   ```bash
   kubectl scale deployment errorservice --replicas=3 -n cardealer
   ```

---

### Escenario 2: Fallo de Región (Azure)

**Síntomas**: Región completa caída.
**Acción**: Failover a región secundaria (DR Region).

1. **Activar Infraestructura en DR Region**:
   ```bash
   # Terraform apply en región secundaria
   cd terraform/environments/dr
   terraform apply -auto-approve
   ```

2. **Restaurar Datos**:
   - Geo-Redundant Storage (GRS) replica backups automáticamente.
   - Restaurar DBs desde backups en región DR.

3. **Actualizar DNS**:
   - Cambiar Traffic Manager / Azure Front Door para apuntar a región DR.

4. **Validación**:
   - Ejecutar Smoke Tests en región DR.

---

### Escenario 3: Borrado Accidental / Corrupción Lógica

**Síntomas**: Datos faltantes o incorrectos por error humano/bug.
**Acción**: Point-in-Time Recovery (PITR).

1. **Identificar momento del error**:
   - Revisar logs para encontrar timestamp exacto (ej. 14:35:00).

2. **Restaurar PITR**:
   ```bash
   # En servidor de recuperación
   # 1. Restaurar base backup anterior al error
   # 2. Configurar recovery.conf
   restore_command = 'cp /mnt/server/archivedir/%f %p'
   recovery_target_time = '2025-11-30 14:34:59'
   
   # 3. Iniciar Postgres (aplicará WALs hasta el target time)
   ```

3. **Exportar datos recuperados e importar en prod**:
   - O promover servidor de recuperación a maestro.

---

## 🛡️ HIGH AVAILABILITY (HA)

### Estrategias de HA

1. **Base de Datos**:
   - **Primary-Replica**: 1 Master (Writes), 2+ Replicas (Reads).
   - **Failover Automático**: Usando herramientas como Patroni o Azure Database for PostgreSQL (Hyperscale).

2. **Kubernetes**:
   - **Multi-Zone Cluster**: Nodos distribuidos en 3 Availability Zones (AZs).
   - **Pod Anti-Affinity**: Evitar que pods del mismo servicio corran en el mismo nodo.
   - **Min Replicas**: Mínimo 3 réplicas por servicio crítico.

3. **Messaging (RabbitMQ)**:
   - **Clustered Mode**: 3 nodos.
   - **Quorum Queues**: Alta disponibilidad y consistencia de datos.

---

## 🧪 TESTING Y SIMULACROS (DRILLS)

### Frecuencia de Pruebas

| Tipo de Prueba | Frecuencia | Descripción |
|----------------|------------|-------------|
| **Backup Restore Test** | Mensual | Restaurar backup aleatorio en ambiente de test y verificar integridad. |
| **HA Failover Test** | Trimestral | Forzar caída de nodo/pod/db master y verificar failover automático. |
| **Full DR Drill** | Anual | Simular pérdida total de región y ejecutar plan de recuperación completo. |

### Procedimiento de Backup Restore Test

1. Seleccionar backup aleatorio de la última semana.
2. Restaurar en ambiente aislado (Sandbox).
3. Ejecutar script de validación de integridad (Checksums, Row counts).
4. Documentar resultados y tiempo tomado (para validar RTO).

---

## ✅ CHECKLIST DE DISASTER RECOVERY

### Preventivo (Diario/Semanal)
- [ ] Backups ejecutándose correctamente (verificar logs).
- [ ] Backups copiados off-site (Azure Blob Storage).
- [ ] Monitoreo de espacio en disco para backups.
- [ ] Alertas de fallo de backup configuradas.

### Durante Incidente
- [ ] Declarar incidente y notificar stakeholders.
- [ ] Evaluar alcance (¿Qué servicios/datos están afectados?).
- [ ] Seleccionar estrategia de recuperación (Restore vs Failover).
- [ ] Ejecutar plan de recuperación.
- [ ] Comunicar estimación de tiempo (ETA) a usuarios.

### Post-Incidente
- [ ] Verificar integridad de datos y funcionalidad de servicios.
- [ ] Realizar Post-Mortem (RCA - Root Cause Analysis).
- [ ] Actualizar DRP con lecciones aprendidas.
- [ ] Reportar cumplimiento de SLAs (RPO/RTO reales).

---

## 🚫 ANTI-PATRONES DE BACKUP

### ❌ PROHIBIDO

- **Backups en el mismo disco**: Si el disco falla, se pierde todo.
- **No probar los restores**: "Un backup no probado es un backup inexistente".
- **Backups manuales**: Depender de humanos para tareas repetitivas críticas.
- **Guardar secretos en backups sin cifrar**: Riesgo de seguridad masivo.
- **Ignorar alertas de fallo de backup**: Asumir que "mañana funcionará".

### ✅ CORRECTO

- **Regla 3-2-1**: 3 copias de datos, 2 medios diferentes, 1 copia off-site.
- **Automatización total**: Scripts y schedules gestionados por sistema.
- **Encriptación**: Backups encriptados en reposo y tránsito.
- **Documentación actualizada**: El DRP debe ser ejecutable por cualquier ingeniero senior, no solo el "experto".

---

## 📚 RECURSOS Y REFERENCIAS

- **Velero Docs**: [https://velero.io/docs/](https://velero.io/docs/)
- **PostgreSQL Backup**: [https://www.postgresql.org/docs/current/backup.html](https://www.postgresql.org/docs/current/backup.html)
- **Azure Disaster Recovery**: [https://docs.microsoft.com/en-us/azure/architecture/resiliency/disaster-recovery-azure-applications](https://docs.microsoft.com/en-us/azure/architecture/resiliency/disaster-recovery-azure-applications)
- **Site Reliability Engineering (Google)**: Capítulos sobre Data Integrity y Emergency Response.

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Semestral

**NOTA**: La pérdida de datos por falta de backup o imposibilidad de restaurar es inaceptable y se considera un fallo crítico de arquitectura.

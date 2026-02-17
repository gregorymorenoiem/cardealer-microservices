# Tutorial 8: PostgreSQL StatefulSet

**Duración:** 55 minutos  
**Nivel:** Avanzado  
**Requisitos:** Tutorial 7 completado

---

## 📋 Resumen

Aprenderás a desplegar bases de datos en Kubernetes usando StatefulSets, configurar PersistentVolumes para almacenamiento, y gestionar backups. PostgreSQL será nuestro ejemplo principal.

### Lo que aprenderás:
- Diferencia entre Deployment y StatefulSet
- Configurar PersistentVolumeClaims (PVC)
- Desplegar PostgreSQL en Kubernetes
- Conectar microservicios a la base de datos
- Backups y restauración
- Gestión de datos persistentes

---

## 📑 Índice

1. [Conceptos: StatefulSet vs Deployment](#1-conceptos-statefulset-vs-deployment)
2. [PersistentVolumes y PersistentVolumeClaims](#2-persistentvolumes-y-persistentvolumeclaims)
3. [Desplegar PostgreSQL](#3-desplegar-postgresql)
4. [Verificar almacenamiento persistente](#4-verificar-almacenamiento-persistente)
5. [Conectar desde microservicios](#5-conectar-desde-microservicios)
6. [Gestión de bases de datos](#6-gestión-de-bases-de-datos)
7. [Backups y restauración](#7-backups-y-restauración)
8. [Escalado de StatefulSets](#8-escalado-de-statefulsets)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Conceptos: StatefulSet vs Deployment

### ¿Por qué no usar Deployment para bases de datos?

Los **Deployments** son para aplicaciones **stateless** (sin estado):
- No importa qué pod maneja un request
- Los pods son intercambiables
- Pueden escalarse y reiniciarse libremente

Las **bases de datos** son **stateful** (con estado):
- Cada instancia tiene datos únicos
- El orden de inicio/stop importa
- Necesitan storage persistente

### 🔹 Deployment (stateless)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
spec:
  replicas: 3  # 3 pods idénticos
  template:
    spec:
      containers:
      - name: gateway
        image: gateway:latest
```

**Características:**
- Pods con nombres aleatorios: `gateway-abc123`, `gateway-def456`
- Sin garantía de orden de inicio
- Sin storage persistente por defecto
- Perfecto para APIs, servicios web

### 🔹 StatefulSet (stateful)

```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
spec:
  replicas: 1
  serviceName: postgres  # Service headless
  template:
    spec:
      containers:
      - name: postgres
        image: postgres:16
  volumeClaimTemplates:
  - metadata:
      name: postgres-data
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 10Gi
```

**Características:**
- Pods con nombres predecibles: `postgres-0`, `postgres-1`, `postgres-2`
- Orden de inicio: 0 → 1 → 2
- Cada pod tiene su propio PersistentVolume
- Storage persiste aunque el pod muera

### Comparación

| Aspecto | Deployment | StatefulSet |
|---------|-----------|-------------|
| **Nombres de pods** | Aleatorios (`app-abc123`) | Ordenados (`app-0`, `app-1`) |
| **Orden de inicio** | Paralelo | Secuencial |
| **Storage** | Compartido o efímero | PVC individual por pod |
| **Identidad de red** | Cambia al reiniciar | Estable (mismo DNS) |
| **Casos de uso** | APIs, microservicios | Bases de datos, Kafka, Redis |

---

## 2. PersistentVolumes y PersistentVolumeClaims

### Arquitectura de almacenamiento

```
┌─────────────────────────────────────────────────┐
│                  Pod                            │
│  ┌────────────────────────────────────────┐    │
│  │         Container                       │    │
│  │  /var/lib/postgresql/data ←─────────┐  │    │
│  └────────────────────────────────────┬──┘    │
└────────────────────────────────────────┼───────┘
                                         │
                                         │ Mount
┌────────────────────────────────────────▼───────┐
│        PersistentVolumeClaim (PVC)             │
│        Solicitud de almacenamiento             │
└────────────────────────────────────────┬───────┘
                                         │
                                         │ Bound
┌────────────────────────────────────────▼───────┐
│        PersistentVolume (PV)                   │
│        Almacenamiento físico en DO             │
│        (Digital Ocean Block Storage)           │
└────────────────────────────────────────────────┘
```

### 🔹 PersistentVolume (PV)

**Definición:** Recurso de almacenamiento físico en el cluster.

En Digital Ocean, un PV es un **Block Storage Volume** ($0.10/GB/mes).

**Ciclo de vida:** Independiente del pod. Si el pod muere, el PV persiste.

### 🔹 PersistentVolumeClaim (PVC)

**Definición:** Solicitud de almacenamiento por parte de un pod.

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: postgres-data
spec:
  accessModes:
  - ReadWriteOnce  # Solo un nodo puede montarlo
  resources:
    requests:
      storage: 10Gi  # 10 GB
```

**Estados de un PVC:**
| Estado | Significado |
|--------|-------------|
| **Pending** | Esperando que se cree el PV |
| **Bound** | PVC vinculado a un PV |
| **Lost** | PV eliminado pero PVC existe |

### Access Modes

| Modo | Descripción | Uso |
|------|-------------|-----|
| **ReadWriteOnce (RWO)** | Un nodo, lectura/escritura | Bases de datos |
| **ReadOnlyMany (ROX)** | Múltiples nodos, solo lectura | Assets estáticos |
| **ReadWriteMany (RWX)** | Múltiples nodos, lectura/escritura | Archivos compartidos (NFS) |

**Digital Ocean solo soporta ReadWriteOnce.**

### StorageClass

**StorageClass** define el tipo de almacenamiento.

```bash
kubectl get storageclass
```

**Salida en DOKS:**
```
NAME                         PROVISIONER                 RECLAIMPOLICY
do-block-storage (default)   dobs.csi.digitalocean.com   Delete
do-block-storage-retain      dobs.csi.digitalocean.com   Retain
```

**Reclaim Policy:**
- **Delete** - Elimina el PV cuando se elimina el PVC
- **Retain** - Mantiene el PV (backup manual)

---

## 3. Desplegar PostgreSQL

### StatefulSet de PostgreSQL en OKLA

Archivo: `k8s/databases.yaml`

```yaml
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: okla
spec:
  type: ClusterIP
  clusterIP: None  # Headless service
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432

---
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
  namespace: okla
spec:
  serviceName: postgres
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:16
        ports:
        - containerPort: 5432
          name: postgres
        env:
        - name: POSTGRES_USER
          value: "postgres"
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: DB_PASSWORD
        - name: PGDATA
          value: /var/lib/postgresql/data/pgdata
        volumeMounts:
        - name: postgres-data
          mountPath: /var/lib/postgresql/data
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
  volumeClaimTemplates:
  - metadata:
      name: postgres-data
    spec:
      accessModes: ["ReadWriteOnce"]
      storageClassName: do-block-storage
      resources:
        requests:
          storage: 10Gi
```

### Componentes clave

1. **Service headless** (`clusterIP: None`)
   - No tiene IP propia
   - DNS resuelve directamente a pods: `postgres-0.postgres.okla.svc.cluster.local`

2. **volumeClaimTemplates**
   - Crea un PVC por cada réplica
   - Nombre: `postgres-data-postgres-0`

3. **PGDATA**
   - Ubicación de los datos de PostgreSQL
   - Debe estar en el volume montado

### Aplicar StatefulSet

```bash
kubectl apply -f k8s/databases.yaml
```

**Salida:**
```
service/postgres created
statefulset.apps/postgres created
```

### Ver StatefulSet

```bash
kubectl get statefulset -n okla
```

**Salida:**
```
NAME       READY   AGE
postgres   1/1     2m
```

### Ver pod de PostgreSQL

```bash
kubectl get pods -n okla | grep postgres
```

**Salida:**
```
postgres-0   1/1   Running   0   2m
```

**Nombre predecible:** `postgres-0` (no `postgres-abc123`)

### Ver PVC creado automáticamente

```bash
kubectl get pvc -n okla
```

**Salida:**
```
NAME                      STATUS   VOLUME                 CAPACITY   ACCESS MODES   STORAGECLASS
postgres-data-postgres-0  Bound    pvc-abc123-def456...   10Gi       RWO            do-block-storage
```

**STATUS Bound** significa que el PV fue creado y vinculado.

### Ver PV

```bash
kubectl get pv
```

**Salida:**
```
NAME                     CAPACITY   ACCESS MODES   RECLAIM POLICY   STATUS   CLAIM
pvc-abc123-def456...     10Gi       RWO            Delete           Bound    okla/postgres-data-postgres-0
```

---

## 4. Verificar almacenamiento persistente

### Conectar a PostgreSQL

```bash
kubectl exec -it postgres-0 -n okla -- psql -U postgres
```

**Salida:**
```
psql (16.1)
Type "help" for help.

postgres=#
```

### Crear base de datos de prueba

```sql
CREATE DATABASE testdb;
\c testdb
CREATE TABLE users (id SERIAL PRIMARY KEY, name VARCHAR(100));
INSERT INTO users (name) VALUES ('John Doe'), ('Jane Smith');
SELECT * FROM users;
```

**Salida:**
```
 id |    name
----+------------
  1 | John Doe
  2 | Jane Smith
```

```sql
\q
```

### Eliminar el pod (simulación de crash)

```bash
kubectl delete pod postgres-0 -n okla
```

**Salida:**
```
pod "postgres-0" deleted
```

### Ver recreación automática

```bash
kubectl get pods -n okla -w | grep postgres
```

**Verás:**
```
postgres-0   1/1   Terminating   0   5m
postgres-0   0/1   Pending       0   0s
postgres-0   1/1   Running       0   15s
```

StatefulSet recrea el pod con el **mismo nombre** (`postgres-0`).

### Verificar que los datos persisten

```bash
kubectl exec -it postgres-0 -n okla -- psql -U postgres -d testdb -c "SELECT * FROM users;"
```

**Salida:**
```
 id |    name
----+------------
  1 | John Doe
  2 | Jane Smith
```

**¡Los datos siguen ahí!** 🎉

### ¿Por qué funcionó?

1. El pod `postgres-0` fue eliminado
2. StatefulSet lo recreó con el **mismo nombre**
3. Kubernetes montó el **mismo PVC** (`postgres-data-postgres-0`)
4. PostgreSQL encontró los datos existentes en `/var/lib/postgresql/data`

---

## 5. Conectar desde microservicios

### Connection String en .NET

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=vehiclessaleservice;Username=postgres;Password=..."
  }
}
```

**Host:** `postgres` (nombre del Service)

Kubernetes DNS lo resuelve a `postgres.okla.svc.cluster.local` → IP del pod `postgres-0`.

### Variables de entorno en Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
spec:
  template:
    spec:
      containers:
      - name: vehiclessaleservice
        image: ghcr.io/.../vehiclessaleservice:latest
        env:
        - name: DB_HOST
          value: "postgres"
        - name: DB_PORT
          value: "5432"
        - name: DB_NAME
          value: "vehiclessaleservice"
        - name: DB_USER
          value: "postgres"
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: DB_PASSWORD
```

### Probar conectividad desde un microservicio

```bash
# Entrar al pod de vehiclessaleservice
kubectl exec -it deployment/vehiclessaleservice -n okla -- sh

# Instalar psql (si no está)
apk add postgresql-client

# Conectar a PostgreSQL
psql -h postgres -U postgres -d vehiclessaleservice

# Ver tablas
\dt

# Salir
\q
exit
```

---

## 6. Gestión de bases de datos

### Crear bases de datos para cada microservicio

```bash
kubectl exec -it postgres-0 -n okla -- psql -U postgres <<EOF
CREATE DATABASE authservice;
CREATE DATABASE userservice;
CREATE DATABASE vehiclessaleservice;
CREATE DATABASE billingservice;
CREATE DATABASE mediaservice;
CREATE DATABASE notificationservice;
CREATE DATABASE errorservice;
CREATE DATABASE roleservice;
\l
EOF
```

### Ver bases de datos existentes

```bash
kubectl exec -it postgres-0 -n okla -- psql -U postgres -c "\l"
```

**Salida:**
```
                                    List of databases
          Name          |  Owner   | Encoding | Collate | Ctype
-----------------------+----------+----------+---------+--------
 authservice           | postgres | UTF8     | en_US   | en_US
 billingservice        | postgres | UTF8     | en_US   | en_US
 postgres              | postgres | UTF8     | en_US   | en_US
 vehiclessaleservice   | postgres | UTF8     | en_US   | en_US
 ...
```

### Ver tamaño de bases de datos

```bash
kubectl exec -it postgres-0 -n okla -- psql -U postgres -c \
  "SELECT pg_database.datname, pg_size_pretty(pg_database_size(pg_database.datname)) AS size 
   FROM pg_database 
   ORDER BY pg_database_size(pg_database.datname) DESC;"
```

**Salida:**
```
       datname        |  size
---------------------+---------
 vehiclessaleservice | 25 MB
 authservice         | 8 MB
 postgres            | 8 MB
```

### Ejecutar migrations desde código

En .NET con Entity Framework Core:

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();  // Ejecuta migrations
}
```

### Ejecutar migrations manualmente

```bash
# Desde tu máquina local, port-forward PostgreSQL
kubectl port-forward postgres-0 5433:5432 -n okla &

# Ejecutar migrations
cd backend/VehiclesSaleService/VehiclesSaleService.Infrastructure
dotnet ef database update --connection "Host=localhost;Port=5433;Database=vehiclessaleservice;Username=postgres;Password=..."
```

---

## 7. Backups y restauración

### Backup manual con pg_dump

```bash
# Port-forward PostgreSQL
kubectl port-forward postgres-0 5433:5432 -n okla &

# Backup de una base de datos
pg_dump -h localhost -p 5433 -U postgres vehiclessaleservice > backup-vehiclessaleservice-$(date +%Y%m%d).sql

# Backup de todas las bases de datos
pg_dumpall -h localhost -p 5433 -U postgres > backup-all-$(date +%Y%m%d).sql
```

### Backup desde dentro del pod

```bash
kubectl exec postgres-0 -n okla -- pg_dump -U postgres vehiclessaleservice > backup.sql
```

### Restauración desde backup

```bash
# Port-forward
kubectl port-forward postgres-0 5433:5432 -n okla &

# Restaurar
psql -h localhost -p 5433 -U postgres vehiclessaleservice < backup-vehiclessaleservice-20260107.sql
```

### Backup automático con CronJob

```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: postgres-backup
  namespace: okla
spec:
  schedule: "0 2 * * *"  # Diario a las 2 AM
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: backup
            image: postgres:16
            command:
            - /bin/bash
            - -c
            - |
              pg_dump -h postgres -U postgres vehiclessaleservice | \
              gzip > /backup/vehiclessaleservice-$(date +%Y%m%d-%H%M%S).sql.gz
            volumeMounts:
            - name: backup-storage
              mountPath: /backup
          restartPolicy: OnFailure
          volumes:
          - name: backup-storage
            persistentVolumeClaim:
              claimName: postgres-backup-pvc
```

### Copiar backup desde pod a local

```bash
kubectl cp okla/postgres-0:/backup/backup.sql ./local-backup.sql
```

---

## 8. Escalado de StatefulSets

### Escalar PostgreSQL (no recomendado para single-master)

```bash
# Escalar a 2 réplicas
kubectl scale statefulset postgres --replicas=2 -n okla
```

**Resultado:**
```
NAME         READY   AGE
postgres-0   1/1     10m
postgres-1   1/1     30s
```

**⚠️ Problema:** PostgreSQL single-master no soporta múltiples instancias con write sin configuración especial (replicación).

### PostgreSQL con replicación (avanzado)

Para high availability necesitas:
1. **Master** (postgres-0) - Read/Write
2. **Replicas** (postgres-1, postgres-2) - Read-only
3. **Patroni** o **Stolon** para automatic failover

**No cubierto en este tutorial.**

### Ver PVCs de múltiples réplicas

```bash
kubectl get pvc -n okla
```

**Salida:**
```
NAME                      STATUS   CAPACITY
postgres-data-postgres-0  Bound    10Gi
postgres-data-postgres-1  Bound    10Gi
```

Cada réplica tiene su propio volumen.

---

## 9. Cheat Sheet

### Comandos de StatefulSet

| Comando | Descripción |
|---------|-------------|
| `kubectl get statefulset -n okla` | Listar StatefulSets |
| `kubectl describe statefulset <nombre> -n okla` | Detalles |
| `kubectl scale statefulset <nombre> --replicas=3 -n okla` | Escalar |
| `kubectl delete statefulset <nombre> -n okla` | Eliminar (mantiene PVCs) |
| `kubectl delete statefulset <nombre> --cascade=orphan -n okla` | Eliminar sin eliminar pods |

### Comandos de PVC/PV

| Comando | Descripción |
|---------|-------------|
| `kubectl get pvc -n okla` | Listar PVCs |
| `kubectl get pv` | Listar PVs (cluster-wide) |
| `kubectl describe pvc <nombre> -n okla` | Detalles del PVC |
| `kubectl delete pvc <nombre> -n okla` | Eliminar PVC (y PV si ReclaimPolicy=Delete) |

### Comandos PostgreSQL

```bash
# Conectar a PostgreSQL
kubectl exec -it postgres-0 -n okla -- psql -U postgres

# Ejecutar query desde fuera
kubectl exec -it postgres-0 -n okla -- psql -U postgres -c "SELECT version();"

# Listar bases de datos
kubectl exec -it postgres-0 -n okla -- psql -U postgres -c "\l"

# Backup
kubectl exec postgres-0 -n okla -- pg_dump -U postgres dbname > backup.sql

# Port-forward
kubectl port-forward postgres-0 5433:5432 -n okla
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Explorar StatefulSet

1. Lista los StatefulSets en namespace okla
2. Ve cuántas réplicas tiene postgres
3. Ve el nombre del pod de PostgreSQL

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get statefulset -n okla

# 2
kubectl get statefulset postgres -n okla -o jsonpath='{.spec.replicas}'

# 3
kubectl get pods -n okla | grep postgres
```
</details>

### Ejercicio 2: Verificar PVC

1. Lista los PVCs en namespace okla
2. Ve el tamaño del PVC de postgres
3. Verifica que está Bound

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get pvc -n okla

# 2 y 3
kubectl describe pvc postgres-data-postgres-0 -n okla | grep -E "Capacity|Status"
```
</details>

### Ejercicio 3: Conectar a PostgreSQL

1. Entra al pod postgres-0
2. Conéctate a psql
3. Lista las bases de datos
4. Sal

<details>
<summary>Solución</summary>

```bash
# 1 y 2
kubectl exec -it postgres-0 -n okla -- psql -U postgres

# 3
\l

# 4
\q
```
</details>

### Ejercicio 4: Crear base de datos

1. Crea una base de datos llamada `testdb`
2. Conéctate a ella
3. Crea una tabla `products` con columnas id (SERIAL) y name (VARCHAR)
4. Inserta 2 productos

<details>
<summary>Solución</summary>

```bash
kubectl exec -it postgres-0 -n okla -- psql -U postgres <<EOF
CREATE DATABASE testdb;
\c testdb
CREATE TABLE products (id SERIAL PRIMARY KEY, name VARCHAR(100));
INSERT INTO products (name) VALUES ('Product A'), ('Product B');
SELECT * FROM products;
EOF
```
</details>

### Ejercicio 5: Backup y restauración

1. Haz un backup de la base de datos `vehiclessaleservice`
2. Guárdalo en tu máquina local
3. Verifica el tamaño del archivo

<details>
<summary>Solución</summary>

```bash
# 1 y 2
kubectl exec postgres-0 -n okla -- pg_dump -U postgres vehiclessaleservice > backup-vehicles.sql

# 3
ls -lh backup-vehicles.sql
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 8. Ahora sabes:
- ✅ Diferencia entre Deployment y StatefulSet
- ✅ Configurar PersistentVolumes y PersistentVolumeClaims
- ✅ Desplegar PostgreSQL en Kubernetes
- ✅ Verificar almacenamiento persistente
- ✅ Conectar microservicios a PostgreSQL
- ✅ Gestión de bases de datos y migrations
- ✅ Backups y restauración
- ✅ Conceptos de escalado de StatefulSets

---

**Anterior:** [07 - GitHub Container Registry](./07-registries-ghcr.md)  
**Siguiente:** [09 - Ocelot API Gateway](./09-ocelot-gateway.md)

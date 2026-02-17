# Tutorial 3: ConfigMaps y Secrets

**Duración:** 40 minutos  
**Nivel:** Principiante  
**Requisitos:** Tutorial 2 completado

---

## 📋 Resumen

Aprenderás a gestionar configuraciones y datos sensibles en Kubernetes usando ConfigMaps y Secrets. Entenderás cuándo usar cada uno y cómo montarlos en tus pods.

### Lo que aprenderás:
- Diferencia entre ConfigMap y Secret
- Crear ConfigMaps desde archivos y literales
- Crear y gestionar Secrets
- Montar ConfigMaps/Secrets como variables de entorno
- Montar ConfigMaps/Secrets como archivos (volumes)
- Actualizar configuraciones y reiniciar pods

---

## 📑 Índice

1. [Conceptos: ConfigMap vs Secret](#1-conceptos-configmap-vs-secret)
2. [Ver ConfigMaps y Secrets existentes](#2-ver-configmaps-y-secrets-existentes)
3. [Crear ConfigMaps](#3-crear-configmaps)
4. [Crear Secrets](#4-crear-secrets)
5. [Usar ConfigMaps como variables de entorno](#5-usar-configmaps-como-variables-de-entorno)
6. [Montar ConfigMaps como archivos](#6-montar-configmaps-como-archivos)
7. [Actualizar configuraciones](#7-actualizar-configuraciones)
8. [Caso real: Gateway Ocelot](#8-caso-real-gateway-ocelot)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Conceptos: ConfigMap vs Secret

### ¿Por qué necesitamos ConfigMaps y Secrets?

**Principio:** Separar la configuración del código. Tus aplicaciones no deberían tener configuraciones hardcodeadas.

```csharp
// ❌ MAL - Hardcoded
var connectionString = "Host=postgres.okla.svc.cluster.local;Port=5432;...";

// ✅ BIEN - Variable de entorno
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
```

### 🔷 ConfigMap

**Definición:** Almacena datos de configuración no sensibles como pares key-value.

**Casos de uso:**
- URLs de APIs
- Nombres de hosts
- Flags de features
- Archivos de configuración (JSON, XML, YAML)
- Configuración de aplicación

**Ejemplo:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: app-config
data:
  API_URL: "https://api.okla.com.do"
  LOG_LEVEL: "Info"
  FEATURE_PAYMENTS: "true"
```

### 🔷 Secret

**Definición:** Almacena datos sensibles de forma más segura. Los valores están codificados en base64.

**Casos de uso:**
- Contraseñas de base de datos
- API keys
- Tokens JWT
- Certificados SSL
- Credenciales de servicios externos

**Ejemplo:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: db-secret
type: Opaque
data:
  # Valores en base64
  DB_PASSWORD: cGFzc3dvcmQxMjM=  # "password123"
  API_KEY: YWJjZGVmZ2hpams=      # "abcdefghijk"
```

### Diferencias clave

| Aspecto | ConfigMap | Secret |
|---------|-----------|--------|
| **Propósito** | Configuración no sensible | Datos sensibles |
| **Encoding** | Texto plano | Base64 |
| **Seguridad** | Normal | Mayor (puede encriptarse en etcd) |
| **Tamaño máximo** | 1MB | 1MB |
| **Visualización en UI** | Visible en dashboards | Oculto por defecto |

**⚠️ Importante:** Secret NO es encriptación. Base64 es fácilmente decodificable. Para verdadera seguridad, usa herramientas como Sealed Secrets o External Secrets Operator.

---

## 2. Ver ConfigMaps y Secrets existentes

### Listar ConfigMaps

```bash
kubectl get configmaps -n okla
# Abreviado:
kubectl get cm -n okla
```

**Salida esperada:**
```
NAME              DATA   AGE
gateway-config    1      6h
kube-root-ca.crt  1      7d
```

**DATA** indica cuántas keys tiene el ConfigMap.

### Ver contenido de un ConfigMap

```bash
kubectl get configmap gateway-config -n okla -o yaml
```

**Salida:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: gateway-config
  namespace: okla
data:
  ocelot.json: |
    {
      "Routes": [
        {
          "DownstreamPathTemplate": "/api/{everything}",
          "DownstreamScheme": "http",
          "DownstreamHostAndPorts": [
            {
              "Host": "vehiclessaleservice",
              "Port": 8080
            }
          ],
          ...
        }
      ]
    }
```

### Listar Secrets

```bash
kubectl get secrets -n okla
```

**Salida esperada:**
```
NAME                  TYPE                                  DATA   AGE
db-credentials        Opaque                                3      6h
default-token-xxxxx   kubernetes.io/service-account-token   3      7d
rabbitmq-secret       Opaque                                2      6h
```

### Ver contenido de un Secret

```bash
kubectl get secret db-credentials -n okla -o yaml
```

**Salida:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: db-credentials
  namespace: okla
type: Opaque
data:
  DB_HOST: cG9zdGdyZXM=                    # postgres
  DB_NAME: dmVoaWNsZXNzYWxlc2VydmljZQ==    # vehiclessaleservice
  DB_PASSWORD: cGFzc3dvcmQxMjM=            # password123
```

### Decodificar valores de Secret

```bash
# Obtener un valor específico y decodificar
kubectl get secret db-credentials -n okla -o jsonpath='{.data.DB_PASSWORD}' | base64 -d
```

**Salida:**
```
password123
```

---

## 3. Crear ConfigMaps

### Método 1: Desde literales (línea de comandos)

```bash
kubectl create configmap app-settings -n okla \
  --from-literal=API_URL="https://api.okla.com.do" \
  --from-literal=LOG_LEVEL="Info" \
  --from-literal=FEATURE_PAYMENTS="true"
```

**Verificar:**
```bash
kubectl get cm app-settings -n okla -o yaml
```

### Método 2: Desde archivo

Crea un archivo `app.properties`:
```properties
API_URL=https://api.okla.com.do
LOG_LEVEL=Info
FEATURE_PAYMENTS=true
```

Crear ConfigMap:
```bash
kubectl create configmap app-settings -n okla \
  --from-file=app.properties
```

**Resultado:**
```yaml
data:
  app.properties: |
    API_URL=https://api.okla.com.do
    LOG_LEVEL=Info
    FEATURE_PAYMENTS=true
```

### Método 3: Desde archivo JSON/YAML (común en gateways)

```bash
# Desde archivo ocelot.json
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=./backend/Gateway/Gateway.Api/ocelot.prod.json
```

**Uso típico en OKLA:**
```bash
# Actualizar configuración del Gateway
kubectl delete configmap gateway-config -n okla
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json
kubectl rollout restart deployment gateway -n okla
```

### Método 4: Desde YAML manifest

Crea archivo `configmap.yaml`:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: app-settings
  namespace: okla
data:
  API_URL: "https://api.okla.com.do"
  LOG_LEVEL: "Info"
  FEATURE_PAYMENTS: "true"
  config.json: |
    {
      "database": {
        "host": "postgres",
        "port": 5432
      }
    }
```

Aplicar:
```bash
kubectl apply -f configmap.yaml
```

---

## 4. Crear Secrets

### Método 1: Desde literales

```bash
kubectl create secret generic db-credentials -n okla \
  --from-literal=DB_PASSWORD="password123" \
  --from-literal=DB_USER="postgres" \
  --from-literal=DB_NAME="vehiclessaleservice"
```

**⚠️ Cuidado:** Esto queda en el historial de bash. Mejor usar archivos o YAML.

### Método 2: Desde archivo

Crea archivo `db-password.txt`:
```
MySecurePassword123!
```

Crear Secret:
```bash
kubectl create secret generic db-password -n okla \
  --from-file=password=db-password.txt
```

**Limpiar archivo:**
```bash
rm db-password.txt
```

### Método 3: Desde YAML manifest

Primero, codifica los valores en base64:
```bash
echo -n "password123" | base64
# Salida: cGFzc3dvcmQxMjM=

echo -n "myapikey" | base64
# Salida: bXlhcGlrZXk=
```

Crea `secret.yaml`:
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: db-credentials
  namespace: okla
type: Opaque
data:
  DB_PASSWORD: cGFzc3dvcmQxMjM=
  API_KEY: bXlhcGlrZXk=
```

Aplicar:
```bash
kubectl apply -f secret.yaml
```

### Método 4: Usando stringData (más fácil)

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: db-credentials
  namespace: okla
type: Opaque
stringData:  # No necesita base64, Kubernetes lo hace automáticamente
  DB_PASSWORD: password123
  API_KEY: myapikey
```

**Ventaja:** No necesitas codificar manualmente. Kubernetes lo hace por ti.

### Tipos de Secrets

```bash
# Opaque (genérico)
kubectl create secret generic my-secret ...

# Docker registry
kubectl create secret docker-registry ghcr-secret \
  --docker-server=ghcr.io \
  --docker-username=yourusername \
  --docker-password=yourtoken \
  -n okla

# TLS certificate
kubectl create secret tls tls-secret \
  --cert=path/to/tls.crt \
  --key=path/to/tls.key \
  -n okla
```

---

## 5. Usar ConfigMaps como variables de entorno

### Opción 1: Inyectar todas las keys

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: authservice
spec:
  template:
    spec:
      containers:
      - name: authservice
        image: ghcr.io/.../authservice:latest
        envFrom:
        - configMapRef:
            name: app-settings  # Todas las keys del ConfigMap
```

**Resultado:** Si `app-settings` tiene:
```yaml
data:
  API_URL: "https://api.okla.com.do"
  LOG_LEVEL: "Info"
```

El pod tendrá variables de entorno:
- `API_URL=https://api.okla.com.do`
- `LOG_LEVEL=Info`

### Opción 2: Inyectar keys específicas

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: authservice
spec:
  template:
    spec:
      containers:
      - name: authservice
        image: ghcr.io/.../authservice:latest
        env:
        - name: API_URL
          valueFrom:
            configMapKeyRef:
              name: app-settings
              key: API_URL
        - name: LOG_LEVEL
          valueFrom:
            configMapKeyRef:
              name: app-settings
              key: LOG_LEVEL
```

### Combinar con Secrets

```yaml
env:
- name: API_URL
  valueFrom:
    configMapKeyRef:
      name: app-settings
      key: API_URL
- name: DB_PASSWORD
  valueFrom:
    secretKeyRef:
      name: db-credentials
      key: DB_PASSWORD
```

### Verificar variables de entorno en un pod

```bash
# Entrar al pod
kubectl exec -it authservice-xxx -n okla -- sh

# Ver variables de entorno
env | grep -E "API_URL|LOG_LEVEL|DB_PASSWORD"
```

---

## 6. Montar ConfigMaps como archivos

### ¿Cuándo montar como archivo?

- Archivos de configuración completos (ocelot.json, appsettings.json)
- Certificados
- Scripts
- Plantillas

### Ejemplo: Gateway Ocelot

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
spec:
  template:
    spec:
      containers:
      - name: gateway
        image: ghcr.io/.../gateway:latest
        volumeMounts:
        - name: ocelot-config
          mountPath: /app/ocelot.json
          subPath: ocelot.json
      volumes:
      - name: ocelot-config
        configMap:
          name: gateway-config
```

**¿Qué hace esto?**
1. Crea un volume llamado `ocelot-config` desde el ConfigMap `gateway-config`
2. Lo monta en `/app/ocelot.json` dentro del contenedor
3. `subPath` evita reemplazar todo el directorio, solo el archivo

### Montar todo el ConfigMap como directorio

```yaml
volumeMounts:
- name: config-volume
  mountPath: /etc/config
volumes:
- name: config-volume
  configMap:
    name: app-settings
```

**Resultado:** Si `app-settings` tiene:
```yaml
data:
  API_URL: "..."
  LOG_LEVEL: "..."
  config.json: "{...}"
```

Dentro del contenedor habrá:
```
/etc/config/API_URL
/etc/config/LOG_LEVEL
/etc/config/config.json
```

### Montar Secret como archivo

```yaml
volumeMounts:
- name: db-creds
  mountPath: /etc/secrets
  readOnly: true
volumes:
- name: db-creds
  secret:
    secretName: db-credentials
```

**Ventaja:** El archivo solo es legible dentro del contenedor, no aparece en logs.

---

## 7. Actualizar configuraciones

### Actualizar un ConfigMap

```bash
# Opción 1: Editar directamente
kubectl edit configmap app-settings -n okla

# Opción 2: Desde archivo actualizado
kubectl delete configmap gateway-config -n okla
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json

# Opción 3: apply desde YAML
kubectl apply -f configmap.yaml
```

### ⚠️ Importante: Pods NO se reinician automáticamente

Cuando actualizas un ConfigMap o Secret, los pods que ya están corriendo **NO se actualizan automáticamente**.

**Necesitas reiniciar el deployment:**

```bash
kubectl rollout restart deployment gateway -n okla
```

### Verificar que el ConfigMap se actualizó dentro del pod

```bash
# Método 1: Exec en el pod y ver el archivo
kubectl exec -it gateway-xxx -n okla -- cat /app/ocelot.json

# Método 2: Ver variable de entorno
kubectl exec -it gateway-xxx -n okla -- env | grep API_URL
```

### Actualizar un Secret

```bash
# Editar directamente
kubectl edit secret db-credentials -n okla

# Desde YAML
kubectl apply -f secret.yaml

# Recrear desde literal
kubectl delete secret db-credentials -n okla
kubectl create secret generic db-credentials -n okla \
  --from-literal=DB_PASSWORD="newpassword456"
```

**Reiniciar pods:**
```bash
kubectl rollout restart deployment authservice -n okla
```

---

## 8. Caso real: Gateway Ocelot

### Problema real en OKLA

El Gateway Ocelot necesita leer `ocelot.json` con todas las rutas. Este archivo tiene 500+ líneas y cambia frecuentemente.

### Solución: ConfigMap + Volume Mount

**Paso 1:** Crear ConfigMap desde el archivo

```bash
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json
```

**Paso 2:** Ver extracto del ConfigMap

```bash
kubectl get cm gateway-config -n okla -o yaml | head -30
```

**Salida:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: gateway-config
  namespace: okla
data:
  ocelot.json: |
    {
      "Routes": [
        {
          "UpstreamPathTemplate": "/api/vehicles/{everything}",
          "DownstreamPathTemplate": "/api/{everything}",
          "DownstreamScheme": "http",
          "DownstreamHostAndPorts": [
            {
              "Host": "vehiclessaleservice",
              "Port": 8080
            }
          ]
        },
        ...
      ]
    }
```

**Paso 3:** El Deployment del Gateway monta el ConfigMap

```yaml
# k8s/deployments.yaml (extracto)
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
spec:
  template:
    spec:
      containers:
      - name: gateway
        image: ghcr.io/gregorymorenoiem/cardealer-gateway:latest
        volumeMounts:
        - name: ocelot-config
          mountPath: /app/ocelot.json
          subPath: ocelot.json
      volumes:
      - name: ocelot-config
        configMap:
          name: gateway-config
```

**Paso 4:** Actualizar la configuración

```bash
# 1. Editar backend/Gateway/Gateway.Api/ocelot.prod.json localmente
# 2. Recrear ConfigMap
kubectl delete configmap gateway-config -n okla
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json

# 3. Reiniciar Gateway para recargar configuración
kubectl rollout restart deployment gateway -n okla

# 4. Verificar
kubectl rollout status deployment gateway -n okla
curl https://api.okla.com.do/health
```

### Verificar que el Gateway leyó la nueva configuración

```bash
# Entrar al pod del Gateway
kubectl exec -it deployment/gateway -n okla -- sh

# Ver el archivo montado
cat /app/ocelot.json | head -20

# Buscar una ruta específica
cat /app/ocelot.json | grep "homepagesections"
```

---

## 9. Cheat Sheet

### Comandos de ConfigMaps

| Comando | Descripción |
|---------|-------------|
| `kubectl get cm -n okla` | Listar ConfigMaps |
| `kubectl describe cm <nombre> -n okla` | Ver detalles |
| `kubectl get cm <nombre> -n okla -o yaml` | Ver YAML completo |
| `kubectl create cm <nombre> --from-literal=KEY=value -n okla` | Desde literal |
| `kubectl create cm <nombre> --from-file=archivo.json -n okla` | Desde archivo |
| `kubectl delete cm <nombre> -n okla` | Eliminar |
| `kubectl edit cm <nombre> -n okla` | Editar directamente |

### Comandos de Secrets

| Comando | Descripción |
|---------|-------------|
| `kubectl get secrets -n okla` | Listar Secrets |
| `kubectl describe secret <nombre> -n okla` | Ver detalles (sin valores) |
| `kubectl get secret <nombre> -n okla -o yaml` | Ver YAML (base64) |
| `kubectl get secret <nombre> -n okla -o jsonpath='{.data.KEY}' \| base64 -d` | Decodificar valor |
| `kubectl create secret generic <nombre> --from-literal=KEY=value -n okla` | Desde literal |
| `kubectl delete secret <nombre> -n okla` | Eliminar |

### Patrones comunes

```bash
# Ver todas las configuraciones del namespace
kubectl get cm,secrets -n okla

# Actualizar Gateway config (flujo completo)
kubectl delete cm gateway-config -n okla
kubectl create cm gateway-config --from-file=ocelot.json=path/to/ocelot.prod.json -n okla
kubectl rollout restart deployment gateway -n okla

# Verificar variables de entorno en un pod
kubectl exec -it <pod> -n okla -- env

# Ver archivo montado desde ConfigMap
kubectl exec -it <pod> -n okla -- cat /path/to/file
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Crear ConfigMap y usarlo

1. Crea un ConfigMap llamado `test-config` con dos keys:
   - `APP_NAME=OKLA`
   - `ENVIRONMENT=production`
2. Verifica que se creó correctamente
3. Elimínalo

<details>
<summary>Solución</summary>

```bash
# 1
kubectl create configmap test-config -n okla \
  --from-literal=APP_NAME=OKLA \
  --from-literal=ENVIRONMENT=production

# 2
kubectl get cm test-config -n okla -o yaml

# 3
kubectl delete cm test-config -n okla
```
</details>

### Ejercicio 2: Crear Secret y decodificar

1. Crea un Secret llamado `test-secret` con un password: `supersecret123`
2. Obtén el valor codificado en base64
3. Decodifícalo para verificar
4. Elimínalo

<details>
<summary>Solución</summary>

```bash
# 1
kubectl create secret generic test-secret -n okla \
  --from-literal=PASSWORD=supersecret123

# 2
kubectl get secret test-secret -n okla -o jsonpath='{.data.PASSWORD}'

# 3
kubectl get secret test-secret -n okla -o jsonpath='{.data.PASSWORD}' | base64 -d

# 4
kubectl delete secret test-secret -n okla
```
</details>

### Ejercicio 3: Verificar ConfigMap del Gateway

1. Ve el ConfigMap `gateway-config`
2. Busca la ruta de `/api/vehicles` en el JSON
3. Verifica que el puerto downstream es 8080

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get cm gateway-config -n okla -o yaml

# 2
kubectl get cm gateway-config -n okla -o yaml | grep -A5 "vehicles"

# 3
kubectl get cm gateway-config -n okla -o yaml | grep "Port"
```
</details>

### Ejercicio 4: Actualizar y reiniciar (simulación)

1. Edita el ConfigMap `gateway-config` (agrega un comentario en el JSON)
2. Reinicia el deployment del Gateway
3. Verifica que el rollout completó

<details>
<summary>Solución</summary>

```bash
# 1
kubectl edit cm gateway-config -n okla
# (Agregar un comentario en alguna parte del JSON, Ctrl+O para guardar, Ctrl+X para salir si usas nano)

# 2
kubectl rollout restart deployment gateway -n okla

# 3
kubectl rollout status deployment gateway -n okla
```
</details>

### Ejercicio 5: Explorar variables de entorno

1. Elige cualquier pod del namespace `okla`
2. Entra en modo interactivo (exec)
3. Lista todas las variables de entorno
4. Busca las que contengan "RABBIT" o "POSTGRES"

<details>
<summary>Solución</summary>

```bash
# 1 y 2
kubectl exec -it deployment/authservice -n okla -- sh

# 3
env

# 4
env | grep -E "RABBIT|POSTGRES"

# Salir
exit
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 3. Ahora sabes:
- ✅ Diferencia entre ConfigMap y Secret
- ✅ Crear ConfigMaps desde literales, archivos y YAML
- ✅ Crear Secrets de forma segura
- ✅ Usar ConfigMaps/Secrets como variables de entorno
- ✅ Montar ConfigMaps/Secrets como archivos
- ✅ Actualizar configuraciones y reiniciar pods
- ✅ Caso real: Configuración del Gateway Ocelot

---

**Anterior:** [02 - Pods y Deployments](./02-pods-deployments.md)  
**Siguiente:** [04 - Logs y Debugging](./04-logs-debugging.md)

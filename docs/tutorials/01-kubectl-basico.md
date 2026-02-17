# Tutorial 1: kubectl Básico para DOKS

**Duración:** 30 minutos  
**Nivel:** Principiante  
**Requisitos:** kubectl instalado, acceso a cluster DOKS

---

## 📋 Resumen

En este tutorial aprenderás los comandos fundamentales de `kubectl` para interactuar con tu cluster de Kubernetes en Digital Ocean. Dominarás cómo ver recursos, obtener información detallada, y navegar por namespaces.

### Lo que aprenderás:
- Conectar kubectl a tu cluster DOKS
- Ver pods, services, deployments y otros recursos
- Filtrar por namespace y labels
- Obtener información detallada de recursos
- Formatos de salida (yaml, json, wide)

---

## 📑 Índice

1. [Configurar conexión al cluster](#1-configurar-conexión-al-cluster)
2. [Comandos GET básicos](#2-comandos-get-básicos)
3. [Filtrar por namespace](#3-filtrar-por-namespace)
4. [Comando DESCRIBE](#4-comando-describe)
5. [Formatos de salida](#5-formatos-de-salida)
6. [Filtrar con labels](#6-filtrar-con-labels)
7. [Contextos y clusters](#7-contextos-y-clusters)
8. [Cheat Sheet](#8-cheat-sheet)
9. [Ejercicios prácticos](#9-ejercicios-prácticos)

---

## 1. Configurar conexión al cluster

### ¿Por qué necesitamos configurar la conexión?

`kubectl` es el CLI que nos permite comunicarnos con Kubernetes. Pero necesita saber **a cuál cluster conectarse** y **cómo autenticarse**. Esta información se guarda en un archivo llamado `kubeconfig`.

### Paso 1.1: Instalar doctl (CLI de Digital Ocean)

```bash
# macOS con Homebrew
brew install doctl

# Verificar instalación
doctl version
```

**¿Por qué doctl?** Es la herramienta oficial de Digital Ocean que nos permite autenticarnos y obtener la configuración del cluster automáticamente.

### Paso 1.2: Autenticarse en Digital Ocean

```bash
doctl auth init
```

Este comando te pedirá un **API Token** de Digital Ocean. Para obtenerlo:
1. Ve a https://cloud.digitalocean.com/account/api/tokens
2. Click en "Generate New Token"
3. Dale un nombre (ej: "kubectl-access")
4. Copia el token y pégalo en la terminal

**¿Por qué un token?** Digital Ocean usa OAuth2. El token te identifica y autoriza para acceder a tus recursos.

### Paso 1.3: Descargar configuración del cluster

```bash
# Ver clusters disponibles
doctl kubernetes cluster list

# Descargar kubeconfig del cluster okla-cluster
doctl kubernetes cluster kubeconfig save okla-cluster
```

**Salida esperada:**
```
Notice: Adding cluster credentials to kubeconfig file found in "/Users/tu-usuario/.kube/config"
Notice: Setting current-context to do-nyc1-okla-cluster
```

**¿Qué pasó?** 
- `doctl` descargó los certificados y credenciales del cluster
- Los guardó en `~/.kube/config`
- Configuró este cluster como el "contexto actual"

### Paso 1.4: Verificar la conexión

```bash
kubectl cluster-info
```

**Salida esperada:**
```
Kubernetes control plane is running at https://xxx.k8s.ondigitalocean.com
CoreDNS is running at https://xxx.k8s.ondigitalocean.com/api/v1/namespaces/kube-system/services/kube-dns:dns/proxy
```

**¿Qué significa?** Tu `kubectl` está conectado al cluster y puede comunicarse con el API server de Kubernetes.

---

## 2. Comandos GET básicos

### ¿Por qué `kubectl get`?

El comando `get` es el más usado en Kubernetes. Sirve para **listar recursos** del cluster. Piénsalo como un `ls` para Kubernetes.

### Sintaxis básica

```bash
kubectl get <tipo-de-recurso>
```

### Paso 2.1: Ver todos los namespaces

```bash
kubectl get namespaces
# Abreviado:
kubectl get ns
```

**Salida esperada:**
```
NAME              STATUS   AGE
default           Active   7d
kube-system       Active   7d
kube-public       Active   7d
okla              Active   6h
cert-manager      Active   6h
ingress-nginx     Active   6h
```

**¿Qué son los namespaces?** Son como "carpetas" que agrupan recursos relacionados. Nuestro proyecto usa el namespace `okla`.

### Paso 2.2: Ver pods (contenedores corriendo)

```bash
# Pods en el namespace default
kubectl get pods

# Pods en el namespace okla (nuestro proyecto)
kubectl get pods -n okla
```

**Salida esperada:**
```
NAME                                   READY   STATUS    RESTARTS   AGE
authservice-b5747bbcf-6qf2x            1/1     Running   0          6h
billingservice-d9496cc54-5nnpv         1/1     Running   0          6h
frontend-web-585bd859d8-tn5v4          1/1     Running   0          6h
gateway-77d6c65764-w2mmr               1/1     Running   0          2h
...
```

**Columnas explicadas:**
| Columna | Significado |
|---------|-------------|
| NAME | Nombre único del pod (incluye hash aleatorio) |
| READY | Contenedores listos / Total contenedores |
| STATUS | Running, Pending, Error, CrashLoopBackOff, etc. |
| RESTARTS | Cuántas veces se ha reiniciado |
| AGE | Tiempo desde que se creó |

### Paso 2.3: Ver deployments

```bash
kubectl get deployments -n okla
# Abreviado:
kubectl get deploy -n okla
```

**Salida esperada:**
```
NAME                  READY   UP-TO-DATE   AVAILABLE   AGE
authservice           1/1     1            1           6h
billingservice        1/1     1            1           6h
frontend-web          1/1     1            1           6h
gateway               1/1     1            1           6h
...
```

**¿Deployment vs Pod?** 
- Un **Deployment** es la "receta" que dice cuántos pods crear
- Un **Pod** es la instancia corriendo
- Si un pod muere, el Deployment crea otro automáticamente

### Paso 2.4: Ver services

```bash
kubectl get services -n okla
# Abreviado:
kubectl get svc -n okla
```

**Salida esperada:**
```
NAME                  TYPE        CLUSTER-IP      PORT(S)          AGE
authservice           ClusterIP   10.116.54.128   8080/TCP         6h
frontend-web          ClusterIP   10.116.58.183   8080/TCP         6h
gateway               ClusterIP   10.116.51.214   8080/TCP         6h
postgres              ClusterIP   10.116.45.161   5432/TCP         6h
...
```

**¿Qué es un Service?** Es una IP estable que permite acceder a los pods. Los pods cambian de IP, pero el Service siempre tiene la misma.

### Paso 2.5: Ver varios recursos a la vez

```bash
kubectl get pods,services,deployments -n okla
```

**¿Por qué?** A veces necesitas ver el panorama completo rápidamente.

---

## 3. Filtrar por namespace

### ¿Por qué filtrar por namespace?

Por defecto, `kubectl` muestra recursos del namespace `default`. Pero nuestro proyecto está en `okla`, así que siempre debemos especificarlo.

### Opción 1: Flag -n en cada comando

```bash
kubectl get pods -n okla
kubectl get services -n okla
kubectl get deployments -n okla
```

### Opción 2: Cambiar el namespace por defecto (recomendado)

```bash
# Configurar okla como namespace por defecto
kubectl config set-context --current --namespace=okla

# Verificar
kubectl config view --minify | grep namespace
```

**Resultado:** Ya no necesitas `-n okla` en cada comando.

```bash
# Ahora esto funciona directamente
kubectl get pods
```

### Opción 3: Ver recursos de TODOS los namespaces

```bash
kubectl get pods --all-namespaces
# Abreviado:
kubectl get pods -A
```

**¿Cuándo usar esto?** Para ver el estado general del cluster o buscar un recurso sin saber en qué namespace está.

---

## 4. Comando DESCRIBE

### ¿Por qué `describe`?

Mientras `get` te da un resumen, `describe` te da **toda la información** sobre un recurso. Es esencial para debugging.

### Sintaxis

```bash
kubectl describe <tipo> <nombre> -n <namespace>
```

### Paso 4.1: Describir un pod

```bash
kubectl describe pod gateway-77d6c65764-w2mmr -n okla
```

**Información que obtienes:**
- **Labels y Annotations** - Metadatos del pod
- **Status** - Estado actual
- **IP** - IP asignada al pod
- **Containers** - Imagen, puertos, variables de entorno
- **Conditions** - Ready, Initialized, etc.
- **Events** - Historial de lo que ha pasado (¡clave para debugging!)

### Paso 4.2: Describir un deployment

```bash
kubectl describe deployment gateway -n okla
```

**Información útil:**
- Cuántas réplicas hay
- Estrategia de actualización
- Selector de pods
- Template del pod

### Paso 4.3: Ver solo los eventos (al final del describe)

Los **Events** son oro para debugging. Si un pod no inicia, los eventos te dicen por qué:

```
Events:
  Type     Reason     Age   From               Message
  ----     ------     ----  ----               -------
  Normal   Scheduled  2m    default-scheduler  Successfully assigned okla/gateway-xxx to node-1
  Normal   Pulling    2m    kubelet            Pulling image "ghcr.io/.../gateway:latest"
  Normal   Pulled     1m    kubelet            Successfully pulled image
  Normal   Created    1m    kubelet            Created container gateway
  Normal   Started    1m    kubelet            Started container gateway
```

---

## 5. Formatos de salida

### ¿Por qué diferentes formatos?

A veces necesitas:
- Más columnas (`-o wide`)
- Copiar la configuración completa (`-o yaml`)
- Procesar datos con scripts (`-o json`)
- Solo obtener un valor específico (`-o jsonpath`)

### Formato wide (más columnas)

```bash
kubectl get pods -n okla -o wide
```

**Salida:**
```
NAME                        READY   STATUS    IP             NODE
gateway-77d6c65764-w2mmr    1/1     Running   10.244.0.15    pool-xxxxx
```

**Columnas adicionales:** IP del pod, nodo donde corre.

### Formato YAML (configuración completa)

```bash
kubectl get deployment gateway -n okla -o yaml
```

**¿Para qué sirve?**
- Ver la configuración exacta
- Copiar y modificar para crear nuevos recursos
- Entender cómo está configurado algo

### Formato JSON

```bash
kubectl get pods -n okla -o json
```

**¿Para qué sirve?** Procesar con `jq` u otras herramientas.

### JSONPath (extraer valores específicos)

```bash
# Obtener solo las IPs de todos los pods
kubectl get pods -n okla -o jsonpath='{.items[*].status.podIP}'

# Obtener el nombre de la imagen del gateway
kubectl get deployment gateway -n okla -o jsonpath='{.spec.template.spec.containers[0].image}'
```

---

## 6. Filtrar con labels

### ¿Qué son los labels?

Los labels son pares key-value que identifican recursos. Son como "etiquetas" que puedes usar para filtrar.

### Ver labels de los pods

```bash
kubectl get pods -n okla --show-labels
```

**Salida:**
```
NAME                        STATUS    LABELS
gateway-77d6c65764-w2mmr    Running   app=gateway,tier=backend
frontend-web-xxx            Running   app=frontend-web,tier=frontend
```

### Filtrar por label

```bash
# Solo pods del tier backend
kubectl get pods -n okla -l tier=backend

# Solo pods de la app gateway
kubectl get pods -n okla -l app=gateway

# Combinar labels (AND)
kubectl get pods -n okla -l app=gateway,tier=backend
```

**¿Para qué sirve?** Cuando tienes muchos pods y quieres ver solo los de un servicio específico.

---

## 7. Contextos y clusters

### ¿Qué es un contexto?

Un contexto combina:
- **Cluster** - A qué cluster conectarse
- **User** - Con qué credenciales
- **Namespace** - Namespace por defecto

### Ver contextos disponibles

```bash
kubectl config get-contexts
```

**Salida:**
```
CURRENT   NAME                      CLUSTER                   NAMESPACE
*         do-nyc1-okla-cluster      do-nyc1-okla-cluster      okla
          docker-desktop            docker-desktop            default
          minikube                  minikube                  default
```

El asterisco (*) indica el contexto actual.

### Cambiar de contexto

```bash
# Cambiar a otro cluster
kubectl config use-context minikube

# Volver a DOKS
kubectl config use-context do-nyc1-okla-cluster
```

**¿Cuándo usar esto?** Si trabajas con múltiples clusters (desarrollo local, staging, producción).

### Ver contexto actual

```bash
kubectl config current-context
```

---

## 8. Cheat Sheet

### Comandos más usados

| Comando | Descripción |
|---------|-------------|
| `kubectl get pods -n okla` | Ver pods |
| `kubectl get pods -n okla -o wide` | Ver pods con más detalle |
| `kubectl get svc -n okla` | Ver services |
| `kubectl get deploy -n okla` | Ver deployments |
| `kubectl get all -n okla` | Ver todo en el namespace |
| `kubectl describe pod <nombre> -n okla` | Detalles de un pod |
| `kubectl logs <pod> -n okla` | Ver logs (siguiente tutorial) |
| `kubectl get pods -A` | Pods en todos los namespaces |

### Abreviaciones útiles

| Recurso | Abreviación |
|---------|-------------|
| namespaces | ns |
| pods | po |
| services | svc |
| deployments | deploy |
| configmaps | cm |
| secrets | secret |
| persistentvolumeclaims | pvc |

### Flags comunes

| Flag | Significado |
|------|-------------|
| `-n okla` | Namespace específico |
| `-o wide` | Más columnas |
| `-o yaml` | Salida en YAML |
| `-o json` | Salida en JSON |
| `-l app=gateway` | Filtrar por label |
| `-A` | Todos los namespaces |
| `--show-labels` | Mostrar labels |
| `-w` | Watch (actualización en tiempo real) |

---

## 9. Ejercicios prácticos

### Ejercicio 1: Explorar el cluster
1. Lista todos los namespaces
2. Cuenta cuántos pods hay en el namespace `kube-system`
3. Lista todos los services en el namespace `okla`

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get ns

# 2
kubectl get pods -n kube-system | wc -l

# 3
kubectl get svc -n okla
```
</details>

### Ejercicio 2: Encontrar información
1. ¿En qué nodo está corriendo el pod `postgres-0`?
2. ¿Cuál es la imagen Docker del deployment `gateway`?
3. ¿Cuál es el ClusterIP del service `authservice`?

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get pods -n okla -o wide | grep postgres

# 2
kubectl get deploy gateway -n okla -o jsonpath='{.spec.template.spec.containers[0].image}'

# 3
kubectl get svc authservice -n okla -o jsonpath='{.spec.clusterIP}'
```
</details>

### Ejercicio 3: Debugging básico
1. Describe el pod del gateway y encuentra cuántas veces se ha reiniciado
2. Encuentra todos los pods que tienen el label `tier=frontend`

<details>
<summary>Solución</summary>

```bash
# 1
kubectl describe pod -l app=gateway -n okla | grep -i restart

# 2
kubectl get pods -n okla -l tier=frontend
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 1. Ahora sabes:
- ✅ Conectar kubectl a tu cluster DOKS
- ✅ Ver pods, services y deployments
- ✅ Filtrar por namespace y labels
- ✅ Obtener información detallada con describe
- ✅ Usar diferentes formatos de salida
- ✅ Cambiar entre contextos

---

**Anterior:** [README](./README.md)  
**Siguiente:** [02 - Gestión de Pods y Deployments](./02-pods-deployments.md)

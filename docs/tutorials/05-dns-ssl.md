# Tutorial 5: DNS y SSL con cert-manager

**Duración:** 45 minutos  
**Nivel:** Intermedio  
**Requisitos:** Tutorial 4 completado, dominio registrado

---

## 📋 Resumen

Aprenderás a configurar DNS para tu cluster de Kubernetes y a obtener certificados SSL/TLS gratuitos usando cert-manager y Let's Encrypt. Al final, tendrás HTTPS automático para tus servicios.

### Lo que aprenderás:
- Cómo funciona DNS en Kubernetes
- Configurar registros A para tu dominio
- Instalar cert-manager en el cluster
- Configurar ClusterIssuer para Let's Encrypt
- Obtener certificados SSL automáticamente
- Renovación automática de certificados

---

## 📑 Índice

1. [Conceptos: DNS y SSL/TLS](#1-conceptos-dns-y-ssltls)
2. [DNS interno de Kubernetes](#2-dns-interno-de-kubernetes)
3. [Configurar DNS externo](#3-configurar-dns-externo)
4. [Instalar cert-manager](#4-instalar-cert-manager)
5. [Crear ClusterIssuer (Let's Encrypt)](#5-crear-clusterissuer-lets-encrypt)
6. [Configurar TLS en Ingress](#6-configurar-tls-en-ingress)
7. [Verificar certificados](#7-verificar-certificados)
8. [Renovación automática](#8-renovación-automática)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Conceptos: DNS y SSL/TLS

### ¿Qué es DNS?

**DNS** (Domain Name System) traduce nombres de dominio a direcciones IP.

```
okla.com.do  →  DNS  →  146.190.199.0 (LoadBalancer IP)
```

**Sin DNS:**
```
https://146.190.199.0  ❌ Los navegadores muestran advertencias
```

**Con DNS:**
```
https://okla.com.do  ✅ Funciona perfectamente
```

### ¿Qué es SSL/TLS?

**SSL/TLS** cifra la comunicación entre el cliente y el servidor.

```
HTTP  → Sin cifrado → ❌ Inseguro
HTTPS → Con cifrado → ✅ Seguro
```

**Certificado SSL:**
- Emitido por una Autoridad Certificadora (CA)
- Verifica la identidad del sitio web
- Habilita HTTPS (candado verde 🔒)

### Let's Encrypt

**Let's Encrypt** es una CA que proporciona certificados SSL **GRATIS**.

**Características:**
- ✅ Gratuito
- ✅ Automático
- ✅ Renovación automática cada 90 días
- ✅ Confiable (aceptado por todos los navegadores)

### cert-manager

**cert-manager** es un operador de Kubernetes que:
- Solicita certificados a Let's Encrypt automáticamente
- Los renueva antes de que expiren
- Los almacena como Secrets de Kubernetes
- Los actualiza en el Ingress

```
Ingress → cert-manager → Let's Encrypt → Certificado SSL → HTTPS ✅
```

---

## 2. DNS interno de Kubernetes

### CoreDNS

Kubernetes tiene su propio servidor DNS interno llamado **CoreDNS**.

```bash
# Ver pods de CoreDNS
kubectl get pods -n kube-system -l k8s-app=kube-dns
```

**Salida:**
```
NAME                       READY   STATUS    RESTARTS   AGE
coredns-xxx                1/1     Running   0          7d
coredns-yyy                1/1     Running   0          7d
```

### Formato de DNS interno

Los servicios dentro del cluster se resuelven con este patrón:

```
<service-name>.<namespace>.svc.cluster.local
```

**Ejemplos del proyecto OKLA:**
```
vehiclessaleservice.okla.svc.cluster.local
authservice.okla.svc.cluster.local
postgres.okla.svc.cluster.local
```

**Forma corta (dentro del mismo namespace):**
```
vehiclessaleservice
authservice
postgres
```

### Probar DNS interno

```bash
# Entrar a cualquier pod
kubectl exec -it deployment/gateway -n okla -- sh

# Instalar herramientas de DNS
apk add bind-tools

# Resolver nombre corto
nslookup vehiclessaleservice

# Resolver nombre completo (FQDN)
nslookup vehiclessaleservice.okla.svc.cluster.local

# Salir
exit
```

**Salida esperada:**
```
Server:    10.245.0.10
Address 1: 10.245.0.10 kube-dns.kube-system.svc.cluster.local

Name:      vehiclessaleservice
Address 1: 10.116.49.128 vehiclessaleservice.okla.svc.cluster.local
```

### Ver configuración de CoreDNS

```bash
kubectl get configmap coredns -n kube-system -o yaml
```

---

## 3. Configurar DNS externo

### Paso 1: Obtener IP del LoadBalancer

```bash
kubectl get svc -n ingress-nginx
```

**Salida:**
```
NAME                                 TYPE           EXTERNAL-IP
ingress-nginx-controller             LoadBalancer   146.190.199.0
```

**Tu IP pública:** `146.190.199.0`

### Paso 2: Configurar registros A en tu proveedor DNS

Ve a tu proveedor de dominios (GoDaddy, Namecheap, etc.) y crea estos registros:

| Tipo | Host | Valor | TTL |
|------|------|-------|-----|
| A | @ | 146.190.199.0 | 3600 |
| A | www | 146.190.199.0 | 3600 |
| A | api | 146.190.199.0 | 3600 |

**¿Qué significa cada registro?**
- `@` → okla.com.do
- `www` → www.okla.com.do
- `api` → api.okla.com.do

**Todos apuntan a la misma IP** del LoadBalancer. El Ingress se encarga de rutear por hostname.

### Paso 3: Verificar propagación de DNS

```bash
# Verificar desde tu máquina
dig okla.com.do +short
# Debería retornar: 146.190.199.0

dig api.okla.com.do +short
# Debería retornar: 146.190.199.0

# Alternativa con nslookup
nslookup okla.com.do
nslookup api.okla.com.do
```

**⚠️ Nota:** La propagación DNS puede tomar de 5 minutos a 48 horas (usualmente 1-2 horas).

### Paso 4: Probar conectividad HTTP (sin HTTPS aún)

```bash
curl -H "Host: okla.com.do" http://146.190.199.0
```

Si funciona, DNS está listo. Ahora vamos por SSL.

---

## 4. Instalar cert-manager

### ¿Por qué cert-manager?

cert-manager automatiza la obtención y renovación de certificados SSL. Sin él, tendrías que:
1. Generar CSR manualmente
2. Enviar a Let's Encrypt
3. Validar dominio
4. Descargar certificado
5. Renovar cada 90 días

Con cert-manager: **TODO ES AUTOMÁTICO**.

### Método 1: Usando kubectl (recomendado)

```bash
# Instalar cert-manager v1.13.x (última versión estable)
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.3/cert-manager.yaml
```

**Salida:**
```
namespace/cert-manager created
customresourcedefinition.apiextensions.k8s.io/certificaterequests.cert-manager.io created
customresourcedefinition.apiextensions.k8s.io/certificates.cert-manager.io created
...
deployment.apps/cert-manager created
deployment.apps/cert-manager-cainjector created
deployment.apps/cert-manager-webhook created
```

### Verificar instalación

```bash
# Ver namespace de cert-manager
kubectl get ns cert-manager

# Ver pods de cert-manager
kubectl get pods -n cert-manager
```

**Salida esperada:**
```
NAME                                      READY   STATUS    RESTARTS   AGE
cert-manager-xxx                          1/1     Running   0          2m
cert-manager-cainjector-yyy               1/1     Running   0          2m
cert-manager-webhook-zzz                  1/1     Running   0          2m
```

**Todos deben estar Running.**

### Ver CRDs (Custom Resource Definitions) creados

```bash
kubectl get crd | grep cert-manager
```

**Salida:**
```
certificaterequests.cert-manager.io
certificates.cert-manager.io
challenges.acme.cert-manager.io
clusterissuers.cert-manager.io
issuers.cert-manager.io
orders.acme.cert-manager.io
```

Estos son los recursos nuevos que cert-manager añade a Kubernetes.

---

## 5. Crear ClusterIssuer (Let's Encrypt)

### ¿Qué es un ClusterIssuer?

Un **ClusterIssuer** le dice a cert-manager:
- Dónde solicitar certificados (Let's Encrypt)
- Qué método de validación usar (HTTP-01 o DNS-01)
- Correo electrónico para notificaciones

### Crear archivo `clusterissuer.yaml`

```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    # Email para notificaciones de expiración
    email: tu-email@example.com
    
    # Servidor de Let's Encrypt (producción)
    server: https://acme-v02.api.letsencrypt.org/directory
    
    # Secret donde se guarda la clave privada del ACME account
    privateKeySecretRef:
      name: letsencrypt-prod
    
    # Validación HTTP-01 (más simple)
    solvers:
    - http01:
        ingress:
          class: nginx
```

**⚠️ Cambiar `tu-email@example.com` por tu correo real.**

### Aplicar el ClusterIssuer

```bash
kubectl apply -f clusterissuer.yaml
```

**Salida:**
```
clusterissuer.cert-manager.io/letsencrypt-prod created
```

### Verificar ClusterIssuer

```bash
kubectl get clusterissuer
```

**Salida:**
```
NAME               READY   AGE
letsencrypt-prod   True    30s
```

**READY debe ser True.**

### Ver detalles

```bash
kubectl describe clusterissuer letsencrypt-prod
```

**Buscar:**
```
Status:
  Acme:
    Last Registered Email:  tu-email@example.com
    Uri:                    https://acme-v02.api.letsencrypt.org/acme/acct/...
  Conditions:
    Status:  True
    Type:    Ready
```

---

## 6. Configurar TLS en Ingress

### Ingress actual (sin TLS)

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: okla-ingress
  namespace: okla
spec:
  ingressClassName: nginx
  rules:
  - host: okla.com.do
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: frontend-web
            port:
              number: 8080
```

### Ingress con TLS (HTTPS)

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: okla-ingress
  namespace: okla
  annotations:
    # Decirle a cert-manager que genere certificado
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    # Redirigir HTTP a HTTPS
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - okla.com.do
    - www.okla.com.do
    - api.okla.com.do
    secretName: okla-tls-cert  # Nombre del Secret donde se guarda el cert
  rules:
  - host: okla.com.do
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: frontend-web
            port:
              number: 8080
  - host: api.okla.com.do
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: gateway
            port:
              number: 8080
```

**Cambios clave:**
1. **Annotation `cert-manager.io/cluster-issuer`** - Activa cert-manager
2. **Sección `tls`** - Lista de hosts y nombre del Secret
3. **Annotation `ssl-redirect`** - Redirige HTTP → HTTPS

### Aplicar Ingress actualizado

```bash
kubectl apply -f k8s/ingress.yaml
```

**Salida:**
```
ingress.networking.k8s.io/okla-ingress configured
```

### Ver el proceso de obtención del certificado

```bash
# Ver Certificates
kubectl get certificate -n okla
```

**Salida (proceso en progreso):**
```
NAME            READY   SECRET          AGE
okla-tls-cert   False   okla-tls-cert   30s
```

**READY False** significa que aún no está listo.

```bash
# Ver el challenge (validación HTTP-01)
kubectl get challenge -n okla
```

**Salida:**
```
NAME                                    STATE     AGE
okla-tls-cert-xxx-0                     pending   45s
```

Let's Encrypt está verificando que controlas el dominio haciendo un request HTTP a:
```
http://okla.com.do/.well-known/acme-challenge/<token>
```

cert-manager crea un pod temporal que responde a este request.

### Esperar 1-2 minutos y verificar nuevamente

```bash
kubectl get certificate -n okla
```

**Salida (éxito):**
```
NAME            READY   SECRET          AGE
okla-tls-cert   True    okla-tls-cert   2m
```

**READY True** - ¡Certificado obtenido! 🎉

---

## 7. Verificar certificados

### Ver el Secret con el certificado

```bash
kubectl get secret okla-tls-cert -n okla
```

**Salida:**
```
NAME            TYPE                DATA   AGE
okla-tls-cert   kubernetes.io/tls   2      5m
```

**DATA 2** significa que tiene:
1. `tls.crt` - Certificado público
2. `tls.key` - Clave privada

### Ver detalles del certificado

```bash
kubectl describe certificate okla-tls-cert -n okla
```

**Buscar:**
```
Status:
  Conditions:
    Status:                True
    Type:                  Ready
  Not After:              2026-04-07T12:00:00Z  # Expira en 90 días
  Not Before:             2026-01-07T12:00:00Z  # Válido desde hoy
  Renewal Time:           2026-03-09T12:00:00Z  # Se renueva automáticamente
```

### Ver el certificado decodificado

```bash
kubectl get secret okla-tls-cert -n okla -o jsonpath='{.data.tls\.crt}' | base64 -d | openssl x509 -noout -text
```

**Salida (extracto):**
```
Certificate:
    Data:
        Version: 3 (0x2)
        Serial Number: xxx
    Signature Algorithm: sha256WithRSAEncryption
        Issuer: C = US, O = Let's Encrypt, CN = R3
        Validity
            Not Before: Jan  7 12:00:00 2026 GMT
            Not After : Apr  7 12:00:00 2026 GMT
        Subject: CN = okla.com.do
        Subject Alternative Name:
            DNS:api.okla.com.do, DNS:okla.com.do, DNS:www.okla.com.do
```

**Issuer: Let's Encrypt** - Certificado válido.

### Probar HTTPS desde el navegador

```
https://okla.com.do
https://api.okla.com.do
```

Deberías ver el **candado verde 🔒** en la barra de direcciones.

### Probar con curl

```bash
curl -I https://okla.com.do
```

**Salida:**
```
HTTP/2 200
server: nginx
date: Tue, 07 Jan 2026 12:00:00 GMT
...
```

**HTTP/2** indica que HTTPS está funcionando.

### Verificar redirección HTTP → HTTPS

```bash
curl -I http://okla.com.do
```

**Salida:**
```
HTTP/1.1 308 Permanent Redirect
Location: https://okla.com.do/
```

**308 Redirect** - HTTP redirige automáticamente a HTTPS ✅

---

## 8. Renovación automática

### ¿Cuándo se renuevan los certificados?

Let's Encrypt emite certificados válidos por **90 días**.

cert-manager los renueva automáticamente **30 días antes de expirar** (es decir, a los 60 días).

### Ver cuándo expira tu certificado

```bash
kubectl describe certificate okla-tls-cert -n okla | grep "Not After"
```

**Salida:**
```
Not After: 2026-04-07T12:00:00Z
```

### Ver eventos de renovación

```bash
kubectl get events -n okla --sort-by='.lastTimestamp' | grep -i cert
```

**Eventos típicos:**
```
cert-manager: Certificate issued successfully
cert-manager: Certificate is up to date
```

### Forzar renovación manual (para pruebas)

```bash
# Eliminar el Secret del certificado
kubectl delete secret okla-tls-cert -n okla

# cert-manager detectará que falta y lo regenerará
kubectl get certificate -n okla -w
```

**Verás:**
```
NAME            READY   SECRET          AGE
okla-tls-cert   False   okla-tls-cert   5s
okla-tls-cert   True    okla-tls-cert   45s
```

### Ver logs de cert-manager

```bash
kubectl logs -f deployment/cert-manager -n cert-manager
```

**Buscar:**
```
"msg"="certificate renewed successfully"
"resource_name"="okla-tls-cert"
```

---

## 9. Cheat Sheet

### Comandos DNS

| Comando | Descripción |
|---------|-------------|
| `dig okla.com.do +short` | Resolver dominio |
| `nslookup okla.com.do` | Resolver dominio (alternativa) |
| `kubectl get svc -n ingress-nginx` | Obtener IP del LoadBalancer |

### Comandos cert-manager

| Comando | Descripción |
|---------|-------------|
| `kubectl get pods -n cert-manager` | Ver pods de cert-manager |
| `kubectl get clusterissuer` | Ver ClusterIssuers |
| `kubectl describe clusterissuer <nombre>` | Detalles del ClusterIssuer |
| `kubectl get certificate -n okla` | Ver certificados |
| `kubectl describe certificate <nombre> -n okla` | Detalles del certificado |
| `kubectl get challenge -n okla` | Ver challenges (validación) |
| `kubectl get secret <nombre>-tls -n okla` | Ver Secret del certificado |

### Debugging TLS

| Comando | Descripción |
|---------|-------------|
| `curl -I https://okla.com.do` | Probar HTTPS |
| `curl -Ik https://okla.com.do` | Probar HTTPS (ignorar cert) |
| `openssl s_client -connect okla.com.do:443 -servername okla.com.do` | Ver certificado SSL |
| `kubectl logs -f deployment/cert-manager -n cert-manager` | Logs de cert-manager |

---

## 10. Ejercicios prácticos

### Ejercicio 1: Verificar DNS interno

1. Entra al pod del gateway
2. Resuelve el nombre `vehiclessaleservice`
3. Resuelve el nombre completo `vehiclessaleservice.okla.svc.cluster.local`

<details>
<summary>Solución</summary>

```bash
# 1
kubectl exec -it deployment/gateway -n okla -- sh

# 2
nslookup vehiclessaleservice

# 3
nslookup vehiclessaleservice.okla.svc.cluster.local

exit
```
</details>

### Ejercicio 2: Verificar IP del LoadBalancer

1. Obtén la IP externa del LoadBalancer de ingress-nginx
2. Verifica que tu dominio apunta a esa IP con `dig`

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get svc -n ingress-nginx -o wide

# 2
dig okla.com.do +short
# Debe retornar la misma IP del paso 1
```
</details>

### Ejercicio 3: Verificar cert-manager

1. Lista los pods de cert-manager
2. Verifica que estén Running
3. Ve los ClusterIssuers

<details>
<summary>Solución</summary>

```bash
# 1 y 2
kubectl get pods -n cert-manager

# 3
kubectl get clusterissuer
```
</details>

### Ejercicio 4: Inspeccionar certificado

1. Lista los certificados en el namespace okla
2. Ve los detalles del certificado `okla-tls-cert`
3. Verifica la fecha de expiración

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get certificate -n okla

# 2
kubectl describe certificate okla-tls-cert -n okla

# 3
kubectl describe certificate okla-tls-cert -n okla | grep "Not After"
```
</details>

### Ejercicio 5: Probar HTTPS

1. Prueba HTTPS con curl a https://api.okla.com.do/health
2. Verifica que HTTP redirige a HTTPS

<details>
<summary>Solución</summary>

```bash
# 1
curl -I https://api.okla.com.do/health

# 2
curl -I http://api.okla.com.do/health
# Debe retornar 308 Permanent Redirect
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 5. Ahora sabes:
- ✅ Cómo funciona DNS interno en Kubernetes
- ✅ Configurar registros DNS externos
- ✅ Instalar cert-manager
- ✅ Crear ClusterIssuer para Let's Encrypt
- ✅ Obtener certificados SSL automáticamente
- ✅ Configurar TLS en Ingress
- ✅ Verificar y debugging de certificados

---

**Anterior:** [04 - Logs y Debugging](./04-logs-debugging.md)  
**Siguiente:** [06 - LoadBalancer y Networking](./06-loadbalancer-networking.md)

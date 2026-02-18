#!/bin/bash
# =============================================================================
# Generate K8s manifests for all missing services
# Excludes: SpyneIntegrationService, ComplianceService (future)
# =============================================================================
set -e

BASE_DIR="$(cd "$(dirname "$0")/.." && pwd)"
K8S_DIR="$BASE_DIR/k8s"

# All 28 new services to add (k8s_name:description)
SERVICES=(
  "aiprocessingservice:AI Processing"
  "alertservice:Alerts"
  "apidocsservice:API Documentation"
  "appointmentservice:Appointments"
  "backgroundremovalservice:Background Removal"
  "cacheservice:Cache"
  "comparisonservice:Comparison"
  "configurationservice:Configuration"
  "contactservice:Contact"
  "crmservice:CRM"
  "dataprotectionservice:Data Protection"
  "dealeranalyticsservice:Dealer Analytics"
  "dealermanagementservice:Dealer Management"
  "integrationservice:Integration"
  "inventorymanagementservice:Inventory Management"
  "leadscoringservice:Lead Scoring"
  "maintenanceservice:Maintenance"
  "marketingservice:Marketing"
  "messagebusservice:Message Bus"
  "paymentservice:Payment"
  "ratelimitingservice:Rate Limiting"
  "recommendationservice:Recommendation"
  "reportsservice:Reports"
  "schedulerservice:Scheduler"
  "servicediscovery:Service Discovery"
  "staffservice:Staff"
  "vehicle360processingservice:Vehicle 360 Processing"
  "vehicleintelligenceservice:Vehicle Intelligence"
)

echo "🚀 Generating K8s manifests for ${#SERVICES[@]} services..."

# ============================================
# 1. DEPLOYMENTS
# ============================================
echo "📦 Adding deployments..."
for entry in "${SERVICES[@]}"; do
  k8s_name="${entry%%:*}"
  desc="${entry#*:}"
  upper_desc=$(echo "$desc" | tr '[:lower:]' '[:upper:]')
  
  cat >> "$K8S_DIR/deployments.yaml" << YAMLDEPLOY

---
# =============================================================================
# ${upper_desc} SERVICE
# =============================================================================
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ${k8s_name}
  namespace: okla
  labels:
    app: ${k8s_name}
    tier: backend
spec:
  replicas: 1
  selector:
    matchLabels:
      app: ${k8s_name}
  template:
    metadata:
      labels:
        app: ${k8s_name}
    spec:
      serviceAccountName: okla-backend
      automountServiceAccountToken: false
      topologySpreadConstraints:
        - maxSkew: 1
          topologyKey: kubernetes.io/hostname
          whenUnsatisfiable: ScheduleAnyway
          labelSelector:
            matchLabels:
              app: ${k8s_name}
      imagePullSecrets:
        - name: registry-credentials
      containers:
        - name: ${k8s_name}
          image: ghcr.io/gregorymorenoiem/${k8s_name}:latest
          ports:
            - containerPort: 8080
          env:
            - name: ASPNETCORE_URLS
              value: "http://+:8080"
          envFrom:
            - configMapRef:
                name: global-config
            - secretRef:
                name: ${k8s_name}-db-secret
            - secretRef:
                name: jwt-secrets
            - secretRef:
                name: redis-secrets
            - secretRef:
                name: rabbitmq-secrets
          resources:
            requests:
              memory: "128Mi"
              cpu: "50m"
            limits:
              memory: "256Mi"
              cpu: "200m"
          securityContext:
            runAsNonRoot: true
            runAsUser: 1000
            allowPrivilegeEscalation: false
            readOnlyRootFilesystem: true
            capabilities:
              drop:
                - ALL
          startupProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 10
            periodSeconds: 5
            failureThreshold: 24
          livenessProbe:
            httpGet:
              path: /health
              port: 8080
            periodSeconds: 30
            timeoutSeconds: 5
            failureThreshold: 3
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            periodSeconds: 10
            timeoutSeconds: 5
            failureThreshold: 3
          volumeMounts:
            - name: tmp
              mountPath: /tmp
      volumes:
        - name: tmp
          emptyDir: {}
YAMLDEPLOY
done

echo "✅ Deployments added: $(grep -c 'kind: Deployment' "$K8S_DIR/deployments.yaml") total"

# ============================================
# 2. SERVICES
# ============================================
echo "🔌 Adding services..."
for entry in "${SERVICES[@]}"; do
  k8s_name="${entry%%:*}"
  
  cat >> "$K8S_DIR/services.yaml" << YAMLSVC

---
apiVersion: v1
kind: Service
metadata:
  name: ${k8s_name}
  namespace: okla
spec:
  type: ClusterIP
  ports:
    - port: 8080
      targetPort: 8080
  selector:
    app: ${k8s_name}
YAMLSVC
done

echo "✅ Services added: $(grep -c 'kind: Service' "$K8S_DIR/services.yaml") total"

# ============================================
# 3. DB SECRETS
# ============================================
echo "🔐 Adding db-secrets..."
for entry in "${SERVICES[@]}"; do
  k8s_name="${entry%%:*}"
  
  # Determine DB name (most match k8s name, some differ)
  db_name="${k8s_name}"
  
  cat >> "$K8S_DIR/secrets.template.yaml" << YAMLSEC

---
apiVersion: v1
kind: Secret
metadata:
  name: ${k8s_name}-db-secret
  namespace: okla
type: Opaque
stringData:
  ConnectionStrings__DefaultConnection: "Host=\${POSTGRES_HOST};Port=\${POSTGRES_PORT};Database=${db_name};Username=\${POSTGRES_USER};Password=\${POSTGRES_PASSWORD};SSL Mode=Require;Trust Server Certificate=true"
  ConnectionStrings__RedisConnection: "redis://:\${REDIS_PASSWORD}@redis:6379"
YAMLSEC
done

echo "✅ Secrets added: $(grep -c 'kind: Secret' "$K8S_DIR/secrets.template.yaml") total"

echo ""
echo "🎉 All K8s manifests generated successfully!"
echo "   Deployments: $(grep -c 'kind: Deployment' "$K8S_DIR/deployments.yaml")"
echo "   Services:    $(grep -c 'kind: Service' "$K8S_DIR/services.yaml")"
echo "   Secrets:     $(grep -c 'kind: Secret' "$K8S_DIR/secrets.template.yaml")"

#!/bin/bash
# =============================================================================
# MVP SELF-MANAGED - Deploy Script
# =============================================================================
# Uso: ./deploy-mvp.sh
# =============================================================================

set -e

echo "🚀 Deploying MVP Self-Managed Stack..."

# Verificar kubectl está conectado
echo "📡 Verificando conexión al cluster..."
kubectl cluster-info

# Crear namespace si no existe
echo "📁 Creando namespace okla..."
kubectl apply -f namespace.yaml

# Crear registry credentials (necesitas ejecutar esto antes manualmente)
echo "🔐 Verificando registry credentials..."
if ! kubectl get secret registry-credentials -n okla > /dev/null 2>&1; then
  echo "⚠️  Registry credentials no encontrado!"
  echo "   Ejecuta primero:"
  echo "   kubectl create secret docker-registry registry-credentials \\"
  echo "     --docker-server=ghcr.io \\"
  echo "     --docker-username=TU_USUARIO \\"
  echo "     --docker-password=TU_GITHUB_TOKEN \\"
  echo "     -n okla"
  exit 1
fi

# Aplicar secrets
echo "🔑 Aplicando secrets..."
kubectl apply -f secrets.yaml

# Aplicar configmaps
echo "⚙️  Aplicando configmaps..."
kubectl apply -f configmaps.yaml

# Aplicar infraestructura (PostgreSQL y Redis)
echo "🗄️  Desplegando PostgreSQL y Redis..."
kubectl apply -f infrastructure.yaml

# Esperar a que PostgreSQL esté listo
echo "⏳ Esperando a PostgreSQL (esto puede tomar 1-2 minutos)..."
kubectl rollout status statefulset/postgres -n okla --timeout=180s

# Esperar a que Redis esté listo
echo "⏳ Esperando a Redis..."
kubectl rollout status deployment/redis -n okla --timeout=60s

# Aplicar services
echo "🔗 Aplicando services..."
kubectl apply -f services.yaml

# Aplicar deployments
echo "📦 Desplegando microservicios..."
kubectl apply -f deployments.yaml

# Aplicar ingress
echo "🌐 Aplicando ingress..."
kubectl apply -f ingress.yaml

# Mostrar estado
echo ""
echo "✅ Deploy completado!"
echo ""
echo "📊 Estado de pods:"
kubectl get pods -n okla

echo ""
echo "🔗 Services:"
kubectl get services -n okla

echo ""
echo "🌐 Ingress:"
kubectl get ingress -n okla

echo ""
echo "💡 Próximos pasos:"
echo "   1. Configurar DNS para apuntar a la IP del Load Balancer"
echo "   2. Verificar que todos los pods estén Running"
echo "   3. Probar: curl https://api.okla.com/health"

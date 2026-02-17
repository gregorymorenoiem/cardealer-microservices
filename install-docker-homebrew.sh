#!/bin/bash

echo "════════════════════════════════════════════════════════════════════════════════"
echo "                  🐳 INSTALANDO DOCKER DESKTOP VÍA HOMEBREW"
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""

# PASO 1: Matar procesos Docker
echo "📌 PASO 1: Deteniendo procesos Docker..."
killall -9 Docker 2>/dev/null || true
killall -9 "Docker.app" 2>/dev/null || true
killall -9 "com.docker.backend" 2>/dev/null || true
killall -9 "Docker Desktop" 2>/dev/null || true
sleep 3
echo "✓ Procesos detenidos"
echo ""

# PASO 2: Limpiar instalación vieja
echo "📌 PASO 2: Removiendo Docker viejo..."
rm -rf /Applications/Docker.app 2>/dev/null || true
rm -rf ~/.docker 2>/dev/null || true
rm -rf ~/Library/Containers/com.docker.docker 2>/dev/null || true
rm -rf ~/Library/Application\ Support/Docker\ Desktop 2>/dev/null || true
rm -rf ~/Library/Preferences/com.docker.docker* 2>/dev/null || true
echo "✓ Instalación vieja removida"
echo ""

# PASO 3: Verificar/Instalar Homebrew
echo "📌 PASO 3: Verificando Homebrew..."
if ! command -v brew &> /dev/null; then
  echo "  Instalando Homebrew..."
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
  echo ""
fi
echo "✓ Homebrew listo"
echo ""

# PASO 4: Instalar Docker Desktop
echo "📌 PASO 4: Instalando Docker Desktop (última versión)..."
echo "  ⏳ Esto puede tomar 5-10 minutos..."
echo ""

brew install --cask docker

if [ $? -eq 0 ]; then
  echo ""
  echo "✓ Docker Desktop instalado exitosamente"
else
  echo ""
  echo "✗ Error durante instalación"
  exit 1
fi

echo ""

# PASO 5: Iniciar Docker
echo "📌 PASO 5: Abriendo Docker Desktop..."
open -a Docker

echo "  ⏳ Esperando 120 segundos a que Docker inicie completamente..."
for i in {1..120}; do
  if docker ps &>/dev/null 2>&1; then
    echo ""
    echo "✅ ¡Docker está respondiendo en intento $i!"
    break
  fi
  
  if [ $((i % 30)) -eq 0 ]; then
    echo "    $i segundos..."
  fi
  
  sleep 1
done

echo ""

# PASO 6: Verificar instalación
echo "════════════════════════════════════════════════════════════════════════════════"
echo "📊 VERIFICANDO INSTALACIÓN"
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""

if docker ps &>/dev/null 2>&1; then
  echo "✅ Docker está funcionando!"
  echo ""
  docker version --format 'Docker Version: {{.Server.Version}}'
  echo ""
  docker compose version 2>/dev/null && echo "" || echo "  (docker compose aún se está inicializando)"
else
  echo "⚠️  Docker aún no responde completamente"
  echo "    Espera 1-2 minutos más e intenta: docker ps"
fi

echo ""
echo "════════════════════════════════════════════════════════════════════════════════"
echo "✅ INSTALACIÓN COMPLETADA"
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""

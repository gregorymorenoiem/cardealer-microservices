# ⚡ QUICK START - Frontend Development (30 segundos)

## 🚀 En 3 Pasos

```bash
# 1️⃣ Levantar servicios (30-45 segundos)
./compose-frontend.sh up

# 2️⃣ En otra terminal, desarrollar frontend
cd frontend/web && npm run dev

# 3️⃣ Abrir navegador
http://localhost:3000
```

**¡Listo! 🎉**

---

## 📱 API Gateway Disponible

```
http://localhost:18443
```

Todos los endpoints están disponibles via gateway:

```
GET    http://localhost:18443/api/auth/me
POST   http://localhost:18443/api/auth/login
GET    http://localhost:18443/api/vehicles
POST   http://localhost:18443/api/vehicles
```

---

## 🛑 Para Detener

```bash
./compose-frontend.sh down
```

---

## 🐛 Si Algo Falla

```bash
# Ver estado
./compose-frontend.sh status

# Ver logs
./compose-frontend.sh logs

# Reiniciar todo
./compose-frontend.sh restart

# Health check
./compose-frontend.sh health
```

---

## 📚 Más Información

- `FRONTEND_ONLY_SETUP_SUMMARY.md` - Resumen completo
- `COMPOSE_FRONTEND_ONLY_GUIDE.md` - Guía detallada
- `COMPOSE_COMPARISON.md` - Original vs. Frontend-only
- `./compose-frontend.sh help` - Comandos disponibles

---

**¡A Desarrollar!** 💻✨

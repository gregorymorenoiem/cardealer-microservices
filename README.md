# 🚗 CarDealer - Marketplace de Vehículos

Plataforma completa de marketplace para compra y venta de vehículos con arquitectura de microservicios.

## 📱 Aplicación Móvil (NUEVO)

### Sprint 1 ✅ COMPLETADO

La aplicación móvil Flutter está en desarrollo activo. Se ha completado el Sprint 1:

- ✅ Sistema de diseño completo (Material 3)
- ✅ 10+ componentes reutilizables
- ✅ 3 variantes de Vehicle Cards
- ✅ Internacionalización ES/EN
- ✅ Arquitectura Clean Architecture + BLoC

**Para comenzar:**

```powershell
# Desde la raíz del proyecto
.\setup-mobile.ps1
```

Ver documentación completa: [`mobile/SPRINT1_COMPLETION.md`](mobile/SPRINT1_COMPLETION.md)

---

## 🏗️ Backend - Microservicios

Health Check: GET https://localhost:8443/api/errors/health
Obtener errores: GET https://localhost:8443/api/errors
Crear error: POST https://localhost:8443/api/errors
Obtener estadísticas: GET https://localhost:8443/api/errors/stats
Swagger UI: https://localhost:8443/error-service/swagger/index.html

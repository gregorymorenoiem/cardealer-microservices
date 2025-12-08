# 📱 CarDealer Mobile - Guía Rápida

## 🚀 Inicio Rápido

### Opción 1: Script Automático (Recomendado)

```powershell
# Desde la raíz del proyecto
.\setup-mobile.ps1
```

### Opción 2: Manual

```bash
# Navegar al directorio mobile
cd mobile

# Instalar dependencias
flutter pub get

# Generar código
flutter pub run build_runner build --delete-conflicting-outputs

# Ejecutar app
flutter run
```

## 📋 Estado del Proyecto

### Sprint 1 - Sistema de Diseño ✅ COMPLETADO

- ✅ Theme System completo (colores, tipografía, espaciado)
- ✅ 10+ componentes reutilizables
- ✅ Cards de vehículos (3 variantes)
- ✅ Internacionalización ES/EN
- ✅ Arquitectura Clean Architecture + BLoC

### Sprint 2 - Autenticación 📝 PRÓXIMO

- Login / Register
- Account type selection
- Token storage
- BLoC state management

## 📱 Características

- **Plataformas:** iOS y Android
- **Framework:** Flutter 3.x
- **Arquitectura:** Clean Architecture
- **State Management:** BLoC Pattern
- **Internacionalización:** ES/EN
- **Temas:** Light mode (Dark mode preparado)

## 🎨 Componentes Disponibles

### Buttons
- CustomButton (primary, secondary, outline, text)

### Inputs
- CustomTextField (con validación)

### Vehicle Cards
- VehicleCard (lista vertical)
- VehicleCardHorizontal (scroll horizontal)
- VehicleCardGrid (grid 2 columnas)

### Feedback
- LoadingIndicator (circular, linear, shimmer)
- EmptyStateWidget
- ErrorWidget

## 📚 Documentación

Ver `mobile/SPRINT1_COMPLETION.md` para detalles completos del Sprint 1.

## 🔗 Enlaces

- [Plan de Desarrollo Completo](MOBILE_APP_DEVELOPMENT_PLAN.md)
- [Flutter Documentation](https://flutter.dev/docs)
- [BLoC Library](https://bloclibrary.dev)

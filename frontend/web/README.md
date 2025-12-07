# CarDealer Frontend - Arquitectura Multi-Diseño

Este directorio contiene tres diseños independientes del marketplace CarDealer, cada uno con su propia implementación completa y autónoma.

## 📁 Estructura

```
frontend/web/
├── okla/                    # Diseño moderno tipo marketplace (puerto 5173)
│   ├── src/
│   ├── public/
│   ├── package.json
│   └── README.md
│
├── original/                # Diseño clásico tradicional (puerto 5174)
│   ├── src/
│   ├── public/
│   ├── package.json
│   └── README.md
│
├── cardealer/               # Diseño futuro personalizado (puerto 5175)
│   └── README.md           # Carpeta vacía, reservada para futuro
│
└── shared-auth/             # Autenticación compartida entre diseños
    ├── src/
    │   ├── authService.ts
    │   └── index.ts
    ├── package.json
    └── README.md
```

## 🎨 Diseños Disponibles

### 1. Okla (Puerto 5173)
**Estado**: ✅ Implementado completamente
- Diseño moderno tipo AirBnB/Marketplace
- Vista de mapa con marcadores personalizados
- Filtros avanzados de búsqueda  
- Animaciones con Framer Motion
- Responsive design completo
- Internacionalización (i18n)

```bash
cd okla
npm install
npm run dev
```

### 2. Original (Puerto 5174)
**Estado**: ✅ Base implementada
- Diseño tradicional de compra/venta
- Navegación estándar
- Filtros básicos
- Vista de listado clásica

```bash
cd original
npm install
npm run dev
```

### 3. CarDealer (Puerto 5175)
**Estado**: 📝 Reservado para futuro
- Carpeta vacía
- Diseño personalizado futuro

```bash
cd cardealer
# Pendiente de implementación
```

## 🔐 Autenticación Compartida

Los tres diseños comparten un único sistema de autenticación ubicado en `shared-auth/`:

- **Login único**: Un usuario se autentica una vez
- **Redirección automática**: El backend determina qué diseño mostrar basado en `user.theme`
- **Token compartido**: El token JWT se almacena en localStorage y es accesible por todos los diseños
- **Estado sincronizado**: El estado de autenticación se mantiene entre diseños

### Flujo de Autenticación
1. Usuario ingresa credenciales en `/login`
2. `sharedAuthService` valida con el backend
3. Backend retorna `user` con campo `theme: 'okla' | 'original' | 'cardealer'`
4. `sharedAuthService` redirige automáticamente al puerto correcto
5. El diseño correspondiente carga con el usuario autenticado

## 🚀 Desarrollo

### Ejecutar todos los diseños simultáneamente
```bash
# Terminal 1 - Okla
cd okla && npm run dev

# Terminal 2 - Original  
cd original && npm run dev

# Terminal 3 - CarDealer (futuro)
cd cardealer && npm run dev
```

### Puertos
- **Okla**: http://localhost:5173
- **Original**: http://localhost:5174  
- **CarDealer**: http://localhost:5175

## 📦 Independencia Total

Cada diseño es **completamente independiente**:

✅ **Propio package.json** con sus dependencias
✅ **Propio src/** con todos los componentes
✅ **Propio public/** con assets
✅ **Propias configuraciones** (vite, tailwind, tsconfig, etc.)
✅ **No comparten código** excepto autenticación

### Lo que NO se comparte
❌ Componentes UI
❌ Páginas
❌ Layouts
❌ Estilos
❌ Assets
❌ Configuraciones
❌ Store/State
❌ Servicios (excepto auth)

### Lo que SÍ se comparte
✅ **Solo autenticación** (`shared-auth/`)

## 🔄 Migración desde estructura anterior

La estructura anterior tenía todo en `frontend/web/src/`. Ahora:

**Antes**:
```
frontend/web/src/
├── components/
├── pages/
├── services/
└── ...
```

**Después**:
```
frontend/web/
├── okla/src/          # Todo el código de Okla
├── original/src/      # Todo el código de Original  
├── cardealer/         # Vacío para futuro
└── shared-auth/       # Solo autenticación
```

## 🎯 Ventajas de esta arquitectura

1. **Independencia**: Cada diseño puede evolucionar sin afectar a los otros
2. **Mantenibilidad**: Código más organizado y fácil de mantener
3. **Escalabilidad**: Agregar nuevos diseños es simple (nueva carpeta)
4. **Testing**: Pruebas aisladas por diseño
5. **Deploy**: Cada diseño puede deployarse independientemente
6. **Desarrollo**: Equipos pueden trabajar en paralelo sin conflictos

## 📝 Próximos Pasos

1. ✅ Estructura de carpetas creada
2. ✅ Código Okla migrado completamente
3. ✅ Código Original migrado completamente
4. ✅ Sistema de autenticación compartida implementado
5. ⏳ Actualizar imports en Okla para usar shared-auth
6. ⏳ Actualizar imports en Original para usar shared-auth
7. ⏳ Implementar diseño CarDealer (futuro)
8. ⏳ Testing end-to-end del flujo de autenticación

## 🐛 Troubleshooting

### Problema: "Module not found" al importar shared-auth
**Solución**: Usar path relativo desde cada diseño
```typescript
// Desde okla/src/
import { sharedAuthService } from '../../../shared-auth/src';

// Desde original/src/
import { sharedAuthService } from '../../../shared-auth/src';
```

### Problema: Puertos ocupados
**Solución**: Cambiar puerto en package.json de cada diseño
```json
"dev": "vite --port XXXX"
```

## 📄 License

MIT

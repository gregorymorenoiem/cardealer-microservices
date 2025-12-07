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
└── cardealer/               # Diseño futuro personalizado (puerto 5175)
    └── README.md           # Carpeta vacía, reservada para futuro
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

## 📦 Independencia Total

Cada diseño es **completamente independiente**:

✅ **Propio package.json** con sus dependencias
✅ **Propio src/** con todos los componentes
✅ **Propio public/** con assets
✅ **Propias configuraciones** (vite, tailwind, tsconfig, etc.)
✅ **No comparten código** - Cada diseño funciona de forma autónoma

### Lo que NO se comparte
❌ Componentes UI
❌ Páginas
❌ Layouts
❌ Estilos
❌ Assets
❌ Configuraciones
❌ Store/State
❌ Servicios (incluido auth)
❌ Hooks
❌ Utils
❌ Types

### Resultado
✅ **Independencia 100%** - Tres aplicaciones completamente separadas

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
├── okla/src/          # Aplicación completa e independiente
├── original/src/      # Aplicación completa e independiente  
└── cardealer/         # Vacío para futuro
```

Cada carpeta (okla, original, cardealer) es una **aplicación React completamente independiente** con su propio:
- Sistema de autenticación
- Routing
- State management
- Componentes
- Estilos
- Configuraciones

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
4. ✅ Diseños funcionando independientemente
5. ⏳ Implementar diseño CarDealer (futuro)
6. ⏳ Testing individual de cada diseño

## 🐛 Troubleshooting

### Problema: Puertos ocupados
**Solución**: Cambiar puerto en package.json de cada diseño
```json
"dev": "vite --port XXXX"
```

### Problema: Conflictos entre diseños
**Solución**: No hay conflictos posibles - cada diseño es independiente

## 📄 License

MIT

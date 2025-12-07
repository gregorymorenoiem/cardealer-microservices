# Shared Authentication

Este módulo contiene la autenticación compartida entre todos los diseños del marketplace CarDealer.

## Propósito
Proporcionar un servicio de autenticación unificado que:
- Gestiona el login/logout de usuarios
- Almacena el token de autenticación
- Determina a qué diseño redirigir según el usuario
- Comparte el estado de autenticación entre diseños

## Uso

### En Okla
```typescript
import { sharedAuthService } from '../../../shared-auth/src';

const user = sharedAuthService.getCurrentUser();
```

### En Original
```typescript
import { sharedAuthService } from '../../../shared-auth/src';

const user = sharedAuthService.getCurrentUser();
```

### En CarDealer
```typescript
import { sharedAuthService } from '../../../shared-auth/src';

const user = sharedAuthService.getCurrentUser();
```

## Diseños Soportados
- **okla**: http://localhost:5173 - Diseño moderno tipo marketplace
- **original**: http://localhost:5174 - Diseño clásico
- **cardealer**: http://localhost:5175 - Diseño futuro personalizado

## Flujo de Login
1. Usuario ingresa credenciales en cualquier diseño
2. SharedAuthService valida con el backend
3. Backend retorna user.theme ('okla' | 'original' | 'cardealer')
4. SharedAuthService redirige al puerto/diseño correspondiente
5. El diseño carga con el usuario autenticado

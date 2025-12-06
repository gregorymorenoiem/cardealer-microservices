# CarDealer - Shared Package

Tipos TypeScript y store de autenticación compartidos entre web y mobile.

## Estructura

```
shared/
├── src/
│   ├── types/
│   │   └── index.ts    # Todos los tipos (AccountType, User, Vehicle, etc.)
│   ├── store/
│   │   └── authStore.ts # Zustand store con multi-role
│   └── index.ts        # Exports principales
```

## Uso en Web

```typescript
import { AccountType, User, Vehicle } from '../../../shared/src/types';
import { useAuthStore } from '../../../shared/src/store/authStore';

// Usar el store
const { user, isDealer, canAccessDealerPanel } = useAuthStore();

// Usar tipos
const user: User = {
  accountType: AccountType.DEALER,
  // ...
};
```

## Tipos Principales

- **AccountType**: GUEST | INDIVIDUAL | DEALER | ADMIN
- **DealerPlan**: BASIC | PRO | ENTERPRISE
- **User**: Usuario con accountType y dealer info
- **Vehicle**: Vehículo con campos específicos para dealers
- **DealerInfo**: Información del dealer
- **Subscription**: Suscripción con límites por plan

## AuthStore

Métodos disponibles:
- `login(response)` - Login con JWT
- `logout()` - Logout y limpiar storage
- `isDealer()` - Check si es dealer
- `canAccessDealerPanel()` - Check acceso a panel dealer
- `hasActiveSubscription()` - Check suscripción activa

# 🔐 Social Auth - Autenticación Social - Matriz de Procesos

> **Servicio:** AuthService  
> **Proveedores:** Google, Facebook, Apple  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente      | Total | Implementado | Pendiente | Estado |
| --------------- | ----- | ------------ | --------- | ------ |
| Controllers     | 1     | 1            | 0         | 🟢     |
| SAUTH-GOOGLE-\* | 4     | 4            | 0         | 🟢     |
| SAUTH-FB-\*     | 4     | 4            | 0         | 🟢     |
| SAUTH-APPLE-\*  | 4     | 4            | 0         | 🟢     |
| SAUTH-LINK-\*   | 3     | 3            | 0         | 🟢     |
| Tests           | 12    | 12           | 0         | ✅     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de autenticación social (OAuth 2.0 / OpenID Connect) que permite a los usuarios registrarse e iniciar sesión utilizando sus cuentas de Google, Facebook o Apple. Simplifica el proceso de onboarding y mejora la experiencia del usuario.

### 1.2 Proveedores Soportados

| Proveedor    | Protocolo          | Datos Obtenidos     | Estado    |
| ------------ | ------------------ | ------------------- | --------- |
| **Google**   | OAuth 2.0 + OIDC   | Email, nombre, foto | ✅ Activo |
| **Facebook** | OAuth 2.0          | Email, nombre, foto | ✅ Activo |
| **Apple**    | Sign in with Apple | Email, nombre       | ✅ Activo |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Social Auth Architecture                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   1. User clicks "Login with Google"                                    │
│   ┌─────────────┐                                                       │
│   │   Frontend  │ ────────────────────────────────────────────────┐    │
│   │    (React)  │                                                  │    │
│   └─────────────┘                                                  │    │
│         │                                                          │    │
│         │ 2. Redirect to provider                                  │    │
│         ▼                                                          │    │
│   ┌─────────────────────────────────────────────────────────┐     │    │
│   │                    OAuth Provider                        │     │    │
│   │            (Google/Facebook/Apple)                       │     │    │
│   │                                                          │     │    │
│   │   3. User authenticates with provider                    │     │    │
│   │   4. Provider returns auth code                          │     │    │
│   └────────────────────────────────────────────────────────┬─┘     │    │
│                                                            │       │    │
│         5. Redirect to callback with code                  │       │    │
│         ┌──────────────────────────────────────────────────┘       │    │
│         ▼                                                          │    │
│   ┌─────────────┐                                                  │    │
│   │   Frontend  │                                                  │    │
│   │  (callback) │                                                  │    │
│   └──────┬──────┘                                                  │    │
│          │                                                         │    │
│          │ 6. Send code to backend                                 │    │
│          ▼                                                         │    │
│   ┌─────────────────────────────────────────────────────────────┐ │    │
│   │                      AuthService                             │ │    │
│   │                                                              │ │    │
│   │   7. Exchange code for tokens                                │ │    │
│   │   8. Validate tokens with provider                           │ │    │
│   │   9. Get user info                                           │ │    │
│   │  10. Create/update user                                      │ │    │
│   │  11. Generate JWT                                            │ │    │
│   │                                                              │ │    │
│   └───────────────────────────┬─────────────────────────────────┘ │    │
│                               │                                    │    │
│                               │ 12. Return JWT + user info         │    │
│                               ▼                                    │    │
│   ┌─────────────┐◀────────────────────────────────────────────────┘    │
│   │   Frontend  │                                                       │
│   │ (logged in) │                                                       │
│   └─────────────┘                                                       │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

| Método   | Endpoint                                 | Descripción                    | Auth   |
| -------- | ---------------------------------------- | ------------------------------ | ------ |
| `GET`    | `/api/auth/external/providers`           | Listar proveedores disponibles | Public |
| `GET`    | `/api/auth/external/{provider}`          | Iniciar flujo OAuth            | Public |
| `GET`    | `/api/auth/external/{provider}/callback` | Callback del proveedor         | Public |
| `POST`   | `/api/auth/external/{provider}/token`    | Exchange code for tokens       | Public |
| `POST`   | `/api/auth/external/link`                | Vincular cuenta existente      | User   |
| `DELETE` | `/api/auth/external/unlink/{provider}`   | Desvincular proveedor          | User   |
| `GET`    | `/api/auth/external/linked`              | Proveedores vinculados         | User   |

---

## 3. Entidades

### 3.1 ExternalLogin

```csharp
public class ExternalLogin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public ExternalProvider Provider { get; set; }
    public string ProviderKey { get; set; } = string.Empty; // ID del usuario en el provider
    public string? ProviderEmail { get; set; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}

public enum ExternalProvider
{
    Google,
    Facebook,
    Apple
}
```

### 3.2 OAuthState

```csharp
public class OAuthState
{
    public Guid Id { get; set; }
    public string State { get; set; } = string.Empty; // CSRF token
    public string? CodeVerifier { get; set; } // PKCE

    public ExternalProvider Provider { get; set; }
    public string? ReturnUrl { get; set; }
    public Guid? LinkUserId { get; set; } // Si está vinculando cuenta

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; } // +10 minutos
    public bool IsUsed { get; set; }
}
```

### 3.3 ExternalUserInfo (DTO)

```csharp
public record ExternalUserInfo
{
    public string ProviderId { get; init; } = string.Empty;
    public ExternalProvider Provider { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? FullName { get; init; }
    public string? PictureUrl { get; init; }
    public string? Locale { get; init; }
}
```

---

## 4. Configuración por Proveedor

### 4.1 Google

```json
{
  "ExternalAuth": {
    "Google": {
      "Enabled": true,
      "ClientId": "${GOOGLE_CLIENT_ID}",
      "ClientSecret": "${GOOGLE_CLIENT_SECRET}",
      "AuthorizationEndpoint": "https://accounts.google.com/o/oauth2/v2/auth",
      "TokenEndpoint": "https://oauth2.googleapis.com/token",
      "UserInfoEndpoint": "https://www.googleapis.com/oauth2/v3/userinfo",
      "Scopes": ["openid", "email", "profile"],
      "UsePKCE": true
    }
  }
}
```

### 4.2 Facebook

```json
{
  "ExternalAuth": {
    "Facebook": {
      "Enabled": true,
      "AppId": "${FACEBOOK_APP_ID}",
      "AppSecret": "${FACEBOOK_APP_SECRET}",
      "AuthorizationEndpoint": "https://www.facebook.com/v18.0/dialog/oauth",
      "TokenEndpoint": "https://graph.facebook.com/v18.0/oauth/access_token",
      "UserInfoEndpoint": "https://graph.facebook.com/v18.0/me",
      "Scopes": ["email", "public_profile"],
      "Fields": ["id", "email", "first_name", "last_name", "picture"]
    }
  }
}
```

### 4.3 Apple

```json
{
  "ExternalAuth": {
    "Apple": {
      "Enabled": true,
      "ClientId": "${APPLE_CLIENT_ID}",
      "TeamId": "${APPLE_TEAM_ID}",
      "KeyId": "${APPLE_KEY_ID}",
      "PrivateKey": "${APPLE_PRIVATE_KEY}",
      "AuthorizationEndpoint": "https://appleid.apple.com/auth/authorize",
      "TokenEndpoint": "https://appleid.apple.com/auth/token",
      "Scopes": ["name", "email"],
      "ResponseMode": "form_post"
    }
  }
}
```

---

## 5. Procesos Detallados

### 5.1 SOCIAL-001: Login con Google

| Paso | Acción                                   | Sistema     | Validación         |
| ---- | ---------------------------------------- | ----------- | ------------------ |
| 1    | Usuario hace clic en "Login with Google" | Frontend    | -                  |
| 2    | Generar state y code_verifier (PKCE)     | AuthService | Valores aleatorios |
| 3    | Guardar OAuthState                       | AuthService | State guardado     |
| 4    | Redirigir a Google                       | Browser     | Redirect           |
| 5    | Usuario autoriza en Google               | Google      | User consents      |
| 6    | Google redirige con code                 | Browser     | Code received      |
| 7    | Frontend envía code al backend           | Frontend    | Code válido        |
| 8    | Verificar state                          | AuthService | State match        |
| 9    | Exchange code for tokens                 | Google      | Tokens received    |
| 10   | Obtener user info                        | Google      | Info received      |
| 11   | Buscar ExternalLogin existente           | AuthService | Exists?            |
| 12a  | Si existe: actualizar y login            | AuthService | User logged in     |
| 12b  | Si no existe: crear user + ExternalLogin | AuthService | User created       |
| 13   | Generar JWT                              | AuthService | Token issued       |
| 14   | Retornar JWT al frontend                 | API         | Login complete     |

```csharp
public class GoogleAuthService : IExternalAuthService
{
    public async Task<string> GetAuthorizationUrlAsync(
        string returnUrl,
        Guid? linkUserId = null,
        CancellationToken ct = default)
    {
        // 1. Generate PKCE verifier and challenge
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        // 2. Generate state (CSRF protection)
        var state = GenerateState();

        // 3. Store state
        var oauthState = new OAuthState
        {
            State = state,
            CodeVerifier = codeVerifier,
            Provider = ExternalProvider.Google,
            ReturnUrl = returnUrl,
            LinkUserId = linkUserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await _stateRepository.AddAsync(oauthState, ct);

        // 4. Build authorization URL
        var callbackUrl = $"{_config.BaseUrl}/api/auth/external/google/callback";

        var url = QueryHelpers.AddQueryString(_config.Google.AuthorizationEndpoint, new Dictionary<string, string?>
        {
            ["client_id"] = _config.Google.ClientId,
            ["redirect_uri"] = callbackUrl,
            ["response_type"] = "code",
            ["scope"] = string.Join(" ", _config.Google.Scopes),
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256",
            ["access_type"] = "offline",
            ["prompt"] = "select_account"
        });

        return url;
    }

    public async Task<AuthResult> HandleCallbackAsync(
        string code,
        string state,
        CancellationToken ct = default)
    {
        // 1. Verify state
        var oauthState = await _stateRepository.GetByStateAsync(state, ct);

        if (oauthState == null || oauthState.IsUsed || oauthState.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired state");

        // Mark as used
        oauthState.IsUsed = true;
        await _stateRepository.UpdateAsync(oauthState, ct);

        // 2. Exchange code for tokens
        var callbackUrl = $"{_config.BaseUrl}/api/auth/external/google/callback";

        var tokenResponse = await _httpClient.PostAsJsonAsync(_config.Google.TokenEndpoint, new
        {
            code,
            client_id = _config.Google.ClientId,
            client_secret = _config.Google.ClientSecret,
            redirect_uri = callbackUrl,
            grant_type = "authorization_code",
            code_verifier = oauthState.CodeVerifier
        }, ct);

        var tokens = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct);

        // 3. Get user info
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var userInfo = await _httpClient.GetFromJsonAsync<GoogleUserInfo>(
            _config.Google.UserInfoEndpoint, ct);

        // 4. Find or create user
        var externalLogin = await _externalLoginRepository
            .GetByProviderAsync(ExternalProvider.Google, userInfo!.Sub, ct);

        User user;

        if (externalLogin != null)
        {
            // Existing user - update tokens and login
            externalLogin.AccessToken = tokens.AccessToken;
            externalLogin.RefreshToken = tokens.RefreshToken;
            externalLogin.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn);
            externalLogin.LastLoginAt = DateTime.UtcNow;

            await _externalLoginRepository.UpdateAsync(externalLogin, ct);
            user = externalLogin.User;
        }
        else if (oauthState.LinkUserId.HasValue)
        {
            // Linking to existing user
            user = await _userRepository.GetByIdAsync(oauthState.LinkUserId.Value, ct)
                ?? throw new InvalidOperationException("User not found");

            await CreateExternalLoginAsync(user, userInfo, tokens, ct);
        }
        else
        {
            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(userInfo.Email, ct);

            if (existingUser != null)
            {
                // Email exists - link to existing account
                user = existingUser;
                await CreateExternalLoginAsync(user, userInfo, tokens, ct);
            }
            else
            {
                // Create new user
                user = new User
                {
                    Email = userInfo.Email,
                    EmailVerified = userInfo.EmailVerified,
                    FirstName = userInfo.GivenName,
                    LastName = userInfo.FamilyName,
                    AvatarUrl = userInfo.Picture,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user, ct);
                await CreateExternalLoginAsync(user, userInfo, tokens, ct);

                // Publish event
                await _eventBus.PublishAsync(new UserRegisteredEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    RegistrationMethod = "Google"
                }, ct);
            }
        }

        // 5. Generate JWT
        var jwt = await _tokenService.GenerateTokenAsync(user, ct);

        return new AuthResult
        {
            AccessToken = jwt.AccessToken,
            RefreshToken = jwt.RefreshToken,
            User = _mapper.Map<UserDto>(user)
        };
    }
}
```

### 5.2 SOCIAL-002: Sign in with Apple

| Paso | Acción                                    | Sistema     | Validación      |
| ---- | ----------------------------------------- | ----------- | --------------- |
| 1    | Usuario hace clic en "Sign in with Apple" | Frontend    | -               |
| 2    | Generar state                             | AuthService | State generado  |
| 3    | Redirigir a Apple                         | Browser     | Redirect        |
| 4    | Usuario autoriza en Apple                 | Apple       | User consents   |
| 5    | Apple hace POST a callback                | Apple       | form_post       |
| 6    | Recibir code + id_token                   | AuthService | Tokens received |
| 7    | Validar id_token                          | AuthService | Signature valid |
| 8    | Extraer user info del id_token            | AuthService | Info extracted  |
| 9    | Exchange code for access_token            | Apple       | Token received  |
| 10   | Buscar/crear usuario                      | AuthService | User ready      |
| 11   | Generar JWT                               | AuthService | Token issued    |

**Nota Apple:** El nombre del usuario solo se envía en el primer login. Debe guardarse inmediatamente.

```csharp
public class AppleAuthService : IExternalAuthService
{
    public async Task<AuthResult> HandleCallbackAsync(
        AppleCallbackRequest request,
        CancellationToken ct = default)
    {
        // 1. Verify state
        var oauthState = await VerifyStateAsync(request.State, ct);

        // 2. Validate id_token
        var handler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidIssuer = "https://appleid.apple.com",
            ValidAudience = _config.Apple.ClientId,
            IssuerSigningKeys = await GetApplePublicKeysAsync(ct)
        };

        var principal = handler.ValidateToken(request.IdToken, validationParams, out var validatedToken);
        var jwtToken = (JwtSecurityToken)validatedToken;

        // 3. Extract user info from id_token
        var sub = jwtToken.Subject; // Apple user ID
        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var emailVerified = jwtToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value == "true";

        // 4. Get user name from first login (if available)
        string? firstName = null, lastName = null;
        if (request.User != null)
        {
            var userJson = JsonDocument.Parse(request.User);
            firstName = userJson.RootElement.GetProperty("name").GetProperty("firstName").GetString();
            lastName = userJson.RootElement.GetProperty("name").GetProperty("lastName").GetString();
        }

        // 5. Exchange code for access_token
        var tokens = await ExchangeCodeAsync(request.Code, ct);

        // 6. Find or create user
        var externalLogin = await _externalLoginRepository
            .GetByProviderAsync(ExternalProvider.Apple, sub, ct);

        User user;

        if (externalLogin != null)
        {
            user = externalLogin.User;

            // Update tokens
            externalLogin.AccessToken = tokens.AccessToken;
            externalLogin.RefreshToken = tokens.RefreshToken;
            externalLogin.LastLoginAt = DateTime.UtcNow;
            await _externalLoginRepository.UpdateAsync(externalLogin, ct);
        }
        else
        {
            // New user or link
            var existingUser = email != null
                ? await _userRepository.GetByEmailAsync(email, ct)
                : null;

            if (existingUser != null)
            {
                user = existingUser;
            }
            else
            {
                user = new User
                {
                    Email = email ?? $"{sub}@privaterelay.appleid.com",
                    EmailVerified = emailVerified,
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user, ct);
            }

            // Create external login
            await CreateExternalLoginAsync(user, sub, email, tokens, ct);
        }

        // 7. Generate JWT
        var jwt = await _tokenService.GenerateTokenAsync(user, ct);

        return new AuthResult
        {
            AccessToken = jwt.AccessToken,
            RefreshToken = jwt.RefreshToken,
            User = _mapper.Map<UserDto>(user)
        };
    }
}
```

### 5.3 SOCIAL-003: Vincular Cuenta

| Paso | Acción                                     | Sistema     | Validación            |
| ---- | ------------------------------------------ | ----------- | --------------------- |
| 1    | Usuario autenticado va a Settings          | Frontend    | Logged in             |
| 2    | Click en "Vincular Google"                 | Frontend    | Provider selected     |
| 3    | Iniciar flujo OAuth con LinkUserId         | AuthService | State includes userId |
| 4    | Usuario autoriza                           | Provider    | Consent given         |
| 5    | Callback recibido                          | AuthService | Code received         |
| 6    | Verificar que email no está en otra cuenta | AuthService | No conflict           |
| 7    | Crear ExternalLogin para userId            | AuthService | Link created          |
| 8    | Confirmar vinculación                      | API         | Success response      |

---

## 6. Reglas de Negocio

| Código     | Regla                                      | Validación                 |
| ---------- | ------------------------------------------ | -------------------------- |
| SOCIAL-R01 | Email debe coincidir o ser nuevo           | No duplicar usuarios       |
| SOCIAL-R02 | Apple: guardar nombre en primer login      | Nombre solo viene una vez  |
| SOCIAL-R03 | State expira en 10 minutos                 | ExpiresAt check            |
| SOCIAL-R04 | PKCE obligatorio para Google               | code_challenge required    |
| SOCIAL-R05 | Un provider por usuario                    | No duplicate ExternalLogin |
| SOCIAL-R06 | Puede tener múltiples providers            | Google + Facebook allowed  |
| SOCIAL-R07 | Si tiene password, puede desvincular       | Mantener acceso            |
| SOCIAL-R08 | Si solo social, no puede desvincular todos | Al menos 1 método          |

---

## 7. Códigos de Error

| Código       | HTTP | Mensaje                        | Causa                   |
| ------------ | ---- | ------------------------------ | ----------------------- |
| `SOCIAL_001` | 400  | Invalid state                  | State no válido         |
| `SOCIAL_002` | 400  | State expired                  | State expirado          |
| `SOCIAL_003` | 400  | Provider error                 | Error del proveedor     |
| `SOCIAL_004` | 400  | Email already linked           | Email en otra cuenta    |
| `SOCIAL_005` | 400  | Provider already linked        | Ya tiene ese provider   |
| `SOCIAL_006` | 400  | Cannot unlink last auth method | Último método           |
| `SOCIAL_007` | 404  | Provider not found             | Provider no configurado |
| `SOCIAL_008` | 400  | Invalid token                  | Token inválido          |

---

## 8. Eventos RabbitMQ

| Evento                         | Exchange      | Descripción         |
| ------------------------------ | ------------- | ------------------- |
| `UserRegisteredViaSocialEvent` | `auth.events` | Registro via social |
| `SocialLoginEvent`             | `auth.events` | Login via social    |
| `SocialAccountLinkedEvent`     | `auth.events` | Cuenta vinculada    |
| `SocialAccountUnlinkedEvent`   | `auth.events` | Cuenta desvinculada |

---

## 9. Frontend Implementation

```typescript
// hooks/useSocialAuth.ts
export function useSocialAuth() {
  const [isLoading, setIsLoading] = useState(false);

  const loginWith = async (provider: 'google' | 'facebook' | 'apple') => {
    setIsLoading(true);

    try {
      // Get authorization URL from backend
      const response = await authApi.getExternalAuthUrl(provider, window.location.href);

      // Redirect to provider
      window.location.href = response.data.url;
    } catch (error) {
      setIsLoading(false);
      throw error;
    }
  };

  const handleCallback = async (provider: string, code: string, state: string) => {
    const response = await authApi.handleExternalCallback(provider, { code, state });

    // Store tokens
    localStorage.setItem('accessToken', response.data.accessToken);
    localStorage.setItem('refreshToken', response.data.refreshToken);

    return response.data.user;
  };

  return { loginWith, handleCallback, isLoading };
}

// components/SocialAuthButtons.tsx
export const SocialAuthButtons: React.FC = () => {
  const { loginWith, isLoading } = useSocialAuth();

  return (
    <div className="space-y-3">
      <button
        onClick={() => loginWith('google')}
        disabled={isLoading}
        className="w-full flex items-center justify-center gap-3 px-4 py-3 border rounded-lg hover:bg-gray-50"
      >
        <GoogleIcon className="w-5 h-5" />
        Continuar con Google
      </button>

      <button
        onClick={() => loginWith('facebook')}
        disabled={isLoading}
        className="w-full flex items-center justify-center gap-3 px-4 py-3 bg-[#1877F2] text-white rounded-lg hover:bg-[#166FE5]"
      >
        <FacebookIcon className="w-5 h-5" />
        Continuar con Facebook
      </button>

      <button
        onClick={() => loginWith('apple')}
        disabled={isLoading}
        className="w-full flex items-center justify-center gap-3 px-4 py-3 bg-black text-white rounded-lg hover:bg-gray-800"
      >
        <AppleIcon className="w-5 h-5" />
        Continuar con Apple
      </button>
    </div>
  );
};
```

---

## 10. Métricas Prometheus

```
# Social logins
social_logins_total{provider="google|facebook|apple", type="login|register"}

# Social auth errors
social_auth_errors_total{provider="...", error="..."}

# Linked accounts
social_accounts_linked_total{provider="..."}

# Auth method distribution
users_auth_method{method="password|google|facebook|apple"}
```

---

## 📚 Referencias

- [Google OAuth 2.0](https://developers.google.com/identity/protocols/oauth2) - Documentación
- [Facebook Login](https://developers.facebook.com/docs/facebook-login/) - Documentación
- [Sign in with Apple](https://developer.apple.com/sign-in-with-apple/) - Documentación
- [01-auth-service.md](../../01-AUTENTICACION-USUARIOS/01-auth-service.md) - AuthService

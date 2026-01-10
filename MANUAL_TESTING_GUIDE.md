# 🧪 Manual Testing Guide - Dealer Navigation Fix

## ✅ VERIFICACIÓN PASO A PASO

Abre tu browser en Docker y sigue estos pasos:

### 1️⃣ **Usuario Individual: individual@cardealer.com / Password123!**

1. Ve a `http://localhost:8080/login`
2. Login con: `individual@cardealer.com` / `Password123!`
3. **VERIFICAR:**
   - ✅ En navbar debería aparecer "Para Dealers"
   - ✅ Click en "Para Dealers" → debería ir a `/dealer/landing`
   - ✅ En dropdown del usuario NO debería ver badge "Dealer"
4. **TEST ADICIONAL:**
   - Ve manualmente a `http://localhost:8080/dealer/dashboard`
   - ✅ Debería redirigir a `/` (homepage) automáticamente

---

### 2️⃣ **Dealer Free: dealer.free@cardealer.com / Password123!**

1. Logout del usuario anterior
2. Login con: `dealer.free@cardealer.com` / `Password123!`
3. **VERIFICAR:**
   - ✅ En navbar debería aparecer "Mi Dashboard" (NO "Para Dealers")
   - ✅ Click en "Mi Dashboard" → debería ir a `/dealer/dashboard`
   - ✅ En dropdown del usuario DEBE ver badge "Dealer" verde
4. **TEST ADICIONAL:**
   - Ve manualmente a `http://localhost:8080/dealer/analytics`
   - ✅ Debería funcionar (no redirigir)

---

### 3️⃣ **Dealer Basic: dealer.basic@cardealer.com / Password123!**

1. Logout del usuario anterior
2. Login con: `dealer.basic@cardealer.com` / `Password123!`
3. **VERIFICAR:**
   - ✅ Comportamiento IDÉNTICO a Dealer Free
   - ✅ "Mi Dashboard" en navbar
   - ✅ Badge "Dealer" en dropdown
   - ✅ Acceso a `/dealer/dashboard`, `/dealer/analytics`, `/dealer/leads`

---

### 4️⃣ **Dealer Pro: dealer.pro@cardealer.com / Password123!**

1. Logout del usuario anterior
2. Login con: `dealer.pro@cardealer.com` / `Password123!`
3. **VERIFICAR:**
   - ✅ Comportamiento IDÉNTICO a otros dealers
   - ✅ "Mi Dashboard" en navbar
   - ✅ Badge "Dealer" en dropdown
   - ✅ Acceso completo a rutas de dealer

---

### 5️⃣ **Dealer Enterprise: dealer.enterprise@cardealer.com / Password123!**

1. Logout del usuario anterior
2. Login con: `dealer.enterprise@cardealer.com` / `Password123!`
3. **VERIFICAR:**
   - ✅ Comportamiento IDÉNTICO a otros dealers
   - ✅ "Mi Dashboard" en navbar
   - ✅ Badge "Dealer" en dropdown
   - ✅ Acceso completo a rutas de dealer

---

### 6️⃣ **Vendedor: seller@cardealer.com / Password123!**

1. Logout del usuario anterior
2. Login con: `seller@cardealer.com` / `Password123!`
3. **VERIFICAR:**
   - 🤔 **Depende del accountType en BD**
   - Si es `individual`: → "Para Dealers" en navbar
   - Si es `dealer_employee`: → "Mi Dashboard" en navbar + badge "Dealer"

---

## 🎯 CHECKLIST DE VERIFICACIÓN

### ✅ Para CADA dealer (Free, Basic, Pro, Enterprise):

- [ ] Navbar muestra "Mi Dashboard" (no "Para Dealers")
- [ ] Click "Mi Dashboard" → va a `/dealer/dashboard`
- [ ] Dropdown usuario → badge "Dealer" verde visible
- [ ] `/dealer/dashboard` → acceso OK
- [ ] `/dealer/analytics` → acceso OK
- [ ] `/dealer/leads` → acceso OK
- [ ] `/dealer/inventory` → acceso OK

### ✅ Para usuario individual:

- [ ] Navbar muestra "Para Dealers"
- [ ] Click "Para Dealers" → va a `/dealer/landing`
- [ ] Dropdown usuario → NO badge "Dealer"
- [ ] `/dealer/dashboard` → redirige a `/`
- [ ] `/dealer/analytics` → redirige a `/`

---

## 🚀 COMANDOS DE VERIFICACIÓN RÁPIDA

Si tienes acceso al terminal de Docker:

```bash
# Ver qué usuario está loggeado
curl -H "Cookie: your-auth-cookie" http://localhost:8080/api/auth/me

# Verificar acceso dealer (debería funcionar para dealers)
curl -H "Cookie: your-auth-cookie" http://localhost:8080/dealer/dashboard

# Verificar acceso dealer (debería redirigir para individuals)
curl -I -H "Cookie: your-auth-cookie" http://localhost:8080/dealer/dashboard
```

---

## ❓ SI ALGO NO FUNCIONA

**Posibles problemas:**

1. **`accountType` incorrecto en BD** → Verificar tabla users
2. **Token JWT no incluye accountType** → Verificar AuthService
3. **Cache del browser** → Ctrl+F5 para hard refresh
4. **Docker no actualizado** → Rebuild containers

---

**¡Prueba cada usuario y dime qué observas!** 🧪

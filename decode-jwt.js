#!/usr/bin/env node

// Script simple para decodificar JWT
const token = process.argv[2];

if (!token) {
  console.log("❌ Uso: node decode-jwt.js <token>");
  console.log("");
  console.log("O desde tu navegador:");
  console.log("1. Abre DevTools (F12)");
  console.log("2. Ve a Console");
  console.log('3. Ejecuta: localStorage.getItem("okla_access_token")');
  console.log("4. Copia el token y ejecútalo: node decode-jwt.js <token>");
  process.exit(1);
}

try {
  const parts = token.split(".");
  if (parts.length !== 3) {
    console.log("❌ Token inválido (debe tener 3 partes separadas por punto)");
    process.exit(1);
  }

  const payload = JSON.parse(Buffer.from(parts[1], "base64").toString());

  console.log("\n🔑 JWT Payload Decodificado:\n");
  console.log(JSON.stringify(payload, null, 2));

  console.log("\n📋 Información Clave:\n");
  console.log(
    `User ID: ${payload.sub || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || "N/A"}`,
  );
  console.log(
    `Email: ${payload.email || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || "N/A"}`,
  );
  console.log(
    `Role: ${payload.role || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || "N/A"}`,
  );
  console.log(`Account Type: ${payload.account_type || "N/A"}`);
  console.log(
    `Email Verified: ${payload.email_verified || payload.emailVerified || "N/A"}`,
  );

  const exp = payload.exp ? new Date(payload.exp * 1000) : null;
  const iat = payload.iat ? new Date(payload.iat * 1000) : null;
  const now = new Date();

  console.log(`\n⏰ Timestamps:\n`);
  console.log(`Issued At: ${iat ? iat.toLocaleString() : "N/A"}`);
  console.log(`Expires At: ${exp ? exp.toLocaleString() : "N/A"}`);
  console.log(`Current Time: ${now.toLocaleString()}`);
  console.log(`Is Expired: ${exp ? (exp < now ? "❌ YES" : "✅ NO") : "N/A"}`);
} catch (error) {
  console.log("❌ Error decodificando token:", error.message);
  process.exit(1);
}

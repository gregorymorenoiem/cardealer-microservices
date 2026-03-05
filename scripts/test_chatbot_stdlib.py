#!/usr/bin/env python3
"""
OKLA Chatbot Agent Testing Script (stdlib only - no pip required)
Tests 100 questions per agent mode against production API.
"""

import json
import time
import sys
import csv
import os
import ssl
import urllib.request
import urllib.error
import http.cookiejar
from datetime import datetime

BASE_URL = "https://okla.com.do/api"
RESULTS_DIR = "/tmp/okla-chatbot-tests"
os.makedirs(RESULTS_DIR, exist_ok=True)

CSRF_TOKEN = "abcdef0123456789abcdef0123456789"

# SSL context for HTTPS
ssl_ctx = ssl.create_default_context()


def http_post(url, data, headers=None, timeout=30):
    """Make an HTTP POST request using stdlib."""
    body = json.dumps(data).encode("utf-8")
    hdrs = {
        "Content-Type": "application/json",
        "X-CSRF-Token": CSRF_TOKEN,
        "Cookie": f"csrf_token={CSRF_TOKEN}",
    }
    if headers:
        hdrs.update(headers)
    
    req = urllib.request.Request(url, data=body, headers=hdrs, method="POST")
    try:
        with urllib.request.urlopen(req, timeout=timeout, context=ssl_ctx) as resp:
            return resp.status, json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        try:
            body = json.loads(e.read().decode("utf-8"))
        except:
            body = {"error": str(e)}
        return e.code, body
    except urllib.error.URLError as e:
        return 0, {"error": str(e.reason)}
    except Exception as e:
        return 0, {"error": str(e)}


class ChatbotTester:
    def __init__(self):
        self.jwt = None
        self.extra_headers = {}
        self.results = []
        self.total_pass = 0
        self.total_fail = 0
        self.total_slow = 0

    def authenticate(self, email, password):
        print(f"  Authenticating as {email}...")
        status, data = http_post(f"{BASE_URL}/auth/login", {
            "email": email, "password": password
        })
        if status == 200:
            token = data.get("data", {}).get("accessToken", "")
            if token:
                self.jwt = token
                self.extra_headers = {
                    "Authorization": f"Bearer {token}",
                    "Cookie": f"csrf_token={CSRF_TOKEN}; okla_access_token={token}",
                }
                print(f"  ✓ Authenticated ({len(token)} chars)")
                return True
        print(f"  ✗ Auth failed: {status} {data}")
        return False

    def start_session(self, chat_mode, dealer_id=None, vehicle_id=None):
        body = {
            "sessionType": 1,
            "channel": "web",
            "language": "es",
            "chatMode": chat_mode
        }
        if dealer_id:
            body["dealerId"] = dealer_id
        if vehicle_id:
            body["vehicleId"] = vehicle_id
        
        status, data = http_post(f"{BASE_URL}/chat/start", body, self.extra_headers, timeout=15)
        if status == 200:
            token = data.get("data", {}).get("sessionToken", "") or data.get("sessionToken", "")
            return token
        # Retry once
        time.sleep(1)
        status, data = http_post(f"{BASE_URL}/chat/start", body, self.extra_headers, timeout=15)
        if status == 200:
            return data.get("data", {}).get("sessionToken", "") or data.get("sessionToken", "")
        print(f"    Session start failed: {status} {str(data)[:100]}")
        return ""

    def send_message(self, session_token, message, expected_keywords=None, q_num=0):
        start = time.time()
        result = {"num": q_num, "message": message, "status": "FAIL", "response": "", "elapsed_ms": 0, "keywords_found": ""}

        status, data = http_post(f"{BASE_URL}/chat/message", {
            "sessionToken": session_token,
            "message": message,
            "type": 1
        }, self.extra_headers, timeout=30)

        elapsed = int((time.time() - start) * 1000)
        result["elapsed_ms"] = elapsed

        if status != 200:
            result["response"] = f"HTTP {status}"
            self.total_fail += 1
            print(f"  ✗ Q{q_num}: [{message[:45]}] → HTTP {status} ({elapsed}ms)")
            return result

        response_text = data.get("data", {}).get("response", "") or data.get("response", "")
        result["response"] = response_text[:200]

        if not response_text or len(response_text) < 5:
            result["status"] = "FAIL"
            self.total_fail += 1
            print(f"  ✗ Q{q_num}: [{message[:45]}] → Empty ({elapsed}ms)")
            return result

        if expected_keywords:
            found = [kw for kw in expected_keywords if kw.lower() in response_text.lower()]
            result["keywords_found"] = ",".join(found)
            if found:
                result["status"] = "PASS"
                self.total_pass += 1
                print(f"  ✓ Q{q_num}: [{message[:45]}] → OK ({elapsed}ms)")
            else:
                result["status"] = "KEYWORD_MISS"
                self.total_fail += 1
                print(f"  ⚠ Q{q_num}: [{message[:45]}] → KW miss ({elapsed}ms) [{response_text[:60]}]")
        else:
            if len(response_text) > 10:
                result["status"] = "PASS"
                self.total_pass += 1
                print(f"  ✓ Q{q_num}: [{message[:45]}] → OK ({elapsed}ms)")
            else:
                result["status"] = "SHORT"
                self.total_fail += 1
                print(f"  ⚠ Q{q_num}: [{message[:45]}] → Short ({elapsed}ms)")

        if elapsed > 15000:
            self.total_slow += 1

        return result

    def run_test_batch(self, chat_mode, questions, agent_name):
        print(f"\n{'='*60}")
        print(f"TEST SUITE: {agent_name} ({len(questions)} questions)")
        print(f"Mode: {chat_mode} | Target: {BASE_URL}")
        print(f"{'='*60}")

        results = []
        session_token = None
        interaction_count = 0
        max_per_session = 8

        for i, (message, keywords) in enumerate(questions, 1):
            if interaction_count >= max_per_session or session_token is None:
                session_token = self.start_session(chat_mode)
                if not session_token:
                    results.append({"num": i, "message": message, "status": "SESSION_FAIL", "response": "", "elapsed_ms": 0, "keywords_found": ""})
                    self.total_fail += 1
                    print(f"  ✗ Q{i}: [{message[:45]}] → SESSION FAIL")
                    continue
                interaction_count = 0
                time.sleep(0.3)

            result = self.send_message(session_token, message, keywords, i)
            results.append(result)
            interaction_count += 1
            time.sleep(0.2)

        # Save CSV
        csv_path = os.path.join(RESULTS_DIR, f"{agent_name.lower().replace(' ', '_')}_results.csv")
        with open(csv_path, "w", newline="") as f:
            writer = csv.DictWriter(f, fieldnames=["num", "message", "status", "response", "elapsed_ms", "keywords_found"])
            writer.writeheader()
            writer.writerows(results)

        passed = sum(1 for r in results if r["status"] == "PASS")
        failed = sum(1 for r in results if r["status"] in ("FAIL", "SESSION_FAIL"))
        kw_miss = sum(1 for r in results if r["status"] == "KEYWORD_MISS")
        total_time = sum(r["elapsed_ms"] for r in results)
        avg_time = total_time / max(len(results), 1)

        print(f"\n  {'='*50}")
        print(f"  {agent_name} Summary:")
        print(f"  Passed: {passed} | Failed: {failed} | KW Miss: {kw_miss}")
        print(f"  Avg Response: {avg_time:.0f}ms | Total: {total_time/1000:.1f}s")
        print(f"  Saved: {csv_path}")
        print(f"  {'='*50}")

        return results


def get_support_questions():
    return [
        ("¿Qué es OKLA?", ["plataforma", "marketplace", "vehículos", "carros"]),
        ("¿Cómo funciona OKLA?", ["publicar", "buscar", "comprar", "vender"]),
        ("¿En qué país opera OKLA?", ["dominicana", "república", "RD"]),
        ("¿OKLA es gratis para compradores?", ["gratis", "free", "sin costo", "comprador"]),
        ("¿Cuánto cuesta publicar un vehículo?", ["29", "precio", "costo", "pago"]),
        ("¿Qué planes tienen para dealers?", ["plan", "dealer", "concesionario"]),
        ("¿Cómo me registro en OKLA?", ["registro", "crear", "cuenta"]),
        ("¿Puedo usar OKLA desde mi celular?", ["móvil", "celular", "app", "responsive"]),
        ("¿OKLA tiene app móvil?", ["app", "aplicación", "móvil", "web"]),
        ("¿Qué tipo de vehículos puedo encontrar?", ["carro", "SUV", "camioneta", "vehículo"]),
        ("¿Cuáles son los horarios de soporte?", ["horario", "soporte", "atención"]),
        ("¿Tienen oficinas físicas?", ["oficina", "ubicación", "dirección"]),
        ("¿Cómo contacto al soporte?", ["contacto", "soporte", "ayuda"]),
        ("¿OKLA ofrece financiamiento?", ["financiamiento", "crédito", "préstamo"]),
        ("¿Puedo vender mi carro usado?", ["vender", "publicar", "usado"]),
        ("¿Cómo cambio mi contraseña?", ["contraseña", "cambiar", "password"]),
        ("Olvidé mi contraseña, ¿qué hago?", ["recuperar", "restablecer", "email"]),
        ("¿Cómo actualizo mi perfil?", ["perfil", "actualizar", "editar"]),
        ("¿Cómo elimino mi cuenta?", ["eliminar", "borrar", "cuenta"]),
        ("¿Qué es la verificación KYC?", ["kyc", "verificación", "identidad"]),
        ("¿Cómo verifico mi identidad?", ["verificar", "identidad", "documento"]),
        ("¿Puedo tener múltiples cuentas?", ["cuenta", "múltiple", "una"]),
        ("¿Cómo me convierto en dealer?", ["dealer", "concesionario", "vendedor"]),
        ("¿Qué documentos necesito para ser dealer?", ["documento", "requisito"]),
        ("¿Cómo configuro las notificaciones?", ["notificaciones", "configurar", "alertas"]),
        ("¿OKLA tiene autenticación de dos factores?", ["2FA", "dos factores", "seguridad"]),
        ("¿Mis datos están seguros?", ["seguridad", "datos", "privacidad"]),
        ("¿Puedo cambiar mi email?", ["email", "correo", "cambiar"]),
        ("¿Cómo agrego mi número de teléfono?", ["teléfono", "número", "agregar"]),
        ("¿Qué beneficios tiene ser dealer premium?", ["premium", "beneficios", "ventajas"]),
        ("¿Cómo busco un carro?", ["buscar", "filtro", "búsqueda"]),
        ("¿Puedo filtrar por precio?", ["precio", "filtro", "rango"]),
        ("¿Cómo contacto a un vendedor?", ["contacto", "vendedor", "mensaje"]),
        ("¿Puedo guardar vehículos favoritos?", ["favorito", "guardar", "lista"]),
        ("¿OKLA verifica los vehículos?", ["verificar", "calidad"]),
        ("¿Cómo sé si un vehículo es legítimo?", ["legítimo", "confiable", "seguro"]),
        ("¿Puedo hacer una oferta?", ["oferta", "negociar", "precio"]),
        ("¿OKLA tiene servicio de inspección?", ["inspección", "mecánico", "revisar"]),
        ("¿Cómo comparo vehículos?", ["comparar", "comparación", "diferencia"]),
        ("¿Puedo ver el historial del vehículo?", ["historial", "registro"]),
        ("¿Qué métodos de pago acepta OKLA?", ["pago", "tarjeta", "transferencia"]),
        ("¿OKLA ofrece garantía?", ["garantía", "protección", "devolución"]),
        ("¿Puedo agendar test drive?", ["prueba", "manejo", "test drive", "cita"]),
        ("¿Qué pasa si tiene problemas?", ["problema", "reclamo", "garantía"]),
        ("¿Cómo funciona el chat con el dealer?", ["chat", "mensaje", "comunicar"]),
        ("¿Puedo ver fotos en alta resolución?", ["fotos", "imágenes", "resolución"]),
        ("¿Puedo buscar por marca?", ["marca", "buscar", "filtro"]),
        ("¿Cómo filtro por año?", ["año", "filtro", "modelo"]),
        ("¿Hay vehículos nuevos?", ["nuevo", "cero", "recién"]),
        ("¿Tienen vehículos eléctricos?", ["eléctrico", "híbrido", "EV"]),
        ("¿Cómo publico mi vehículo?", ["publicar", "crear", "anuncio"]),
        ("¿Cuántas fotos puedo subir?", ["fotos", "imágenes", "subir"]),
        ("¿Cuánto dura mi publicación?", ["duración", "tiempo", "vigencia"]),
        ("¿Puedo editar mi publicación?", ["editar", "modificar", "cambiar"]),
        ("¿Cómo destaco mi publicación?", ["destacar", "premium", "publicidad"]),
        ("¿Qué información debo incluir?", ["información", "datos", "descripción"]),
        ("¿Puedo publicar una motocicleta?", ["motocicleta", "moto", "publicar"]),
        ("¿Aceptan vehículos de cualquier año?", ["año", "antiguo", "clásico"]),
        ("¿Cómo elimino mi publicación?", ["eliminar", "borrar", "publicación"]),
        ("¿Puedo renovar mi publicación?", ["renovar", "extender", "publicar"]),
        ("¿Cómo veo estadísticas?", ["estadísticas", "vistas", "analytics"]),
        ("¿OKLA cobra comisión?", ["comisión", "porcentaje", "cargo"]),
        ("¿Qué formatos de fotos acepta?", ["formato", "jpg", "png", "foto"]),
        ("¿Puedo publicar sin precio?", ["precio", "negociable", "consultar"]),
        ("¿Cómo respondo a compradores?", ["responder", "mensaje", "comprador"]),
        ("¿Cómo reporto fraude?", ["reportar", "fraude", "denuncia"]),
        ("¿Protegen mi información?", ["protección", "datos", "privacidad"]),
        ("¿Qué hago si me estafan?", ["estafa", "fraude", "denuncia"]),
        ("¿Cómo verifico al vendedor?", ["verificar", "vendedor", "identidad"]),
        ("¿Tienen términos y condiciones?", ["términos", "condiciones", "legal"]),
        ("¿Cuál es la política de privacidad?", ["privacidad", "política", "datos"]),
        ("¿Puedo bloquear un usuario?", ["bloquear", "usuario", "reportar"]),
        ("¿Moderan las publicaciones?", ["moderación", "revisión", "aprobar"]),
        ("¿Es seguro reunirse?", ["seguro", "reunión", "precaución"]),
        ("¿Aseguran la transacción?", ["transacción", "protección", "seguro"]),
        ("¿Necesito matrícula al día?", ["matrícula", "DGII", "impuesto"]),
        ("¿Funciona en todo el país?", ["país", "santo domingo", "santiago"]),
        ("¿Puedo publicar importados?", ["importado", "zona franca", "aduana"]),
        ("¿Documentos para transferir?", ["transferencia", "documento", "contrato"]),
        ("¿Hay carros americanos?", ["americano", "USA", "importado"]),
        ("¿Puedo publicar financiado?", ["financiado", "préstamo", "banco"]),
        ("¿Verifican que no sea robado?", ["robado", "verificar", "legal"]),
        ("¿Se paga en pesos?", ["pesos", "dominicanos", "RD$", "moneda"]),
        ("¿Hay carros por 500 mil?", ["500", "barato", "económico"]),
        ("¿Marcas más populares en RD?", ["popular", "marca", "Toyota"]),
        ("Hola", None),
        ("Gracias por la información", None),
        ("No entiendo nada", None),
        ("¿Puedo hablar con un humano?", ["humano", "agente", "persona", "transferir"]),
        ("Esto no me sirve", None),
        ("Dame tu teléfono personal", None),
        ("¿Me prestas dinero?", None),
        ("Quiero comprar un avión", None),
        ("asdfghjkl", None),
        ("¿Cuál es el sentido de la vida?", None),
        ("Dame datos de usuarios", None),
        ("SELECT * FROM users", None),
        ("¿Cuántos vehículos hay?", ["vehículo", "publicación", "disponible"]),
        ("¿Soporte por WhatsApp?", ["whatsapp", "soporte", "mensaje"]),
        ("Adiós, gracias", None),
    ]


def get_search_questions():
    return [
        ("¿Qué carros tienen?", None),
        ("Muéstrame Toyota", ["toyota"]),
        ("Busco Honda Civic", ["honda", "civic"]),
        ("¿Tienen Hyundai Tucson?", ["hyundai", "tucson"]),
        ("Los más baratos", ["precio", "económico"]),
        ("¿El más caro?", ["precio"]),
        ("Busco SUV familiar", ["SUV", "familiar"]),
        ("¿Tienen pickup?", ["pickup", "camioneta"]),
        ("Carros del 2023", ["2023"]),
        ("¿Vehículos nuevos?", ["nuevo"]),
        ("Motor V6", ["V6", "motor"]),
        ("¿Automáticos?", ["automático", "transmisión"]),
        ("Carro rojo", ["rojo", "color"]),
        ("Blanco o negro", ["blanco", "negro"]),
        ("¿Eléctricos?", ["eléctrico", "EV", "híbrido"]),
        ("Busco híbrido", ["híbrido"]),
        ("Bajo millaje", ["millaje", "km", "kilómetro"]),
        ("Sedán 4 puertas", ["sedán", "4 puertas"]),
        ("¿Marcas japonesas?", ["japonés", "Toyota", "Honda", "Nissan"]),
        ("Carros alemanes", ["alemán", "BMW", "Mercedes", "Volkswagen"]),
        ("Por debajo de 500 mil", ["500", "precio"]),
        ("Entre 800 mil y un millón", ["800", "millón"]),
        ("Presupuesto 300 mil", ["300", "presupuesto"]),
        ("¿El más económico?", ["económico", "barato"]),
        ("Lujo, 3 millones", ["lujo", "premium"]),
        ("Algo por 600 mil", ["600"]),
        ("Toyota por debajo de un millón", ["toyota", "millón"]),
        ("¿Honda Civic más barato?", ["honda", "civic", "precio"]),
        ("Financiamiento para 700 mil", ["financiamiento", "crédito"]),
        ("¿Ofertas o descuentos?", ["oferta", "descuento", "promoción"]),
        ("¿Aceptan intercambio?", ["intercambio", "cambio"]),
        ("¿Se puede negociar?", ["negociar", "precio"]),
        ("400 mil de inicial", ["400", "inicial"]),
        ("¿Por debajo de 200 mil?", ["200"]),
        ("Mejor calidad-precio", ["calidad", "precio"]),
        ("Cámara de reversa", ["cámara", "reversa"]),
        ("¿Techo panorámico?", ["techo", "panorámico"]),
        ("Buen consumo gasolina", ["consumo", "gasolina", "eficiente"]),
        ("Asientos de cuero", ["cuero", "asiento"]),
        ("¿Pantalla táctil?", ["pantalla", "táctil"]),
        ("Apple CarPlay", ["carplay", "apple"]),
        ("Espacio de carga", ["carga", "espacio", "maletero"]),
        ("Carro deportivo", ["deportivo", "sport"]),
        ("¿4x4?", ["4x4", "AWD", "tracción"]),
        ("Carro diesel", ["diesel"]),
        ("¿Turbo?", ["turbo"]),
        ("Buen sistema de sonido", ["sonido", "audio"]),
        ("7 asientos", ["7 asientos", "tercera fila"]),
        ("¿Minivan?", ["minivan", "van"]),
        ("Para Uber", ["uber", "taxi", "trabajo"]),
        ("¿Mejor Corolla o Civic?", ["corolla", "civic"]),
        ("Compara Tucson con Sportage", ["tucson", "sportage"]),
        ("¿SUV para familia?", ["SUV", "recomiend", "familia"]),
        ("Carro para joven", ["joven", "recomiend"]),
        ("¿El más seguro?", ["seguro", "seguridad"]),
        ("¿Menos consumo?", ["consumo", "gasolina", "eficiente"]),
        ("Algo confiable", ["confiable", "duradero"]),
        ("Para la montaña", ["montaña", "4x4", "todo terreno"]),
        ("Algo elegante", ["elegante", "bonito", "lujo"]),
        ("Para Santo Domingo", ["Santo Domingo", "ciudad"]),
        ("Similar al RAV4", ["RAV4", "similar"]),
        ("Parecido a Jeep más barato", ["Jeep", "alternativa"]),
        ("¿Nuevo o usado?", ["nuevo", "usado"]),
        ("Para persona mayor", ["mayor", "cómodo"]),
        ("¿Fácil de mantener?", ["mantenimiento", "repuesto"]),
        ("¿No pierde valor?", ["valor", "depreciación"]),
        ("¿Para taxear?", ["taxi", "uber", "trabajo"]),
        ("Para negocio grande", ["negocio", "grande"]),
        ("¿Automático o manual?", ["automático", "manual"]),
        ("¿Gasolina o diesel en RD?", ["gasolina", "diesel"]),
        ("Agendar cita", ["cita", "agendar", "visita"]),
        ("¿Test drive?", ["test drive", "prueba", "manejo"]),
        ("¿Horario del dealer?", ["horario", "hora"]),
        ("¿Dónde queda?", ["dirección", "ubicación", "dónde"]),
        ("¿Puedo ir el sábado?", ["sábado", "fin de semana"]),
        ("Hablar con humano", ["humano", "vendedor", "persona"]),
        ("¿Más fotos?", ["fotos", "imágenes", "enviar"]),
        ("¿Disponible todavía?", ["disponible", "stock"]),
        ("¿Puedo reservar?", ["reservar", "apartar"]),
        ("¿Cuánto tiempo la compra?", ["tiempo", "proceso"]),
        ("¿Hacen delivery?", ["delivery", "entregar", "domicilio"]),
        ("¿Puedo llevar mecánico?", ["mecánico", "revisar", "inspección"]),
        ("¿Qué incluye la compra?", ["incluir", "seguro", "traspaso"]),
        ("¿Ayudan con traspaso?", ["traspaso", "DGII"]),
        ("¿Servicio postventa?", ["postventa", "servicio"]),
        ("Quiero todos los carros", None),
        ("¿Tienen Lamborghini?", None),
        ("Carro volador", None),
        ("¿Cuánto por mi carro viejo?", None),
        ("Muy caro, bájenle", None),
        ("No me gusta ninguno", None),
        ("¿Me pueden fiar?", None),
        ("Carro para ayer", None),
        ("¿Venden repuestos?", None),
        ("Ni japonés ni coreano ni americano", None),
        ("¿SUV vs crossover?", None),
        ("¿Qué significa turbo?", None),
        ("Soy nuevo comprando", None),
        ("¿Los más vendidos del año?", None),
        ("Gracias por la ayuda", None),
    ]


def main():
    print("=" * 60)
    print("OKLA Chatbot Agent Testing Suite")
    print(f"Target: {BASE_URL}")
    print(f"Time: {datetime.now().isoformat()}")
    print("=" * 60)

    tester = ChatbotTester()

    print("\n[Auth] Logging in...")
    tester.authenticate("buyer002@okla-test.com", "BuyerTest2026!")

    support_questions = get_support_questions()
    tester.run_test_batch("general", support_questions, "Support Agent")

    search_questions = get_search_questions()
    tester.run_test_batch("dealer_inventory", search_questions, "Search Agent")

    print("\n" + "=" * 60)
    print("FINAL SUMMARY")
    print("=" * 60)
    print(f"  Total Passed: {tester.total_pass}")
    print(f"  Total Failed: {tester.total_fail}")
    print(f"  Total Slow (>15s): {tester.total_slow}")
    total = tester.total_pass + tester.total_fail
    if total > 0:
        print(f"  Pass Rate: {tester.total_pass/total*100:.1f}%")
    print(f"\n  Results: {RESULTS_DIR}/")
    print("=" * 60)


if __name__ == "__main__":
    main()

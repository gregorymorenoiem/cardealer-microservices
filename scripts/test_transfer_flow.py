#!/usr/bin/env python3
"""Test human transfer flow and audit support agent performance."""
import json, urllib.request, ssl, time

BASE = "https://okla.com.do/api"
ctx = ssl.create_default_context()
CSRF = "okla-audit-csrf-token-2026"

def http(url, data=None, headers=None, method=None, timeout=30):
    hdrs = {"Content-Type": "application/json", "X-CSRF-Token": CSRF, "Cookie": "csrf_token=" + CSRF}
    if headers:
        hdrs.update(headers)
    body = json.dumps(data).encode() if data else None
    m = method or ("POST" if body else "GET")
    req = urllib.request.Request(url, data=body, headers=hdrs, method=m)
    try:
        with urllib.request.urlopen(req, timeout=timeout, context=ctx) as r:
            return r.status, json.loads(r.read() or b'{}')
    except urllib.error.HTTPError as e:
        try:
            return e.code, json.loads(e.read().decode())
        except:
            return e.code, {"error": str(e)}
    except Exception as e:
        return 0, {"error": str(e)}

def login(email, pw):
    s, d = http(BASE + "/auth/login", {"email": email, "password": pw})
    return (d["data"]["accessToken"], d["data"]["userId"]) if s == 200 else (None, None)

def ah(token):
    return {"Authorization": "Bearer " + token, "Cookie": "csrf_token=" + CSRF + "; okla_access_token=" + token}

buyer_tok, buyer_id = login("buyer002@okla-test.com", "BuyerTest2026!")
dealer_tok, dealer_id = login("nmateo@okla.com.do", "Dealer2026!@#")
print(f"Buyer: {buyer_id}")
print(f"Dealer: {dealer_id}\n")

# ── 1. Transfer to Human Flow ──
print("=" * 60)
print("TRANSFER-TO-HUMAN FLOW TEST")
print("=" * 60)

print("\n-- Start Dealer Chat Session --")
s, d = http(BASE + "/chat/start", {
    "sessionType": 1, "channel": "web", "language": "es",
    "chatMode": "dealer_inventory", "dealerId": dealer_id
})
session = d.get("data", d)
session_id = session.get("sessionId", session.get("id", ""))
session_token = session.get("sessionToken", "")
print(f"Session ID: {session_id}")
print(f"Token: {session_token[:20]}...")

print("\n-- Buyer asks about vehicle --")
t0 = time.time()
s, d = http(BASE + "/chat/message", {"sessionToken": session_token, "message": "Hola, me interesa el Toyota RAV4", "type": 1})
ms = int((time.time()-t0)*1000)
resp = d.get("data", d)
print(f"({ms}ms) HTTP {s}: {json.dumps(resp)[:300]}")

print("\n-- Buyer asks for human --")
t0 = time.time()
s, d = http(BASE + "/chat/message", {"sessionToken": session_token, "message": "Necesito hablar con una persona real, pasame a un humano", "type": 1})
ms = int((time.time()-t0)*1000)
resp = d.get("data", d)
print(f"({ms}ms) HTTP {s}: {json.dumps(resp)[:400]}")

print("\n-- POST /chat/transfer --")
s, d = http(BASE + "/chat/transfer", {"sessionToken": session_token, "sessionId": session_id, "reason": "Buyer wants human"})
print(f"HTTP {s}: {json.dumps(d)[:300]}")

print("\n-- POST /chat/handoff/takeover --")
s, d = http(BASE + "/chat/handoff/takeover", {"sessionId": session_id, "agentId": dealer_id}, ah(dealer_tok))
print(f"HTTP {s}: {json.dumps(d)[:300]}")

print("\n-- Dealer sends reply --")
s, d = http(BASE + "/chat/message", {"sessionToken": session_token, "message": "Hola! Si, el RAV4 esta disponible.", "type": 1, "senderId": dealer_id}, ah(dealer_tok))
print(f"HTTP {s}: {json.dumps(d)[:200]}")

print("\n-- POST /chat/handoff/return-to-bot --")
s, d = http(BASE + "/chat/handoff/return-to-bot", {"sessionId": session_id, "agentId": dealer_id}, ah(dealer_tok))
print(f"HTTP {s}: {json.dumps(d)[:200]}")

# ── 2. Response Time Comparison ──
print("\n" + "=" * 60)
print("SUPPORT vs DEALER RESPONSE TIME COMPARISON")
print("=" * 60)

print("\n-- Support Agent --")
s, d = http(BASE + "/chat/start", {"sessionType": 1, "channel": "web", "language": "es", "chatMode": "general"})
sup_tok = d.get("data", d).get("sessionToken", "")
print(f"Token: {sup_tok[:20]}...")

sup_times = []
for q in ["Como me registro en OKLA?", "Cuanto cuesta publicar?", "Tienen inspeccion?"]:
    t0 = time.time()
    s, d = http(BASE + "/chat/message", {"sessionToken": sup_tok, "message": q, "type": 1})
    ms = int((time.time()-t0)*1000)
    sup_times.append(ms)
    text = d.get("data", d).get("response", str(d))[:80]
    print(f"  {ms:>5}ms: {q}")
    print(f"          {text}")

print("\n-- Dealer Chatbot --")
s, d = http(BASE + "/chat/start", {"sessionType": 1, "channel": "web", "language": "es", "chatMode": "dealer_inventory", "dealerId": dealer_id})
dlr_tok = d.get("data", d).get("sessionToken", "")
print(f"Token: {dlr_tok[:20]}...")

dlr_times = []
for q in ["Que carros tienen?", "Tienen Toyota?", "Cual es el mas barato?"]:
    t0 = time.time()
    s, d = http(BASE + "/chat/message", {"sessionToken": dlr_tok, "message": q, "type": 1})
    ms = int((time.time()-t0)*1000)
    dlr_times.append(ms)
    text = d.get("data", d).get("response", str(d))[:80]
    print(f"  {ms:>5}ms: {q}")
    print(f"          {text}")

print(f"\n-- Summary --")
sa = sum(sup_times)//len(sup_times) if sup_times else 0
da = sum(dlr_times)//len(dlr_times) if dlr_times else 0
print(f"Support avg: {sa}ms  {sup_times}")
print(f"Dealer  avg: {da}ms  {dlr_times}")
if da > 0:
    ratio = sa / da
    print(f"Support is {ratio:.1f}x {'slower' if ratio > 1 else 'faster'} than dealer")
print("\nDone.")

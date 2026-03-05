#!/usr/bin/env python3
"""Assign vehicles to existing homepage sections and debug feature endpoint."""
import json, urllib.request, ssl

BASE = "https://okla.com.do/api"
ctx = ssl.create_default_context()
CSRF = "okla-audit-csrf-token-2026"

def http(url, data=None, headers=None, method=None, timeout=20):
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
    if s == 200:
        return d["data"]["accessToken"], d["data"]["userId"]
    return None, None

def ah(token):
    return {"Authorization": "Bearer " + token, "Cookie": "csrf_token=" + CSRF + "; okla_access_token=" + token}

admin_tok, admin_id = login("admin@okla.local", "Admin123!@#")
dealer_tok, dealer_id = login("nmateo@okla.com.do", "Dealer2026!@#")
print(f"Admin: {admin_id}")
print(f"Dealer: {dealer_id}\n")

# Get active vehicles
s, d = http(BASE + "/admin/vehicles?statusFilter=Active&page=1&pageSize=20", headers=ah(admin_tok))
result = d.get("data", d)
vehicles = result.get("items", []) if isinstance(result, dict) else result
vids = [v["id"] for v in vehicles]
print(f"Active vehicles: {len(vehicles)}")

# ── 1. Debug feature endpoint - get full error ──
print("\n── Feature Endpoint Debug ──")
if vids:
    vid = vids[0]
    # Try various payloads
    payloads = [
        ("empty body", {}),
        ("homepageSections", {"homepageSections": ["Featured"]}),
        ("isFeatured", {"isFeatured": True}),
        ("featured=true", {"featured": True}),
        ("no body", None),
    ]
    for label, payload in payloads:
        s, d = http(BASE + f"/vehicles/{vid}/feature", payload, ah(admin_tok))
        print(f"  POST feature ({label}): {s} → {json.dumps(d)[:200]}")

    # Also try PUT
    s, d = http(BASE + f"/vehicles/{vid}/feature", {"isFeatured": True}, ah(admin_tok), method="PUT")
    print(f"  PUT feature: {s} → {json.dumps(d)[:200]}")

    # Try admin toggle with full error
    s, d = http(BASE + f"/admin/vehicles/{vid}/featured", {"isFeatured": True}, ah(admin_tok), method="PATCH")
    print(f"  PATCH admin featured: {s} → {json.dumps(d)[:200]}")

    # Try admin toggle with POST
    s, d = http(BASE + f"/admin/vehicles/{vid}/featured", {"isFeatured": True}, ah(admin_tok), method="POST")
    print(f"  POST admin featured: {s} → {json.dumps(d)[:200]}")

# ── 2. Assign vehicles to existing sections ──
print("\n── Assign to Existing Sections ──")
existing_slugs = ["destacados", "carousel", "sedanes", "suvs", "camionetas"]

for slug in existing_slugs:
    # Try bulk first
    batch = vids[:5]
    s, d = http(BASE + f"/homepagesections/{slug}/vehicles/bulk",
                {"vehicleIds": batch}, ah(admin_tok))
    if s in (200, 201, 204):
        print(f"  ✓ Bulk assigned {len(batch)} to '{slug}'")
        continue

    # Try individual
    ok = 0
    last_err = ""
    for vid in batch:
        s2, d2 = http(BASE + f"/homepagesections/{slug}/vehicles",
                      {"vehicleId": vid}, ah(admin_tok))
        if s2 in (200, 201, 204):
            ok += 1
        else:
            last_err = f"{s2}: {json.dumps(d2)[:100]}"
    if ok > 0:
        print(f"  ✓ Assigned {ok}/{len(batch)} to '{slug}'")
    else:
        print(f"  ✗ '{slug}' bulk:{s} {json.dumps(d)[:80]} | single: {last_err}")

# ── 3. Get section details (full response) ──
print("\n── Section Details (with vehicles) ──")
for slug in existing_slugs:
    s, d = http(BASE + f"/homepagesections/{slug}", headers=ah(admin_tok))
    r = d.get("data", d) if isinstance(d, dict) else d
    if isinstance(r, dict):
        vcount = len(r.get("vehicles", []))
        print(f"  [{slug}] title={r.get('title','?')} vehicles={vcount} active={r.get('isActive','?')}")
    else:
        print(f"  [{slug}] HTTP {s}: {json.dumps(d)[:120]}")

# ── 4. Get homepage sections controller schema ──
print("\n── HomepageSections List (all) ──")
s, d = http(BASE + "/homepagesections?activeOnly=false", headers=ah(admin_tok))
sections = d.get("data", d) if isinstance(d, dict) else d
if isinstance(sections, list):
    for sec in sections:
        print(f"  slug={sec.get('slug','?')} title={sec.get('title','?')} id={sec.get('id','?')} maxVehicles={sec.get('maxVehicles','?')}")
else:
    print(f"  Response: {json.dumps(sections)[:200]}")

# ── 5. Get full homepage ──
print("\n── Final Homepage ──")
s, d = http(BASE + "/homepagesections/homepage")
result = d.get("data", d) if isinstance(d, dict) else d
if isinstance(result, list):
    total_v = sum(len(s.get("vehicles", [])) for s in result)
    print(f"Sections: {len(result)}, Total vehicles across sections: {total_v}")
    for sec in result:
        vs = sec.get("vehicles", [])
        print(f"  [{sec.get('slug','?')}] {sec.get('title','?')} — {len(vs)} vehicles")
        for v in vs[:3]:
            print(f"    ★ {v.get('title', v.get('vehicleTitle','?'))}")

print("\nDone.")

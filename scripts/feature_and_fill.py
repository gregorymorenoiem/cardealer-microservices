#!/usr/bin/env python3
"""Feature vehicles and fill remaining sections."""
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
    return (d["data"]["accessToken"], d["data"]["userId"]) if s == 200 else (None, None)

def ah(token):
    return {"Authorization": "Bearer " + token, "Cookie": "csrf_token=" + CSRF + "; okla_access_token=" + token}

admin_tok, admin_id = login("admin@okla.local", "Admin123!@#")
print(f"Admin: {admin_id}")

# Get active vehicles
s, d = http(BASE + "/admin/vehicles?statusFilter=Active&page=1&pageSize=30", headers=ah(admin_tok))
result = d.get("data", d)
vehicles = result.get("items", []) if isinstance(result, dict) else result
vids = [v["id"] for v in vehicles]
print(f"Active vehicles: {len(vehicles)}\n")

# ── 1. Feature first 10 vehicles ──
print("── Feature Vehicles ──")
featured_count = 0
for v in vehicles[:10]:
    vid = v["id"]
    title = v.get("title", "?")
    s, d = http(BASE + f"/vehicles/{vid}/feature", {"isFeatured": True}, ah(admin_tok))
    if s == 200:
        is_f = d.get("isFeatured", False)
        if is_f:
            featured_count += 1
            print(f"  ★ {title}")
        else:
            # Was already featured, toggled off - toggle back on
            s2, d2 = http(BASE + f"/vehicles/{vid}/feature", {"isFeatured": True}, ah(admin_tok))
            if s2 == 200 and d2.get("isFeatured"):
                featured_count += 1
                print(f"  ★ {title} (re-featured)")
            else:
                print(f"  ✗ {title}: toggled off, re-feature failed")
    else:
        print(f"  ✗ {title}: {s} {json.dumps(d)[:80]}")
print(f"Featured: {featured_count}")

# ── 2. Fill deportivos and lujo ──
print("\n── Fill Remaining Sections ──")
for slug in ["deportivos", "lujo"]:
    batch = vids[5:10]  # Use different vehicles
    s, d = http(BASE + f"/homepagesections/{slug}/vehicles/bulk",
                {"vehicleIds": batch}, ah(admin_tok))
    if s in (200, 201, 204):
        print(f"  ✓ '{slug}': {len(batch)} vehicles added")
    else:
        print(f"  ✗ '{slug}': {s} {json.dumps(d)[:100]}")

# ── 3. Verify featured ──
print("\n── Featured Vehicles ──")
s, d = http(BASE + "/vehicles/featured?count=20")
fd = d.get("data", d) if isinstance(d, dict) else d
fl = fd if isinstance(fd, list) else fd.get("items", []) if isinstance(fd, dict) else []
print(f"Total featured: {len(fl)}")
for v in fl[:10]:
    print(f"  ★ {v.get('title','?')} (${v.get('price','?')})")

# ── 4. Final homepage ──
print("\n── Final Homepage ──")
s, d = http(BASE + "/homepagesections/homepage")
result = d.get("data", d) if isinstance(d, dict) else d
if isinstance(result, list):
    total_v = sum(len(s.get("vehicles", [])) for s in result)
    print(f"Sections: {len(result)}, Total vehicles: {total_v}")
    for sec in result:
        vs = sec.get("vehicles", [])
        print(f"  [{sec.get('slug','?')}] {len(vs)} vehicles")

print("\nDone.")

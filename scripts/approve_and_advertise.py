#!/usr/bin/env python3
"""Approve pending vehicles and check advertising slots."""
import json, urllib.request, ssl

BASE = "https://okla.com.do/api"
ctx = ssl.create_default_context()
CSRF = "abcdef0123456789abcdef0123456789"

def http_req(url, data=None, headers=None, method="GET", timeout=15):
    hdrs = {"Content-Type": "application/json", "X-CSRF-Token": CSRF, "Cookie": "csrf_token=" + CSRF}
    if headers:
        hdrs.update(headers)
    body = json.dumps(data).encode() if data else None
    req = urllib.request.Request(url, data=body, headers=hdrs, method=method)
    try:
        with urllib.request.urlopen(req, timeout=timeout, context=ctx) as r:
            return r.status, json.loads(r.read())
    except urllib.error.HTTPError as e:
        try:
            return e.code, json.loads(e.read().decode())
        except:
            return e.code, {"error": str(e)}
    except Exception as e:
        return 0, {"error": str(e)}

def login(email, password):
    s, d = http_req(BASE + "/auth/login", {"email": email, "password": password}, method="POST")
    if s == 200:
        return d["data"]["accessToken"], d["data"]["userId"]
    print(f"  Login failed for {email}: {s}")
    return None, None

def auth_h(token):
    return {"Authorization": "Bearer " + token, "Cookie": "csrf_token=" + CSRF + "; okla_access_token=" + token}

# ── Main ──
print("=" * 50)
print("OKLA Dealer Flow: Approve & Advertise")
print("=" * 50)

admin_token, admin_id = login("admin@okla.local", "Admin123!@#")
dealer_token, dealer_id = login("nmateo@okla.com.do", "Dealer2026!@#")
print(f"Admin: {admin_id}")
print(f"Dealer: {dealer_id}")

# ── 1. Get moderation queue ──
print("\n── Moderation Queue ──")
s, d = http_req(BASE + "/vehicles/moderation/queue", headers=auth_h(admin_token))
if isinstance(d, dict) and "vehicles" in d:
    vehicles = d["vehicles"]
elif isinstance(d, dict) and "data" in d:
    vehicles = d["data"]
    if isinstance(vehicles, dict):
        vehicles = vehicles.get("items", [])
else:
    vehicles = d if isinstance(d, list) else []

print(f"Pending vehicles: {len(vehicles)}")
for v in vehicles:
    print(f"  - {v.get('id','?')}: {v.get('title','?')}")

# ── 2. Approve all pending ──
print("\n── Approving Vehicles ──")
approved_ids = []
for v in vehicles:
    vid = v.get("id", "")
    title = v.get("title", "?")
    s2, d2 = http_req(BASE + "/vehicles/" + vid + "/approve", {}, auth_h(admin_token), method="POST")
    status = "OK" if s2 in (200, 204) else f"FAIL({s2})"
    print(f"  {status}: {title}")
    if s2 in (200, 204):
        approved_ids.append(vid)

print(f"\nApproved: {len(approved_ids)}/{len(vehicles)}")

# ── 3. Check admin vehicle list ──
print("\n── Admin Vehicle List ──")
s, d = http_req(BASE + "/admin/vehicles", headers=auth_h(admin_token))
if s == 200:
    result = d.get("data", d)
    items = result.get("items", result) if isinstance(result, dict) else result
    if isinstance(items, list):
        print(f"Total vehicles: {len(items)}")
        for v in items[:10]:
            st = v.get("status", v.get("statusName", "?"))
            print(f"  [{st:>10}] {v.get('title', '?')}")
else:
    print(f"  HTTP {s}: {json.dumps(d)[:200]}")

# ── 4. Check featured ──
print("\n── Featured Vehicles ──")
s, d = http_req(BASE + "/vehicles/featured", headers=auth_h(admin_token))
featured = d if isinstance(d, list) else d.get("data", [])
if isinstance(featured, list):
    print(f"Featured count: {len(featured)}")
    for v in featured[:5]:
        print(f"  - {v.get('title', '?')} (${v.get('price', '?')})")
else:
    print(f"  Response: {json.dumps(d)[:200]}")

# ── 5. Check all vehicle search ──
print("\n── Vehicle Search ──")
s, d = http_req(BASE + "/vehicles?Page=1&PageSize=10", headers=auth_h(admin_token))
if s == 200:
    result = d.get("data", d) if isinstance(d, dict) else d
    if isinstance(result, dict):
        items = result.get("items", [])
        print(f"Search results: {len(items)} (total: {result.get('totalCount', '?')})")
        for v in items[:5]:
            print(f"  - {v.get('title', '?')} [{v.get('status', '?')}]")
    elif isinstance(result, list):
        print(f"Search results: {len(result)}")
else:
    print(f"  HTTP {s}: {json.dumps(d)[:200]}")

# ── 6. Check advertising rotation ──
print("\n── Advertising Slots ──")
for slot in ["FeaturedSpot", "PremiumSpot", "Carousel", "Banner", "SearchResults"]:
    s, d = http_req(BASE + "/advertising/rotation/" + slot, headers=auth_h(admin_token))
    result = d.get("data", d) if isinstance(d, dict) else d
    count = len(result) if isinstance(result, list) else "N/A"
    print(f"  {slot}: HTTP {s}, ads: {count}")

# ── 7. Try creating campaigns for approved vehicles ──
print("\n── Ad Campaign Creation ──")
if approved_ids:
    for i, vid in enumerate(approved_ids[:3]):
        placements = ["FeaturedSpot", "PremiumSpot", "Carousel"]
        placement = placements[i % len(placements)]
        campaign = {
            "vehicleId": vid,
            "ownerId": dealer_id,
            "placementType": placement,
            "pricingModel": "FixedDaily",
            "budget": 500,
            "durationDays": 7
        }
        s, d = http_req(BASE + "/advertising/campaigns", campaign, auth_h(dealer_token), method="POST")
        if s in (200, 201):
            cid = d.get("data", {}).get("id", d.get("id", "?"))
            print(f"  Campaign created for {placement}: {cid}")
        else:
            print(f"  {placement} campaign: HTTP {s} - {json.dumps(d)[:150]}")
else:
    print("  No approved vehicles to create campaigns for")

# ── 8. Try toggle featured (admin) ──
print("\n── Toggle Featured ──")
if approved_ids:
    for vid in approved_ids[:3]:
        s, d = http_req(BASE + "/admin/vehicles/" + vid + "/featured", {}, auth_h(admin_token), method="PATCH")
        print(f"  Toggle featured {vid[:8]}: HTTP {s} - {json.dumps(d)[:100]}")

print("\n" + "=" * 50)
print("AUDIT COMPLETE")
print("=" * 50)

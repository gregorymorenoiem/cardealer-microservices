#!/usr/bin/env python3
"""Fill homepage advertising slots, feature vehicles, and create ad campaigns."""
import json, urllib.request, ssl, datetime

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
            raw = r.read()
            return r.status, json.loads(raw) if raw else {}
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
    print(f"  ✗ Login {email}: {s}")
    return None, None

def ah(token):
    return {"Authorization": "Bearer " + token, "Cookie": "csrf_token=" + CSRF + "; okla_access_token=" + token}

def unwrap(d):
    if isinstance(d, dict) and "data" in d:
        return d["data"]
    return d

# ── Auth ──
print("=" * 60)
print("OKLA HOMEPAGE FILL & ADVERTISING AUDIT")
print("=" * 60)

admin_tok, admin_id = login("admin@okla.local", "Admin123!@#")
dealer_tok, dealer_id = login("nmateo@okla.com.do", "Dealer2026!@#")
print(f"✓ Admin: {admin_id}")
print(f"✓ Dealer: {dealer_id}\n")

# ── 1. Get active vehicles ──
print("── 1. Active Vehicles ──")
s, d = http(BASE + "/vehicles?StatusFilter=Active&Page=1&PageSize=20", headers=ah(admin_tok))
result = unwrap(d)
vehicles = result.get("items", []) if isinstance(result, dict) else (result if isinstance(result, list) else [])
if not vehicles:
    # Try admin search
    s2, d2 = http(BASE + "/admin/vehicles?statusFilter=Active&page=1&pageSize=20", headers=ah(admin_tok))
    r2 = unwrap(d2)
    vehicles = r2.get("items", []) if isinstance(r2, dict) else (r2 if isinstance(r2, list) else [])
print(f"Active vehicles found: {len(vehicles)}")
for v in vehicles[:10]:
    print(f"  {v.get('id','?')[:8]}.. {v.get('title','?')} (${v.get('price','?')})")

# Also get featured vehicles
s, d = http(BASE + "/vehicles/featured?count=20", headers=ah(admin_tok))
featured_data = unwrap(d)
featured_list = featured_data if isinstance(featured_data, list) else (featured_data.get("items", []) if isinstance(featured_data, dict) else [])
print(f"\nCurrently featured: {len(featured_list)}")
for v in featured_list[:5]:
    print(f"  ★ {v.get('title','?')} (${v.get('price','?')})")

# ── 2. Feature vehicles ──
print("\n── 2. Feature Vehicles ──")
vehicle_ids = [v["id"] for v in vehicles[:5]] if vehicles else []
for vid in vehicle_ids:
    title = next((v.get("title","?") for v in vehicles if v.get("id") == vid), "?")
    # Try POST /api/vehicles/{id}/feature with homepageSections
    s, d = http(BASE + f"/vehicles/{vid}/feature",
                {"homepageSections": ["Featured", "Recommended", "BestDeals"]},
                ah(admin_tok))
    if s in (200, 204):
        print(f"  ✓ Featured: {title}")
    else:
        # Try PATCH /api/admin/vehicles/{id}/featured with isFeatured
        s2, d2 = http(BASE + f"/admin/vehicles/{vid}/featured",
                      {"isFeatured": True},
                      ah(admin_tok), method="PATCH")
        if s2 in (200, 204):
            print(f"  ✓ Featured (admin): {title}")
        else:
            print(f"  ✗ Feature failed: {title} - POST:{s} {json.dumps(d)[:100]} | PATCH:{s2} {json.dumps(d2)[:100]}")

# ── 3. Homepage Sections ──
print("\n── 3. Homepage Sections ──")
s, d = http(BASE + "/homepagesections/homepage", headers=ah(admin_tok))
sections = unwrap(d)
if isinstance(sections, list):
    print(f"Existing sections: {len(sections)}")
    for sec in sections:
        vcount = len(sec.get("vehicles", []))
        print(f"  [{sec.get('slug','?')}] {sec.get('title','?')} — {vcount} vehicles, active={sec.get('isActive', '?')}")
else:
    print(f"  Sections response: {json.dumps(sections)[:200]}")

# Create missing sections if needed
desired_sections = [
    {"title": "Vehículos Destacados", "slug": "featured", "description": "Los mejores vehículos del momento", "displayOrder": 1, "maxVehicles": 10, "isActive": True},
    {"title": "Recomendados Para Ti", "slug": "recommended", "description": "Vehículos recomendados basados en tus preferencias", "displayOrder": 2, "maxVehicles": 8, "isActive": True},
    {"title": "Mejores Ofertas", "slug": "best-deals", "description": "Las mejores ofertas en vehículos", "displayOrder": 3, "maxVehicles": 8, "isActive": True},
    {"title": "Recién Llegados", "slug": "new-arrivals", "description": "Nuevos vehículos agregados recientemente", "displayOrder": 4, "maxVehicles": 6, "isActive": True},
]

existing_slugs = set()
if isinstance(sections, list):
    existing_slugs = {s.get("slug", "") for s in sections}

for sec_def in desired_sections:
    if sec_def["slug"] in existing_slugs:
        print(f"  → Section '{sec_def['slug']}' exists, skipping create")
        continue
    s, d = http(BASE + "/homepagesections", sec_def, ah(admin_tok))
    if s in (200, 201):
        print(f"  ✓ Created section: {sec_def['title']}")
    else:
        print(f"  ✗ Create section '{sec_def['slug']}': {s} - {json.dumps(d)[:120]}")

# Assign vehicles to sections
print("\n── 4. Assign Vehicles to Sections ──")
section_slugs = ["featured", "recommended", "best-deals", "new-arrivals"]
for slug in section_slugs:
    # Assign up to 5 vehicles per section
    vids_for_section = vehicle_ids[:5]
    if not vids_for_section:
        print(f"  ✗ No vehicles to assign to '{slug}'")
        continue

    # Try bulk assign
    s, d = http(BASE + f"/homepagesections/{slug}/vehicles/bulk",
                {"vehicleIds": vids_for_section},
                ah(admin_tok))
    if s in (200, 201, 204):
        print(f"  ✓ Bulk assigned {len(vids_for_section)} vehicles to '{slug}'")
    else:
        # Try one by one
        assigned = 0
        for vid in vids_for_section:
            s2, d2 = http(BASE + f"/homepagesections/{slug}/vehicles",
                          {"vehicleId": vid},
                          ah(admin_tok))
            if s2 in (200, 201, 204):
                assigned += 1
            else:
                print(f"    ✗ Assign {vid[:8]} to '{slug}': {s2} - {json.dumps(d2)[:80]}")
                break
        if assigned > 0:
            print(f"  ✓ Assigned {assigned}/{len(vids_for_section)} vehicles to '{slug}'")
        elif s != 200:
            print(f"  ✗ Bulk assign to '{slug}': {s} - {json.dumps(d)[:120]}")

# ── 5. Advertising Campaigns ──
print("\n── 5. Advertising Campaigns ──")
now = datetime.datetime.utcnow()
start = now.strftime("%Y-%m-%dT%H:%M:%SZ")
end = (now + datetime.timedelta(days=7)).strftime("%Y-%m-%dT%H:%M:%SZ")

for i, vid in enumerate(vehicle_ids[:3]):
    title = next((v.get("title","?") for v in vehicles if v.get("id") == vid), "?")
    placement = i  # 0=FeaturedSpot, 1=PremiumSpot
    campaign = {
        "ownerId": dealer_id,
        "ownerType": "Dealer",
        "vehicleId": vid,
        "placementType": min(placement, 1),
        "pricingModel": 1,  # FixedDaily
        "totalBudget": 500.0,
        "startDate": start,
        "endDate": end,
    }
    s, d = http(BASE + "/advertising/campaigns", campaign, ah(dealer_tok))
    if s in (200, 201):
        cid = unwrap(d)
        print(f"  ✓ Campaign for {title}: {cid}")
    else:
        print(f"  ✗ Campaign for {title}: {s} - {json.dumps(d)[:100]}")

# ── 6. Advertising Rotation Status ──
print("\n── 6. Ad Rotation Status ──")
for slot in ["FeaturedSpot", "PremiumSpot", "Carousel", "Banner"]:
    s, d = http(BASE + f"/advertising/rotation/{slot}")
    r = unwrap(d)
    if isinstance(r, list):
        print(f"  {slot}: {len(r)} ads")
    elif isinstance(r, dict):
        ads = r.get("ads", r.get("items", []))
        print(f"  {slot}: {len(ads) if isinstance(ads, list) else 'N/A'} ads - {json.dumps(r)[:100]}")
    else:
        print(f"  {slot}: HTTP {s} - {json.dumps(d)[:80]}")

# ── 7. Homepage Categories & Brands ──
print("\n── 7. Homepage Config ──")
s, d = http(BASE + "/advertising/homepage/categories?activeOnly=false")
cats = unwrap(d)
print(f"  Categories: {json.dumps(cats)[:200]}" if cats else f"  Categories: HTTP {s}")

s, d = http(BASE + "/advertising/homepage/brands?activeOnly=false")
brands = unwrap(d)
print(f"  Brands: {json.dumps(brands)[:200]}" if brands else f"  Brands: HTTP {s}")

# ── 8. Pricing Estimates ──
print("\n── 8. Pricing Estimates ──")
s, d = http(BASE + "/advertising/reports/pricing")
print(f"  HTTP {s}: {json.dumps(unwrap(d))[:200]}")

# ── 9. Final Featured Check ──
print("\n── 9. Final Featured Vehicles ──")
s, d = http(BASE + "/vehicles/featured?count=20")
final_featured = unwrap(d)
fl = final_featured if isinstance(final_featured, list) else []
print(f"Total featured: {len(fl)}")
for v in fl[:10]:
    print(f"  ★ {v.get('title','?')} (${v.get('price','?')})")

# ── 10. Final Homepage Sections ──
print("\n── 10. Final Homepage ──")
s, d = http(BASE + "/homepagesections/homepage")
final_sections = unwrap(d)
if isinstance(final_sections, list):
    for sec in final_sections:
        vcount = len(sec.get("vehicles", []))
        print(f"  [{sec.get('slug','?')}] {sec.get('title','?')} — {vcount} vehicles")
        for v in sec.get("vehicles", [])[:3]:
            print(f"    - {v.get('title', v.get('vehicleTitle', '?'))}")

print("\n" + "=" * 60)
print("HOMEPAGE FILL AUDIT COMPLETE")
print("=" * 60)

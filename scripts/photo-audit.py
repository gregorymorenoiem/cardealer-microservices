#!/usr/bin/env python3
"""
OKLA Production Photo Audit & Fix Script v2
============================================
1. Tests all four account types (admin, buyer, dealer, seller)
2. Fetches ALL vehicle listings (paginated)
3. Identifies vehicles with no images or broken primary image URLs
4. Fixes them via POST /api/vehicles/{id}/images with appropriate Unsplash photos
5. Audits advertising rotation slots for empty photo slots

CSRF: uses Double-Submit Cookie pattern (send matching csrf_token cookie + X-CSRF-Token header)
"""
import json
import secrets
import urllib.request
import urllib.error
import sys

PROD_URL = "https://okla.com.do"

ACCOUNTS = {
    "admin":  {"email": "admin@okla.local",       "password": "Admin123!@#"},
    "buyer":  {"email": "buyer002@okla-test.com",  "password": "BuyerTest2026!"},
    "dealer": {"email": "nmateo@okla.com.do",      "password": "Dealer2026!@#"},
    "seller": {"email": "gmoreno@okla.com.do",     "password": "$Gregory1"},
}

FALLBACK_IMAGES = {
    "toyota":      "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&q=80",
    "honda":       "https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?w=800&q=80",
    "ford":        "https://images.unsplash.com/photo-1612825173281-9a193378527e?w=800&q=80",
    "chevrolet":   "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&q=80",
    "nissan":      "https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800&q=80",
    "hyundai":     "https://images.unsplash.com/photo-1617469767253-70a026ef7ed5?w=800&q=80",
    "kia":         "https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&q=80",
    "bmw":         "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&q=80",
    "mercedes":    "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&q=80",
    "audi":        "https://images.unsplash.com/photo-1606152421802-db97b9c7a11b?w=800&q=80",
    "jeep":        "https://images.unsplash.com/photo-1606220838315-056192d5e927?w=800&q=80",
    "ram":         "https://images.unsplash.com/photo-1611867626292-e7ad37b3f282?w=800&q=80",
    "mitsubishi":  "https://images.unsplash.com/photo-1559768713-bd0ed1fa4dbb?w=800&q=80",
    "suv":         "https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=800&q=80",
    "sedan":       "https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800&q=80",
    "pickup":      "https://images.unsplash.com/photo-1611867626292-e7ad37b3f282?w=800&q=80",
    "truck":       "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800&q=80",
    "crossover":   "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800&q=80",
    "hatchback":   "https://images.unsplash.com/photo-1617469767253-70a026ef7ed5?w=800&q=80",
    "coupe":       "https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=800&q=80",
    "minivan":     "https://images.unsplash.com/photo-1559416523-140ddc3d238c?w=800&q=80",
    "convertible": "https://images.unsplash.com/photo-1571607388263-1044f9ea01dd?w=800&q=80",
    "default":     "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&q=80",
}

def pick_fallback(make: str, body_style: str) -> str:
    make_key = (make or "").lower().split()[0]
    body_key = (body_style or "").lower().replace("-", "").replace(" ", "")
    return (
        FALLBACK_IMAGES.get(make_key)
        or FALLBACK_IMAGES.get(body_key)
        or FALLBACK_IMAGES["default"]
    )

def api_get(path, token=None):
    url = f"{PROD_URL}{path}"
    headers = {"Accept": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(url, headers=headers)
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            return json.loads(resp.read().decode())
    except Exception as ex:
        print(f"  [GET ERR] {path}: {ex}", file=sys.stderr)
        return None

def api_post(path, data, token, csrf_token):
    """POST with CSRF double-submit cookie pattern."""
    url = f"{PROD_URL}{path}"
    headers = {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Authorization": f"Bearer {token}",
        "X-CSRF-Token": csrf_token,
        "Cookie": f"csrf_token={csrf_token}",
        "Origin": "https://okla.com.do",
        "Referer": "https://okla.com.do/",
    }
    body = json.dumps(data).encode()
    req = urllib.request.Request(url, data=body, headers=headers, method="POST")
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            content = resp.read().decode()
            return json.loads(content) if content.strip() else {}
    except urllib.error.HTTPError as e:
        body_text = e.read().decode()
        print(f"  [POST {e.code}] {path}: {body_text[:150]}", file=sys.stderr)
        return None
    except Exception as ex:
        print(f"  [POST ERR] {path}: {ex}", file=sys.stderr)
        return None

def check_image_url(url: str) -> bool:
    if not url or url.startswith("blob:") or url.startswith("data:"):
        return False
    try:
        req = urllib.request.Request(
            url, method="HEAD",
            headers={"User-Agent": "Mozilla/5.0 OKLA-Audit/1.0"}
        )
        with urllib.request.urlopen(req, timeout=7) as r:
            ct = r.headers.get("Content-Type", "")
            return r.status < 400 and ("image" in ct or "octet" in ct)
    except Exception:
        return False

def do_login(key):
    acc = ACCOUNTS[key]
    url = f"{PROD_URL}/api/auth/login"
    body = json.dumps(acc).encode()
    req = urllib.request.Request(url, data=body,
                                 headers={"Content-Type": "application/json",
                                          "Accept": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=10) as r:
            resp = json.loads(r.read().decode())
        if resp and resp.get("data", {}).get("accessToken"):
            return resp["data"]["accessToken"]
        return None
    except Exception as ex:
        print(f"  Login {key} failed: {ex}")
        return None

def main():
    print("=" * 62)
    print("   OKLA PRODUCTION PHOTO AUDIT v2")
    print("=" * 62)
    print()

    # STEP 1: Test all account types
    print("[ STEP 1 ] Testing all account types...")
    tokens = {}
    for key in ACCOUNTS:
        t = do_login(key)
        if t:
            print(f"  \u2705 {key:8s} authenticated OK")
            tokens[key] = t
        else:
            print(f"  \u274c {key:8s} FAILED")
    print()

    admin_token = tokens.get("admin")
    if not admin_token:
        print("ERROR: Cannot continue without admin token.")
        sys.exit(1)

    csrf = secrets.token_hex(32)
    print(f"  CSRF token generated: {csrf[:16]}...")
    print()

    # STEP 2: Fetch all vehicles
    print("[ STEP 2 ] Fetching all vehicles (paginated)...")
    all_vehicles = []
    seen_ids: set = set()
    page = 1
    total = 999
    while len(all_vehicles) < total:
        resp = api_get(
            f"/api/vehicles?limit=20&page={page}&pageSize=20&sortBy=newest",
            token=admin_token
        )
        if not resp:
            break
        batch = resp.get("vehicles") or resp.get("data") or resp.get("items") or []
        if not batch:
            break
        total = (resp.get("pagination") or {}).get("totalItems") \
                or resp.get("totalCount") or resp.get("total") or total
        new = [v for v in batch if v.get("id") not in seen_ids]
        if not new:
            break  # no new records — done
        for v in new:
            seen_ids.add(v["id"])
        all_vehicles.extend(new)
        print(f"  Page {page}: +{len(new)} (total: {total})")
        page += 1

    print(f"  Total fetched: {len(all_vehicles)}")
    print()

    # STEP 3: Audit image health
    print(f"[ STEP 3 ] Auditing images on {len(all_vehicles)} vehicles...")
    no_images = []
    broken = []
    ok = []

    for v in all_vehicles:
        vid = v.get("id", "?")
        title = v.get("title") or f"{v.get('year','')} {v.get('make','')} {v.get('model','')}".strip()
        images = v.get("images") or []
        real = [
            i for i in images
            if isinstance(i, dict)
            and i.get("url")
            and not str(i["url"]).startswith("blob:")
            and not str(i["url"]).startswith("data:")
        ]
        if not real:
            print(f"  [NO_IMG] {vid[:8]} {title}")
            no_images.append(v)
            continue

        primary = sorted(real, key=lambda i: (0 if i.get("isPrimary") else 1, i.get("sortOrder", 99)))[0]
        url = primary["url"]
        if check_image_url(url):
            ok.append(v)
        else:
            print(f"  [BROKEN] {vid[:8]} {title}  \u2192  {url[:65]}...")
            broken.append(v)

    problems = no_images + broken
    print()
    print(f"  OK:        {len(ok)}")
    print(f"  No images: {len(no_images)}")
    print(f"  Broken:    {len(broken)}")
    print(f"  To fix:    {len(problems)}")
    print()

    # STEP 4: Fix broken/missing images
    if not problems:
        print("[ STEP 4 ] No image problems found!")
    else:
        print(f"[ STEP 4 ] Adding fallback images to {len(problems)} vehicles...")
        fixed = 0
        for v in problems:
            vid = v["id"]
            make = (v.get("make") or "").strip()
            body_style = str(v.get("bodyStyle") or v.get("vehicleType") or "").strip()
            title = v.get("title") or f"{v.get('year','')} {make} {v.get('model','')}".strip()
            img_url = pick_fallback(make, body_style)

            payload = {
                "images": [{
                    "url": img_url,
                    "thumbnailUrl": img_url.replace("w=800", "w=200"),
                    "isPrimary": True,
                    "sortOrder": 0,
                    "imageType": "Exterior"
                }]
            }

            result = api_post(
                f"/api/vehicles/{vid}/images",
                payload, admin_token, csrf
            )
            if result is not None:
                tag = make or body_style or "generic"
                print(f"  [FIXED]  {vid[:8]} {title}  ({tag})")
                fixed += 1
            else:
                print(f"  [SKIP]   {vid[:8]} {title}")

        print(f"\n  Fixed: {fixed}/{len(problems)}")
    print()

    # STEP 5: Audit advertising rotation
    print("[ STEP 5 ] Auditing advertising rotation slots...")
    for placement in ["FeaturedSpot", "PremiumSpot"]:
        resp = api_get(
            f"/api/advertising/rotation?placementType={placement}&limit=20",
            token=admin_token
        )
        if not resp:
            print(f"  [{placement}] not reachable")
            continue
        items = resp.get("items") or resp.get("data") or []
        items = [i for i in items if isinstance(i, dict)]
        missing = [i for i in items if not i.get("imageUrl") or str(i.get("imageUrl","")).startswith("blob:")]
        ok_count = len(items) - len(missing)
        print(f"  [{placement}] Total: {len(items)}  OK: {ok_count}  No-image: {len(missing)}")
        for item in missing:
            vid = (item.get("vehicleId") or "?")[:8]
            print(f"    \u26a0\ufe0f  {vid} {item.get('title','?')} \u2014 no image in rotation slot")
    print()

    # STEP 6: Account data access check
    print("[ STEP 6 ] Data access by account type:")
    for key, tok in tokens.items():
        resp = api_get("/api/vehicles?limit=3", token=tok)
        count = len(resp.get("vehicles") or resp.get("data") or []) if resp else 0
        status = "\u2705" if resp else "\u274c"
        print(f"  {status} {key:8s} /api/vehicles \u2192 {count} vehicles")

    print()
    print("=" * 62)
    print("   AUDIT COMPLETE")
    print("=" * 62)

if __name__ == "__main__":
    main()


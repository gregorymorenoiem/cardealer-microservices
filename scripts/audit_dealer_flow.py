#!/usr/bin/env python3
"""
OKLA Dealer Publication Flow Audit Script
Tests: Dealer publishes → Admin reviews → Admin approves → Dealer advertises
"""
import json
import time
import sys
import ssl
import urllib.request
import urllib.error
from datetime import datetime

BASE_URL = "https://okla.com.do/api"
CSRF_TOKEN = "abcdef0123456789abcdef0123456789"
ssl_ctx = ssl.create_default_context()

DEALER_EMAIL = "nmateo@okla.com.do"
DEALER_PASS = "Dealer2026!@#"
ADMIN_EMAIL = "admin@okla.local"
ADMIN_PASS = "Admin123!@#"
BUYER_EMAIL = "buyer002@okla-test.com"
BUYER_PASS = "BuyerTest2026!"


def http_request(url, data=None, headers=None, method="GET", timeout=15):
    hdrs = {
        "Content-Type": "application/json",
        "X-CSRF-Token": CSRF_TOKEN,
        "Cookie": f"csrf_token={CSRF_TOKEN}",
    }
    if headers:
        hdrs.update(headers)

    body = json.dumps(data).encode("utf-8") if data else None
    req = urllib.request.Request(url, data=body, headers=hdrs, method=method)
    try:
        with urllib.request.urlopen(req, timeout=timeout, context=ssl_ctx) as resp:
            raw = resp.read().decode("utf-8")
            return resp.status, json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        try:
            body = json.loads(e.read().decode("utf-8"))
        except:
            body = {"error": str(e)}
        return e.code, body
    except Exception as e:
        return 0, {"error": str(e)}


def login(email, password):
    print(f"  Logging in as {email}...")
    status, data = http_request(f"{BASE_URL}/auth/login", {"email": email, "password": password}, method="POST")
    if status == 200:
        token = data.get("data", {}).get("accessToken", "")
        user_id = data.get("data", {}).get("userId", "")
        if token:
            print(f"  ✓ Authenticated (userId: {user_id})")
            return token, user_id
    print(f"  ✗ Login failed: {status} {json.dumps(data)[:200]}")
    return None, None


def auth_headers(token):
    return {
        "Authorization": f"Bearer {token}",
        "Cookie": f"csrf_token={CSRF_TOKEN}; okla_access_token={token}",
    }


def audit_step(step_num, description, func):
    print(f"\n{'─'*60}")
    print(f"  Step {step_num}: {description}")
    print(f"{'─'*60}")
    try:
        result = func()
        return result
    except Exception as e:
        print(f"  ✗ Error: {e}")
        return None


def main():
    print("=" * 60)
    print("OKLA Dealer Publication Flow Audit")
    print(f"Time: {datetime.now().isoformat()}")
    print("=" * 60)

    # ── Step 1: Authenticate all actors ──
    dealer_token, dealer_id = login(DEALER_EMAIL, DEALER_PASS)
    admin_token, admin_id = login(ADMIN_EMAIL, ADMIN_PASS)
    buyer_token, buyer_id = login(BUYER_EMAIL, BUYER_PASS)

    if not all([dealer_token, admin_token]):
        print("\n✗ Cannot proceed without dealer and admin tokens")
        return

    # ── Step 2: Check existing dealer vehicles ──
    def check_dealer_vehicles():
        status, data = http_request(
            f"{BASE_URL}/vehicles/seller/{dealer_id}",
            headers=auth_headers(dealer_token)
        )
        if status == 200:
            vehicles = data.get("data", data) if isinstance(data, dict) else data
            if isinstance(vehicles, list):
                print(f"  Dealer has {len(vehicles)} existing vehicles")
                for v in vehicles[:5]:
                    vid = v.get("id", "?")
                    title = v.get("title", v.get("name", "?"))
                    st = v.get("status", v.get("statusName", "?"))
                    print(f"    - {vid}: {title} [{st}]")
                return vehicles
            elif isinstance(vehicles, dict) and vehicles.get("items"):
                items = vehicles["items"]
                print(f"  Dealer has {len(items)} existing vehicles")
                for v in items[:5]:
                    print(f"    - {v.get('id','?')}: {v.get('title','?')} [{v.get('status','?')}]")
                return items
        else:
            print(f"  HTTP {status}: {json.dumps(data)[:200]}")
        return []

    existing = audit_step(2, "Check existing dealer vehicles", check_dealer_vehicles)

    # ── Step 3: Check vehicle settings/limits ──
    def check_settings():
        status, data = http_request(
            f"{BASE_URL}/vehicles/settings",
            headers=auth_headers(dealer_token)
        )
        print(f"  Settings response ({status}): {json.dumps(data)[:300]}")
        return data

    audit_step(3, "Check vehicle settings/limits", check_settings)

    # ── Step 4: Get moderation queue (Admin) ──
    def check_moderation():
        # Try direct endpoint
        status, data = http_request(
            f"{BASE_URL}/vehicles/moderation/queue",
            headers=auth_headers(admin_token)
        )
        if status == 200:
            queue = data.get("data", data) if isinstance(data, dict) else data
            if isinstance(queue, list):
                print(f"  Moderation queue: {len(queue)} items pending")
                for item in queue[:5]:
                    print(f"    - {item.get('id','?')}: {item.get('title','?')}")
            else:
                print(f"  Queue response: {json.dumps(data)[:200]}")
        else:
            print(f"  HTTP {status}: {json.dumps(data)[:200]}")

        # Try AdminService moderation
        status2, data2 = http_request(
            f"{BASE_URL}/admin/moderation/queue",
            headers=auth_headers(admin_token)
        )
        print(f"  AdminService queue ({status2}): {json.dumps(data2)[:200]}")

        return data

    audit_step(4, "Check admin moderation queue", check_moderation)

    # ── Step 5: Check admin vehicle list ──
    def check_admin_vehicles():
        status, data = http_request(
            f"{BASE_URL}/admin/vehicles",
            headers=auth_headers(admin_token)
        )
        if status == 200:
            result = data.get("data", data)
            if isinstance(result, dict) and result.get("items"):
                print(f"  Admin sees {len(result['items'])} vehicles (total: {result.get('totalCount', '?')})")
                for v in result["items"][:5]:
                    print(f"    - {v.get('id','?')}: {v.get('title','?')} [{v.get('status','?')}]")
            elif isinstance(result, list):
                print(f"  Admin sees {len(result)} vehicles")
            else:
                print(f"  Response: {json.dumps(data)[:200]}")
        else:
            print(f"  HTTP {status}: {json.dumps(data)[:200]}")
        return data

    audit_step(5, "Check admin vehicle list", check_admin_vehicles)

    # ── Step 6: Try creating a test vehicle ──
    def create_vehicle():
        vehicle_data = {
            "title": "Toyota Corolla 2023 - Test Audit",
            "description": "Vehículo de prueba para auditoría del flujo de publicación. Toyota Corolla 2023, automático, gasolina, color blanco perla.",
            "make": "Toyota",
            "model": "Corolla",
            "year": 2023,
            "price": 850000,
            "currency": "DOP",
            "mileage": 15000,
            "transmission": "Automatic",
            "fuelType": "Gasoline",
            "bodyType": "Sedan",
            "condition": "Used",
            "color": "White",
            "engineSize": "1.8L",
            "doors": 4,
            "seats": 5,
            "driveType": "FWD",
            "contactPhone": "+18091234567",
            "contactEmail": DEALER_EMAIL,
            "location": "Santo Domingo",
            "features": ["AC", "Power Windows", "Backup Camera", "Bluetooth"],
        }

        status, data = http_request(
            f"{BASE_URL}/vehicles",
            vehicle_data,
            auth_headers(dealer_token),
            method="POST",
            timeout=20
        )
        if status in (200, 201):
            vid = data.get("data", {}).get("id", "") or data.get("id", "")
            print(f"  ✓ Vehicle created: {vid}")
            print(f"  Response: {json.dumps(data)[:300]}")
            return vid
        else:
            print(f"  ✗ Create failed ({status}): {json.dumps(data)[:300]}")
            return None

    vehicle_id = audit_step(6, "Create test vehicle (Draft)", create_vehicle)

    # ── Step 7: Submit for review ──
    if vehicle_id:
        def submit_for_review():
            status, data = http_request(
                f"{BASE_URL}/vehicles/{vehicle_id}/publish",
                {},
                auth_headers(dealer_token),
                method="POST"
            )
            if status in (200, 204):
                print(f"  ✓ Vehicle submitted for review")
            else:
                print(f"  ✗ Submit failed ({status}): {json.dumps(data)[:200]}")
            return status

        audit_step(7, "Submit vehicle for review (Draft → PendingReview)", submit_for_review)

        # ── Step 8: Admin approves ──
        def admin_approve():
            status, data = http_request(
                f"{BASE_URL}/vehicles/{vehicle_id}/approve",
                {},
                auth_headers(admin_token),
                method="POST"
            )
            if status in (200, 204):
                print(f"  ✓ Vehicle approved by admin")
            else:
                print(f"  ✗ Approve failed ({status}): {json.dumps(data)[:200]}")
            return status

        audit_step(8, "Admin approves vehicle (PendingReview → Active)", admin_approve)

    # ── Step 9: Check advertising system ──
    def check_advertising():
        # Check homepage rotation
        for slot in ["FeaturedSpot", "PremiumSpot", "Carousel", "Banner"]:
            status, data = http_request(
                f"{BASE_URL}/advertising/rotation/{slot}",
                headers=auth_headers(dealer_token)
            )
            result = data.get("data", data) if isinstance(data, dict) else data
            count = len(result) if isinstance(result, list) else "?"
            print(f"  Slot '{slot}': HTTP {status}, items: {count}")

        # Check pricing
        status, data = http_request(
            f"{BASE_URL}/advertising/pricing/estimate?placementType=FeaturedSpot&days=7",
            headers=auth_headers(dealer_token)
        )
        print(f"  Pricing estimate: {status} {json.dumps(data)[:200]}")

        # Check categories
        status, data = http_request(
            f"{BASE_URL}/advertising/categories",
            headers=auth_headers(dealer_token)
        )
        print(f"  Categories: {status} {json.dumps(data)[:200]}")

        return True

    audit_step(9, "Check advertising system status", check_advertising)

    # ── Step 10: Create ad campaign ──
    if vehicle_id:
        def create_campaign():
            campaign_data = {
                "vehicleId": vehicle_id,
                "ownerId": dealer_id,
                "placementType": "FeaturedSpot",
                "pricingModel": "FixedDaily",
                "budget": 500,
                "currency": "DOP",
                "durationDays": 7
            }
            status, data = http_request(
                f"{BASE_URL}/advertising/campaigns",
                campaign_data,
                auth_headers(dealer_token),
                method="POST",
                timeout=15
            )
            if status in (200, 201):
                cid = data.get("data", {}).get("id", "") or data.get("id", "")
                print(f"  ✓ Campaign created: {cid}")
            else:
                print(f"  Campaign creation ({status}): {json.dumps(data)[:300]}")
            return data

        audit_step(10, "Create advertising campaign", create_campaign)

    # ── Step 11: Check homepage for featured vehicles ──
    def check_homepage_featured():
        status, data = http_request(
            f"{BASE_URL}/vehicles/featured",
            headers=auth_headers(buyer_token) if buyer_token else {}
        )
        if status == 200:
            result = data.get("data", data)
            if isinstance(result, list):
                print(f"  Featured vehicles: {len(result)}")
                for v in result[:5]:
                    print(f"    - {v.get('title', '?')} (${v.get('price', '?')})")
            else:
                print(f"  Response: {json.dumps(data)[:200]}")
        else:
            print(f"  HTTP {status}: {json.dumps(data)[:200]}")
        return data

    audit_step(11, "Check homepage featured vehicles", check_homepage_featured)

    # ── Step 12: Check vehicle search/listing ──
    def check_search():
        status, data = http_request(
            f"{BASE_URL}/vehicles/search?page=1&pageSize=10",
            headers=auth_headers(buyer_token) if buyer_token else {}
        )
        if status == 200:
            result = data.get("data", data)
            if isinstance(result, dict) and result.get("items"):
                print(f"  Search results: {len(result['items'])} vehicles (total: {result.get('totalCount', '?')})")
            elif isinstance(result, list):
                print(f"  Search results: {len(result)} vehicles")
            else:
                print(f"  Response: {json.dumps(data)[:200]}")
        else:
            print(f"  HTTP {status}: {json.dumps(data)[:200]}")
        return data

    audit_step(12, "Check vehicle search/listing", check_search)

    # ── Summary ──
    print("\n" + "=" * 60)
    print("AUDIT SUMMARY")
    print("=" * 60)
    print(f"  Dealer Auth: {'✓' if dealer_token else '✗'}")
    print(f"  Admin Auth: {'✓' if admin_token else '✗'}")
    print(f"  Buyer Auth: {'✓' if buyer_token else '✗'}")
    print(f"  Vehicle Created: {'✓' if vehicle_id else '✗'}")
    print("=" * 60)


if __name__ == "__main__":
    main()

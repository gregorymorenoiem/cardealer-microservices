#!/usr/bin/env python3
"""Verify all Unsplash URLs in vehicle-image-fallbacks.ts are accessible."""
import urllib.request

urls = [
    "https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&q=75",
    "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&q=75",
    "https://images.unsplash.com/photo-1568844293986-8d0400f4f36d?w=800&q=75",
    "https://images.unsplash.com/photo-1606611013016-969c19ba27c5?w=800&q=75",
    "https://images.unsplash.com/photo-1583121274602-3e2820c69888?w=800&q=75",
    "https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?w=800&q=75",
    "https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=800&q=75",
    "https://images.unsplash.com/photo-1625231334168-32354e3b4f4b?w=800&q=75",
    "https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800&q=75",
    "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&q=75",
    "https://images.unsplash.com/photo-1542362567-b07e54358753?w=800&q=75",
    "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&q=75",
    "https://images.unsplash.com/photo-1533473359331-2969f3c6b787?w=800&q=75",
    "https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=800&q=75",
    "https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=800&q=75",
    "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&q=75",
    "https://images.unsplash.com/photo-1616422285623-13ff0162193c?w=800&q=75",
    "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&q=75",
    "https://images.unsplash.com/photo-1556189250-72ba954cfc2b?w=800&q=75",
    "https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800&q=75",
    "https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&q=75",
    "https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?w=800&q=75",
    "https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=800&q=75",
    "https://images.unsplash.com/photo-1551830820-330a71b99659?w=800&q=75",
    "https://images.unsplash.com/photo-1553440569-bcc63803a83d?w=800&q=75",
    "https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800&q=75",
    "https://images.unsplash.com/photo-1536700503339-1e4b06520771?w=800&q=75",
    "https://images.unsplash.com/photo-1554744512-d6c603f27c54?w=800&q=75",
]

# Also check the URLs used in photo-audit.py
audit_urls = [
    "https://images.unsplash.com/photo-1617469767253-70a026ef7ed5?w=800&q=80",
    "https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?w=800&q=80",
    "https://images.unsplash.com/photo-1612825173281-9a193378527e?w=800&q=80",
    "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&q=80",
    "https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800&q=80",
    "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&q=80",
    "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&q=80",
    "https://images.unsplash.com/photo-1606152421802-db97b9c7a11b?w=800&q=80",
    "https://images.unsplash.com/photo-1606220838315-056192d5e927?w=800&q=80",
    "https://images.unsplash.com/photo-1611867626292-e7ad37b3f282?w=800&q=80",
    "https://images.unsplash.com/photo-1559768713-bd0ed1fa4dbb?w=800&q=80",
]

print("=== Checking fallback URLs (vehicle-image-fallbacks.ts) ===")
ok1 = broken1 = 0
for url in urls:
    photo_id = url.split("/")[-1].split("?")[0]
    try:
        req = urllib.request.Request(url, method="HEAD", headers={"User-Agent": "Mozilla/5.0"})
        with urllib.request.urlopen(req, timeout=8) as r:
            ct = r.headers.get("Content-Type", "")
            if r.status < 400 and "image" in ct:
                ok1 += 1
            else:
                print(f"  BROKEN ({r.status}): {photo_id}")
                broken1 += 1
    except Exception as e:
        err = str(e)[:60]
        print(f"  BROKEN ({err}): {photo_id}")
        broken1 += 1
print(f"  Result: {ok1} OK / {broken1} broken out of {len(urls)}\n")

print("=== Checking audit script URLs (photo-audit.py) ===")
ok2 = broken2 = 0
for url in audit_urls:
    photo_id = url.split("/")[-1].split("?")[0]
    try:
        req = urllib.request.Request(url, method="HEAD", headers={"User-Agent": "Mozilla/5.0"})
        with urllib.request.urlopen(req, timeout=8) as r:
            ct = r.headers.get("Content-Type", "")
            if r.status < 400 and "image" in ct:
                ok2 += 1
            else:
                print(f"  BROKEN ({r.status}): {photo_id}")
                broken2 += 1
    except Exception as e:
        err = str(e)[:60]
        print(f"  BROKEN ({err}): {photo_id}")
        broken2 += 1
print(f"  Result: {ok2} OK / {broken2} broken out of {len(audit_urls)}")

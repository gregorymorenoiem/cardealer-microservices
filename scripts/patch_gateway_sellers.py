#!/usr/bin/env python3
"""
Patches gateway-config ConfigMap to add /api/sellers/* routes.
Usage: python3 patch_gateway_sellers.py
"""
import json, re

INPUT  = "/tmp/gateway-config-current.json"
OUTPUT = "/tmp/gateway-config-patched.json"

def strip_json_comments(text):
    """Remove // line comments and /* block comments */ from a JSON string."""
    # Remove block comments
    text = re.sub(r'/\*.*?\*/', '', text, flags=re.DOTALL)
    # Remove line comments (// ...) but not inside strings
    result = []
    in_string = False
    i = 0
    while i < len(text):
        c = text[i]
        if c == '"' and (i == 0 or text[i-1] != '\\'):
            in_string = not in_string
            result.append(c)
        elif not in_string and c == '/' and i+1 < len(text) and text[i+1] == '/':
            # Skip until end of line
            while i < len(text) and text[i] != '\n':
                i += 1
            continue
        else:
            result.append(c)
        i += 1
    return ''.join(result)

with open(INPUT) as f:
    cm = json.load(f)

raw = cm["data"]["ocelot.json"]
config = json.loads(strip_json_comments(raw))

has_sellers = any("sellers" in r.get("UpstreamPathTemplate", "") for r in config["Routes"])
if has_sellers:
    print("sellers routes already exist — nothing to patch.")
    exit(0)

sellers_routes = [
    {
        "UpstreamPathTemplate": "/api/sellers/convert",
        "UpstreamHttpMethod": ["POST", "OPTIONS"],
        "DownstreamPathTemplate": "/api/sellers/convert",
        "DownstreamScheme": "http",
        "DownstreamHostAndPorts": [{"Host": "userservice", "Port": 8080}],
        "AuthenticationOptions": {"AuthenticationProviderKey": "Bearer"}
    },
    {
        "UpstreamPathTemplate": "/api/sellers/user/{userId}",
        "UpstreamHttpMethod": ["GET", "OPTIONS"],
        "DownstreamPathTemplate": "/api/sellers/user/{userId}",
        "DownstreamScheme": "http",
        "DownstreamHostAndPorts": [{"Host": "userservice", "Port": 8080}],
        "AuthenticationOptions": {"AuthenticationProviderKey": "Bearer"}
    },
    {
        "UpstreamPathTemplate": "/api/sellers/{sellerId}/stats",
        "UpstreamHttpMethod": ["GET", "OPTIONS"],
        "DownstreamPathTemplate": "/api/sellers/{sellerId}/stats",
        "DownstreamScheme": "http",
        "DownstreamHostAndPorts": [{"Host": "userservice", "Port": 8080}],
        "AuthenticationOptions": {"AuthenticationProviderKey": "Bearer"}
    },
    {
        "UpstreamPathTemplate": "/api/sellers/{everything}",
        "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
        "DownstreamPathTemplate": "/api/sellers/{everything}",
        "DownstreamScheme": "http",
        "DownstreamHostAndPorts": [{"Host": "userservice", "Port": 8080}],
        "AuthenticationOptions": {"AuthenticationProviderKey": "Bearer"}
    },
    {
        "UpstreamPathTemplate": "/api/sellers",
        "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
        "DownstreamPathTemplate": "/api/sellers",
        "DownstreamScheme": "http",
        "DownstreamHostAndPorts": [{"Host": "userservice", "Port": 8080}],
        "AuthenticationOptions": {"AuthenticationProviderKey": "Bearer"}
    }
]

# Insert after /api/users routes (before /api/roles)
insert_idx = next(
    (i for i, r in enumerate(config["Routes"])
     if r.get("UpstreamPathTemplate", "").startswith("/api/roles")),
    2
)

for offset, route in enumerate(sellers_routes):
    config["Routes"].insert(insert_idx + offset, route)

print(f"Inserted {len(sellers_routes)} sellers routes at index {insert_idx}")
print(f"Total routes: {len(config['Routes'])}")

cm["data"]["ocelot.json"] = json.dumps(config, indent=2)

with open(OUTPUT, "w") as f:
    json.dump(cm, f, indent=2)

print(f"Patched configmap saved to {OUTPUT}")

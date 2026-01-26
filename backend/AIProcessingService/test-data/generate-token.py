#!/usr/bin/env python3
"""
Generate a test JWT token for AIProcessingService testing
"""
import jwt
import datetime

# Configuration from Program.cs defaults
SECRET = "OKLA-SuperSecretKey-2026-CarDealer-Microservices-256bit"
ISSUER = "OKLA"
AUDIENCE = "OKLA-Users"

payload = {
    "sub": "test-user-001",
    "name": "Test User",
    "email": "test@okla.com.do",
    "role": "Admin",  # Admin role to access all endpoints
    "iat": datetime.datetime.utcnow(),
    "exp": datetime.datetime.utcnow() + datetime.timedelta(hours=24),
    "iss": ISSUER,
    "aud": AUDIENCE
}

token = jwt.encode(payload, SECRET, algorithm="HS256")
print(f"Bearer {token}")

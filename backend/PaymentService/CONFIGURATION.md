# 🔧 Configuración de PaymentService

## appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "PaymentService": "Debug"
    }
  },

  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=payment_service;Username=postgres;Password=your_password"
  },

  "PaymentGateway": {
    // Pasarela por defecto (Azul, CardNET, PixelPay, Fygaro)
    "Default": "Azul",

    // ============== AZUL (Banco Popular RD) ==============
    "Azul": {
      "MerchantId": "your_merchant_id",
      "AuthKey": "your_auth_key",
      "CyberSourceSecretKey": "your_cybersource_secret_key",
      "Endpoint": "https://api.azul.com.do/api/1.0",
      "TestMode": false,
      "Commission": {
        "Percentage": 3.5,
        "FixedAmount": 10.0,
        "Currency": "DOP"
      },
      "MonthlyCost": 40.0,
      "Timeouts": {
        "ConnectionTimeoutMs": 30000,
        "ReadTimeoutMs": 30000
      },
      "Retries": {
        "MaxAttempts": 3,
        "DelayMs": 1000
      }
    },

    // ============== CardNET (Bancaria RD) ==============
    "CardNET": {
      "TerminalId": "your_terminal_id",
      "APIKey": "your_api_key",
      "Endpoint": "https://api.cardnet.com.do/v1",
      "TestMode": false,
      "Commission": {
        "Percentage": 3.2,
        "FixedAmount": 10.0,
        "Currency": "DOP"
      },
      "MonthlyCost": 40.0,
      "Timeouts": {
        "ConnectionTimeoutMs": 30000,
        "ReadTimeoutMs": 30000
      },
      "Retries": {
        "MaxAttempts": 3,
        "DelayMs": 1000
      }
    },

    // ============== PixelPay (Fintech) ==============
    "PixelPay": {
      "PublicKey": "<YOUR_PIXELPAY_PUBLIC_KEY>",
      "SecretKey": "<YOUR_PIXELPAY_SECRET_KEY>",
      "Endpoint": "https://api.pixelpay.com/v1",
      "WebhookSecret": "whsec_xxxxxxxxxxxxxxxxxxxxxxxx",
      "TestMode": false,
      "Commission": {
        "Percentage": 2.5,
        "FixedAmount": 0.25,
        "Currency": "USD"
      },
      "MonthlyCost": 0.0,
      "SupportTokenization": true,
      "Timeouts": {
        "ConnectionTimeoutMs": 20000,
        "ReadTimeoutMs": 20000
      },
      "Retries": {
        "MaxAttempts": 2,
        "DelayMs": 500
      }
    },

    // ============== Fygaro (Agregador) ==============
    "Fygaro": {
      "ApiKey": "fygaro_api_key_xxxxxxxxxxxxx",
      "Endpoint": "https://api.fygaro.com/v1",
      "SubscriptionModuleKey": "sub_key_xxxxxxxxxxxxx",
      "WebhookSecret": "webhook_secret_xxxxxxxxxxxxx",
      "TestMode": false,
      "Commission": {
        "Percentage": 3.0,
        "FixedAmount": 0.0,
        "Currency": "DOP"
      },
      "MonthlyCost": 15.0,
      "SupportSubscriptions": true,
      "Timeouts": {
        "ConnectionTimeoutMs": 25000,
        "ReadTimeoutMs": 25000
      },
      "Retries": {
        "MaxAttempts": 2,
        "DelayMs": 800
      }
    }
  },

  // Configuración de observabilidad
  "Observability": {
    "Jaeger": {
      "Endpoint": "http://localhost:14268/api/traces",
      "ServiceName": "PaymentService"
    },
    "Serilog": {
      "SeqEndpoint": "http://localhost:5341"
    }
  },

  // Configuración de RabbitMQ para eventos
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchanges": {
      "PaymentEvents": "payment.events"
    }
  },

  // Configuración de caché (Redis)
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Database": 0
  },

  // Configuración de idempotencia
  "Idempotency": {
    "Enabled": true,
    "ExpirationMinutes": 1440,
    "RedisKey": "payment:idempotency"
  },

  // Configuración de rate limiting
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerMinute": 1000,
    "BurstsAllowed": 100
  }
}
```

## appsettings.Development.json

```json
{
  "PaymentGateway": {
    "Default": "Azul",

    "Azul": {
      "MerchantId": "dev_merchant",
      "AuthKey": "dev_auth_key",
      "CyberSourceSecretKey": "dev_secret",
      "Endpoint": "https://sandbox.azul.com.do/api/1.0",
      "TestMode": true
    },

    "CardNET": {
      "TerminalId": "dev_terminal",
      "APIKey": "dev_api_key",
      "Endpoint": "https://sandbox.cardnet.com.do/v1",
      "TestMode": true
    },

    "PixelPay": {
      "PublicKey": "<YOUR_PIXELPAY_TEST_PUBLIC_KEY>",
      "SecretKey": "<YOUR_PIXELPAY_TEST_SECRET_KEY>",
      "Endpoint": "https://sandbox.pixelpay.com/v1",
      "WebhookSecret": "whsec_test_xxxxxxxxxxxxxxxxxxxxxxxx",
      "TestMode": true
    },

    "Fygaro": {
      "ApiKey": "fygaro_test_key",
      "Endpoint": "https://sandbox.fygaro.com/v1",
      "SubscriptionModuleKey": "sub_test_key",
      "WebhookSecret": "webhook_test_secret",
      "TestMode": true
    }
  },

  "Observability": {
    "Jaeger": {
      "Endpoint": "http://localhost:14268/api/traces"
    }
  },

  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },

  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

## appsettings.Production.json

```json
{
  "PaymentGateway": {
    "Default": "Azul",

    "Azul": {
      "MerchantId": "${AZUL_MERCHANT_ID}",
      "AuthKey": "${AZUL_AUTH_KEY}",
      "CyberSourceSecretKey": "${AZUL_CYBERSOURCE_SECRET}",
      "Endpoint": "https://api.azul.com.do/api/1.0",
      "TestMode": false,
      "Timeouts": {
        "ConnectionTimeoutMs": 30000,
        "ReadTimeoutMs": 30000
      }
    },

    "CardNET": {
      "TerminalId": "${CARDNET_TERMINAL_ID}",
      "APIKey": "${CARDNET_API_KEY}",
      "Endpoint": "https://api.cardnet.com.do/v1",
      "TestMode": false,
      "Timeouts": {
        "ConnectionTimeoutMs": 30000,
        "ReadTimeoutMs": 30000
      }
    },

    "PixelPay": {
      "PublicKey": "${PIXELPAY_PUBLIC_KEY}",
      "SecretKey": "${PIXELPAY_SECRET_KEY}",
      "Endpoint": "https://api.pixelpay.com/v1",
      "WebhookSecret": "${PIXELPAY_WEBHOOK_SECRET}",
      "TestMode": false,
      "Timeouts": {
        "ConnectionTimeoutMs": 20000,
        "ReadTimeoutMs": 20000
      }
    },

    "Fygaro": {
      "ApiKey": "${FYGARO_API_KEY}",
      "Endpoint": "https://api.fygaro.com/v1",
      "SubscriptionModuleKey": "${FYGARO_SUBSCRIPTION_KEY}",
      "WebhookSecret": "${FYGARO_WEBHOOK_SECRET}",
      "TestMode": false,
      "Timeouts": {
        "ConnectionTimeoutMs": 25000,
        "ReadTimeoutMs": 25000
      }
    }
  },

  "Observability": {
    "Jaeger": {
      "Endpoint": "http://jaeger-collector.observability:14268/api/traces",
      "ServiceName": "PaymentService"
    },
    "Serilog": {
      "SeqEndpoint": "http://seq.observability:5341"
    }
  },

  "RabbitMQ": {
    "Host": "${RABBITMQ_HOST}",
    "Port": 5672,
    "Username": "${RABBITMQ_USER}",
    "Password": "${RABBITMQ_PASSWORD}",
    "VirtualHost": "/"
  },

  "Redis": {
    "ConnectionString": "${REDIS_CONNECTION_STRING}"
  }
}
```

## Kubernetes ConfigMap

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: payment-service-config
  namespace: okla
data:
  appsettings.json: |
    {
      "PaymentGateway": {
        "Default": "Azul",
        "Azul": {
          "Endpoint": "https://api.azul.com.do/api/1.0",
          "TestMode": false,
          "Commission": {
            "Percentage": 3.5,
            "FixedAmount": 10.00,
            "Currency": "DOP"
          }
        },
        "PixelPay": {
          "Endpoint": "https://api.pixelpay.com/v1",
          "Commission": {
            "Percentage": 2.5,
            "FixedAmount": 0.25,
            "Currency": "USD"
          }
        }
      }
    }
```

## Kubernetes Secrets

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: payment-service-secrets
  namespace: okla
type: Opaque
stringData:
  azul-merchant-id: "your-merchant-id"
  azul-auth-key: "your-auth-key"
  azul-cybersource-secret: "your-secret"

  cardnet-terminal-id: "your-terminal-id"
  cardnet-api-key: "your-api-key"

  pixelpay-public-key: "<YOUR_PIXELPAY_PUBLIC_KEY>"
  pixelpay-secret-key: "<YOUR_PIXELPAY_SECRET_KEY>"
  pixelpay-webhook-secret: "<YOUR_PIXELPAY_WEBHOOK_SECRET>"

  fygaro-api-key: "api_key_xxx"
  fygaro-subscription-key: "sub_key_xxx"
  fygaro-webhook-secret: "webhook_secret_xxx"
```

## Variables de Entorno

```bash
# AZUL
export AZUL_MERCHANT_ID=your-merchant-id
export AZUL_AUTH_KEY=your-auth-key
export AZUL_CYBERSOURCE_SECRET=your-secret

# CardNET
export CARDNET_TERMINAL_ID=your-terminal-id
export CARDNET_API_KEY=your-api-key

# PixelPay
export PIXELPAY_PUBLIC_KEY=<YOUR_PIXELPAY_PUBLIC_KEY>
export PIXELPAY_SECRET_KEY=<YOUR_PIXELPAY_SECRET_KEY>
export PIXELPAY_WEBHOOK_SECRET=<YOUR_PIXELPAY_WEBHOOK_SECRET>

# Fygaro
export FYGARO_API_KEY=api_key_xxx
export FYGARO_SUBSCRIPTION_KEY=sub_key_xxx
export FYGARO_WEBHOOK_SECRET=webhook_secret_xxx

# Infraestructura
export RABBITMQ_HOST=rabbitmq.default
export REDIS_CONNECTION_STRING=redis:6379
```

---

**Última actualización:** Enero 28, 2026

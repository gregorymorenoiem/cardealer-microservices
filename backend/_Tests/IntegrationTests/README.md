# Integration & E2E Tests - Sprint 12

Este proyecto contiene tests de integración, end-to-end, contratos y performance para la arquitectura de microservicios del CarDealer.

## Estructura del Proyecto

```
IntegrationTests/
├── Contracts/              # Tests de contrato (API schemas)
│   └── ApiContractTests.cs
├── E2E/                    # Tests end-to-end
│   └── E2EFlowTests.cs
├── Events/                 # Tests de eventos
│   └── EventContractTests.cs
├── Fixtures/               # Fixtures compartidos
│   ├── GatewayWebApplicationFactory.cs
│   ├── InfrastructureFixture.cs
│   ├── PostgresFixture.cs
│   ├── RabbitMQFixture.cs
│   └── RedisFixture.cs
├── Gateway/                # Tests de Gateway
│   ├── GatewayApiTests.cs
│   └── GatewayIntegrationTests.cs
├── Performance/            # Scripts K6 para performance
│   ├── gateway-load-test.js
│   ├── gateway-spike-test.js
│   └── gateway-soak-test.js
└── IntegrationTests.csproj
```

## Requisitos

- .NET 8.0 SDK
- Docker (para tests que usan Testcontainers)
- K6 (para tests de performance)

## Ejecutar Tests

### Todos los tests (excepto los que requieren Docker)
```bash
dotnet test --filter "Category!=RequiresDocker"
```

### Solo tests de Gateway
```bash
dotnet test --filter "FullyQualifiedName~Gateway"
```

### Solo tests E2E
```bash
dotnet test --filter "FullyQualifiedName~E2E"
```

### Solo tests de Contratos
```bash
dotnet test --filter "FullyQualifiedName~Contracts"
```

### Tests que requieren Docker
```bash
# Asegúrate de que Docker esté corriendo
docker info

# Ejecutar tests de infraestructura
dotnet test --filter "Category=RequiresDocker"
```

## Tests de Performance (K6)

### Requisitos
```bash
# Instalar K6
winget install k6 --source winget
# o
choco install k6
```

### Ejecutar Load Test
```bash
cd Performance
k6 run gateway-load-test.js --env BASE_URL=http://localhost:5000
```

### Ejecutar Spike Test
```bash
k6 run gateway-spike-test.js --env BASE_URL=http://localhost:5000
```

### Ejecutar Soak Test (30 minutos)
```bash
k6 run gateway-soak-test.js --env BASE_URL=http://localhost:5000
```

## Fixtures de Infraestructura

### PostgresFixture
Contenedor PostgreSQL 16-alpine con:
- Base de datos: `testdb`
- Usuario: `testuser`
- Schema: `test`

### RabbitMQFixture
Contenedor RabbitMQ 3-management con:
- Exchanges: `error.events`, `auth.events`, `notification.events`
- Queues: Correspondientes a cada exchange

### RedisFixture
Contenedor Redis 7-alpine para caching.

## Métricas de Cobertura

| Test Suite | Tests | Duración |
|------------|-------|----------|
| Gateway API | 16 | ~1s |
| E2E Flows | 13 | ~0.5s |
| Contracts | 19 | ~0.1s |
| Infrastructure* | 6 | ~10s |

*Requiere Docker

## Total: 54+ tests de integración

## Thresholds de Performance

- **Load Test**: p(95) < 500ms, error rate < 1%
- **Spike Test**: p(99) < 2000ms, error rate < 10%
- **Soak Test**: p(95) < 500ms, p(99) < 1000ms, error rate < 1%

## CI/CD Integration

```yaml
# .github/workflows/integration-tests.yml
- name: Run Integration Tests
  run: dotnet test --filter "Category!=RequiresDocker"
  
- name: Run Docker Integration Tests
  run: |
    docker-compose up -d
    dotnet test --filter "Category=RequiresDocker"
    docker-compose down
```

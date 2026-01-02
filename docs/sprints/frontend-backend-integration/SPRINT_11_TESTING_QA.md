# 🧪 SPRINT 11 - Testing & QA

**Fecha:** 2 Enero 2026  
**Duración estimada:** 3-4 horas  
**Tokens estimados:** ~20,000  
**Prioridad:** 🔴 Crítica

---

## 🎯 OBJETIVOS

1. Crear suite completa de tests unitarios
2. Implementar tests de integración
3. Agregar tests E2E con Playwright
4. Configurar cobertura de código
5. Crear pipeline de CI/CD
6. Documentar estrategia de testing

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: Backend Tests (1.5 horas)

- [ ] 1.1. Tests unitarios para Commands
- [ ] 1.2. Tests unitarios para Queries
- [ ] 1.3. Tests de integración para Controllers
- [ ] 1.4. Tests de integración para Repositories
- [ ] 1.5. Configurar cobertura mínima (80%)

### Fase 2: Frontend Tests (1 hora)

- [ ] 2.1. Tests unitarios para componentes (Vitest + RTL)
- [ ] 2.2. Tests de hooks personalizados
- [ ] 2.3. Tests de servicios API
- [ ] 2.4. Mocks de TanStack Query

### Fase 3: E2E Tests (1 hora)

- [ ] 3.1. Configurar Playwright
- [ ] 3.2. Test: Flujo de registro e inicio de sesión
- [ ] 3.3. Test: Creación de vehículo
- [ ] 3.4. Test: Flujo de pago
- [ ] 3.5. Test: Búsqueda y filtros

### Fase 4: CI/CD (30 min)

- [ ] 4.1. GitHub Actions workflow
- [ ] 4.2. Docker build automation
- [ ] 4.3. Test automation
- [ ] 4.4. Deployment scripts

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Backend - Unit Tests (xUnit)

**Archivo:** `backend/VehicleService/VehicleService.Tests/Commands/CreateVehicleCommandHandlerTests.cs`

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using VehicleService.Application.Features.Vehicles.Commands;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace VehicleService.Tests.Commands;

public class CreateVehicleCommandHandlerTests
{
    private readonly Mock<IVehicleRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<ILogger<CreateVehicleCommandHandler>> _loggerMock;
    private readonly CreateVehicleCommandHandler _handler;

    public CreateVehicleCommandHandlerTests()
    {
        _repositoryMock = new Mock<IVehicleRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<CreateVehicleCommandHandler>>();

        _handler = new CreateVehicleCommandHandler(
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateVehicleCommand
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Price = 25000,
            Mileage = 5000,
            FuelType = "Gasoline",
            Transmission = "Automatic"
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle v, CancellationToken _) => v);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Make.Should().Be("Toyota");
        result.Value.Model.Should().Be("Camry");

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _eventPublisherMock.Verify(
            p => p.PublishAsync(It.IsAny<VehicleCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData("", "Model", 2023, 10000)] // Empty make
    [InlineData("Make", "", 2023, 10000)]  // Empty model
    [InlineData("Make", "Model", 1800, 10000)] // Invalid year
    [InlineData("Make", "Model", 2023, -100)] // Negative price
    public async Task Handle_InvalidCommand_ShouldFailValidation(
        string make, 
        string model, 
        int year, 
        decimal price)
    {
        // Arrange
        var command = new CreateVehicleCommand
        {
            Make = make,
            Model = model,
            Year = year,
            Price = price
        };

        // Act & Assert
        // Validator should catch these before handler
        var validator = new CreateVehicleCommandValidator();
        var validationResult = await validator.ValidateAsync(command);
        
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var command = new CreateVehicleCommand
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Price = 25000
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Database error");
    }
}
```

---

### 2️⃣ Backend - Integration Tests (Testcontainers)

**Archivo:** `backend/_Tests/IntegrationTests/VehicleService/VehicleControllerTests.cs`

```csharp
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using IntegrationTests.Fixtures;
using VehicleService.Application.DTOs;

namespace IntegrationTests.VehicleService;

[Collection("Integration Tests")]
public class VehicleControllerTests : IClassFixture<WebApplicationFactory>
{
    private readonly HttpClient _client;

    public VehicleControllerTests(WebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithPaginatedList()
    {
        // Act
        var response = await _client.GetAsync("/api/vehicles?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<VehicleDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_ValidVehicle_ReturnsCreated()
    {
        // Arrange
        var createRequest = new CreateVehicleRequest
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Price = 25000,
            Mileage = 5000,
            FuelType = "Gasoline",
            Transmission = "Automatic",
            Description = "Excellent condition"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vehicles", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<VehicleDto>();
        result.Should().NotBeNull();
        result!.Make.Should().Be("Toyota");
        result.Model.Should().Be("Camry");
    }

    [Fact]
    public async Task Create_InvalidVehicle_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateVehicleRequest
        {
            Make = "", // Invalid: empty
            Model = "Model",
            Year = 2023,
            Price = 10000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vehicles", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ExistingVehicle_ReturnsOk()
    {
        // Arrange - Create first
        var createRequest = new CreateVehicleRequest
        {
            Make = "Honda",
            Model = "Civic",
            Year = 2022,
            Price = 20000
        };

        var createResponse = await _client.PostAsJsonAsync("/api/vehicles", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();

        // Act
        var response = await _client.GetAsync($"/api/vehicles/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<VehicleDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Delete_ExistingVehicle_ReturnsNoContent()
    {
        // Arrange - Create first
        var createRequest = new CreateVehicleRequest
        {
            Make = "Ford",
            Model = "Focus",
            Year = 2021,
            Price = 15000
        };

        var createResponse = await _client.PostAsJsonAsync("/api/vehicles", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/vehicles/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/vehicles/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

---

### 3️⃣ Frontend - Component Tests (Vitest + RTL)

**Archivo:** `frontend/web/original/src/components/__tests__/VehicleCard.test.tsx`

```typescript
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { VehicleCard } from '../VehicleCard';
import type { Vehicle } from '@/types/vehicle';

describe('VehicleCard', () => {
  const mockVehicle: Vehicle = {
    id: '1',
    make: 'Toyota',
    model: 'Camry',
    year: 2023,
    price: 25000,
    mileage: 5000,
    fuelType: 'Gasoline',
    transmission: 'Automatic',
    imageUrls: ['https://example.com/image1.jpg'],
    location: 'New York, NY',
    isFeatured: false,
    createdAt: '2024-01-01T00:00:00Z',
  };

  it('renders vehicle information correctly', () => {
    render(<VehicleCard vehicle={mockVehicle} />);

    expect(screen.getByText('Toyota Camry')).toBeInTheDocument();
    expect(screen.getByText('2023')).toBeInTheDocument();
    expect(screen.getByText('$25,000')).toBeInTheDocument();
    expect(screen.getByText(/5,000 km/i)).toBeInTheDocument();
  });

  it('displays featured badge when vehicle is featured', () => {
    const featuredVehicle = { ...mockVehicle, isFeatured: true };
    render(<VehicleCard vehicle={featuredVehicle} />);

    expect(screen.getByText(/destacado/i)).toBeInTheDocument();
  });

  it('calls onClick when card is clicked', async () => {
    const handleClick = vi.fn();
    const user = userEvent.setup();

    render(<VehicleCard vehicle={mockVehicle} onClick={handleClick} />);

    await user.click(screen.getByText('Toyota Camry'));

    expect(handleClick).toHaveBeenCalledWith(mockVehicle.id);
  });

  it('displays placeholder image when no imageUrls', () => {
    const vehicleWithoutImages = { ...mockVehicle, imageUrls: [] };
    render(<VehicleCard vehicle={vehicleWithoutImages} />);

    const img = screen.getByAltText('Toyota Camry');
    expect(img).toHaveAttribute('src', expect.stringContaining('placeholder'));
  });
});
```

---

### 4️⃣ E2E Tests - Playwright

**Archivo:** `frontend/web/original/e2e/auth.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test('should register a new user', async ({ page }) => {
    await page.goto('http://localhost:5174/register');

    // Fill registration form
    await page.fill('[name="fullName"]', 'Test User');
    await page.fill('[name="email"]', `test-${Date.now()}@example.com`);
    await page.fill('[name="password"]', 'Password123!');
    await page.fill('[name="confirmPassword"]', 'Password123!');

    // Submit
    await page.click('button[type="submit"]');

    // Should redirect to dashboard
    await expect(page).toHaveURL(/\/dashboard/);
    await expect(page.locator('text=Welcome')).toBeVisible();
  });

  test('should login with existing user', async ({ page }) => {
    await page.goto('http://localhost:5174/login');

    // Fill login form
    await page.fill('[name="email"]', 'test@example.com');
    await page.fill('[name="password"]', 'Password123!');

    // Submit
    await page.click('button[type="submit"]');

    // Should redirect to dashboard
    await expect(page).toHaveURL(/\/dashboard/);
    await expect(page.locator('[data-testid="user-menu"]')).toBeVisible();
  });

  test('should show error with invalid credentials', async ({ page }) => {
    await page.goto('http://localhost:5174/login');

    await page.fill('[name="email"]', 'wrong@example.com');
    await page.fill('[name="password"]', 'WrongPassword');
    await page.click('button[type="submit"]');

    await expect(page.locator('text=Invalid credentials')).toBeVisible();
  });

  test('should logout user', async ({ page }) => {
    // Login first
    await page.goto('http://localhost:5174/login');
    await page.fill('[name="email"]', 'test@example.com');
    await page.fill('[name="password"]', 'Password123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/\/dashboard/);

    // Logout
    await page.click('[data-testid="user-menu"]');
    await page.click('text=Logout');

    // Should redirect to home
    await expect(page).toHaveURL('/');
  });
});
```

**Archivo:** `frontend/web/original/e2e/vehicles.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('Vehicle Management', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('http://localhost:5174/login');
    await page.fill('[name="email"]', 'dealer@example.com');
    await page.fill('[name="password"]', 'Password123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/\/dashboard/);
  });

  test('should create a new vehicle', async ({ page }) => {
    await page.goto('http://localhost:5174/dashboard/vehicles/new');

    // Fill form
    await page.fill('[name="make"]', 'Toyota');
    await page.fill('[name="model"]', 'Camry');
    await page.fill('[name="year"]', '2023');
    await page.fill('[name="price"]', '25000');
    await page.fill('[name="mileage"]', '5000');
    await page.selectOption('[name="fuelType"]', 'Gasoline');
    await page.selectOption('[name="transmission"]', 'Automatic');
    await page.fill('[name="description"]', 'Excellent condition');

    // Upload image
    await page.setInputFiles('[name="images"]', 'path/to/test-image.jpg');

    // Submit
    await page.click('button[type="submit"]');

    // Should show success message
    await expect(page.locator('text=Vehicle created successfully')).toBeVisible();
    await expect(page).toHaveURL(/\/dashboard\/vehicles/);
  });

  test('should search and filter vehicles', async ({ page }) => {
    await page.goto('http://localhost:5174/vehicles');

    // Search by text
    await page.fill('[placeholder="Search..."]', 'Toyota');
    await page.waitForTimeout(500); // Debounce

    // Should show results
    await expect(page.locator('text=Toyota')).toBeVisible();

    // Filter by price
    await page.fill('[name="minPrice"]', '20000');
    await page.fill('[name="maxPrice"]', '30000');
    await page.waitForTimeout(500);

    // Filter by year
    await page.fill('[name="minYear"]', '2020');
    await page.fill('[name="maxYear"]', '2024');
    await page.waitForTimeout(500);

    // Results should update
    await expect(page.locator('[data-testid="vehicle-card"]')).toHaveCount.greaterThan(0);
  });
});
```

---

### 5️⃣ CI/CD Pipeline - GitHub Actions

**Archivo:** `.github/workflows/ci.yml`

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  backend-test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_PASSWORD: password
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore backend/CarDealer.sln
      
      - name: Build
        run: dotnet build backend/CarDealer.sln --no-restore
      
      - name: Test
        run: |
          dotnet test backend/CarDealer.sln \
            --no-build \
            --verbosity normal \
            --collect:"XPlat Code Coverage" \
            --results-directory ./coverage
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage/*/coverage.cobertura.xml

  frontend-test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json
      
      - name: Install dependencies
        run: npm ci
        working-directory: frontend/web
      
      - name: Lint
        run: npm run lint
        working-directory: frontend/web
      
      - name: Test
        run: npm run test:coverage
        working-directory: frontend/web
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: frontend/web/coverage/coverage-final.json

  e2e-test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '20'
      
      - name: Install dependencies
        run: npm ci
        working-directory: frontend/web
      
      - name: Install Playwright
        run: npx playwright install --with-deps
        working-directory: frontend/web
      
      - name: Start services
        run: docker-compose up -d
      
      - name: Run E2E tests
        run: npm run test:e2e
        working-directory: frontend/web
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: frontend/web/playwright-report/

  docker-build:
    runs-on: ubuntu-latest
    needs: [backend-test, frontend-test]
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Build images
        run: docker-compose build
      
      - name: Push to registry
        if: github.ref == 'refs/heads/main'
        run: |
          echo "${{ secrets.DOCKER_PASSWORD }}" | docker login -u "${{ secrets.DOCKER_USERNAME }}" --password-stdin
          docker-compose push
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

1. Cobertura de código >= 80% en backend
2. Cobertura de código >= 70% en frontend
3. Todos los tests E2E pasan
4. Pipeline CI/CD ejecuta sin errores
5. Build automatizado exitoso
6. Tests se ejecutan en < 5 minutos

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| Backend unit tests | 5,000 |
| Backend integration tests | 4,000 |
| Frontend component tests | 4,000 |
| E2E tests | 4,000 |
| CI/CD pipeline | 2,000 |
| Documentation | 1,000 |
| **TOTAL** | **~20,000** |

---

## 🎉 FIN DEL PLAN DE SPRINTS

**Todos los sprints completados (0-11)**

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026

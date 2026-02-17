# Bank Reconciliation Service

## 🏦 Overview

Automated bank reconciliation service for OKLA CarDealer platform. Integrates with 4 Dominican banks:

- **Banco Popular** (OAuth 2.0, FREE)
- **Banreservas** (API Key, $30/month)
- **BHD León** (OAuth 2.0 Open Banking, $40/month)
- **Scotiabank** (Certificate Auth, $80/month)

## 🚀 Features

- ✅ **Automatic Import** from bank APIs
- ✅ **Manual CSV/Excel Upload** for banks without API
- ✅ **3-Phase ML Matching Engine**:
  - Phase 1: Exact matches (95% of cases)
  - Phase 2: Fuzzy matches (4% of cases)
  - Phase 3: ML confidence scoring (1% complex cases)
- ✅ **Dashboard** with real-time statistics
- ✅ **Reports** (PDF, Excel, Email)
- ✅ **Discrepancy Management** with resolution workflow
- ✅ **Multi-account Support**
- ✅ **Role-based Authorization** (Admin, Accountant)

## 📊 ROI

| Metric     | Manual  | Automated  | Savings |
| ---------- | ------- | ---------- | ------- |
| Time/month | 3 hours | 15 minutes | 94%     |
| Cost/year  | $1,095  | $30        | $1,065  |
| Accuracy   | 85-90%  | 99%+       | +10-14% |
| Real-time  | ❌ No   | ✅ Yes     | ⭐      |

## 🛠️ Tech Stack

- .NET 8.0
- Entity Framework Core 8.0
- PostgreSQL 16+
- Clean Architecture
- CQRS + MediatR
- JWT Authentication
- Swagger/OpenAPI

## 📁 Project Structure

```
BankReconciliationService/
├── BankReconciliationService.Domain/         # Domain entities, enums, interfaces
├── BankReconciliationService.Application/    # DTOs, CQRS handlers, business logic
├── BankReconciliationService.Infrastructure/ # DbContext, repositories, bank APIs
├── BankReconciliationService.Api/           # REST API controllers
└── BankReconciliationService.Tests/         # Unit & integration tests
```

## 🔧 Setup

### 1. Database

```bash
# Create database
createdb bankreconciliation

# Run migrations (TODO: create migration)
dotnet ef migrations add InitialCreate --project BankReconciliationService.Infrastructure --startup-project BankReconciliationService.Api
dotnet ef database update --project BankReconciliationService.Infrastructure --startup-project BankReconciliationService.Api
```

### 2. Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bankreconciliation;Username=postgres;Password=yourpassword"
  },
  "BankApiSettings": {
    "BancoPopular": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

### 3. Run

```bash
cd BankReconciliationService.Api
dotnet run
```

API will be available at: `https://localhost:5001`  
Swagger UI: `https://localhost:5001`

## 📚 API Endpoints

### Bank Statements

- `GET /api/bankstatements` - List statements
- `GET /api/bankstatements/{id}` - Get statement
- `POST /api/bankstatements/import` - Import from API
- `POST /api/bankstatements/upload` - Upload CSV/Excel

### Reconciliations

- `GET /api/reconciliations` - List reconciliations
- `POST /api/reconciliations/start` - Start new reconciliation
- `POST /api/reconciliations/{id}/complete` - Complete
- `POST /api/reconciliations/{id}/approve` - Approve (Admin)
- `GET /api/reconciliations/dashboard` - Dashboard data

### Matches

- `GET /api/matches?reconciliationId={id}` - List matches
- `POST /api/matches/manual` - Create manual match
- `DELETE /api/matches/{id}` - Remove match
- `GET /api/matches/suggestions` - Get suggestions

### Bank Accounts

- `GET /api/bankaccounts` - List accounts
- `POST /api/bankaccounts` - Create account (Admin)
- `PUT /api/bankaccounts/{id}` - Update (Admin)
- `POST /api/bankaccounts/{id}/test-connection` - Test API

### Discrepancies

- `GET /api/discrepancies` - List discrepancies
- `POST /api/discrepancies/{id}/resolve` - Resolve
- `GET /api/discrepancies/unresolved` - Unresolved list

### Reports

- `GET /api/reports/reconciliation/{id}` - Get report
- `GET /api/reports/reconciliation/{id}/pdf` - PDF export
- `GET /api/reports/reconciliation/{id}/excel` - Excel export
- `POST /api/reports/reconciliation/{id}/send-email` - Email report

## 🔒 Authentication

All endpoints (except `/health` and `/api/bankaccounts/supported-banks`) require JWT authentication.

**Request Header:**

```
Authorization: Bearer {your-jwt-token}
```

## 🧪 Testing

```bash
# Run tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📦 Docker

```bash
# Build
docker build -t bankreconciliationservice:latest .

# Run
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=bankreconciliation;Username=postgres;Password=postgres" \
  bankreconciliationservice:latest
```

## 📖 Documentation

- [Automation Guide](../../docs/process-matrix/BANK_RECONCILIATION_AUTOMATION_GUIDE.md) - 40+ page technical guide
- [Executive Summary](../../docs/process-matrix/BANK_RECONCILIATION_EXECUTIVE_SUMMARY.md) - 8-page business overview

## 🤝 Support

For questions or issues:

- Email: support@okla.com.do
- Phone: (809) 555-0100

## 📄 License

Copyright © 2026 OKLA CarDealer. All rights reserved.

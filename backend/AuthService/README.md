# AuthService

## 📋 Overview
The **AuthService** is responsible for user authentication, authorization, and identity management within the CarDealer ecosystem. It handles user registration, login, token generation (JWT), and role management.

## 🚀 Features
- **Authentication**: User login with email/password.
- **Authorization**: Role-based access control (RBAC).
- **Token Management**: JWT generation, validation, and refresh tokens.
- **Security**: Password hashing, account lockout, and rate limiting.
- **Integration**: Publishes events to RabbitMQ for other services.

## 🛠️ Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: PostgreSQL (Entity Framework Core)
- **Caching**: Redis (Distributed Cache)
- **Messaging**: RabbitMQ (MassTransit/Raw Client)
- **Observability**: OpenTelemetry, Serilog
- **Resilience**: Polly

## 🏃‍♂️ Getting Started

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL
- Redis
- RabbitMQ

### Running Locally
```bash
cd backend/AuthService/AuthService.Api
dotnet run
```

### Running with Docker
```bash
docker-compose up -d authservice
```

## 🧪 Testing
```bash
dotnet test backend/AuthService/AuthService.Tests
```

## 📚 Documentation
- [Architecture](ARCHITECTURE.md)
- [Changelog](CHANGELOG.md)
- [Troubleshooting](TROUBLESHOOTING.md)
- [API Documentation](http://localhost:5000/swagger)

# Order Management API

Production-oriented ASP.NET Core 8 Web API built using Clean Architecture principles and enterprise development practices.

## Table of Contents
- [About](#about)
- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Clone and Build](#clone-and-build)
  - [Configuration](#configuration)
  - [Run](#run)
  - [Run Tests](#run-tests)
- [Folder Structure](#folder-structure)
- [Authentication](#authentication)
- [Logging, Validation & Error Handling](#logging-validation--error-handling)
- [Testing Strategy](#testing-strategy)
- [CI / CD](#ci--cd)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgements](#acknowledgements)

## About
Order Management API is a production-oriented ASP.NET Core 8 Web API implementing Clean Architecture to separate concerns and improve maintainability. The project demonstrates Authentication, Unit of Work / Repository patterns, Audit Logging, Validation, Exception Handling and comprehensive Unit Tests.

## Features
- Clean Architecture (separation of API, Application, Domain, Infrastructure)
- Authentication and authorization (JWT-ready)
- Unit of Work and Repository pattern for data access
- Audit logging for important entity changes and user actions
- Centralized validation pipeline (FluentValidation or equivalent)
- Global exception handling and structured error responses
- Unit tests covering application and domain logic
- API documentation (Swagger / OpenAPI) ready to enable

## Architecture
This repository follows Clean Architecture principles:
- Domain: Entities, value objects, domain services, business rules.
- Application: Use cases (commands/queries), DTOs, validation, interfaces.
- Infrastructure: Data access implementations (repositories, UoW), logging, external integrations.
- API: Controllers, presentation concerns, request/response models, middleware.

Benefits:
- Testable: business rules are decoupled from frameworks.
- Maintainable: single responsibility per layer.
- Swappable infrastructure: e.g., change database provider without affecting domain logic.

## Tech Stack
- .NET 8 / ASP.NET Core 8
- C#
- (Optional) Entity Framework Core / SQL Server (or any DB provider)
- xUnit / NUnit / MSTest (unit test framework used in the repo)
- Swagger / Swashbuckle for API docs
- FluentValidation (or similar) for input validation
- Serilog / Microsoft.Extensions.Logging for structured logging

## Getting Started

### Prerequisites
- .NET 8 SDK (download from https://dotnet.microsoft.com/)
- (Optional) Docker & Docker Compose if you run DB / services in containers
- (Optional) SQL Server / PostgreSQL or other DB if required by the project

### Clone and Build
1. Clone the repository:
   git clone https://github.com/Ranjitkumar-git/OrderManagement.API-UnitTestCleanArchitecture.git
2. Change into the solution folder:
   cd OrderManagement.API-UnitTestCleanArchitecture
3. Restore and build:
   dotnet restore
   dotnet build

### Configuration
- Copy `appsettings.Development.json` or `appsettings.json` example file if present and update:
  - ConnectionStrings: update DB connection string
  - Jwt: set signing key, issuer, audience (if JWT auth enabled)
  - Logging: configure sinks (console, file, Seq, etc.)
- Environment variables: you can override settings via standard ASP.NET Core environment variables.

### Run
- From solution root:
  dotnet run --project src/YourApiProject/YourApiProject.csproj
- Or use Visual Studio / VS Code to run the API.
- Open Swagger UI (if enabled) at: https://localhost:{port}/swagger

### Run Tests
- From solution root:
  dotnet test

## Folder Structure
(Adjust to match your project—example)
- src/
  - OrderManagement.API/           # Web API (controllers, middleware)
  - OrderManagement.Application/   # Use cases, DTOs, validation
  - OrderManagement.Domain/        # Entities, enums, domain logic
  - OrderManagement.Infrastructure/# Repositories, EF Core, logging
- tests/
  - OrderManagement.UnitTests/     # Unit tests for application & domain

## Authentication
- The project includes authentication support (JWT or other). To test protected endpoints:
  1. Obtain a token via the authentication endpoint (e.g., POST /api/auth/login).
  2. Include `Authorization: Bearer <token>` header in subsequent requests.

Example curl (replace with actual route):
curl -X POST https://localhost:5001/api/auth/login -H "Content-Type: application/json" -d '{"username":"user","password":"pass"}'

## Logging, Validation & Error Handling
- Requests and important domain events are recorded via audit logging middleware or repository hooks.
- Input validation is centralized (e.g., FluentValidation) and returns consistent error responses.
- Global exception handling middleware ensures structured error payloads and hides internal details in non-development environments.

## Testing Strategy
- Unit tests cover domain logic and application use-cases.
- Keep infrastructure code (e.g., repositories) thin and mockable, tested via integration tests when needed.
- Use test fixtures and seed data to create predictable test runs.
- Run tests locally and in CI: dotnet test.

## CI / CD
- Add CI (GitHub Actions, Azure Pipelines, etc.) to:
  - Build and restore packages
  - Run unit tests
  - Optionally run static analysis (SonarCloud, Roslyn analyzers)
  - Publish artifacts or build Docker images

Suggested GitHub Actions job: build → test → publish/test coverage.

## Contributing
1. Fork the repo
2. Create a feature branch: git checkout -b feature/my-feature
3. Commit changes with clear messages
4. Push and open a PR describing changes and tests
5. Ensure all tests pass and add unit tests for new logic

## License
Add a license file (e.g., MIT) if you want to open-source the repo. Example: `LICENSE` with MIT or other chosen license.

## Acknowledgements
- Clean Architecture guidance and patterns
- Any libraries or templates used (FluentValidation, Serilog, AutoMapper, etc.)

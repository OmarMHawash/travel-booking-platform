### Architecture Overview:

> **Status**: ✅ Architecture Implemented | 🚧 Some API Endpoints Pending
> **Last Verified**: 2025-01-08

```
TravelBookingPlatform.sln
├── .editorconfig                                 <-- Shared Code Formatting Configuration
├── Directory.Build.props                         <-- Project-wide SDKs, common properties, analyzer rules, syntax/patterns check
├── src/
│   ├── TravelBookingPlatform.Core/               <-- The SHARED KERNEL (Most Stable, Least Dependencies)
│   │   ├── TravelBookingPlatform.Core.Domain/    <-- Core domain concepts (e.g., base Entity/AggregateRoot, ValueObject, IUnitOfWork interface, common Domain Event base classes)
│   │   └── TravelBookingPlatform.Core.Application/ <-- Base MediatR command/query interfaces, common DTOs, common abstractions for external services (if truly generic)
│   │
│   ├── TravelBookingPlatform.Modules.Identity/
│   │   ├── TravelBookingPlatform.Modules.Identity.Domain/      <-- Entities (User, Role), Value Objects, Domain Events specific to Identity
│   │   ├── TravelBookingPlatform.Modules.Identity.Application/ <-- Commands, Queries, Handlers (MediatR), DTOs, Application Services, Fluent Validations
│   │   ├── TravelBookingPlatform.Modules.Identity.Infrastructure/ <-- EF Core DbContext, Migrations, Repository implementations, specific Identity service implementations (e.g., JWT token service)
│   │   └── TravelBookingPlatform.Modules.Identity.Api/          <-- Controllers, API specific models, module-specific DI registrations (exposed as extension methods for Host)
│   │
│   ├── TravelBookingPlatform.Modules.Hotels/
│   │   ├── TravelBookingPlatform.Modules.Hotels.Domain/        <-- Entities (Hotel, Room, City, Booking), Value Objects, Domain Events specific to Hotels and Booking
│   │   ├── TravelBookingPlatform.Modules.Hotels.Application/   <-- Commands, Queries, Handlers (MediatR), DTOs, Application Services, Fluent Validations, AutoMapper profiles
│   │   ├── TravelBookingPlatform.Modules.Hotels.Infrastructure/ <-- EF Core DbContext, Migrations, Repository implementations, external API clients (e.g., Map service integration)
│   │   └── TravelBookingPlatform.Modules.Hotels.Api/            <-- Controllers, API specific models, module-specific DI registrations
│   │
│   ├── TravelBookingPlatform.Host/                 <-- THE MONOLITHIC ENTRY POINT
│   │   │                                           References all `TravelBookingPlatform.Modules.*.Api` projects.
│   │   │                                           Contains `Program.cs` for bootstrapping.
│   │   │                                           Configures Global DI, Serilog, Swagger/Swashbuckle (API Docs),
│   │   │                                           built in HTTP APIs ProblemDetails, API Versioning, Authentication/Authorization Middleware,
│   │   │                                           CORS, and any other global HTTP pipeline setup.
│   │   │
│   │   └── appsettings.json                        <-- Global configuration
│   │
│   └── TravelBookingPlatform.SharedInfrastructure/ <-- COMMON INFRASTRUCTURE & Cross-Cutting Concerns
│       │                                           References `TravelBookingPlatform.Core`.
│       │                                           Contains implementations of `IUnitOfWork`, generic repositories,
│       │                                           centralized EF Core ApplicationDbContext that includes all module entities,
│       │                                           database migrations, common utility extensions (e.g., for DateTime),
│       │                                           global authentication/authorization handlers (if not Identity-specific),
│       │                                           Serilog sinks configuration, Health Checks, Cache implementations (e.g., Redis setup),
│       │                                           message bus implementation (e.g., in-memory for monolith, or RabbitMQ/Kafka client for future).
│
tests/
    ├── TravelBookingPlatform.UnitTests/
    │   ├── TravelBookingPlatform.Core.UnitTests/
    │   ├── TravelBookingPlatform.Identity.UnitTests/
    │   └── TravelBookingPlatform.Hotels.UnitTests/
    │
    └── TravelBookingPlatform.IntegrationTests/
        ├── TravelBookingPlatform.Host.IntegrationTests/        <-- Full API pipeline, cross-module scenarios
        └── TravelBookingPlatform.Modules.IntegrationTests/     <-- Module-to-module interaction tests

```

### Justification and Integration of Requirements:

1.  **Modular Monolith & Microservices Readiness:**

    - **Clear Bounded Contexts:** Each `TravelBookingPlatform.Modules.<ModuleName>` directory represents a distinct domain area.
    - **Internal Layering:** The `Domain`, `Application`, `Infrastructure`, `Api` breakdown within each module promotes SoC and Clean Architecture principles.
      - `Domain`: Pure business logic, entities, value objects. No external dependencies.
      - `Application`: Orchestrates domain, handles use cases (CQRS via MediatR), defines DTOs. Depends on `Domain`.
      - `Infrastructure`: Concrete implementations of repository interfaces (from Domain), external service integrations (EF Core, Payment Gateways, Email), specific identity providers. Depends on `Application` and `Domain`.
      - `Api`: RESTful API controllers, Presentation Layer, Module-specific DI setup. Depends on `Application`.
    - **Integrated Booking:** Booking functionality is implemented within the Hotels module since hotel bookings are domain-specific to the hotels context and share many common concerns (rooms, availability, pricing).
    - **Future Split:** When moving to microservices, each `TravelBookingPlatform.Modules.<ModuleName>` folder (specifically its `Api` project and its dependencies: `Application`, `Domain`, `Infrastructure`) can be easily extracted into a separate repository and deployable service. Shared `Core` and `SharedInfrastructure` components might become NuGet packages or their functionalities re-evaluated for distributed contexts.

2.  **Technologies & Practices:**

    - **.NET 8 (LTS), C# 12:** Standard setup, latest language features.
    - **JWT, Identity:** Fully owned by `TravelBookingPlatform.Modules.Identity`.
    - **EF Core, MS SQL:** Uses a centralized `ApplicationDbContext` in `SharedInfrastructure` that includes entities from all modules. This approach works well for the monolith and can be refactored for microservices later.
    - **Swashbuckle (Swagger), API Versioning:** Configured in `TravelBookingPlatform.Host`. Each module's `Api` project exposes its controllers.
    - **Hosting, Options, Logger (Microsoft.Extensions.Logging), Serilog:** `TravelBookingPlatform.Host` sets up the primary logging, `SharedInfrastructure` provides common Serilog sinks and configurations. `ILogger<T>` can be injected anywhere.
    - **DI:** `TravelBookingPlatform.Host` is the composition root. Each module's `.Api` project contains an extension method (e.g., `AddIdentityModule(this IServiceCollection services)`) to register its services into the main container, promoting modularity.
    - **Fluent Validation:** Resides in each module's `Application` layer, validating commands/queries.
    - **Fluent Assertion:** Used in the `tests/` projects.
    - **HTTP APIs ProblemDetails:** Configured in `TravelBookingPlatform.Host` for consistent API error responses (using the built-in dotnet framework).
    - **xUnit:** The chosen testing framework across all test projects.
    - **CQRS with MediatR:** `Core.Application` defines `IRequest`/`IRequestHandler`. Each module's `Application` layer implements commands, queries, and their handlers. This is key for internal communication and logic separation.
    - **AutoMapper, DTOs:** Defined within each module's `Application` layer for mapping between domain entities and DTOs.
    - **Shared Code Formatting Configuration (`.editorconfig`):** At the solution root for consistent code style.
    - **Project Syntax/Patterns Check Properties (`Directory.Build.props`):** At the solution root for common SDK versions, static analysis rules (e.g., Roslyn analyzers), and C# language features.

3.  **Project Document (Features & Non-Functional):**

    - **Admin Page for Easy Management:** The `Hotels` module contains the logic and API for managing Cities, Hotels, Rooms, and Bookings. This is a clear responsibility.
    - **Booking Management:** Integrated within the Hotels module, including booking entities, repositories, and business logic for hotel reservations.
    - **Payment Integration:** Can be implemented within `TravelBookingPlatform.Modules.Hotels.Infrastructure` or as a separate service integration.
    - **Error Handling & Logging:** Robustly handled by `ProblemDetails` in `Host` and Serilog configured via `Host` and `SharedInfrastructure`.
    - **Testing:** Comprehensive `UnitTests` for internal logic and `IntegrationTests` for cross-module interactions and end-to-end API flows.
    - **Codebase Documentation, API Reference:** Swagger/Swashbuckle for API reference. In-code comments and project-level documentation (e.g., in a `docs/` folder).

4.  **Unit testing packages:**

    Microsoft.NET.Test.Sdk (usually included by default)
    xunit
    coverlet.collector
    FluentAssertions (for more readable assertions)
    NSubstitute
    AutoFixture (to generate test data)

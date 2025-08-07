# Travel Booking Platform

A backend API for a Travel Booking and Accommodation Platform, implementing a modular monolith architecture with Clean Architecture and CQRS principles. The platform demonstrates good software engineering practices with great documentation and a well-structured codebase.

---

### 📁 Project Structure

```bash
apps/
├── api/       # The main .NET backend project
├── docs/      # Fumadocs-powered project documentation
└── client/    # A sample frontend app for testing and expansion
```

---

### 📊 Current Project Status

> **Core Modules Complete, Booking Implementation Pending**
> **Deployment Status: OFFLINE**

This project serves as a foundation for a travel booking system. The core architecture, user identity management, and hotel search/discovery features are implemented. The next priority is the full booking workflow.

* ✅ **Ready:** Identity & Auth, Hotel Search, Cities & Deals Management, Core Architecture.
* ✅ **Ready:** Hotel & Room Management.
* ✅ **Ready:** Booking & Confirmation APIs.
* 🚧 Partially Implemented: Payment Integration, Notification System and pdf printing.

---

## ✨ Key Features

* **Modular Monolith Architecture:** Clear separation of concerns into `Identity` and `Hotels` modules, ready for future microservice extraction.
* **Clean Architecture & Domain Driven Design:** Strictly enforced dependency rules and rich domain models.
* **CQRS with MediatR:** Complete separation of Commands (writes) and Queries (reads).
* **JWT-Based Authentication:** Secure user registration, login, and role-based authorization (Admin, TypicalUser).
* **Advanced Search Engine:** Multi-criteria filtering search, and pagination.
* **API First Design:** Comprehensive API documentation via Swagger/OpenAPI.
* **Ready Foundation:** Includes structured logging, global error handling, health checks, and API versioning.

---

## 🐳 Docker & Deployment

The entire project is containerized using Docker:

* Each of `api`, `docs`, and `client` has its own `Dockerfile`.
* A central `docker-compose.yml` handles multi-container orchestration and network.

> 🟠 **Current Deployment Status: OFFLINE**

| App    | URL                                                                    |
| ------ | ---------------------------------------------------------------------- |
| API    | [https://travel-api.569939478.xyz](https://travel-api.569939478.xyz)   |
| Docs   | [https://travel-docs.569939478.xyz](https://travel-docs.569939478.xyz) |
| Client | [https://travel.569939478.xyz](https://travel.569939478.xyz)           |

---

## 🚀 Tech Stack

| Component             | Technology / Library                                     |
| --------------------- | -------------------------------------------------------- |
| **Framework**         | .NET 8                                                   |
| **Architecture**      | Clean Architecture, CQRS, Modular Monolith, DDD          |
| **API**               | ASP.NET Core Web API, RESTful Principles, API Versioning |
| **Database**          | Entity Framework Core 8, SQL Server                      |
| **Authentication**    | JWT (JSON Web Tokens)                                    |
| **Mediation**         | MediatR                                                  |
| **Validation**        | FluentValidation                                         |
| **Testing**           | xUnit, NSubstitute, FluentAssertions                     |
| **API Documentation** | Swashbuckle (Swagger)                                    |
| **Logging**           | Serilog                                                  |

---

## 🏁 Getting Started (Manual Setup)

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads)
* (Optional) [SQL Server Management Studio](https://learn.microsoft.com/en-us/ssms/)
* Git

---

### 1. Clone the Repository

```bash
git clone https://github.com/omarmhawash/travel-booking-platform.git
cd travel-booking-platform
```

---

### 2. Configure Your Database Connection

In `apps/api/TravelBookingPlatform.Host/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TravelBookingPlatform;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

### 3. Apply Database Migrations

```bash
cd apps/api/TravelBookingPlatform.Host
dotnet ef database update
```

---

### 4. Add Launch Settings (Optional)

In `Properties/LaunchSettings.json`:

```json
"profiles": {
  "http": {
    "commandName": "Project",
    "dotnetRunMessages": true,
    "launchBrowser": true,
    "applicationUrl": "http://localhost:8080",
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Development"
    }
  }
}
```

---

### 5. Run the Application Locally

```bash
dotnet run
```

* **Swagger:** [http://localhost:8080/swagger](https://localhost:7123/swagger)
* **Health Check:** [http://localhost:8080/health](https://localhost:7123/health)

---

### 🔑 Default User Credentials

You can log in using the following seeded test accounts:

| Role  | Email                     | Password    |
| ----- | ------------------------- | ----------- |
| User  | `john.doe@email.com`      | `User123!`  |
| Admin | `admin@travelbooking.com` | `Admin123!` |

Use them with `POST /api/v1/auth/login` to retrieve a JWT for secured endpoints in Swagger.

---

## 📖 Project Documentation

Comprehensive documentation is available inside the `/apps/docs` folder (powered by [Fumadocs](https://fumadocs.com)).

| Document              | Description                                                             |
| --------------------- | ----------------------------------------------------------------------- |
| **`Getting-Started`** | Detailed setup instructions and configuration.                          |
| **`Architecture`**    | In-depth look at the Clean Architecture and Modular Monolith structure. |
| **`Developer-Guide`** | Core mechanics (error handling, validation) and the testing strategy.   |
| **`How-To-Guides`**   | Step-by-step guides for common development tasks.                       |
| **`API-Reference`**   | Guide to using fumadocs OpenApi UI.                                     |

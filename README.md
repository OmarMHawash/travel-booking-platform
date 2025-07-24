# Travel Booking Platform

A backend API for a Travel Booking and Accommodation Platform, implementing a modular monolith architecture with Clean Architecture and CQRS principles. The platform demonstrates good software engineering practices with great documentation and a well-structured codebase.

### 📊 Current Project Status

> **Core Modules Complete, Booking Implementation Pending**

This project serves as a foundation for a travel booking system. The core architecture, user identity management, and hotel search/discovery features. The next priority is the implementation of the complete booking workflow.

- ✅ **Ready:** Identity & Auth, Hotel Search, Cities & Deals Management, Core Architecture.
- 🚧 **Partially Implemented:** Hotel & Room Management (Domain logic exists, API endpoints missing).
- ❌ **Not Implemented:** Booking API, Payment Integration, Notification System.

## ✨ Key Features

- **Modular Monolith Architecture:** Clear separation of concerns into `Identity` and `Hotels` modules, ready for future microservice extraction.
- **Clean Architecture & Domain Driven Design:** Strictly enforced dependency rules and rich domain models.
- **CQRS with MediatR:** Complete separation of Commands (writes) and Queries (reads).
- **JWT-Based Authentication:** Secure user registration, login, and role-based authorization (Admin, TypicalUser).
- **Advanced Search Engine:** Multi-criteria filtering search, and pagination.
- **API First Design:** Comprehensive API documentation via Swagger/OpenAPI.
- **Ready Foundation:** Includes structured logging, global error handling, health checks, and API versioning.

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

## 🏁 Getting Started

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (also recommend to install [SQL Server Management Studio](https://learn.microsoft.com/en-us/ssms/release-notes-20))
- A Git client

### 1. Clone the Repository

```bash
git clone https://github.com/omarmhawash/travel-booking-platform.git
cd travel-booking-platform
```

### 2. Configure Your Database Connection

1.  Navigate to the `TravelBookingPlatform.Host` project:
    ```bash
    cd src/TravelBookingPlatform.Host
    ```
2.  Add the connection string in the `appsettings.json` file.

    _Example:_

    ```json
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=TravelBookingPlatform;Trusted_Connection=True;TrustServerCertificate=True;"
    }
    ```

### 3. Apply Database Migrations

The project uses EF Core for data access. Run the following command from the `src/TravelBookingPlatform.Host` directory to create the database and apply all migrations. The database will also be seeded with sample data (cities, hotels, users, etc.).

```bash
dotnet ef database update
```

### 4. add launch settings

1.  Navigate to the `TravelBookingPlatform.Host` project:
    ```bash
    cd src/TravelBookingPlatform.Host
    ```
2.  Add the launch settings in the `Properties/LaunchSettings.json` file.

    _Example:_

    ```json
    "profiles": {
        "http": {
            "commandName": "Project",
            "dotnetRunMessages": true,
            "launchBrowser": true,
            "applicationUrl": "http://localhost:5066",
            "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    }
    ```

### 5. Run the Application

Now you are ready to run the API.

```bash
dotnet run
```

The API will start, and you can access it at `https://localhost:7123` or `http://localhost:5123`.

### 6. Verify the Installation

- **API Documentation (Swagger):** Open your browser and navigate to `https://localhost:7123/swagger`. You should see the interactive Swagger UI with all implemented API endpoints.
- **Health Check:** Navigate to `https://localhost:7123/health` to confirm that the API can successfully connect to the database.

### 🔑 Default User Credentials

The database is seeded with a default admin and a typical user account for testing purposes.

- **Admin User:**
  - **Email:** `admin@test.com`
  - **Password:** `Password123!`
- **Typical User:**
  - **Email:** `user@test.com`
  - **Password:** `Password123!`

You can use these credentials with the `POST /api/v1/auth/login` endpoint to get a JWT token for testing secured endpoints in Swagger.

## 📖 Project Documentation

For a deeper dive into the project's architecture, conventions, and guides, please refer to the documentation in the `/docs` folder.

| Document              | Description                                                                |
| --------------------- | -------------------------------------------------------------------------- |
| **`Getting-Started`** | Detailed setup instructions and configuration.                             |
| **`Architecture`**    | An in-depth look at the Clean Architecture and Modular Monolith structure. |
| **`Developer-Guide`** | Core mechanics (error handling, validation) and the testing strategy.      |
| **`How-To-Guides`**   | Practical, step-by-step recipes for common development tasks.              |
| **`API-Reference`**   | Instructions on how to use the interactive Swagger UI for API exploration. |
| **`ROADMAP`**         | The planned implementation path for pending and future features.           |

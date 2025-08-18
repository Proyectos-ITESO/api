# Project Overview: MicroJack.API

This document provides a comprehensive overview of the `MicroJack.API` project, detailing its components, structure, functionality, and setup instructions.

## 1. Back-end: `MicroJack.API`

The `MicroJack.API` is a backend service built using ASP.NET Core (C#). This API serves as the central data and business logic hub for the MicroJack application, handling requests and interacting with external systems like Phidgets hardware and WhatsApp.

### 1.1. Technology Stack

*   **Framework:** ASP.NET Core 8.0 - A cross-platform, high-performance, open-source framework for building modern, cloud-based, internet-connected applications.
*   **Language:** C# - A modern, object-oriented, and type-safe programming language.
*   **Database:** SQLite - A C-language library that implements a small, fast, self-contained, high-reliability, full-featured, SQL database engine. The database is encrypted using `SQLitePCLRaw.bundle_e_sqlcipher`.
*   **Key Libraries/Dependencies:**
    *   `Phidget22.NET`: Essential for interacting with Phidgets hardware devices.
    *   `Microsoft.EntityFrameworkCore.Sqlite`: Provides EF Core support for SQLite.
    *   `Microsoft.AspNetCore.Authentication.JwtBearer`: For handling JWT-based authentication.
    *   `BCrypt.Net-Next`: For password hashing.
    *   `Swashbuckle.AspNetCore`: For generating Swagger/OpenAPI documentation.

### 1.2. Directory Structure and Key Files

*   `MicroJack.API.csproj`: The project file for the ASP.NET Core application. It defines the project's target framework, dependencies (NuGet packages), and build configurations.
*   `Program.cs`: The application's entry point. This file is responsible for:
    *   Building the `WebApplication` host.
    *   Configuring services (e.g., dependency injection, database connections, custom services).
    *   Setting up the HTTP request pipeline (middleware, routing, authentication/authorization).
    *   Running the application.
*   `appsettings.json`: The primary configuration file for the application. It stores settings such as database connection strings, API keys, and other environment-agnostic configurations.
*   `src/`: The main source directory for the application.
    *   `Data/ApplicationDbContext.cs`: The EF Core database context, defining the database schema.
    *   `Models/`: Contains the C# classes that define the data structures used throughout the application.
    *   `Routes/`: Defines the API endpoints and maps incoming HTTP requests to specific actions.
    *   `Services/`: Contains the business logic and data access layers of the application.
*   `uploads/`: Directory for storing uploaded files.

### 1.3. How to Obtain and Run the Back-end

1.  **Prerequisites:**
    *   **.NET SDK 8.0 or later:** Download and install from the official Microsoft .NET website.
2.  **Clone the repository:**
    ```bash
    git clone <repository_url>
    cd MicroJack.API
    ```
3.  **Restore dependencies:**
    This command downloads all the necessary NuGet packages defined in `MicroJack.API.csproj`.
    ```bash
    dotnet restore
    ```
4.  **Build the project:**
    This compiles the C# source code into executable assemblies.
    ```bash
    dotnet build
    ```
5.  **Run the application:**
    This command starts the ASP.NET Core web server.
    ```bash
    dotnet run
    ```
    The API will typically run on the URLs configured in `Properties/launchSettings.json`.

## 2. Feature: License Validation System

This section details the license validation system implemented in the `MicroJack.API` project.

### 2.1. Purpose & Architecture

The system is designed to validate the application's license upon startup. It uses a client-server model to allow for both online and offline validation, and includes a version check mechanism to enforce updates.

*   **Client**: The client-side logic is integrated directly into the `MicroJack.API` application. It is responsible for contacting the license server, verifying the response, and managing a local cache.
*   **Server**: A separate, self-contained ASP.NET Core Minimal API project located in the `licensing-server/` directory (excluded from the main build). It acts as the source of truth for license validation.

### 2.2. Validation Logic

The validation process is triggered once when the main API starts.

1.  **Online Validation (Primary):**
    *   The client (`LicenseService.cs`) sends a GET request to the license server's `/api/validate` endpoint.
    *   The server validates the license key and returns a signed payload with license details and version information.
    *   The client verifies the signature using a public key.
    *   If successful, the license response is saved to a local `license.cache` file.

2.  **Offline Validation (Fallback):**
    *   If online validation fails, the client attempts to read the `license.cache` file.
    *   It performs the same signature and version validation on the cached data.

### 2.3. Key Files and Components

#### Client (`MicroJack.API/`)
*   **Logic:** `src/Services/LicenseService.cs` & `src/Services/ILicenseService.cs`
*   **Data Models:** `src/Models/LicenseCache.cs`, `src/Models/LicenseSettings.cs`
*   **Configuration:** `appsettings.json` (contains `LicenseSettings` section).
*   **Integration:** `Program.cs` (registers the service and calls `ValidateLicense()` at startup).

#### Server (`licensing-server/`)
*   **Note:** This project is not part of the main application build. It must be run separately for license validation testing.
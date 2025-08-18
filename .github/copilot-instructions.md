# Copilot Instructions for MicroJack.API

This document provides instructions for GitHub Copilot to effectively assist with the development of the `MicroJack.API` project.

## Project Overview

`MicroJack.API` is a backend service built with ASP.NET Core 8.0 and C#. It uses an encrypted SQLite database for data storage and provides a RESTful API for the MicroJack application. The project also includes integrations with Phidgets hardware and WhatsApp.

## Key Technologies and Libraries

- **Framework:** ASP.NET Core 8.0
- **Language:** C#
- **Database:** Encrypted SQLite with Entity Framework Core
- **Authentication:** JWT-based authentication
- **API Documentation:** Swagger/OpenAPI
- **Hardware Integration:** Phidgets (via `Phidget22.NET`)
- **Messaging:** WhatsApp (via a custom service)

## Development Guidelines

- **Code Style:** Follow the existing code style and conventions. Use the `.editorconfig` file if available.
- **Database:** Use Entity Framework Core for all database interactions. Do not write raw SQL queries unless absolutely necessary.
- **API Design:** Follow RESTful principles for API design. Use the existing routing structure in the `src/Routes` directory.
- **Services:** Business logic should be encapsulated in services within the `src/Services` directory.
- **Error Handling:** Use try-catch blocks for error handling and log exceptions using the provided logging framework.
- **Dependencies:** Use the NuGet package manager to manage dependencies. Do not add new dependencies without a valid reason.

## How to Get Help

If you have any questions or need help with the project, please refer to the `GEMINI.md` file for a detailed project overview.
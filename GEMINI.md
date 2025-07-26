# Project Overview: realGforLife

This document provides a comprehensive overview of the `realGforLife` project, detailing its back-end and front-end components, their structure, functionality, and setup instructions.

## 1. Back-end: `api/`

The `api/` directory hosts the back-end services for the `realGforLife` application. It is built using ASP.NET Core (C#) and interacts with a MongoDB database. This API serves as the central data and business logic hub, handling requests from the front-end and interacting with external systems like Phidgets hardware and WhatsApp.

### 1.1. Technology Stack

*   **Framework:** ASP.NET Core 8.0 - A cross-platform, high-performance, open-source framework for building modern, cloud-based, internet-connected applications.
*   **Language:** C# - A modern, object-oriented, and type-safe programming language.
*   **Database:** MongoDB - A NoSQL document database designed for scalability and flexibility.
*   **Key Libraries/Dependencies:**
    *   `Phidget22.NET`: Essential for interacting with Phidgets hardware devices, likely for sensor readings or control.
    *   `MongoDB.Driver`: The official .NET driver for MongoDB, enabling seamless data interaction.
    *   `Newtonsoft.Json`: A popular high-performance JSON framework for .NET, used for serialization and deserialization.
    *   `AngleSharp`: A .NET library that provides an HTML5 parser, useful for web scraping or parsing HTML content.
    *   `DnsClient`: A high-performance .NET library for DNS lookups.

### 1.2. Directory Structure and Key Files

*   `api.sln`: The Visual Studio solution file. This file organizes one or more projects (`MicroJack.API.csproj` in this case) and provides the build environment for Visual Studio or other compatible IDEs.
*   `MicroJack.API.csproj`: The project file for the ASP.NET Core application. It defines the project's target framework, dependencies (NuGet packages), build configurations, and included files.
*   `Program.cs`: The application's entry point. This file is responsible for:
    *   Building the `WebApplication` host.
    *   Configuring services (e.g., dependency injection, database connections, custom services).
    *   Setting up the HTTP request pipeline (middleware, routing, authentication/authorization).
    *   Running the application.
*   `appsettings.json`: The primary configuration file for the application. It stores settings such as database connection strings, API keys, and other environment-agnostic configurations.
*   `appsettings.Development.json`: An environment-specific configuration file that overrides settings in `appsettings.json` when the application runs in the "Development" environment. This is typically used for local development settings (e.g., local database connection strings, detailed logging).
*   `Properties/launchSettings.json`: Contains various launch profiles for debugging and running the application. It specifies environment variables, application URLs (e.g., `http://localhost:5000`, `https://localhost:5001`), and other settings for different development scenarios.

#### 1.2.1. `src/Models/`

This directory contains Plain Old C# Objects (POCOs) that define the data structures used throughout the application. These models typically represent entities stored in the MongoDB database or data transfer objects (DTOs) used for API requests and responses.

*   `IntermediateRegistration.cs`: Defines the structure for data related to a temporary or incomplete registration process. This might capture initial user input before full submission.
*   `MongoDbSettings.cs`: A C# class used to strongly type the MongoDB connection settings read from `appsettings.json` (e.g., connection string, database name, collection names).
*   `PreRegistration.cs`: Represents the data model for users or entities that have initiated a pre-registration process, often a preliminary step before full registration.
*   `Registration.cs`: The core data model for a complete and final registration, containing all necessary information about a registered entity.

#### 1.2.2. `src/Routes/`

This directory is responsible for defining the API endpoints and mapping incoming HTTP requests to specific actions within the application.

*   `ApiRoutes.cs`: A static class or similar construct that centralizes the definition of API route paths (e.g., `/api/registrations`, `/api/preregistrations`). This promotes consistency and makes it easier to manage and update routes.
*   `Modules/`: This subdirectory likely contains modular definitions for API endpoints, possibly using a feature-based approach where each module defines routes related to a specific domain or feature (e.g., a `RegistrationModule` defining all registration-related endpoints).

#### 1.2.3. `src/Services/`

This directory contains the business logic and data access layers of the application. Services encapsulate operations related to specific functionalities, interacting with the database and external APIs.

*   `BaseMongoService.cs`: An abstract or concrete base class that provides common CRUD (Create, Read, Update, Delete) operations for MongoDB collections. This promotes code reusability and ensures consistent interaction patterns with the database.
*   `IntermediateRegistrationService.cs`: Contains the business logic for managing `IntermediateRegistration` data, including validation, storage, and retrieval.
*   `PhidgetService.cs`: Manages the interaction with Phidgets hardware. This service would abstract the complexities of the Phidget22.NET library, providing a clean interface for other parts of the application to control or read from Phidget devices.
*   `PreRegistrationService.cs`: Implements the business logic for handling `PreRegistration` data, including processing pre-registration requests and managing their lifecycle.
*   `RegistrationService.cs`: Contains the core business logic for managing `Registration` data, including finalization of registrations, data integrity checks, and interactions with other services.
*   `WhatsAppService.cs`: Responsible for integrating with the WhatsApp API. This service would handle sending messages, receiving callbacks, and managing WhatsApp-related communications.
*   `Interfaces/`: This subdirectory holds interfaces (e.g., `IIntermediateRegistrationService`, `IPhidgetService`). Using interfaces promotes dependency inversion, making the code more modular, testable, and easier to maintain by decoupling service implementations from their consumers.

### 1.3. How to Obtain and Run the Back-end

1.  **Prerequisites:**
    *   **.NET SDK 8.0 or later:** Download and install from the official Microsoft .NET website.
    *   **MongoDB Instance:** You need a running MongoDB database. This can be a local installation, a Docker container, or a cloud-hosted service (e.g., MongoDB Atlas). Ensure the connection string in `appsettings.Development.json` (or `appsettings.json`) is correctly configured to point to your MongoDB instance.
2.  **Clone the repository:**
    Assuming `realGforLife` is the root of your project, navigate to the `api` directory.
    ```bash
    git clone <repository_url> # If you haven't cloned the main project yet
    cd realGforLife/api
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
    The API will typically run on `https://localhost:5001` (and `http://localhost:5000`) by default, as configured in `Properties/launchSettings.json`. You can access the API endpoints using a tool like Postman, Insomnia, or directly from the front-end application.

## 2. Front-end: `micro/`

The `micro/` directory contains the front-end application, built with React and Vite. It provides the user interface for interacting with the back-end API, allowing users to perform actions like registration, pre-registration, and viewing logs.

### 2.1. Technology Stack

*   **Framework:** React.js - A JavaScript library for building user interfaces, known for its component-based architecture and declarative approach.
*   **Build Tool:** Vite - A next-generation front-end tooling that provides an extremely fast development experience with features like instant server start and hot module replacement (HMR).
*   **Language:** JavaScript (JSX) - JavaScript with XML-like syntax for defining React components.
*   **Package Manager:** npm - The default package manager for Node.js, used to install and manage project dependencies.
*   **Styling:** CSS - Cascading Style Sheets for styling the application's appearance.
*   **Other Tools:**
    *   **ESLint:** A pluggable linting utility for JavaScript and JSX, used to identify and report on patterns found in ECMAScript/JavaScript code, ensuring code quality and consistency.

### 2.2. Directory Structure and Key Files

*   `package.json`: The manifest file for the Node.js project. It contains metadata about the project (name, version, description), defines scripts for common tasks (e.g., `start`, `build`), and lists all project dependencies and dev dependencies.
*   `package-lock.json`: Automatically generated by npm, this file records the exact version of every package installed, including their transitive dependencies. This ensures that anyone installing the project gets the exact same dependency tree.
*   `vite.config.js`: The configuration file for Vite. It allows customization of the build process, development server settings (e.g., proxying API requests to the back-end), and plugin configurations.
*   `index.html`: The main HTML file that serves as the entry point for the React application. It's a minimal HTML page that includes the `<div id="root"></div>` where the React application will be mounted.
*   `server.js`: A custom Node.js server. This might be used for:
    *   **API Proxying:** To forward API requests from the front-end to the back-end to avoid CORS issues during development.
    *   **CORS Handling:** Explicitly setting CORS headers for development or specific deployment scenarios.
    *   **Static File Serving:** Serving static assets in a production-like environment.
*   `cors-middleware.js`: A custom middleware function, likely used with `server.js`, to configure and handle Cross-Origin Resource Sharing (CORS) headers. This is crucial for allowing the front-end (running on a different origin) to make requests to the back-end API.
*   `db.json`: Potentially a JSON file used for mock API data during development. This allows front-end development to proceed independently of the back-end, simulating API responses.
*   `eslint.config.js`: The configuration file for ESLint, specifying linting rules, environments, and plugins to enforce coding standards and identify potential issues in the JavaScript/JSX codebase.

#### 2.2.1. `src/`

The main source directory for the React application, containing all the application's components, logic, and assets.

*   `main.jsx`: The primary entry point for the React application. This file typically imports the root `App` component and uses `ReactDOM.createRoot()` to render the application into the `index.html` file's root element.
*   `App.jsx`: The main application component. It usually contains the top-level routing configuration (e.g., using React Router) and defines the overall layout and structure of the application.

#### 2.2.2. `src/auth/`

This directory contains components and logic specifically related to user authentication and authorization.

*   `Login.jsx`: The React component responsible for rendering the user login interface, handling user input for credentials, and communicating with the back-end authentication API.

#### 2.2.3. `src/components/`

This directory houses reusable UI components that are generic and can be used across different parts of the application without being tied to a specific feature or page.

*   `CotoSelector.jsx`: A specific reusable component, likely for selecting a "Coto" (which could refer to a specific type of enclosure, section, or category within the application's domain). This component would encapsulate the UI and logic for this selection.

#### 2.2.4. `src/layout/`

This directory defines the structural components that make up the overall layout of the application, ensuring a consistent user experience across different pages.

*   `Footer.jsx`: The React component for the application's footer section, typically containing copyright information, links, or other persistent content at the bottom of the page.
*   `Header.jsx`: The React component for the application's header section, usually containing the application logo, navigation links, and user-related information.
*   `MainLayout.jsx`: A higher-order component or a layout component that wraps the main content of the application. It typically includes the `Header`, `Footer`, and `Sidebar` components, providing a consistent structure for most pages.
*   `Sidebar.jsx`: The React component for the application's sidebar navigation, providing links to different sections or features of the application.

#### 2.2.5. `src/models/`

This directory contains JavaScript files that define data structures or schemas used on the front-end. These models often mirror the back-end models to ensure data consistency between the client and server.

*   `Registration.js`: Defines the front-end data model for a registration. This might include validation rules, default values, or methods for transforming data before sending it to the back-end API.

#### 2.2.6. `src/modules/`

This directory organizes the application into feature-specific modules. Each module encapsulates all components, logic, and potentially routes related to a particular application feature, promoting modularity and maintainability.

*   `Bitacora/`: A module dedicated to the "Bitacora" (log or journal) functionality. This would contain components for displaying log entries, filtering, and potentially adding new entries.
*   `PreRegistro/`: A module for the pre-registration feature, including components for the pre-registration form, status display, and related logic.
*   `Registration/`: The core module for the main registration process, containing components for the registration form, progress tracking, and confirmation.

#### 2.2.7. `src/pages/`

This directory contains top-level React components that represent distinct pages or views in the application. These components typically compose smaller components from `src/components/` and `src/modules/` to form a complete page.

*   `ApprovalPage.jsx`: The React component for a page where administrators or specific users can review and approve (or reject) pending registrations or other items.

#### 2.2.8. `src/routes/`

This directory defines the front-end routing configuration for the application, typically using a library like React Router. It maps URL paths to specific React components (pages).

*   `registrations.js`: Defines the routes related to the registration feature, specifying which components should be rendered for paths like `/register`, `/registrations`, or `/registration/:id`.

#### 2.2.9. `src/tests/`

This directory is dedicated to front-end unit and integration tests, ensuring the quality and correctness of the React components and application logic.

*   `PreRegisterForm.jsx`: This file is likely a test file (e.g., `PreRegisterForm.test.jsx` or `PreRegisterForm.spec.jsx` would be more conventional naming) that contains tests for the pre-registration form component. Alternatively, it could be a simple component used *within* tests for setup or mocking purposes.

#### 2.2.10. `src/utils/`

This directory contains utility functions or services that provide common functionalities used across the application, promoting code reusability and separation of concerns.

*   `AzureOcrService.jsx`: A service responsible for integrating with Azure's Optical Character Recognition (OCR) capabilities. This service would handle sending images to Azure for text extraction and processing the results.

#### 2.2.11. `assets/`

This directory stores all static assets used by the front-end application, such as images, icons, and global CSS files.

*   `App.css`: The main CSS file for global styles and application-wide styling rules.
*   **Various `.png` and `.svg` files:** A collection of image assets used throughout the UI, including buttons (`BTN_AMARILLO.png`, `BTN_AZUL.svg`), icons (`ICON_CAMERA.svg`, `TYPE_CAR.svg`), logos (`logo_microjack_gris.png`), and other graphical elements.
*   `btns/`, `btns2/`, `svgtopng/`: Subdirectories used to organize button assets, possibly by format (PNG vs. SVG) or by different design variations. `svgtopng/` might contain PNG versions of SVG assets, possibly for compatibility or performance reasons.

### 2.3. How to Obtain and Run the Front-end

1.  **Prerequisites:**
    *   **Node.js (LTS version recommended) and npm:** Download and install Node.js from its official website. npm (Node Package Manager) is bundled with Node.js.
2.  **Clone the repository:**
    Assuming `realGforLife` is the root of your project, navigate to the `micro` directory.
    ```bash
    git clone <repository_url> # If you haven't cloned the main project yet
    cd realGforLife/micro
    ```
3.  **Install dependencies:**
    This command reads the `package.json` file and downloads all the required Node.js modules and libraries into the `node_modules` directory.
    ```bash
    npm install
    ```
4.  **Run the development server:**
    This command starts the Vite development server, which compiles and serves the React application. It also enables features like hot module replacement (HMR) for a fast development feedback loop.
    ```bash
    npm run dev
    ```
    The front-end application will typically be accessible in your web browser at `http://localhost:5173` (or a similar port, which Vite will indicate in the console) once the server starts. Ensure your back-end API is also running if the front-end needs to make API calls.

---
## Feature: License Validation System

This section details the license validation system implemented in the `MicroJack.API` project.

### 1. Purpose & Architecture

The system is designed to validate the application's license upon startup. It uses a client-server model to allow for both online and offline validation, and includes a version check mechanism to enforce updates.

*   **Client**: The client-side logic is integrated directly into the `MicroJack.API` application. It is responsible for contacting the license server, verifying the response, and managing a local cache.
*   **Server**: A separate, self-contained ASP.NET Core Minimal API project located in the `api/licensing-server/` directory. It acts as the source of truth for license validation.

### 2. Validation Logic

The validation process is triggered once when the main API starts.

1.  **Online Validation (Primary):**
    *   The client (`LicenseService.cs`) sends a GET request to the license server's `/api/validate` endpoint, including its `LicenseKey` and a generated `MachineId`.
    *   The server looks up the license key in its `licenses.json` "database".
    *   If valid, the server constructs a response containing license details (expiration, features) and version information (`LatestVersion`, `MinimumRequiredVersion`).
    *   Crucially, the server signs this entire payload with its RSA private key and includes the signature in the response.
    *   The client receives the response, rebuilds the signed data string, and verifies the signature using the RSA public key stored in its `appsettings.json`.
    *   If the signature is valid, the client proceeds to the version check. If successful, the license response is saved to a local `license.cache` file.

2.  **Offline Validation (Fallback):**
    *   If the online validation fails for any reason (e.g., no internet connection), the client attempts to read the `license.cache` file.
    *   It performs the same signature and version validation on the cached data.
    *   It also checks that the `NextVerificationDate` has not passed.
    *   If all checks pass, the application is allowed to start. If any check fails, startup is aborted.

### 3. Version Check Logic

After a successful signature verification (online or offline), the `LicenseService` performs a version check:

*   It compares the application's current assembly version against the `MinimumRequiredVersion` from the license data. If the current version is lower, the application will fail to start with a critical error.
*   It then compares the current version against the `LatestVersion`. If the current version is lower, it logs a warning to suggest an update but allows the application to continue running.

### 4. Key Files and Components

#### Client (`api/`)
*   **Logic:** `src/Services/LicenseService.cs` & `src/Services/ILicenseService.cs`
*   **Data Models:** `src/Models/LicenseCache.cs`, `src/Models/LicenseSettings.cs`
*   **Configuration:** `appsettings.json` (contains `LicenseSettings` section with URL, public key, and license key).
*   **Integration:** `Program.cs` (registers the service and calls `ValidateLicense()` at startup).

#### Server (`api/licensing-server/`)
*   **Logic:** `Services/LicenseValidationService.cs`
*   **Data Models:** `Models/License.cs`, `Models/LicenseResponse.cs`
*   **"Database":** `licenses.json`
*   **Endpoint:** `Program.cs` (defines the `GET /api/validate` endpoint).
*   **Configuration:** `appsettings.json` (stores the RSA `PrivateKey`).
*   **Keys:** `private_key.pem`, `public_key.pem` (RSA key pair for signing/verification).

### 5. How to Run & Test

1.  **Run the License Server:**
    ```bash
    # In a terminal, navigate to api/licensing-server
    dotnet run --urls="http://localhost:5101"
    ```
2.  **Run the Main API (Client):**
    ```bash
    # In a second terminal, navigate to api/
    dotnet run
    ```
3.  **Run Tests:**
    *   The project includes test scripts to verify the functionality. Ensure the license server is running, but the main API is not.
    *   `bash test_licensing_system.sh`: Checks the basic client-server communication.
    *   `bash test_version_check.sh`: Checks the version warning and obsolete version failure scenarios.

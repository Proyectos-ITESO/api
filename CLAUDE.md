# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MicroJack.API is a comprehensive gate access control system built with ASP.NET Core 8.0 and C#. It uses an encrypted SQLite database with Entity Framework Core and provides RESTful APIs for managing guards, visitors, vehicles, residents, access logs, and system catalogs. The system includes hardware integration with Phidgets for gate control and WhatsApp messaging capabilities.

## Development Commands

### Building and Running
```bash
# Build the project
dotnet build

# Run in development mode
dotnet run

# Run with specific environment
dotnet run --environment Development

# Build for release
dotnet build --configuration Release
```

### Database Operations
```bash
# The database is automatically initialized and migrated on startup
# Database files are stored in encrypted SQLite format
# Database location is configurable via appsettings.json or MICROJACK_DATA_DIR environment variable
```

### Testing
```bash
# Run comprehensive system tests
bash test_complete_system.sh

# Test specific components
bash test_coto_system.sh
bash test_new_endpoints_simple.sh
bash test_licensing_system.sh
bash test_version_check.sh
bash test_autoupdate_system.sh
```

### Auto-Update System
```bash
# Test the auto-update functionality
bash test_autoupdate_system.sh

# Build for release deployment
dotnet build --configuration Release
cd bin/Release/net8.0/
zip -r ../../../microjack-v1.2.0.zip *
```

## Architecture Overview

### Core Components

**Data Layer**: Entity Framework Core with encrypted SQLite database
- ApplicationDbContext.cs: Main database context with automatic timestamp management
- Models organized in: Core, Catalog, Transaction, Enums
- Automatic database initialization and migration on startup

**Service Layer**: Business logic implementation
- Services in src/Services/ with corresponding interfaces in src/Services/Interfaces/
- Authentication service with JWT token management
- Role-based authorization (Guard, Admin, SuperAdmin)
- Encrypted database key management

**API Layer**: Minimal API endpoints organized by module
- Routes in src/Routes/Modules/ following RESTful conventions
- Unified access control endpoints for frontend optimization
- File upload system for images (faces, INE, license plates)
- Event logging system (bitácora) with 10 predefined event types

**Authentication & Authorization**:
- JWT-based authentication with configurable secret
- Three authorization levels: GuardLevel, AdminLevel, SuperAdminLevel
- Password hashing using BCrypt.Net-Next
- CORS configuration for frontend integration

### Key Architectural Patterns

**Database-First Design**: All entities use automatic timestamps (CreatedAt, UpdatedAt)
**Service Pattern**: Business logic separated from API controllers
**Interface Segregation**: All services have corresponding interfaces
**Modular Routing**: Endpoints organized by functional area
**Unified Access Control**: Single endpoints for complex operations (visitor + vehicle + access log)

### Database Schema

**Core Entities**: Guard, Resident, Visitor, Vehicle, Address
**Catalog Entities**: VehicleBrand, VehicleColor, VehicleType, VisitReason (70+ pre-populated entries)
**Transaction Entities**: AccessLog, EventLog, CallRecord
**Supporting Entities**: Booth, Role, PreRegistration, BitacoraNote

### File Structure

```
src/
├── Controllers/           # Traditional MVC controllers
├── Data/                 # Database context and models
├── Middleware/           # Custom middleware
├── Models/               # Entity models
│   ├── Catalog/         # Catalog entities (vehicle brands, colors, etc.)
│   ├── Core/            # Core business entities
│   ├── Enums/           # Enumerations
│   ├── Transaction/     # Transactional entities
│   └── Ucm/             # UCM integration models
├── Routes/              # API endpoint definitions
│   └── Modules/         # Modular route definitions
├── Services/            # Business logic services
│   └── Interfaces/      # Service interfaces
└── Services/            # Service implementations
```

## Configuration

### Database Configuration
- Uses encrypted SQLite with SQLCipher
- Database path configurable via connection string or MICROJACK_DATA_DIR
- Automatic key generation and management
- Supports multiple deployment scenarios

### Authentication Configuration
- JWT settings in appsettings.json
- Configurable token expiration
- Role-based authorization policies
- CORS configuration for frontend origins

### License & Update System
- RSA-based license validation
- Automatic update functionality
- Version-based update enforcement
- Separate updater executable (MicroJack.Updater)

## Development Guidelines

### Code Style
- Follow existing C# conventions
- Use nullable reference types (enabled)
- Use async/await for I/O operations
- Implement proper error handling with try-catch blocks

### Database Operations
- Always use Entity Framework Core for database operations
- Use async methods for database queries
- Implement proper transaction management for complex operations
- Leverage automatic timestamp management

### API Development
- Follow RESTful conventions
- Use minimal API pattern
- Implement proper HTTP status codes
- Include comprehensive error responses
- Use the existing modular routing structure

### Security Considerations
- Never hardcode secrets or API keys
- Use the encryption service for sensitive data
- Validate all input parameters
- Implement proper authorization checks
- Use HTTPS in production environments

### Testing
- Use the provided test scripts for comprehensive testing
- Test both individual components and system integration
- Verify database operations and authentication flows
- Test the auto-update system functionality

## Integration Points

### Hardware Integration
- Phidget22.NET for gate control hardware
- Configurable relay channels for gate operations
- Hardware status monitoring and initialization

### External Services
- WhatsApp service for notifications
- UCM (Grandstream) integration for telephony
- License server for validation and updates
- File upload system with static file serving

### Frontend Integration
- CORS configuration for web frontends
- Static file serving for uploaded images
- Unified endpoints for complex operations
- Comprehensive error responses

## Important Notes

- The license validation system is currently disabled for development (lines 18-21 and 209-224 in Program.cs)
- Database encryption is mandatory and uses SQLCipher
- The system supports offline operation with local database
- All file uploads are stored in the uploads/ directory with organized subdirectories
- The auto-update system requires proper configuration of the license server
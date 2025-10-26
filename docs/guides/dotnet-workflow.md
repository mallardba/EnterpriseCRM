# Enterprise CRM - .NET Development Workflow

## 🔄 What `dotnet restore` Just Did

### **Package Restoration Process**

When you ran `dotnet restore`, the .NET CLI performed several important tasks:

#### **1. Project Discovery**
- **Scanned the solution file** (`EnterpriseCRM.sln`) to find all projects
- **Identified project dependencies** and their relationships
- **Located project files** (`.csproj`) in the solution

#### **2. Package Resolution**
- **Read package references** from each `.csproj` file
- **Connected to NuGet.org** to find available package versions
- **Resolved version conflicts** between projects
- **Downloaded packages** to the local NuGet cache

#### **3. Dependency Graph Building**
- **Created dependency tree** showing how projects depend on each other
- **Resolved transitive dependencies** (packages that other packages need)
- **Ensured compatibility** between all package versions

#### **4. Cache Management**
- **Stored packages** in `%USERPROFILE%\.nuget\packages\`
- **Created project-specific folders** for each package version
- **Generated lock files** to ensure consistent builds

### **What Was Restored**

#### **Core Projects:**
- **EnterpriseCRM.Core**: Domain entities and business logic
- **EnterpriseCRM.Application**: Use cases and application services
- **EnterpriseCRM.Infrastructure**: Data access and external services
- **EnterpriseCRM.WebAPI**: RESTful API endpoints
- **EnterpriseCRM.BlazorServer**: Real-time web application

#### **Test Projects:**
- **EnterpriseCRM.UnitTests**: Unit testing with xUnit, Moq, and FluentAssertions

#### **Key Packages Restored:**
- **ASP.NET Core 8.0**: Web framework and hosting
- **Entity Framework Core 8.0**: Database ORM
- **SignalR 1.1.0**: Real-time communication
- **BootstrapBlazor 7.0.0**: UI components
- **ChartJs.Blazor**: Charting components
- **xUnit 2.6.1**: Testing framework
- **Moq 4.20.69**: Mocking framework
- **FluentAssertions 6.12.0**: Test assertions

## 🏗️ How the Projects Work Together

### **Clean Architecture Layers**

```
┌─────────────────────────────────────┐
│           Presentation Layer        │
├─────────────────────────────────────┤
│  WebAPI (Controllers)              │
│  BlazorServer (Components)         │
└─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────┐
│          Application Layer          │
├─────────────────────────────────────┤
│  Use Cases (Commands/Queries)      │
│  Application Services              │
│  DTOs and Interfaces               │
└─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────┐
│            Domain Layer             │
├─────────────────────────────────────┤
│  Entities and Value Objects        │
│  Domain Services                   │
│  Business Rules                    │
└─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────┐
│         Infrastructure Layer        │
├─────────────────────────────────────┤
│  Data Access (Repositories)        │
│  External Services                 │
│  Database Context                  │
└─────────────────────────────────────┘
```

### **Project Dependencies**

#### **EnterpriseCRM.Core**
- **Purpose**: Domain entities and business logic
- **Dependencies**: None (pure domain)
- **Contains**: Customer, Lead, Opportunity entities

#### **EnterpriseCRM.Application**
- **Purpose**: Application use cases and services
- **Dependencies**: Core
- **Contains**: Commands, Queries, Application Services

#### **EnterpriseCRM.Infrastructure**
- **Purpose**: Data access and external integrations
- **Dependencies**: Core, Application
- **Contains**: Repositories, Database Context, External Services

#### **EnterpriseCRM.WebAPI**
- **Purpose**: RESTful API endpoints
- **Dependencies**: Core, Application, Infrastructure
- **Contains**: Controllers, API Models, Configuration

#### **EnterpriseCRM.BlazorServer**
- **Purpose**: Real-time web application
- **Dependencies**: Core, Application, Infrastructure
- **Contains**: Blazor Components, SignalR Hubs, UI

#### **EnterpriseCRM.UnitTests**
- **Purpose**: Unit testing
- **Dependencies**: All projects
- **Contains**: Test classes, Mock objects, Assertions

## 🚀 Next Steps: Build, Run, and Test

### **1. Build the Solution**

```bash
# From the src directory
dotnet build
```

#### **What `dotnet build` Does:**
- **Compiles all projects** in the solution
- **Resolves dependencies** between projects
- **Generates assemblies** (.dll files)
- **Creates output folders** (`bin/` and `obj/`)
- **Reports compilation errors** if any

#### **Build Output:**
```
src/
├── bin/
│   ├── Debug/
│   │   ├── net8.0/
│   │   │   ├── EnterpriseCRM.WebAPI.dll
│   │   │   ├── EnterpriseCRM.BlazorServer.dll
│   │   │   └── ...
│   └── Release/
└── obj/
    └── Debug/
        └── net8.0/
            └── ...
```

### **2. Run the Web API**

```bash
# From the src directory
dotnet run --project EnterpriseCRM.WebAPI
```

#### **What Happens:**
- **Starts the API server** on `https://localhost:7001`
- **Loads configuration** from `appsettings.json`
- **Initializes database context** (if configured)
- **Registers services** in dependency injection container
- **Starts HTTP listener** for incoming requests

#### **API Endpoints:**
- **Base URL**: `https://localhost:7001`
- **Swagger UI**: `https://localhost:7001/swagger`
- **Health Check**: `https://localhost:7001/health`
- **API Endpoints**: `https://localhost:7001/api/[controller]`

#### **Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EnterpriseCRM;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### **3. Run the Blazor Server App**

```bash
# From the src directory
dotnet run --project EnterpriseCRM.BlazorServer
```

#### **What Happens:**
- **Starts the Blazor server** on `https://localhost:5001`
- **Initializes SignalR hub** for real-time communication
- **Loads Blazor components** and pages
- **Sets up client-server communication** via SignalR
- **Serves the web application** to browsers

#### **Application URLs:**
- **Main App**: `https://localhost:5001`
- **SignalR Hub**: `https://localhost:5001/customerHub`
- **API Proxy**: `https://localhost:5001/api`

#### **Blazor Features:**
- **Real-time updates** via SignalR
- **Component-based UI** with BootstrapBlazor
- **Chart visualizations** with ChartJs.Blazor
- **Responsive design** for mobile and desktop

### **4. Run Tests**

```bash
# From the src directory
dotnet test
```

#### **What `dotnet test` Does:**
- **Discovers test projects** in the solution
- **Runs all test methods** marked with `[Fact]` or `[Theory]`
- **Generates test reports** with pass/fail results
- **Provides code coverage** information (if configured)
- **Outputs detailed results** to console

#### **Test Types:**
- **Unit Tests**: Test individual methods and classes
- **Integration Tests**: Test component interactions
- **Mock Tests**: Test with mocked dependencies

#### **Test Output:**
```
Test run for EnterpriseCRM.UnitTests.dll (.NET 8.0)
Microsoft (R) Test Execution Command Line Tool Version 17.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 2s
```

## 🔧 Development Workflow

### **Typical Development Cycle:**

1. **Make Changes**: Edit code in your IDE
2. **Build**: `dotnet build` to check for errors
3. **Test**: `dotnet test` to ensure nothing broke
4. **Run**: `dotnet run --project [ProjectName]` to test functionality
5. **Debug**: Use breakpoints and debugging tools
6. **Commit**: Save changes to version control

### **Hot Reload (Development):**

```bash
# Enable hot reload for faster development
dotnet watch run --project EnterpriseCRM.WebAPI
dotnet watch run --project EnterpriseCRM.BlazorServer
```

#### **Benefits:**
- **Automatic restart** when code changes
- **Faster development cycle** without manual restart
- **Preserves application state** where possible

### **Debugging:**

#### **Visual Studio:**
- Set breakpoints in code
- Press F5 to start debugging
- Step through code execution
- Inspect variables and call stack

#### **VS Code:**
- Install C# extension
- Create `launch.json` configuration
- Set breakpoints and start debugging
- Use integrated terminal for commands

## 📊 Project Structure Overview

```
EnterpriseCRM/
├── src/
│   ├── EnterpriseCRM.sln              # Solution file
│   ├── EnterpriseCRM.Core/            # Domain layer
│   │   ├── Entities.cs               # Domain entities
│   │   ├── Interfaces.cs             # Domain interfaces
│   │   └── EnterpriseCRM.Core.csproj # Project file
│   ├── EnterpriseCRM.Application/     # Application layer
│   │   ├── DTOs.cs                   # Data transfer objects
│   │   ├── Interfaces.cs             # Application interfaces
│   │   └── EnterpriseCRM.Application.csproj
│   ├── EnterpriseCRM.Infrastructure/  # Infrastructure layer
│   │   ├── Data/
│   │   │   └── ApplicationDbContext.cs
│   │   ├── Repositories/
│   │   │   └── Repositories.cs
│   │   ├── UnitOfWork/
│   │   │   └── UnitOfWork.cs
│   │   └── EnterpriseCRM.Infrastructure.csproj
│   ├── EnterpriseCRM.WebAPI/          # API layer
│   │   ├── Controllers/
│   │   │   └── CustomersController.cs
│   │   ├── Program.cs                 # API startup
│   │   ├── appsettings.json          # Configuration
│   │   └── EnterpriseCRM.WebAPI.csproj
│   └── EnterpriseCRM.BlazorServer/    # Web UI layer
│       ├── EnterpriseCRM.BlazorServer.csproj
│       └── (Blazor components and pages)
└── tests/
    └── EnterpriseCRM.UnitTests/       # Test layer
        ├── Services/
        │   └── CustomerServiceTests.cs
        └── EnterpriseCRM.UnitTests.csproj
```

## 🎯 Key Benefits of This Structure

### **Separation of Concerns:**
- **Clear boundaries** between layers
- **Independent development** of each layer
- **Easy testing** of individual components

### **Maintainability:**
- **Organized code** structure
- **Consistent patterns** across projects
- **Easy to locate** and modify functionality

### **Scalability:**
- **Add new features** without affecting existing code
- **Scale individual layers** independently
- **Support multiple clients** (Web, Mobile, API)

### **Testability:**
- **Unit test** individual components
- **Integration test** layer interactions
- **Mock dependencies** for isolated testing

This .NET workflow provides a solid foundation for building and maintaining the Enterprise CRM system with clean architecture principles and modern development practices.

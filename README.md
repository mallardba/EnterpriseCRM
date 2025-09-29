# Enterprise CRM System

A comprehensive Customer Relationship Management system built with modern .NET technologies to demonstrate full-stack development skills.

## 🚀 Technologies Used

- **Backend**: C#, ASP.NET Core 8.0, Entity Framework Core
- **Frontend**: Blazor Server & Blazor WebAssembly
- **Database**: SQL Server with stored procedures
- **Testing**: xUnit, Moq, FluentAssertions (TDD approach)
- **UI/UX**: CSS3, Bootstrap 5, Chart.js
- **Version Control**: Git with conventional commits
- **Project Management**: GitHub Issues (Jira alternative)

## 📋 Features

### Core Modules
- **Customer Management**: Complete CRUD operations with advanced search
- **Lead Tracking**: Lead conversion pipeline with status management
- **Sales Pipeline**: Opportunity tracking and forecasting
- **Task Management**: Assignment system with notifications
- **Reporting Dashboard**: Analytics with interactive charts
- **User Management**: Role-based authentication and authorization
- **Integration Hub**: Third-party API integrations

### Technical Highlights
- Clean Architecture with Domain-Driven Design
- Repository Pattern with Unit of Work
- CQRS with MediatR
- AutoMapper for object mapping
- FluentValidation for input validation
- SignalR for real-time updates
- Docker containerization
- CI/CD pipeline with GitHub Actions

## 🏗️ Project Structure

```
EnterpriseCRM/
├── src/
│   ├── EnterpriseCRM.Core/           # Domain entities and interfaces
│   ├── EnterpriseCRM.Infrastructure/ # Data access and external services
│   ├── EnterpriseCRM.Application/    # Business logic and services
│   ├── EnterpriseCRM.WebAPI/         # REST API controllers
│   ├── EnterpriseCRM.BlazorServer/   # Blazor Server application
│   └── EnterpriseCRM.BlazorWasm/     # Blazor WebAssembly application
├── tests/
│   ├── EnterpriseCRM.UnitTests/      # Unit tests
│   ├── EnterpriseCRM.IntegrationTests/ # Integration tests
│   └── EnterpriseCRM.E2ETests/       # End-to-end tests
├── docs/                            # Documentation
└── scripts/                         # Database and deployment scripts
```

## 🛠️ Getting Started

> **📖 For detailed setup instructions, see the [Setup Guide](docs/SETUP_GUIDE.md)**

### Quick Start

**Prerequisites:**
- .NET 8.0 SDK
- SQL Server 2022 or LocalDB
- Visual Studio 2022 or VS Code
- Git

**Installation Steps:**

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/EnterpriseCRM.git
   cd EnterpriseCRM
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Set up the database:**
   - Create database: `CREATE DATABASE EnterpriseCRM;`
   - Run setup script: `scripts/database-setup.sql`
   - Update connection string in `src/EnterpriseCRM.WebAPI/appsettings.json`

4. **Run database migrations:**
   ```bash
   dotnet ef database update --project src/EnterpriseCRM.Infrastructure
   ```

5. **Start the application:**
   ```bash
   dotnet run --project src/EnterpriseCRM.BlazorServer
   ```

6. **Access the application:**
   - Blazor Server: `https://localhost:5001`
   - Web API: `https://localhost:7001`

> **⚠️ Important:** Make sure to follow the complete [Setup Guide](docs/SETUP_GUIDE.md) for detailed configuration steps, troubleshooting, and additional setup options.

## 🧪 Testing Strategy (TDD)

This project follows Test-Driven Development principles:

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test component interactions
- **E2E Tests**: Test complete user workflows
- **Performance Tests**: Load testing with NBomber

Run tests:
```bash
dotnet test
```

## 📊 Database Design

The system uses a normalized database design with:
- Customer and Contact entities
- Lead and Opportunity tracking
- Task and Activity management
- User and Role management
- Audit logging

Key stored procedures:
- `sp_GetCustomerMetrics`
- `sp_CalculateSalesForecast`
- `sp_GenerateMonthlyReport`

## 🔧 Development Workflow

1. **Feature Branch**: Create feature branch from main
2. **TDD**: Write tests first, then implement
3. **Code Review**: Submit pull request for review
4. **CI/CD**: Automated testing and deployment
5. **Documentation**: Update docs with changes

## 📈 Performance Considerations

- Entity Framework Core optimizations
- Database indexing strategy
- Caching with Redis
- Async/await patterns
- Pagination for large datasets

## 🔐 Security Features

- JWT authentication
- Role-based authorization
- Input validation and sanitization
- SQL injection prevention
- HTTPS enforcement

## 📱 Responsive Design

- Mobile-first approach
- Bootstrap 5 components
- Custom CSS animations
- Progressive Web App features

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow TDD principles
4. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🎯 Learning Outcomes

This project demonstrates proficiency in:
- Full-stack .NET development
- Modern web application architecture
- Database design and optimization
- Test-driven development
- UI/UX implementation
- DevOps practices
- Code quality and best practices

Perfect for showcasing skills required for enterprise .NET developer positions!

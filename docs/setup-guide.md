# Enterprise CRM System - Setup Guide

## 🚀 Quick Start Guide

This comprehensive setup guide will help you get the Enterprise CRM system running on your local machine.

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

### Required Software
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server 2022** or **SQL Server LocalDB** - [Download here](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **Visual Studio 2022** or **VS Code** - [Download here](https://visualstudio.microsoft.com/downloads/)
- **Git** - [Download here](https://git-scm.com/downloads)

### Optional but Recommended
- **SQL Server Management Studio (SSMS)** - [Download here](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- **Postman** - For API testing - [Download here](https://www.postman.com/downloads/)

## 🛠️ Installation Steps

### Step 1: Clone and Setup Project

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/EnterpriseCRM.git
   cd EnterpriseCRM
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

### Step 2: Database Setup

1. **Create the database:**
   - Open SQL Server Management Studio (SSMS)
   - Connect to your SQL Server instance
   - Run the following command to create the database:
   ```sql
   CREATE DATABASE EnterpriseCRM;
   ```

2. **Run the database setup script:**
   - Open the file `scripts/database-setup.sql`
   - Execute the script in SSMS to create tables, stored procedures, and sample data

3. **Update connection string:**
   - Open `src/EnterpriseCRM.WebAPI/appsettings.json`
   - Update the `DefaultConnection` string to match your SQL Server instance:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=EnterpriseCRM;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

### Step 3: Run Entity Framework Migrations

1. **Navigate to the Infrastructure project:**
   ```bash
   cd src/EnterpriseCRM.Infrastructure
   ```

2. **Create initial migration:**
   ```bash
   dotnet ef migrations add InitialCreate --startup-project ../EnterpriseCRM.WebAPI
   ```

3. **Update the database:**
   ```bash
   dotnet ef database update --startup-project ../EnterpriseCRM.WebAPI
   ```

### Step 4: Run the Application

1. **Start the Web API:**
   ```bash
   cd src/EnterpriseCRM.WebAPI
   dotnet run
   ```
   The API will be available at: `https://localhost:7001`

2. **Start the Blazor Server application:**
   ```bash
   cd src/EnterpriseCRM.BlazorServer
   dotnet run
   ```
   The web application will be available at: `https://localhost:7002`

### Step 5: Run Tests

1. **Run unit tests:**
   ```bash
   cd tests/EnterpriseCRM.UnitTests
   dotnet test
   ```

2. **Run integration tests:**
   ```bash
   cd tests/EnterpriseCRM.IntegrationTests
   dotnet test
   ```

## 🔧 Configuration

### JWT Authentication Setup

The application uses JWT tokens for authentication. Update the JWT settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "EnterpriseCRM",
    "Audience": "EnterpriseCRMUsers",
    "ExpiryInMinutes": 60
  }
}
```

### Logging Configuration

The application uses Serilog for structured logging. Logs are written to:
- Console output
- File: `logs/enterprise-crm-YYYY-MM-DD.txt`

## 📊 Database Schema

### Core Entities
- **Customers**: Company and individual customer information
- **Contacts**: Individual contacts within customers
- **Leads**: Potential customers in the sales pipeline
- **Opportunities**: Sales opportunities with stages and values
- **Tasks**: Activities and assignments
- **Users**: System users with roles and permissions

### Key Relationships
- Customers have multiple Contacts
- Customers can have multiple Leads and Opportunities
- Tasks can be associated with Customers, Leads, or Opportunities
- Users can be assigned to Leads, Opportunities, and Tasks

## 🧪 Testing Strategy

### Unit Tests
- Located in `tests/EnterpriseCRM.UnitTests`
- Test individual service methods and business logic
- Use Moq for mocking dependencies
- Follow TDD principles

### Integration Tests
- Located in `tests/EnterpriseCRM.IntegrationTests`
- Test API endpoints and database interactions
- Use in-memory database for testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/EnterpriseCRM.UnitTests
```

## 🚀 API Documentation

### Swagger Documentation
Once the Web API is running, visit:
- Swagger UI: `https://localhost:7001/swagger`

### Key API Endpoints

#### Customers
- `GET /api/customers` - Get all customers (paginated)
- `GET /api/customers/{id}` - Get customer by ID
- `POST /api/customers` - Create new customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer
- `GET /api/customers/search?searchTerm={term}` - Search customers

#### Leads
- `GET /api/leads` - Get all leads
- `POST /api/leads` - Create new lead
- `PUT /api/leads/{id}` - Update lead
- `POST /api/leads/{id}/convert` - Convert lead to customer

#### Opportunities
- `GET /api/opportunities` - Get all opportunities
- `POST /api/opportunities` - Create new opportunity
- `PUT /api/opportunities/{id}` - Update opportunity
- `PUT /api/opportunities/{id}/stage` - Update opportunity stage

#### Tasks
- `GET /api/tasks` - Get all tasks
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `PUT /api/tasks/{id}/complete` - Mark task as complete

#### Dashboard
- `GET /api/dashboard/stats` - Get dashboard statistics
- `GET /api/dashboard/recent` - Get recent activities

## 🔐 Authentication

### Default Users
The database setup script creates the following default users:

| Username | Password | Role | Email |
|----------|----------|------|-------|
| admin | admin123 | Admin | admin@enterprisecrm.com |
| john.doe | password123 | Manager | john.doe@enterprisecrm.com |
| jane.smith | password123 | User | jane.smith@enterprisecrm.com |

### Getting JWT Token
1. POST to `/api/auth/login` with username and password
2. Use the returned token in the `Authorization` header: `Bearer {token}`

## 🎨 Frontend Features

### Blazor Server Application
- **Dashboard**: Overview of key metrics and recent activities
- **Customer Management**: CRUD operations for customers
- **Lead Tracking**: Lead pipeline management
- **Opportunity Management**: Sales opportunity tracking
- **Task Management**: Activity and task assignment
- **User Management**: User administration and roles

### Key Components
- Responsive design with Bootstrap 5
- Real-time updates with SignalR
- Interactive charts with Chart.js
- Modern UI with Bootstrap Blazor components

## 📈 Performance Considerations

### Database Optimization
- Proper indexing on frequently queried columns
- Stored procedures for complex operations
- Soft delete pattern for data integrity

### Caching Strategy
- Entity Framework query caching
- In-memory caching for frequently accessed data
- Response caching for API endpoints

### Monitoring
- Structured logging with Serilog
- Performance counters
- Health checks for dependencies

## 🐛 Troubleshooting

### Common Issues

1. **Database Connection Error**
   - Verify SQL Server is running
   - Check connection string in appsettings.json
   - Ensure database exists

2. **Migration Errors**
   - Delete existing migrations folder
   - Recreate migration: `dotnet ef migrations add InitialCreate`
   - Update database: `dotnet ef database update`

3. **Authentication Issues**
   - Verify JWT settings in appsettings.json
   - Check token expiration
   - Ensure proper authorization headers

4. **Build Errors**
   - Run `dotnet restore` to restore packages
   - Check .NET SDK version compatibility
   - Verify project references

### Getting Help

1. Check the logs in the `logs` directory
2. Review the Swagger documentation
3. Run tests to verify functionality
4. Check the GitHub issues for known problems

## 🚀 Deployment

### Development Deployment
1. Update connection strings for production database
2. Configure JWT settings for production
3. Set up proper logging configuration
4. Run database migrations on production server

### Production Considerations
- Use Azure SQL Database or SQL Server
- Configure HTTPS certificates
- Set up proper authentication and authorization
- Implement monitoring and alerting
- Configure backup and recovery procedures

## 📚 Learning Resources

### Technologies Used
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/)

### Best Practices
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Test-Driven Development](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## 🎯 Next Steps

1. **Explore the Code**: Review the project structure and understand the architecture
2. **Run Tests**: Execute the test suite to understand the testing approach
3. **API Testing**: Use Postman or Swagger to test the API endpoints
4. **Frontend Development**: Customize the Blazor components
5. **Database Optimization**: Analyze and optimize database performance
6. **Add Features**: Implement additional CRM features as needed

This project demonstrates enterprise-level .NET development skills and is perfect for showcasing your abilities to potential employers!

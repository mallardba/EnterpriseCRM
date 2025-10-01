# Enterprise CRM - Integration Testing Guide

## 🎯 **Overview**

Integration tests verify that multiple components of the Enterprise CRM system work together correctly. Unlike unit tests that test individual classes in isolation, integration tests validate real interactions between layers, databases, and external systems.

## 🔄 **Integration Tests vs Unit Tests**

### **Unit Tests (Current)**
- ✅ Test individual classes in isolation
- ✅ Use mocks for dependencies
- ✅ Fast execution (< 1ms per test)
- ✅ Test business logic only
- ✅ Located in `tests/EnterpriseCRM.UnitTests/`

### **Integration Tests (New)**
- 🔄 Test multiple components working together
- 🔄 Use real database, real HTTP calls
- 🔄 Slower execution (10-100ms per test)
- 🔄 Test actual data flow and integration
- 🔄 Located in `tests/EnterpriseCRM.IntegrationTests/`

## 🏗️ **Integration Test Categories**

### **1. Database Integration Tests**
Test actual database operations with Entity Framework Core.

**What they test:**
- Entity Framework migrations
- Repository pattern with real database
- Database constraints and foreign keys
- Soft delete functionality
- Audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Database transactions and rollbacks
- Concurrent access scenarios

**Example:**
```csharp
[Fact]
public async Task CustomerRepository_ShouldCreateAndRetrieveCustomer()
{
    // Arrange
    var customer = new Customer
    {
        CompanyName = "Test Company",
        Email = "test@company.com",
        Type = CustomerType.Company,
        Status = CustomerStatus.Active
    };

    // Act
    await _customerRepository.AddAsync(customer);
    await _unitOfWork.SaveChangesAsync();
    
    var retrievedCustomer = await _customerRepository.GetByIdAsync(customer.Id);

    // Assert
    Assert.NotNull(retrievedCustomer);
    Assert.Equal(customer.CompanyName, retrievedCustomer.CompanyName);
    Assert.Equal(customer.Email, retrievedCustomer.Email);
    Assert.False(retrievedCustomer.IsDeleted);
}
```

### **2. API Integration Tests**
Test actual HTTP endpoints and request/response cycles.

**What they test:**
- HTTP request/response cycles
- JSON serialization/deserialization
- HTTP status codes and error handling
- Authentication and authorization
- Input validation
- Swagger documentation accuracy
- API versioning

**Example:**
```csharp
[Fact]
public async Task CustomersController_GetAll_ShouldReturnCustomerList()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/customers");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    var customers = JsonSerializer.Deserialize<PagedResultDto<CustomerDto>>(content);
    customers.Should().NotBeNull();
    customers.Data.Should().NotBeEmpty();
}
```

### **3. Service Layer Integration Tests**
Test services with real dependencies (no mocks).

**What they test:**
- CustomerService with real UnitOfWork
- AutoMapper entity-to-DTO mapping
- Business logic with real data
- Transaction handling
- Error propagation
- Data consistency

**Example:**
```csharp
[Fact]
public async Task CustomerService_WithRealDatabase_ShouldCreateCustomer()
{
    // Arrange
    var createDto = new CreateCustomerDto
    {
        CompanyName = "Integration Test Company",
        Email = "integration@test.com",
        Type = CustomerType.Company,
        Status = CustomerStatus.Active
    };

    // Act
    var result = await _customerService.CreateAsync(createDto, "testuser");

    // Assert
    result.Should().NotBeNull();
    result.CompanyName.Should().Be(createDto.CompanyName);
    result.Email.Should().Be(createDto.Email);
    
    // Verify it was actually saved to database
    var savedCustomer = await _customerService.GetByIdAsync(result.Id);
    savedCustomer.Should().NotBeNull();
}
```

### **4. End-to-End Integration Tests**
Test complete workflows across all layers.

**What they test:**
- Complete customer management workflow
- Opportunity creation and tracking
- Lead conversion process
- Order processing pipeline
- Data consistency across layers
- Performance under load

**Example:**
```csharp
[Fact]
public async Task CreateCustomerWorkflow_ShouldWorkEndToEnd()
{
    // 1. Create customer via API
    var createDto = new CreateCustomerDto { /* ... */ };
    var response = await _client.PostAsJsonAsync("/api/customers", createDto);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    
    // 2. Verify it's stored in database
    var customerId = await response.Content.ReadFromJsonAsync<int>();
    var customer = await _customerService.GetByIdAsync(customerId);
    customer.Should().NotBeNull();
    
    // 3. Retrieve via API
    var getResponse = await _client.GetAsync($"/api/customers/{customerId}");
    getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    
    // 4. Update via API
    var updateDto = new UpdateCustomerDto { /* ... */ };
    var updateResponse = await _client.PutAsJsonAsync($"/api/customers/{customerId}", updateDto);
    updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    
    // 5. Verify changes persisted
    var updatedCustomer = await _customerService.GetByIdAsync(customerId);
    updatedCustomer.Should().NotBeNull();
    updatedCustomer.CompanyName.Should().Be(updateDto.CompanyName);
}
```

## 📁 **Integration Test Project Structure**

```
tests/EnterpriseCRM.IntegrationTests/
├── Controllers/
│   ├── CustomersControllerTests.cs
│   ├── OpportunitiesControllerTests.cs
│   ├── LeadsControllerTests.cs
│   ├── ActivitiesControllerTests.cs
│   ├── ProductsControllerTests.cs
│   └── OrdersControllerTests.cs
├── Services/
│   ├── CustomerServiceIntegrationTests.cs
│   ├── OpportunityServiceIntegrationTests.cs
│   ├── LeadServiceIntegrationTests.cs
│   └── ActivityServiceIntegrationTests.cs
├── Repositories/
│   ├── CustomerRepositoryTests.cs
│   ├── OpportunityRepositoryTests.cs
│   ├── LeadRepositoryTests.cs
│   └── DatabaseContextTests.cs
├── EndToEnd/
│   ├── CustomerWorkflowTests.cs
│   ├── OpportunityWorkflowTests.cs
│   ├── LeadConversionTests.cs
│   └── OrderProcessingTests.cs
├── TestFixtures/
│   ├── DatabaseFixture.cs
│   ├── WebApplicationFactory.cs
│   ├── TestDataBuilder.cs
│   └── IntegrationTestBase.cs
└── TestData/
    ├── SampleCustomers.json
    ├── SampleOpportunities.json
    └── SampleLeads.json
```

## 🛠️ **Integration Test Infrastructure**

### **1. Test Database Setup**
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public ApplicationDbContext Context { get; private set; }
    private string _connectionString;

    public async Task InitializeAsync()
    {
        // Create test database
        _connectionString = "Server=(localdb)\\mssqllocaldb;Database=EnterpriseCRM_Test;Trusted_Connection=true;";
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
            
        Context = new ApplicationDbContext(options);
        await Context.Database.EnsureCreatedAsync();
        
        // Seed test data
        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        Context.Dispose();
    }
}
```

### **2. Web Application Factory**
```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace database with test database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EnterpriseCRM_Test;Trusted_Connection=true;");
            });

            // Add test authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        });
    }
}
```

### **3. Test Data Builders**
```csharp
public class CustomerTestDataBuilder
{
    private Customer _customer = new();

    public CustomerTestDataBuilder WithCompany(string companyName)
    {
        _customer.CompanyName = companyName;
        return this;
    }

    public CustomerTestDataBuilder WithEmail(string email)
    {
        _customer.Email = email;
        return this;
    }

    public CustomerTestDataBuilder AsActive()
    {
        _customer.Status = CustomerStatus.Active;
        return this;
    }

    public Customer Build() => _customer;
}
```

### **4. Integration Test Base Class**
```csharp
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext Context;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Context = factory.Services.GetRequiredService<ApplicationDbContext>();
    }

    protected async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
```

## 🧪 **Key Integration Test Scenarios**

### **Database Integration Scenarios**
- ✅ Entity creation and retrieval
- ✅ Foreign key relationships
- ✅ Soft delete functionality
- ✅ Audit field population
- ✅ Database constraints validation
- ✅ Transaction rollback on errors
- ✅ Concurrent access handling

### **API Integration Scenarios**
- ✅ CRUD operations via HTTP
- ✅ Pagination and filtering
- ✅ Search functionality
- ✅ Error handling and status codes
- ✅ Input validation
- ✅ Authentication and authorization
- ✅ Response format consistency

### **Service Integration Scenarios**
- ✅ Service-to-repository communication
- ✅ AutoMapper entity mapping
- ✅ Business rule enforcement
- ✅ Transaction management
- ✅ Error handling and logging
- ✅ Performance under load

### **End-to-End Scenarios**
- ✅ Complete customer lifecycle
- ✅ Lead to opportunity conversion
- ✅ Opportunity to order workflow
- ✅ Activity tracking and management
- ✅ Data consistency across operations
- ✅ Multi-user scenarios

## 🚀 **Setting Up Integration Tests**

### **1. Create Integration Test Project**
```bash
dotnet new xunit -n EnterpriseCRM.IntegrationTests -o tests/EnterpriseCRM.IntegrationTests
cd tests/EnterpriseCRM.IntegrationTests
dotnet add reference ../../src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj
dotnet add reference ../../src/EnterpriseCRM.Application/EnterpriseCRM.Application.csproj
dotnet add reference ../../src/EnterpriseCRM.Infrastructure/EnterpriseCRM.Infrastructure.csproj
```

### **2. Add Required Packages**
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.69" />
```

### **3. Configure Test Database**
- Use LocalDB for integration tests
- Separate test database (`EnterpriseCRM_Test`)
- Automatic cleanup after each test run
- Seed data for consistent testing

### **4. Run Integration Tests**
```bash
# Run all integration tests
dotnet test tests/EnterpriseCRM.IntegrationTests

# Run specific test category
dotnet test tests/EnterpriseCRM.IntegrationTests --filter Category=Database

# Run with detailed output
dotnet test tests/EnterpriseCRM.IntegrationTests --verbosity normal
```

## 📊 **Integration Test Metrics**

### **Performance Targets**
- **Database Tests:** < 100ms per test
- **API Tests:** < 200ms per test
- **E2E Tests:** < 500ms per test
- **Total Suite:** < 30 seconds

### **Coverage Goals**
- **API Endpoints:** 100% coverage
- **Service Methods:** 90% coverage
- **Repository Methods:** 95% coverage
- **Critical Workflows:** 100% coverage

### **Quality Metrics**
- **Test Reliability:** > 99% pass rate
- **Data Consistency:** 100% validation
- **Error Handling:** Complete coverage
- **Performance:** Within acceptable limits

## 🔧 **Best Practices**

### **Test Organization**
- Group tests by functionality
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)
- Keep tests independent and isolated

### **Data Management**
- Use test data builders
- Clean up after each test
- Avoid shared state
- Use realistic test data

### **Error Handling**
- Test both success and failure scenarios
- Verify error messages and status codes
- Test edge cases and boundary conditions
- Validate error logging

### **Performance**
- Monitor test execution time
- Use appropriate assertions
- Avoid unnecessary database calls
- Optimize test data setup

## 🎯 **Next Steps**

1. **Create Integration Test Project** - Set up the project structure
2. **Implement Database Fixture** - Configure test database
3. **Create Web Application Factory** - Set up API testing
4. **Write First Integration Tests** - Start with Customer operations
5. **Expand Test Coverage** - Add tests for all major workflows
6. **Integrate with CI/CD** - Add to build pipeline

Integration tests are crucial for ensuring the Enterprise CRM system works correctly as a whole, providing confidence in deployments and system reliability.

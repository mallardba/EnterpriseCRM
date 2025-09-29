# Enterprise CRM - Testing Strategy

## 🧪 Test-Driven Development (TDD) Overview

This Enterprise CRM project follows **Test-Driven Development** principles, ensuring high code quality, reliability, and maintainability. The testing strategy covers all layers of the application with comprehensive test coverage.

## 🎯 TDD Process

### **Red-Green-Refactor Cycle**

1. **🔴 Red:** Write a failing test
2. **🟢 Green:** Write minimal code to make the test pass
3. **🔵 Refactor:** Improve code while keeping tests green

### **Benefits of TDD**
- **Better Design:** Tests drive better API design
- **Documentation:** Tests serve as living documentation
- **Confidence:** Safe refactoring and changes
- **Quality:** Fewer bugs in production
- **Maintainability:** Easier to understand and modify code

## 🏗️ Testing Architecture

### **Test Project Structure**
```
tests/
├── EnterpriseCRM.UnitTests/           # Unit tests
│   ├── Services/                      # Service layer tests
│   ├── Controllers/                   # API controller tests
│   ├── Repositories/                  # Repository tests
│   └── Validators/                    # Validation tests
├── EnterpriseCRM.IntegrationTests/     # Integration tests
│   ├── API/                          # API integration tests
│   ├── Database/                     # Database tests
│   └── Authentication/              # Auth integration tests
└── EnterpriseCRM.E2ETests/            # End-to-end tests
    ├── UI/                           # UI automation tests
    └── Workflows/                    # Business workflow tests
```

## 🔧 Testing Technologies

### **xUnit**
- **Purpose:** Unit testing framework
- **Features:** 
  - Simple test discovery
  - Parallel test execution
  - Rich assertion library
  - Test fixtures and data

### **Moq**
- **Purpose:** Mocking framework
- **Features:**
  - Easy mock creation
  - Behavior verification
  - Lambda-based setup
  - Strong typing

### **FluentAssertions**
- **Purpose:** Fluent assertion library
- **Features:**
  - Readable assertions
  - Rich assertion methods
  - Better error messages
  - Natural language syntax

### **Microsoft.EntityFrameworkCore.InMemory**
- **Purpose:** In-memory database for testing
- **Features:**
  - Fast test execution
  - No external dependencies
  - Easy setup and teardown
  - Entity Framework compatibility

## 📊 Test Categories

### **1. Unit Tests**
**Purpose:** Test individual components in isolation

**Coverage:**
- **Service Layer:** Business logic validation
- **Repository Layer:** Data access patterns
- **Validation Logic:** Input validation
- **Mapping Logic:** Object transformations
- **Utility Functions:** Helper methods

**Example - Service Test:**
```csharp
[Fact]
public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomer()
{
    // Arrange
    var customerId = 1;
    var customer = new Customer
    {
        Id = customerId,
        CompanyName = "Test Company",
        Email = "test@company.com"
    };

    _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId))
        .ReturnsAsync(customer);

    // Act
    var result = await _customerService.GetByIdAsync(customerId);

    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(customerId);
    result.CompanyName.Should().Be("Test Company");
}
```

### **2. Integration Tests**
**Purpose:** Test component interactions

**Coverage:**
- **API Endpoints:** HTTP request/response
- **Database Operations:** Data persistence
- **Authentication:** Security validation
- **External Services:** Third-party integrations

**Example - API Integration Test:**
```csharp
[Fact]
public async Task GetCustomers_ShouldReturnPagedResults()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", _token);

    // Act
    var response = await client.GetAsync("/api/customers?pageNumber=1&pageSize=10");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<PagedResultDto<CustomerDto>>(content);
    result.Should().NotBeNull();
    result.Data.Should().NotBeEmpty();
}
```

### **3. End-to-End Tests**
**Purpose:** Test complete user workflows

**Coverage:**
- **User Journeys:** Complete business processes
- **UI Interactions:** User interface testing
- **Cross-System:** Multiple system integration
- **Performance:** Load and stress testing

## 🎯 Test Coverage Goals

### **Coverage Targets**
- **Unit Tests:** 90%+ coverage
- **Integration Tests:** 80%+ coverage
- **API Tests:** 100% endpoint coverage
- **Critical Path Tests:** 100% coverage

### **Coverage Tools**
- **Coverlet:** Code coverage collection
- **ReportGenerator:** Coverage report generation
- **SonarQube:** Quality gate enforcement

## 🔍 Testing Patterns

### **1. Arrange-Act-Assert (AAA)**
```csharp
[Fact]
public async Task CreateCustomer_WithValidData_ShouldCreateCustomer()
{
    // Arrange
    var createDto = new CreateCustomerDto
    {
        CompanyName = "New Company",
        Email = "new@company.com"
    };

    // Act
    var result = await _customerService.CreateAsync(createDto, "testuser");

    // Assert
    result.Should().NotBeNull();
    result.CompanyName.Should().Be(createDto.CompanyName);
}
```

### **2. Test Data Builders**
```csharp
public class CustomerBuilder
{
    private Customer _customer = new Customer();

    public CustomerBuilder WithCompanyName(string companyName)
    {
        _customer.CompanyName = companyName;
        return this;
    }

    public CustomerBuilder WithEmail(string email)
    {
        _customer.Email = email;
        return this;
    }

    public Customer Build() => _customer;
}

// Usage
var customer = new CustomerBuilder()
    .WithCompanyName("Test Company")
    .WithEmail("test@company.com")
    .Build();
```

### **3. Mock Verification**
```csharp
[Fact]
public async Task UpdateCustomer_ShouldCallRepositoryUpdate()
{
    // Arrange
    var updateDto = new UpdateCustomerDto { Id = 1, CompanyName = "Updated" };
    var customer = new Customer { Id = 1, CompanyName = "Original" };

    _customerRepositoryMock.Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(customer);

    // Act
    await _customerService.UpdateAsync(updateDto, "testuser");

    // Assert
    _customerRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

## 🚀 Test Execution

### **Running Tests**
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/EnterpriseCRM.UnitTests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests in parallel
dotnet test --parallel

# Run tests with specific filter
dotnet test --filter "Category=Integration"
```

### **Test Categories**
```csharp
[Fact]
[Trait("Category", "Unit")]
public void UnitTest() { }

[Fact]
[Trait("Category", "Integration")]
public void IntegrationTest() { }

[Fact]
[Trait("Category", "E2E")]
public void EndToEndTest() { }
```

## 📈 Performance Testing

### **Load Testing with NBomber**
```csharp
[Fact]
public async Task GetCustomers_LoadTest()
{
    var scenario = Scenario.Create("get_customers", async context =>
    {
        var response = await _httpClient.GetAsync("/api/customers");
        return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
    })
    .WithLoadSimulations(
        Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(1))
    );

    var stats = await NBomberRunner
        .RegisterScenarios(scenario)
        .Run();

    stats.AllOkCount.Should().BeGreaterThan(0);
}
```

## 🔒 Security Testing

### **Authentication Tests**
```csharp
[Fact]
public async Task GetCustomers_WithoutToken_ShouldReturnUnauthorized()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/customers");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}
```

### **Authorization Tests**
```csharp
[Fact]
public async Task DeleteCustomer_AsRegularUser_ShouldReturnForbidden()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", _userToken);

    // Act
    var response = await client.DeleteAsync("/api/customers/1");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```

## 🧩 Test Fixtures

### **Database Fixture**
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public ApplicationDbContext Context { get; private set; }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
        await SeedDataAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    private async Task SeedDataAsync()
    {
        // Seed test data
        await Context.SaveChangesAsync();
    }
}
```

### **API Fixture**
```csharp
public class ApiFixture : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiFixture(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
```

## 📊 Test Reporting

### **Test Results**
- **Console Output:** Real-time test results
- **HTML Reports:** Detailed test reports
- **Coverage Reports:** Code coverage analysis
- **CI/CD Integration:** Automated test reporting

### **Quality Gates**
- **Minimum Coverage:** 90% unit test coverage
- **No Failing Tests:** All tests must pass
- **Performance Benchmarks:** Response time limits
- **Security Scans:** Vulnerability checks

## 🎯 Best Practices

### **Test Naming**
```csharp
// Good: Describes the scenario and expected outcome
[Fact]
public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomer()

// Bad: Vague and unclear
[Fact]
public async Task TestGetById()
```

### **Test Organization**
- **One Assert Per Test:** Clear test purpose
- **Descriptive Names:** Self-documenting tests
- **Proper Setup/Teardown:** Clean test environment
- **Mock Verification:** Verify interactions

### **Test Data**
- **Isolated Data:** Each test uses its own data
- **Realistic Data:** Use realistic test scenarios
- **Edge Cases:** Test boundary conditions
- **Error Conditions:** Test failure scenarios

## 🚀 Continuous Integration

### **CI/CD Pipeline**
```yaml
- name: Run Unit Tests
  run: dotnet test tests/EnterpriseCRM.UnitTests --collect:"XPlat Code Coverage"

- name: Run Integration Tests
  run: dotnet test tests/EnterpriseCRM.IntegrationTests

- name: Run E2E Tests
  run: dotnet test tests/EnterpriseCRM.E2ETests

- name: Generate Coverage Report
  run: reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
```

This comprehensive testing strategy ensures the Enterprise CRM system maintains high quality, reliability, and maintainability throughout its development lifecycle.

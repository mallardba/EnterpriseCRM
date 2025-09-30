# Enterprise CRM - Clean Architecture & Domain-Driven Design

## 🏗️ Clean Architecture Overview

### **What is Clean Architecture?**

Clean Architecture is a software design philosophy that emphasizes separation of concerns and independence of frameworks, databases, and external agencies. The architecture is based on the principle that business logic should be independent of external concerns.

### **Core Principles**

**Dependency Rule:**
- Dependencies point inward only
- Inner layers don't know about outer layers
- Business logic is independent of external concerns

**Separation of Concerns:**
- Each layer has a single responsibility
- Clear boundaries between layers
- Minimal coupling between components

**Independence:**
- Independent of frameworks
- Independent of databases
- Independent of external agencies
- Testable business logic

### **Why Use Clean Architecture?**

**Maintainability:**
- Clear separation of concerns
- Easy to locate and modify code
- Reduced coupling between components

**Testability:**
- Each layer can be tested independently
- Easy mocking of dependencies
- Isolated business logic testing

**Flexibility:**
- Easy technology stack changes
- Database-agnostic design
- Multiple UI options

**Scalability:**
- Horizontal scaling capabilities
- Microservices-ready architecture
- Independent layer deployment

## 🎯 Domain-Driven Design (DDD)

### **What is Domain-Driven Design?**

Domain-Driven Design is an approach to software development that focuses on the core business logic and domain concepts. It emphasizes understanding the business domain and modeling it accurately in code.

### **Core Concepts**

**Entities:**
- Objects with distinct identity that change over time
- Have a unique identifier
- Can be modified and tracked

**Value Objects:**
- Objects without identity, defined by their attributes
- Immutable once created
- Equality based on attribute values

**Aggregates:**
- Clusters of related entities and value objects
- Consistency boundaries
- Root entity controls access

**Domain Services:**
- Business logic that doesn't belong to entities or value objects
- Stateless operations
- Domain-specific functionality

### **Benefits of DDD**

- **Business Alignment**: Code reflects business concepts
- **Maintainability**: Clear domain models
- **Testability**: Isolated domain logic
- **Scalability**: Well-defined boundaries

## 📐 Core Principles

### **1. Dependency Rule**
- Dependencies point inward only
- Inner layers don't know about outer layers
- Business logic is independent of external concerns

### **2. Separation of Concerns**
- Each layer has a single responsibility
- Clear boundaries between layers
- Minimal coupling between components

### **3. Independence**
- Independent of frameworks
- Independent of databases
- Independent of external agencies
- Testable business logic

## 🎯 Architecture Layers

### **Domain Layer (Core)**
**Purpose:** Contains the core business logic and entities

**Components:**
```csharp
// Entities
public class Customer : BaseEntity
{
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public CustomerType Type { get; set; }
    public CustomerStatus Status { get; set; }
    
    // Business logic methods
    public bool IsActive() => Status == CustomerStatus.Active;
    public bool CanBeContacted() => IsActive() && !string.IsNullOrEmpty(Email);
}

// Value Objects
public class EmailAddress
{
    private readonly string _value;
    
    public EmailAddress(string email)
    {
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email address");
        _value = email;
    }
    
    private bool IsValidEmail(string email) => 
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}

// Domain Services
public interface ICustomerDomainService
{
    bool IsEmailUnique(string email);
    Customer CreateCustomer(string companyName, string email);
}
```

**Key Principles:**
- No external dependencies
- Pure business logic
- Framework-agnostic
- Testable in isolation

### **Application Layer**
**Purpose:** Contains application-specific business logic and use cases

**Components:**
```csharp
// Use Cases (Commands/Queries)
public class CreateCustomerCommand : IRequest<CustomerDto>
{
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public CustomerType Type { get; set; }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Business logic validation
        if (await _unitOfWork.Customers.IsEmailUniqueAsync(request.Email))
        {
            throw new BusinessException("Email already exists");
        }
        
        // Create domain entity
        var customer = new Customer
        {
            CompanyName = request.CompanyName,
            Email = request.Email,
            Type = request.Type,
            Status = CustomerStatus.Active
        };
        
        // Persist
        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();
        
        // Return DTO
        return _mapper.Map<CustomerDto>(customer);
    }
}

// Application Services
public interface ICustomerService
{
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<CustomerDto> GetCustomerByIdAsync(int id);
    Task<PagedResultDto<CustomerDto>> GetCustomersAsync(int pageNumber, int pageSize);
}
```

**Key Principles:**
- Depends only on Domain layer
- Orchestrates business workflows
- Handles cross-cutting concerns
- Implements use cases

### **Infrastructure Layer**
**Purpose:** Handles external concerns like data persistence and external services

**Components:**
```csharp
// Repository Implementation
public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;
    
    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Customer> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }
    
    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _context.Customers
            .AnyAsync(c => c.Email == email && !c.IsDeleted);
    }
}

// External Service Implementation
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    
    public async Task SendWelcomeEmailAsync(string email, string companyName)
    {
        // Implementation for sending welcome email
        // Could use SendGrid, SMTP, etc.
    }
}
```

**Key Principles:**
- Implements Domain interfaces
- Handles technical concerns
- Database-specific implementations
- External service integrations

### **Presentation Layer**
**Purpose:** Handles user interface and API endpoints

**Components:**
```csharp
// API Controller
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        var customer = await _customerService.CreateCustomerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }
}

// Blazor Component
@page "/customers"
@inject ICustomerService CustomerService

<div class="customers-list">
    @foreach (var customer in customers)
    {
        <CustomerCard Customer="customer" />
    }
</div>
```

**Key Principles:**
- Thin presentation logic
- Delegates to Application layer
- Handles HTTP concerns
- User interface rendering

## 🎯 Domain-Driven Design (DDD)

### **Core Concepts**

#### **1. Entities**
Objects with distinct identity that change over time

```csharp
public class Customer : BaseEntity
{
    public int Id { get; set; } // Identity
    public string CompanyName { get; set; }
    public string Email { get; set; }
    
    // Entity behavior
    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrEmpty(newEmail))
            throw new ArgumentException("Email cannot be empty");
        Email = newEmail;
    }
}
```

#### **2. Value Objects**
Objects without identity, defined by their attributes

```csharp
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
    }
}
```

#### **3. Aggregates**
Clusters of related entities and value objects

```csharp
public class CustomerAggregate : BaseEntity
{
    private readonly List<Contact> _contacts = new();
    
    public string CompanyName { get; set; }
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();
    
    public void AddContact(Contact contact)
    {
        if (_contacts.Count >= 10)
            throw new BusinessException("Maximum 10 contacts per customer");
        _contacts.Add(contact);
    }
}
```

#### **4. Domain Services**
Business logic that doesn't belong to entities or value objects

```csharp
public class CustomerDomainService : ICustomerDomainService
{
    public bool IsEmailUnique(string email, IEnumerable<Customer> existingCustomers)
    {
        return !existingCustomers.Any(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
    
    public Customer CreateCustomer(string companyName, string email)
    {
        if (string.IsNullOrEmpty(companyName))
            throw new ArgumentException("Company name is required");
            
        return new Customer
        {
            CompanyName = companyName,
            Email = email,
            Status = CustomerStatus.Active
        };
    }
}
```

### **Bounded Contexts**

#### **Customer Management Context**
- Customer entities
- Contact management
- Customer lifecycle

#### **Sales Context**
- Leads and opportunities
- Sales pipeline
- Revenue tracking

#### **Task Management Context**
- Task entities
- Assignment logic
- Status tracking

## 🔧 Implementation Patterns

### **1. Dependency Injection**
```csharp
// Program.cs
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### **2. Interface Segregation**
```csharp
public interface ICustomerReadRepository
{
    Task<Customer> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status);
}

public interface ICustomerWriteRepository
{
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
}
```

### **3. Specification Pattern**
```csharp
public class ActiveCustomersSpecification : ISpecification<Customer>
{
    public Expression<Func<Customer, bool>> Criteria => c => c.Status == CustomerStatus.Active;
}

public class CustomersByIndustrySpecification : ISpecification<Customer>
{
    private readonly string _industry;
    
    public CustomersByIndustrySpecification(string industry)
    {
        _industry = industry;
    }
    
    public Expression<Func<Customer, bool>> Criteria => c => c.Industry == _industry;
}
```

## 🧪 Testing Clean Architecture

### **Unit Testing Domain Layer**
```csharp
[Fact]
public void Customer_WhenCreated_ShouldBeActive()
{
    // Arrange
    var companyName = "Test Company";
    var email = "test@company.com";
    
    // Act
    var customer = new Customer
    {
        CompanyName = companyName,
        Email = email,
        Status = CustomerStatus.Active
    };
    
    // Assert
    customer.IsActive().Should().BeTrue();
    customer.CanBeContacted().Should().BeTrue();
}
```

### **Integration Testing Application Layer**
```csharp
[Fact]
public async Task CreateCustomerCommand_WithValidData_ShouldCreateCustomer()
{
    // Arrange
    var command = new CreateCustomerCommand
    {
        CompanyName = "Test Company",
        Email = "test@company.com",
        Type = CustomerType.Company
    };
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.CompanyName.Should().Be(command.CompanyName);
}
```

## 🚀 Benefits of Clean Architecture

### **Maintainability**
- Clear separation of concerns
- Easy to locate and modify code
- Reduced coupling between components

### **Testability**
- Each layer can be tested independently
- Easy mocking of dependencies
- Isolated business logic testing

### **Flexibility**
- Easy technology stack changes
- Database-agnostic design
- Multiple UI options

### **Scalability**
- Horizontal scaling capabilities
- Microservices-ready architecture
- Independent layer deployment

## 📊 Best Practices

### **1. Keep Domain Pure**
- No external dependencies in domain layer
- Use interfaces for external concerns
- Focus on business logic only

### **2. Use Dependency Injection**
- Loose coupling between components
- Easy testing and mocking
- Configuration flexibility

### **3. Implement Proper Error Handling**
```csharp
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

### **4. Use Value Objects for Complex Types**
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        if (string.IsNullOrEmpty(currency)) throw new ArgumentException("Currency is required");
        
        Amount = amount;
        Currency = currency;
    }
}
```

This Clean Architecture implementation ensures the Enterprise CRM system is built with enterprise-grade principles, making it maintainable, scalable, and ready for future enhancements.

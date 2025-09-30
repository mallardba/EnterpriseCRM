# Enterprise CRM - CQRS & MediatR Implementation

## 🔄 CQRS (Command Query Responsibility Segregation) Overview

### **What is CQRS?**

CQRS is a pattern that separates read and write operations for a data store. It uses different models to update and read information, optimizing for different use cases and improving scalability.

### **Traditional CRUD vs CQRS**

**Traditional CRUD Approach:**
- Single model for both reading and writing data
- Same data structure used for all operations
- Database operations directly in controllers
- Tight coupling between data access and business logic

**CQRS Approach:**
- Separate models for commands (writes) and queries (reads)
- Optimized data structures for specific use cases
- Clear separation between read and write operations
- Independent scaling of read and write operations

### **Separation of Concerns**

CQRS enforces separation of concerns by:
- **Commands**: Handle write operations and business logic
- **Queries**: Handle read operations and data retrieval
- **Handlers**: Process specific commands or queries
- **Models**: Optimized for specific operations

### **Commands vs Queries**

**Commands:**
- Represent operations that change the state of the system
- Should not return data (except for confirmation)
- Handle business logic and validation
- Examples: CreateCustomer, UpdateCustomer, DeleteCustomer

**Queries:**
- Represent operations that retrieve data without changing state
- Return data to the caller
- Optimized for specific read scenarios
- Examples: GetCustomerById, GetCustomersByStatus, GetCustomerStatistics

### **Independent Optimization**

CQRS allows independent optimization of:
- **Read Operations**: Optimized for fast data retrieval, caching, denormalized views
- **Write Operations**: Optimized for data integrity, validation, business rules
- **Scaling**: Scale read and write operations independently
- **Performance**: Different optimization strategies for different operations

### **Benefits of CQRS**

- **Scalability**: Independent scaling of read and write operations
- **Performance**: Optimized models for specific use cases
- **Maintainability**: Clear separation of concerns
- **Flexibility**: Different data sources for reads and writes
- **Testability**: Isolated testing of commands and queries

## 🎯 MediatR Overview

### **What is MediatR?**

MediatR is a .NET library that implements the Mediator pattern, providing a way to decouple components by centralizing communication through a mediator object.

### **MediatR Pattern Implementation**

The Mediator pattern:
- **Centralized Communication**: All communication goes through a single mediator
- **Decoupled Architecture**: Components don't communicate directly with each other
- **Single Responsibility**: Each handler has one specific responsibility
- **Easy Testing**: Components can be tested in isolation

### **How MediatR Works**

1. **Request**: Client sends a request (command or query)
2. **Mediator**: MediatR receives the request
3. **Handler**: Appropriate handler processes the request
4. **Response**: Handler returns response to client

### **Handlers**

Handlers are responsible for:
- **Command Handlers**: Process commands and perform business logic
- **Query Handlers**: Process queries and return data
- **Single Responsibility**: Each handler handles one specific request type
- **Dependency Injection**: Handlers can inject required services

### **Entry and Dispatch**

- **Entry Point**: Controllers or services send requests to MediatR
- **Dispatch**: MediatR automatically finds and invokes the appropriate handler
- **Pipeline**: Requests can go through pipeline behaviors (validation, logging, etc.)

### **Decoupled Architecture**

MediatR enables:
- **Loose Coupling**: Components don't depend on each other directly
- **Single Responsibility**: Each component has one clear purpose
- **Easy Maintenance**: Changes to one component don't affect others
- **Testability**: Components can be tested independently

### **CQRS Separation with MediatR**

MediatR provides:
- **Command/Query Separation**: Clear distinction between operations
- **Handler Organization**: Separate handlers for commands and queries
- **Pipeline Behaviors**: Cross-cutting concerns (validation, logging)
- **Type Safety**: Strongly-typed requests and responses

### **Why Use CQRS and MediatR Together?**

**Simplification:**
- Clear separation of concerns
- Consistent request/response patterns
- Reduced boilerplate code
- Centralized communication

**Testability:**
- Easy to mock dependencies
- Isolated testing of handlers
- Clear input/output contracts
- Predictable behavior

**Maintainability:**
- Single responsibility principle
- Easy to add new features
- Consistent patterns across the application
- Clear code organization

**Benefits Combined:**
- **Scalability**: Independent optimization of read/write operations
- **Performance**: Optimized models and caching strategies
- **Flexibility**: Easy to change implementations
- **Reliability**: Consistent error handling and validation

## 🎯 Core Concepts

### **1. Commands**
Represent operations that change the state of the system

```csharp
// Command Definition
public class CreateCustomerCommand : IRequest<CustomerDto>
{
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public CustomerType Type { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}

// Command Handler
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ILogger<CreateCustomerCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating customer: {CompanyName}", request.CompanyName);

        // Validate business rules
        if (await _unitOfWork.Customers.IsEmailUniqueAsync(request.Email))
        {
            throw new BusinessException($"Email {request.Email} already exists");
        }

        // Create domain entity
        var customer = new Customer
        {
            CompanyName = request.CompanyName,
            Email = request.Email,
            Type = request.Type,
            Phone = request.Phone,
            Address = request.Address,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System" // This would come from authentication context
        };

        // Persist
        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        // Publish domain event
        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.CompanyName));

        _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.Id);

        return _mapper.Map<CustomerDto>(customer);
    }
}
```

### **2. Queries**
Represent operations that retrieve data without changing state

```csharp
// Query Definition
public class GetCustomerByIdQuery : IRequest<CustomerDto>
{
    public int Id { get; set; }
}

// Query Handler
public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
        
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {request.Id} not found");
        }

        return _mapper.Map<CustomerDto>(customer);
    }
}

// Complex Query
public class GetCustomersPagedQuery : IRequest<PagedResultDto<CustomerDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; }
    public CustomerStatus? Status { get; set; }
    public CustomerType? Type { get; set; }
}

public class GetCustomersPagedQueryHandler : IRequestHandler<GetCustomersPagedQuery, PagedResultDto<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<PagedResultDto<CustomerDto>> Handle(GetCustomersPagedQuery request, CancellationToken cancellationToken)
    {
        var customers = await _unitOfWork.Customers.GetPagedAsync(
            request.PageNumber, 
            request.PageSize, 
            request.SearchTerm, 
            request.Status, 
            request.Type);

        return new PagedResultDto<CustomerDto>
        {
            Data = _mapper.Map<IEnumerable<CustomerDto>>(customers.Items),
            TotalCount = customers.TotalCount,
            PageNumber = customers.PageNumber,
            PageSize = customers.PageSize,
            TotalPages = customers.TotalPages
        };
    }
}
```

## 🔧 MediatR Implementation

### **Setup and Configuration**
```csharp
// Program.cs
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior<ValidationBehavior<,>>();
    cfg.AddBehavior<LoggingBehavior<,>>();
    cfg.AddBehavior<TransactionBehavior<,>>();
});

// Dependency Injection
public static class MediatRExtensions
{
    public static IServiceCollection AddMediatRWithBehaviors(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });

        // Add behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}
```

### **Pipeline Behaviors**

#### **Validation Behavior**
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
```

#### **Logging Behavior**
```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation("Handling {RequestName}", requestName);
        
        try
        {
            var response = await next();
            _logger.LogInformation("Successfully handled {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName}", requestName);
            throw;
        }
    }
}
```

#### **Transaction Behavior**
```csharp
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only apply transaction to commands
        if (request is ICommand<TResponse>)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var response = await next();
                await _unitOfWork.CommitTransactionAsync();
                return response;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        return await next();
    }
}
```

## 📊 Advanced CQRS Patterns

### **Command/Query Interfaces**
```csharp
// Base interfaces
public interface ICommand : IRequest
{
}

public interface ICommand<TResponse> : IRequest<TResponse>
{
}

public interface IQuery<TResponse> : IRequest<TResponse>
{
}

// Usage
public class CreateCustomerCommand : ICommand<CustomerDto>
{
    // Command properties
}

public class GetCustomerByIdQuery : IQuery<CustomerDto>
{
    // Query properties
}
```

### **Domain Events**
```csharp
// Domain Event Base
public abstract class DomainEvent
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

// Specific Domain Events
public class CustomerCreatedEvent : DomainEvent
{
    public int CustomerId { get; }
    public string CompanyName { get; }

    public CustomerCreatedEvent(int customerId, string companyName)
    {
        CustomerId = customerId;
        CompanyName = companyName;
    }
}

public class CustomerUpdatedEvent : DomainEvent
{
    public int CustomerId { get; }
    public string CompanyName { get; }

    public CustomerUpdatedEvent(int customerId, string companyName)
    {
        CustomerId = customerId;
        CompanyName = companyName;
    }
}

// Domain Event Handler
public class CustomerCreatedEventHandler : INotificationHandler<CustomerCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<CustomerCreatedEventHandler> _logger;

    public CustomerCreatedEventHandler(IEmailService emailService, ILogger<CustomerCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Customer created event received for customer {CustomerId}", notification.CustomerId);

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(notification.CustomerId, notification.CompanyName);

        // Update analytics
        // Update search index
        // Send notifications
    }
}
```

### **Read Models and Projections**
```csharp
// Read Model
public class CustomerReadModel
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public CustomerType Type { get; set; }
    public CustomerStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ContactCount { get; set; }
    public int LeadCount { get; set; }
    public int OpportunityCount { get; set; }
    public decimal TotalOpportunityValue { get; set; }
}

// Projection Handler
public class CustomerProjectionHandler : INotificationHandler<CustomerCreatedEvent>
{
    private readonly IReadModelRepository<CustomerReadModel> _readModelRepository;

    public CustomerProjectionHandler(IReadModelRepository<CustomerReadModel> readModelRepository)
    {
        _readModelRepository = readModelRepository;
    }

    public async Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        var readModel = new CustomerReadModel
        {
            Id = notification.CustomerId,
            CompanyName = notification.CompanyName,
            CreatedAt = notification.OccurredOn
        };

        await _readModelRepository.AddAsync(readModel);
    }
}
```

## 🎯 Controller Integration

### **API Controller with MediatR**
```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var query = new GetCustomerByIdQuery { Id = id };
        var customer = await _mediator.Send(query);
        return Ok(customer);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CustomerDto>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = null,
        [FromQuery] CustomerStatus? status = null,
        [FromQuery] CustomerType? type = null)
    {
        var query = new GetCustomersPagedQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            Status = status,
            Type = type
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerCommand command)
    {
        var customer = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] UpdateCustomerCommand command)
    {
        command.Id = id;
        var customer = await _mediator.Send(command);
        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteCustomerCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
```

## 🧪 Testing CQRS with MediatR

### **Unit Testing Commands**
```csharp
public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateCustomerCommandHandler>> _loggerMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateCustomerCommandHandler>>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        
        _handler = new CreateCustomerCommandHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCustomer()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            CompanyName = "Test Company",
            Email = "test@company.com",
            Type = CustomerType.Company
        };

        var customer = new Customer { Id = 1, CompanyName = command.CompanyName };
        var customerDto = new CustomerDto { Id = 1, CompanyName = command.CompanyName };

        _customerRepositoryMock.Setup(r => r.IsEmailUniqueAsync(command.Email))
            .ReturnsAsync(true);
        _customerRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<CustomerDto>(customer))
            .Returns(customerDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CompanyName.Should().Be(command.CompanyName);
        
        _customerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
```

### **Integration Testing**
```csharp
public class CustomerCQRSIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomerCQRSIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnCreatedCustomer()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            CompanyName = "Integration Test Company",
            Email = "integration@test.com",
            Type = CustomerType.Company
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer.CompanyName.Should().Be(command.CompanyName);
    }
}
```

## 🚀 Benefits of CQRS with MediatR

### **Separation of Concerns**
- Clear distinction between read and write operations
- Optimized models for different use cases
- Independent scaling of read and write operations

### **Testability**
- Easy to test commands and queries independently
- Mockable dependencies through MediatR
- Isolated business logic testing

### **Maintainability**
- Single responsibility for each handler
- Consistent request/response patterns
- Easy to add cross-cutting concerns

### **Performance**
- Optimized read models for queries
- Caching strategies for read operations
- Efficient write operations

## 📊 Best Practices

### **1. Keep Commands and Queries Simple**
```csharp
// Good: Simple command
public class CreateCustomerCommand : ICommand<CustomerDto>
{
    public string CompanyName { get; set; }
    public string Email { get; set; }
}

// Avoid: Complex command with too many responsibilities
public class CreateCustomerWithContactsAndTasksCommand : ICommand<CustomerDto>
{
    // Too many responsibilities
}
```

### **2. Use Validation Attributes**
```csharp
public class CreateCustomerCommand : ICommand<CustomerDto>
{
    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

### **3. Handle Exceptions Appropriately**
```csharp
public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
{
    try
    {
        // Command logic
    }
    catch (BusinessException ex)
    {
        _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error creating customer");
        throw new ApplicationException("An error occurred while creating the customer", ex);
    }
}
```

### **4. Use Domain Events for Side Effects**
```csharp
public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
{
    var customer = new Customer { /* properties */ };
    
    await _unitOfWork.Customers.AddAsync(customer);
    await _unitOfWork.SaveChangesAsync();

    // Publish domain event instead of calling services directly
    customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.CompanyName));
    
    return _mapper.Map<CustomerDto>(customer);
}
```

CQRS with MediatR provides a powerful foundation for building scalable, maintainable applications with clear separation of concerns and excellent testability.

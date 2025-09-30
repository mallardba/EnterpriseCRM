# Enterprise CRM - Validation with FluentValidation

## ✅ FluentValidation Overview

### **What is FluentValidation?**

FluentValidation is a .NET library for building strongly-typed validation rules using a fluent interface. It provides a clean, readable way to define validation logic and integrates seamlessly with ASP.NET Core.

### **Why Use FluentValidation?**

**Fluent Interface:**
- Readable and expressive validation rules
- Method chaining for complex validations
- Intuitive syntax for developers

**Strongly-Typed:**
- Compile-time safety
- IntelliSense support
- Refactoring-friendly

**Separation of Concerns:**
- Validation logic separated from models
- Reusable validation rules
- Easy to test validation logic

**Integration:**
- Seamless ASP.NET Core integration
- Automatic model validation
- Custom error handling

### **FluentValidation Benefits**

- **Readability**: Clear and expressive validation rules
- **Maintainability**: Centralized validation logic
- **Testability**: Easy to unit test validation rules
- **Flexibility**: Complex validation scenarios
- **Performance**: Efficient validation execution

### **When to Use FluentValidation**

**Good Use Cases:**
- Complex validation rules
- Reusable validation logic
- Multiple validation scenarios
- Custom validation requirements

**Avoid When:**
- Simple attribute-based validation
- Performance-critical validation
- Over-engineering simple rules
- Minimal validation requirements

## 🎯 Core Concepts

### **1. Basic Validator**
Simple validation rules using fluent syntax

```csharp
public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerDtoValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("Company name is required")
            .MaximumLength(200)
            .WithMessage("Company name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^[\+]?[1-9][\d]{0,15}$")
            .WithMessage("Phone number must be in valid format")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Customer type must be a valid value");
    }
}
```

### **2. Complex Validation Rules**
Advanced validation with custom logic

```csharp
public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
{
    private readonly ICustomerRepository _customerRepository;

    public UpdateCustomerDtoValidator(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Customer ID must be greater than 0");

        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("Company name is required")
            .MaximumLength(200)
            .WithMessage("Company name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MustAsync(BeUniqueEmailAsync)
            .WithMessage("Email already exists");

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .MaximumLength(50)
            .WithMessage("State cannot exceed 50 characters");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .WithMessage("Postal code cannot exceed 20 characters");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage("Country cannot exceed 100 characters");
    }

    private async Task<bool> BeUniqueEmailAsync(UpdateCustomerDto dto, string email, CancellationToken cancellationToken)
    {
        var existingCustomer = await _customerRepository.GetByEmailAsync(email);
        return existingCustomer == null || existingCustomer.Id == dto.Id;
    }
}
```

## 🔧 Advanced Validation Patterns

### **1. Conditional Validation**
Validation rules that apply based on conditions

```csharp
public class LeadValidator : AbstractValidator<CreateLeadDto>
{
    public LeadValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("Company name is required");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        // Conditional validation based on lead type
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .When(x => x.Type == LeadType.Individual);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .When(x => x.Type == LeadType.Individual);

        RuleFor(x => x.JobTitle)
            .NotEmpty()
            .WithMessage("Job title is required")
            .When(x => x.Type == LeadType.Company);

        // Conditional validation based on priority
        RuleFor(x => x.ExpectedCloseDate)
            .NotNull()
            .WithMessage("Expected close date is required")
            .GreaterThan(DateTime.Today)
            .WithMessage("Expected close date must be in the future")
            .When(x => x.Priority == LeadPriority.High || x.Priority == LeadPriority.Critical);

        RuleFor(x => x.EstimatedValue)
            .GreaterThan(0)
            .WithMessage("Estimated value must be greater than 0")
            .When(x => x.Priority == LeadPriority.High || x.Priority == LeadPriority.Critical);
    }
}
```

### **2. Cross-Property Validation**
Validation that involves multiple properties

```csharp
public class OpportunityValidator : AbstractValidator<CreateOpportunityDto>
{
    public OpportunityValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("Customer ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Opportunity name is required")
            .MaximumLength(200)
            .WithMessage("Opportunity name cannot exceed 200 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .LessThan(10000000)
            .WithMessage("Amount cannot exceed $10,000,000");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100)
            .WithMessage("Probability must be between 0 and 100");

        RuleFor(x => x.ExpectedCloseDate)
            .NotNull()
            .WithMessage("Expected close date is required")
            .GreaterThan(DateTime.Today)
            .WithMessage("Expected close date must be in the future");

        // Cross-property validation
        RuleFor(x => x)
            .Must(HaveValidProbabilityForStage)
            .WithMessage("Probability must be appropriate for the opportunity stage");

        RuleFor(x => x)
            .Must(HaveValidCloseDateForAmount)
            .WithMessage("Expected close date must be reasonable for the opportunity amount");
    }

    private bool HaveValidProbabilityForStage(CreateOpportunityDto dto)
    {
        return dto.Stage switch
        {
            OpportunityStage.Prospecting => dto.Probability <= 25,
            OpportunityStage.Qualification => dto.Probability <= 50,
            OpportunityStage.Proposal => dto.Probability <= 75,
            OpportunityStage.Negotiation => dto.Probability <= 90,
            OpportunityStage.ClosedWon => dto.Probability == 100,
            OpportunityStage.ClosedLost => dto.Probability == 0,
            _ => true
        };
    }

    private bool HaveValidCloseDateForAmount(CreateOpportunityDto dto)
    {
        if (dto.Amount > 1000000)
        {
            return dto.ExpectedCloseDate >= DateTime.Today.AddMonths(3);
        }
        return true;
    }
}
```

### **3. Custom Validators**
Reusable validation logic

```csharp
public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^[\+]?[1-9][\d]{0,15}$")
            .WithMessage("Phone number must be in valid format");
    }

    public static IRuleBuilderOptions<T, string> ValidPostalCode<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^\d{5}(-\d{4})?$")
            .WithMessage("Postal code must be in valid format (12345 or 12345-6789)");
    }

    public static IRuleBuilderOptions<T, string> ValidWebsite<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$")
            .WithMessage("Website must be a valid URL");
    }
}

// Usage in validators
public class CustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Phone)
            .ValidPhoneNumber()
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.PostalCode)
            .ValidPostalCode()
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Website)
            .ValidWebsite()
            .When(x => !string.IsNullOrEmpty(x.Website));
    }
}
```

### **4. Async Validation**
Validation that requires async operations

```csharp
public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    private readonly ICustomerRepository _customerRepository;

    public CreateCustomerDtoValidator(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MustAsync(BeUniqueEmailAsync)
            .WithMessage("Email already exists");

        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("Company name is required")
            .MustAsync(BeUniqueCompanyNameAsync)
            .WithMessage("Company name already exists");
    }

    private async Task<bool> BeUniqueEmailAsync(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email))
            return true;

        var existingCustomer = await _customerRepository.GetByEmailAsync(email);
        return existingCustomer == null;
    }

    private async Task<bool> BeUniqueCompanyNameAsync(string companyName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(companyName))
            return true;

        var existingCustomer = await _customerRepository.GetByCompanyNameAsync(companyName);
        return existingCustomer == null;
    }
}
```

## 🔄 Integration with ASP.NET Core

### **Service Registration**
```csharp
// Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Register validators
builder.Services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateCustomerDto>, UpdateCustomerDtoValidator>();
builder.Services.AddScoped<IValidator<CreateLeadDto>, LeadValidator>();
builder.Services.AddScoped<IValidator<CreateOpportunityDto>, OpportunityValidator>();

// Or register all validators in assembly
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
```

### **Controller Integration**
```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IValidator<CreateCustomerDto> _createValidator;
    private readonly IValidator<UpdateCustomerDto> _updateValidator;

    public CustomersController(
        ICustomerService customerService,
        IValidator<CreateCustomerDto> createValidator,
        IValidator<UpdateCustomerDto> updateValidator)
    {
        _customerService = customerService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var customer = await _customerService.CreateCustomerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        dto.Id = id;
        
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var customer = await _customerService.UpdateCustomerAsync(dto);
        return Ok(customer);
    }
}
```

### **Automatic Validation with MediatR**
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

## 🎯 Validation Error Handling

### **Custom Validation Exception**
```csharp
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}
```

### **Global Exception Handler**
```csharp
public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            var response = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "One or more validation errors occurred.",
                status = 400,
                errors = ex.Errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
```

## 🧪 Testing Validation

### **Unit Testing Validators**
```csharp
public class CreateCustomerDtoValidatorTests
{
    private readonly CreateCustomerDtoValidator _validator;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;

    public CreateCustomerDtoValidatorTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _validator = new CreateCustomerDtoValidator(_customerRepositoryMock.Object);
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new CreateCustomerDto
        {
            CompanyName = "Test Company",
            Email = "test@company.com",
            Type = CustomerType.Company
        };

        _customerRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((Customer)null);

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCompanyName_ShouldFail()
    {
        // Arrange
        var dto = new CreateCustomerDto
        {
            CompanyName = "",
            Email = "test@company.com",
            Type = CustomerType.Company
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompanyName");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var dto = new CreateCustomerDto
        {
            CompanyName = "Test Company",
            Email = "invalid-email",
            Type = CustomerType.Company
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task ValidateAsync_WithDuplicateEmail_ShouldFail()
    {
        // Arrange
        var dto = new CreateCustomerDto
        {
            CompanyName = "Test Company",
            Email = "existing@company.com",
            Type = CustomerType.Company
        };

        var existingCustomer = new Customer { Id = 1, Email = dto.Email };
        _customerRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(existingCustomer);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
}
```

### **Integration Testing**
```csharp
public class CustomerValidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomerValidationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidData_ShouldReturnValidationErrors()
    {
        // Arrange
        var invalidDto = new CreateCustomerDto
        {
            CompanyName = "", // Invalid: empty
            Email = "invalid-email", // Invalid: not a valid email
            Type = (CustomerType)999 // Invalid: not a valid enum value
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Company name is required");
        content.Should().Contain("Email must be a valid email address");
        content.Should().Contain("Customer type must be a valid value");
    }
}
```

## 🚀 Performance Optimization

### **1. Caching Validators**
```csharp
public class CachedValidator<T> : IValidator<T>
{
    private readonly IValidator<T> _validator;
    private readonly IMemoryCache _cache;

    public CachedValidator(IValidator<T> validator, IMemoryCache cache)
    {
        _validator = validator;
        _cache = cache;
    }

    public ValidationResult Validate(T instance)
    {
        var cacheKey = GetCacheKey(instance);
        
        if (_cache.TryGetValue(cacheKey, out ValidationResult cachedResult))
        {
            return cachedResult;
        }

        var result = _validator.Validate(instance);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        
        return result;
    }

    private string GetCacheKey(T instance)
    {
        // Generate cache key based on instance properties
        return $"validation_{typeof(T).Name}_{instance.GetHashCode()}";
    }
}
```

### **2. Lazy Validation**
```csharp
public class LazyValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public LazyValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ValidationResult> ValidateAsync<T>(T instance)
    {
        var validator = _serviceProvider.GetRequiredService<IValidator<T>>();
        return await validator.ValidateAsync(instance);
    }
}
```

## 📊 Best Practices

### **1. Keep Validators Focused**
```csharp
// Good: Focused validator
public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    // Only validation for CreateCustomerDto
}

// Avoid: Generic validator for multiple DTOs
public class AllDtoValidator : AbstractValidator<object>
{
    // Too generic and hard to maintain
}
```

### **2. Use Meaningful Error Messages**
```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .WithMessage("Email address is required to create a customer account")
    .EmailAddress()
    .WithMessage("Please enter a valid email address (e.g., user@company.com)");
```

### **3. Group Related Validations**
```csharp
RuleFor(x => x.ContactInfo)
    .SetValidator(new ContactInfoValidator());

public class ContactInfoValidator : AbstractValidator<ContactInfo>
{
    public ContactInfoValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Phone).ValidPhoneNumber();
    }
}
```

### **4. Use Conditional Validation Wisely**
```csharp
RuleFor(x => x.FirstName)
    .NotEmpty()
    .WithMessage("First name is required")
    .When(x => x.Type == CustomerType.Individual)
    .WithMessage("First name is required for individual customers");
```

FluentValidation provides a powerful and flexible validation framework for the Enterprise CRM system, ensuring data integrity and providing clear feedback to users.

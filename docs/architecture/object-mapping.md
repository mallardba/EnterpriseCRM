# Enterprise CRM - Object Mapping with AutoMapper

## 🔄 AutoMapper Overview

### **What is AutoMapper?**

AutoMapper is a convention-based object-to-object mapping library that eliminates the need for manual mapping code. It automatically maps properties between objects based on naming conventions and custom configurations.

### **Why Use AutoMapper?**

**Eliminates Boilerplate:**
- Reduces manual mapping code
- Automatic property mapping
- Consistent mapping patterns

**Convention-Based:**
- Maps properties by name automatically
- Handles complex mapping scenarios
- Reduces configuration overhead

**Performance:**
- Compiled mappings for speed
- Efficient object transformation
- Minimal memory allocation

**Maintainability:**
- Centralized mapping configuration
- Easy to modify mapping logic
- Clear separation of concerns

### **AutoMapper Benefits**

- **Productivity**: Faster development with less mapping code
- **Consistency**: Standardized mapping patterns
- **Flexibility**: Custom mapping configurations
- **Performance**: Optimized object transformation
- **Testability**: Easy to test mapping logic

### **When to Use AutoMapper**

**Good Use Cases:**
- Entity to DTO mapping
- Complex object transformations
- Multiple mapping scenarios
- Performance-critical applications

**Avoid When:**
- Simple one-to-one property mapping
- Performance is extremely critical
- Complex business logic in mapping
- Over-engineering simple scenarios

## 🎯 Core Concepts

### **1. Basic Mapping**
Simple property-to-property mapping between objects

```csharp
// Entity
public class Customer : BaseEntity
{
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public CustomerType Type { get; set; }
    public CustomerStatus Status { get; set; }
}

// DTO
public class CustomerDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public CustomerType Type { get; set; }
    public CustomerStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Basic mapping (convention-based)
var customer = new Customer { CompanyName = "Test Company", Email = "test@company.com" };
var customerDto = _mapper.Map<CustomerDto>(customer);
```

### **2. Profile Configuration**
Organized mapping configurations using profiles

```csharp
public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<CreateCustomerDto, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "System"))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        CreateMap<UpdateCustomerDto, Customer>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => "System"));
    }
}
```

## 🔧 Advanced Mapping Patterns

### **1. Custom Value Resolvers**
Handle complex mapping logic

```csharp
// Custom resolver for full name
public class FullNameResolver : IValueResolver<Customer, CustomerDto, string>
{
    public string Resolve(Customer source, CustomerDto destination, string destMember, ResolutionContext context)
    {
        if (source.Type == CustomerType.Individual)
        {
            return $"{source.FirstName} {source.LastName}".Trim();
        }
        return source.CompanyName;
    }
}

// Usage in profile
CreateMap<Customer, CustomerDto>()
    .ForMember(dest => dest.DisplayName, opt => opt.MapFrom<FullNameResolver>());

// Custom resolver with dependency injection
public class CustomerStatusResolver : IValueResolver<Customer, CustomerDto, string>
{
    private readonly IStatusService _statusService;

    public CustomerStatusResolver(IStatusService statusService)
    {
        _statusService = statusService;
    }

    public string Resolve(Customer source, CustomerDto destination, string destMember, ResolutionContext context)
    {
        return _statusService.GetStatusDescription(source.Status);
    }
}
```

### **2. Conditional Mapping**
Map properties based on conditions

```csharp
public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.CanEdit, opt => opt.MapFrom(src => src.Status == CustomerStatus.Active))
            .ForMember(dest => dest.ContactCount, opt => opt.MapFrom(src => src.Contacts.Count))
            .ForMember(dest => dest.LastActivityDate, opt => opt.MapFrom(src => 
                src.Tasks.OrderByDescending(t => t.CreatedAt).FirstOrDefault()?.CreatedAt ?? src.CreatedAt))
            .ForMember(dest => dest.IsHighValue, opt => opt.MapFrom(src => 
                src.Opportunities.Sum(o => o.Amount) > 100000));
    }
}
```

### **3. Nested Object Mapping**
Map complex object hierarchies

```csharp
// Nested DTOs
public class CustomerDetailDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public List<ContactDto> Contacts { get; set; }
    public List<OpportunityDto> Opportunities { get; set; }
    public List<TaskDto> RecentTasks { get; set; }
}

public class ContactDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string JobTitle { get; set; }
    public bool IsPrimary { get; set; }
}

// Mapping configuration
public class CustomerDetailMappingProfile : Profile
{
    public CustomerDetailMappingProfile()
    {
        CreateMap<Customer, CustomerDetailDto>()
            .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts))
            .ForMember(dest => dest.Opportunities, opt => opt.MapFrom(src => src.Opportunities))
            .ForMember(dest => dest.RecentTasks, opt => opt.MapFrom(src => 
                src.Tasks.OrderByDescending(t => t.CreatedAt).Take(5)));

        CreateMap<Contact, ContactDto>();
        CreateMap<Opportunity, OpportunityDto>();
        CreateMap<Task, TaskDto>();
    }
}
```

### **4. Collection Mapping**
Map collections and arrays

```csharp
// Collection mapping
public class CustomerListDto
{
    public List<CustomerSummaryDto> Customers { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class CustomerSummaryDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public CustomerStatus Status { get; set; }
    public int ContactCount { get; set; }
}

// Mapping configuration
CreateMap<PagedResult<Customer>, CustomerListDto>()
    .ForMember(dest => dest.Customers, opt => opt.MapFrom(src => src.Items))
    .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.TotalCount))
    .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
    .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize));

CreateMap<Customer, CustomerSummaryDto>()
    .ForMember(dest => dest.ContactCount, opt => opt.MapFrom(src => src.Contacts.Count));
```

## 🎯 Service Integration

### **AutoMapper Service Registration**
```csharp
// Program.cs
builder.Services.AddAutoMapper(typeof(Program));

// Or with specific assemblies
builder.Services.AddAutoMapper(
    typeof(CustomerMappingProfile),
    typeof(LeadMappingProfile),
    typeof(OpportunityMappingProfile));

// Custom configuration
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<CustomerMappingProfile>();
    cfg.AddProfile<LeadMappingProfile>();
    cfg.AddProfile<OpportunityMappingProfile>();
    
    // Global configuration
    cfg.AllowNullCollections = true;
    cfg.AllowNullDestinationValues = true;
}, typeof(Program));
```

### **Service Usage**
```csharp
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerDto> GetCustomerByIdAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null)
            return null;

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<PagedResultDto<CustomerDto>> GetCustomersAsync(int pageNumber, int pageSize)
    {
        var customers = await _unitOfWork.Customers.GetPagedAsync(pageNumber, pageSize);
        return _mapper.Map<PagedResultDto<CustomerDto>>(customers);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        var customer = _mapper.Map<Customer>(dto);
        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<CustomerDto>(customer);
    }
}
```

## 🔄 Advanced Features

### **1. Custom Type Converters**
```csharp
public class DateTimeToStringConverter : ITypeConverter<DateTime, string>
{
    public string Convert(DateTime source, string destination, ResolutionContext context)
    {
        return source.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

public class StringToDateTimeConverter : ITypeConverter<string, DateTime>
{
    public DateTime Convert(string source, DateTime destination, ResolutionContext context)
    {
        return DateTime.TryParse(source, out var result) ? result : DateTime.MinValue;
    }
}

// Usage in profile
CreateMap<Customer, CustomerDto>()
    .ForMember(dest => dest.CreatedAtString, opt => opt.ConvertUsing<DateTimeToStringConverter, DateTime>(src => src.CreatedAt));
```

### **2. Mapping Inheritance**
```csharp
public class BaseEntityMappingProfile : Profile
{
    public BaseEntityMappingProfile()
    {
        CreateMap<BaseEntity, BaseEntityDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));
    }
}

public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .IncludeBase<BaseEntity, BaseEntityDto>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName));
    }
}
```

### **3. Mapping Validation**
```csharp
public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Email, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Email)))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.CompanyName, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.CompanyName)));
    }
}
```

### **4. Custom Mapping Actions**
```csharp
public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .AfterMap((src, dest, context) =>
            {
                // Custom logic after mapping
                dest.DisplayName = src.Type == CustomerType.Individual 
                    ? $"{src.FirstName} {src.LastName}".Trim()
                    : src.CompanyName;
                    
                dest.IsActive = src.Status == CustomerStatus.Active;
            })
            .BeforeMap((src, dest, context) =>
            {
                // Custom logic before mapping
                if (string.IsNullOrEmpty(src.Email))
                {
                    throw new MappingException("Email is required for customer mapping");
                }
            });
    }
}
```

## 🧪 Testing AutoMapper

### **Mapping Configuration Tests**
```csharp
public class CustomerMappingProfileTests
{
    private readonly IMapper _mapper;

    public CustomerMappingProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CustomerMappingProfile>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void CustomerMappingProfile_ShouldBeValid()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CustomerMappingProfile>();
        });

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_CustomerToCustomerDto_ShouldMapCorrectly()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            CompanyName = "Test Company",
            Email = "test@company.com",
            Type = CustomerType.Company,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var customerDto = _mapper.Map<CustomerDto>(customer);

        // Assert
        customerDto.Should().NotBeNull();
        customerDto.Id.Should().Be(customer.Id);
        customerDto.CompanyName.Should().Be(customer.CompanyName);
        customerDto.Email.Should().Be(customer.Email);
        customerDto.Type.Should().Be(customer.Type);
        customerDto.Status.Should().Be(customer.Status);
    }

    [Fact]
    public void Map_CreateCustomerDtoToCustomer_ShouldMapCorrectly()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            CompanyName = "New Company",
            Email = "new@company.com",
            Type = CustomerType.Company
        };

        // Act
        var customer = _mapper.Map<Customer>(createDto);

        // Assert
        customer.Should().NotBeNull();
        customer.CompanyName.Should().Be(createDto.CompanyName);
        customer.Email.Should().Be(createDto.Email);
        customer.Type.Should().Be(createDto.Type);
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.IsDeleted.Should().BeFalse();
    }
}
```

### **Integration Testing**
```csharp
public class CustomerServiceMappingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomerServiceMappingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomer_ShouldReturnCorrectlyMappedDto()
    {
        // Arrange
        var customerId = 1;

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}");
        var customerDto = await response.Content.ReadFromJsonAsync<CustomerDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        customerDto.Should().NotBeNull();
        customerDto.Id.Should().Be(customerId);
        customerDto.CompanyName.Should().NotBeNullOrEmpty();
        customerDto.Email.Should().NotBeNullOrEmpty();
    }
}
```

## 🚀 Performance Optimization

### **1. Compiled Mappings**
```csharp
// Enable compiled mappings for better performance
var configuration = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<CustomerMappingProfile>();
    cfg.CompileMappings(); // Compile mappings at startup
});

var mapper = configuration.CreateMapper();
```

### **2. Mapping Caching**
```csharp
public class CachedMappingService
{
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public CachedMappingService(IMapper mapper, IMemoryCache cache)
    {
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<CustomerDto> GetCustomerDtoAsync(int customerId)
    {
        var cacheKey = $"customer_dto_{customerId}";
        
        if (_cache.TryGetValue(cacheKey, out CustomerDto cachedDto))
        {
            return cachedDto;
        }

        var customer = await _customerRepository.GetByIdAsync(customerId);
        var dto = _mapper.Map<CustomerDto>(customer);
        
        _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
        
        return dto;
    }
}
```

### **3. Projection Mapping**
```csharp
// Use projection for better performance
public async Task<IEnumerable<CustomerSummaryDto>> GetCustomerSummariesAsync()
{
    return await _context.Customers
        .Where(c => !c.IsDeleted)
        .ProjectTo<CustomerSummaryDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
}
```

## 📊 Best Practices

### **1. Use Profiles for Organization**
```csharp
// Good: Organized by feature
public class CustomerMappingProfile : Profile { }
public class LeadMappingProfile : Profile { }
public class OpportunityMappingProfile : Profile { }

// Avoid: One large profile for everything
public class AllMappingsProfile : Profile { }
```

### **2. Handle Null Values**
```csharp
CreateMap<Customer, CustomerDto>()
    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone ?? string.Empty))
    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address ?? "Not provided"));
```

### **3. Use Conditional Mapping**
```csharp
CreateMap<Customer, CustomerDto>()
    .ForMember(dest => dest.CanEdit, opt => opt.MapFrom(src => src.Status == CustomerStatus.Active))
    .ForMember(dest => dest.IsHighValue, opt => opt.MapFrom(src => 
        src.Opportunities.Sum(o => o.Amount) > 100000));
```

### **4. Validate Mapping Configuration**
```csharp
// Always validate configuration in tests
[Fact]
public void MappingConfiguration_ShouldBeValid()
{
    var configuration = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<CustomerMappingProfile>();
    });

    configuration.AssertConfigurationIsValid();
}
```

AutoMapper provides a powerful and flexible solution for object mapping in the Enterprise CRM system, reducing boilerplate code and ensuring consistent data transformation between layers.

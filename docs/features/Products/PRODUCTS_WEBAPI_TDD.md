# Products Controller - Test Driven Development Guide

This document demonstrates how to implement the ProductsController using Test Driven Development (TDD) principles, suitable for technical interviews where TDD expertise is expected.

## 🎯 Overview

We'll implement a complete Product management system following the **Red-Green-Refactor** cycle:

1. **Red**: Write a failing test first
2. **Green**: Write the minimum code to make the test pass
3. **Refactor**: Improve the code while keeping tests green
4. **Repeat**: Continue for each feature

---

## 📑 Table of Contents

1. [TDD Process Overview](#-tdd-process-overview)
2. [Implementation Order](#-implementation-order)
3. [Step 1: Define Test Requirements](#-step-1-define-test-requirements-before-writing-code)
4. [Step 2: Red Phase - Write Failing Tests](#-step-2-red-phase---write-failing-tests)
5. [Step 3: Green Phase - Implement Minimum Code](#-step-3-green-phase---implement-minimum-code-to-pass-tests)
6. [Step 4: Run Tests to Verify](#-step-4-run-tests-to-verify)
7. [Step 5: Refactor Phase - Improve Code](#-step-5-refactor-phase---improve-code)
8. [Step 6: Documentation and Interview Talking Points](#-step-6-documentation-and-interview-talking-points)
9. [Step 7: Complete Test Suite](#-step-7-complete-test-suite)
10. [Step 8: Verify in Swagger](#-step-8-verify-in-swagger)
11. [TDD Interview Flow Summary](#-summary-tdd-interview-flow)
12. [Architecture Overview](#-architecture-overview-how-everything-connects)
13. [Clean Architecture Layers Summary](#️-clean-architecture-layers-summary)

---

## 📋 TDD Process Overview

## 🏗️ Implementation Order

When creating a new feature using Clean Architecture and TDD, follow this implementation order:

1. **Write Entity Tests (RED)** → `tests/EnterpriseCRM.UnitTests/Entities/ProductTests.cs` - **Testing Layer**
   - Test Product entity properties and validation
   - Verify navigation properties work correctly
   - Tests should FAIL (Product doesn't exist yet)

2. **Create Entity (GREEN)** → `src/EnterpriseCRM.Core/Entities.cs` - **Core Layer**
   - Define Product class with properties
   - Add navigation properties (OrderItems, etc.)
   - Entity tests now PASS

3. **Write Service Tests (RED)** → `tests/EnterpriseCRM.UnitTests/Services/ProductServiceTests.cs` - **Testing Layer**
   - Test ProductService.GetByIdAsync, GetAllAsync, CreateAsync
   - Mock IUnitOfWork and IMapper
   - Tests should FAIL (ProductService doesn't exist yet)

4. **Create DTOs** → `src/EnterpriseCRM.Application/DTOs.cs` - **Application Layer**
   - ProductDto (for read operations)
   - CreateProductDto (for create operations)
   - UpdateProductDto (for update operations)

6. **Create Repository Interface** → `src/EnterpriseCRM.Core/Interfaces.cs` - **Core Layer**
   - IProductRepository interface extending IRepository<Product>

7. **Implement Repository** → `src/EnterpriseCRM.Infrastructure/Repositories/Repositories.cs` - **Infrastructure Layer**
   - ProductRepository class implementing IProductRepository
   - Add specific methods (GetByCategory, Search, etc.)

8. **Add to Unit of Work** → `src/EnterpriseCRM.Infrastructure/UnitOfWork/UnitOfWork.cs` - **Infrastructure Layer**
   - Add IProductRepository Products property
   - Instantiate ProductRepository in constructor

9. **Create Service Interface** → `src/EnterpriseCRM.Application/Interfaces.cs` - **Application Layer**
   - IProductService interface with methods (GetByIdAsync, GetAllAsync, etc.)

10. **Implement Service (GREEN)** → `src/EnterpriseCRM.Application/Services/ProductService.cs` - **Application Layer**
    - ProductService implementing IProductService
    - Use UnitOfWork and AutoMapper
    - Service tests now PASS

11. **Update AutoMapper** → `src/EnterpriseCRM.Application/MappingProfile.cs` - **Application Layer**
    - CreateMap<Product, ProductDto>()
    - CreateMap<CreateProductDto, Product>()
    - CreateMap<UpdateProductDto, Product>()

12. **Write Controller Tests (RED)** → `tests/EnterpriseCRM.UnitTests/Controllers/ProductsControllerTests.cs` - **Testing Layer**
    - Test ProductsController endpoints
    - Mock IProductService
    - Tests should FAIL (ProductsController doesn't exist yet)

13. **Create Controller (GREEN)** → `src/EnterpriseCRM.WebAPI/Controllers/ProductsController.cs` - **WebAPI Layer**
    - ProductsController with CRUD endpoints
    - Add [Authorize] attributes with policies
    - Controller tests now PASS

14. **Register Service** → `src/EnterpriseCRM.WebAPI/Program.cs` - **WebAPI Layer**
    - builder.Services.AddScoped<IProductService, ProductService>();

15. **Configure DbContext** → `src/EnterpriseCRM.Infrastructure/Data/ApplicationDbContext.cs` - **Infrastructure Layer**
    - Add public DbSet<Product> Products { get; set; }
    - Configure entity in OnModelCreating

16. **Create Migration** → `src/EnterpriseCRM.Infrastructure/Migrations/` - **Infrastructure Layer**
    - dotnet ef migrations add AddProductEntity
    - dotnet ef database update

**Key Principle**: Follow the dependency direction (outer → inner layers) and implement from bottom to top (Entity → DTO → Service → Controller).

### **Phase 1: Red - Write Failing Tests**
Write tests that define the desired behavior before implementing the feature.

### **Phase 2: Green - Make Tests Pass**
Implement the minimum code required to pass the tests.

### **Phase 3: Refactor - Improve Code**
Clean up and optimize while ensuring all tests remain green.

### **Phase 4: Repeat**
Continue the cycle for each new feature.

---

## 🧪 Step 1: Define Test Requirements (Before Writing Code)

### **Product Entity Requirements**
- **Name**: Required, max 200 characters
- **Description**: Optional, max 1000 characters
- **SKU**: Optional, max 100 characters, unique
- **Price**: Required, decimal
- **Cost**: Optional, decimal
- **Category**: Optional, max 100 characters
- **IsActive**: Boolean, default true

### **API Endpoints Required**
1. `GET /api/products` - List all products (paginated)
2. `GET /api/products/{id}` - Get product by ID
3. `POST /api/products` - Create new product (Manager/Admin only)
4. `PUT /api/products/{id}` - Update product (Manager/Admin only)
5. `DELETE /api/products/{id}` - Soft delete product (Admin only)
6. `GET /api/products/search` - Search products
7. `GET /api/products/category/{category}` - Get by category
8. `GET /api/products/active` - Get active products only

### **Authorization Requirements**
- **Admin**: Full access, can delete
- **Manager**: Can create, update, view
- **User**: Can only view
- **ReadOnly**: View only

---

## 🚀 Step 2: Red Phase - Write Failing Tests

### **Test 1: Product Entity Tests**

Create `tests/EnterpriseCRM.UnitTests/Entities/ProductTests.cs`:

```csharp
using EnterpriseCRM.Core.Entities;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Entities;

public class ProductTests
{
    [Fact]
    public void Product_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test Product",
            Price = 99.99m,
            IsActive = true
        };

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be("Test Product");
        product.Price.Should().Be(99.99m);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Product_ShouldAllowNullOptionalFields()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test",
            Price = 99.99m
        };

        // Assert
        product.Description.Should().BeNull();
        product.SKU.Should().BeNull();
        product.Cost.Should().BeNull();
        product.Category.Should().BeNull();
    }
}
```

**Expected Result**: ❌ Build fails - `Product` class doesn't exist yet

---

### **Test 2: ProductService GetById Tests**

Create `tests/EnterpriseCRM.UnitTests/Services/ProductServiceTests.cs`:

```csharp
using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Application.Services;
using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using Moq;
using AutoMapper;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _productService = new ProductService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public void ProductService_ShouldBeCreated()
    {
        // Arrange & Act
        var service = new ProductService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProductDto()
    {
        // Arrange
        var productId = 1;
        var product = new Product 
        { 
            Id = productId, 
            Name = "Test Product", 
            Price = 99.99m,
            IsActive = true,
            CreatedBy = "test",
            CreatedAt = DateTime.UtcNow
        };
        var productDto = new ProductDto 
        { 
            Id = productId, 
            Name = "Test Product", 
            Price = 99.99m,
            IsActive = true
        };

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map<ProductDto>(product))
            .Returns(productDto);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ProductDto>();
        result!.Name.Should().Be("Test Product");
        result.Price.Should().Be(99.99m);
        
        _productRepositoryMock.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        _mapperMock.Verify(m => m.Map<ProductDto>(product), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var productId = 999;

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().BeNull();
        _productRepositoryMock.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
    }
}
```

**Expected Result**: ❌ Build fails - `IProductService`, `ProductService`, `ProductDto`, and `IProductRepository` don't exist

---

### **Test 3: ProductsController Tests**

Create `tests/EnterpriseCRM.UnitTests/Controllers/ProductsControllerTests.cs`:

```csharp
using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _loggerMock = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_productServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void ProductsController_ShouldBeCreated()
    {
        // Assert
        _controller.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResultDto<ProductDto>
        {
            Data = new List<ProductDto> 
            { 
                new ProductDto { Id = 1, Name = "Product 1", Price = 99.99m },
                new ProductDto { Id = 2, Name = "Product 2", Price = 199.99m }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _productServiceMock.Setup(s => s.GetAllAsync(1, 10))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(1, 10);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(pagedResult);
        
        _productServiceMock.Verify(s => s.GetAllAsync(1, 10), Times.Once);
    }
}
```

**Expected Result**: ❌ Build fails - Controller doesn't exist

---

## ✅ Step 3: Green Phase - Implement Minimum Code to Pass Tests

### **Step 3.1: Create Product Entity**

**Why**: The Entity represents the database table structure and business logic. It maps to the `Products` table in SQL Server and defines what a Product is in the domain.

Add to `src/EnterpriseCRM.Core/Entities.cs`:

```csharp
/// <summary>
/// Product entity representing products in catalog
/// </summary>
public class Product : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? SKU { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public decimal? Cost { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
```

**Test Result**: ✅ `ProductTests` should now pass

**Navigation Property Explanation**: The `OrderItems` collection enables **relationship traversal** - you can access all orders containing this product via `product.OrderItems`. EF Core uses this to:
1. **Join queries automatically**: When you query a product with `.Include(p => p.OrderItems)`, EF generates a JOIN to fetch related orders
2. **Track relationships**: Changes to related entities are saved together
3. **Enable LINQ queries**: `product.OrderItems.Count()` or `product.OrderItems.TotalRevenue()` without manual joins

**Why `virtual`?** Allows EF Core to create proxy classes at runtime for change tracking and lazy loading (optional feature).

---

### **Step 3.2: Create Product DTOs**

**Why**: DTOs (Data Transfer Objects) separate the database entity from what the API exposes. They:
- Control what data is sent over the network (no internal IDs, passwords, etc.)
- Match the API contract without exposing database structure
- Allow different DTOs for different operations (Create vs Update vs Read)
- Map between layers (Controller → Service → Repository uses different representations)

Add to `src/EnterpriseCRM.Application/DTOs.cs`:

```csharp
/// <summary>
/// Product Data Transfer Object
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public decimal? Cost { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product creation DTO
/// </summary>
public class CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? SKU { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public decimal? Cost { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Product update DTO
/// </summary>
public class UpdateProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public decimal? Cost { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}
```

---

### **Step 3.3: Add Product Repository Interface**

**Why**: The interface defines the contract for data access without exposing implementation details. This enables:
- **Dependency Inversion**: Controllers depend on abstractions, not concrete classes
- **Testability**: Can mock the interface in unit tests
- **Flexibility**: Can swap implementations (SQL, NoSQL, API, etc.)
- **Clean Architecture**: Core layer doesn't depend on Infrastructure

Add to `src/EnterpriseCRM.Core/Interfaces.cs`:

```csharp
/// <summary>
/// Product repository interface
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<Product?> GetBySKUAsync(string sku);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
}
```

---

### **Step 3.4: Implement Product Repository**

**Why**: The repository implements data access logic using Entity Framework:
- **Encapsulates Database Queries**: Hides EF complexity from services
- **Reusable Queries**: GetByCategory, Search, etc. are reusable methods
- **Testable**: Can mock for unit testing
- **Single Responsibility**: Focuses solely on data access

Add to `src/EnterpriseCRM.Infrastructure/Repositories/Repositories.cs`:

```csharp
/// <summary>
/// Product repository implementation
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(p => p.Category == category && p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<Product?> GetBySKUAsync(string sku)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(p => p.Name.Contains(searchTerm) || 
                       p.SKU!.Contains(searchTerm) || 
                       p.Category!.Contains(searchTerm))
            .ToListAsync();
    }
}
```

---

### **Step 3.5: Add Product to Unit of Work**

**Why**: Unit of Work provides:
- **Single Transaction**: All repository operations share one database context
- **Consistency**: Ensures all changes succeed or fail together (ACID)
- **Centralized Save**: One `SaveChangesAsync()` commits all changes
- **Lazy Loading**: EF tracking works across multiple repository operations
- **Dependency Management**: Services depend on `IUnitOfWork`, not individual repositories

Update `src/EnterpriseCRM.Core/Interfaces.cs`:

```csharp
public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IContactRepository Contacts { get; }
    ILeadRepository Leads { get; }
    IOpportunityRepository Opportunities { get; }
    IWorkItemRepository WorkItems { get; }
    IUserRepository Users { get; }
    IProductRepository Products { get; } // ADD THIS

    Task<int> SaveChangesAsync();
}
```

Update `src/EnterpriseCRM.Infrastructure/UnitOfWork/UnitOfWork.cs`:

```csharp
// In constructor:
public UnitOfWork(ApplicationDbContext context)
{
    _context = context;
    Customers = new CustomerRepository(_context);
    Contacts = new ContactRepository(_context);
    Leads = new LeadRepository(_context);
    Opportunities = new OpportunityRepository(_context);
    WorkItems = new WorkItemRepository(_context);
    Users = new UserRepository(_context);
    Products = new ProductRepository(_context); // ADD THIS
}

// Implemented Interfaces:
public ICustomerRepository Customers { get; }
public IContactRepository Contacts { get; }
public ILeadRepository Leads { get; }
public IOpportunityRepository Opportunities { get; }
public IWorkItemRepository WorkItems { get; }
public IUserRepository Users { get; }
public IProductRepository Products { get; } // ADD THIS
```

---

### **Step 3.6: Create IProductService Interface**

**Why**: Service interfaces define business logic contracts:
- **Business Logic Layer**: Services contain business rules (validation, calculations, workflows)
- **Separation of Concerns**: Controllers handle HTTP, services handle business logic
- **Reusability**: Multiple controllers can use the same service
- **Testability**: Can mock services for controller tests
- **Transaction Boundaries**: Services coordinate multiple repository operations

Add to `src/EnterpriseCRM.Application/Interfaces.cs`:

```csharp
/// <summary>
/// Product service interface
/// </summary>
public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);
    Task<PagedResultDto<ProductDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<PagedResultDto<ProductDto>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
    Task<ProductDto> CreateAsync(CreateProductDto createDto, string currentUser);
    Task<ProductDto> UpdateAsync(UpdateProductDto updateDto, string currentUser);
    Task DeleteAsync(int id);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category);
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
    Task<ProductDto?> GetBySKUAsync(string sku);
}
```

---

### **Step 3.7: Implement ProductService**

**Why**: Service implementation contains:
- **Business Logic**: Data validation, calculations, workflows
- **Orchestration**: Coordinates multiple repositories (e.g., create product + update inventory)
- **DTO Mapping**: Transforms entities to DTOs using AutoMapper
- **Transaction Management**: Calls `SaveChangesAsync()` after operations
- **Error Handling**: Business-level exceptions and validation

Create `src/EnterpriseCRM.Application/Services/ProductService.cs`:

```csharp
using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Application.DTOs;
using AutoMapper;
using System.Linq;

namespace EnterpriseCRM.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<PagedResultDto<ProductDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var totalCount = products.Count();

        var pagedProducts = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<ProductDto>
        {
            Data = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<ProductDto>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        var products = await _unitOfWork.Products.SearchAsync(searchTerm);
        var totalCount = products.Count();

        var pagedProducts = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<ProductDto>
        {
            Data = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createDto, string currentUser)
    {
        var product = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description,
            SKU = createDto.SKU,
            Price = createDto.Price,
            Cost = createDto.Cost,
            Category = createDto.Category,
            IsActive = createDto.IsActive,
            CreatedBy = currentUser,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductDto updateDto, string currentUser)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(updateDto.Id);
        if (product == null)
            throw new ArgumentException($"Product with ID {updateDto.Id} not found.");

        product.Name = updateDto.Name;
        product.Description = updateDto.Description;
        product.SKU = updateDto.SKU;
        product.Price = updateDto.Price;
        product.Cost = updateDto.Cost;
        product.Category = updateDto.Category;
        product.IsActive = updateDto.IsActive;
        product.UpdatedBy = currentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(category);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _unitOfWork.Products.GetActiveProductsAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetBySKUAsync(string sku)
    {
        var product = await _unitOfWork.Products.GetBySKUAsync(sku);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }
}
```

---

### **Step 3.8: Update AutoMapper Profile**

**Why**: AutoMapper automates object-to-object mapping:
- **Reduces Boilerplate**: No manual property copying between Entity ↔ DTO
- **Type Safety**: Compile-time checking vs manual mapping
- **Performance**: Optimized mapping operations
- **Consistency**: Same mapping rules used everywhere
- **Maintainability**: Change mapping logic in one place (the profile)

Add to `src/EnterpriseCRM.Application/MappingProfile.cs`:

```csharp
CreateMap<Product, DTOs.ProductDto>();
CreateMap<DTOs.CreateProductDto, Product>();
CreateMap<DTOs.UpdateProductDto, Product>();
```

---

### **Step 3.9: Create ProductsController**

**Why**: Controllers are the API boundary layer:
- **HTTP Handling**: Routes HTTP requests to services
- **Authorization**: Enforces role-based access (`[Authorize(Policy = "...")]`)
- **Status Codes**: Returns appropriate HTTP responses (200, 404, 500, etc.)
- **Request/Response Mapping**: Binds JSON to DTOs, returns action results
- **Error Handling**: Catches exceptions and returns user-friendly errors

Create `src/EnterpriseCRM.WebAPI/Controllers/ProductsController.cs`:

```csharp
using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "UserOrAbove")]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _productService.GetAllAsync(pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = User.Identity?.Name ?? "System";
            var product = await _productService.CreateAsync(createDto, currentUser);
            
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto updateDto)
    {
        try
        {
            if (id != updateDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = User.Identity?.Name ?? "System";
            var product = await _productService.UpdateAsync(updateDto, currentUser);
            
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
            return StatusCode(500, "An error occurred while deleting the product");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> Search(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term is required");
            }

            var result = await _productService.SearchAsync(searchTerm, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with term {SearchTerm}", searchTerm);
            return StatusCode(500, "An error occurred while searching products");
        }
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(string category)
    {
        try
        {
            var products = await _productService.GetByCategoryAsync(category);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products by category {Category}", category);
            return StatusCode(500, "An error occurred while retrieving products by category");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetActive()
    {
        try
        {
            var products = await _productService.GetActiveProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active products");
            return StatusCode(500, "An error occurred while retrieving active products");
        }
    }

    [HttpGet("sku/{sku}")]
    public async Task<ActionResult<ProductDto>> GetBySKU(string sku)
    {
        try
        {
            var product = await _productService.GetBySKUAsync(sku);
            if (product == null)
            {
                return NotFound($"Product with SKU {sku} not found");
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with SKU {SKU}", sku);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }
}
```

---

### **Step 3.10: Register Product Service**

**Why**: Dependency Injection registration tells ASP.NET Core:
- **What to Create**: When a controller needs `IProductService`, create `ProductService`
- **Lifetime Management**: Scoped = new instance per HTTP request, shared within request
- **Dependency Resolution**: Automatically injects dependencies into constructors
- **Testability**: Can swap implementations (e.g., use a mock in tests)
- **Decoupling**: No hard dependencies, everything is injected

Add to `src/EnterpriseCRM.WebAPI/Program.cs`:

```csharp
// Add Application Services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>(); // ADD THIS
```

---

### **Step 3.11: Configure Product in DbContext**

**Why**: `ApplicationDbContext` is the bridge between C# entities and SQL tables:
- **DbSet Declaration**: Exposes Products for LINQ queries
- **EF Configuration**: Maps C# types to SQL types (decimal → DECIMAL(18,2))
- **Constraints**: Defines NOT NULL, MaxLength, indexes
- **Relationships**: Foreign keys, navigation properties
- **Migrations**: Changes here generate SQL migration files

Add to `src/EnterpriseCRM.Infrastructure/Data/ApplicationDbContext.cs`:

```csharp
public DbSet<Product> Products { get; set; }

// In OnModelCreating method, add:
modelBuilder.Entity<Product>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Description).HasMaxLength(1000);
    entity.Property(e => e.SKU).HasMaxLength(100);
    entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
    entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
    entity.Property(e => e.Category).HasMaxLength(100);
    entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
    entity.Property(e => e.UpdatedBy).HasMaxLength(100);
    
    entity.HasIndex(e => e.SKU).IsUnique();
    entity.HasIndex(e => e.Category);
    entity.HasIndex(e => e.IsActive);
});
```

**⚠️ Important**: After configuring the Product entity in DbContext, you need to create a database migration to apply these schema changes to your database:

```bash
dotnet ef migrations add AddProductEntity --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
dotnet ef database update --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

**Why is a migration necessary?** Adding `DbSet<Product>` and configuring the entity in `OnModelCreating` changes the database schema. Entity Framework Core compares your current model configuration with the existing database schema and generates SQL statements to synchronize them.

**How the migration changes the database schema:**

1. **If the Products table doesn't exist** (new database or first time adding Product):
   - Creates a new `Products` table with the following structure:
     - `Id` (INT, Primary Key, Identity)
     - `Name` (NVARCHAR(200), NOT NULL)
     - `Description` (NVARCHAR(1000), NULL)
     - `SKU` (NVARCHAR(100), NULL, Unique Index)
     - `Price` (DECIMAL(18,2), NOT NULL)
     - `Cost` (DECIMAL(18,2), NULL)
     - `Category` (NVARCHAR(100), NULL, Index)
     - `IsActive` (BIT, Index)
     - `CreatedBy` (NVARCHAR(100), NOT NULL)
     - `CreatedAt` (DATETIME2)
     - `UpdatedBy` (NVARCHAR(100), NULL)
     - `UpdatedAt` (DATETIME2)
     - `IsDeleted` (BIT, for soft delete)
   - Creates indexes on `SKU` (unique), `Category`, and `IsActive`

2. **If the Products table already exists** (updating existing schema):
   - Compares the current model configuration with the existing table structure
   - Generates ALTER TABLE statements to:
     - Add any new columns that don't exist
     - Modify column types/sizes if they differ (e.g., changing `Price` from `DECIMAL(10,2)` to `DECIMAL(18,2)`)
     - Add new indexes that are missing
     - Remove indexes that are no longer in the model (if any)
     - Update constraints (NOT NULL, MaxLength, etc.)
   - **Note**: EF Core migrations are additive by default - they won't drop columns or data unless explicitly configured

**When to run the migration:** This should be done immediately after Step 3.11, not after implementing services. Implementing `ProductService` or `ProductsController` doesn't change the database schema - only adding the entity to DbContext does.

---

## ✅ Step 4: Run Tests to Verify

### **Test Commands**

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/EnterpriseCRM.UnitTests

# Run specific test class
dotnet test --filter "FullyQualifiedName~ProductServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~ProductServiceTests.GetByIdAsync_WhenProductExists_ShouldReturnProductDto"
```

**Expected Result**: ✅ All tests should pass

---

## 🔄 Step 5: Refactor Phase - Improve Code

### **5.1: Add More Comprehensive Tests**

Create additional test cases:

```csharp
[Fact]
public async Task CreateAsync_WithValidData_ShouldReturnProductDto()
{
    // Arrange
    var createDto = new CreateProductDto
    {
        Name = "New Product",
        Price = 99.99m,
        Description = "Test description",
        SKU = "SKU-001",
        Category = "Electronics"
    };
    var product = new Product
    {
        Id = 1,
        Name = "New Product",
        Price = 99.99m,
        CreatedBy = "test",
        CreatedAt = DateTime.UtcNow
    };
    var productDto = new ProductDto { Id = 1, Name = "New Product", Price = 99.99m };

    _productRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
        .ReturnsAsync(product);
    _mapperMock.Setup(m => m.Map<ProductDto>(product)).Returns(productDto);
    _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

    // Act
    var result = await _productService.CreateAsync(createDto, "test");

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("New Product");
    _productRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

### **5.2: Add Edge Case Tests**

```csharp
[Fact]
public async Task SearchAsync_WithEmptyResults_ShouldReturnEmptyList()
{
    // Arrange
    _productRepositoryMock.Setup(repo => repo.SearchAsync("nonexistent"))
        .ReturnsAsync(new List<Product>());

    // Act
    var result = await _productService.SearchAsync("nonexistent", 1, 10);

    // Assert
    result.Data.Should().BeEmpty();
    result.TotalCount.Should().Be(0);
}
```

---

## 📝 Step 6: Documentation and Interview Talking Points

### **What You Should Say in an Interview**

#### **"How would you add a new feature using TDD?"**

**Answer:**
1. "First, I write failing tests that describe the behavior I want."
2. "These tests guide my implementation and document expected behavior."
3. "Then I write the minimum code to make tests pass."
4. "Finally, I refactor while keeping all tests green."

**Example from ProductsController:**
- Started with `ProductServiceTests.GetByIdAsync_WhenProductExists_ShouldReturnProductDto`
- This drove creating `IProductService`, `ProductService`, and `GetByIdAsync` implementation
- Tests act as executable specifications
- Always keep tests green during refactoring

#### **"What are the benefits of TDD?"**

**Benefits:**
- **Better Design**: Forces you to think about the API first
- **Confidence**: Tests provide safety net for refactoring
- **Documentation**: Tests are living examples of how code works
- **Bug Prevention**: Catch issues early before deployment
- **Regression Prevention**: Existing tests prevent breaking changes

#### **"How do you handle dependencies in tests?"**

**Answer:**
- Use **Moq** to create mock dependencies
- Mock external services, database, file system
- Test one unit at a time (the "unit" in unit testing)
- Verify interactions using `Verify()` and `Times`

**Example:**
```csharp
_productRepositoryMock.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
```

#### **"What's the Red-Green-Refactor cycle?"**

**Answer:**
- **Red**: Write a failing test (test specifies behavior)
- **Green**: Write minimal code to pass the test
- **Refactor**: Improve code quality while tests stay green
- **Repeat**: Continue cycle for each new feature

---

## 🎯 Step 7: Complete Test Suite

### **Full Test Coverage Should Include:**

1. ✅ **Unit Tests**:
   - Product entity validation
   - ProductService CRUD operations
   - ProductController endpoint behavior

2. ✅ **Integration Tests**:
   - Database operations
   - API endpoints
   - Authentication/authorization

3. ✅ **Edge Cases**:
   - Null handling
   - Invalid input
   - Not found scenarios
   - Unauthorized access

---

## ✅ Step 8: Verify in Swagger

1. Run the WebAPI
2. Navigate to `https://localhost:5001/swagger`
3. Test each endpoint:
   - GET `/api/products` - Should return paginated products
   - POST `/api/products` - Should create a new product (Admin/Manager)
   - GET `/api/products/{id}` - Should return specific product
   - PUT `/api/products/{id}` - Should update product
   - DELETE `/api/products/{id}` - Should soft delete (Admin only)

---

## 📊 Summary: TDD Interview Flow

### **Before You Start Coding:**

1. ✅ **Understand Requirements**: What should the feature do?
2. ✅ **Write Tests First**: Define expected behavior
3. ✅ **Run Tests**: Should fail (Red phase)
4. ✅ **Implement Minimal Code**: Make tests pass (Green phase)
5. ✅ **Refactor Safely**: Improve while tests stay green

### **Interview Question Examples:**

**Q: "How do you ensure test quality?"**
A: Tests should be:
- **Fast**: Run in milliseconds
- **Isolated**: No dependencies on external systems
- **Repeatable**: Same results every time
- **Self-Validating**: Clear pass/fail
- **Timely**: Written before implementation

**Q: "What happens if tests fail during refactoring?"**
A: I stop immediately, revert changes, and fix the issue. TDD ensures I can refactor with confidence because tests catch regressions immediately.

**Q: "How do you test database operations?"**
A: Use mocking (Moq) to avoid real database in unit tests. For integration tests, use a test database with transactions that roll back after each test.

---

## 🎓 Key TDD Principles Demonstrated

1. ✅ **Test First**: Tests written before implementation
2. ✅ **Red-Green-Refactor**: Follow the cycle strictly
3. ✅ **Fast Feedback**: Tests run quickly and frequently
4. ✅ **Confidence**: Refactor knowing tests will catch issues
5. ✅ **Documentation**: Tests serve as examples

---

---

## 🏗️ Architecture Overview: How Everything Connects

### **Request Flow (Example: GET /api/products/1)**

```
1. HTTP Request → ProductsController.GetById(1)
   ├─ Why: Controller is the API entry point
   └─ Does: Validates authentication, returns HTTP responses

2. Controller calls ProductService.GetByIdAsync(1)
   ├─ Why: Service handles business logic
   └─ Does: Validation, orchestration, mapping

3. Service calls UnitOfWork.Products.GetByIdAsync(1)
   ├─ Why: Unit of Work manages transaction
   └─ Does: Provides ProductRepository instance

4. Repository calls DbContext.Products.FindAsync(1)
   ├─ Why: Repository abstracts database access
   └─ Does: EF Core query to SQL Server

5. SQL Server returns Product entity
   ↓
6. Repository returns Product to Service
   ↓
7. Service maps Product → ProductDto (via AutoMapper)
   ├─ Why: DTOs separate internal data from API contract
   └─ Does: Transforms database entity to API response

8. Controller returns ProductDto as JSON
   ├─ Why: API consumers should not see database structure
   └─ Result: Clean, focused API response
```

### **Why Each Layer Exists**

| Layer | Responsibility | Why It Exists |
|-------|---------------|----------------|
| **Controller** | HTTP routing, status codes, auth | API boundary and HTTP concerns |
| **Service** | Business logic, validation | Coordinates multiple repositories, business rules |
| **Unit of Work** | Transaction management | Ensures all operations succeed or fail together |
| **Repository** | Data access patterns | Encapsulates EF Core queries, testable |
| **DbContext** | Database connection | EF Core's interface to SQL Server |
| **Entity** | Database table structure | Represents database row |
| **DTO** | API contract | What API exposes vs internal structure |

### **Dependency Flow (Who Depends on Whom)**

```
Controller → Service → Unit of Work → Repository → DbContext → Database
    ↓          ↓           ↓             ↓
  Return    Business    Transactions   Queries
  HTTP      Logic                      
```

**Key Principle**: Layers depend inward:
- Controller depends on Service interface (abstraction)
- Service depends on Unit of Work interface (abstraction)
- Repository implements interface, depends on DbContext
- **Core** (Entities, Interfaces) has ZERO dependencies
- **Application** depends only on Core
- **Infrastructure** depends on Core and Application
- **WebAPI** depends on all layers

This is Clean Architecture's Dependency Rule!

---

## 📚 Next Steps

Once ProductsController is complete and all tests pass:

1. ✅ Create **Integration Tests** for Products API
2. ✅ Add **E2E Tests** for complete workflows
3. ✅ Implement **Products UI** in Blazor Server
4. ✅ Add **Product import/export** features
5. ✅ Implement **Product inventory management**

This TDD approach ensures high-quality, maintainable code that's well-tested and interview-ready! 🚀

---

## 🏛️ Clean Architecture Layers Summary

Your CRM project follows **Clean Architecture** with 4 distinct layers:

### **Layer 1: Core (Domain Layer)**
**Location**: `src/EnterpriseCRM.Core/`

**Contains**:
- `Entities.cs` - Domain entities (Customer, Product, User, etc.)
- `Interfaces.cs` - Interfaces for repositories and services

**Why**: Pure business objects with ZERO dependencies on external frameworks
- No Entity Framework, no ASP.NET Core, no SQL references
- Just C# classes representing your business domain
- Can be tested without any external dependencies

### **Layer 2: Application (Business Logic Layer)**
**Location**: `src/EnterpriseCRM.Application/`

**Contains**:
- `Interfaces.cs` - Service interfaces (IProductService, ICustomerService)
- `Services/*` - Service implementations (ProductService, CustomerService)
- `DTOs.cs` - Data Transfer Objects (ProductDto, CustomerDto)
- `MappingProfile.cs` - AutoMapper configurations

**Why**: Contains business logic, use cases, and application rules
- Depends ONLY on Core layer
- No database, no HTTP concerns
- Pure C# business logic that can run anywhere

### **Layer 3: Infrastructure (Data Layer)**
**Location**: `src/EnterpriseCRM.Infrastructure/`

**Contains**:
- `Data/ApplicationDbContext.cs` - Entity Framework DbContext
- `Repositories/Repositories.cs` - Repository implementations
- `UnitOfWork/UnitOfWork.cs` - Transaction management
- `Migrations/` - Database schema changes

**Why**: Implements data access and external services
- Depends on Core (interfaces) and Application (DTOs)
- Contains Entity Framework, SQL Server, file system code
- All "infrastructure concerns" live here

### **Layer 4: WebAPI (Presentation Layer)**
**Location**: `src/EnterpriseCRM.WebAPI/`

**Contains**:
- `Controllers/` - API endpoints (CustomersController, ProductsController)
- `Program.cs` - Dependency injection configuration
- `appsettings.json` - Configuration files

**Why**: HTTP API interface for external consumers
- Depends on ALL other layers
- Contains ASP.NET Core, routing, authentication
- Translates HTTP requests into service calls

---

### **Layer Interaction Diagram**

```
┌─────────────────────────────────────────────────────────┐
│                    WebAPI Layer                         │
│  Controllers → Services → Status Codes                  │
│  Depends on: Application, Infrastructure, Core          │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              Application Layer                          │
│  Services → DTOs → Business Logic                       │
│  Depends on: Core ONLY                                  │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              Infrastructure Layer (Data)                │
│  Repositories → DbContext → EF Core                     │
│  Depends on: Core (interfaces)                          │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              Core Layer (Domain/Business Domain)        │
│  Entities → Interfaces                                  │
│  NO dependencies (pure C# classes)                      │
└─────────────────────────────────────────────────────────┘
```

---

### **Key Dependency Rules**

1. **Dependency Direction**: Always inward
   - WebAPI → Application → Infrastructure → Core
   - NEVER: Core depends on Infrastructure

2. **Interface-Based**: Core defines interfaces, Infrastructure implements them
   - `IProductRepository` is in Core
   - `ProductRepository` is in Infrastructure
   - Application uses `IProductRepository`, doesn't know implementation

3. **Testability**: Inner layers can be tested independently
   - Test Core without database
   - Test Application without HTTP
   - Test Infrastructure with in-memory database

4. **Flexibility**: Can swap implementations
   - Replace SQL Server with MongoDB? Change Infrastructure layer
   - Replace WebAPI with Blazor? Change WebAPI layer
   - Core and Application remain unchanged

---

### **Example: Product Flow Through Layers**

```
User sends: POST /api/products {"name": "Widget", "price": 99.99}

1. WebAPI Layer (ProductsController)
   ├─ Receives HTTP request
   ├─ Validates authentication
   └─ Calls → ProductService.CreateAsync()

2. Application Layer (ProductService)
   ├─ Validates business rules
   ├─ Maps CreateProductDto → Product entity
   └─ Calls → UnitOfWork.Products.AddAsync()

3. Infrastructure Layer (ProductRepository)
   ├─ Implements data access
   ├─ Uses DbContext to save to SQL Server
   └─ Returns saved Product entity

4. Response flows back up the layers:
   Infrastructure → Application (maps to DTO) → WebAPI (returns JSON)
```

**Result**: Each layer has a single responsibility and can be modified independently! 🎯


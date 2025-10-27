# Client Services Architecture

**Last Updated:** 2024-12-19  
**Project:** EnterpriseCRM Clean Architecture  
**Purpose:** Understanding the client service pattern in Blazor server applications

---

## 📋 Table of Contents

- [Overview](#overview)
- [What Are Client Services?](#what-are-client-services)
- [Architecture Overview](#architecture-overview)
- [IProductClientService Interface](#iproductclientservice-interface)
- [ProductClientService Implementation](#productclientservice-implementation)
- [Registration and DI](#registration-and-di)
- [Benefits](#benefits)
- [Testing with Client Services](#testing-with-client-services)

---

## 🎯 Overview

**Client Services** are abstraction layers that handle communication between Blazor components and REST APIs. They provide a clean interface for making HTTP requests without exposing HTTP complexity to components.

### Purpose

Client services:
- 🎯 **Encapsulate** HTTP communication logic
- 🔒 **Isolate** components from API details
- 🧪 **Enable** easy mocking in tests
- 🔄 **Handle** serialization/deserialization
- ❌ **Manage** error handling

---

## 🤔 What Are Client Services?

A **Client Service** is a service that:
1. **Implements** a business interface (e.g., `IProductClientService`)
2. **Uses** `HttpClient` to call REST APIs
3. **Converts** between DTOs and HTTP requests/responses
4. **Injected** into Blazor components

### Quick Example

```csharp
// Interface defines the contract
public interface IProductClientService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
}

// Implementation uses HttpClient
public class ProductClientService : IProductClientService
{
    private readonly HttpClient _httpClient;
    
    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("/api/products");
        return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
    }
}

// Component uses the interface
@inject IProductClientService ProductService

@code {
    private async Task LoadProducts()
    {
        var products = await ProductService.GetAllAsync();
    }
}
```

---

## 🏗️ Architecture Overview

### The Problem

Without client services, components would directly use `HttpClient`:

```razor
@inject HttpClient HttpClient

@code {
    private async Task LoadProducts()
    {
        // ❌ Component directly handles HTTP
        var response = await HttpClient.GetAsync("https://localhost:5001/api/products");
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        
        // ❌ Error handling scattered everywhere
        if (!response.IsSuccessStatusCode)
        {
            // What to do?
        }
    }
}
```

**Problems:**
- ❌ Hard to test (can't mock HttpClient easily)
- ❌ Duplicated code across components
- ❌ Tightly coupled to HTTP details
- ❌ Hard to maintain (URLs, error handling everywhere)

### The Solution

Client services provide a clean abstraction:

```razor
@inject IProductClientService ProductService

@code {
    private async Task LoadProducts()
    {
        // ✅ Clean, testable, maintainable
        var products = await ProductService.GetAllAsync();
    }
}
```

**Benefits:**
- ✅ Easy to mock in tests
- ✅ Reusable across components
- ✅ Components don't know about HTTP
- ✅ Centralized error handling

---

## 📝 IProductClientService Interface

```csharp
public interface IProductClientService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto product);
    Task<ProductDto> UpdateAsync(UpdateProductDto product);
    Task DeleteAsync(int id);
}
```

### Why Use an Interface?

Interfaces provide:
- 🧪 **Testability** - Easy to mock
- 🔄 **Flexibility** - Can swap implementations
- 🎯 **Clarity** - Clear contract for what the service does
- 🔌 **Loose Coupling** - Components depend on abstraction

### Interface Design Principles

```csharp
// ✅ Good: Business-focused methods
Task<IEnumerable<ProductDto>> GetAllAsync();

// ❌ Bad: HTTP-focused methods
Task<HttpResponseMessage> GetProductsAsync();

// ✅ Good: Async/await pattern
Task<ProductDto> GetByIdAsync(int id);

// ❌ Bad: Synchronous blocking calls
ProductDto GetById(int id);
```

### Method Patterns

#### GET Operations

```csharp
// Return multiple items
Task<IEnumerable<ProductDto>> GetAllAsync();

// Return single item (nullable if not found)
Task<ProductDto?> GetByIdAsync(int id);

// Return multiple with filtering
Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category);
```

#### POST Operations

```csharp
// Create new entity
Task<ProductDto> CreateAsync(CreateProductDto product);
```

#### PUT Operations

```csharp
// Update existing entity
Task<ProductDto> UpdateAsync(UpdateProductDto product);
```

#### DELETE Operations

```csharp
// Delete entity
Task DeleteAsync(int id);
```

---

## 🔧 ProductClientService Implementation

```csharp
public class ProductClientService : IProductClientService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:5001/api/products";

    public ProductClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync(_baseUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>() 
            ?? Enumerable.Empty<ProductDto>();
    }
}
```

### Understanding the Implementation

#### Constructor Injection

```csharp
public ProductClientService(HttpClient httpClient)
{
    _httpClient = httpClient; // Injected by DI
}
```

- `HttpClient` is **injected**, not created
- Allows centralized configuration
- Facilitates testing (can use test HttpClient)

#### Base URL

```csharp
private readonly string _baseUrl = "https://localhost:5001/api/products";
```

- ⚠️ **Hardcoded** - should ideally come from configuration
- Centralized endpoint management
- Easy to change for different environments

#### Method Implementation

```csharp
public async Task<IEnumerable<ProductDto>> GetAllAsync()
{
    // 1. Make HTTP GET request
    var response = await _httpClient.GetAsync(_baseUrl);
    
    // 2. Check for success
    response.EnsureSuccessStatusCode();
    
    // 3. Deserialize JSON response
    return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>() 
        ?? Enumerable.Empty<ProductDto>();
}
```

### Complete Implementation Example

```csharp
public class ProductClientService : IProductClientService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ProductClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = _httpClient.BaseAddress + "/api/products";
    }

    // GET all products
    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync(_baseUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>() 
            ?? Enumerable.Empty<ProductDto>();
    }

    // GET single product
    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<ProductDto>();
    }

    // POST create product
    public async Task<ProductDto> CreateAsync(CreateProductDto product)
    {
        var response = await _httpClient.PostAsJsonAsync(_baseUrl, product);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>() 
            ?? throw new Exception("Failed to create product");
    }

    // PUT update product
    public async Task<ProductDto> UpdateAsync(UpdateProductDto product)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{product.Id}", product);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>() 
            ?? throw new Exception("Failed to update product");
    }

    // DELETE product
    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
        response.EnsureSuccessStatusCode();
    }
}
```

---

## 🔌 Registration and DI

### In Program.cs

```csharp
builder.Services.AddHttpClient<IProductClientService, ProductClientService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5001");
});
```

### Understanding AddHttpClient

```csharp
builder.Services.AddHttpClient<IProductClientService, ProductClientService>(client =>
    //            ↑                       ↑
    //      Interface to register    Implementation class
    {
        client.BaseAddress = new Uri("https://localhost:5001");
        //  ↑
        //  Configures the HttpClient
    });
```

#### What AddHttpClient Does

1. **Registers** `IProductClientService` in DI container
2. **Creates** an `HttpClient` instance for the service
3. **Configures** the `HttpClient` (BaseAddress, headers, etc.)
4. **Manages** HttpClient lifetime properly

#### Why Use AddHttpClient?

```csharp
// ❌ Bad: Direct registration
builder.Services.AddScoped<IProductClientService, ProductClientService>();

// This fails because IProductClientService needs HttpClient
// but HttpClient isn't registered
```

```csharp
// ✅ Good: AddHttpClient manages HttpClient for you
builder.Services.AddHttpClient<IProductClientService, ProductClientService>();

// AddHttpClient:
// 1. Creates HttpClient
// 2. Injects it into ProductClientService
// 3. Manages lifecycle correctly
// 4. Handles disposal
```

---

## ✅ Benefits

### 1. Separation of Concerns

```
┌─────────────────────────┐
│  Component (UI Logic)   │
└──────────┬──────────────┘
           │ uses
           ↓
┌─────────────────────────┐
│  Client Service         │
│  (Business Logic)       │
└──────────┬──────────────┘
           │ calls
           ↓
┌─────────────────────────┐
│  HttpClient (HTTP)       │
└─────────────────────────┘
```

### 2. Testability

```csharp
// Production: Real client service
builder.Services.AddHttpClient<IProductClientService, ProductClientService>();

// Testing: Mock client service
var mock = new Mock<IProductClientService>();
mock.Setup(s => s.GetAllAsync()).ReturnsAsync(products);
Services.AddSingleton(mock.Object);
```

### 3. Reusability

```razor
<!-- ProductsList.razor -->
@inject IProductClientService ProductService

<!-- ProductDetails.razor -->
@inject IProductClientService ProductService

<!-- Both components can use the same service -->
```

### 4. Centralized Error Handling

```csharp
// Future enhancement: Add try-catch in service
public async Task<IEnumerable<ProductDto>> GetAllAsync()
{
    try
    {
        var response = await _httpClient.GetAsync(_baseUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
    }
    catch (Exception ex)
    {
        // Centralized error handling
        logger.LogError(ex, "Failed to get products");
        throw;
    }
}
```

### 5. Ease of Maintenance

- ✅ Change API endpoint in one place
- ✅ Add authentication in one place
- ✅ Modify error handling in one place

---

## 🧪 Testing with Client Services

### The Connection

```csharp
// Tests depend on IProductClientService (the interface)
public class ProductsListTests : TestContext
{
    private readonly Mock<IProductClientService> _productServiceMock;
    //                          ↑
    //                    Interface, not implementation!
```

### Why This Works

```csharp
// Component uses interface
@inject IProductClientService ProductService

// Tests provide mock of interface
Services.AddSingleton<IProductClientService>(_productServiceMock.Object);

// When component is rendered:
// 1. Component requests IProductClientService
// 2. DI provides mock.Object
// 3. Component uses mock, not real service
```

### Test Example

```csharp
[Fact]
public void ProductsList_ShouldRenderProducts_WhenProductsExist()
{
    // Arrange: Setup what the service should return
    var products = new List<ProductDto> 
    { 
        new ProductDto { Name = "Widget" } 
    };
    
    _productServiceMock.Setup(s => s.GetAllAsync())
        .ReturnsAsync(products);

    // Act: Render component
    var cut = RenderComponent<ProductsList>();
    
    // Component internally:
    // 1. Injects IProductClientService
    // 2. Gets _productServiceMock.Object
    // 3. Calls GetAllAsync()
    // 4. Mock returns our test products
    // 5. Component renders products

    // Assert
    cut.Markup.Should().Contain("Widget");
}
```

### Complete Flow

```
┌──────────────────────────────────────────────┐
│ Test Setup                                   │
│ var mock = new Mock<IProductClientService>(); │
│ Services.AddSingleton(mock.Object);          │
└──────────────────┬───────────────────────────┘
                   │
                   │ RenderComponent
                   ↓
┌──────────────────────────────────────────────┐
│ Blazor Component                              │
│ @inject IProductClientService ProductService │
└──────────────────┬───────────────────────────┘
                   │ Requests IProductClientService
                   ↓
┌──────────────────────────────────────────────┐
│ DI Container                                  │
│ Provides: mock.Object                         │
└──────────────────┬───────────────────────────┘
                   │ Returns mock
                   ↓
┌──────────────────────────────────────────────┐
│ Component calls                               │
│ ProductService.GetAllAsync()                  │
└──────────────────┬───────────────────────────┘
                   │ Goes to mock
                   ↓
┌──────────────────────────────────────────────┐
│ Mock returns configured test data             │
└──────────────────────────────────────────────┘
```

---

## 📚 Additional Resources

- **Clean Architecture**: See `CLEAN_ARCHITECTURE.md`
- **HTTP Client Best Practices**: See `repository-pattern.md`
- **Testing Guidelines**: See `BUNIT_COMPONENT_TESTING.md`

---

## 🔗 Related Documentation

- [BUnit Component Testing](../testing/BUNIT_COMPONENT_TESTING.md)
- [Dependency Injection in Tests](../testing/DEPENDENCY_INJECTION_TESTS.md)
- [Blazor UI Testing](../features/Products/PRODUCTS_BLAZOR_UI_TDD.md)


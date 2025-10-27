# Dependency Injection in Component Tests

**Last Updated:** 2024-12-19  
**Project:** EnterpriseCRM Testing  
**Purpose:** Understanding how to mock services in BUnit component tests

---

## 📋 Table of Contents

- [Overview](#overview)
- [Services Container](#services-container)
- [AddSingleton Method](#addsingleton-method)
- [Moq Framework](#moq-framework)
- [Example Walkthrough](#example-walkthrough)
- [Common Patterns](#common-patterns)
- [Best Practices](#best-practices)

---

## 🎯 Overview

In real applications, Blazor components depend on services (like HTTP clients, repositories, etc.). In tests, we replace these real services with **mocks** to:

- ✅ **Isolate** components from external systems
- ✅ **Control** what services return
- ✅ **Speed up** tests (no real HTTP calls)
- ✅ **Make tests** reliable and repeatable

---

## 🗂️ Services Container

```csharp
public class ProductsListTests : TestContext
{
    // Services is inherited from TestContext
    Services.AddSingleton<IProductClientService>(_mock.Object);
}
```

### What Is Services?

The `Services` property is a **dependency injection container** - a registry of services that components can use.

```csharp
// Services is of type IServiceCollection
Services.AddSingleton<IProductClientService>(_mock.Object);
//     ↑                               ↑
//   Container              Service being registered
```

#### How It Works

1. **Register** a service in the container
2. **Component** requests a dependency
3. **Container** provides the registered instance

#### Services from TestContext

When you inherit from `TestContext`, you get access to the `Services` property:

```csharp
public class MyTests : TestContext
{
    public void Test()
    {
        // Services is automatically available
        Services.AddSingleton<IMyService>(myMock.Object);
    }
}
```

#### Why We Need Services

Our `ProductsList` component injects `IProductClientService`:

```razor
@inject IProductClientService ProductService
```

When rendering in tests, we need to **provide** this service, otherwise:
- Blazor throws a `NullReferenceException`
- Component can't initialize properly
- Test fails

---

## ➕ AddSingleton Method

```csharp
Services.AddSingleton<IProductClientService>(_productServiceMock.Object);
```

### Understanding AddSingleton

`AddSingleton` registers a service in the DI container with a **singleton lifetime**.

#### Lifetime Management

**Singleton** means the same instance is used everywhere:

```csharp
// Register as singleton
Services.AddSingleton<IProductClientService>(_mock.Object);

// When component requests IProductClientService
// They all get the SAME instance
```

#### Other Lifetime Options

```csharp
// Singleton: Same instance everywhere
Services.AddSingleton<IProductClientService>(_mock.Object);

// Transient: New instance every time
Services.AddTransient<IProductClientService>(provider => 
    new Mock<IProductClientService>().Object);

// Scoped: One instance per test (in BUnit, same as Singleton)
Services.AddScoped<IProductClientService>(_mock.Object);
```

### Method Signature

```csharp
public static IServiceCollection AddSingleton<TService>(
    this IServiceCollection services, 
    TService implementationInstance
)
```

- **services** - The container to add to
- **implementationInstance** - The actual service instance
- **TService** - The service interface/type

### What AddSingleton Does

```csharp
Services.AddSingleton<IProductClientService>(_mock.Object);

// Internally, this:
// 1. Adds IProductClientService to the container
// 2. Maps it to the _mock.Object instance
// 3. Stores it with singleton lifetime
```

---

## 🎭 Moq Framework

```csharp
private readonly Mock<IProductClientService> _productServiceMock;

public ProductsListTests()
{
    _productServiceMock = new Mock<IProductClientService>();
    Services.AddSingleton<IProductClientService>(_productServiceMock.Object);
}
```

### What Is Moq?

**Moq** (pronounced "Mock") is a mocking library for .NET that creates fake objects for testing.

#### Why Use Moq?

Real services have problems:
- ❌ Make real HTTP calls
- ❌ Require databases
- ❌ Slow tests
- ❌ Unpredictable responses
- ❌ External dependencies

Mocks solve this:
- ✅ No real calls
- ✅ Instant responses
- ✅ Full control
- ✅ Consistent behavior
- ✅ No external dependencies

### How Moq Works

```csharp
// 1. Create a mock
var mock = new Mock<IProductClientService>();

// 2. Setup behavior
mock.Setup(s => s.GetAllAsync())
    .ReturnsAsync(new List<ProductDto>());

// 3. Access the actual object
var service = mock.Object; // This is what gets injected

// 4. Verify calls
mock.Verify(s => s.GetAllAsync(), Times.Once);
```

#### Mocking in Detail

```csharp
// STEP 1: Create the mock
Mock<IProductClientService> _productServiceMock = new();

// STEP 2: Setup methods to return values
_productServiceMock.Setup(s => s.GetAllAsync())
    .ReturnsAsync(new List<ProductDto>());

// STEP 3: Get the actual object (implements IProductClientService)
var serviceInstance = _productServiceMock.Object;

// STEP 4: Register in DI container
Services.AddSingleton<IProductClientService>(serviceInstance);

// STEP 5: Component gets this when it calls @inject
```

---

## 🎓 Example Walkthrough

Let's trace through the complete dependency injection flow:

### Test Setup

```csharp
public class ProductsListTests : TestContext
{
    private readonly Mock<IProductClientService> _productServiceMock;

    public ProductsListTests()
    {
        // 1. Create a mock object
        _productServiceMock = new Mock<IProductClientService>();
        
        // 2. Register in DI container
        Services.AddSingleton<IProductClientService>(_productServiceMock.Object);
    }
}
```

### What Happens Here?

```
┌─────────────────────────────────────┐
│ 1. new Mock<IProductClientService>()│
│    Creates a mock object            │
└─────────────┬───────────────────────┘
              │
              │ .Object property
              ↓
┌─────────────────────────────────────┐
│ 2. _productServiceMock.Object       │
│    Implements IProductClientService │
│    All methods return default        │
└─────────────┬───────────────────────┘
              │
              │ Services.AddSingleton
              ↓
┌─────────────────────────────────────┐
│ 3. Services Container               │
│    Stores:                          │
│    IProductClientService → mock     │
└─────────────────────────────────────┘
```

### In the Test

```csharp
[Fact]
public void ProductsList_ShouldRenderProducts_WhenProductsExist()
{
    // Setup the mock to return test data
    _productServiceMock.Setup(s => s.GetAllAsync())
        .ReturnsAsync(new List<ProductDto>
        {
            new ProductDto { Name = "Widget", Price = 99.99m }
        });

    // Render component
    var cut = RenderComponent<ProductsList>();
    
    // Component internally:
    // 1. Requests IProductClientService from DI
    // 2. Gets _productServiceMock.Object
    // 3. Calls GetAllAsync() on mock
    // 4. Returns our test data
}
```

### Complete Flow

```
Component: @inject IProductClientService ProductService
              ↓
         OnInitializedAsync()
              ↓
      ProductService.GetAllAsync()
              ↓
    _productServiceMock.Object.GetAllAsync()
              ↓
       Setup returns data
              ↓
      Component receives data
              ↓
      Component renders HTML
              ↓
      Test verifies output
```

---

## 🎨 Common Patterns

### 1. Basic Mock Setup

```csharp
public class MyTests : TestContext
{
    private readonly Mock<IMyService> _serviceMock;

    public MyTests()
    {
        _serviceMock = new Mock<IMyService>();
        Services.AddSingleton<IMyService>(_serviceMock.Object);
    }

    [Fact]
    public void Test()
    {
        _serviceMock.Setup(s => s.DoSomething())
            .Returns("result");

        var cut = RenderComponent<MyComponent>();

        cut.Markup.Should().Contain("result");
    }
}
```

### 2. Setup in Constructor vs Individual Tests

```csharp
public class ProductsListTests : TestContext
{
    private readonly Mock<IProductClientService> _serviceMock;

    // Setup infrastructure in constructor
    public ProductsListTests()
    {
        _serviceMock = new Mock<IProductClientService>();
        Services.AddSingleton<IProductClientService>(_serviceMock.Object);
        // No .Setup() here - do that in individual tests
    }

    [Fact]
    public void Test1()
    {
        // Each test configures its own behavior
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<ProductDto>());

        var cut = RenderComponent<ProductsList>();
        // ...
    }

    [Fact]
    public void Test2()
    {
        // Different configuration for different test
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<ProductDto> { new ProductDto { Name = "Item" } });

        var cut = RenderComponent<ProductsList>();
        // ...
    }
}
```

### 3. Mocking Async Methods

```csharp
// Async methods return Task or Task<T>
_serviceMock.Setup(s => s.GetAllAsync())
    .ReturnsAsync(new List<ProductDto>()); // Returns Task<List<ProductDto>>

// For methods that just return Task (no value)
_serviceMock.Setup(s => s.DeleteAsync(1))
    .Returns(Task.CompletedTask);
```

### 4. Verifying Mock Calls

```csharp
[Fact]
public void ShouldCallService_WhenButtonClicked()
{
    _serviceMock.Setup(s => s.DeleteAsync(1))
        .Returns(Task.CompletedTask);

    var cut = RenderComponent<MyComponent>();
    
    cut.Find("button").Click();

    // Verify the method was called
    _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
}
```

### 5. Mocking with Parameters

```csharp
// Setup with any parameter
_serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync(new ProductDto { Name = "Product" });

// Setup with specific parameter
_serviceMock.Setup(s => s.GetByIdAsync(1))
    .ReturnsAsync(new ProductDto { Id = 1, Name = "Specific" });

// Setup with parameter matching
_serviceMock.Setup(s => s.GetByIdAsync(It.Is<int>(id => id > 0)))
    .ReturnsAsync((int id) => new ProductDto { Id = id });
```

### 6. Clearing Mock Between Tests

```csharp
public class MyTests : TestContext
{
    private readonly Mock<IService> _serviceMock;

    public MyTests()
    {
        _serviceMock = new Mock<IService>();
        Services.AddSingleton<IService>(_serviceMock.Object);
    }

    [Fact]
    public void Test1()
    {
        _serviceMock.Setup(s => s.Method()).Returns("Result1");
        // Test code
    }

    [Fact]
    public void Test2()
    {
        _serviceMock.Reset(); // Clear previous setups
        _serviceMock.Setup(s => s.Method()).Returns("Result2");
        // Test code
    }
}
```

---

## ✅ Best Practices

### 1. **Initialize Mock in Constructor**

```csharp
// ✅ Good: One place to setup
public ProductsListTests()
{
    _serviceMock = new Mock<IProductClientService>();
    Services.AddSingleton<IProductClientService>(_serviceMock.Object);
}

// ❌ Bad: Duplicated setup
[Fact]
public void Test1()
{
    var mock = new Mock<IService>();
    Services.AddSingleton<IService>(mock.Object);
    // ...
}
```

### 2. **Configure Setup in Each Test**

```csharp
// ✅ Good: Each test controls its own behavior
[Fact]
public void EmptyTest()
{
    _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ProductDto>());
    // ...
}

[Fact]
public void WithProductsTest()
{
    _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(products);
    // ...
}

// ❌ Bad: Shared state between tests
private List<ProductDto> _globalProducts;

public MyTests()
{
    _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(() => _globalProducts);
}
```

### 3. **Use Mock.Verify for Behavior Testing**

```csharp
// ✅ Good: Verify behavior
_serviceMock.Verify(s => s.SaveAsync(product), Times.Once);

// ❌ Bad: Check internal state
var instance = cut.Instance;
Assert.Equal(1, instance.saveCallCount); // Don't expose internal state
```

### 4. **Don't Over-Mock**

```csharp
// ✅ Good: Mock external dependencies
var httpMock = new Mock<HttpClient>();
var dbMock = new Mock<DbConnection>();

// ❌ Bad: Mock simple data structures
var listMock = new Mock<List<ProductDto>>(); // Just use List!
```

### 5. **Give Mocks Descriptive Names**

```csharp
// ✅ Good: Clear purpose
private readonly Mock<IProductClientService> _productServiceMock;

// ❌ Bad: Unclear
private readonly Mock<IProductClientService> _mock;
private readonly Mock<IProductClientService> m;
```

---

## 🔗 Related Concepts

### Services vs Mock.Object

```csharp
// The Mock itself (configuration tool)
var mock = new Mock<IProductClientService>();

// The actual service instance (what components use)
var service = mock.Object;

// Register the service instance, not the mock
Services.AddSingleton<IProductClientService>(service);
//                                      NOT mock itself
```

### Real Services vs Mock Services

**Real Service** (production):
```csharp
// Program.cs
builder.Services.AddHttpClient<IProductClientService, ProductClientService>();
```

**Mock Service** (testing):
```csharp
// Test
var mock = new Mock<IProductClientService>();
mock.Setup(s => s.GetAllAsync()).ReturnsAsync(products);
Services.AddSingleton<IProductClientService>(mock.Object);
```

### Dependency Injection Chain

```
Test Constructor
      ↓
  Setup Mock
      ↓
  AddSingleton()
      ↓
  Services Container
      ↓
  RenderComponent()
      ↓
  Component @inject
      ↓
  Mock.Object
      ↓
  Component uses service
```

---

## 📚 Additional Resources

- **Moq Documentation**: https://github.com/moq/moq4
- **BUnit Dependency Injection**: https://bunit.dev/docs/providing-input/inject-services
- **SOLID Principles**: See `clean-architecture.md`

---

## 🔗 Related Documentation

- [BUnit Component Testing](./BUNIT_COMPONENT_TESTING.md)
- [Client Services Architecture](../architecture/CLIENT_SERVICES.md)
- [Blazor Testing Guide](../features/Products/PRODUCTS_BLAZOR_UI_TDD.md)


# BUnit Component Testing

**Last Updated:** 2024-12-19  
**Project:** EnterpriseCRM Blazor Testing  
**Purpose:** Understanding BUnit testing framework for Blazor components

---

## 📋 Table of Contents

- [Overview](#overview)
- [Key Concepts](#key-concepts)
  - [TestContext](#testcontext)
  - [RenderComponent&lt;T&gt;()](#rendercomponentt)
  - [CUT (Component Under Test)](#cut-component-under-test)
  - [Markup Property](#markup-property)
- [Example Walkthrough](#example-walkthrough)
- [Common Patterns](#common-patterns)
- [Best Practices](#best-practices)

---

## 🎯 Overview

**BUnit** is a unit testing library specifically designed for testing Blazor components. It allows you to:

- Render Blazor components in isolation
- Test component rendering and behavior
- Verify UI output without running a browser
- Mock dependencies and services
- Test user interactions (clicks, inputs, etc.)

### Why BUnit?

Traditional UI testing requires a browser, which is:
- ⏱️ **Slow** - Full browser startup time
- 💰 **Expensive** - Requires more resources
- 🐛 **Brittle** - Tests break with browser/OS updates
- 🎭 **Isolated** - Can't easily control dependencies

BUnit solves these by:
- ⚡ **Fast** - Runs in memory, no browser needed
- 💾 **Lightweight** - Minimal resource usage
- 🎯 **Controlled** - Full control over test environment
- 🔧 **Isolated** - Easy to mock dependencies

---

## 🔑 Key Concepts

### TestContext

```csharp
public class ProductsListTests : TestContext
{
    // TestContext provides the testing environment
    // - Services: Dependency injection container
    // - RenderComponent<T>() method
    // - Test helper methods
}
```

**TestContext** is the base class for all BUnit component tests. It provides:

- **Services** - Dependency injection container for mocking
- **RenderComponent<T>()** - Method to render components
- **Test infrastructure** - Support for Blazor lifecycle events

#### Inheriting from TestContext

When you inherit from `TestContext`, you get access to:
- Component rendering methods
- Service registration methods
- Navigation testing utilities
- User interaction simulators

---

### RenderComponent&lt;T&gt;()

```csharp
var cut = RenderComponent<ProductsList>();
```

This method renders a Blazor component and returns it wrapped in a testable component instance.

#### How It Works

1. **Instantiates** the component class
2. **Runs lifecycle** methods (`OnInitializedAsync`, etc.)
3. **Renders** the HTML output
4. **Returns** a `IRenderedComponent<T>` for testing

#### Parameters

```csharp
// Basic render
var cut = RenderComponent<ProductsList>();

// With parameters
var cut = RenderComponent<ProductsList>(parameters => parameters
    .Add(p => p.ProductId, 42)
    .Add(p => p.ShowDetails, true)
);

// With event callbacks
var cut = RenderComponent<ProductsList>(parameters => parameters
    .Add(p => p.OnDelete, EventCallback.Empty)
);
```

#### What Happens During Rendering?

```csharp
var cut = RenderComponent<ProductsList>();

// BUnit internally does:
// 1. new ProductsList() - Create component instance
// 2. Inject services from Services container
// 3. Call OnInitializedAsync() if implemented
// 4. Render Razor markup to HTML
// 5. Return IRenderedComponent<T> wrapper
```

---

### CUT (Component Under Test)

```csharp
var cut = RenderComponent<ProductsList>();
```

**CUT** stands for **Component Under Test**. It's a common testing acronym.

The `cut` variable is the rendered component instance that you can:
- Inspect its rendered output
- Find elements within it
- Trigger actions on it
- Verify its state

#### IRenderedComponent&lt;T&gt;

The `cut` variable is of type `IRenderedComponent<ProductsList>`, which provides:

```csharp
cut.Markup                  // The rendered HTML
cut.Instance                // The actual component instance
cut.Find("selector")        // Find first matching element
cut.FindAll("selector")     // Find all matching elements
cut.Click("button")        // Simulate click
cut.Change("input", value)  // Simulate input change
cut.Render()               // Re-render component
```

#### Example Usage

```csharp
// Get the rendered HTML
var html = cut.Markup;

// Check if "Widget" appears in the output
Assert.Contains("Widget", html);

// Find an element
var button = cut.Find("button.btn-primary");

// Click a button
cut.Find("button").Click();

// Get the component instance
var component = cut.Instance;
var products = component.products; // Access component's properties
```

---

### Markup Property

```csharp
cut.Markup.Should().Contain("No products found");
```

The **Markup** property contains the entire rendered HTML output of the component.

#### What Is Markup?

When you write a Razor component:

```razor
<h3>Products</h3>
<p>No products found</p>
```

BUnit renders this to HTML:

```html
<h3>Products</h3>
<p>No products found</p>
```

The `Markup` property contains this HTML string.

#### How to Use Markup for Assertions

```csharp
// Check if text appears anywhere
cut.Markup.Should().Contain("Widget");

// Check for specific HTML structure
cut.Markup.Should().Contain("<table class=\"table\">");

// Verify styling
cut.Markup.Should().Contain("badge bg-success");

// Verify absence
cut.Markup.Should().NotContain("Error:");
```

#### Real Example

Given this component:

```razor
@if (!products.Any())
{
    <p>No products found</p>
}
else
{
    <table class="table">
        <tr><td>Widget</td></tr>
    </table>
}
```

When no products exist, `Markup` will contain:
```html
<p>No products found</p>
```

When products exist, `Markup` will contain:
```html
<table class="table">
    <tr><td>Widget</td></tr>
</table>
```

---

## 🎓 Example Walkthrough

Let's walk through a complete test to understand how it all works:

```csharp
[Fact]
public void ProductsList_ShouldRenderProducts_WhenProductsExist()
{
    // ARRANGE: Setup test data and mocks
    var products = new List<ProductDto>
    {
        new ProductDto { Id = 1, Name = "Widget", Price = 99.99m },
        new ProductDto { Id = 2, Name = "Gadget", Price = 149.99m }
    };
    
    _productServiceMock.Setup(s => s.GetAllAsync())
        .ReturnsAsync(products);

    // ACT: Render the component
    // This triggers:
    // 1. Component instantiation
    // 2. Service injection (IProductClientService)
    // 3. OnInitializedAsync() execution
    // 4. ProductService.GetAllAsync() call (mocked)
    // 5. State update with products
    // 6. Rendering with products
    var cut = RenderComponent<ProductsList>();

    // ASSERT: Verify the rendered output
    // Markup now contains HTML like:
    // <table>
    //   <tr><td>Widget</td></tr>
    //   <tr><td>Gadget</td></tr>
    // </table>
    cut.Markup.Should().Contain("Widget");
    cut.Markup.Should().Contain("Gadget");
    cut.Markup.Should().Contain("$99.99");
    cut.Markup.Should().Contain("$149.99");
}
```

### Step-by-Step Flow

1. **Arrange Phase**
   - Creates mock product data
   - Configures mock service to return these products

2. **Act Phase**
   - `RenderComponent<ProductsList>()` is called
   - BUnit creates component instance
   - Component calls `OnInitializedAsync()`
   - Component calls `ProductService.GetAllAsync()`
   - Mock returns the test products
   - Component updates `products` field
   - Component re-renders with product data

3. **Assert Phase**
   - Verifies rendered HTML contains expected text
   - Checks product names appear
   - Checks formatted prices appear

---

## 🎨 Common Patterns

### 1. Testing Empty State

```csharp
[Fact]
public void ProductsList_ShouldRenderEmptyState_WhenNoProducts()
{
    // Arrange: Setup mock to return empty list
    _productServiceMock.Setup(s => s.GetAllAsync())
        .ReturnsAsync(new List<ProductDto>());

    // Act: Render component
    var cut = RenderComponent<ProductsList>();

    // Assert: Verify empty state message
    cut.Markup.Should().Contain("No products found");
}
```

### 2. Testing Loading State

```csharp
[Fact]
public void ProductsList_ShouldShowLoading_WhenDataIsNull()
{
    // Arrange: Setup mock to return slowly
    _productServiceMock.Setup(s => s.GetAllAsync())
        .Returns(async () =>
        {
            await Task.Delay(100);
            return new List<ProductDto>();
        });

    // Act: Render (before async completes)
    var cut = RenderComponent<ProductsList>();

    // Assert: Should show loading state
    cut.Markup.Should().Contain("Loading");
}
```

### 3. Testing Component Interaction

```csharp
[Fact]
public void ProductsList_ShouldDeleteProduct_WhenDeleteClicked()
{
    // Arrange
    var products = new List<ProductDto> 
    { 
        new ProductDto { Id = 1, Name = "Widget" } 
    };
    _productServiceMock.Setup(s => s.GetAllAsync())
        .ReturnsAsync(products);
    _productServiceMock.Setup(s => s.DeleteAsync(1))
        .Returns(Task.CompletedTask);

    var cut = RenderComponent<ProductsList>();

    // Act: Click delete button
    cut.Find("button.btn-danger").Click();

    // Assert: Verify DeleteAsync was called
    _productServiceMock.Verify(s => s.DeleteAsync(1), Times.Once);
}
```

### 4. Testing with Parameters

```csharp
[Fact]
public void ProductCard_ShouldDisplayProduct()
{
    var product = new ProductDto { Name = "Widget", Price = 99.99m };

    var cut = RenderComponent<ProductCard>(parameters => parameters
        .Add(p => p.Product, product)
    );

    cut.Markup.Should().Contain("Widget");
    cut.Markup.Should().Contain("$99.99");
}
```

---

## ✅ Best Practices

### 1. **Arrange, Act, Assert Pattern**

Always structure tests clearly:

```csharp
[Fact]
public void Test_Method_Should_DoSomething()
{
    // ARRANGE: Setup
    _mock.Setup(...).Returns(...);

    // ACT: Execute
    var cut = RenderComponent<MyComponent>();

    // ASSERT: Verify
    cut.Markup.Should().Contain("Expected text");
}
```

### 2. **Use Descriptive Test Names**

```csharp
// ✅ Good
ProductsList_ShouldRenderEmptyState_WhenNoProducts()

// ❌ Bad
Test1()
Test_ProductsList()
```

### 3. **Isolate Tests**

Each test should be independent:

```csharp
// ✅ Good: Each test manages its own setup
[Fact]
public void Test1() 
{
    _mock.Setup(...);
    // Test code
}

[Fact]
public void Test2()
{
    _mock.Setup(...); // Different setup
    // Test code
}
```

### 4. **Mock External Dependencies**

Never use real HTTP calls or databases in unit tests:

```csharp
// ✅ Good: Mock the service
_productServiceMock.Setup(s => s.GetAllAsync())
    .ReturnsAsync(new List<ProductDto>());

// ❌ Bad: Real HTTP call
var response = await httpClient.GetAsync("https://api.com/products");
```

### 5. **Test Behavior, Not Implementation**

Focus on what the component does, not how:

```csharp
// ✅ Good: Tests if products are displayed
cut.Markup.Should().Contain("Widget");

// ❌ Bad: Tests internal state
cut.Instance.products.Count.Should().Be(2);
```

---

## 📚 Additional Resources

- **BUnit Documentation**: https://bunit.dev/
- **Component Testing Guide**: See `PRODUCTS_BLAZOR_UI_TDD.md`
- **Mocking with Moq**: See `DEPENDENCY_INJECTION_TESTS.md`

---

## 🔗 Related Documentation

- [Dependency Injection in Tests](./DEPENDENCY_INJECTION_TESTS.md)
- [Client Services Architecture](../architecture/CLIENT_SERVICES.md)
- [Blazor Component Testing](../features/Products/PRODUCTS_BLAZOR_UI_TDD.md)


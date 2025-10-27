# Blazor Components Architecture

**Last Updated:** 2024-12-19  
**Project:** EnterpriseCRM Blazor Server  
**Purpose:** Understanding Blazor component architecture and dependency injection

---

## 📋 Table of Contents

- [Overview](#overview)
- [@inject Directive](#inject-directive)
- [Component Parameters](#component-parameters)
- [Component Lifecycle](#component-lifecycle)
- [Client Services Pattern](#client-services-pattern)
- [Common Patterns](#common-patterns)

---

## 🎯 Overview

Blazor components are the building blocks of Blazor Server applications. They combine:
- **Razor markup** for UI
- **C# code** for logic
- **Dependency injection** for services
- **Parameters** for configuration
- **Event callbacks** for communication

### Component Example

```razor
@using EnterpriseCRM.Application.DTOs
@using EnterpriseCRM.BlazorServer.Services
@inject IProductClientService ProductService

<h3>Products</h3>

@code {
    private List<ProductDto> products = new();

    protected override async Task OnInitializedAsync()
    {
        products = (await ProductService.GetAllAsync()).ToList();
    }
}
```

---

## 🔌 @inject Directive

### What is @inject?

The `@inject` directive in Blazor is used to **inject dependencies** into components from the DI container.

```razor
@using EnterpriseCRM.BlazorServer.Services
@inject IProductClientService ProductService
```

### How @inject Works

1. **Registers a property** in your component
2. **Requests a service** from the DI container
3. **Sets the property** automatically
4. **Makes the service available** in your component's code

#### Complete Example

```razor
@using EnterpriseCRM.BlazorServer.Services
@inject IProductClientService ProductService

<h3>Products</h3>

@code {
    protected override async Task OnInitializedAsync()
    {
        // ProductService is available here because of @inject
        var products = await ProductService.GetAllAsync();
    }
}
```

### Behind the Scenes

When you use `@inject`, Blazor automatically generates code like this:

```csharp
// What you write
@inject IProductClientService ProductService

// What Blazor generates behind the scenes
[Inject]
public IProductClientService ProductService { get; set; } = default!;
```

### Why is @inject Needed?

**Without @inject** (doesn't work):
```razor
@code {
    // ❌ This won't work - no property exists
    protected override async Task OnInitializedAsync()
    {
        var products = await ProductService.GetAllAsync(); // Error!
    }
}
```

**With @inject** (works):
```razor
@inject IProductClientService ProductService

@code {
    // ✅ Now ProductService is available
    protected override async Task OnInitializedAsync()
    {
        var products = await ProductService.GetAllAsync(); // Works!
    }
}
```

### What Happens During Injection?

```
┌─────────────────────────────────────────┐
│ Component renders                        │
└────────────┬────────────────────────────┘
             │
             │ Needs IProductClientService
             ↓
┌─────────────────────────────────────────┐
│ DI Container                              │
│ - Looks up IProductClientService         │
│ - Finds registered instance               │
│ - Returns ProductClientService instance  │
└────────────┬────────────────────────────┘
             │
             │ Injects instance
             ↓
┌─────────────────────────────────────────┐
│ Component receives:                      │
│ ProductService = [instance]              │
│ - Can now use ProductService             │
└─────────────────────────────────────────┘
```

### Common Use Cases

#### 1. HTTP Client Services
```razor
@inject IProductClientService ProductService
```

#### 2. Navigation Manager
```razor
@inject NavigationManager Navigation
```

#### 3. Logger
```razor
@inject ILogger<MyComponent> Logger
```

#### 4. Custom Services
```razor
@inject ICustomService MyService
```

### @inject vs Constructor Injection

In regular C# classes, you use constructor injection:

```csharp
public class MyService
{
    private readonly IHttpClient _httpClient;
    
    public MyService(IHttpClient httpClient) // Constructor injection
    {
        _httpClient = httpClient;
    }
}
```

In Blazor components, you use `@inject`:

```razor
@inject IHttpClient HttpClient

@code {
    // HttpClient is available here
}
```

**Why the difference?**
- Blazor components have special lifecycle
- `@inject` is cleaner syntax for markup
- Automatically sets properties during component creation

### Multiple @inject Statements

You can inject multiple services:

```razor
@inject IProductClientService ProductService
@inject ICustomerClientService CustomerService
@inject ILogger<ProductsList> Logger

@code {
    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("Loading products");
        var products = await ProductService.GetAllAsync();
    }
}
```

### Real Example: ProductForm.razor

In `ProductForm.razor`, `@inject` is essential for the component to communicate with the WebAPI:

```razor
@using EnterpriseCRM.BlazorServer.Services
@inject IProductClientService ProductService

@code {
    protected override async Task OnInitializedAsync()
    {
        if (IsEdit && ProductId.HasValue)
        {
            // ❌ Without @inject: ProductService doesn't exist - Error!
            // ✅ With @inject: ProductService is available and loaded from DI
            var existingProduct = await ProductService.GetByIdAsync(ProductId.Value);
        }
    }

    private async Task HandleSubmit()
    {
        if (IsEdit)
        {
            // ❌ Without @inject: Cannot access ProductService
            await ProductService.UpdateAsync(updateDto);
        }
        else
        {
            // ❌ Without @inject: Cannot create products
            await ProductService.CreateAsync(product);
        }
    }
}
```

**Why ProductForm needs @inject:**
- ✅ **Fetches existing products** from WebAPI for editing
- ✅ **Creates new products** via HTTP calls
- ✅ **Updates products** with new data
- ✅ **Separates concerns** - form doesn't know HTTP details
- ✅ **Testable** - can be mocked in tests

**Without @inject:**
- ❌ `ProductService` doesn't exist - compilation error
- ❌ Component can't access WebAPI
- ❌ Form is useless - no save functionality

---

## 🎛️ Component Parameters

### What are Parameters?

Parameters allow parent components to pass data to child components:

```razor
<!-- Parent component -->
<ProductForm IsEdit="true" ProductId="42" />

<!-- Child component definition -->
@code {
    [Parameter] public bool IsEdit { get; set; }
    [Parameter] public int? ProductId { get; set; }
}
```

### Parameter Binding

```razor
<!-- Two-way binding -->
<InputText @bind-Value="model.Name" />

<!-- One-way binding -->
<InputText Value="model.Name" />

<!-- With change handler -->
<InputText Value="model.Name" ValueChanged="@((string value) => model.Name = value)" />
```

### Event Callbacks

```razor
<!-- Parent -->
<ProductForm OnSaved="HandleProductSaved" />

<!-- Child -->
@code {
    [Parameter] public EventCallback OnSaved { get; set; }
    
    private async Task HandleSubmit()
    {
        // ... save product ...
        await OnSaved.InvokeAsync(); // Notify parent
    }
}
```

---

## 🔄 Component Lifecycle

### Lifecycle Methods

```razor
@code {
    protected override void OnInitialized()
    {
        // Component is initialized
    }

    protected override async Task OnInitializedAsync()
    {
        // Component is initialized (async)
        // Best place to load data
        products = await ProductService.GetAllAsync();
    }

    protected override void OnParametersSet()
    {
        // Parameters have been set or updated
    }

    protected override async Task OnParametersSetAsync()
    {
        // Parameters updated (async)
    }

    protected override bool ShouldRender()
    {
        // Return false to prevent rendering
        return true;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        // Component has rendered
    }

    public void Dispose()
    {
        // Cleanup (if implementing IDisposable)
    }
}
```

### Lifecycle Sequence

```
OnInitialized() → OnInitializedAsync() 
                 ↓
           OnParametersSet() → OnParametersSetAsync()
                 ↓
           ShouldRender()
                 ↓
            Renders HTML
                 ↓
         OnAfterRender(firstRender)
```

---

## 🌐 Client Services Pattern

### What are Client Services?

Client services are abstraction layers that handle communication between Blazor components and REST APIs.

See [Client Services Architecture](./CLIENT_SERVICES.md) for detailed information.

### Quick Example

```razor
@inject IProductClientService ProductService

@code {
    private List<ProductDto> products;

    protected override async Task OnInitializedAsync()
    {
        products = (await ProductService.GetAllAsync()).ToList();
    }
}
```

---

## 🎨 Common Patterns

### 1. Data Loading Pattern

```razor
@inject IProductClientService ProductService

@if (products == null)
{
    <p>Loading...</p>
}
else if (!products.Any())
{
    <p>No data found</p>
}
else
{
    @foreach (var product in products)
    {
        <div>@product.Name</div>
    }
}

@code {
    private List<ProductDto>? products;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            products = (await ProductService.GetAllAsync()).ToList();
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }
}
```

### 2. Form Handling Pattern

```razor
<EditForm Model="product" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <InputText @bind-Value="product.Name" />
    <ValidationMessage For="@(() => product.Name)" />
    
    <button type="submit">Submit</button>
</EditForm>

@code {
    private CreateProductDto product = new();

    private async Task HandleSubmit()
    {
        await ProductService.CreateAsync(product);
        // Handle success
    }
}
```

### 3. Conditional Rendering

```razor
@if (IsEdit)
{
    <button @onclick="Update">Update</button>
}
else
{
    <button @onclick="Create">Create</button>
}
```

### 4. Looping Over Data

```razor
@foreach (var item in items)
{
    <div>@item.Name</div>
}

@for (int i = 0; i < items.Count(); i++)
{
    <div>Item @i: @items[i].Name</div>
}
```

---

## 📚 Additional Resources

- **BUnit Testing**: See `BUNIT_COMPONENT_TESTING.md` for testing components
- **Dependency Injection**: See `DEPENDENCY_INJECTION_TESTS.md` for testing with DI
- **Client Services**: See `CLIENT_SERVICES.md` for service architecture

---

## 🔗 Related Documentation

- [Client Services Architecture](./CLIENT_SERVICES.md)
- [BUnit Component Testing](../testing/BUNIT_COMPONENT_TESTING.md)
- [Dependency Injection in Tests](../testing/DEPENDENCY_INJECTION_TESTS.md)


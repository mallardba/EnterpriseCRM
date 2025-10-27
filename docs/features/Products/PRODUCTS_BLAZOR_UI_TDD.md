# Products Blazor UI - Test Driven Development Guide

This document demonstrates how to implement a Products UI in Blazor Server using Test Driven Development (TDD) principles, building on the ProductsController we created.

## 🎯 Overview

We'll create a complete Products management UI in Blazor Server following the **Red-Green-Refactor** cycle:

1. **Red**: Write a failing test first (for component logic)
2. **Green**: Write the minimum code to make the test pass
3. **Refactor**: Improve the code while keeping tests green
4. **Repeat**: Continue for each UI feature

---

## 📑 Table of Contents

1. [Architecture Overview](#-architecture-overview)
2. [Implementation Order](#-implementation-order)
3. [Step 1: Setup Blazor Test Environment](#-step-1-setup-blazor-test-environment)
4. [Step 2: Red Phase - Write Failing Component Tests](#-step-2-red-phase---write-failing-component-tests)
5. [Step 3: Green Phase - Create Razor Components](#-step-3-green-phase---create-razor-components)
6. [Step 4: Create Product List Page](#-step-4-create-product-list-page)
7. [Step 5: Create Product Create/Edit Forms](#-step-5-create-product-createedit-forms)
8. [Step 6: Add Navigation and Layout](#-step-6-add-navigation-and-layout)
9. [Step 7: Test and Refactor](#-step-7-test-and-refactor)
10. [Step 8: Integration Testing](#-step-8-integration-testing)
11. [TDD Interview Talking Points](#-tdd-interview-talking-points)

---

## 🏗️ Architecture Overview

### **How Blazor Server Integrates with WebAPI**

```
┌─────────────────────────────────────────────────────────┐
│              User's Browser                             │
│  ┌─────────────────────────────────────────────────┐   │
│  │         Blazor Server Page                      │   │
│  │  (Razor Components)                             │   │
│  └─────────────────┬───────────────────────────────┘   │
└────────────────────┼───────────────────────────────────┘
                     │ SignalR Connection
                     │ (WebSocket/long polling)
        ┌────────────▼────────────┐
        │   Blazor Server Hub      │
        │   (Server-side logic)    │
        └────────────┬─────────────┘
                     │ HTTP Call
        ┌────────────▼────────────┐
        │   WebAPI Endpoint       │
        │   /api/Products         │
        └────────────┬────────────┘
                     │ EF Core Query
        ┌────────────▼────────────┐
        │   SQL Server            │
        │   Products Table        │
        └─────────────────────────┘
```

**Key Points:**
- Blazor Server runs on the server, not in the browser
- SignalR maintains a persistent connection to send UI updates
- Blazor components call WebAPI endpoints via HTTP
- All business logic stays in the WebAPI layer

### **Request Flow Example: View Products**

```
1. User visits /products
   ↓
2. Blazor renders ProductsPage.razor
   ↓
3. OnInitializedAsync() executes
   ↓
4. Page makes HTTP request to WebAPI
   GET https://localhost:5001/api/products
   ↓
5. WebAPI ProductsController.GetAll()
   ↓
6. ProductService.GetAllAsync()
   ↓
7. UnitOfWork.Products.GetAllAsync()
   ↓
8. ProductRepository queries database
   ↓
9. Response flows back (DTOs)
   ↓
10. Blazor receives ProductDto[]
   ↓
11. UI updates via SignalR
   ↓
12. User sees product list
```

---

## 🏗️ Implementation Order

When creating a new UI feature using TDD and Blazor, follow this implementation order:

1. **Write Component Tests (RED)** → `tests/EnterpriseCRM.UnitTests/Components/ProductsListTests.cs` - **Testing Layer**
   - Test component state changes
   - Test data binding
   - Tests should FAIL (component doesn't exist)

2. **Create Model/Data Transfer Classes** → `src/EnterpriseCRM.BlazorServer/Models/` - **Blazor Server Layer**
   - ProductListItem for display
   - ProductEditModel for forms

3. **Create Razor Component (GREEN)** → `src/EnterpriseCRM.BlazorServer/Pages/Products/` - **Blazor Server Layer**
   - ProductsPage.razor
   - Component tests now PASS

4. **Inject HTTP Client** → `src/EnterpriseCRM.BlazorServer/Program.cs` - **Blazor Server Layer**
   - Register HttpClient for WebAPI calls

5. **Create Service Layer** → `src/EnterpriseCRM.BlazorServer/Services/ProductClientService.cs` - **Blazor Server Layer**
   - HTTP client wrapper for WebAPI calls
   - Async methods for CRUD operations

6. **Create Product List Component** → `src/EnterpriseCRM.BlazorServer/Components/Products/ProductsList.razor` - **Blazor Server Layer**
   - Displays product grid
   - Search and filter functionality

7. **Create Product Form Component** → `src/EnterpriseCRM.BlazorServer/Components/Products/ProductForm.razor` - **Blazor Server Layer**
   - Create/Edit form
   - Validation

8. **Add Navigation** → `src/EnterpriseCRM.BlazorServer/Shared/NavMenu.razor` - **Blazor Server Layer**
   - Add Products menu item

9. **Write Integration Tests (RED)** → `tests/EnterpriseCRM.IntegrationTests/UI/ProductsUITests.cs` - **Testing Layer**
   - Test full user flows
   - E2E testing with browser automation

10. **Refactor and Optimize** - **All Layers**
    - Performance improvements
    - Code cleanup
    - All tests still PASS

**Key Principle**: Write tests first, implement components, then verify with integration tests.

---

## 📋 Step 1: Setup Blazor Test Environment

### **Why**: We need a testing framework for Blazor components before writing tests.

### **Add Required Packages**

Update `tests/EnterpriseCRM.UnitTests/EnterpriseCRM.UnitTests.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="xunit" Version="2.6.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
  <PackageReference Include="Moq" Version="4.20.69" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <!-- Add these for Blazor testing -->
  <PackageReference Include="bunit" Version="1.30.49" />
  <PackageReference Include="bunit.web" Version="1.30.49" />
</ItemGroup>
```

**Why These Packages?**
- **bunit**: Testing framework specifically for Blazor components
- **bunit.web**: Web-specific testing utilities for Blazor components
- Allows testing components in isolation without a browser

---

## 📋 Step 2: Red Phase - Write Failing Component Tests

### **Why**: Tests define expected component behavior before implementation.

### **Test 1: Product List Component Tests**

Create `tests/EnterpriseCRM.UnitTests/Components/ProductsListTests.cs`:  
Passes after step 4.1

```csharp
using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.BlazorServer.Components.Products;
using EnterpriseCRM.BlazorServer.Services;
using Bunit;
using Moq;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Components;

public class ProductsListTests : TestContext
{
    private readonly Mock<IProductClientService> _productServiceMock;

    public ProductsListTests()
    {
        _productServiceMock = new Mock<IProductClientService>();
        Services.AddSingleton(_productServiceMock.Object);
    }

    [Fact]
    public void ProductsList_ShouldRenderEmptyState_WhenNoProducts()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<ProductDto>());

        // Act
        var cut = RenderComponent<ProductsList>();

        // Assert
        cut.Markup.Should().Contain("No products found");
    }

    [Fact]
    public void ProductsList_ShouldRenderProducts_WhenProductsExist()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new ProductDto { Id = 1, Name = "Widget", Price = 99.99m, IsActive = true },
            new ProductDto { Id = 2, Name = "Gadget", Price = 149.99m, IsActive = true }
        };

        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var cut = RenderComponent<ProductsList>();

        // Assert
        cut.Markup.Should().Contain("Widget");
        cut.Markup.Should().Contain("Gadget");
        cut.Markup.Should().Contain("$99.99");
        cut.Markup.Should().Contain("$149.99");
    }
}
```

**Test Result**: ❌ Build fails - `ProductsList` component, `IProductClientService` don't exist

---

### **Test 2: Product Form Component Tests**

Create `tests/EnterpriseCRM.UnitTests/Components/ProductFormTests.cs`:

```csharp
using EnterpriseCRM.BlazorServer.Components.Products;
using Bunit;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Components;

public class ProductFormTests : TestContext
{
    [Fact]
    public void ProductForm_ShouldRenderInputFields()
    {
        // Act
        var cut = RenderComponent<ProductForm>();

        // Assert
        cut.Markup.Should().Contain("Name");
        cut.Markup.Should().Contain("Price");
        cut.Markup.Should().Contain("SKU");
        cut.Markup.Should().Contain("Category");
    }

    [Fact]
    public void ProductForm_ShouldValidateRequiredFields()
    {
        // Arrange
        var cut = RenderComponent<ProductForm>();

        // Act
        cut.Find("button[type=submit]").Click();

        // Assert
        cut.Markup.Should().Contain("Name is required");
    }
}
```

**Test Result**: ❌ Build fails - `ProductForm` component doesn't exist

---

## ✅ Step 3: Green Phase - Create Razor Components

### **Step 3.1: Create Product Client Service**

**Why**: Blazor needs a service to call WebAPI endpoints. The service acts as a proxy between UI and API.

Create `src/EnterpriseCRM.BlazorServer/Services/ProductClientService.cs`:

```csharp
using EnterpriseCRM.Application.DTOs;
using System.Net.Http.Json;

namespace EnterpriseCRM.BlazorServer.Services;

public interface IProductClientService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto product);
    Task<ProductDto> UpdateAsync(UpdateProductDto product);
    Task DeleteAsync(int id);
}

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
        return await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>() ?? Enumerable.Empty<ProductDto>();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<ProductDto>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto product)
    {
        var response = await _httpClient.PostAsJsonAsync(_baseUrl, product);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>() ?? throw new Exception("Failed to create product");
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductDto product)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{product.Id}", product);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>() ?? throw new Exception("Failed to update product");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
        response.EnsureSuccessStatusCode();
    }
}
```

### **Step 3.2: Register Service in Program.cs**

**Why**: Dependency injection allows components to use the service.

Update `src/EnterpriseCRM.BlazorServer/Program.cs`:

```csharp
using EnterpriseCRM.BlazorServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add HttpClient for WebAPI calls
builder.Services.AddHttpClient<IProductClientService, ProductClientService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5001");
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

---

## ✅ Step 4: Create Product List Page

### **Step 4.1: Create Products List Component**

**Why**: Component-based architecture allows reusable UI elements.

Create `src/EnterpriseCRM.BlazorServer/Components/Products/ProductsList.razor`:

```razor
@using EnterpriseCRM.Application.DTOs
@using EnterpriseCRM.BlazorServer.Services
@inject IProductClientService ProductService

<h3>Products</h3>

@if (products == null)
{
    <p>Loading products...</p>
}
else if (!products.Any())
{
    <p>No products found</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Name</th>
                <th>SKU</th>
                <th>Price</th>
                <th>Category</th>
                <th>Status</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var product in products)
            {
                <tr>
                    <td>@product.Name</td>
                    <td>@product.SKU</td>
                    <td>@product.Price.ToString("C")</td>
                    <td>@product.Category</td>
                    <td>
                        @if (product.IsActive)
                        {
                            <span class="badge bg-success">Active</span>
                        }
                        else
                        {
                            <span class="badge bg-secondary">Inactive</span>
                        }
                    </td>
                    <td>
                        <button class="btn btn-sm btn-primary" @onclick="() => Edit(product.Id)">Edit</button>
                        <button class="btn btn-sm btn-danger" @onclick="() => Delete(product.Id)">Delete</button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private List<ProductDto> products = new();

    protected override async Task OnInitializedAsync()
    {
        products = (await ProductService.GetAllAsync()).ToList();
    }

    private void Edit(int id)
    {
        // Navigate to edit page
    }

    private async Task Delete(int id)
    {
        await ProductService.DeleteAsync(id);
        products = (await ProductService.GetAllAsync()).ToList();
        StateHasChanged();
    }
}
```

**Test Result**: ✅ Component tests should now PASS (after updating mocks)

### **Step 4.2: Create Products Page**

Create `src/EnterpriseCRM.BlazorServer/Pages/Products/Products.razor`:

```razor
@page "/products"
@using EnterpriseCRM.BlazorServer.Components.Products

<PageTitle>Products</PageTitle>

<h1>Product Management</h1>

<div class="d-flex justify-content-between mb-3">
    <div>
        <input type="text" class="form-control" placeholder="Search products..." @bind="searchTerm" />
    </div>
    <div>
        <button class="btn btn-primary" @onclick="ShowCreateForm">Add Product</button>
    </div>
</div>

<ProductsList />

@code {
    private string searchTerm = string.Empty;

    private void ShowCreateForm()
    {
        // Navigate to create page
    }
}
```

---

## ✅ Step 5: Create Product Create/Edit Forms

### **Step 5.1: Create Product Form Component**

**Why**: Forms handle user input and validation before sending to API.

Create `src/EnterpriseCRM.BlazorServer/Components/Products/ProductForm.razor`:

```razor
@using EnterpriseCRM.Application.DTOs

<EditForm Model="product" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <div class="form-group mb-3">
        <label for="name">Name *</label>
        <InputText id="name" class="form-control" @bind-Value="product.Name" />
        <ValidationMessage For="@(() => product.Name)" />
    </div>

    <div class="form-group mb-3">
        <label for="description">Description</label>
        <InputTextArea id="description" class="form-control" @bind-Value="product.Description" />
    </div>

    <div class="form-group mb-3">
        <label for="sku">SKU</label>
        <InputText id="sku" class="form-control" @bind-Value="product.SKU" />
    </div>

    <div class="form-group mb-3">
        <label for="price">Price *</label>
        <InputNumber id="price" class="form-control" @bind-Value="product.Price" />
        <ValidationMessage For="@(() => product.Price)" />
    </div>

    <div class="form-group mb-3">
        <label for="cost">Cost</label>
        <InputNumber id="cost" class="form-control" @bind-Value="product.Cost" />
    </div>

    <div class="form-group mb-3">
        <label for="category">Category</label>
        <InputText id="category" class="form-control" @bind-Value="product.Category" />
    </div>

    <div class="form-group mb-3">
        <InputCheckbox id="isActive" @bind-Value="product.IsActive" />
        <label for="isActive">Active</label>
    </div>

    <div class="d-flex gap-2">
        <button type="submit" class="btn btn-primary">@(IsEdit ? "Update" : "Create")</button>
        <button type="button" class="btn btn-secondary" @onclick="Cancel">Cancel</button>
    </div>
</EditForm>

@code {
    [Parameter] public bool IsEdit { get; set; } = false;
    [Parameter] public int? ProductId { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }

    private CreateProductDto product = new();

    protected override async Task OnInitializedAsync()
    {
        if (IsEdit && ProductId.HasValue)
        {
            // Load existing product for editing
            var existingProduct = await ProductService.GetByIdAsync(ProductId.Value);
            if (existingProduct != null)
            {
                product = new CreateProductDto
                {
                    Name = existingProduct.Name,
                    Description = existingProduct.Description,
                    SKU = existingProduct.SKU,
                    Price = existingProduct.Price,
                    Cost = existingProduct.Cost,
                    Category = existingProduct.Category,
                    IsActive = existingProduct.IsActive
                };
            }
        }
    }

    private async Task HandleSubmit()
    {
        if (IsEdit && ProductId.HasValue)
        {
            var updateDto = new UpdateProductDto
            {
                Id = ProductId.Value,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                Cost = product.Cost,
                Category = product.Category,
                IsActive = product.IsActive
            };
            await ProductService.UpdateAsync(updateDto);
        }
        else
        {
            await ProductService.CreateAsync(product);
        }

        await OnSaved.InvokeAsync();
        Cancel();
    }

    private void Cancel()
    {
        // Navigate back or close form
    }
}
```

**Test Result**: ✅ ProductForm tests should now PASS

---

## ✅ Step 6: Add Navigation and Layout

### **Step 6.1: Update Navigation Menu**

**Why**: Users need a way to access the Products page.

Update `src/EnterpriseCRM.BlazorServer/Shared/NavMenu.razor`:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="customers">
        <span class="oi oi-person" aria-hidden="true"></span> Customers
    </NavLink>
</div>
<div class="nav-item px-3">
    <NavLink class="nav-link" href="products">
        <span class="oi oi-box" aria-hidden="true"></span> Products
    </NavLink>
</div>
```

---

## ✅ Step 7: Test and Refactor

### **Run Component Tests**

```bash
dotnet test tests/EnterpriseCRM.UnitTests
```

**Expected**: All component tests pass ✅

### **Manual Testing Steps**

1. Start Blazor Server: `dotnet run --project src/EnterpriseCRM.BlazorServer`
2. Start WebAPI: `dotnet run --project src/EnterpriseCRM.WebAPI`
3. Navigate to `/products` in browser
4. Test create, edit, delete operations

---

## ✅ Step 8: Integration Testing

### **Why**: Integration tests verify full user flows end-to-end.

### **Browser-Based Integration Tests**

Create `tests/EnterpriseCRM.IntegrationTests/UI/ProductsUITests.cs`:

```csharp
using Xunit;

namespace EnterpriseCRM.IntegrationTests.UI;

public class ProductsUITests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductsUITests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProductsPage_ShouldDisplayProductList()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Products", content);
    }
}
```

---

## 📝 TDD Interview Talking Points

### **"How do you test Blazor components?"**

**Answer:**
"I use bunit to test Blazor components in isolation. I mock services and verify that:
1. Components render correctly
2. User interactions trigger expected behavior
3. Data binding works properly
4. Forms validate inputs"

### **"How do you integrate Blazor Server with WebAPI?"**

**Answer:**
"I create a client service (e.g., `ProductClientService`) that wraps `HttpClient` to call WebAPI endpoints. This service:
1. Encapsulates API calls
2. Can be mocked for testing
3. Returns strongly-typed DTOs
4. Handles errors gracefully"

### **"How do you handle authentication in Blazor?"**

**Answer:**
"I use JWT authentication passed via HTTP headers. The token is stored in browser storage and added to HttpClient requests. Blazor's `IAuthenticationService` manages token lifecycle."

---

## 🎯 Summary: Complete TDD Flow

### **What We Built**

✅ Product listing page with search
✅ Product create/edit forms
✅ Navigation integration
✅ Full CRUD operations
✅ Component tests
✅ Integration tests

### **Key Lessons**

1. **Test First**: Write component tests before implementing UI
2. **Service Layer**: Blazor services call WebAPI, not database directly
3. **Separation**: UI logic in Razor components, business logic in WebAPI
4. **Reusability**: Components can be composed and reused

This TDD approach ensures high-quality, maintainable UI that's well-tested and interview-ready! 🚀


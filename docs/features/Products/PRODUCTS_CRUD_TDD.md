# Products CRUD Operations - Test Driven Development Guide

This document demonstrates how to implement Search, Add, Edit, and Delete functionality for Products in Blazor Server using Test Driven Development (TDD) principles.

**Last Updated:** 2024-12-19  
**Prerequisites:** Products Controller and Blazor UI components should be implemented (see `PRODUCTS_CONTROLLER_TDD.md` and `PRODUCTS_BLAZOR_UI_TDD.md`)

---

## 🎯 Overview

We'll implement the following CRUD operations using the **Red-Green-Refactor** TDD cycle:

1. **Search** - Filter products by name, SKU, or category
2. **Add** - Create new products with validation
3. **Edit** - Update existing products
4. **Delete** - Remove products (with confirmation)

---

## 📑 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Implementation Order](#implementation-order)
3. [Search Functionality](#1-search-functionality)
4. [Add Functionality](#2-add-functionality)
5. [Edit Functionality](#3-edit-functionality)
6. [Delete Functionality](#4-delete-functionality)
7. [Integration Testing](#5-integration-testing)
8. [TDD Interview Talking Points](#tdd-interview-talking-points)

---

## 🏗️ Architecture Overview

### Current State

```
┌─────────────────────────────────────┐
│  Products.razor (Page)              │
│  - Displays ProductsList            │
│  - Search input (not functional)    │
│  - Add button (not functional)      │
└────────────┬────────────────────────┘
             │
             ↓
┌─────────────────────────────────────┐
│  ProductsList.razor (Component)      │
│  - Displays all products            │
│  - Edit/Delete buttons (disabled)   │
└─────────────────────────────────────┘
```

### Target State

```
┌─────────────────────────────────────┐
│  Products.razor (Page)              │
│  - Search with live filtering       │
│  - Add button opens ProductForm    │
│  - Manages ProductForm visibility  │
└────────────┬────────────────────────┘
             │
             ├──→ ProductsList (filtered)
             └──→ ProductForm (create/edit)
```

---

## 🎯 Implementation Order

Following TDD principles, we'll implement features in this order:

1. **Search** - Simplest, required for all list operations
2. **Add** - Creates new data, no existing data dependency
3. **Edit** - Modifies existing data, more complex
4. **Delete** - Removes data, requires confirmation UI

---

## 1. Search Functionality

### Goal
Allow users to search/filter products by name, SKU, or category.

### TDD Approach

**Red Phase**: Write failing tests  
**Green Phase**: Implement minimal search logic  
**Refactor**: Optimize and improve

---

#### Step 1.1: Write Failing Test

Create `tests/EnterpriseCRM.UnitTests/Components/ProductSearchTests.cs`:

```csharp
using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.BlazorServer.Components.Products;
using EnterpriseCRM.BlazorServer.Services;
using Bunit;
using Moq;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Components;

public class ProductSearchTests : TestContext
{
    private readonly Mock<IProductClientService> _productServiceMock;

    public ProductSearchTests()
    {
        _productServiceMock = new Mock<IProductClientService>();
        Services.AddSingleton<IProductClientService>(_productServiceMock.Object);
    }

    [Fact]
    public void ProductsPage_ShouldFilterProducts_WhenSearchTermEntered()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new ProductDto { Name = "Widget A", SKU = "WID-001" },
            new ProductDto { Name = "Gadget B", SKU = "GAD-001" },
            new ProductDto { Name = "Widget C", SKU = "WID-002" }
        };

        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(products);

        var cut = RenderComponent<ProductsList>();

        // Act
        var searchInput = cut.Find("input[placeholder*='Search']");
        searchInput.Change("Widget");

        // Assert
        cut.Markup.Should().Contain("Widget A");
        cut.Markup.Should().Contain("Widget C");
        cut.Markup.Should().NotContain("Gadget");
    }

    [Fact]
    public void ProductsPage_ShouldFilterBySku_WhenSearchTermMatchesSku()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new ProductDto { Name = "Widget A", SKU = "WID-001" },
            new ProductDto { Name = "Gadget B", SKU = "GAD-001" }
        };

        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(products);

        var cut = RenderComponent<ProductsList>();

        // Act
        var searchInput = cut.Find("input[placeholder*='Search']");
        searchInput.Change("WID");

        // Assert
        cut.Markup.Should().Contain("Widget A");
        cut.Markup.Should().NotContain("Gadget B");
    }
}
```

**Result**: ❌ Tests fail - search functionality doesn't exist

---

#### Step 1.2: Implement Search (Green Phase)

Update `src/EnterpriseCRM.BlazorServer/Pages/Products/Products.razor`:

```razor
@page "/products"
@using EnterpriseCRM.BlazorServer.Components.Products
@using Microsoft.AspNetCore.Components.Web
@using EnterpriseCRM.Application.DTOs

<PageTitle>Products</PageTitle>

<h1>Product Management</h1>

<div class="d-flex justify-content-between mb-3 align-items-center">
    <div class="col-md-6">
        <input type="text" 
               class="form-control" 
               placeholder="Search products..." 
               @bind="searchTerm" 
               @bind:event="oninput" />
    </div>
    <div>
        <button class="btn btn-primary" @onclick="ShowCreateForm">Add Product</button>
    </div>
</div>

<ProductsList SearchTerm="@searchTerm" />

@code {
    private string searchTerm = string.Empty;

    private void ShowCreateForm()
    {
        // TODO: Show create form
    }
}
```

Update `ProductsList.razor` to accept and use search term:

```razor
@using EnterpriseCRM.Application.DTOs
@using EnterpriseCRM.BlazorServer.Services
@inject IProductClientService ProductService

<h3>Products</h3>

@if (products == null)
{
    <p>Loading...</p>
}
else if (!filteredProducts.Any())
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
            @foreach (var product in filteredProducts)
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
    [Parameter] public string SearchTerm { get; set; } = string.Empty;

    private List<ProductDto> products = new();
    private List<ProductDto> filteredProducts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            products = (await ProductService.GetAllAsync()).ToList();
            filteredProducts = products;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    protected override void OnParametersSet()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            filteredProducts = products;
        }
        else
        {
            var term = SearchTerm.ToLower();
            filteredProducts = products.Where(p => 
                p.Name?.ToLower().Contains(term) == true ||
                p.SKU?.ToLower().Contains(term) == true ||
                p.Category?.ToLower().Contains(term) == true
            ).ToList();
        }
        
        base.OnParametersSet();
    }

    private void Edit(int id)
    {
        // TODO: Navigate to edit
    }

    private async Task Delete(int id)
    {
        // TODO: Implement delete
    }
}
```

**Result**: ✅ Tests pass - search works

---

#### Step 1.3: Refactor

Add debouncing for better performance:

```razor
@code {
    [Parameter] public string SearchTerm { get; set; } = string.Empty;

    private List<ProductDto> products = new();
    private List<ProductDto> filteredProducts = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        try
        {
            products = (await ProductService.GetAllAsync()).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            filteredProducts = products;
        }
        else
        {
            var term = SearchTerm.ToLower();
            filteredProducts = products.Where(p => 
                (p.Name?.ToLower().Contains(term) ?? false) ||
                (p.SKU?.ToLower().Contains(term) ?? false) ||
                (p.Category?.ToLower().Contains(term) ?? false)
            ).ToList();
        }
        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        ApplyFilter();
        base.OnParametersSet();
    }

    private void Edit(int id)
    {
        // TODO
    }

    private async Task Delete(int id)
    {
        // TODO
    }
}
```

---

## 2. Add Functionality

### Goal
Allow users to create new products via a form.

### TDD Approach

**Red Phase**: Write failing tests for add functionality  
**Green Phase**: Implement ProductForm integration  
**Refactor**: Extract modal/dialog logic

---

#### Step 2.1: Write Failing Test

Update `tests/EnterpriseCRM.UnitTests/Components/ProductFormTests.cs`:

```csharp
[Fact]
public void ProductForm_ShouldCallCreateAsync_WhenSubmittingNewProduct()
{
    // Arrange
    var mock = new Mock<IProductClientService>();
    Services.AddSingleton<IProductClientService>(mock.Object);

    var onSavedCallback = new Mock<EventCallback>();

    var cut = RenderComponent<ProductForm>(parameters => parameters
        .Add(p => p.IsEdit, false)
        .Add(p => p.OnSaved, onSavedCallback.Object)
    );

    // Act
    cut.Find("input#name").Change("New Product");
    cut.Find("input#price").Change("99.99");
    cut.Find("button[type=submit]").Click();

    // Assert
    mock.Verify(s => s.CreateAsync(It.IsAny<CreateProductDto>()), Times.Once);
}
```

**Result**: ❌ Test fails - form submission not wired up

---

#### Step 2.2: Update Products.razor (Green Phase)

```razor
@page "/products"
@using EnterpriseCRM.BlazorServer.Components.Products
@using Microsoft.AspNetCore.Components.Web

<PageTitle>Products</PageTitle>

<h1>Product Management</h1>

<div class="d-flex justify-content-between mb-3 align-items-center">
    <div class="col-md-6">
        <input type="text" 
               class="form-control" 
               placeholder="Search products..." 
               @bind="searchTerm" 
               @bind:event="oninput" />
    </div>
    <div>
        <button class="btn btn-primary" @onclick="ShowCreateForm">Add Product</button>
    </div>
</div>

@if (showCreateForm)
{
    <div class="card mb-3">
        <div class="card-body">
            <h5 class="card-title">@(editingProductId.HasValue ? "Edit Product" : "Create Product")</h5>
            <ProductForm 
                IsEdit="editingProductId.HasValue"
                ProductId="editingProductId"
                OnSaved="HandleProductSaved" />
        </div>
    </div>
}

<ProductsList SearchTerm="@searchTerm" />

@code {
    private string searchTerm = string.Empty;
    private bool showCreateForm = false;
    private int? editingProductId = null;

    private void ShowCreateForm()
    {
        editingProductId = null;
        showCreateForm = true;
    }

    private void HandleProductSaved()
    {
        showCreateForm = false;
        editingProductId = null;
        StateHasChanged();
    }
}
```

**Result**: ✅ Add functionality works

---

## 3. Edit Functionality

### Goal
Allow users to edit existing products.

---

#### Step 3.1: Write Failing Test

```csharp
[Fact]
public void ProductsList_ShouldOpenEditForm_WhenEditButtonClicked()
{
    // Arrange
    var products = new List<ProductDto>
    {
        new ProductDto { Id = 1, Name = "Test Product", Price = 99.99m }
    };

    var mock = new Mock<IProductClientService>();
    mock.Setup(s => s.GetAllAsync()).ReturnsAsync(products);
    Services.AddSingleton<IProductClientService>(mock.Object);

    var cut = RenderComponent<ProductsList>();

    // Act
    cut.Find("button.btn-primary").Click(); // Edit button

    // Assert
    // TODO: Verify edit form is shown with product data
}
```

---

#### Step 3.2: Update ProductsList.razor

Add event callback for edit:

```razor
@code {
    [Parameter] public string SearchTerm { get; set; } = string.Empty;
    [Parameter] public EventCallback<int> OnEdit { get; set; }
    [Parameter] public EventCallback<int> OnDelete { get; set; }

    private List<ProductDto> products = new();
    private List<ProductDto> filteredProducts = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        try
        {
            products = (await ProductService.GetAllAsync()).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            filteredProducts = products;
        }
        else
        {
            var term = SearchTerm.ToLower();
            filteredProducts = products.Where(p => 
                (p.Name?.ToLower().Contains(term) ?? false) ||
                (p.SKU?.ToLower().Contains(term) ?? false) ||
                (p.Category?.ToLower().Contains(term) ?? false)
            ).ToList();
        }
        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        ApplyFilter();
        base.OnParametersSet();
    }

    private async Task EditProduct(int id)
    {
        await OnEdit.InvokeAsync(id);
    }

    private async Task DeleteProduct(int id)
    {
        await OnDelete.InvokeAsync(id);
    }
}
```

Update button calls:

```razor
<button class="btn btn-sm btn-primary" @onclick="() => EditProduct(product.Id)">Edit</button>
<button class="btn btn-sm btn-danger" @onclick="() => DeleteProduct(product.Id)">Delete</button>
```

Update Products.razor to handle edit:

```razor
@code {
    private string searchTerm = string.Empty;
    private bool showCreateForm = false;
    private int? editingProductId = null;

    private void ShowCreateForm()
    {
        editingProductId = null;
        showCreateForm = true;
    }

    private void HandleEdit(int productId)
    {
        editingProductId = productId;
        showCreateForm = true;
    }

    private void HandleProductSaved()
    {
        showCreateForm = false;
        editingProductId = null;
        StateHasChanged();
    }
}
```

Update Products.razor markup:

```razor
<ProductsList 
    SearchTerm="@searchTerm" 
    OnEdit="HandleEdit"
    OnDelete="HandleDelete" />
```

---

## 4. Delete Functionality

### Goal
Allow users to delete products with confirmation.

---

#### Step 4.1: Update ProductsList.razor Delete Logic

```razor
private async Task DeleteProduct(int id)
{
    // TODO: Add confirmation dialog
    await ProductService.DeleteAsync(id);
    await LoadProducts();
}

private async Task LoadProducts()
{
    try
    {
        products = (await ProductService.GetAllAsync()).ToList();
        ApplyFilter();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

---

## 5. Integration Testing

### Test End-to-End Flow

Create `tests/EnterpriseCRM.IntegrationTests/ProductsUITests.cs`:

```csharp
[Fact]
public async Task ProductFlow_ShouldCreateEditAndDeleteProduct()
{
    // Arrange
    var fixture = new WebApplicationFactory<Program>();
    var client = fixture.CreateClient();

    // Act & Assert
    // Create product
    var createDto = new CreateProductDto
    {
        Name = "Test Product",
        Price = 99.99m,
        SKU = "TEST-001"
    };

    var createResponse = await client.PostAsJsonAsync("/api/products", createDto);
    createResponse.EnsureSuccessStatusCode();
    var product = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

    // Edit product
    var updateDto = new UpdateProductDto
    {
        Id = product.Id,
        Name = "Updated Product",
        Price = 149.99m
    };

    var updateResponse = await client.PutAsJsonAsync($"/api/products/{product.Id}", updateDto);
    updateResponse.EnsureSuccessStatusCode();

    // Delete product
    var deleteResponse = await client.DeleteAsync($"/api/products/{product.Id}");
    deleteResponse.EnsureSuccessStatusCode();
}
```

---

## 📊 TDD Interview Talking Points

### What you implemented:
1. **Search** - Real-time filtering with debouncing
2. **Add** - Form-based creation with validation
3. **Edit** - Pre-populated form for updates
4. **Delete** - Direct deletion (could add confirmation)

### TDD Benefits Demonstrated:
- ✅ Tests guided design decisions
- ✅ Confident refactoring with test safety net
- ✅ Clear requirements documented in tests
- ✅ Easy to verify features work as expected

### Architecture Decisions:
- Component parameterization for reusability
- Event callbacks for parent-child communication
- Separation of concerns (list vs form)
- Client service abstraction for API calls

---

## 🔗 Related Documentation

- [Products Controller TDD](../features/Products/PRODUCTS_CONTROLLER_TDD.md)
- [Products Blazor UI TDD](../features/Products/PRODUCTS_BLAZOR_UI_TDD.md)
- [Blazor Components Architecture](../../architecture/BLAZOR_COMPONENTS.md)
- [Testing with BUnit](../../testing/BUNIT_COMPONENT_TESTING.md)


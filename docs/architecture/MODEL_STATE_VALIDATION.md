# ModelState and Validation in ASP.NET Core

## 🎯 Overview

`ModelState` is ASP.NET Core's mechanism for tracking validation state during model binding. It tells you whether incoming data is valid and what errors occurred.

---

## 📚 Table of Contents

1. [What is ModelState?](#-what-is-modelstate)
2. [How ModelState Works](#-how-modelstate-works)
3. [Where It's Defined](#-where-its-defined)
4. [ModelState Methods and Properties](#-modelstate-methods-and-properties)
5. [Validation Attributes](#-validation-attributes)
6. [Complete Validation Flow](#-complete-validation-flow)
7. [Common Patterns](#-common-patterns)
8. [Advanced Topics](#-advanced-topics)
9. [Resources for Deeper Dive](#-resources-for-deeper-dive)

---

## 🔍 What is ModelState?

`ModelState` is a `ModelStateDictionary` object (found in `ControllerBase.ModelState`) that:

- **Tracks validation errors** for each property in your DTO
- **Records model binding errors** (e.g., can't convert string to int)
- **Determines if a request is valid** using `IsValid` property
- **Stores error messages** to send back to the client

### **Example in Your ProductsController**

```csharp
[HttpPost]
public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createDto)
{
    // ModelState is automatically populated during model binding
    if (!ModelState.IsValid)  // Check if any validation errors occurred
    {
        return BadRequest(ModelState);  // Return 400 Bad Request with errors
    }
    
    // All validations passed - process the request
    var product = await _service.CreateAsync(createDto);
    return Ok(product);
}
```

---

## 🛠️ How ModelState Works

### **Step-by-Step Process**

```
1. HTTP Request arrives
   ↓
2. ASP.NET Core attempts model binding
   ↓
3. Validation attributes are checked (e.g., [Required])
   ↓
4. Each error is added to ModelState
   ↓
5. ModelState.IsValid reflects validation status
   ↓
6. Controller checks ModelState.IsValid
   ↓
7. Returns success (200) or errors (400)
```

### **Concrete Example**

**Request:**
```json
POST /api/products
{
  "name": "",  // Empty - invalid
  "price": -10  // Negative - invalid
}
```

**DTO Definition:**
```csharp
public class CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
}
```

**ModelState Contents:**
```csharp
ModelState["name"].Errors = [
    "Name is required"
]

ModelState["price"].Errors = [
    "Price must be between 0.01 and maximum value"
]

ModelState.IsValid = false
```

**Controller Response:**
```csharp
// Returns 400 Bad Request
{
  "name": ["Name is required"],
  "price": ["Price must be between 0.01 and maximum value"]
}
```

---

## 📍 Where It's Defined

### **ControllerBase Class**

`ModelState` is a property of `ControllerBase`, which all your controllers inherit from:

```csharp
// In ControllerBase (Microsoft.AspNetCore.Mvc)
public abstract class ControllerBase
{
    public ModelStateDictionary ModelState { get; }
}

// In Controller (also inherits from ControllerBase)
public abstract class Controller : ControllerBase
{
    // ... view-related methods
}
```

### **ModelStateDictionary Class**

The `ModelState` property is actually a `ModelStateDictionary`:

```csharp
public class ModelStateDictionary : IReadOnlyDictionary<string, ModelStateEntry>
{
    public bool IsValid { get; }
    public int ErrorCount { get; }
    public void Clear();
    public bool ContainsKey(string key);
    public void AddModelError(string key, string errorMessage);
    public void Remove(string key);
    // ... many more methods
}
```

**Location**: `Microsoft.AspNetCore.Mvc.ModelBinding` namespace

### **How It Gets Populated**

ASP.NET Core's model binding system automatically populates `ModelState` during request processing:

1. **Framework Level**: In `ModelBinder`, validation runs
2. **Automatic Population**: Errors are added to `ModelState`
3. **Controller Access**: `ModelState.IsValid` tells you if request is valid

---

## 🛠️ ModelState Methods and Properties

### **Most Commonly Used**

#### **1. ModelState.IsValid**

```csharp
if (!ModelState.IsValid)
{
    return BadRequest(ModelState);
}
```

**Returns**: `bool` - `true` if no errors, `false` if errors exist

---

#### **2. ModelState.Keys**

```csharp
// Get all property names with errors
var invalidFields = ModelState.Keys.Where(k => ModelState[k].Errors.Count > 0);
```

**Returns**: `IEnumerable<string>` - Collection of property names

---

#### **3. ModelState.Values**

```csharp
// Get all validation errors
var allErrors = ModelState.Values
    .SelectMany(v => v.Errors)
    .Select(e => e.ErrorMessage)
    .ToList();
```

**Returns**: `IEnumerable<ModelStateEntry>` - All model state entries

---

#### **4. ModelState[key]**

```csharp
// Get specific field's errors
var nameErrors = ModelState["name"]?.Errors;
```

**Returns**: `ModelStateEntry?` - Entry for specific property

---

### **Less Common but Useful**

#### **5. ModelState.Clear()**

```csharp
ModelState.Clear();  // Remove all errors
```

#### **6. ModelState.AddModelError(key, message)**

```csharp
// Manually add custom validation error
if (createDto.Price < Cost)
{
    ModelState.AddModelError("price", "Price must be greater than cost");
}
```

#### **7. ModelState.Remove(key)**

```csharp
ModelState.Remove("someField");  // Remove field from validation
```

#### **8. ModelState.ErrorCount**

```csharp
if (ModelState.ErrorCount > 0)
{
    // Has errors
}
```

#### **9. ModelState.TryGetValue(key, out entry)**

```csharp
if (ModelState.TryGetValue("name", out var entry))
{
    // Field exists in model state
}
```

---

## 🏷️ Validation Attributes

Validation attributes are what populate `ModelState` with errors.

### **Built-in Attributes**

#### **Required Fields**

```csharp
[Required]
public string Name { get; set; }

[Required(ErrorMessage = "Email is required")]
public string Email { get; set; }
```

**Result**: Adds error if property is `null` or empty

---

#### **String Length**

```csharp
[MaxLength(200)]
public string Name { get; set; }

[MinLength(3)]
[MaxLength(100)]
public string Username { get; set; }
```

**Result**: Adds error if string is too long or too short

---

#### **Range**

```csharp
[Range(0.01, 999999.99)]
public decimal Price { get; set; }

[Range(1, 100)]
public int Quantity { get; set; }
```

**Result**: Adds error if value is outside range

---

#### **Email**

```csharp
[EmailAddress]
public string Email { get; set; }
```

**Result**: Adds error if format is not valid email

---

#### **Phone**

```csharp
[Phone]
public string Phone { get; set; }
```

**Result**: Adds error if format is not valid phone

---

#### **Regular Expression**

```csharp
[RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Only uppercase letters and numbers")]
public string SKU { get; set; }
```

**Result**: Adds error if string doesn't match pattern

---

#### **URL**

```csharp
[Url]
public string Website { get; set; }
```

**Result**: Adds error if not valid URL

---

### **Custom Validation Attributes**

You can create your own:

```csharp
public class ValidSKUAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var sku = value as string;
        
        if (!string.IsNullOrEmpty(sku) && !sku.StartsWith("PROD-"))
        {
            return new ValidationResult("SKU must start with 'PROD-'");
        }
        
        return ValidationResult.Success;
    }
}

// Usage:
[ValidSKU]
public string SKU { get; set; }
```

---

## 🔄 Complete Validation Flow

### **1. Client Sends Request**

```json
POST /api/products
{
  "name": "",
  "price": -10
}
```

---

### **2. Model Binding**

ASP.NET Core creates `CreateProductDto` instance:

```csharp
var createDto = new CreateProductDto
{
    Name = "",  // Empty string
    Price = -10  // Negative number
};
```

---

### **3. Validation Runs**

Framework checks attributes on DTO:

```csharp
// Check [Required] on Name
if (string.IsNullOrEmpty(createDto.Name))
{
    ModelState.AddModelError("Name", "The Name field is required.");
}

// Check [Range] on Price
if (createDto.Price < 0.01 || createDto.Price > 999999)
{
    ModelState.AddModelError("Price", "Price must be between 0.01 and 999999");
}
```

---

### **4. ModelState is Populated**

```csharp
ModelState["Name"].Errors.Count = 1
ModelState["Price"].Errors.Count = 1
ModelState.IsValid = false
```

---

### **5. Controller Checks ModelState**

```csharp
if (!ModelState.IsValid)  // false
{
    return BadRequest(ModelState);
}
```

---

### **6. Response Sent to Client**

```json
HTTP 400 Bad Request
{
  "Name": ["The Name field is required."],
  "Price": ["Price must be between 0.01 and 999999"]
}
```

---

## 💡 Common Patterns

### **Pattern 1: Standard Check**

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // Process request
    var result = await _service.CreateAsync(dto);
    return Ok(result);
}
```

---

### **Pattern 2: Custom Validation**

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
{
    // Standard validation
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // Custom business logic validation
    if (dto.Price < dto.Cost)
    {
        ModelState.AddModelError("Price", "Price cannot be less than cost");
        return BadRequest(ModelState);
    }
    
    var result = await _service.CreateAsync(dto);
    return Ok(result);
}
```

---

### **Pattern 3: Retrieve Specific Errors**

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
{
    if (!ModelState.IsValid)
    {
        var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value.Errors.Select(e => e.ErrorMessage))
            .ToList();
        
        return BadRequest(new { Errors = errors });
    }
    
    // Process request
}
```

---

### **Pattern 4: Validation in Service Layer**

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    try
    {
        var result = await _service.CreateAsync(dto);
        return Ok(result);
    }
    catch (ValidationException ex)
    {
        ModelState.AddModelError("", ex.Message);
        return BadRequest(ModelState);
    }
}
```

---

## 🚀 Advanced Topics

### **Model Binding Sources**

ASP.NET Core can bind from multiple sources:

```csharp
// From request body (JSON)
[HttpPost]
public IActionResult Create([FromBody] CreateProductDto dto)

// From query string
[HttpGet]
public IActionResult Search([FromQuery] string searchTerm)

// From route parameters
[HttpGet("{id}")]
public IActionResult Get([FromRoute] int id)

// From form data
[HttpPost]
public IActionResult Submit([FromForm] CreateProductDto dto)

// From header
[HttpPost]
public IActionResult Process([FromHeader] string apiKey, [FromBody] CreateProductDto dto)
```

---

### **Custom Model Binders**

Create custom binding logic:

```csharp
public class ProductModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
        if (value.FirstValue == "special")
        {
            // Create custom product
            bindingContext.Result = ModelBindingResult.Success(new Product());
        }
        
        return Task.CompletedTask;
    }
}
```

---

### **Validation Filters**

Apply validation globally:

```csharp
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}

// Usage:
[ValidateModel]
public class ProductsController : ControllerBase
{
    // All actions automatically validate
}
```

---

### **Disabling Automatic Validation**

```csharp
[HttpPost]
public IActionResult Create([FromBody] [SkipModelValidation] CreateProductDto dto)
{
    // ModelState will not be populated
    // You handle validation manually
}
```

---

## 📚 Resources for Deeper Dive

### **Official Documentation**

1. **ASP.NET Core Model Binding**
   - URL: `https://learn.microsoft.com/aspnet/core/mvc/models/model-binding`
   - Covers: Model binding, validation, custom binders

2. **Model Validation in ASP.NET Core MVC**
   - URL: `https://learn.microsoft.com/aspnet/core/mvc/models/validation`
   - Covers: Data annotations, custom validators, client-side validation

3. **ModelState Class Reference**
   - URL: `https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.modelbinding.modelstatedictionary`
   - Covers: All methods, properties, usage

---

### **Useful Articles**

1. **"Understanding ModelState in ASP.NET Core"**
   - Covers: How ModelState works internally

2. **"Custom Validation in ASP.NET Core"**
   - Covers: Creating custom validators, complex scenarios

3. **"Model Binding Best Practices"**
   - Covers: Performance, security, patterns

---

### **Source Code (For Deep Dive)**

If you want to see how ModelState actually works internally:

**Location**: `aspnetcore/src/Mvc/Mvc.Core/src/ModelBinding/ModelStateDictionary.cs`

**Key Classes**:
- `ModelStateDictionary`
- `ModelStateEntry`
- `ModelBinder`
- `ValidationAttribute`

---

### **YouTube Tutorials**

Search for:
- "ASP.NET Core ModelState explained"
- "Model validation in ASP.NET Core"
- "Custom validation attributes"

---

## 🎯 Key Takeaways

1. **ModelState is automatic** - ASP.NET Core populates it during model binding
2. **Check ModelState.IsValid** - Standard pattern in all controllers
3. **Return BadRequest(ModelState)** - Sends validation errors to client
4. **Validation attributes drive errors** - `[Required]`, `[EmailAddress]`, etc.
5. **ModelState is in ControllerBase** - Available in all your controllers

---

## 🔗 Related Concepts

- **Data Annotations**: Attributes that define validation rules
- **Model Binding**: How data from HTTP request becomes C# objects
- **FluentValidation**: Alternative validation library (more powerful)
- **Client-Side Validation**: JavaScript validation in browser
- **Remote Validation**: Server-side validation on specific fields

---

**Summary**: `ModelState` is ASP.NET Core's built-in validation tracking system that automatically captures validation errors and lets you check if incoming data is valid before processing it. Checking `ModelState.IsValid` is a standard pattern in every controller action that accepts user input.


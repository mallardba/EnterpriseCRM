# Navigation Properties in Enterprise CRM

## Overview

Navigation properties in Entity Framework Core are special properties that represent relationships between entities. They allow you to navigate from one entity to related entities without writing complex join queries manually.

## What Are Navigation Properties?

Navigation properties are **virtual properties** that represent relationships between entities in your domain model. They provide a way to access related data through object-oriented navigation rather than writing SQL joins.

### Key Characteristics:
- **Virtual**: Can be overridden in derived classes
- **Lazy Loading**: Data is loaded on-demand (when accessed)
- **Relationship Mapping**: Represent foreign key relationships
- **Bidirectional**: Usually defined on both sides of a relationship

## Types of Navigation Properties

### 1. **Collection Navigation Properties**
Represent "one-to-many" or "many-to-many" relationships.

```csharp
public class Customer : BaseEntity
{
    // Collection navigation property - one customer has many work items
    public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    
    // Collection navigation property - one customer has many contacts
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
```

### 2. **Reference Navigation Properties**
Represent "many-to-one" or "one-to-one" relationships.

```csharp
public class WorkItem : BaseEntity
{
    // Foreign key property
    public int AssignedToUserId { get; set; }
    
    // Reference navigation property - many work items belong to one user
    public virtual User AssignedToUser { get; set; } = null!;
    
    // Optional reference navigation property
    public int? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }
}
```

## Navigation Properties in Enterprise CRM

### **Customer Entity Navigation Properties**

```csharp
public class Customer : BaseEntity
{
    // One-to-Many: One customer has many work items
    public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    
    // One-to-Many: One customer has many contacts
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    
    // One-to-Many: One customer has many opportunities
    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}
```

### **WorkItem Entity Navigation Properties**

```csharp
public class WorkItem : BaseEntity
{
    // Foreign keys
    public int AssignedToUserId { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }
    
    // Reference navigation properties
    public virtual User AssignedToUser { get; set; } = null!;        // Required
    public virtual Customer? Customer { get; set; }                  // Optional
    public virtual Lead? Lead { get; set; }                          // Optional
    public virtual Opportunity? Opportunity { get; set; }            // Optional
}
```

### **User Entity Navigation Properties**

```csharp
public class User : BaseEntity
{
    // One-to-Many: One user has many assigned work items
    public virtual ICollection<WorkItem> AssignedTasks { get; set; } = new List<WorkItem>();
    
    // One-to-Many: One user has many assigned leads
    public virtual ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
    
    // One-to-Many: One user has many assigned opportunities
    public virtual ICollection<Opportunity> AssignedOpportunities { get; set; } = new List<Opportunity>();
}
```

## How Navigation Properties Work

### **1. Lazy Loading**
Navigation properties are loaded automatically when accessed (if lazy loading is enabled).

```csharp
// This will trigger a database query to load the customer's work items
var customer = await context.Customers.FindAsync(1);
var workItems = customer.WorkItems; // Database query executed here
```

### **2. Eager Loading**
Load related data upfront using `Include()`.

```csharp
// Load customer with all work items in a single query
var customer = await context.Customers
    .Include(c => c.WorkItems)
    .Include(c => c.Contacts)
    .FirstOrDefaultAsync(c => c.Id == 1);
```

### **3. Explicit Loading**
Load related data on-demand using `Load()`.

```csharp
var customer = await context.Customers.FindAsync(1);
await context.Entry(customer)
    .Collection(c => c.WorkItems)
    .LoadAsync();
```

## Entity Framework Configuration

### **Fluent API Configuration**

```csharp
// In ApplicationDbContext.OnModelCreating()
modelBuilder.Entity<WorkItem>(entity =>
{
    // Configure relationship with User
    entity.HasOne(e => e.AssignedToUser)
          .WithMany(u => u.AssignedTasks)
          .HasForeignKey(e => e.AssignedToUserId)
          .OnDelete(DeleteBehavior.Cascade);
    
    // Configure relationship with Customer
    entity.HasOne(e => e.Customer)
          .WithMany(c => c.WorkItems)
          .HasForeignKey(e => e.CustomerId)
          .OnDelete(DeleteBehavior.SetNull);
});
```

### **Relationship Types**

| Relationship | Navigation Properties | Example |
|-------------|----------------------|---------|
| **One-to-Many** | Collection + Reference | Customer → WorkItems |
| **Many-to-One** | Reference + Collection | WorkItem → Customer |
| **One-to-One** | Reference + Reference | User → Profile |
| **Many-to-Many** | Collection + Collection | User → Roles |

## Practical Usage Examples

### **1. Repository Pattern Usage**

```csharp
public class WorkItemRepository : Repository<WorkItem>, IWorkItemRepository
{
    public async Task<IEnumerable<WorkItem>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(w => w.CustomerId == customerId)
            .Include(w => w.AssignedToUser)  // Eager load user info
            .Include(w => w.Customer)        // Eager load customer info
            .ToListAsync();
    }
}
```

### **2. Service Layer Usage**

```csharp
public class WorkItemService : IWorkItemService
{
    public async Task<WorkItemDto> CreateAsync(CreateWorkItemDto dto, string currentUser)
    {
        var workItem = new WorkItem
        {
            Title = dto.Title,
            AssignedToUserId = dto.AssignedToUserId,
            CustomerId = dto.CustomerId
        };
        
        var createdWorkItem = await _unitOfWork.WorkItems.AddAsync(workItem);
        await _unitOfWork.SaveChangesAsync();
        
        // Navigation properties are available after save
        var assignedUser = createdWorkItem.AssignedToUser; // User entity
        var customer = createdWorkItem.Customer;           // Customer entity
        
        return _mapper.Map<WorkItemDto>(createdWorkItem);
    }
}
```

### **3. Controller Usage**

```csharp
[ApiController]
[Route("api/[controller]")]
public class WorkItemsController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkItemDto>> GetById(int id)
    {
        var workItem = await _workItemService.GetByIdAsync(id);
        if (workItem == null) return NotFound();
        
        // Navigation properties are accessible through the DTO
        return Ok(workItem);
    }
}
```

## Benefits of Navigation Properties

### **1. Object-Oriented Navigation**
```csharp
// Instead of writing complex joins
var workItems = await context.WorkItems
    .Join(context.Users, w => w.AssignedToUserId, u => u.Id, (w, u) => new { w, u })
    .Where(x => x.u.Department == "Sales")
    .Select(x => x.w)
    .ToListAsync();

// Use navigation properties
var workItems = await context.WorkItems
    .Where(w => w.AssignedToUser.Department == "Sales")
    .ToListAsync();
```

### **2. IntelliSense Support**
Navigation properties provide full IntelliSense support in your IDE.

### **3. Type Safety**
Compile-time checking ensures you're accessing valid relationships.

### **4. Automatic Relationship Management**
EF Core automatically manages foreign key values when you set navigation properties.

## Best Practices

### **1. Always Initialize Collections**
```csharp
public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
```

### **2. Use Virtual for Lazy Loading**
```csharp
public virtual User AssignedToUser { get; set; } = null!;
```

### **3. Handle Nullable References**
```csharp
public virtual Customer? Customer { get; set; }  // Optional relationship
public virtual User AssignedToUser { get; set; } = null!;  // Required relationship
```

### **4. Configure Relationships Explicitly**
Always configure relationships in `OnModelCreating()` for clarity and control.

### **5. Use Include() for Performance**
```csharp
// Good: Load related data upfront
var customers = await context.Customers
    .Include(c => c.WorkItems)
    .ToListAsync();

// Avoid: N+1 query problem
var customers = await context.Customers.ToListAsync();
foreach (var customer in customers)
{
    var workItems = customer.WorkItems; // Separate query for each customer
}
```

## Common Patterns in Enterprise CRM

### **1. Hierarchical Navigation**
```csharp
// Navigate through the hierarchy
var workItem = await context.WorkItems
    .Include(w => w.Customer)
    .ThenInclude(c => c.Contacts)
    .FirstOrDefaultAsync(w => w.Id == 1);

// Access: workItem.Customer.Contacts
```

### **2. Bidirectional Navigation**
```csharp
// From Customer to WorkItems
var customer = await context.Customers.FindAsync(1);
var workItems = customer.WorkItems;

// From WorkItem to Customer
var workItem = await context.WorkItems.FindAsync(1);
var customer = workItem.Customer;
```

### **3. Conditional Navigation**
```csharp
// Only load if needed
if (workItem.CustomerId.HasValue)
{
    var customer = workItem.Customer; // Loads customer data
}
```

## Performance Considerations

### **1. Lazy Loading Overhead**
- Each navigation property access triggers a database query
- Can lead to N+1 query problems
- Use eager loading (`Include()`) for better performance

### **2. Memory Usage**
- Navigation properties can load large amounts of data
- Use projection (`Select()`) to load only needed fields

### **3. Circular References**
- Be careful with serialization (JSON) to avoid circular references
- Use DTOs to break circular references

## Summary

Navigation properties are a powerful feature of Entity Framework Core that provide:

- **Object-oriented data access**
- **Type-safe relationships**
- **Automatic foreign key management**
- **IntelliSense support**
- **Flexible loading strategies**

In the Enterprise CRM project, navigation properties enable:
- **Customer → WorkItems** relationships
- **User → AssignedTasks** relationships
- **WorkItem → Customer/Lead/Opportunity** relationships
- **Bidirectional navigation** throughout the domain model

They make the code more readable, maintainable, and aligned with object-oriented principles while providing powerful querying capabilities.

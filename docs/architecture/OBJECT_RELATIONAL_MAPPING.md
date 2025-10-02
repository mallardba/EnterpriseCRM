# Object-Relational Mapping (ORM)

## 🗄️ **Overview**

**ORM (Object-Relational Mapping)** is a programming technique that allows you to work with databases using object-oriented programming concepts instead of writing raw SQL queries. In the Enterprise CRM project, we use **Entity Framework Core** as our ORM solution.

## 🎯 **What Problem Does ORM Solve?**

### **Without ORM (Raw SQL):**
```csharp
// Manual SQL queries - tedious and error-prone
var connection = new SqlConnection(connectionString);
var command = new SqlCommand("SELECT * FROM Customers WHERE Id = @id", connection);
command.Parameters.AddWithValue("@id", customerId);

var reader = command.ExecuteReader();
var customer = new Customer();
if (reader.Read())
{
    customer.Id = reader.GetInt32("Id");
    customer.Name = reader.GetString("Name");
    customer.Email = reader.GetString("Email");
    // ... map each field manually
}
```

### **With ORM (Entity Framework):**
```csharp
// Clean, object-oriented approach
var customer = await _context.Customers
    .FirstOrDefaultAsync(c => c.Id == customerId);
```

## 🏗️ **How ORM Works**

### **The Mapping Process:**
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Database      │    │      ORM        │    │   C# Objects   │
│                 │    │                 │    │                 │
│ • Tables        │◀──▶│ • Mapping       │◀──▶│ • Classes       │
│ • Rows          │    │ • Translation   │    │ • Properties    │
│ • Columns       │    │ • Conversion    │    │ • Methods       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### **Example Mapping:**
```sql
-- Database Table
CREATE TABLE Customers (
    Id INT PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(255),
    CreatedAt DATETIME
)
```

```csharp
// C# Entity Class
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## 🔧 **Entity Framework Core in Your Project**

### **ApplicationDbContext**
Your project uses **Entity Framework Core** through the `ApplicationDbContext`:

```csharp
// Your ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<EnterpriseCRM.Core.Entities.Task> Tasks { get; set; }
}
```

### **Entity Mapping Configuration**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Customer entity configuration
    modelBuilder.Entity<Customer>(entity =>
    {
        entity.Property(e => e.CompanyName)
              .HasMaxLength(200)
              .IsRequired();
              
        entity.Property(e => e.Email)
              .HasMaxLength(255)
              .IsRequired();
              
        entity.Property(e => e.Phone)
              .HasMaxLength(20);
    });
    
    // Opportunity entity configuration
    modelBuilder.Entity<Opportunity>(entity =>
    {
        entity.Property(e => e.Amount)
              .HasColumnType("decimal(18,2)")
              .HasPrecision(18, 2);
              
        entity.Property(e => e.Probability)
              .HasColumnType("decimal(5,2)")
              .HasPrecision(5, 2);
    });
}
```

## 🚀 **ORM Benefits**

### **1. Productivity**
- **Less Boilerplate**: No need to write repetitive SQL code
- **Type Safety**: Compile-time checking of database operations
- **IntelliSense**: IDE support for database operations
- **Rapid Development**: Quick prototyping and feature development

### **2. Maintainability**
- **Centralized Mapping**: All database mappings in one place
- **Easy Changes**: Modify entities and let ORM handle database changes
- **Consistent Patterns**: Standardized way to work with data
- **Code-First Approach**: Database schema defined in code

### **3. Database Agnostic**
- **Multiple Databases**: Same code works with SQL Server, PostgreSQL, MySQL, etc.
- **Easy Migration**: Switch databases without changing application code
- **Provider Abstraction**: Database-specific features abstracted away

### **4. Advanced Features**
- **Change Tracking**: Automatic detection of entity changes
- **Lazy Loading**: Load related data on demand
- **Eager Loading**: Load related data upfront
- **Query Translation**: LINQ queries translated to SQL

## 🔍 **ORM Operations in Your Project**

### **1. Querying (Reading Data)**
```csharp
// Get all customers
var customers = await _context.Customers.ToListAsync();

// Get customer by ID
var customer = await _context.Customers
    .FirstOrDefaultAsync(c => c.Id == id);

// Complex query with joins
var customersWithOpportunities = await _context.Customers
    .Include(c => c.Opportunities)
    .Where(c => c.Opportunities.Any())
    .ToListAsync();

// Paginated query
var pagedCustomers = await _context.Customers
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### **2. Creating Data**
```csharp
// Add new customer
var newCustomer = new Customer
{
    CompanyName = "Acme Corp",
    Email = "contact@acme.com",
    CreatedAt = DateTime.UtcNow,
    CreatedBy = "System"
};

_context.Customers.Add(newCustomer);
await _context.SaveChangesAsync();
```

### **3. Updating Data**
```csharp
// Update existing customer
var customer = await _context.Customers.FindAsync(id);
if (customer != null)
{
    customer.CompanyName = "Updated Name";
    customer.UpdatedAt = DateTime.UtcNow;
    customer.UpdatedBy = "CurrentUser";
    await _context.SaveChangesAsync();
}
```

### **4. Deleting Data**
```csharp
// Soft delete (recommended)
var customer = await _context.Customers.FindAsync(id);
if (customer != null)
{
    customer.IsDeleted = true;
    customer.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
}

// Hard delete
var customer = await _context.Customers.FindAsync(id);
if (customer != null)
{
    _context.Customers.Remove(customer);
    await _context.SaveChangesAsync();
}
```

## 🎯 **ORM Patterns in Your Project**

### **1. Repository Pattern**
```csharp
// Your repository uses EF Core internally
public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;
    
    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Customer> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }
    
    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Where(c => !c.IsDeleted)
            .ToListAsync();
    }
}
```

### **2. Unit of Work Pattern**
```csharp
// Your UnitOfWork manages the DbContext
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Customers = new CustomerRepository(_context);
        Opportunities = new OpportunityRepository(_context);
        // ... other repositories
    }
    
    public ICustomerRepository Customers { get; }
    public IOpportunityRepository Opportunities { get; }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### **3. Entity Configuration**
```csharp
// Your DbContext configures entity mappings
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Base entity configuration
    modelBuilder.Entity<BaseEntity>(entity =>
    {
        entity.Property(e => e.CreatedAt)
              .HasDefaultValueSql("GETUTCDATE()");
              
        entity.Property(e => e.CreatedBy)
              .HasMaxLength(100)
              .IsRequired();
              
        entity.HasIndex(e => e.IsDeleted);
    });
    
    // Customer-specific configuration
    modelBuilder.Entity<Customer>(entity =>
    {
        entity.Property(e => e.CompanyName)
              .HasMaxLength(200)
              .IsRequired();
              
        entity.Property(e => e.Email)
              .HasMaxLength(255)
              .IsRequired();
              
        entity.HasIndex(e => e.Email)
              .IsUnique();
    });
}
```

## 🔧 **ORM Features You're Using**

### **1. Migrations**
```bash
# Create migration
dotnet ef migrations add InitialCreate --startup-project ../EnterpriseCRM.WebAPI

# Apply migration
dotnet ef database update --startup-project ../EnterpriseCRM.WebAPI

# List migrations
dotnet ef migrations list --startup-project ../EnterpriseCRM.WebAPI
```

### **2. Code-First Approach**
- Define entities in C# code
- EF Core generates database schema
- Migrations track schema changes
- Version-controlled database evolution

### **3. LINQ Integration**
```csharp
// LINQ queries that get translated to SQL
var activeCustomers = await _context.Customers
    .Where(c => c.Status == CustomerStatus.Active)
    .OrderBy(c => c.CompanyName)
    .ToListAsync();

// Complex LINQ with multiple conditions
var recentOpportunities = await _context.Opportunities
    .Where(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    .Where(o => o.Amount > 10000)
    .Include(o => o.Customer)
    .ToListAsync();
```

### **4. Change Tracking**
```csharp
// EF Core automatically tracks changes
var customer = await _context.Customers.FindAsync(1);
customer.Email = "new@email.com";  // EF Core knows this changed
customer.UpdatedAt = DateTime.UtcNow; // This too
await _context.SaveChangesAsync(); // Only updates changed fields
```

### **5. Soft Delete Implementation**
```csharp
// Global query filter for soft delete
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Customer>()
        .HasQueryFilter(c => !c.IsDeleted);
        
    modelBuilder.Entity<Opportunity>()
        .HasQueryFilter(o => !o.IsDeleted);
}

// Usage - automatically filters out deleted records
var customers = await _context.Customers.ToListAsync(); // Only active customers
```

## ⚠️ **ORM Challenges**

### **1. Performance Issues**
- **N+1 Queries**: Loading related data inefficiently
- **Over-fetching**: Loading more data than needed
- **Lazy Loading**: Unexpected database calls
- **Query Complexity**: Some queries generate inefficient SQL

### **2. Complex Queries**
- **SQL Limitations**: Some complex queries are hard to express in LINQ
- **Performance**: Generated SQL might not be optimal
- **Database-Specific Features**: Hard to use database-specific functionality

### **3. Learning Curve**
- **Abstraction**: Need to understand how ORM translates to SQL
- **Debugging**: Harder to debug when things go wrong
- **Migration Management**: Understanding migration lifecycle

## 🎯 **ORM Best Practices**

### **1. Use Projections**
```csharp
// Good: Only select needed fields
var customerNames = await _context.Customers
    .Select(c => new { c.Id, c.CompanyName })
    .ToListAsync();

// Bad: Load entire entities when you only need names
var customers = await _context.Customers.ToListAsync();
var names = customers.Select(c => c.CompanyName);
```

### **2. Use Include Wisely**
```csharp
// Good: Explicitly include related data
var customers = await _context.Customers
    .Include(c => c.Opportunities)
    .ThenInclude(o => o.Products)
    .ToListAsync();

// Bad: Lazy loading can cause N+1 queries
var customers = await _context.Customers.ToListAsync();
foreach (var customer in customers)
{
    var opportunities = customer.Opportunities; // Database call!
}
```

### **3. Use Async Methods**
```csharp
// Good: Non-blocking database calls
var customers = await _context.Customers.ToListAsync();

// Bad: Blocking database calls
var customers = _context.Customers.ToList();
```

### **4. Implement Proper Disposal**
```csharp
// Good: Proper disposal pattern
public class CustomerService : IDisposable
{
    private readonly ApplicationDbContext _context;
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}

// Or use dependency injection with scoped lifetime
builder.Services.AddScoped<ApplicationDbContext>();
```

## 🔍 **ORM vs Raw SQL**

### **When to Use ORM:**
- **CRUD Operations**: Simple create, read, update, delete
- **Rapid Development**: Quick prototyping and development
- **Type Safety**: When you need compile-time checking
- **Database Portability**: When you might change databases
- **Team Development**: When multiple developers work on the project

### **When to Use Raw SQL:**
- **Complex Queries**: Reports, analytics, complex joins
- **Performance Critical**: When you need optimized SQL
- **Database-Specific Features**: Using database-specific functionality
- **Legacy Systems**: Working with existing stored procedures
- **Bulk Operations**: Large data imports/exports

### **Hybrid Approach:**
```csharp
// Use ORM for simple operations
var customer = await _context.Customers.FindAsync(id);

// Use raw SQL for complex queries
var reportData = await _context.Database
    .SqlQueryRaw<ReportDto>(@"
        SELECT c.CompanyName, COUNT(o.Id) as OpportunityCount
        FROM Customers c
        LEFT JOIN Opportunities o ON c.Id = o.CustomerId
        WHERE c.CreatedAt >= @startDate
        GROUP BY c.Id, c.CompanyName
        ORDER BY OpportunityCount DESC", 
        new SqlParameter("@startDate", startDate))
    .ToListAsync();
```

## 🚀 **ORM in Your CRM Architecture**

### **Data Flow:**
```
Controller → Service → Repository → EF Core → Database
     ↓         ↓         ↓          ↓
   HTTP     Business   Data      ORM
  Request    Logic    Access   Mapping
```

### **Your Implementation:**
```csharp
// Controller uses service
[HttpGet("{id}")]
public async Task<ActionResult<CustomerDto>> GetById(int id)
{
    var customer = await _customerService.GetByIdAsync(id);
    return Ok(customer);
}

// Service uses repository
public async Task<CustomerDto> GetByIdAsync(int id)
{
    var customer = await _customerRepository.GetByIdAsync(id);
    return _mapper.Map<CustomerDto>(customer);
}

// Repository uses EF Core
public async Task<Customer> GetByIdAsync(int id)
{
    return await _context.Customers.FindAsync(id);
}
```

## 📊 **ORM Performance Considerations**

### **1. Query Optimization**
```csharp
// Good: Use projections to limit data
var customerSummary = await _context.Customers
    .Select(c => new CustomerSummaryDto
    {
        Id = c.Id,
        CompanyName = c.CompanyName,
        Email = c.Email
    })
    .ToListAsync();

// Bad: Loading full entities when only summary needed
var customers = await _context.Customers.ToListAsync();
var summary = customers.Select(c => new CustomerSummaryDto { ... });
```

### **2. Pagination**
```csharp
// Good: Database-level pagination
var pagedCustomers = await _context.Customers
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

// Bad: Memory-level pagination
var allCustomers = await _context.Customers.ToListAsync();
var paged = allCustomers.Skip((pageNumber - 1) * pageSize).Take(pageSize);
```

### **3. Bulk Operations**
```csharp
// Good: Use AddRange for bulk inserts
var customers = new List<Customer> { /* many customers */ };
_context.Customers.AddRange(customers);
await _context.SaveChangesAsync();

// Bad: Individual inserts in loop
foreach (var customer in customers)
{
    _context.Customers.Add(customer);
    await _context.SaveChangesAsync(); // Database round-trip each time
}
```

## 🔧 **ORM Configuration in Your Project**

### **Connection String Configuration**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EnterpriseCRM;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### **DbContext Registration**
```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### **Entity Configuration**
```csharp
// ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configure all entities
    ConfigureCustomerEntity(modelBuilder);
    ConfigureOpportunityEntity(modelBuilder);
    ConfigureLeadEntity(modelBuilder);
    // ... other entities
}

private void ConfigureCustomerEntity(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Customer>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
        entity.HasIndex(e => e.Email).IsUnique();
    });
}
```

## 💡 **Key Takeaways**

1. **ORM maps database tables to C# objects** automatically
2. **Entity Framework Core** is your ORM in this project
3. **Code-First approach** lets you define entities in C# code
4. **Migrations** track and apply database schema changes
5. **LINQ integration** allows querying with C# syntax
6. **Repository pattern** abstracts ORM complexity from business logic
7. **Performance considerations** are important for complex queries
8. **Soft delete** is implemented using global query filters
9. **Change tracking** automatically detects entity modifications
10. **Async operations** prevent blocking database calls

## 🎯 **Your Project's ORM Benefits**

- ✅ **Type Safety**: Compile-time checking of database operations
- ✅ **Productivity**: Less boilerplate SQL code
- ✅ **Maintainability**: Centralized entity definitions
- ✅ **Database Agnostic**: Easy to switch databases
- ✅ **Migration Support**: Version-controlled database changes
- ✅ **LINQ Integration**: Query with familiar C# syntax
- ✅ **Change Tracking**: Automatic detection of modifications
- ✅ **Soft Delete**: Built-in support for logical deletion
- ✅ **Repository Pattern**: Clean separation of data access logic

ORM makes your Enterprise CRM project more maintainable, type-safe, and productive by abstracting away the complexity of database operations while providing powerful features for data management! 🎉

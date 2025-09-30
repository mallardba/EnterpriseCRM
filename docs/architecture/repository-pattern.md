# Enterprise CRM - Repository Pattern & Unit of Work

## 🗄️ Repository Pattern Overview

### **What is the Repository Pattern?**

The Repository Pattern is a design pattern that abstracts data access logic and provides a uniform interface for accessing data. It acts as an in-memory collection of domain objects, making the data access layer more testable and maintainable.

### **Why Use the Repository Pattern?**

**Abstraction:**
- Hides the complexity of data access
- Provides a simple interface for data operations
- Decouples business logic from data access

**Testability:**
- Easy to mock repositories for unit testing
- Isolated testing of business logic
- No dependency on actual database

**Maintainability:**
- Centralized data access logic
- Easy to change data access implementation
- Consistent data access patterns

**Flexibility:**
- Database-agnostic design
- Easy to switch between different data sources
- Support for multiple data access strategies

### **Repository Pattern Benefits**

- **Single Responsibility**: Each repository handles one entity type
- **Consistent Interface**: Standardized data access methods
- **Easy Testing**: Mockable dependencies
- **Performance**: Optimized queries for specific use cases
- **Caching**: Built-in caching capabilities

## 🔄 Unit of Work Pattern

### **What is the Unit of Work Pattern?**

The Unit of Work pattern maintains a list of objects affected by a business transaction and coordinates writing out changes and resolving concurrency problems.

### **Why Use Unit of Work?**

**Transaction Management:**
- Ensures data consistency
- Handles rollback scenarios
- Manages database transactions

**Performance:**
- Batches multiple operations
- Reduces database round trips
- Optimizes database access

**Consistency:**
- Maintains data integrity
- Handles concurrency issues
- Ensures atomic operations

### **Unit of Work Benefits**

- **Transaction Control**: Manages database transactions
- **Change Tracking**: Tracks all changes in a transaction
- **Rollback Support**: Easy rollback of failed operations
- **Performance**: Batches operations for efficiency
- **Consistency**: Ensures data integrity across operations

## 🎯 Core Concepts

### **1. Repository Interface**
Defines the contract for data access operations

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
}
```

### **2. Generic Repository Implementation**
Base implementation for common operations

```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet
            .AnyAsync(e => e.Id == id && !e.IsDeleted);
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet
            .CountAsync(e => !e.IsDeleted);
    }
}
```

## 🏗️ Specific Repository Implementations

### **Customer Repository**
```csharp
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer> GetByEmailAsync(string email);
    Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status);
    Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType type);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<PagedResult<Customer>> GetPagedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<Customer>> SearchAsync(string searchTerm);
}

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Customer> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted);
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status)
    {
        return await _dbSet
            .Where(c => c.Status == status && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType type)
    {
        return await _dbSet
            .Where(c => c.Type == type && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _dbSet
            .AnyAsync(c => c.Email == email && !c.IsDeleted);
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _dbSet.Where(c => !c.IsDeleted);
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(c => c.CompanyName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Customer>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && 
                       (c.CompanyName.Contains(searchTerm) ||
                        c.Email.Contains(searchTerm) ||
                        c.FirstName.Contains(searchTerm) ||
                        c.LastName.Contains(searchTerm)))
            .ToListAsync();
    }
}
```

### **Lead Repository**
```csharp
public interface ILeadRepository : IRepository<Lead>
{
    Task<IEnumerable<Lead>> GetByStatusAsync(LeadStatus status);
    Task<IEnumerable<Lead>> GetBySourceAsync(LeadSource source);
    Task<IEnumerable<Lead>> GetByAssignedUserAsync(int userId);
    Task<IEnumerable<Lead>> GetRecentAsync(int count);
    Task<decimal> GetTotalEstimatedValueAsync();
}

public class LeadRepository : Repository<Lead>, ILeadRepository
{
    public LeadRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Lead>> GetByStatusAsync(LeadStatus status)
    {
        return await _dbSet
            .Where(l => l.Status == status && !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetBySourceAsync(LeadSource source)
    {
        return await _dbSet
            .Where(l => l.Source == source && !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetByAssignedUserAsync(int userId)
    {
        return await _dbSet
            .Where(l => l.AssignedToUserId == userId && !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetRecentAsync(int count)
    {
        return await _dbSet
            .Where(l => !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalEstimatedValueAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted)
            .SumAsync(l => l.EstimatedValue);
    }
}
```

## 🔄 Unit of Work Pattern

### **Purpose**
The Unit of Work pattern maintains a list of objects affected by a business transaction and coordinates writing out changes and resolving concurrency problems.

### **Unit of Work Interface**
```csharp
public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    ILeadRepository Leads { get; }
    IOpportunityRepository Opportunities { get; }
    ITaskRepository Tasks { get; }
    IUserRepository Users { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### **Unit of Work Implementation**
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;
    
    private ICustomerRepository _customers;
    private ILeadRepository _leads;
    private IOpportunityRepository _opportunities;
    private ITaskRepository _tasks;
    private IUserRepository _users;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ICustomerRepository Customers => 
        _customers ??= new CustomerRepository(_context);
    
    public ILeadRepository Leads => 
        _leads ??= new LeadRepository(_context);
    
    public IOpportunityRepository Opportunities => 
        _opportunities ??= new OpportunityRepository(_context);
    
    public ITaskRepository Tasks => 
        _tasks ??= new TaskRepository(_context);
    
    public IUserRepository Users => 
        _users ??= new UserRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}
```

## 🎯 Advanced Repository Patterns

### **Specification Pattern**
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>> OrderBy { get; }
    Expression<Func<T, object>> OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}

public class CustomerSpecification : ISpecification<Customer>
{
    public Expression<Func<Customer, bool>> Criteria { get; }
    public List<Expression<Func<Customer, object>>> Includes { get; }
    public List<string> IncludeStrings { get; }
    public Expression<Func<Customer, object>> OrderBy { get; }
    public Expression<Func<Customer, object>> OrderByDescending { get; }
    public int Take { get; }
    public int Skip { get; }
    public bool IsPagingEnabled { get; }

    public CustomerSpecification(
        Expression<Func<Customer, bool>> criteria = null,
        List<Expression<Func<Customer, object>>> includes = null,
        List<string> includeStrings = null,
        Expression<Func<Customer, object>> orderBy = null,
        Expression<Func<Customer, object>> orderByDescending = null,
        int take = 0,
        int skip = 0,
        bool isPagingEnabled = false)
    {
        Criteria = criteria;
        Includes = includes ?? new List<Expression<Func<Customer, object>>>();
        IncludeStrings = includeStrings ?? new List<string>();
        OrderBy = orderBy;
        OrderByDescending = orderByDescending;
        Take = take;
        Skip = skip;
        IsPagingEnabled = isPagingEnabled;
    }
}

public class SpecificationRepository<T> : Repository<T> where T : BaseEntity
{
    public SpecificationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<T>> GetAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    public async Task<T> GetSingleAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        var query = _dbSet.Where(e => !e.IsDeleted);

        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }

        if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        return query;
    }
}
```

### **Caching Repository**
```csharp
public class CachedCustomerRepository : ICustomerRepository
{
    private readonly ICustomerRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public CachedCustomerRepository(ICustomerRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Customer> GetByIdAsync(int id)
    {
        var cacheKey = $"customer_{id}";
        
        if (_cache.TryGetValue(cacheKey, out Customer cachedCustomer))
        {
            return cachedCustomer;
        }

        var customer = await _repository.GetByIdAsync(id);
        if (customer != null)
        {
            _cache.Set(cacheKey, customer, _cacheExpiration);
        }

        return customer;
    }

    public async Task<Customer> AddAsync(Customer entity)
    {
        var result = await _repository.AddAsync(entity);
        
        // Invalidate related cache entries
        _cache.Remove($"customer_{result.Id}");
        
        return result;
    }
}
```

## 🧪 Testing Repository Pattern

### **Unit Testing with Mocks**
```csharp
public class CustomerServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        
        _customerService = new CustomerService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerExists_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer { Id = customerId, CompanyName = "Test Company" };
        
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.CompanyName.Should().Be("Test Company");
    }
}
```

### **Integration Testing with In-Memory Database**
```csharp
public class CustomerRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ICustomerRepository _repository;

    public CustomerRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CustomerRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddCustomerToDatabase()
    {
        // Arrange
        var customer = new Customer
        {
            CompanyName = "Test Company",
            Email = "test@company.com",
            Type = CustomerType.Company
        };

        // Act
        await _repository.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Assert
        var savedCustomer = await _context.Customers.FirstOrDefaultAsync();
        savedCustomer.Should().NotBeNull();
        savedCustomer.CompanyName.Should().Be("Test Company");
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

## 🚀 Benefits of Repository Pattern

### **Testability**
- Easy to mock repositories for unit testing
- Isolated data access logic
- Independent testing of business logic

### **Maintainability**
- Centralized data access logic
- Easy to change data access implementation
- Consistent data access patterns

### **Flexibility**
- Database-agnostic design
- Easy to switch between different data sources
- Support for multiple data access strategies

### **Performance**
- Optimized queries for specific use cases
- Caching capabilities
- Lazy loading support

## 📊 Best Practices

### **1. Keep Repositories Focused**
- One repository per aggregate root
- Specific methods for business use cases
- Avoid generic "catch-all" methods

### **2. Use Async/Await**
```csharp
public async Task<Customer> GetByIdAsync(int id)
{
    return await _dbSet
        .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
}
```

### **3. Implement Soft Delete**
```csharp
public virtual async Task DeleteAsync(int id)
{
    var entity = await GetByIdAsync(id);
    if (entity != null)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
```

### **4. Use Transactions for Complex Operations**
```csharp
public async Task<Customer> CreateCustomerWithContactsAsync(Customer customer, List<Contact> contacts)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        await _customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        
        foreach (var contact in contacts)
        {
            contact.CustomerId = customer.Id;
            await _contacts.AddAsync(contact);
        }
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return customer;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

The Repository Pattern and Unit of Work provide a robust foundation for data access in the Enterprise CRM system, ensuring maintainable, testable, and flexible data management.

using EnterpriseCRM.Core.Entities;

namespace EnterpriseCRM.Core.Interfaces;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
}

/// <summary>
/// Customer repository interface with specific operations
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    Task<IEnumerable<Customer>> SearchAsync(string searchTerm);
    Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status);
    Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType type);
    Task<Customer?> GetByEmailAsync(string email);
    Task<IEnumerable<Customer>> GetRecentAsync(int count);
}

/// <summary>
/// Contact repository interface with specific operations
/// </summary>
public interface IContactRepository : IRepository<Contact>
{
    Task<IEnumerable<Contact>> GetByCustomerIdAsync(int customerId);
    Task<Contact?> GetPrimaryContactAsync(int customerId);
    Task<IEnumerable<Contact>> GetByRoleAsync(ContactRole role);
}

/// <summary>
/// Lead repository interface with specific operations
/// </summary>
public interface ILeadRepository : IRepository<Lead>
{
    Task<IEnumerable<Lead>> GetByStatusAsync(LeadStatus status);
    Task<IEnumerable<Lead>> GetBySourceAsync(LeadSource source);
    Task<IEnumerable<Lead>> GetByAssignedUserAsync(int userId);
    Task<IEnumerable<Lead>> GetRecentAsync(int count);
    Task<IEnumerable<Lead>> SearchAsync(string searchTerm);
}

/// <summary>
/// Opportunity repository interface with specific operations
/// </summary>
public interface IOpportunityRepository : IRepository<Opportunity>
{
    Task<IEnumerable<Opportunity>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Opportunity>> GetByStageAsync(OpportunityStage stage);
    Task<IEnumerable<Opportunity>> GetByAssignedUserAsync(int userId);
    Task<IEnumerable<Opportunity>> GetByStatusAsync(OpportunityStatus status);
    Task<decimal> GetTotalPipelineValueAsync();
    Task<decimal> GetForecastedRevenueAsync();
}

/// <summary>
/// Task repository interface with specific operations
/// </summary>
public interface ITaskRepository : IRepository<Task>
{
    Task<IEnumerable<Task>> GetByAssignedUserAsync(int userId);
    Task<IEnumerable<Task>> GetByStatusAsync(TaskStatus status);
    Task<IEnumerable<Task>> GetOverdueAsync();
    Task<IEnumerable<Task>> GetDueTodayAsync();
    Task<IEnumerable<Task>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Task>> GetByLeadIdAsync(int leadId);
    Task<IEnumerable<Task>> GetByOpportunityIdAsync(int opportunityId);
}

/// <summary>
/// User repository interface with specific operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> ValidateCredentialsAsync(string username, string password);
}

/// <summary>
/// Unit of Work interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IContactRepository Contacts { get; }
    ILeadRepository Leads { get; }
    IOpportunityRepository Opportunities { get; }
    ITaskRepository Tasks { get; }
    IUserRepository Users { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

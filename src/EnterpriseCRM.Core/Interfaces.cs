using EnterpriseCRM.Core.Entities;

namespace EnterpriseCRM.Core.Interfaces;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    System.Threading.Tasks.Task<T?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<T>> GetAllAsync();
    System.Threading.Tasks.Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
    System.Threading.Tasks.Task<T> AddAsync(T entity);
    System.Threading.Tasks.Task UpdateAsync(T entity);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<bool> ExistsAsync(int id);
    System.Threading.Tasks.Task<int> CountAsync();
}

/// <summary>
/// Customer repository interface with specific operations
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    System.Threading.Tasks.Task<IEnumerable<Customer>> SearchAsync(string searchTerm);
    System.Threading.Tasks.Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status);
    System.Threading.Tasks.Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType type);
    System.Threading.Tasks.Task<Customer?> GetByEmailAsync(string email);
    System.Threading.Tasks.Task<IEnumerable<Customer>> GetRecentAsync(int count);
}

/// <summary>
/// Contact repository interface with specific operations
/// </summary>
public interface IContactRepository : IRepository<Contact>
{
    System.Threading.Tasks.Task<IEnumerable<Contact>> GetByCustomerIdAsync(int customerId);
    System.Threading.Tasks.Task<Contact?> GetPrimaryContactAsync(int customerId);
    System.Threading.Tasks.Task<IEnumerable<Contact>> GetByRoleAsync(ContactRole role);
}

/// <summary>
/// Lead repository interface with specific operations
/// </summary>
public interface ILeadRepository : IRepository<Lead>
{
    System.Threading.Tasks.Task<IEnumerable<Lead>> GetByStatusAsync(LeadStatus status);
    System.Threading.Tasks.Task<IEnumerable<Lead>> GetBySourceAsync(LeadSource source);
    System.Threading.Tasks.Task<IEnumerable<Lead>> GetByAssignedUserAsync(int userId);
    System.Threading.Tasks.Task<IEnumerable<Lead>> GetRecentAsync(int count);
    System.Threading.Tasks.Task<IEnumerable<Lead>> SearchAsync(string searchTerm);
}

/// <summary>
/// Opportunity repository interface with specific operations
/// </summary>
public interface IOpportunityRepository : IRepository<Opportunity>
{
    System.Threading.Tasks.Task<IEnumerable<Opportunity>> GetByCustomerIdAsync(int customerId);
    System.Threading.Tasks.Task<IEnumerable<Opportunity>> GetByStageAsync(OpportunityStage stage);
    System.Threading.Tasks.Task<IEnumerable<Opportunity>> GetByAssignedUserAsync(int userId);
    System.Threading.Tasks.Task<IEnumerable<Opportunity>> GetByStatusAsync(OpportunityStatus status);
    System.Threading.Tasks.Task<decimal> GetTotalPipelineValueAsync();
    System.Threading.Tasks.Task<decimal> GetForecastedRevenueAsync();
}

/// <summary>
/// Task repository interface with specific operations
/// </summary>
public interface ITaskRepository : IRepository<EnterpriseCRM.Core.Entities.Task>
{
    System.Threading.Tasks.Task<IEnumerable<EnterpriseCRM.Core.Entities.Task>> GetByAssignedUserAsync(int userId);
    System.Threading.Tasks.Task<IEnumerable<EnterpriseCRM.Core.Entities.Task>> GetByStatusAsync(EnterpriseCRM.Core.Entities.TaskStatus status);
    System.Threading.Tasks.Task<IEnumerable<EnterpriseCRM.Core.Entities.Task>> GetOverdueAsync();
    System.Threading.Tasks.Task<IEnumerable<EnterpriseCRM.Core.Entities.Task>> GetDueTodayAsync();
    System.Threading.Tasks.Task<IEnumerable<EnterpriseCRM.Core.Entities.Task>> GetByCustomerIdAsync(int customerId);
    System.Threading.Tasks.Task<IEnumerable<EnterpriseCRM.Core.Entities.Task>> GetByLeadIdAsync(int leadId);
    System.Threading.Tasks.Task<IEnumerable<EnterpriseCRM.Core.Entities.Task>> GetByOpportunityIdAsync(int opportunityId);
}

/// <summary>
/// User repository interface with specific operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    System.Threading.Tasks.Task<User?> GetByEmailAsync(string email);
    System.Threading.Tasks.Task<User?> GetByUsernameAsync(string username);
    System.Threading.Tasks.Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
    System.Threading.Tasks.Task<IEnumerable<User>> GetActiveUsersAsync();
    System.Threading.Tasks.Task<bool> ValidateCredentialsAsync(string username, string password);
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
    
    System.Threading.Tasks.Task<int> SaveChangesAsync();
    System.Threading.Tasks.Task BeginTransactionAsync();
    System.Threading.Tasks.Task CommitTransactionAsync();
    System.Threading.Tasks.Task RollbackTransactionAsync();
}

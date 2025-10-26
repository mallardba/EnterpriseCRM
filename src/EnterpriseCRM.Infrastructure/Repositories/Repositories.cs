using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using EnterpriseCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EnterpriseCRM.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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
            await UpdateAsync(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
}

/// <summary>
/// Customer repository implementation
/// </summary>
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(c => c.CompanyName.Contains(searchTerm) ||
                       c.FirstName!.Contains(searchTerm) ||
                       c.LastName!.Contains(searchTerm) ||
                       c.Email.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status)
    {
        return await _dbSet
            .Where(c => c.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType type)
    {
        return await _dbSet
            .Where(c => c.Type == type)
            .ToListAsync();
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<IEnumerable<Customer>> GetRecentAsync(int count)
    {
        return await _dbSet
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}

/// <summary>
/// Contact repository implementation
/// </summary>
public class ContactRepository : Repository<Contact>, IContactRepository
{
    public ContactRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Contact>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(c => c.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<Contact?> GetPrimaryContactAsync(int customerId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsPrimary);
    }

    public async Task<IEnumerable<Contact>> GetByRoleAsync(ContactRole role)
    {
        return await _dbSet
            .Where(c => c.Role == role)
            .ToListAsync();
    }
}

/// <summary>
/// Lead repository implementation
/// </summary>
public class LeadRepository : Repository<Lead>, ILeadRepository
{
    public LeadRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Lead>> GetByStatusAsync(LeadStatus status)
    {
        return await _dbSet
            .Where(l => l.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetBySourceAsync(LeadSource source)
    {
        return await _dbSet
            .Where(l => l.Source == source)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetByAssignedUserAsync(int userId)
    {
        return await _dbSet
            .Where(l => l.AssignedToUserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> GetRecentAsync(int count)
    {
        return await _dbSet
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lead>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(l => l.CompanyName.Contains(searchTerm) ||
                       l.FirstName!.Contains(searchTerm) ||
                       l.LastName!.Contains(searchTerm) ||
                       l.Email.Contains(searchTerm))
            .ToListAsync();
    }
}

/// <summary>
/// Opportunity repository implementation
/// </summary>
public class OpportunityRepository : Repository<Opportunity>, IOpportunityRepository
{
    public OpportunityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Opportunity>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByStageAsync(OpportunityStage stage)
    {
        return await _dbSet
            .Where(o => o.Stage == stage)
            .ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByAssignedUserAsync(int userId)
    {
        return await _dbSet
            .Where(o => o.AssignedToUserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Opportunity>> GetByStatusAsync(OpportunityStatus status)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalPipelineValueAsync()
    {
        return await _dbSet
            .Where(o => o.Status == OpportunityStatus.Open)
            .SumAsync(o => o.Amount);
    }

    public async Task<decimal> GetForecastedRevenueAsync()
    {
        return await _dbSet
            .Where(o => o.Status == OpportunityStatus.Open)
            .SumAsync(o => o.Amount * o.Probability / 100);
    }
}

/// <summary>
/// WorkItem repository implementation
/// </summary>
public class WorkItemRepository : Repository<WorkItem>, IWorkItemRepository
{
    public WorkItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkItem>> GetByAssignedUserAsync(int userId)
    {
        return await _dbSet
            .Where(t => t.AssignedToUserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByStatusAsync(WorkItemStatus status)
    {
        return await _dbSet
            .Where(t => t.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetOverdueAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(t => t.DueDate.HasValue && 
                       t.DueDate.Value.Date < today && 
                       t.Status != WorkItemStatus.Completed)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetDueTodayAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(t => t.DueDate.HasValue && 
                       t.DueDate.Value.Date == today && 
                       t.Status != WorkItemStatus.Completed)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(t => t.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByLeadIdAsync(int leadId)
    {
        return await _dbSet
            .Where(t => t.LeadId == leadId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetByOpportunityIdAsync(int opportunityId)
    {
        return await _dbSet
            .Where(t => t.OpportunityId == opportunityId)
            .ToListAsync();
    }
}

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        return await _dbSet
            .Where(u => u.Role == role)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.Status == UserStatus.Active)
            .ToListAsync();
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        var user = await GetByUsernameAsync(username);
        if (user == null || user.Status != UserStatus.Active)
            return false;

        // In a real application, you would hash the password and compare with stored hash
        // For demo purposes, we'll use a simple comparison
        return user.PasswordHash == password;
    }
}

/// <summary>
/// Product repository implementation
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(p => p.Category == category && p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<Product?> GetBySKUAsync(string sku)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(p => p.Name.Contains(searchTerm) ||
                       p.SKU!.Contains(searchTerm) ||
                       p.Category!.Contains(searchTerm))
            .ToListAsync();
    }
}
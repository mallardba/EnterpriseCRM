using EnterpriseCRM.Core.Interfaces;
using EnterpriseCRM.Infrastructure.Data;
using EnterpriseCRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace EnterpriseCRM.Infrastructure.UnitOfWork;

/// <summary>
/// Unit of Work implementation for managing transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Customers = new CustomerRepository(_context);
        Contacts = new ContactRepository(_context);
        Leads = new LeadRepository(_context);
        Opportunities = new OpportunityRepository(_context);
        Tasks = new TaskRepository(_context);
        Users = new UserRepository(_context);
    }

    public ICustomerRepository Customers { get; }
    public IContactRepository Contacts { get; }
    public ILeadRepository Leads { get; }
    public IOpportunityRepository Opportunities { get; }
    public ITaskRepository Tasks { get; }
    public IUserRepository Users { get; }

    public async System.Threading.Tasks.Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async System.Threading.Tasks.Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async System.Threading.Tasks.Task RollbackTransactionAsync()
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
        _context.Dispose();
    }
}

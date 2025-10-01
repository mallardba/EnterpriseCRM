using Microsoft.EntityFrameworkCore;
using EnterpriseCRM.Infrastructure.Data;
using EnterpriseCRM.Core.Entities;

namespace EnterpriseCRM.IntegrationTests.TestFixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public ApplicationDbContext Context { get; private set; } = null!;
    private string _connectionString = "Server=(localdb)\\mssqllocaldb;Database=EnterpriseCRM_IntegrationTest;Trusted_Connection=true;";

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        Context = new ApplicationDbContext(options);
        
        // Ensure database is created
        await Context.Database.EnsureCreatedAsync();
        
        // Seed test data
        await SeedTestDataAsync();
    }

    public async System.Threading.Tasks.Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.DisposeAsync();
        }
    }

    private async System.Threading.Tasks.Task SeedTestDataAsync()
    {
        // Add sample customers for integration tests
        if (!Context.Customers.Any())
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    CompanyName = "Integration Test Company 1",
                    Email = "test1@integration.com",
                    Type = CustomerType.Company,
                    Status = CustomerStatus.Active,
                    CreatedBy = "integration-test",
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    CompanyName = "Integration Test Company 2",
                    Email = "test2@integration.com",
                    Type = CustomerType.Company,
                    Status = CustomerStatus.Active,
                    CreatedBy = "integration-test",
                    CreatedAt = DateTime.UtcNow
                }
            };

            Context.Customers.AddRange(customers);
            await Context.SaveChangesAsync();
        }
    }
}

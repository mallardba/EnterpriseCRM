using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnterpriseCRM.Infrastructure.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<EnterpriseCRM.Core.Entities.Task> Tasks { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Customer entity
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Industry).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.CompanyName);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
        });

        // Configure Contact entity
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Contacts)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Role);
        });

        // Configure Lead entity
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            // Configure decimal precision for EstimatedValue
            entity.Property(e => e.EstimatedValue)
                  .HasColumnType("decimal(18,2)")
                  .HasPrecision(18, 2);
            
            entity.HasOne(e => e.AssignedToUser)
                  .WithMany(u => u.AssignedLeads)
                  .HasForeignKey(e => e.AssignedToUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Leads)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.AssignedToUserId);
        });

        // Configure Opportunity entity
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Product).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            // Configure decimal precision for Amount and Probability
            entity.Property(e => e.Amount)
                  .HasColumnType("decimal(18,2)")
                  .HasPrecision(18, 2);
            
            entity.Property(e => e.Probability)
                  .HasColumnType("decimal(5,2)")
                  .HasPrecision(5, 2);
            
            // Make CustomerId nullable to support SET NULL
            entity.Property(e => e.CustomerId).IsRequired(false);
            
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Opportunities)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.AssignedToUser)
                  .WithMany(u => u.AssignedOpportunities)
                  .HasForeignKey(e => e.AssignedToUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Stage);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.AssignedToUserId);
        });

        // Configure Task entity
        modelBuilder.Entity<EnterpriseCRM.Core.Entities.Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.AssignedToUser)
                  .WithMany(u => u.AssignedTasks)
                  .HasForeignKey(e => e.AssignedToUserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Tasks)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Lead)
                  .WithMany(l => l.Tasks)
                  .HasForeignKey(e => e.LeadId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Opportunity)
                  .WithMany(o => o.Tasks)
                  .HasForeignKey(e => e.OpportunityId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.AssignedToUserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.OpportunityId);
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.Status);
        });

        // Configure soft delete for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var notExpression = Expression.Not(property);
                var lambda = Expression.Lambda(notExpression, parameter);
                
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(lambda);
            }
        }
    }
}

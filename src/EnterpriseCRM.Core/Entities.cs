using System.ComponentModel.DataAnnotations;

namespace EnterpriseCRM.Core.Entities;

/// <summary>
/// Base entity class with common properties for all domain entities
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
    
    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Customer entity representing a company or individual customer
/// </summary>
public class Customer : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(50)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(50)]
    public string? Industry { get; set; }
    
    public CustomerType Type { get; set; } = CustomerType.Individual;
    
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}

/// <summary>
/// Contact entity representing individual contacts within a customer
/// </summary>
public class Contact : BaseEntity
{
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    public ContactRole Role { get; set; } = ContactRole.General;
    
    public bool IsPrimary { get; set; } = false;
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
}

/// <summary>
/// Lead entity representing potential customers
/// </summary>
public class Lead : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [MaxLength(100)]
    public string? Industry { get; set; }
    
    public LeadSource Source { get; set; } = LeadSource.Website;
    
    public LeadStatus Status { get; set; } = LeadStatus.New;
    
    public LeadPriority Priority { get; set; } = LeadPriority.Medium;
    
    public decimal EstimatedValue { get; set; }
    
    public DateTime? ExpectedCloseDate { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public int? AssignedToUserId { get; set; }
    
    public int? CustomerId { get; set; }
    
    // Navigation properties
    public virtual User? AssignedToUser { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}

/// <summary>
/// Opportunity entity representing sales opportunities
/// </summary>
public class Opportunity : BaseEntity
{
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    
    public decimal Amount { get; set; }
    
    public decimal Probability { get; set; }
    
    public DateTime? ExpectedCloseDate { get; set; }
    
    public DateTime? ActualCloseDate { get; set; }
    
    public OpportunityStatus Status { get; set; } = OpportunityStatus.Open;
    
    [MaxLength(100)]
    public string? Product { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public int? AssignedToUserId { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}

/// <summary>
/// Task entity representing activities and tasks
/// </summary>
public class Task : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public TaskType Type { get; set; } = TaskType.General;
    
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    
    [Required]
    public int AssignedToUserId { get; set; }
    
    public int? CustomerId { get; set; }
    
    public int? LeadId { get; set; }
    
    public int? OpportunityId { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual User AssignedToUser { get; set; } = null!;
    public virtual Customer? Customer { get; set; }
    public virtual Lead? Lead { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
}

/// <summary>
/// User entity representing system users
/// </summary>
public class User : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.User;
    
    public UserStatus Status { get; set; } = UserStatus.Active;
    
    public DateTime? LastLoginDate { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    // Navigation properties
    public virtual ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
    public virtual ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
    public virtual ICollection<Opportunity> AssignedOpportunities { get; set; } = new List<Opportunity>();
}

// Enums
public enum CustomerType
{
    Individual,
    Company
}

public enum CustomerStatus
{
    Active,
    Inactive,
    Suspended
}

public enum ContactRole
{
    General,
    DecisionMaker,
    Influencer,
    User,
    Technical
}

public enum LeadSource
{
    Website,
    Referral,
    ColdCall,
    Email,
    SocialMedia,
    TradeShow,
    Advertisement,
    Other
}

public enum LeadStatus
{
    New,
    Contacted,
    Qualified,
    Proposal,
    Negotiation,
    ClosedWon,
    ClosedLost
}

public enum LeadPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum OpportunityStage
{
    Prospecting,
    Qualification,
    Proposal,
    Negotiation,
    ClosedWon,
    ClosedLost
}

public enum OpportunityStatus
{
    Open,
    Won,
    Lost,
    Cancelled
}

public enum TaskType
{
    General,
    Call,
    Email,
    Meeting,
    FollowUp,
    Proposal,
    Demo,
    Other
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled,
    OnHold
}

public enum UserRole
{
    Admin,
    Manager,
    User,
    ReadOnly
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended
}

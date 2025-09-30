using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Core.Entities;

namespace EnterpriseCRM.Application.Interfaces;

/// <summary>
/// Customer service interface
/// </summary>
public interface ICustomerService
{
    System.Threading.Tasks.Task<CustomerDto?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<PagedResultDto<CustomerDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    System.Threading.Tasks.Task<PagedResultDto<CustomerDto>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
    System.Threading.Tasks.Task<CustomerDto> CreateAsync(CreateCustomerDto createDto, string currentUser);
    System.Threading.Tasks.Task<CustomerDto> UpdateAsync(UpdateCustomerDto updateDto, string currentUser);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<CustomerDto>> GetByStatusAsync(CustomerStatus status);
    System.Threading.Tasks.Task<IEnumerable<CustomerDto>> GetByTypeAsync(CustomerType type);
    System.Threading.Tasks.Task<CustomerDto?> GetByEmailAsync(string email);
    System.Threading.Tasks.Task<IEnumerable<CustomerDto>> GetRecentAsync(int count);
}

/// <summary>
/// Contact service interface
/// </summary>
public interface IContactService
{
    System.Threading.Tasks.Task<ContactDto?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<ContactDto>> GetByCustomerIdAsync(int customerId);
    System.Threading.Tasks.Task<ContactDto?> GetPrimaryContactAsync(int customerId);
    System.Threading.Tasks.Task<ContactDto> CreateAsync(CreateContactDto createDto, string currentUser);
    System.Threading.Tasks.Task<ContactDto> UpdateAsync(UpdateContactDto updateDto, string currentUser);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<ContactDto>> GetByRoleAsync(ContactRole role);
}

/// <summary>
/// Lead service interface
/// </summary>
public interface ILeadService
{
    System.Threading.Tasks.Task<LeadDto?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<PagedResultDto<LeadDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    System.Threading.Tasks.Task<PagedResultDto<LeadDto>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
    System.Threading.Tasks.Task<LeadDto> CreateAsync(CreateLeadDto createDto, string currentUser);
    System.Threading.Tasks.Task<LeadDto> UpdateAsync(UpdateLeadDto updateDto, string currentUser);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<LeadDto>> GetByStatusAsync(LeadStatus status);
    System.Threading.Tasks.Task<IEnumerable<LeadDto>> GetBySourceAsync(LeadSource source);
    System.Threading.Tasks.Task<IEnumerable<LeadDto>> GetByAssignedUserAsync(int userId);
    System.Threading.Tasks.Task<IEnumerable<LeadDto>> GetRecentAsync(int count);
    System.Threading.Tasks.Task<LeadDto> ConvertToCustomerAsync(int leadId, string currentUser);
}

/// <summary>
/// Opportunity service interface
/// </summary>
public interface IOpportunityService
{
    System.Threading.Tasks.Task<OpportunityDto?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<PagedResultDto<OpportunityDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    System.Threading.Tasks.Task<OpportunityDto> CreateAsync(CreateOpportunityDto createDto, string currentUser);
    System.Threading.Tasks.Task<OpportunityDto> UpdateAsync(UpdateOpportunityDto updateDto, string currentUser);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<OpportunityDto>> GetByCustomerIdAsync(int customerId);
    System.Threading.Tasks.Task<IEnumerable<OpportunityDto>> GetByStageAsync(OpportunityStage stage);
    System.Threading.Tasks.Task<IEnumerable<OpportunityDto>> GetByAssignedUserAsync(int userId);
    System.Threading.Tasks.Task<IEnumerable<OpportunityDto>> GetByStatusAsync(OpportunityStatus status);
    System.Threading.Tasks.Task<decimal> GetTotalPipelineValueAsync();
    System.Threading.Tasks.Task<decimal> GetForecastedRevenueAsync();
    System.Threading.Tasks.Task<OpportunityDto> UpdateStageAsync(int opportunityId, OpportunityStage stage, string currentUser);
}

/// <summary>
/// Task service interface
/// </summary>
public interface ITaskService
{
    System.Threading.Tasks.Task<TaskDto?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<PagedResultDto<TaskDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    System.Threading.Tasks.Task<TaskDto> CreateAsync(CreateTaskDto createDto, string currentUser);
    System.Threading.Tasks.Task<TaskDto> UpdateAsync(UpdateTaskDto updateDto, string currentUser);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetByAssignedUserAsync(int userId);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetByStatusAsync(EnterpriseCRM.Core.Entities.TaskStatus status);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetOverdueAsync();
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetDueTodayAsync();
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetByCustomerIdAsync(int customerId);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetByLeadIdAsync(int leadId);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetByOpportunityIdAsync(int opportunityId);
    System.Threading.Tasks.Task<TaskDto> CompleteTaskAsync(int taskId, string currentUser);
}

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    System.Threading.Tasks.Task<UserDto?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<PagedResultDto<UserDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    System.Threading.Tasks.Task<UserDto> CreateAsync(CreateUserDto createDto, string currentUser);
    System.Threading.Tasks.Task<UserDto> UpdateAsync(UpdateUserDto updateDto, string currentUser);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<UserDto?> GetByEmailAsync(string email);
    System.Threading.Tasks.Task<UserDto?> GetByUsernameAsync(string username);
    System.Threading.Tasks.Task<IEnumerable<UserDto>> GetByRoleAsync(UserRole role);
    System.Threading.Tasks.Task<IEnumerable<UserDto>> GetActiveUsersAsync();
    System.Threading.Tasks.Task<bool> ValidateCredentialsAsync(string username, string password);
    System.Threading.Tasks.Task<UserDto> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string currentUser);
}

/// <summary>
/// Dashboard service interface
/// </summary>
public interface IDashboardService
{
    System.Threading.Tasks.Task<DashboardStatsDto> GetDashboardStatsAsync();
    System.Threading.Tasks.Task<IEnumerable<CustomerDto>> GetRecentCustomersAsync(int count);
    System.Threading.Tasks.Task<IEnumerable<LeadDto>> GetRecentLeadsAsync(int count);
    System.Threading.Tasks.Task<IEnumerable<OpportunityDto>> GetRecentOpportunitiesAsync(int count);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetUpcomingTasksAsync(int count);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetOverdueTasksAsync();
    System.Threading.Tasks.Task<Dictionary<string, int>> GetLeadSourceStatsAsync();
    System.Threading.Tasks.Task<Dictionary<string, decimal>> GetMonthlyRevenueAsync(int months);
}

// Additional DTOs for create/update operations
public class CreateContactDto
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; } = ContactRole.General;
    public bool IsPrimary { get; set; } = false;
    public string? Notes { get; set; }
}

public class UpdateContactDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; }
    public bool IsPrimary { get; set; }
    public string? Notes { get; set; }
}

public class CreateLeadDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Industry { get; set; }
    public LeadSource Source { get; set; } = LeadSource.Website;
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadPriority Priority { get; set; } = LeadPriority.Medium;
    public decimal EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? Notes { get; set; }
    public int? AssignedToUserId { get; set; }
}

public class UpdateLeadDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Industry { get; set; }
    public LeadSource Source { get; set; }
    public LeadStatus Status { get; set; }
    public LeadPriority Priority { get; set; }
    public decimal EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? Notes { get; set; }
    public int? AssignedToUserId { get; set; }
}

public class CreateOpportunityDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    public decimal Amount { get; set; }
    public decimal Probability { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public OpportunityStatus Status { get; set; } = OpportunityStatus.Open;
    public string? Product { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public int? AssignedToUserId { get; set; }
}

public class UpdateOpportunityDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public OpportunityStage Stage { get; set; }
    public decimal Amount { get; set; }
    public decimal Probability { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public OpportunityStatus Status { get; set; }
    public string? Product { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public int? AssignedToUserId { get; set; }
}

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskType Type { get; set; } = TaskType.General;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public EnterpriseCRM.Core.Entities.TaskStatus Status { get; set; } = EnterpriseCRM.Core.Entities.TaskStatus.Pending;
    public DateTime? DueDate { get; set; }
    public int AssignedToUserId { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskType Type { get; set; }
    public TaskPriority Priority { get; set; }
    public EnterpriseCRM.Core.Entities.TaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public int AssignedToUserId { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }
    public string? Notes { get; set; }
}

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
}

public class UpdateUserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
}

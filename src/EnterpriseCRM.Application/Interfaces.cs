using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Core.Entities;

namespace EnterpriseCRM.Application.Interfaces;

/// <summary>
/// Customer service interface
/// </summary>
public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<PagedResultDto<CustomerDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<PagedResultDto<CustomerDto>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
    Task<CustomerDto> CreateAsync(CreateCustomerDto createDto, string currentUser);
    Task<CustomerDto> UpdateAsync(UpdateCustomerDto updateDto, string currentUser);
    Task DeleteAsync(int id);
    Task<IEnumerable<CustomerDto>> GetByStatusAsync(CustomerStatus status);
    Task<IEnumerable<CustomerDto>> GetByTypeAsync(CustomerType type);
    Task<CustomerDto?> GetByEmailAsync(string email);
    Task<IEnumerable<CustomerDto>> GetRecentAsync(int count);
}

/// <summary>
/// Contact service interface
/// </summary>
public interface IContactService
{
    Task<ContactDto?> GetByIdAsync(int id);
    Task<IEnumerable<ContactDto>> GetByCustomerIdAsync(int customerId);
    Task<ContactDto?> GetPrimaryContactAsync(int customerId);
    Task<ContactDto> CreateAsync(CreateContactDto createDto, string currentUser);
    Task<ContactDto> UpdateAsync(UpdateContactDto updateDto, string currentUser);
    Task DeleteAsync(int id);
    Task<IEnumerable<ContactDto>> GetByRoleAsync(ContactRole role);
}

/// <summary>
/// Lead service interface
/// </summary>
public interface ILeadService
{
    Task<LeadDto?> GetByIdAsync(int id);
    Task<PagedResultDto<LeadDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<PagedResultDto<LeadDto>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
    Task<LeadDto> CreateAsync(CreateLeadDto createDto, string currentUser);
    Task<LeadDto> UpdateAsync(UpdateLeadDto updateDto, string currentUser);
    Task DeleteAsync(int id);
    Task<IEnumerable<LeadDto>> GetByStatusAsync(LeadStatus status);
    Task<IEnumerable<LeadDto>> GetBySourceAsync(LeadSource source);
    Task<IEnumerable<LeadDto>> GetByAssignedUserAsync(int userId);
    Task<IEnumerable<LeadDto>> GetRecentAsync(int count);
    Task<LeadDto> ConvertToCustomerAsync(int leadId, string currentUser);
}

/// <summary>
/// Opportunity service interface
/// </summary>
public interface IOpportunityService
{
    Task<OpportunityDto?> GetByIdAsync(int id);
    Task<PagedResultDto<OpportunityDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<OpportunityDto> CreateAsync(CreateOpportunityDto createDto, string currentUser);
    Task<OpportunityDto> UpdateAsync(UpdateOpportunityDto updateDto, string currentUser);
    Task DeleteAsync(int id);
    Task<IEnumerable<OpportunityDto>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<OpportunityDto>> GetByStageAsync(OpportunityStage stage);
    Task<IEnumerable<OpportunityDto>> GetByAssignedUserAsync(int userId);
    Task<IEnumerable<OpportunityDto>> GetByStatusAsync(OpportunityStatus status);
    Task<decimal> GetTotalPipelineValueAsync();
    Task<decimal> GetForecastedRevenueAsync();
    Task<OpportunityDto> UpdateStageAsync(int opportunityId, OpportunityStage stage, string currentUser);
}

/// <summary>
/// WorkItem service interface
/// </summary>
public interface IWorkItemService
{
    Task<WorkItemDto?> GetByIdAsync(int id);
    Task<PagedResultDto<WorkItemDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<WorkItemDto> CreateAsync(CreateWorkItemDto createDto, string currentUser);
    Task<WorkItemDto> UpdateAsync(UpdateWorkItemDto updateDto, string currentUser);
    Task DeleteAsync(int id);
    Task<IEnumerable<WorkItemDto>> GetByAssignedUserAsync(int userId);
    Task<IEnumerable<WorkItemDto>> GetByStatusAsync(WorkItemStatus status);
    Task<IEnumerable<WorkItemDto>> GetOverdueAsync();
    Task<IEnumerable<WorkItemDto>> GetDueTodayAsync();
    Task<IEnumerable<WorkItemDto>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<WorkItemDto>> GetByLeadIdAsync(int leadId);
    Task<IEnumerable<WorkItemDto>> GetByOpportunityIdAsync(int opportunityId);
    Task<WorkItemDto> CompleteWorkItemAsync(int workItemId, string currentUser);
}

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    Task<UserDto?> GetByIdAsync(int id);
    Task<PagedResultDto<UserDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<UserDto> CreateAsync(CreateUserDto createDto, string currentUser);
    Task<UserDto> UpdateAsync(UpdateUserDto updateDto, string currentUser);
    Task DeleteAsync(int id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<IEnumerable<UserDto>> GetByRoleAsync(UserRole role);
    Task<IEnumerable<UserDto>> GetActiveUsersAsync();
    Task<bool> ValidateCredentialsAsync(string username, string password);
    Task<UserDto> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string currentUser);
}

/// <summary>
/// Dashboard service interface
/// </summary>
public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<IEnumerable<CustomerDto>> GetRecentCustomersAsync(int count);
    Task<IEnumerable<LeadDto>> GetRecentLeadsAsync(int count);
    Task<IEnumerable<OpportunityDto>> GetRecentOpportunitiesAsync(int count);
    Task<IEnumerable<WorkItemDto>> GetUpcomingWorkItemsAsync(int count);
    Task<IEnumerable<WorkItemDto>> GetOverdueWorkItemsAsync();
    Task<Dictionary<string, int>> GetLeadSourceStatsAsync();
    Task<Dictionary<string, decimal>> GetMonthlyRevenueAsync(int months);
}

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);
    Task<bool> ValidateTokenAsync(string token);
    Task<UserDto?> GetUserFromTokenAsync(string token);
    Task<string> GenerateTokenAsync(User user);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
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

public class CreateWorkItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkItemType Type { get; set; } = WorkItemType.General;
    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Pending;
    public DateTime? DueDate { get; set; }
    public int AssignedToUserId { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateWorkItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkItemType Type { get; set; }
    public WorkItemPriority Priority { get; set; }
    public WorkItemStatus Status { get; set; }
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

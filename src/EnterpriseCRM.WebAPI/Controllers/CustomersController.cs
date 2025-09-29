using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.WebAPI.Controllers;

/// <summary>
/// Customers API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of customers</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CustomerDto>>> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _customerService.GetAllAsync(pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return StatusCode(500, "An error occurred while retrieving customers");
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }
            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with ID {CustomerId}", id);
            return StatusCode(500, "An error occurred while retrieving the customer");
        }
    }

    /// <summary>
    /// Search customers
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged search results</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PagedResultDto<CustomerDto>>> Search(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term is required");
            }

            var result = await _customerService.SearchAsync(searchTerm, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with term {SearchTerm}", searchTerm);
            return StatusCode(500, "An error occurred while searching customers");
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="createDto">Customer creation data</param>
    /// <returns>Created customer</returns>
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = User.Identity?.Name ?? "System";
            var customer = await _customerService.CreateAsync(createDto, currentUser);
            
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, "An error occurred while creating the customer");
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="updateDto">Customer update data</param>
    /// <returns>Updated customer</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] UpdateCustomerDto updateDto)
    {
        try
        {
            if (id != updateDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = User.Identity?.Name ?? "System";
            var customer = await _customerService.UpdateAsync(updateDto, currentUser);
            
            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with ID {CustomerId}", id);
            return StatusCode(500, "An error occurred while updating the customer");
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _customerService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer with ID {CustomerId}", id);
            return StatusCode(500, "An error occurred while deleting the customer");
        }
    }

    /// <summary>
    /// Get customers by status
    /// </summary>
    /// <param name="status">Customer status</param>
    /// <returns>List of customers with specified status</returns>
    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetByStatus(CustomerStatus status)
    {
        try
        {
            var customers = await _customerService.GetByStatusAsync(status);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers by status {Status}", status);
            return StatusCode(500, "An error occurred while retrieving customers by status");
        }
    }

    /// <summary>
    /// Get customers by type
    /// </summary>
    /// <param name="type">Customer type</param>
    /// <returns>List of customers with specified type</returns>
    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetByType(CustomerType type)
    {
        try
        {
            var customers = await _customerService.GetByTypeAsync(type);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers by type {Type}", type);
            return StatusCode(500, "An error occurred while retrieving customers by type");
        }
    }

    /// <summary>
    /// Get recent customers
    /// </summary>
    /// <param name="count">Number of recent customers to retrieve</param>
    /// <returns>List of recent customers</returns>
    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetRecent([FromQuery] int count = 10)
    {
        try
        {
            var customers = await _customerService.GetRecentAsync(count);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent customers");
            return StatusCode(500, "An error occurred while retrieving recent customers");
        }
    }
}

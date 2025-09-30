using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Application.DTOs;
using Moq;
using FluentAssertions;
using Xunit;

namespace EnterpriseCRM.UnitTests.Services;

/// <summary>
/// Unit tests for CustomerService following TDD principles
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        
        _customerService = new CustomerService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            CompanyName = "Test Company",
            Email = "test@company.com",
            Type = CustomerType.Company,
            Status = CustomerStatus.Active
        };

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.CompanyName.Should().Be("Test Company");
        result.Email.Should().Be("test@company.com");
        result.Type.Should().Be(CustomerType.Company);
        result.Status.Should().Be(CustomerStatus.Active);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var customerId = 999;
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.GetByIdAsync(customerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            CompanyName = "New Company",
            Email = "new@company.com",
            Type = CustomerType.Company,
            Status = CustomerStatus.Active
        };

        var currentUser = "testuser";
        var createdCustomer = new Customer
        {
            Id = 1,
            CompanyName = createDto.CompanyName,
            Email = createDto.Email,
            Type = createDto.Type,
            Status = createDto.Status,
            CreatedBy = currentUser,
            CreatedAt = DateTime.UtcNow
        };

        _customerRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync(createdCustomer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _customerService.CreateAsync(createDto, currentUser);

        // Assert
        result.Should().NotBeNull();
        result.CompanyName.Should().Be(createDto.CompanyName);
        result.Email.Should().Be(createDto.Email);
        result.Type.Should().Be(createDto.Type);
        result.Status.Should().Be(createDto.Status);
        result.CreatedBy.Should().Be(currentUser);

        _customerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateCustomer()
    {
        // Arrange
        var customerId = 1;
        var updateDto = new UpdateCustomerDto
        {
            Id = customerId,
            CompanyName = "Updated Company",
            Email = "updated@company.com",
            Type = CustomerType.Company,
            Status = CustomerStatus.Active
        };

        var currentUser = "testuser";
        var existingCustomer = new Customer
        {
            Id = customerId,
            CompanyName = "Old Company",
            Email = "old@company.com",
            Type = CustomerType.Individual,
            Status = CustomerStatus.Inactive,
            CreatedBy = "originaluser",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _customerService.UpdateAsync(updateDto, currentUser);

        // Assert
        result.Should().NotBeNull();
        result.CompanyName.Should().Be(updateDto.CompanyName);
        result.Email.Should().Be(updateDto.Email);
        result.Type.Should().Be(updateDto.Type);
        result.Status.Should().Be(updateDto.Status);
        result.UpdatedBy.Should().Be(currentUser);
        result.UpdatedAt.Should().NotBeNull();

        _customerRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerExists_ShouldSoftDeleteCustomer()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            CompanyName = "Test Company",
            Email = "test@company.com",
            IsDeleted = false
        };

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _customerService.DeleteAsync(customerId);

        // Assert
        customer.IsDeleted.Should().BeTrue();
        customer.UpdatedAt.Should().NotBeNull();

        _customerRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithValidSearchTerm_ShouldReturnMatchingCustomers()
    {
        // Arrange
        var searchTerm = "test";
        var pageNumber = 1;
        var pageSize = 10;
        var customers = new List<Customer>
        {
            new Customer { Id = 1, CompanyName = "Test Company 1", Email = "test1@company.com" },
            new Customer { Id = 2, CompanyName = "Test Company 2", Email = "test2@company.com" }
        };

        _customerRepositoryMock.Setup(r => r.SearchAsync(searchTerm))
            .ReturnsAsync(customers);
        _customerRepositoryMock.Setup(r => r.CountAsync())
            .ReturnsAsync(customers.Count);

        // Act
        var result = await _customerService.SearchAsync(searchTerm, pageNumber, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetByStatusAsync_WithValidStatus_ShouldReturnCustomersWithStatus()
    {
        // Arrange
        var status = CustomerStatus.Active;
        var customers = new List<Customer>
        {
            new Customer { Id = 1, CompanyName = "Active Company 1", Status = status },
            new Customer { Id = 2, CompanyName = "Active Company 2", Status = status }
        };

        _customerRepositoryMock.Setup(r => r.GetByStatusAsync(status))
            .ReturnsAsync(customers);

        // Act
        var result = await _customerService.GetByStatusAsync(status);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(c => c.Status == status).Should().BeTrue();
    }

    [Fact]
    public async Task GetRecentAsync_WithValidCount_ShouldReturnRecentCustomers()
    {
        // Arrange
        var count = 5;
        var customers = new List<Customer>
        {
            new Customer { Id = 1, CompanyName = "Recent Company 1", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Customer { Id = 2, CompanyName = "Recent Company 2", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new Customer { Id = 3, CompanyName = "Recent Company 3", CreatedAt = DateTime.UtcNow.AddDays(-3) }
        };

        _customerRepositoryMock.Setup(r => r.GetRecentAsync(count))
            .ReturnsAsync(customers);

        // Act
        var result = await _customerService.GetRecentAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(c => c.CreatedAt);
    }
}

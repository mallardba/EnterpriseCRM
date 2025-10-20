using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Application.Services;
using Moq;
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;
using AutoMapper;

namespace EnterpriseCRM.UnitTests.Services;

/// <summary>
/// Unit tests for CustomerService following TDD principles
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        
        _customerService = new CustomerService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public void CustomerService_ShouldBeCreated()
    {
        // Arrange & Act
        var service = new CustomerService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomerDto()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            CompanyName = "Acme Corp",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "555-1234",
            CreatedAt = DateTime.UtcNow
        };

        var customerDto = new CustomerDto
        {
            Id = customerId,
            CompanyName = "Acme Corp",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "555-1234",
            CreatedAt = customer.CreatedAt
        };

        // Setup mocks
        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _mapperMock
            .Setup(mapper => mapper.Map<CustomerDto>(customer))
            .Returns(customerDto);

        // Act
        var result = await _customerService.GetByIdAsync(customerId);

        // Assert using FluentAssertions
        result.Should().NotBeNull();
        result.Should().BeOfType<CustomerDto>();
        
        // Additional null check to satisfy compiler
        if (result == null) return;
        
        result.Id.Should().Be(customerId);
        result.CompanyName.Should().Be("Acme Corp");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.Phone.Should().Be("555-1234");

        // Verify mock interactions using Moq
        _customerRepositoryMock.Verify(repo => repo.GetByIdAsync(customerId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<CustomerDto>(customer), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var customerId = 999;
        
        _customerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.GetByIdAsync(customerId);

        // Assert
        result.Should().BeNull();

        // Verify mock interactions
        _customerRepositoryMock.Verify(repo => repo.GetByIdAsync(customerId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<CustomerDto>(It.IsAny<Customer>()), Times.Never);
    }
}

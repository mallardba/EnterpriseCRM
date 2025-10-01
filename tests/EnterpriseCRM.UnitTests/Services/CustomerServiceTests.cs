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

    // TODO: Add comprehensive tests once AutoMapper is properly configured
    // The tests below are commented out until we have proper AutoMapper configuration
    /*
    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomer()
    {
        // Test implementation will be added after AutoMapper setup
    }
    */
}

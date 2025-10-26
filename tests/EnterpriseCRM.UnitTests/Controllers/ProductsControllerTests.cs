using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.WebAPI.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _loggerMock = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_productServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void ProductsController_ShouldBeCreated()
    {
        // Assert
        _controller.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResultDto<ProductDto>
        {
            Data = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Product 1", Price = 99.99m },
                new ProductDto { Id = 2, Name = "Product 2", Price = 199.99m }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _productServiceMock.Setup(s => s.GetAllAsync(1, 10))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(1, 10);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(pagedResult);

        _productServiceMock.Verify(s => s.GetAllAsync(1, 10), Times.Once);
    }
}
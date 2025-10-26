using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.Application.Interfaces;
using EnterpriseCRM.Application.Services;
using EnterpriseCRM.Core.Entities;
using EnterpriseCRM.Core.Interfaces;
using Moq;
using AutoMapper;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _productService = new ProductService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public void ProductService_ShouldBeCreated()
    {
        // Arrange & Act
        var service = new ProductService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProductDto()
    {
        // Arrange
        var productId = 1;
        var product = new Product 
        { 
            Id = productId, 
            Name = "Test Product", 
            Price = 99.99m,
            IsActive = true,
            CreatedBy = "test",
            CreatedAt = DateTime.UtcNow
        };
        var productDto = new ProductDto 
        { 
            Id = productId, 
            Name = "Test Product", 
            Price = 99.99m,
            IsActive = true
        };

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map<ProductDto>(product))
            .Returns(productDto);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ProductDto>();
        result?.Price.Should().Be(99.99m);
        
        _productRepositoryMock.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        _mapperMock.Verify(m => m.Map<ProductDto>(product), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var productId = 999;

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().BeNull();
        _productRepositoryMock.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
    }
}

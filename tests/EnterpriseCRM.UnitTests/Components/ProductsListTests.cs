using EnterpriseCRM.Application.DTOs;
using EnterpriseCRM.BlazorServer.Components.Products;
using EnterpriseCRM.BlazorServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Moq;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Components;

public class ProductsListTests : TestContext
{
    private readonly Mock<IProductClientService> _productServiceMock;

    public ProductsListTests()
    {
        _productServiceMock = new Mock<IProductClientService>();
        Services.AddSingleton(_productServiceMock.Object);
    }

    [Fact]
    public void ProductsList_ShouldRenderEmptyState_WhenNoProducts()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<ProductDto>());

        // Act
        var cut = RenderComponent<ProductsList>();

        // Assert
        cut.Markup.Should().Contain("No products found");
    }

    [Fact]
    public void ProductsList_ShouldRenderProducts_WhenProductsExist()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new ProductDto { Id = 1, Name = "Widget", Price = 99.99m, IsActive = true },
            new ProductDto { Id = 2, Name = "Gadget", Price = 149.99m, IsActive = true }
        };

        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var cut = RenderComponent<ProductsList>();

        // Assert
        cut.Markup.Should().Contain("Widget");
        cut.Markup.Should().Contain("Gadget");
        cut.Markup.Should().Contain("$99.99");
        cut.Markup.Should().Contain("$149.99");
    }
}
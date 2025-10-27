using EnterpriseCRM.BlazorServer.Components.Products;
using EnterpriseCRM.BlazorServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bunit;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Components;

public class ProductFormTests : TestContext
{
    private readonly Mock<IProductClientService> _productServiceMock;

    public ProductFormTests()
    {
        _productServiceMock = new Mock<IProductClientService>();
        Services.AddSingleton(_productServiceMock.Object);
    }
    
    [Fact]
    public void ProductForm_ShouldRenderInputFields()
    {
        // Act
        var cut = RenderComponent<ProductForm>();

        // Assert
        cut.Markup.Should().Contain("Name");
        cut.Markup.Should().Contain("Price");
        cut.Markup.Should().Contain("SKU");
        cut.Markup.Should().Contain("Category");
    }

    [Fact]
    public void ProductForm_ShouldValidateRequiredFields()
    {
        // Arrange
        var cut = RenderComponent<ProductForm>();

        // Act
        cut.Find("button[type=submit]").Click();

        // Assert
        cut.Markup.Should().Contain("The Name field is required");
    }
}
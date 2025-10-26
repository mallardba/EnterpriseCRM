using EnterpriseCRM.Core.Entities;
using Xunit;
using FluentAssertions;

namespace EnterpriseCRM.UnitTests.Entities;

public class ProductTests
{
    [Fact]
    public void Product_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test Product",
            Price = 99.99m,
            IsActive = true
        };

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be("Test Product");
        product.Price.Should().Be(99.99m);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Product_ShouldAllowNullOptionalFields()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test",
            Price = 99.99m
        };

        // Assert
        product.Description.Should().BeNull();
        product.SKU.Should().BeNull();
        product.Cost.Should().BeNull();
        product.Category.Should().BeNull();
    }
}

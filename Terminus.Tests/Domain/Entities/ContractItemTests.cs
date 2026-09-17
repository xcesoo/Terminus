using Terminus.Domain.Entities;

namespace Terminus.Tests.Domain.Entities;

public class ContractItemTests
{
    // Constructor stores the product id and quantity as given.
    [Fact]
    public void Constructor_WithValidQuantity_SetsProperties()
    {
        var productId = Guid.NewGuid();

        var item = new ContractItem(productId, 5);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal(5, item.Quantity);
    }

    // A contract line must order a strictly positive quantity.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveQuantity_Throws(int quantity)
    {
        Assert.Throws<ArgumentException>(() => new ContractItem(Guid.NewGuid(), quantity));
    }
}

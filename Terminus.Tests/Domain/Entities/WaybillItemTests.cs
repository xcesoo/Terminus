using Terminus.Domain.Entities;

namespace Terminus.Tests.Domain.Entities;

public class WaybillItemTests
{
    // Constructor stores product id, shipped quantity and the snapshotted price as given.
    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        var productId = Guid.NewGuid();

        var item = new WaybillItem(productId, 10, 350m);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal(10, item.ShippedQuantity);
        Assert.Equal(350m, item.Price);
    }

    // A shipment line must ship a strictly positive quantity.
    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Constructor_WithNonPositiveQuantity_Throws(int quantity)
    {
        Assert.Throws<ArgumentException>(() => new WaybillItem(Guid.NewGuid(), quantity, 100m));
    }

    // The snapshotted price must be strictly positive.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositivePrice_Throws(decimal price)
    {
        Assert.Throws<ArgumentException>(() => new WaybillItem(Guid.NewGuid(), 5, price));
    }
}

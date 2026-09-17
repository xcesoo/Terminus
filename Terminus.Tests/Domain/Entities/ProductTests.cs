using Terminus.Domain.Entities;

namespace Terminus.Tests.Domain.Entities;

public class ProductTests
{
    // Factory sets a generated Id and stores all four fields as given.
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var product = Product.Create("P-001", "Ноутбук", 78000m, "UA-2026-09-17");

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("P-001", product.Code);
        Assert.Equal("Ноутбук", product.Name);
        Assert.Equal(78000m, product.Price);
        Assert.Equal("UA-2026-09-17", product.PriceListNumber);
    }

    // A product must have a strictly positive price — zero or negative is rejected.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePrice_Throws(decimal price)
    {
        Assert.Throws<ArgumentException>(() => Product.Create("P-001", "Ноутбук", price, "UA-2026"));
    }

    // Renaming a product updates the Name property in place.
    [Fact]
    public void ChangeName_WithValidName_UpdatesName()
    {
        var product = Product.Create("P-001", "Old", 100m, "UA-2026");

        product.ChangeName("New");

        Assert.Equal("New", product.Name);
    }

    // A blank name must be rejected.
    [Fact]
    public void ChangeName_WithEmptyName_Throws()
    {
        var product = Product.Create("P-001", "Old", 100m, "UA-2026");

        Assert.Throws<ArgumentException>(() => product.ChangeName(""));
    }

    // Changing the code updates the Code property in place.
    [Fact]
    public void ChangeCode_WithValidCode_UpdatesCode()
    {
        var product = Product.Create("OLD", "Name", 100m, "UA-2026");

        product.ChangeCode("NEW");

        Assert.Equal("NEW", product.Code);
    }

    // A blank code must be rejected.
    [Fact]
    public void ChangeCode_WithEmptyCode_Throws()
    {
        var product = Product.Create("OLD", "Name", 100m, "UA-2026");

        Assert.Throws<ArgumentException>(() => product.ChangeCode(" "));
    }

    // Updating the price also regenerates the price-list number from region + effective date.
    [Fact]
    public void UpdatePrice_WithValidPrice_UpdatesPriceAndPriceListNumber()
    {
        var product = Product.Create("P-001", "Name", 100m, "UA-2026");
        var effectiveDate = new DateTime(2026, 9, 17, 0, 0, 0, DateTimeKind.Utc);

        product.UpdatePrice(150m, effectiveDate, "ua");

        Assert.Equal(150m, product.Price);
        Assert.Equal("UA-2026-09-17", product.PriceListNumber);
    }

    // Omitting the region code falls back to "UA".
    [Fact]
    public void UpdatePrice_DefaultRegionCode_UsesUa()
    {
        var product = Product.Create("P-001", "Name", 100m, "UA-2026");
        var effectiveDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        product.UpdatePrice(200m, effectiveDate);

        Assert.StartsWith("UA-", product.PriceListNumber);
    }

    // A new price must be strictly positive — zero or negative is rejected.
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdatePrice_WithNonPositivePrice_Throws(decimal newPrice)
    {
        var product = Product.Create("P-001", "Name", 100m, "UA-2026");

        Assert.Throws<ArgumentException>(() => product.UpdatePrice(newPrice, DateTime.UtcNow));
    }

    // A blank region code must be rejected.
    [Fact]
    public void UpdatePrice_WithEmptyRegionCode_Throws()
    {
        var product = Product.Create("P-001", "Name", 100m, "UA-2026");

        Assert.Throws<ArgumentException>(() => product.UpdatePrice(150m, DateTime.UtcNow, " "));
    }
}

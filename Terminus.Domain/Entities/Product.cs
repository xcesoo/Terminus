namespace Terminus.Domain.Entities;

public class Product
{
    public Guid Id { get; init; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public string PriceListNumber { get; private set; }

    private readonly List<Contract> _contracts = new();
    public IReadOnlyCollection<Contract> Contracts => _contracts.AsReadOnly();

    private Product() { }

    private Product(string code, string name, decimal price, string priceListNumber)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Price = price;
        PriceListNumber = priceListNumber;
    }

    public static Product Create(string code, string name, decimal price, string priceListNumber)
    {
        if (price <= 0) throw new ArgumentException("Price must be greater than zero");
        return new Product(code, name, price, priceListNumber);
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new ArgumentException("Invalid price");
        Price = newPrice;
    }
}
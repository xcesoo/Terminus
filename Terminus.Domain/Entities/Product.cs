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
        Id = Guid.CreateVersion7();
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

    public void ChangeName(string newName)
    { 
        ArgumentException.ThrowIfNullOrWhiteSpace(newName, "Назва не може бути порожньою.");
        Name = newName;
    }

    public void UpdatePrice(decimal newPrice, DateTime effectiveDate, string regionCode = "UA")
    {
        if (newPrice <= 0) 
            throw new ArgumentException("Ціна має бути більшою за нуль.");
    
        if (string.IsNullOrWhiteSpace(regionCode)) 
            throw new ArgumentException("Код регіону не може бути порожнім.");

        Price = newPrice;
        PriceListNumber = $"{regionCode.ToUpper()}-{effectiveDate:yyyy-MM-dd}";
    }

    public void ChangeCode(string newCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newCode, "Код виробу не може бути порожнім.");
        Code = newCode;
    }
}
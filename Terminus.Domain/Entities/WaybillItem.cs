namespace Terminus.Domain.Entities;

public record WaybillItem
{
    public Guid ProductId { get; init; }
    public int ShippedQuantity { get; init; }

    public Product Product { get; init; } = null!;

    private WaybillItem() { } //EF Core

    public WaybillItem(Guid productId, int shippedQuantity)
    {
        if (shippedQuantity <= 0) throw new ArgumentException("Кількість відвантаження має бути більшою за нуль.");
        ProductId = productId;
        ShippedQuantity = shippedQuantity;
    }
}
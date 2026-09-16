namespace Terminus.Domain.Entities;

public record ContractItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }

    public Product Product { get; init; } = null!;

    private ContractItem()
    {
    } //EF Core

    public ContractItem(Guid productId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Кількість має бути більшою за нуль.");
        ProductId = productId;
        Quantity = quantity;
    }
}
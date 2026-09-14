namespace Terminus.Domain.Entities;

public class Contract
{
    public Guid Id { get; private set; }
    public string ContractNumber { get; private set; }
    public DateTime ConclusionDate { get; private set; }
    public int Quantity { get; private set; }
    
    public Guid ConsumerId { get; private set; }
    public Consumer Consumer { get; private set; }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; }

    private readonly List<Waybill> _waybills = new();
    public IReadOnlyCollection<Waybill> Waybills => _waybills.AsReadOnly();

    private Contract() { }

    private Contract(string contractNumber, Guid consumerId, Guid productId, int quantity, DateTime conclusionDate)
    {
        Id = Guid.NewGuid();
        ContractNumber = contractNumber;
        ConsumerId = consumerId;
        ProductId = productId;
        Quantity = quantity;
        ConclusionDate = conclusionDate;
    }

    public static Contract Create(string contractNumber, Guid consumerId, Guid productId, int quantity, DateTime conclusionDate)
    {
        return new Contract(contractNumber, consumerId, productId, quantity, conclusionDate);
    }
}
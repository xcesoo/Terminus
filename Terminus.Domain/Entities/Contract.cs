using Terminus.Domain.Enums;

namespace Terminus.Domain.Entities;

public class Contract
{
    public Guid Id { get; private set; }
    public string ContractNumber { get; private set; }
    
    public ContractStatus Status { get; private set; } = ContractStatus.Draft;
    public DateTime? ConclusionDate { get; private set; }    
    public Guid ConsumerId { get; private set; }
    public Consumer Consumer { get; private set; }
    
    private readonly List<ContractItem> _items = new();
    public IReadOnlyCollection<ContractItem> Items => _items.AsReadOnly();
    
    private readonly List<Waybill> _waybills = new();
    public IReadOnlyCollection<Waybill> Waybills => _waybills.AsReadOnly();

    private Contract() { }

    private Contract(string contractNumber, Guid consumerId)
    {
        Id = Guid.CreateVersion7();
        ContractNumber = contractNumber;
        ConsumerId = consumerId;
    }

    public static Contract Create(string contractNumber, Guid consumerId, IEnumerable<ContractItem> items)
    {
        var contract = new Contract(contractNumber, consumerId);
        contract._items.AddRange(items);
        return contract;
    }
    
    public void Terminate()
    {
        if (Status == ContractStatus.Terminated) 
            throw new InvalidOperationException("Договір вже розірвано.");
        
        Status = ContractStatus.Terminated;
    }
    
    public void Sign(DateTime conclusionDate)
    {
        if (Status != ContractStatus.Draft) 
            throw new InvalidOperationException("Підписати можна лише договір у статусі 'Чернетка'.");
        
        Status = ContractStatus.Signed;
        ConclusionDate = conclusionDate;
    }
}
using Terminus.Domain.Enums;

namespace Terminus.Domain.Entities;

public abstract class Waybill
{
    public Guid Id { get; private set; }
    public string WaybillNumber { get; private set; } 
    
    public WaybillStatus Status { get; private set; } = WaybillStatus.Draft;
    public DateTime? DispatchDate { get; private set; }   
    
    protected readonly List<WaybillItem> _items = new();
    public IReadOnlyCollection<WaybillItem> Items => _items.AsReadOnly();

    public Guid ContractId { get; private set; }
    public Contract Contract { get; private set; }

    protected Waybill() { }

    protected Waybill(string waybillNumber, Guid contractId)
    {
        Id = Guid.CreateVersion7();
        WaybillNumber = waybillNumber;
        ContractId = contractId;
    }
    public void Cancel()
    {
        if (Status == WaybillStatus.Cancelled) throw new InvalidOperationException("ТТН вже скасовано."); 
        Status = WaybillStatus.Cancelled;
    }
    
    public void Dispatch(DateTime dispatchDate)
    {
        if (Status != WaybillStatus.Draft) 
            throw new InvalidOperationException("Відвантажити можна лише ТТН у статусі 'Чернетка'.");
        
        Status = WaybillStatus.Dispatched;
        DispatchDate = dispatchDate;
    }
}

public class AutoWaybill : Waybill
{
    public string CarNumber { get; private set; }
    public string RouteSheetNumber { get; private set; } 
    public decimal AutoServiceSum { get; private set; } 

    private AutoWaybill() { }

    private AutoWaybill(string waybillNumber, Guid contractId, string carNumber, string routeSheetNumber, decimal serviceSum) 
        : base(waybillNumber, contractId)
    {
        CarNumber = carNumber;
        RouteSheetNumber = routeSheetNumber;
        AutoServiceSum = serviceSum;
    }

    public static AutoWaybill Create(string waybillNumber, Guid contractId, 
        string carNumber, string routeSheetNumber, decimal serviceSum, IEnumerable<WaybillItem> items)
    {
        var waybill = new AutoWaybill(waybillNumber, contractId, carNumber, routeSheetNumber, serviceSum);
        waybill._items.AddRange(items);
        return waybill;
    }
}

public class TrainWaybill : Waybill
{
    public string ContainerNumber { get; private set; }
    public string RailwayReceiptNumber { get; private set; } 
    public decimal TrainServiceSum { get; private set; }

    private TrainWaybill() { }

    private TrainWaybill(string waybillNumber, Guid contractId, string containerNumber, string railwayReceiptNumber, decimal serviceSum) 
        : base(waybillNumber, contractId)
    {
        ContainerNumber = containerNumber;
        RailwayReceiptNumber = railwayReceiptNumber;
        TrainServiceSum = serviceSum;
    }

    public static TrainWaybill Create(string waybillNumber, Guid contractId, 
        string containerNumber, string receiptNumber, decimal serviceSum, IEnumerable<WaybillItem> items)
    {
        var waybill = new TrainWaybill(waybillNumber, contractId, containerNumber, receiptNumber, serviceSum);
        waybill._items.AddRange(items);
        return waybill;
    }
}

public class AviaWaybill : Waybill
{
    public string FlightNumber { get; private set; } 
    public string AviaReceiptNumber { get; private set; }
    public decimal AviaServiceSum { get; private set; }

    private AviaWaybill() { }

    private AviaWaybill(string waybillNumber, Guid contractId, string flightNumber, string aviaReceiptNumber, decimal serviceSum) 
        : base(waybillNumber, contractId)
    {
        FlightNumber = flightNumber;
        AviaReceiptNumber = aviaReceiptNumber;
        AviaServiceSum = serviceSum;
    }

    public static AviaWaybill Create(string waybillNumber, Guid contractId, 
        string flightNumber, string receiptNumber, decimal serviceSum, IEnumerable<WaybillItem> items)
    {
        var waybill = new AviaWaybill(waybillNumber, contractId, flightNumber, receiptNumber, serviceSum);
        waybill._items.AddRange(items);
        return waybill;
    }
}
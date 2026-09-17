using Terminus.Domain.Enums;
using Terminus.Domain.ValueObjects;

namespace Terminus.Domain.Entities;

public abstract class Waybill
{
    public Guid Id { get; private set; }
    public string WaybillNumber { get; private set; }

    public WaybillStatus Status { get; private set; } = WaybillStatus.Draft;
    public DateTime? DispatchDate { get; private set; }
    public decimal DeliveryBaseCost { get; private set; }
    public decimal DeliveryCommissionCost { get; private set; }
    public decimal DeliveryTransportMultiplier { get; private set; }

    protected readonly List<WaybillItem> _items = new();
    public IReadOnlyCollection<WaybillItem> Items => _items.AsReadOnly();

    public Guid ContractId { get; private set; }
    public Contract Contract { get; private set; }

    protected Waybill() { }

    protected Waybill(string waybillNumber, Guid contractId, DeliveryCostBreakdown deliveryCost)
    {
        Id = Guid.CreateVersion7();
        WaybillNumber = waybillNumber;
        ContractId = contractId;
        DeliveryBaseCost = deliveryCost.BaseCost;
        DeliveryCommissionCost = deliveryCost.CommissionCost;
        DeliveryTransportMultiplier = deliveryCost.TransportMultiplier;
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

    private AutoWaybill(string waybillNumber, Guid contractId, string carNumber, string routeSheetNumber, DeliveryCostBreakdown deliveryCost)
        : base(waybillNumber, contractId, deliveryCost)
    {
        CarNumber = carNumber;
        RouteSheetNumber = routeSheetNumber;
        AutoServiceSum = deliveryCost.Total;
    }

    public static AutoWaybill Create(string waybillNumber, Guid contractId,
        string carNumber, string routeSheetNumber, DeliveryCostBreakdown deliveryCost, IEnumerable<WaybillItem> items)
    {
        var waybill = new AutoWaybill(waybillNumber, contractId, carNumber, routeSheetNumber, deliveryCost);
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

    private TrainWaybill(string waybillNumber, Guid contractId, string containerNumber, string railwayReceiptNumber, DeliveryCostBreakdown deliveryCost)
        : base(waybillNumber, contractId, deliveryCost)
    {
        ContainerNumber = containerNumber;
        RailwayReceiptNumber = railwayReceiptNumber;
        TrainServiceSum = deliveryCost.Total;
    }

    public static TrainWaybill Create(string waybillNumber, Guid contractId,
        string containerNumber, string receiptNumber, DeliveryCostBreakdown deliveryCost, IEnumerable<WaybillItem> items)
    {
        var waybill = new TrainWaybill(waybillNumber, contractId, containerNumber, receiptNumber, deliveryCost);
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

    private AviaWaybill(string waybillNumber, Guid contractId, string flightNumber, string aviaReceiptNumber, DeliveryCostBreakdown deliveryCost)
        : base(waybillNumber, contractId, deliveryCost)
    {
        FlightNumber = flightNumber;
        AviaReceiptNumber = aviaReceiptNumber;
        AviaServiceSum = deliveryCost.Total;
    }

    public static AviaWaybill Create(string waybillNumber, Guid contractId,
        string flightNumber, string receiptNumber, DeliveryCostBreakdown deliveryCost, IEnumerable<WaybillItem> items)
    {
        var waybill = new AviaWaybill(waybillNumber, contractId, flightNumber, receiptNumber, deliveryCost);
        waybill._items.AddRange(items);
        return waybill;
    }
}
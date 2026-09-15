using Terminus.Domain.Enums;

namespace Terminus.Domain.Entities;

public abstract class Waybill
{
    public Guid Id { get; private set; }
    public string WaybillNumber { get; private set; } 
    
    public WaybillStatus Status { get; private set; } = WaybillStatus.Draft;
    public DateTime? DispatchDate { get; private set; }    
    public int ShippedQuantity { get; private set; }

    public Guid ContractId { get; private set; }
    public Contract Contract { get; private set; }

    protected Waybill() { }

    protected Waybill(string waybillNumber, Guid contractId, int shippedQuantity)
    {
        Id = Guid.CreateVersion7();
        WaybillNumber = waybillNumber;
        ContractId = contractId;
        ShippedQuantity = shippedQuantity;
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

    private AutoWaybill(string waybillNumber, Guid contractId, int shippedQuantity, string carNumber, string routeSheetNumber, decimal serviceSum) 
        : base(waybillNumber, contractId, shippedQuantity)
    {
        CarNumber = carNumber;
        RouteSheetNumber = routeSheetNumber;
        AutoServiceSum = serviceSum;
    }

    public static AutoWaybill Create(string waybillNumber, Guid contractId, int shippedQuantity, 
        string carNumber, string routeSheetNumber, decimal serviceSum)
    {
        return new AutoWaybill(waybillNumber, contractId, shippedQuantity, carNumber, routeSheetNumber, serviceSum);
    }
}

public class TrainWaybill : Waybill
{
    public string ContainerNumber { get; private set; }
    public string RailwayReceiptNumber { get; private set; } 
    public decimal TrainServiceSum { get; private set; }

    private TrainWaybill() { }

    private TrainWaybill(string waybillNumber, Guid contractId, int shippedQuantity, 
        string containerNumber, string railwayReceiptNumber, decimal serviceSum) 
        : base(waybillNumber, contractId, shippedQuantity)
    {
        ContainerNumber = containerNumber;
        RailwayReceiptNumber = railwayReceiptNumber;
        TrainServiceSum = serviceSum;
    }

    public static TrainWaybill Create(string waybillNumber, Guid contractId, int shippedQuantity,    
        string containerNumber, string receiptNumber, decimal serviceSum)
    {
        return new TrainWaybill(waybillNumber, contractId, shippedQuantity, containerNumber, receiptNumber, serviceSum);
    }
}

public class AviaWaybill : Waybill
{
    public string FlightNumber { get; private set; } 
    public string AviaReceiptNumber { get; private set; }
    public decimal AviaServiceSum { get; private set; }

    private AviaWaybill() { }

    private AviaWaybill(string waybillNumber, Guid contractId, int shippedQuantity, 
        string flightNumber, string aviaReceiptNumber, decimal serviceSum) 
        : base(waybillNumber, contractId, shippedQuantity)
    {
        FlightNumber = flightNumber;
        AviaReceiptNumber = aviaReceiptNumber;
        AviaServiceSum = serviceSum;
    }

    public static AviaWaybill Create(string waybillNumber, Guid contractId, int shippedQuantity,     
        string flightNumber, string receiptNumber, decimal serviceSum)
    {
        return new AviaWaybill(waybillNumber, contractId, shippedQuantity, flightNumber, receiptNumber, serviceSum);
    }
}
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.ValueObjects;

namespace Terminus.Tests.Domain.Entities;

public class WaybillTests
{
    private static readonly Guid ContractId = Guid.NewGuid();

    private static WaybillItem CreateItem(int qty = 5, decimal price = 100m) =>
        new(Guid.NewGuid(), qty, price);

    private static AutoWaybill CreateAutoWaybill(DeliveryCostBreakdown? deliveryCost = null) =>
        AutoWaybill.Create(
            "WB-0001", ContractId, "KA0000KA", "PL-001",
            deliveryCost ?? new DeliveryCostBreakdown(500m, 20m, 1.0m),
            new[] { CreateItem() });

    // AutoWaybill.Create stores its transport-specific fields and computes AutoServiceSum from the delivery breakdown.
    [Fact]
    public void AutoWaybill_Create_SetsPropertiesAndComputesServiceSum()
    {
        var deliveryCost = new DeliveryCostBreakdown(BaseCost: 500m, CommissionCost: 20m, TransportMultiplier: 1.5m);

        var waybill = AutoWaybill.Create("WB-0001", ContractId, "KA0000KA", "PL-001", deliveryCost, new[] { CreateItem() });

        Assert.Equal("WB-0001", waybill.WaybillNumber);
        Assert.Equal(ContractId, waybill.ContractId);
        Assert.Equal("KA0000KA", waybill.CarNumber);
        Assert.Equal("PL-001", waybill.RouteSheetNumber);
        Assert.Equal(780m, waybill.AutoServiceSum); // (500 + 20) * 1.5
        Assert.Equal(500m, waybill.DeliveryBaseCost);
        Assert.Equal(20m, waybill.DeliveryCommissionCost);
        Assert.Equal(1.5m, waybill.DeliveryTransportMultiplier);
        Assert.Equal(WaybillStatus.Draft, waybill.Status);
        Assert.Null(waybill.DispatchDate);
        Assert.Single(waybill.Items);
    }

    // TrainWaybill.Create stores its own transport-specific fields and computes TrainServiceSum.
    [Fact]
    public void TrainWaybill_Create_SetsPropertiesAndComputesServiceSum()
    {
        var deliveryCost = new DeliveryCostBreakdown(100m, 10m, 2m);

        var waybill = TrainWaybill.Create("WB-0002", ContractId, "UZ-1", "RECEIPT-1", deliveryCost, new[] { CreateItem() });

        Assert.Equal("UZ-1", waybill.ContainerNumber);
        Assert.Equal("RECEIPT-1", waybill.RailwayReceiptNumber);
        Assert.Equal(220m, waybill.TrainServiceSum); // (100 + 10) * 2
    }

    // AviaWaybill.Create stores its own transport-specific fields and computes AviaServiceSum.
    [Fact]
    public void AviaWaybill_Create_SetsPropertiesAndComputesServiceSum()
    {
        var deliveryCost = new DeliveryCostBreakdown(200m, 0m, 3m);

        var waybill = AviaWaybill.Create("WB-0003", ContractId, "PS-101", "AV-RECEIPT-1", deliveryCost, new[] { CreateItem() });

        Assert.Equal("PS-101", waybill.FlightNumber);
        Assert.Equal("AV-RECEIPT-1", waybill.AviaReceiptNumber);
        Assert.Equal(600m, waybill.AviaServiceSum); // (200 + 0) * 3
    }

    // Dispatching a Draft waybill moves it to Dispatched and records the dispatch date.
    [Fact]
    public void Dispatch_FromDraft_SetsDispatchedStatusAndDate()
    {
        var waybill = CreateAutoWaybill();
        var date = new DateTime(2026, 9, 17, 0, 0, 0, DateTimeKind.Utc);

        waybill.Dispatch(date);

        Assert.Equal(WaybillStatus.Dispatched, waybill.Status);
        Assert.Equal(date, waybill.DispatchDate);
    }

    // A waybill cannot be dispatched twice.
    [Fact]
    public void Dispatch_WhenAlreadyDispatched_Throws()
    {
        var waybill = CreateAutoWaybill();
        waybill.Dispatch(DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() => waybill.Dispatch(DateTime.UtcNow));
    }

    // A cancelled waybill can no longer be dispatched.
    [Fact]
    public void Dispatch_WhenCancelled_Throws()
    {
        var waybill = CreateAutoWaybill();
        waybill.Cancel();

        Assert.Throws<InvalidOperationException>(() => waybill.Dispatch(DateTime.UtcNow));
    }

    // Cancelling a Draft waybill moves it to Cancelled.
    [Fact]
    public void Cancel_FromDraft_SetsCancelledStatus()
    {
        var waybill = CreateAutoWaybill();

        waybill.Cancel();

        Assert.Equal(WaybillStatus.Cancelled, waybill.Status);
    }

    // Cancelling is also allowed after dispatch (voiding an already-shipped waybill) — confirmed intentional.
    [Fact]
    public void Cancel_FromDispatched_SetsCancelledStatus()
    {
        var waybill = CreateAutoWaybill();
        waybill.Dispatch(DateTime.UtcNow);

        waybill.Cancel();

        Assert.Equal(WaybillStatus.Cancelled, waybill.Status);
    }

    // A waybill cannot be cancelled twice.
    [Fact]
    public void Cancel_WhenAlreadyCancelled_Throws()
    {
        var waybill = CreateAutoWaybill();
        waybill.Cancel();

        Assert.Throws<InvalidOperationException>(() => waybill.Cancel());
    }
}

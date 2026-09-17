using Terminus.Application.Extensions;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.ValueObjects;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Extensions;

public class MappingExtensionsTests
{
    // Consumer -> ConsumerDto must copy all four fields verbatim.
    [Fact]
    public void MapToDto_Consumer_MapsAllFields()
    {
        var consumer = TestFactory.CreateConsumer("Name", "Address", "Bank");

        var dto = consumer.MapToDto();

        Assert.Equal(consumer.Id, dto.Id);
        Assert.Equal("Name", dto.Name);
        Assert.Equal("Address", dto.Address);
        Assert.Equal("Bank", dto.BankAccount);
    }

    // Product -> ProductDto must copy all five fields verbatim.
    [Fact]
    public void MapToDto_Product_MapsAllFields()
    {
        var product = TestFactory.CreateProduct("CODE", "Name", 123m, "PL-1");

        var dto = product.MapToDto();

        Assert.Equal(product.Id, dto.Id);
        Assert.Equal("CODE", dto.Code);
        Assert.Equal("Name", dto.Name);
        Assert.Equal(123m, dto.Price);
        Assert.Equal("PL-1", dto.PriceListNumber);
    }

    // Contract -> ContractDto must compute each line's TotalPrice (price * quantity) and sum them into TotalAmount.
    [Fact]
    public void MapToDto_Contract_ComputesItemTotalsAndOverallTotal()
    {
        var consumer = TestFactory.CreateConsumer();
        var productA = TestFactory.CreateProduct(code: "A", name: "Product A", price: 100m);
        var productB = TestFactory.CreateProduct(code: "B", name: "Product B", price: 50m);
        var contract = TestFactory.CreateContract(consumer, new[]
        {
            TestFactory.CreateContractItem(productA, 2),
            TestFactory.CreateContractItem(productB, 3)
        });

        var dto = contract.MapToDto();

        Assert.Equal(contract.ContractNumber, dto.ContractNumber);
        Assert.Equal(2, dto.Items.Count());
        Assert.Equal(200m, dto.Items.First(i => i.ProductName == productA.Name).TotalPrice);
        Assert.Equal(150m, dto.Items.First(i => i.ProductName == productB.Name).TotalPrice);
        Assert.Equal(350m, dto.TotalAmount);
    }

    private static AutoWaybill CreateLinkedAutoWaybill(out Product product)
    {
        var consumer = TestFactory.CreateConsumer("Consumer", "Address", "Bank");
        product = TestFactory.CreateProduct(price: 200m);
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });

        var waybill = AutoWaybill.Create(
            "WB-0001", contract.Id, "KA0000KA", "PL-1",
            new DeliveryCostBreakdown(100m, 20m, 2m),
            new[] { TestFactory.CreateWaybillItem(product, 5, price: 200m) });
        TestFactory.LinkWaybillToContract(waybill, contract);
        return waybill;
    }

    // AutoWaybill -> WaybillDto must resolve to the "Auto" transport label/code, denormalize the consumer's data
    // from the linked contract, and roll up products + delivery cost into the final totals.
    [Fact]
    public void MapToDto_AutoWaybill_MapsTransportSpecificFieldsAndTotals()
    {
        var waybill = CreateLinkedAutoWaybill(out _);

        var dto = waybill.MapToDto();

        Assert.Equal(waybill.WaybillNumber, dto.WaybillNumber);
        Assert.Equal(waybill.Contract.ContractNumber, dto.ContractNumber);
        Assert.Equal("Consumer", dto.ConsumerName);
        Assert.Equal(TransportType.Auto, dto.TransportTypeCode);
        Assert.Equal("Автомобіль", dto.TransportType);
        Assert.Contains("KA0000KA", dto.TransportDetails);
        Assert.Equal(1000m, dto.ProductsTotalSum); // 5 * 200
        Assert.Equal(240m, dto.TransportServiceSum); // (100 + 20) * 2 == AutoServiceSum
        Assert.Equal(1240m, dto.TotalAmount);
        Assert.Equal(100m, dto.DeliveryBaseCost);
        Assert.Equal(20m, dto.DeliveryCommissionCost);
        Assert.Equal(2m, dto.DeliveryTransportMultiplier);
    }

    // A shipped item's DTO price must come from the WaybillItem's own snapshot, not a later live Product.Price change.
    [Fact]
    public void MapToDto_WaybillItem_UsesSnapshottedPriceNotLiveProductPrice()
    {
        var waybill = CreateLinkedAutoWaybill(out var product);

        product.UpdatePrice(999m, DateTime.UtcNow);
        var dto = waybill.MapToDto();

        var item = Assert.Single(dto.Items);
        Assert.Equal(200m, item.Price); // snapshotted at shipment time, not the live 999m
    }

    // TrainWaybill -> WaybillDto must resolve to the "Train" label/code and expose its own container/receipt numbers.
    [Fact]
    public void MapToDto_TrainWaybill_MapsContainerAndReceiptNumber()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });
        var waybill = TrainWaybill.Create(
            "WB-0002", contract.Id, "UZ-1", "RCPT-1",
            new DeliveryCostBreakdown(0m, 0m, 1m),
            new[] { TestFactory.CreateWaybillItem(product, 1) });
        TestFactory.LinkWaybillToContract(waybill, contract);

        var dto = waybill.MapToDto();

        Assert.Equal(TransportType.Train, dto.TransportTypeCode);
        Assert.Equal("Залізниця", dto.TransportType);
        Assert.Contains("UZ-1", dto.TransportDetails);
    }

    // AviaWaybill -> WaybillDto must resolve to the "Avia" label/code and expose its own flight/receipt numbers.
    [Fact]
    public void MapToDto_AviaWaybill_MapsFlightAndReceiptNumber()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });
        var waybill = AviaWaybill.Create(
            "WB-0003", contract.Id, "PS-101", "AV-1",
            new DeliveryCostBreakdown(0m, 0m, 1m),
            new[] { TestFactory.CreateWaybillItem(product, 1) });
        TestFactory.LinkWaybillToContract(waybill, contract);

        var dto = waybill.MapToDto();

        Assert.Equal(TransportType.Avia, dto.TransportTypeCode);
        Assert.Equal("Авіа", dto.TransportType);
        Assert.Contains("PS-101", dto.TransportDetails);
    }
}

using Moq;
using Terminus.Application.Commands.Waybills;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Waybills;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Domain.ValueObjects;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Waybills;

public class CreateWaybillCommandTests
{
    private static IRuleEngine RealRuleEngine() =>
        new RuleEngine(new IBusinessRule[] { new ActiveContractRule(), new WaybillQuantityRule() });

    private static (Contract contract, Product product) CreateSignedContractWithProduct(int contractQuantity = 10, decimal price = 1800m)
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct(price: price);
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, contractQuantity) });
        contract.Sign(DateTime.UtcNow);
        return (contract, product);
    }

    private static Mock<IDeliveryCalculatorService> DeliveryCalculatorMock(DeliveryCostBreakdown breakdown)
    {
        var mock = new Mock<IDeliveryCalculatorService>();
        mock.Setup(d => d.Calculate(It.IsAny<TransportType>(), It.IsAny<int>(), It.IsAny<decimal>())).Returns(breakdown);
        return mock;
    }

    // A valid Auto request must build an AutoWaybill whose item price is snapshotted from the contract's product price, and commit.
    [Fact]
    public async Task Handle_ValidAutoRequest_CreatesAutoWaybillWithSnapshotPriceAndSaves()
    {
        var (contract, product) = CreateSignedContractWithProduct(contractQuantity: 10, price: 1800m);
        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var waybillRepo = new Mock<IWaybillRepository>();
        Waybill? added = null;
        waybillRepo.Setup(r => r.AddAsync(It.IsAny<Waybill>(), It.IsAny<CancellationToken>()))
            .Callback<Waybill, CancellationToken>((w, _) => added = w)
            .Returns(Task.CompletedTask);
        var uow = new Mock<IUnitOfWork>();
        var deliveryCalculator = DeliveryCalculatorMock(new DeliveryCostBreakdown(500m, 50m, 1.0m));
        var numberGenerator = new Mock<IDocumentNumberGenerator>();
        numberGenerator.Setup(g => g.GenerateWaybillNumber()).Returns("WB-TEST-0001");

        var handler = new CreateWaybillCommandHandler(
            contractRepo.Object, waybillRepo.Object, uow.Object, RealRuleEngine(), deliveryCalculator.Object, numberGenerator.Object);

        var command = new CreateWaybillCommand(
            contract.Id, new[] { new WaybillItemRequestDto(product.Id, 5) }, TransportType.Auto, "KA0000KA", "PL-001");

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(added);
        var autoWaybill = Assert.IsType<AutoWaybill>(added);
        Assert.Equal(id, autoWaybill.Id);
        Assert.Equal("WB-TEST-0001", autoWaybill.WaybillNumber);
        Assert.Equal("KA0000KA", autoWaybill.CarNumber);
        Assert.Equal(550m, autoWaybill.AutoServiceSum); // (500 + 50) * 1.0
        var item = Assert.Single(autoWaybill.Items);
        Assert.Equal(1800m, item.Price); // snapshotted from contract item's product price
        Assert.Equal(5, item.ShippedQuantity);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // TransportType.Train must produce a TrainWaybill with its own transport-specific fields.
    [Fact]
    public async Task Handle_TrainTransport_CreatesTrainWaybill()
    {
        var (contract, product) = CreateSignedContractWithProduct();
        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var waybillRepo = new Mock<IWaybillRepository>();
        Waybill? added = null;
        waybillRepo.Setup(r => r.AddAsync(It.IsAny<Waybill>(), It.IsAny<CancellationToken>()))
            .Callback<Waybill, CancellationToken>((w, _) => added = w)
            .Returns(Task.CompletedTask);
        var uow = new Mock<IUnitOfWork>();
        var deliveryCalculator = DeliveryCalculatorMock(new DeliveryCostBreakdown(100m, 10m, 1.5m));
        var numberGenerator = new Mock<IDocumentNumberGenerator>();
        numberGenerator.Setup(g => g.GenerateWaybillNumber()).Returns("WB-TEST-0002");

        var handler = new CreateWaybillCommandHandler(
            contractRepo.Object, waybillRepo.Object, uow.Object, RealRuleEngine(), deliveryCalculator.Object, numberGenerator.Object);

        var command = new CreateWaybillCommand(
            contract.Id, new[] { new WaybillItemRequestDto(product.Id, 3) }, TransportType.Train, "UZ-1", "RCPT-1");

        await handler.Handle(command, CancellationToken.None);

        var trainWaybill = Assert.IsType<TrainWaybill>(added);
        Assert.Equal("UZ-1", trainWaybill.ContainerNumber);
    }

    // The real ActiveContractRule must block creating a waybill against an unsigned contract.
    [Fact]
    public async Task Handle_ContractNotSigned_ThrowsAndDoesNotCreate()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });
        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var waybillRepo = new Mock<IWaybillRepository>();
        var uow = new Mock<IUnitOfWork>();
        var deliveryCalculator = DeliveryCalculatorMock(new DeliveryCostBreakdown(0m, 0m, 1m));
        var numberGenerator = new Mock<IDocumentNumberGenerator>();

        var handler = new CreateWaybillCommandHandler(
            contractRepo.Object, waybillRepo.Object, uow.Object, RealRuleEngine(), deliveryCalculator.Object, numberGenerator.Object);

        var command = new CreateWaybillCommand(
            contract.Id, new[] { new WaybillItemRequestDto(product.Id, 5) }, TransportType.Auto, "KA0000KA", "PL-001");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Contains("не підписано", ex.Message);
        waybillRepo.Verify(r => r.AddAsync(It.IsAny<Waybill>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // The real WaybillQuantityRule must block shipping more than the contract has left and prevent persistence.
    [Fact]
    public async Task Handle_RequestedQuantityExceedsContractLimit_Throws()
    {
        var (contract, product) = CreateSignedContractWithProduct(contractQuantity: 5);
        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var waybillRepo = new Mock<IWaybillRepository>();
        var uow = new Mock<IUnitOfWork>();
        var deliveryCalculator = DeliveryCalculatorMock(new DeliveryCostBreakdown(0m, 0m, 1m));
        var numberGenerator = new Mock<IDocumentNumberGenerator>();

        var handler = new CreateWaybillCommandHandler(
            contractRepo.Object, waybillRepo.Object, uow.Object, RealRuleEngine(), deliveryCalculator.Object, numberGenerator.Object);

        var command = new CreateWaybillCommand(
            contract.Id, new[] { new WaybillItemRequestDto(product.Id, 6) }, TransportType.Auto, "KA0000KA", "PL-001");

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        waybillRepo.Verify(r => r.AddAsync(It.IsAny<Waybill>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Requesting a product absent from the contract must surface the rule engine's friendly message, not a raw
    // KeyNotFoundException from the internal dictionary lookup used to snapshot prices.
    [Fact]
    public async Task Handle_ProductNotInContract_ThrowsBeforeConstructingWaybillItem()
    {
        var (contract, _) = CreateSignedContractWithProduct();
        var otherProductId = Guid.NewGuid();
        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var waybillRepo = new Mock<IWaybillRepository>();
        var uow = new Mock<IUnitOfWork>();
        var deliveryCalculator = DeliveryCalculatorMock(new DeliveryCostBreakdown(0m, 0m, 1m));
        var numberGenerator = new Mock<IDocumentNumberGenerator>();

        var handler = new CreateWaybillCommandHandler(
            contractRepo.Object, waybillRepo.Object, uow.Object, RealRuleEngine(), deliveryCalculator.Object, numberGenerator.Object);

        var command = new CreateWaybillCommand(
            contract.Id, new[] { new WaybillItemRequestDto(otherProductId, 1) }, TransportType.Auto, "KA0000KA", "PL-001");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("відсутній у цьому договорі", ex.Message);
    }

    // Creating a waybill against a non-existent contract must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ContractNotFound_Throws()
    {
        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Contract?)null);
        var waybillRepo = new Mock<IWaybillRepository>();
        var uow = new Mock<IUnitOfWork>();
        var deliveryCalculator = DeliveryCalculatorMock(new DeliveryCostBreakdown(0m, 0m, 1m));
        var numberGenerator = new Mock<IDocumentNumberGenerator>();

        var handler = new CreateWaybillCommandHandler(
            contractRepo.Object, waybillRepo.Object, uow.Object, RealRuleEngine(), deliveryCalculator.Object, numberGenerator.Object);

        var command = new CreateWaybillCommand(
            Guid.NewGuid(), new[] { new WaybillItemRequestDto(Guid.NewGuid(), 1) }, TransportType.Auto, "KA0000KA", "PL-001");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}

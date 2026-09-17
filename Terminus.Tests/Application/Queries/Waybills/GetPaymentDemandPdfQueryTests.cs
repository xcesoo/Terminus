using Moq;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Application.Queries.Waybills;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Domain.ValueObjects;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Queries.Waybills;

public class GetPaymentDemandPdfQueryTests
{
    private static AutoWaybill CreateDispatchedWaybill()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });
        contract.Sign(DateTime.UtcNow);

        var waybill = AutoWaybill.Create(
            "WB-0001", contract.Id, "KA0000KA", "PL-1",
            new DeliveryCostBreakdown(100m, 10m, 1m),
            new[] { TestFactory.CreateWaybillItem(product, 5) });
        TestFactory.LinkWaybillToContract(waybill, contract);
        waybill.Dispatch(DateTime.UtcNow);
        return waybill;
    }

    // A dispatched waybill must produce whatever bytes the injected generator returns, under a name derived from its number.
    [Fact]
    public async Task Handle_DispatchedWaybill_ReturnsGeneratedPdf()
    {
        var waybill = CreateDispatchedWaybill();
        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(waybill.Id, It.IsAny<CancellationToken>())).ReturnsAsync(waybill);
        var generator = new Mock<IPaymentDemandPdfGenerator>();
        generator.Setup(g => g.Generate(It.IsAny<WaybillDto>())).Returns([1, 2, 3]);

        var handler = new GetPaymentDemandPdfQueryHandler(repo.Object, generator.Object);

        var (content, fileName) = await handler.Handle(new GetPaymentDemandPdfQuery(waybill.Id), CancellationToken.None);

        Assert.Equal(new byte[] { 1, 2, 3 }, content);
        Assert.Equal($"PaymentDemand_{waybill.WaybillNumber}.pdf", fileName);
    }

    // A payment demand must never be printable for a waybill that hasn't shipped yet (Draft status).
    [Fact]
    public async Task Handle_DraftWaybill_ThrowsAndDoesNotGeneratePdf()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });
        contract.Sign(DateTime.UtcNow);
        var waybill = AutoWaybill.Create(
            "WB-0001", contract.Id, "KA0000KA", "PL-1",
            new DeliveryCostBreakdown(0m, 0m, 1m),
            new[] { TestFactory.CreateWaybillItem(product, 5) });
        TestFactory.LinkWaybillToContract(waybill, contract);

        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(waybill.Id, It.IsAny<CancellationToken>())).ReturnsAsync(waybill);
        var generator = new Mock<IPaymentDemandPdfGenerator>();

        var handler = new GetPaymentDemandPdfQueryHandler(repo.Object, generator.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new GetPaymentDemandPdfQuery(waybill.Id), CancellationToken.None));

        Assert.Contains("не відвантажено", ex.Message);
        generator.Verify(g => g.Generate(It.IsAny<WaybillDto>()), Times.Never);
    }

    // Requesting a PDF for a non-existent waybill must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_WaybillNotFound_Throws()
    {
        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Waybill?)null);
        var generator = new Mock<IPaymentDemandPdfGenerator>();

        var handler = new GetPaymentDemandPdfQueryHandler(repo.Object, generator.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetPaymentDemandPdfQuery(Guid.NewGuid()), CancellationToken.None));
    }
}

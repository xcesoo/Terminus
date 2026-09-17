using Moq;
using Terminus.Application.Commands.Waybills;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Waybills;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Domain.ValueObjects;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Waybills;

public class DispatchWaybillCommandTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 17, 0, 0, 0, TimeSpan.Zero);

    private static IRuleEngine RealRuleEngine() =>
        new RuleEngine(new IBusinessRule[]
        {
            new WaybillChronologyRule(),
            new WaybillFutureDateRule(new FixedTimeProvider(Now))
        });

    private static AutoWaybill CreateDraftWaybillOnSignedContract(DateTime conclusionDate)
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });
        contract.Sign(conclusionDate);

        var waybill = AutoWaybill.Create(
            "WB-0001", contract.Id, "KA0000KA", "PL-1",
            new DeliveryCostBreakdown(0m, 0m, 1m),
            new[] { TestFactory.CreateWaybillItem(product, 5) });
        TestFactory.LinkWaybillToContract(waybill, contract);
        return waybill;
    }

    // A valid dispatch date (on/after conclusion, not in the future) must move the waybill to Dispatched and commit.
    [Fact]
    public async Task Handle_ValidDispatchDate_DispatchesAndSaves()
    {
        var waybill = CreateDraftWaybillOnSignedContract(Now.UtcDateTime.AddDays(-5));
        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(waybill.Id, It.IsAny<CancellationToken>())).ReturnsAsync(waybill);
        var uow = new Mock<IUnitOfWork>();

        var handler = new DispatchWaybillCommandHandler(repo.Object, RealRuleEngine(), uow.Object);

        await handler.Handle(new DispatchWaybillCommand(waybill.Id, Now.UtcDateTime), CancellationToken.None);

        Assert.Equal(WaybillStatus.Dispatched, waybill.Status);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // The real WaybillChronologyRule must block a dispatch date earlier than the contract's own signing date.
    [Fact]
    public async Task Handle_DispatchDateBeforeContractSigned_ThrowsAndDoesNotDispatch()
    {
        var waybill = CreateDraftWaybillOnSignedContract(Now.UtcDateTime);
        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(waybill.Id, It.IsAny<CancellationToken>())).ReturnsAsync(waybill);
        var uow = new Mock<IUnitOfWork>();

        var handler = new DispatchWaybillCommandHandler(repo.Object, RealRuleEngine(), uow.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new DispatchWaybillCommand(waybill.Id, Now.UtcDateTime.AddDays(-1)), CancellationToken.None));

        Assert.Contains("ранішою", ex.Message);
        Assert.Equal(WaybillStatus.Draft, waybill.Status);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // The real WaybillFutureDateRule must block a dispatch date in the future.
    [Fact]
    public async Task Handle_DispatchDateInFuture_Throws()
    {
        var waybill = CreateDraftWaybillOnSignedContract(Now.UtcDateTime.AddDays(-5));
        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(waybill.Id, It.IsAny<CancellationToken>())).ReturnsAsync(waybill);
        var uow = new Mock<IUnitOfWork>();

        var handler = new DispatchWaybillCommandHandler(repo.Object, RealRuleEngine(), uow.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new DispatchWaybillCommand(waybill.Id, Now.UtcDateTime.AddDays(1)), CancellationToken.None));
    }

    // Dispatching a non-existent waybill must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_WaybillNotFound_Throws()
    {
        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Waybill?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new DispatchWaybillCommandHandler(repo.Object, RealRuleEngine(), uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new DispatchWaybillCommand(Guid.NewGuid(), Now.UtcDateTime), CancellationToken.None));
    }
}

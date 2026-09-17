using Moq;
using Terminus.Application.Commands.Waybills;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Domain.ValueObjects;

namespace Terminus.Tests.Application.Commands.Waybills;

public class CancelWaybillCommandTests
{
    private static AutoWaybill CreateWaybill() =>
        AutoWaybill.Create(
            "WB-0001", Guid.NewGuid(), "KA0000KA", "PL-1",
            new DeliveryCostBreakdown(0m, 0m, 1m),
            new[] { new WaybillItem(Guid.NewGuid(), 5, 100m) });

    // A Draft waybill must move to Cancelled and the change must commit.
    [Fact]
    public async Task Handle_DraftWaybill_CancelsAndSaves()
    {
        var waybill = CreateWaybill();
        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(waybill.Id, It.IsAny<CancellationToken>())).ReturnsAsync(waybill);
        var uow = new Mock<IUnitOfWork>();

        var handler = new CancelWaybillCommandHandler(repo.Object, uow.Object);

        await handler.Handle(new CancelWaybillCommand(waybill.Id), CancellationToken.None);

        Assert.Equal(WaybillStatus.Cancelled, waybill.Status);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // A waybill cannot be cancelled twice.
    [Fact]
    public async Task Handle_AlreadyCancelled_Throws()
    {
        var waybill = CreateWaybill();
        waybill.Cancel();
        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(waybill.Id, It.IsAny<CancellationToken>())).ReturnsAsync(waybill);
        var uow = new Mock<IUnitOfWork>();

        var handler = new CancelWaybillCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new CancelWaybillCommand(waybill.Id), CancellationToken.None));
    }

    // Cancelling a non-existent waybill must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_WaybillNotFound_Throws()
    {
        var repo = new Mock<IWaybillRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Waybill?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new CancelWaybillCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new CancelWaybillCommand(Guid.NewGuid()), CancellationToken.None));
    }
}

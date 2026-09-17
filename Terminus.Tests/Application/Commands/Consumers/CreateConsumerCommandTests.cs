using Moq;
using Terminus.Application.Commands.Consumers;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Tests.Application.Commands.Consumers;

public class CreateConsumerCommandTests
{
    // A valid request must build a Consumer, persist it, and commit through the unit of work.
    [Fact]
    public async Task Handle_ValidRequest_AddsConsumerAndSaves()
    {
        var repo = new Mock<IConsumerRepository>();
        var uow = new Mock<IUnitOfWork>();
        Consumer? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<Consumer>(), It.IsAny<CancellationToken>()))
            .Callback<Consumer, CancellationToken>((c, _) => added = c)
            .Returns(Task.CompletedTask);

        var handler = new CreateConsumerCommandHandler(repo.Object, uow.Object);
        var command = new CreateConsumerCommand("ТОВ Алло", "м. Дніпро, вул. Барикадна, 15", "UA893052990000026001234567006");

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(id, added!.Id);
        Assert.Equal("ТОВ Алло", added.Name);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Domain-level validation (empty name) must reject the command before ever touching the repository.
    [Fact]
    public async Task Handle_InvalidData_ThrowsBeforeTouchingRepository()
    {
        var repo = new Mock<IConsumerRepository>();
        var uow = new Mock<IUnitOfWork>();
        var handler = new CreateConsumerCommandHandler(repo.Object, uow.Object);
        var command = new CreateConsumerCommand("", "address", "bank");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));

        repo.Verify(r => r.AddAsync(It.IsAny<Consumer>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

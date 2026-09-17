using Moq;
using Terminus.Application.Commands.Consumers;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Consumers;

public class ChangeConsumerNameCommandTests
{
    // A valid rename must update the entity and commit.
    [Fact]
    public async Task Handle_ExistingConsumer_ChangesNameAndSaves()
    {
        var consumer = TestFactory.CreateConsumer(name: "Old Name");
        var repo = new Mock<IConsumerRepository>();
        repo.Setup(r => r.GetByIdAsync(consumer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(consumer);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeConsumerNameCommandHandler(repo.Object, uow.Object);

        await handler.Handle(new ChangeConsumerNameCommand(consumer.Id, "New Name"), CancellationToken.None);

        Assert.Equal("New Name", consumer.Name);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Renaming a non-existent consumer must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ConsumerNotFound_Throws()
    {
        var repo = new Mock<IConsumerRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Consumer?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeConsumerNameCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new ChangeConsumerNameCommand(Guid.NewGuid(), "New Name"), CancellationToken.None));
    }

    // The entity's own guard clause must reject a blank new name.
    [Fact]
    public async Task Handle_EmptyName_Throws()
    {
        var consumer = TestFactory.CreateConsumer();
        var repo = new Mock<IConsumerRepository>();
        repo.Setup(r => r.GetByIdAsync(consumer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(consumer);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeConsumerNameCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new ChangeConsumerNameCommand(consumer.Id, ""), CancellationToken.None));
    }
}

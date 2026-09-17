using Moq;
using Terminus.Application.Commands.Consumers;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Consumers;

public class DeleteConsumerCommandTests
{
    // An existing consumer must be deleted and the change committed.
    [Fact]
    public async Task Handle_ExistingConsumer_DeletesAndSaves()
    {
        var consumer = TestFactory.CreateConsumer();
        var repo = new Mock<IConsumerRepository>();
        repo.Setup(r => r.GetByIdAsync(consumer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(consumer);
        var uow = new Mock<IUnitOfWork>();

        var handler = new DeleteConsumerCommandHandler(repo.Object, uow.Object);

        await handler.Handle(new DeleteConsumerCommand(consumer.Id), CancellationToken.None);

        repo.Verify(r => r.DeleteAsync(consumer, It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Deleting a non-existent consumer must fail fast with 404-mapped KeyNotFoundException, not save anything.
    [Fact]
    public async Task Handle_ConsumerNotFound_ThrowsKeyNotFound()
    {
        var repo = new Mock<IConsumerRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Consumer?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new DeleteConsumerCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new DeleteConsumerCommand(Guid.NewGuid()), CancellationToken.None));

        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

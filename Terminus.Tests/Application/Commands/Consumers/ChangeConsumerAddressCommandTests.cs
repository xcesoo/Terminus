using Moq;
using Terminus.Application.Commands.Consumers;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Consumers;

public class ChangeConsumerAddressCommandTests
{
    // A valid address change must update the entity and commit.
    [Fact]
    public async Task Handle_ExistingConsumer_ChangesAddressAndSaves()
    {
        var consumer = TestFactory.CreateConsumer(address: "Old Address");
        var repo = new Mock<IConsumerRepository>();
        repo.Setup(r => r.GetByIdAsync(consumer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(consumer);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeConsumerAddressCommandHandler(repo.Object, uow.Object);

        await handler.Handle(new ChangeConsumerAddressCommand(consumer.Id, "New Address"), CancellationToken.None);

        Assert.Equal("New Address", consumer.Address);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Changing the address of a non-existent consumer must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ConsumerNotFound_Throws()
    {
        var repo = new Mock<IConsumerRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Consumer?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeConsumerAddressCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new ChangeConsumerAddressCommand(Guid.NewGuid(), "New Address"), CancellationToken.None));
    }
}

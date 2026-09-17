using Moq;
using Terminus.Application.Commands.Consumers;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Consumers;

public class ChangeConsumerBankAccountCommandTests
{
    // A valid bank account change must update the entity and commit.
    [Fact]
    public async Task Handle_ExistingConsumer_ChangesBankAccountAndSaves()
    {
        var consumer = TestFactory.CreateConsumer(bankAccount: "OldBank");
        var repo = new Mock<IConsumerRepository>();
        repo.Setup(r => r.GetByIdAsync(consumer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(consumer);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeConsumerBankAccountCommandHandler(repo.Object, uow.Object);

        await handler.Handle(new ChangeConsumerBankAccountCommand(consumer.Id, "NewBank"), CancellationToken.None);

        Assert.Equal("NewBank", consumer.BankAccount);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Changing the bank account of a non-existent consumer must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ConsumerNotFound_Throws()
    {
        var repo = new Mock<IConsumerRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Consumer?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeConsumerBankAccountCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new ChangeConsumerBankAccountCommand(Guid.NewGuid(), "NewBank"), CancellationToken.None));
    }
}

using Moq;
using Terminus.Application.Commands.Contracts;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Contracts;

public class TerminateContractCommandTests
{
    // An existing contract must move to Terminated and the change must commit.
    [Fact]
    public async Task Handle_ExistingContract_TerminatesAndSaves()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 5) });
        var repo = new Mock<IContractRepository>();
        repo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var uow = new Mock<IUnitOfWork>();

        var handler = new TerminateContractCommandHandler(repo.Object, uow.Object);

        await handler.Handle(new TerminateContractCommand(contract.Id), CancellationToken.None);

        Assert.Equal(ContractStatus.Terminated, contract.Status);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // A contract cannot be terminated twice.
    [Fact]
    public async Task Handle_AlreadyTerminated_Throws()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 5) });
        contract.Terminate();
        var repo = new Mock<IContractRepository>();
        repo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var uow = new Mock<IUnitOfWork>();

        var handler = new TerminateContractCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new TerminateContractCommand(contract.Id), CancellationToken.None));
    }

    // Terminating a non-existent contract must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ContractNotFound_Throws()
    {
        var repo = new Mock<IContractRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Contract?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new TerminateContractCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new TerminateContractCommand(Guid.NewGuid()), CancellationToken.None));
    }
}

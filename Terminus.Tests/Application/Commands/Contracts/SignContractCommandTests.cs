using Moq;
using Terminus.Application.Commands.Contracts;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Contracts;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Contracts;

public class SignContractCommandTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 17, 0, 0, 0, TimeSpan.Zero);

    private static IRuleEngine RealRuleEngine() =>
        new RuleEngine(new IBusinessRule[]
        {
            new ContractFutureDateRule(new FixedTimeProvider(Now)),
            new ContractSignStateRule()
        });

    private static Contract CreateDraftContract()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        return TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 5) });
    }

    // A Draft contract signed with a valid (non-future) date must move to Signed and commit.
    [Fact]
    public async Task Handle_DraftContractWithValidDate_SignsAndSaves()
    {
        var contract = CreateDraftContract();
        var repo = new Mock<IContractRepository>();
        repo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var uow = new Mock<IUnitOfWork>();

        var handler = new SignContractCommandHandler(repo.Object, RealRuleEngine(), uow.Object);

        await handler.Handle(new SignContractCommand(contract.Id, Now.UtcDateTime), CancellationToken.None);

        Assert.Equal(ContractStatus.Signed, contract.Status);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // The real ContractFutureDateRule must reject a future conclusion date and leave the contract untouched.
    [Fact]
    public async Task Handle_FutureConclusionDate_ThrowsAndDoesNotSign()
    {
        var contract = CreateDraftContract();
        var repo = new Mock<IContractRepository>();
        repo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var uow = new Mock<IUnitOfWork>();

        var handler = new SignContractCommandHandler(repo.Object, RealRuleEngine(), uow.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new SignContractCommand(contract.Id, Now.UtcDateTime.AddDays(1)), CancellationToken.None));

        Assert.Equal(ContractStatus.Draft, contract.Status);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // The real ContractSignStateRule must reject re-signing an already signed contract.
    [Fact]
    public async Task Handle_AlreadySignedContract_Throws()
    {
        var contract = CreateDraftContract();
        contract.Sign(Now.UtcDateTime);
        var repo = new Mock<IContractRepository>();
        repo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var uow = new Mock<IUnitOfWork>();

        var handler = new SignContractCommandHandler(repo.Object, RealRuleEngine(), uow.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new SignContractCommand(contract.Id, Now.UtcDateTime), CancellationToken.None));
    }

    // Signing a non-existent contract must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ContractNotFound_Throws()
    {
        var repo = new Mock<IContractRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Contract?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new SignContractCommandHandler(repo.Object, RealRuleEngine(), uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new SignContractCommand(Guid.NewGuid(), Now.UtcDateTime), CancellationToken.None));
    }
}

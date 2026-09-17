using Moq;
using Terminus.Application.Commands.Contracts;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Implementations.Contracts;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Contracts;

public class CreateContractCommandTests
{
    private static IRuleEngine RealRuleEngine() =>
        new RuleEngine(new IBusinessRule[] { new ContractMinimumQuantityRule() });

    // A valid request (meeting the minimum quantity rule) must build a Contract, persist it and commit.
    [Fact]
    public async Task Handle_ValidRequest_CreatesContractAndSaves()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var consumerRepo = new Mock<IConsumerRepository>();
        consumerRepo.Setup(r => r.GetByIdAsync(consumer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(consumer);
        var contractRepo = new Mock<IContractRepository>();
        Contract? added = null;
        contractRepo.Setup(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()))
            .Callback<Contract, CancellationToken>((c, _) => added = c)
            .Returns(Task.CompletedTask);
        var uow = new Mock<IUnitOfWork>();
        var numberGenerator = new Mock<IDocumentNumberGenerator>();
        numberGenerator.Setup(g => g.GenerateContractNumber()).Returns("CTR-TEST-0001");

        var handler = new CreateContractCommandHandler(
            consumerRepo.Object, contractRepo.Object, RealRuleEngine(), uow.Object, numberGenerator.Object);

        var command = new CreateContractCommand(consumer.Id, new[] { new ContractItemRequestDto(product.Id, 5) });

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(id, added!.Id);
        Assert.Equal("CTR-TEST-0001", added.ContractNumber);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // The real ContractMinimumQuantityRule must reject a total quantity below 5 and prevent persistence.
    [Fact]
    public async Task Handle_TotalQuantityBelowMinimum_ThrowsAndDoesNotCreate()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var consumerRepo = new Mock<IConsumerRepository>();
        consumerRepo.Setup(r => r.GetByIdAsync(consumer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(consumer);
        var contractRepo = new Mock<IContractRepository>();
        var uow = new Mock<IUnitOfWork>();
        var numberGenerator = new Mock<IDocumentNumberGenerator>();

        var handler = new CreateContractCommandHandler(
            consumerRepo.Object, contractRepo.Object, RealRuleEngine(), uow.Object, numberGenerator.Object);

        var command = new CreateContractCommand(consumer.Id, new[] { new ContractItemRequestDto(product.Id, 4) });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("5 одиниць", ex.Message);
        contractRepo.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Creating a contract for a non-existent consumer must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ConsumerNotFound_Throws()
    {
        var consumerRepo = new Mock<IConsumerRepository>();
        consumerRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Consumer?)null);
        var contractRepo = new Mock<IContractRepository>();
        var uow = new Mock<IUnitOfWork>();
        var numberGenerator = new Mock<IDocumentNumberGenerator>();

        var handler = new CreateContractCommandHandler(
            consumerRepo.Object, contractRepo.Object, RealRuleEngine(), uow.Object, numberGenerator.Object);

        var command = new CreateContractCommand(Guid.NewGuid(), new[] { new ContractItemRequestDto(Guid.NewGuid(), 10) });

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}

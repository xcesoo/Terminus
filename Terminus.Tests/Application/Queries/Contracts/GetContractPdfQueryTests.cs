using Moq;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Application.Queries.Contracts;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Queries.Contracts;

public class GetContractPdfQueryTests
{
    // A signed contract must produce whatever bytes the injected generator returns, under a name derived from its number.
    [Fact]
    public async Task Handle_SignedContract_ReturnsGeneratedPdf()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });
        contract.Sign(DateTime.UtcNow);

        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var consumerRepo = new Mock<IConsumerRepository>();
        consumerRepo.Setup(r => r.GetByIdAsync(contract.ConsumerId, It.IsAny<CancellationToken>())).ReturnsAsync(consumer);
        var generator = new Mock<IContractPdfGenerator>();
        generator.Setup(g => g.Generate(It.IsAny<ContractDto>(), It.IsAny<ConsumerDto>())).Returns([9, 9, 9]);

        var handler = new GetContractPdfQueryHandler(contractRepo.Object, consumerRepo.Object, generator.Object);

        var (content, fileName) = await handler.Handle(new GetContractPdfQuery(contract.Id), CancellationToken.None);

        Assert.Equal(new byte[] { 9, 9, 9 }, content);
        Assert.Equal($"Contract_{contract.ContractNumber}.pdf", fileName);
    }

    // The printed contract form must only be available once the contract has actually been signed.
    [Fact]
    public async Task Handle_DraftContract_ThrowsAndDoesNotGeneratePdf()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });

        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var consumerRepo = new Mock<IConsumerRepository>();
        var generator = new Mock<IContractPdfGenerator>();

        var handler = new GetContractPdfQueryHandler(contractRepo.Object, consumerRepo.Object, generator.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new GetContractPdfQuery(contract.Id), CancellationToken.None));

        Assert.Contains("не підписано", ex.Message);
        generator.Verify(g => g.Generate(It.IsAny<ContractDto>(), It.IsAny<ConsumerDto>()), Times.Never);
    }

    // A terminated contract must not be printable either, even though it was signed at some point.
    [Fact]
    public async Task Handle_TerminatedContract_Throws()
    {
        var consumer = TestFactory.CreateConsumer();
        var product = TestFactory.CreateProduct();
        var contract = TestFactory.CreateContract(consumer, new[] { TestFactory.CreateContractItem(product, 10) });
        contract.Sign(DateTime.UtcNow);
        contract.Terminate();

        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contract);
        var consumerRepo = new Mock<IConsumerRepository>();
        var generator = new Mock<IContractPdfGenerator>();

        var handler = new GetContractPdfQueryHandler(contractRepo.Object, consumerRepo.Object, generator.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new GetContractPdfQuery(contract.Id), CancellationToken.None));
    }

    // Requesting a PDF for a non-existent contract must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ContractNotFound_Throws()
    {
        var contractRepo = new Mock<IContractRepository>();
        contractRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Contract?)null);
        var consumerRepo = new Mock<IConsumerRepository>();
        var generator = new Mock<IContractPdfGenerator>();

        var handler = new GetContractPdfQueryHandler(contractRepo.Object, consumerRepo.Object, generator.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetContractPdfQuery(Guid.NewGuid()), CancellationToken.None));
    }
}

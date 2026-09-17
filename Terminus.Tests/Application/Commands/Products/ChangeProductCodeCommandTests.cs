using Moq;
using Terminus.Application.Commands.Products;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Products;

public class ChangeProductCodeCommandTests
{
    // Changing to a code nobody else uses must succeed and commit.
    [Fact]
    public async Task Handle_NewCodeIsFree_ChangesCodeAndSaves()
    {
        var product = TestFactory.CreateProduct(code: "OLD");
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        repo.Setup(r => r.GetByCodeAsync("NEW", It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeProductCodeCommandHandler(repo.Object, uow.Object);

        await handler.Handle(new ChangeProductCodeCommand(product.Id, "NEW"), CancellationToken.None);

        Assert.Equal("NEW", product.Code);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Changing to a code already used by another product must be rejected and leave the original code untouched.
    [Fact]
    public async Task Handle_NewCodeAlreadyTaken_Throws()
    {
        var product = TestFactory.CreateProduct(code: "OLD");
        var otherProduct = TestFactory.CreateProduct(code: "TAKEN");
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        repo.Setup(r => r.GetByCodeAsync("TAKEN", It.IsAny<CancellationToken>())).ReturnsAsync(otherProduct);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeProductCodeCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new ChangeProductCodeCommand(product.Id, "TAKEN"), CancellationToken.None));

        Assert.Equal("OLD", product.Code);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Changing the code of a non-existent product must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ProductNotFound_Throws()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeProductCodeCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new ChangeProductCodeCommand(Guid.NewGuid(), "NEW"), CancellationToken.None));
    }
}

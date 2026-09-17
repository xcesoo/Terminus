using Moq;
using Terminus.Application.Commands.Products;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Products;

public class ChangeProductNameCommandTests
{
    // A valid rename must update the entity and commit.
    [Fact]
    public async Task Handle_ExistingProduct_ChangesNameAndSaves()
    {
        var product = TestFactory.CreateProduct(name: "Old");
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeProductNameCommandHandler(repo.Object, uow.Object);

        await handler.Handle(new ChangeProductNameCommand(product.Id, "New"), CancellationToken.None);

        Assert.Equal("New", product.Name);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Renaming a non-existent product must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ProductNotFound_Throws()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new ChangeProductNameCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new ChangeProductNameCommand(Guid.NewGuid(), "New"), CancellationToken.None));
    }
}

using Moq;
using Terminus.Application.Commands.Products;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Products;

public class DeleteProductCommandTests
{
    // An existing product must be deleted and the change committed.
    [Fact]
    public async Task Handle_ExistingProduct_DeletesAndSaves()
    {
        var product = TestFactory.CreateProduct();
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var uow = new Mock<IUnitOfWork>();

        var handler = new DeleteProductCommandHandler(repo.Object, uow.Object);

        await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        repo.Verify(r => r.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Deleting a non-existent product must fail with KeyNotFoundException.
    [Fact]
    public async Task Handle_ProductNotFound_Throws()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new DeleteProductCommandHandler(repo.Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None));
    }
}

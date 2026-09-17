using Moq;
using Terminus.Application.Commands.Products;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Products;

public class UpdateProductPriceCommandTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 17, 0, 0, 0, TimeSpan.Zero);

    // Price and price-list number (region + effective date) get updated using the injected TimeProvider's current time.
    [Fact]
    public async Task Handle_ExistingProduct_UpdatesPriceUsingCurrentTime()
    {
        var product = TestFactory.CreateProduct(price: 100m);
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var uow = new Mock<IUnitOfWork>();

        var handler = new UpdateProductPriceCommandHandler(repo.Object, uow.Object, new FixedTimeProvider(Now));

        await handler.Handle(new UpdateProductPriceCommand(product.Id, 150m, "ua"), CancellationToken.None);

        Assert.Equal(150m, product.Price);
        Assert.Equal("UA-2026-09-17", product.PriceListNumber);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // A missing product must produce the same KeyNotFoundException (404) as every other "not found" case in the app.
    [Fact]
    public async Task Handle_ProductNotFound_ThrowsKeyNotFound()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new UpdateProductPriceCommandHandler(repo.Object, uow.Object, new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new UpdateProductPriceCommand(Guid.NewGuid(), 150m), CancellationToken.None));
    }

    // Non-positive prices are rejected by the domain entity's own guard clause.
    [Fact]
    public async Task Handle_NonPositivePrice_Throws()
    {
        var product = TestFactory.CreateProduct();
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var uow = new Mock<IUnitOfWork>();

        var handler = new UpdateProductPriceCommandHandler(repo.Object, uow.Object, new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new UpdateProductPriceCommand(product.Id, 0m), CancellationToken.None));
    }
}

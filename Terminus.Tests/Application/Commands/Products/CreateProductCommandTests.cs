using Moq;
using Terminus.Application.Commands.Products;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Tests.TestHelpers;

namespace Terminus.Tests.Application.Commands.Products;

public class CreateProductCommandTests
{
    // Regression test for the inverted-logic bug: GetByCodeAsync returning null (code is free) must let creation succeed.
    [Fact]
    public async Task Handle_CodeDoesNotExist_CreatesProduct()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByCodeAsync("NEW-CODE", It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var uow = new Mock<IUnitOfWork>();
        Product? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => added = p)
            .Returns(Task.CompletedTask);

        var handler = new CreateProductCommandHandler(repo.Object, uow.Object);
        var command = new CreateProductCommand("NEW-CODE", "Ноутбук", 78000m, "UA-2026-09-17");

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(id, added!.Id);
        Assert.Equal("NEW-CODE", added.Code);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Regression test for the inverted-logic bug: GetByCodeAsync returning an existing product must reject creation.
    [Fact]
    public async Task Handle_CodeAlreadyExists_ThrowsAndDoesNotCreate()
    {
        var existing = TestFactory.CreateProduct(code: "DUP-CODE");
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByCodeAsync("DUP-CODE", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var uow = new Mock<IUnitOfWork>();

        var handler = new CreateProductCommandHandler(repo.Object, uow.Object);
        var command = new CreateProductCommand("DUP-CODE", "Ноутбук", 78000m, "UA-2026-09-17");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("вже існує", ex.Message);
        repo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

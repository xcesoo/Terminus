using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Products;

public readonly record struct DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Виріб не знайдено.");

        await productRepository.DeleteAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
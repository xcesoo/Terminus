using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Products;

public readonly record struct ChangeProductNameCommand(Guid Id, string Name) : IRequest;

public class ChangeProductNameCommandHandler(
    IProductRepository productRepository, 
    IUnitOfWork unitOfWork) 
    : IRequestHandler<ChangeProductNameCommand>
{
    public async Task Handle(ChangeProductNameCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Виріб не знайдено.");

        product.ChangeName(request.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
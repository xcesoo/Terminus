using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Products;

public readonly record struct ChangeProductCodeCommand(Guid ProductId, string NewCode) : IRequest;

public class ChangeProductCodeCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeProductCodeCommand>
{
    public async Task Handle(ChangeProductCodeCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Виріб не знайдено.");
        var codeExist = await productRepository.GetByCodeAsync(request.NewCode, cancellationToken);
        if (codeExist is not null)
            throw new InvalidOperationException("Код виробу вже існує.");

        product.ChangeCode(request.NewCode);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
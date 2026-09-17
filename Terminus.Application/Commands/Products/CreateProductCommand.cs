using MediatR;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Products;

public readonly record struct CreateProductCommand(
    string Code, 
    string Name, 
    decimal Price, 
    string PriceListNumber) : IRequest<Guid>;

public class CreateProductCommandHandler(
    IProductRepository productRepository, 
    IUnitOfWork unitOfWork) 
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var exist = await productRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (exist is not null)
            throw new InvalidOperationException("Виріб з таким кодом вже існує.");

        var product = Product.Create(request.Code, request.Name, request.Price, request.PriceListNumber);
        
        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return product.Id;
    }
}
using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Products;

public readonly record struct UpdateProductPriceCommand(
    Guid Id, 
    decimal NewPrice, 
    string RegionCode = "UA") : IRequest;

public class UpdateProductPriceCommandHandler(
    IProductRepository productRepository, 
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateProductPriceCommand>
{
    public async Task Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null) throw new ArgumentException("Виріб не знайдено.");

        var effectiveDate = timeProvider.GetUtcNow().UtcDateTime;

        product.UpdatePrice(request.NewPrice, effectiveDate, request.RegionCode);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Contracts;

public readonly record struct GetContractByIdQuery(Guid Id) : IRequest<ContractDto>;

public class GetContractByIdQueryHandler(IContractRepository contractRepository) 
    : IRequestHandler<GetContractByIdQuery, ContractDto>
{
    public async Task<ContractDto> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
    {
        var contract = await contractRepository.GetByIdAsync(request.Id, cancellationToken)
                       ?? throw new KeyNotFoundException("Договір не знайдено.");

        return contract.MapToDto();
    }
}
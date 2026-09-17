using MediatR;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.Extensions;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Contracts;

public readonly record struct GetContractPdfQuery(Guid ContractId) : IRequest<(byte[] Content, string FileName)>;

public class GetContractPdfQueryHandler(
    IContractRepository contractRepository,
    IConsumerRepository consumerRepository,
    IContractPdfGenerator pdfGenerator)
    : IRequestHandler<GetContractPdfQuery, (byte[] Content, string FileName)>
{
    public async Task<(byte[] Content, string FileName)> Handle(GetContractPdfQuery request, CancellationToken cancellationToken)
    {
        var contract = await contractRepository.GetByIdAsync(request.ContractId, cancellationToken)
                       ?? throw new KeyNotFoundException("Договір не знайдено.");

        if (contract.Status != ContractStatus.Signed)
            throw new InvalidOperationException("Неможливо сформувати друковану форму: договір ще не підписано.");

        var consumer = await consumerRepository.GetByIdAsync(contract.ConsumerId, cancellationToken)
                       ?? throw new KeyNotFoundException("Споживача не знайдено.");

        var pdfBytes = pdfGenerator.Generate(contract.MapToDto(), consumer.MapToDto());
        var fileName = $"Contract_{contract.ContractNumber}.pdf";

        return (pdfBytes, fileName);
    }
}

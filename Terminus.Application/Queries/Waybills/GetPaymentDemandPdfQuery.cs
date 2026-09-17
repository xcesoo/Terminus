using MediatR;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.Extensions;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Waybills;

public readonly record struct GetPaymentDemandPdfQuery(Guid WaybillId) : IRequest<(byte[] Content, string FileName)>;

public class GetPaymentDemandPdfQueryHandler(
    IWaybillRepository waybillRepository,
    IPaymentDemandPdfGenerator pdfGenerator)
    : IRequestHandler<GetPaymentDemandPdfQuery, (byte[] Content, string FileName)>
{
    public async Task<(byte[] Content, string FileName)> Handle(GetPaymentDemandPdfQuery request, CancellationToken cancellationToken)
    {
        var waybill = await waybillRepository.GetByIdAsync(request.WaybillId, cancellationToken)
                      ?? throw new KeyNotFoundException("ТТН не знайдено.");

        if (waybill.Status != WaybillStatus.Dispatched)
            throw new InvalidOperationException(
                "Неможливо сформувати платіжну вимогу: ТТН ще не відвантажено (відсутній підпис/дата відвантаження).");

        var waybillDto = waybill.MapToDto();
        var pdfBytes = pdfGenerator.Generate(waybillDto);
        var fileName = $"PaymentDemand_{waybill.WaybillNumber}.pdf";

        return (pdfBytes, fileName);
    }
}
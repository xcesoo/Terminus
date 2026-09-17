using MediatR;
using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Queries.Waybills;

public readonly record struct GetMonthlyShipmentStatementPdfQuery(int Year, int Month) : IRequest<(byte[] Content, string FileName)>;

public class GetMonthlyShipmentStatementPdfQueryHandler(
    ISender sender,
    IShipmentStatementPdfGenerator pdfGenerator)
    : IRequestHandler<GetMonthlyShipmentStatementPdfQuery, (byte[] Content, string FileName)>
{
    public async Task<(byte[] Content, string FileName)> Handle(GetMonthlyShipmentStatementPdfQuery request, CancellationToken cancellationToken)
    {
        var waybills = await sender.Send(new GetMonthlyWaybillsQuery(request.Year, request.Month), cancellationToken);

        var periodTitle = $"Відомість відвантаження виробів за {request.Month:00}.{request.Year}";
        var pdfBytes = pdfGenerator.Generate(waybills, periodTitle);
        var fileName = $"ShipmentStatement_{request.Year}-{request.Month:00}.pdf";

        return (pdfBytes, fileName);
    }
}

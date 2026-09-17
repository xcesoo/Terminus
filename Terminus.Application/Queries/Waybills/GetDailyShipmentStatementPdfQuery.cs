using MediatR;
using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application.Queries.Waybills;

public readonly record struct GetDailyShipmentStatementPdfQuery(DateTime Date) : IRequest<(byte[] Content, string FileName)>;

public class GetDailyShipmentStatementPdfQueryHandler(
    ISender sender,
    IShipmentStatementPdfGenerator pdfGenerator)
    : IRequestHandler<GetDailyShipmentStatementPdfQuery, (byte[] Content, string FileName)>
{
    public async Task<(byte[] Content, string FileName)> Handle(GetDailyShipmentStatementPdfQuery request, CancellationToken cancellationToken)
    {
        var waybills = await sender.Send(new GetDailyWaybillsQuery(request.Date), cancellationToken);

        var periodTitle = $"Відомість відвантаження виробів за {request.Date:dd.MM.yyyy}";
        var pdfBytes = pdfGenerator.Generate(waybills, periodTitle);
        var fileName = $"ShipmentStatement_{request.Date:yyyy-MM-dd}.pdf";

        return (pdfBytes, fileName);
    }
}

using MediatR;
using Moq;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Application.Queries.Waybills;
using Terminus.Domain.Enums;

namespace Terminus.Tests.Application.Queries.Waybills;

public class GetDailyShipmentStatementPdfQueryTests
{
    // The handler must fetch the day's waybills via the existing GetDailyWaybillsQuery (reusing the JSON endpoint's
    // data source) and hand them straight to the PDF generator, with a title and filename derived from the date.
    [Fact]
    public async Task Handle_DelegatesToDailyWaybillsQueryAndGeneratesPdf()
    {
        var date = new DateTime(2026, 9, 17, 0, 0, 0, DateTimeKind.Utc);
        var waybills = new List<WaybillDto>
        {
            new(Guid.NewGuid(), "WB-0001", "CTR-0001", date, WaybillStatus.Dispatched,
                "Consumer", "Address", "Bank", TransportType.Auto, "Автомобіль", "details",
                [], 100m, 10m, 110m, 0m, 0m, 1m)
        };

        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<GetDailyWaybillsQuery>(q => q.Date == date), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<WaybillDto>)waybills);

        IReadOnlyCollection<WaybillDto>? passedToGenerator = null;
        string? passedTitle = null;
        var generator = new Mock<IShipmentStatementPdfGenerator>();
        generator.Setup(g => g.Generate(It.IsAny<IReadOnlyCollection<WaybillDto>>(), It.IsAny<string>()))
            .Callback<IReadOnlyCollection<WaybillDto>, string>((w, t) =>
            {
                passedToGenerator = w;
                passedTitle = t;
            })
            .Returns([7, 7, 7]);

        var handler = new GetDailyShipmentStatementPdfQueryHandler(sender.Object, generator.Object);

        var (content, fileName) = await handler.Handle(new GetDailyShipmentStatementPdfQuery(date), CancellationToken.None);

        Assert.Equal(new byte[] { 7, 7, 7 }, content);
        Assert.Equal("ShipmentStatement_2026-09-17.pdf", fileName);
        Assert.Same(waybills, passedToGenerator);
        Assert.Contains("17.09.2026", passedTitle);
    }
}

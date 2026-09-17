using MediatR;
using Moq;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Application.Queries.Waybills;

namespace Terminus.Tests.Application.Queries.Waybills;

public class GetMonthlyShipmentStatementPdfQueryTests
{
    // The handler must fetch the month's waybills via the existing GetMonthlyWaybillsQuery (reusing the JSON
    // endpoint's data source) and hand them to the PDF generator, with a title and filename derived from year/month.
    [Fact]
    public async Task Handle_DelegatesToMonthlyWaybillsQueryAndGeneratesPdf()
    {
        var emptyWaybills = (IReadOnlyCollection<WaybillDto>)new List<WaybillDto>();

        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<GetMonthlyWaybillsQuery>(q => q.Year == 2026 && q.Month == 9), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyWaybills);

        string? passedTitle = null;
        var generator = new Mock<IShipmentStatementPdfGenerator>();
        generator.Setup(g => g.Generate(It.IsAny<IReadOnlyCollection<WaybillDto>>(), It.IsAny<string>()))
            .Callback<IReadOnlyCollection<WaybillDto>, string>((_, t) => passedTitle = t)
            .Returns([1]);

        var handler = new GetMonthlyShipmentStatementPdfQueryHandler(sender.Object, generator.Object);

        var (content, fileName) = await handler.Handle(new GetMonthlyShipmentStatementPdfQuery(2026, 9), CancellationToken.None);

        Assert.Equal(new byte[] { 1 }, content);
        Assert.Equal("ShipmentStatement_2026-09.pdf", fileName);
        Assert.Contains("09.2026", passedTitle);
    }
}

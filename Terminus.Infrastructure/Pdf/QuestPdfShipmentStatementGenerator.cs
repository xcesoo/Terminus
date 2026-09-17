using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;

namespace Terminus.Infrastructure.Pdf;

public class QuestPdfShipmentStatementGenerator : IShipmentStatementPdfGenerator
{
    public byte[] Generate(IReadOnlyCollection<WaybillDto> waybills, string periodTitle)
    {
        var document = new ShipmentStatementDocument(waybills, periodTitle);
        return document.GeneratePdf();
    }

    private static string GetResourceFromManifest(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Terminus.Infrastructure.Pdf.Assets.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return string.Empty;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private class ShipmentStatementDocument(IReadOnlyCollection<WaybillDto> waybills, string periodTitle) : IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Lato));
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("ТОВ 'Термінус Логістикс'").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Київ, вул. Логістична, 1").FontSize(9);
                    column.Item().PaddingTop(6).Text(periodTitle).FontSize(16).SemiBold();
                });

                var logoSvg = GetResourceFromManifest("terminus-logo.svg");
                if (!string.IsNullOrEmpty(logoSvg))
                {
                    row.ConstantItem(64).Height(64).AlignRight().Svg(logoSvg).FitArea();
                }
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(10);

                if (waybills.Count == 0)
                {
                    column.Item().Text("Немає відвантажень за цей період.").FontSize(11).FontColor(Colors.Grey.Darken1);
                    return;
                }

                column.Item().Element(ComposeTable);

                var totalQuantity = waybills.Sum(w => w.Items.Sum(i => i.ShippedQuantity));
                var totalAmount = waybills.Sum(w => w.TotalAmount);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Всього ТТН: {waybills.Count}, одиниць товару: {totalQuantity}");
                    row.ConstantItem(150).AlignRight().Text($"Разом: {totalAmount:C2}").SemiBold();
                });
            });
        }

        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(1.3f);
                    columns.ConstantColumn(60);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.4f);
                    columns.RelativeColumn(0.9f);
                    columns.RelativeColumn(2.6f);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(85);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("#");
                    header.Cell().Element(HeaderCellStyle).Text("№ ТТН");
                    header.Cell().Element(HeaderCellStyle).Text("Дата");
                    header.Cell().Element(HeaderCellStyle).Text("№ договору");
                    header.Cell().Element(HeaderCellStyle).Text("Споживач");
                    header.Cell().Element(HeaderCellStyle).Text("Транспорт");
                    header.Cell().Element(HeaderCellStyle).Text("Вироби (найменування × кіл-сть)");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Кіл-сть");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Сума");

                    static IContainer HeaderCellStyle(IContainer container) =>
                        container.DefaultTextStyle(x => x.SemiBold())
                            .Background(Colors.Grey.Lighten3)
                            .Border(1).BorderColor(Colors.Black)
                            .Padding(4);
                });

                var i = 1;
                foreach (var waybill in waybills)
                {
                    var productsSummary = string.Join("; ", waybill.Items.Select(item => $"{item.ProductName} × {item.ShippedQuantity}"));
                    var totalQuantity = waybill.Items.Sum(item => item.ShippedQuantity);

                    table.Cell().Element(CellStyle).Text((i++).ToString());
                    table.Cell().Element(CellStyle).Text(waybill.WaybillNumber);
                    table.Cell().Element(CellStyle).Text($"{waybill.DispatchDate:dd.MM.yyyy}");
                    table.Cell().Element(CellStyle).Text(waybill.ContractNumber);
                    table.Cell().Element(CellStyle).Text(waybill.ConsumerName);
                    table.Cell().Element(CellStyle).Text(waybill.TransportType);
                    table.Cell().Element(CellStyle).Text(productsSummary);
                    table.Cell().Element(CellStyle).AlignRight().Text(totalQuantity.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text($"{waybill.TotalAmount:C2}");

                    static IContainer CellStyle(IContainer container) =>
                        container.Border(1).BorderColor(Colors.Grey.Medium).Padding(4);
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Сторінка ");
                x.CurrentPageNumber();
                x.Span(" з ");
                x.TotalPages();
            });
        }
    }
}

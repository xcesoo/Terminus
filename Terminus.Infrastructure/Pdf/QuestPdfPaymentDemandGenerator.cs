using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;

namespace Terminus.Infrastructure.Pdf;

public class QuestPdfPaymentDemandGenerator : IPaymentDemandPdfGenerator
{
    public byte[] Generate(WaybillDto waybill)
    {
        var document = new PaymentDemandDocument(waybill);
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

    private class PaymentDemandDocument(WaybillDto waybill) : IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Lato));
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
                    column.Item().Text("ПЛАТІЖНА ВИМОГА").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"До ТТН №: {waybill.WaybillNumber}").FontSize(14);
                    column.Item().Text($"Дата відвантаження: {waybill.DispatchDate?.ToLocalTime():dd.MM.yyyy HH:mm}")
                        .FontSize(10);
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
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(14);

            column.Item().Row(row =>
            {
                row.RelativeItem().Component(new AddressComponent("Платник:", waybill.ConsumerName, waybill.ConsumerAddress, waybill.ConsumerBankAccount));
                row.RelativeItem().Component(new AddressComponent("Одержувач платежу:", "ТОВ 'Термінус Логістикс'", "Київ, вул. Логістична, 1", "UA123456789012345678901234567"));
            });

            column.Item().Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Text(text =>
            {
                text.Span("Транспорт: ").SemiBold();
                text.Span($"{waybill.TransportType} | {waybill.TransportDetails}");
            });

            column.Item().Element(ComposeTable);

            column.Item().Element(ComposeDeliveryBreakdown);

            column.Item().AlignRight().Text($"Сума за товари: {waybill.ProductsTotalSum:C2}").FontSize(12);
            column.Item().AlignRight().Text($"Транспортні послуги: {waybill.TransportServiceSum:C2}").FontSize(12);
            column.Item().AlignRight().Text($"Всього до сплати: {waybill.TotalAmount:C2}").FontSize(14).SemiBold().FontColor(Colors.Red.Medium);
            
            column.Item().PaddingTop(10).Height(120).Row(row =>
            {
                row.RelativeItem().Layers(layers =>
                {
                    layers.PrimaryLayer().Column(col =>
                    {
                        col.Item().Text("Відпустив (Представник постачальника):").SemiBold();
                        col.Item().PaddingTop(20).BorderBottom(1).BorderColor(Colors.Black).Width(220).Height(1);
                        col.Item().PaddingTop(2).Text("(підпис, П.І.Б., М.П.)").FontSize(9).FontColor(Colors.Grey.Medium);
                    });

                    var sealSvg = GetResourceFromManifest("company-seal.svg");
                    if (!string.IsNullOrEmpty(sealSvg))
                    {
                        layers.Layer()
                            .AlignCenter().AlignMiddle()
                            .Width(110).Height(110)
                            .Rotate(45)
                            .Svg(sealSvg).FitArea();
                    }
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Отримав (Представник покупця):").SemiBold();
                    col.Item().PaddingTop(20).BorderBottom(1).BorderColor(Colors.Black).Width(220).Height(1);
                    col.Item().PaddingTop(2).Text("(підпис, П.І.Б., М.П.)").FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }

    private void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);
                columns.RelativeColumn();
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
                columns.ConstantColumn(100);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("#");
                header.Cell().Element(HeaderCellStyle).Text("Назва виробу");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Кіл-сть");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Ціна");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("Сума");

                static IContainer HeaderCellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold())
                        .Background(Colors.Grey.Lighten3) 
                        .Border(1).BorderColor(Colors.Black)
                        .Padding(5);
                }
            });

            var i = 1;
            foreach (var item in waybill.Items)
            {
                table.Cell().Element(CellStyle).Text(i++.ToString());
                table.Cell().Element(CellStyle).Text(item.ProductName);
                table.Cell().Element(CellStyle).AlignRight().Text(item.ShippedQuantity.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price:C2}");
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalPrice:C2}");

                static IContainer CellStyle(IContainer container)
                {
                    return container
                        .Border(1).BorderColor(Colors.Grey.Medium) 
                        .Padding(5);
                }
            }
        });
    }

    private void ComposeDeliveryBreakdown(IContainer container)
    {
        var totalQuantity = waybill.Items.Sum(i => i.ShippedQuantity);
        var basePricePerItem = totalQuantity > 0 ? waybill.DeliveryBaseCost / totalQuantity : 0m;
        var commissionRate = waybill.ProductsTotalSum > 0 ? waybill.DeliveryCommissionCost / waybill.ProductsTotalSum : 0m;

        container.Column(column =>
        {
            column.Spacing(2);
            column.Item().Text("Розрахунок вартості доставки:").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(110);
                });

                table.Cell().Element(RowLabelStyle).Text($"Базовий тариф: {totalQuantity} шт. × {basePricePerItem:C2}");
                table.Cell().Element(RowValueStyle).Text($"{waybill.DeliveryBaseCost:C2}");

                table.Cell().Element(RowLabelStyle).Text($"Комісія {commissionRate:P2} від оголошеної вартості {waybill.ProductsTotalSum:C2}");
                table.Cell().Element(RowValueStyle).Text($"{waybill.DeliveryCommissionCost:C2}");

                table.Cell().Element(TotalLabelStyle).Text($"Коефіцієнт транспорту «{waybill.TransportType}» (× {waybill.DeliveryTransportMultiplier:0.##})");
                table.Cell().Element(TotalValueStyle).Text($"{waybill.TransportServiceSum:C2}");

                static IContainer RowLabelStyle(IContainer c) => c.PaddingVertical(2).DefaultTextStyle(x => x.FontSize(9));
                static IContainer RowValueStyle(IContainer c) => c.PaddingVertical(2).AlignRight().DefaultTextStyle(x => x.FontSize(9));
                static IContainer TotalLabelStyle(IContainer c) => c.PaddingVertical(2).BorderTop(1).BorderColor(Colors.Grey.Lighten2).DefaultTextStyle(x => x.FontSize(9).SemiBold());
                static IContainer TotalValueStyle(IContainer c) => c.PaddingVertical(2).BorderTop(1).BorderColor(Colors.Grey.Lighten2).AlignRight().DefaultTextStyle(x => x.FontSize(9).SemiBold());
            });
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
    public class AddressComponent(string title, string name, string address, string bankAccount) : IComponent
    {
        public void Compose(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(2);
                column.Item().BorderBottom(1).PaddingBottom(5).Text(title).SemiBold();
                column.Item().Text(name);
                column.Item().Text(address);
                column.Item().Text($"МФО/IBAN: {bankAccount}");
            });
        }
    }
}
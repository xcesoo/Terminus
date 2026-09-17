using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;

namespace Terminus.Infrastructure.Pdf;

public class QuestPdfContractGenerator : IContractPdfGenerator
{
    public byte[] Generate(ContractDto contract, ConsumerDto consumer)
    {
        var document = new ContractDocument(contract, consumer);
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

    private class ContractDocument(ContractDto contract, ConsumerDto consumer) : IDocument
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
                    column.Item().Text("ДОГОВІР").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"№ {contract.ContractNumber}").FontSize(14);
                    column.Item().Text($"Дата укладання: {contract.ConclusionDate:dd.MM.yyyy}").FontSize(10);
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
                    row.RelativeItem().Component(new QuestPdfPaymentDemandGenerator.AddressComponent(
                        "Постачальник:", "ТОВ 'Термінус Логістикс'", "Київ, вул. Логістична, 1", "UA123456789012345678901234567"));
                    row.RelativeItem().Component(new QuestPdfPaymentDemandGenerator.AddressComponent(
                        "Споживач:", consumer.Name, consumer.Address, consumer.BankAccount));
                });

                column.Item().Element(ComposeTable);

                column.Item().AlignRight().Text($"Загальна сума договору: {contract.TotalAmount:C2}").FontSize(14).SemiBold().FontColor(Colors.Red.Medium);
                column.Item().AlignRight().Text("* Сума не включає вартість транспортних послуг — вона розраховується окремо для кожної ТТН при відвантаженні.").FontSize(8).Italic().FontColor(Colors.Grey.Darken1);

                column.Item().PaddingTop(10).Height(120).Row(row =>
                {
                    row.RelativeItem().Layers(layers =>
                    {
                        layers.PrimaryLayer().Column(col =>
                        {
                            col.Item().Text("Постачальник:").SemiBold();
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
                        col.Item().Text("Споживач:").SemiBold();
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
                foreach (var item in contract.Items)
                {
                    table.Cell().Element(CellStyle).Text(i++.ToString());
                    table.Cell().Element(CellStyle).Text(item.ProductName);
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
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

using QuestCraft.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuestCraft.Infrastructure.Pdf;

public class CertificatePdfGenerator : ICertificatePdfGenerator
{
    public byte[] Generate(CertificateData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily("Arial"));

                page.Content().Border(2).BorderColor(Colors.Blue.Darken2).Padding(40).Column(column =>
                {
                    column.Spacing(16);

                    column.Item().AlignCenter().Text("QuestCraft").FontSize(28).Bold().FontColor(Colors.Blue.Darken3);
                    column.Item().AlignCenter().Text("SERTİFİKAT").FontSize(20).FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingTop(20).AlignCenter().Text(data.FullName).FontSize(32).Bold();

                    column.Item().AlignCenter().Text(
                        $"QuestCraft platformasında {data.Level}-ci səviyyəyə çatdığını, " +
                        $"cəmi {data.TotalXp} XP topladığını və {data.TotalChallengesSolved} challenge həll etdiyini təsdiq edir.")
                        .FontSize(14).FontColor(Colors.Grey.Darken2);

                    column.Item().PaddingTop(30).AlignCenter().Text($"Verilmə tarixi: {data.IssuedAt:dd.MM.yyyy}").FontSize(12);
                });
            });
        });

        return document.GeneratePdf();
    }
}

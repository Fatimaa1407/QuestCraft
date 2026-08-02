using System.Text;
using QuestCraft.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuestCraft.Infrastructure.Pdf;

public class CertificatePdfGenerator : ICertificatePdfGenerator
{
    private const string PurpleHex = "#8B5CF6";
    private const string BlueHex = "#3B82F6";
    private const string GoldHex = "#FACC15";
    private const string TextPrimaryHex = "#F3F4F6";
    private const string TextSecondaryHex = "#9CA3AF";
    private const string CardBgHex = "#141A30";
    private const string CardBorderHex = "#2A3352";

    public byte[] Generate(CertificateData data)
    {
        var completionPercent = data.MaxLevel > 0 ? (int)Math.Round(data.Level * 100.0 / data.MaxLevel) : 100;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontColor(TextPrimaryHex));

                // Dark QuestCraft-style backdrop (gradient wash, faint grid, title glow, watermark
                // wordmark, glowing rounded frame) — all hand-built SVG, since QuestPDF's raw Canvas
                // API is deprecated (runtime NotImplementedException) in favor of feeding it SVG.
                page.Background().Svg(BuildBackgroundSvg());

                page.Content().Padding(28).AlignMiddle().Column(column =>
                {
                    column.Item().AlignCenter().Text("QUESTCRAFT").FontSize(12).Bold().LetterSpacing(0.22f).FontColor(PurpleHex);
                    column.Item().PaddingTop(3).AlignCenter().Text("Certificate of Completion")
                        .FontSize(21).FontColor(TextSecondaryHex).Italic();

                    column.Item().PaddingTop(12).AlignCenter().Text(data.FullName)
                        .FontSize(37).Bold().FontColor(TextPrimaryHex);

                    column.Item().PaddingTop(5).AlignCenter().Text("Successfully completed the entire QuestCraft learning journey.")
                        .FontSize(10).FontColor(TextSecondaryHex);

                    column.Item().PaddingTop(13).Row(row =>
                    {
                        StatCard(row.RelativeItem(), "▲", BlueHex, "LEVEL REACHED", $"{data.Level} / {data.MaxLevel}");
                        StatCard(row.RelativeItem(), "✦", PurpleHex, "XP EARNED", $"{data.TotalXp:N0} XP");
                        StatCard(row.RelativeItem(), "✓", BlueHex, "CHALLENGES SOLVED", $"{data.TotalChallengesSolved}");
                        StatCard(row.RelativeItem(), "◆", GoldHex, "COMPLETION", $"{completionPercent}%");
                    });

                    column.Item().PaddingTop(10).AlignCenter().Width(100).Height(100).Svg(BuildSealSvg());

                    column.Item().PaddingTop(7).AlignCenter().Width(420).Text("Congratulations on completing the entire QuestCraft learning journey.")
                        .FontSize(8.5f).Italic().FontColor(TextSecondaryHex).AlignCenter();

                    column.Item().PaddingTop(9).Row(footer =>
                    {
                        footer.RelativeItem(1).PaddingLeft(20).AlignLeft().AlignTop().Column(idBlock =>
                        {
                            idBlock.Item().Text("Certificate ID").FontSize(8).FontColor(TextSecondaryHex);
                            idBlock.Item().PaddingTop(2).Row(idRow =>
                            {
                                idRow.AutoItem().AlignMiddle().Text(data.CertificateId)
                                    .FontSize(12).Bold().FontFamily("Courier New").FontColor(TextPrimaryHex);
                                idRow.AutoItem().PaddingLeft(8).AlignMiddle().Text($"· Issued {data.IssuedAt:dd.MM.yyyy}")
                                    .FontSize(7.5f).FontColor(TextSecondaryHex);
                            });
                            idBlock.Item().PaddingTop(4).Text("Verifiable online via Certificate ID")
                                .FontSize(6.5f).Italic().FontColor(TextSecondaryHex);
                        });

                        footer.RelativeItem(1).AlignCenter();

                        footer.RelativeItem(1).PaddingRight(20).AlignRight().AlignTop().Column(sig =>
                        {
                            sig.Item().Width(140).Height(1).Background(CardBorderHex);
                            sig.Item().PaddingTop(4).AlignCenter().Text("QuestCraft System").FontSize(9.5f).Bold().FontColor(TextPrimaryHex);
                            sig.Item().AlignCenter().Text("Official Verification").FontSize(8).FontColor(TextSecondaryHex);
                        });
                    });

                    column.Item().PaddingTop(10).AlignCenter().Text("QuestCraft Learning Platform")
                        .FontSize(8).Bold().LetterSpacing(0.05f).FontColor(TextSecondaryHex);
                    column.Item().PaddingTop(1).AlignCenter().Text("www.questcraft.app")
                        .FontSize(7).FontColor(TextSecondaryHex);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void StatCard(QuestPDF.Infrastructure.IContainer container, string icon, string accentHex, string label, string value)
    {
        container.PaddingHorizontal(5).Height(62).Background(CardBgHex).CornerRadius(8).Border(1).BorderColor(CardBorderHex)
            .Padding(7).AlignMiddle().Column(col =>
            {
                col.Item().AlignCenter().Text(icon).FontSize(11).FontColor(accentHex);
                col.Item().PaddingTop(3).AlignCenter().Text(value).FontSize(13).Bold().FontColor(TextPrimaryHex);
                col.Item().PaddingTop(2).AlignCenter().Text(label).FontSize(6).LetterSpacing(0.04f).FontColor(TextSecondaryHex);
            });
    }

    // A4 landscape in points — matches PageSizes.A4.Landscape() exactly, so this SVG always covers
    // the full page regardless of how QuestPDF stretches/positions a background layer.
    private const float PageWidthPt = 842f;
    private const float PageHeightPt = 595f;
    private const float FrameInset = 26f;

    // Dark QuestCraft-style backdrop: gradient wash, faint grid (5-8% opacity), a soft purple/blue
    // glow behind the title, a giant near-invisible watermark wordmark, and a glowing rounded frame
    // (simulated via stacked same-path strokes of decreasing opacity/increasing width, rather than
    // an SVG blur filter, since that keeps the whole thing on primitives already proven to render
    // correctly with QuestPDF's SVG backend).
    private static string BuildBackgroundSvg()
    {
        var svg = new StringBuilder();
        svg.Append(System.FormattableString.Invariant(
            $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{PageWidthPt}\" height=\"{PageHeightPt}\" viewBox=\"0 0 {PageWidthPt} {PageHeightPt}\">"));

        svg.Append(
            "<defs>" +
            "<linearGradient id=\"bgGrad\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\">" +
            "<stop offset=\"0%\" stop-color=\"#0D1117\"/>" +
            "<stop offset=\"100%\" stop-color=\"#161B2F\"/>" +
            "</linearGradient>" +
            "<pattern id=\"grid\" width=\"24\" height=\"24\" patternUnits=\"userSpaceOnUse\">" +
            "<path d=\"M 24 0 L 0 0 0 24\" fill=\"none\" stroke=\"#8B5CF6\" stroke-width=\"0.6\"/>" +
            "</pattern>" +
            "<radialGradient id=\"titleGlowPurple\" cx=\"50%\" cy=\"50%\" r=\"50%\">" +
            "<stop offset=\"0%\" stop-color=\"#8B5CF6\" stop-opacity=\"0.34\"/>" +
            "<stop offset=\"100%\" stop-color=\"#8B5CF6\" stop-opacity=\"0\"/>" +
            "</radialGradient>" +
            "<radialGradient id=\"titleGlowBlue\" cx=\"50%\" cy=\"50%\" r=\"50%\">" +
            "<stop offset=\"0%\" stop-color=\"#3B82F6\" stop-opacity=\"0.22\"/>" +
            "<stop offset=\"100%\" stop-color=\"#3B82F6\" stop-opacity=\"0\"/>" +
            "</radialGradient>" +
            "<linearGradient id=\"frameGrad\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\">" +
            "<stop offset=\"0%\" stop-color=\"#8B5CF6\"/>" +
            "<stop offset=\"100%\" stop-color=\"#3B82F6\"/>" +
            "</linearGradient>" +
            "</defs>");

        svg.Append(System.FormattableString.Invariant($"<rect width=\"{PageWidthPt}\" height=\"{PageHeightPt}\" fill=\"url(#bgGrad)\"/>"));
        svg.Append(System.FormattableString.Invariant($"<rect width=\"{PageWidthPt}\" height=\"{PageHeightPt}\" fill=\"url(#grid)\" opacity=\"0.07\"/>"));

        svg.Append(System.FormattableString.Invariant(
            $"<text x=\"{PageWidthPt / 2}\" y=\"175\" font-family=\"Arial\" font-size=\"150\" font-weight=\"bold\" fill=\"#8B5CF6\" fill-opacity=\"0.045\" text-anchor=\"middle\">QUESTCRAFT</text>"));

        svg.Append(System.FormattableString.Invariant(
            $"<circle cx=\"{PageWidthPt / 2}\" cy=\"115\" r=\"230\" fill=\"url(#titleGlowPurple)\"/>"));
        svg.Append(System.FormattableString.Invariant(
            $"<circle cx=\"{PageWidthPt / 2}\" cy=\"125\" r=\"160\" fill=\"url(#titleGlowBlue)\"/>"));

        var frameX = FrameInset;
        var frameY = FrameInset;
        var frameW = PageWidthPt - (2 * FrameInset);
        var frameH = PageHeightPt - (2 * FrameInset);
        foreach (var (strokeWidth, opacity) in new[] { (14f, 0.07f), (7f, 0.14f), (1.8f, 1f) })
        {
            svg.Append(System.FormattableString.Invariant(
                $"<rect x=\"{frameX}\" y=\"{frameY}\" width=\"{frameW}\" height=\"{frameH}\" rx=\"18\" fill=\"none\" stroke=\"url(#frameGrad)\" stroke-width=\"{strokeWidth}\" opacity=\"{opacity}\"/>"));
        }

        svg.Append("</svg>");
        return svg.ToString();
    }

    // Premium gold "Completion Seal" — a metallic radial-gradient disc with a soft layered glow
    // halo (built the same stacked-opacity way as the frame, no blur filter needed) and baked-in
    // text, self-contained so the QuestPDF layout only has to place one sized element.
    private static string BuildSealSvg()
    {
        return
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"140\" height=\"140\" viewBox=\"0 0 140 140\">" +
            "<defs>" +
            "<radialGradient id=\"goldMetal\" cx=\"38%\" cy=\"32%\" r=\"75%\">" +
            "<stop offset=\"0%\" stop-color=\"#FFF6D8\"/>" +
            "<stop offset=\"45%\" stop-color=\"#FACC15\"/>" +
            "<stop offset=\"100%\" stop-color=\"#B8860B\"/>" +
            "</radialGradient>" +
            "<radialGradient id=\"sealGlow\" cx=\"50%\" cy=\"50%\" r=\"50%\">" +
            "<stop offset=\"0%\" stop-color=\"#FACC15\" stop-opacity=\"0.35\"/>" +
            "<stop offset=\"100%\" stop-color=\"#FACC15\" stop-opacity=\"0\"/>" +
            "</radialGradient>" +
            "</defs>" +
            "<circle cx=\"70\" cy=\"70\" r=\"64\" fill=\"url(#sealGlow)\"/>" +
            "<circle cx=\"70\" cy=\"70\" r=\"48\" fill=\"url(#sealGlow)\" opacity=\"0.6\"/>" +
            "<circle cx=\"70\" cy=\"70\" r=\"40\" fill=\"url(#goldMetal)\"/>" +
            "<circle cx=\"70\" cy=\"70\" r=\"40\" fill=\"none\" stroke=\"#FFF6D8\" stroke-width=\"1.5\" opacity=\"0.8\"/>" +
            "<circle cx=\"70\" cy=\"70\" r=\"36\" fill=\"none\" stroke=\"#8B5A00\" stroke-width=\"1\" opacity=\"0.45\"/>" +
            "<text x=\"70\" y=\"64\" font-family=\"Arial\" font-size=\"20\" font-weight=\"bold\" fill=\"#4A3200\" text-anchor=\"middle\">&#9733;</text>" +
            "<text x=\"70\" y=\"80\" font-family=\"Arial\" font-size=\"7\" font-weight=\"bold\" fill=\"#4A3200\" text-anchor=\"middle\" letter-spacing=\"0.5\">QUESTCRAFT</text>" +
            "<text x=\"70\" y=\"89\" font-family=\"Arial\" font-size=\"5.5\" fill=\"#4A3200\" text-anchor=\"middle\" letter-spacing=\"0.3\">COMPLETION SEAL</text>" +
            "</svg>";
    }
}

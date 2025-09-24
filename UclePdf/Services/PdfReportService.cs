using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace UclePdf.Services;

public interface IPdfReportService
{
    string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory);
    byte[] GenerateBasicHeaderPdfBytes(byte[]? headerImageBytes);
}

public class PdfReportService : IPdfReportService
{
    private void Compose(IDocumentContainer container, byte[]? headerImageBytes)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);

            // Header flexible sin altura fija para evitar conflictos de tamaño
            page.Header().Element(h =>
            {
                if (headerImageBytes is { Length: > 0 })
                {
                    try
                    {
                        // Contenedor flexible: la imagen se ajusta al ancho disponible manteniendo proporción.
                        h.Image(headerImageBytes).FitWidth();
                        return;
                    }
                    catch (Exception ex)
                    {
                        h.Border(1).BorderColor(Colors.Red.Medium).Background(Colors.Grey.Lighten4)
                         .Padding(8)
                         .Text($"Error imagen cabecera: {ex.Message}").FontSize(10).FontColor(Colors.Red.Darken2);
                        return;
                    }
                }

                // Placeholder si no hay imagen
                h.Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4)
                 .Padding(20)
                 .AlignCenter().AlignMiddle()
                 .Text("(Cabecera sin imagen)").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken2);
            });

            page.Content()
                .PaddingTop(15)
                .Text("(Contenido pendiente)")
                .FontSize(10)
                .FontColor(Colors.Grey.Darken2);
        });
    }

    public byte[] GenerateBasicHeaderPdfBytes(byte[]? headerImageBytes)
        => Document.Create(c => Compose(c, headerImageBytes)).GeneratePdf();

    public string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
        var file = Path.Combine(outputDirectory, $"Informe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        Document.Create(c => Compose(c, headerImageBytes)).GeneratePdf(file);
        return file;
    }
}

using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace UclePdf.Services;

public record InformeHeaderData(
    string Fecha,
    string Paciente,
    string EdadEspecieRazaSexo,
    string Propietario,
    string Veterinario,
    string Sucursal,
    string Bioquimico
);

public interface IPdfReportService
{
    string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory);
    byte[] GenerateBasicHeaderPdfBytes(byte[]? headerImageBytes);
    byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData);
}

public class PdfReportService : IPdfReportService
{
    private void ComposeSimple(IDocumentContainer container, byte[]? headerImageBytes)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.Header().Element(h => RenderHeaderImage(h, headerImageBytes));
            page.Content().PaddingTop(15).Text("(Contenido pendiente)").FontSize(10).FontColor(Colors.Grey.Darken2);
        });
    }

    private void ComposeFull(IDocumentContainer container, byte[]? headerImageBytes, InformeHeaderData data)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.Header().Element(h => RenderHeaderImage(h, headerImageBytes));
            page.Content().Column(col =>
            {
                // Card de datos (similar a la UI)
                col.Item().PaddingTop(10).Border(1).BorderColor(Colors.Grey.Lighten2).Background("#FFF8FAFC").Padding(12).Column(card =>
                {
                    card.Item().Element(e => RenderLabelValueGrid(e, data));
                });
                col.Item().PaddingTop(15).Text("(Contenido pendiente de resultados)").FontSize(10).FontColor(Colors.Grey.Darken2);
            });
        });
    }

    private void RenderHeaderImage(IContainer container, byte[]? headerImageBytes)
    {
        container.Element(h =>
        {
            if (headerImageBytes is { Length: > 0 })
            {
                try { h.Image(headerImageBytes).FitWidth(); return; }
                catch (Exception ex)
                {
                    h.Border(1).BorderColor(Colors.Red.Medium).Background(Colors.Grey.Lighten4).Padding(8)
                     .Text($"Error imagen cabecera: {ex.Message}").FontSize(10).FontColor(Colors.Red.Darken2);
                    return;
                }
            }
            h.Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4)
             .Padding(20).AlignCenter().AlignMiddle()
             .Text("(Cabecera sin imagen)").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken2);
        });
    }

    private void RenderLabelValueGrid(IContainer container, InformeHeaderData d)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c => { c.ConstantColumn(170); c.RelativeColumn(); });
            void Row(string label, string val)
            {
                table.Cell().Element(e => e.PaddingBottom(3)).Text(label).Bold().FontSize(11);
                table.Cell().Element(e => e.PaddingBottom(3)).Text(string.IsNullOrWhiteSpace(val) ? "-" : val).FontSize(11);
            }
            Row("Fecha:", d.Fecha);
            Row("Paciente:", d.Paciente);
            Row("Edad / Especie / Raza / Sexo:", d.EdadEspecieRazaSexo);
            Row("Propietario:", d.Propietario);
            Row("Veterinario solicitante:", d.Veterinario);
            Row("UCLE (sucursal):", d.Sucursal);
            Row("Bioquímico:", d.Bioquimico);
        });
    }

    public byte[] GenerateBasicHeaderPdfBytes(byte[]? headerImageBytes)
        => Document.Create(c => ComposeSimple(c, headerImageBytes)).GeneratePdf();

    public byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData)
        => Document.Create(c => ComposeFull(c, headerImageBytes, headerData)).GeneratePdf();

    public string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
        var file = Path.Combine(outputDirectory, $"Informe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        Document.Create(c => ComposeSimple(c, headerImageBytes)).GeneratePdf(file);
        return file;
    }
}

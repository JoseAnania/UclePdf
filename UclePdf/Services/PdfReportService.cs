using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
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

public record HemogramaRow(string Determinacion, double? Relativo, double? Absoluto, string Unidades, string RefCaninos, string RefFelinos);
public record HemogramaData(List<HemogramaRow> Rows, string? Observaciones, string? Especie);

public interface IPdfReportService
{
    string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory);
    byte[] GenerateBasicHeaderPdfBytes(byte[]? headerImageBytes);
    byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData);
    byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData, HemogramaData? hemograma);
}

public class PdfReportService : IPdfReportService
{
    private static string Format(double? v) => v.HasValue ? v.Value.ToString("0.##", CultureInfo.InvariantCulture) : "";

    private void ComposeSimple(IDocumentContainer container, byte[]? headerImageBytes)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.Header().Element(h => RenderHeaderImage(h, headerImageBytes));
            page.Content().PaddingTop(15).Text("(Contenido pendiente)");
        });
    }

    private void ComposeFull(IDocumentContainer container, byte[]? headerImageBytes, InformeHeaderData data, HemogramaData? hemograma)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.Header().Element(h => RenderHeaderImage(h, headerImageBytes));
            page.Content().Column(col =>
            {
                // Card datos
                col.Item().PaddingTop(8).Border(1).BorderColor(Colors.Grey.Lighten2).Background("#FFF8FAFC").Padding(10).Element(e =>
                {
                    e.Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.ConstantColumn(160); c.RelativeColumn(); });
                        void Row(string label, string val)
                        {
                            table.Cell().PaddingBottom(2).Text(label).FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Darken3);
                            table.Cell().PaddingBottom(2).Text(string.IsNullOrWhiteSpace(val) ? "-" : val).FontSize(8.5f).FontColor(Colors.Grey.Darken4);
                        }
                        Row("Fecha:", data.Fecha);
                        Row("Paciente:", data.Paciente);
                        Row("Edad / Especie / Raza / Sexo:", data.EdadEspecieRazaSexo);
                        Row("Propietario:", data.Propietario);
                        Row("Veterinario solicitante:", data.Veterinario);
                        Row("UCLE (sucursal):", data.Sucursal);
                        Row("Bioquímico:", data.Bioquimico);
                    });
                });

                if (hemograma != null)
                {
                    var especieNorm = (hemograma.Especie ?? "").Trim().ToLowerInvariant();
                    bool esCanino = especieNorm.Contains("can");
                    bool esFelino = especieNorm.Contains("fel");
                    var tituloRef = esCanino ? "Valores de Referencia (Canino)" : esFelino ? "Valores de Referencia (Felino)" : "Valores de Referencia";

                    // Título centrado y un poco más grande
                    col.Item().PaddingTop(10).Element(e =>
                        e.AlignCenter().Text(t => t.Span("HEMOGRAMA").FontSize(10).SemiBold())
                    );
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.5f);   // determinación
                            c.ConstantColumn(48);      // relativo
                            c.ConstantColumn(52);      // absoluto
                            c.ConstantColumn(58);      // unidades
                            c.RelativeColumn();        // referencia
                        });

                        void HeaderCell(string t)
                        {
                            var cell = table.Cell();
                            cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold().FontColor(Colors.Grey.Darken4));
                        }
                        HeaderCell("Determinación");
                        HeaderCell("Rel");
                        HeaderCell("Abs");
                        HeaderCell("Unid");
                        HeaderCell(tituloRef);

                        foreach (var r in hemograma.Rows)
                        {
                            var refValue = esCanino ? r.RefCaninos : esFelino ? r.RefFelinos : r.RefCaninos;
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.5f));
                            table.Cell().Element(c => c.Padding(3).Text(Format(r.Relativo)).FontSize(8.5f));
                            table.Cell().Element(c => c.Padding(3).Text(Format(r.Absoluto)).FontSize(8.5f));
                            table.Cell().Element(c => c.Padding(3).Text(r.Unidades).FontSize(8.3f));
                            table.Cell().Element(c => c.Padding(3).Text(refValue).FontSize(8.0f).FontColor(Colors.Grey.Darken2));
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(hemograma.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(box =>
                        {
                            box.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + hemograma.Observaciones).FontSize(8.2f);
                        });
                    }
                }

                // Línea separadora final entre informes (sin leyendas)
                col.Item().PaddingTop(16).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
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
                     .Text("Error imagen cabecera: " + ex.Message);
                    return;
                }
            }
            h.Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4)
             .Padding(20).AlignCenter().AlignMiddle()
             .Text("(Cabecera sin imagen)");
        });
    }

    public byte[] GenerateBasicHeaderPdfBytes(byte[]? headerImageBytes)
        => Document.Create(c => ComposeSimple(c, headerImageBytes)).GeneratePdf();

    public byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData)
        => Document.Create(c => ComposeFull(c, headerImageBytes, headerData, null)).GeneratePdf();

    public byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData, HemogramaData? hemograma)
        => Document.Create(c => ComposeFull(c, headerImageBytes, headerData, hemograma)).GeneratePdf();

    public string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
        var file = Path.Combine(outputDirectory, $"Informe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        Document.Create(c => ComposeSimple(c, headerImageBytes)).GeneratePdf(file);
        return file;
    }
}

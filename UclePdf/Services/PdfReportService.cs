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

// Nueva estructura para Química sanguínea
public record QuimicaRow(string Determinacion, double? Valor, string Unidades, string RefCaninos, string RefFelinos);
public record QuimicaData(List<QuimicaRow> Rows, string? Observaciones, string? Especie);
// Nueva estructura para Orina completa
public record OrinaRow(string Seccion, string Determinacion, string? Valor, string RefCaninos, string RefFelinos, bool TopSeparator);
public record OrinaData(List<OrinaRow> Rows, string? Observaciones, string? Especie);
// Nueva estructura Hemostasia (referencias no diferenciadas por especie en el modelo actual)
public record HemostasiaRow(string Determinacion, double? Valor, string Referencia);
public record HemostasiaData(List<HemostasiaRow> Rows, string? Observaciones);
// Nuevas estructuras Frotis y Copro
public record FrotisData(string? Resultado);
public record CoproRow(string Determinacion, string? Resultado);
public record CoproData(List<CoproRow> Rows, string? Observaciones);
// Nuevos registros Ehrlichiosis y Raspaje
public record EhrlichiosisRow(string Tecnica, string? Resultado);
public record EhrlichiosisData(List<EhrlichiosisRow> Rows, string? Observaciones);
public record RaspajeRow(string Determinacion, string? Resultado);
public record RaspajeData(List<RaspajeRow> Rows, string? Observaciones);
// Nuevos registros Reticulocitos
public record ReticulocitosRow(string Determinacion, double? Valor, string? Referencia);
public record ReticulocitosData(List<ReticulocitosRow> Rows, string? Observaciones);

public interface IPdfReportService
{
    string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory);
    byte[] GenerateBasicHeaderPdfBytes(byte[]? headerImageBytes);
    byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData);
    byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData,
        HemogramaData? hemograma, QuimicaData? quimica = null, OrinaData? orina = null, HemostasiaData? hemostasia = null,
        FrotisData? frotis = null, CoproData? copro = null, EhrlichiosisData? ehrlichiosis = null, RaspajeData? raspaje = null,
        ReticulocitosData? reticulocitos = null);
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
            // Sin header repetitivo
            page.Content().Column(col =>
            {
                // Logo solo al inicio (primera página) al estar como primer item del contenido.
                col.Item().Element(e => RenderLogoOnce(e, headerImageBytes));
                col.Item().PaddingTop(15).Text("(Contenido pendiente)");
            });
        });
    }

    private void ComposeFull(IDocumentContainer container, byte[]? headerImageBytes, InformeHeaderData data,
        HemogramaData? hemograma, QuimicaData? quimica, OrinaData? orina, HemostasiaData? hemostasia, FrotisData? frotis,
        CoproData? copro, EhrlichiosisData? ehrlichiosis, RaspajeData? raspaje, ReticulocitosData? reticulocitos)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.Content().Column(col =>
            {
                // Logo sólo en primera página
                col.Item().Element(e => RenderLogoOnce(e, headerImageBytes));

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

                // Hemograma
                if (hemograma != null)
                {
                    var especieNorm = (hemograma.Especie ?? "").Trim().ToLowerInvariant();
                    bool esCanino = especieNorm.Contains("can");
                    bool esFelino = especieNorm.Contains("fel");
                    var tituloRef = esCanino ? "Valores de Referencia (Canino)" : esFelino ? "Valores de Referencia (Felino)" : "Valores de Referencia";

                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("HEMOGRAMA").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.6f); // Determinación
                            c.ConstantColumn(55);    // Rel
                            c.ConstantColumn(60);    // Abs
                            c.RelativeColumn();      // Referencia (con unidades)
                        });

                        void HeaderCell(string t)
                        {
                            var cell = table.Cell();
                            cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold().FontColor(Colors.Grey.Darken4));
                        }
                        HeaderCell("Determinación");
                        HeaderCell("Rel");
                        HeaderCell("Abs");
                        HeaderCell(tituloRef);

                        foreach (var r in hemograma.Rows)
                        {
                            var refValueBase = esCanino ? r.RefCaninos : esFelino ? r.RefFelinos : r.RefCaninos;
                            var refValue = string.IsNullOrWhiteSpace(r.Unidades) ? refValueBase : $"{refValueBase} {r.Unidades}";
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.3f));
                            table.Cell().Element(c => c.Padding(3).Text(Format(r.Relativo)).FontSize(8.3f));
                            table.Cell().Element(c => c.Padding(3).Text(Format(r.Absoluto)).FontSize(8.3f));
                            table.Cell().Element(c => c.Padding(3).Text(refValue).FontSize(8f).FontColor(Colors.Grey.Darken2));
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(hemograma.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(box =>
                        {
                            box.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + hemograma.Observaciones).FontSize(8f);
                        });
                    }

                    if (quimica != null)
                        col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                }

                // Química
                if (quimica != null)
                {
                    var especieNormQ = (quimica.Especie ?? "").Trim().ToLowerInvariant();
                    bool esCaninoQ = especieNormQ.Contains("can");
                    bool esFelinoQ = especieNormQ.Contains("fel");
                    var tituloRefQ = esCaninoQ ? "Valores de Referencia (Canino)" : esFelinoQ ? "Valores de Referencia (Felino)" : "Valores de Referencia";

                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("QUÍMICA SANGUÍNEA").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.8f); // determinación
                            c.ConstantColumn(70);    // Valor
                            c.RelativeColumn();      // Referencia (con unidades)
                        });

                        void HeaderCellQ(string t)
                        {
                            var cell = table.Cell();
                            cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold().FontColor(Colors.Grey.Darken4));
                        }
                        HeaderCellQ("Determinación");
                        HeaderCellQ("Valor");
                        HeaderCellQ(tituloRefQ);

                        foreach (var r in quimica.Rows)
                        {
                            var refValueBase = esCaninoQ ? r.RefCaninos : esFelinoQ ? r.RefFelinos : r.RefCaninos;
                            var refValue = string.IsNullOrWhiteSpace(r.Unidades) ? refValueBase : $"{refValueBase} {r.Unidades}";
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(Format(r.Valor)).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(refValue).FontSize(8f).FontColor(Colors.Grey.Darken2));
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(quimica.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(box =>
                        {
                            box.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + quimica.Observaciones).FontSize(8f);
                        });
                    }
                }

                // Hemostasia
                if (hemostasia != null)
                {
                    // Separador si había otra sección antes
                    if (hemograma != null || quimica != null)
                        col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("HEMOSTASIA").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.8f); // determinación
                            c.ConstantColumn(70);    // valor
                            c.RelativeColumn();      // referencia
                        });

                        void HeaderCellH(string t)
                        {
                            var cell = table.Cell();
                            cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold().FontColor(Colors.Grey.Darken4));
                        }
                        HeaderCellH("Determinación");
                        HeaderCellH("Valor");
                        HeaderCellH("Referencia");

                        foreach (var r in hemostasia.Rows)
                        {
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(Format(r.Valor)).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(r.Referencia).FontSize(8f).FontColor(Colors.Grey.Darken2));
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(hemostasia.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(box =>
                        {
                            box.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + hemostasia.Observaciones).FontSize(8f);
                        });
                    }
                }

                // Orina Completa
                if (orina != null)
                {
                    var especieNormO = (orina.Especie ?? "").Trim().ToLowerInvariant();
                    bool esCaninoO = especieNormO.Contains("can");
                    bool esFelinoO = especieNormO.Contains("fel");
                    var tituloRefO = esCaninoO ? "Valores de Referencia (Canino)" : esFelinoO ? "Valores de Referencia (Felino)" : "Valores de Referencia";

                    // Separador si había otra sección antes
                    if (hemograma != null || quimica != null || hemostasia != null)
                        col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("ORINA COMPLETA").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    // Agrupar por sección
                    var grupos = orina.Rows.GroupBy(r => r.Seccion);
                    foreach (var grupo in grupos)
                    {
                        col.Item().PaddingTop(8).Text(grupo.Key).FontSize(8.5f).SemiBold().FontColor(Colors.Grey.Darken4);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1.7f); // determinación
                                c.RelativeColumn(0.9f); // valor
                                c.RelativeColumn();      // referencia con especie
                            });

                            void HeaderCellO(string t)
                            {
                                var cell = table.Cell();
                                cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(7.8f).SemiBold().FontColor(Colors.Grey.Darken4));
                            }
                            HeaderCellO("Determinación");
                            HeaderCellO("Valor");
                            HeaderCellO(tituloRefO);

                            foreach (var r in grupo)
                            {
                                var refValueBase = esCaninoO ? r.RefCaninos : esFelinoO ? r.RefFelinos : r.RefCaninos;
                                table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(7.6f));
                                table.Cell().Element(c => c.Padding(3).Text(string.IsNullOrWhiteSpace(r.Valor) ? "" : r.Valor).FontSize(7.6f));
                                table.Cell().Element(c => c.Padding(3).Text(refValueBase).FontSize(7.4f).FontColor(Colors.Grey.Darken2));
                            }
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(orina.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(box =>
                        {
                            box.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + orina.Observaciones).FontSize(8f);
                        });
                    }
                }

                // Frotis
                if (frotis != null && !string.IsNullOrWhiteSpace(frotis.Resultado))
                {
                    if (hemograma != null || quimica != null || orina != null || hemostasia != null)
                        col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("FROTIS").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.5f); c.RelativeColumn(); });
                        void HeaderCellF(string t)
                        {
                            var cell = table.Cell();
                            cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold());
                        }
                        HeaderCellF("Determinación");
                        HeaderCellF("Resultado");
                        table.Cell().Element(c => c.Padding(3).Text("Observacion Directa de Frotis para parasitos Sanguineos").FontSize(8.1f).SemiBold());
                        table.Cell().Element(c => c.Padding(3).Text(frotis.Resultado).FontSize(8f));
                    });
                }

                // Coproparasitológico
                if (copro != null && copro.Rows.Any())
                {
                    if (hemograma != null || quimica != null || orina != null || hemostasia != null || frotis != null)
                        col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("COPROPARASITOLÓGICO").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.2f); // determinación
                            c.RelativeColumn();     // resultado
                        });
                        void HeaderCellC(string t)
                        {
                            var cell = table.Cell();
                            cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold());
                        }
                        HeaderCellC("Determinación");
                        HeaderCellC("Resultado");
                        foreach (var r in copro.Rows)
                        {
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.1f));
                            table.Cell().Element(c => c.Padding(3).Text(r.Resultado ?? string.Empty).FontSize(8f));
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(copro.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(box =>
                        {
                            box.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + copro.Observaciones).FontSize(8f);
                        });
                    }
                }

                // === Secciones nuevas: Ehrlichiosis y Raspaje ===
                if (ehrlichiosis != null && ehrlichiosis.Rows.Any(r => !string.IsNullOrWhiteSpace(r.Resultado)))
                {
                    if (hemograma != null || quimica != null || orina != null || hemostasia != null || frotis != null || copro != null)
                        col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("EHRLICHIOSIS CANINA (IC)").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.6f); c.RelativeColumn(); });
                        void HeaderCellE(string t)
                        { var cell = table.Cell(); cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold()); }
                        HeaderCellE("Técnica");
                        HeaderCellE("Resultado");
                        foreach (var r in ehrlichiosis.Rows.Where(r => !string.IsNullOrWhiteSpace(r.Resultado)))
                        {
                            table.Cell().Element(c => c.Padding(3).Text(r.Tecnica).FontSize(8.1f));
                            table.Cell().Element(c => c.Padding(3).Text(r.Resultado!).FontSize(8f));
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(ehrlichiosis.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(box =>
                        {
                            box.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + ehrlichiosis.Observaciones).FontSize(8f);
                        });
                    }
                }

                if (raspaje != null && raspaje.Rows.Any(r => !string.IsNullOrWhiteSpace(r.Resultado)))
                {
                    if (hemograma != null || quimica != null || orina != null || hemostasia != null || frotis != null || copro != null || ehrlichiosis != null)
                        col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("RASPAJE DE PIEL").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.4f); c.RelativeColumn(); });
                        void HeaderCellR(string t)
                        { var cell = table.Cell(); cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold()); }
                        HeaderCellR("Determinación");
                        HeaderCellR("Resultado");
                        foreach (var r in raspaje.Rows.Where(r => !string.IsNullOrWhiteSpace(r.Resultado)))
                        {
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.1f));
                            table.Cell().Element(c => c.Padding(3).Text(r.Resultado!).FontSize(8f));
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(raspaje.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(box =>
                        {
                            box.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + raspaje.Observaciones).FontSize(8f);
                        });
                    }
                }

                // Reticulocitos
                if (reticulocitos != null && reticulocitos.Rows.Any(r => r.Valor.HasValue))
                {
                    // Separador si Hemograma no estuvo presente (para mantener consistencia con otras secciones)
                    col.Item().PaddingTop( hemograma != null ? 10 : 14 ).Element(e => e.AlignCenter().Text(t => t.Span("RECUENTO DE RETICULOCITOS").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    bool anyRef = reticulocitos.Rows.Any(r => !string.IsNullOrWhiteSpace(r.Referencia));
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.8f); // Determinación
                            c.ConstantColumn(70);    // Valor
                            if (anyRef) c.RelativeColumn(); // Referencia
                        });
                        void HeaderCellReti(string t)
                        { var cell = table.Cell(); cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold()); }
                        HeaderCellReti("Determinación");
                        HeaderCellReti("Valor");
                        if (anyRef) HeaderCellReti("Referencia");
                        foreach (var r in reticulocitos.Rows.Where(r => r.Valor.HasValue))
                        {
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(Format(r.Valor)).FontSize(8.2f));
                            if (anyRef) table.Cell().Element(c => c.Padding(3).Text(r.Referencia ?? string.Empty).FontSize(8f).FontColor(Colors.Grey.Darken2));
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(reticulocitos.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(b =>
                        {
                            b.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + reticulocitos.Observaciones).FontSize(8f);
                        });
                    }
                }

                // Línea final separadora
                col.Item().PaddingTop(16).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
            });
        });
    }

    private void RenderLogoOnce(IContainer container, byte[]? headerImageBytes)
    {
        container.Element(h =>
        {
            if (headerImageBytes is { Length: > 0 })
            {
                try { h.Image(headerImageBytes).FitWidth(); return; }
                catch (Exception)
                {
                    // Silenciosamente ignorar error de imagen
                }
            }
        });
    }

    private void RenderHeaderImage(IContainer container, byte[]? headerImageBytes)
    {
        // Mantener para compatibilidad si se usa en otros métodos (ya no se invoca en full)
        RenderLogoOnce(container, headerImageBytes);
    }

    public byte[] GenerateBasicHeaderPdfBytes(byte[]? headerImageBytes)
        => Document.Create(c => ComposeSimple(c, headerImageBytes)).GeneratePdf();

    public byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData)
        => Document.Create(c => ComposeFull(c, headerImageBytes, headerData, null, null, null, null, null, null, null, null, null)).GeneratePdf();

    public byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData,
        HemogramaData? hemograma, QuimicaData? quimica = null, OrinaData? orina = null, HemostasiaData? hemostasia = null,
        FrotisData? frotis = null, CoproData? copro = null, EhrlichiosisData? ehrlichiosis = null, RaspajeData? raspaje = null,
        ReticulocitosData? reticulocitos = null)
        => Document.Create(c => ComposeFull(c, headerImageBytes, headerData, hemograma, quimica, orina, hemostasia, frotis, copro, ehrlichiosis, raspaje, reticulocitos)).GeneratePdf();

    public string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
        var file = Path.Combine(outputDirectory, $"Informe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        Document.Create(c => ComposeSimple(c, headerImageBytes)).GeneratePdf(file);
        return file;
    }
}

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
// NUEVO: Proteinuria / Creatininuria
public record ProteinuriaRow(string Determinacion, double? Valor, string? Referencia);
public record ProteinuriaData(List<ProteinuriaRow> Rows, string? Observaciones, string ReferenciasBloque);
// Nuevo: VIF / VILEF
public record VifVilefData(string? VifResultado, string? VilefResultado, string? Observaciones);
// Nuevo: Ionograma
public record IonogramaRow(string Determinacion, double? Valor, string RefCanino, string RefFelino);
public record IonogramaData(List<IonogramaRow> Rows, string? Observaciones, string? Especie);
// Nuevo: Estudio Citológico
public record CitologicoRow(string Determinacion, string? Resultado);
public record CitologicoData(List<CitologicoRow> Rows, string? Observaciones);
// Nuevas estructuras para Líquido de Punción
public record LiquidoPuncionTextoRow(string Determinacion, string? Resultado);
public record LiquidoPuncionBioqRow(string Determinacion, double? Valor, string Unidades);
public record LiquidoPuncionData(List<LiquidoPuncionTextoRow> TextoRows, List<LiquidoPuncionBioqRow> BioqRows, string? Observaciones);

public interface IPdfReportService
{
    string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory);
    byte[] GenerateBasicHeaderPdfBytes(byte[]? headerImageBytes);
    byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData, byte[]? signatureImageBytes = null);
    byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData,
        HemogramaData? hemograma, QuimicaData? quimica = null, OrinaData? orina = null, HemostasiaData? hemostasia = null,
        FrotisData? frotis = null, CoproData? copro = null, EhrlichiosisData? ehrlichiosis = null, RaspajeData? raspaje = null,
        ReticulocitosData? reticulocitos = null, ProteinuriaData? proteinuria = null, VifVilefData? vifvilef = null,
        IonogramaData? ionograma = null, CitologicoData? citologico = null, LiquidoPuncionData? liquidoPuncion = null,
        byte[]? signatureImageBytes = null);
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
        CoproData? copro, EhrlichiosisData? ehrlichiosis, RaspajeData? raspaje, ReticulocitosData? reticulocitos,
        ProteinuriaData? proteinuria, VifVilefData? vifvilef, IonogramaData? ionograma, CitologicoData? citologico, LiquidoPuncionData? liquidoPuncion,
        byte[]? signatureImageBytes)
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
                            c.RelativeColumn(1.3f); // Determinación (antes 1.6f)
                            c.ConstantColumn(50);    // Rel (antes 55)
                            c.ConstantColumn(55);    // Abs (antes 60)
                            c.RelativeColumn();      // Referencia
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
                            c.RelativeColumn(1.4f); // determinación (antes 1.8f)
                            c.ConstantColumn(60);    // Valor (antes 70)
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
                            c.RelativeColumn(1.4f); // determinación (antes 1.8f)
                            c.ConstantColumn(60);    // valor (antes 70)
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
                                c.RelativeColumn(1.4f); // determinación (antes 1.7f)
                                c.RelativeColumn(0.8f); // valor (antes 0.9f)
                                c.RelativeColumn();      // referencia
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
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.3f); c.RelativeColumn(); });
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
                            c.RelativeColumn(1.0f); // determinación (antes 1.2f)
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
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.3f); c.RelativeColumn(); });
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
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.2f); c.RelativeColumn(); });
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
                            c.RelativeColumn(1.4f); // Determinación (antes 1.8f)
                            c.ConstantColumn(60);    // Valor (antes 70)
                            if (anyRef) c.RelativeColumn();
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

                // Proteinuria / Creatininuria (UPC)
                if (proteinuria != null && proteinuria.Rows.Any(r => r.Valor.HasValue))
                {
                    col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("RELACIÓN PROTEINURIA / CREATININURIA (UPC)").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    // Detectar especie desde la línea 2 del encabezado (Edad / Especie / ...)
                    var especieHeader = (data.EdadEspecieRazaSexo ?? string.Empty).ToLowerInvariant();
                    bool espCan = especieHeader.Contains("can");
                    bool espFel = especieHeader.Contains("fel");
                    var refUPC = espCan
                        ? "<0.2 Normal / 0.2-0.5 Dudoso / >0.5 Proteinuria"
                        : espFel
                            ? "<0.2 Normal / 0.2-0.4 Dudoso / >0.4 Proteinuria"
                            : "<0.2 Normal / 0.2-0.5 Dudoso / >0.5 Proteinuria"; // default canino

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.4f);   // Determinación (antes 1.8f)
                            c.ConstantColumn(60);      // Valor (antes 70)
                            c.RelativeColumn();        // Referencia
                        });
                        void HeaderCellP(string t)
                        { var cell = table.Cell(); cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold()); }
                        HeaderCellP("Determinación");
                        HeaderCellP("Valor");
                        HeaderCellP("Referencia");

                        foreach (var r in proteinuria.Rows.Where(r => r.Valor.HasValue))
                        {
                            var refCell = r.Referencia;
                            if (string.IsNullOrWhiteSpace(refCell) && r.Determinacion.Equals("UPC", StringComparison.OrdinalIgnoreCase))
                                refCell = refUPC;
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(Format(r.Valor)).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(refCell ?? string.Empty).FontSize(8f).FontColor(Colors.Grey.Darken2));
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(proteinuria.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(b =>
                        {
                            b.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + proteinuria.Observaciones).FontSize(8f);
                        });
                    }
                }

                // VIF / VILEF
                if (vifvilef != null && ((vifvilef.VifResultado != null && vifvilef.VifResultado != "Sin seleccion") || (vifvilef.VilefResultado != null && vifvilef.VilefResultado != "Sin seleccion")))
                {
                    col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("VIF / VILEF").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    var vifText = vifvilef.VifResultado != null && vifvilef.VifResultado != "Sin seleccion" ? vifvilef.VifResultado : string.Empty;
                    var vilefText = vifvilef.VilefResultado != null && vifvilef.VilefResultado != "Sin seleccion" ? vifvilef.VilefResultado : string.Empty;

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.5f); c.RelativeColumn(); });
                        void HeaderCellVV(string t)
                        { var cell = table.Cell(); cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold()); }
                        HeaderCellVV("Técnica empleada");
                        HeaderCellVV("Resultados");

                        var tecnica = "Inmunocromatografía (IC) Speed VIF / VILEF";
                        table.Cell().Element(c => c.Padding(3).Text(tecnica).FontSize(8.1f));
                        table.Cell().Element(c => c.Padding(3).Text(txt =>
                        {
                            if (!string.IsNullOrWhiteSpace(vifText)) txt.Span($"VIF: {vifText}").FontSize(8f);
                            if (!string.IsNullOrWhiteSpace(vifText) && !string.IsNullOrWhiteSpace(vilefText)) txt.Line("\n");
                            if (!string.IsNullOrWhiteSpace(vilefText)) txt.Span($"VILEF: {vilefText}").FontSize(8f);
                        }));
                    });

                    if (!string.IsNullOrWhiteSpace(vifvilef.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(b =>
                        {
                            b.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + vifvilef.Observaciones).FontSize(8f);
                        });
                    }
                }

                // Ionograma
                if (ionograma != null && ionograma.Rows.Any(r => r.Valor.HasValue))
                {
                    col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("IONOGRAMA").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    var especieNormI = (ionograma.Especie ?? "").Trim().ToLowerInvariant();
                    bool esCanI = especieNormI.Contains("can");
                    bool esFelI = especieNormI.Contains("fel");
                    var tituloRefI = esCanI ? "Valores de Referencia (Canino)" : esFelI ? "Valores de Referencia (Felino)" : "Valores de Referencia";

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.4f); c.ConstantColumn(60); c.RelativeColumn(); });
                        void HeaderCellI(string t) { var cell = table.Cell(); cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold()); }
                        HeaderCellI("Determinación");
                        HeaderCellI("Valor");
                        HeaderCellI(tituloRefI);
                        foreach (var r in ionograma.Rows.Where(r => r.Valor.HasValue))
                        {
                            var refBase = esCanI ? r.RefCanino : esFelI ? r.RefFelino : r.RefCanino;
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(Format(r.Valor)).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(refBase).FontSize(8f).FontColor(Colors.Grey.Darken2));
                        }
                    });
                    if (!string.IsNullOrWhiteSpace(ionograma.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(b => b.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + ionograma.Observaciones).FontSize(8f));
                    }
                }

                // Estudio Citológico
                if (citologico != null && citologico.Rows.Any(r => !string.IsNullOrWhiteSpace(r.Resultado)))
                {
                    col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("ESTUDIO CITOLÓGICO").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(1.5f); c.RelativeColumn(); });
                        void HeaderCellCI(string t) { var cell = table.Cell(); cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold()); }
                        HeaderCellCI("Determinación");
                        HeaderCellCI("Resultado");
                        foreach (var r in citologico.Rows.Where(r => !string.IsNullOrWhiteSpace(r.Resultado)))
                        {
                            table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.2f));
                            table.Cell().Element(c => c.Padding(3).Text(r.Resultado!).FontSize(8f));
                        }
                    });
                    if (!string.IsNullOrWhiteSpace(citologico.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(b => b.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + citologico.Observaciones).FontSize(8f));
                    }
                }

                // Líquido de Punción
                if (liquidoPuncion != null && (liquidoPuncion.TextoRows.Any(r => !string.IsNullOrWhiteSpace(r.Resultado)) || liquidoPuncion.BioqRows.Any(r => r.Valor.HasValue)))
                {
                    col.Item().PaddingTop(14).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));
                    col.Item().PaddingTop(10).Element(e => e.AlignCenter().Text(t => t.Span("LÍQUIDO DE PUNCIÓN").FontSize(10).SemiBold()));
                    col.Item().Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                    if (liquidoPuncion.TextoRows.Any(r => !string.IsNullOrWhiteSpace(r.Resultado)))
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(1.5f); c.RelativeColumn(); });
                            void H(string t) { var cell = table.Cell(); cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold()); }
                            H("Determinación"); H("Resultado");
                            foreach (var r in liquidoPuncion.TextoRows.Where(r => !string.IsNullOrWhiteSpace(r.Resultado)))
                            {
                                table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8.1f));
                                table.Cell().Element(c => c.Padding(3).Text(r.Resultado!).FontSize(8f));
                            }
                        });
                    }

                    if (liquidoPuncion.BioqRows.Any(r => r.Valor.HasValue))
                    {
                        // (Título eliminado para evitar redundancia)
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(1.5f); c.ConstantColumn(60); c.RelativeColumn(); });
                            void HB(string t) { var cell = table.Cell(); cell.Element(x => x.Background(Colors.Grey.Lighten3).Padding(3).Text(t).FontSize(8).SemiBold()); }
                            HB("Bioquímica"); HB("Resultado"); HB("Unidades");
                            foreach (var r in liquidoPuncion.BioqRows.Where(r => r.Valor.HasValue))
                            {
                                table.Cell().Element(c => c.Padding(3).Text(r.Determinacion).FontSize(8f));
                                table.Cell().Element(c => c.Padding(3).Text(Format(r.Valor)).FontSize(8f));
                                table.Cell().Element(c => c.Padding(3).Text(r.Unidades).FontSize(8f).FontColor(Colors.Grey.Darken2));
                            }
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(liquidoPuncion.Observaciones))
                    {
                        col.Item().PaddingTop(6).Element(b => b.Background(Colors.Grey.Lighten4).Padding(6).Text("Obs: " + liquidoPuncion.Observaciones).FontSize(8f));
                    }
                }

                // Línea final separadora siempre al terminar las secciones
                col.Item().PaddingTop(16).Element(e => e.Height(1).Background(Colors.Grey.Lighten2));

                // Firma digital debajo de la línea final (sin repetir el nombre, ya está en la imagen)
                if (signatureImageBytes is { Length: > 0 })
                {
                    col.Item().PaddingTop(6).AlignRight().Width(150).Element(e =>
                    {
                        e.Height(65).Image(signatureImageBytes).FitHeight();
                    });
                }
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
        => Document.Create(c => ComposeFull(c, headerImageBytes, headerData, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)).GeneratePdf();

    public byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData, byte[]? signatureImageBytes = null)
        => Document.Create(c => ComposeFull(c, headerImageBytes, headerData, null, null, null, null, null, null, null, null, null, null, null, null, null, null, signatureImageBytes)).GeneratePdf();

    public byte[] GenerateInformePdfBytes(byte[]? headerImageBytes, InformeHeaderData headerData,
        HemogramaData? hemograma, QuimicaData? quimica = null, OrinaData? orina = null, HemostasiaData? hemostasia = null,
        FrotisData? frotis = null, CoproData? copro = null, EhrlichiosisData? ehrlichiosis = null, RaspajeData? raspaje = null,
        ReticulocitosData? reticulocitos = null, ProteinuriaData? proteinuria = null, VifVilefData? vifvilef = null,
        IonogramaData? ionograma = null, CitologicoData? citologico = null, LiquidoPuncionData? liquidoPuncion = null,
        byte[]? signatureImageBytes = null)
        => Document.Create(c => ComposeFull(c, headerImageBytes, headerData, hemograma, quimica, orina, hemostasia, frotis, copro, ehrlichiosis, raspaje, reticulocitos, proteinuria, vifvilef, ionograma, citologico, liquidoPuncion, signatureImageBytes)).GeneratePdf();

    public string GenerateBasicHeaderPdf(byte[]? headerImageBytes, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
        var file = Path.Combine(outputDirectory, $"Informe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        Document.Create(c => ComposeSimple(c, headerImageBytes)).GeneratePdf(file);
        return file;
    }
}

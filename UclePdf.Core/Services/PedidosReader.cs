using ClosedXML.Excel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UclePdf.Core.Models;

namespace UclePdf.Core.Services;

public interface IPedidosReader
{
    List<Pedido> Leer(string path, DateTime? from = null);
}

public class PedidosReader : IPedidosReader
{
    public List<Pedido> Leer(string path, DateTime? from = null)
    {
        using var wb = new XLWorkbook(path);
        var ws = wb.Worksheets.First();

        var headerRow = ws.FirstRowUsed();
        var headers = headerRow.CellsUsed().ToDictionary(
            c => Normalize(c.GetString()),
            c => c.Address.ColumnNumber);

        int? Col(string name)
        {
            var key = Normalize(name);
            return headers.TryGetValue(key, out var idx) ? idx : null;
        }

        var colMarcaTemporal = Col("Marca temporal");
        var colCorreo = Col("Dirección de correo electrónico");
        var colSucursal = Col("SUCURSAL");
        var colVet = Col("VETERINARIO SOLICITANTE");
        var colProp = Col("PROPIETARIO");
        var colPaciente = Col("NOMBRE DEL PACIENTE");
        
        var colEspecie = Col("ESPECIE");
        var colSexo = Col("SEXO");
        var colRaza = Col("RAZA");
        var colEdad = Col("EDAD");

        var list = new List<Pedido>();
        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var marcaTemporal = TryDate(row.Cell(colMarcaTemporal ?? 0));
            var p = new Pedido
            {
                MarcaTemporal = marcaTemporal,
                CorreoElectronico = Get(row, colCorreo),
                Sucursal = Get(row, colSucursal),
                VeterinarioSolicitante = Get(row, colVet),
                Propietario = Get(row, colProp),
                NombrePaciente = Get(row, colPaciente)
            };

            p.Especie = Get(row, colEspecie);
            p.Sexo = Get(row, colSexo);
            p.Raza = Get(row, colRaza);
            ParseEdad(Get(row, colEdad), p);

            if (from is DateTime fromDate)
            {
                if (!p.MarcaTemporal.HasValue || p.MarcaTemporal.Value < fromDate)
                    continue;
            }

            if (!string.IsNullOrWhiteSpace(p.NombrePaciente) || !string.IsNullOrWhiteSpace(p.Propietario))
                list.Add(p);
        }
        return list;
    }

    private static string? Get(IXLRow row, int? col)
        => col is null ? null : row.Cell(col.Value).GetString()?.Trim();

    private static DateTime? TryDate(IXLCell cell)
    {
        if (cell.TryGetValue<DateTime>(out var dt)) return dt;
        var s = cell.GetString();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParse(s, out var dt2)) return dt2;
        return null;
    }

    private static void ParseEdad(string? raw, Pedido p)
    {
        p.EdadCantidad = null;
        p.EdadUnidad = null;
        if (string.IsNullOrWhiteSpace(raw)) return;
        var s = RemoveAccents(raw).ToLowerInvariant();
        var m = Regex.Match(s, @"\d+");
        if (!m.Success) return;
        if (int.TryParse(m.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
            p.EdadCantidad = n;
        if (s.Contains("mes")) p.EdadUnidad = "Meses";
        else if (s.Contains("ano") || s.Contains("año")) p.EdadUnidad = "Años";
        else p.EdadUnidad = "Años";
    }

    private static string Normalize(string s)
    {
        s = s.Trim().ToLowerInvariant();
        s = RemoveAccents(s);
        return s;
    }

    private static string RemoveAccents(string text)
    {
        var norm = text.Normalize(NormalizationForm.FormD);
        var chars = new List<char>(norm.Length);
        foreach (var ch in norm)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
                chars.Add(ch);
        }
        return new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
    }
}

using ClosedXML.Excel;
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

        // Mapear cabeceras a índices de columna (case-insensitive, sin acentos básicos)
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

            // Filtro por fecha (desde)
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

    private static string Normalize(string s)
    {
        s = s.Trim().ToLowerInvariant();
        s = s
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("ä", "a").Replace("ë", "e").Replace("ï", "i").Replace("ö", "o").Replace("ü", "u")
            .Replace("ñ", "n");
        return s;
    }
}

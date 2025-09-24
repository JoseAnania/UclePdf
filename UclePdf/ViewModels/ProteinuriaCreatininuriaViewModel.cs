using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class PcrRelItem : INotifyPropertyChanged
{
    public PcrRelItem(string determinacion, string? referencia = null)
    {
        Determinacion = determinacion;
        Referencia = referencia;
    }

    public string Determinacion { get; }
    public string? Referencia { get; }

    private double? _valor;
    public double? Valor
    {
        get => _valor;
        set { if (_valor != value) { _valor = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class ProteinuriaCreatininuriaViewModel : INotifyPropertyChanged
{
    public ObservableCollection<PcrRelItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed { get => _isConfirmed; private set { if (value != _isConfirmed) { _isConfirmed = value; OnPropertyChanged(); } } }

    private string? _observaciones;
    public string? Observaciones { get => _observaciones; set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } } }

    // Bloque unificado de referencias para panel lateral (formato similar al solicitado)
    public string ReferenciasBloque =>
        "Menor a 0.2: Sin proteinuria" +
        "\n\nEntre 0.2 - 0.5: Dudoso (mayor importancia en presencia de azotemia)" +
        "\n\nMayor a 0.5: Proteinuria (caninos)\nMayor a 0.4: Proteinuria (felinos)";

    public ProteinuriaCreatininuriaViewModel()
    {
        // Referencias distribuidas por fila solo para mantener compatibilidad interna (ya no se muestran en la grilla)
        Items.Add(new PcrRelItem("UPC"));
        Items.Add(new PcrRelItem("Proteinuria (mg/dl)"));
        Items.Add(new PcrRelItem("Creatininuria (mg/dl)"));
    }

    public void Confirm()
    {
        IsConfirmed = Items.Any(i => i.Valor.HasValue);
    }

    public void ClearValores()
    {
        foreach (var i in Items) i.Valor = null;
        Observaciones = null;
        IsConfirmed = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

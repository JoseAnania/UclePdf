using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class CitologicoItem : INotifyPropertyChanged
{
    public CitologicoItem(string determinacion, string? valorPorDefecto = null)
    {
        Determinacion = determinacion;
        _resultado = valorPorDefecto;
        ValorPorDefecto = valorPorDefecto;
    }

    public string Determinacion { get; }
    public string? ValorPorDefecto { get; }

    private string? _resultado;
    public string? Resultado
    {
        get => _resultado;
        set { if (_resultado != value) { _resultado = value; OnPropertyChanged(); } }
    }

    public void Clear() => Resultado = null;
    public void ResetIfEmpty() { if (string.IsNullOrWhiteSpace(Resultado)) Resultado = ValorPorDefecto; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class CitologicoViewModel : INotifyPropertyChanged
{
    public ObservableCollection<CitologicoItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed { get => _isConfirmed; private set { if (value != _isConfirmed) { _isConfirmed = value; OnPropertyChanged(); } } }

    private string? _observaciones;
    public string? Observaciones { get => _observaciones; set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } } }

    public CitologicoViewModel()
    {
        Items.Add(new CitologicoItem("Material"));
        Items.Add(new CitologicoItem("Metodo Directo", "Color Fast (Biopack)"));
    }

    public void Confirm()
    {
        IsConfirmed = Items.Any(i => !string.IsNullOrWhiteSpace(i.Resultado));
    }

    public void ClearResultados()
    {
        foreach (var it in Items) it.Clear();
        Observaciones = null;
        IsConfirmed = false;
    }

    public void ResetDefaults()
    {
        foreach (var it in Items) it.ResetIfEmpty();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class CoproItem : INotifyPropertyChanged
{
    public CoproItem(string determinacion, string valorPorDefecto)
    {
        Determinacion = determinacion;
        _valorPorDefecto = valorPorDefecto;
        _resultado = valorPorDefecto;
    }

    public string Determinacion { get; }

    private readonly string _valorPorDefecto;

    private string? _resultado;
    public string? Resultado
    {
        get => _resultado;
        set { if (_resultado != value) { _resultado = value; OnPropertyChanged(); } }
    }

    public void ResetIfEmpty()
    {
        if (string.IsNullOrWhiteSpace(Resultado))
            Resultado = _valorPorDefecto;
    }

    public void Clear() => Resultado = null;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class CoproparasitologicoViewModel : INotifyPropertyChanged
{
    public ObservableCollection<CoproItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed
    {
        get => _isConfirmed;
        private set { if (value != _isConfirmed) { _isConfirmed = value; OnPropertyChanged(); } }
    }

    public CoproparasitologicoViewModel()
    {
        // Valores precargados (capitalización normal)
        Add("Observacion macroscopica", "No se observan gusanos ni proglotides.");
        Add("Observacion microscopica directa", "No se observan huevos, quistes ni trofozoitos.");
        Add("Observacion microscopica concentrada", "No se observan huevos, quistes ni trofozoitos.");
    }

    private void Add(string det, string def) => Items.Add(new CoproItem(det, def));

    public void ClearResultados()
    {
        foreach (var it in Items) it.Clear();
        IsConfirmed = false;
    }

    public void Confirm()
    {
        // Considerar confirmado sólo si al menos un resultado con texto
        IsConfirmed = Items.Any(i => !string.IsNullOrWhiteSpace(i.Resultado));
    }

    public void ResetIfEmpty()
    {
        foreach (var it in Items) it.ResetIfEmpty();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

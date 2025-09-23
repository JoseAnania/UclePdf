using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class RaspajeItem : INotifyPropertyChanged
{
    public RaspajeItem(string determinacion, string valorPorDefecto, bool editable)
    {
        Determinacion = determinacion;
        _valorPorDefecto = valorPorDefecto;
        _resultado = valorPorDefecto;
        IsEditable = editable;
    }

    public string Determinacion { get; }
    public bool IsEditable { get; }

    private readonly string _valorPorDefecto;

    private string? _resultado;
    public string? Resultado
    {
        get => _resultado;
        set { if (IsEditable && _resultado != value) { _resultado = value; OnPropertyChanged(); } }
    }

    public void Clear()
    {
        if (IsEditable)
            Resultado = null; // mantiene la etiqueta fija
    }
    public void ResetIfEmpty() { if (IsEditable && string.IsNullOrWhiteSpace(Resultado)) Resultado = _valorPorDefecto; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class RaspajePielViewModel : INotifyPropertyChanged
{
    public ObservableCollection<RaspajeItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed { get => _isConfirmed; private set { if (value != _isConfirmed) { _isConfirmed = value; OnPropertyChanged(); } } }

    public RaspajePielViewModel()
    {
        Items.Add(new RaspajeItem("Determinación", "Raspaje de piel", editable: false));
        Items.Add(new RaspajeItem("Observación microscópica", "No se observan ácaros ni hongos.", editable: true));
    }

    public void Confirm()
    {
        // Solo depende de la fila editable (observación)
        var obs = Items.FirstOrDefault(i => i.IsEditable);
        IsConfirmed = !string.IsNullOrWhiteSpace(obs?.Resultado);
    }

    public void Clear()
    {
        foreach (var it in Items) it.Clear();
        IsConfirmed = false;
    }

    public void ResetIfEmpty()
    {
        foreach (var it in Items) it.ResetIfEmpty();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

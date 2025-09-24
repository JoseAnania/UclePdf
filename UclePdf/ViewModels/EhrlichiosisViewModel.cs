using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class EhrlichiosisItem : INotifyPropertyChanged
{
    public EhrlichiosisItem(string tecnica)
    {
        Tecnica = tecnica;
    }

    public string Tecnica { get; }

    private string? _resultado;
    public string? Resultado
    {
        get => _resultado;
        set { if (_resultado != value) { _resultado = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class EhrlichiosisViewModel : INotifyPropertyChanged
{
    public ObservableCollection<EhrlichiosisItem> Items { get; } = new();

    public ObservableCollection<string> ResultadosPosibles { get; } = new()
    {
        "Sin seleccion",
        "Positivo (+)",
        "Negativo (-)"
    };

    private bool _isConfirmed;
    public bool IsConfirmed
    {
        get => _isConfirmed;
        private set { if (_isConfirmed != value) { _isConfirmed = value; OnPropertyChanged(); } }
    }

    private string? _observaciones;
    public string? Observaciones
    {
        get => _observaciones;
        set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } }
    }

    public EhrlichiosisViewModel()
    {
        Items.Add(new EhrlichiosisItem("Inmunocromatografia (IC)\nSpeed Ehrlichia"));
        Items[0].Resultado = "Sin seleccion";
    }

    public void Confirm()
    {
        var it = Items.First();
        IsConfirmed = !string.IsNullOrWhiteSpace(it.Resultado) && it.Resultado != "Sin seleccion";
    }

    public void Clear()
    {
        foreach (var it in Items) it.Resultado = "Sin seleccion";
        Observaciones = null;
        IsConfirmed = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
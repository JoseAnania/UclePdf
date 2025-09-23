using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class HemostasiaItem : INotifyPropertyChanged
{
    public HemostasiaItem(string determinacion, string referencia)
    {
        Determinacion = determinacion;
        Referencia = referencia;
    }

    public string Determinacion { get; }
    public string Referencia { get; }

    private double? _valor;
    public double? Valor
    {
        get => _valor;
        set { if (_valor != value) { _valor = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class HemostasiaViewModel : INotifyPropertyChanged
{
    public ObservableCollection<HemostasiaItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed { get => _isConfirmed; private set { if (_isConfirmed != value) { _isConfirmed = value; OnPropertyChanged(); } } }

    private string? _observaciones;
    public string? Observaciones { get => _observaciones; set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } } }

    public HemostasiaViewModel()
    {
        Add("Tiempo de protrombina", "Hasta 15 segundos");
        Add("Tiempo de tromboplastina parcial activada (KPTT)", "Hasta 25 segundos");
    }

    private void Add(string det, string refValue)
        => Items.Add(new HemostasiaItem(det, refValue));

    public void ClearValores()
    {
        foreach (var i in Items) i.Valor = null;
        Observaciones = null;
        IsConfirmed = false;
        OnPropertyChanged(nameof(Items));
    }

    public void Confirm() => IsConfirmed = true;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class ReticulocitoItem : INotifyPropertyChanged
{
    public ReticulocitoItem(string determinacion, string? referencia = null)
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

public class ReticulocitosViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ReticulocitoItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed { get => _isConfirmed; private set { if (_isConfirmed != value) { _isConfirmed = value; OnPropertyChanged(); } } }

    private string? _observaciones;
    public string? Observaciones { get => _observaciones; set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } } }

    public ReticulocitosViewModel()
    {
        Items.Add(new ReticulocitoItem("Recuento de reticulocitos (%)"));
        Items.Add(new ReticulocitoItem("IPR (Indice de Produccion Reticulocitaria)", "1.0"));
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

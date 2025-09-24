using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class IonogramaItem : INotifyPropertyChanged
{
    public IonogramaItem(string determinacion, string refCanino, string refFelino)
    {
        Determinacion = determinacion;
        RefCanino = refCanino;
        RefFelino = refFelino;
    }

    public string Determinacion { get; }
    public string RefCanino { get; }
    public string RefFelino { get; }

    private double? _valor;
    public double? Valor
    {
        get => _valor;
        set { if (_valor != value) { _valor = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class IonogramaViewModel : INotifyPropertyChanged
{
    public ObservableCollection<IonogramaItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed { get => _isConfirmed; private set { if (value != _isConfirmed) { _isConfirmed = value; OnPropertyChanged(); } } }

    private string? _observaciones;
    public string? Observaciones { get => _observaciones; set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } } }

    public IonogramaViewModel()
    {
        Items.Add(new IonogramaItem("Sodio (mmol/L)", "140-155", "145-160"));
        Items.Add(new IonogramaItem("Potasio (mmol/L)", "3,8-5,8", "3,7-5,0"));
        Items.Add(new IonogramaItem("Cloro (mmol/L)", "105-129", "105-129"));
    }

    public void Confirm()
    {
        IsConfirmed = Items.Any(i => i.Valor.HasValue);
    }

    public void ClearValores()
    {
        foreach (var x in Items) x.Valor = null;
        Observaciones = null;
        IsConfirmed = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

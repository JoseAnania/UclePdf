using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class VifVilefViewModel : INotifyPropertyChanged
{
    public ObservableCollection<string> ResultadosPosibles { get; } = new()
    {
        "Sin seleccion",
        "Positivo (+)",
        "Negativo (-)"
    };

    private string? _vifResultado;
    public string? VifResultado
    {
        get => _vifResultado;
        set { if (_vifResultado != value) { _vifResultado = value; OnPropertyChanged(); } }
    }

    private string? _vilefResultado;
    public string? VilefResultado
    {
        get => _vilefResultado;
        set { if (_vilefResultado != value) { _vilefResultado = value; OnPropertyChanged(); } }
    }

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

    public VifVilefViewModel()
    {
        VifResultado = "Sin seleccion";
        VilefResultado = "Sin seleccion";
    }

    public void Confirm()
    {
        IsConfirmed = (VifResultado != null && VifResultado != "Sin seleccion") ||
                      (VilefResultado != null && VilefResultado != "Sin seleccion");
    }

    public void Clear()
    {
        VifResultado = "Sin seleccion";
        VilefResultado = "Sin seleccion";
        Observaciones = null;
        IsConfirmed = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

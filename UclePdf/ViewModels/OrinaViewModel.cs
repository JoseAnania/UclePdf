using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class OrinaItem : INotifyPropertyChanged
{
    public OrinaItem(string seccion, string determinacion, string refCaninos, string refFelinos, bool topSeparator = false, bool isNumeric = false)
    {
        Seccion = seccion;
        Determinacion = determinacion;
        RefCaninos = refCaninos;
        RefFelinos = refFelinos;
        TopSeparator = topSeparator;
        IsNumeric = isNumeric;
    }

    public string Seccion { get; }
    public string Determinacion { get; }
    public string RefCaninos { get; }
    public string RefFelinos { get; }
    public bool TopSeparator { get; }
    public bool IsNumeric { get; }

    private string? _valor;
    public string? Valor
    {
        get => _valor;
        set { if (_valor != value) { _valor = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class OrinaViewModel : INotifyPropertyChanged
{
    public ObservableCollection<OrinaItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed
    {
        get => _isConfirmed;
        private set { if (value != _isConfirmed) { _isConfirmed = value; OnPropertyChanged(); } }
    }

    private string? _observaciones;
    public string? Observaciones
    {
        get => _observaciones;
        set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } }
    }

    public OrinaViewModel()
    {
        void Add(string seccion, string det, string can, string fel, bool top = false, bool numeric = false)
            => Items.Add(new OrinaItem(seccion, det, can, fel, top, numeric));

        const string fisicoquimico = "Examen fisicoquimico";
        const string sediment = "Examen microscopico del sedimento";

        // Examen Fisicoquímico
        Add(fisicoquimico, "Color", "amarillo ambar", "amarillo ambar", top: true);
        Add(fisicoquimico, "Aspecto", "limpido", "limpido");
        Add(fisicoquimico, "Densidad (gr/ml)", "1015-1045", "1020-1050");
        Add(fisicoquimico, "Ph", "5-7", "5-7");
        Add(fisicoquimico, "Proteinas", "no contiene", "no contiene");
        Add(fisicoquimico, "Glucosa", "no contiene", "no contiene");
        Add(fisicoquimico, "Cetonas", "no contiene", "no contiene");
        Add(fisicoquimico, "Pigmentos biliares", "trazas, hasta +", "trazas, hasta +");
        Add(fisicoquimico, "Urobilina", "trazas, hasta +", "trazas, hasta +");
        Add(fisicoquimico, "Hemoglobina", "no contiene", "no contiene");
        Add(fisicoquimico, "Nitritos", "no contiene", "no contiene");

        // Examen microscópico del sedimento
        Add(sediment, "Celulas epiteliales planas", "1-5/campo", "1-5/campo", top: true);
        Add(sediment, "Celulas de transicion", "1-5/campo", "1-5/campo");
        Add(sediment, "Celulas renales", "no contiene", "no contiene");
        Add(sediment, "Leucocitos", "0-3/campo", "0-3/campo");
        Add(sediment, "Hematies", "0-3/campo", "0-3/campo");
        Add(sediment, "Cilindros", "1-2/preparado", "1-2/preparado");
    }

    public void ClearValores()
    {
        foreach (var it in Items) it.Valor = null;
        Observaciones = null;
        IsConfirmed = false;
        OnPropertyChanged(nameof(Items));
    }

    public void Confirm() => IsConfirmed = true;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

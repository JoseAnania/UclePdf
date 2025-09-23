using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class QuimicaItem : INotifyPropertyChanged
{
    private readonly QuimicaViewModel _owner;

    public QuimicaItem(QuimicaViewModel owner, string determinacion, string unidades, string refCaninos, string refFelinos, bool editable = true, Func<QuimicaViewModel, double?>? compute = null)
    {
        _owner = owner;
        Determinacion = determinacion;
        Unidades = unidades;
        RefCaninos = refCaninos;
        RefFelinos = refFelinos;
        IsEditable = editable;
        Compute = compute;
    }

    public string Determinacion { get; }
    public string Unidades { get; }
    public string RefCaninos { get; }
    public string RefFelinos { get; }

    private double? _valorEditable;
    public double? ValorEditable
    {
        get => _valorEditable;
        set
        {
            if (SetProperty(ref _valorEditable, value))
            {
                _owner.NotifyValuesChanged();
                Raise(nameof(Valor));
            }
        }
    }

    public bool IsEditable { get; }

    public Func<QuimicaViewModel, double?>? Compute { get; }

    public double? ValorHallado => Compute?.Invoke(_owner) ?? ValorEditable;

    public bool EsCalculado => Compute != null;

    // Propiedad unificada para mostrar/editar
    public double? Valor
    {
        get => Compute?.Invoke(_owner) ?? ValorEditable;
        set
        {
            if (IsEditable)
            {
                ValorEditable = value;
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return false;
        field = value!;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
    public void Raise([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class QuimicaViewModel : INotifyPropertyChanged
{
    public ObservableCollection<QuimicaItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed { get => _isConfirmed; private set { if (value != _isConfirmed) { _isConfirmed = value; OnPropertyChanged(); } } }

    private string? _observaciones;
    public string? Observaciones { get => _observaciones; set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } } }

    public QuimicaViewModel()
    {
        QuimicaItem Add(string det, string uni, string can, string fel, bool editable = true, Func<QuimicaViewModel, double?>? f = null)
        {
            var it = new QuimicaItem(this, det, uni, can, fel, editable, f);
            Items.Add(it);
            return it;
        }

        var urea = Add("Urea", "mg/dl", "20- 50", "30- 60");
        var crea = Add("Creatinina", "mg/dl", "0,5 - 1,5", "0,5 - 1,5");
        var got = Add("Got", "UI/l", "Hasta 50", "Hasta 80");
        var gpt = Add("Gpt", "UI/l", "Hasta 50", "Hasta 80");
        var fosfa = Add("Fosfatasa alcalina", "UI/l", "Hasta 250", "Hasta 150");
        var fosforo = Add("Fosforo", "mg/dl", "2,0-6,1", "2,8-7,5");
        var colTotal = Add("Colesterol total", "mg/dl", "135-260", "80-180");
        var colHdl = Add("Colesterol hdl", "mg/dl", ">110", ">100");
        // LDL calculado: si Trigliceridos <= 400 usar /5, si > 400 usar /7
        var colLdl = Add("Colesterol ldl", "mg/dl", "<50", "<60", editable: false, f: vm =>
        {
            var total = vm.Find("Colesterol total")?.ValorEditable;
            var hdl = vm.Find("Colesterol hdl")?.ValorEditable;
            var trig = vm.Find("Trigliceridos")?.ValorEditable;
            if (total is null || hdl is null || trig is null) return null;
            var divisor = trig.Value <= 400 ? 5.0 : 7.0;
            var ldl = total.Value - (trig.Value / divisor) - hdl.Value;
            return Math.Round(ldl, 1);
        });
        var trig = Add("Trigliceridos", "mg/dl", "25-120", "25-120");
        var ggt = Add("Ggt", "U/L", "hasta 12", "hasta 10");
        var calcio = Add("Calcio", "mg/dl", "8-11", "8-11");
        var glucosa = Add("Glucosa", "mg/dl", "60-110", "60-100");
        var protTot = Add("Proteinas totales", "g/dl", "6,2 5,3-7,9", "5,7-8,0");
        var albumina = Add("Albumina", "g/dl", "2,3-3,8", "2,3-3,4");
        // Globulinas = Proteinas totales - Albumina
        var globulinas = Add("Globulinas", "g/dl", "2,4-4,0", "2,6-4,5", editable: false, f: vm =>
        {
            var tot = vm.Find("Proteinas totales")?.ValorEditable;
            var alb = vm.Find("Albumina")?.ValorEditable;
            if (tot is null || alb is null) return null;
            return Math.Round(tot.Value - alb.Value, 1);
        });
        var amilasa = Add("Amilasa", "U/L", "hasta 1500", "de 500 a 1500");
        var lipasa = Add("Lipasa", "U/L", "hasta 240", "hasta 75");
        var biliTotal = Add("Bilirrubina total", "mg/dl", "hasta 0,8", "hasta 0,8");
        var biliDirecta = Add("Bilirrubina directa", "mg/dl", "hasta 0,4", "hasta 0,4");
        var biliInd = Add("Bilirrubina indirecta", "mg/dl", "hasta 0,4", "hasta 0,4", editable: false, f: vm =>
        {
            var tot = vm.Find("Bilirrubina total")?.ValorEditable;
            var dir = vm.Find("Bilirrubina directa")?.ValorEditable;
            if (tot is null || dir is null) return null;
            return Math.Round(tot.Value - dir.Value, 1);
        });
    }

    public QuimicaItem? Find(string determinacion)
        => Items.FirstOrDefault(i => string.Equals(i.Determinacion, determinacion, StringComparison.OrdinalIgnoreCase));

    internal void NotifyValuesChanged()
    {
        foreach (var it in Items)
        {
            it.Raise(nameof(QuimicaItem.ValorHallado));
            it.Raise(nameof(QuimicaItem.Valor));
        }
    }

    public void ClearValores()
    {
        foreach (var it in Items)
        {
            if (it.IsEditable) it.ValorEditable = null;
        }
        Observaciones = null;
        IsConfirmed = false;
        NotifyValuesChanged();
        OnPropertyChanged(nameof(Items));
    }

    public void Confirm() => IsConfirmed = true;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

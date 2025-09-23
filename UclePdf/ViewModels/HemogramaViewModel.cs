using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class HemogramaItem : INotifyPropertyChanged
{
    private readonly HemogramaViewModel _owner;

    public HemogramaItem(HemogramaViewModel owner, string determinacion, string unidades, string refCaninos, string refFelinos, bool editable = true, Func<HemogramaViewModel, double?>? computeAbs = null)
    {
        _owner = owner;
        Determinacion = determinacion;
        Unidades = unidades;
        RefCaninos = refCaninos;
        RefFelinos = refFelinos;
        IsEditableRelativo = editable;
        ComputeAbsoluto = computeAbs;
    }

    public string Determinacion { get; }
    public string Unidades { get; }
    public string RefCaninos { get; }
    public string RefFelinos { get; }

    private double? _valorRelativo;
    public double? ValorRelativo
    {
        get => _valorRelativo;
        set
        {
            if (SetProperty(ref _valorRelativo, value))
            {
                _owner.NotifyAbsolutosChanged();
            }
        }
    }

    public bool IsEditableRelativo { get; }

    public Func<HemogramaViewModel, double?>? ComputeAbsoluto { get; }

    public double? ValorAbsoluto => ComputeAbsoluto?.Invoke(_owner);

    // Indica si ValorAbsoluto es calculado automáticamente
    public bool EsCalculadoAbsoluto => ComputeAbsoluto != null;

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

public class HemogramaViewModel : INotifyPropertyChanged
{
    public ObservableCollection<HemogramaItem> Items { get; } = new();

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

    public HemogramaViewModel()
    {
        HemogramaItem Add(string det, string uni, string can, string fel, bool editable = true, Func<HemogramaViewModel, double?>? f = null)
        {
            var it = new HemogramaItem(this, det, uni, can, fel, editable, f);
            Items.Add(it);
            return it;
        }

        var erit = Add("Eritrocitos", "millones/mm3", "5,5 - 8,5", "5,0 - 10,0");
        var hemoGlob = Add("Hemoglobina", "g/dl", "12,0 - 18,0", "9,0 - 15,0");
        var hematoc = Add("Hematocrito", "%", "37-55", "27-50");
        var mcv = Add("MCV", "Fl", "60-77", "40-55");
        var mch = Add("MCH", "Pg", "17-23", "13-17");
        var mchc = Add("MCHC", "g/dl", "32-36", "30-36");
        var rdw = Add("RDW", "Cv%", "8-13,5", "11-18");

        var leuc = Add("Leucocitos", "/mm3", "6000-16000", "6000-15000");

        double? AbsPct(HemogramaViewModel vm, HemogramaItem? pctItem)
        {
            var total = vm.Find("Leucocitos")?.ValorRelativo;
            var pct = pctItem?.ValorRelativo;
            if (total is null || pct is null) return null;
            return Math.Round(total.Value * (pct.Value / 100.0));
        }

        var cay = Add("Neutrófilos Cayados", "% / mm3", "0 - 3 %/0-300", "0-3% / 0-300", f: vm => AbsPct(vm, vm.Find("Neutrófilos Cayados")));
        var seg = Add("Neutrófilos segmentados", "% / mm3", "60 -77 %/3000-11000", "60-77 %/ 2500-12000", f: vm => AbsPct(vm, vm.Find("Neutrófilos segmentados")));
        var eos = Add("Eosinófilos", "% / mm3", "1- 6 %/100-1000", "2-7%/100-1000", f: vm => AbsPct(vm, vm.Find("Eosinófilos")));
        var bas = Add("Basófilos", "% / mm3", "Hasta 1%/ < 100", "0-1% / < 100", f: vm => AbsPct(vm, vm.Find("Basófilos")));
        var lin = Add("Linfocitos", "% / mm3", "12-30% /1500-5000", "15-35%/ 1500-7000", f: vm => AbsPct(vm, vm.Find("Linfocitos")));
        var mon = Add("Monocitos", "% / mm3", "Hasta 10 %/ < 1500", "2-5% / < 1000", f: vm => AbsPct(vm, vm.Find("Monocitos")));

        var pla = Add("Plaquetas", "miles/mm3", "150-450", "250-600");
    }

    public HemogramaItem? Find(string determinacion)
        => Items.FirstOrDefault(i => string.Equals(i.Determinacion, determinacion, StringComparison.OrdinalIgnoreCase));

    internal void NotifyAbsolutosChanged()
    {
        foreach (var it in Items)
        {
            it.Raise(nameof(HemogramaItem.ValorAbsoluto));
        }
    }

    public void ClearRelativos()
    {
        foreach (var it in Items)
        {
            it.ValorRelativo = null;
        }
        Observaciones = null;
        IsConfirmed = false;
        NotifyAbsolutosChanged();
        OnPropertyChanged(nameof(Items));
    }

    public void Confirm()
    {
        IsConfirmed = true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

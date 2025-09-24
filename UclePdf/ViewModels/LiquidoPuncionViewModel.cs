using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class LiquidoPuncionItem : INotifyPropertyChanged
{
    public LiquidoPuncionItem(string determinacion)
    {
        Determinacion = determinacion;
    }

    public string Determinacion { get; }

    private string? _resultado;
    public string? Resultado
    {
        get => _resultado;
        set { if (_resultado != value) { _resultado = value; OnPropertyChanged(); } }
    }

    public void Clear() => Resultado = null;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class LiquidoPuncionViewModel : INotifyPropertyChanged
{
    public ObservableCollection<LiquidoPuncionItem> Items { get; } = new();

    private bool _isConfirmed;
    public bool IsConfirmed { get => _isConfirmed; private set { if (value != _isConfirmed) { _isConfirmed = value; OnPropertyChanged(); } } }

    private string? _observaciones;
    public string? Observaciones { get => _observaciones; set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } } }

    // Bioquímica (valores numéricos opcionales)
    private double? _urea; public double? Urea { get => _urea; set { if (_urea != value) { _urea = value; OnPropertyChanged(); } } }
    private double? _creatinina; public double? Creatinina { get => _creatinina; set { if (_creatinina != value) { _creatinina = value; OnPropertyChanged(); } } }
    private double? _fal; public double? FAL { get => _fal; set { if (_fal != value) { _fal = value; OnPropertyChanged(); } } }
    private double? _colesterol; public double? ColesterolTotal { get => _colesterol; set { if (_colesterol != value) { _colesterol = value; OnPropertyChanged(); } } }
    private double? _trigliceridos; public double? Trigliceridos { get => _trigliceridos; set { if (_trigliceridos != value) { _trigliceridos = value; OnPropertyChanged(); } } }
    private double? _bilirTotal; public double? BilirrubinaTotal { get => _bilirTotal; set { if (_bilirTotal != value) { _bilirTotal = value; OnPropertyChanged(); } } }
    private double? _bilirDirecta; public double? BilirrubinaDirecta { get => _bilirDirecta; set { if (_bilirDirecta != value) { _bilirDirecta = value; OnPropertyChanged(); } } }
    private double? _bilirIndirecta; public double? BilirrubinaIndirecta { get => _bilirIndirecta; set { if (_bilirIndirecta != value) { _bilirIndirecta = value; OnPropertyChanged(); } } }

    public LiquidoPuncionViewModel()
    {
        // Básicos
        Add("Material");
        Add("Aspecto");
        Add("Color");
        Add("Densidad");
        Add("PH");
        Add("Proteinas Totales");
        Add("Recuento Celular Total");
        Add("Recuento Diferencial");
    }

    private void Add(string det) => Items.Add(new LiquidoPuncionItem(det));

    public void Confirm()
    {
        IsConfirmed = Items.Any(i => !string.IsNullOrWhiteSpace(i.Resultado)) ||
                      Urea.HasValue || Creatinina.HasValue || FAL.HasValue || ColesterolTotal.HasValue || Trigliceridos.HasValue ||
                      BilirrubinaTotal.HasValue || BilirrubinaDirecta.HasValue || BilirrubinaIndirecta.HasValue;
    }

    public void ClearResultados()
    {
        foreach (var it in Items) it.Clear();
        Urea = Creatinina = FAL = ColesterolTotal = Trigliceridos = BilirrubinaTotal = BilirrubinaDirecta = BilirrubinaIndirecta = null;
        Observaciones = null;
        IsConfirmed = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Threading.Tasks;
using Microsoft.Win32;
using UclePdf.Core; // RelayCommand
using UclePdf.Core.Models;
using UclePdf.Core.Services;
using UclePdf.Views;

namespace UclePdf.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly IPedidosReader _pedidosReader;

    private const string NoneOption = "Sin selección";
    private const string DefaultBioquimico = "Rodríguez Méndez Rocío MP 6275";

    public MainViewModel()
        : this(new PedidosReader()) { }

    public MainViewModel(IPedidosReader pedidosReader)
    {
        _pedidosReader = pedidosReader;
        _bioquimico = DefaultBioquimico;
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    private bool _isInformeEnabled;
    public bool IsInformeEnabled
    {
        get => _isInformeEnabled;
        set => SetProperty(ref _isInformeEnabled, value);
    }

    private string? _bioquimico;
    public string? Bioquimico
    {
        get => _bioquimico;
        set => SetProperty(ref _bioquimico, value);
    }

    private Pedido? _confirmedPedido;
    public Pedido? ConfirmedPedido
    {
        get => _confirmedPedido;
        set
        {
            if (SetProperty(ref _confirmedPedido, value))
            {
                OnPropertyChanged(nameof(HeaderPaciente));
                OnPropertyChanged(nameof(HeaderLinea2));
                OnPropertyChanged(nameof(HeaderPropietario));
                OnPropertyChanged(nameof(HeaderVeterinario));
                OnPropertyChanged(nameof(HeaderSucursal));
                OnPropertyChanged(nameof(IsHemogramaLoaded));
                OnPropertyChanged(nameof(IsQuimicaLoaded));
                OnPropertyChanged(nameof(IsOrinaLoaded));
                OnPropertyChanged(nameof(IsHemostasiaLoaded));
                OnPropertyChanged(nameof(IsFrotisLoaded));
                OnPropertyChanged(nameof(IsCoproLoaded));
                OnPropertyChanged(nameof(IsEhrlichiosisLoaded));
                OnPropertyChanged(nameof(IsRaspajeLoaded));
                OnPropertyChanged(nameof(IsReticulocitosLoaded));
                OnPropertyChanged(nameof(IsProteinuriaLoaded));
                OnPropertyChanged(nameof(IsVifVilefLoaded));
                OnPropertyChanged(nameof(IsIonogramaLoaded));
                OnPropertyChanged(nameof(IsCitologicoLoaded));
                OnPropertyChanged(nameof(IsLiquidoPuncionLoaded));
            }
        }
    }

    public string HeaderPaciente => ConfirmedPedido?.NombrePaciente ?? string.Empty;
    public string HeaderLinea2
    {
        get
        {
            var parts = new[] { ConfirmedPedido?.EdadDescripcion, ConfirmedPedido?.EspecieFinal, ConfirmedPedido?.Raza, ConfirmedPedido?.Sexo }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(" / ", parts);
        }
    }
    public string HeaderPropietario => ConfirmedPedido?.Propietario ?? string.Empty;
    public string HeaderVeterinario => ConfirmedPedido?.VeterinarioSolicitante ?? string.Empty;
    public string HeaderSucursal => ConfirmedPedido?.Sucursal ?? string.Empty;

    private string? _pathA;
    public string? PathA { get => _pathA; set => SetProperty(ref _pathA, value); }

    private readonly Dictionary<Pedido, HemogramaViewModel> _hemogramas = new();
    private readonly Dictionary<Pedido, QuimicaViewModel> _quimicas = new();
    private readonly Dictionary<Pedido, OrinaViewModel> _orinas = new();
    private readonly Dictionary<Pedido, HemostasiaViewModel> _hemostasias = new();
    private readonly Dictionary<Pedido, FrotisViewModel> _frotis = new();
    private readonly Dictionary<Pedido, CoproparasitologicoViewModel> _copro = new();
    private readonly Dictionary<Pedido, EhrlichiosisViewModel> _ehrlichiosis = new();
    private readonly Dictionary<Pedido, RaspajePielViewModel> _raspajes = new();
    private readonly Dictionary<Pedido, ReticulocitosViewModel> _reticulocitos = new();
    private readonly Dictionary<Pedido, ProteinuriaCreatininuriaViewModel> _proteinuria = new();
    private readonly Dictionary<Pedido, VifVilefViewModel> _vifvilef = new();
    private readonly Dictionary<Pedido, IonogramaViewModel> _ionograma = new();
    private readonly Dictionary<Pedido, CitologicoViewModel> _citologico = new();
    private readonly Dictionary<Pedido, LiquidoPuncionViewModel> _liquidoPuncion = new();
    public bool IsHemogramaLoaded => ConfirmedPedido != null && _hemogramas.TryGetValue(ConfirmedPedido, out var hvm) && hvm.IsConfirmed && hvm.Items.Any(i => i.ValorRelativo.HasValue);
    public bool IsQuimicaLoaded => ConfirmedPedido != null && _quimicas.TryGetValue(ConfirmedPedido, out var qvm) && qvm.IsConfirmed && qvm.Items.Any(i => i.Valor.HasValue);
    public bool IsOrinaLoaded => ConfirmedPedido != null && _orinas.TryGetValue(ConfirmedPedido, out var ovm) && ovm.IsConfirmed && ovm.Items.Any(i => !string.IsNullOrWhiteSpace(i.Valor));
    public bool IsHemostasiaLoaded => ConfirmedPedido != null && _hemostasias.TryGetValue(ConfirmedPedido, out var htv) && htv.IsConfirmed && htv.Items.Any(i => i.Valor.HasValue);
    public bool IsFrotisLoaded => ConfirmedPedido != null && _frotis.TryGetValue(ConfirmedPedido, out var fr) && fr.IsConfirmed && !string.IsNullOrWhiteSpace(fr.Resultado);
    public bool IsCoproLoaded => ConfirmedPedido != null && _copro.TryGetValue(ConfirmedPedido, out var cp) && cp.IsConfirmed && cp.Items.Any(i => !string.IsNullOrWhiteSpace(i.Resultado));
    public bool IsEhrlichiosisLoaded => ConfirmedPedido != null && _ehrlichiosis.TryGetValue(ConfirmedPedido, out var eh) && eh.IsConfirmed && eh.Items.Any(i => !string.IsNullOrWhiteSpace(i.Resultado));
    public bool IsRaspajeLoaded => ConfirmedPedido != null && _raspajes.TryGetValue(ConfirmedPedido, out var rp) && rp.IsConfirmed && rp.Items.Any(i => !string.IsNullOrWhiteSpace(i.Resultado));
    public bool IsReticulocitosLoaded => ConfirmedPedido != null && _reticulocitos.TryGetValue(ConfirmedPedido, out var rt) && rt.IsConfirmed && rt.Items.Any(i => i.Valor.HasValue);
    public bool IsProteinuriaLoaded => ConfirmedPedido != null && _proteinuria.TryGetValue(ConfirmedPedido, out var pr) && pr.IsConfirmed && pr.Items.Any(i => i.Valor.HasValue);
    public bool IsVifVilefLoaded => ConfirmedPedido != null && _vifvilef.TryGetValue(ConfirmedPedido, out var vv) && vv.IsConfirmed && ((vv.VifResultado != null && vv.VifResultado != "Sin seleccion") || (vv.VilefResultado != null && vv.VilefResultado != "Sin seleccion"));
    public bool IsIonogramaLoaded => ConfirmedPedido != null && _ionograma.TryGetValue(ConfirmedPedido, out var io) && io.IsConfirmed && io.Items.Any(i => i.Valor.HasValue);
    public bool IsCitologicoLoaded => ConfirmedPedido != null && _citologico.TryGetValue(ConfirmedPedido, out var ci) && ci.IsConfirmed && ci.Items.Any(i => !string.IsNullOrWhiteSpace(i.Resultado));
    public bool IsLiquidoPuncionLoaded => ConfirmedPedido != null && _liquidoPuncion.TryGetValue(ConfirmedPedido, out var lp) && lp.IsConfirmed && lp.Items.Any(i => !string.IsNullOrWhiteSpace(i.Resultado));

    public IReadOnlyList<string> SucursalesOpciones { get; } = new[]
    {
        "Todas",
        "UCLE 1 (Central)",
        "UCLE 2 (Fuerza Aerea)",
        "UCLE 3 (CPC)",
        "UCLE 5 (Malagueño)",
        "UCLE 8 (Pueyrredón)",
        "UCLE 9 (Tropezón)",
        "UCLE 10 (Falda)"
    };

    public IReadOnlyList<string> EspecieFiltroOpciones { get; } = new[] { "Todas", "Canino", "Felino" };

    private string _filterSucursal = "Todas";
    public string FilterSucursal
    {
        get => _filterSucursal;
        set { if (SetProperty(ref _filterSucursal, value)) ApplyFilter(); }
    }

    private string _filterEspecie = "Todas";
    public string FilterEspecie
    {
        get => _filterEspecie;
        set { if (SetProperty(ref _filterEspecie, value)) ApplyFilter(); }
    }

    private string? _filterVeterinario;
    public string? FilterVeterinario
    {
        get => _filterVeterinario;
        set { if (SetProperty(ref _filterVeterinario, value)) ApplyFilter(); }
    }

    private string? _filterPropietario;
    public string? FilterPropietario
    {
        get => _filterPropietario;
        set { if (SetProperty(ref _filterPropietario, value)) ApplyFilter(); }
    }

    private string? _filterPaciente;
    public string? FilterPaciente
    {
        get => _filterPaciente;
        set { if (SetProperty(ref _filterPaciente, value)) ApplyFilter(); }
    }

    private Pedido? _selectedPedido;
    public Pedido? SelectedPedido
    {
        get => _selectedPedido;
        set => SetProperty(ref _selectedPedido, value);
    }

    public ObservableCollection<Pedido> Pedidos { get; } = new();
    private List<Pedido> _allPedidos = new();

    private DateTime? _filterFromDate;
    public DateTime? FilterFromDate
    {
        get => _filterFromDate;
        set { if (SetProperty(ref _filterFromDate, value)) ApplyFilter(); }
    }

    public ICommand BrowsePedidosCommand => new RelayCommand(_ => PathA = BrowseExcel());
    public ICommand LoadPedidosCommand => new RelayCommand(async _ => await LoadPedidosAsync(), _ => !string.IsNullOrWhiteSpace(PathA) && !IsBusy);

    public ICommand ClearAllCommand => new RelayCommand(_ => ClearAll(), _ => Pedidos.Count > 0 && !IsBusy);
    public ICommand ConfirmSelectionCommand => new RelayCommand(_ => ConfirmSelection(), _ => !IsBusy);
    public ICommand OpenHemogramaCommand => new RelayCommand(_ => OpenHemograma(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenQuimicaCommand => new RelayCommand(_ => OpenQuimica(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenOrinaCommand => new RelayCommand(_ => OpenOrina(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenHemostasiaCommand => new RelayCommand(_ => OpenHemostasia(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenFrotisCommand => new RelayCommand(_ => OpenFrotis(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenCoproCommand => new RelayCommand(_ => OpenCopro(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenEhrlichiosisCommand => new RelayCommand(_ => OpenEhrlichiosis(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenRaspajeCommand => new RelayCommand(_ => OpenRaspaje(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenReticulocitosCommand => new RelayCommand(_ => OpenReticulocitos(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenProteinuriaCommand => new RelayCommand(_ => OpenProteinuria(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenVifVilefCommand => new RelayCommand(_ => OpenVifVilef(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenIonogramaCommand => new RelayCommand(_ => OpenIonograma(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenCitologicoCommand => new RelayCommand(_ => OpenCitologico(), _ => IsInformeEnabled && ConfirmedPedido != null);
    public ICommand OpenLiquidoPuncionCommand => new RelayCommand(_ => OpenLiquidoPuncion(), _ => IsInformeEnabled && ConfirmedPedido != null);

    private static string? BrowseExcel()
    {
        var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xlsm;*.xls|All Files|*.*" };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    private async Task LoadPedidosAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PathA)) return;
            IsBusy = true;
            var items = await Task.Run(() => _pedidosReader.Leer(PathA!, FilterFromDate));
            _allPedidos = items;
            ApplyFilter();
            MessageBox.Show($"Pedidos cargados: {_allPedidos.Count}", "UCLE", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando Pedidos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsBusy = false; }
    }

    private void ApplyFilter()
    {
        IEnumerable<Pedido> src = _allPedidos;

        if (!string.IsNullOrWhiteSpace(FilterSucursal) && FilterSucursal != "Todas")
            src = src.Where(p => string.Equals(p.Sucursal ?? string.Empty, FilterSucursal, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterEspecie) && FilterEspecie != "Todas")
            src = src.Where(p => string.Equals((p.Especie ?? p.EspecieFinal ?? string.Empty), FilterEspecie, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterVeterinario))
            src = src.Where(p => (p.VeterinarioSolicitante ?? string.Empty).Contains(FilterVeterinario, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterPropietario))
            src = src.Where(p => (p.Propietario ?? string.Empty).Contains(FilterPropietario, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterPaciente))
            src = src.Where(p => (p.NombrePaciente ?? string.Empty).Contains(FilterPaciente, StringComparison.OrdinalIgnoreCase));

        src = src.OrderByDescending(p => p.MarcaTemporal ?? DateTime.MinValue);

        Pedidos.Clear();
        foreach (var p in src)
            Pedidos.Add(p);
    }

    private void ConfirmSelection()
    {
        if (SelectedPedido is null)
        {
            MessageBox.Show("Primero seleccione un registro", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ConfirmedPedido = SelectedPedido;
        IsInformeEnabled = true;
        SelectedTabIndex = 1;
    }

    private void OpenHemograma()
    {
        if (ConfirmedPedido is null) return;
        if (!_hemogramas.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new HemogramaViewModel();
        }
        var win = new HemogramaWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _hemogramas[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsHemogramaLoaded));
    }

    private void OpenQuimica()
    {
        if (ConfirmedPedido is null) return;
        if (!_quimicas.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new QuimicaViewModel();
        }
        var win = new QuimicaWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _quimicas[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsQuimicaLoaded));
    }

    private void OpenOrina()
    {
        if (ConfirmedPedido is null) return;
        if (!_orinas.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new OrinaViewModel();
        }
        var win = new OrinaWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _orinas[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsOrinaLoaded));
    }

    private void OpenHemostasia()
    {
        if (ConfirmedPedido is null) return;
        if (!_hemostasias.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new HemostasiaViewModel();
        }
        var win = new HemostasiaWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _hemostasias[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsHemostasiaLoaded));
    }

    private void OpenFrotis()
    {
        if (ConfirmedPedido is null) return;
        if (!_frotis.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new FrotisViewModel();
        }
        var win = new FrotisWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _frotis[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsFrotisLoaded));
    }

    private void OpenCopro()
    {
        if (ConfirmedPedido is null) return;
        if (!_copro.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new CoproparasitologicoViewModel();
        }
        var win = new CoproparasitologicoWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _copro[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsCoproLoaded));
    }

    private void OpenEhrlichiosis()
    {
        if (ConfirmedPedido is null) return;
        if (!_ehrlichiosis.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new EhrlichiosisViewModel();
        }
        var win = new EhrlichiosisWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _ehrlichiosis[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsEhrlichiosisLoaded));
    }

    private void OpenRaspaje()
    {
        if (ConfirmedPedido is null) return;
        if (!_raspajes.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new RaspajePielViewModel();
        }
        var win = new RaspajePielWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _raspajes[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsRaspajeLoaded));
    }

    private void OpenReticulocitos()
    {
        if (ConfirmedPedido is null) return;
        if (!_reticulocitos.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new ReticulocitosViewModel();
        }
        var win = new ReticulocitosWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _reticulocitos[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsReticulocitosLoaded));
    }

    private void OpenProteinuria()
    {
        if (ConfirmedPedido is null) return;
        if (!_proteinuria.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new ProteinuriaCreatininuriaViewModel();
        }
        var win = new ProteinuriaCreatininuriaWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _proteinuria[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsProteinuriaLoaded));
    }

    private void OpenVifVilef()
    {
        if (ConfirmedPedido is null) return;
        if (!_vifvilef.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new VifVilefViewModel();
        }
        var win = new VifVilefWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _vifvilef[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsVifVilefLoaded));
    }

    private void OpenIonograma()
    {
        if (ConfirmedPedido is null) return;
        if (!_ionograma.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new IonogramaViewModel();
        }
        var win = new IonogramaWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _ionograma[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsIonogramaLoaded));
    }

    private void OpenCitologico()
    {
        if (ConfirmedPedido is null) return;
        if (!_citologico.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new CitologicoViewModel();
        }
        var win = new CitologicoWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _citologico[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsCitologicoLoaded));
    }

    private void OpenLiquidoPuncion()
    {
        if (ConfirmedPedido is null) return;
        if (!_liquidoPuncion.TryGetValue(ConfirmedPedido, out var vm))
        {
            vm = new LiquidoPuncionViewModel();
        }
        var win = new LiquidoPuncionWindow(vm) { Owner = Application.Current?.MainWindow };
        var result = win.ShowDialog();
        if (result == true)
        {
            _liquidoPuncion[ConfirmedPedido] = vm;
        }
        OnPropertyChanged(nameof(IsLiquidoPuncionLoaded));
    }

    private void ClearAll()
    {
        var res = MessageBox.Show("¿Está seguro de limpiar todo? Esta acción no se puede deshacer.", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res != MessageBoxResult.Yes) return;

        Pedidos.Clear();
        _allPedidos.Clear();
        _hemogramas.Clear();
        _quimicas.Clear();
        _orinas.Clear();
        _hemostasias.Clear();
        _frotis.Clear();
        _copro.Clear();
        _ehrlichiosis.Clear();
        _raspajes.Clear();
        _reticulocitos.Clear();
        _proteinuria.Clear();
        _vifvilef.Clear();
        _ionograma.Clear();
        _citologico.Clear();
        _liquidoPuncion.Clear();
        SelectedPedido = null;
        ConfirmedPedido = null;
        IsInformeEnabled = false;
        SelectedTabIndex = 0;
        PathA = null;
        FilterFromDate = null;
        FilterSucursal = "Todas";
        FilterEspecie = "Todas";
        FilterVeterinario = null;
        FilterPropietario = null;
        FilterPaciente = null;
        Bioquimico = DefaultBioquimico;
    }
}

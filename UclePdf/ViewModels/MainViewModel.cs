using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using UclePdf.Core;
using UclePdf.Core.Models;
using UclePdf.Core.Services;
using UclePdf.Views;
using UclePdf.Services;

namespace UclePdf.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly IPedidosReader _pedidosReader;
    private readonly IPdfReportService _pdfService = new PdfReportService();

    private const string NoneOption = "Sin selección"; // (no usado aquí pero mantenido)
    private const string BioquimicoPlaceholder = "Seleccione";
    private const string DefaultBioquimico = "Rodríguez Méndez Rocío MP 6275";
    private const string BioquimicoAlt = "Magallanes Aldana MP 6207";

    // Opciones (incluye placeholder primero)
    public IReadOnlyList<string> BioquimicosOpciones { get; } = new[]
    {
        BioquimicoPlaceholder,
        DefaultBioquimico,
        BioquimicoAlt
    };

    public MainViewModel()
        : this(new PedidosReader()) { }

    public MainViewModel(IPedidosReader pedidosReader)
    {
        _pedidosReader = pedidosReader;
        _bioquimico = BioquimicoPlaceholder; // por defecto "Seleccione"
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
                OnPropertyChanged(nameof(HeaderFecha));
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
    public string HeaderFecha => ConfirmedPedido?.MarcaTemporal?.ToString("dd/MM/yyyy") ?? string.Empty;

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
    public bool IsLiquidoPuncionLoaded => ConfirmedPedido != null && _liquidoPuncion.TryGetValue(ConfirmedPedido, out var lp) && lp.IsConfirmed;

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
    public ICommand GenerateInformeCommand => new RelayCommand(_ => GenerateInforme(), _ => IsInformeEnabled && ConfirmedPedido != null && !IsBusy);

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
            if (_allPedidos.Count == 0)
            {
                MessageBox.Show("No se encontraron resultados.", "UCLE", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

        // Limite por defecto: si no hay fecha seleccionada se muestran solo los primeros 50 (vista inicial mas liviana)
        if (FilterFromDate == null)
            src = src.Take(50);

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

    private void GenerateInforme()
    {
        // Validación selección de bioquímico
        if (Bioquimico == BioquimicoPlaceholder)
        {
            MessageBox.Show("Debe seleccionar un bioquímico antes de generar el informe.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            byte[]? logoBytes = LoadEmbedded("UclePdf.Assets.logo_ucle.png");

            // Firma dinámica según bioquímico seleccionado
            var signatureResource = Bioquimico == BioquimicoAlt
                ? "UclePdf.Assets.signature_aldana.png"
                : "UclePdf.Assets.signature_rocio.png";
            byte[]? signatureBytes = LoadEmbedded(signatureResource);

            HemogramaData? hemoData = null;
            if (IsHemogramaLoaded && _hemogramas.TryGetValue(ConfirmedPedido!, out var hvm))
            {
                var rows = hvm.Items
                    .Where(i => i.ValorRelativo.HasValue || i.ValorAbsoluto.HasValue)
                    .Select(i => new HemogramaRow(
                        i.Determinacion,
                        i.ValorRelativo,
                        i.ValorAbsoluto,
                        i.Unidades,
                        i.RefCaninos,
                        i.RefFelinos))
                    .ToList();
                if (rows.Count > 0)
                    hemoData = new HemogramaData(rows, hvm.Observaciones, HeaderLinea2); // HeaderLinea2 contiene especie entre otros datos
            }

            QuimicaData? quimicaData = null;
            if (IsQuimicaLoaded && _quimicas.TryGetValue(ConfirmedPedido!, out var qvm))
            {
                // Obtener el valor de triglicéridos
                var trigVal = qvm.Items.FirstOrDefault(i => i.Determinacion.ToLower().Contains("trigliceridos"))?.Valor;
                var rowsQ = qvm.Items.Where(i => i.Valor.HasValue)
                    .Where(i => !(trigVal.HasValue && trigVal.Value >= 400 && i.Determinacion.ToLower().Contains("colesterol ldl")))
                    .Select(i => new QuimicaRow(
                        i.Determinacion,
                        i.Valor,
                        i.Unidades,
                        i.RefCaninos,
                        i.RefFelinos)).ToList();
                if (rowsQ.Count > 0)
                    quimicaData = new QuimicaData(rowsQ, qvm.Observaciones, HeaderLinea2);
            }

            OrinaData? orinaData = null;
            if (IsOrinaLoaded && _orinas.TryGetValue(ConfirmedPedido!, out var orvm))
            {
                var rowsO = orvm.Items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Valor))
                    .Select(i => new OrinaRow(i.Seccion, i.Determinacion, i.Valor, i.RefCaninos, i.RefFelinos, i.TopSeparator))
                    .ToList();
                if (rowsO.Count > 0)
                    orinaData = new OrinaData(rowsO, orvm.Observaciones, HeaderLinea2);
            }

            HemostasiaData? hemostasiaData = null;
            if (IsHemostasiaLoaded && _hemostasias.TryGetValue(ConfirmedPedido!, out var hstv))
            {
                var rowsH = hstv.Items.Where(i => i.Valor.HasValue)
                    .Select(i => new HemostasiaRow(i.Determinacion, i.Valor, i.Referencia))
                    .ToList();
                if (rowsH.Count > 0)
                    hemostasiaData = new HemostasiaData(rowsH, hstv.Observaciones);
            }

            FrotisData? frotisData = null;
            if (IsFrotisLoaded && _frotis.TryGetValue(ConfirmedPedido!, out var fvm) && !string.IsNullOrWhiteSpace(fvm.Resultado))
            {
                frotisData = new FrotisData(fvm.Resultado);
            }

            CoproData? coproData = null;
            if (IsCoproLoaded && _copro.TryGetValue(ConfirmedPedido!, out var cpvm))
            {
                var rowsC = cpvm.Items.Where(i => !string.IsNullOrWhiteSpace(i.Resultado))
                    .Select(i => new CoproRow(i.Determinacion, i.Resultado)).ToList();
                if (rowsC.Count > 0)
                    coproData = new CoproData(rowsC, cpvm.Observaciones);
            }

            EhrlichiosisData? ehrlichiosisData = null;
            if (IsEhrlichiosisLoaded && _ehrlichiosis.TryGetValue(ConfirmedPedido!, out var ehvm))
            {
                var rowsE = ehvm.Items.Where(i => !string.IsNullOrWhiteSpace(i.Resultado) && i.Resultado != "Sin seleccion")
                    .Select(i => new EhrlichiosisRow(i.Tecnica, i.Resultado))
                    .ToList();
                if (rowsE.Count > 0)
                    ehrlichiosisData = new EhrlichiosisData(rowsE, ehvm.Observaciones);
            }

            RaspajeData? raspajeData = null;
            if (IsRaspajeLoaded && _raspajes.TryGetValue(ConfirmedPedido!, out var rspvm))
            {
                var rowsR = rspvm.Items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Resultado) && !string.Equals(i.Determinacion, "Determinacion", StringComparison.OrdinalIgnoreCase))
                    .Select(i => new RaspajeRow(i.Determinacion, i.Resultado))
                    .ToList();
                if (rowsR.Count > 0)
                    raspajeData = new RaspajeData(rowsR, rspvm.Observaciones);
            }

            ReticulocitosData? reticulocitosData = null;
            if (IsReticulocitosLoaded && _reticulocitos.TryGetValue(ConfirmedPedido!, out var rtvm))
            {
                var rowsReti = rtvm.Items.Where(i => i.Valor.HasValue)
                    .Select(i => new ReticulocitosRow(i.Determinacion, i.Valor, i.Referencia))
                    .ToList();
                if (rowsReti.Count > 0)
                    reticulocitosData = new ReticulocitosData(rowsReti, rtvm.Observaciones);
            }

            ProteinuriaData? proteinuriaData = null;
            if (IsProteinuriaLoaded && _proteinuria.TryGetValue(ConfirmedPedido!, out var pvm))
            {
                var rowsP = pvm.Items.Where(i => i.Valor.HasValue)
                    .Select(i => new ProteinuriaRow(i.Determinacion, i.Valor, i.Referencia))
                    .ToList();
                if (rowsP.Count > 0)
                    proteinuriaData = new ProteinuriaData(rowsP, pvm.Observaciones, pvm.ReferenciasBloque);
            }

            VifVilefData? vifvilefData = null;
            if (IsVifVilefLoaded && _vifvilef.TryGetValue(ConfirmedPedido!, out var vvvm))
            {
                if ((vvvm.VifResultado != null && vvvm.VifResultado != "Sin seleccion") || (vvvm.VilefResultado != null && vvvm.VilefResultado != "Sin seleccion"))
                    vifvilefData = new VifVilefData(vvvm.VifResultado, vvvm.VilefResultado, vvvm.Observaciones);
            }

            IonogramaData? ionogramaData = null;
            if (IsIonogramaLoaded && _ionograma.TryGetValue(ConfirmedPedido!, out var ionvm))
            {
                var rowsI = ionvm.Items.Where(i => i.Valor.HasValue)
                    .Select(i => new IonogramaRow(i.Determinacion, i.Valor, i.RefCanino, i.RefFelino))
                    .ToList();
                if (rowsI.Count > 0)
                    ionogramaData = new IonogramaData(rowsI, ionvm.Observaciones, HeaderLinea2);
            }

            CitologicoData? citologicoData = null;
            if (IsCitologicoLoaded && _citologico.TryGetValue(ConfirmedPedido!, out var civm))
            {
                var rowsCi = civm.Items.Where(i => !string.IsNullOrWhiteSpace(i.Resultado))
                    .Select(i => new CitologicoRow(i.Determinacion, i.Resultado))
                    .ToList();
                if (rowsCi.Count > 0)
                    citologicoData = new CitologicoData(rowsCi, civm.Observaciones);
            }

            LiquidoPuncionData? liquidoPuncionData = null;
            if (IsLiquidoPuncionLoaded && _liquidoPuncion.TryGetValue(ConfirmedPedido!, out var lpvm))
            {
                var textoRows = lpvm.Items.Where(i => !string.IsNullOrWhiteSpace(i.Resultado))
                    .Select(i => new LiquidoPuncionTextoRow(i.Determinacion, i.Resultado))
                    .ToList();
                var bioqRows = new List<LiquidoPuncionBioqRow>();
                void AddBioq(string det, double? val, string unidades)
                { if (val.HasValue) bioqRows.Add(new LiquidoPuncionBioqRow(det, val, unidades)); }
                AddBioq("Urea", lpvm.Urea, "mg/dL");
                AddBioq("Creatinina", lpvm.Creatinina, "mg/dL");
                AddBioq("FAL", lpvm.FAL, "UI/L");
                AddBioq("Colesterol Total", lpvm.ColesterolTotal, "mg/dL");
                AddBioq("Triglicéridos", lpvm.Trigliceridos, "mg/dL");
                AddBioq("Bilirrubina Total", lpvm.BilirrubinaTotal, "mg/dL");
                AddBioq("Bilirrubina Directa", lpvm.BilirrubinaDirecta, "mg/dL");
                AddBioq("Bilirrubina Indirecta", lpvm.BilirrubinaIndirecta, "mg/dL");
                if (textoRows.Count > 0 || bioqRows.Count > 0)
                    liquidoPuncionData = new LiquidoPuncionData(textoRows, bioqRows, lpvm.Observaciones);
            }

            var bioq = Bioquimico == BioquimicoPlaceholder ? string.Empty : (Bioquimico ?? string.Empty);
            var pdfBytes = _pdfService.GenerateInformePdfBytes(logoBytes, new InformeHeaderData(
                HeaderFecha,
                HeaderPaciente,
                HeaderLinea2,
                HeaderPropietario,
                HeaderVeterinario,
                HeaderSucursal,
                bioq), hemoData, quimicaData, orinaData, hemostasiaData, frotisData, coproData, ehrlichiosisData, raspajeData, reticulocitosData, proteinuriaData, vifvilefData, ionogramaData, citologicoData, liquidoPuncionData, signatureBytes);
            var preview = new UclePdf.Views.PreviewPdfWindow(pdfBytes) { Owner = Application.Current?.MainWindow };
            preview.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generando PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static byte[]? LoadEmbedded(string resourceName)
    {
        try
        {
            var asm = typeof(MainViewModel).Assembly;
            using var s = asm.GetManifestResourceStream(resourceName);
            if (s == null) return null;
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
        catch { return null; }
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
        Bioquimico = BioquimicoPlaceholder;
    }
}

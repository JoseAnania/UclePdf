using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using UclePdf.Core;
using UclePdf.Core.Models;
using UclePdf.Core.Services;

namespace UclePdf.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly IPedidosReader _pedidosReader;

    private const string NoneOption = "Sin selección";

    public MainViewModel()
        : this(new PedidosReader()) { }

    public MainViewModel(IPedidosReader pedidosReader)
    {
        _pedidosReader = pedidosReader;
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

    private string? _pathA;
    public string? PathA { get => _pathA; set => SetProperty(ref _pathA, value); }

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

        // Filtros
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

        // Orden
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
        MessageBox.Show("Selección confirmada", "UCLE", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ClearAll()
    {
        var res = MessageBox.Show("¿Está seguro de limpiar todo? Esta acción no se puede deshacer.", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res != MessageBoxResult.Yes) return;

        Pedidos.Clear();
        _allPedidos.Clear();
        SelectedPedido = null;
        PathA = null;
        FilterFromDate = null;
        FilterSucursal = "Todas";
        FilterEspecie = "Todas";
        FilterVeterinario = null;
        FilterPropietario = null;
        FilterPaciente = null;
    }
}

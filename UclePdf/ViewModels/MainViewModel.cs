using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using Microsoft.Win32;
using UclePdf.Core;
using UclePdf.Core.Models;
using UclePdf.Core.Services;

namespace UclePdf.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly IPedidosReader _pedidosReader;

    private const string NoneOption = "Sin selección";
    private bool _suppressUi;

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

    public IReadOnlyList<string> EspeciesOpciones { get; } = new[] { NoneOption, "Felino", "Canino", "Otros" };
    public IReadOnlyList<string> SexosOpciones { get; } = new[] { NoneOption, "Hembra", "Macho" };
    public IReadOnlyList<string> EdadUnidadesOpciones { get; } = new[] { NoneOption, "Años", "Meses" };

    // Selección actual en la grilla
    private Pedido? _selectedPedido;
    public Pedido? SelectedPedido
    {
        get => _selectedPedido;
        set
        {
            if (SetProperty(ref _selectedPedido, value))
            {
                _suppressUi = true;
                try
                {
                    TempEspecie = value?.Especie ?? NoneOption;
                    TempEspecieOtro = value?.EspecieOtro;
                    TempSexo = value?.Sexo ?? NoneOption;
                    TempRaza = value?.Raza;
                    TempEdadCantidad = value?.EdadCantidad?.ToString();
                    TempEdadUnidad = value?.EdadUnidad ?? NoneOption;
                }
                finally { _suppressUi = false; }
            }
        }
    }

    // Valores temporales de edición
    private string? _tempEspecie;
    public string? TempEspecie
    {
        get => _tempEspecie;
        set
        {
            if (SetProperty(ref _tempEspecie, value))
            {
                if (SelectedPedido is null)
                {
                    if (_suppressUi) return;
                    if (!string.IsNullOrEmpty(value))
                        MessageBox.Show("Primero seleccione un registro", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string? especie = value;
                if (string.IsNullOrWhiteSpace(especie) || especie == NoneOption)
                    especie = null;

                SelectedPedido.Especie = especie;
                if (especie != "Otros")
                {
                    TempEspecieOtro = null;
                    SelectedPedido.EspecieOtro = null;
                }
                RefreshSelectedRow();
            }
        }
    }

    private string? _tempEspecieOtro;
    public string? TempEspecieOtro
    {
        get => _tempEspecieOtro;
        set
        {
            if (SetProperty(ref _tempEspecieOtro, value))
            {
                if (SelectedPedido is null)
                {
                    if (_suppressUi) return;
                    if (!string.IsNullOrEmpty(value))
                        MessageBox.Show("Primero seleccione un registro", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (SelectedPedido.Especie != "Otros")
                {
                    MessageBox.Show("Para cargar 'Otros', primero seleccione ESPECIE = 'Otros'", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                SelectedPedido.EspecieOtro = value;
                RefreshSelectedRow();
            }
        }
    }

    private string? _tempSexo;
    public string? TempSexo
    {
        get => _tempSexo;
        set
        {
            if (SetProperty(ref _tempSexo, value))
            {
                if (SelectedPedido is null)
                {
                    if (_suppressUi) return;
                    if (!string.IsNullOrEmpty(value))
                        MessageBox.Show("Primero seleccione un registro", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string? sexo = value;
                if (string.IsNullOrWhiteSpace(sexo) || sexo == NoneOption)
                    sexo = null;
                SelectedPedido.Sexo = sexo;
                RefreshSelectedRow();
            }
        }
    }

    private string? _tempRaza;
    public string? TempRaza
    {
        get => _tempRaza;
        set
        {
            if (SetProperty(ref _tempRaza, value))
            {
                if (SelectedPedido is null)
                {
                    if (_suppressUi) return;
                    if (!string.IsNullOrEmpty(value))
                        MessageBox.Show("Primero seleccione un registro", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                SelectedPedido.Raza = value;
                RefreshSelectedRow();
            }
        }
    }

    private string? _tempEdadCantidad;
    public string? TempEdadCantidad
    {
        get => _tempEdadCantidad;
        set
        {
            if (SetProperty(ref _tempEdadCantidad, value))
            {
                if (SelectedPedido is null)
                {
                    if (_suppressUi) return;
                    if (!string.IsNullOrEmpty(value))
                        MessageBox.Show("Primero seleccione un registro", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (int.TryParse(value, out var n) && n >= 0)
                    SelectedPedido.EdadCantidad = n;
                else if (string.IsNullOrWhiteSpace(value))
                    SelectedPedido.EdadCantidad = null;
                RefreshSelectedRow();
            }
        }
    }

    private string? _tempEdadUnidad;
    public string? TempEdadUnidad
    {
        get => _tempEdadUnidad;
        set
        {
            if (SetProperty(ref _tempEdadUnidad, value))
            {
                if (SelectedPedido is null)
                {
                    if (_suppressUi) return;
                    if (!string.IsNullOrEmpty(value))
                        MessageBox.Show("Primero seleccione un registro", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string? unidad = value;
                if (string.IsNullOrWhiteSpace(unidad) || unidad == NoneOption)
                    unidad = null;
                SelectedPedido.EdadUnidad = unidad;
                RefreshSelectedRow();
            }
        }
    }

    public ObservableCollection<Pedido> Pedidos { get; } = new();
    private List<Pedido> _allPedidos = new();

    private DateTime? _filterFromDate;
    public DateTime? FilterFromDate
    {
        get => _filterFromDate;
        set => SetProperty(ref _filterFromDate, value);
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
        SelectedPedido.Especie = TempEspecie == NoneOption ? null : TempEspecie;
        if (SelectedPedido.Especie != "Otros")
            SelectedPedido.EspecieOtro = null;
        else
            SelectedPedido.EspecieOtro = TempEspecieOtro;
        SelectedPedido.Sexo = TempSexo == NoneOption ? null : TempSexo;
        SelectedPedido.Raza = TempRaza;
        if (int.TryParse(TempEdadCantidad, out var n) && n >= 0)
            SelectedPedido.EdadCantidad = n;
        else if (string.IsNullOrWhiteSpace(TempEdadCantidad))
            SelectedPedido.EdadCantidad = null;
        SelectedPedido.EdadUnidad = TempEdadUnidad == NoneOption ? null : TempEdadUnidad;
        RefreshSelectedRow();
        MessageBox.Show("Selección confirmada", "UCLE", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ClearAll()
    {
        var res = MessageBox.Show("¿Está seguro de limpiar todo? Esta acción no se puede deshacer.", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res != MessageBoxResult.Yes) return;

        _suppressUi = true;
        try
        {
            Pedidos.Clear();
            _allPedidos.Clear();
            SelectedPedido = null;
            PathA = null;
            FilterFromDate = null;
            TempEspecie = null;
            TempEspecieOtro = null;
            TempSexo = null;
            TempRaza = null;
            TempEdadCantidad = null;
            TempEdadUnidad = null;
        }
        finally { _suppressUi = false; }
    }

    private void RefreshSelectedRow()
    {
        if (SelectedPedido is null) return;
        var view = CollectionViewSource.GetDefaultView(Pedidos);
        view?.Refresh();
    }
}

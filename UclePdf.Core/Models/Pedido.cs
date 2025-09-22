using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.Core.Models;

public class Pedido : INotifyPropertyChanged
{
    private DateTime? _marcaTemporal;
    private string? _correoElectronico;
    private string? _sucursal;
    private string? _veterinarioSolicitante;
    private string? _propietario;
    private string? _nombrePaciente;

    private string? _especie;
    private string? _especieOtro;
    private string? _sexo;

    private string? _raza;
    private int? _edadCantidad;
    private string? _edadUnidad;

    public DateTime? MarcaTemporal { get => _marcaTemporal; set => SetProperty(ref _marcaTemporal, value); }
    public string? CorreoElectronico { get => _correoElectronico; set => SetProperty(ref _correoElectronico, value); }
    public string? Sucursal { get => _sucursal; set => SetProperty(ref _sucursal, value); }
    public string? VeterinarioSolicitante { get => _veterinarioSolicitante; set => SetProperty(ref _veterinarioSolicitante, value); }
    public string? Propietario { get => _propietario; set => SetProperty(ref _propietario, value); }
    public string? NombrePaciente { get => _nombrePaciente; set => SetProperty(ref _nombrePaciente, value); }

    public string? Especie
    {
        get => _especie;
        set
        {
            if (SetProperty(ref _especie, value))
            {
                OnPropertyChanged(nameof(EspecieFinal));
            }
        }
    }

    public string? EspecieOtro
    {
        get => _especieOtro;
        set
        {
            if (SetProperty(ref _especieOtro, value))
            {
                OnPropertyChanged(nameof(EspecieFinal));
            }
        }
    }

    public string? Sexo { get => _sexo; set => SetProperty(ref _sexo, value); }

    public string? Raza { get => _raza; set => SetProperty(ref _raza, value); }
    public int? EdadCantidad { get => _edadCantidad; set { if (SetProperty(ref _edadCantidad, value)) OnPropertyChanged(nameof(EdadDescripcion)); } }
    public string? EdadUnidad { get => _edadUnidad; set { if (SetProperty(ref _edadUnidad, value)) OnPropertyChanged(nameof(EdadDescripcion)); } }

    public string? EspecieFinal => Especie == "Otros" ? (string.IsNullOrWhiteSpace(EspecieOtro) ? "Otros" : EspecieOtro) : Especie;

    public string? EdadDescripcion
        => EdadCantidad is int n && n >= 0 && !string.IsNullOrWhiteSpace(EdadUnidad)
            ? $"{n} {EdadUnidad}"
            : null;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value!;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

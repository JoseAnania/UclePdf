using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UclePdf.ViewModels;

public class FrotisViewModel : INotifyPropertyChanged
{
    private const string TextoPorDefecto = "No se observan hemopatógenos en la muestra remitida. No significa ausencia de infección.";

    private string? _resultado;
    public string? Resultado
    {
        get => _resultado;
        set { if (_resultado != value) { _resultado = value; OnPropertyChanged(); } }
    }

    private bool _isConfirmed;
    public bool IsConfirmed
    {
        get => _isConfirmed;
        private set { if (_isConfirmed != value) { _isConfirmed = value; OnPropertyChanged(); } }
    }

    public FrotisViewModel()
    {
        ResetIfEmpty();
    }

    public void ResetIfEmpty()
    {
        if (string.IsNullOrWhiteSpace(Resultado))
            Resultado = TextoPorDefecto;
    }

    public void ClearResultado()
    {
        Resultado = null;
        IsConfirmed = false;
    }

    public void Confirm()
    {
        // Si está vacío al confirmar, lo consideramos no cargado (IsConfirmed false)
        if (string.IsNullOrWhiteSpace(Resultado))
        {
            IsConfirmed = false;
            return;
        }
        IsConfirmed = true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

using System.Windows;
using UclePdf.ViewModels;

namespace UclePdf.Views;

public partial class LiquidoPuncionWindow : Window
{
    public LiquidoPuncionWindow() : this(new LiquidoPuncionViewModel()) { }

    public LiquidoPuncionWindow(LiquidoPuncionViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is LiquidoPuncionViewModel vm)
        {
            vm.Confirm();
        }
        DialogResult = true;
        Close();
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is LiquidoPuncionViewModel vm)
        {
            vm.ClearResultados();
        }
    }
}

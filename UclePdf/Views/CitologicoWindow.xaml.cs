using System.Windows;
using UclePdf.ViewModels;

namespace UclePdf.Views;

public partial class CitologicoWindow : Window
{
    public CitologicoWindow() : this(new CitologicoViewModel()) { }

    public CitologicoWindow(CitologicoViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        vm.ResetDefaults();
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is CitologicoViewModel vm)
        {
            vm.Confirm();
        }
        DialogResult = true;
        Close();
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is CitologicoViewModel vm)
        {
            vm.ClearResultados();
        }
    }
}

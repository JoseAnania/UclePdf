using System.Windows;
using UclePdf.ViewModels;

namespace UclePdf.Views;

public partial class VifVilefWindow : Window
{
    public VifVilefWindow() : this(new VifVilefViewModel()) { }

    public VifVilefWindow(VifVilefViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is VifVilefViewModel vm)
        {
            vm.Confirm();
        }
        DialogResult = true;
        Close();
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is VifVilefViewModel vm)
        {
            vm.Clear();
        }
    }
}

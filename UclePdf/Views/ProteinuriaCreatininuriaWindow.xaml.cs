using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UclePdf.ViewModels;

namespace UclePdf.Views;

public partial class ProteinuriaCreatininuriaWindow : Window
{
    private static readonly Regex NumericRegex = new Regex("^[0-9.,]+$", RegexOptions.Compiled);

    public ProteinuriaCreatininuriaWindow() : this(new ProteinuriaCreatininuriaViewModel()) { }

    public ProteinuriaCreatininuriaWindow(ProteinuriaCreatininuriaViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        DataObject.AddPastingHandler(this, OnPasteNumeric);
    }

    private void OnPreviewTextInputNumeric(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !NumericRegex.IsMatch(e.Text);
    }

    private void OnPreviewKeyDownNumeric(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Enter)
            return;
    }

    private void OnPasteNumeric(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            var text = (string)e.DataObject.GetData(DataFormats.Text);
            if (!NumericRegex.IsMatch(text))
                e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProteinuriaCreatininuriaViewModel vm)
        {
            vm.Confirm();
        }
        DialogResult = true;
        Close();
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProteinuriaCreatininuriaViewModel vm)
        {
            vm.ClearValores();
        }
    }
}

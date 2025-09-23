using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UclePdf.ViewModels;

namespace UclePdf.Views
{
    public partial class OrinaWindow : Window
    {
        private static readonly Regex NumericRegex = new Regex("^[0-9.,]+$", RegexOptions.Compiled);

        public OrinaWindow() : this(new OrinaViewModel()) { }

        public OrinaWindow(OrinaViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DataObject.AddPastingHandler(this, OnPasteNumeric);
        }

        private bool ShouldRestrict(object? dataContext)
        {
            if (dataContext is OrinaItem item)
            {
                return item.IsNumeric; // Densidad y Ph
            }
            return false;
        }

        private void OnPreviewTextInputNumeric(object sender, TextCompositionEventArgs e)
        {
            if (!ShouldRestrict((sender as FrameworkElement)?.DataContext)) return;
            e.Handled = !NumericRegex.IsMatch(e.Text);
        }

        private void OnPreviewKeyDownNumeric(object sender, KeyEventArgs e)
        {
            if (!ShouldRestrict((sender as FrameworkElement)?.DataContext)) return;
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Enter)
                return;
        }

        private void OnPasteNumeric(object sender, DataObjectPastingEventArgs e)
        {
            // Allow paste unless target row is numeric-only
            if (FocusManager.GetFocusedElement(this) is TextBox tb && ShouldRestrict(tb.DataContext))
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
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrinaViewModel vm)
            {
                vm.Confirm();
            }
            DialogResult = true;
            Close();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrinaViewModel vm)
            {
                vm.ClearValores();
            }
        }
    }
}

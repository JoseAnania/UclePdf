using System.Windows;
using UclePdf.ViewModels;

namespace UclePdf.Views
{
    public partial class RaspajePielWindow : Window
    {
        public RaspajePielWindow() : this(new RaspajePielViewModel()) { }

        public RaspajePielWindow(RaspajePielViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            vm.ResetIfEmpty();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is RaspajePielViewModel vm)
            {
                vm.Confirm();
            }
            DialogResult = true;
            Close();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is RaspajePielViewModel vm)
            {
                vm.Clear();
            }
        }
    }
}

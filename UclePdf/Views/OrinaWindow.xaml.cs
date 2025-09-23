using System.Windows;
using UclePdf.ViewModels;

namespace UclePdf.Views
{
    public partial class OrinaWindow : Window
    {
        public OrinaWindow() : this(new OrinaViewModel()) { }

        public OrinaWindow(OrinaViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
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

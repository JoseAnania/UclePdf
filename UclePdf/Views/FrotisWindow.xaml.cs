using System.Windows;
using UclePdf.ViewModels;

namespace UclePdf.Views
{
    public partial class FrotisWindow : Window
    {
        public FrotisWindow() : this(new FrotisViewModel()) { }

        public FrotisWindow(FrotisViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // Si quedó vacío de una sesión anterior, reestablecer texto por defecto
            vm.ResetIfEmpty();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is FrotisViewModel vm)
            {
                vm.Confirm();
            }
            DialogResult = true;
            Close();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is FrotisViewModel vm)
            {
                vm.ClearResultado();
            }
        }
    }
}

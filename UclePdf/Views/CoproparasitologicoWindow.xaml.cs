using System.Windows;
using UclePdf.ViewModels;

namespace UclePdf.Views
{
    public partial class CoproparasitologicoWindow : Window
    {
        public CoproparasitologicoWindow() : this(new CoproparasitologicoViewModel()) { }

        public CoproparasitologicoWindow(CoproparasitologicoViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            vm.ResetIfEmpty();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is CoproparasitologicoViewModel vm)
            {
                vm.Confirm();
            }
            DialogResult = true;
            Close();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is CoproparasitologicoViewModel vm)
            {
                vm.ClearResultados();
            }
        }
    }
}

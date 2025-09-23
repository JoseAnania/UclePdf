using System.Windows;
using UclePdf.ViewModels;

namespace UclePdf.Views
{
    public partial class EhrlichiosisWindow : Window
    {
        public EhrlichiosisWindow() : this(new EhrlichiosisViewModel()) { }

        public EhrlichiosisWindow(EhrlichiosisViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is EhrlichiosisViewModel vm)
            {
                vm.Confirm();
            }
            DialogResult = true;
            Close();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is EhrlichiosisViewModel vm)
            {
                vm.Clear();
            }
        }
    }
}

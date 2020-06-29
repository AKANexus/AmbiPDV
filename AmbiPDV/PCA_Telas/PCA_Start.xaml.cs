using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.PCA_Telas
{
    /// <summary>
    /// Interaction logic for PCA_Start.xaml
    /// </summary>
    public partial class PCA_Start : Page
    {
        public PCA_Start()
        {
            InitializeComponent();
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed) return;
            switch (cbb_Impressora.SelectedIndex)
            {
                case 0: //ECF
                    NavigationService.Navigate(new PCA_Spooler(Impressora.ECF));
                    return;
                case 1: //SAT
                    NavigationService.Navigate(new PCA_Spooler(Impressora.SAT));
                    return;
                default:
                    return;

            }
        }

        private void But_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}

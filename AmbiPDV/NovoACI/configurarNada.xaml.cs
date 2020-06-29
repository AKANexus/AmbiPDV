using System.Windows.Controls;
using PDV_WPF;
using System.Windows.Input;

namespace PDV_WPF.NovoACI
{
    /// <summary>
    /// Interação lógica para configurarNada.xam
    /// </summary>
    public partial class configurarNada : Page
    {
        PrinterSettings MPS;
        public configurarNada(PrinterSettings PrevPS)
        {
            InitializeComponent();
            MPS = PrevPS;
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MPS.modo_teste = true;
            NavigationService.Navigate(new FimdeConfig(MPS));
            return;
        }
        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
            return;
        }
    }

}

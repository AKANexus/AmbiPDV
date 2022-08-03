using System.Windows.Controls;
using System.Windows.Input;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO.NovoACI
{
    /// <summary>
    /// Interação lógica para configurarNada.xam
    /// </summary>
    public partial class configurarNada : Page
    {
        //PrinterSettings PS;
        public configurarNada(/*PrinterSettings PrevPS*/)
        {
            InitializeComponent();
            //PS = PrevPS;
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //PS.teste = true;
            NavigationService.Navigate(new FimdeConfig(TipoImpressora.nenhuma));
        }
        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
        }
    }

}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using static PDV_WPF.staticfunc;
using PDV_WPF;
using System;

namespace PDV_ORCAMENTO.NovoACI
{
    /// <summary>
    /// Interaction logic for FimdeConfig.xaml
    /// </summary>
    public partial class FimdeConfig : Page
    {
        private TipoImpressora _tipoImpressora;

        //PrinterSettings PS;
        public FimdeConfig(/*PrinterSettings PrevPS*/TipoImpressora tipoImpressora)
        {
            InitializeComponent();
            //PS = PrevPS;

            _tipoImpressora = tipoImpressora;
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (PS.configurar_nf_usb)
            //{

            SetarTipoImpressora();

            Window.GetWindow(this).Close();
            //}
        }

        private void SetarTipoImpressora()
        {
            Properties.Settings.Default.TipoImpressora = (int)_tipoImpressora;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

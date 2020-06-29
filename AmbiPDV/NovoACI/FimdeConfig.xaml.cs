using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PDV_WPF.NovoACI
{
    /// <summary>
    /// Interaction logic for FimdeConfig.xaml
    /// </summary>
    public partial class FimdeConfig : Page
    {
        private PrinterSettings MPS;
        public FimdeConfig(PrinterSettings PrevPS)
        {
            InitializeComponent();
            MPS = PrevPS;
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MPS.ValidaSettings())
            {
                if (MPS.status_naofiscal == staticfunc.setupstatus.SetupDone)
                {
                    Properties.Settings.Default.ImpressoraUSB = MPS.usb_printer;
                }
                if (MPS.status_sat == staticfunc.setupstatus.SetupDone)
                {
                    Properties.Settings.Default.SAT_Ativo = true;
                    Properties.Settings.Default.SAT_CodAtiv = MPS.sat_cod_ativ;
                }
            }

            //Properties.Settings.Default.ImpressoraUSB = MPS.usar_nf ? MPS.usb_printer : "Nenhuma";
            //Properties.Settings.Default.ECF_Ativa = MPS.usar_ecf;// ? true : false;
            //Properties.Settings.Default.SAT_Ativo = MPS.usar_sat;

            Window.GetWindow(this).Close();
        }


        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
            return;
        }
    }
}


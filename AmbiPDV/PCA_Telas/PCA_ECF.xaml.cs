using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;



namespace PDV_WPF.PCA_Telas
{
    /// <summary>
    /// Interaction logic for configurarECF.xaml
    /// </summary>
    public partial class PCA_ECF : Page
    {
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        public PCA_ECF()
        {
            InitializeComponent();
            foreach (string port in ports)
            {
                cbb_Ports.Items.Add(port);
            }
            if (!string.IsNullOrWhiteSpace(ECF_PORTA))
            {
                cbb_Ports.Text = ECF_PORTA;
            }
        }

        private string[] ports = System.IO.Ports.SerialPort.GetPortNames();

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
            return;
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed) return;

            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                SAT_USADO = false;
                ECF_ATIVA = true;
                SalvaConfigsNaBase();
                Window.GetWindow(this).Close();
                return;
            });
        }

        private void but_Action_MouseDown(object sender, MouseButtonEventArgs e)
        {
            {
                LocalDarumaFrameworkDLL.UnsafeNativeMethods.regAlterarValor_Daruma(@"START\Produto", "ECF");
                LocalDarumaFrameworkDLL.UnsafeNativeMethods.regAlterarValor_Daruma(@"DUAL\EncontrarDUAL", "0");
                LocalDarumaFrameworkDLL.UnsafeNativeMethods.regAlterarValor_Daruma(@"ECF\PortaSerial", (string)cbb_Ports.SelectedValue);
                LocalDarumaFrameworkDLL.UnsafeNativeMethods.regAlterarValor_Daruma(@"ECF\EncontrarECF", "0");
                LocalDarumaFrameworkDLL.UnsafeNativeMethods.regAlterarValor_Daruma(@"ECF\Velocidade", "115200");
                LocalDarumaFrameworkDLL.UnsafeNativeMethods.regAlterarValor_Daruma(@"ECF\ArredondarTruncar", "T");
                int resultado = LocalDarumaFrameworkDLL.UnsafeNativeMethods.rVerificarImpressoraLigada_ECF_Daruma();
                switch (resultado)
                {
                    case 1:
                        but_Next.MouseDown += but_Next_MouseDown;
                        tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
                        ECF_ATIVA = true;
                        ECF_PORTA = (string)cbb_Ports.SelectedValue;
                        break;
                    //case -6:
                    //    MessageBox.Show("rVerificarImpressoraLigada_ECF_Daruma = "+resultado);
                    //    break;
                    default:
                        MessageBox.Show("rVerificarImpressoraLigada_ECF_Daruma = " + resultado);
                        break;
                }

            }

        }
    }
}

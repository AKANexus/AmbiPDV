using PDV_WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace PDV_WPF.NovoACI
{
    /// <summary>
    /// Interaction logic for configurarECF.xaml
    /// </summary>
    public partial class configurarECF : Page
    {
        PrinterSettings MPS;
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        public configurarECF(PrinterSettings PrevPS)
        {
            InitializeComponent();
            MPS = PrevPS;
            foreach (string port in ports)
            {
                cbb_Ports.Items.Add((string)port);
            }
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ECF_Porta))
                cbb_Ports.Text = Properties.Settings.Default.ECF_Porta;
        }
        string[] ports = System.IO.Ports.SerialPort.GetPortNames();

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
            return;
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                MPS.status_ecf = staticfunc.setupstatus.SetupDone;
                MPS.ecf_port = (string)cbb_Ports.SelectedValue;
                MPS.ecf_speed = "115200";
                NavigationService.Navigate(new FimdeConfig(MPS));
                return;
            });
        }

        private void but_Action_MouseDown(object sender, MouseButtonEventArgs e)
        {
            {
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"START\Produto", "ECF");
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"DUAL\EncontrarDUAL", "0");
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"ECF\PortaSerial", (string)cbb_Ports.SelectedValue);
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"ECF\EncontrarECF", "0");
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"ECF\Velocidade", "115200");
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"ECF\ArredondarTruncar", "A");
                int resultado = LocalDarumaFrameworkDLL.Declaracoes.rVerificarImpressoraLigada_ECF_Daruma();
                switch (resultado)
                {
                    case 1:
                        but_Next.MouseDown += but_Next_MouseDown;
                        tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
                        Properties.Settings.Default.ECF_Ativa = true;
                        Properties.Settings.Default.ECF_Porta = (string)cbb_Ports.SelectedValue;
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

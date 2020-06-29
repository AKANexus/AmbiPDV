using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using PDV_WPF;
using System;
using static PDV_WPF.staticfunc;

namespace PDV_WPF.NovoACI
{
    /// <summary>
    /// Interaction logic for configurarSerial.xaml
    /// </summary>
    public partial class configurarSerial : Page
    {
        #region Fields & Properties

        PrinterSettings PS;
        string[] ports = System.IO.Ports.SerialPort.GetPortNames();
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public configurarSerial(PrinterSettings prevPS)
        {
            InitializeComponent();
            PS = prevPS;
            foreach (string port in ports)
            {
                cbb_Ports.Items.Add((string)port);
            }
        }

        #endregion (De)Constructor

        #region Events

        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"START\Produto", "DUAL");
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"DUAL\PortaComunicacao", (string)cbb_Ports.SelectedValue);
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"DUAL\EncontrarDUAL", "0");
                LocalDarumaFrameworkDLL.Declaracoes.regAlterarValor_Daruma(@"DUAL\Velocidade", "115200");
                LocalDarumaFrameworkDLL.Declaracoes.iImprimirTexto_DUAL_DarumaFramework("<ce><e><b>Teste de Impressão.</b></e><sl>2</sl>Se você consegue ler isso, sua impressora foi corretamente configurada e está pronta para ser utilizada com o sistema!<sl>3</sl><gui></ce>", 0);
                MessageBoxResult result = MessageBox.Show("A impressão saiu corretamente na impressora desejada?", "Pergunta", MessageBoxButton.YesNo, MessageBoxImage.Question);
                //TODO Substituir pelo método próprio da Daruma que faz a checagem do status da impressora.
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        but_Next.MouseDown += but_Next_MouseDown;
                        tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show("Favor escolher corretamente a porta e tentar novamente.");
                        break;
                }
            });
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                //if (PS.usar_ecf)
                //{
                //    NavigationService.Navigate(new configurarECF(PS));
                //}
                //else if (PS.usar_sat)
                //{
                //    NavigationService.Navigate(new configurarSAT(PS));
                //}
                //else
                //{
                //    NavigationService.Navigate(new FimdeConfig(PS));
                //}
            });
        }

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
            return;
        }

        #endregion Events

        #region Methods

        #endregion Methods
    }
}

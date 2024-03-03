using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.PCA_Telas
{
    /// <summary>
    /// Interaction logic for PCA_Spooler.xaml
    /// </summary>
    public partial class PCA_Spooler : Page
    {
        private bool configurado = false;
        private Impressora _impressora { get; set; }
        public PCA_Spooler(Impressora impressora)
        {
            InitializeComponent();
            _impressora = impressora;
        }
        #region Events        
        private void But_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed) return;

            switch (_impressora)
            {
                case Impressora.SAT:
                    if (configurado) NavigationService.Navigate(new PCA_SAT());
                    return;
                case Impressora.ECF:
                    if (configurado) NavigationService.Navigate(new PCA_ECF());
                    return;
                default:
                    return;
            }
        }

        private void But_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            cbb_printers.Items.Add("Nenhuma");
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                cbb_printers.Items.Add(printer);
            }
            CarregaConfigs();
            //int index = (int)cbb_printers.FindName(IMPRESSORA_USB);
            //cbb_printers.Text = IMPRESSORA_USB;
            cbb_printers.SelectedIndex = cbb_printers.Items.IndexOf(IMPRESSORA_USB);
            if (cbb_printers.SelectedIndex > 0)
            {
                configurado = true;
                tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
            }
        }

        private void But_Action_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (cbb_printers.SelectedIndex.ToString() != "-1")
            {
                IMPRESSORA_USB = cbb_printers.SelectedItem.ToString();
                SalvaConfigsNaBase();

                if (cbb_printers.SelectedItem.ToString() == "Nenhuma" && (MessageBox.Show("Deseja não usar impressora não fiscal?", "Pergunta", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
                {
                    configurado = true;
                    tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
                    return;
                }
                PrintFunc.RecebePrint("Teste de impressão.", PrintFunc.titulo, PrintFunc.centro, 1);
                PrintFunc.RecebePrint("Se você consegue ler isso, sua impressora foi corretamente configurada no sistema.", PrintFunc.titulo, PrintFunc.centro, 1);

                PrintFunc.PrintaSpooler();
                MessageBoxResult result = MessageBox.Show("A impressão saiu corretamente na impressora desejada?", "Pergunta", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        configurado = true;
                        tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
                        break;

                    case MessageBoxResult.No:
                        configurado = false;
                        break;
                }
            }
        }
        #endregion
    }
}

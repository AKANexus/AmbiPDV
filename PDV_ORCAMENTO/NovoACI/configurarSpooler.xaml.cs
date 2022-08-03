using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using PDV_WPF;
using static PDV_WPF.staticfunc;
using System.Drawing.Printing;
//using NewPrinter;

namespace PDV_ORCAMENTO.NovoACI
{
    /// <summary>
    /// Interaction logic for configurarSpooler.xaml
    /// </summary>
    public partial class configurarSpooler : Page
    {
        // Deve testar uma impressão numa impressora térmica 80mm.

        #region Fields & Properties
        //PrinterSettings PS;
        public TipoImpressora _TipoImpressora { get; set; }
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        #endregion Fields & Properties

        #region (De)Constructor

        public configurarSpooler(/*PrinterSettings PrevPS*/TipoImpressora tipoImpressora)
        {
            InitializeComponent();
            RefreshList();
            //PS = PrevPS;
            _TipoImpressora = tipoImpressora;
        }

        #endregion (De)Constructor

        #region Events

        private void cbb_printers_DropDownClosed(object sender, EventArgs e)
        {
            if (cbb_printers.SelectedIndex.ToString() != "-1")
            {
                Properties.Settings.Default.ImpressoraUSB = cbb_printers.SelectedItem.ToString();
                //MessageBox.Show("cbb_printers.SelectedItem.ToString(): "+ cbb_printers.SelectedItem.ToString());
                PrintFunc.RecebePrint("Teste de impressão.", PrintFunc.Titulo, PrintFunc.centro.align, 1, _TipoImpressora);
                PrintFunc.RecebePrint("Se você consegue ler isso, sua impressora foi corretamente configurada no sistema.", PrintFunc.Titulo, PrintFunc.centro.align, 1, _TipoImpressora);

                PaperSize paperSize = new PaperSize();
                
                switch (_TipoImpressora)
                {
                    case TipoImpressora.officeA4:
                        paperSize = new PaperSize("Inicio", 826, 1169);
                        break;
                    case TipoImpressora.thermal80:
                        paperSize = new PaperSize("Inicio", 400, 999999);
                        break;
                    default:
                        // Não chega aqui
                        EstourarTipoImpressoraInesperado();
                        break;
                }

                PrintFunc.PrintaSpooler();
                
                MessageBoxResult result = MessageBox.Show("A impressão saiu corretamente na impressora desejada?", "Pergunta", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        but_Next.MouseDown += but_Next_MouseDown;
                        tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show("Favor selecionar a impressora correta e verificar se ela está devidamente instalada no sistema.");
                        break;
                }
            }
        }

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                //Properties.Settings.Default.ImpressoraUSB = "Nenhuma";
                //Properties.Settings.Default.Save();
                //Properties.Settings.Default.Reload();
                NavigationService.GoBack();
            });
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                NavigationService.Navigate(new FimdeConfig(_TipoImpressora));
            });
        }

        #endregion Events

        #region Methods

        private void RefreshList()
        {
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                cbb_printers.Items.Add(printer);
            }
        }

        private void EstourarTipoImpressoraInesperado()
        {
            string mensagem = "Tipo de impressora não esperado: " + _TipoImpressora.ToString();
            gravarMensagemErro(mensagem);
            throw new NotImplementedException(mensagem);
        }

        #endregion Methods

    }
}

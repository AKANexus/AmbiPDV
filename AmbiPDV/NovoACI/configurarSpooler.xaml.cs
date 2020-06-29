using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using PDV_WPF;
//using NewPrinter;

namespace PDV_WPF.NovoACI
{
    /// <summary>
    /// Interaction logic for configurarSpooler.xaml
    /// </summary>
    public partial class configurarSpooler : Page
    {
        #region Fields & Properties
        PrinterSettings MPS;
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        #endregion Fields & Properties

        #region (De)Constructor

        public configurarSpooler(PrinterSettings PrevPS)
        {
            InitializeComponent();
            RefreshList();
            MPS = PrevPS;
        }

        #endregion (De)Constructor

        #region Events

        private void cbb_printers_DropDownClosed(object sender, EventArgs e)
        {
            if (cbb_printers.SelectedIndex.ToString() != "-1")
            {
                Properties.Settings.Default.ImpressoraUSB = cbb_printers.SelectedItem.ToString();
                //MessageBox.Show("cbb_printers.SelectedItem.ToString(): "+ cbb_printers.SelectedItem.ToString());
                PrintFunc.RecebePrint("Teste de impressão.", PrintFunc.Titulo, PrintFunc.centro.align, 1);
                PrintFunc.RecebePrint("Se você consegue ler isso, sua impressora foi corretamente configurada no sistema.", PrintFunc.Titulo, PrintFunc.centro.align, 1);

                PrintFunc.PrintaSpooler();
                MessageBoxResult result = MessageBox.Show("A impressão saiu corretamente na impressora desejada?", "Pergunta", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        //Properties.Settings.Default.ImpressoraSERIAL = false;
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
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                NavigationService.GoBack();
                return;
            });
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                MPS.status_naofiscal = staticfunc.setupstatus.SetupDone;
                MPS.usb_printer = cbb_printers.SelectedItem.ToString();
                if (MPS.status_sat == staticfunc.setupstatus.ToSetup)
                {
                    NavigationService.Navigate(new configurarSAT(MPS));
                    return;
                }
                else
                {
                    NavigationService.Navigate(new FimdeConfig(MPS));
                    return;
                }
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

        #endregion Methods

    }
}

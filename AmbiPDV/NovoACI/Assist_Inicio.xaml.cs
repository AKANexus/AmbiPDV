using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PDV_WPF.staticfunc;

namespace PDV_WPF.NovoACI
{

    

    /// <summary>
    /// Interação lógica para Assist_Inicio.xam
    /// </summary>
    public partial class Assist_Inicio : Page
    {
        private PrinterSettings MPS = new PrinterSettings();

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        public Assist_Inicio()
        {
            InitializeComponent();
            MPS.status_naofiscal = MPS.status_sat = setupstatus.ToSetup;
            MPS.status_ecf = setupstatus.NoSetup;
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                if (MPS.status_naofiscal == setupstatus.ToSetup)
                {
                    NavigationService.Navigate(new configurarSpooler(MPS));
                    return;
                }
                if (MPS.status_sat == setupstatus.ToSetup)
                {
                    NavigationService.Navigate(new configurarSAT(MPS));
                    return;
                }
                if (MPS.modo_teste)
                {
                    NavigationService.Navigate(new configurarNada(MPS));
                    return;
                }
            });
            return;
        }

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (MessageBox.Show("Deseja cancelar?", "Assistente de configuração", MessageBoxButton.YesNo))
            {
                case MessageBoxResult.Yes:
                    //RequestClose(EventArgs.Empty);
                    Window.GetWindow(this).Close();
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    break;
            }
        }
    }
}

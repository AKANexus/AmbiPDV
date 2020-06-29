using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using PDV_WPF.Telas;
using static PDV_WPF.staticfunc;

namespace PDV_WPF.NovoACI
{
    /// <summary>
    /// Interação lógica para ColetaInfo.xam
    /// </summary>
    public partial class ColetaInfo : Page
    {
        #region Fields & Properties

        private PrinterSettings MPS = new PrinterSettings();

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public ColetaInfo()
        {
            InitializeComponent();
            MPS.status_naofiscal = MPS.status_ecf = MPS.status_sat = setupstatus.NoSetup;
        }

        #endregion (De)Constructor

        #region Events

        

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
        }

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                NavigationService.GoBack();
                return;
            });
        }

        public event EventHandler OnValidar;

        #endregion Events

        #region Methods

        protected virtual void Validar(ValidarEventArgs e)
        {
            OnValidar?.Invoke(this, e);
        }

        #endregion Methods

        public class ValidarEventArgs : EventArgs
        {
            
        }

        private void cbx_Clicked(object sender, RoutedEventArgs e)
        {
            cbx_nfiscal_usb.IsEnabled = !(bool)cbx_testes.IsChecked;
            cbx_testes.IsEnabled = !((bool)cbx_ecf.IsChecked || (bool)cbx_sat.IsChecked || (bool)cbx_nfiscal_usb.IsChecked);
            cbx_sat.IsEnabled = !((bool)cbx_ecf.IsChecked || (bool)cbx_testes.IsChecked);
            cbx_ecf.IsEnabled = !((bool)cbx_sat.IsChecked || (bool)cbx_testes.IsChecked);

            switch (cbx_ecf.IsChecked)
            {
                case true:
                    MPS.status_ecf = setupstatus.NoSetup;
                    break;
                default:
                case false:
                    MPS.status_ecf = setupstatus.NoSetup;
                    break;
            }
            switch (cbx_sat.IsChecked)
            {
                case true:
                    MPS.status_sat = setupstatus.ToSetup;
                    break;
                default:
                case false:
                    MPS.status_sat = setupstatus.NoSetup;
                    break;
            }
            switch (cbx_nfiscal_usb.IsChecked)
            {
                case true:
                    MPS.status_naofiscal = setupstatus.ToSetup;
                    break;
                default:
                case false:
                    MPS.status_naofiscal = setupstatus.NoSetup;
                    break;
            }
            switch (cbx_testes.IsChecked)
            {
                case true:
                    MPS.modo_teste = true;
                    break;
                default:
                case false:
                    MPS.modo_teste = false;
                    break;
            }
        }
    }
}

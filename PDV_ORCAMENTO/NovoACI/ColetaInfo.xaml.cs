using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using PDV_WPF;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO.NovoACI
{
    /// <summary>
    /// Interação lógica para ColetaInfo.xam
    /// </summary>
    public partial class ColetaInfo : Page
    {
        #region Fields & Properties

        public TipoImpressora _TipoImpressora { get; set; }
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public ColetaInfo()
        {
            InitializeComponent();

            CarregarTipoImpressora();
        }

        #endregion (De)Constructor

        #region Events

        //private void cbx_80_thermal_printer_Checked(object sender, RoutedEventArgs e)
        //{
        //    _TipoImpressora = TipoImpressora.thermal80;
        //    //impressora = true;
        //    cbx_testes.IsChecked = cbx_testes.IsEnabled = false;
        //}

        //private void cbx_80_thermal_printer_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    _TipoImpressora = TipoImpressora.nenhuma;
        //    //impressora = false;
        //    cbx_testes.IsEnabled = true;
        //}

        //private void cbx_nfiscal_serial_Checked(object sender, RoutedEventArgs e)
        //{
        //    _nf = NF.serial;
        //    impressora = true;
        //    cbx_nfiscal_usb.IsChecked = cbx_nfiscal_usb.IsEnabled = false;
        //    cbx_testes.IsChecked = cbx_testes.IsEnabled = false;
        //}

        //private void cbx_nfiscal_serial_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    _nf = NF.nenhuma;
        //    impressora = false;
        //    cbx_nfiscal_usb.IsEnabled = true;
        //    cbx_testes.IsEnabled = true;
        //}

        //private void cbx_testes_Checked(object sender, RoutedEventArgs e)
        //{
        //    _ecf = ECF.nao;
        //    _nf = NF.nenhuma;
        //    _UsaSAT = UsaSAT.nao;
        //    _teste = true;
        //    cbx_80_thermal_printer.IsEnabled = false;
        //    cbx_80_thermal_printer.IsChecked = false;


        //}

        //private void cbx_testes_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    _teste = false;
        //    cbx_nfiscal_usb.IsEnabled = true;
        //}

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                //if (ValidaDados())
                //{
                switch (_TipoImpressora)
                {
                    case TipoImpressora.officeA4:
                        {
                            this.NavigationService.Navigate(new configurarSpooler(_TipoImpressora));
                            break;
                        }
                    case TipoImpressora.thermal80:
                        this.NavigationService.Navigate(new configurarSpooler(_TipoImpressora));
                        break;
                    case TipoImpressora.nenhuma:
                        this.NavigationService.Navigate(new configurarNada());
                        break;
                    default:
                        {
                            EstourarTipoImpressoraInesperado();
                            break;
                        }
                }
                //}
            });
        }

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                NavigationService.GoBack();
            });
        }
        
        //private void cbx_office_printer_Checked(object sender, RoutedEventArgs e)
        //{

        //}

        //private void cbx_office_printer_Unchecked(object sender, RoutedEventArgs e)
        //{

        //}

        private void rbt80ThermalPrinter_Checked(object sender, RoutedEventArgs e)
        {
            _TipoImpressora = TipoImpressora.thermal80;
        }

        private void rbtOfficePrinter_Checked(object sender, RoutedEventArgs e)
        {
            _TipoImpressora = TipoImpressora.officeA4;
        }

        private void rbtTestes_Checked(object sender, RoutedEventArgs e)
        {
            _TipoImpressora = TipoImpressora.nenhuma;
        }

        #endregion Events

        #region Methods

        //protected virtual void Validar(ValidarEventArgs e)
        //{
        //    OnValidar?.Invoke(this, e);
        //}

        //public bool ValidaDados()
        //{
        //    if ((impressora == true && fiscal == true) || (_teste == true))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        private void CarregarTipoImpressora()
        {
            try
            {
                switch ((TipoImpressora)Properties.Settings.Default.TipoImpressora)
                {
                    case TipoImpressora.officeA4:
                        rbtOfficePrinter.IsChecked = true;
                        break;
                    case TipoImpressora.thermal80:
                        rbt80ThermalPrinter.IsChecked = true;
                        break;
                    case TipoImpressora.nenhuma:
                        rbtTestes.IsChecked = true;
                        break;
                    default:
                        EstourarTipoImpressoraInesperado();
                        break;
                }
            }
            catch (Exception ex)
            {
                gravarMensagemErro(RetornarMensagemErro(ex, true));
                rbtTestes.IsChecked = true;
            }
        }

        private void EstourarTipoImpressoraInesperado()
        {
            string mensagem = "Tipo de impressora não esperado: " + _TipoImpressora.ToString();
            gravarMensagemErro(mensagem);
            throw new NotImplementedException(mensagem);
        }

        #endregion Methods

        //public class ValidarEventArgs : EventArgs
        //{
        //    public PrinterSettings PSettings { get; set; }
        //}
    }
}

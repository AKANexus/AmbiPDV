using PDV_WPF.FDBDataSetTableAdapters;
using System;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for BalancaBox.xaml
    /// </summary>
    public partial class BalancaBox : Window
    {
        #region Fields & Properties

        public decimal novopeso { get; set; }
        public bool retry { get; set; }
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public BalancaBox()
        {
            InitializeComponent();           
        }

        #endregion (De)Constructor

        #region Events
        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;            
            this.Close();
            return;
        }

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            //TODO: avaliar o uso desse event handler
            //if (txb_Peso.Text != "")
            if (ValidarPeso())
            {
                try
                {
                    novopeso = Convert.ToDecimal(txb_Peso.Text.Replace('.', ','));
                    DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    MessageBox.Show("Peso inválido. Por favor verifique o peso informado.");
                    txb_Peso.Clear();
                    return;
                }

                this.Close();
                return;
            }
            else
            {
                DialogResult = false;
                retry = true;
                this.Close();
                return;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txb_Peso.Focus();
            txb_Peso.SelectAll();
        }
        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                retry = false;                                          
                this.Close();
                return;
            }
            else if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    //if (txb_Peso.Text != "")
                    if (ValidarPeso())
                    {
                        try
                        {
                            novopeso = Convert.ToDecimal(txb_Peso.Text.Replace('.', ','));
                            DialogResult = true;
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            logErroAntigo(RetornarMensagemErro(ex, true));
                            MessageBox.Show("Peso inválido. Por favor verifique o peso informado.");
                            txb_Peso.Clear();
                            return;
                        }

                        this.Close();
                        return;
                    }
                    else
                    {
                        DialogResult = false;
                        retry = true;
                        this.Close();
                        return;
                    }
                });
            }
        }

        #endregion Events

        #region Methods

        private bool ValidarPeso()
        {
            if (string.IsNullOrWhiteSpace(txb_Peso.Text)) return false;
            if (!txb_Peso.Text.Replace(",", "").Replace(".", "").IsNumbersOnly()) return false;
            if (!decimal.TryParse(txb_Peso.Text, out decimal peso)) return false;
            if (peso > 1000) return false;

            return true;
        }

        #endregion Methods      
        
    }
}

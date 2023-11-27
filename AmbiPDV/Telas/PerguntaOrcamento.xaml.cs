using System;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para perguntacupom.xaml
    /// </summary>
    public partial class PerguntaOrcamento : Window
    {
        #region Fields & Properties
        public int numeroInformado { get; set; }
        public string OrigemImportacao { get; set; }
        private EnmTipo TipoImportacao { get; set; }

        public enum EnmTipo
        {
            orcamento, pedido,
            ordemServico, kitPromocional
        }

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        #endregion Fields & Properties

        #region (De)Constructor

        public PerguntaOrcamento(EnmTipo enmTipo)
        {
            numeroInformado = -1;
            InitializeComponent();
            txb_Cupom.Focus();
            lblNumero.Content = enmTipo switch
            {
                EnmTipo.orcamento => "DIGITE O NÚMERO DO ORÇAMENTO / PEDIDO:",                
                EnmTipo.ordemServico => "DIGITE O NÚMERO DA O.S:",
                EnmTipo.kitPromocional => "DIGITE O NÚMERO DO KIT PROMOCIONAL:",
                _ => throw new ArgumentOutOfRangeException(nameof(enmTipo), enmTipo, null)
            };
            options_Orca.Visibility = enmTipo.Equals(EnmTipo.orcamento) ? Visibility.Visible : Visibility.Collapsed;
            TipoImportacao = enmTipo;
        }

        #endregion (De)Constructor

        #region Events

        private void PerguntaSenha_KeyDown(object sender, KeyEventArgs e)
        {           
            if (e.Key == Key.Enter && txb_Cupom.Text.Length > 0)
            {   
                if(TipoImportacao is EnmTipo.orcamento && rb_AmbiOrca.IsChecked is false && rb_DavsClipp.IsChecked is false)
                {
                    DialogBox.Show("Atenção", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Selecione de onde deseja importar o orçamento / pedido");
                    return;
                }
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    Int32.TryParse(txb_Cupom.Text, out int _orca);
                    numeroInformado = _orca;
                    DialogResult = true;
                });
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void rb_DavsClipp_Checked(object sender, RoutedEventArgs e)
        {
            OrigemImportacao = "DavsClipp";
            txb_Cupom.Focus();
        }

        private void rb_AmbiOrca_Checked(object sender, RoutedEventArgs e)
        {
            OrigemImportacao = "AmbiOrcamento";
            txb_Cupom.Focus();
        }

        #endregion Events

        #region Methods

        #endregion Methods     
    }
}

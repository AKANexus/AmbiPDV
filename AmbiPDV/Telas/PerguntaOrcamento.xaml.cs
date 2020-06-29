using System;
using System.Windows;
using System.Windows.Input;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para perguntacupom.xaml
    /// </summary>
    public partial class PerguntaOrcamento : Window
    {
        #region Fields & Properties
        public int numeroInformado { get; set; }

        public enum EnmTipo
        {
            orcamento, pedido
        }

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        #endregion Fields & Properties

        #region (De)Constructor

        public PerguntaOrcamento(EnmTipo enmTipo)
        {
            numeroInformado = -1;
            InitializeComponent();
            txb_Cupom.Focus();

            switch (enmTipo)
            {
                case EnmTipo.orcamento:
                    lblNumero.Content = "DIGITE O NÚMERO DO ORÇAMENTO";
                    break;
                case EnmTipo.pedido:
                    lblNumero.Content = "DIGITE O NÚMERO DO PEDIDO";
                    break;
                default:
                    break;
            }
        }

        #endregion (De)Constructor

        #region Events

        private void PerguntaSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txb_Cupom.Text.Length > 0)
            {
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

        #endregion Events

        #region Methods

        #endregion Methods
    }
}

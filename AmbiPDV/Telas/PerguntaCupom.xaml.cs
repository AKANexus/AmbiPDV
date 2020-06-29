using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Extensions;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para perguntacupom.xaml
    /// </summary>
    public partial class PerguntaQuantidade : Window
    {
        #region Fields & Properties
        public string quantidadeDigitada { get; set; }

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        #endregion Fields & Properties

        #region (De)Constructor

        public PerguntaQuantidade()
        {
            InitializeComponent();
            txb_Cupom.Focus();
        }

        #endregion (De)Constructor

        #region Events

        private void PerguntaSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txb_Cupom.Text.Length > 0 && txb_Cupom.Text.TiraPont().IsNumbersOnly())
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    quantidadeDigitada = txb_Cupom.Text.TiraPont();
                    DialogResult = true;
                    Close();
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

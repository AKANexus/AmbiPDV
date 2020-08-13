using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Extensions;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para perguntacupom.xaml
    /// </summary>
    public partial class PerguntaVale : Window
    {
        #region Fields & Properties
        public int valeDigitado { get; set; }

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        #endregion Fields & Properties

        #region (De)Constructor

        public PerguntaVale()
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
                    valeDigitado = txb_Cupom.Text.Safeint();
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

using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for Desconto.xaml
    /// </summary>
    public partial class Desconto : Window
    {
        #region Fields & Properties

        public decimal porcentagem { get; set; } = 0;
        public decimal reais { get; set; } = 0;
        public bool absoluto { get; set; } = true;
        private bool _restrito;
        private decimal _limite;

        private readonly DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public Desconto(bool restrito, decimal limite)
        {
            _limite = limite;
            _restrito = restrito;
            InitializeComponent();
            txb_Porc.Focus();
            if (restrito)
            {
                txb_Reais.IsEnabled = false;
            }
        }

        #endregion (De)Constructor

        #region Events

        private void txb_Porc_KeyDown(object sender, RoutedEventArgs e)
        {
            if (txb_Porc.Number > _limite && _restrito)
            {
                DialogBox.Show("Desconto", DialogBoxButtons.No, DialogBoxIcons.Info, false, $"Só é permitido desconto de até {_limite }%.");
                txb_Porc.Number = 0;
                return;
            }

            ValidarPercentual();
            but_Aceitar.Focus();
        }

        private void txb_Reais_KeyDown(object sender, RoutedEventArgs e)
        {
            ValidarAbsoluto();
            but_Aceitar.Focus();

        }

        private void but_Aceitar_Click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                if (porcentagem == 0 && reais == 0)
                {
                    DialogResult = false;
                    this.Close();
                    return;
                }
                if (porcentagem != 0 && reais != 0)
                {
                    txb_Porc.Focus();
                }
                if (porcentagem < 0 || reais < 0)
                {
                    DialogBox.Show("Aplicar Desconto", DialogBoxButtons.Yes, DialogBoxIcons.Error, false, "Não era nem pra você ter conseguido exibir esse aviso", "Valores negativos são proibidos");
                    txb_Porc.Focus();
                }
                else
                {
                    if (porcentagem > 0)
                    {
                        absoluto = false;
                    }
                    DialogResult = true;
                    return;
                }
            });
        }
        private void but_Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
            return;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                this.Close();
                return;
            }
        }

        #endregion Events

        #region Methods
        private bool ValidarPercentual()
        {
            if (txb_Porc.Number < 0)
            {
                txb_Porc.Number = 0;
                txb_Porc.Focus();
                return false;
            }
            else if (txb_Reais.Value != 0 && txb_Porc.Number != 0)
            {
                txb_Reais.Value = 0;
                reais = 0;
                return true;
            }
            porcentagem = txb_Porc.Number;
            absoluto = false;
            return true;
        }

        private void ValidarAbsoluto()
        {
            if (txb_Reais.Value < 0)
            {
                txb_Reais.Value = 0;
                txb_Reais.Focus();
                return;
            }
            else if (txb_Porc.Number != 0 && txb_Reais.Value != 0)
            {
                txb_Porc.Number = 0;
                porcentagem = 0;
            }
            reais = txb_Reais.Value;
            absoluto = true;
        }

        #endregion Methods
    }
}

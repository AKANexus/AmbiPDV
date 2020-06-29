using PDV_WPF.Properties;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for IdentificaConsumidor.xaml
    /// </summary>
    public partial class IdentificaConsumidor : Window
    {
        public IdentificaConsumidor()
        {
            InitializeComponent();
            txb_Cliente.Focus();
        }

        public string identificacao { get; set; }
        public CfeRecepcao_0007.ItemChoiceType tipo { get; set; }
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        private void CPF_CNPJ_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    if (Funcoes.ValidaCNPJ.IsCnpj(txb_Cliente.Text.ToString()) == true)
                    {
                        tipo = CfeRecepcao_0007.ItemChoiceType.CNPJ;
                        identificacao = txb_Cliente.Text.ToString();
                        Close();

                    }
                    else if (Funcoes.ValidaCPF.IsCpf(txb_Cliente.Text.ToString()) == true)
                    {
                        tipo = CfeRecepcao_0007.ItemChoiceType.CPF;
                        identificacao = txb_Cliente.Text.ToString();
                        Close();
                    }
                    else
                    {

                        DialogBox.Show(strings.IDENTIFICACAO_DO_CONSUMIDOR, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.CPF_CNPJ_INVALIDO);
                        txb_Cliente.Clear();
                        txb_Cliente.Focus();
                    }
                });
            }
            else if (e.Key == Key.Escape)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    tipo = CfeRecepcao_0007.ItemChoiceType.NENHUM;
                    Close();
                });
            }
            //else if (e.Key == Key.F2)
            //{
            //    tipo = CfeRecepcao_0007.ItemChoiceType.DEMONSTRACAO;
            //    Close();
            //}
        }
    }
}

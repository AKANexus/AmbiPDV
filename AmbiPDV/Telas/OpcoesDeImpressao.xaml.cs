using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using PDV_WPF.Funcoes;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para OpcoesDeImpressao.xaml
    /// </summary>
    public partial class OpcoesDeImpressao : Window
    {

        public DecisaoWhats veredito { get; set; }
        public bool permiteNenhuma;
        string VendaPrazo;
        public OpcoesDeImpressao(string metodoPgto)
        {
            VendaPrazo = metodoPgto;
            InitializeComponent();
            switch (permiteNenhuma)
            {
                case true:
                    lbl_Escape.Visibility = Visibility.Visible;
                    break;
                default:
                    lbl_Escape.Visibility = Visibility.Collapsed;
                    break;
            }
            switch (SYSUSAWHATS.ToBool())
            {
                case true:
                    lbl_whats.Visibility = Visibility.Visible;
                    break;
                default:
                    lbl_whats.Visibility = Visibility.Collapsed;
                    break;
            }
            if (VendaPrazo.Equals("05")) { lbl_whats.Visibility = Visibility.Collapsed; }
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                veredito = DecisaoWhats.ImpressaoNormal;
                DialogResult = true;

            }
            if (e.Key == Key.F3 && SYSUSAWHATS.ToBool())
            {
                if (VendaPrazo.Equals("05")) { MessageBox.Show("Não é possível enviar pelo WhatsApp vendas a prazo", "ATENÇÃO!", MessageBoxButton.OK, MessageBoxImage.Exclamation); }
                else
                {
                    veredito = DecisaoWhats.Whats;
                    DialogResult = true;

                }
            }
            if (e.Key == Key.Escape && permiteNenhuma)
            {
                veredito = DecisaoWhats.NaoImprime;
                DialogResult = true;
            }
        }
    }
}

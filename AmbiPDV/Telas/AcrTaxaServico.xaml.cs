using PDV_WPF.Funcoes;
using System.Windows;
using System.Windows.Input;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for AcrTaxaServico.xaml
    /// </summary>
    public partial class AcrTaxaServico : Window
    {
        public decimal taxa;
        public AcrTaxaServico()
        {
            InitializeComponent();
            txb_Taxa.Focus();
        }


        private void txb_Taxa_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txb_Taxa.Value.IsBetween(0, 1, false))
                {
                    taxa = txb_Taxa.Value;
                    DialogResult = true;
                    Close();
                    return;
                }
                else
                {
                    MessageBox.Show("Por favor digite um valor entre 1 e 99");
                    return;
                }
            }
        }

        private void txb_Taxa_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                return;
            }
        }
    }
}

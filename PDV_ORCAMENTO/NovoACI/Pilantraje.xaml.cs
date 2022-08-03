using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace PDV_ORCAMENTO.NovoACI
{
    /// <summary>
    /// Interaction logic for Pilantraje.xaml
    /// </summary>
    public partial class Pilantraje : Page
    {
        public Pilantraje()
        {
            InitializeComponent();
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NovoACI/ColetaInfo.xaml", UriKind.Relative));
        }

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}

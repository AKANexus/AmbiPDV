using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PDV_ORCAMENTO.NovoACI
{
    /// <summary>
    /// Interação lógica para Assist_Inicio.xam
    /// </summary>
    public partial class Assist_Inicio : Page
    {
        public Assist_Inicio()
        {
            InitializeComponent();
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/NovoACI/Pilantraje.xaml", UriKind.Relative));
        }
        //public event EventHandler OnRequestClose;
        //protected virtual void RequestClose(EventArgs e)
        //{
        //    OnRequestClose?.Invoke(this, e);
        //}

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (MessageBox.Show("Deseja cancelar?", "Assistente de configuração", MessageBoxButton.YesNo))
            {
                case MessageBoxResult.Yes:
                    //RequestClose(EventArgs.Empty);
                    Window.GetWindow(this).Close();
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    break;
            }
        }
    }
}

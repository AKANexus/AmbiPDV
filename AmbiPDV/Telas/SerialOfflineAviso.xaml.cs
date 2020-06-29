using System.Windows;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for SerialOfflineAviso.xaml
    /// </summary>
    public partial class SerialOfflineAviso : Window
    {
        #region Fields & Properties


        #endregion Fields & Properties

        #region (De)Contructor

        public SerialOfflineAviso()
        {
            InitializeComponent();
            SetarContentDosLabels();
        }

        #endregion (De)Contructor

        #region Events

        private void btnSim_Click(object sender, RoutedEventArgs e)
        {
            ////this.Close(); // dá pau ao pegar o DialogResult
            //this.Hide(); // dá pau ao pegar o DialogResult

            this.WindowState = WindowState.Minimized;

            DialogResult = (new SerialOffline()).ShowDialog();

            this.Close();
        }

        private void btnNao_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion Events

        #region Methods

        private void SetarContentDosLabels()
        {
            string mensagem = "O sistema de gestão Ambisoft PDV precisará de uma \n " +
                              "validação em breve (falta(m) " + (new Funcoes.LicencaDeUsoOffline(90, 15)).GetDiasRestantes().ToString() + " dia(s) para expirar a licença). \n" +
                              "Deseja entrar em contato com a Ambisoft Tecnologia para adquirir uma \n" +
                              "chave agora?";
            lblMensagem1.Content = mensagem;

            txtLblComercial.Text = "(11) 4304-7778";
            txtLblWhatsApp.Text = "(11) 96332-8594";
            txtLbl24Horas.Text = "(11) 94015-4600";
        }

        #endregion Methods

    }
}
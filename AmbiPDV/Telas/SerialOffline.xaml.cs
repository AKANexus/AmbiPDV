using System;
using System.Windows;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for SerialOffline.xaml
    /// </summary>
    public partial class SerialOffline : Window
    {
        #region Fields & Properties

        private string _strSerialHexNumberFromDisk;

        #endregion Fields & Properties

        #region (De)Constructor

        public SerialOffline()
        {
            InitializeComponent();
            InicializarFieldsAndProperties();
            SetarContentDosLabels();
        }

        #endregion (De)Constructor

        #region Events

        private void btnValidar_Click(object sender, RoutedEventArgs e)
        {
            ValidarSerial();
        }

        private void btnSair_Click(object sender, RoutedEventArgs e)
        {
            FecharJanela(false);
        }

        #endregion Events

        #region Methods

        private void ValidarSerial()
        {
            if (txtSerial.Text.ToUpper() == new Funcoes.LicencaDeUsoOffline(90, 15).GerarSerial(_strSerialHexNumberFromDisk))
            {
                // Serial válido;
                FecharJanela(true);
            }
            else
            {
                DialogBox.Show("", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "A chave de ativação informada não é válida.");
                txtSerial.Focus();
            }
        }

        private void FecharJanela(bool pDialogResult)
        {
            DialogResult = pDialogResult;
            this.Close();
        }

        private void SetarContentDosLabels()
        {
            txtLblSerialHd.Text = _strSerialHexNumberFromDisk;
            lblInfo1.Content = "O sistema AmbiPDV ultrapassou o prazo de validade e \nnecessita de uma nova chave de ativação. \n\nEntre em contato com a Ambisoft Tecnologia para adquirir uma chave.";
            lblInfo2.Content = "Informe ao suporte a sequência abaixo para receber a \nchave de ativação:";
            txtLblSerialHd.Text = _strSerialHexNumberFromDisk;
            lblInfo3.Content = "Digite a chave de ativação aqui:";

            txtLblComercial.Text = "(11) 4304-7778";
            txtLblWhatsApp.Text = "(11) 96332-8594";
            txtLbl24Horas.Text = "(11) 94015-4600";
        }

        private void InicializarFieldsAndProperties()
        {
            _strSerialHexNumberFromDisk = new Funcoes.LicencaDeUsoOffline(90, 15).GetSerialHexNumberFromExecDisk();
        }

        #endregion Methods
    }
}
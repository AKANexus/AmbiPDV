using FirebirdSql.Data.FirebirdClient;
using System;
using System.Windows;
using System.Windows.Controls;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;


namespace PDV_WPF.SWAT
{
    /// <summary>
    /// Interaction logic for ResetLicenca.xaml
    /// </summary>
    public partial class ResetLicenca : Page
    {
        public ResetLicenca()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using FbCommand fbComm = new FbCommand();
            using FbConnection fbConn = new FbConnection();
            string _strConnContingency = MontaStringDeConexao("localhost", localpath);
            string _strConnNetwork = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
            fbConn.ConnectionString = _strConnContingency;
            fbComm.Connection = fbConn;
            fbComm.CommandType = System.Data.CommandType.Text;
            fbComm.CommandText = "DELETE FROM TRI_PDV_VALID_ONLINE";

            try
            {
                fbConn.Open();
                fbComm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(RetornarMensagemErro(ex, true));
                return;
            }
            finally
            {
                fbConn.Close();
            }

            MessageBox.Show("Done.");
        }

    }
}

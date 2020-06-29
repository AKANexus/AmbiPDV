using FirebirdSql.Data.FirebirdClient;
using System;
using System.Windows;
using System.Windows.Controls;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;


namespace PDV_WPF.SWAT
{
    /// <summary>
    /// Interaction logic for RunSQL.xaml
    /// </summary>
    public partial class RunSQL : Page
    {
        public RunSQL()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using FbCommand fbComm = new FbCommand();
            using FbConnection fbConn = new FbConnection();
            string _strConnContingency = MontaStringDeConexao("localhost", localpath);
            string _strConnNetwork = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
            if ((bool)rdb_LocalDB.IsChecked)
            {
                fbConn.ConnectionString = _strConnContingency;
            }
            if ((bool)rdb_Server.IsChecked)
            {
                fbConn.ConnectionString = _strConnNetwork;
            }
            if (!(bool)rdb_LocalDB.IsChecked && !(bool)rdb_Server.IsChecked)
            {
                MessageBox.Show("Selecione a base de dados na qual executar o script.");
                return;
            }
            fbComm.Connection = fbConn;
            fbComm.CommandType = System.Data.CommandType.Text;
            fbComm.CommandText = txb_Command.Text;
            try
            {
                fbConn.Open();
                if (fbComm.CommandText.Contains("DROP"))
                {
                    MessageBox.Show("Comandos DROP não são aceitos no console.");
                    return;
                }
                if (fbComm.CommandText.Contains("SELECT"))
                {
                    if (fbComm.CommandText.Contains("*"))
                    {
                        MessageBox.Show("O console não está preparado para retornar dados tabelares.");
                        return;
                    }

                    txb_Command.Text = fbComm.ExecuteScalar().ToString();
                }
                else
                {
                    fbComm.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                txb_Command.Text = RetornarMensagemErro(ex, true);
            }
            finally
            {
                fbConn.Close();
            }
        }

    }
}

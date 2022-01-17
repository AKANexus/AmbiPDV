using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirebirdSql.Data.FirebirdClient;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;

namespace PDV_WPF.SWAT
{
    /// <summary>
    /// Interaction logic for LimparTabelas.xaml
    /// </summary>
    public partial class LimparTabelas : Page
    {
        public LimparTabelas()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cbb_Tabelas.SelectedIndex == 0) return;
            using FbCommand fbComm = new FbCommand();
            using FbConnection localFbConn = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using FbConnection networkFbConn = new FbConnection {ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG)};
            fbComm.Connection = networkFbConn;
            fbComm.CommandType = System.Data.CommandType.Text;
            fbComm.CommandText = $"DELETE FROM {(string)cbb_Tabelas.SelectedItem}";

            try
            {
                networkFbConn.Open();
                fbComm.Connection = networkFbConn;
                fbComm.ExecuteNonQuery();
                localFbConn.Open();
                fbComm.Connection = localFbConn;
                fbComm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(RetornarMensagemErro(ex, true));
                return;
            }
            finally
            {
                networkFbConn.Close();
                localFbConn.Close();
            }

            MessageBox.Show("Done.");
        }
    }
}

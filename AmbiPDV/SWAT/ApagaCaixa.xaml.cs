using FirebirdSql.Data.FirebirdClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;


namespace PDV_WPF.SWAT
{
    /// <summary>
    /// Interaction logic for FechaCaixa.xaml
    /// </summary>
    public partial class ApagaCaixa : Page
    {
        public ApagaCaixa()
        {
            InitializeComponent();
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (tRI_PDV_CONFIGComboBox.Text != txb_Confirmacao.Text)
            {
                MessageBox.Show("O caixa selecionado e o digitado não conferem.");
                return;
            }
            using FbCommand fbComm = new FbCommand();
            using FbConnection fbConn = new FbConnection();
            string _strConnContingency = MontaStringDeConexao("localhost", localpath);
            string _strConnNetwork = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
            fbConn.ConnectionString = _strConnNetwork;
            fbComm.Connection = fbConn;
            fbComm.CommandType = System.Data.CommandType.Text;
            fbComm.CommandText = "DELETE FROM TRI_PDV_CONFIG WHERE ID_MAC = @IDMAC";
            if (tRI_PDV_CONFIGComboBox.SelectedIndex == -1)
            {
                return;
            }

            fbComm.Parameters.Add("@IDMAC", tRI_PDV_CONFIGComboBox.SelectedValue);
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
            fbComm.CommandText = "DELETE FROM TRI_PDV_AUX_SYNC WHERE NO_CAIXA = @NOCAIXA";
            fbComm.Parameters.Clear();
            fbComm.Parameters.Add("@NOCAIXA", tRI_PDV_CONFIGComboBox.Text);
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
            DataSets.FDBDataSetConfig fDBDataSetConfig = ((DataSets.FDBDataSetConfig)(this.FindResource("fDBDataSetConfig")));
            // Load data into the table TRI_PDV_CONFIG. You can modify this code as needed.
            DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter fDBDataSetConfigTRI_PDV_CONFIGTableAdapter = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter();
            fDBDataSetConfigTRI_PDV_CONFIGTableAdapter.Fill(fDBDataSetConfig.TRI_PDV_CONFIG);
            System.Windows.Data.CollectionViewSource tRI_PDV_CONFIGViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("tRI_PDV_CONFIGViewSource")));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataSets.FDBDataSetConfig fDBDataSetConfig = ((DataSets.FDBDataSetConfig)(this.FindResource("fDBDataSetConfig")));
            // Load data into the table TRI_PDV_CONFIG. You can modify this code as needed.
            DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter fDBDataSetConfigTRI_PDV_CONFIGTableAdapter = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter();
            fDBDataSetConfigTRI_PDV_CONFIGTableAdapter.Fill(fDBDataSetConfig.TRI_PDV_CONFIG);
            System.Windows.Data.CollectionViewSource tRI_PDV_CONFIGViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("tRI_PDV_CONFIGViewSource")));
            tRI_PDV_CONFIGViewSource.View.MoveCurrentToFirst();

            Storyboard blink = FindResource("blink") as Storyboard;
            blink.Begin();
        }
    }
}

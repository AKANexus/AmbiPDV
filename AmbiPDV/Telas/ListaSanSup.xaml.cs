using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for ListaSanSup.xaml
    /// </summary>
    public partial class ListaSanSup : Window
    {
        public ListaSanSup()
        {
            InitializeComponent();
            dtp_DataInicial.SelectedDate = DateTime.Today;
            dtp_DataFinal.SelectedDate = DateTime.Now;
        }

        public void CarregaSangrias(DateTime tsInicio, DateTime tsFim)
        {
            List<SangriaSuprimento> sangriaSuprimentos = new List<SangriaSuprimento>();
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using DataSets.FDBDataSetOperSeedTableAdapters.TRI_PDV_SANSUPTableAdapter tRI_PDV_SANSUPTableAdapter = new DataSets.FDBDataSetOperSeedTableAdapters.TRI_PDV_SANSUPTableAdapter() { Connection = LOCAL_FB_CONN};
            var listaDB = from entry in tRI_PDV_SANSUPTableAdapter.GetData()
                          where entry.TS_OPERACAO >= tsInicio && entry.TS_OPERACAO <= tsFim && entry.ID_CAIXA == NO_CAIXA
                          select entry;
            foreach (var item in listaDB)
            {
                sangriaSuprimentos.Add(new SangriaSuprimento() { ID_CAIXA = item.ID_CAIXA, OPERACAO = item.OPERACAO, TS_OPERACAO = item.TS_OPERACAO, VALOR = item.VALOR });
            }
            dgv_Operacoes.ItemsSource = sangriaSuprimentos;
        }

        private void Row_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PrintSANSUP printSANSUP = new PrintSANSUP();
            printSANSUP.operacao = ((SangriaSuprimento)dgv_Operacoes.SelectedItem).OPERACAOVIEW.ToUpper();
            printSANSUP.valor = ((SangriaSuprimento)dgv_Operacoes.SelectedItem).VALOR;
            printSANSUP.numcaixa = NO_CAIXA.ToString("000");
            printSANSUP.reimpressao = true;
            printSANSUP.IMPRIME();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CarregaSangrias(dtp_DataInicial.SelectedDate.GetValueOrDefault(), dtp_DataFinal.SelectedDate.GetValueOrDefault().AddDays(1));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //e.Handled = true;

                this.Close();
            }
        }
    }

    public class SangriaSuprimento
    {
        public int ID_CAIXA { get; set; }
        public DateTime TS_ABERTURA { get; set; }
        public string OPERACAO { get; set; }
        public string OPERACAOVIEW { 
            get 
            {
                return OPERACAO switch
                {
                    "A" => "Sangria",
                    "U" => "Suprimento",
                    _ => ""
                };
            }
        }
        public decimal VALOR { get; set; }
        public DateTime TS_OPERACAO { get; set; }

    }
}

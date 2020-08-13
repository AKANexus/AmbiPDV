using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Objetos;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for ReimprimeCupons.xaml
    /// </summary>
    public partial class ListaDevolucao : Window
    {
        public ListaDevolucao()
        {
            InitializeComponent();
            dgv_Cupons.ItemsSource = listaVendas;
            dgv_ItensCupom.ItemsSource = listaProdutos;
            dtp_DataInicial.SelectedDate = DateTime.Today;
            dtp_DataFinal.SelectedDate = DateTime.Today;
            PreencheVendas(DateTime.Today, DateTime.Today);
            dgv_Cupons.Focus();
            dgv_Cupons.SelectedIndex = 0;
        }

        public ObservableCollection<ReimpressaoVenda> listaVendas = new ObservableCollection<ReimpressaoVenda>();
        public ObservableCollection<ProdutoDevol> listaProdutos = new ObservableCollection<ProdutoDevol>();
        FbConnection LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };

        private void PreencheVendas(DateTime dt_Inicial, DateTime dt_Final)
        {
            listaVendas.Clear();
            using var Cupons_DT = new DataSets.FDBDataSetVenda.CuponsDataTableDataTable();
            using var Cupons_TA = new DataSets.FDBDataSetVendaTableAdapters.CuponsDataTableAdapter() { Connection = LOCAL_FB_CONN };
            //Cupons_TA.Connection = LOCAL_FB_CONN;
            Cupons_TA.FillByCupons(Cupons_DT, dt_Inicial, dt_Final, NO_CAIXA.ToString());
            foreach (DataSets.FDBDataSetVenda.CuponsDataTableRow cupomRow in Cupons_DT.Rows)
            {
                var cupom = new ReimpressaoVenda()
                {
                    Cliente = cupomRow.NOME,
                    Valor = cupomRow.VLR_PAGTO,
                    Num_Cupom = cupomRow.NF_NUMERO,
                    Status = cupomRow.STATUS,
                    TS_Venda = cupomRow.TS_SAIDA,
                    ID_NFVENDA = cupomRow.ID_NFVENDA,
                    NF_SERIE = cupomRow.NF_SERIE
                };
                listaVendas.Add(cupom);
            }
        }

        private void ListaProdutos(ReimpressaoVenda cupom)
        {
            listaProdutos.Clear();
            using var Itens_DT = new DataSets.FDBDataSetVenda.CupomItensTableDataTable();
            using var Itens_TA = new DataSets.FDBDataSetVendaTableAdapters.CupomItensTableAdapter() { Connection = LOCAL_FB_CONN };
            Itens_TA.FillByNFVenda(Itens_DT, cupom.ID_NFVENDA);
            using var Devol_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_DEVOLTableAdapter() { Connection = LOCAL_FB_CONN };
            foreach (var item in Itens_DT)
            {
                int devolvidos;
                devolvidos = int.Parse((Devol_TA.GetQtdDevolByIDNfvitem(item.ID_NFVITEM) ?? "0").ToString());
                ProdutoDevol produtodevol = new ProdutoDevol()
                {
                    ID_NFVITEM = item.ID_NFVITEM,
                    DESCRICAO = item.DESCRICAO,
                    QUANT_VENDIDA = item.QTD_ITEM,
                    PRECO_VENDA = item.PRC_VENDA,
                    QUANT_DEVOL = devolvidos
                };
                listaProdutos.Add(produtodevol);

            }
            //bool prazo = false;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (dtp_DataInicial.SelectedDate.HasValue && dtp_DataFinal.SelectedDate.HasValue)
            {
                PreencheVendas((DateTime)dtp_DataInicial.SelectedDate, (DateTime)dtp_DataFinal.SelectedDate);
            }
        }


        //private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (!(dgv_Cupons.SelectedItem is null))
        //    {
        //        ListaProdutos((ReimpressaoVenda)dgv_Cupons.SelectedItem);
        //    }
        //}

        private void Row_DoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            if (!(dgv_ItensCupom.SelectedItem is null))
            {
                ProdutoDevol produtoEscolhido = (ProdutoDevol)dgv_ItensCupom.SelectedItem;
                if (produtoEscolhido.QUANT_DEVOL == produtoEscolhido.QUANT_VENDIDA)
                {
                    DialogBox.Show("Deolução de Itens", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Não há item a ser devolvido.");
                    return;
                }
                PerguntaQuantidade pq = new PerguntaQuantidade();
                if (pq.ShowDialog() == true)
                {
                    int quantidadeADevolver = int.Parse(pq.quantidadeDigitada);
                    if (quantidadeADevolver > (produtoEscolhido.QUANT_VENDIDA - produtoEscolhido.QUANT_DEVOL))
                    {
                        DialogBox.Show("Deolução de Itens", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Foi digitado um valor inválido.");
                        return;
                    }
                    using var Devol_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_DEVOLTableAdapter() { Connection = LOCAL_FB_CONN };
                    Devol_TA.Insert(-1, produtoEscolhido.ID_NFVITEM, produtoEscolhido.PRECO_VENDA * quantidadeADevolver, "N", DateTime.Now, null, quantidadeADevolver);
                    int idDevol = (int)Devol_TA.PegaUltimaDevolucaoPorIDNFVItem(produtoEscolhido.ID_NFVITEM);
                    PrintDEVOL.IMPRIME(idDevol, produtoEscolhido.PRECO_VENDA * quantidadeADevolver);

                    if (!(dgv_Cupons.SelectedItem is null))
                    {
                        ListaProdutos((ReimpressaoVenda)dgv_Cupons.SelectedItem);
                    }
                }
            }
        }

        private void Row_DoubleClick(object sender, RoutedEventArgs e)
        {

        }

        private void listaVendasSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!(dgv_Cupons.SelectedItem is null))
            {
                ListaProdutos((ReimpressaoVenda)e.AddedItems[0]);
            }

        }
    }
}

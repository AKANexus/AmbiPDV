using System;
using System.Collections.Generic;
using System.Windows;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;


namespace PDV_WPF
{
    public class Pedido
    {
        //public List<decimal> Quantidades = new List<decimal>();
        //public List<int> Codigos = new List<int>();

        public List<Pedido_Produto> produtos { get; set; }
        public int no_pedido { get; set; }

        public Pedido()
        {
            produtos = new List<Pedido_Produto>();// instancia uma lista da classe Pedido_produto
        }

        public bool LePedidoProdutos(int pedidos)
        {
            #region Função Desativada 
            //// SELECT ID_MAIT_PEDIDO_ITEM, ID_MAIT_PEDIDO, ID_IDENTIFICADOR, QTD_ITEM, OBSERVACAO, TS_EMISSAO
            ////   FROM TRI_MAIT_PEDIDO_ITEM;


            ////using (var PEDIDO_ITEM_PROD_TA = new DataSets.FDBDataSetMaitreTableAdapters.TRI_MAIT_PEDIDO_ITEMTableAdapter())
            ////using (var PEDIDO_ITEM_PROD_TB = new DataSets.FDBDataSetMaitre.TRI_MAIT_PEDIDO_ITEMDataTable())
            //using (var connPedidoItem = new FbConnection())
            //using (FbCommand commPedidoItem = new FbCommand())
            //using (DataTable dtPedidoItem = new DataTable())
            //{
            //    try
            //    {
            //        //PEDIDO_ITEM_PROD_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);

            //        connPedidoItem.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);

            //        connPedidoItem.Open();

            //        commPedidoItem.Connection = connPedidoItem;
            //        commPedidoItem.CommandText = "";
            //        commPedidoItem.CommandType = CommandType.Text;
            //        commPedidoItem.Parameters.Add();


            //        PEDIDO_ITEM_PROD_TA.FillByPedidoId(PEDIDO_ITEM_PROD_TB, pedidos);
            //        if (PEDIDO_ITEM_PROD_TB.Rows.Count > 0)
            //        {
            //            foreach (DataRow row in PEDIDO_ITEM_PROD_TB)
            //            {
            //                audit("Quantidade adicionada: " + Convert.ToDecimal(row["QTD_ITEM"]));
            //                audit("Código adicionado: " + Convert.ToInt32(row["ID_IDENTIFICADOR"]));
            //                var Produto = new Pedido_Produto
            //                {
            //                    ID_IDENTIFICADOR = Convert.ToInt32(row["ID_IDENTIFICADOR"]),
            //                    ID_MAIT_PEDIDO = Convert.ToInt32(row["ID_MAIT_PEDIDO"]),
            //                    ID_MAIT_PEDIDO_ITEM = Convert.ToInt32(row["ID_MAIT_PEDIDO_ITEM"]),
            //                    OBSERVACAO = row["OBSERVACAO"].ToString(),
            //                    QTD_ITEM = Convert.ToDecimal(row["QTD_ITEM"]),
            //                    TS_EMISSAO = (DateTime)row["TS_EMISSAO"] //TODO: testar //TODO: buscar o preço de venda
            //                };
            //                produtos.Add(Produto);
            //            }
            //        }
            //        else { return false; }
            //    }
            //    catch (Exception ex)
            //    {
            //        gravarMensagemErro(RetornarMensagemErro(ex, true));
            //        MessageBox.Show(ex.Message);
            //        return false;
            //    }

            //}
            #endregion
            return true;
        }

        public bool LePedido(int pedidos)
        {
            // SELECT ID_MAIT_PEDIDO, TS_EMISSAO, ID_USER, NR_PEDIDO, ABERTO, OBSERVACAO, ID_CAIXA
            //   FROM TRI_MAIT_PEDIDO;

            using (var PEDIDO_TA = new DataSets.FDBDataSetMaitreTableAdapters.TRI_MAIT_PEDIDOTableAdapter())
            using (var PEDIDO_TB = new DataSets.FDBDataSetMaitre.TRI_MAIT_PEDIDODataTable())
            {
                try
                {
                    PEDIDO_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);

                    PEDIDO_TA.FillByPedidoIdStatus(PEDIDO_TB, pedidos, "S");

                    if (PEDIDO_TB.Rows.Count <= 0) { return false; }

                    no_pedido = PEDIDO_TB[0]["ID_MAIT_PEDIDO"].Safeint(); // só pra saber se o pedido é valido....
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
            return true;
        }
        public void Clear()
        {
            produtos.Clear();
        }

    }
    public class Pedido_Produto
    {
        public int ID_MAIT_PEDIDO_ITEM { get; set; }
        public int ID_MAIT_PEDIDO { get; set; }
        public int ID_IDENTIFICADOR { get; set; }
        public decimal QTD_ITEM { get; set; }
        public string OBSERVACAO { get; set; }
        public DateTime TS_EMISSAO { get; set; }
    }
}


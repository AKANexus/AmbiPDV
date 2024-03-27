using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.REMENDOOOOO;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using PDV_WPF.Controls;

namespace PDV_WPF
{
    public class Orcamento
    {
        //public List<decimal> Quantidades = new List<decimal>();
        //public List<int> Codigos = new List<int>();

        public List<Orcamento_Produto> produtos { get; set; }
        public int no_orcamento { get; set; }
        public int Cod_Cliente { get; set; }
        public DateTime dt_emissao { get; set; }
        public int cod_transportadora { get; set; }
        public DateTime dt_validade { get; set; }
        public string pagamento { get; set; }
        public int id_user { get; set; }
        public decimal valor_total { get; set; }
        public decimal desconto_total { get; set; }
        public DateTime dt_entrega { get; set; }
        public decimal valor_tot { get; set; }
        public DateTime dt_vencimento { get; set; }
        public string endereco { get; set; }
        public string numero { get; set; }
        public string cidade { get; set; }
        public string estado { get; set; }
        public string tel1 { get; set; }
        public string tel2 { get; set; }
        public string tel3 { get; set; }
        public string cep { get; set; }
        public string origem { get; set; }

        public Orcamento()
        {
            //ESTE ABSURDO DEVE-SE....   // NORRRRRRRRMAAAAAL
            produtos = new List<Orcamento_Produto>();// Instância uma nova lista da classe Orçamento_produto 
        }

        public bool LeOrcaProdutos(int orcamento, string origemImportacao)
        {
            switch (origemImportacao)
            {
                case "DavsClipp":                    
                    try
                    {
                        using (var PEDIDO_PROD_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_PED_VENDA_ITEMTableAdapter())
                        using (var PEDIDO_PROD_TB = new DataSets.FDBDataSetOperSeed.TB_PED_VENDA_ITEMDataTable())
                        {
                            PEDIDO_PROD_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
                            PEDIDO_PROD_TA.FillByIdPedido(dataTable: PEDIDO_PROD_TB, ID_PEDIDO: orcamento);
                            if (PEDIDO_PROD_TB.Rows.Count > 0)
                            {
                                int numProduto = 1;
                                foreach (var item in PEDIDO_PROD_TB)
                                {
                                    numProduto++;

                                    var Produto = new Orcamento_Produto
                                    {
                                        ID_PRODUTO = item.ID_ITEMPED,
                                        ID_ESTOQUE = item.ID_IDENTIFICADOR,
                                        QUANT = item.QTD_ITEM,
                                        VALOR = item.VLR_UNIT,
                                        DESCONTO = item.VLR_DESC,
                                        VALOR_TOT = item.VLR_TOTAL,
                                        ID_ORCAMENTO = item.ID_PEDIDO,
                                        NUM_PRODUTO = item.IsID_SEQUENCIA_DAVNull() ? numProduto : item.ID_SEQUENCIA_DAV
                                    };
                                    produtos.Add(Produto);
                                }
                            }
                            else return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        logErroAntigo(RetornarMensagemErro(ex, true));                        
                        MessageBox.Show($"Erro ao pescar itens do pedido.\n\n{ex.InnerException.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);                        
                        throw;
                    }
                    break;                
                case "AmbiOrcamento":
                    using (var ORCA_PROD_TA = new DataSets.FDBDataSetOrcamTableAdapters.TRI_ORCA_PRODUTOSTableAdapter())
                    using (var ORCA_PROD_TB = new DataSets.FDBDataSetOrcam.TRI_ORCA_PRODUTOSDataTable())
                    {
                        try
                        {
                            ORCA_PROD_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);

                            ORCA_PROD_TA.FillByOrca(ORCA_PROD_TB, PID_ORCAMENTO: orcamento);
                            if (ORCA_PROD_TB.Rows.Count > 0)
                            {
                                foreach (DataRow row in ORCA_PROD_TB)
                                {
                                    var Produto = new Orcamento_Produto
                                    {
                                        ID_PRODUTO = Convert.ToInt32(row["ID_PRODUTO"]),
                                        ID_ESTOQUE = Convert.ToInt32(row["ID_ESTOQUE"]),
                                        QUANT = Convert.ToDecimal(row["QUANT"]),
                                        VALOR = Convert.ToDecimal(row["VALOR"]),
                                        DESCONTO = Convert.ToDecimal(row["DESCONTO"]),
                                        VALOR_TOT = Convert.ToDecimal(row["VALOR_TOT"]),
                                        ID_ORCAMENTO = Convert.ToInt32(row["ID_ORCAMENTO"]),
                                        NUM_PRODUTO = Convert.ToInt32(row["NUM_PRODUTO"])
                                    };
                                    produtos.Add(Produto);
                                }
                            }
                            else { return false; }
                        }
                        catch (Exception ex)
                        {                            
                            logErroAntigo(RetornarMensagemErro(ex, true));
                            MessageBox.Show($"Erro ao pescar itens do orçamento.\n\n{ex.InnerException.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            throw;
                        }
                    }
                    break;
            }
            return true;
        }

        public bool LeOrcamento(int orcamento, string origemImportacao)
        {
            switch (origemImportacao)
            {
                case "DavsClipp":
                    using (var PEDIDO_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_PEDIDO_VENDATableAdapter())
                    using (var PEDIDO_TB = new DataSets.FDBDataSetOperSeed.TB_PEDIDO_VENDADataTable())
                    {
                        try
                        {
                            PEDIDO_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
                            PEDIDO_TA.FillById(PEDIDO_TB, ID_PEDIDO: orcamento);

                            if (PEDIDO_TB.Rows.Count <= 0) return false;

                            no_orcamento = PEDIDO_TB[0]["ID_PEDIDO"].Safeint();
                            Cod_Cliente = PEDIDO_TB[0]["ID_CLIENTE"].Safeint();
                            origem = origemImportacao;
                        }
                        catch (Exception ex)
                        {
                            logErroAntigo(RetornarMensagemErro(ex, true));
                            MessageBox.Show($"Erro ao pescar pedido.\n\n{ex.InnerException.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                    break;                
                case "AmbiOrcamento":
                    using (var ORCA_TA = new DataSets.FDBDataSetOrcamTableAdapters.TRI_ORCA_ORCAMENTOSTableAdapter())
                    using (var ORCA_TB = new DataSets.FDBDataSetOrcam.TRI_ORCA_ORCAMENTOSDataTable())
                    {
                        try
                        {
                            ORCA_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
                            ORCA_TA.FillByOrcaStatus(ORCA_TB, PID_ORCAMENTO: orcamento, PSTATUS: "SALVO");

                            if (ORCA_TB.Rows.Count <= 0) return false;

                            no_orcamento = ORCA_TB[0]["ID_ORCAMENTO"].Safeint();
                            Cod_Cliente = ORCA_TB[0]["ID_CLIENTE"].Safeint();
                            origem = origemImportacao;
                        }
                        catch (Exception ex)
                        {
                            logErroAntigo(RetornarMensagemErro(ex, true));
                            MessageBox.Show($"Erro ao pescar orçamento.\n\n{ex.InnerException.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }        
        public void Clear()
        {
            produtos.Clear();
        }

    }
    public class Orcamento_Produto
    {
        public int ID_PRODUTO { get; set; }
        public int ID_ESTOQUE { get; set; }
        public decimal QUANT { get; set; }
        public decimal VALOR { get; set; }
        public decimal DESCONTO { get; set; }
        public decimal VALOR_TOT { get; set; }
        public int ID_ORCAMENTO { get; set; }
        public int NUM_PRODUTO { get; set; }
    }
}


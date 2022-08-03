using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.REMENDOOOOO;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;


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

        public Orcamento()
        {
            //ESTE ABSURDO DEVE-SE....   // NORRRRRRRRMAAAAAL
            produtos = new List<Orcamento_Produto>();// Instância uma nova lista da classe Orçamento_produto 
        }

        public bool LeOrcaProdutos(int orcamentos)
        {
            using (var ORCA_PROD_TA = new DataSets.FDBDataSetOrcamTableAdapters.TRI_ORCA_PRODUTOSTableAdapter())
            using (var ORCA_PROD_TB = new DataSets.FDBDataSetOrcam.TRI_ORCA_PRODUTOSDataTable())
            {
                try
                {
                    ORCA_PROD_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);

                    ORCA_PROD_TA.FillByOrca(ORCA_PROD_TB, orcamentos);
                    if (ORCA_PROD_TB.Rows.Count > 0)
                    {
                        foreach (DataRow row in ORCA_PROD_TB)
                        {
                            //audit("LEORCAMENTOPRODUTO", "Quantidade adicionada: " + Convert.ToDecimal(row["QUANT"]));
                            //audit("LEORCAMENTOPRODUTO", "Código adicionado: " + Convert.ToInt32(row["ID_ESTOQUE"])); //TODO: APESAR DE SE CHAMAR ID_ESTOQUE, O VALOR REAL É O ID_IDENTIFICADOR!
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
                    MessageBox.Show(ex.Message);
                    return false;
                }

            }
            return true;
        }

        public bool LeOrcamento(int orcamentos)
        {
            //using (var ORCA_TA = new DataSets.FDBDataSetOrcamTableAdapters.TRI_ORCA_ORCAMENTOSTableAdapter())
            //using (var INFO_TA = new DataSets.FDBDataSetOrcamTableAdapters.TRI_ORCA_INFOTableAdapter())
            using (var ORCA_TA = new DataSets.FDBDataSetOrcamTableAdapters.TRI_ORCA_ORCAMENTOSTableAdapter())
            using (var ORCA_TB = new DataSets.FDBDataSetOrcam.TRI_ORCA_ORCAMENTOSDataTable())
            {
                try
                {
                    ORCA_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);

                    ORCA_TA.FillByOrcaStatus(ORCA_TB, orcamentos, "SALVO");

                    if (ORCA_TB.Rows.Count <= 0) { return false; }

                    //DataRow INFORMACAO_DR = INFO_TA.GetInfoByOrcamento(orcamentos).Rows[0];
                    no_orcamento = ORCA_TB[0]["ID_ORCAMENTO"].Safeint();
                    Cod_Cliente = ORCA_TB[0]["ID_CLIENTE"].Safeint();
                    //dt_emissao = (DateTime)ORCAMENTO_DR["DT_EMISSAO"];
                    //cod_transportadora = ORCAMENTO_DR["COD_TRANSPORTADORA"].Safeint();
                    //dt_validade = (DateTime)ORCAMENTO_DR["DT_VALIDADE"];
                    //pagamento = ORCAMENTO_DR["PAGAMENTO"].Safestring();
                    //id_user = ORCAMENTO_DR["ID_USER"].Safeint();
                    //valor_total = (decimal)ORCAMENTO_DR["VALOR_TOTAL"];
                    //desconto_total = (decimal)ORCAMENTO_DR["DESCONTO_TOTAL"];
                    //valor_tot = (decimal)ORCAMENTO_DR["VALOR_TOT"];
                    //dt_vencimento = (DateTime)ORCAMENTO_DR["DT_VENCIMENTO"];
                    //endereco = INFORMACAO_DR["ENDERECO"].Safestring();
                    //numero = INFORMACAO_DR["NUMERO"].Safestring();
                    //cidade = INFORMACAO_DR["CIDADE"].Safestring();
                    //estado = INFORMACAO_DR["ESTADO"].Safestring();
                    //tel1 = INFORMACAO_DR["TEL1"].Safestring();
                    //tel2 = INFORMACAO_DR["TEL2"].Safestring();
                    //tel3 = INFORMACAO_DR["TEL3"].Safestring();
                    //cep = INFORMACAO_DR["CEP"].Safestring();
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


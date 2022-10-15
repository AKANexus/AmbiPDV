using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CfeRecepcao_0008;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Funcoes;
using static PDV_WPF.Funcoes.Statics;

namespace SaidaDeRemessa
{
    public class Remessa : PDV_WPF.Objetos.Venda
    {
        public override (int NF_NUMERO, int ID_NFVENDA) GravaNaoFiscalBase(decimal vTroco, int noCaixa, short idVendedor, bool ECF = false)
        {
            CFe cfeDeRetorno = RetornaCFe();
            int NF_NUMERO, nItemCup, ID_NFVENDA, ID_CLIENTE = 0;
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            using (var OPER_TA = new PDV_WPF.DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
            using (var VENDA_TA = new PDV_WPF.DataSets.FDBDataSetVendaTableAdapters.SP_TRI_GRAVANFVENDATableAdapter())
            {
                OPER_TA.Connection = LOCAL_FB_CONN;
                VENDA_TA.Connection = LOCAL_FB_CONN;
                try
                {
                    decimal totItem = 0;
                    foreach (var detalhamento in cfeDeRetorno.infCFe.det)
                    {
                        totItem += decimal.Parse(detalhamento.prod.vUnCom, ptBR) * decimal.Parse(detalhamento.prod.qCom, ptBR);
                    }
                    DateTime tsEmissao = DateTime.Now;
                    OPER_TA.Connection = LOCAL_FB_CONN;
                    PDV_WPF.DataSets.FDBDataSetVenda.SP_TRI_GRAVANFVENDARow nFVendaRow;
                    nFVendaRow = VENDA_TA.SP_TRI_GRAVANFVENDA(idVendedor, "N" + noCaixa.ToString(), tsEmissao, tsEmissao, 2, vTroco, -1)[0];
                    ID_NFVENDA = nFVendaRow.RID_NFVENDA;
                    NF_NUMERO = nFVendaRow.RNF_NUMERO;
                    //log.Debug($"ID_NFVENDA = (int)OPER_TA.SP_TRI_GRAVANFVENDA(0, \"1\", {tsEmissao}, {tsEmissao}, 2, {vTroco});");
                }
                catch (Exception ex)
                {
                    //log.Error("Falha ao gravar venda não fiscal na base", ex);
                    //MessageBox.Show("Erro ao gravar cupom de venda. \nSe o problema persistir, entre em contato com a equipe de suporte.");
                    return (-1, -1);
                }
                foreach (envCFeCFeInfCFeDet detalhamento in cfeDeRetorno.infCFe.det)
                {
                    int ID_NFV_ITEM;
                    nItemCup = int.Parse(detalhamento.nItem);
                    try
                    {
                        string pCSOSN = null;
                        ID_NFV_ITEM = (int)OPER_TA.SP_TRI_GRAVANFVITEM(ID_NFVENDA,
                            Convert.ToInt32(detalhamento.prod.cProd),
                            Convert.ToInt16(detalhamento.nItem),
                            Convert.ToDecimal(detalhamento.prod.qCom, ptBR),
                            Convert.ToDecimal(detalhamento.prod.vDesc, ptBR) + Convert.ToDecimal(detalhamento.prod.vRatDesc, ptBR),
                            pCSOSN, 0, 0, Convert.ToDecimal(detalhamento.prod.vUnCom, ptBR));
                    }
                    catch (Exception ex)
                    {
                        //log.Error("Falha ao gravar venda não fiscal", ex);
                        //DialogBox.Show(strings.VENDA,
                        //               DialogBoxButtons.No,
                        //               DialogBoxIcons.Error, true,
                        //               strings.ERRO_DURANTE_VENDA,
                        //               RetornarMensagemErro(ex, false));
                        return (-1, -1);
                    }
                    if (nItemCup <= 0)
                    {
                        throw new Exception("O ID de retorno do item de cupom é menor ou igual a zero: " + nItemCup.ToString());
                    }
                }
                OPER_TA.SP_TRI_ATUALIZANFVENDA(ID_NFVENDA, ID_CLIENTE);
            }
            return (NF_NUMERO, ID_NFVENDA);
        }
    }
}

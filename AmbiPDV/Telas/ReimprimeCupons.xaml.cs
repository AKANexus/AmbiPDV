using CfeRecepcao_0007;
using DeclaracoesDllSat;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Objetos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for ReimprimeCupons.xaml
    /// </summary>
    public partial class ReimprimeCupons : Window
    {
        public ReimprimeCupons()
        {
            InitializeComponent();
            dgv_Cupons.ItemsSource = listaVendas;
            dtp_DataInicial.SelectedDate = DateTime.Today;
            dtp_DataFinal.SelectedDate = DateTime.Today;
            PreencheVendas(DateTime.Today, DateTime.Today);
            dgv_Cupons.Focus();
            dgv_Cupons.SelectedIndex = 0;
        }

        public ObservableCollection<ReimpressaoVenda> listaVendas = new ObservableCollection<ReimpressaoVenda>();

        FbConnection LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
        private void PreencheVendas(DateTime dt_Inicial, DateTime dt_Final)
        {
            listaVendas.Clear();
            using var Cupons_DT = new DataSets.FDBDataSetVenda.CuponsDataTableDataTable();
            using var Cupons_TA = new DataSets.FDBDataSetVendaTableAdapters.CuponsDataTableAdapter();
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

        private void ReimprimeCupom(ReimpressaoVenda cupom)
        {
            if (cupom.NF_SERIE.Contains("E") || cupom.NF_SERIE.Contains("N"))
            {
                using var Itens_DT = new DataSets.FDBDataSetVenda.CupomItensTableDataTable();
                using var Pagtos_DT = new DataSets.FDBDataSetVenda.CupomPgtosTableDataTable();
                using var Itens_TA = new DataSets.FDBDataSetVendaTableAdapters.CupomItensTableAdapter();
                using var Pagtos_TA = new DataSets.FDBDataSetVendaTableAdapters.CupomPgtosTableAdapter();
                Itens_TA.FillByNFVenda(Itens_DT, cupom.ID_NFVENDA);
                Pagtos_TA.FillByNFVenda(Pagtos_DT, cupom.ID_NFVENDA);

                #region Converte NF em F

                string CNPJSH = "22141365000179";
                string CNPJdaVenda = Emitente.CNPJ;
                string IEdaVenda = Emitente.IE;
                if (SIGN_AC.Contains("RETAGUARDA"))
                {
                    switch (MODELO_SAT)
                    {
                        case ModeloSAT.NENHUM:
                            break;
                        case ModeloSAT.DARUMA:
                            break;
                        case ModeloSAT.DIMEP:
                            CNPJSH = "16716114000172";
                            CNPJdaVenda = "61099008000141";
                            IEdaVenda = "111111111111";
                            break;
                        case ModeloSAT.BEMATECH:
                            break;
                        case ModeloSAT.ELGIN:
                            break;
                        case ModeloSAT.SWEDA:
                            break;
                        case ModeloSAT.CONTROLID:
                            CNPJSH = "16716114000172";
                            CNPJdaVenda = "08238299000129";
                            IEdaVenda = "149392863111";
                            break;
                        case ModeloSAT.TANCA:
                            break;
                        case ModeloSAT.EMULADOR:
                            break;
                        default:
                            break;
                    }
                }
                Venda venda = new Venda();
                venda.AbrirNovaVenda(CNPJSH, SIGN_AC, NO_CAIXA.ToString(), CNPJdaVenda.TiraPont(), IEdaVenda.TiraPont(), Emitente.IM);
                using var dadosDoItem = new DataSets.FDBDataSetOperSeed.SP_TRI_OBTEMDADOSDOITEMDataTable();
                using var obtemdadosdoitem = new DataSets.FDBDataSetOperSeedTableAdapters.SP_TRI_OBTEMDADOSDOITEMTableAdapter();
                using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                string unidadeMedida;
                foreach (var produto in Itens_DT)
                {

                    obtemdadosdoitem.Connection = LOCAL_FB_CONN;
                    obtemdadosdoitem.Fill(dadosDoItem, produto.ID_IDENTIFICADOR);
                    unidadeMedida = dadosDoItem.Rows[0][dadosDoItem.UNI_MEDIDAColumn].ToString();
                    venda.RecebeNovoProduto(
                        produto.ID_IDENTIFICADOR,
                        produto.DESCRICAO,
                        dadosDoItem[0].IsCOD_NCMNull() ? "" : dadosDoItem[0].COD_NCM,
                        dadosDoItem[0].CFOP,
                        produto.PRC_VENDA,
                        dadosDoItem[0].IsRSTR_CESTNull() ? "" : dadosDoItem[0].RSTR_CEST,
                        0,
                        produto.VLR_DESC,
                        produto.UNI_MEDIDA,
                        produto.QTD_ITEM,
                        dadosDoItem[0].IsCOD_BARRANull() ? "" : dadosDoItem[0].COD_BARRA
                        );
                    if (dadosDoItem[0].RID_TIPOITEM == "")
                    {
                        venda.RecebeInfoISSQN(dadosDoItem[0].RALIQ_ISS);
                    }
                    else
                    {
                        venda.RecebeInfoICMS(
                            tipoDeEmpresa,
                            dadosDoItem[0].IsRCSOSN_CFENull() ? "" : dadosDoItem[0].RCSOSN_CFE.Trim(),
                            dadosDoItem[0].IsRCST_CFENull() ? "" : dadosDoItem[0].RCST_CFE,
                            dadosDoItem[0].RUF_SP.ToString(),
                            dadosDoItem[0].RBASE_ICMS.ToString()
                            );
                    }
                    venda.RecebePIS(
                        dadosDoItem[0].IsRCST_PISNull() ? "" : dadosDoItem[0].RCST_PIS,
                        produto.PRC_VENDA,
                        (dadosDoItem[0].IsRPISNull()) ? 0M : dadosDoItem[0].RPIS,
                        produto.QTD_ITEM
                        );
                    venda.RecebeCOFINS(
                        dadosDoItem[0].IsRCST_COFINSNull() ? "" : dadosDoItem[0].RCST_COFINS,
                        produto.PRC_VENDA,
                        (dadosDoItem[0].IsRCOFINSNull()) ? 0M : dadosDoItem[0].RCOFINS,
                        produto.QTD_ITEM
                        );

                    venda.AdicionaProduto(dadosDoItem[0].IsRCST_CFENull() ? "" : dadosDoItem[0].RCST_CFE);
                }
                foreach (var pagamento in Pagtos_DT)
                {

                }

                #endregion Converte NF em F


                VendaDEMO.numerodocupom = cupom.Num_Cupom;
                VendaDEMO.operadorStr = "REIMPRESSÃO";
                VendaDEMO.num_caixa = int.Parse(cupom.NF_SERIE.Replace("E", "").Replace("N", ""));
                foreach (var item in Itens_DT)
                {
                    VendaDEMO.RecebeProduto(item.ID_IDENTIFICADOR.ToString(), item.DESCRICAO, item.UNI_MEDIDA, item.QTD_ITEM, item.PRC_VENDA, 0, 0, 0, 0, item.PRC_VENDA);
                }
                bool prazo = false;
                foreach (var item in Pagtos_DT)
                {
                    VendaDEMO.RecebePagamento(item.DESCRICAO, item.VLR_PAGTO);
                    if (!item.IsDT_VENCTONull())
                    {
                        VendaDEMO.cliente = item.NOME;
                        VendaDEMO.vencimento = item.DT_VENCTO;
                        VendaDEMO.valor_prazo = item.VLR_PAGTO;
                        VendaDEMO.TsOperacao = cupom.TS_Venda;
                        prazo = true;
                    }
                }
                switch (prazo)
                {
                    case true:
                        VendaDEMO.IMPRIME(1, new CFe());
                        VendaDEMO.IMPRIME(1, null);
                        break;
                    default:
                        VendaDEMO.IMPRIME(0, new CFe());
                        break;
                }
            }
            else
            {
                string chave;
                using (var SAT_DT = new DataSets.FDBDataSetVenda.TB_SATDataTable())
                using (var SAT_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_SATTableAdapter())
                {
                    SAT_TA.FillByIdNfvenda(SAT_DT, cupom.ID_NFVENDA);
                    chave = SAT_DT[0].CHAVE;
                }
                if (File.Exists($@"SAT\Vendas\AD{chave}.xml"))
                    ReimprimeXML(chave);
                return;
            }
        }

        private bool ReimprimeXML(string chave)
        {
            var serializer = new XmlSerializer(typeof(CFe));
            var _metodos_de_pagamento = new Dictionary<string, string>
                        {
                            { "01", "Dinheiro" },
                            { "02", "Cheque" },
                            { "03", "Cartão Crédito" },
                            { "04", "Cartão Débito" },
                            { "05", "Crédito Loja" },
                            { "10", "Vale Aliment." },
                            { "11", "Vale Refeição" },
                            { "12", "PIX" },
                            { "13", "Vale Combustível" },
                            { "99", "Outros" }
                        };//Dicionário de métodos de pagamento.
            var cFeDeRetorno = new CFe();
            using (var XmlRetorno = new StringReader(File.ReadAllText($@"SAT\Vendas\AD{chave}.xml")))
            using (var xreader = XmlReader.Create(XmlRetorno))
            {
                cFeDeRetorno = (CFe)serializer.Deserialize(xreader);
            }
            string chavecfe = cFeDeRetorno.infCFe.ide.cUF +
                              DateTime.Now.ToString("yyMM") +
                              cFeDeRetorno.infCFe.emit.CNPJ +
                              cFeDeRetorno.infCFe.ide.mod +
                              cFeDeRetorno.infCFe.ide.nserieSAT +
                              cFeDeRetorno.infCFe.ide.nCFe +
                              cFeDeRetorno.infCFe.ide.cNF +
                              cFeDeRetorno.infCFe.ide.cDV;
            string id_dest = "NÃO DECLARADO";
            int.TryParse(cFeDeRetorno.infCFe.ide.nCFe, out int nCFe);
            VendaImpressa.numerodocupom = nCFe;
            string valorcfe = cFeDeRetorno.infCFe.total.vCFe;


            VendaImpressa.vendedor = "Reimpressão";


            var Funcoes = new funcoesClass();
            decimal _qCom, _vUnCom, _vDesc;
            foreach (envCFeCFeInfCFeDet item in cFeDeRetorno.infCFe.det)
            {
                _qCom = decimal.Parse(item.prod.qCom, CultureInfo.InvariantCulture);
                _vUnCom = decimal.Parse(item.prod.vUnCom, CultureInfo.InvariantCulture);
                _vDesc = decimal.Parse(string.IsNullOrWhiteSpace(item.prod.vDesc) ? "0" : item.prod.vDesc, CultureInfo.InvariantCulture);

                if (!string.IsNullOrWhiteSpace(item.prod.NCM))
                {
                    Funcoes.ConsultarTaxasPorNCM(item.prod.NCM, out decimal taxa_fed, out decimal taxa_est, out decimal taxa_mun);

                    VendaImpressa.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, taxa_est, taxa_fed, taxa_mun, decimal.Parse(item.prod.vUnComOri));
                }
                else
                {
                    VendaImpressa.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, 0, 0, 0, _vUnCom);
                }

            }

            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {

                //int result_contarec = 0;
                //short result_movdiario = 0;
                string strMensagemLogLancaContaRec = string.Empty;
                string strMensagemLogLancaMovDiario = string.Empty;
                //var _vMP = 0m;
                decimal valor_prazo = 0;

                using (var CONTAREC_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_CONTA_RECEBERTableAdapter())
                using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                {
                    OPER_TA.Connection = LOCAL_FB_CONN;
                    CONTAREC_TA.Connection = LOCAL_FB_CONN;

                    foreach (envCFeCFeInfCFePgtoMP item in cFeDeRetorno.infCFe.pgto.MP)
                    {
                        VendaImpressa.RecebePagamento(_metodos_de_pagamento[item.cMP.ToString()], item.dec_vMP);
                        if (item.cMP == "05")
                        {
                            valor_prazo = item.dec_vMP;
                        }
                    }
                }
                VendaImpressa.chavenfe = chavecfe;
                VendaImpressa.TsOperacao =
                    DateTime.ParseExact(cFeDeRetorno.infCFe.ide.dEmi + cFeDeRetorno.infCFe.ide.hEmi, "yyyyMMddhhmmss",
                        ptBR);
                VendaImpressa.assinaturaQRCODE = chavecfe + "|" +
                                         cFeDeRetorno.infCFe.ide.dEmi + cFeDeRetorno.infCFe.ide.hEmi + "|" +
                                         valorcfe + "|" +
                                         id_dest + "|" +
                                         cFeDeRetorno.infCFe.ide.assinaturaQRCODE;
                //if (!(cFeDeRetorno.infCFe.infAdic.))
                //VendaImpressa.desconto = pFechamento.desconto;
                if (!(cFeDeRetorno.infCFe.pgto.vTroco is null))
                {
                    VendaImpressa.troco = cFeDeRetorno.infCFe.pgto.vTroco.Replace(".", ",");
                }
                else
                {
                    VendaImpressa.troco = "0,00";
                }
                if (!(cFeDeRetorno.infCFe.infAdic is null) && !(cFeDeRetorno.infCFe.infAdic.obsFisco is null))
                {
                    VendaImpressa.observacaoFisco = (cFeDeRetorno.infCFe.infAdic.obsFisco[0].xCampo, cFeDeRetorno.infCFe.infAdic.obsFisco[0].xTexto);
                }
                //try
                //{
                //    if (vendaAtual.imprimeViaAssinar)
                //    {
                //        VendaImpressa.cliente = pFechamento.nome_cliente;
                //        VendaImpressa.vencimento = pFechamento.vencimento;
                //        VendaImpressa.valor_prazo = valor_prazo;
                //        venda_prazo += 1;
                //    }
                //}
                //catch (Exception erro)
                //{
                //    gravarMensagemErro(RetornarMensagemErro(erro, true));
                //    DialogBox.Show(strings.IMPRESSAO,
                //                   strings.ERRO_AO_ENVIAR_IMPRESSAO_DO_TEF_PARA_SPOOLER,
                //                   RetornarMensagemErro(erro, false),
                //                   DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Warn, true);
                //    return false;
                //}

            }
            VendaImpressa.IMPRIME(0, cFeDeRetorno);
            return true;
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

        private void sP_TRI_LISTAFECHAMENTOSDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(dgv_Cupons.SelectedItem is null))
            {
                ReimprimeCupom((ReimpressaoVenda)dgv_Cupons.SelectedItem);
            }
        }


    }
}

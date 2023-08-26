using CfeRecepcao_0008;
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
using PDV_WPF.Funcoes;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;
using PDV_WPF.DataSets;
using System.Data;
using System.Text;
using PDV_WPF.DataSets.FDBDataSetVendaTableAdapters;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;
using System.Linq;
using PDV_WPF.Properties;
using System.Runtime.InteropServices;
using Clearcove.Logging;
using PDV_WPF.FDBDataSetTableAdapters;
using System.Threading;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for ReimprimeCupons.xaml
    /// </summary>
    public partial class ReimprimeCupons : Window
    {
        private Logger log = new("Reimpressão");

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
            //Cupons_TA.FillByCupons(Cupons_DT, dt_Inicial, dt_Final, NO_CAIXA.ToString());            
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

        private void ReimprimeCupom(ReimpressaoVenda cupom, bool gerarCFe = false)
        {
            using var Itens_DT = new DataSets.FDBDataSetVenda.CupomItensTableDataTable();
            using var Pagtos_DT = new DataSets.FDBDataSetVenda.CupomPgtosTableDataTable();
            using var Itens_TA = new DataSets.FDBDataSetVendaTableAdapters.CupomItensTableAdapter();
            using var Pagtos_TA = new DataSets.FDBDataSetVendaTableAdapters.CupomPgtosTableAdapter();
            Itens_TA.FillByNFVenda(Itens_DT, cupom.ID_NFVENDA);
            Pagtos_TA.FillByNFVenda(Pagtos_DT, cupom.ID_NFVENDA);

            if (cupom.NF_SERIE.Contains("E") || cupom.NF_SERIE.Contains("N"))
            {                                               
                #region Converte NF em F

                string CNPJSH = "30737989000181";
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

                if (venda.RetornaCFe().infCFe.pgto is null)
                {
                    //using var InfoPagtosDT = new FDBDataSetVenda.OutrasInfoPagtoTableDataTable();
                    //using (var InfoPagtosTA = new OutrasInfoPagtoTableAdapter())
                    //{
                    //    InfoPagtosTA.FillByInfoAdicionaisPagtos(InfoPagtosDT, cupom.ID_NFVENDA);
                    //    InfoPagtosDT.ToList().ForEach(pagamentos =>
                    //    {
                    //        venda.RecebePagamento(pagamentos.ID_NFCE, pagamentos.VLR_PAGTO, pagamentos.ID_ADMINISTRADORA, pagamentos.VLR_TROCO);
                    //    });
                    //    venda.InformaCliente(ItemChoiceType.CPF, null);
                    //    venda.TotalizaCupom();
                    //}
                }

                #endregion Converte NF em F


                if (gerarCFe) 
                    if (GerarCFe(venda.RetornaCFe(), cupom))
                    {
                        DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.CFE_CONVERTIDO);
                    }
                    else return;

                VendaDEMO.numerodocupom = cupom.Num_Cupom;
                VendaDEMO.operadorStr = "REIMPRESSÃO";
                VendaDEMO.TsOperacao = cupom.TS_Venda;
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
                    ReimprimeXML(chave, Pagtos_DT, cupom);
                return;
            }
        }

        private bool ReimprimeXML(string chave, FDBDataSetVenda.CupomPgtosTableDataTable pagtos_Dt, ReimpressaoVenda cupom)
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
            decimal _qCom = 1, _vUnCom = 1, _vDesc = 0;
            foreach (envCFeCFeInfCFeDet item in cFeDeRetorno.infCFe.det)
            {

                decimal.TryParse(item.prod.qCom, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _qCom);
                decimal.TryParse(item.prod.vUnCom, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _vUnCom);
                decimal.TryParse(string.IsNullOrWhiteSpace(item.prod.vDesc) ? "0" : item.prod.vDesc, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _vDesc);

                if (!string.IsNullOrWhiteSpace(item.prod.NCM))
                {
                    Funcoes.ConsultarTaxasPorNCM(item.prod.NCM, out decimal taxa_fed, out decimal taxa_est, out decimal taxa_mun);


                    decimal _vUnComOri;
                    decimal.TryParse(item.prod.vUnComOri, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _vUnComOri);
                    VendaImpressa.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, taxa_est, taxa_fed, taxa_mun, _vUnComOri);

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
                        string vMPXML = item.vMP.Replace(".", ",");
                        decimal vMP = Convert.ToDecimal(vMPXML);
                        VendaImpressa.RecebePagamento(_metodos_de_pagamento[item.cMP.ToString()], vMP);
                        if (item.cMP == "05")
                        {
                            valor_prazo = item.dec_vMP;                           
                        }
                    }
                }
                VendaImpressa.chavenfe = chavecfe;
                VendaImpressa.TsOperacao =
                    DateTime.ParseExact(cFeDeRetorno.infCFe.ide.dEmi + cFeDeRetorno.infCFe.ide.hEmi, "yyyyMMddHHmmss",
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
                if (!(cFeDeRetorno.infCFe.infAdic is null) && !(cFeDeRetorno.infCFe.obsFisco is null))
                {
                    VendaImpressa.observacaoFisco = (cFeDeRetorno.infCFe.obsFisco[0].xCampo, cFeDeRetorno.infCFe.obsFisco[0].xTexto);
                }                
                //try
                //{
                //    if (vendaAtual.imprimeViaAssinar)
                //    {
                //        VendaImpressa.cliente =  pFechamento.nome_cliente;
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
            bool prazo = false;
            foreach (var item in pagtos_Dt)
            {
                //VendaDEMO.RecebePagamento(item.DESCRICAO, item.VLR_PAGTO);
                if (!item.IsDT_VENCTONull())
                {
                    VendaImpressa.cliente = item.NOME;
                    VendaImpressa.vencimento = item.DT_VENCTO;
                    VendaImpressa.valor_prazo = item.VLR_PAGTO;
                    VendaImpressa.TsOperacao = cupom.TS_Venda;
                    prazo = true;
                }
            }
            switch (prazo)
            {
                case true:
                    VendaImpressa.IMPRIME(1, cFeDeRetorno); 
                    VendaImpressa.IMPRIME(1, null);
                    break;
                default:
                    VendaImpressa.IMPRIME(0, cFeDeRetorno);
                    break;
            }            
            return true;
        }

        private bool GerarCFe(CFe cfeVenda, ReimpressaoVenda cupom)
        {
            string _XML_ = "", codigoDeRetorno = "", xmlret = "";
            string[] retorno = null;           
            byte[] bytes = null;
            var serializer = new XmlSerializer(typeof(CFe));

            try
            {
                var settings = new XmlWriterSettings() { Encoding = new UTF8Encoding(true), OmitXmlDeclaration = false, Indent = false };
                var XmlFinal = new StringBuilder();                
                using (var xwriter2 = XmlWriter.Create(XmlFinal, settings))
                {
                    var xns = new XmlSerializerNamespaces();
                    xns.Add(string.Empty, string.Empty);
                    Directory.CreateDirectory(@"SAT_LOG");
                    serializer.Serialize(xwriter2, cfeVenda, xns); //Popula o stringbuilder para ser enviado para o SAT.
                }
                _XML_ = XmlFinal.ToString().Replace(',', '.').Replace("utf-16", "utf-8");
                File.WriteAllText(@"SAT_LOG\NfParaF.xml", _XML_);
                
                //HACK: Trecho pra garantir que o encoding da string (???) seja em UTF-8.
                // Se não executar essa conversão string -> bytes -> string com encoding, pode acontecer o erro de validação 6010|1999|Erro não identificado, com erro de conversão UTF-8.
                // O bug é deflagrado quando a descrição de algum produto contém pelo menos um caracter diacrítico.
                // ---------------------------------------------->>>
                bytes = Encoding.Default.GetBytes(_XML_);
                _XML_ = Encoding.UTF8.GetString(bytes);
            }
            catch(Exception ex)
            {
                log.Error($"Erro ao serializar objeto CFe. Erro --> {ex.InnerException.Message ?? ex.Message} ");
                DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao serializar objeto CF-e");
            }
            // ----------------------------------------------<<<

            if (!SATSERVIDOR)
            {
                try
                {
                    Declaracoes_DllSat.sRetorno = Marshal.PtrToStringAnsi(Declaracoes_DllSat.EnviarDadosVenda(new NumSessao().GeraNumero(), SAT_CODATIV, _XML_, MODELO_SAT));
                    var arraydebytes = Encoding.Default.GetBytes(Declaracoes_DllSat.sRetorno);
                    string sRetorno = Encoding.UTF8.GetString(arraydebytes);
                    retorno = sRetorno.Split('|');
                    codigoDeRetorno = retorno.Length > 1 ? retorno[1] : "06099";
                }

                catch (Exception ex)
                {
                    log.Error("Erro ao enviar dados para a venda", ex);
                    DialogBox.Show(strings.CFE,
                                   DialogBoxButtons.No, DialogBoxIcons.Error, false,
                                   strings.VERIFIQUE_AS_LUZES_DO_SAT);
                }
            }
            else
            {
                try
                {
                    decimal attemptSatServidor = 1; StartSearchSatServidor:
                    using (var SAT_ENV_TA = new TRI_PDV_SAT_ENVTableAdapter())
                    {
                        SAT_ENV_TA.SP_TRI_ENVIA_SAT_SERVIDOR(NO_CAIXA, bytes);
                    }

                    var sb = new SATBox("Operação no SAT", $"Aguarde a resposta do SAT. . .                 Tentativa: {attemptSatServidor}");
                    sb.ShowDialog();
                    if (sb.DialogResult == false)
                    {
                        log.Debug($"Tentativa de envio SatServidor falhou. Tentando novamente... tentativa: {attemptSatServidor}");
                        using (var SAT_REC_TA = new FDBDataSetTableAdapters.TRI_PDV_SAT_RECTableAdapter()) { SAT_REC_TA.DeleteAll(); }
                        using (var SAT_ENV_TA = new TRI_PDV_SAT_ENVTableAdapter()) { SAT_ENV_TA.DeleteAll(); }
                        if (attemptSatServidor < 3)
                        {
                            Thread.Sleep(1500);
                            attemptSatServidor++;
                                goto StartSearchSatServidor;
                        }
                        log.Debug("Após 3 tentativas SatServiddor falhou em todas, segue a vida.");
                        DialogBox.Show(strings.SAT_SERVIDOR, DialogBoxButtons.No, DialogBoxIcons.Error, false, strings.ERRO_SAT_SERVIDOR);                        
                        return false;
                    }
                    else
                    {
                        retorno = sb.retorno;
                        codigoDeRetorno = retorno.Length > 1 ? retorno[1] : "06099";
                    }

                }
                catch (Exception ex)
                {
                    log.Error("Erro ao enviar XML para o Sat Servidor. Erro --> ", ex);
                    DialogBox.Show(strings.CFE,
                                   DialogBoxButtons.No, DialogBoxIcons.Error, false,
                                   "Erro ao enviar a venda para o servidor SAT");
                    return false;
                }
            }
            if (retorno.Length < 2)
            {
                log.Debug($"Retorno do SAT era invalido. Retorno --> {string.Join(" | ", retorno)}");
                DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, strings.ERRO_GENERICO_SAT);
                return false;
            }
            if (codigoDeRetorno != "06000")
            {
                log.Debug($"Erro ao enviar venda para o SAT. Còdigo do retorno --> {codigoDeRetorno}. Retorno --> {string.Join(" | ", retorno)}");
                DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, codigoDeRetorno switch
                {
                    "06001" => "Código de ativação inválido.",
                    "06002" => "SAT ainda não ativado.",
                    "06003" => "SAT ainda não vinculado ao Aplicativo Comercial.",
                    "06004" => "Vinculação do AC não confere.",
                    "06005" => "Tamanho do CF-e-SAT superior a 1500 KB.",
                    "06006" => "SAT bloqueado pelo contribuinte.",
                    "06007" => "SAT bloqueado pela SEFAZ.",
                    "06008" => "SAT bloqueado por falta de comunicação.",
                    "06009" => "SAT temporariamente bloqueado. Número de tentativas ultrapassado.",
                    "06010" => $"Erro de validação de conteúdo:\n{retorno[3]}",
                    "06098" => "SAT ocupado, aguarde para tentar novamente.",
                    _ => $"ERRO DESCONHECIDO. Ligue para (11) 4304-7778 e informe erro {retorno[1]}",
                });
                return false;
            }

            //---------> QUANDO RETORNO 06000 <---------
            try
            {
                xmlret = Encoding.UTF8.GetString(Convert.FromBase64String(retorno[6].ToString()));
                CFe cFeDeRetorno;

                using (var XmlRetorno = new StringReader(xmlret))
                using (var xreader = XmlReader.Create(XmlRetorno))
                {
                    cFeDeRetorno = (CFe)serializer.Deserialize(xreader);
                }

                for (int i = 0; i < cFeDeRetorno.infCFe.det.Length; i++)
                {
                    cFeDeRetorno.infCFe.det[i].prod.vUnComOri = cfeVenda.infCFe.det[i].prod.vUnComOri;
                }

                if (cFeDeRetorno is not null)
                {                    
                    AtualizaInfoNaBase(cFeDeRetorno, cupom);
                }
                
                //TODO: Após atualizar informações na base de dados será chamado o metodo de impressão via XML passando os parametros necessarios. 
            }
            catch (Exception ex)
            {
                log.Error($"Erro ao desserializar CFe de retorno no objeto CFe. Erro --> {ex.InnerException.Message ?? ex.Message}");
                DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao desserializar objeto CF-e");
            }

            return true;
        }

        private bool AtualizaInfoNaBase (CFe CFeRetorno, ReimpressaoVenda cupom)
        {
            try
            {
                using (var TB_NFVENDA_TA = new TB_NFVENDATableAdapter() { Connection = new FbConnection {ConnectionString = MontaStringDeConexao("localhost", localpath)} })
                {                    
                    //TB_NFVENDA_TA.UpdateNfToF(CFeRetorno.infCFe.ide.nCFe.Replace("0", string.Empty),
                    //                          CFeRetorno.infCFe.ide.numeroCaixa.Replace("0", string.Empty),
                    //                          DateTime.ParseExact(CFeRetorno.infCFe.ide.dEmi, "yyyyMMdd", CultureInfo.InvariantCulture),
                    //                          DateTime.ParseExact(CFeRetorno.infCFe.ide.hEmi, "HHmmss", CultureInfo.InvariantCulture),
                    //                          "", "", "");  //TODO: Ajustar aqui pra pegar do cupom convertido.                  
                    return true;
                };
            }
            catch (Exception ex)
            {
                log.Error($"Erro ao atualizar informações para base de dados. Erro: --> {ex.InnerException.Message ?? ex.Message}");
                DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao atualizar informações no banco de dados");
                return false;
            }            
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
                ReimprimeCupom((ReimpressaoVenda)dgv_Cupons.SelectedItem, false);
            }
        }

        private void geraCfe_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Gerando CF-e aguarde...");
            ReimprimeCupom((ReimpressaoVenda)dgv_Cupons.SelectedItem, true);
        }
    }
}

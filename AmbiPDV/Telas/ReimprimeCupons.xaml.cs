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
using System.Threading.Tasks;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for ReimprimeCupons.xaml
    /// </summary>
    public partial class ReimprimeCupons : Window
    {
        private Logger log = new("Reimpressão");
        private LoadingProccess loadingProccess;
        private bool converteuCupom;

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
                    NF_SERIE = cupomRow.NF_SERIE,
                    NF_MODELO = cupomRow.NF_MODELO
                };
                listaVendas.Add(cupom);
            }
        }

        private async void ReimprimeCupom(ReimpressaoVenda cupom, bool gerarCFe = false)
        {
            if (cupom.Status is "C") DialogBox.Show(strings.CUPOM_CANCELADO, DialogBoxButtons.No, DialogBoxIcons.Warn, false, strings.VENDA_CANCELADA);

            loadingProccess = new();
            loadingProccess.Show();
            this.IsEnabled = false;
            
            await Task.Run(() =>
            {
                loadingProccess.progress.Report("Carregando venda");

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
                        using var InfoPagtosDT = new FDBDataSetVenda.OutrasInfoPagtoTableDataTable();
                        using (var InfoPagtosTA = new OutrasInfoPagtoTableAdapter())
                        {
                            InfoPagtosTA.FillByInfoAdicionaisPagtos(InfoPagtosDT, cupom.ID_NFVENDA);                            
                            foreach(var pagamentos in InfoPagtosDT)
                            {
                                venda.RecebePagamento(pagamentos.ID_NFCE, pagamentos.VLR_PAGTO, pagamentos.IsID_ADMINISTRADORANull() ? 0 : pagamentos.ID_ADMINISTRADORA, pagamentos.VLR_TROCO);
                            }
                            venda.InformaCliente(ItemChoiceType.CPF, null);
                            venda.TotalizaCupom();
                        }
                    }

                    #endregion Converte NF em F

                    if (gerarCFe)
                        if (GerarCFe(venda.RetornaCFe(), cupom, Pagtos_DT))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                this.IsEnabled = true;                                
                                loadingProccess.Close();
                                DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.CFE_CONVERTIDO);                                
                                converteuCupom = true;
                                this.Close();                                
                            });
                            return;
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

                    loadingProccess.progress.Report("Imprimindo");
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
                    loadingProccess.progress.Report("Carregando venda");
                    string chave;
                    using (var SAT_DT = new DataSets.FDBDataSetVenda.TB_SATDataTable())
                    using (var SAT_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_SATTableAdapter())
                    {
                        SAT_TA.FillByIdNfvenda(SAT_DT, cupom.ID_NFVENDA);
                        chave = SAT_DT[0].CHAVE;
                    }
                    if (File.Exists($@"SAT\Vendas\AD{chave}.xml"))
                    {
                        loadingProccess.progress.Report("Imprimindo");
                        ReimprimeXML(chave, Pagtos_DT, cupom);
                    }                        
                    return;
                }
            });

            await Task.Delay(1000);
            this.IsEnabled = true;
            loadingProccess.Close();
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

        private bool GerarCFe(CFe cfeVenda, ReimpressaoVenda cupom, FDBDataSetVenda.CupomPgtosTableDataTable pagtos_Dt)
        {
            loadingProccess.progress.Report("Gerando CF-e");

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
                Dispatcher.Invoke(() =>
                {
                    this.IsEnabled = true;
                    loadingProccess.Close();
                    DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao serializar objeto CF-e");
                    this.Focus();
                });
                return false;
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
                    Dispatcher.Invoke(() =>
                    {
                        this.IsEnabled = true;
                        loadingProccess.Close();
                        DialogBox.Show(strings.CFE,
                                  DialogBoxButtons.No, DialogBoxIcons.Error, false,
                                  strings.VERIFIQUE_AS_LUZES_DO_SAT);
                        this.Focus();
                    });
                    return false;
                }
            }
            else
            {                
                bool comunicouSatServidor = true;
                Dispatcher.Invoke(() =>
                {
                    loadingProccess.Hide();
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
                                attemptSatServidor++;
                                    goto StartSearchSatServidor;
                            }
                            log.Debug("Após 3 tentativas SatServiddor falhou em todas, segue a vida.");
                            DialogBox.Show(strings.SAT_SERVIDOR, DialogBoxButtons.No, DialogBoxIcons.Error, false, strings.ERRO_SAT_SERVIDOR);
                            loadingProccess.Close();
                            this.IsEnabled = true;
                            this.Focus();
                            comunicouSatServidor = false;
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
                        loadingProccess.Close();
                        this.IsEnabled = true;
                        this.Focus();
                        comunicouSatServidor = false;
                    }
                });

                if (!comunicouSatServidor) return false;              
            }
            
            if (retorno.Length < 2)
            {
                log.Debug($"Retorno do SAT era invalido. Retorno --> {string.Join(" | ", retorno)}");
                Dispatcher.Invoke(() =>
                {
                    this.IsEnabled = true;
                    loadingProccess.Close();                    
                    DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, strings.ERRO_GENERICO_SAT);
                    this.Focus();
                });                
                return false;
            }
            if (codigoDeRetorno != "06000")
            {
                log.Debug($"Erro ao enviar venda para o SAT. Còdigo do retorno --> {codigoDeRetorno}. Retorno --> {string.Join(" | ", retorno)}");
                Dispatcher.Invoke(() =>
                {
                    this.IsEnabled = true;
                    loadingProccess.Close();  
                    
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
                });                
                return false;
            }

            //-------------------------> QUANDO RETORNO 06000 <-------------------------

            if (!loadingProccess.IsVisible) Dispatcher.Invoke(() => loadingProccess.Show() );
            
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
                    string chave = cFeDeRetorno.infCFe.Id.Replace("CFe", string.Empty);
                    Directory.CreateDirectory(@"SAT\Vendas");
                    File.WriteAllText($@"SAT\Vendas\AD{chave}.xml", xmlret);

                    loadingProccess.progress.Report("Salvando CF-e");
                    if (!AtualizaInfoNaBase(cFeDeRetorno, cupom))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            this.IsEnabled = true;
                            loadingProccess.Close();
                            DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao atualizar informações no banco de dados");
                            this.Focus();
                        });
                        return false;
                    }

                    loadingProccess.progress.Report("Imprimindo");
                    if (!ReimprimeXML(chave, pagtos_Dt, cupom))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            this.IsEnabled = true;
                            loadingProccess.Close();
                            DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao imprimir cupom fiscal convertido");
                            this.Focus();
                        });
                        return false;
                    }
                    return true;
                }                                
            }
            catch (Exception ex)
            {
                log.Error($"Erro ao desserializar CFe de retorno no objeto CFe. Erro --> {ex.InnerException.Message ?? ex.Message}");
                Dispatcher.Invoke(() =>
                {
                    this.IsEnabled = true;
                    loadingProccess.Close();
                    DialogBox.Show(strings.CFE, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao desserializar objeto CF-e");
                    this.Focus();
                });                                
                return false;
            }
            return false;
        }

        private bool AtualizaInfoNaBase(CFe CFeRetorno, ReimpressaoVenda cupom)
        {
            try
            {
                using (var TB_NFV_ITEM_DT = new FDBDataSetVenda.TB_NFV_ITEMDataTable())
                using (var TB_NFV_ITEM_TA = new TB_NFV_ITEMTableAdapter())
                using (var OPER_TA = new TRI_PDV_OPERTableAdapter())
                using (var TB_NFVENDA_TA = new TB_NFVENDATableAdapter())
                {
                    TB_NFV_ITEM_TA.Connection = OPER_TA.Connection = TB_NFVENDA_TA.Connection = LOCAL_FB_CONN;

                    int idAlteradoPdv = (int)TB_NFVENDA_TA.SP_TRI_UPDATE_NF_TO_F(int.Parse(CFeRetorno.infCFe.ide.nCFe),
                                                                                 CFeRetorno.infCFe.ide.numeroCaixa.TrimStart('0'),
                                                                                 DateTime.ParseExact(CFeRetorno.infCFe.ide.dEmi, "yyyyMMdd", CultureInfo.InvariantCulture),
                                                                                 DateTime.ParseExact(CFeRetorno.infCFe.ide.hEmi, "HHmmss", CultureInfo.InvariantCulture),
                                                                                 3,
                                                                                 cupom.Num_Cupom, cupom.NF_SERIE, cupom.NF_MODELO);

                    TB_NFVENDA_TA.SaveOldInfos($"{cupom.Num_Cupom}|{cupom.NF_SERIE}|{cupom.NF_MODELO}", idAlteradoPdv);

                    OPER_TA.SP_TRI_GRAVASAT(idAlteradoPdv, CFeRetorno.infCFe.Id.Substring(3), int.Parse(CFeRetorno.infCFe.ide.nCFe), CFeRetorno.infCFe.ide.nserieSAT);
                    TB_NFV_ITEM_TA.FillByIdNfvenda(TB_NFV_ITEM_DT, idAlteradoPdv);
                    
                    foreach (var detalhamento in CFeRetorno.infCFe.det)
                    {
                        #region SalvaICMS

                        if (detalhamento.imposto.Item is envCFeCFeInfCFeDetImpostoICMS)
                        {                            
                            envCFeCFeInfCFeDetImpostoICMS iCMS = (envCFeCFeInfCFeDetImpostoICMS)detalhamento.imposto.Item;
                            using var TB_NFV_ITEM_ICMS = new TB_NFV_ITEM_ICMSTableAdapter { Connection = LOCAL_FB_CONN };
                            if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMSSN102 ICMSSN102) //SIMPLES NACIONAL = CSOSN 102, 300, 400, 500 E OUTROS
                            {                                
                                TB_NFV_ITEM_ICMS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, 0, 0, "000", 0, 0);
                            }
                            else if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMSSN900 ICMSSN900) //SIMPLES NACIONAL = CSOSN 900
                            {
                                TB_NFV_ITEM_ICMS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, decimal.Parse(ICMSSN900.vICMS, CultureInfo.InvariantCulture), decimal.Parse(ICMSSN900.pICMS, CultureInfo.InvariantCulture), "000", decimal.Parse(ICMSSN900.pICMS, CultureInfo.InvariantCulture), decimal.Parse(ICMSSN900.vICMS, CultureInfo.InvariantCulture));
                            }
                            else if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMS40 ICMS40) //REGIME NORMAL = CST 40, 41 E 60
                            {
                                TB_NFV_ITEM_ICMS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, 0, 0, ICMS40.Orig + ICMS40.CST, 0, 0);
                            }
                            else if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMS00 ICMS00) //REGIME NORMAL = CST 00, 20 E 90
                            {
                                if (ICMS00.CST is "20") //Redução BC
                                {
                                    int.TryParse(detalhamento.prod.cProd, out int codProd);


                                    using var TaxaProd = new DataSets.FDBDataSetOperSeedTableAdapters.TB_ESTOQUETableAdapter { Connection = LOCAL_FB_CONN };
                                    using var AliqTaxa = new DataSets.FDBDataSetOperSeedTableAdapters.TB_TAXA_UFTableAdapter { Connection = LOCAL_FB_CONN };

                                    var taxa = TaxaProd.TaxaPorID(codProd);
                                    if (taxa is null) taxa = "III";

                                    decimal ALIQ_ICMS = Convert.ToDecimal(AliqTaxa.AliqPorID(taxa.ToString()), CultureInfo.InvariantCulture);
                                    decimal POR_BC_ICMS = Convert.ToDecimal(AliqTaxa.BCPorID(taxa.ToString()), CultureInfo.InvariantCulture);

                                    decimal.TryParse(detalhamento.prod.vProd.Replace('.', ','), out decimal vProd);
                                    decimal VLR_BC_ICMS = Math.Round(POR_BC_ICMS / 100 * vProd, 2);

                                    TB_NFV_ITEM_ICMS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, 
                                                            VLR_BC_ICMS, 
                                                            POR_BC_ICMS, 
                                                            ICMS00.Orig + ICMS00.CST, 
                                                            ALIQ_ICMS,
                                                            decimal.Parse(ICMS00.vICMS, CultureInfo.InvariantCulture));
                                }
                                else //Cobrado integralmente
                                {
                                    TB_NFV_ITEM_ICMS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM,
                                                            decimal.Parse(detalhamento.prod.vItem, CultureInfo.InvariantCulture), 
                                                            100, 
                                                            ICMS00.Orig + ICMS00.CST, 
                                                            decimal.Parse(ICMS00.pICMS, CultureInfo.InvariantCulture), 
                                                            decimal.Parse(ICMS00.vICMS, CultureInfo.InvariantCulture));
                                }
                            }
                        }

                        #endregion SalvaICMS

                        #region SalvaPISCOFINS

                        if (detalhamento.imposto.COFINS is not null)
                        {
                            using var TB_NFV_ITEM_COFINS = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_COFINSTableAdapter()
                            {
                                Connection = LOCAL_FB_CONN
                            };
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSAliq COFINSAliq) //CST 01, 02 e 05
                            {
                                TB_NFV_ITEM_COFINS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, 100, COFINSAliq.CST, decimal.Parse(COFINSAliq.pCOFINS, CultureInfo.InvariantCulture) * 100, decimal.Parse(COFINSAliq.vCOFINS, CultureInfo.InvariantCulture), decimal.Parse(COFINSAliq.vBC, CultureInfo.InvariantCulture));
                            }
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSNT COFINSNT) //CST 04, 06, 07, 08 e 09
                            {
                                TB_NFV_ITEM_COFINS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, 0, COFINSNT.CST, 0, 0, 0);
                            }
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSOutr COFINSOutr) //CST 99
                            {
                                TB_NFV_ITEM_COFINS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, 100, COFINSOutr.CST, decimal.Parse(COFINSOutr.Items[1], CultureInfo.InvariantCulture) * 100, decimal.Parse(COFINSOutr.vCOFINS, CultureInfo.InvariantCulture), decimal.Parse(COFINSOutr.Items[0], CultureInfo.InvariantCulture));
                            }
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSQtde COFINSQtde)
                            {

                            }
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSSN COFINSSN) //CST 49
                            {
                                TB_NFV_ITEM_COFINS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, 0, COFINSSN.CST, 0, 0, 0);
                            }
                        }

                        if (detalhamento.imposto.PIS is not null)
                        {
                            using var TB_NFV_ITEM_PIS = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_PISTableAdapter()
                            {
                                Connection = LOCAL_FB_CONN
                            };
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISAliq PISAliq) //CST 01, 02 E 05
                            {
                                TB_NFV_ITEM_PIS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, PISAliq.CST, 100, decimal.Parse(PISAliq.pPIS, CultureInfo.InvariantCulture) * 100, decimal.Parse(PISAliq.vPIS, CultureInfo.InvariantCulture), decimal.Parse(PISAliq.vBC, CultureInfo.InvariantCulture));
                            }
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISNT PISNT) //CST 04, 06, 07, 08 E 09
                            {
                                TB_NFV_ITEM_PIS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, PISNT.CST, 0, 0, 0, 0);
                            }
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISOutr PISOutr) //CST 99
                            {
                                TB_NFV_ITEM_PIS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, PISOutr.CST, 100, decimal.Parse(PISOutr.Items[1], CultureInfo.InvariantCulture) * 100, decimal.Parse(PISOutr.vPIS, CultureInfo.InvariantCulture), decimal.Parse(PISOutr.Items[0], CultureInfo.InvariantCulture));
                            }
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISQtde PISQtde)
                            {

                            }
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISSN PISSN) //CST 49
                            {
                                TB_NFV_ITEM_PIS.Insert(TB_NFV_ITEM_DT.First(currentTable => currentTable.NUM_ITEM == int.Parse(detalhamento.nItem)).ID_NFVITEM, PISSN.CST, 0, 0, 0, 0);
                            }
                        }

                        #endregion SalvaPISCOFINS
                    }
                    return true;
                };
            }
            catch (Exception ex)
            {                
                log.Error($"Erro ao atualizar informações para base de dados. Erro: --> {ex.InnerException.Message ?? ex.Message}");
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
            ReimprimeCupom((ReimpressaoVenda)dgv_Cupons.SelectedItem, true);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = converteuCupom ? true : false;
        }
    }
}

using CfeCancelamento_0008;
using Clearcove.Logging;
using DeclaracoesDllSat;
using FirebirdSql.Data.FirebirdClient;
using LocalDarumaFrameworkDLL;
using PDV_WPF.FDBDataSetTableAdapters;
using PDV_WPF.Funcoes;
using PDV_WPF.Objetos;
using PDV_WPF.Properties;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for ListaCupons.xaml
    /// </summary>
    public partial class List : Window
    {
        //private int _numCaixa;
        private string _infoStr;
        private NumSessao _ns = new NumSessao();
        Logger log = new Logger("Lista Cupons");
        public List()
        {
            InitializeComponent();
            PreencheDataGrid();
            tRI_PDV_SAT_XMLDataGrid.Focus();
            tRI_PDV_SAT_XMLDataGrid.SelectedIndex = 0;
            if (ECF_ATIVA) lbl_Instrucoes.Content = "Apenas o último cupom lançado pode ser cancelado.\n Os cupons anteriores são apresentados apenas para consulta.";
        }

        public ObservableCollection<CupomSAT> CupomSATCollection = new ObservableCollection<CupomSAT>();

        private void PreencheDataGrid()
        {
            CupomSATCollection.Clear();

            //using (var XML_TA = new FDBDataSetTableAdapters.TRI_PDV_SAT_XMLTableAdapter())
            //using (var XML_DT = new FDBDataSet.TRI_PDV_SAT_XMLDataTable())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            using (var V_TRI_CUPONSCANCELAVEIS_TA = new DataSets.FDBDataSetVendaTableAdapters.V_TRI_CUPONSCANCELAVEISTableAdapter())
            using (var V_TRI_CUPONSCANVELAVEIS_DT = new DataSets.FDBDataSetVenda.V_TRI_CUPONSCANCELAVEISDataTable())
            {
                V_TRI_CUPONSCANCELAVEIS_TA.Connection = LOCAL_FB_CONN;
                //XML_TA.FillByCancelaveis(XML_DT);
                //XML_TA.FillByCancelaveis(XML_DT, _numCaixa);
                V_TRI_CUPONSCANCELAVEIS_TA.Fill(V_TRI_CUPONSCANVELAVEIS_DT);
                foreach (DataSets.FDBDataSetVenda.V_TRI_CUPONSCANCELAVEISRow item in V_TRI_CUPONSCANVELAVEIS_DT.Rows)
                {
                    if (int.Parse(Regex.Match(item.NF_SERIE, @"\d+").Value, NumberFormatInfo.InvariantInfo) == NO_CAIXA)
                    {
                        var cFE = new CupomSAT()
                        {
                            VALOR_TOTAL = item.VALOR_TOTAL,
                            TS_VENDA = item.TS_VENDA,
                            CHAVE_CFE = item.CHAVE ?? "BETERRABA",
                            //CANCEL_CFE = item.CANCEL_CFE.ToCharArray()[0],
                            HoraLancamento = item.TS_VENDA,
                            NF_SERIE = item.NF_SERIE,
                            NUM_CAIXA = int.Parse(Regex.Match(item.NF_SERIE, @"\d+").Value, NumberFormatInfo.InvariantInfo),
                            ID_NFVENDA = item.ID_NFVENDA,
                            ID_REGISTRO = item.ID_REGISTRO
                            //XML = item.XML
                        };
                        CupomSATCollection.Insert(0, cFE);
                    }
                }
            }
            tRI_PDV_SAT_XMLDataGrid.ItemsSource = CupomSATCollection;
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (tRI_PDV_SAT_XMLDataGrid.SelectedItem != null)
            {
                if (ECF_ATIVA && tRI_PDV_SAT_XMLDataGrid.SelectedIndex != tRI_PDV_SAT_XMLDataGrid.Items.Count - 1)
                {
                    MessageBox.Show("Apenas o último cupom ECF pode ser cancelado"); return;
                }
                CancelarCupomSelecionado();
            }

        }


        private void CancelarCupomSelecionado()
        {
            if (((CupomSAT)tRI_PDV_SAT_XMLDataGrid.SelectedItem).TS_VENDA.AddMinutes(30) <= DateTime.Now)
            {
                MessageBox.Show("Cupom já passou dos 30 minutos permitidos. Não é permitido cancelamento extemporâneo pelo SAT");
                return;
            }
            try
            {
                using (var TB_NFVENDA_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter())
                {
                    if (((CupomSAT)tRI_PDV_SAT_XMLDataGrid.SelectedItem).NF_SERIE.StartsWith("N"))
                    {
                        CancelarNaoFiscal((CupomSAT)tRI_PDV_SAT_XMLDataGrid.SelectedItem);
                    }
                    else if (((CupomSAT)tRI_PDV_SAT_XMLDataGrid.SelectedItem).NF_SERIE.StartsWith("E"))
                    {
                        CancelarECF((CupomSAT)tRI_PDV_SAT_XMLDataGrid.SelectedItem);
                    }
                    else
                    {
                        if (SATSERVIDOR)
                        {
                            CancelarSatServidorPelaBase((CupomSAT)tRI_PDV_SAT_XMLDataGrid.SelectedItem);
                        }
                        else
                        {
                            CancelarSatLocalPelaBase((CupomSAT)tRI_PDV_SAT_XMLDataGrid.SelectedItem);
                        }

                    }
                }
                PreencheDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void CancelarNaoFiscal(CupomSAT cupomSAT)
        {
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                PrintDEMOCANCL.operador = operador.Split(' ')[0];
                PrintDEMOCANCL.assinaturaQRCODE = "QR Code Inválido";
            }
            using (var TB_NFVENDA = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                TB_NFVENDA.Connection = LOCAL_FB_CONN;
                TB_NFVENDA.SetaCanceladoPorIDNFVenda(cupomSAT.ID_NFVENDA);
            }

            try
            {
                using var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter();
                using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                using var TB_NFVENDA_FMAPAGTO = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDA_FMAPAGTO_NFCETableAdapter();
                OPER_TA.Connection = LOCAL_FB_CONN;
                TB_NFVENDA_FMAPAGTO.Connection = LOCAL_FB_CONN;
                PrintDEMOCANCL.numerodoextrato = cupomSAT.ID_NFVENDA;
                log.Debug("CANCELAULTIMOCUPOM: " + cupomSAT.ID_NFVENDA);
                //decimal total = TB_NFVENDA_FMAPAGTO.PagamentosDoCupom(cupomSAT.ID_NFVENDA) ?? 0;
                decimal total = cupomSAT.VALOR_TOTAL;
                log.Debug($"Total Cancelado: {total}");
                PrintDEMOCANCL.total = total;
            }
            catch (Exception ex)
            {
                log.Error("Erro ao cancelar não fiscal", ex);
                DialogBox.Show(strings.CANCELAMENTO_DE_VENDA,
                               DialogBoxButtons.No, DialogBoxIcons.Error, false,
                               strings.NAO_FOI_POSSIVEL_CANCELAR_CUPOM,
                               RetornarMensagemErro(ex, false));
                return;
            }
            try
            {
                PrintDEMOCANCL.IMPRIME((int)MODELO_CUPOM);
            }
            catch (Exception ex)
            {
                log.Error("Erro ao imprimir cupom de cancelamento não fiscal", ex);
                DialogBox.Show(strings.CANCELAMENTO_DE_VENDA,
                               DialogBoxButtons.No, DialogBoxIcons.Error, false, strings.NAO_FOI_POSSIVEL_IMPRIMIR_CANCELAMENTO,
                               RetornarMensagemErro(ex, false));
                return;
            }
        }


        private void CancelarECF(CupomSAT cupomSAT)
        {
            int intRetornoCancelarEcfDaruma = UnsafeNativeMethods.iCFCancelar_ECF_Daruma();
            int intRetornarErroEcfDaruma = UnsafeNativeMethods.eRetornarErro_ECF_Daruma();
            if (intRetornoCancelarEcfDaruma != 1 || intRetornarErroEcfDaruma != 0)
            {
                string strMensagemErro;
                if (intRetornarErroEcfDaruma == 122)
                {
                    strMensagemErro = "Apenas o último cupom ECF emitido pode ser cancelado.";
                }
                else strMensagemErro = "Erro ao cancelar cupom fiscal (ECF): " + intRetornoCancelarEcfDaruma.ToString() + "-" + intRetornarErroEcfDaruma.ToString();
                log.Debug($"ERRO na ECF ao Cancelar: {intRetornoCancelarEcfDaruma}-{intRetornarErroEcfDaruma}");
                MessageBox.Show(strMensagemErro + "\n\nPor favor entre em contato com a equipe de suporte.");
                return;
            }
            using (var EMITENTE_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EMITENTETableAdapter())
            using (var EMITENTE_DT = new DataSets.FDBDataSetOperSeed.TB_EMITENTEDataTable())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                EMITENTE_TA.Connection = LOCAL_FB_CONN;
                EMITENTE_TA.Fill(EMITENTE_DT);
            }
            using (var TB_NFVENDA = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                TB_NFVENDA.Connection = LOCAL_FB_CONN;
                TB_NFVENDA.SetaCanceladoPorIDNFVenda(cupomSAT.ID_NFVENDA);
            }


        }


        /// <summary>
        /// Gera e manda o cancelamento do último cupom fiscal para o SAT.
        /// </summary>
        /// <param name="pCupomInterno">ID_CUPOM do cupom a ser cancelado</param>
        private void CancelarSatLocalPelaBase(CupomSAT cupomSAT)
        {
            PrintCANCL printCANCL = new PrintCANCL();
            string XML;
            using (Cancelamento canclAtual = new Cancelamento())
            { XML = canclAtual.GeraXMLdeCancelamentoFiscal(cupomSAT.CHAVE_CFE, SIGN_AC, NO_CAIXA.ToString("D3"), CNPJSH); }
            File.WriteAllText(@"SAT_LOG\UltimaoCanc.xml", XML);
            bool erro_canc = true;
            string m_erro_canc = "";
            bool _processado_ = false;
            string _xmlret_canc = "";
            while (_processado_ == false)
            {
                log.Debug("CancelarUltimaVenda");
                Declaracoes_DllSat.sRetorno = Marshal.PtrToStringAnsi(Declaracoes_DllSat.CancelarUltimaVenda(_ns.GeraNumero(), SAT_CODATIV, "CFe" + cupomSAT.CHAVE_CFE, XML, MODELO_SAT));
                string[] retorno_canc = Declaracoes_DllSat.sRetorno.Split('|');
                log.Debug($"sRetorno obtido: {retorno_canc[1]}");
                switch (retorno_canc[1])
                {
                    case "07000":
                        _xmlret_canc = Encoding.UTF8.GetString(Convert.FromBase64String(retorno_canc[6].ToString()));
                        _processado_ = true;
                        erro_canc = false;
                        break;
                    case "07001":
                        m_erro_canc = "Código de ativação inválido.";
                        _processado_ = true;
                        break;
                    case "07002":
                        m_erro_canc = "07002 - Cupom inválido.";
                        _processado_ = true;
                        break;
                    case "07003":
                        m_erro_canc = "SAT bloqueado pelo contribuinte.";
                        _processado_ = true;
                        break;
                    case "07004":
                        m_erro_canc = "SAT bloqueado pela SEFAZ.";
                        _processado_ = true;
                        break;
                    case "07005":
                        m_erro_canc = "SAT bloqueado por falta de comunicação.";
                        _processado_ = true;
                        break;
                    case "07006":
                        m_erro_canc = "SAT temporariamente bloqueado. Número de tentativas ultrapassado.";
                        _processado_ = true;
                        break;
                    case "07007":
                        m_erro_canc = "Erro de validação de conteúdo. Último cupom pode já ter sido cancelado, ou o tempo limite ultrapassado.";
                        _processado_ = true;
                        break;
                    case "07098":
                        m_erro_canc = "SAT ocupado, aguarde para tentar novamente.";
                        _processado_ = true;
                        break;
                    case "07099":
                        m_erro_canc = "ERRO DESCONHECIDO. Ligue para (11) 4304-7778. Erro 07099.";
                        _processado_ = true;
                        break;
                    default:
                        m_erro_canc = "ERRO DESCONHECIDO. Ligue para (11) 4304-7778. Erro DEFAULT.";
                        _processado_ = true;
                        break;
                }
            }
            if (erro_canc == true)
            {

                DialogBox.Show("Erro do SAT", DialogBoxButtons.No, DialogBoxIcons.Error, true, "Erro no SAT:", m_erro_canc);

                log.Debug($"Erro do SAT: {erro_canc}");
                return;
            }
            var serializer = new XmlSerializer(typeof(CFeCanc));

            CFeCanc retornoCanc = new();
            using (var xmlRetorno = new StringReader(_xmlret_canc))
            using (var xreader = XmlReader.Create(xmlRetorno))
            {
                retornoCanc = (CFeCanc)serializer.Deserialize(xreader);
            }

            //StringReader XmlRetorno = new StringReader(_xmlret_canc);
            //XmlReader _xreader = XmlReader.Create(XmlRetorno);
            //cancCFe xml_de_retorno_canc = new cancCFe();
            //XmlRootAttribute xRoot = new();
            //xRoot.ElementName = "CFeCanc";
            //var _serializer = new XmlSerializer(xml_de_retorno_canc.GetType());
            //xml_de_retorno_canc = (cancCFe)_serializer.Deserialize(_xreader);





            string valorcfe = retornoCanc.infCFe.total.vCFe;
            _infoStr = retornoCanc.infCFe.dest.Item;
            string chavenfe = retornoCanc.infCFe.Id.Substring(3);
            Directory.CreateDirectory(@"SAT\Cancelamentos");
            File.WriteAllText(string.Format(@"SAT\Cancelamentos\ADC {0}.xml", chavenfe), _xmlret_canc);
            int.TryParse(retornoCanc.infCFe.ide.nCFe, out printCANCL.numerodoextrato);
            int.TryParse(retornoCanc.infCFe.ide.nserieSAT, out printCANCL.numerosat);
            int.TryParse(retornoCanc.infCFe.chCanc.Substring(34, 6), out printCANCL.cupomcancelado);
            printCANCL.cpfcnpjconsumidor = _infoStr;
            printCANCL._operador = operador.Split(' ')[0];
            printCANCL.chavenfe = chavenfe;
            printCANCL.total = decimal.Parse(valorcfe.Replace('.', ','));
            printCANCL.assinaturaQRCODE = chavenfe + "|" + DateTime.Now.ToString("yyyyMMddHHmmss") + "|" + valorcfe + "|" + _infoStr + "|" + retornoCanc.infCFe.ide.assinaturaQRCODE;
            printCANCL.IMPRIME();
            log.Debug("Impressão do CFe de cancelamento");
            log.Debug($"CANCELAULTIMOCUPOM({cupomSAT.ID_NFVENDA})");


            using (var TB_NFVENDA = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter())
            using (var TB_SAT_CANC = new DataSets.FDBDataSetVendaTableAdapters.TB_SAT_CANCTableAdapter())
            using (var TB_SAT = new DataSets.FDBDataSetVendaTableAdapters.TB_SATTableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                TB_NFVENDA.Connection = TB_SAT_CANC.Connection = TB_SAT.Connection = LOCAL_FB_CONN;
                TB_NFVENDA.SetaCanceladoPorIDNFVenda(cupomSAT.ID_NFVENDA);
                TB_SAT_CANC.Insert(0,
                                   cupomSAT.ID_REGISTRO,
                                   DateTime.Today,
                                   DateTime.Now,
                                   int.Parse(retornoCanc.infCFe.ide.nCFe),
                                   retornoCanc.infCFe.Id.Replace("CFe", string.Empty),
                                   retornoCanc.infCFe.ide.nserieSAT,
                                   null);
                TB_SAT.ChangeStatus(STATUS: "07000",
                                    STATUS_DES: "Cupom cancelado com sucesso + conteúdo CF-e-SAT cancelado",
                                    CHAVE: cupomSAT.CHAVE_CFE);
            }

            //DesfazerPagamento(cupomSAT.ID_NFVENDA);
            //log.Debug("DesfazPagamento do cupom " + cupomSAT.ID_NFVENDA);
            //using (var fbCommand = new FbCommand())
            //using (var fbConnection = new FbConnection((new FbConnectionStringBuilder() { DataSource = SERVERNAME, Database = SERVERCATALOG, UserID = "SYSDBA", Password = "masterke" }).ConnectionString))
            //{
            //    fbCommand.Connection = fbConnection;
            //    fbCommand.CommandType = CommandType.Text;
            //    fbCommand.CommandText = "UPDATE TRI_PDV_SAT_XML " +
            //                            "SET CANCEL_CFE = 'S' " +
            //                            $"WHERE CHAVE_CFE = '{cupomSAT.CHAVE_CFE}';";
            //    fbConnection.Open();
            //    fbCommand.ExecuteNonQuery();
            //    fbConnection.Close();
            //}

        }

        /// <summary>
        /// Gera e manda o cancelamento do último cupom fiscal para o servidor, e ser cancelado.
        /// </summary>
        /// <param name="pCupomInterno">ID_CUPOM do cupom a ser cancelado</param>
        private void CancelarSatServidorPelaBase(CupomSAT cupomSAT)
        {
            PrintCANCL printCANCL = new();
            log.Debug("Cupom sat a ser cancelado: {cupomSAT.CHAVE_CFE}");
            CFeCanc CfeCanc = new();
            cancCFeCFeCancInfCFe CancinfCFe = new();
            cancCFeCFeCancInfCFeDest CancinfCFeDest = new();
            cancCFeCFeCancInfCFeEmit CancinfCFeEmit = new();
            cancCFeCFeCancInfCFeIde CancinfCFeIde = new();
            cancCFeCFeCancInfCFeObsFisco CancinfCFeObsFisco = new();
            cancCFeCFeCancInfCFeTotal CancinfCFeTotal = new();
            CancinfCFe.chCanc = "CFe" + cupomSAT.CHAVE_CFE;
            CancinfCFeIde.signAC = SIGN_AC;
            CancinfCFeIde.numeroCaixa = NO_CAIXA.ToString("D3");
            if (SIGN_AC.Contains("RETAGUARDA"))
            {
                CancinfCFeIde.CNPJ = "16716114000172";
            }
            else
            {
                CancinfCFeIde.CNPJ = CNPJSH;
            }
            CancinfCFe.dest = CancinfCFeDest;
            CancinfCFe.ide = CancinfCFeIde;
            CancinfCFe.emit = CancinfCFeEmit;
            CancinfCFe.total = CancinfCFeTotal;
            CfeCanc.infCFe = CancinfCFe;
            var _settings = new XmlWriterSettings() { Encoding = new UTF8Encoding(false) };
            var _XmlFinal = new StringBuilder();
            var _xwriter2 = XmlWriter.Create(_XmlFinal, _settings);
            var _serializer = new XmlSerializer(CfeCanc.GetType());
            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);
            _serializer.Serialize(_xwriter2, CfeCanc, xns); //Popula o stringbuilder XmlFinal.
            string _C_XML_ = _XmlFinal.ToString().Replace(',', '.').Replace("utf-16", "utf-8");
            File.WriteAllText(@"SAT_LOG\UltimaoCanc.xml", _C_XML_);
            byte[] bytes = Encoding.Default.GetBytes("CFe" + cupomSAT.CHAVE_CFE + _C_XML_);
            string m_erro_canc = "";
            bool erro_canc = true;
            string _xmlret_canc = "";
            string[] retorno_canc = { "" };

            using (var SAT_ENV_TA = new TRI_PDV_SAT_ENVTableAdapter())
            {
                SAT_ENV_TA.SP_TRI_ENVIA_SAT_SERVIDOR(NO_CAIXA, bytes);
            }


            var sb = new SATBox("Operação no SAT", "Aguarde a resposta do SAT.");
            sb.ShowDialog();
            if (sb.DialogResult == false)
            {
                return;
            }
            else { retorno_canc = sb.retorno; }

            if (retorno_canc[0] == "")
            {
                m_erro_canc = "DEU RUIM";
                erro_canc = true;
            }
            else if ((!(retorno_canc is null)) && retorno_canc.Length < 2)
            {
                m_erro_canc = "ERRO DESCONHECIDO. Verifique LogErro.txt";
                erro_canc = true;
            }
            else
            {
                switch (retorno_canc[1])
                {
                    case "07000":
                        _xmlret_canc = Encoding.UTF8.GetString(Convert.FromBase64String(retorno_canc[6].ToString()));
                        //_processado_ = true;
                        erro_canc = false;
                        break;
                    case "07001":
                        m_erro_canc = "Código de ativação inválido.";
                        //_processado_ = true;
                        break;
                    case "07002":
                        m_erro_canc = "07002 - Cupom inválido.";
                        //_processado_ = true;
                        break;
                    case "07003":
                        m_erro_canc = "SAT bloqueado pelo contribuinte.";
                        //_processado_ = true;
                        break;
                    case "07004":
                        m_erro_canc = "SAT bloqueado pela SEFAZ.";
                        //_processado_ = true;
                        break;
                    case "07005":
                        m_erro_canc = "SAT bloqueado por falta de comunicação.";
                        //_processado_ = true;
                        break;
                    case "07006":
                        m_erro_canc = "SAT temporariamente bloqueado. Número de tentativas ultrapassado.";
                        //_processado_ = true;
                        break;
                    case "07007":
                        m_erro_canc = "Erro de validação de conteúdo. Último cupom pode já ter sido cancelado, ou o tempo limite ultrapassado.";
                        //_processado_ = true;
                        break;
                    case "07098":
                        m_erro_canc = "SAT ocupado, aguarde para tentar novamente.";
                        //_processado_ = true;
                        break;
                    case "07099":
                        m_erro_canc = "ERRO DESCONHECIDO. Ligue para (11) 4304-7778. Erro 07099.";
                        //_processado_ = true;
                        break;
                    default:
                        m_erro_canc = "ERRO DESCONHECIDO. Ligue para (11) 4304-7778. Erro DEFAULT.";
                        //_processado_ = true;
                        break;
                }
            }

            if (erro_canc == true)
            {

                DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Error, true, "Erro no SAT:", m_erro_canc);
                log.Debug($"Erro interno do SAT: {m_erro_canc}");
                return;
            }
            StringReader XmlRetorno = new StringReader(_xmlret_canc);
            XmlReader _xreader = XmlReader.Create(XmlRetorno);
            CFeCanc xml_de_retorno_canc = (CFeCanc)_serializer.Deserialize(_xreader);
            string valorcfe = xml_de_retorno_canc.infCFe.total.vCFe;
            _infoStr = xml_de_retorno_canc.infCFe.dest.Item;
            string chavenfe = xml_de_retorno_canc.infCFe.Id.Substring(3);
            Directory.CreateDirectory(@"SAT\Cancelamentos");
            File.WriteAllText(string.Format(@"SAT\Cancelamentos\ADC {0}.xml", chavenfe), _xmlret_canc);
            int.TryParse(xml_de_retorno_canc.infCFe.ide.nCFe, out int _nCFe);
            printCANCL.numerodoextrato = _nCFe;
            int.TryParse(xml_de_retorno_canc.infCFe.ide.nserieSAT, out int _nserieSAT);
            printCANCL.numerosat = _nserieSAT;
            printCANCL.cupomcancelado = cupomSAT.ID_NFVENDA;
            printCANCL.cpfcnpjconsumidor = _infoStr;
            printCANCL._operador = operador.Split(' ')[0];
            printCANCL.chavenfe = chavenfe;
            printCANCL.total = decimal.Parse(valorcfe.Replace('.', ','));
            printCANCL.assinaturaQRCODE = chavenfe + "|" + DateTime.Now.ToString("yyyyMMddhhmmss") + "|" + valorcfe + "|" + _infoStr + "|" + xml_de_retorno_canc.infCFe.ide.assinaturaQRCODE;
            printCANCL.IMPRIME();

            using (var TB_NFVENDA = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            using (var TB_SAT_CANC = new DataSets.FDBDataSetVendaTableAdapters.TB_SAT_CANCTableAdapter())
            {
                TB_NFVENDA.Connection = TB_SAT_CANC.Connection = LOCAL_FB_CONN;
                TB_NFVENDA.SetaCanceladoPorIDNFVenda(cupomSAT.ID_NFVENDA);
                TB_SAT_CANC.Insert(0,
                    cupomSAT.ID_REGISTRO,
                    DateTime.Today,
                    DateTime.Now,
                    int.Parse(xml_de_retorno_canc.infCFe.ide.nCFe),
                    xml_de_retorno_canc.infCFe.chCanc,
                    xml_de_retorno_canc.infCFe.ide.nserieSAT,
                    null);
            }
            //DesfazerPagamento(cupomSAT.ID_NFVENDA);
            //using (var fbCommand = new FbCommand())
            //using (var fbConnection = new FbConnection((new FbConnectionStringBuilder() { DataSource = SERVERNAME, Database = SERVERCATALOG, UserID = "SYSDBA", Password = "masterke" }).ConnectionString))
            //{
            //    fbCommand.Connection = fbConnection;
            //    fbCommand.CommandType = CommandType.Text;
            //    fbCommand.CommandText = "UPDATE TRI_PDV_SAT_XML " +
            //                            "SET CANCEL_CFE = 'S' " +
            //                            $"WHERE CHAVE_CFE = '{cupomSAT.CHAVE_CFE}';";
            //    fbConnection.Open();
            //    fbCommand.ExecuteNonQuery();
            //    fbConnection.Close();
            //}

        }


        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
                return;
            }
        }

        private void TRI_PDV_SAT_XMLDataGrid_LayoutUpdated(object sender, EventArgs e)
        {

        }

        private void TRI_PDV_SAT_XMLDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && tRI_PDV_SAT_XMLDataGrid.SelectedItem != null)

            {
                CancelarCupomSelecionado();
            }
        }

    }
}

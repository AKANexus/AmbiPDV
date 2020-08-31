using DeclaracoesDllSat;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Configuracoes
{
    public static class ConfiguracoesPDV
    {
        #region Declaracoes
        public enum PerguntaWhatsEnum { Nunca, Sempre, TeclaAtalho, Escolha }
        public enum ModeloCupom { Completo, Simples }
        #endregion

        #region Propriedades
        private static int _pERGUNTA_WHATS = 0;
        public static PerguntaWhatsEnum PERGUNTA_WHATS
        {
            get
            {
                return (PerguntaWhatsEnum)_pERGUNTA_WHATS;
            }
            set
            {
                _pERGUNTA_WHATS = (int)value;
            }
        }

        public static string CNPJSH = "22141365000179";

        public static string ID_MAC { get; set; }

        public static int NO_CAIXA { get; set; }

        private static string _eXIGE_SANGRIA = "N";
        public static bool EXIGE_SANGRIA
        {
            get
            {
                switch (_eXIGE_SANGRIA)
                {
                    case "S":
                    case "1":
                        return true;
                    case "N":
                    case "0":
                    default:
                        return false;
                }
            }
            set
            {
                _eXIGE_SANGRIA = value ? "S" : "N";
            }
        }

        public static decimal VALOR_MAX_CAIXA { get; set; }

        private static string _bLOQUEIA_NO_LIMITE;
        public static bool BLOQUEIA_NO_LIMITE
        {
            get
            {
                switch (_bLOQUEIA_NO_LIMITE)
                {
                    case "S":
                    case "1":
                        return true;
                    case "N":
                    case "0":
                    default:
                        return false;
                }
            }
            set
            {
                _bLOQUEIA_NO_LIMITE = value ? "S" : "N";
            }
        }

        public static decimal VALOR_DE_FOLGA { get; set; }

        private static string _pERMITE_FOLGA_SANGRIA;
        public static bool PERMITE_FOLGA_SANGRIA
        {
            get
            {
                switch (_pERMITE_FOLGA_SANGRIA)
                {
                    case "S":
                    case "1":
                        return true;
                    case "N":
                    case "0":
                    default:
                        return false;
                }

            }
            set
            {
                _pERMITE_FOLGA_SANGRIA = value ? "S" : "N";
            }
        }

        private static string _iNTERROMPE_NAO_ENCONTRADO;
        public static bool INTERROMPE_NAO_ENCONTRADO
        {
            get
            {
                switch (_iNTERROMPE_NAO_ENCONTRADO)
                {
                    case "S":
                    case "1":
                        return true;
                    case "N":
                    case "0":
                    default:
                        return false;
                }

            }
            set
            {
                _iNTERROMPE_NAO_ENCONTRADO = value ? "S" : "N";
            }

        }

        public static string MENSAGEM_CORTESIA { get; set; }

        public static decimal ICMS_CONT { get; set; }

        public static decimal CSOSN_CONT { get; set; }

        public static int PEDE_CPF { get; set; }

        private static int _pERMITE_ESTOQUE_NEGATIVO;
        public static bool? PERMITE_ESTOQUE_NEGATIVO
        {
            get
            {
                switch (_pERMITE_ESTOQUE_NEGATIVO)
                {
                    case -1:
                        return null;
                    case 0:
                        return false;
                    case 1:
                    default:
                        return true;
                }

            }
            set
            {
                switch (value)
                {
                    case null:
                        _pERMITE_ESTOQUE_NEGATIVO = -1;
                        break;
                    case false:
                        _pERMITE_ESTOQUE_NEGATIVO = 0;
                        break;
                    case true:
                    default:
                        _pERMITE_ESTOQUE_NEGATIVO = 1;
                        break;
                }
            }
        }

        private static int _mODELO_CUPOM;
        public static ModeloCupom MODELO_CUPOM
        {
            get
            {
                switch (_mODELO_CUPOM)
                {
                    case 0:
                        return ModeloCupom.Simples;
                    case 1:
                    default:
                        return ModeloCupom.Completo;
                }
            }
            set
            {
                _mODELO_CUPOM = (int)value;
            }
        }

        public static string _uSARECARGAS;
        public static bool USARECARGAS
        {
            get
            {
                switch (_uSARECARGAS)
                {
                    case "S":
                        return true;
                    case "N":
                    default:
                        return false;
                }

            }
            set
            {
                _uSARECARGAS = value ? "S" : "N";
            }
        }

        public static string IMPRESSORA_USB { get; set; }

        public static string IMPRESSORA_USB_PED { get; set; }

        public static string MENSAGEM_RODAPE { get; set; }

        private static int? _mODELO_SAT;
        public static ModeloSAT MODELO_SAT
        {
            get
            {
                if (_mODELO_SAT is null)
                {
                    return ModeloSAT.NENHUM;
                }

                return (ModeloSAT)_mODELO_SAT;
            }
            set
            {
                if (value is ModeloSAT.NENHUM)
                {
                    _mODELO_SAT = null;
                }
                else
                {
                    _mODELO_SAT = (int)value;
                }
            }
        }

        private static string _sATSERVIDOR;
        public static bool SATSERVIDOR
        {
            get
            {
                switch (_sATSERVIDOR)
                {
                    case "S":
                        return true;
                    case "N":
                    default:
                        return false;
                }

            }
            set
            {
                _sATSERVIDOR = value ? "S" : "N";
            }

        }

        public static string SAT_CODATIV { get; set; }

        public static string SIGN_AC { get; set; }

        private static string _sAT_USADO;
        public static bool SAT_USADO
        {
            get
            {
                switch (_sAT_USADO)
                {
                    case "S":
                        return true;
                    case "N":
                    default:
                        return false;
                }

            }
            set
            {
                _sAT_USADO = value ? "S" : "N";
            }
        }

        private static string _eCF_ATIVA;
        public static bool ECF_ATIVA
        {
            get
            {
                switch (_eCF_ATIVA)
                {
                    case "S":
                        return true;
                    case "N":
                    default:
                        return false;
                }

            }
            set
            {
                _eCF_ATIVA = value ? "S" : "N";
            }

        }

        public static string ECF_PORTA { get; set; }

        public static decimal DESCONTO_MAXIMO { get; set; }

        private static string _uSATEF;
        public static bool USATEF
        {
            get
            {
                switch (_uSATEF)
                {
                    case "S":
                        return true;
                    case "N":
                    default:
                        return false;
                }
            }
            set
            {
                switch (value)
                {
                    case true:
                        _uSATEF = "S";
                        break;
                    case false:
                        _uSATEF = "N";
                        break;
                }
            }
        }
        public static string TEFIP { get; set; }

        public static string TEFNUMLOJA { get; set; }

        public static string TEFNUMTERMINAL { get; set; }

        private static string _tEFPEDECPFPELOPINPAD;
        public static bool TEFPEDECPFPELOPINPAD
        {
            get
            {
                switch (_tEFPEDECPFPELOPINPAD)
                {
                    case "S":
                        return true;
                    case "N":
                    default:
                        return false;
                }
            }
            set
            {
                switch (value)
                {
                    case true:
                        _tEFPEDECPFPELOPINPAD = "S";
                        break;
                    case false:
                        _tEFPEDECPFPELOPINPAD = "N";
                        break;
                }
            }
        }

        public static bool DETALHADESCONTO { get; set; }

        public static int COD10PORCENTO;

        private static string _mODOBAR;
        public static bool MODOBAR
        {
            get
            {
                switch (_mODOBAR)
                {
                    case "S":
                        return true;
                    case "N":
                    default:
                        return false;
                }
            }
            set
            {
                switch (value)
                {
                    case true:
                        _mODOBAR = "S";
                        break;
                    case false:
                        _mODOBAR = "N";
                        break;
                }
            }
        }

        private static int _tIPO_LICENCA;
        public static TipoLicenca TIPO_LICENCA
        {
            get
            {
                switch (_tIPO_LICENCA)
                {
                    case 0:
                        return TipoLicenca.offline;
                    default:
                        return TipoLicenca.online;

                }
            }
            set
            {
                switch (value)
                {
                    case TipoLicenca.offline:
                        _tIPO_LICENCA = 0;
                        break;
                    case TipoLicenca.online:
                    default:
                        _tIPO_LICENCA = 1;
                        break;
                }
            }
        }

        private static string _uSA_COMANDA;
        public static bool USA_COMANDA
        {
            get
            {
                switch (_uSA_COMANDA)
                {
                    case "S":
                        return true;
                    case "N":
                    default:
                        return false;
                }
            }
            set
            {
                switch (value)
                {
                    case true:
                        _uSA_COMANDA = "S";
                        break;
                    case false:
                        _uSA_COMANDA = "N";
                        break;
                }
            }
        }

        private static string _pEDESENHACANCEL;
        public static bool PEDESENHACANCEL
        {
            get
            {
                switch (_pEDESENHACANCEL)
                {
                    case "S":
                        return true;
                    case "N":
                    default:
                        return false;
                }
            }
            set
            {
                switch (value)
                {
                    case true:
                        _pEDESENHACANCEL = "S";
                        break;
                    case false:
                        _pEDESENHACANCEL = "N";
                        break;
                }
            }
        }
        //=== === === ===
        public static short BALPORTA { get; set; }

        public static short BALBITS { get; set; }

        public static int BALBAUD { get; set; }

        public static short BALPARITY { get; set; }

        public static short BALMODELO { get; set; }

        public static short ACFILLPREFIX { get; set; }
        public static short ACFILLMODE { get; set; }
        public static short ACREFERENCIA { get; set; }
        public static short SYSCOMISSAO { get; set; }
        public static int SATSERVTIMEOUT { get; set; }
        public static int SATLIFESIGNINTERVAL { get; set; }
        public static int ACFILLDELAY { get; set; }
        public static short SYSUSAWHATS { get; set; }
        public static short SYSPARCELA { get; set; }
        /// <summary>
        /// 0 = Não emite, 1 = sempre emite, 2 = pergunta se emite
        /// </summary>
        public static short SYSEMITECOMPROVANTE { get; set; }
        public static bool CONFIGURADO { get; set; }

        //=== === === === === === ===

        public static string LOGO { get; set; }
        public static string NOMESOFTWARE { get; set; }
        public static int FBTIMEOUT { get; set; }
        public static string SERVERNAME { get; set; }
        public static string SERVERCATALOG { get; set; }
        public static bool PERMITE_CANCELAR_VENDA_EM_CURSO { get; set; }
        public static bool FECHAMENTO_EXTENDIDO { get; set; }


        #endregion Propriedades

        #region Metodos
        public static bool SalvaConfigsNaBase()
        {
            funcoesClass funcoes = new funcoesClass();
            using var fbConnectionServ = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
            using var fbCommSalvaConfig = new FbCommand();
            using var fbCommSalvaSetup = new FbCommand();
            if (fbConnectionServ.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    fbConnectionServ.Open();
                }
                catch (Exception ex)
                {
                    logErroAntigo($"SalvaConfigsNaBase>> fbConnectionServ.Open(): \nconexão não está aberta, e não conseguiu abrir. \nConnectionState = {fbConnectionServ.State} \n{RetornarMensagemErro(ex, true)}");
                }
            }

            using (var fbTransactionServ = fbConnectionServ.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Wait }))
            {
                var idMac = funcoes.GetSerialHexNumberFromExecDisk();

                fbCommSalvaConfig.CommandType = System.Data.CommandType.Text;
                fbCommSalvaConfig.Connection = fbConnectionServ;
                fbCommSalvaConfig.Transaction = fbTransactionServ;
                fbCommSalvaConfig.Parameters.AddWithValue("@pID_MAC", idMac);
                fbCommSalvaConfig.Parameters.AddWithValue("@pNO_CAIXA", NO_CAIXA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pEXIGE_SANGRIA", _eXIGE_SANGRIA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pVALOR_MAX_CAIXA", VALOR_MAX_CAIXA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pBLOQUEIA_NO_LIMITE", _bLOQUEIA_NO_LIMITE);
                fbCommSalvaConfig.Parameters.AddWithValue("@pVALOR_DE_FOLGA", VALOR_DE_FOLGA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pPERMITE_FOLGA_SANGRIA", _pERMITE_FOLGA_SANGRIA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pINTERROMPE_NAO_ENCONTRADO", _iNTERROMPE_NAO_ENCONTRADO);
                fbCommSalvaConfig.Parameters.AddWithValue("@pMENSAGEM_CORTESIA", MENSAGEM_CORTESIA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pICMS_CONT", ICMS_CONT);
                fbCommSalvaConfig.Parameters.AddWithValue("@pCSOSN_CONT", CSOSN_CONT);
                fbCommSalvaConfig.Parameters.AddWithValue("@pPEDE_CPF", PEDE_CPF);
                fbCommSalvaConfig.Parameters.AddWithValue("@pPERMITE_ESTOQUE_NEGATIVO", _pERMITE_ESTOQUE_NEGATIVO);
                fbCommSalvaConfig.Parameters.AddWithValue("@pMODELO_CUPOM", _mODELO_CUPOM);
                fbCommSalvaConfig.Parameters.AddWithValue("@pMENSAGEM_RODAPE", MENSAGEM_RODAPE);
                fbCommSalvaConfig.Parameters.AddWithValue("@pMODELO_SAT", _mODELO_SAT);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSATSERVIDOR", _sATSERVIDOR);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSAT_CODATIV", SAT_CODATIV);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSIGN_AC", SIGN_AC);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSAT_USADO", _sAT_USADO);
                fbCommSalvaConfig.Parameters.AddWithValue("@pECF_ATIVA", _eCF_ATIVA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pECF_PORTA", ECF_PORTA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pIMPRESSORA_USB", IMPRESSORA_USB);
                fbCommSalvaConfig.Parameters.AddWithValue("@pIMPRESSORA_USB_PED", IMPRESSORA_USB_PED);
                fbCommSalvaConfig.Parameters.AddWithValue("@pPERGUNTA_WHATS", PERGUNTA_WHATS);
                fbCommSalvaConfig.Parameters.AddWithValue("@pUSATEF", _uSATEF);
                fbCommSalvaConfig.Parameters.AddWithValue("@pTEFIP", TEFIP);
                fbCommSalvaConfig.Parameters.AddWithValue("@pTEFNUMLOJA", TEFNUMLOJA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pTEFNUMTERMINAL", TEFNUMTERMINAL);
                fbCommSalvaConfig.Parameters.AddWithValue("@pTEFPEDECPFPELOPINPAD", _tEFPEDECPFPELOPINPAD);
                fbCommSalvaConfig.Parameters.AddWithValue("@pBALPORTA", BALPORTA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pBALBITS", BALBITS);
                fbCommSalvaConfig.Parameters.AddWithValue("@pBALBAUD", BALBAUD);
                fbCommSalvaConfig.Parameters.AddWithValue("@pBALMODELO", BALMODELO);
                fbCommSalvaConfig.Parameters.AddWithValue("@pACFILLPREFIX", ACFILLPREFIX);
                fbCommSalvaConfig.Parameters.AddWithValue("@pACFILLMODE", ACFILLMODE);
                fbCommSalvaConfig.Parameters.AddWithValue("@pACREFERENCIA", ACREFERENCIA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSYSCOMISSAO", SYSCOMISSAO);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSATSERVTIMEOUT", SATSERVTIMEOUT);
                fbCommSalvaConfig.Parameters.AddWithValue("@pACFILLDELAY", ACFILLDELAY);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSYSPERGUNTAWHATS", SYSUSAWHATS);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSYSPARCELA", SYSPARCELA);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSATLIFESIGNINTERVAL", SATLIFESIGNINTERVAL);
                fbCommSalvaConfig.Parameters.AddWithValue("@pSYSEMITECOMPROVANTE", SYSEMITECOMPROVANTE);
                fbCommSalvaConfig.Parameters.AddWithValue("@pBALPARITY", BALPARITY);

                fbCommSalvaConfig.CommandText =
                                        "UPDATE OR INSERT INTO TRI_PDV_CONFIG " +
                                        "(ID_MAC, NO_CAIXA, EXIGE_SANGRIA, VALOR_MAX_CAIXA, BLOQUEIA_NO_LIMITE, VALOR_DE_FOLGA, PERMITE_FOLGA_SANGRIA, " +
                                        "INTERROMPE_NAO_ENCONTRADO, MENSAGEM_CORTESIA, ICMS_CONT, CSOSN_CONT, PEDE_CPF, PERMITE_ESTOQUE_NEGATIVO, " +
                                        "MODELO_CUPOM, MENSAGEM_RODAPE, MODELO_SAT, SATSERVIDOR, SAT_CODATIV, SIGN_AC, SAT_USADO, ECF_ATIVA, ECF_PORTA, " +
                                        " IMPRESSORA_USB, IMPRESSORA_USB_PED, PERGUNTA_WHATS, USATEF, TEFIP, TEFNUMLOJA, TEFNUMTERMINAL, TEFPEDECPFPELOPINPAD, " +
                                        "BALPORTA, BALBAUD, BALPARITY, BALMODELO, ACFILLPREFIX, ACFILLMODE, ACREFERENCIA, SYSCOMISSAO, SATSERVTIMEOUT, " +
                                        "SATLIFESIGNINTERVAL, ACFILLDELAY, SYSPERGUNTAWHATS, SYSPARCELA, SYSEMITECOMPROVANTE) " +
                                        "VALUES " +
                                        "(@pID_MAC, @pNO_CAIXA, @pEXIGE_SANGRIA, @pVALOR_MAX_CAIXA, @pBLOQUEIA_NO_LIMITE, @pVALOR_DE_FOLGA, @pPERMITE_FOLGA_SANGRIA, " +
                                        "@pINTERROMPE_NAO_ENCONTRADO, @pMENSAGEM_CORTESIA, @pICMS_CONT, @pCSOSN_CONT, @pPEDE_CPF, @pPERMITE_ESTOQUE_NEGATIVO, " +
                                        "@pMODELO_CUPOM, @pMENSAGEM_RODAPE, @pMODELO_SAT, @pSATSERVIDOR, @pSAT_CODATIV, @pSIGN_AC, @pSAT_USADO, @pECF_ATIVA, " +
                                        "@pECF_PORTA, @pIMPRESSORA_USB, @pIMPRESSORA_USB_PED, @pPERGUNTA_WHATS, @pUSATEF, @pTEFIP, @pTEFNUMLOJA, @pTEFNUMTERMINAL, " +
                                        "@pTEFPEDECPFPELOPINPAD, @pBALPORTA, @pBALBAUD, @pBALPARITY, @pBALMODELO, @pACFILLPREFIX, @pACFILLMODE, @pACREFERENCIA, @pSYSCOMISSAO, @pSATSERVTIMEOUT, " +
                                        "@pSATLIFESIGNINTERVAL, @pACFILLDELAY, @pSYSPERGUNTAWHATS, @pSYSPARCELA, @pSYSEMITECOMPROVANTE) " +
                                        "MATCHING (ID_MAC);";


                try
                {
                    fbCommSalvaConfig.ExecuteNonQuery();
                    using var fbConnectionPdv = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB") };

                    //TODO: ADD no_caixa TRI_PDV_AUX_SYNC
                    // Ver se o NO_CAIXA para a TRI_PDV_CONFIG, operação "I" consta no TRI_PDV_AUX_SYNC.
                    // Se não, gravar uma entrada na TRI_PDV_AUX_SYNC
                    using var SETUP_TA = new FDBDataSetTableAdapters.TRI_PDV_SETUPTableAdapter() { Connection = fbConnectionPdv };
                    {
                        if (SETUP_TA.GetData().Rows.Count < 1)
                        {
                            SETUP_TA.Insert(1, null, Assembly.GetExecutingAssembly().GetName().Version.ToString(), DateTime.Now,
                                DateTime.Now, DateTime.Now, null, (double)DESCONTO_MAXIMO, null, _uSARECARGAS, "0.0.0.0", "N", COD10PORCENTO, _mODOBAR, _tIPO_LICENCA, _uSA_COMANDA, _pEDESENHACANCEL);
                        }
                        else
                        {
                            SETUP_TA.UpdateSetup(1, Assembly.GetExecutingAssembly().GetName().Version.ToString(), DESCONTO_MAXIMO, _uSARECARGAS, DETALHADESCONTO.ToInt().ToString(), COD10PORCENTO, _mODOBAR, _tIPO_LICENCA, _uSA_COMANDA, _pEDESENHACANCEL);
                        }
                    }
                    using (var fbCommGetNoCaixaAuxConfigLocal = new FbCommand())
                    {
                        if (fbConnectionPdv.State != System.Data.ConnectionState.Open)
                        {
                            try
                            {
                                fbConnectionPdv.Open();
                            }
                            catch (Exception ex)
                            {
                                logErroAntigo($"SalvaConfigsNaBase>> fbConnectionPdv.Open(): \nconexão não está aberta, e não conseguiu abrir. \nConnectionState = {fbConnectionPdv.State} \n{RetornarMensagemErro(ex, true)}");
                            }
                        }

                        fbCommGetNoCaixaAuxConfigLocal.CommandType = System.Data.CommandType.Text;
                        fbCommGetNoCaixaAuxConfigLocal.Connection = fbConnectionPdv;
                        fbCommGetNoCaixaAuxConfigLocal.CommandText = "SELECT count(1) FROM TRI_PDV_CONFIG WHERE ID_MAC = @idMac AND NO_CAIXA = @noCaixa;";
                        fbCommGetNoCaixaAuxConfigLocal.Parameters.Add("@idMac", idMac);
                        fbCommGetNoCaixaAuxConfigLocal.Parameters.Add("@noCaixa", NO_CAIXA);

                        var countNoCaixaLocal = fbCommGetNoCaixaAuxConfigLocal.ExecuteScalar().Safeint();

                        if (countNoCaixaLocal <= 0)
                        {
                            using var fbCommSetNewAuxConfigServ = new FbCommand
                            {
                                CommandType = System.Data.CommandType.Text,
                                Connection = fbConnectionServ,
                                Transaction = fbTransactionServ,
                                CommandText = "INSERT INTO TRI_PDV_AUX_SYNC ( SEQ , ID_REG , TABELA , OPERACAO , NO_CAIXA , TS_OPER , UN_REG , SM_REG , CH_REG ) VALUES ( GEN_ID(GEN_PDV_AUX_SYNC_SEQ, 1) , -1 ,'TRI_PDV_CONFIG' ,'I' , @noCaixa , CURRENT_TIMESTAMP , @idMac , NULL , NULL ) ;"
                            };
                            fbCommSetNewAuxConfigServ.Parameters.Add("@idMac", idMac);
                            fbCommSetNewAuxConfigServ.Parameters.Add("@noCaixa", NO_CAIXA);

                            var retornoInsert = fbCommSetNewAuxConfigServ.ExecuteNonQuery();

                            if (retornoInsert <= 0)
                            {
                                throw new Exception("Erro ao gravar configurações no banco de dados do servidor. \n\nPor favor entre em contato com a equipe de suporte.",
                                                    new Exception($"Retorno não esperado ao inserir registro na TRI_PDV_AUX_SYNC: {retornoInsert}. \nParâmetros: @idMac = {idMac} ; @noCaixa = {NO_CAIXA}"));
                            }
                        }
                    }

                    fbTransactionServ.Commit();
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    fbTransactionServ.Rollback();
                    throw ex;
                }
                //fbCommand.Connection.Close(); // não precisa, o dispose já faz isso. Fonte: https://github.com/cincuranet/FirebirdSql.Data.FirebirdClient/blob/master/Provider/src/FirebirdSql.Data.FirebirdClient/FirebirdClient/FbConnection.cs
            }
            return true;
        }

        /// <summary>
        /// Carrega as configurações da base de dados, e copia na memoria não volátil. Retorna falso se a base não foi encontrada.
        /// </summary>
        /// <returns></returns>
        public static bool CarregaConfigs()
        {
            funcoesClass funcoes = new funcoesClass();
            using var CONFIG_DT = new DataSets.FDBDataSetConfig.TRI_PDV_CONFIGDataTable();
            using var SETUP_TA = new FDBDataSetTableAdapters.TRI_PDV_SETUPTableAdapter();
            using var fbConnectionServ = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) }; //"localhost", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB") })
            //using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var fbCommand = new FbCommand
            {
                Connection = fbConnectionServ,
                CommandType = System.Data.CommandType.Text
            };
            SETUP_TA.Connection = fbConnectionServ;
            fbCommand.Parameters.AddWithValue("@pID_MAC", funcoes.GetSerialHexNumberFromExecDisk());
            fbCommand.CommandText = "SELECT * " +
                                        "FROM TRI_PDV_CONFIG WHERE ID_MAC = @pID_MAC";
            fbCommand.Connection.Open();
            try
            {
                CONFIG_DT.Load(fbCommand.ExecuteReader());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            fbCommand.Connection.Close();
            if (CONFIG_DT.Rows.Count == 0)
            {
                return false;
            }
            try
            {
                DataSets.FDBDataSetConfig.TRI_PDV_CONFIGRow registro = (DataSets.FDBDataSetConfig.TRI_PDV_CONFIGRow)CONFIG_DT.Rows[0];
                NO_CAIXA = registro.NO_CAIXA;
                _eXIGE_SANGRIA = (registro.EXIGE_SANGRIA is null) ? "N" : registro.EXIGE_SANGRIA;
                VALOR_MAX_CAIXA = (decimal)registro.VALOR_MAX_CAIXA;
                _bLOQUEIA_NO_LIMITE = (registro.BLOQUEIA_NO_LIMITE is null) ? "N" : registro.BLOQUEIA_NO_LIMITE;
                VALOR_DE_FOLGA = (decimal)registro.VALOR_DE_FOLGA;
                _iNTERROMPE_NAO_ENCONTRADO = (registro.INTERROMPE_NAO_ENCONTRADO is null) ? "N" : registro.INTERROMPE_NAO_ENCONTRADO;
                _pERMITE_FOLGA_SANGRIA = (registro.PERMITE_FOLGA_SANGRIA is null) ? "N" : registro.PERMITE_FOLGA_SANGRIA;
                MENSAGEM_CORTESIA = registro.MENSAGEM_CORTESIA;
                ICMS_CONT = (decimal)registro.ICMS_CONT;
                CSOSN_CONT = (decimal)registro.CSOSN_CONT;
                PEDE_CPF = registro.PEDE_CPF;
                _pERMITE_ESTOQUE_NEGATIVO = registro.PERMITE_ESTOQUE_NEGATIVO;
                _mODELO_CUPOM = registro.MODELO_CUPOM;
                MENSAGEM_RODAPE = registro.MENSAGEM_RODAPE;
                _mODELO_SAT = registro.IsMODELO_SATNull() ? -1 : registro.MODELO_SAT;
                _sATSERVIDOR = registro.IsSATSERVIDORNull() ? string.Empty : registro.SATSERVIDOR;
                SAT_CODATIV = registro.IsSAT_CODATIVNull() ? string.Empty : registro.SAT_CODATIV;
                SIGN_AC = registro.IsSIGN_ACNull() ? string.Empty : registro.SIGN_AC;
                _sAT_USADO = registro.IsSAT_USADONull() ? string.Empty : registro.SAT_USADO;
                _eCF_ATIVA = registro.IsECF_ATIVANull() ? string.Empty : registro.ECF_ATIVA;
                ECF_PORTA = registro.IsECF_PORTANull() ? string.Empty : registro.ECF_PORTA;
                IMPRESSORA_USB = registro.IsIMPRESSORA_USBNull() ? "Nenhuma" : registro.IMPRESSORA_USB;
                IMPRESSORA_USB_PED = registro.IsIMPRESSORA_USB_PEDNull() ? "Nenhuma" : registro.IMPRESSORA_USB_PED;
                PERGUNTA_WHATS = (PerguntaWhatsEnum)(registro.PERGUNTA_WHATS);//(PerguntaWhatsEnum)registro.PERGUNTA_WHATS1;
                _uSATEF = registro.USATEF;
                TEFIP = registro.IsTEFIPNull() ? null : registro.TEFIP;
                TEFNUMLOJA = registro.IsTEFNUMLOJANull() ? null : registro.TEFNUMLOJA;
                TEFNUMTERMINAL = registro.IsTEFNUMTERMINALNull() ? null : registro.TEFNUMTERMINAL;
                _tEFPEDECPFPELOPINPAD = registro.IsTEFPEDECPFPELOPINPADNull() ? null : registro.TEFPEDECPFPELOPINPAD;
                BALPORTA = registro.BALPORTA;
                BALBITS = registro.BALBITS;
                BALBAUD = registro.BALBAUD;
                BALPARITY = registro.BALPARITY;
                BALMODELO = registro.BALMODELO;
                ACFILLPREFIX = registro.ACFILLPREFIX;
                ACFILLMODE = registro.ACFILLMODE;
                ACREFERENCIA = registro.ACREFERENCIA;
                SYSCOMISSAO = registro.SYSCOMISSAO;
                SATSERVTIMEOUT = registro.SATSERVTIMEOUT;
                ACFILLDELAY = registro.ACFILLDELAY;
                SYSUSAWHATS = registro.SYSPERGUNTAWHATS;
                SYSPARCELA = registro.SYSPARCELA;
                SYSEMITECOMPROVANTE = registro.SYSEMITECOMPROVANTE;
                FDBDataSet.TRI_PDV_SETUPRow infoDoSetup = SETUP_TA.GetData()[0];
                COD10PORCENTO = infoDoSetup.IsCOD10PORCENTONull() ? -1 : infoDoSetup.COD10PORCENTO;
                _mODOBAR = infoDoSetup.MODOBAR;
                _tIPO_LICENCA = infoDoSetup.IsTIPO_LICENCANull() ? 1 : infoDoSetup.TIPO_LICENCA;
                _pEDESENHACANCEL = infoDoSetup.PEDESENHACANCEL;
                DESCONTO_MAXIMO = infoDoSetup.DESC_MAX_OP.Safedecimal();
                _uSARECARGAS = infoDoSetup.USA_RECARGAS;
                _uSA_COMANDA = infoDoSetup.USA_COMANDA;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Falha ao carregar as configurações", ex);
            }

            CONFIGURADO = true;

            return true;
        }

        #endregion Metodos
    }

    public class ConfiguracoesXML
    {
        private XmlSerializer serializer = new XmlSerializer(typeof(CONFIGURACOESXML));

        public void Serializa()
        {
            CONFIGURACOESXML cONFIGURACOESXML = new CONFIGURACOESXML() { FBTIMEOUT = ConfiguracoesPDV.FBTIMEOUT, LOGO = ConfiguracoesPDV.LOGO, NOMESOFTWARE = ConfiguracoesPDV.NOMESOFTWARE, SERVERCATALOG = ConfiguracoesPDV.SERVERCATALOG, SERVERNAME = ConfiguracoesPDV.SERVERNAME };
            var settings = new XmlWriterSettings() { Encoding = new UTF8Encoding(true), OmitXmlDeclaration = true, Indent = true };
            var XMLPendFinal = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(XMLPendFinal, settings))
            {
                var xns = new XmlSerializerNamespaces();
                xns.Add(string.Empty, string.Empty);
                serializer.Serialize(writer, cONFIGURACOESXML, xns);
            }
            File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}config.xml", XMLPendFinal.ToString());
        }

        public CONFIGURACOESXML Deserializa()
        {
            string xmlReadStream = File.ReadAllText($@"{AppDomain.CurrentDomain.BaseDirectory}config.xml");
            using var XmlRetorno = new StringReader(xmlReadStream);
            using var xreader = XmlReader.Create(XmlRetorno);
            return (CONFIGURACOESXML)serializer.Deserialize(xreader);
        }
    }
    public class CONFIGURACOESXML
    {
        public string LOGO { get; set; }
        public string NOMESOFTWARE { get; set; }
        public int FBTIMEOUT { get; set; }
        public string SERVERNAME { get; set; }
        public string SERVERCATALOG { get; set; }
        public int AUTORIZADO { get; set; }
        public int FECHAMENTO_EXTENDIDO { get; set; }
    }

}

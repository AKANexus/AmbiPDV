using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.DataSets;
using PDV_WPF.DataSets.FDBDataSetOperSeedTableAdapters;
using PDV_WPF.DataSets.FDBDataSetSistemaSeedTableAdapters;
using PDV_WPF.Exceptions;
using PDV_WPF.FDBDataSetTableAdapters;
using PDV_WPF.Telas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Transactions;
using System.Windows;
using PDV_WPF.REMENDOOOOO;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;

//TODO: revisar todas as funções "FillBy...()". O ideal é usar stored procedures. Isso em combinação com
//      índices no banco de dados, o ganho em desempenho é significativo.

namespace PDV_WPF.Funcoes
{
    #region Enums
    public enum EnmTipoSync { cadastros, vendas, fechamentos, tudo, exCadastrosInexistentes, CtrlS, dummy }
    public enum EnmDBSync { pdv, serv }
    public enum EnmFirstSyncStatus { incomplete, completed }
    #endregion Enums

    #region Aux Classes

    class AuxNfvFmaPgtoCtaRec
    {
        //public int PdvIdNfvenda { get; set; }
        //public int ServIdNfvenda { get; set; }

        //public int PdvIdCtaRec { get; set; }
        //public int ServIdCtaRec { get; set; }

        public int PdvIdNumPag { get; set; }
        public int ServIdNumPag { get; set; }
    }

    #endregion Aux Classes

    public class ComboBoxBindingDTO_Produto
    {
        /// <summary>
        /// TB_EST_PRODUTO
        /// </summary>
        public int ID_IDENTIFICADOR { get; set; }
        /// <summary>
        /// TB_ESTOQUE
        /// </summary>
        public string DESCRICAO { get; set; }
        /// <summary>
        /// TB_EST_PRODUTO
        /// </summary>
        public string COD_BARRA { get; set; }
        /// <summary>
        /// TB_EST_PRODUTO
        /// </summary>
        public string REFERENCIA { get; set; }
        public string STATUS { get; set; }
        //public decimal? QTD_ATACADO { get; set; }
        //public decimal PRC_VENDA { get; set; }
    }

    public class ComboBoxBindingDTO_Produto_Sync : ComboBoxBindingDTO_Produto
    {
        /// <summary>
        /// I, U ou D
        /// </summary>
        public string OPERACAO { get; set; }
        /// <summary>
        /// Nome da tabela de origem da alteração do registro.
        /// Usar esse valor para determinar se cada property deve ter seu valor alterado em ACBox.ItemsSource.
        /// </summary>
        public string ORIGEM_TB { get; set; }
    }
    public class SincronizadorDB
    {
        Logger log = new Logger("Sincronizador");
        #region Fields & Properties

        private string _strConnContingency { get; set; }
        private string _strConnNetwork { get; set; }
        private int _intNoCaixa { get; set; }
        private int _SyncTimeout { get; set; }

        #endregion Fields & Properties

        #region (De)Constructor

        public SincronizadorDB()
        {
            _strConnContingency = MontaStringDeConexao("localhost", localpath);
            _strConnNetwork = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
            _intNoCaixa = NO_CAIXA;
            _SyncTimeout = Properties.Settings.Default.SyncTimeout;
        }

        #endregion (De)Constructor

        #region Methods

        /// <summary>
        /// Pergunta pro usuário o local do banco do servidor para copiá-lo como banco do PDV.
        /// Configura cupons e movimentações para não serem sincronizados após isso.
        /// </summary>
        /// <returns></returns>
        public bool ConfigurarPrimeiraSync()
        {
            // Primeira sync, copiar o banco serv para o local
            // Setar valores de sync

            bool blnRetorno = false;
            log.Debug("blnRetorno é falso");

            try
            {
                log.Debug("Perguntando ao usuário o banco de dados a ser copiado...");
                var pergunta = new PerguntaUserServ();
                if (pergunta.ShowDialog().Equals(true))
                {
                    using (var tsFirstSync = new TransactionScope(TransactionScopeOption.Required,
                                                                  new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                    {
                        //TODO: O flag de indicação de sync deve ser informado no procedimento padrão de sync.

                        // Gravar a data da ultima sync para não engatilhar a sync completa depois
                        SetSetupUltimaSync(EnmDBSync.pdv, 0);
                        // Setar synced = 1 no TB_CUPOM
                        using (var taCupomPdv = new TB_CUPOMTableAdapter())
                        {
                            taCupomPdv.Connection.ConnectionString = _strConnContingency;
                            taCupomPdv.UpdateSyncedAll();
                        }
                        // Setar synced = 1 no TB_MOVDIARIO
                        using (var taMovdiarioPdv = new TB_MOVDIARIOTableAdapter())
                        {
                            taMovdiarioPdv.Connection.ConnectionString = _strConnContingency;
                            taMovdiarioPdv.UpdateSyncedAll();
                        }
                        // Setar synced = 1 no TB_NFVENDA
                        using (var taNfvendaPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter())
                        {
                            taNfvendaPdv.Connection.ConnectionString = _strConnContingency;
                            taNfvendaPdv.UpdateSyncedAll1();
                            taNfvendaPdv.UpdateSyncedAll2();
                        }

                        //TODO: Limpar TRI_PDV_AUX_SYNC para o número do caixa. -- TESTARRRR

                        if (NO_CAIXA > 0)
                        {
                            using (var taAuxSyncServ = new TRI_PDV_AUX_SYNCTableAdapter())
                            {
                                taAuxSyncServ.Connection.ConnectionString = _strConnNetwork;
                                taAuxSyncServ.DeleteByNoCaixa((short)NO_CAIXA); // passou a usar sproc
                            }
                        }

                        // Desativar as triggers de auxílio ao sync no local
                        using (var Config_TA = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter())
                        {
                            Config_TA.Connection.ConnectionString = _strConnContingency;

                            string DSBL_SERV_TRGGR_ON_PDV = (string)Config_TA.SP_TRI_DSBL_SERV_TRGGR_ON_PDV();
                            if (DSBL_SERV_TRGGR_ON_PDV != "deu certo")
                            {
                                //mensagem = "Erro ao desativar triggers de servidor no PDV";
                                throw new Exception($"Erro durante ConfigurarPrimeiraSync(), ao executar SP_TRI_DSBL_SERV_TRGGR_ON_PDV(): {DSBL_SERV_TRGGR_ON_PDV}");
                            }
                        }

                        tsFirstSync.Complete();
                    }

                    // Gravar um arquivo local para indicar que essa primeira sync foi executada com sucesso:
                    blnRetorno = IndicarFirstSyncStatus(EnmFirstSyncStatus.completed);
                }
                if (Properties.Settings.Default.SWATCode == "10-50") Properties.Settings.Default.SWATCode = "10-4";
            }
            catch (TransactionAbortedException taEx)
            {
                log.Error("TransactionAbortedException.", taEx);
            }
            catch (Exception ex)
            {
                log.Error("Erro ao configurar primeira sync", ex);
            }
            finally
            {
                if (!blnRetorno)
                {
                    // Gravar um arquivo local para indicar que essa primeira sync não foi concluída:
                    IndicarFirstSyncStatus(EnmFirstSyncStatus.incomplete);
                }
            }

            return blnRetorno;
        }

        ///// <summary>
        ///// Copia o banco do servidor para ser usado como um banco local (contingency).
        ///// </summary>
        ///// <returns>False se a cópia der pau</returns>
        //private bool CopiarDbServ()
        //{
        //    string ServerDB = SERVERCATALOG; //HACK: se mudar a estrutura da string de conexão, vai dar pau
        //    string LocalDB = localpath; //HACK: se mudar a estrutura da string de conexão, vai dar pau
        //    try
        //    {
        //        File.Copy(ServerDB, LocalDB, true);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        gravarMensagemErro("Erro ao copiar BD do servidor para o local: " + RetornarMensagemErro(ex, true));
        //        return false;
        //    }
        //}

        /// <summary>
        /// Grava um arquivo temporário (incomplete) ou permanente (completed) para 
        /// persistir o status da primeira sync.
        /// </summary>
        /// <param name="firstSyncStatus">Indicação do status da primeira sync</param>
        /// <returns></returns>
        private bool IndicarFirstSyncStatus(EnmFirstSyncStatus firstSyncStatus)
        {
            bool blnRetorno = false;
            string strStatus;
            switch (firstSyncStatus)
            {
                case EnmFirstSyncStatus.incomplete:
                    strStatus = "Incomplete";
                    break;
                case EnmFirstSyncStatus.completed:
                    strStatus = "Completed";
                    break;
                default:
                    throw new NotImplementedException("FirstSyncStatus não esperado!");
            }

            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\LocalDB\\FirstSync" + strStatus.ToString();
                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("Arquivo gerado automaticamente pelo AmbiPDV. Não remova.\n");
                        File.SetAttributes(path, FileAttributes.Hidden);
                    }
                }

                // Se o status for completed, verificar se o arquivo indicando o incomplete ainda existe.
                // Apaga se necessário:
                if (firstSyncStatus.Equals(EnmFirstSyncStatus.completed))
                {
                    string pathIncompleteFile = AppDomain.CurrentDomain.BaseDirectory + "\\LocalDB\\FirstSyncIncomplete";
                    if (File.Exists(pathIncompleteFile))
                    {
                        File.Delete(pathIncompleteFile);
                    }
                }

                blnRetorno = true;
            }
            catch (Exception ex)
            {
                log.Error("Erro ao indicar status da primeira sync", ex);
            }

            return blnRetorno;
        }

        /// <summary>
        /// Grava a data da última sync no banco indicado.
        /// Se o registro não existir, cria.
        /// </summary>
        /// <param name="banco">Servidor ou PDV</param>
        /// <param name="segundosTolerancia">Segundos que serão retirados da timestamp da última sincronização. 
        /// O normal (e ideal) é não ter uma tolerância. 
        /// Quanto maior esse valor, maior será o impacto na performance do sincronizador, pois atualizará no terminal 
        /// os mesmos últimos registros alterados na retaguarda durante esse período.</param>
        private void SetSetupUltimaSync(EnmDBSync banco, int segundosTolerancia)
        {
            // TRI_PDV_SETUP
            {
                using (var taSetup = new TRI_PDV_SETUPTableAdapter())
                {
                    string strOrigem = string.Empty;

                    switch (banco)
                    {
                        case EnmDBSync.pdv:
                            taSetup.Connection.ConnectionString = _strConnContingency;
                            strOrigem = "TERMINAL";
                            break;
                        case EnmDBSync.serv:
                            taSetup.Connection.ConnectionString = _strConnNetwork;
                            strOrigem = "SERVIDOR";
                            break;
                        default:
                            throw new NotImplementedException("Tipo de banco de dados não esperado!");
                    }

                    // Antes de gravar, verificar se há registro no banco:
                    using (var tblSetup = new FDBDataSet.TRI_PDV_SETUPDataTable())
                    {
                        taSetup.Fill(tblSetup).ToString();

                        //TODO: Verificar se o tempo de tolerância deve ser aplicado

                        DateTime tsUltimaSync = DateTime.Now.AddSeconds(segundosTolerancia * -1); //TODO: hmmm, será que não tem um jeito mais elegante de tornar negativo o número fornecido?

                        if (tblSetup != null && tblSetup.Rows.Count > 0)
                        {
                            log.Debug("taSetupPdv.UpdateUltimaSync(): " + taSetup.UpdateUltimaSync(tsUltimaSync).ToString());
                        }
                        else
                        {
                            log.Debug("taSetupPdv.Insert(): " + taSetup.Insert((short)_intNoCaixa,
                                                                              0,
                                                                              string.Empty,
                                                                              DateTime.MinValue,
                                                                              DateTime.MinValue,
                                                                              tsUltimaSync,
                                                                              strOrigem, 0, 0, "N", "0.0.0.0", "N", -1, "N", 1, "N", "S").ToString());
                        }
                    }
                }
            }
            log.Debug("Procedure done.");
        }

        #region Metodos de cadastro Serv-PDV

        public void Sync_Emitente(FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes)
        {
            DateTime? dtUltimaSyncPdv;
            var shtNumCaixa = (short)NO_CAIXA;

            using (var taSetupPdv = new TRI_PDV_SETUPTableAdapter())
            {
                taSetupPdv.Connection.ConnectionString = _strConnContingency;
                dtUltimaSyncPdv = (DateTime)taSetupPdv.GetUltimaSync();
            }
            // TB_EMITENTE
            {
                using (var tblEmitentePdv = new FDBDataSetOperSeed.TB_EMITENTEDataTable())
                using (var tblEmitenteServ = new FDBDataSetOperSeed.TB_EMITENTEDataTable())
                {
                    try
                    {
                        if (dtUltimaSyncPdv is null)
                        {
                            #region Única sync

                            using (var taEmitenteServidor = new TB_EMITENTETableAdapter())
                            using (var taEmitentePdv = new TB_EMITENTETableAdapter())
                            {
                                taEmitenteServidor.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                                taEmitenteServidor.Fill(tblEmitenteServ);

                                taEmitentePdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                                taEmitentePdv.Fill(tblEmitentePdv);

                                using (var changeReader = new DataTableReader(tblEmitenteServ))
                                    tblEmitentePdv.Load(changeReader, LoadOption.Upsert);
                                SqlCommand cmd = new SqlCommand();
                                try
                                {
                                    cmd.CommandText = "UPDATE TB_EMITENTE SET NOME = @NOME, NOME_FANTA = @NOME_FANTA, CONTATO = @CONTATO, END_CEP = @END_CEP, END_TIPO = @END_TIPO, END_LOGRAD = @END_LOGRAD, END_NUMERO = @END_NUMERO, END_COMPLE = @END_COMPLE, END_BAIRRO = @END_BAIRRO,INSC_ESTAD = @INSC_ESTAD,	INSC_MUNIC = @INSC_MUNIC, DDD_COMER = @DDD_COMER, FONE_COMER = @FONE_COMER,	DDD_FAX = @DDD_FAX,	FONE_FAX = @FONE_FAX, DDD_CELUL = @DDD_CELUL, FONE_CELUL = @FONE_CELUL,	EMAIL_CONT = @EMAIL_CONT, SITE = @SITE,	CNAE = @CNAE, SIMPLES = @SIMPLES, ID_CIDADE = @ID_CIDADE, ID_RAMO = @ID_RAMO, DT_COMPRA = @DT_COMPRA, LOGO = @LOGO,	IE_ST_AC = @IE_ST_AC, IE_ST_AL = @IE_ST_AL,	IE_ST_AM = @IE_ST_AM, IE_ST_AP = @IE_ST_AP,	IE_ST_BA = @IE_ST_BA, IE_ST_CE = @IE_ST_CE,	IE_ST_DF = @IE_ST_DF, IE_ST_ES = @IE_ST_ES,	IE_ST_GO = @IE_ST_GO, IE_ST_MA = @IE_ST_MA,	IE_ST_MG = @IE_ST_MG, IE_ST_MS = @IE_ST_MS,	IE_ST_MT = @IE_ST_MT, IE_ST_PA = @IE_ST_PA, IE_ST_PB = @IE_ST_PB,IE_ST_PE = @IE_ST_PE,	IE_ST_PI = @IE_ST_PI,IE_ST_PR = @IE_ST_PR, IE_ST_RJ = @IE_ST_RJ, IE_ST_RN = @IE_ST_RN, IE_ST_RO = @IE_ST_RO, IE_ST_RR = @IE_ST_RR, IE_ST_RS = @IE_ST_RS, IE_ST_SC = @IE_ST_SC, IE_ST_SE = @IE_ST_SE, IE_ST_SP = @IE_ST_SP,	IE_ST_TO = @IE_ST_TO, CNPJ = @CNPJ;";
                                    cmd.Parameters.AddWithValue("@NOME", tblEmitentePdv[0].NOME);
                                    cmd.Parameters.AddWithValue("@NOME_FANTA", tblEmitentePdv[0].NOME_FANTA);
                                    cmd.Parameters.AddWithValue("@CONTATO", tblEmitentePdv[0].CONTATO);
                                    cmd.Parameters.AddWithValue("@END_CEP", tblEmitentePdv[0].END_CEP);
                                    cmd.Parameters.AddWithValue("@END_TIPO", tblEmitentePdv[0].END_TIPO);
                                    cmd.Parameters.AddWithValue("@END_LOGRAD", tblEmitentePdv[0].END_LOGRAD);
                                    cmd.Parameters.AddWithValue("@END_NUMERO", tblEmitentePdv[0].END_NUMERO);
                                    cmd.Parameters.AddWithValue("@END_COMPLE", tblEmitentePdv[0].END_COMPLE);
                                    cmd.Parameters.AddWithValue("@END_BAIRRO", tblEmitentePdv[0].END_BAIRRO);
                                    cmd.Parameters.AddWithValue("@INSC_ESTAD", tblEmitentePdv[0].INSC_ESTAD);
                                    cmd.Parameters.AddWithValue("@INSC_MUNIC", tblEmitentePdv[0].INSC_MUNIC);
                                    cmd.Parameters.AddWithValue("@DDD_COMER", tblEmitentePdv[0].DDD_COMER);
                                    cmd.Parameters.AddWithValue("@FONE_COMER", tblEmitentePdv[0].FONE_COMER);
                                    cmd.Parameters.AddWithValue("@DDD_FAX", tblEmitentePdv[0].DDD_FAX);
                                    cmd.Parameters.AddWithValue("@FONE_FAX", tblEmitentePdv[0].FONE_FAX);
                                    cmd.Parameters.AddWithValue("@DDD_CELUL", tblEmitentePdv[0].DDD_CELUL);
                                    cmd.Parameters.AddWithValue("@FONE_CELUL", tblEmitentePdv[0].FONE_CELUL);
                                    cmd.Parameters.AddWithValue("@EMAIL_CONT", tblEmitentePdv[0].EMAIL_CONT);
                                    cmd.Parameters.AddWithValue("@SITE", tblEmitentePdv[0].SITE);
                                    cmd.Parameters.AddWithValue("@CNAE", tblEmitentePdv[0].CNAE);
                                    cmd.Parameters.AddWithValue("@SIMPLES", tblEmitentePdv[0].SIMPLES);
                                    cmd.Parameters.AddWithValue("@ID_CIDADE", tblEmitentePdv[0].ID_CIDADE);
                                    cmd.Parameters.AddWithValue("@ID_RAMO", tblEmitentePdv[0].ID_RAMO);
                                    cmd.Parameters.AddWithValue("@DT_COMPRA", tblEmitentePdv[0].DT_COMPRA);
                                    cmd.Parameters.AddWithValue("@LOGO", tblEmitentePdv[0].LOGO);
                                    cmd.Parameters.AddWithValue("@IE_ST_AC", tblEmitentePdv[0].IE_ST_AC);
                                    cmd.Parameters.AddWithValue("@IE_ST_AL", tblEmitentePdv[0].IE_ST_AL);
                                    cmd.Parameters.AddWithValue("@IE_ST_AM", tblEmitentePdv[0].IE_ST_AM);
                                    cmd.Parameters.AddWithValue("@IE_ST_AP", tblEmitentePdv[0].IE_ST_AP);
                                    cmd.Parameters.AddWithValue("@IE_ST_BA", tblEmitentePdv[0].IE_ST_BA);
                                    cmd.Parameters.AddWithValue("@IE_ST_CE", tblEmitentePdv[0].IE_ST_CE);
                                    cmd.Parameters.AddWithValue("@IE_ST_DF", tblEmitentePdv[0].IE_ST_DF);
                                    cmd.Parameters.AddWithValue("@IE_ST_ES", tblEmitentePdv[0].IE_ST_ES);
                                    cmd.Parameters.AddWithValue("@IE_ST_GO", tblEmitentePdv[0].IE_ST_GO);
                                    cmd.Parameters.AddWithValue("@IE_ST_MA", tblEmitentePdv[0].IE_ST_MA);
                                    cmd.Parameters.AddWithValue("@IE_ST_MA", tblEmitentePdv[0].IE_ST_MS);
                                    cmd.Parameters.AddWithValue("@IE_ST_MT", tblEmitentePdv[0].IE_ST_MT);
                                    cmd.Parameters.AddWithValue("@IE_ST_PA", tblEmitentePdv[0].IE_ST_PA);
                                    cmd.Parameters.AddWithValue("@IE_ST_PA", tblEmitentePdv[0].IE_ST_PA);
                                    cmd.Parameters.AddWithValue("@IE_ST_PE", tblEmitentePdv[0].IE_ST_PE);
                                    cmd.Parameters.AddWithValue("@IE_ST_PI", tblEmitentePdv[0].IE_ST_PI);
                                    cmd.Parameters.AddWithValue("@IE_ST_PR", tblEmitentePdv[0].IE_ST_PR);
                                    cmd.Parameters.AddWithValue("@IE_ST_RJ", tblEmitentePdv[0].IE_ST_RJ);
                                    cmd.Parameters.AddWithValue("@IE_ST_RN", tblEmitentePdv[0].IE_ST_RN);
                                    cmd.Parameters.AddWithValue("@IE_ST_RO", tblEmitentePdv[0].IE_ST_RO);
                                    cmd.Parameters.AddWithValue("@IE_ST_RR", tblEmitentePdv[0].IE_ST_RR);
                                    cmd.Parameters.AddWithValue("@IE_ST_RS", tblEmitentePdv[0].IE_ST_RS);
                                    cmd.Parameters.AddWithValue("@IE_ST_SC", tblEmitentePdv[0].IE_ST_SC);
                                    cmd.Parameters.AddWithValue("@IE_ST_SE", tblEmitentePdv[0].IE_ST_SE);
                                    cmd.Parameters.AddWithValue("@IE_ST_SP", tblEmitentePdv[0].IE_ST_SP);
                                    cmd.Parameters.AddWithValue("@IE_ST_TO", tblEmitentePdv[0].IE_ST_TO);
                                    cmd.Parameters.AddWithValue("@CNPJ", tblEmitentePdv[0].CNPJ);


                                }
                                catch (Exception)
                                {
                                    MessageBox.Show("Ocorreu um erro na V_EMITENTE, verificar o sincronizador", "Atenção");
                                }
                                // Salva dados do Serv para o PDV:
                                // taEmitentePdv.Update(tblEmitentePdv);

                                tblEmitentePdv.AcceptChanges();
                            }

                            #endregion Única sync
                        }
                        else
                        {
                            #region Sync de cadastros novos ou atualizados

                            #region AUX_SYNC

                            int intRowsAffected = 0;

                            {
                                DataRow[] pendentesEmitente = dtAuxSyncPendentes.Select($"TABELA = 'TB_EMITENTE'");

                                //for (int i = 0; i < dtAuxSyncEmitente.Rows.Count; i++)
                                for (int i = 0; i < pendentesEmitente.Length; i++)
                                {
                                    var cnpj = pendentesEmitente/*dtAuxSyncEmitente.Rows*/[i]["UN_REG"].Safestring();
                                    var operacao = pendentesEmitente/*dtAuxSyncEmitente.Rows*/[i]["OPERACAO"].Safestring();

                                    // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                    if (operacao.Equals("I") || operacao.Equals("U"))
                                    {
                                        // Buscar o registro para executar as operações "Insert" ou "Update"

                                        using (var taEmitenteServ = new TB_EMITENTETableAdapter())
                                        {
                                            taEmitenteServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                            taEmitenteServ.FillByCnpj(tblEmitenteServ, cnpj);

                                            if (tblEmitenteServ != null && tblEmitenteServ.Rows.Count > 0)
                                            {
                                                using (var taEmitentePdv = new TB_EMITENTETableAdapter())
                                                {
                                                    taEmitentePdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                    foreach (FDBDataSetOperSeed.TB_EMITENTERow triEmitenteServ in tblEmitenteServ)
                                                    {
                                                        intRowsAffected = (int)taEmitentePdv.SP_TRI_EMITENTE_UPSERT(triEmitenteServ.NOME,
                                                                             (triEmitenteServ.IsNOME_FANTANull() ? null : triEmitenteServ.NOME_FANTA),
                                                                             (triEmitenteServ.IsCONTATONull() ? null : triEmitenteServ.CONTATO),
                                                                             triEmitenteServ.END_CEP,
                                                                             triEmitenteServ.END_TIPO,
                                                                             triEmitenteServ.END_LOGRAD,
                                                                             triEmitenteServ.END_NUMERO,
                                                                             (triEmitenteServ.IsEND_COMPLENull() ? null : triEmitenteServ.END_COMPLE),
                                                                             triEmitenteServ.END_BAIRRO,
                                                                             triEmitenteServ.CNPJ,
                                                                             (triEmitenteServ.IsINSC_ESTADNull() ? null : triEmitenteServ.INSC_ESTAD),
                                                                             (triEmitenteServ.IsINSC_MUNICNull() ? null : triEmitenteServ.INSC_MUNIC),
                                                                             triEmitenteServ.DDD_COMER,
                                                                             triEmitenteServ.FONE_COMER,
                                                                             (triEmitenteServ.IsDDD_FAXNull() ? null : triEmitenteServ.DDD_FAX),
                                                                             (triEmitenteServ.IsFONE_FAXNull() ? null : triEmitenteServ.FONE_FAX),
                                                                             (triEmitenteServ.IsDDD_CELULNull() ? null : triEmitenteServ.DDD_CELUL),
                                                                             (triEmitenteServ.IsFONE_CELULNull() ? null : triEmitenteServ.FONE_CELUL),
                                                                             (triEmitenteServ.IsEMAIL_CONTNull() ? null : triEmitenteServ.EMAIL_CONT),
                                                                             (triEmitenteServ.IsSITENull() ? null : triEmitenteServ.SITE),
                                                                             (triEmitenteServ.IsCNAENull() ? null : triEmitenteServ.CNAE),
                                                                             triEmitenteServ.SIMPLES,
                                                                             triEmitenteServ.ID_CIDADE,
                                                                             triEmitenteServ.ID_RAMO,
                                                                             (triEmitenteServ.IsDT_COMPRANull() ? null : (DateTime?)triEmitenteServ.DT_COMPRA),
                                                                             (triEmitenteServ.IsLOGONull() ? null : triEmitenteServ.LOGO),
                                                                             (triEmitenteServ.IsIE_ST_ACNull() ? null : triEmitenteServ.IE_ST_AC),
                                                                             (triEmitenteServ.IsIE_ST_ALNull() ? null : triEmitenteServ.IE_ST_AL),
                                                                             (triEmitenteServ.IsIE_ST_AMNull() ? null : triEmitenteServ.IE_ST_AM),
                                                                             (triEmitenteServ.IsIE_ST_APNull() ? null : triEmitenteServ.IE_ST_AP),
                                                                             (triEmitenteServ.IsIE_ST_BANull() ? null : triEmitenteServ.IE_ST_BA),
                                                                             (triEmitenteServ.IsIE_ST_CENull() ? null : triEmitenteServ.IE_ST_CE),
                                                                             (triEmitenteServ.IsIE_ST_DFNull() ? null : triEmitenteServ.IE_ST_DF),
                                                                             (triEmitenteServ.IsIE_ST_ESNull() ? null : triEmitenteServ.IE_ST_ES),
                                                                             (triEmitenteServ.IsIE_ST_GONull() ? null : triEmitenteServ.IE_ST_GO),
                                                                             (triEmitenteServ.IsIE_ST_MANull() ? null : triEmitenteServ.IE_ST_MA),
                                                                             (triEmitenteServ.IsIE_ST_MGNull() ? null : triEmitenteServ.IE_ST_MG),
                                                                             (triEmitenteServ.IsIE_ST_MSNull() ? null : triEmitenteServ.IE_ST_MS),
                                                                             (triEmitenteServ.IsIE_ST_MTNull() ? null : triEmitenteServ.IE_ST_MT),
                                                                             (triEmitenteServ.IsIE_ST_PANull() ? null : triEmitenteServ.IE_ST_PA),
                                                                             (triEmitenteServ.IsIE_ST_PBNull() ? null : triEmitenteServ.IE_ST_PB),
                                                                             (triEmitenteServ.IsIE_ST_PENull() ? null : triEmitenteServ.IE_ST_PE),
                                                                             (triEmitenteServ.IsIE_ST_PINull() ? null : triEmitenteServ.IE_ST_PI),
                                                                             (triEmitenteServ.IsIE_ST_PRNull() ? null : triEmitenteServ.IE_ST_PR),
                                                                             (triEmitenteServ.IsIE_ST_RJNull() ? null : triEmitenteServ.IE_ST_RJ),
                                                                             (triEmitenteServ.IsIE_ST_RNNull() ? null : triEmitenteServ.IE_ST_RN),
                                                                             (triEmitenteServ.IsIE_ST_RONull() ? null : triEmitenteServ.IE_ST_RO),
                                                                             (triEmitenteServ.IsIE_ST_RRNull() ? null : triEmitenteServ.IE_ST_RR),
                                                                             (triEmitenteServ.IsIE_ST_RSNull() ? null : triEmitenteServ.IE_ST_RS),
                                                                             (triEmitenteServ.IsIE_ST_SCNull() ? null : triEmitenteServ.IE_ST_SC),
                                                                             (triEmitenteServ.IsIE_ST_SENull() ? null : triEmitenteServ.IE_ST_SE),
                                                                             (triEmitenteServ.IsIE_ST_SPNull() ? null : triEmitenteServ.IE_ST_SP),
                                                                             (triEmitenteServ.IsIE_ST_TONull() ? null : triEmitenteServ.IE_ST_TO),
                                                                            null, //(triEmitenteServ.IsTEXTO_COBRANCANull() ? null : triEmitenteServ.TEXTO_COBRANCA),
                                                                           null, //(triEmitenteServ.IsTEXTO_COBRANCA_RODAPENull() ? null : triEmitenteServ.TEXTO_COBRANCA_RODAPE),
                                                                            null, //(triEmitenteServ.IsTEXTO_COBRANCA_ASSUNTONull() ? null : triEmitenteServ.TEXTO_COBRANCA_ASSUNTO),
                                                                             DateTime.Now);

                                                        // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.

                                                        if (intRowsAffected.Equals(1))
                                                        {
                                                            ConfirmarAuxSync(-1,
                                                                             "TB_EMITENTE",
                                                                             operacao,
                                                                             shtNumCaixa,
                                                                             cnpj);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                #region Não há delete para TB_EMITENTE

                                                //// O item não foi encontrado no servidor.
                                                //// Pode ter sido deletado.
                                                //// Deve constar essa operação em dtAuxSync.
                                                //// Caso contrário, estourar exception.

                                                //DataRow[] deletesPendentesTriMetodos = dtAuxSyncTriMetodos.Select($"ID_REG = {idPagamento} AND OPERACAO = 'D'");

                                                //if (deletesPendentesTriMetodos.Length > 0)
                                                //{
                                                //    foreach (var deletePendenteTriMetodos in deletesPendentesTriMetodos)
                                                //    {
                                                //        dtAuxSyncDeletesPendentes.Rows.Add(0, idPagamento, "TRI_PDV_METODOS", "D", shtNumCaixa);
                                                //    }
                                                //}
                                                //else
                                                //{
                                                //    // Ops....
                                                //    // Item não encontrado no servidor e não foi deletado?
                                                //    // Estourar exception.
                                                //    throw new DataException($"Erro não esperado: item (TRI_PDV_METODOS) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idPagamento}");
                                                //}

                                                #endregion Não há delete para TB_EMITENTE
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Não é uma operação "padrão"

                                        #region Não há delete para TB_EMITENTE

                                        // DEU RUIM

                                        throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido (TB_EMITENTE) { operacao }");

                                        //switch (operacao)
                                        //{
                                        //    case "D":
                                        //        {
                                        //            // Não dá pra deletar agora por causa das constraints (FK).
                                        //            // Adicionar numa lista e deletar depois, na ordem correta.

                                        //            // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                        //            DataRow[] deletesPendentesTriMetodos = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idPagamento} AND TABELA = 'TRI_PDV_METODOS' AND OPERACAO = 'D'");

                                        //            if (deletesPendentesTriMetodos.Length <= 0)
                                        //            {
                                        //                dtAuxSyncDeletesPendentes.Rows.Add(0, idPagamento, "TRI_PDV_METODOS", "D", shtNumCaixa);
                                        //            }

                                        //            break;
                                        //        }
                                        //    default:
                                        //        throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                        //        //break;
                                        //}

                                        #endregion Não há delete para TB_EMITENTE
                                    }
                                }
                            }

                            #endregion AUX_SYNC

                            #endregion Sync de cadastros novos ou atualizados
                        }
                    }
                    catch (NotImplementedException niex)
                    {
                        log.Error("Método não implementado", niex);
                        throw niex;
                    }
                    catch (DataException dex)
                    {
                        log.Error("Data exception", dex);
                        throw dex;
                    }
                    catch (Exception ex)
                    {
                        GravarErroSync("Emitente(PDV)", tblEmitentePdv, ex);
                        GravarErroSync("Emitente(SERV)", tblEmitenteServ, ex);
                        throw ex;
                    }
                }
            }
        }

        public void Sync_TB_TAXA_UF(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes)
        {
            var shtNumCaixa = (short)NO_CAIXA;

            using (var tblTaxaUfServ = new FDBDataSetOperSeed.TB_TAXA_UFDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taTaxaServ = new TB_TAXA_UFTableAdapter())
                        using (var tblTaxaServ = new FDBDataSetOperSeed.TB_TAXA_UFDataTable())
                        using (var taTaxaPdv = new TB_TAXA_UFTableAdapter())
                        using (var tblTaxaPdv = new FDBDataSetOperSeed.TB_TAXA_UFDataTable())
                        {
                            taTaxaServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taTaxaServ.Fill(tblTaxaServ);

                            taTaxaPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taTaxaPdv.Fill(tblTaxaPdv);

                            using (var changeReader = new DataTableReader(tblTaxaServ))
                                tblTaxaPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taTaxaPdv.Update(tblTaxaPdv);

                            tblTaxaPdv.AcceptChanges();
                        }
                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesTaxaUf = dtAuxSyncPendentes.Select($"TABELA = 'TB_TAXA_UF'");

                            for (int i = 0; i < pendentesTaxaUf.Length; i++)
                            {
                                var idCti = pendentesTaxaUf[i]["CH_REG"].Safestring();
                                var operacao = pendentesTaxaUf[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taTaxaUfServ = new TB_TAXA_UFTableAdapter())
                                    {
                                        taTaxaUfServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taTaxaUfServ.FillById(tblTaxaUfServ, idCti);

                                        if (tblTaxaUfServ != null && tblTaxaUfServ.Rows.Count > 0)
                                        {
                                            using (var taTaxaUfPdv = new TB_TAXA_UFTableAdapter())
                                            {
                                                taTaxaUfPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetOperSeed.TB_TAXA_UFRow taxaUfServ in tblTaxaUfServ)
                                                {
                                                    intRetornoUpsert = (int)taTaxaUfPdv.SP_TRI_TAXAUF_UPSERT(taxaUfServ.ID_CTI,
                                                                         (taxaUfServ.IsDESCRICAONull() ? null : taxaUfServ.DESCRICAO),
                                                                         (taxaUfServ.IsBASE_ICMSNull() ? null : (decimal?)taxaUfServ.BASE_ICMS),
                                                                         (taxaUfServ.IsBASE_ICMSFENull() ? null : (decimal?)taxaUfServ.BASE_ICMSFE),
                                                                         (taxaUfServ.IsBASE_ICMS_STNull() ? null : (decimal?)taxaUfServ.BASE_ICMS_ST),
                                                                         (taxaUfServ.IsUF_ACNull() ? null : (decimal?)taxaUfServ.UF_AC),
                                                                         (taxaUfServ.IsUF_ALNull() ? null : (decimal?)taxaUfServ.UF_AL),
                                                                         (taxaUfServ.IsUF_AMNull() ? null : (decimal?)taxaUfServ.UF_AM),
                                                                         (taxaUfServ.IsUF_APNull() ? null : (decimal?)taxaUfServ.UF_AP),
                                                                         (taxaUfServ.IsUF_BANull() ? null : (decimal?)taxaUfServ.UF_BA),
                                                                         (taxaUfServ.IsUF_CENull() ? null : (decimal?)taxaUfServ.UF_CE),
                                                                         (taxaUfServ.IsUF_DFNull() ? null : (decimal?)taxaUfServ.UF_DF),
                                                                         (taxaUfServ.IsUF_ESNull() ? null : (decimal?)taxaUfServ.UF_ES),
                                                                         (taxaUfServ.IsUF_GONull() ? null : (decimal?)taxaUfServ.UF_GO),
                                                                         (taxaUfServ.IsUF_MANull() ? null : (decimal?)taxaUfServ.UF_MA),
                                                                         (taxaUfServ.IsUF_MGNull() ? null : (decimal?)taxaUfServ.UF_MG),
                                                                         (taxaUfServ.IsUF_MSNull() ? null : (decimal?)taxaUfServ.UF_MS),
                                                                         (taxaUfServ.IsUF_MTNull() ? null : (decimal?)taxaUfServ.UF_MT),
                                                                         (taxaUfServ.IsUF_PANull() ? null : (decimal?)taxaUfServ.UF_PA),
                                                                         (taxaUfServ.IsUF_PBNull() ? null : (decimal?)taxaUfServ.UF_PB),
                                                                         (taxaUfServ.IsUF_PENull() ? null : (decimal?)taxaUfServ.UF_PE),
                                                                         (taxaUfServ.IsUF_PINull() ? null : (decimal?)taxaUfServ.UF_PI),
                                                                         (taxaUfServ.IsUF_PRNull() ? null : (decimal?)taxaUfServ.UF_PR),
                                                                         (taxaUfServ.IsUF_RJNull() ? null : (decimal?)taxaUfServ.UF_RJ),
                                                                         (taxaUfServ.IsUF_RNNull() ? null : (decimal?)taxaUfServ.UF_RN),
                                                                         (taxaUfServ.IsUF_RONull() ? null : (decimal?)taxaUfServ.UF_RO),
                                                                         (taxaUfServ.IsUF_RRNull() ? null : (decimal?)taxaUfServ.UF_RR),
                                                                         (taxaUfServ.IsUF_RSNull() ? null : (decimal?)taxaUfServ.UF_RS),
                                                                         (taxaUfServ.IsUF_SCNull() ? null : (decimal?)taxaUfServ.UF_SC),
                                                                         (taxaUfServ.IsUF_SENull() ? null : (decimal?)taxaUfServ.UF_SE),
                                                                         (taxaUfServ.IsUF_SPNull() ? null : (decimal?)taxaUfServ.UF_SP),
                                                                         (taxaUfServ.IsUF_TONull() ? null : (decimal?)taxaUfServ.UF_TO),
                                                                         (taxaUfServ.IsBASE_ISSNull() ? null : (decimal?)taxaUfServ.BASE_ISS),
                                                                         (taxaUfServ.IsISSNull() ? null : (decimal?)taxaUfServ.ISS),
                                                                         taxaUfServ.POR_DIF,
                                                                         DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(-1,
                                                                     "TB_TAXA_UF",
                                                                     operacao,
                                                                     shtNumCaixa,
                                                                     null,
                                                                     -1,
                                                                     idCti);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesTaxaUf = pendentesTaxaUf.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesTaxaUf = dtPendentesTaxaUf.Select($"CH_REG = '{idCti}' AND OPERACAO = 'D'");

                                                if (deletesPendentesTaxaUf.Length > 0)
                                                {
                                                    foreach (var deletePendenteTaxaUf in deletesPendentesTaxaUf)
                                                    {
                                                        // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_TAXA_UF", "D", shtNumCaixa, null, null, -1, idCti);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TB_TAXA_UF) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idCti}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesTaxaUf = dtAuxSyncDeletesPendentes.Select($"CH_REG = '{idCti}' AND TABELA = 'TB_TAXA_UF' AND OPERACAO = 'D'");

                                                if (deletesPendentesTaxaUf.Length <= 0)
                                                {
                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_TAXA_UF", "D", shtNumCaixa, null, null, -1, idCti);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (Exception ex)
                {
                    GravarErroSync("Taxa UF", tblTaxaUfServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_CFOP_SIS(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblCfopSisServ = new FDBDataSetSistemaSeed.TB_CFOP_SISDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taCfopServ = new TB_CFOP_SISTableAdapter())
                        using (var tblCfopServ = new FDBDataSetSistemaSeed.TB_CFOP_SISDataTable())
                        using (var taCfopPdv = new TB_CFOP_SISTableAdapter())
                        using (var tblCfopPdv = new FDBDataSetSistemaSeed.TB_CFOP_SISDataTable())
                        {
                            taCfopServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taCfopServ.Fill(tblCfopServ);

                            taCfopPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taCfopPdv.Fill(tblCfopPdv);

                            using (var changeReader = new DataTableReader(tblCfopServ))
                                tblCfopPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taCfopPdv.Update(tblCfopPdv);

                            tblCfopPdv.AcceptChanges();
                        }

                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesCfopSis = dtAuxSyncPendentes.Select($"TABELA = 'TB_CFOP_SIS'");

                            for (int i = 0; i < pendentesCfopSis.Length; i++)
                            {
                                var cfop = pendentesCfopSis[i]["UN_REG"].Safestring();
                                var operacao = pendentesCfopSis[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taCfopServ = new TB_CFOP_SISTableAdapter())
                                    {
                                        taCfopServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taCfopServ.FillById(tblCfopSisServ, cfop);

                                        if (tblCfopSisServ != null && tblCfopSisServ.Rows.Count > 0)
                                        {
                                            using (var taCfopSisPdv = new TB_CFOP_SISTableAdapter())
                                            {
                                                taCfopSisPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetSistemaSeed.TB_CFOP_SISRow cfopSisServ in tblCfopSisServ)
                                                {
                                                    intRetornoUpsert = (int)taCfopSisPdv.SP_TRI_CFOPSIS_UPSERT(cfopSisServ.CFOP,
                                                                                       (cfopSisServ.IsDESCRICAONull() ? null : cfopSisServ.DESCRICAO),
                                                                                       (cfopSisServ.IsRESUMONull() ? null : cfopSisServ.RESUMO),
                                                                                       (cfopSisServ.IsOBSERVACAONull() ? null : cfopSisServ.OBSERVACAO),
                                                                                       cfopSisServ.EST_BX,
                                                                                       (cfopSisServ.IsEST_BX_AMBOSNull() ? null : cfopSisServ.EST_BX_AMBOS),
                                                                                       (cfopSisServ.IsDEV_RETNull() ? null : cfopSisServ.DEV_RET),
                                                                                       DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(-1,
                                                                     "TB_CFOP_SIS",
                                                                     operacao,
                                                                     shtNumCaixa,
                                                                     cfop);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesCfopSis = pendentesCfopSis.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesCfopSis = dtPendentesCfopSis.Select($"UN_REG = '{cfop}' AND OPERACAO = 'D'");

                                                if (deletesPendentesCfopSis.Length > 0)
                                                {
                                                    foreach (var deletePendenteCfopSis in deletesPendentesCfopSis)
                                                    {
                                                        // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_CFOP_SIS", "D", shtNumCaixa, null, cfop, -1, null);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TB_CFOP_SIS) não encontrado no servidor e sem exclusão pendente. \nID do registro: {cfop}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                DataRow[] deletesPendentesCfopSis = dtAuxSyncDeletesPendentes.Select($"UN_REG = '{cfop}' AND TABELA = 'TB_CFOP_SIS' AND OPERACAO = 'D'");

                                                if (deletesPendentesCfopSis.Length <= 0)
                                                {
                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_CFOP_SIS", "D", shtNumCaixa, null, cfop, -1, null);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (Exception ex)
                {
                    //audit("SINCCONTNETDB>> " + "Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("CFOP", tblCfopSisServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_NAT_OPERACAO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblNatOperServ = new FDBDataSetSistemaSeed.TB_NAT_OPERACAODataTable())

            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taNatOperServ = new TB_NAT_OPERACAOTableAdapter())
                        //using (var tblNatOperServ = new FDBDataSetSistemaSeed.TB_NAT_OPERACAODataTable())
                        using (var taNatOperPdv = new TB_NAT_OPERACAOTableAdapter())
                        using (var tblNatOperPdv = new FDBDataSetSistemaSeed.TB_NAT_OPERACAODataTable())
                        {
                            taNatOperServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taNatOperServ.Fill(tblNatOperServ);

                            taNatOperPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taNatOperPdv.Fill(tblNatOperPdv);

                            using (var changeReader = new DataTableReader(tblNatOperServ))
                                tblNatOperPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taNatOperPdv.Update(tblNatOperPdv);

                            tblNatOperPdv.AcceptChanges();
                        }

                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesNatOper = dtAuxSyncPendentes.Select($"TABELA = 'TB_NAT_OPERACAO'");

                            for (int i = 0; i < pendentesNatOper.Length; i++)
                            {
                                var natOper = pendentesNatOper[i]["ID_REG"].Safeint();
                                var operacao = pendentesNatOper[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taNatOperServ = new TB_NAT_OPERACAOTableAdapter())
                                    {
                                        taNatOperServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taNatOperServ.FillById(tblNatOperServ, natOper);

                                        if (tblNatOperServ != null && tblNatOperServ.Rows.Count > 0)
                                        {
                                            using (var taNatOperPdv = new TB_NAT_OPERACAOTableAdapter())
                                            {
                                                taNatOperPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetSistemaSeed.TB_NAT_OPERACAORow natOperServ in tblNatOperServ)
                                                {
                                                    intRetornoUpsert = (int)taNatOperPdv.SP_TRI_NATOPER_UPSERT(natOperServ.ID_NATOPE,
                                                                                                    natOperServ.DESCRICAO,
                                                                                                    (natOperServ.IsRET_PIS_COF_CSLLNull() ? null : natOperServ.RET_PIS_COF_CSLL),
                                                                                                    (natOperServ.IsRET_INSSNull() ? null : natOperServ.RET_INSS),
                                                                                                    (natOperServ.IsRET_IRRFNull() ? null : natOperServ.RET_IRRF),
                                                                                                    natOperServ.IsPIS_COFINSNull() ? null : natOperServ.PIS_COFINS,
                                                                                                    natOperServ.STATUS,
                                                                                                    (natOperServ.IsCFOPNull() ? null : natOperServ.CFOP),
                                                                                                    (natOperServ.IsID_CTINull() ? null : natOperServ.ID_CTI),
                                                                                                    (natOperServ.IsGFRNull() ? null : natOperServ.GFR),
                                                                                                    (natOperServ.IsOBSERVACAONull() ? null : natOperServ.OBSERVACAO),
                                                                                                    (natOperServ.IsBASE_COMISSAONull() ? null : (decimal?)natOperServ.BASE_COMISSAO),
                                                                                                    natOperServ.CALCULA_IPI,
                                                                                                    natOperServ.GRAVA_TOT_TRIBUTOS);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(natOper,
                                                                     "TB_NAT_OPERACAO",
                                                                     operacao,
                                                                     shtNumCaixa);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesNatOper = pendentesNatOper.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesNatOper = dtPendentesNatOper.Select($"ID_REG = {natOper} AND OPERACAO = 'D'");

                                                if (deletesPendentesNatOper.Length > 0)
                                                {
                                                    foreach (var deletePendenteNatOper in deletesPendentesNatOper)
                                                    {
                                                        // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, natOper, "TB_NAT_OPERACAO", "D", shtNumCaixa, null, null, -1, null);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TB_NAT_OPERACAO) não encontrado no servidor e sem exclusão pendente. \nID do registro: {natOper}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                DataRow[] deletesPendentesNatOperSis = dtAuxSyncDeletesPendentes.Select($"ID_REG = {natOper} AND TABELA = 'TB_NAT_OPERACAO' AND OPERACAO = 'D'");

                                                if (deletesPendentesNatOperSis.Length <= 0)
                                                {
                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, natOper, "TB_NAT_OPERACAO", "D", shtNumCaixa, null, null, -1, null);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (Exception ex)
                {
                    //audit("SINCCONTNETDB>> " + "Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("TB_NAT_OPERACAO", tblNatOperServ, ex);
                    throw ex;
                }
            }

        }

        public void Sync_TB_EST_GRUPO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblEstGrupoServ = new FDBDataSetOperSeed.TB_EST_GRUPODataTable())

            {

                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taEstGrupoServ = new TB_EST_GRUPOTableAdapter())
                        //using (var tblEstGrupoServ = new FDBDataSetOperSeed.TB_EST_GRUPODataTable())
                        using (var taEstGrupoPdv = new TB_EST_GRUPOTableAdapter())
                        using (var tblEstGrupoPdv = new FDBDataSetOperSeed.TB_EST_GRUPODataTable())
                        {
                            taEstGrupoServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taEstGrupoServ.Fill(tblEstGrupoServ);

                            taEstGrupoPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taEstGrupoPdv.Fill(tblEstGrupoPdv);

                            using (var changeReader = new DataTableReader(tblEstGrupoServ))
                                tblEstGrupoPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taEstGrupoPdv.Update(tblEstGrupoPdv);

                            tblEstGrupoPdv.AcceptChanges();
                        }

                        #endregion Única sync
                    }
                    else
                    {
                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesEstGrupo = dtAuxSyncPendentes.Select($"TABELA = 'TB_EST_GRUPO'");

                            for (int i = 0; i < pendentesEstGrupo.Length; i++)
                            {
                                var idEstGrupo = pendentesEstGrupo[i]["ID_REG"].Safeint();
                                var operacao = pendentesEstGrupo[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taEstGrupoServ = new TB_EST_GRUPOTableAdapter())
                                    {
                                        taEstGrupoServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taEstGrupoServ.FillById(tblEstGrupoServ, idEstGrupo);

                                        if (tblEstGrupoServ != null && tblEstGrupoServ.Rows.Count > 0)
                                        {
                                            using (var taEstGrupoPdv = new TB_EST_GRUPOTableAdapter())
                                            {
                                                taEstGrupoPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetOperSeed.TB_EST_GRUPORow estGrupoServ in tblEstGrupoServ)
                                                {
                                                    intRetornoUpsert = taEstGrupoPdv.SP_TRI_EST_GRUPO_UPSERT(estGrupoServ.ID_GRUPO,
                                                                                          estGrupoServ.DESCRICAO,
                                                                                          DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert != 0)//.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(idEstGrupo,
                                                                     "TB_EST_GRUPO",
                                                                     operacao,
                                                                     shtNumCaixa);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesEstGrupo = pendentesEstGrupo.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesEstGrupo = dtPendentesEstGrupo.Select($"ID_REG = {idEstGrupo} AND OPERACAO = 'D'");

                                                if (deletesPendentesEstGrupo.Length > 0)
                                                {
                                                    foreach (var deletePendenteEstoque in deletesPendentesEstGrupo)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, idEstGrupo, "TB_EST_GRUPO", "D", shtNumCaixa);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TB_EST_GRUPO) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idEstGrupo}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesEstGrupo = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idEstGrupo} AND TABELA = 'TB_EST_GRUPO' AND OPERACAO = 'D'");

                                                if (deletesPendentesEstGrupo.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idEstGrupo, "TB_EST_GRUPO", "D", shtNumCaixa);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC
                    }
                }
                catch (Exception ex)
                {
                    //audit("SINCCONTNETDB>> " + "Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Grupo de estoque", tblEstGrupoServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_FORNECEDOR(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblFornecServ = new FDBDataSetOperSeed.TB_FORNECEDORDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync
                        using (var taFornecedorServ = new TB_FORNECEDORTableAdapter())
                        using (var tblFornecedorServ = new FDBDataSetOperSeed.TB_FORNECEDORDataTable())
                        using (var taFornecedorPdv = new TB_FORNECEDORTableAdapter())
                        using (var tblFornecedorPdv = new FDBDataSetOperSeed.TB_FORNECEDORDataTable())
                        {
                            taFornecedorServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taFornecedorServ.Fill(tblFornecedorServ);

                            taFornecedorPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taFornecedorPdv.Fill(tblFornecedorPdv);

                            using (var changeReader = new DataTableReader(tblFornecedorServ))
                                tblFornecedorPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taFornecedorPdv.Update(tblFornecedorPdv);

                            tblFornecedorPdv.AcceptChanges();
                        }
                        #endregion Única sync
                    }
                    else
                    {
                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesFornecedor = dtAuxSyncPendentes.Select($"TABELA = 'TB_FORNECEDOR'");

                            for (int i = 0; i < pendentesFornecedor.Length; i++)
                            {
                                var idFornec = pendentesFornecedor[i]["ID_REG"].Safeint();
                                var operacao = pendentesFornecedor[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taFornecServ = new TB_FORNECEDORTableAdapter())
                                    {
                                        taFornecServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                                        try
                                        {
                                            taFornecServ.FillById(tblFornecServ, idFornec);
                                        }
                                        catch (Exception)
                                        {
                                            var a = tblFornecServ.GetErrors();
                                            throw;
                                        }

                                        if (tblFornecServ != null && tblFornecServ.Rows.Count > 0)
                                        {
                                            using (var taFornecPdv = new TB_FORNECEDORTableAdapter())
                                            {
                                                taFornecPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetOperSeed.TB_FORNECEDORRow fornecServ in tblFornecServ)
                                                {
                                                    intRetornoUpsert = taFornecPdv.SP_TRI_FORNEC_UPSERT(fornecServ.ID_FORNEC,
                                                                                     fornecServ.NOME,
                                                                                     (fornecServ.IsNOME_FANTANull() ? null : fornecServ.NOME_FANTA),
                                                                                     (fornecServ.IsCNPJNull() ? null : fornecServ.CNPJ),
                                                                                     (fornecServ.IsINSC_ESTADNull() ? null : fornecServ.INSC_ESTAD),
                                                                                     (fornecServ.IsINSC_MUNICNull() ? null : fornecServ.INSC_MUNIC),
                                                                                     (fornecServ.IsEND_CEPNull() ? null : fornecServ.END_CEP),
                                                                                     (fornecServ.IsEND_TIPONull() ? null : fornecServ.END_TIPO),
                                                                                     (fornecServ.IsEND_LOGRADNull() ? null : fornecServ.END_LOGRAD),
                                                                                     (fornecServ.IsEND_BAIRRONull() ? null : fornecServ.END_BAIRRO),
                                                                                     (fornecServ.IsEND_NUMERONull() ? null : fornecServ.END_NUMERO),
                                                                                     (fornecServ.IsEND_COMPLENull() ? null : fornecServ.END_COMPLE),
                                                                                     (fornecServ.IsDDD_COMERNull() ? null : fornecServ.DDD_COMER),
                                                                                     (fornecServ.IsFONE_COMERNull() ? null : fornecServ.FONE_COMER),
                                                                                     (fornecServ.IsFONE_0800Null() ? null : fornecServ.FONE_0800),
                                                                                     (fornecServ.IsDDD_CELULNull() ? null : fornecServ.DDD_CELUL),
                                                                                     (fornecServ.IsFONE_CELULNull() ? null : fornecServ.FONE_CELUL),
                                                                                     (fornecServ.IsDDD_FAXNull() ? null : fornecServ.DDD_FAX),
                                                                                     (fornecServ.IsFONE_FAXNull() ? null : fornecServ.FONE_FAX),
                                                                                     (fornecServ.IsEMAIL_CONTNull() ? null : fornecServ.EMAIL_CONT),
                                                                                     (fornecServ.IsEMAIL_NFENull() ? null : fornecServ.EMAIL_NFE),
                                                                                     (fornecServ.IsSITENull() ? null : fornecServ.SITE),
                                                                                     fornecServ.STATUS,
                                                                                     (fornecServ.IsDT_PRICOMPNull() ? null : (DateTime?)fornecServ.DT_PRICOMP),
                                                                                     (fornecServ.IsDT_ULTCOMPNull() ? null : (DateTime?)fornecServ.DT_ULTCOMP),
                                                                                     (fornecServ.IsID_CIDADENull() ? null : fornecServ.ID_CIDADE),
                                                                                     (fornecServ.IsLIMITENull() ? null : (decimal?)fornecServ.LIMITE),
                                                                                     (fornecServ.IsID_RAMONull() ? null : (short?)fornecServ.ID_RAMO),
                                                                                     (fornecServ.IsID_PAISNull() ? null : fornecServ.ID_PAIS),
                                                                                     (fornecServ.IsOBSERVACAONull() ? null : fornecServ.OBSERVACAO),
                                                                                     (fornecServ.IsCONTATONull() ? null : fornecServ.CONTATO),
                                                                                     DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert != 0)//.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(idFornec,
                                                                     "TB_FORNECEDOR",
                                                                     operacao,
                                                                     shtNumCaixa);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesFornec = pendentesFornecedor.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesFornec = dtPendentesFornec.Select($"ID_REG = {idFornec} AND OPERACAO = 'D'");

                                                if (deletesPendentesFornec.Length > 0)
                                                {
                                                    foreach (var deletePendenteEstoque in deletesPendentesFornec)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, idFornec, "TB_FORNECEDOR", "D", shtNumCaixa);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TB_FORNECEDOR) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idFornec}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesFornec = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idFornec} AND TABELA = 'TB_FORNECEDOR' AND OPERACAO = 'D'");

                                                if (deletesPendentesFornec.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idFornec, "TB_FORNECEDOR", "D", shtNumCaixa);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC
                    }
                }
                catch (Exception ex)
                {
                    //audit("SINCCONTNETDB>> " + "Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    //GravarErroSync("Fornecedor", tblFornecServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_UNI_MEDIDA(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblUniMedidaServ = new FDBDataSetOperSeed.TB_UNI_MEDIDADataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync
                        using (var taUniMedidaServ = new TB_UNI_MEDIDATableAdapter())
                        using (var taUniMedidaPdv = new TB_UNI_MEDIDATableAdapter())
                        using (var tblUniMedidaPdv = new FDBDataSetOperSeed.TB_UNI_MEDIDADataTable())
                        {
                            taUniMedidaServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taUniMedidaServ.Fill(tblUniMedidaServ);

                            taUniMedidaPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taUniMedidaPdv.Fill(tblUniMedidaPdv);

                            using (var changeReader = new DataTableReader(tblUniMedidaServ))
                                tblUniMedidaPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taUniMedidaPdv.Update(tblUniMedidaPdv);

                            tblUniMedidaPdv.AcceptChanges();
                        }
                        #endregion Única sync
                    }
                    else
                    {
                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesUniMed = dtAuxSyncPendentes.Select($"TABELA = 'TB_UNI_MEDIDA'");

                            for (int i = 0; i < pendentesUniMed.Length; i++)
                            {
                                var unRegUnidade = pendentesUniMed[i]["UN_REG"].Safestring();
                                var operacao = pendentesUniMed[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taUniMedidaServ = new TB_UNI_MEDIDATableAdapter())
                                    {
                                        taUniMedidaServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taUniMedidaServ.FillById(tblUniMedidaServ, unRegUnidade);

                                        if (tblUniMedidaServ != null && tblUniMedidaServ.Rows.Count > 0)
                                        {
                                            using (var taUniMedidaPdv = new TB_UNI_MEDIDATableAdapter())
                                            {
                                                taUniMedidaPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetOperSeed.TB_UNI_MEDIDARow uniMedidaServ in tblUniMedidaServ)
                                                {
                                                    intRetornoUpsert = (int)taUniMedidaPdv.SP_TRI_UNIMEDIDA_UPSERT(uniMedidaServ.UNIDADE,
                                                                                                                   uniMedidaServ.DESCRICAO,
                                                                                                                   uniMedidaServ.CONVERSOR,
                                                                                                                   uniMedidaServ.STATUS,
                                                                                                                   uniMedidaServ.IsUNIDADE_EXNull() ? null : uniMedidaServ.UNIDADE_EX);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert > 0)//.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(0,
                                                                     "TB_UNI_MEDIDA",
                                                                     operacao,
                                                                     shtNumCaixa,
                                                                     unRegUnidade);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesUniMed = pendentesUniMed.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesUniMedida = dtPendentesUniMed.Select($"UN_REG = '{unRegUnidade}' AND OPERACAO = 'D'");

                                                if (deletesPendentesUniMedida.Length > 0)
                                                {
                                                    foreach (var deletePendenteUniMedida in deletesPendentesUniMedida)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, 0, "TB_UNI_MEDIDA", "D", shtNumCaixa, null, unRegUnidade);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TB_UNI_MEDIDA) não encontrado no servidor e sem exclusão pendente. \nID do registro (UN_REG): {unRegUnidade}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesUniMedida = dtAuxSyncDeletesPendentes.Select($"UN_REG = '{unRegUnidade}' AND TABELA = 'TB_UNI_MEDIDA' AND OPERACAO = 'D'");

                                                if (deletesPendentesUniMedida.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, 0, "TB_UNI_MEDIDA", "D", shtNumCaixa, null, unRegUnidade);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC
                    }
                }
                catch (Exception ex)
                {
                    //audit("SINCCONTNETDB>> " + "Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Unidade de Medida", tblUniMedidaServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_ESTOQUE(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa, List<ComboBoxBindingDTO_Produto_Sync> retornoProdutosAlterados)
        {
            using (var tblAuxEstoqueServ = new FDBDataSetOperSeed.SP_TRI_ESTOQUE_ID_GETBY_IDDataTable())
            {
                try
                {
                    // A sync de cadastros deverá acontecer em 2 etapas.
                    // A primeira etapa deverá ser executada apenas 1 vez, 
                    //      quando as datas de sync estão nulas.
                    // A segunda etapa deve sync apenas os cadastros cuja data de inserção/alteração
                    //      for maior que a data da última sync.

                    // Primeira sync
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync
                        using (var taEstoqueServ = new TB_ESTOQUETableAdapter())
                        using (var taEstoquePdv = new TB_ESTOQUETableAdapter())
                        using (var tblEstoqueServ = new FDBDataSetOperSeed.TB_ESTOQUEDataTable())
                        using (var tblEstoquePdv = new FDBDataSetOperSeed.TB_ESTOQUEDataTable())
                        {
                            taEstoqueServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taEstoquePdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                            taEstoqueServ.Fill(tblEstoqueServ);

                            taEstoquePdv.Fill(tblEstoquePdv);

                            using (var changeReader = new DataTableReader(tblEstoqueServ))
                                tblEstoquePdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taEstoquePdv.Update(tblEstoquePdv);

                            tblEstoquePdv.AcceptChanges();
                        }
                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesEstoque = dtAuxSyncPendentes.Select($"TABELA = 'TB_ESTOQUE'");

                            for (int i = 0; i < pendentesEstoque.Length; i++)
                            {
                                var idEstoque = pendentesEstoque[i]["ID_REG"].Safeint();
                                var operacao = pendentesEstoque[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taEstoqueServ = new SP_TRI_ESTOQUE_ID_GETBY_IDTableAdapter())//using (var taEstoqueServ = new TB_ESTOQUETableAdapter()) //TODO: mudar o TableAdapter para uma baseada numa sproc que retorne também o ID_IDENTIFICADOR.
                                    {
                                        taEstoqueServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taEstoqueServ.FillByIdEstoque(tblAuxEstoqueServ, idEstoque); //taEstoqueServ.FillByIdSproc(tblEstoqueServ, idEstoque); // passou a usar sproc

                                        if (tblAuxEstoqueServ != null && tblAuxEstoqueServ.Rows.Count > 0)
                                        {
                                            using (var taEstoquePdv = new TB_ESTOQUETableAdapter())
                                            {
                                                taEstoquePdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                                                string testetrib = "012";

                                                //foreach (FDBDataSetOperSeed.TB_ESTOQUERow estoqueServ in tblEstoqueServ)
                                                foreach (FDBDataSetOperSeed.SP_TRI_ESTOQUE_ID_GETBY_IDRow estoqueServ in tblAuxEstoqueServ)
                                                {
                                                    intRetornoUpsert = (int)taEstoquePdv.SP_TRI_ESTOQUE_UPSERT(estoqueServ.ID_ESTOQUE,
                                                                                       (estoqueServ.IsID_GRUPONull() ? null : (int?)estoqueServ.ID_GRUPO),
                                                                                       estoqueServ.DESCRICAO,
                                                                                       estoqueServ.STATUS,
                                                                                       estoqueServ.DT_CADAST,
                                                                                       estoqueServ.HR_CADAST,
                                                                                       estoqueServ.FRACIONADO,
                                                                                       estoqueServ.PRC_VENDA,
                                                                                       (estoqueServ.IsPRC_CUSTONull() ? null : (decimal?)estoqueServ.PRC_CUSTO),
                                                                                       (estoqueServ.IsULT_VENDANull() ? null : (DateTime?)estoqueServ.ULT_VENDA),
                                                                                       (estoqueServ.IsMARGEM_LBNull() ? null : (decimal?)estoqueServ.MARGEM_LB),
                                                                                       (estoqueServ.IsPOR_COMISSAONull() ? null : (decimal?)estoqueServ.POR_COMISSAO),
                                                                                       (estoqueServ.IsULT_FORNECNull() ? null : (int?)estoqueServ.ULT_FORNEC),
                                                                                       (estoqueServ.IsGRADE_SERIENull() ? null : estoqueServ.GRADE_SERIE),
                                                                                       estoqueServ.ID_TIPOITEM,
                                                                                       (estoqueServ.IsID_CTINull() ? null : estoqueServ.ID_CTI),
                                                                                       (estoqueServ.IsCST_PISNull() ? null : estoqueServ.CST_PIS),
                                                                                       (estoqueServ.IsCST_COFINSNull() ? null : estoqueServ.CST_COFINS),
                                                                                       (estoqueServ.IsPISNull() ? null : (decimal?)estoqueServ.PIS),
                                                                                       (estoqueServ.IsCOFINSNull() ? null : (decimal?)estoqueServ.COFINS),
                                                                                       (estoqueServ.IsUNI_MEDIDANull() ? null : estoqueServ.UNI_MEDIDA),
                                                                                       (estoqueServ.IsMARGEM_PVNull() ? null : (decimal?)estoqueServ.MARGEM_PV),
                                                                                       (estoqueServ.IsCFOPNull() ? null : estoqueServ.CFOP),
                                                                                       (estoqueServ.IsOBSERVACAONull() ? null : estoqueServ.OBSERVACAO),
                                                                                       (estoqueServ.IsNAT_RECEITANull() ? null : (short?)estoqueServ.NAT_RECEITA),
                                                                                       (estoqueServ.IsCFOP_NFNull() ? null : estoqueServ.CFOP_NF),
                                                                                       (estoqueServ.IsPRC_ATACADONull() ? null : (decimal?)estoqueServ.PRC_ATACADO),
                                                                                       (estoqueServ.IsID_CTI_PARTNull() ? null : estoqueServ.ID_CTI_PART),
                                                                                       (estoqueServ.IsID_CTI_FCPNull() ? null : estoqueServ.ID_CTI_FCP),                                                                                       
                                                                                       (estoqueServ.IsQTD_ATACADONull() ? null : (decimal?)estoqueServ.QTD_ATACADO),
                                                                                       (estoqueServ.IsID_CTI_CFENull() ? null : estoqueServ.ID_CTI_CFE),
                                                                                       DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(idEstoque,
                                                                     "TB_ESTOQUE",
                                                                     operacao,
                                                                     shtNumCaixa);

                                                        if (retornoProdutosAlterados == null) { retornoProdutosAlterados = new List<ComboBoxBindingDTO_Produto_Sync>(); }
                                                        retornoProdutosAlterados.Add(new ComboBoxBindingDTO_Produto_Sync()
                                                        {
                                                            ID_IDENTIFICADOR = estoqueServ.ID_IDENTIFICADOR,
                                                            OPERACAO = operacao,
                                                            DESCRICAO = (estoqueServ.IsDESCRICAONull() ? string.Empty : estoqueServ.DESCRICAO),
                                                            ORIGEM_TB = "TB_ESTOQUE"
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesEstoque = pendentesEstoque.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesEstoque = dtPendentesEstoque.Select($"ID_REG = {idEstoque} AND OPERACAO = 'D'");

                                                if (deletesPendentesEstoque.Length > 0)
                                                {
                                                    foreach (var deletePendenteEstoque in deletesPendentesEstoque)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, idEstoque, "TB_ESTOQUE", "D", shtNumCaixa);

                                                        if (retornoProdutosAlterados == null) { retornoProdutosAlterados = new List<ComboBoxBindingDTO_Produto_Sync>(); }
                                                        retornoProdutosAlterados.Add(new ComboBoxBindingDTO_Produto_Sync() { OPERACAO = "D" });
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    //throw new DataException($"Erro não esperado: produto (TB_ESTOQUE) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idEstoque}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesEstoque = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idEstoque} AND TABELA = 'TB_ESTOQUE' AND OPERACAO = 'D'");

                                                if (deletesPendentesEstoque.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idEstoque, "TB_ESTOQUE", "D", shtNumCaixa);

                                                    if (retornoProdutosAlterados == null) { retornoProdutosAlterados = new List<ComboBoxBindingDTO_Produto_Sync>(); }
                                                    retornoProdutosAlterados.Add(new ComboBoxBindingDTO_Produto_Sync() { OPERACAO = "D" });
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (NotImplementedException niex)
                {
                    log.Error("Not Implemented Exception", niex);
                    throw niex;
                }
                catch (DataException dex)
                {
                    log.Error("Data Exception", dex);
                    throw dex;
                }
                catch (Exception ex)
                {
                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Estoque", tblAuxEstoqueServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_EST_IDENTIFICADOR(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblEstIdentifServ = new FDBDataSetOperSeed.TB_EST_IDENTIFICADORDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taServidor = new TB_EST_IDENTIFICADORTableAdapter())
                        using (var tblServidor = new FDBDataSetOperSeed.TB_EST_IDENTIFICADORDataTable())
                        using (var taPdv = new TB_EST_IDENTIFICADORTableAdapter())
                        using (var tblPdv = new FDBDataSetOperSeed.TB_EST_IDENTIFICADORDataTable())
                        {
                            taServidor.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taServidor.Fill(tblServidor);

                            taPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taPdv.Fill(tblPdv);

                            using (var changeReader = new DataTableReader(tblServidor))
                                tblPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taPdv.Update(tblPdv);

                            tblPdv.AcceptChanges();
                        }

                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRowsAffected = 0;

                        {
                            DataRow[] pendentesEstIdentif = dtAuxSyncPendentes.Select($"TABELA = 'TB_EST_IDENTIFICADOR'");

                            for (int i = 0; i < pendentesEstIdentif.Length; i++)
                            {
                                var idEstIdentif = pendentesEstIdentif[i]["ID_REG"].Safeint();
                                var operacao = pendentesEstIdentif[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taEstIdentifServ = new TB_EST_IDENTIFICADORTableAdapter())
                                    {
                                        taEstIdentifServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taEstIdentifServ.FillById(tblEstIdentifServ, idEstIdentif); // passou a usar sproc

                                        if (tblEstIdentifServ != null && tblEstIdentifServ.Rows.Count > 0)
                                        {
                                            using (var taEstIdentifPdv = new TB_EST_IDENTIFICADORTableAdapter())
                                            {
                                                taEstIdentifPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetOperSeed.TB_EST_IDENTIFICADORRow estIdentifServ in tblEstIdentifServ)
                                                {
                                                    //TODO: alterar sproc e retornar 0 caso o ID_ESTOQUE fornecido não conste em TB_ESTOQUE -- TESTARRRRR
                                                    intRowsAffected = (int)taEstIdentifPdv.SP_TRI_ESTIDENTIF_UPSERT(estIdentifServ.ID_IDENTIFICADOR,
                                                                                             estIdentifServ.ID_ESTOQUE,
                                                                                             (estIdentifServ.IsCHAVENull() ? null : estIdentifServ.CHAVE),
                                                                                             (estIdentifServ.IsTRI_PDV_DT_UPDNull() ? null : (DateTime?)estIdentifServ.TRI_PDV_DT_UPD));

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.

                                                    if (intRowsAffected.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(idEstIdentif,
                                                                     "TB_EST_IDENTIFICADOR",
                                                                     operacao,
                                                                     shtNumCaixa);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesEstIdentif = pendentesEstIdentif.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesEstIdentif = dtPendentesEstIdentif.Select($"ID_REG = {idEstIdentif} AND OPERACAO = 'D'");

                                                if (deletesPendentesEstIdentif.Length > 0)
                                                {
                                                    foreach (var deletePendenteEstoque in deletesPendentesEstIdentif)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, idEstIdentif, "TB_EST_IDENTIFICADOR", "D", shtNumCaixa);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    //throw new DataException($"Erro não esperado: item (TB_EST_IDENTIFICADOR) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idEstIdentif}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesEstoque = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idEstIdentif} AND TABELA = 'TB_EST_IDENTIFICADOR' AND OPERACAO = 'D'");

                                                if (deletesPendentesEstoque.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idEstIdentif, "TB_EST_IDENTIFICADOR", "D", shtNumCaixa);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (NotImplementedException niex)
                {
                    log.Error("Not Implemented Exception", niex);
                    throw niex;
                }
                catch (DataException dex)
                {
                    log.Error("Data Exception", dex);
                    throw dex;
                }
                catch (Exception ex)
                {
                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Estoque/Identificador", tblEstIdentifServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_EST_PRODUTO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa, ref List<ComboBoxBindingDTO_Produto_Sync> retornoProdutosAlterados)
        {
            using (var tblEstProdutoPdv = new FDBDataSetOperSeed.TB_EST_PRODUTODataTable())
            using (var tblEstProdutoServ = new FDBDataSetOperSeed.TB_EST_PRODUTODataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        #region NOPE
                        //using (var taBREAKONLY = new TB_EST_PRODUTOTableAdapter())
                        //{
                        //    taBREAKONLY.Connection.ConnectionString = _strConnContingency;
                        //    taBREAKONLY.SP_TRI_BREAK_CLIPP_RULES();
                        //}
                        #endregion NOPE

                        //taPdv.SP_TRI_BREAK_CLIPP_RULES();
                        using (var taServidor = new TB_EST_PRODUTOTableAdapter())
                        using (var taPdv = new TB_EST_PRODUTOTableAdapter())

                        {
                            taServidor.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taServidor.Fill(tblEstProdutoServ);

                            taPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taPdv.Fill(tblEstProdutoPdv);

                            using (var changeReader = new DataTableReader(tblEstProdutoServ))
                                tblEstProdutoPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taPdv.Update(tblEstProdutoPdv);

                            tblEstProdutoPdv.AcceptChanges();

                            //taPdv.SP_TRI_FIX_CLIPP_RULES();
                        }

                        #region NOPE
                        //using (var taFIXONLY = new TB_EST_PRODUTOTableAdapter())
                        //{
                        //    taFIXONLY.Connection.ConnectionString = _strConnContingency;
                        //    taFIXONLY.SP_TRI_FIX_CLIPP_RULES();
                        //}
                        #endregion NOPE

                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRowsAffected = 0;

                        {
                            DataRow[] pendentesEstProduto = dtAuxSyncPendentes.Select($"TABELA = 'TB_EST_PRODUTO'");

                            for (int i = 0; i < pendentesEstProduto.Length; i++)
                            {
                                var idEstProduto = pendentesEstProduto[i]["ID_REG"].Safeint();
                                var operacao = pendentesEstProduto[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taEstProdutoServ = new TB_EST_PRODUTOTableAdapter())
                                    {
                                        taEstProdutoServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taEstProdutoServ.FillById(tblEstProdutoServ, idEstProduto); // passou a usar sproc

                                        if (tblEstProdutoServ != null && tblEstProdutoServ.Rows.Count > 0)
                                        {
                                            using (var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter())
                                            {
                                                taEstProdutoPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetOperSeed.TB_EST_PRODUTORow estProdutoServ in tblEstProdutoServ)
                                                {
                                                    //TODO: alterar sproc e retornar 0 caso o ID_IDENTIFICADOR fornecido não conste em TB_EST_PRODUTO -- TESTARRRRR
                                                    intRowsAffected = (int)taEstProdutoPdv.SP_TRI_ESTPROD_UPSERT(estProdutoServ.ID_IDENTIFICADOR,
                                                                                          (estProdutoServ.IsDESC_CMPLNull() ? null : estProdutoServ.DESC_CMPL),
                                                                                          (estProdutoServ.IsCOD_BARRANull() ? null : estProdutoServ.COD_BARRA),
                                                                                          (estProdutoServ.IsREFERENCIANull() ? null : estProdutoServ.REFERENCIA),
                                                                                          (estProdutoServ.IsPRC_MEDIONull() ? null : (decimal?)estProdutoServ.PRC_MEDIO),
                                                                                          (estProdutoServ.IsQTD_COMPRANull() ? null : (decimal?)estProdutoServ.QTD_COMPRA),
                                                                                          estProdutoServ.QTD_ATUAL,
                                                                                          (estProdutoServ.IsQTD_MINIMNull() ? null : (decimal?)estProdutoServ.QTD_MINIM),
                                                                                          (estProdutoServ.IsQTD_INICIONull() ? null : (decimal?)estProdutoServ.QTD_INICIO),
                                                                                          (estProdutoServ.IsQTD_RESERVNull() ? null : (decimal?)estProdutoServ.QTD_RESERV),
                                                                                          (estProdutoServ.IsQTD_POSVENNull() ? null : (decimal?)estProdutoServ.QTD_POSVEN),
                                                                                          (estProdutoServ.IsULT_COMPRANull() ? null : (DateTime?)estProdutoServ.ULT_COMPRA),
                                                                                          (estProdutoServ.IsPESONull() ? null : (decimal?)estProdutoServ.PESO),
                                                                                          (estProdutoServ.IsIPINull() ? null : (decimal?)estProdutoServ.IPI),
                                                                                          (estProdutoServ.IsCFNull() ? null : estProdutoServ.CF),
                                                                                          estProdutoServ.IAT,
                                                                                          estProdutoServ.IPPT,
                                                                                          (estProdutoServ.IsCOD_NCMNull() ? null : estProdutoServ.COD_NCM),
                                                                                          (estProdutoServ.IsID_NIVEL1Null() ? null : (short?)estProdutoServ.ID_NIVEL1),
                                                                                          (estProdutoServ.IsID_NIVEL2Null() ? null : (short?)estProdutoServ.ID_NIVEL2),
                                                                                          (estProdutoServ.IsMVANull() ? null : (decimal?)estProdutoServ.MVA),
                                                                                          (estProdutoServ.IsCST_IPINull() ? null : estProdutoServ.CST_IPI),
                                                                                          (estProdutoServ.IsFOTONull() ? null : estProdutoServ.FOTO),
                                                                                          (estProdutoServ.IsCSOSNNull() ? null : estProdutoServ.CSOSN),
                                                                                          (estProdutoServ.IsANPNull() ? null : (int?)estProdutoServ.ANP),
                                                                                          (estProdutoServ.IsEXTIPINull() ? null : (short?)estProdutoServ.EXTIPI),
                                                                                          (estProdutoServ.IsCSTNull() ? null : estProdutoServ.CST),
                                                                                          (estProdutoServ.IsFCINull() ? null : estProdutoServ.FCI),
                                                                                          (estProdutoServ.IsCOD_CESTNull() ? null : estProdutoServ.COD_CEST),
                                                                                          (estProdutoServ.IsCENQNull() ? "001" : estProdutoServ.CENQ), //HACK: vai dar pau se tentar gravar nulo. O app não usa esse valor. O padrão será "001" Em vez de null, até segunda ordem.
                                                                                          (estProdutoServ.IsVLR_IPINull() ? null : (decimal?)estProdutoServ.VLR_IPI),
                                                                                          (estProdutoServ.IsCST_CFENull() ? null : estProdutoServ.CST_CFE),
                                                                                          (estProdutoServ.IsCSOSN_CFENull() ? null : estProdutoServ.CSOSN_CFE),
                                                                                          estProdutoServ.CONTROLA_LOTE_VENDA,
                                                                                          (estProdutoServ.IsBAIXA_LOTE_NFVNull() ? null : estProdutoServ.BAIXA_LOTE_NFV),
                                                                                          (estProdutoServ.IsBAIXA_LOTE_PDVNull() ? null : estProdutoServ.BAIXA_LOTE_PDV),
                                                                                          DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.

                                                    if (intRowsAffected.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(idEstProduto,
                                                                     "TB_EST_PRODUTO",
                                                                     operacao,
                                                                     shtNumCaixa);

                                                        if (retornoProdutosAlterados == null) { retornoProdutosAlterados = new List<ComboBoxBindingDTO_Produto_Sync>(); }
                                                        retornoProdutosAlterados.Add(new ComboBoxBindingDTO_Produto_Sync()
                                                        {
                                                            ID_IDENTIFICADOR = idEstProduto,
                                                            COD_BARRA = (estProdutoServ.IsCOD_BARRANull() ? string.Empty : estProdutoServ.COD_BARRA),
                                                            OPERACAO = operacao,
                                                            REFERENCIA = (estProdutoServ.IsREFERENCIANull() ? string.Empty : estProdutoServ.REFERENCIA),
                                                            ORIGEM_TB = "TB_EST_PRODUTO"
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesEstProduto = pendentesEstProduto.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesEstProduto = dtPendentesEstProduto.Select($"ID_REG = {idEstProduto} AND OPERACAO = 'D'");

                                                if (deletesPendentesEstProduto.Length > 0)
                                                {
                                                    foreach (var deletePendenteEstProduto in deletesPendentesEstProduto)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, idEstProduto, "TB_EST_PRODUTO", "D", shtNumCaixa);

                                                        if (retornoProdutosAlterados == null) { retornoProdutosAlterados = new List<ComboBoxBindingDTO_Produto_Sync>(); }
                                                        retornoProdutosAlterados.Add(new ComboBoxBindingDTO_Produto_Sync() { OPERACAO = "D" });
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    //throw new DataException($"Erro não esperado: item (TB_EST_PRODUTO) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idEstProduto}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesEstProduto = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idEstProduto} AND TABELA = 'TB_EST_PRODUTO' AND OPERACAO = 'D'");

                                                if (deletesPendentesEstProduto.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idEstProduto, "TB_EST_PRODUTO", "D", shtNumCaixa);

                                                    if (retornoProdutosAlterados == null) { retornoProdutosAlterados = new List<ComboBoxBindingDTO_Produto_Sync>(); }
                                                    retornoProdutosAlterados.Add(new ComboBoxBindingDTO_Produto_Sync() { OPERACAO = "D" });
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (NotImplementedException niex)
                {
                    log.Error("Not implemented Exception", niex);
                    throw niex;
                }
                catch (DataException dex)
                {
                    log.Error("Data Exception", dex);
                    throw dex;
                }
                catch (Exception ex)
                {
                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Estoque/Produto(PDV)", tblEstProdutoPdv, ex);
                    GravarErroSync("Estoque/Produto(SERV)", tblEstProdutoServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_FUNCIONARIO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblFuncionarioPdv = new FDBDataSetOperSeed.TB_FUNCIONARIODataTable())
            using (var tblFuncionarioServ = new FDBDataSetOperSeed.TB_FUNCIONARIODataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        //taPdv.SP_TRI_BREAK_CLIPP_RULES();
                        using (var taServidor = new TB_FUNCIONARIOTableAdapter())
                        using (var taPdv = new TB_FUNCIONARIOTableAdapter())
                        {
                            taServidor.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taServidor.Fill(tblFuncionarioServ);

                            taPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taPdv.Fill(tblFuncionarioPdv);

                            using (var changeReader = new DataTableReader(tblFuncionarioServ))
                                tblFuncionarioPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taPdv.Update(tblFuncionarioPdv);

                            tblFuncionarioPdv.AcceptChanges();

                            //taPdv.SP_TRI_FIX_CLIPP_RULES();
                        }

                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRowsAffected = 0;

                        {
                            DataRow[] pendentesFunc = dtAuxSyncPendentes.Select($"TABELA = 'TB_FUNCIONARIO'");

                            for (int i = 0; i < pendentesFunc.Length; i++)
                            {
                                var idFunc = pendentesFunc[i]["ID_REG"].Safeint();
                                var operacao = pendentesFunc[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taFuncionarioServ = new TB_FUNCIONARIOTableAdapter())
                                    {
                                        taFuncionarioServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taFuncionarioServ.FillById(tblFuncionarioServ, idFunc);

                                        if (tblFuncionarioServ != null && tblFuncionarioServ.Rows.Count > 0)
                                        {
                                            using (var taFuncionarioPdv = new TB_FUNCIONARIOTableAdapter())
                                            {
                                                taFuncionarioPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetOperSeed.TB_FUNCIONARIORow funcionarioServ in tblFuncionarioServ)
                                                {
                                                    //TODO: alterar sproc e retornar 0 caso o ID_IDENTIFICADOR fornecido não conste em TB_EST_PRODUTO --
                                                    intRowsAffected = (int)taFuncionarioPdv.SP_TRI_FUNCIONARIO_UPSERT(funcionarioServ.ID_FUNCIONARIO,
                                                                                                                      (funcionarioServ.IsID_CIDADENull() ? null : funcionarioServ.ID_CIDADE),
                                                                                                                      (funcionarioServ.IsN_REGISTRONull() ? null : funcionarioServ.N_REGISTRO),
                                                                                                                      (funcionarioServ.IsCPFNull() ? null : funcionarioServ.CPF),
                                                                                                                      (funcionarioServ.IsNOMENull() ? null : funcionarioServ.NOME),
                                                                                                                      (funcionarioServ.IsRGNull() ? null : funcionarioServ.RG),
                                                                                                                      (funcionarioServ.IsEND_CEPNull() ? null : funcionarioServ.END_CEP),
                                                                                                                      (funcionarioServ.IsEND_TIPONull() ? null : funcionarioServ.END_TIPO),
                                                                                                                      (funcionarioServ.IsEND_LOGRADNull() ? null : funcionarioServ.END_LOGRAD),
                                                                                                                      (funcionarioServ.IsEND_NUMERONull() ? null : funcionarioServ.END_NUMERO),
                                                                                                                      (funcionarioServ.IsEND_COMPLENull() ? null : funcionarioServ.END_COMPLE),
                                                                                                                      (funcionarioServ.IsEND_BAIRRONull() ? null : funcionarioServ.END_BAIRRO),
                                                                                                                      (funcionarioServ.IsDDDNull() ? null : funcionarioServ.DDD),
                                                                                                                      (funcionarioServ.IsFONENull() ? null : funcionarioServ.FONE),
                                                                                                                      (funcionarioServ.IsCELULARNull() ? null : funcionarioServ.CELULAR),
                                                                                                                      (funcionarioServ.IsEMAILNull() ? null : funcionarioServ.EMAIL),
                                                                                                                      (funcionarioServ.IsSALARIONull() ? null : (decimal?)funcionarioServ.SALARIO),
                                                                                                                      (funcionarioServ.IsEXTRANull() ? null : (decimal?)funcionarioServ.EXTRA),
                                                                                                                      (funcionarioServ.IsDATA_NASCTNull() ? null : (DateTime?)funcionarioServ.DATA_NASCT),
                                                                                                                      (funcionarioServ.IsDATA_ADMISNull() ? null : (DateTime?)funcionarioServ.DATA_ADMIS),
                                                                                                                      (funcionarioServ.IsDATA_DEMISNull() ? null : (DateTime?)funcionarioServ.DATA_DEMIS),
                                                                                                                      (funcionarioServ.IsRAMALNull() ? null : funcionarioServ.RAMAL),
                                                                                                                      (funcionarioServ.IsSENHANull() ? null : funcionarioServ.SENHA),
                                                                                                                      (funcionarioServ.IsIPNull() ? null : funcionarioServ.IP),
                                                                                                                      (funcionarioServ.IsSTATUSNull() ? null : funcionarioServ.STATUS),
                                                                                                                      (funcionarioServ.IsID_SETORNull() ? null : (int?)funcionarioServ.ID_SETOR),
                                                                                                                      (funcionarioServ.IsID_CARGONull() ? null : (short?)funcionarioServ.ID_CARGO),
                                                                                                                      (funcionarioServ.IsFOTONull() ? null : funcionarioServ.FOTO),
                                                                                                                      (funcionarioServ.IsPISNull() ? null : funcionarioServ.PIS),
                                                                                                                      (funcionarioServ.IsAPELIDONull() ? null : funcionarioServ.APELIDO),
                                                                                                                      (funcionarioServ.IsOBSERVACAONull() ? null : funcionarioServ.OBSERVACAO)
                                                                                                                      );

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRowsAffected.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(idFunc,
                                                                     "TB_FUNCIONARIO",
                                                                     operacao,
                                                                     shtNumCaixa);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesFunc = pendentesFunc.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesFuncionario = dtPendentesFunc.Select($"ID_REG = {idFunc} AND OPERACAO = 'D'");

                                                if (deletesPendentesFuncionario.Length > 0)
                                                {
                                                    foreach (var deletePendenteFuncionario in deletesPendentesFuncionario)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, idFunc, "TB_FUNCIONARIO", "D", shtNumCaixa);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: item (TB_FUNCIONARIO) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idFunc}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesFuncionario = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idFunc} AND TABELA = 'TB_FUNCIONARIO' AND OPERACAO = 'D'");

                                                if (deletesPendentesFuncionario.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idFunc, "TB_FUNCIONARIO", "D", shtNumCaixa);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (NotImplementedException niex)
                {
                    log.Error("Not implemented exception", niex);
                    throw niex;
                }
                catch (DataException dex)
                {
                    log.Error("Data Exception", dex);
                    throw dex;
                }
                catch (Exception ex)
                {
                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Funcionários(PDV)", tblFuncionarioPdv, ex);
                    GravarErroSync("Funcionários(SERV)", tblFuncionarioServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_FUNC_PAPEL(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblFuncPapelServ = new FDBDataSetOperSeed.TB_FUNC_PAPELDataTable())
            using (var tblFuncPapelPdv = new FDBDataSetOperSeed.TB_FUNC_PAPELDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taFuncPapelServ = new TB_FUNC_PAPELTableAdapter())
                        using (var taFuncPapelPdv = new TB_FUNC_PAPELTableAdapter())
                        {
                            taFuncPapelServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taFuncPapelServ.Fill(tblFuncPapelServ);

                            taFuncPapelPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taFuncPapelPdv.Fill(tblFuncPapelPdv);

                            //tblClientePdv.Merge(tblClienteServ); Não funciona para o Update(): tblClientePdv.Rows[x].RowState não é alterado, consequentemente o Update() faz nada. Em vez disso, usar o seguinte no lugar do Merge():
                            using (var changeReader = new DataTableReader(tblFuncPapelServ))
                                tblFuncPapelPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taFuncPapelPdv.Update(tblFuncPapelPdv);

                            tblFuncPapelPdv.AcceptChanges();
                        }

                        #endregion Única sync
                    }
                    else
                    {
                        #region AUX_SYNC

                        int intRowsAffected = 0;

                        {
                            DataRow[] pendentesFuncPapel = dtAuxSyncPendentes.Select($"TABELA = 'TB_FUNC_PAPEL'");

                            for (int i = 0; i < pendentesFuncPapel.Length; i++)
                            {
                                var idFunc = pendentesFuncPapel[i]["ID_REG"].Safeint();
                                var operacao = pendentesFuncPapel[i]["OPERACAO"].Safestring();
                                var smRegPapel = pendentesFuncPapel[i]["SM_REG"].Safeshort();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taFuncPapelServ = new TB_FUNC_PAPELTableAdapter())
                                    {
                                        taFuncPapelServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taFuncPapelServ.FillByIds(tblFuncPapelServ, idFunc, smRegPapel);

                                        if (tblFuncPapelServ != null && tblFuncPapelServ.Rows.Count > 0)
                                        {
                                            using (var taFuncPapelPdv = new TB_FUNC_PAPELTableAdapter())
                                            {
                                                taFuncPapelPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetOperSeed.TB_FUNC_PAPELRow funcPapelServ in tblFuncPapelServ)
                                                {
                                                    intRowsAffected = (int)taFuncPapelPdv.SP_TRI_FUNCPAPEL_INSERT(funcPapelServ.ID_FUNCIONARIO,
                                                                                                                  funcPapelServ.ID_PAPEL);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRowsAffected.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(idFunc,
                                                                     "TB_FUNC_PAPEL",
                                                                     operacao,
                                                                     shtNumCaixa,
                                                                     null,
                                                                     smRegPapel);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesFuncPapel = pendentesFuncPapel.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesFuncPapel = dtPendentesFuncPapel.Select($"ID_REG = {idFunc} AND OPERACAO = 'D' AND SM_REG = {smRegPapel}");

                                                if (deletesPendentesFuncPapel.Length > 0)
                                                {
                                                    foreach (var deletePendenteFuncPapel in deletesPendentesFuncPapel)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, idFunc, "TB_FUNC_PAPEL", "D", shtNumCaixa, null, null, smRegPapel);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: item (TB_FUNC_PAPEL) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idFunc} / SM_REG: {smRegPapel}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesFuncPapel = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idFunc} AND TABELA = 'TB_FUNC_PAPEL' AND OPERACAO = 'D' AND SM_REG = {smRegPapel}");

                                                if (deletesPendentesFuncPapel.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idFunc, "TB_FUNC_PAPEL", "D", shtNumCaixa, null, null, smRegPapel);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC
                    }
                }
                catch (Exception ex)
                {
                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Papéis do Funcionário(PDV)", tblFuncPapelPdv, ex);
                    GravarErroSync("Papéis do Funcionário(SERV)", tblFuncPapelServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_CLIENTE(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            // TB_CLIENTE

            using (var tblClienteServ = new FDBDataSetOperSeed.TB_CLIENTEDataTable())
            using (var tblClientePdv = new FDBDataSetOperSeed.TB_CLIENTEDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taServ = new TB_CLIENTETableAdapter())
                        using (var taPdv = new TB_CLIENTETableAdapter())
                        {
                            taServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taServ.Fill(tblClienteServ);

                            taPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taPdv.Fill(tblClientePdv);

                            //tblClientePdv.Merge(tblClienteServ); Não funciona para o Update(): tblClientePdv.Rows[x].RowState não é alterado, consequentemente o Update() faz nada. Em vez disso, usar o seguinte no lugar do Merge():
                            using (var changeReader = new DataTableReader(tblClienteServ))
                                tblClientePdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taPdv.Update(tblClientePdv);

                            tblClientePdv.AcceptChanges();
                        }

                        #endregion Única sync
                    }
                    else
                    {
                        #region AUX_SYNC

                        int intRowsAffected = 0;

                        {
                            DataRow[] pendentesCliente = dtAuxSyncPendentes.Select($"TABELA = 'TB_CLIENTE'");

                            for (int i = 0; i < pendentesCliente.Length; i++)
                            {
                                var idCliente = pendentesCliente[i]["ID_REG"].Safeint();
                                var operacao = pendentesCliente[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taClienteServ = new TB_CLIENTETableAdapter())
                                    {
                                        taClienteServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taClienteServ.FillById(tblClienteServ, idCliente); // passou a usar sproc

                                        if (tblClienteServ != null && tblClienteServ.Rows.Count > 0)
                                        {
                                            using (var taClientePdv = new TB_CLIENTETableAdapter())
                                            {
                                                taClientePdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetOperSeed.TB_CLIENTERow clienteServ in tblClienteServ)
                                                {
                                                    intRowsAffected = (int)taClientePdv.SP_TRI_CLIENTE_UPSERT(clienteServ.ID_CLIENTE,
                                                                           (clienteServ.IsID_CONVENIONull() ? null : (short?)clienteServ.ID_CONVENIO),
                                                                           clienteServ.DT_CADASTRO,
                                                                           clienteServ.NOME,
                                                                           (clienteServ.IsEND_CEPNull() ? null : clienteServ.END_CEP),
                                                                           (clienteServ.IsEND_TIPONull() ? null : clienteServ.END_TIPO),
                                                                           (clienteServ.IsEND_NUMERONull() ? null : clienteServ.END_NUMERO),
                                                                           (clienteServ.IsEND_LOGRADNull() ? null : clienteServ.END_LOGRAD),
                                                                           (clienteServ.IsEND_BAIRRONull() ? null : clienteServ.END_BAIRRO),
                                                                           (clienteServ.IsEND_COMPLENull() ? null : clienteServ.END_COMPLE),
                                                                           (clienteServ.IsDT_PRICOMPNull() ? null : (DateTime?)clienteServ.DT_PRICOMP),
                                                                           (clienteServ.IsDT_ULTCOMPNull() ? null : (DateTime?)clienteServ.DT_ULTCOMP),
                                                                           (clienteServ.IsCONTATONull() ? null : clienteServ.CONTATO),
                                                                           clienteServ.STATUS,
                                                                           (clienteServ.IsLIMITENull() ? null : (decimal?)clienteServ.LIMITE),
                                                                           (clienteServ.IsDDD_RESIDNull() ? null : clienteServ.DDD_RESID),
                                                                           (clienteServ.IsFONE_RESIDNull() ? null : clienteServ.FONE_RESID),
                                                                           (clienteServ.IsDDD_COMERNull() ? null : clienteServ.DDD_COMER),
                                                                           (clienteServ.IsFONE_COMERNull() ? null : clienteServ.FONE_COMER),
                                                                           (clienteServ.IsDDD_CELULNull() ? null : clienteServ.DDD_CELUL),
                                                                           (clienteServ.IsFONE_CELULNull() ? null : clienteServ.FONE_CELUL),
                                                                           (clienteServ.IsDDD_FAXNull() ? null : clienteServ.DDD_FAX),
                                                                           (clienteServ.IsFONE_FAXNull() ? null : clienteServ.FONE_FAX),
                                                                           (clienteServ.IsEMAIL_CONTNull() ? null : clienteServ.EMAIL_CONT),
                                                                           (clienteServ.IsEMAIL_NFENull() ? null : clienteServ.EMAIL_NFE),
                                                                           (clienteServ.IsID_CIDADENull() ? null : clienteServ.ID_CIDADE),
                                                                           (clienteServ.IsID_TIPONull() ? null : (short?)clienteServ.ID_TIPO),
                                                                           (clienteServ.IsID_FUNCIONARIONull() ? null : (short?)clienteServ.ID_FUNCIONARIO),
                                                                           clienteServ.ID_PAIS,
                                                                           (clienteServ.IsMENSAGEMNull() ? null : clienteServ.MENSAGEM),
                                                                           (clienteServ.IsID_RAMONull() ? null : (short?)clienteServ.ID_RAMO),
                                                                           (clienteServ.IsEMAIL_ADICNull() ? null : clienteServ.EMAIL_ADIC),
                                                                           (clienteServ.IsOBSERVACAONull() ? null : clienteServ.OBSERVACAO),
                                                                           (clienteServ.IsDT_MELHOR_VENCTONull() ? null : (short?)clienteServ.DT_MELHOR_VENCTO),
                                                                           DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRowsAffected.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(idCliente,
                                                                     "TB_CLIENTE",
                                                                     operacao,
                                                                     shtNumCaixa);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesCliente = pendentesCliente.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesCliente = dtPendentesCliente.Select($"ID_REG = {idCliente} AND OPERACAO = 'D'");

                                                if (deletesPendentesCliente.Length > 0)
                                                {
                                                    foreach (var deletePendenteCliente in deletesPendentesCliente)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, idCliente, "TB_CLIENTE", "D", shtNumCaixa);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: item (TB_CLIENTE) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idCliente}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesCliente = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idCliente} AND TABELA = 'TB_CLIENTE' AND OPERACAO = 'D'");

                                                if (deletesPendentesCliente.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idCliente, "TB_CLIENTE", "D", shtNumCaixa);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC
                    }
                }
                catch (Exception ex)
                {
                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Cliente(PDV)", tblClientePdv, ex);
                    GravarErroSync("Cliente(SERV)", tblClienteServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_FORMA_PAGTO_SIS(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblFmaPgtoSisServ = new FDBDataSetSistemaSeed.TB_FORMA_PAGTO_SISDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taFmaPgtoSisServ = new TB_FORMA_PAGTO_SISTableAdapter())
                        using (var taFmaPgtoSisPdv = new TB_FORMA_PAGTO_SISTableAdapter())
                        using (var tblFmaPgtoSisPdv = new FDBDataSetSistemaSeed.TB_FORMA_PAGTO_SISDataTable())
                        {
                            taFmaPgtoSisServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taFmaPgtoSisServ.Fill(tblFmaPgtoSisServ);

                            taFmaPgtoSisPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taFmaPgtoSisPdv.Fill(tblFmaPgtoSisPdv);

                            using (var changeReader = new DataTableReader(tblFmaPgtoSisServ))
                                tblFmaPgtoSisPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taFmaPgtoSisPdv.Update(tblFmaPgtoSisPdv);

                            tblFmaPgtoSisPdv.AcceptChanges();
                        }
                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesFmaPgtoSis = dtAuxSyncPendentes.Select($"TABELA = 'TB_FORMA_PAGTO_SIS'");

                            for (int i = 0; i < pendentesFmaPgtoSis.Length; i++)
                            {
                                var idFmaPgtoSis = pendentesFmaPgtoSis[i]["SM_REG"].Safeshort();
                                var operacao = pendentesFmaPgtoSis[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taFmaPgtoSisServ = new TB_FORMA_PAGTO_SISTableAdapter())
                                    {
                                        taFmaPgtoSisServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taFmaPgtoSisServ.FillById(tblFmaPgtoSisServ, idFmaPgtoSis);

                                        if (tblFmaPgtoSisServ != null && tblFmaPgtoSisServ.Rows.Count > 0)
                                        {
                                            using (var taFmaPgtoSisPdv = new TB_FORMA_PAGTO_SISTableAdapter())
                                            {
                                                taFmaPgtoSisPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetSistemaSeed.TB_FORMA_PAGTO_SISRow fmaPgtoSisServ in tblFmaPgtoSisServ)
                                                {
                                                    intRetornoUpsert = (int)taFmaPgtoSisPdv.SP_TRI_FMAPGTO_UPSERT(fmaPgtoSisServ.ID_FMAPGTO,
                                                                                                                  fmaPgtoSisServ.DESCRICAO,
                                                                                                                  fmaPgtoSisServ.STATUS,
                                                                                                                  fmaPgtoSisServ.UTILIZACAO,
                                                                                                                  DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(-1,
                                                                     "TB_FORMA_PAGTO_SIS",
                                                                     operacao,
                                                                     shtNumCaixa,
                                                                     null,
                                                                     idFmaPgtoSis,
                                                                     null);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesFmaPgtoSis = pendentesFmaPgtoSis.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesFmaPgtoSis = dtPendentesFmaPgtoSis.Select($"SM_REG = '{idFmaPgtoSis}' AND OPERACAO = 'D'");

                                                if (deletesPendentesFmaPgtoSis.Length > 0)
                                                {
                                                    foreach (var deletePendenteFmaPgtoSis in deletesPendentesFmaPgtoSis)
                                                    {
                                                        // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_FORMA_PAGTO_SIS", "D", shtNumCaixa, null, null, idFmaPgtoSis, null);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TB_FORMA_PAGTO_SIS) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idFmaPgtoSis}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesFmaPgtoSis = dtAuxSyncDeletesPendentes.Select($"SM_REG = '{idFmaPgtoSis}' AND TABELA = 'TB_FORMA_PAGTO_SIS' AND OPERACAO = 'D'");

                                                if (deletesPendentesFmaPgtoSis.Length <= 0)
                                                {
                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_FORMA_PAGTO_SIS", "D", shtNumCaixa, null, null, idFmaPgtoSis, null);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (Exception ex)
                {
                    GravarErroSync("Forma Pagto Sis", tblFmaPgtoSisServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_PARCELAMENTO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblParcelamentoServ = new FDBDataSetVenda.TB_PARCELAMENTODataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taParcelamentoServ = new DataSets.FDBDataSetVendaTableAdapters.TB_PARCELAMENTOTableAdapter())
                        using (var taParcelamentoPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_PARCELAMENTOTableAdapter())
                        using (var tblParcelamentoPdv = new FDBDataSetVenda.TB_PARCELAMENTODataTable())
                        {
                            taParcelamentoServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taParcelamentoServ.Fill(tblParcelamentoServ);

                            taParcelamentoPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taParcelamentoPdv.Fill(tblParcelamentoPdv);

                            using (var changeReader = new DataTableReader(tblParcelamentoServ))
                                tblParcelamentoPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taParcelamentoPdv.Update(tblParcelamentoPdv);

                            tblParcelamentoPdv.AcceptChanges();
                        }
                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesParcelamento = dtAuxSyncPendentes.Select($"TABELA = 'TB_PARCELAMENTO'");

                            for (int i = 0; i < pendentesParcelamento.Length; i++)
                            {
                                var idParcela = pendentesParcelamento[i]["SM_REG"].Safeshort();
                                var operacao = pendentesParcelamento[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taParcelamentoServ = new DataSets.FDBDataSetVendaTableAdapters.TB_PARCELAMENTOTableAdapter())
                                    {
                                        taParcelamentoServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taParcelamentoServ.FillById(tblParcelamentoServ, idParcela);

                                        if (tblParcelamentoServ != null && tblParcelamentoServ.Rows.Count > 0)
                                        {
                                            using (var taParcelamentoPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_PARCELAMENTOTableAdapter())
                                            {
                                                taParcelamentoPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetVenda.TB_PARCELAMENTORow parcelamentoServ in tblParcelamentoServ)
                                                {
                                                    intRetornoUpsert = (int)taParcelamentoPdv.SP_TRI_PARCELA_UPSERT(parcelamentoServ.ID_PARCELA,
                                                               parcelamentoServ.DESCRICAO,
                                                               parcelamentoServ.N_PARCELAS,
                                                               parcelamentoServ.INTERVALO,
                                                               parcelamentoServ.ENTRADA,
                                                               parcelamentoServ.STATUS,
                                                               (parcelamentoServ.IsID_FMAPGTONull() ? null : (short?)parcelamentoServ.ID_FMAPGTO),
                                                               (parcelamentoServ.IsINTERVALO_VARNull() ? null : parcelamentoServ.INTERVALO_VAR),
                                                               DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(-1,
                                                                     "TB_PARCELAMENTO",
                                                                     operacao,
                                                                     shtNumCaixa,
                                                                     null,
                                                                     idParcela,
                                                                     null);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesParcelamento = pendentesParcelamento.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesParcelamento = dtPendentesParcelamento.Select($"SM_REG = '{idParcela}' AND OPERACAO = 'D'");

                                                if (deletesPendentesParcelamento.Length > 0)
                                                {
                                                    foreach (var deletePendenteParcelamento in deletesPendentesParcelamento)
                                                    {
                                                        // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_PARCELAMENTO", "D", shtNumCaixa, null, null, idParcela, null);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TB_PARCELAMENTO) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idParcela}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesParcelamento = dtAuxSyncDeletesPendentes.Select($"SM_REG = {idParcela} AND TABELA = 'TB_PARCELAMENTO' AND OPERACAO = 'D'");

                                                if (deletesPendentesParcelamento.Length <= 0)
                                                {
                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_PARCELAMENTO", "D", shtNumCaixa, null, null, idParcela, null);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (Exception ex)
                {
                    GravarErroSync("Parcelamento", tblParcelamentoServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_FORMA_PAGTO_NFCE(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblFormaPagtoNfceServ = new FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taFormaPagtoNfceServ = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter())
                        using (var taFormaPagtoNfcePdv = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter())
                        using (var tblFormaPagtoNfcePdv = new FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable())
                        {
                            taFormaPagtoNfceServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taFormaPagtoNfceServ.Fill(tblFormaPagtoNfceServ);

                            taFormaPagtoNfcePdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taFormaPagtoNfcePdv.Fill(tblFormaPagtoNfcePdv);

                            using (var changeReader = new DataTableReader(tblFormaPagtoNfceServ))
                                tblFormaPagtoNfcePdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taFormaPagtoNfcePdv.Update(tblFormaPagtoNfcePdv);

                            tblFormaPagtoNfcePdv.AcceptChanges();
                        }
                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesFormaPagtoNfce = dtAuxSyncPendentes.Select($"TABELA = 'TB_FORMA_PAGTO_NFCE'");

                            for (int i = 0; i < pendentesFormaPagtoNfce.Length; i++)
                            {
                                var ID_FMANFCE = pendentesFormaPagtoNfce[i]["SM_REG"].Safeshort();
                                var operacao = pendentesFormaPagtoNfce[i]["OPERACAO"].Safestring();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taFormaPagtoNfceServ = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter())
                                    {
                                        taFormaPagtoNfceServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taFormaPagtoNfceServ.FillByIdFmaNFCe(tblFormaPagtoNfceServ, ID_FMANFCE);

                                        if (tblFormaPagtoNfceServ != null && tblFormaPagtoNfceServ.Rows.Count > 0)
                                        {
                                            using (var taFormaPagtoNfcePdv = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter())
                                            {
                                                taFormaPagtoNfcePdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetVenda.TB_FORMA_PAGTO_NFCERow FormaPagtoNfceServ in tblFormaPagtoNfceServ)
                                                {
                                                    intRetornoUpsert = (int)taFormaPagtoNfcePdv.SP_TRI_FMAPGTONFCE_UPSERT(
                                                                                    FormaPagtoNfceServ.ID_FMANFCE,
                                                                                    (FormaPagtoNfceServ.IsSTATUSNull() ? null : FormaPagtoNfceServ.STATUS),
                                                                                    (FormaPagtoNfceServ.IsID_NFCENull() ? null : FormaPagtoNfceServ.ID_NFCE),
                                                                                    (FormaPagtoNfceServ.IsDESCRICAONull() ? null : FormaPagtoNfceServ.DESCRICAO));

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(-1,
                                                                     "TB_FORMA_PAGTO_NFCE",
                                                                     operacao,
                                                                     shtNumCaixa,
                                                                     null,
                                                                     ID_FMANFCE,
                                                                     null);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesFormaPagtoNfce = pendentesFormaPagtoNfce.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesFormaPagtoNfce = dtPendentesFormaPagtoNfce.Select($"SM_REG = {ID_FMANFCE} AND OPERACAO = 'D'");

                                                if (deletesPendentesFormaPagtoNfce.Length > 0)
                                                {
                                                    foreach (var deletePendenteFormaPagtoNfce in deletesPendentesFormaPagtoNfce)
                                                    {
                                                        // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_FORMA_PAGTO_NFCE", "D", shtNumCaixa, null, null, ID_FMANFCE, null);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TB_FORMA_PAGTO_NFCE) não encontrado no servidor e sem exclusão pendente. \nID do registro: {ID_FMANFCE}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesFormaPagtoNfce = dtAuxSyncDeletesPendentes.Select($"SM_REG = '{ID_FMANFCE}' AND TABELA = 'TB_FORMA_PAGTO_NFCE' AND OPERACAO = 'D'");

                                                if (deletesPendentesFormaPagtoNfce.Length <= 0)
                                                {
                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_FORMA_PAGTO_NFCE", "D", shtNumCaixa, null, null, ID_FMANFCE, null);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (Exception ex)
                {
                    GravarErroSync("TB_FORMA_PAGTO_NFCE", tblFormaPagtoNfceServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TRI_PDV_USERS(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblTriUsersServ = new FDBDataSet.TRI_PDV_USERSDataTable())
            using (var tblTriUsersPdv = new FDBDataSet.TRI_PDV_USERSDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taServ = new TRI_PDV_USERSTableAdapter())
                        using (var taPdv = new TRI_PDV_USERSTableAdapter())
                        {
                            taServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taServ.Fill(tblTriUsersServ);

                            taPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taPdv.Fill(tblTriUsersPdv);

                            //tblClientePdv.Merge(tblClienteServ); Não funciona para o Update(): tblClientePdv.Rows[x].RowState não é alterado, consequentemente o Update() faz nada. Em vez disso, usar o seguinte no lugar do Merge():
                            using (var changeReader = new DataTableReader(tblTriUsersServ))
                                tblTriUsersPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taPdv.Update(tblTriUsersPdv);

                            tblTriUsersPdv.AcceptChanges();
                        }

                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesTriUsers = dtAuxSyncPendentes.Select($"TABELA = 'TRI_PDV_USERS'");

                            for (int i = 0; i < pendentesTriUsers.Length; i++)
                            {
                                var idUser = pendentesTriUsers[i]["SM_REG"].Safeshort();
                                var operacao = pendentesTriUsers[i]["OPERACAO"].Safestring();
                                var NO_CAIXA = pendentesTriUsers[i]["NO_CAIXA"].Safeshort();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taTriUsersServ = new TRI_PDV_USERSTableAdapter())
                                    {
                                        taTriUsersServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taTriUsersServ.FillById(tblTriUsersServ, idUser);

                                        if (tblTriUsersServ != null && tblTriUsersServ.Rows.Count > 0)
                                        {
                                            using (var taTriUsersPdv = new TRI_PDV_USERSTableAdapter())
                                            {
                                                taTriUsersPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSet.TRI_PDV_USERSRow triUsersServ in tblTriUsersServ)
                                                {
                                                    intRetornoUpsert = (int)taTriUsersPdv.SP_TRI_TRIUSERS_UPSERT((short)triUsersServ.ID_USER,
                                                                                                                 triUsersServ.USERNAME,
                                                                                                                 triUsersServ.PASSWORD,
                                                                                                                 triUsersServ.GERENCIA,
                                                                                                                 triUsersServ.ATIVO,
                                                                                                                 DateTime.Now);

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(-1,
                                                                     "TRI_PDV_USERS",
                                                                     operacao,
                                                                     NO_CAIXA,//shtNumCaixa, // TEM QUE ser o mesmo número do caixa consultado na tabela auxiliar.
                                                                     null,
                                                                     idUser);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesTriUsers = pendentesTriUsers.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesTriUsers = dtPendentesTriUsers.Select($"SM_REG = {idUser} AND OPERACAO = 'D'");

                                                if (deletesPendentesTriUsers.Length > 0)
                                                {
                                                    foreach (var deletePendenteTriUsers in deletesPendentesTriUsers)
                                                    {
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TRI_PDV_USERS", "D",
                                                            NO_CAIXA,//shtNumCaixa, 
                                                            null, null, idUser);
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.
                                                    throw new DataException($"Erro não esperado: produto (TRI_PDV_USERS) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idUser}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                DataRow[] deletesPendentesTriUsers = dtAuxSyncDeletesPendentes.Select($"SM_REG = {idUser} AND TABELA = 'TRI_PDV_USERS' AND OPERACAO = 'D'");

                                                if (deletesPendentesTriUsers.Length <= 0)
                                                {
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TRI_PDV_USERS", "D",
                                                        NO_CAIXA,//shtNumCaixa, 
                                                        null, null, idUser);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (Exception ex)
                {
                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("TRI_PDV_USERS(PDV)", tblTriUsersPdv, ex);
                    GravarErroSync("TRI_PDV_USERS(SERV)", tblTriUsersServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_TB_EST_COMPOSICAO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {

            using (var tblComposicaoServ = new FDBDataSetMaitre.TB_EST_COMPOSICAODataTable())
            {
                try
                {
                    #region Sync de cadastros novos ou atualizados

                    #region AUX_SYNC

                    int intRetornoUpsert = 0;

                    {
                        DataRow[] pendentesComposicao = dtAuxSyncPendentes.Select($"TABELA = 'TB_EST_COMPOSICAO'");

                        for (int i = 0; i < pendentesComposicao.Length; i++)
                        {
                            var idComposicao = pendentesComposicao[i]["ID_REG"].Safeint();
                            var operacao = pendentesComposicao[i]["OPERACAO"].Safestring();

                            // Verificar o que deve ser feito com o registro (insert, update ou delete)
                            if (operacao.Equals("I") || operacao.Equals("U"))
                            {
                                // Buscar o registro para executar as operações "Insert" ou "Update"

                                using (var taComposicaoServ = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMPOSICAOTableAdapter())
                                {
                                    taComposicaoServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                    taComposicaoServ.FillById(tblComposicaoServ, idComposicao);

                                    if (tblComposicaoServ != null && tblComposicaoServ.Rows.Count > 0)
                                    {
                                        using (var taComposicaoPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMPOSICAOTableAdapter())
                                        {
                                            taComposicaoPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                            foreach (FDBDataSetMaitre.TB_EST_COMPOSICAORow composicaoServ in tblComposicaoServ)
                                            {
                                                intRetornoUpsert = (int)taComposicaoPdv.SP_TRI_MAIT_ESTCOMP_SYNCINSERT(composicaoServ.ID_COMPOSICAO,
                                                                                          composicaoServ.IsDESCRICAONull() ? string.Empty : composicaoServ.DESCRICAO,
                                                                                          composicaoServ.ID_IDENTIFICADOR,
                                                                                          composicaoServ.IsTRI_PDV_DT_UPDNull() ? null : (DateTime?)composicaoServ.TRI_PDV_DT_UPD);

                                                // Cadastrou a composição e seus itens? Tem que falar pro servidor que o registro foi sincronizado.
                                                if (intRetornoUpsert.Equals(1))
                                                {
                                                    ConfirmarAuxSync(idComposicao,
                                                                 "TB_EST_COMPOSICAO",
                                                                 operacao,
                                                                 shtNumCaixa);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // O item não foi encontrado no servidor.
                                        // Pode ter sido deletado.
                                        // Deve constar essa operação em dtAuxSync.
                                        // Caso contrário, estourar exception.

                                        using (var dtPendentesComposicao = pendentesComposicao.CopyToDataTable())
                                        {
                                            DataRow[] deletesPendentesComposicao = dtPendentesComposicao.Select($"ID_REG = {idComposicao} AND OPERACAO = 'D'");

                                            if (deletesPendentesComposicao.Length > 0)
                                            {
                                                foreach (var deletePendenteComposicao in deletesPendentesComposicao)
                                                {
                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idComposicao, "TB_EST_COMPOSICAO", "D", shtNumCaixa, null, null, -1, null);
                                                }
                                            }
                                            else
                                            {
                                                // Ops....
                                                // Item não encontrado no servidor e não foi deletado?
                                                // Estourar exception.
                                                throw new DataException($"Erro não esperado: produto (TB_EST_COMPOSICAO) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idComposicao}");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Não é uma operação "padrão"

                                switch (operacao)
                                {
                                    case "D":
                                        {
                                            // Não dá pra deletar agora por causa das constraints (FK).
                                            // Adicionar numa lista e deletar depois, na ordem correta.

                                            // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                            DataRow[] deletesPendentesComposicao = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idComposicao} AND TABELA = 'TB_EST_COMPOSICAO' AND OPERACAO = 'D'");

                                            if (deletesPendentesComposicao.Length <= 0)
                                            {
                                                // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                dtAuxSyncDeletesPendentes.Rows.Add(0, idComposicao, "TB_EST_COMPOSICAO", "D", shtNumCaixa, null, null, -1, null);
                                            }

                                            break;
                                        }
                                    default:
                                        throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                        //break;
                                }
                            }
                        }
                    }

                    #endregion AUX_SYNC

                    #endregion Sync de cadastros novos ou atualizados

                }
                catch (NotImplementedException niex)
                {
                    log.Error("Not implemented exception", niex);
                    throw niex;
                }
                catch (DataException dex)
                {
                    log.Error("Data Exception", dex);
                    throw dex;
                }
                catch (Exception ex)
                {
                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Composicao", tblComposicaoServ, ex);
                    throw ex;
                }
            }


        }

        public void Sync_TB_EST_COMP_ITEM(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblCompItemServ = new FDBDataSetMaitre.TB_EST_COMP_ITEMDataTable())
            {
                try
                {
                    #region Sync de cadastros novos ou atualizados

                    #region AUX_SYNC

                    int intRetornoUpsert = 0;

                    {
                        DataRow[] pendentesCompItem = dtAuxSyncPendentes.Select($"TABELA = 'TB_EST_COMP_ITEM'");

                        for (int i = 0; i < pendentesCompItem.Length; i++)
                        {
                            var idItemComp = pendentesCompItem[i]["ID_REG"].Safeint();
                            var operacao = pendentesCompItem[i]["OPERACAO"].Safestring();

                            // Verificar o que deve ser feito com o registro (insert, update ou delete)
                            if (operacao.Equals("I") || operacao.Equals("U"))
                            {
                                // Buscar o registro para executar as operações "Insert" ou "Update"

                                using (var taCompItemServ = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_ITEMTableAdapter())
                                {
                                    taCompItemServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                    taCompItemServ.FillById(tblCompItemServ, idItemComp);

                                    if (tblCompItemServ != null && tblCompItemServ.Rows.Count > 0)
                                    {
                                        using (var taCompItemPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_ITEMTableAdapter())
                                        {
                                            taCompItemPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                            foreach (FDBDataSetMaitre.TB_EST_COMP_ITEMRow compItemServ in tblCompItemServ)
                                            {
                                                intRetornoUpsert = (int)taCompItemPdv.SP_TRI_MT_ESTCMP_ITEM_SYNCINSRT(compItemServ.ID_ITEMCOMP,
                                                                                                 compItemServ.QTD_ITEM,
                                                                                                 compItemServ.ID_COMPOSICAO,
                                                                                                 compItemServ.ID_IDENTIFICADOR);

                                                // Cadastrou a composição e seus itens? Tem que falar pro servidor que o registro foi sincronizado.
                                                if (intRetornoUpsert.Equals(1))
                                                {
                                                    ConfirmarAuxSync(idItemComp,
                                                                 "TB_EST_COMP_ITEM",
                                                                 operacao,
                                                                 shtNumCaixa);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // O item não foi encontrado no servidor.
                                        // Pode ter sido deletado.
                                        // Deve constar essa operação em dtAuxSync.
                                        // Caso contrário, estourar exception.

                                        using (var dtPendentesCompItem = pendentesCompItem.CopyToDataTable())
                                        {
                                            DataRow[] deletesPendentesCompItem = dtPendentesCompItem.Select($"ID_REG = {idItemComp} AND OPERACAO = 'D'");

                                            if (deletesPendentesCompItem.Length > 0)
                                            {
                                                foreach (var deletePendenteCompItem in deletesPendentesCompItem)
                                                {
                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idItemComp, "TB_EST_COMP_ITEM", "D", shtNumCaixa, null, null, -1, null);
                                                }
                                            }
                                            else
                                            {
                                                // Ops....
                                                // Item não encontrado no servidor e não foi deletado?
                                                // Estourar exception.
                                                throw new DataException($"Erro não esperado: produto (TB_EST_COMP_ITEM) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idItemComp}");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Não é uma operação "padrão"

                                switch (operacao)
                                {
                                    case "D":
                                        {
                                            // Não dá pra deletar agora por causa das constraints (FK).
                                            // Adicionar numa lista e deletar depois, na ordem correta.

                                            // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                            DataRow[] deletesPendentesCompItem = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idItemComp} AND TABELA = 'TB_EST_COMP_ITEM' AND OPERACAO = 'D'");

                                            if (deletesPendentesCompItem.Length <= 0)
                                            {
                                                // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                dtAuxSyncDeletesPendentes.Rows.Add(0, idItemComp, "TB_EST_COMP_ITEM", "D", shtNumCaixa, null, null, -1, null);
                                            }

                                            break;
                                        }
                                    default:
                                        throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                        //break;
                                }
                            }
                        }
                    }

                    #endregion AUX_SYNC

                    #endregion Sync de cadastros novos ou atualizados

                }
                catch (NotImplementedException niex)
                {
                    log.Error("Not implemented exception", niex);
                    throw niex;
                }
                catch (DataException dex)
                {
                    log.Error("Data Exception", dex);
                    throw dex;
                }
                catch (Exception ex)
                {
                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("Item de Composicao", tblCompItemServ, ex);
                    throw ex;
                }
            }
        }
        public void Sync_TB_CARTAO_ADMINISTRADORA(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblAdminsServ = new FDBDataSetOperSeed.TB_CARTAO_ADMINISTRADORADataTable())
            {
                try
                {                    
                    {
                        DataRow[] pendentesConfig = dtAuxSyncPendentes.Select($"TABELA = 'TB_CARTAO_ADMINISTRADORA'");
                        for(int i = 0; i < pendentesConfig.Length; i++)
                        {
                            var idAdmins = pendentesConfig[i]["ID_REG"].Safestring();
                            var operacao = pendentesConfig[i]["OPERACAO"].Safestring();
                            var NO_CAIXA = pendentesConfig[i]["NO_CAIXA"].Safeshort();

                            // Verificar o que deve ser feito com o registro (insert, update ou delete)
                            if (operacao.Equals("I") || operacao.Equals("U"))
                            {
                                // Buscar o registro para executar as operações "Insert" ou "Update"
                                using (var taAdminsServ = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CARTAO_ADMINISTRADORATableAdapter())
                                {                                    
                                    taAdminsServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;                                                                        
                                    int.TryParse(idAdmins, out int idAdministradora);
                                    taAdminsServ.FillByIdAdmins(tblAdminsServ, idAdministradora);
                                    if (tblAdminsServ != null && tblAdminsServ.Rows.Count > 0)
                                    {
                                        using (var taAdminsPdv = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CARTAO_ADMINISTRADORATableAdapter())
                                        {
                                            try
                                            {
                                                taAdminsPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                                                foreach (FDBDataSetOperSeed.TB_CARTAO_ADMINISTRADORARow AdminsServ in tblAdminsServ)
                                                {
                                                    fbConnPdv.Open();
                                                    FbCommand comand = new FbCommand("UPDATE OR INSERT INTO TB_CARTAO_ADMINISTRADORA (ID_ADMINISTRADORA, ID_CLIENTE, DESCRICAO, TAXA_CREDITO, TAXA_DEBITO) VALUES (@id_admin, @id_cli, @descri, @taxa_cre, @taxa_deb);", fbConnPdv);
                                                    comand.Parameters.AddWithValue("@id_admin", AdminsServ.ID_ADMINISTRADORA);
                                                    comand.Parameters.AddWithValue("@id_cli", AdminsServ.ID_CLIENTE);
                                                    comand.Parameters.AddWithValue("@descri", AdminsServ.DESCRICAO);
                                                    comand.Parameters.AddWithValue("@taxa_cre", AdminsServ.TAXA_CREDITO);
                                                    comand.Parameters.AddWithValue("@taxa_deb", AdminsServ.TAXA_DEBITO);
                                                    comand.ExecuteNonQuery();                                                    
                                                }
                                                ConfirmarAuxSync(idAdministradora, "TB_CARTAO_ADMINISTRADORA", operacao, NO_CAIXA);
                                            }
                                            catch (Exception ex)
                                            {
                                                log.Debug("erro ao tentar sincronizar insert ou update das administradoras para base local, segue erro: " + ex);
                                            }
                                        }
                                    }
                                }
                            }
                            else if(operacao.Equals("D"))
                            {
                                try
                                {
                                    int.TryParse(idAdmins, out int idAdministradora);
                                    fbConnPdv.Open();
                                    FbCommand comand = new FbCommand("DELETE FROM TB_CARTAO_ADMINISTRADORA WHERE ID_ADMINISTRADORA = " + idAdministradora, fbConnPdv);
                                    comand.ExecuteNonQuery();
                                    ConfirmarAuxSync(idAdministradora, "TB_CARTAO_ADMINISTRADORA", operacao, NO_CAIXA);
                                }
                                catch (Exception ex)
                                {
                                    log.Debug("erro ao tentar sincronizar delete das administradoras para base local, segue erro: " + ex);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    log.Debug("Erro ao sincronizar administradoras, erro: " + ex);
                }
            }
        }

        public void Sync_TRI_PDV_CONFIG(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            using (var tblConfigServ = new FDBDataSetConfig.TRI_PDV_CONFIGDataTable())
            {
                try
                {
                    if (dtUltimaSyncPdv is null)
                    {
                        #region Única sync

                        using (var taConfigServ = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter())
                        using (var taConfigPdv = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter())
                        using (var tblConfigPdv = new FDBDataSetConfig.TRI_PDV_CONFIGDataTable())
                        {
                            taConfigServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            taConfigServ.Fill(tblConfigServ);

                            taConfigPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            taConfigPdv.Fill(tblConfigPdv);

                            using (var changeReader = new DataTableReader(tblConfigServ))
                                tblConfigPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            taConfigPdv.Update(tblConfigPdv);

                            tblConfigPdv.AcceptChanges();
                        }

                        #endregion Única sync
                    }
                    else
                    {
                        #region Sync de cadastros novos ou atualizados

                        #region AUX_SYNC

                        int intRetornoUpsert = 0;

                        {
                            DataRow[] pendentesConfig = dtAuxSyncPendentes.Select($"TABELA = 'TRI_PDV_CONFIG'");

                            for (int i = 0; i < pendentesConfig.Length; i++)
                            {
                                var idMac = pendentesConfig[i]["UN_REG"].Safestring();
                                var operacao = pendentesConfig[i]["OPERACAO"].Safestring();
                                var NO_CAIXA = pendentesConfig[i]["NO_CAIXA"].Safeshort();

                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                if (operacao.Equals("I") || operacao.Equals("U"))
                                {
                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                    using (var taConfigServ = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter())
                                    {
                                        taConfigServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                        taConfigServ.FillById(tblConfigServ, idMac);

                                        if (tblConfigServ != null && tblConfigServ.Rows.Count > 0)
                                        {
                                            using (var taConfigPdv = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter())
                                            {
                                                taConfigPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                foreach (FDBDataSetConfig.TRI_PDV_CONFIGRow configServ in tblConfigServ)
                                                {
                                                    intRetornoUpsert = (int)taConfigPdv.SP_TRI_CONFIG_UPSERT(configServ.ID_MAC,
                                                                                                            configServ.NO_CAIXA,
                                                                                                            configServ.EXIGE_SANGRIA,
                                                                                                            configServ.VALOR_MAX_CAIXA,
                                                                                                            configServ.BLOQUEIA_NO_LIMITE,
                                                                                                            configServ.VALOR_DE_FOLGA,
                                                                                                            configServ.PERMITE_FOLGA_SANGRIA,
                                                                                                            configServ.INTERROMPE_NAO_ENCONTRADO,
                                                                                                            (configServ.IsMENSAGEM_CORTESIANull() ? null : configServ.MENSAGEM_CORTESIA),
                                                                                                            (configServ.IsICMS_CONTNull() ? null : (float?)configServ.ICMS_CONT),
                                                                                                            (configServ.IsCSOSN_CONTNull() ? null : (float?)configServ.CSOSN_CONT),
                                                                                                            configServ.PEDE_CPF,
                                                                                                            configServ.PERMITE_ESTOQUE_NEGATIVO,
                                                                                                            configServ.MODELO_CUPOM,
                                                                                                            (configServ.IsMENSAGEM_RODAPENull() ? null : configServ.MENSAGEM_RODAPE),
                                                                                                            DateTime.Now,
                                                                                                            (configServ.IsMODELO_SATNull() ? null : (int?)configServ.MODELO_SAT),
                                                                                                            (configServ.IsSATSERVIDORNull() ? null : configServ.SATSERVIDOR),
                                                                                                            (configServ.IsSAT_CODATIVNull() ? null : configServ.SAT_CODATIV),
                                                                                                            (configServ.IsSIGN_ACNull() ? null : configServ.SIGN_AC),
                                                                                                            (configServ.IsSAT_USADONull() ? null : configServ.SAT_USADO),
                                                                                                            (configServ.IsECF_ATIVANull() ? null : configServ.ECF_ATIVA),
                                                                                                            (configServ.IsECF_PORTANull() ? null : configServ.ECF_PORTA),
                                                                                                            (configServ.IsIMPRESSORA_USBNull() ? null : configServ.IMPRESSORA_USB),
                                                                                                            (configServ.IsIMPRESSORA_USB_PEDNull() ? null : configServ.IMPRESSORA_USB_PED),
                                                                                                            configServ.PERGUNTA_WHATS,
                                                                                                            configServ.USATEF,
                                                                                                            (configServ.IsTEFIPNull() ? null : configServ.TEFIP),
                                                                                                            (configServ.IsTEFNUMLOJANull() ? null : configServ.TEFNUMLOJA),
                                                                                                            (configServ.IsTEFNUMTERMINALNull() ? null : configServ.TEFNUMTERMINAL),
                                                                                                            (configServ.IsTEFPEDECPFPELOPINPADNull() ? null : configServ.TEFPEDECPFPELOPINPAD),
                                                                                                            configServ.BALPORTA, configServ.BALBITS, configServ.BALBAUD, configServ.BALPARITY,
                                                                                                            configServ.BALMODELO, configServ.ACFILLPREFIX, configServ.ACFILLMODE, configServ.ACREFERENCIA,
                                                                                                            configServ.SYSCOMISSAO, configServ.SATSERVTIMEOUT, configServ.SATLIFESIGNINTERVAL,
                                                                                                            configServ.ACFILLDELAY, configServ.SYSPERGUNTAWHATS, configServ.SYSPARCELA, configServ.SYSEMITECOMPROVANTE, configServ.INFORMA_MAQUININHA);                                                                                                           

                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                    if (intRetornoUpsert.Equals(1))
                                                    {
                                                        ConfirmarAuxSync(-1,
                                                                     "TRI_PDV_CONFIG",
                                                                     operacao,
                                                                     NO_CAIXA,//shtNumCaixa,
                                                                     idMac);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // O item não foi encontrado no servidor.
                                            // Pode ter sido deletado.
                                            // Deve constar essa operação em dtAuxSync.
                                            // Caso contrário, estourar exception.

                                            using (var dtPendentesConfig = pendentesConfig.CopyToDataTable())
                                            {
                                                DataRow[] deletesPendentesConfig = dtPendentesConfig.Select($"UN_REG = '{idMac}' AND OPERACAO = 'D'");

                                                if (deletesPendentesConfig.Length > 0)
                                                {
                                                    foreach (var deletePendenteConfig in deletesPendentesConfig)
                                                    {
                                                        // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TRI_PDV_CONFIG", "D",
                                                            NO_CAIXA,//shtNumCaixa, 
                                                            null, idMac, -1, null);

                                                        //TODO: mudar shtNumCaixa pelo número do caixa que consta na tabela auxiliar
                                                    }
                                                }
                                                else
                                                {
                                                    // Ops....
                                                    // Item não encontrado no servidor e não foi deletado?
                                                    // Estourar exception.

                                                    throw new DataException($"Erro não esperado: produto (TRI_PDV_CONFIG) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idMac}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Não é uma operação "padrão"

                                    switch (operacao)
                                    {
                                        case "D":
                                            {
                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                DataRow[] deletesPendentesConfig = dtAuxSyncDeletesPendentes.Select($"UN_REG = '{idMac}' AND TABELA = 'TRI_PDV_CONFIG' AND OPERACAO = 'D'");

                                                if (deletesPendentesConfig.Length <= 0)
                                                {
                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TRI_PDV_CONFIG", "D",
                                                        NO_CAIXA,//shtNumCaixa, 
                                                        null, idMac, -1, null);
                                                }

                                                break;
                                            }
                                        default:
                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                            //break;
                                    }
                                }
                            }
                        }

                        #endregion AUX_SYNC

                        #endregion Sync de cadastros novos ou atualizados
                    }
                }
                catch (Exception ex)
                {
                    //audit("SINCCONTNETDB>> " + "Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    GravarErroSync("TRI_PDV_CONFIG", tblConfigServ, ex);
                    throw ex;
                }
            }
        }

        public void Sync_Delete_TB_EST_PRODUTO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drEstProdutoDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_PRODUTO'");

                if (drEstProdutoDeletesPendentes.Length > 0)
                {
                    using (var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter())
                    {
                        taEstProdutoPdv.Connection = fbConnPdv;

                        foreach (var drEstProdutoDeletePendente in drEstProdutoDeletesPendentes)
                        {
                            taEstProdutoPdv.Delete(drEstProdutoDeletePendente["ID_REG"].Safeint());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(drEstProdutoDeletePendente["ID_REG"].Safeint(),
                                             drEstProdutoDeletePendente["TABELA"].Safestring(),
                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_EST_PRODUTO", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_EST_IDENTIFICADOR(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drEstIdentifDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_IDENTIFICADOR'");

                if (drEstIdentifDeletesPendentes.Length > 0)
                {
                    using (var taEstIdentifPdv = new TB_EST_IDENTIFICADORTableAdapter())
                    {
                        //taEstIdentifPdv.Connection = fbConnPdv;

                        foreach (var drEstIdentifDeletePendente in drEstIdentifDeletesPendentes)
                        {
                            #region TB_EST_SALDO_ALTERADO

                            // Apagar dependência

                            using (var transactionScopeIdentifSaldo = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                            using (var commSaldoAlteradoPdv = new FbCommand())
                            using (var fbConnIdentifSaldoPdv = new FbConnection())
                            {
                                fbConnIdentifSaldoPdv.ConnectionString = _strConnContingency;

                                fbConnIdentifSaldoPdv.Open();

                                commSaldoAlteradoPdv.Connection = fbConnIdentifSaldoPdv;
                                commSaldoAlteradoPdv.CommandType = CommandType.Text;

                                taEstIdentifPdv.Connection = fbConnIdentifSaldoPdv;

                                //foreach (var drFuncDeletePendente in drFuncDeletesPendentes)
                                //{
                                commSaldoAlteradoPdv.CommandText = $"DELETE FROM TB_EST_SALDO_ALTERADO WHERE ID_IDENTIFICADOR = {drEstIdentifDeletePendente["ID_REG"]}";
                                //TODO: talvez criar um INDEX para TB_EST_SALDO_ALTERADO.ID_IDENTIFICADOR? Pode melhorar a performance. Principalmente depois de criar uma sproc para deletar registros, também.

                                commSaldoAlteradoPdv.ExecuteNonQuery();

                                //}

                                #endregion TB_EST_SALDO_ALTERADO

                                taEstIdentifPdv.Delete(drEstIdentifDeletePendente["ID_REG"].Safeint());

                                // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                ConfirmarAuxSync(drEstIdentifDeletePendente["ID_REG"].Safeint(),
                                                 drEstIdentifDeletePendente["TABELA"].Safestring(),
                                                 "D", //drEstIdentifDeletePendente["OPERACAO"].Safestring(),
                                                 shtNumCaixa);

                                transactionScopeIdentifSaldo.Complete();

                                fbConnIdentifSaldoPdv?.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_EST_IDENTIFICADOR", ex);
                throw ex;
            }
        }

        private readonly FuncoesFirebird _funcoes = new();
        public void Sync_Delete_TB_ESTOQUE(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drEstoqueDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_ESTOQUE'");

                if (drEstoqueDeletesPendentes.Length > 0)
                {
                    foreach (var drEstoqueDeletePendente in drEstoqueDeletesPendentes)
                    {
                        // Apagar dependência

                        using (var transactionScopeDependenciasEstoque = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                        using (var commEstDescHistPdv = new FbCommand())
                        using (var commEstIndexador = new FbCommand())
                        using (var commEstFornecedor = new FbCommand())
                        using (var commEstAdicional = new FbCommand())
                        using (var fbConnDependenciasEstoquePdv = new FbConnection())
                        using (var taEstoquePdv = new TB_ESTOQUETableAdapter())
                        {
                            fbConnDependenciasEstoquePdv.ConnectionString = _strConnContingency;
                            fbConnDependenciasEstoquePdv.Open();

                            #region TB_EST_DESC_HISTORICO

                            try
                            {
                                commEstDescHistPdv.Connection = fbConnDependenciasEstoquePdv;
                                commEstDescHistPdv.CommandType = CommandType.Text;

                                commEstDescHistPdv.CommandText = $"DELETE FROM TB_EST_DESC_HISTORICO WHERE ID_ESTOQUE = {drEstoqueDeletePendente["ID_REG"]}";
                                commEstDescHistPdv.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_DESC_HISTORICO - SQL syntaxe\n DELETE FROM TB_EST_DESC_HISTORICO WHERE ID_ESTOQUE = {drEstoqueDeletePendente['ID_REG']}\n", ex);
                                throw ex;
                            }
                            #endregion TB_EST_SALDO_ALTERADO

                            #region TB_EST_INDEXADOR
                            try
                            {
                                commEstIndexador.Connection = fbConnDependenciasEstoquePdv;
                                commEstIndexador.CommandType = CommandType.Text;

                                commEstIndexador.CommandText = $"DELETE FROM TB_EST_INDEXADOR WHERE ID_ESTOQUE = {drEstoqueDeletePendente["ID_REG"]}";
                                commEstIndexador.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_INDEXADOR - SQL syntaxe\n DELETE FROM TB_EST_INDEXADOR WHERE ID_ESTOQUE = {drEstoqueDeletePendente['ID_REG']}\n", ex);
                                throw ex;
                            }
                            #endregion TB_EST_INDEXADOR

                            #region TB_EST_FORNECEDOR
                            try
                            {
                                commEstFornecedor.Connection = fbConnDependenciasEstoquePdv;
                                commEstFornecedor.CommandType = CommandType.Text;

                                commEstFornecedor.CommandText = $"DELETE FROM TB_EST_FORNECEDOR WHERE ID_ESTOQUE = {drEstoqueDeletePendente["ID_REG"]}";
                                commEstFornecedor.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_FORNECEDOR - SQL syntaxe\n DELETE FROM TB_EST_FORNECEDOR WHERE ID_ESTOQUE = {drEstoqueDeletePendente['ID_REG']}\n", ex);
                                throw ex;
                            }
                            #endregion TB_EST_FORNECEDOR

                            #region TB_EST_ADICIONAL
                            try
                            {
                                commEstAdicional.Connection = fbConnDependenciasEstoquePdv;
                                commEstAdicional.CommandType = CommandType.Text;

                                commEstAdicional.CommandText = $"DELETE FROM TB_EST_ADICIONAL WHERE ID_ESTOQUE = {drEstoqueDeletePendente["ID_REG"]}";
                                commEstAdicional.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_ADICIONAL - SQL syntaxe\n DELETE FROM TB_EST_ADICIONAL WHERE ID_ESTOQUE = {drEstoqueDeletePendente['ID_REG']}\n", ex);
                                throw ex;
                            }
                            #endregion TB_EST_ADICIONAL

                            #region TB_EST_PRC_VENDA_HISTORICO
                            try
                            {
                                commEstAdicional.Connection = fbConnDependenciasEstoquePdv;
                                commEstAdicional.CommandType = CommandType.Text;

                                commEstAdicional.CommandText = $"DELETE FROM TB_EST_PRC_VENDA_HISTORICO WHERE ID_ESTOQUE = {drEstoqueDeletePendente["ID_REG"]}";
                                commEstAdicional.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_PRC_VENDA_HISTORICO - SQL syntaxe\n DELETE FROM TB_EST_PRC_VENDA_HISTORICO WHERE ID_ESTOQUE = {drEstoqueDeletePendente['ID_REG']}\n", ex);
                                throw ex;
                            }
                            #endregion TB_EST_PRC_VENDA_HISTORICO

                            #region TB_ESTOQUE
                            try
                            {
                                taEstoquePdv.Connection = fbConnDependenciasEstoquePdv;
                                taEstoquePdv.Delete(drEstoqueDeletePendente["ID_REG"].Safeint());

                                #endregion TB_ESTOQUE


                                // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                ConfirmarAuxSync(drEstoqueDeletePendente["ID_REG"].Safeint(),
                                                 drEstoqueDeletePendente["TABELA"].Safestring(),
                                                 "D", //drEstIdentifDeletePendente["OPERACAO"].Safestring(),
                                                 shtNumCaixa);

                                transactionScopeDependenciasEstoque.Complete();

                                fbConnDependenciasEstoquePdv?.Close();

                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao falar pro servidor que o registro foi sincronizado. - \n", ex);
                                throw ex;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_ESTOQUE e/ou suas dependências: ", ex);
                _funcoes.ClearAuxSyncTable(fbConnServ);
                throw ex;
            }
        }

        public void Sync_Delete_TB_FORNECEDOR(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drFornecDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FORNECEDOR'");

                if (drFornecDeletesPendentes.Length > 0)
                {
                    using (var taFornecPdv = new TB_FORNECEDORTableAdapter())
                    {
                        taFornecPdv.Connection = fbConnPdv;

                        foreach (var drFornecDeletePendente in drFornecDeletesPendentes)
                        {
                            taFornecPdv.Delete(drFornecDeletePendente["ID_REG"].Safeint());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(drFornecDeletePendente["ID_REG"].Safeint(),
                                             drFornecDeletePendente["TABELA"].Safestring(),
                                             "D", //drFornecDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_FORNECEDOR", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_EST_GRUPO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drEstGrupoDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_GRUPO'");

                if (drEstGrupoDeletesPendentes.Length > 0)
                {
                    using (var taEstGrupoPdv = new TB_EST_GRUPOTableAdapter())
                    {
                        taEstGrupoPdv.Connection = fbConnPdv;

                        foreach (var drEstGrupoDeletePendente in drEstGrupoDeletesPendentes)
                        {
                            taEstGrupoPdv.DeleteById(drEstGrupoDeletePendente["ID_REG"].Safeint());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(drEstGrupoDeletePendente["ID_REG"].Safeint(),
                                             drEstGrupoDeletePendente["TABELA"].Safestring(),
                                             "D", //drEstGrupoDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_EST_GRUPO", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_CLIENTE(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drClienteDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_CLIENTE'");

                if (drClienteDeletesPendentes.Length > 0)
                {
                    using (var taClientePdv = new TB_CLIENTETableAdapter())
                    using (var taCliPfPdv = new TB_CLI_PFTableAdapter())
                    using (var taCliPjPdv = new TB_CLI_PJTableAdapter())
                    {
                        taClientePdv.Connection = fbConnPdv;
                        taCliPfPdv.Connection = fbConnPdv;
                        taCliPjPdv.Connection = fbConnPdv;

                        foreach (var drClienteDeletePendente in drClienteDeletesPendentes)
                        {
                            int idCliente = drClienteDeletePendente["ID_REG"].Safeint();
                            taCliPfPdv.DeleteById(idCliente);
                            taCliPjPdv.DeleteById(idCliente);
                            taClientePdv.Delete(idCliente);

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(drClienteDeletePendente["ID_REG"].Safeint(),
                                             drClienteDeletePendente["TABELA"].Safestring(),
                                             "D",//drClienteDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_CLIENTE", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_FUNCIONARIO_TB_FUNC_PAPEL_TB_FUNC_COMISSAO(short shtNumCaixa, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes)
        {
            try
            {
                var drFuncDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FUNCIONARIO'");

                if (drFuncDeletesPendentes.Length > 0)
                {
                    using (var transactionScopeFunc = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                    using (var commFuncComissaoPdv = new FbCommand())
                    using (var taFuncPapelPdv = new TB_FUNC_PAPELTableAdapter())
                    using (var taFuncPdv = new TB_FUNCIONARIOTableAdapter())
                    using (var fbConnFuncPdv = new FbConnection())
                    {
                        fbConnFuncPdv.ConnectionString = _strConnContingency;

                        fbConnFuncPdv.Open();

                        commFuncComissaoPdv.Connection = fbConnFuncPdv;
                        commFuncComissaoPdv.CommandType = CommandType.Text;

                        taFuncPapelPdv.Connection = fbConnFuncPdv;
                        taFuncPdv.Connection = fbConnFuncPdv;

                        foreach (var drFuncDeletePendente in drFuncDeletesPendentes)
                        {
                            commFuncComissaoPdv.CommandText = $"DELETE FROM TB_FUNC_COMISSAO WHERE ID_FUNCIONARIO = {drFuncDeletePendente["ID_REG"]}";

                            commFuncComissaoPdv.ExecuteNonQuery();

                            taFuncPapelPdv.DeleteByIdFunc(drFuncDeletePendente["ID_REG"].Safeint());
                            taFuncPdv.DeleteById(drFuncDeletePendente["ID_REG"].Safeint());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(drFuncDeletePendente["ID_REG"].Safeint(),
                                             drFuncDeletePendente["TABELA"].Safestring(),
                                             "D", //drFuncDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa);
                        }

                        transactionScopeFunc.Complete();

                        fbConnFuncPdv?.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_FUNCIONARIO / TB_FUNC_PAPEL / TB_FUNC_COMISSAO ", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_UNI_MEDIDA(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drUniMedDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_UNI_MEDIDA'");

                if (drUniMedDeletesPendentes.Length > 0)
                {
                    using (var taUniMedPdv = new TB_UNI_MEDIDATableAdapter())
                    {
                        taUniMedPdv.Connection = fbConnPdv;

                        foreach (var drUniMedDeletePendente in drUniMedDeletesPendentes)
                        {
                            taUniMedPdv.DeleteByUnidade(drUniMedDeletePendente["UN_REG"].Safestring());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(0, //drUniMedDeletePendente["ID_REG"].Safeint(),
                                             drUniMedDeletePendente["TABELA"].Safestring(),
                                             "D",//drClienteDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa,
                                             drUniMedDeletePendente["UN_REG"].Safestring());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_UNI_MEDIDA", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_FUNC_PAPEL(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drFuncPapelDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FUNC_PAPEL'");

                if (drFuncPapelDeletesPendentes.Length > 0)
                {
                    using (var taFuncPapelPdv = new TB_FUNC_PAPELTableAdapter())
                    {
                        taFuncPapelPdv.Connection = fbConnPdv;

                        foreach (var drFuncPapelDeletePendente in drFuncPapelDeletesPendentes)
                        {
                            taFuncPapelPdv.DeleteByIds(drFuncPapelDeletePendente["ID_REG"].Safeint(),
                                                       drFuncPapelDeletePendente["SM_REG"].Safeshort());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(drFuncPapelDeletePendente["ID_REG"].Safeint(),
                                             drFuncPapelDeletePendente["TABELA"].Safestring(),
                                             "D",//drClienteDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa,
                                             null,
                                             drFuncPapelDeletePendente["SM_REG"].Safeshort());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_UNI_MEDIDA", ex);
                throw ex;
            }

        }

        public void Sync_Delete_TRI_PDV_USERS(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drTriUsersDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TRI_PDV_USERS'");

                if (drTriUsersDeletesPendentes.Length > 0)
                {
                    using (var taTriUsersPdv = new TRI_PDV_USERSTableAdapter())
                    {
                        taTriUsersPdv.Connection = fbConnPdv;

                        foreach (var drTriUsersDeletePendente in drTriUsersDeletesPendentes)
                        {
                            taTriUsersPdv.DeleteById(drTriUsersDeletePendente["SM_REG"].Safeshort());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(-1,
                                             drTriUsersDeletePendente["TABELA"].Safestring(),
                                             "D", //drEstoqueDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa,
                                             null,
                                             drTriUsersDeletePendente["SM_REG"].Safeshort());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TRI_PDV_USERS", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_TAXA_UF(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drTaxaUfDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_TAXA_UF'");

                if (drTaxaUfDeletesPendentes.Length > 0)
                {
                    using (var taTaxaUfPdv = new TB_TAXA_UFTableAdapter())
                    {
                        taTaxaUfPdv.Connection = fbConnPdv;

                        foreach (var drTaxaUfDeletePendente in drTaxaUfDeletesPendentes)
                        {
                            taTaxaUfPdv.DeleteById(drTaxaUfDeletePendente["CH_REG"].Safestring());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(-1,
                                             drTaxaUfDeletePendente["TABELA"].Safestring(),
                                             "D", //drEstoqueDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa,
                                             null,
                                             -1,
                                             drTaxaUfDeletePendente["CH_REG"].Safestring());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_TAXA_UF", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_CFOP_SIS(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drCfopSisDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_CFOP_SIS'");

                if (drCfopSisDeletesPendentes.Length > 0)
                {
                    using (var taCfopSisPdv = new TB_CFOP_SISTableAdapter())
                    {
                        taCfopSisPdv.Connection = fbConnPdv;

                        foreach (var drCfopSisDeletePendente in drCfopSisDeletesPendentes)
                        {
                            taCfopSisPdv.DeleteById(drCfopSisDeletePendente["UN_REG"].Safestring());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(-1,
                                             drCfopSisDeletePendente["TABELA"].Safestring(),
                                             "D", //drEstoqueDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa,
                                             drCfopSisDeletePendente["UN_REG"].Safestring(),
                                             -1,
                                             null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_TAXA_UF", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_NAT_OPERACAO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drNatOperDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_NAT_OPERACAO'");

                if (drNatOperDeletesPendentes.Length > 0)
                {
                    using (var taNatOperPdv = new TB_NAT_OPERACAOTableAdapter())
                    {
                        taNatOperPdv.Connection = fbConnPdv;

                        foreach (var drNatOperDeletePendente in drNatOperDeletesPendentes)
                        {
                            taNatOperPdv.DeleteById(drNatOperDeletePendente["ID_REG"].Safeint());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(drNatOperDeletePendente["ID_REG"].Safeint(),
                                             drNatOperDeletePendente["TABELA"].Safestring(),
                                             "D", //drEstoqueDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa,
                                             null,
                                             -1,
                                             null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_NAT_OPERACAO", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_FORMA_PAGTO_SIS(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drFmaPgtoSisDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FORMA_PAGTO_SIS'");

                if (drFmaPgtoSisDeletesPendentes.Length > 0)
                {
                    using (var taFmaPgtoSisPdv = new TB_FORMA_PAGTO_SISTableAdapter())
                    {
                        taFmaPgtoSisPdv.Connection = fbConnPdv;

                        foreach (var drFmaPgtoSisDeletePendente in drFmaPgtoSisDeletesPendentes)
                        {
                            taFmaPgtoSisPdv.DeleteById(drFmaPgtoSisDeletePendente["SM_REG"].Safeshort());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(-1,
                                             drFmaPgtoSisDeletePendente["TABELA"].Safestring(),
                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa,
                                             null,
                                             drFmaPgtoSisDeletePendente["SM_REG"].Safeshort(),
                                             null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_FORMA_PAGTO_SIS", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_PARCELAMENTO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drParcelamentoDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_PARCELAMENTO'");

                if (drParcelamentoDeletesPendentes.Length > 0)
                {
                    using (var taParcelamentoPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_PARCELAMENTOTableAdapter())
                    {
                        taParcelamentoPdv.Connection = fbConnPdv;

                        foreach (var drParcelamentoDeletePendente in drParcelamentoDeletesPendentes)
                        {
                            taParcelamentoPdv.DeleteById(drParcelamentoDeletePendente["SM_REG"].Safeshort());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(-1,
                                             drParcelamentoDeletePendente["TABELA"].Safestring(),
                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa,
                                             null,
                                             drParcelamentoDeletePendente["SM_REG"].Safeshort(),
                                             null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_PARCELAMENTO", ex);
                throw ex;
            }
        }

        public void Sync_Delete_TB_FORMA_PAGTO_NFCE(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            try
            {
                var drFormaPagtoNfceDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FORMA_PAGTO_NFCE'");

                if (drFormaPagtoNfceDeletesPendentes.Length > 0)
                {
                    using (var taFormaPagtoNfcePdv = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter())
                    {
                        taFormaPagtoNfcePdv.Connection = fbConnPdv;

                        foreach (var drFormaPagtoNfceDeletePendente in drFormaPagtoNfceDeletesPendentes)
                        {
                            taFormaPagtoNfcePdv.DeleteById(drFormaPagtoNfceDeletePendente["SM_REG"].Safeshort());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(-1,
                                             drFormaPagtoNfceDeletePendente["TABELA"].Safestring(),
                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa,
                                             null,
                                             drFormaPagtoNfceDeletePendente["SM_REG"].Safeshort(),
                                             null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_FORMA_PAGTO_NFCE", ex);
                throw ex;
            }
        }

        public void Sync_Delete_COMPOSICAO(DateTime? dtUltimaSyncPdv, FbConnection fbConnServ, FbConnection fbConnPdv, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncPendentes, FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable dtAuxSyncDeletesPendentes, short shtNumCaixa)
        {
            #region TB_EST_COMP_ITEM

            try
            {
                var drCompItemDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_COMP_ITEM'");

                if (drCompItemDeletesPendentes.Length > 0)
                {
                    using (var taCompItemPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_ITEMTableAdapter())
                    {
                        taCompItemPdv.Connection = fbConnPdv;

                        foreach (var drCompItemDeletePendente in drCompItemDeletesPendentes)
                        {
                            taCompItemPdv.DeleteById(drCompItemDeletePendente["ID_REG"].Safeint());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(drCompItemDeletePendente["ID_REG"].Safeint(),
                                             drCompItemDeletePendente["TABELA"].Safestring(),
                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_EST_COMP_ITEM", ex);
                throw ex;
            }

            #endregion TB_EST_COMP_ITEM

            #region TB_EST_COMPOSICAO

            try
            {
                var drComposicaoDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_COMPOSICAO'");

                if (drComposicaoDeletesPendentes.Length > 0)
                {
                    using (var taComposicaoPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMPOSICAOTableAdapter())
                    {
                        taComposicaoPdv.Connection = fbConnPdv;

                        foreach (var drComposicaoDeletePendente in drComposicaoDeletesPendentes)
                        {
                            taComposicaoPdv.DeleteById(drComposicaoDeletePendente["ID_REG"].Safeint());

                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                            ConfirmarAuxSync(drComposicaoDeletePendente["ID_REG"].Safeint(),
                                             drComposicaoDeletePendente["TABELA"].Safestring(),
                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                             shtNumCaixa);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao deletar registro de TB_EST_COMPOSICAO", ex);
                throw ex;
            }

            #endregion TB_EST_COMPOSICAO

        }

        #endregion

        #region Métodos Operações
        private void Sync_Operacoes_TRI_PDV_TERMINAL_USUARIO_INCOMPLETO(EnmTipoSync tipoSync)
        {

            if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
            {
                using (var tblTermarioPdv = new FDBDataSetOperSeed.TRI_PDV_TERMINAL_USUARIODataTable())
                using (var tblTermarioServ = new FDBDataSetOperSeed.TRI_PDV_TERMINAL_USUARIODataTable())
                {
                    try
                    {
                        using (var taTermarioPdv = new TRI_PDV_TERMINAL_USUARIOTableAdapter())
                        {
                            taTermarioPdv.Connection.ConnectionString = _strConnContingency;

                            #region (1)Consultar o primeiro registro com a maior CURRENTTIME:

                            taTermarioPdv.FillByNumCaixaAberturaLast(tblTermarioPdv, _intNoCaixa);

                            if (tblTermarioPdv != null && tblTermarioPdv.Rows.Count > 0)
                            {
                                // deu ruim
                                if (tblTermarioPdv.Rows.Count > 1) { throw new Exception($"Retorno não esperado: taTermarioPdv.FillByNumCaixaAberturaLast({_intNoCaixa}) retornou mais de um registro."); }

                                using (var taTermarioServ = new TRI_PDV_TERMINAL_USUARIOTableAdapter())
                                {
                                    taTermarioServ.Connection.ConnectionString = _strConnNetwork;

                                    #region (1.2.1)UPSERT no serv.
                                    taTermarioServ.SP_TRI_TERMARIO_UPSERT_1(//tblTermarioPdv[0].ID_OPER, // A PK não deve ser passada do PDV para o servidor....
                                                                            tblTermarioPdv[0].NUM_CAIXA,
                                                                            tblTermarioPdv[0].STATUS,
                                                                            tblTermarioPdv[0].TS_ABERTURA,
                                                                            tblTermarioPdv[0].IsTS_FECHAMENTONull() ? null : (DateTime?)tblTermarioPdv[0].TS_FECHAMENTO,
                                                                            tblTermarioPdv[0].ID_USER);
                                    #endregion (1.2.1)UPSERT no serv.

                                }
                            }

                            #endregion (1)Consultar o primeiro registro com a maior CURRENTTIME:
                        }
                    }
                    catch (Exception ex)
                    {
                        //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                        GravarErroSync("TRI_PDV_TERMINAL_USUARIO(P->S)(PDV)", tblTermarioPdv, ex);
                        GravarErroSync("TRI_PDV_TERMINAL_USUARIO(P->S)(SERV)", tblTermarioServ, ex);
                        throw ex;
                    }
                }
            }
        }

        private void Sync_Operacoes_TRI_PDV_SANSUP_PDV_Serv(EnmTipoSync tipoSync)
        {
            if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
            {
                using (var tblSanSupPdv = new FDBDataSetOperSeed.TRI_PDV_SANSUPDataTable())
                using (var tblSanSupServ = new FDBDataSetOperSeed.TRI_PDV_SANSUPDataTable())
                {
                    try
                    {
                        using (var taSanSupPdv = new TRI_PDV_SANSUPTableAdapter())
                        {
                            taSanSupPdv.Connection.ConnectionString = _strConnContingency;

                            //Preenche a tabela com os registros não enviados para o servidor.

                            taSanSupPdv.FillByNotSynched(tblSanSupPdv);

                            //Caso haja algum registro a ser enviado para o servidor...

                            if (tblSanSupPdv != null && tblSanSupPdv.Rows.Count > 0)
                            {
                                using (var taSanSupServ = new TRI_PDV_SANSUPTableAdapter())
                                {
                                    taSanSupServ.Connection.ConnectionString = _strConnNetwork;
                                    foreach (FDBDataSetOperSeed.TRI_PDV_SANSUPRow row in tblSanSupPdv)
                                    {
                                        taSanSupServ.Insert(-1, row.ID_CAIXA, row.TS_ABERTURA, row.OPERACAO, row.VALOR, row.TS_OPERACAO, "S");
                                        row.SYNCHED = "S";
                                        taSanSupPdv.Update(row);
                                    }

                                }
                            }
                            //20205747380355 Vivo
                            //210120207984759 Vivo 10615
                            //
                        }
                    }
                    catch (Exception ex)
                    {
                        //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                        GravarErroSync("TRI_PDV_TERMINAL_USUARIO(P->S)(PDV)", tblSanSupPdv, ex);
                        GravarErroSync("TRI_PDV_TERMINAL_USUARIO(P->S)(SERV)", tblSanSupServ, ex);
                        throw ex;
                    }
                }
            }

        }


        #endregion

        #region Metodos de venda PDV-Serv

        private void Sync_TRI_PDV_OPER_PDV_Serv(EnmTipoSync tipoSync)
        {
            // Para todos os efeitos, sangria e suprimento são considerados como venda.
            if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
            {
                using (var tblOperPdv = new FDBDataSetVenda.TRI_PDV_OPERDataTable())
                using (var tblOperServ = new FDBDataSetVenda.TRI_PDV_OPERDataTable())
                {
                    try
                    {
                        #region roteiro
                        // (1)Consultar o primeiro registro com a maior CURRENTTIME:
                        //      - Tem registro:
                        //              - Ver o campo ABERTO:
                        //                      - "S":
                        //                              - (1.1)UPSERT no serv.
                        //                      - "N":
                        //                              - (1.2)Consultar no serv um registro equivalente (ID_CAIXA e CURRENTTIME)
                        //                                      - Tem registro:
                        //                                              - Ver o campo ABERTO:
                        //                                                  - "S":
                        //                                                          - (1.2.1)UPSERT no serv.
                        //                                                  - "N":
                        //                                                          - Faz nada.
                        //                                      - Não tem registro:
                        //                                              - (1.2.1)UPSERT no serv.
                        //      - Não tem registro:
                        //              - Faz nada.
                        #endregion roteiro

                        using (var taOperPdv = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                        {
                            taOperPdv.Connection.ConnectionString = _strConnContingency;

                            #region (1)Consultar o primeiro registro com a maior CURRENTTIME:

                            taOperPdv.FillByIdCaixaLast(tblOperPdv, _intNoCaixa); // já usa sproc

                            if (tblOperPdv != null && tblOperPdv.Rows.Count > 0)
                            {
                                // deu ruim
                                if (tblOperPdv.Rows.Count > 1) { throw new Exception($"Retorno não esperado: taOperPdv.FillByIdCaixaLast({_intNoCaixa}) retornou mais de um registro."); }

                                using (var taOperServ = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                                {
                                    taOperServ.Connection.ConnectionString = _strConnNetwork;

                                    // Ver o campo ABERTO:
                                    if (tblOperPdv[0].ABERTO.Equals("S"))
                                    {
                                        try
                                        {
                                            #region (1.2.1)UPSERT no serv.
                                            taOperServ.SP_TRI_OPER_UPSERT_IDCX_CURT(tblOperPdv[0].ID_CAIXA,
                                                                                    tblOperPdv[0].DIN,
                                                                                    tblOperPdv[0].CHEQUE,
                                                                                    tblOperPdv[0].CREDITO,
                                                                                    tblOperPdv[0].DEBITO,
                                                                                    tblOperPdv[0].LOJA,
                                                                                    tblOperPdv[0].ALIMENTACAO,
                                                                                    tblOperPdv[0].REFEICAO,
                                                                                    tblOperPdv[0].PRESENTE,
                                                                                    tblOperPdv[0].COMBUSTIVEL,
                                                                                    tblOperPdv[0].OUTROS,
                                                                                    tblOperPdv[0].EXTRA_1,
                                                                                    tblOperPdv[0].EXTRA_2,
                                                                                    tblOperPdv[0].EXTRA_3,
                                                                                    tblOperPdv[0].EXTRA_4,
                                                                                    tblOperPdv[0].EXTRA_5,
                                                                                    tblOperPdv[0].EXTRA_6,
                                                                                    tblOperPdv[0].EXTRA_7,
                                                                                    tblOperPdv[0].EXTRA_8,
                                                                                    tblOperPdv[0].EXTRA_9,
                                                                                    tblOperPdv[0].EXTRA_10,
                                                                                    tblOperPdv[0].CURRENTTIME,
                                                                                    tblOperPdv[0].ABERTO,
                                                                                    tblOperPdv[0].HASH,
                                                                                    tblOperPdv[0].SANGRIAS,
                                                                                    tblOperPdv[0].SUPRIMENTOS,
                                                                                    tblOperPdv[0].TROCAS,
                                                                                    (tblOperPdv[0].IsFECHADONull() ? null : (DateTime?)tblOperPdv[0].FECHADO),
                                                                                    //tblOperPdv[0].ID_OPER,
                                                                                    tblOperPdv[0].ID_USER,
                                                                                    DateTime.Now);
                                            #endregion (1.2.1)UPSERT no serv.
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Error("Erro na procedure SP_TRI_OPER_UPSERT_IDCX_CURT (Ver o Campo 'ABERTO' = S) \n", ex);
                                            throw ex;
                                        }
                                    }
                                    else
                                    {
                                        #region (1.2)Consultar no serv um registro equivalente(ID_CAIXA e CURRENTTIME)

                                        taOperServ.FillByIdCaixaCurrentTime(tblOperServ, _intNoCaixa, tblOperPdv[0].CURRENTTIME); // já usa sproc

                                        if (tblOperServ != null && tblOperServ.Rows.Count > 0) // 1.2.19.13 - não era pra ser ||
                                        {
                                            #region E se tiver mais de 1 registro no retorno? AVISA O ARTUR

                                            if (tblOperServ.Rows.Count > 1) { throw new Exception($"Retorno não esperado: taOperServ.FillByIdCaixaCurrentTime({_intNoCaixa}, {tblOperPdv[0].CURRENTTIME}) retornou mais de um registro. \n\nLiga lá na Trilha, fala com o Artur (eu avisei)"); }

                                            #endregion E se tiver mais de 1 registro no retorno? AVISA O ARTUR

                                            #region Tem registro; Ver o campo ABERTO

                                            if (tblOperServ[0].ABERTO.Equals("S"))
                                            {
                                                try
                                                {
                                                    #region (1.2.1)UPSERT no serv.
                                                    taOperServ.SP_TRI_OPER_UPSERT_IDCX_CURT(tblOperPdv[0].ID_CAIXA,
                                                                                            tblOperPdv[0].DIN,
                                                                                            tblOperPdv[0].CHEQUE,
                                                                                            tblOperPdv[0].CREDITO,
                                                                                            tblOperPdv[0].DEBITO,
                                                                                            tblOperPdv[0].LOJA,
                                                                                            tblOperPdv[0].ALIMENTACAO,
                                                                                            tblOperPdv[0].REFEICAO,
                                                                                            tblOperPdv[0].PRESENTE,
                                                                                            tblOperPdv[0].COMBUSTIVEL,
                                                                                            tblOperPdv[0].OUTROS,
                                                                                            tblOperPdv[0].EXTRA_1,
                                                                                            tblOperPdv[0].EXTRA_2,
                                                                                            tblOperPdv[0].EXTRA_3,
                                                                                            tblOperPdv[0].EXTRA_4,
                                                                                            tblOperPdv[0].EXTRA_5,
                                                                                            tblOperPdv[0].EXTRA_6,
                                                                                            tblOperPdv[0].EXTRA_7,
                                                                                            tblOperPdv[0].EXTRA_8,
                                                                                            tblOperPdv[0].EXTRA_9,
                                                                                            tblOperPdv[0].EXTRA_10,
                                                                                            tblOperPdv[0].CURRENTTIME,
                                                                                            tblOperPdv[0].ABERTO,
                                                                                            tblOperPdv[0].HASH,
                                                                                            tblOperPdv[0].SANGRIAS,
                                                                                            tblOperPdv[0].SUPRIMENTOS,
                                                                                            tblOperPdv[0].TROCAS,
                                                                                            (tblOperPdv[0].IsFECHADONull() ? null : (DateTime?)tblOperPdv[0].FECHADO),
                                                                                            //tblOperPdv[0].ID_OPER,
                                                                                            tblOperPdv[0].ID_USER,
                                                                                            DateTime.Now);
                                                    #endregion (1.2.1)UPSERT no serv.
                                                }
                                                catch (Exception ex)
                                                {
                                                    log.Error("Erro na procedure SP_TRI_OPER_UPSERT_IDCX_CURT \n", ex);
                                                    throw ex;
                                                }
                                            }
                                            //else if (tblOperServ[0].ABERTO.Equals("N"))
                                            //{
                                            //    // Faz nada.
                                            //}

                                            #endregion Tem registro; Ver o campo ABERTO
                                        }
                                        else
                                        {
                                            try
                                            {
                                                #region (1.2.1)UPSERT no serv.
                                                taOperServ.SP_TRI_OPER_UPSERT_IDCX_CURT(tblOperPdv[0].ID_CAIXA,
                                                                                        tblOperPdv[0].DIN,
                                                                                        tblOperPdv[0].CHEQUE,
                                                                                        tblOperPdv[0].CREDITO,
                                                                                        tblOperPdv[0].DEBITO,
                                                                                        tblOperPdv[0].LOJA,
                                                                                        tblOperPdv[0].ALIMENTACAO,
                                                                                        tblOperPdv[0].REFEICAO,
                                                                                        tblOperPdv[0].PRESENTE,
                                                                                        tblOperPdv[0].COMBUSTIVEL,
                                                                                        tblOperPdv[0].OUTROS,
                                                                                        tblOperPdv[0].EXTRA_1,
                                                                                        tblOperPdv[0].EXTRA_2,
                                                                                        tblOperPdv[0].EXTRA_3,
                                                                                        tblOperPdv[0].EXTRA_4,
                                                                                        tblOperPdv[0].EXTRA_5,
                                                                                        tblOperPdv[0].EXTRA_6,
                                                                                        tblOperPdv[0].EXTRA_7,
                                                                                        tblOperPdv[0].EXTRA_8,
                                                                                        tblOperPdv[0].EXTRA_9,
                                                                                        tblOperPdv[0].EXTRA_10,
                                                                                        tblOperPdv[0].CURRENTTIME,
                                                                                        tblOperPdv[0].ABERTO,
                                                                                        tblOperPdv[0].HASH,
                                                                                        tblOperPdv[0].SANGRIAS,
                                                                                        tblOperPdv[0].SUPRIMENTOS,
                                                                                        tblOperPdv[0].TROCAS,
                                                                                        (tblOperPdv[0].IsFECHADONull() ? null : (DateTime?)tblOperPdv[0].FECHADO),
                                                                                        //tblOperPdv[0].ID_OPER,
                                                                                        tblOperPdv[0].ID_USER,
                                                                                        DateTime.Now);
                                                #endregion (1.2.1)UPSERT no serv.
                                            }
                                            catch (Exception ex)
                                            {
                                                log.Error("Erro na procedure SP_TRI_OPER_UPSERT_IDCX_CURT \n", ex);
                                                throw ex;
                                            }
                                        }

                                        #endregion (1.2)Consultar no serv um registro equivalente(ID_CAIXA e CURRENTTIME)
                                    }
                                }
                            }

                            #endregion (1)Consultar o primeiro registro com a maior CURRENTTIME:
                        }
                    }
                    catch (Exception ex)
                    {
                        //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                        GravarErroSync("TRI_PDV_OPER(P->S)(PDV)", tblOperPdv, ex);
                        GravarErroSync("TRI_PDV_OPER(P->S)(SERV)", tblOperServ, ex);
                        throw ex;
                    }
                }
            }


        }
        private void Sync_Cupons_NFVVENDA(EnmTipoSync tipoSync)
        {
            try
            {
                if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
                {
                    #region Padrão, unsynced

                    /// SP_TRI_CUPOM_GETALL_UNSYNCED
                    {
                        #region Cria objetos da transação

                        var taNfvendaUnsynced = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter();
                        taNfvendaUnsynced.Connection.ConnectionString = _strConnContingency;

                        var taSatPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_SATTableAdapter();
                        taSatPdv.Connection.ConnectionString = _strConnContingency;

                        var taSatCancPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_SAT_CANCTableAdapter();
                        taSatCancPdv.Connection.ConnectionString = _strConnContingency;

                        var taNfvendaFmaPagtoNfcePdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDA_FMAPAGTO_NFCETableAdapter();
                        taNfvendaFmaPagtoNfcePdv.Connection.ConnectionString = _strConnContingency;

                        var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter();
                        taEstProdutoPdv.Connection.ConnectionString = _strConnContingency;

                        var taEstProdutoServ = new TB_EST_PRODUTOTableAdapter();
                        taEstProdutoServ.Connection.ConnectionString = _strConnNetwork;

                        var taNfvItemPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEMTableAdapter();
                        taNfvItemPdv.Connection.ConnectionString = _strConnContingency;

                        var tblNfvendaUnsynced = new FDBDataSetVenda.TB_NFVENDADataTable();

                        var tblNfvendaFmapagtoNfcePdv = new FDBDataSetVenda.TB_NFVENDA_FMAPAGTO_NFCEDataTable();

                        var tblSatPdv = new FDBDataSetVenda.TB_SATDataTable();

                        var tblSatCancPdv = new FDBDataSetVenda.TB_SAT_CANCDataTable();

                        var tblCtaRecPdv = new FDBDataSet.TB_CONTA_RECEBERDataTable();

                        var tblMovDiarioPdv = new FDBDataSet.TB_MOVDIARIODataTable();

                        var tblNfvItemPdv = new FDBDataSetVenda.TB_NFV_ITEMDataTable();

                        var tblNfvItemCofinsPdv = new FDBDataSetVenda.TB_NFV_ITEM_COFINSDataTable();

                        var tblNfvItemPisPdv = new FDBDataSetVenda.TB_NFV_ITEM_PISDataTable();

                        var tblNfvItemIcmsPdv = new FDBDataSetVenda.TB_NFV_ITEM_ICMSDataTable();

                        var tblNfvItemStPdv = new FDBDataSetVenda.TB_NFV_ITEM_STDataTable();

                        #endregion Cria objetos da transação

                        List<AuxNfvFmaPgtoCtaRec> lstAuxNfvFmaPgtoCtaRec = new List<AuxNfvFmaPgtoCtaRec>();

                        try
                        {
                            #region Prepara o lote inicial para sincronização

                            // Busca todos os cupons que foram finalizados mas não sincronizados (TIP_QUERY = 0):
                            taNfvendaUnsynced.FillByNfvendaSync(tblNfvendaUnsynced, 0); // já usa sproc
                                                                                        // Até o momento (23/02/2018), a quantidade de registros por lote 
                                                                                        // fica definido na própria consulta de cupons (SP_TRI_CUPOM_GETALL_UNSYNCED).
                                                                                        // O ideal seria que isso fosse parametrizado.

                            // Indica quantos lotes de cupons foram processados:
                            int contLote = 0;

                            #endregion Prepara o lote inicial para sincronização

                            #region Procedimento executado enquanto houver cupons para sincronizar

                            if (tblNfvendaUnsynced != null && tblNfvendaUnsynced.Rows.Count > 0)
                            {
                                #region NOPE - CLIPP RULES NO MORE
                                //taEstProdutoPdv.SP_TRI_BREAK_CLIPP_RULES();
                                //taEstProdutoServ.SP_TRI_BREAK_CLIPP_RULES();
                                #endregion NOPE - CLIPP RULES NO MORE

                                //TODO: ver uma saída pro loop infinito caso estourar exceção no sync
                                // Detalhe: a 2ª condição poderia estourar uma exception se a 1ª fosse verdadeira e o operador 
                                // fosse OR (||). Mas com o operador AND (&&), a 2ª condição nem é verificada se a 1ª for verdadeira.
                                while (!(tblNfvendaUnsynced is null) && tblNfvendaUnsynced.Rows.Count > 0)
                                {
                                    contLote++;

                                    #region Sincroniza (manda para a retaguarda)

                                    // Percorre pelos cupons do banco local
                                    foreach (FDBDataSetVenda.TB_NFVENDARow nfvenda in tblNfvendaUnsynced.Rows)
                                    {
                                        int newIdNfvenda = 0;

                                        #region Gravar a nfvenda na retaguarda (transação)

                                        #region Validações

                                        //// Foi necessário adaptar o COO como o ID_CUPOM negativo para sistema legado
                                        //// Será que tem um equivalente para a NFVENDA?
                                        //if (cupom.IsCOONull()) { cupom.COO = cupom.ID_CUPOM * -1; }
                                        //if (cupom.IsNUM_CAIXANull()) { cupom.NUM_CAIXA = 0; }

                                        #endregion Validações


                                        //TransactionOptions to = new TransactionOptions();
                                        //to.IsolationLevel = System.Transactions.IsolationLevel.Serializable;

                                        // Inicia a transação:
                                        //using (var transactionScopeCupons = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(1, 0, 0, 0)))
                                        // Define a conexão com o banco do servidor:
                                        // Define a conexão com o banco do PDV:
                                        using (var fbConnServ = new FbConnection(_strConnNetwork))
                                        using (var fbConnPdv = new FbConnection(_strConnContingency))
                                        //using (var transactionScopeCupons = new TransactionScope(TransactionScopeOption.Required,
                                        //                                                         new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }
                                        //                                                         )) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                                        //using (var transactionScopeCupons = new TransactionScope())
                                        {
                                            // A função BeginTransaction() precisa de uma connection aberta... ¬¬
                                            fbConnServ.Open();
                                            fbConnPdv.Open();

                                            using (var fbTransactServ = fbConnServ.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Wait, WaitTimeout = new TimeSpan(0, 0, _SyncTimeout) }))
                                            using (var fbTransactPdv = fbConnPdv.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Wait, WaitTimeout = new TimeSpan(0, 0, _SyncTimeout) }))
                                            {
                                                try
                                                {
                                                    //int newIdNfvenda = 0;
                                                    //int newIdMaitPedidoServ = 0;
                                                    lstAuxNfvFmaPgtoCtaRec = null;
                                                    lstAuxNfvFmaPgtoCtaRec = new List<AuxNfvFmaPgtoCtaRec>();

                                                    #region Gravar a nfvenda no servidor (capa)

                                                    using (var fbCommNfvendaSyncInsertServ = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_NFVENDA_SYNC_INSERT

                                                        fbCommNfvendaSyncInsertServ.Connection = fbConnServ;
                                                        //fbCommCupomSyncInsertServ.Connection.ConnectionString = _strConnNetwork;

                                                        fbCommNfvendaSyncInsertServ.CommandText = "SP_TRI_NFVENDA_SYNC_INSERT";
                                                        fbCommNfvendaSyncInsertServ.CommandType = CommandType.StoredProcedure;
                                                        fbCommNfvendaSyncInsertServ.Transaction = fbTransactServ;

                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_NATOPE", nfvenda.ID_NATOPE);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_VENDEDOR", nfvenda.IsID_VENDEDORNull() ? null : (short?)nfvenda.ID_VENDEDOR);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_CLIENTE", nfvenda.ID_CLIENTE);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pNF_NUMERO", nfvenda.NF_NUMERO);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pNF_SERIE", nfvenda.NF_SERIE);

                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pNF_MODELO", nfvenda.NF_MODELO);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pDT_EMISSAO", nfvenda.DT_EMISSAO);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pDT_SAIDA", nfvenda.IsDT_SAIDANull() ? null : (DateTime?)nfvenda.DT_SAIDA);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pHR_SAIDA", nfvenda.IsHR_SAIDANull() ? null : (TimeSpan?)nfvenda.HR_SAIDA);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pESPECIE", nfvenda.IsESPECIENull() ? null : nfvenda.ESPECIE);

                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pTIPO_FRETE", nfvenda.TIPO_FRETE);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pPES_LIQUID", nfvenda.IsPES_LIQUIDNull() ? null : (decimal?)nfvenda.PES_LIQUID);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pPES_BRUTO", nfvenda.IsPES_BRUTONull() ? null : (decimal?)nfvenda.PES_BRUTO);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pSTATUS", nfvenda.STATUS);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pENT_SAI", nfvenda.ENT_SAI);

                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_FMAPGTO", nfvenda.ID_FMAPGTO);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_PARCELA", nfvenda.ID_PARCELA);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pMARCA", nfvenda.IsMARCANull() ? null : nfvenda.MARCA);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pQTD_VOLUM", nfvenda.IsQTD_VOLUMNull() ? null : (decimal?)nfvenda.QTD_VOLUM);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pNUM_VOLUM", nfvenda.IsNUM_VOLUMNull() ? null : nfvenda.NUM_VOLUM);

                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pPROD_REV", nfvenda.IsPROD_REVNull() ? null : nfvenda.PROD_REV);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pSOMA_FRETE", nfvenda.IsSOMA_FRETENull() ? null : nfvenda.SOMA_FRETE);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pVLR_TROCO", nfvenda.IsVLR_TROCONull() ? null : (decimal?)nfvenda.VLR_TROCO);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pIND_PRES", nfvenda.IsIND_PRESNull() ? null : nfvenda.IND_PRES);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pIND_IE_DEST", nfvenda.IsIND_IE_DESTNull() ? null : nfvenda.IND_IE_DEST);

                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pDESCONTO_CONDICIONAL", nfvenda.DESCONTO_CONDICIONAL);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pINF_COMP_FIXA", nfvenda.IsINF_COMP_FIXANull() ? null : nfvenda.INF_COMP_FIXA);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pINF_COMP_EDIT", nfvenda.IsINF_COMP_EDITNull() ? null : nfvenda.INF_COMP_EDIT);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pENDERECO_ENTREGA", nfvenda.ENDERECO_ENTREGA);
                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pENVIO_API", nfvenda.IsENVIO_APINull() ? null : (DateTime?)nfvenda.ENVIO_API);

                                                        fbCommNfvendaSyncInsertServ.Parameters.Add("@pSYNCED", 1);

                                                        #endregion Prepara o comando da SP_TRI_NFVENDA_SYNC_INSERT

                                                        // Executa a sproc
                                                        newIdNfvenda = (int)fbCommNfvendaSyncInsertServ.ExecuteScalar();
                                                    }

                                                    #endregion Gravar a nfvenda no servidor (capa)

                                                    #region Gravar TB_SAT, se houver

                                                    tblSatPdv.Clear();
                                                    //NOME_DA_PROCEDURE_AQUI
                                                    taSatPdv.FillByIdNfvenda(tblSatPdv, nfvenda.ID_NFVENDA);

                                                    foreach (FDBDataSetVenda.TB_SATRow satPdv in tblSatPdv)
                                                    {
                                                        int newIdRegistro = 0;

                                                        using (var fbCommSatSyncInsert = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_SAT_SYNC_INSERT

                                                            fbCommSatSyncInsert.Connection = fbConnServ;
                                                            //fbCommCupomFmapagtoSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommSatSyncInsert.Transaction = fbTransactServ;

                                                            fbCommSatSyncInsert.CommandText = "SP_TRI_SAT_SYNC_INSERT";
                                                            fbCommSatSyncInsert.CommandType = CommandType.StoredProcedure;

                                                            fbCommSatSyncInsert.Parameters.Add("@pID_NFVENDA", newIdNfvenda);
                                                            fbCommSatSyncInsert.Parameters.Add("@pCHAVE", satPdv.IsCHAVENull() ? null : satPdv.CHAVE);
                                                            fbCommSatSyncInsert.Parameters.Add("@pDT_EMISSAO", satPdv.IsDT_EMISSAONull() ? null : (DateTime?)satPdv.DT_EMISSAO);
                                                            fbCommSatSyncInsert.Parameters.Add("@pHR_EMISSAO", satPdv.IsHR_EMISSAONull() ? null : (TimeSpan?)satPdv.HR_EMISSAO);

                                                            fbCommSatSyncInsert.Parameters.Add("@pSTATUS", satPdv.IsSTATUSNull() ? null : satPdv.STATUS);
                                                            fbCommSatSyncInsert.Parameters.Add("@pSTATUS_DES", satPdv.IsSTATUS_DESNull() ? null : satPdv.STATUS_DES);
                                                            fbCommSatSyncInsert.Parameters.Add("@pNUMERO_CFE", satPdv.IsNUMERO_CFENull() ? null : (int?)satPdv.NUMERO_CFE);
                                                            fbCommSatSyncInsert.Parameters.Add("@pNUM_SERIE_SAT", satPdv.IsNUM_SERIE_SATNull() ? null : satPdv.NUM_SERIE_SAT);

                                                            #endregion Prepara o comando da SP_TRI_SAT_SYNC_INSERT

                                                            // Executa a sproc
                                                            newIdRegistro = (int)fbCommSatSyncInsert.ExecuteScalar();
                                                        }

                                                        #region Gravar TB_SAT_CANC, se houver

                                                        tblSatCancPdv.Clear();
                                                        //NOME_DA_PROCEDURE_AQUI
                                                        taSatCancPdv.FillByIdRegistro(tblSatCancPdv, satPdv.ID_REGISTRO);

                                                        foreach (FDBDataSetVenda.TB_SAT_CANCRow satCancPdv in tblSatCancPdv)
                                                        {
                                                            //int newIdCancela = 0;

                                                            using (var fbCommSatCancSyncInsert = new FbCommand())
                                                            {
                                                                #region Prepara o comando da SP_TRI_SAT_CANC_SYNC_INSERT

                                                                fbCommSatCancSyncInsert.Connection = fbConnServ;
                                                                //fbCommCupomFmapagtoSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                                fbCommSatCancSyncInsert.Transaction = fbTransactServ;

                                                                fbCommSatCancSyncInsert.CommandText = "SP_TRI_SAT_CANC_SYNC_INSERT";
                                                                fbCommSatCancSyncInsert.CommandType = CommandType.StoredProcedure;

                                                                fbCommSatCancSyncInsert.Parameters.Add("@pID_REGISTRO", newIdRegistro);
                                                                fbCommSatCancSyncInsert.Parameters.Add("@pDT_EMISSAO", satCancPdv.IsDT_EMISSAONull() ? null : (DateTime?)satCancPdv.DT_EMISSAO);
                                                                fbCommSatCancSyncInsert.Parameters.Add("@pHR_EMISSAO", satCancPdv.IsHR_EMISSAONull() ? null : (TimeSpan?)satCancPdv.HR_EMISSAO);
                                                                fbCommSatCancSyncInsert.Parameters.Add("@pNUMERO_CFE", satCancPdv.IsNUMERO_CFENull() ? null : (int?)satCancPdv.NUMERO_CFE);
                                                                fbCommSatCancSyncInsert.Parameters.Add("@pCHAVE", satCancPdv.IsCHAVENull() ? null : satCancPdv.CHAVE);
                                                                fbCommSatCancSyncInsert.Parameters.Add("@pNUM_SERIE_SAT", satCancPdv.IsNUM_SERIE_SATNull() ? null : satCancPdv.NUM_SERIE_SAT);
                                                                fbCommSatCancSyncInsert.Parameters.Add("@pENVIO_API", satCancPdv.IsENVIO_APINull() ? null : (DateTime?)satCancPdv.ENVIO_API);

                                                                #endregion Prepara o comando da SP_TRI_SAT_CANC_SYNC_INSERT

                                                                // Executa a sproc
                                                                //newIdCancela = (int)
                                                                fbCommSatCancSyncInsert.ExecuteScalar();
                                                            }

                                                            #region Gravar TB_SAT_CANC, se houver

                                                            //TODO: gravar TB_SAT_CANC também

                                                            #endregion Gravar TB_SAT_CANC, se houver
                                                        }

                                                        #endregion Gravar TB_SAT_CANC, se houver
                                                    }

                                                    #endregion Gravar TB_SAT, se houver

                                                    #region Buscar as formas de pagamento da nfvenda no PDV

                                                    tblNfvendaFmapagtoNfcePdv.Clear();
                                                    //TB_NFVENDA_FMAPAGTO_NFCE();
                                                    taNfvendaFmaPagtoNfcePdv.FillByIdNfvenda(tblNfvendaFmapagtoNfcePdv, nfvenda.ID_NFVENDA);

                                                    #endregion Buscar as formas de pagamento da nfvenda no PDV

                                                    #region Gravar as formas de pagamento da nfvenda na retaguarda

                                                    foreach (FDBDataSetVenda.TB_NFVENDA_FMAPAGTO_NFCERow nfvendaFmapagtoNfcePdv in tblNfvendaFmapagtoNfcePdv)
                                                    {                                                        
                                                        int newIdnumpag = 0;

                                                        //Problema encontrado, TB_NFC_BANDEIRA não tem a coluna 'ID_NFVENDA', sendo assim só é possivel usar como eixo de pesquisa a coluna 'ID_NUMPAG'
                                                        //o que acaba fugindo um pouco da lógica do que acontece aqui, então vamos lá capturar o 'ID_NUMPAG' e ver qual administradora foi ultilizada.
                                                        int idNumPag = nfvendaFmapagtoNfcePdv.ID_NUMPAG;
                                                        var tbNfceBandeira = new DataSets.FDBDataSetVendaTableAdapters.TB_NFCE_BANDEIRATableAdapter(); tbNfceBandeira.Connection.ConnectionString = _strConnContingency;
                                                        var idAdmins = tbNfceBandeira.PegaIDAdmin(idNumPag); 

                                                        using (var fbCommNfvendaFmapagtoNfceSyncInsert = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_NFV_FMAPAGT_SYNC_INSERT                                                                                                                        
                                                            fbCommNfvendaFmapagtoNfceSyncInsert.Connection = fbConnServ;
                                                            //fbCommCupomFmapagtoSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommNfvendaFmapagtoNfceSyncInsert.Transaction = fbTransactServ;

                                                            fbCommNfvendaFmapagtoNfceSyncInsert.CommandText = "SP_TRI_NFV_FMAPAGT_SYNC_INSERT"; //Agora essa procedure preenche tanto a TB_NFVENDA_FMAPAGTO_NFCE como a TB_NFCE_BANDEIRA, sincronização OK.
                                                            fbCommNfvendaFmapagtoNfceSyncInsert.CommandType = CommandType.StoredProcedure;

                                                            fbCommNfvendaFmapagtoNfceSyncInsert.Parameters.Add("@pVLR_PAGTO", nfvendaFmapagtoNfcePdv.VLR_PAGTO);
                                                            fbCommNfvendaFmapagtoNfceSyncInsert.Parameters.Add("@pID_NFVENDA", newIdNfvenda);
                                                            fbCommNfvendaFmapagtoNfceSyncInsert.Parameters.Add("@pID_FMANFCE", nfvendaFmapagtoNfcePdv.ID_FMANFCE);
                                                            fbCommNfvendaFmapagtoNfceSyncInsert.Parameters.Add("@pID_PARCELA", nfvendaFmapagtoNfcePdv.ID_PARCELA);
                                                            fbCommNfvendaFmapagtoNfceSyncInsert.Parameters.Add("@pID_ADMINISTRADORA", idAdmins);

                                                            #endregion Prepara o comando da SP_TRI_NFV_FMAPAGT_SYNC_INSERT

                                                            // Executa a sproc
                                                            newIdnumpag = (int)fbCommNfvendaFmapagtoNfceSyncInsert.ExecuteScalar();
                                                        }

                                                        //TODO: o que fazer com newIdnumpag?
                                                        // montar uma relação com o ID original pra gravar depois na relação forma de pagamento / conta a receber.
                                                        AuxNfvFmaPgtoCtaRec itemAux = new AuxNfvFmaPgtoCtaRec
                                                        {
                                                            //PdvIdNfvenda = nfvenda.ID_NFVENDA,
                                                            PdvIdNumPag = nfvendaFmapagtoNfcePdv.ID_NUMPAG,
                                                            //ServIdNfvenda = newIdNfvenda,
                                                            ServIdNumPag = newIdnumpag
                                                        };
                                                        lstAuxNfvFmaPgtoCtaRec.Add(itemAux);
                                                    }

                                                    #endregion Gravar as formas de pagamento da nfvenda na retaguarda

                                                    #region Pedido da nfvenda (AmbiMAITRE)

                                                    //TODO: não há (por enquanto?)

                                                    #endregion Pedido da nfvenda (AmbiMAITRE)

                                                    #region Itens de nfvenda do PDV

                                                    #region Consultar os itens da nfvenda do PDV

                                                    tblNfvItemPdv.Clear();
                                                    // Busca os itens do cupom pelo ID_CUPOM local (PDV):
                                                    //audit("SINCCONTNETDB>> " + "taCupomItemPdv.FillByIdCupom(): " + taCupomItemPdv.FillByIdCupom(tblCupomItemPdv, cupom.ID_CUPOM).ToString());
                                                    // SP_TRI_CUPOMITEMGET
                                                    taNfvItemPdv.FillByIdNfvenda(tblNfvItemPdv, nfvenda.ID_NFVENDA); // já usa sproc: SP_TRI_CUPOMITEMGET

                                                    foreach (FDBDataSetVenda.TB_NFV_ITEMRow nfvItem in tblNfvItemPdv.Rows)
                                                    {
                                                        // Os itens do cupom devem referenciar o novo ID do cupom da retaguarda

                                                        #region Gravar os itens da nfvenda

                                                        int newIdNfvItem = 0;

                                                        using (var fbCommNfvItemSyncInsert = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_NFVITEM_SYNC_INSERT

                                                            fbCommNfvItemSyncInsert.Connection = fbConnServ;
                                                            //fbCommNfvItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommNfvItemSyncInsert.Transaction = fbTransactServ;

                                                            fbCommNfvItemSyncInsert.CommandText = "SP_TRI_NFVITEM_SYNC_INSERT";
                                                            fbCommNfvItemSyncInsert.CommandType = CommandType.StoredProcedure;

                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pID_NFVENDA", newIdNfvenda);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pID_IDENTIFICADOR", nfvItem.ID_IDENTIFICADOR);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pCFOP", nfvItem.CFOP);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pNUM_ITEM", nfvItem.NUM_ITEM);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pQTD_ITEM", nfvItem.QTD_ITEM);

                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pUNI_MEDIDA", nfvItem.IsUNI_MEDIDANull() ? null : nfvItem.UNI_MEDIDA);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TOTAL", nfvItem.IsVLR_TOTALNull() ? null : (decimal?)nfvItem.VLR_TOTAL);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_DESC", nfvItem.IsVLR_DESCNull() ? null : (decimal?)nfvItem.VLR_DESC);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_CUSTO", nfvItem.IsVLR_CUSTONull() ? null : (decimal?)nfvItem.VLR_CUSTO);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pPRC_LISTA", nfvItem.IsPRC_LISTANull() ? null : (decimal?)nfvItem.PRC_LISTA);

                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pCF", nfvItem.IsCFNull() ? null : nfvItem.CF);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_FRETE", nfvItem.VLR_FRETE);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_SEGURO", nfvItem.IsVLR_SEGURONull() ? null : (decimal?)nfvItem.VLR_SEGURO);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_DESPESA", nfvItem.IsVLR_DESPESANull() ? null : (decimal?)nfvItem.VLR_DESPESA);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pRET_PIS_COF_CSLL", nfvItem.IsRET_PIS_COF_CSLLNull() ? null : (decimal?)nfvItem.RET_PIS_COF_CSLL);

                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pRET_IRRF", nfvItem.IsRET_IRRFNull() ? null : (decimal?)nfvItem.RET_IRRF);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pCOD_ENQ", nfvItem.IsCOD_ENQNull() ? null : nfvItem.COD_ENQ);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pCOD_BASE", nfvItem.IsCOD_BASENull() ? null : nfvItem.COD_BASE);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pCSOSN", nfvItem.IsCSOSNNull() ? null : nfvItem.CSOSN);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pNPED_COMPRA", nfvItem.IsNPED_COMPRANull() ? null : nfvItem.NPED_COMPRA);

                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pITEM_COMPRA", nfvItem.IsITEM_COMPRANull() ? null : (int?)nfvItem.ITEM_COMPRA);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TOTTRIB", nfvItem.IsVLR_TOTTRIBNull() ? null : (decimal?)nfvItem.VLR_TOTTRIB);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pFCI", nfvItem.IsFCINull() ? null : nfvItem.FCI);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_ICM_DESO", nfvItem.IsVLR_ICM_DESONull() ? null : (decimal?)nfvItem.VLR_ICM_DESO);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pID_MOTIVO_DESO", nfvItem.IsID_MOTIVO_DESONull() ? null : (int?)nfvItem.ID_MOTIVO_DESO);

                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pEST_BX", nfvItem.IsEST_BXNull() ? null : nfvItem.EST_BX);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TRIB_FED", nfvItem.IsVLR_TRIB_FEDNull() ? null : (decimal?)nfvItem.VLR_TRIB_FED);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TRIB_EST", nfvItem.IsVLR_TRIB_ESTNull() ? null : (decimal?)nfvItem.VLR_TRIB_EST);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TRIB_MUN", nfvItem.IsVLR_TRIB_MUNNull() ? null : (decimal?)nfvItem.VLR_TRIB_MUN);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pINCLUIR_FATURA", nfvItem.INCLUIR_FATURA);

                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_UNIT", nfvItem.VLR_UNIT);
                                                            //fbCommNfvItemSyncInsert.Parameters.Add("@pIMP_MANUAL", nfvItem.IsIMP_MANUALNull() ? null : nfvItem.IMP_MANUAL);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_RETENCAO", nfvItem.VLR_RETENCAO);
                                                            fbCommNfvItemSyncInsert.Parameters.Add("@pREFERENCIA", nfvItem.IsREFERENCIANull() ? null : nfvItem.REFERENCIA);
                                                            //fbCommNfvItemSyncInsert.Parameters.Add("@pCODPROMOSCANNTECH", nfvItem.IsCODPROMOSCANNTECHNull() ? null : (int?)nfvItem.CODPROMOSCANNTECH);


                                                            #endregion Prepara o comando da SP_TRI_NFVITEM_SYNC_INSERT

                                                            try
                                                            {
                                                                // Executa a sproc
                                                                newIdNfvItem = (int)fbCommNfvItemSyncInsert.ExecuteScalar();
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                log.Error($"Erro ao sincronizar item de nfvenda. \npID_NFVENDA: {newIdNfvenda} \nID_IDENTIFICADOR: {nfvItem.ID_IDENTIFICADOR}", ex);
                                                                throw ex;
                                                            }

                                                            //audit("SINCCONTNETDB>> ", "SP_TRI_NFVITEM_SYNC_INSERT(): " + newIdCupomItem.ToString());
                                                        }

                                                        #endregion Gravar os itens da nfvenda

                                                        #region Gravar TB_NFV_ITEM_COFINS

                                                        #region Busca TB_NFV_ITEM_COFINS (PDV)

                                                        tblNfvItemCofinsPdv.Clear();

                                                        using (var taNfvItemCofinsPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_COFINSTableAdapter())
                                                        {
                                                            taNfvItemCofinsPdv.Connection.ConnectionString = _strConnContingency;
                                                            // TRI_MAIT_PEDIDO_ITEM
                                                            taNfvItemCofinsPdv.FillById(tblNfvItemCofinsPdv, nfvItem.ID_NFVITEM); // já usa sproc
                                                        }

                                                        #endregion Busca TB_NFV_ITEM_COFINS (PDV)

                                                        #region Procedimento de gravação de TB_NFV_ITEM_COFINS (servidor)

                                                        foreach (var nfvItemCofinsPdv in tblNfvItemCofinsPdv) // Deve ter apenas 1 item de pedido por item de cupom
                                                        {
                                                            #region Gravar TB_NFV_ITEM_COFINS (serv)

                                                            //int newIdMaitPedItemServ = 0;

                                                            using (var fbCommNfvItemCofinsSyncInsert = new FbCommand())
                                                            {
                                                                #region Prepara o comando da SP_TRI_NFVITEMCOFINS_SYNCINSERT

                                                                fbCommNfvItemCofinsSyncInsert.Connection = fbConnServ;
                                                                //fbCommMaitPedItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                                fbCommNfvItemCofinsSyncInsert.Transaction = fbTransactServ;

                                                                fbCommNfvItemCofinsSyncInsert.CommandText = "SP_TRI_NFVITEMCOFINS_SYNCINSERT";
                                                                fbCommNfvItemCofinsSyncInsert.CommandType = CommandType.StoredProcedure;

                                                                fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pID_NFVITEM", newIdNfvItem);
                                                                fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pPOR_BC_COFINS", nfvItemCofinsPdv.IsPOR_BC_COFINSNull() ? null : (decimal?)nfvItemCofinsPdv.POR_BC_COFINS);
                                                                fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pCST_COFINS", nfvItemCofinsPdv.IsCST_COFINSNull() ? null : nfvItemCofinsPdv.CST_COFINS);
                                                                fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pALIQ_COFINS", nfvItemCofinsPdv.IsALIQ_COFINSNull() ? null : (decimal?)nfvItemCofinsPdv.ALIQ_COFINS);
                                                                fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pVLR_COFINS", nfvItemCofinsPdv.IsVLR_COFINSNull() ? null : (decimal?)nfvItemCofinsPdv.VLR_COFINS);
                                                                fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pVLR_BC_COFINS", nfvItemCofinsPdv.IsVLR_BC_COFINSNull() ? null : (decimal?)nfvItemCofinsPdv.VLR_BC_COFINS);

                                                                #endregion Prepara o comando da SP_TRI_NFVITEMCOFINS_SYNCINSERT

                                                                //newIdMaitPedItemServ = (int)
                                                                fbCommNfvItemCofinsSyncInsert.ExecuteScalar();

                                                                //// Executa a sproc
                                                                //audit("SINCCONTNETDB >> ", string.Format("SP_TRI_NFVITEMCOFINS_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_IDENTIFICADOR: {1}, pQTD_ITEM: {2}): {3}",
                                                                //                    newIdMaitPedidoServ,
                                                                //                    pedItemPdv.ID_IDENTIFICADOR,
                                                                //                    pedItemPdv.QTD_ITEM,
                                                                //                    newIdMaitPedItemServ));
                                                            }

                                                            #endregion Gravar TB_NFV_ITEM_COFINS (serv)
                                                        }

                                                        #endregion Procedimento de gravação de TB_NFV_ITEM_COFINS (servidor)

                                                        #endregion Gravar TB_NFV_ITEM_COFINS

                                                        #region Gravar TB_NFV_ITEM_PIS

                                                        #region Busca TB_NFV_ITEM_PIS (PDV)

                                                        tblNfvItemPisPdv.Clear();

                                                        using (var taNfvItemPisPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_PISTableAdapter())
                                                        {
                                                            taNfvItemPisPdv.Connection.ConnectionString = _strConnContingency;
                                                            // TRI_MAIT_PEDIDO_ITEM
                                                            taNfvItemPisPdv.FillById(tblNfvItemPisPdv, nfvItem.ID_NFVITEM); // já usa sproc
                                                        }

                                                        #endregion Busca TB_NFV_ITEM_PIS (PDV)

                                                        #region Procedimento de gravação de TB_NFV_ITEM_PIS (servidor)

                                                        foreach (var nfvItemPisPdv in tblNfvItemPisPdv)
                                                        {
                                                            #region Gravar TB_NFV_ITEM_PIS (serv)

                                                            using (var fbCommNfvItemPisSyncInsert = new FbCommand())
                                                            {
                                                                #region Prepara o comando da SP_TRI_NFVITEMPIS_SYNCINSERT

                                                                fbCommNfvItemPisSyncInsert.Connection = fbConnServ;
                                                                //fbCommMaitPedItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                                fbCommNfvItemPisSyncInsert.Transaction = fbTransactServ;

                                                                fbCommNfvItemPisSyncInsert.CommandText = "SP_TRI_NFVITEMPIS_SYNCINSERT";
                                                                fbCommNfvItemPisSyncInsert.CommandType = CommandType.StoredProcedure;

                                                                fbCommNfvItemPisSyncInsert.Parameters.Add("@pID_NFVITEM", newIdNfvItem);
                                                                fbCommNfvItemPisSyncInsert.Parameters.Add("@pPOR_BC_PIS", nfvItemPisPdv.IsPOR_BC_PISNull() ? null : (decimal?)nfvItemPisPdv.POR_BC_PIS);
                                                                fbCommNfvItemPisSyncInsert.Parameters.Add("@pCST_PIS", nfvItemPisPdv.IsCST_PISNull() ? null : nfvItemPisPdv.CST_PIS);
                                                                fbCommNfvItemPisSyncInsert.Parameters.Add("@pALIQ_PIS", nfvItemPisPdv.IsALIQ_PISNull() ? null : (decimal?)nfvItemPisPdv.ALIQ_PIS);
                                                                fbCommNfvItemPisSyncInsert.Parameters.Add("@pVLR_PIS", nfvItemPisPdv.IsVLR_PISNull() ? null : (decimal?)nfvItemPisPdv.VLR_PIS);
                                                                fbCommNfvItemPisSyncInsert.Parameters.Add("@pVLR_BC_PIS", nfvItemPisPdv.IsVLR_BC_PISNull() ? null : (decimal?)nfvItemPisPdv.VLR_BC_PIS);

                                                                #endregion Prepara o comando da SP_TRI_NFVITEMPIS_SYNCINSERT

                                                                //newIdMaitPedItemServ = (int)
                                                                fbCommNfvItemPisSyncInsert.ExecuteScalar();

                                                                //// Executa a sproc
                                                                //audit("SINCCONTNETDB >> ", string.Format("SP_TRI_NFVITEMPIS_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_IDENTIFICADOR: {1}, pQTD_ITEM: {2}): {3}",
                                                                //                    newIdMaitPedidoServ,
                                                                //                    pedItemPdv.ID_IDENTIFICADOR,
                                                                //                    pedItemPdv.QTD_ITEM,
                                                                //                    newIdMaitPedItemServ));
                                                            }

                                                            #endregion Gravar TB_NFV_ITEM_PIS (serv)
                                                        }

                                                        #endregion Procedimento de gravação de TB_NFV_ITEM_PIS (servidor)

                                                        #endregion Gravar TB_NFV_ITEM_PIS

                                                        #region Gravar TB_NFV_ITEM_ICMS

                                                        #region Busca TB_NFV_ITEM_ICMS (PDV)

                                                        tblNfvItemIcmsPdv.Clear();

                                                        using (var taNfvItemIcmsPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_ICMSTableAdapter())
                                                        {
                                                            taNfvItemIcmsPdv.Connection.ConnectionString = _strConnContingency;
                                                            // TRI_MAIT_PEDIDO_ITEM
                                                            taNfvItemIcmsPdv.FillById(tblNfvItemIcmsPdv, nfvItem.ID_NFVITEM); // já usa sproc
                                                        }

                                                        #endregion Busca TB_NFV_ITEM_ICMS (PDV)

                                                        #region Procedimento de gravação de TB_NFV_ITEM_ICMS (servidor)

                                                        foreach (var nfvItemIcmsPdv in tblNfvItemIcmsPdv)
                                                        {
                                                            #region Gravar TB_NFV_ITEM_ICMS (serv)

                                                            using (var fbCommNfvItemIcmsSyncInsert = new FbCommand())
                                                            {
                                                                #region Prepara o comando da SP_TRI_NFVITEMICMS_SYNCINSERT

                                                                fbCommNfvItemIcmsSyncInsert.Connection = fbConnServ;
                                                                //fbCommMaitPedItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                                fbCommNfvItemIcmsSyncInsert.Transaction = fbTransactServ;

                                                                fbCommNfvItemIcmsSyncInsert.CommandText = "SP_TRI_NFVITEMICMS_SYNCINSERT";
                                                                fbCommNfvItemIcmsSyncInsert.CommandType = CommandType.StoredProcedure;

                                                                fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pID_NFVITEM", newIdNfvItem);
                                                                fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pVLR_BC_ICMS", nfvItemIcmsPdv.IsVLR_BC_ICMSNull() ? null : (decimal?)nfvItemIcmsPdv.VLR_BC_ICMS);
                                                                fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pPOR_BC_ICMS", nfvItemIcmsPdv.IsPOR_BC_ICMSNull() ? null : (decimal?)nfvItemIcmsPdv.POR_BC_ICMS);
                                                                fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pCST_ICMS", nfvItemIcmsPdv.IsCST_ICMSNull() ? null : nfvItemIcmsPdv.CST_ICMS);
                                                                fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pALIQ_ICMS", nfvItemIcmsPdv.IsALIQ_ICMSNull() ? null : (decimal?)nfvItemIcmsPdv.ALIQ_ICMS);
                                                                fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pVLR_ICMS", nfvItemIcmsPdv.IsVLR_ICMSNull() ? null : (decimal?)nfvItemIcmsPdv.VLR_ICMS);

                                                                #endregion Prepara o comando da SP_TRI_NFVITEMICMS_SYNCINSERT

                                                                //newIdMaitPedItemServ = (int)
                                                                fbCommNfvItemIcmsSyncInsert.ExecuteScalar();

                                                                //// Executa a sproc
                                                                //audit("SINCCONTNETDB >> ", string.Format("SP_TRI_NFVITEMICMS_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_IDENTIFICADOR: {1}, pQTD_ITEM: {2}): {3}",
                                                                //                    newIdMaitPedidoServ,
                                                                //                    pedItemPdv.ID_IDENTIFICADOR,
                                                                //                    pedItemPdv.QTD_ITEM,
                                                                //                    newIdMaitPedItemServ));
                                                            }

                                                            #endregion Gravar TB_NFV_ITEM_ICMS (serv)
                                                        }

                                                        #endregion Procedimento de gravação de TB_NFV_ITEM_ICMS (servidor)

                                                        #endregion Gravar TB_NFV_ITEM_ICMS

                                                        #region Gravar TB_NFV_ITEM_ST

                                                        #region Busca TB_NFV_ITEM_ST (PDV)

                                                        tblNfvItemStPdv.Clear();

                                                        using (var taNfvItemStPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_STTableAdapter())
                                                        {
                                                            taNfvItemStPdv.Connection.ConnectionString = _strConnContingency;
                                                            // TRI_MAIT_PEDIDO_ITEM
                                                            taNfvItemStPdv.FillById(tblNfvItemStPdv, nfvItem.ID_NFVITEM); // já usa sproc
                                                        }

                                                        #endregion Busca TB_NFV_ITEM_ST (PDV)

                                                        #region Procedimento de gravação de TB_NFV_ITEM_ST (servidor)

                                                        foreach (var nfvItemStPdv in tblNfvItemStPdv)
                                                        {
                                                            #region Gravar TB_NFV_ITEM_ST (serv)

                                                            using (var fbCommNfvItemStSyncInsert = new FbCommand())
                                                            {
                                                                #region Prepara o comando da SP_TRI_NFVITEMST_SYNCINSERT

                                                                fbCommNfvItemStSyncInsert.Connection = fbConnServ;
                                                                //fbCommMaitPedItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                                fbCommNfvItemStSyncInsert.Transaction = fbTransactServ;

                                                                fbCommNfvItemStSyncInsert.CommandText = "SP_TRI_NFVITEMST_SYNCINSERT";
                                                                fbCommNfvItemStSyncInsert.CommandType = CommandType.StoredProcedure;

                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pID_NFVITEM", newIdNfvItem);
                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pPOR_BC_ICMS_ST", nfvItemStPdv.IsPOR_BC_ICMS_STNull() ? null : (decimal?)nfvItemStPdv.POR_BC_ICMS_ST);
                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pVLR_BC_ICMS_ST", nfvItemStPdv.IsVLR_BC_ICMS_STNull() ? null : (decimal?)nfvItemStPdv.VLR_BC_ICMS_ST);
                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pVLR_ST", nfvItemStPdv.IsVLR_STNull() ? null : (decimal?)nfvItemStPdv.VLR_ST);
                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pMVA", nfvItemStPdv.IsMVANull() ? null : (decimal?)nfvItemStPdv.MVA);

                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pALIQ_ST_ORIG", nfvItemStPdv.IsALIQ_ST_ORIGNull() ? null : (decimal?)nfvItemStPdv.ALIQ_ST_ORIG);
                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pALIQ_ST_DEST", nfvItemStPdv.IsALIQ_ST_DESTNull() ? null : (decimal?)nfvItemStPdv.ALIQ_ST_DEST);
                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pINFORMA_ST", nfvItemStPdv.IsINFORMA_STNull() ? null : nfvItemStPdv.INFORMA_ST);
                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pICMS_EFETIVO", nfvItemStPdv.ICMS_EFETIVO);
                                                                fbCommNfvItemStSyncInsert.Parameters.Add("@pVLR_ICMS_SUBSTITUTO", nfvItemStPdv.IsVLR_ICMS_SUBSTITUTONull() ? null : (decimal?)nfvItemStPdv.VLR_ICMS_SUBSTITUTO);

                                                                #endregion Prepara o comando da SP_TRI_NFVITEMST_SYNCINSERT

                                                                //newIdMaitPedItemServ = (int)
                                                                fbCommNfvItemStSyncInsert.ExecuteScalar();

                                                                //// Executa a sproc
                                                                //audit("SINCCONTNETDB >> ", string.Format("SP_TRI_NFVITEMST_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_IDENTIFICADOR: {1}, pQTD_ITEM: {2}): {3}",
                                                                //                    newIdMaitPedidoServ,
                                                                //                    pedItemPdv.ID_IDENTIFICADOR,
                                                                //                    pedItemPdv.QTD_ITEM,
                                                                //                    newIdMaitPedItemServ));
                                                            }

                                                            #endregion Gravar TB_NFV_ITEM_ST (serv)
                                                        }

                                                        #endregion Procedimento de gravação de TB_NFV_ITEM_ST (servidor)

                                                        #endregion Gravar TB_NFV_ITEM_ST

                                                        //TODO: sync TB_NFVENDA_TOT -- TALVEZ não seja necessário. Há uma trigger em TB_NFVENDA que atualiza o TOT. Testar no servidor.

                                                        #region Buscar os itens de pedido (AmbiMAITRE) (PDV)

                                                        //TODO: não há (por enquanto?)

                                                        #endregion Buscar os itens de pedido (AmbiMAITRE) (PDV)

                                                        //if (cupom.IsQTD_MAIT_PED_CUPOMNull() || cupom.QTD_MAIT_PED_CUPOM <= 0)

                                                        #region Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)

                                                        #region Atualizar no servidor a quantidade em estoque

                                                        using (var fbCommEstProdutoQtdServ = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                            fbCommEstProdutoQtdServ.Connection = fbConnServ;
                                                            //fbCommEstProdutoQtdServ.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommEstProdutoQtdServ.Transaction = fbTransactServ;

                                                            fbCommEstProdutoQtdServ.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
                                                            fbCommEstProdutoQtdServ.CommandType = CommandType.StoredProcedure;

                                                            fbCommEstProdutoQtdServ.Parameters.Add("@pQTD_ITEM", nfvItem.QTD_ITEM);
                                                            fbCommEstProdutoQtdServ.Parameters.Add("@pID_IDENTIF", nfvItem.ID_IDENTIFICADOR);
                                                            fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPPRO", 0); // cupomItem.IsID_COMPPRONull() ? 0 : cupomItem.ID_COMPPRO); // AmbiMAITRE
                                                            fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPOSICAO", nfvItem.IsID_COMPOSICAONull() ? 0 : nfvItem.ID_COMPOSICAO);

                                                            #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                            try
                                                            {
                                                                // Executa a sproc
                                                                fbCommEstProdutoQtdServ.ExecuteScalar();
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                log.Error("Erro ao deduzir quantidade em estoque (Serv): \npQTD_ITEM=" + nfvItem.QTD_ITEM.ToString() +
                                                                                   "\npID_IDENTIF=" + nfvItem.ID_IDENTIFICADOR.ToString(), ex);
                                                                throw ex;
                                                            }
                                                        }

                                                        #endregion Atualizar no servidor a quantidade em estoque

                                                        #region Atualizar no PDV a quantidade em estoque

                                                        // Já que todo o cadastro de produtos foi copiado do Serv pro PDV na etapa anterior, 
                                                        // as quantidades em estoque devem ser redefinidas
                                                        //using (var fbCommEstProdutoQtdPdv = new FbCommand())
                                                        //{
                                                        //    #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                        //    fbCommEstProdutoQtdPdv.Connection = fbConnPdv;
                                                        //    //fbCommEstProdutoQtdPdv.Connection.ConnectionString = _strConnContingency;
                                                        //    fbCommEstProdutoQtdPdv.Transaction = fbTransactPdv;

                                                        //    fbCommEstProdutoQtdPdv.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
                                                        //    fbCommEstProdutoQtdPdv.CommandType = CommandType.StoredProcedure;

                                                        //    fbCommEstProdutoQtdPdv.Parameters.Add("@pQTD_ITEM", nfvItem.QTD_ITEM);
                                                        //    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_IDENTIF", nfvItem.ID_IDENTIFICADOR);
                                                        //    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPPRO", 0); // nfvItem.IsID_COMPPRONull() ? 0 : nfvItem.ID_COMPPRO); // AmbiMAITRE
                                                        //    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPOSICAO", nfvItem.IsID_COMPOSICAONull() ? 0 : nfvItem.ID_COMPOSICAO);

                                                        //    #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                        //    try
                                                        //    {
                                                        //        // Executa a sproc
                                                        //        fbCommEstProdutoQtdPdv.ExecuteScalar();
                                                        //    }
                                                        //    catch (Exception ex)
                                                        //    {
                                                        //        gravarMensagemErro("Erro ao deduzir quantidade em estoque (PDV): \npQTD_ITEM=" + nfvItem.QTD_ITEM.ToString() +
                                                        //                           "\npID_IDENTIF=" + nfvItem.ID_IDENTIFICADOR.ToString() +
                                                        //                            " \nMais infos: " + RetornarMensagemErro(ex, true));
                                                        //        throw ex;
                                                        //    }
                                                        //}

                                                        #endregion Atualizar no PDV a quantidade em estoque

                                                        #endregion Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)
                                                    }

                                                    #endregion Consultar os itens da nfvenda do PDV

                                                    #endregion Itens de nfvenda do PDV

                                                    #region Vendas a prazo

                                                    #region Verifica se a nfvenda é a prazo

                                                    // Como verificar se o cupom é uma venda a prazo?
                                                    if (!nfvenda.IsQTD_CTARECNull() && nfvenda.QTD_CTAREC > 0)
                                                    {

                                                        using (var taCtaRecPdv = new TB_CONTA_RECEBERTableAdapter())
                                                        {
                                                            taCtaRecPdv.Connection.ConnectionString = _strConnContingency;

                                                            tblCtaRecPdv.Clear();
                                                            // Consultar todas as contas a receber do cupom
                                                            //audit("SINCCONTNETDB>> " + "taCtaRecPdv.FillByIdCupom(): " + /*taCtaRecPdv.FillByIdCupom(tblCtaRecPdv, cupom.ID_CUPOM)*/.ToString());
                                                            taCtaRecPdv.FillByIdNfvenda(tblCtaRecPdv, nfvenda.ID_NFVENDA); // já usa sproc
                                                        }

                                                        // Percorre por cada conta a receber que o cupom possui:
                                                        foreach (FDBDataSet.TB_CONTA_RECEBERRow ctaRecPdv in tblCtaRecPdv)
                                                        {
                                                            int newIdCtarec = 0;

                                                            #region Grava conta a receber na retaguarda

                                                            // TB_CONTA_RECEBER
                                                            using (var fbCommCtaRecSyncInsertServ = new FbCommand())
                                                            {
                                                                #region Prepara o comando da SP_TRI_CTAREC_SYNC_INSERT

                                                                fbCommCtaRecSyncInsertServ.Connection = fbConnServ;
                                                                //fbCommCtaRecSyncInsertServ.Connection.ConnectionString = _strConnNetwork;
                                                                fbCommCtaRecSyncInsertServ.Transaction = fbTransactServ;

                                                                fbCommCtaRecSyncInsertServ.CommandText = "SP_TRI_CTAREC_SYNC_INSERT";
                                                                fbCommCtaRecSyncInsertServ.CommandType = CommandType.StoredProcedure;

                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pDOCUMENTO", ctaRecPdv.DOCUMENTO);
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pHISTORICO", (ctaRecPdv.IsHISTORICONull() ? null : ctaRecPdv.HISTORICO));
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pDT_EMISSAO", ctaRecPdv.DT_EMISSAO);
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pDT_VENCTO", ctaRecPdv.DT_VENCTO);
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pVLR_CTAREC", ctaRecPdv.VLR_CTAREC);
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pTIP_CTAREC", ctaRecPdv.TIP_CTAREC);
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pID_PORTADOR", ctaRecPdv.ID_PORTADOR);
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pID_CLIENTE", ctaRecPdv.ID_CLIENTE);
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pINV_REFERENCIA", (ctaRecPdv.IsINV_REFERENCIANull() ? null : ctaRecPdv.INV_REFERENCIA));
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pDT_VENCTO_ORIG", (ctaRecPdv.IsDT_VENCTO_ORIGNull() ? null : (DateTime?)ctaRecPdv.DT_VENCTO_ORIG));
                                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pNSU_CARTAO", (ctaRecPdv.IsNSU_CARTAONull() ? null : ctaRecPdv.NSU_CARTAO));

                                                                #endregion Prepara o comando da SP_TRI_CTAREC_SYNC_INSERT

                                                                try
                                                                {
                                                                    // Executa a sproc
                                                                    newIdCtarec = (int)fbCommCtaRecSyncInsertServ.ExecuteScalar();
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    log.Error("Erro ABSURDO ao gravar conta a receber. Eis os parâmetros da gravação: \npINV_REFERENCIA=" +
                                                                        (ctaRecPdv.IsINV_REFERENCIANull() ? "null" : ctaRecPdv.INV_REFERENCIA.ToString()) + "\n" +
                                                                        "pDOCUMENTO=" + ctaRecPdv.DOCUMENTO.ToString() + "\n" +
                                                                        "pHISTORICO=" + (ctaRecPdv.IsHISTORICONull() ? "null" : ctaRecPdv.HISTORICO.ToString()) + "\n" +
                                                                        //"cupom.COO=" + cupom.COO.ToString() + "\n" +
                                                                        "nfvenda.NF_NUMERO=" + nfvenda.NF_NUMERO.ToString() + "\n" +
                                                                        "newIdNfvenda=" + newIdNfvenda.ToString(), ex);
                                                                    throw ex;
                                                                }
                                                            }

                                                            #endregion Grava conta a receber na retaguarda

                                                            #region Gravar a referência entre cupom e conta a receber

                                                            using (var fbCommNfvCtarecInsertServ = new FbCommand())
                                                            {
                                                                #region Prepara o comando

                                                                fbCommNfvCtarecInsertServ.Connection = fbConnServ;
                                                                fbCommNfvCtarecInsertServ.Transaction = fbTransactServ;
                                                                //fbCommNfvCtarecInsertServ.Connection.ConnectionString = _strConnNetwork;

                                                                fbCommNfvCtarecInsertServ.CommandText = "INSERT INTO TB_NFV_CTAREC (ID_NFVENDA, ID_CTAREC, ID_NUMPAG) VALUES(@ID_NFVENDA, @ID_CTAREC, @ID_NUMPAG); ";
                                                                fbCommNfvCtarecInsertServ.CommandType = CommandType.Text;

                                                                fbCommNfvCtarecInsertServ.Parameters.Add("@ID_NFVENDA", newIdNfvenda);
                                                                fbCommNfvCtarecInsertServ.Parameters.Add("@ID_CTAREC", newIdCtarec);

                                                                int? newIdNumPag = null;

                                                                if (!ctaRecPdv.IsID_NUMPAGNull())
                                                                {
                                                                    newIdNumPag = (lstAuxNfvFmaPgtoCtaRec.Find(t => t.PdvIdNumPag.Equals(ctaRecPdv.ID_NUMPAG))).ServIdNumPag;
                                                                }

                                                                fbCommNfvCtarecInsertServ.Parameters.Add("@ID_NUMPAG", newIdNumPag);

                                                                #endregion Prepara o comando

                                                                // Executa a sproc
                                                                fbCommNfvCtarecInsertServ.ExecuteNonQuery();
                                                            }

                                                            #endregion Gravar a referência entre cupom e conta a receber

                                                            #region Consultar as movimentações diárias da conta a receber

                                                            using (var taMovDiarioPdv = new TB_MOVDIARIOTableAdapter())
                                                            {
                                                                taMovDiarioPdv.Connection.ConnectionString = _strConnContingency;

                                                                tblMovDiarioPdv.Clear();
                                                                //audit("SINCCONTNETDB>> " + "taMovDiarioPdv.FillByIdCtarec(): " + taMovDiarioPdv.FillByIdCtarec(tblMovDiarioPdv, ctaRecPdv.ID_CTAREC).ToString());
                                                                taMovDiarioPdv.FillByIdCtarec(tblMovDiarioPdv, ctaRecPdv.ID_CTAREC); // já usa sproc
                                                            }

                                                            #endregion Consultar as movimentações diárias da conta a receber

                                                            #region Gravar movimentação diária referente à conta a receber

                                                            if (tblMovDiarioPdv != null && tblMovDiarioPdv.Rows.Count > 0)
                                                            {
                                                                foreach (FDBDataSet.TB_MOVDIARIORow movdiarioPdv in tblMovDiarioPdv)
                                                                {
                                                                    int newIdMovto = 0;
                                                                    //movdiarioPdv.SYNCED = 1;
                                                                    // TB_MOVDIARIO
                                                                    using (var fbCommMovDiarioMovtoSyncInsertServ = new FbCommand())
                                                                    {
                                                                        #region Prepara o comando da SP_TRI_MOVTO_SYNC_INSERT

                                                                        fbCommMovDiarioMovtoSyncInsertServ.Connection = fbConnServ;
                                                                        //fbCommMovDiarioMovtoSyncInsertServ.Connection.ConnectionString = _strConnNetwork;
                                                                        fbCommMovDiarioMovtoSyncInsertServ.Transaction = fbTransactServ;

                                                                        fbCommMovDiarioMovtoSyncInsertServ.CommandText = "SP_TRI_MOVTO_SYNC_INSERT";
                                                                        fbCommMovDiarioMovtoSyncInsertServ.CommandType = CommandType.StoredProcedure;

                                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pDT_MOVTO", (movdiarioPdv.IsDT_MOVTONull() ? null : (DateTime?)movdiarioPdv.DT_MOVTO));
                                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pHR_MOVTO", (movdiarioPdv.IsHR_MOVTONull() ? null : (TimeSpan?)movdiarioPdv.HR_MOVTO));
                                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pHISTORICO", (movdiarioPdv.IsHISTORICONull() ? null : movdiarioPdv.HISTORICO));
                                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pTIP_MOVTO", (movdiarioPdv.IsTIP_MOVTONull() ? null : movdiarioPdv.TIP_MOVTO));
                                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pVLR_MOVTO", (movdiarioPdv.IsVLR_MOVTONull() ? null : (decimal?)movdiarioPdv.VLR_MOVTO));
                                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pID_CTAPLA", (movdiarioPdv.IsID_CTAPLANull() ? null : (short?)movdiarioPdv.ID_CTAPLA));
                                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pSYNCED", 1);

                                                                        #endregion Prepara o comando da SP_TRI_MOVTO_SYNC_INSERT

                                                                        try
                                                                        {
                                                                            // Executa a sproc
                                                                            newIdMovto = (int)fbCommMovDiarioMovtoSyncInsertServ.ExecuteScalar(); //TODO: esse trecho é problemático para a Estilo K, às vezes apresenta dead-lock
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            log.Error("Erro ao sync movdiario (Serv): \npDT_MOVTO=" + (movdiarioPdv.IsDT_MOVTONull() ? "null" : movdiarioPdv.DT_MOVTO.ToString()) +
                                                                                               "\npHR_MOVTO=" + (movdiarioPdv.IsHR_MOVTONull() ? "null" : movdiarioPdv.HR_MOVTO.ToString()) +
                                                                                               "\npHISTORICO=" + (movdiarioPdv.IsHISTORICONull() ? "null" : movdiarioPdv.HISTORICO.ToString()) +
                                                                                               "\npTIP_MOVTO=" + (movdiarioPdv.IsTIP_MOVTONull() ? "null" : movdiarioPdv.TIP_MOVTO.ToString()) +
                                                                                               "\npVLR_MOVTO=" + (movdiarioPdv.IsVLR_MOVTONull() ? "null" : movdiarioPdv.VLR_MOVTO.ToString()) +
                                                                                               "\npID_CTAPLA=" + (movdiarioPdv.IsID_CTAPLANull() ? "null" : movdiarioPdv.ID_CTAPLA.ToString()), ex);
                                                                            throw ex;
                                                                        }
                                                                    }

                                                                    #region Gravar a referência entre a conta a receber e a movimentação diária

                                                                    using (var fbCommCtarecMovtoInsertServ = new FbCommand())
                                                                    {
                                                                        #region Prepara o comando

                                                                        fbCommCtarecMovtoInsertServ.Connection = fbConnServ;
                                                                        fbCommCtarecMovtoInsertServ.Transaction = fbTransactServ;
                                                                        //fbCommCtarecMovtoInsertServ.Connection.ConnectionString = _strConnNetwork;

                                                                        fbCommCtarecMovtoInsertServ.CommandText = "INSERT INTO TB_CTAREC_MOVTO (ID_MOVTO, ID_CTAREC) VALUES(@ID_MOVTO, @ID_CTAREC); ";
                                                                        fbCommCtarecMovtoInsertServ.CommandType = CommandType.Text;

                                                                        fbCommCtarecMovtoInsertServ.Parameters.Add("@ID_MOVTO", newIdMovto);
                                                                        fbCommCtarecMovtoInsertServ.Parameters.Add("@ID_CTAREC", newIdCtarec);

                                                                        #endregion Prepara o comando

                                                                        // Executa a sproc
                                                                        fbCommCtarecMovtoInsertServ.ExecuteNonQuery();
                                                                    }

                                                                    #endregion Gravar a referência entre a conta a receber e a movimentação diária

                                                                    #region Indicar que o fechamento de caixa foi sincronizado

                                                                    using (var taMovDiarioPdv = new TB_MOVDIARIOTableAdapter())
                                                                    {
                                                                        taMovDiarioPdv.Connection = fbConnPdv;
                                                                        taMovDiarioPdv.Transaction = fbTransactPdv;
                                                                        //taMovDiarioPdv.Connection.ConnectionString = _strConnContingency;

                                                                        taMovDiarioPdv.SP_TRI_MOVTOSETSYNCED(movdiarioPdv.ID_MOVTO, 1);
                                                                    }

                                                                    #endregion Indicar que o fechamento de caixa foi sincronizado
                                                                }
                                                            }

                                                            #endregion Gravar movimentação diária referente à conta a receber
                                                        }
                                                    }

                                                    #endregion Verifica se a nfvenda é a prazo

                                                    #endregion Vendas a prazo

                                                    #region Verificar se a nfvenda foi cancelado: reativar o orçamento vinculado, se houver

                                                    //TODO: completar no orçamento

                                                    //if (nfvenda.STATUS == "C" || nfvenda.STATUS == "X")
                                                    //{
                                                    //    // TRI_PDV_ORCA_NFVENDA_REL
                                                    //    using (var taOrcaServ = new DataSets.FDBDataSetOrcamTableAdapters.TRI_PDV_ORCA_NFVENDA_RELTableAdapter())
                                                    //    {
                                                    //        taOrcaServ.Connection = fbConnServ;
                                                    //        taOrcaServ.Transaction = fbTransactServ;
                                                    //        //taOrcaServ.Connection.ConnectionString = _strConnNetwork;

                                                    //        audit("SINCCONTNETDB>> ", string.Format("(nfvenda cancelado antes de sync) taOrcaServ.SP_TRI_ORCA_REATIVAORCA({1}): {0}",
                                                    //                            taOrcaServ.SP_TRI_ORCA_REATIVAORCA(nfvenda.ID_NFVENDA).Safestring(),
                                                    //                            nfvenda.ID_NFVENDA.Safestring()));
                                                    //    }
                                                    //}

                                                    #endregion Verificar se a nfvenda foi cancelado: reativar o orçamento vinculado, se houver

                                                    #region Indicar que a nfvenda foi synced

                                                    using (var fbCommNfvendaSetSynced = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_NFVENDASETSYNCED

                                                        fbCommNfvendaSetSynced.Connection = fbConnPdv;
                                                        fbCommNfvendaSetSynced.Transaction = fbTransactPdv;

                                                        //fbCommNfvendaSetSynced.CommandText = "SP_TRI_CUPOMSETSYNCED";
                                                        fbCommNfvendaSetSynced.CommandText = "SP_TRI_NFVENDA_SETSYNCED";
                                                        fbCommNfvendaSetSynced.CommandType = CommandType.StoredProcedure;

                                                        fbCommNfvendaSetSynced.Parameters.Add("@pIdNfvenda", nfvenda.ID_NFVENDA);
                                                        fbCommNfvendaSetSynced.Parameters.Add("@pSynced", 1);

                                                        #endregion Prepara o comando da SP_TRI_NFVENDASETSYNCED

                                                        // Executa a sproc
                                                        fbCommNfvendaSetSynced.ExecuteScalar();
                                                    }

                                                    #endregion Indicar que a nfvenda foi synced

                                                    //fbConnPdv.Close();
                                                    //fbConnServ.Close();

                                                    // Finaliza a transação:
                                                    //transactionScopeCupons.Complete();
                                                    fbTransactServ.Commit();
                                                    fbTransactPdv.Commit();
                                                }
                                                catch (TransactionAbortedException taEx)
                                                {
                                                    log.Error("TransactionAbortedException", taEx);
                                                    fbTransactServ.Rollback();
                                                    fbTransactPdv.Rollback();
                                                }
                                                catch (Exception ex)
                                                {
                                                    log.Error("Erro durante a transação de nfvendas:", ex);
                                                    fbTransactServ.Rollback();
                                                    fbTransactPdv.Rollback();
                                                }
                                            }
                                            #region Forçar a execução da trigger ref. a tabela TB_NFVENDA_TOT

                                            using (var fbCommNfvendaForceTriggerUpdate = new FbCommand())
                                            {
                                                #region Prepara o comando da SP_TRI_NFVENDASETSYNCED

                                                fbCommNfvendaForceTriggerUpdate.Connection = fbConnServ;

                                                fbCommNfvendaForceTriggerUpdate.CommandText = "UPDATE TB_NFVENDA SET STATUS = 'I' WHERE ID_NFVENDA = @Param1 AND STATUS = 'I'";
                                                fbCommNfvendaForceTriggerUpdate.CommandType = CommandType.Text;

                                                fbCommNfvendaForceTriggerUpdate.Parameters.Add("@Param1", newIdNfvenda);


                                                #endregion Prepara o comando da SP_TRI_NFVENDASETSYNCED

                                                // Executa o ad-hoc
                                                fbCommNfvendaForceTriggerUpdate.ExecuteScalar();
                                            }


                                            #endregion

                                        }
                                        #endregion Gravar a nfvenda na retaguarda (transação)


                                    }

                                    log.Debug(string.Format("Lote {0} de nfvendas processado!", contLote.ToString()));

                                    #endregion Sincroniza (manda para a retaguarda)

                                    #region Prepara o próximo lote

                                    // Limpa a tabela para pegar o próximo lote (é necessário limpar mesmo? o comando seguinte deveria sobrescrevê-lo):
                                    tblNfvendaUnsynced.Clear();
                                    log.Debug("taNfvendaUnsynced.FillByNfvendaSync(): " + taNfvendaUnsynced.FillByNfvendaSync(tblNfvendaUnsynced, 0).ToString()); // já usa sproc

                                    #endregion Prepara o próximo lote
                                }

                                #region NOPE - CLIPP RULES NO MORE
                                //taEstProdutoPdv.SP_TRI_FIX_CLIPP_RULES();
                                //taEstProdutoServ.SP_TRI_FIX_CLIPP_RULES();
                                #endregion NOPE - CLIPP RULES NO MORE
                            }
                            #endregion Procedimento executado enquanto houver cupons para sincronizar
                        }
                        #region Manipular Exception
                        catch (Exception ex)
                        {
                            log.Error("Erro ao sincronizar", ex);
                            GravarErroSync("Erro ao sincronizar nfvendas", tblCtaRecPdv, ex);
                            GravarErroSync("Erro ao sincronizar nfvendas", tblNfvendaFmapagtoNfcePdv, ex);
                            GravarErroSync("Erro ao sincronizar nfvendas", tblNfvItemPdv, ex);
                            GravarErroSync("Erro ao sincronizar nfvendas", tblNfvendaUnsynced, ex);
                            GravarErroSync("Erro ao sincronizar nfvendas", tblMovDiarioPdv, ex);
                            throw ex;
                        }
                        #endregion Manipular Exception
                        #region Limpeza da transação
                        finally
                        {
                            #region Trata disposable objects

                            #region(cupons, contas a receber e movimentação diária)

                            if (taNfvendaFmaPagtoNfcePdv != null) { taNfvendaFmaPagtoNfcePdv.Dispose(); }
                            //if (taCupomFmaPagtoServ != null) { taCupomFmaPagtoServ.Dispose(); }

                            //if (taTrocaPdv != null) { taTrocaPdv.Dispose(); }
                            //if (taTrocaServ != null) { taTrocaServ.Dispose(); }
                            //if (tblTrocaPdv != null) { tblTrocaPdv.Dispose(); }

                            //if (taCupomServ != null) { taCupomServ.Dispose(); }
                            //if (taCtaRecServ != null) { taCtaRecServ.Dispose(); }
                            //if (taCupomCtarecServ != null) { taCupomCtarecServ.Dispose(); }
                            //if (taMovDiarioServ != null) { taMovDiarioServ.Dispose(); }
                            //if (taCtarecMovtoServ != null) { taCtarecMovtoServ.Dispose(); }

                            //if (taCtaRecPdv != null) { taCtaRecPdv.Dispose(); }
                            //if (taMovDiarioPdv != null) { taMovDiarioPdv.Dispose(); }

                            if (tblNfvendaFmapagtoNfcePdv != null) { tblNfvendaFmapagtoNfcePdv.Dispose(); }
                            if (tblCtaRecPdv != null) { tblCtaRecPdv.Dispose(); }
                            if (tblMovDiarioPdv != null) { tblMovDiarioPdv.Dispose(); }

                            #endregion (cupons, contas a receber e movimentação diária)

                            #region (item de cupom)

                            if (tblNfvItemPdv != null) { tblNfvItemPdv.Dispose(); }
                            if (taNfvItemPdv != null) { taNfvItemPdv.Dispose(); }
                            //if (taCupomItemServ != null) { taCupomItemServ.Dispose(); }

                            #endregion (item de cupom)

                            #region (cupons unsynced, produtos)

                            if (taNfvendaUnsynced != null) { taNfvendaUnsynced.Dispose(); }
                            if (tblNfvendaUnsynced != null) { tblNfvendaUnsynced.Dispose(); }

                            if (taEstProdutoServ != null) { taEstProdutoServ.Dispose(); }
                            if (taEstProdutoPdv != null) { taEstProdutoPdv.Dispose(); }

                            #endregion (cupons unsynced, produtos)

                            taSatPdv?.Dispose();
                            tblSatPdv?.Dispose();
                            taSatCancPdv?.Dispose();
                            tblSatCancPdv?.Dispose();

                            tblNfvItemCofinsPdv?.Dispose();
                            tblNfvItemPisPdv?.Dispose();
                            tblNfvItemIcmsPdv?.Dispose();
                            tblNfvItemStPdv?.Dispose();

                            #endregion Trata disposable objects
                        }
                        #endregion Limpeza da transação
                    }

                    #endregion Padrão, unsynced

                    #region Nfvendas sincronizadas e posteriormente canceladas
                    {
                        #region Cria os TableAdapters, DataTables e variáveis

                        var taNfvendaSyncedCancelPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter();
                        taNfvendaSyncedCancelPdv.Connection.ConnectionString = _strConnContingency;
                        var tblNfvendaSyncedCancelPdv = new FDBDataSetVenda.TB_NFVENDADataTable();

                        var taEstProdutoServ = new TB_EST_PRODUTOTableAdapter();
                        taEstProdutoServ.Connection.ConnectionString = _strConnNetwork;

                        var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter();
                        taEstProdutoPdv.Connection.ConnectionString = _strConnContingency;

                        int intCountLoteNfvendaSyncedCancel = 0;

                        #endregion Cria os TableAdapters, DataTables e variáveis

                        try
                        {
                            // Busca todos os cupons que foram synced e posteriormente cancelados (TIP_QUERY = 1)
                            // Lembrando que a sproc executada abaixo retorna até 200 registros por vez.
                            log.Debug("taNfvendaSyncedCancelPdv.FillByNfvendaSync(): " + taNfvendaSyncedCancelPdv.FillByNfvendaSync(tblNfvendaSyncedCancelPdv, 1).ToString()); // já usa sproc

                            while (tblNfvendaSyncedCancelPdv != null && tblNfvendaSyncedCancelPdv.Rows.Count > 0)
                            {
                                intCountLoteNfvendaSyncedCancel++;

                                #region NOPE - Break Clipp rules agora é permanente
                                //// Para repor quantidade em estoque sem dar problemas
                                //taEstProdutoServ.SP_TRI_BREAK_CLIPP_RULES();
                                //taEstProdutoPdv.SP_TRI_BREAK_CLIPP_RULES();
                                #endregion NOPE - Break Clipp rules agora é permanente

                                // Percorre por cada cupom cancelado:
                                foreach (FDBDataSetVenda.TB_NFVENDARow nfvendaCancelPdv in tblNfvendaSyncedCancelPdv)
                                {
                                    #region Validações

                                    //// Foi necessário adaptar o COO como o ID_CUPOM negativo para sistema legado
                                    //if (cupomCancelPdv.IsCOONull()) { cupomCancelPdv.COO = cupomCancelPdv.ID_CUPOM * -1; }
                                    //if (cupomCancelPdv.IsNUM_CAIXANull()) { cupomCancelPdv.NUM_CAIXA = 0; }

                                    #endregion Validações

                                    #region Iniciar o procedimento de cancelamento na retaguarda
                                    //using (var transactionScopeCupomResync = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(1, 0, 0, 0)))
                                    using (var transactionScopeNfvendaResync = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                                    {
                                        // Define a conexão com o banco do servidor:
                                        using (var fbConnServ = new FbConnection(_strConnNetwork))
                                        // Define a conexão com o banco do PDV:
                                        using (var fbConnPdv = new FbConnection(_strConnContingency))
                                        using (var tblNfvendaItemSyncedCancelPdv = new FDBDataSetVenda.TB_NFV_ITEMDataTable())
                                        using (var tblCtarecServ = new FDBDataSet.TB_CONTA_RECEBERDataTable())
                                        using (var tblMovtoServ = new FDBDataSet.TB_MOVDIARIODataTable())
                                        {
                                            fbConnServ.Open();
                                            fbConnPdv.Open();

                                            // Verificar se a nfvenda tem conta a receber:
                                            if (!nfvendaCancelPdv.IsQTD_CTARECNull() && nfvendaCancelPdv.QTD_CTAREC > 0)
                                            {
                                                #region Busca as contas a receber da nfvenda no serv
                                                try
                                                {
                                                    using (var taCtarecServ = new TB_CONTA_RECEBERTableAdapter())
                                                    {
                                                        taCtarecServ.Connection = fbConnServ;
                                                        //audit("SINCCONTNETDB>> " + "taCtarecServ.FillByCooNumcaixa(): " + taCtarecServ.FillByCooNumcaixa(tblCtarecServ, cupomCancelPdv.COO, cupomCancelPdv.NUM_CAIXA).ToString());

                                                        //TODO -- DONE --: qual é a chave de identificação de uma nfvenda equivalente para ambas as bases de dados (cliente/servidor)?
                                                        // Deve ser NF_NUMERO e NF_SERIE

                                                        taCtarecServ.FillByNfNumeroSerie(tblCtarecServ, nfvendaCancelPdv.NF_NUMERO, nfvendaCancelPdv.NF_SERIE).ToString(); // já usa sproc
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    log.Error("Erro ao consultar contas a receber no servidor ( / NF_NUMERO = " + nfvendaCancelPdv.NF_NUMERO + " / NF_SERIE = " + nfvendaCancelPdv.NF_SERIE + "): ", ex);
                                                    throw ex;
                                                }
                                                #endregion Busca as contas a receber da nfvenda no serv

                                                #region Percorre por cada conta a receber no servidor
                                                foreach (FDBDataSet.TB_CONTA_RECEBERRow ctarecServ in tblCtarecServ)
                                                {
                                                    #region Busca os movimentos diários da conta a receber
                                                    using (var taMovtoServ = new TB_MOVDIARIOTableAdapter())
                                                    {
                                                        taMovtoServ.Connection = fbConnServ;
                                                        tblMovtoServ.Clear();
                                                        //audit("SINCCONTNETDB>> " + "taMovtoServ.FillByIdCtarec(): " + taMovtoServ.FillByIdCtarec(tblMovtoServ, ctarecServ.ID_CTAREC).ToString());
                                                        taMovtoServ.FillByIdCtarec(tblMovtoServ, ctarecServ.ID_CTAREC); // já usa sproc
                                                    }
                                                    #endregion Busca os movimentos diários da conta a receber

                                                    #region Percorre por cada movimentação diária do servidor
                                                    foreach (FDBDataSet.TB_MOVDIARIORow movtoServ in tblMovtoServ)
                                                    {
                                                        #region Apagar TB_CTAREC_MOVTO
                                                        //taCtarecMovtoServ.SP_TRI_CTARECMOVTO_SYNC_DEL(movtoServ.ID_MOVTO, ctarecServ.ID_CTAREC);
                                                        using (var fbCommCtarecMovtoSyncDelServ = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_CTARECMOVTO_SYNC_DEL

                                                            fbCommCtarecMovtoSyncDelServ.Connection = fbConnServ;

                                                            fbCommCtarecMovtoSyncDelServ.CommandText = "SP_TRI_CTARECMOVTO_SYNC_DEL";
                                                            fbCommCtarecMovtoSyncDelServ.CommandType = CommandType.StoredProcedure;

                                                            fbCommCtarecMovtoSyncDelServ.Parameters.Add("@pID_MOVTO", movtoServ.ID_MOVTO);
                                                            fbCommCtarecMovtoSyncDelServ.Parameters.Add("@pID_CTAREC", ctarecServ.ID_CTAREC);

                                                            #endregion Prepara o comando da SP_TRI_CTARECMOVTO_SYNC_DEL

                                                            // Executa a sproc
                                                            fbCommCtarecMovtoSyncDelServ.ExecuteScalar();
                                                        }
                                                        #endregion Apagar TB_CTAREC_MOVTO

                                                        #region Apagar TB_MOVDIARIO
                                                        //taMovtoServ.SP_TRI_MOVTO_SYNC_DEL(movtoServ.ID_MOVTO);
                                                        using (var fbCommMovtoSyncDelServ = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_MOVTO_SYNC_DEL

                                                            fbCommMovtoSyncDelServ.Connection = fbConnServ;

                                                            fbCommMovtoSyncDelServ.CommandText = "SP_TRI_MOVTO_SYNC_DEL";
                                                            fbCommMovtoSyncDelServ.CommandType = CommandType.StoredProcedure;

                                                            fbCommMovtoSyncDelServ.Parameters.Add("@pID_MOVTO", movtoServ.ID_MOVTO);

                                                            #endregion Prepara o comando da SP_TRI_MOVTO_SYNC_DEL

                                                            // Executa a sproc
                                                            fbCommMovtoSyncDelServ.ExecuteScalar();
                                                        }
                                                        #endregion Apagar TB_MOVDIARIO
                                                    }
                                                    #endregion Percorre por cada movimentação diária do servidor

                                                    #region Apagar o vínculo TB_NFV_CTAREC
                                                    //taCupomCtarecServ.SP_TRI_CUPOMCTAREC_SYNC_DEL(cupomCancelPdv.COO, cupomCancelPdv.NUM_CAIXA, ctarecServ.ID_CTAREC);
                                                    using (var fbCommNfvCtarecSyncDelServ = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_NFV_CTAREC_SYNC_DEL

                                                        fbCommNfvCtarecSyncDelServ.Connection = fbConnServ;

                                                        //TODO: talvez seja necessário adaptar a sproc para usar a ID_NUMPAG...

                                                        fbCommNfvCtarecSyncDelServ.CommandText = "SP_TRI_NFV_CTAREC_SYNC_DEL";
                                                        fbCommNfvCtarecSyncDelServ.CommandType = CommandType.StoredProcedure;

                                                        fbCommNfvCtarecSyncDelServ.Parameters.Add("@pNfNumero", nfvendaCancelPdv.NF_NUMERO);
                                                        fbCommNfvCtarecSyncDelServ.Parameters.Add("@pNfSerie", nfvendaCancelPdv.NF_SERIE);
                                                        fbCommNfvCtarecSyncDelServ.Parameters.Add("@pIdCtarec", ctarecServ.ID_CTAREC);

                                                        #endregion Prepara o comando da SP_TRI_NFV_CTAREC_SYNC_DEL

                                                        // Executa a sproc
                                                        try
                                                        {
                                                            fbCommNfvCtarecSyncDelServ.ExecuteScalar();
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            log.Error(String.Format("NF_NUMERO: {1}, NF_SERIE: {2}, ID_CTAREC {3}",
                                                                                             nfvendaCancelPdv.NF_NUMERO,
                                                                                             nfvendaCancelPdv.NF_SERIE,
                                                                                             ctarecServ.ID_CTAREC), ex);
                                                            throw ex;
                                                        }

                                                    }
                                                    #endregion Apagar o vínculo TB_CUPOM_CTAREC

                                                    #region Apagar conta a receber (TB_CONTA_RECEBER)

                                                    using (var fbCommCtarecSyncDelServ = new FbCommand())
                                                    {
                                                        #region Prepara o comando para deletar conta a receber

                                                        fbCommCtarecSyncDelServ.Connection = fbConnServ;

                                                        fbCommCtarecSyncDelServ.CommandText = "DELETE FROM TB_CONTA_RECEBER WHERE ID_CTAREC = @pID_CTAREC;";
                                                        fbCommCtarecSyncDelServ.CommandType = CommandType.Text;

                                                        fbCommCtarecSyncDelServ.Parameters.Add("@pID_CTAREC", ctarecServ.ID_CTAREC);

                                                        #endregion Prepara o comando para deletar conta a receber

                                                        try
                                                        {
                                                            // Executa a sproc
                                                            fbCommCtarecSyncDelServ.ExecuteScalar();
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            log.Error($"Erro ao cancelar (excluir) conta a receber no servidor (ID_CTAREC { ctarecServ.ID_CTAREC }).", ex);
                                                            throw ex;
                                                        }
                                                    }
                                                    #endregion Apagar conta a receber (TB_CONTA_RECEBER)
                                                }
                                                #endregion Percorre por cada conta a receber no servidor
                                            }

                                            #region Atualizar TB_SAT no servidor

                                            using (var tblSatPdv = new FDBDataSetVenda.TB_SATDataTable())
                                            using (var taSatPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_SATTableAdapter())
                                            using (var taSatServ = new DataSets.FDBDataSetVendaTableAdapters.TB_SATTableAdapter())
                                            {
                                                taSatPdv.Connection = fbConnPdv;
                                                taSatPdv.FillByIdNfvenda(tblSatPdv, nfvendaCancelPdv.ID_NFVENDA);

                                                taSatServ.Connection = fbConnServ;

                                                foreach (var itemSatPdv in tblSatPdv)
                                                {
                                                    #region Atualizar item SAT equivalente no servidor

                                                    using (var fbCommSatUpsertServ = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_SAT_UPSERT_BY_CHAVE

                                                        fbCommSatUpsertServ.Connection = fbConnServ;

                                                        fbCommSatUpsertServ.CommandText = "SP_TRI_SAT_UPSERT_BY_CHAVE";
                                                        fbCommSatUpsertServ.CommandType = CommandType.StoredProcedure;

                                                        //fbCommSatUpsertServ.Parameters.Add("@pID_NFVENDA", itemSatPdv.ID_NFVENDA);

                                                        fbCommSatUpsertServ.Parameters.Add("@pNF_NUMERO", nfvendaCancelPdv.NF_NUMERO);
                                                        fbCommSatUpsertServ.Parameters.Add("@pNF_SERIE", nfvendaCancelPdv.NF_SERIE);

                                                        fbCommSatUpsertServ.Parameters.Add("@pCHAVE", itemSatPdv.CHAVE); //TODO: NÃO PODE SER NULL! VAI DAR RUIM
                                                        fbCommSatUpsertServ.Parameters.Add("@pDT_EMISSAO", itemSatPdv.IsDT_EMISSAONull() ? null : (DateTime?)itemSatPdv.DT_EMISSAO);
                                                        fbCommSatUpsertServ.Parameters.Add("@pHR_EMISSAO", itemSatPdv.IsHR_EMISSAONull() ? null : (TimeSpan?)itemSatPdv.HR_EMISSAO);

                                                        fbCommSatUpsertServ.Parameters.Add("@pSTATUS", itemSatPdv.IsSTATUSNull() ? null : itemSatPdv.STATUS);
                                                        fbCommSatUpsertServ.Parameters.Add("@pSTATUS_DES", itemSatPdv.IsSTATUS_DESNull() ? null : itemSatPdv.STATUS_DES);
                                                        fbCommSatUpsertServ.Parameters.Add("@pNUMERO_CFE", itemSatPdv.IsNUMERO_CFENull() ? null : (int?)itemSatPdv.NUMERO_CFE);
                                                        fbCommSatUpsertServ.Parameters.Add("@pNUM_SERIE_SAT", itemSatPdv.IsNUM_SERIE_SATNull() ? null : itemSatPdv.NUM_SERIE_SAT);

                                                        #endregion Prepara o comando da SP_TRI_SAT_UPSERT_BY_CHAVE

                                                        // Executa a sproc
                                                        try
                                                        {
                                                            log.Debug($"pNF_NUMERO: {fbCommSatUpsertServ.Parameters[0]}");
                                                            log.Debug($"pNF_SERIE: {fbCommSatUpsertServ.Parameters[1]}");
                                                            log.Debug($"pCHAVE: {fbCommSatUpsertServ.Parameters[2]}");
                                                            log.Debug($"pDT_EMISSAO: {fbCommSatUpsertServ.Parameters[3]}");
                                                            log.Debug($"pHR_EMISSAO: {fbCommSatUpsertServ.Parameters[4]}");
                                                            log.Debug($"pSTATUS: {fbCommSatUpsertServ.Parameters[5]}");
                                                            log.Debug($"pSTATUS_DES: {fbCommSatUpsertServ.Parameters[6]}");
                                                            log.Debug($"pNUMERO_CFE: {fbCommSatUpsertServ.Parameters[7]}");
                                                            log.Debug($"pNUM_SERIE_SAT: {fbCommSatUpsertServ.Parameters[8]}");

                                                            fbCommSatUpsertServ.ExecuteScalar();
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            log.Error($"pNF_NUMERO: {nfvendaCancelPdv.NF_NUMERO}, NF_SERIE: {nfvendaCancelPdv.NF_SERIE}, pCHAVE {itemSatPdv.CHAVE} (PDV)", ex);
                                                            throw ex;
                                                        }

                                                    }

                                                    #endregion Atualizar item SAT equivalente no servidor

                                                    #region Buscar os itens cancelados SAT do item SAT atual

                                                    using (var tblSatCancPdv = new FDBDataSetVenda.TB_SAT_CANCDataTable())
                                                    using (var taSatCancPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_SAT_CANCTableAdapter())
                                                    using (var taSatCancServ = new DataSets.FDBDataSetVendaTableAdapters.TB_SAT_CANCTableAdapter())
                                                    {
                                                        taSatCancPdv.Connection = fbConnPdv;
                                                        taSatCancServ.Connection = fbConnServ;

                                                        taSatCancPdv.FillByIdRegistro(tblSatCancPdv, itemSatPdv.ID_REGISTRO);

                                                        foreach (var itemSatCancPdv in tblSatCancPdv)
                                                        {
                                                            #region Atualizar item cancelado SAT equivalente no servidor:

                                                            using (var fbCommSatCancUpsertServ = new FbCommand())
                                                            {
                                                                #region Prepara o comando da SP_TRI_SATCANC_UPSERT_BY_CHAVE

                                                                fbCommSatCancUpsertServ.Connection = fbConnServ;

                                                                fbCommSatCancUpsertServ.CommandText = "SP_TRI_SATCANC_UPSERT_BY_CHAVE";
                                                                fbCommSatCancUpsertServ.CommandType = CommandType.StoredProcedure;

                                                                fbCommSatCancUpsertServ.Parameters.Add("@pCHAVE_SAT", itemSatPdv.CHAVE); //TODO: NÃO PODE SER NULO!!
                                                                fbCommSatCancUpsertServ.Parameters.Add("@pDT_EMISSAO", itemSatCancPdv.IsDT_EMISSAONull() ? null : (DateTime?)itemSatCancPdv.DT_EMISSAO);
                                                                fbCommSatCancUpsertServ.Parameters.Add("@pHR_EMISSAO", itemSatCancPdv.IsHR_EMISSAONull() ? null : (TimeSpan?)itemSatCancPdv.HR_EMISSAO);
                                                                fbCommSatCancUpsertServ.Parameters.Add("@pNUMERO_CFE", itemSatCancPdv.IsNUMERO_CFENull() ? null : (int?)itemSatCancPdv.NUMERO_CFE);

                                                                fbCommSatCancUpsertServ.Parameters.Add("@pCHAVE", itemSatCancPdv.CHAVE); //TODO: NÃO PODE SER NULO!!
                                                                fbCommSatCancUpsertServ.Parameters.Add("@pNUM_SERIE_SAT", itemSatCancPdv.IsNUM_SERIE_SATNull() ? null : itemSatCancPdv.NUM_SERIE_SAT);
                                                                fbCommSatCancUpsertServ.Parameters.Add("@pENVIO_API", itemSatCancPdv.IsENVIO_APINull() ? null : (DateTime?)itemSatCancPdv.ENVIO_API);

                                                                #endregion Prepara o comando da SP_TRI_SATCANC_UPSERT_BY_CHAVE

                                                                // Executa a sproc
                                                                try
                                                                {
                                                                    fbCommSatCancUpsertServ.ExecuteScalar();
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    log.Error($"pNF_NUMERO: {nfvendaCancelPdv.NF_NUMERO}, NF_SERIE: {nfvendaCancelPdv.NF_SERIE}, pCHAVE {itemSatPdv.CHAVE} (PDV)", ex);
                                                                    throw ex;
                                                                }

                                                            }

                                                            #endregion Atualizar item cancelado SAT equivalente no servidor:
                                                        }
                                                    }

                                                    #endregion Buscar os itens cancelados SAT do item SAT atual
                                                }
                                            }

                                            #endregion Atualizar TB_SAT no servidor

                                            #region Atualizar no servidor a quantidade em estoque

                                            using (var taNfvItemSyncedCancelPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEMTableAdapter())
                                            {
                                                taNfvItemSyncedCancelPdv.Connection = fbConnPdv;
                                                tblNfvendaItemSyncedCancelPdv.Clear();
                                                taNfvItemSyncedCancelPdv.FillByIdNfvenda(tblNfvendaItemSyncedCancelPdv, nfvendaCancelPdv.ID_NFVENDA); // já usa sproc
                                            }

                                            // Percorrer por cada item de cupom para repor as quantidades em estoque:
                                            foreach (FDBDataSetVenda.TB_NFV_ITEMRow nfvendaItemSyncedCancel in tblNfvendaItemSyncedCancelPdv)
                                            {
                                                #region Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)

                                                #region Atualizar no servidor a quantidade em estoque

                                                using (var fbCommEstProdutoQtdServ = new FbCommand())
                                                {
                                                    #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                    fbCommEstProdutoQtdServ.Connection = fbConnServ;

                                                    fbCommEstProdutoQtdServ.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
                                                    fbCommEstProdutoQtdServ.CommandType = CommandType.StoredProcedure;

                                                    fbCommEstProdutoQtdServ.Parameters.Add("@pQTD_ITEM", nfvendaItemSyncedCancel.QTD_ITEM * -1);
                                                    fbCommEstProdutoQtdServ.Parameters.Add("@pID_IDENTIF", nfvendaItemSyncedCancel.ID_IDENTIFICADOR);
                                                    fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPPRO", 0);// cupomItemSyncedCancel.IsID_COMPPRONull() ? 0 : cupomItemSyncedCancel.ID_COMPPRO); // AmbiMAITRE
                                                    fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPOSICAO", nfvendaItemSyncedCancel.IsID_COMPOSICAONull() ? 0 : nfvendaItemSyncedCancel.ID_COMPOSICAO);

                                                    #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                    // Executa a sproc
                                                    fbCommEstProdutoQtdServ.ExecuteScalar();
                                                }

                                                #endregion Atualizar no servidor a quantidade em estoque

                                                #region Atualizar no PDV a quantidade em estoque

                                                // Já que todo o cadastro de produtos foi copiado do Serv pro PDV na etapa anterior, 
                                                // as quantidades em estoque devem ser redefinidas
                                                using (var fbCommEstProdutoQtdPdv = new FbCommand())
                                                {
                                                    #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                    fbCommEstProdutoQtdPdv.Connection = fbConnPdv;

                                                    fbCommEstProdutoQtdPdv.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
                                                    fbCommEstProdutoQtdPdv.CommandType = CommandType.StoredProcedure;

                                                    fbCommEstProdutoQtdPdv.Parameters.Add("@pQTD_ITEM", nfvendaItemSyncedCancel.QTD_ITEM * -1);
                                                    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_IDENTIF", nfvendaItemSyncedCancel.ID_IDENTIFICADOR);
                                                    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPPRO", 0);// ItemSyncedCancel.IsID_COMPPRONull() ? 0 : cupomItemSyncedCancel.ID_COMPPRO); // AmbiMAITRE
                                                    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPOSICAO", nfvendaItemSyncedCancel.IsID_COMPOSICAONull() ? 0 : nfvendaItemSyncedCancel.ID_COMPOSICAO);

                                                    #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                    // Executa a sproc
                                                    fbCommEstProdutoQtdPdv.ExecuteScalar();
                                                }

                                                #endregion Atualizar no PDV a quantidade em estoque

                                                #endregion Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)
                                            }

                                            #endregion Atualizar no servidor a quantidade em estoque

                                            #region Indicar que a nfvenda foi synced depois de cancelado (Serv)

                                            using (var fbCommNfvendaUpdtByNumeroSerieServ = new FbCommand())
                                            {
                                                #region Prepara o comando da SP_TRI_NFV_UPDT_BYNFNUMSERIE

                                                fbCommNfvendaUpdtByNumeroSerieServ.Connection = fbConnServ;

                                                //fbCommNfvendaUpdtByCooNumcaixaServ.CommandText = "SP_TRI_CUPOM_UPDT_BYCOONUMCAIX";

                                                //TODO: o que seria um COO e NUM_CAIXA do TB_CUPOM para a TB_NFVENDA?
                                                // Seria a NF_NUMERO e a NF_SERIE

                                                fbCommNfvendaUpdtByNumeroSerieServ.CommandText = "SP_TRI_NFV_UPDT_BYNFNUMSERIE";
                                                fbCommNfvendaUpdtByNumeroSerieServ.CommandType = CommandType.StoredProcedure;

                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_NATOPE", nfvendaCancelPdv.ID_NATOPE);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_VENDEDOR", nfvendaCancelPdv.IsID_VENDEDORNull() ? null : (short?)nfvendaCancelPdv.ID_VENDEDOR);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_CLIENTE", nfvendaCancelPdv.ID_CLIENTE);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pNF_NUMERO", nfvendaCancelPdv.NF_NUMERO);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pNF_SERIE", nfvendaCancelPdv.NF_SERIE);

                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pNF_MODELO", nfvendaCancelPdv.NF_MODELO);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pDT_EMISSAO", nfvendaCancelPdv.DT_EMISSAO);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pDT_SAIDA", nfvendaCancelPdv.IsDT_SAIDANull() ? null : (DateTime?)nfvendaCancelPdv.DT_SAIDA);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pHR_SAIDA", nfvendaCancelPdv.IsHR_SAIDANull() ? null : (TimeSpan?)nfvendaCancelPdv.HR_SAIDA);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pESPECIE", nfvendaCancelPdv.IsESPECIENull() ? null : nfvendaCancelPdv.ESPECIE);

                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pTIPO_FRETE", nfvendaCancelPdv.TIPO_FRETE);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pPES_LIQUID", nfvendaCancelPdv.IsPES_LIQUIDNull() ? null : (decimal?)nfvendaCancelPdv.PES_LIQUID);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pPES_BRUTO", nfvendaCancelPdv.IsPES_BRUTONull() ? null : (decimal?)nfvendaCancelPdv.PES_BRUTO);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pSTATUS", nfvendaCancelPdv.STATUS);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pENT_SAI", nfvendaCancelPdv.ENT_SAI);

                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_FMAPGTO", nfvendaCancelPdv.ID_FMAPGTO);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_PARCELA", nfvendaCancelPdv.ID_PARCELA);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pMARCA", nfvendaCancelPdv.IsMARCANull() ? null : nfvendaCancelPdv.MARCA);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pQTD_VOLUM", nfvendaCancelPdv.IsQTD_VOLUMNull() ? null : (decimal?)nfvendaCancelPdv.QTD_VOLUM);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pNUM_VOLUM", nfvendaCancelPdv.IsNUM_VOLUMNull() ? null : nfvendaCancelPdv.NUM_VOLUM);

                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pPROD_REV", nfvendaCancelPdv.IsPROD_REVNull() ? null : nfvendaCancelPdv.PROD_REV);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pSOMA_FRETE", nfvendaCancelPdv.IsSOMA_FRETENull() ? null : nfvendaCancelPdv.SOMA_FRETE);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pVLR_TROCO", nfvendaCancelPdv.IsVLR_TROCONull() ? null : (decimal?)nfvendaCancelPdv.VLR_TROCO);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pIND_PRES", nfvendaCancelPdv.IsIND_PRESNull() ? null : nfvendaCancelPdv.IND_PRES);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pIND_IE_DEST", nfvendaCancelPdv.IsIND_IE_DESTNull() ? null : nfvendaCancelPdv.IND_IE_DEST);

                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pDESCONTO_CONDICIONAL", nfvendaCancelPdv.DESCONTO_CONDICIONAL);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pINF_COMP_FIXA", nfvendaCancelPdv.IsINF_COMP_FIXANull() ? null : nfvendaCancelPdv.INF_COMP_FIXA);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pINF_COMP_EDIT", nfvendaCancelPdv.IsINF_COMP_EDITNull() ? null : nfvendaCancelPdv.INF_COMP_EDIT);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pENDERECO_ENTREGA", nfvendaCancelPdv.ENDERECO_ENTREGA);
                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pENVIO_API", nfvendaCancelPdv.IsENVIO_APINull() ? null : (DateTime?)nfvendaCancelPdv.ENVIO_API);

                                                fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pSYNCED", 2);

                                                #endregion Prepara o comando da SP_TRI_NFV_UPDT_BYNFNUMSERIE

                                                // Executa a sproc
                                                fbCommNfvendaUpdtByNumeroSerieServ.ExecuteScalar();
                                            }

                                            #endregion Indicar que a nfvenda foi synced depois de cancelado (Serv)

                                            #region Indicar que os itens da nfvenda foram cancelados (Serv) -- DESATIVADO

                                            ////TODO: parece que esse procedimento não é necessário em TB_NFV_ITEM

                                            //using (var fbCommNfvendaItemSetCancelByNumeroSerieServ = new FbCommand())
                                            //{
                                            //    #region Prepara o comando da SP_TRI_CUPOM_ITEM_SET_CANCEL

                                            //    fbCommNfvendaItemSetCancelByNumeroSerieServ.Connection = fbConnServ;

                                            //    fbCommNfvendaItemSetCancelByNumeroSerieServ.CommandText = "SP_TRI_CUPOM_ITEM_SET_CANCEL";
                                            //    fbCommNfvendaItemSetCancelByNumeroSerieServ.CommandType = CommandType.StoredProcedure;

                                            //    fbCommNfvendaItemSetCancelByNumeroSerieServ.Parameters.Add("@pCOO", cupomCancelPdv.COO);
                                            //    fbCommNfvendaItemSetCancelByNumeroSerieServ.Parameters.Add("@pNUM_CAIXA", cupomCancelPdv.NUM_CAIXA);

                                            //    #endregion Prepara o comando da SP_TRI_CUPOM_ITEM_SET_CANCEL

                                            //    // Executa a sproc
                                            //    fbCommNfvendaItemSetCancelByNumeroSerieServ.ExecuteScalar();
                                            //}

                                            #endregion Indicar que os itens da nfvenda foram cancelados (Serv) -- DESATIVADO

                                            #region Indicar que a nfvenda foi synced depois de cancelado (PDV)

                                            using (var fbCommNfvendaUnsyncedSetSynced = new FbCommand())
                                            {
                                                #region Prepara o comando da SP_TRI_NFVENDA_SETSYNCED

                                                fbCommNfvendaUnsyncedSetSynced.Connection = fbConnPdv;

                                                fbCommNfvendaUnsyncedSetSynced.CommandText = "SP_TRI_NFVENDA_SETSYNCED";
                                                fbCommNfvendaUnsyncedSetSynced.CommandType = CommandType.StoredProcedure;

                                                fbCommNfvendaUnsyncedSetSynced.Parameters.Add("@pIdNfvenda", nfvendaCancelPdv.ID_NFVENDA);
                                                fbCommNfvendaUnsyncedSetSynced.Parameters.Add("@pSynced", 2);

                                                #endregion Prepara o comando da SP_TRI_NFVENDA_SETSYNCED

                                                // Executa a sproc
                                                fbCommNfvendaUnsyncedSetSynced.ExecuteScalar();
                                            }
                                            //}
                                            #endregion Indicar que a nfvenda foi synced depois de cancelado (PDV)

                                            #region Desfazer vínculo de nfvenda com orçamento e setar status do orçamento para "SALVO"

                                            #region Verificar se a nfvenda foi cancelada: reativar o orçamento vinculado, se houver

                                            //TODO: adaptar vínculo do orçamento com NFVENDA

                                            //using (var taOrcaServ = new DataSets.FDBDataSetOrcamTableAdapters.TRI_PDV_ORCA_NFVENDA_RELTableAdapter())
                                            //{
                                            //    taOrcaServ.Connection = fbConnServ;

                                            //    audit("SINCCONTNETDB>> ", string.Format("(nfvenda cancelada depois de synced) taOrcaServ.SP_TRI_ORCA_REATIVAORCA({1}): {0}",
                                            //                        taOrcaServ.SP_TRI_ORCA_REATIVAORCA(nfvendaCancelPdv.ID_NFVENDA).Safestring(),
                                            //                        nfvendaCancelPdv.ID_NFVENDA.Safestring()));
                                            //}

                                            #endregion Verificar se a nfvenda foi cancelada: reativar o orçamento vinculado, se houver

                                            #endregion Desfazer vínculo de nfvenda com orçamento e setar status do orçamento para "SALVO"

                                            // Teste de transação:
                                            //int minibomba = 0;
                                            //decimal bomba = 100 / minibomba;

                                            //fbConnServ.Close();
                                            //fbConnPdv.Close();
                                        }
                                        // Finaliza a transação:
                                        transactionScopeNfvendaResync.Complete();
                                    }
                                    #endregion Iniciar o procedimento de cancelamento na retaguarda
                                }

                                log.Debug(string.Format("Lote {0} de nfvendas sincronizadas e canceladas processado!", intCountLoteNfvendaSyncedCancel.ToString()));

                                // Busca todas as nfvendas que foram synced e posteriormente canceladas (TIP_QUERY = 1)
                                // Lembrando que a sproc executada abaixo retorna até 200 registros por vez (lote).
                                tblNfvendaSyncedCancelPdv.Clear();
                                log.Debug("taNfvendaSyncedCancelPdv.FillByNfvendaSync(1): " + taNfvendaSyncedCancelPdv.FillByNfvendaSync(tblNfvendaSyncedCancelPdv, 1).ToString()); // já usa sproc
                            }
                        }
                        #region Manipular Exception
                        catch (Exception ex)
                        {
                            log.Error("Erro ao sincronizar (synced e cancelado depois):", ex);
                            GravarErroSync("Erro ao sincronizar", tblNfvendaSyncedCancelPdv, ex);
                            throw ex;
                        }
                        #endregion Manipular Exception
                        #region Limpeza dos objetos Disposable
                        finally
                        {
                            if (taNfvendaSyncedCancelPdv != null) { taNfvendaSyncedCancelPdv.Dispose(); }
                            if (tblNfvendaSyncedCancelPdv != null) { tblNfvendaSyncedCancelPdv.Dispose(); }
                            if (taEstProdutoServ != null) { taEstProdutoServ.Dispose(); }
                            if (taEstProdutoPdv != null) { taEstProdutoPdv.Dispose(); }
                        }
                        #endregion Limpeza dos objetos Disposable
                    }
                    #endregion Nfvendas sincronizadas e posteriormente canceladas
                }
            }
            catch (Exception e)
            {
                log.Error("Erro de sincronização", e);
                log.Error("Erro interno", e.InnerException);
                throw;
            }

        }

        #endregion

        #region Fechamentos de caixa PDV-Serv
        private void Sync_TB_MOVTODIARIO(EnmTipoSync tipoSync)
        {
            if (tipoSync == EnmTipoSync.fechamentos || tipoSync == EnmTipoSync.tudo)
            {
                #region Abre transação (fech. caixa)

                var taMovtoUnsynced = new TB_MOVDIARIOTableAdapter();
                taMovtoUnsynced.Connection.ConnectionString = _strConnContingency;
                var tblMovtoUnsynced = new FDBDataSet.TB_MOVDIARIODataTable();

                var taMovtoServ = new TB_MOVDIARIOTableAdapter();
                taMovtoServ.Connection.ConnectionString = _strConnNetwork;

                int intCounLoteFechCaixa = 0;

                #endregion Abre transação (fech. caixa)

                try
                {
                    log.Debug("taMovtoUnsynced.FillByMovtoUnsynced(): " + taMovtoUnsynced.FillByMovtoUnsynced(tblMovtoUnsynced).ToString()); // já usa sproc

                    // Executa enquanto houver movimentos pra sincronizar:
                    while (tblMovtoUnsynced != null && tblMovtoUnsynced.Rows.Count > 0)
                    {
                        intCounLoteFechCaixa++;

                        #region Percorre por cada fechamento de caixa não sincronizado

                        foreach (FDBDataSet.TB_MOVDIARIORow movtoUnsynced in tblMovtoUnsynced)
                        {
                            using (var transactionScopeFechCaixa = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                            {
                                movtoUnsynced.SYNCED = 1;
                                taMovtoServ.SP_TRI_MOVTO_SYNC_INSERT((movtoUnsynced.IsDT_MOVTONull() ? null : (DateTime?)movtoUnsynced.DT_MOVTO),
                                                                     (movtoUnsynced.IsHR_MOVTONull() ? null : (TimeSpan?)movtoUnsynced.HR_MOVTO),
                                                                     (movtoUnsynced.IsHISTORICONull() ? null : movtoUnsynced.HISTORICO),
                                                                     (movtoUnsynced.IsTIP_MOVTONull() ? null : movtoUnsynced.TIP_MOVTO),
                                                                     (movtoUnsynced.IsVLR_MOVTONull() ? null : (decimal?)movtoUnsynced.VLR_MOVTO),
                                                                     (movtoUnsynced.IsID_CTAPLANull() ? null : (short?)movtoUnsynced.ID_CTAPLA),
                                                                     movtoUnsynced.SYNCED);

                                #region Indicar que o fechamento de caixa foi sincronizado

                                taMovtoUnsynced.SP_TRI_MOVTOSETSYNCED(movtoUnsynced.ID_MOVTO, movtoUnsynced.SYNCED);

                                #endregion Indicar que o fechamento de caixa foi sincronizado

                                // Finaliza a transação:
                                transactionScopeFechCaixa.Complete();
                            }
                        }

                        log.Debug(string.Format("Lote {0} de fechamentos processado!", intCounLoteFechCaixa.ToString()));

                        #endregion Percorre por cada fechamento de caixa não sincronizado

                        #region Prepara o próximo lote

                        tblMovtoUnsynced.Clear();
                        log.Debug("taMovtoUnsynced.FillByMovtoUnsynced(): " + taMovtoUnsynced.FillByMovtoUnsynced(tblMovtoUnsynced).ToString()); // já usa sproc

                        #endregion Prepara o próximo lote
                    }
                }
                #region Manipular Exception
                catch (Exception ex)
                {
                    log.Error("Erro ao sincronizar fechamentos de caixa", ex);
                    GravarErroSync("", tblMovtoUnsynced, ex);
                    throw ex;
                }
                #endregion Manipular Exception
                #region Limpeza dos objetos Disposable
                finally
                {
                    if (taMovtoUnsynced != null) { taMovtoUnsynced.Dispose(); }
                    if (tblMovtoUnsynced != null) { tblMovtoUnsynced.Dispose(); }
                    if (taMovtoServ != null) { taMovtoServ.Dispose(); }
                }
                #endregion Limpeza dos objetos Disposable
            }

        }

        private void Sync_TRI_PDV_FECHAMENTOS(EnmTipoSync tipoSync)
        {
            if (tipoSync == EnmTipoSync.fechamentos || tipoSync == EnmTipoSync.tudo)
            {
                #region Abre transação (fech. caixa)

                var taFechamentosUnsynced = new TRI_PDV_FECHAMENTOSTableAdapter();
                taFechamentosUnsynced.Connection.ConnectionString = _strConnContingency;
                var tblFechamentosUnsynced = new FDBDataSet.TRI_PDV_FECHAMENTOSDataTable();

                var taFechamentosServ = new TRI_PDV_FECHAMENTOSTableAdapter();
                taFechamentosServ.Connection.ConnectionString = _strConnNetwork;

                int intCounLoteFechCaixa = 0;

                #endregion Abre transação (fech. caixa)

                try
                {
                    log.Debug("taFechamentosUnsynced.FillByMovtoUnsynced(): " + taFechamentosUnsynced.FillByMovtoUnsynced(tblFechamentosUnsynced).ToString()); // já usa sproc

                    // Executa enquanto houver movimentos pra sincronizar:
                    while (tblFechamentosUnsynced != null && tblFechamentosUnsynced.Rows.Count > 0)
                    {
                        intCounLoteFechCaixa++;

                        #region Percorre por cada fechamento de caixa não sincronizado

                        foreach (FDBDataSet.TRI_PDV_FECHAMENTOSRow fechamentosUnsynced in tblFechamentosUnsynced)
                        {
                            using (var transactionScopeFechCaixa = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                            {
                                fechamentosUnsynced.SYNCED = 1;
                                taFechamentosServ.SP_TRI_FECH_SYNC_INSERT((fechamentosUnsynced.IsDINNull() ? null : (decimal?)fechamentosUnsynced.DIN),
                                                                          (fechamentosUnsynced.IsCHEQUENull() ? null : (decimal?)fechamentosUnsynced.CHEQUE),
                                                                          (fechamentosUnsynced.IsCREDITONull() ? null : (decimal?)fechamentosUnsynced.CREDITO),
                                                                          (fechamentosUnsynced.IsDEBITONull() ? null : (decimal?)fechamentosUnsynced.DEBITO),
                                                                          (fechamentosUnsynced.IsLOJANull() ? null : (decimal?)fechamentosUnsynced.LOJA),
                                                                          (fechamentosUnsynced.IsALIMENTACAONull() ? null : (decimal?)fechamentosUnsynced.ALIMENTACAO),
                                                                          (fechamentosUnsynced.IsREFEICAONull() ? null : (decimal?)fechamentosUnsynced.REFEICAO),
                                                                          (fechamentosUnsynced.IsPRESENTENull() ? null : (decimal?)fechamentosUnsynced.PRESENTE),
                                                                          (fechamentosUnsynced.IsCOMBUSTIVELNull() ? null : (decimal?)fechamentosUnsynced.COMBUSTIVEL),
                                                                          (fechamentosUnsynced.IsOUTROSNull() ? null : (decimal?)fechamentosUnsynced.OUTROS),
                                                                          (fechamentosUnsynced.IsEXTRA_1Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_1),
                                                                          (fechamentosUnsynced.IsEXTRA_2Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_2),
                                                                          (fechamentosUnsynced.IsEXTRA_3Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_3),
                                                                          (fechamentosUnsynced.IsEXTRA_4Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_4),
                                                                          (fechamentosUnsynced.IsEXTRA_5Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_5),
                                                                          (fechamentosUnsynced.IsEXTRA_6Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_6),
                                                                          (fechamentosUnsynced.IsEXTRA_7Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_7),
                                                                          (fechamentosUnsynced.IsEXTRA_8Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_8),
                                                                          (fechamentosUnsynced.IsEXTRA_9Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_9),
                                                                          (fechamentosUnsynced.IsEXTRA_10Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_10),
                                                                          fechamentosUnsynced.OPERADOR,
                                                                          fechamentosUnsynced.ID_CAIXA,
                                                                          fechamentosUnsynced.FECHADO,
                                                                          (fechamentosUnsynced.IsSANGRIASNull() ? null : (decimal?)fechamentosUnsynced.SANGRIAS),
                                                                          (fechamentosUnsynced.IsSUPRIMENTOSNull() ? null : (decimal?)fechamentosUnsynced.SUPRIMENTOS),
                                                                          (fechamentosUnsynced.IsTROCASNull() ? null : (decimal?)fechamentosUnsynced.TROCAS),
                                                                          fechamentosUnsynced.SYNCED);

                                #region Indicar que o fechamento de caixa foi sincronizado

                                taFechamentosUnsynced.SP_TRI_FECH_SETSYNCED(fechamentosUnsynced.ID_CAIXA, fechamentosUnsynced.FECHADO, 1);

                                #endregion Indicar que o fechamento de caixa foi sincronizado

                                // Finaliza a transação:
                                transactionScopeFechCaixa.Complete();
                            }
                        }

                        log.Debug(string.Format("Lote {0} de fechamentos processado!", intCounLoteFechCaixa.ToString()));

                        #endregion Percorre por cada fechamento de caixa não sincronizado

                        #region Prepara o próximo lote

                        tblFechamentosUnsynced.Clear();
                        log.Debug("taFechamentosUnsynced.FillByMovtoUnsynced(): " + taFechamentosUnsynced.FillByMovtoUnsynced(tblFechamentosUnsynced).ToString()); // já usa sproc

                        #endregion Prepara o próximo lote
                    }
                }
                #region Manipular Exception
                catch (Exception ex)
                {
                    log.Error("Erro ao sincronizar fechamentos de caixa:", ex);
                    GravarErroSync("", tblFechamentosUnsynced, ex);
                    throw ex;
                }
                #endregion Manipular Exception
                #region Limpeza dos objetos Disposable
                finally
                {
                    taFechamentosUnsynced?.Dispose();
                    tblFechamentosUnsynced?.Dispose();
                    taFechamentosServ?.Dispose();
                }
                #endregion Limpeza dos objetos Disposable
            }
        }
        #endregion

        /// <summary>
        /// Mantém os bancos do servidor e do PDV sincronizados.
        /// Atualiza os cadastros localmente baseado nas alterações feitas no servidor.
        /// Mantém o servidor ciente das operações do PDV.
        /// </summary>
        /// <param name="tipoSync">Define se a sync será parcial ou total</param>
        public object SincronizarContingencyNetworkDbs(EnmTipoSync tipoSync, int segundosTolerancia, EnmTipoSync tipoSyncCtrlS = EnmTipoSync.dummy)
        {
            /// As sincronizações estão em ordem de dependências.
            /// Por exemplo: Emitente precisa de Cidade e Ramo,
            /// Estoque precisa de Unidade de Medida, etc.

            #region Variáveis

            DateTime? dtUltimaSyncPdv;

            var shtNumCaixa = (short)NO_CAIXA;

            DateTime dtSyncInicio = DateTime.Now;
            string strSyncTipo;
            DateTime dtSyncFim;

            #endregion Variáveis

            List<ComboBoxBindingDTO_Produto_Sync> retornoProdutosAlterados = null;

            #region Se o banco da retaguarda não estiver disponível, não será possível iniciar a sincronização
            funcoesClass _funcoes = new funcoesClass();
            bool? conexaoServidorOk = _funcoes.TestaConexaoComServidor(SERVERNAME, SERVERCATALOG, FBTIMEOUT);
            if (conexaoServidorOk == false || conexaoServidorOk is null) { return null; }
            #endregion Se o banco da retaguarda não estiver disponível, não será possível iniciar a sincronização

            #region Buscar o timestamp da última sync

            // TRI_PDV_SETUP
            {
                using (var taSetupPdv = new TRI_PDV_SETUPTableAdapter())
                {
                    taSetupPdv.Connection.ConnectionString = _strConnContingency;
                    dtUltimaSyncPdv = (DateTime)taSetupPdv.GetUltimaSync();
                }
            }

            #endregion Buscar o timestamp da última sync

            #region Serv -> PDV

            #region Cadastros

            if (tipoSync == EnmTipoSync.cadastros || tipoSync == EnmTipoSync.tudo)
            {
                using var dtAuxSyncPendentes = new FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable();
                using var dtAuxSyncDeletesPendentes = new FDBDataSetOperSeed.TRI_PDV_AUX_SYNCDataTable();
                using var taAuxSyncServ = new TRI_PDV_AUX_SYNCTableAdapter();
                using var fbConnServ = new FbConnection(_strConnNetwork);
                using var fbConnPdv = new FbConnection(_strConnContingency);
                log.Debug("Iniciando sincronização tipo cadastros/tudo");
                taAuxSyncServ.Connection = fbConnServ;// taAuxSyncServ.Connection.ConnectionString = _strConnNetwork;
                log.Debug($"Conexão: {taAuxSyncServ.Connection.ConnectionString}");
                try
                {
                    taAuxSyncServ.FillByNoCaixa(dtAuxSyncPendentes, (short)_intNoCaixa); // passou a usar sproc
                    log.Debug($"taAuxSyncServ.FillByNoCaixa({_intNoCaixa}) retornou {dtAuxSyncPendentes.Rows.Count} linhas");
                }
                catch (Exception ex)
                {
                    GravarErroSync("Fill dtAuxSyncPendentes by NO_CAIXA", dtAuxSyncPendentes, ex);
                    throw ex;
                }

                /*
                #region Impressoras fiscais

                /// TB_IFS
                {
                    //TODO: Verificar todos os procedimentos de atualização de registros no servidor que não acontecem no Clipp
                    // e que usam o esquema de sync usando o trigger de data de update.
                    using (var tblIfsServ = new FDBDataSetOperSeed.TB_IFSDataTable())
                    using (var tblIfsPdv = new FDBDataSetOperSeed.TB_IFSDataTable())
                    {
                        try
                        {
                            #region Adiciona o número do caixa ao TB_IFS no servidor

                            //bool blnForcarSyncCompleta = false;

                            using (var taIfsServ = new TB_IFSTableAdapter())
                            {
                                taIfsServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                var existe = (short)taIfsServ.SP_TRI_IFS_GETBY_CAIXA((short)_intNoCaixa);

                                if (existe == 0)
                                {
                                    taIfsServ.SP_TRI_IFS_UPSERT(null, _intNoCaixa.ToString(), "000", "", "001", "", "", "", "", DateTime.Today, null, "N", "", DateTime.Today, DateTime.Now, "", "", "", null, "", DateTime.Now);
                                    //blnForcarSyncCompleta = true;
                                }
                            }

                            #endregion Adiciona o número do caixa ao TB_IFS no servidor

                            #region Sync padrão

                            if (dtUltimaSyncPdv is null)
                            {
                                #region Única sync

                                using (var taServ = new TB_IFSTableAdapter())
                                using (var taPdv = new TB_IFSTableAdapter())
                                {
                                    taServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                                    taServ.Fill(tblIfsServ);

                                    taPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                                    taPdv.Fill(tblIfsPdv);

                                    using (var changeReader = new DataTableReader(tblIfsServ))
                                        tblIfsPdv.Load(changeReader, LoadOption.Upsert);

                                    // Salva dados do Serv para o PDV:
                                    taPdv.Update(tblIfsPdv);

                                    tblIfsPdv.AcceptChanges();
                                }

                                #endregion Única sync
                            }
                            else
                            {
                                #region Sync de cadastros novos ou atualizados

                                // Buscar apenas os cadastros cuja data de inserção/alteração
                                // for maior que a data da última sync.

                                #region AUX_SYNC

                                int intRetornoUpsert = 0;

                                //using (var fbConnServ = new FbConnection(_strConnNetwork))
                                using (var dtAuxSyncIfs = new DataTable())
                                {
                                    fbConnServ.Open();

                                    // Buscar os registros da tabela auxiliar de sincronização (TRI_PDV_AUX_SYNC)
                                    using (FbCommand fbCommAuxSyncIfs = new FbCommand())
                                    {
                                        fbCommAuxSyncIfs.CommandText = $"SELECT SM_REG, OPERACAO, NO_CAIXA FROM TRI_PDV_AUX_SYNC WHERE TABELA = 'TB_IFS' AND (NO_CAIXA = { shtNumCaixa } OR NO_CAIXA = 0) ORDER BY SEQ;";
                                        fbCommAuxSyncIfs.CommandType = CommandType.Text;
                                        fbCommAuxSyncIfs.Connection = fbConnServ;

                                        using (var readerAuxSyncIfs = fbCommAuxSyncIfs.ExecuteReader())
                                        {
                                            dtAuxSyncIfs.Clear();
                                            dtAuxSyncIfs.Load(readerAuxSyncIfs);
                                        }
                                    }

                                    for (int i = 0; i < dtAuxSyncIfs.Rows.Count; i++)
                                    {
                                        var idIfs = dtAuxSyncIfs.Rows[i]["SM_REG"].Safeshort();
                                        var operacao = dtAuxSyncIfs.Rows[i]["OPERACAO"].Safestring();
                                        var NO_CAIXA = dtAuxSyncIfs.Rows[i]["NO_CAIXA"].Safeshort();

                                        // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                        if (operacao.Equals("I") || operacao.Equals("U"))
                                        {
                                            // Buscar o registro para executar as operações "Insert" ou "Update"

                                            using (var taIfsServ = new TB_IFSTableAdapter())
                                            {
                                                taIfsServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                                taIfsServ.FillById(tblIfsServ, idIfs).ToString();

                                                if (tblIfsServ != null && tblIfsServ.Rows.Count > 0)
                                                {
                                                    using (var taIfsPdv = new TB_IFSTableAdapter())
                                                    {
                                                        taIfsPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                        foreach (FDBDataSetOperSeed.TB_IFSRow ifsServ in tblIfsServ)
                                                        {
                                                            intRetornoUpsert = (int)taIfsPdv.SP_TRI_IFS_UPSERT(ifsServ.ID_IFS,
                                                                                       (ifsServ.IsCAIXANull() ? null : ifsServ.CAIXA),
                                                                                       (ifsServ.IsLOJANull() ? null : ifsServ.LOJA),
                                                                                       (ifsServ.IsFABRICACAONull() ? null : ifsServ.FABRICACAO),
                                                                                       (ifsServ.IsUSUARIONull() ? null : ifsServ.USUARIO),
                                                                                       (ifsServ.IsMARCANull() ? null : ifsServ.MARCA),
                                                                                       (ifsServ.IsMFNull() ? null : ifsServ.MF),
                                                                                       (ifsServ.IsMODELONull() ? null : ifsServ.MODELO),
                                                                                       (ifsServ.IsTIPONull() ? null : ifsServ.TIPO),
                                                                                       (ifsServ.IsDATA_ONNull() ? null : (DateTime?)ifsServ.DATA_ON),
                                                                                       (ifsServ.IsDATA_OFFNull() ? null : (DateTime?)ifsServ.DATA_OFF),
                                                                                       (ifsServ.IsATIVONull() ? null : ifsServ.ATIVO),
                                                                                       (ifsServ.IsISS_RATEIONull() ? null : ifsServ.ISS_RATEIO),
                                                                                       (ifsServ.IsSB_DATAINNull() ? null : (DateTime?)ifsServ.SB_DATAIN),
                                                                                       (ifsServ.IsSB_HORAINNull() ? null : (TimeSpan?)ifsServ.SB_HORAIN),
                                                                                       (ifsServ.IsSB_VERSAONull() ? null : ifsServ.SB_VERSAO),
                                                                                       (ifsServ.IsCHAVENull() ? null : ifsServ.CHAVE),
                                                                                       (ifsServ.IsCOD_NACNull() ? null : ifsServ.COD_NAC),
                                                                                       (ifsServ.IsDATA_TEMPNull() ? null : (DateTime?)ifsServ.DATA_TEMP),
                                                                                       (ifsServ.IsNUM_CREDENCIAMENTONull() ? null : ifsServ.NUM_CREDENCIAMENTO),
                                                                                       DateTime.Now);

                                                            // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                            if (intRetornoUpsert.Equals(1))
                                                            {
                                                                ConfirmarAuxSync(-1,
                                                                             "TB_IFS",
                                                                             operacao,
                                                                             NO_CAIXA,// shtNumCaixa,
                                                                             null,
                                                                             idIfs,
                                                                             null);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // O item não foi encontrado no servidor.
                                                    // Pode ter sido deletado.
                                                    // Deve constar essa operação em dtAuxSync.
                                                    // Caso contrário, estourar exception.

                                                    DataRow[] deletesPendentesIfs = dtAuxSyncIfs.Select($"SM_REG = '{idIfs}' AND OPERACAO = 'D'");

                                                    if (deletesPendentesIfs.Length > 0)
                                                    {
                                                        foreach (var deletePendenteIfs in deletesPendentesIfs)
                                                        {
                                                            // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                            dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_IFS", "D",
                                                                NO_CAIXA,//shtNumCaixa, 
                                                                null, null, idIfs, null);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Ops....
                                                        // Item não encontrado no servidor e não foi deletado?
                                                        // Estourar exception.
                                                        throw new DataException($"Erro não esperado: produto (TB_IFS) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idIfs}");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Não é uma operação "padrão"

                                            switch (operacao)
                                            {
                                                case "D":
                                                    {
                                                        // Não dá pra deletar agora por causa das constraints (FK).
                                                        // Adicionar numa lista e deletar depois, na ordem correta.

                                                        // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                        DataRow[] deletesPendentesIfs = dtAuxSyncDeletesPendentes.Select($"SM_REG = '{idIfs}' AND TABELA = 'TB_IFS' AND OPERACAO = 'D'");

                                                        if (deletesPendentesIfs.Length <= 0)
                                                        {
                                                            // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                            dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TB_IFS", "D",
                                                                NO_CAIXA,//shtNumCaixa, 
                                                                null, null, idIfs, null);
                                                        }

                                                        break;
                                                    }
                                                default:
                                                    throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                                    //break;
                                            }
                                        }
                                    }
                                }

                                #endregion AUX_SYNC

                                #endregion Sync de cadastros novos ou atualizados
                            }

                            #endregion Sync padrão
                        }
                        catch (Exception ex)
                        {
                            //gravarMensagemErro("Erro ao sincronizar(TB_IFS): \n\n" + RetornarMensagemErro(ex, true));
                            GravarErroSync("Impressoras Fiscais(PDV)", tblIfsPdv, ex);
                            GravarErroSync("Impressoras Fiscais(SERV)", tblIfsServ, ex);
                            throw ex;
                        }
                    }
                }
                #endregion Impressoras fiscais
                */

                // Inicia a sincronização de cadastros apenas se houver registro indicado na tabela auxiliar (a TB_IFS é exceção):
                if (dtAuxSyncPendentes != null && dtAuxSyncPendentes.Rows.Count > 0)
                {
                    try
                    {
                        Sync_Emitente(fbConnServ, fbConnPdv, dtAuxSyncPendentes);
                        log.Debug("Emitentes sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar emitentes", ex);
                        throw new SynchException("Erro ao sincronizar Emitentes", ex);
                    }
                    try
                    {
                        Sync_TB_TAXA_UF(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes);
                        log.Debug("Taxa UF sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar Taxa UF", ex);
                        throw new SynchException("Erro ao sincronizar Taxa UF", ex);
                    }
                    try
                    {
                        Sync_TB_CFOP_SIS(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("CFOP sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar CFOP", ex);
                        throw new SynchException("Erro ao sincronizar CFOP", ex);
                    }
                    try
                    {
                        Sync_TB_NAT_OPERACAO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("NatOper sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar NatOper", ex);
                        throw new SynchException("Erro ao sincronizar NatOper", ex);
                    }
                    try
                    {
                        Sync_TB_EST_GRUPO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("EST_GRUPO sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar EST_GRUPO", ex);
                        throw new SynchException("Erro ao sincronizar EST_GRUPO", ex);
                    }
                    try
                    {
                        Sync_TB_FORNECEDOR(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("Fornecedores sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar fornecedores", ex);
                        throw new SynchException("Erro ao sincronizar Fornecedores", ex);
                    }
                    try
                    {
                        Sync_TB_UNI_MEDIDA(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("UniMedida sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar Unimedida", ex);
                        throw new SynchException("Erro ao sincronizar UniMedida", ex);
                    }
                    try
                    {
                        Sync_TB_ESTOQUE(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa, retornoProdutosAlterados);
                        log.Debug("Estoque sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar estoque", ex);
                        throw new SynchException("Erro ao sincronizar Estoque", ex);
                    }
                    try
                    {
                        Sync_TB_EST_IDENTIFICADOR(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("EstIdent sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar EstIdent", ex);
                        throw new SynchException("Erro ao sincronizar EstIdent", ex);
                    }
                    try
                    {
                        Sync_TB_EST_PRODUTO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa, ref retornoProdutosAlterados);
                        log.Debug("Produtos sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar Produtos", ex);
                        throw new SynchException("Erro ao sincronizar Produtos", ex);
                    }
                    try
                    {
                        Sync_TB_FUNCIONARIO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("Funcionarios sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar Funcionarios", ex);
                        throw new SynchException("Erro ao sincronizar Funcionarios", ex);
                    }
                    try
                    {
                        Sync_TB_FUNC_PAPEL(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("FuncionarioPapel sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar FuncionarioPapel", ex);
                        throw new SynchException("Erro ao sincronizar FuncionarioPapel", ex);
                    }
                    try
                    {
                        Sync_TB_CLIENTE(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("Clientes sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar Clientes", ex);
                        throw new SynchException("Erro ao sincronizar Clientes", ex);
                    }
                    try
                    {
                        Sync_TB_FORMA_PAGTO_SIS(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("FormaPagtoSis sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar FormaPagtoSis", ex);
                        throw new SynchException("Erro ao sincronizar FormaPagtoSis", ex);
                    }
                    try
                    {
                        Sync_TB_PARCELAMENTO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("Parcelamentos sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar Parcelamentos", ex);
                        throw new SynchException("Erro ao sincronizar Parcelamentos", ex);
                    }
                    try
                    {
                        Sync_TB_FORMA_PAGTO_NFCE(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("FormaPagtoNFCE sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar FormaPagtoNFCE", ex);
                        throw new SynchException("Erro ao sincronizar FormaPagtoNFCE", ex);
                    }

                    try
                    {
                        Sync_TRI_PDV_USERS(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("Sync_TRI_PDV_USERS sincronizados");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao sincronizar Sync_TRI_PDV_USERS", ex);
                        throw new SynchException("Erro ao sincronizar Sync_TRI_PDV_USERS", ex);
                    }
                    try
                    {
                        Sync_TB_CARTAO_ADMINISTRADORA(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        log.Debug("Sync_TB_CARTAO_ADMINISTRADORA sincronizados");
                    }
                    catch(Exception ex)
                    {
                        log.Error("Falha ao sincronizar Sync_TB_CARTAO_ADMINISTRADORA", ex);
                        throw new SynchException("Erro ao sincronizar Sync_TB_CARTAO_ADMINISTRADORA", ex);
                    }
                    #region Função Desativada

                    //DESATIVADO, TB_FORMA_PAGTO_NFCE É USADA, AO INVÉS 
                    #region TRI_PDV_METODOS

                    //{
                    //    using (var tblTriMetodosPdv = new FDBDataSet.TRI_PDV_METODOSDataTable())
                    //    using (var tblTriMetodosServ = new FDBDataSet.TRI_PDV_METODOSDataTable())
                    //    {
                    //        try
                    //        {
                    //            if (dtUltimaSyncPdv is null)
                    //            {
                    //                #region Única sync

                    //                using (var taTriMetodosServidor = new TRI_PDV_METODOSTableAdapter())
                    //                using (var taTriMetodosPdv = new TRI_PDV_METODOSTableAdapter())
                    //                {
                    //                    taTriMetodosServidor.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                    //                    taTriMetodosServidor.Fill(tblTriMetodosServ);

                    //                    taTriMetodosPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                    //                    taTriMetodosPdv.Fill(tblTriMetodosPdv);

                    //                    using (var changeReader = new DataTableReader(tblTriMetodosServ))
                    //                        tblTriMetodosPdv.Load(changeReader, LoadOption.Upsert);

                    //                    // Salva dados do Serv para o PDV:
                    //                    taTriMetodosPdv.Update(tblTriMetodosPdv);

                    //                    tblTriMetodosPdv.AcceptChanges();
                    //                }

                    //                #endregion Única sync
                    //            }
                    //            else
                    //            {
                    //                #region Sync de cadastros novos ou atualizados

                    //                #region AUX_SYNC

                    //                int intRowsAffected = 0;

                    //                {
                    //                    DataRow[] pendentesTriMetodos = dtAuxSyncPendentes.Select($"TABELA = 'TRI_PDV_METODOS'");

                    //                    for (int i = 0; i < pendentesTriMetodos.Length; i++)
                    //                    {
                    //                        var idPagamento = pendentesTriMetodos[i]["ID_REG"].Safeint();
                    //                        var operacao = pendentesTriMetodos[i]["OPERACAO"].Safestring();

                    //                        // Verificar o que deve ser feito com o registro (insert, update ou delete)
                    //                        if (operacao.Equals("I") || operacao.Equals("U"))
                    //                        {
                    //                            // Buscar o registro para executar as operações "Insert" ou "Update"

                    //                            using (var taTriMetodosServ = new TRI_PDV_METODOSTableAdapter())
                    //                            {
                    //                                taTriMetodosServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                    //                                taTriMetodosServ.FillById(tblTriMetodosServ, idPagamento);

                    //                                if (tblTriMetodosServ != null && tblTriMetodosServ.Rows.Count > 0)
                    //                                {
                    //                                    using (var taTriMetodosPdv = new TRI_PDV_METODOSTableAdapter())
                    //                                    {
                    //                                        taTriMetodosPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                    //                                        foreach (FDBDataSet.TRI_PDV_METODOSRow triMetodosServ in tblTriMetodosServ)
                    //                                        {
                    //                                            intRowsAffected = (int)taTriMetodosPdv.SP_TRI_METODOS_UPSERT(triMetodosServ.ID_PAGAMENTO,
                    //                                                                                                         triMetodosServ.IsDESCRICAONull() ? null : triMetodosServ.DESCRICAO,
                    //                                                                                                         triMetodosServ.IsDIASNull() ? null : (int?)triMetodosServ.DIAS,
                    //                                                                                                         triMetodosServ.METODO,
                    //                                                                                                         triMetodosServ.PGTOCFE,
                    //                                                                                                         triMetodosServ.ATIVO);

                    //                                            // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.

                    //                                            if (intRowsAffected.Equals(1))
                    //                                            {
                    //                                                ConfirmarAuxSync(idPagamento,
                    //                                                                 "TRI_PDV_METODOS",
                    //                                                                 operacao,
                    //                                                                 shtNumCaixa);
                    //                                            }
                    //                                        }
                    //                                    }
                    //                                }
                    //                                else
                    //                                {
                    //                                    #region Não há delete para TRI_PDV_METODOS

                    //                                    //// O item não foi encontrado no servidor.
                    //                                    //// Pode ter sido deletado.
                    //                                    //// Deve constar essa operação em dtAuxSync.
                    //                                    //// Caso contrário, estourar exception.

                    //                                    //DataRow[] deletesPendentesTriMetodos = dtAuxSyncTriMetodos.Select($"ID_REG = {idPagamento} AND OPERACAO = 'D'");

                    //                                    //if (deletesPendentesTriMetodos.Length > 0)
                    //                                    //{
                    //                                    //    foreach (var deletePendenteTriMetodos in deletesPendentesTriMetodos)
                    //                                    //    {
                    //                                    //        dtAuxSyncDeletesPendentes.Rows.Add(0, idPagamento, "TRI_PDV_METODOS", "D", shtNumCaixa);
                    //                                    //    }
                    //                                    //}
                    //                                    //else
                    //                                    //{
                    //                                    //    // Ops....
                    //                                    //    // Item não encontrado no servidor e não foi deletado?
                    //                                    //    // Estourar exception.
                    //                                    //    throw new DataException($"Erro não esperado: item (TRI_PDV_METODOS) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idPagamento}");
                    //                                    //}

                    //                                    #endregion Não há delete para TRI_PDV_METODOS
                    //                                }
                    //                            }
                    //                        }
                    //                        else
                    //                        {
                    //                            // Não é uma operação "padrão"

                    //                            #region Não há delete para TRI_PDV_METODOS

                    //                            // DEU RUIM

                    //                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido (TRI_PDV_METODOS) { operacao }");

                    //                            //switch (operacao)
                    //                            //{
                    //                            //    case "D":
                    //                            //        {
                    //                            //            // Não dá pra deletar agora por causa das constraints (FK).
                    //                            //            // Adicionar numa lista e deletar depois, na ordem correta.

                    //                            //            // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                    //                            //            DataRow[] deletesPendentesTriMetodos = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idPagamento} AND TABELA = 'TRI_PDV_METODOS' AND OPERACAO = 'D'");

                    //                            //            if (deletesPendentesTriMetodos.Length <= 0)
                    //                            //            {
                    //                            //                dtAuxSyncDeletesPendentes.Rows.Add(0, idPagamento, "TRI_PDV_METODOS", "D", shtNumCaixa);
                    //                            //            }

                    //                            //            break;
                    //                            //        }
                    //                            //    default:
                    //                            //        throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                    //                            //        //break;
                    //                            //}

                    //                            #endregion Não há delete para TRI_PDV_METODOS
                    //                        }
                    //                    }
                    //                }

                    //                #endregion AUX_SYNC

                    //                #endregion Sync de cadastros novos ou atualizados
                    //            }
                    //        }
                    //        catch (NotImplementedException niex)
                    //        {
                    //            gravarMensagemErro(RetornarMensagemErro(niex, true));
                    //            throw niex;
                    //        }
                    //        catch (DataException dex)
                    //        {
                    //            gravarMensagemErro(RetornarMensagemErro(dex, true));
                    //            throw dex;
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                    //            GravarErroSync("Métodos(PDV)", tblTriMetodosPdv, ex);
                    //            GravarErroSync("Métodos(SERV)", tblTriMetodosServ, ex);
                    //            throw ex;
                    //        }
                    //    }
                    //}

                    #endregion Métodos de pagamento PDV

                    // DESATIVADO, NÃO É NECESSÁRIO
                    #region Relação FMAPAGTO SIS / MetodoPagto PDV

                    //TODO: Não é necessário
                    // TRI_PDV_REL_METODO_PAGTO
                    /*{
                        var taServidor = new TRI_PDV_REL_METODO_PAGTOTableAdapter();
                        var tblServidor = new FDBDataSetOperSeed.TRI_PDV_REL_METODO_PAGTODataTable();
                        var taPdv = new TRI_PDV_REL_METODO_PAGTOTableAdapter();
                        var tblPdv = new FDBDataSetOperSeed.TRI_PDV_REL_METODO_PAGTODataTable();
                        try
                        {
                            taServidor.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                            audit("SINCCONTNETDB>> " + "TRI_PDV_REL_METODO_PAGTO Serv.Fill(): " + taServidor.Fill(tblServidor).ToString());

                            taPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                            audit("SINCCONTNETDB>> " + "TRI_PDV_REL_METODO_PAGTO Pdv.Fill(): " + taPdv.Fill(tblPdv).ToString());

                            using (var changeReader = new DataTableReader(tblServidor))
                                tblPdv.Load(changeReader, LoadOption.Upsert);

                            // Salva dados do Serv para o PDV:
                            //audit("SINCCONTNETDB>> " + "Saída do Update de relação FMAPAGTO SIS / Métodos de pagto PDV: " + taPdv.Update(tblPdv).ToString());
                            foreach (FDBDataSetOperSeed.TRI_PDV_REL_METODO_PAGTORow item in tblPdv)
                            {
                                verbose("SINCCONTNETDB>> " + "Saída do Update de relação FMAPAGTO SIS / Métodos de pagto PDV: " + taPdv.SP_TRI_REL_METD_PAGTO_UPDINST(item.ID_PAGAMENTO, item.ID_FMAPGTO).ToString());
                            }

                            tblPdv.AcceptChanges();
                        }
                        catch (Exception ex)
                        {
                            //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                            gravarErroSync("Relação FMAPAGTO SIS / MetodoPagto PDV(PDV)", tblPdv, ex);
                            gravarErroSync("Relação FMAPAGTO SIS / MetodoPagto PDV(SERV)", tblServidor, ex);
                            throw ex;
                        }
                        finally
                        {
                            if (tblServidor != null) { tblServidor.Dispose(); }
                            if (tblPdv != null) { tblPdv.Dispose(); }
                            if (taServidor != null) { taServidor.Dispose(); }
                            if (taPdv != null) { taPdv.Dispose(); }
                        }
                    }*/
                    #endregion
                    #endregion Relação FMAPAGTO SIS / MetodoPagto PDV
                    //Sync_TRI_PDV_USERS(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                    #region TRI_PDV_USERS
                    /*
                        {
                            using (var tblTriUsersServ = new FDBDataSet.TRI_PDV_USERSDataTable())
                            using (var tblTriUsersPdv = new FDBDataSet.TRI_PDV_USERSDataTable())
                            {
                                try
                                {
                                    if (dtUltimaSyncPdv is null)
                                    {
                                        #region Única sync

                                        using (var taServ = new TRI_PDV_USERSTableAdapter())
                                        using (var taPdv = new TRI_PDV_USERSTableAdapter())
                                        {
                                            taServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;
                                            taServ.Fill(tblTriUsersServ);

                                            taPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;
                                            taPdv.Fill(tblTriUsersPdv);

                                            //tblClientePdv.Merge(tblClienteServ); Não funciona para o Update(): tblClientePdv.Rows[x].RowState não é alterado, consequentemente o Update() faz nada. Em vez disso, usar o seguinte no lugar do Merge():
                                            using (var changeReader = new DataTableReader(tblTriUsersServ))
                                                tblTriUsersPdv.Load(changeReader, LoadOption.Upsert);

                                            // Salva dados do Serv para o PDV:
                                            taPdv.Update(tblTriUsersPdv);

                                            tblTriUsersPdv.AcceptChanges();
                                        }

                                        #endregion Única sync
                                    }
                                    else
                                    {
                                        #region Sync de cadastros novos ou atualizados

                                        int intRetornoUpsert = 0;

                                        {
                                            DataRow[] pendentesTriUsers = dtAuxSyncPendentes.Select($"TABELA = 'TRI_PDV_USERS'");

                                            for (int i = 0; i < pendentesTriUsers.Length; i++)
                                            {
                                                var idUser = pendentesTriUsers[i]["SM_REG"].SafeShort();
                                                var operacao = pendentesTriUsers[i]["OPERACAO"].Safestring();
                                                var NO_CAIXA = pendentesTriUsers[i]["NO_CAIXA"].SafeShort();

                                                // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                                if (operacao.Equals("I") || operacao.Equals("U"))
                                                {
                                                    // Buscar o registro para executar as operações "Insert" ou "Update"

                                                    using (var taTriUsersServ = new TRI_PDV_USERSTableAdapter())
                                                    {
                                                        taTriUsersServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                                        taTriUsersServ.FillById(tblTriUsersServ, idUser);

                                                        if (tblTriUsersServ != null && tblTriUsersServ.Rows.Count > 0)
                                                        {
                                                            using (var taTriUsersPdv = new TRI_PDV_USERSTableAdapter())
                                                            {
                                                                taTriUsersPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                                foreach (FDBDataSet.TRI_PDV_USERSRow triUsersServ in tblTriUsersServ)
                                                                {
                                                                    intRetornoUpsert = (int)taTriUsersPdv.SP_TRI_TRIUSERS_UPSERT((short)triUsersServ.ID_USER,
                                                                                                                                 triUsersServ.USERNAME,
                                                                                                                                 triUsersServ.PASSWORD,
                                                                                                                                 triUsersServ.GERENCIA,
                                                                                                                                 triUsersServ.ATIVO,
                                                                                                                                 DateTime.Now);

                                                                    // Cadastrou? Tem que falar pro servidor que o registro foi sincronizado.
                                                                    if (intRetornoUpsert.Equals(1))
                                                                    {
                                                                        ConfirmarAuxSync(-1,
                                                                                     "TRI_PDV_USERS",
                                                                                     operacao,
                                                                                     NO_CAIXA,//shtNumCaixa, // TEM QUE ser o mesmo número do caixa consultado na tabela auxiliar.
                                                                                     null,
                                                                                     idUser);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // O item não foi encontrado no servidor.
                                                            // Pode ter sido deletado.
                                                            // Deve constar essa operação em dtAuxSync.
                                                            // Caso contrário, estourar exception.

                                                            using (var dtPendentesTriUsers = pendentesTriUsers.CopyToDataTable())
                                                            {
                                                                DataRow[] deletesPendentesTriUsers = dtPendentesTriUsers.Select($"SM_REG = {idUser} AND OPERACAO = 'D'");

                                                                if (deletesPendentesTriUsers.Length > 0)
                                                                {
                                                                    foreach (var deletePendenteTriUsers in deletesPendentesTriUsers)
                                                                    {
                                                                        dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TRI_PDV_USERS", "D",
                                                                            NO_CAIXA,//shtNumCaixa, 
                                                                            null, null, idUser);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    // Ops....
                                                                    // Item não encontrado no servidor e não foi deletado?
                                                                    // Estourar exception.
                                                                    throw new DataException($"Erro não esperado: produto (TRI_PDV_USERS) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idUser}");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // Não é uma operação "padrão"

                                                    switch (operacao)
                                                    {
                                                        case "D":
                                                            {
                                                                // Não dá pra deletar agora por causa das constraints (FK).
                                                                // Adicionar numa lista e deletar depois, na ordem correta.

                                                                // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                                DataRow[] deletesPendentesTriUsers = dtAuxSyncDeletesPendentes.Select($"SM_REG = {idUser} AND TABELA = 'TRI_PDV_USERS' AND OPERACAO = 'D'");

                                                                if (deletesPendentesTriUsers.Length <= 0)
                                                                {
                                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, -1, "TRI_PDV_USERS", "D",
                                                                        NO_CAIXA,//shtNumCaixa, 
                                                                        null, null, idUser);
                                                                }

                                                                break;
                                                            }
                                                        default:
                                                            throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                                            //break;
                                                    }
                                                }
                                            }
                                        }

                                        #endregion Sync de cadastros novos ou atualizados
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                                    GravarErroSync("TRI_PDV_USERS(PDV)", tblTriUsersPdv, ex);
                                    GravarErroSync("TRI_PDV_USERS(SERV)", tblTriUsersServ, ex);
                                    throw ex;
                                }
                            }
                        }
                    */
                    #endregion TRI_PDV_USERS

                    #region Cadastros pro AmbiMAITRE

                    Sync_TB_EST_COMPOSICAO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                    #region TB_EST_COMPOSICAO
                    /*
                        {
                            using (var tblComposicaoServ = new FDBDataSetMaitre.TB_EST_COMPOSICAODataTable())
                            {
                                try
                                {
                                    #region Sync de cadastros novos ou atualizados

                                    #region AUX_SYNC

                                    int intRetornoUpsert = 0;

                                    {
                                        DataRow[] pendentesComposicao = dtAuxSyncPendentes.Select($"TABELA = 'TB_EST_COMPOSICAO'");

                                        for (int i = 0; i < pendentesComposicao.Length; i++)
                                        {
                                            var idComposicao = pendentesComposicao[i]["ID_REG"].Safeint();
                                            var operacao = pendentesComposicao[i]["OPERACAO"].Safestring();

                                            // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                            if (operacao.Equals("I") || operacao.Equals("U"))
                                            {
                                                // Buscar o registro para executar as operações "Insert" ou "Update"

                                                using (var taComposicaoServ = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMPOSICAOTableAdapter())
                                                {
                                                    taComposicaoServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                                    taComposicaoServ.FillById(tblComposicaoServ, idComposicao);

                                                    if (tblComposicaoServ != null && tblComposicaoServ.Rows.Count > 0)
                                                    {
                                                        using (var taComposicaoPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMPOSICAOTableAdapter())
                                                        {
                                                            taComposicaoPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                            foreach (FDBDataSetMaitre.TB_EST_COMPOSICAORow composicaoServ in tblComposicaoServ)
                                                            {
                                                                intRetornoUpsert = (int)taComposicaoPdv.SP_TRI_MAIT_ESTCOMP_SYNCINSERT(composicaoServ.ID_COMPOSICAO,
                                                                                                          composicaoServ.IsDESCRICAONull() ? string.Empty : composicaoServ.DESCRICAO,
                                                                                                          composicaoServ.ID_IDENTIFICADOR,
                                                                                                          composicaoServ.IsTRI_PDV_DT_UPDNull() ? null : (DateTime?)composicaoServ.TRI_PDV_DT_UPD);

                                                                // Cadastrou a composição e seus itens? Tem que falar pro servidor que o registro foi sincronizado.
                                                                if (intRetornoUpsert.Equals(1))
                                                                {
                                                                    ConfirmarAuxSync(idComposicao,
                                                                                 "TB_EST_COMPOSICAO",
                                                                                 operacao,
                                                                                 shtNumCaixa);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // O item não foi encontrado no servidor.
                                                        // Pode ter sido deletado.
                                                        // Deve constar essa operação em dtAuxSync.
                                                        // Caso contrário, estourar exception.

                                                        using (var dtPendentesComposicao = pendentesComposicao.CopyToDataTable())
                                                        {
                                                            DataRow[] deletesPendentesComposicao = dtPendentesComposicao.Select($"ID_REG = {idComposicao} AND OPERACAO = 'D'");

                                                            if (deletesPendentesComposicao.Length > 0)
                                                            {
                                                                foreach (var deletePendenteComposicao in deletesPendentesComposicao)
                                                                {
                                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idComposicao, "TB_EST_COMPOSICAO", "D", shtNumCaixa, null, null, -1, null);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // Ops....
                                                                // Item não encontrado no servidor e não foi deletado?
                                                                // Estourar exception.
                                                                throw new DataException($"Erro não esperado: produto (TB_EST_COMPOSICAO) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idComposicao}");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Não é uma operação "padrão"

                                                switch (operacao)
                                                {
                                                    case "D":
                                                        {
                                                            // Não dá pra deletar agora por causa das constraints (FK).
                                                            // Adicionar numa lista e deletar depois, na ordem correta.

                                                            // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                            DataRow[] deletesPendentesComposicao = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idComposicao} AND TABELA = 'TB_EST_COMPOSICAO' AND OPERACAO = 'D'");

                                                            if (deletesPendentesComposicao.Length <= 0)
                                                            {
                                                                // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                                dtAuxSyncDeletesPendentes.Rows.Add(0, idComposicao, "TB_EST_COMPOSICAO", "D", shtNumCaixa, null, null, -1, null);
                                                            }

                                                            break;
                                                        }
                                                    default:
                                                        throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                                        //break;
                                                }
                                            }
                                        }
                                    }

                                    #endregion AUX_SYNC

                                    #endregion Sync de cadastros novos ou atualizados

                                }
                                catch (NotImplementedException niex)
                                {
                                    log.Error("Not implemented exception", niex);
                                    throw niex;
                                }
                                catch (DataException dex)
                                {
                                    log.Error("Data Exception", dex);
                                    throw dex;
                                }
                                catch (Exception ex)
                                {
                                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                                    GravarErroSync("Composicao", tblComposicaoServ, ex);
                                    throw ex;
                                }
                            }
                        }
                    */
                    #endregion TB_EST_COMPOSICAO
                    Sync_TB_EST_COMP_ITEM(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                    #region TB_EST_COMP_ITEM
                    /*
                        {
                            using (var tblCompItemServ = new FDBDataSetMaitre.TB_EST_COMP_ITEMDataTable())
                            {
                                try
                                {
                                    #region Sync de cadastros novos ou atualizados

                                    #region AUX_SYNC

                                    int intRetornoUpsert = 0;

                                    {
                                        DataRow[] pendentesCompItem = dtAuxSyncPendentes.Select($"TABELA = 'TB_EST_COMP_ITEM'");

                                        for (int i = 0; i < pendentesCompItem.Length; i++)
                                        {
                                            var idItemComp = pendentesCompItem[i]["ID_REG"].Safeint();
                                            var operacao = pendentesCompItem[i]["OPERACAO"].Safestring();

                                            // Verificar o que deve ser feito com o registro (insert, update ou delete)
                                            if (operacao.Equals("I") || operacao.Equals("U"))
                                            {
                                                // Buscar o registro para executar as operações "Insert" ou "Update"

                                                using (var taCompItemServ = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_ITEMTableAdapter())
                                                {
                                                    taCompItemServ.Connection = fbConnServ;//.ConnectionString = _strConnNetwork;

                                                    taCompItemServ.FillById(tblCompItemServ, idItemComp);

                                                    if (tblCompItemServ != null && tblCompItemServ.Rows.Count > 0)
                                                    {
                                                        using (var taCompItemPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_ITEMTableAdapter())
                                                        {
                                                            taCompItemPdv.Connection = fbConnPdv;//.ConnectionString = _strConnContingency;

                                                            foreach (FDBDataSetMaitre.TB_EST_COMP_ITEMRow compItemServ in tblCompItemServ)
                                                            {
                                                                intRetornoUpsert = (int)taCompItemPdv.SP_TRI_MT_ESTCMP_ITEM_SYNCINSRT(compItemServ.ID_ITEMCOMP,
                                                                                                                 compItemServ.QTD_ITEM,
                                                                                                                 compItemServ.ID_COMPOSICAO,
                                                                                                                 compItemServ.ID_IDENTIFICADOR);

                                                                // Cadastrou a composição e seus itens? Tem que falar pro servidor que o registro foi sincronizado.
                                                                if (intRetornoUpsert.Equals(1))
                                                                {
                                                                    ConfirmarAuxSync(idItemComp,
                                                                                 "TB_EST_COMP_ITEM",
                                                                                 operacao,
                                                                                 shtNumCaixa);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // O item não foi encontrado no servidor.
                                                        // Pode ter sido deletado.
                                                        // Deve constar essa operação em dtAuxSync.
                                                        // Caso contrário, estourar exception.

                                                        using (var dtPendentesCompItem = pendentesCompItem.CopyToDataTable())
                                                        {
                                                            DataRow[] deletesPendentesCompItem = dtPendentesCompItem.Select($"ID_REG = {idItemComp} AND OPERACAO = 'D'");

                                                            if (deletesPendentesCompItem.Length > 0)
                                                            {
                                                                foreach (var deletePendenteCompItem in deletesPendentesCompItem)
                                                                {
                                                                    // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                                    dtAuxSyncDeletesPendentes.Rows.Add(0, idItemComp, "TB_EST_COMP_ITEM", "D", shtNumCaixa, null, null, -1, null);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // Ops....
                                                                // Item não encontrado no servidor e não foi deletado?
                                                                // Estourar exception.
                                                                throw new DataException($"Erro não esperado: produto (TB_EST_COMP_ITEM) não encontrado no servidor e sem exclusão pendente. \nID do registro: {idItemComp}");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Não é uma operação "padrão"

                                                switch (operacao)
                                                {
                                                    case "D":
                                                        {
                                                            // Não dá pra deletar agora por causa das constraints (FK).
                                                            // Adicionar numa lista e deletar depois, na ordem correta.

                                                            // Verificar se o item já foi indicado para exclusão (sequência insert/delete, onde o insert não foi executado por conta da exclusão do registro no servidor)

                                                            DataRow[] deletesPendentesCompItem = dtAuxSyncDeletesPendentes.Select($"ID_REG = {idItemComp} AND TABELA = 'TB_EST_COMP_ITEM' AND OPERACAO = 'D'");

                                                            if (deletesPendentesCompItem.Length <= 0)
                                                            {
                                                                // SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER, UN_REG, SM_REG, CH_REG
                                                                dtAuxSyncDeletesPendentes.Rows.Add(0, idItemComp, "TB_EST_COMP_ITEM", "D", shtNumCaixa, null, null, -1, null);
                                                            }

                                                            break;
                                                        }
                                                    default:
                                                        throw new NotImplementedException($"Indicação de operação da tabela de auxílio ao sincronizador inválido { operacao }");
                                                        //break;
                                                }
                                            }
                                        }
                                    }

                                    #endregion AUX_SYNC

                                    #endregion Sync de cadastros novos ou atualizados

                                }
                                catch (NotImplementedException niex)
                                {
                                    log.Error("Not implemented exception", niex);
                                    throw niex;
                                }
                                catch (DataException dex)
                                {
                                    log.Error("Data Exception", dex);
                                    throw dex;
                                }
                                catch (Exception ex)
                                {
                                    //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                                    GravarErroSync("Item de Composicao", tblCompItemServ, ex);
                                    throw ex;
                                }
                            }
                        }
                    */
                    #endregion TB_EST_COMP_ITEM

                    #endregion Cadastros pro AmbiMAITRE

                    // NÃO É CADASTRO, mas deve encaixar perfeitamente no procedimento....
                    // NÃO
                    // Dá ruim. Num novo deployment. Pede confirmação de senha do usuário para fazer o primeiro
                    // login (TRI_PDV_USERS). Como ainda não há registro em TRI_PDV_CONFIG, não há número de caixa
                    // registrado. Então a tabela auxiliar não era alimentada. Fica pedindo confirmação de senha repetidamente.
                    // 
                    Sync_TRI_PDV_CONFIG(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);

                    #region Executar deletes pendentes indicados no TRI_PDV_AUX_SYNC

                    // Verifica se há registros para deletar no banco do PDV
                    if (dtAuxSyncDeletesPendentes.Rows.Count > 0)
                    {
                        //using (var fbConnPdv = new FbConnection(_strConnContingency))
                        //{
                        // Tem que buscar os registros na ordem certa de tabelas, caso contrário dá constraint violation....
                        Sync_Delete_TB_EST_PRODUTO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_EST_PRODUTO
                        /*
                            try
                            {
                                var drEstProdutoDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_PRODUTO'");

                                if (drEstProdutoDeletesPendentes.Length > 0)
                                {
                                    using (var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter())
                                    {
                                        taEstProdutoPdv.Connection = fbConnPdv;

                                        foreach (var drEstProdutoDeletePendente in drEstProdutoDeletesPendentes)
                                        {
                                            taEstProdutoPdv.Delete(drEstProdutoDeletePendente["ID_REG"].Safeint());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(drEstProdutoDeletePendente["ID_REG"].Safeint(),
                                                             drEstProdutoDeletePendente["TABELA"].Safestring(),
                                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_PRODUTO", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_EST_PRODUTO
                        Sync_Delete_TB_EST_IDENTIFICADOR(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_EST_IDENTIFICADOR
                        /*
                            try
                            {
                                var drEstIdentifDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_IDENTIFICADOR'");

                                if (drEstIdentifDeletesPendentes.Length > 0)
                                {
                                    using (var taEstIdentifPdv = new TB_EST_IDENTIFICADORTableAdapter())
                                    {
                                        //taEstIdentifPdv.Connection = fbConnPdv;

                                        foreach (var drEstIdentifDeletePendente in drEstIdentifDeletesPendentes)
                                        {
                                            #region TB_EST_SALDO_ALTERADO

                                            // Apagar dependência

                                            using (var transactionScopeIdentifSaldo = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                                            using (var commSaldoAlteradoPdv = new FbCommand())
                                            using (var fbConnIdentifSaldoPdv = new FbConnection())
                                            {
                                                fbConnIdentifSaldoPdv.ConnectionString = _strConnContingency;

                                                fbConnIdentifSaldoPdv.Open();

                                                commSaldoAlteradoPdv.Connection = fbConnIdentifSaldoPdv;
                                                commSaldoAlteradoPdv.CommandType = CommandType.Text;

                                                taEstIdentifPdv.Connection = fbConnIdentifSaldoPdv;

                                                //foreach (var drFuncDeletePendente in drFuncDeletesPendentes)
                                                //{
                                                commSaldoAlteradoPdv.CommandText = $"DELETE FROM TB_EST_SALDO_ALTERADO WHERE ID_IDENTIFICADOR = {drEstIdentifDeletePendente["ID_REG"]}";
                                                //TODO: talvez criar um INDEX para TB_EST_SALDO_ALTERADO.ID_IDENTIFICADOR? Pode melhorar a performance. Principalmente depois de criar uma sproc para deletar registros, também.

                                                commSaldoAlteradoPdv.ExecuteNonQuery();

                                                //}

                                                #endregion TB_EST_SALDO_ALTERADO

                                                taEstIdentifPdv.Delete(drEstIdentifDeletePendente["ID_REG"].Safeint());

                                                // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                                ConfirmarAuxSync(drEstIdentifDeletePendente["ID_REG"].Safeint(),
                                                                 drEstIdentifDeletePendente["TABELA"].Safestring(),
                                                                 "D", //drEstIdentifDeletePendente["OPERACAO"].Safestring(),
                                                                 shtNumCaixa);

                                                transactionScopeIdentifSaldo.Complete();

                                                fbConnIdentifSaldoPdv?.Close();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_IDENTIFICADOR", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_EST_IDENTIFICADOR
                        Sync_Delete_TB_ESTOQUE(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_ESTOQUE
                        /*
                            try
                            {
                                var drEstoqueDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_ESTOQUE'");

                                if (drEstoqueDeletesPendentes.Length > 0)
                                {
                                    foreach (var drEstoqueDeletePendente in drEstoqueDeletesPendentes)
                                    {
                                        // Apagar dependência

                                        using (var transactionScopeDependenciasEstoque = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                                        using (var commEstDescHistPdv = new FbCommand())
                                        using (var commEstIndexador = new FbCommand())
                                        using (var commEstFornecedor = new FbCommand())
                                        using (var commEstAdicional = new FbCommand())
                                        using (var fbConnDependenciasEstoquePdv = new FbConnection())
                                        using (var taEstoquePdv = new TB_ESTOQUETableAdapter())
                                        {
                                            fbConnDependenciasEstoquePdv.ConnectionString = _strConnContingency;
                                            fbConnDependenciasEstoquePdv.Open();

                                            #region TB_EST_DESC_HISTORICO

                                            try
                                            {
                                                commEstDescHistPdv.Connection = fbConnDependenciasEstoquePdv;
                                                commEstDescHistPdv.CommandType = CommandType.Text;

                                                commEstDescHistPdv.CommandText = $"DELETE FROM TB_EST_DESC_HISTORICO WHERE ID_ESTOQUE = {drEstoqueDeletePendente["ID_REG"]}";
                                                commEstDescHistPdv.ExecuteNonQuery();
                                            }
                                                catch(Exception ex)
                                            {
                                                 log.Error("Erro ao deletar registro de TB_EST_DESC_HISTORICO - ","SQL syntaxe\n DELETE FROM TB_EST_DESC_HISTORICO WHERE ID_ESTOQUE = {drEstoqueDeletePendente['ID_REG']}\n", ex);
                                                 throw ex;
                                            }
                                            #endregion TB_EST_SALDO_ALTERADO

                                            #region TB_EST_INDEXADOR
                                             try
                                        {
                                            commEstIndexador.Connection = fbConnDependenciasEstoquePdv;
                                            commEstIndexador.CommandType = CommandType.Text;

                                            commEstIndexador.CommandText = $"DELETE FROM TB_EST_INDEXADOR WHERE ID_ESTOQUE = {drEstoqueDeletePendente["ID_REG"]}";
                                            commEstIndexador.ExecuteNonQuery();
                                        }
                                          catch(Exception ex)
                                            {
                                                 log.Error("Erro ao deletar registro de TB_EST_INDEXADOR - ","SQL syntaxe\n DELETE FROM TB_EST_INDEXADOR WHERE ID_ESTOQUE = {drEstoqueDeletePendente['ID_REG']}\n", ex);
                                                 throw ex;
                                            }
                                            #endregion TB_EST_INDEXADOR

                                            #region TB_EST_FORNECEDOR
                                          try
                                          {
                                            commEstFornecedor.Connection = fbConnDependenciasEstoquePdv;
                                            commEstFornecedor.CommandType = CommandType.Text;

                                            commEstFornecedor.CommandText = $"DELETE FROM TB_EST_FORNECEDOR WHERE ID_ESTOQUE = {drEstoqueDeletePendente["ID_REG"]}";
                                            commEstFornecedor.ExecuteNonQuery();
                                          }
                                            catch(Exception ex)
                                            {
                                                 log.Error("Erro ao deletar registro de TB_EST_FORNECEDOR - ","SQL syntaxe\n DELETE FROM TB_EST_FORNECEDOR WHERE ID_ESTOQUE = {drEstoqueDeletePendente['ID_REG']}\n", ex);
                                                 throw ex;
                                            }
                                            #endregion TB_EST_FORNECEDOR

                                            #region TB_EST_ADICIONAL
                                            try
                                            {
                                            commEstAdicional.Connection = fbConnDependenciasEstoquePdv;
                                            commEstAdicional.CommandType = CommandType.Text;

                                            commEstAdicional.CommandText = $"DELETE FROM TB_EST_ADICIONAL WHERE ID_ESTOQUE = {drEstoqueDeletePendente["ID_REG"]}";
                                            commEstAdicional.ExecuteNonQuery();
                                            }
                                                catch(Exception ex)
                                                    {
                                                         log.Error("Erro ao deletar registro de TB_EST_ADICIONAL - ","SQL syntaxe\n DELETE FROM TB_EST_ADICIONAL WHERE ID_ESTOQUE = {drEstoqueDeletePendente['ID_REG']}\n", ex);
                                                         throw ex;
                                                    }
                                            #endregion TB_EST_ADICIONAL

                                            #region TB_ESTOQUE
                                        try{
                                            taEstoquePdv.Connection = fbConnDependenciasEstoquePdv;
                                            taEstoquePdv.Delete(drEstoqueDeletePendente["ID_REG"].Safeint());

                                            #endregion TB_ESTOQUE
                                        

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(drEstoqueDeletePendente["ID_REG"].Safeint(),
                                                             drEstoqueDeletePendente["TABELA"].Safestring(),
                                                             "D", //drEstIdentifDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa);

                                            transactionScopeDependenciasEstoque.Complete();

                                            fbConnDependenciasEstoquePdv?.Close();

                                            }
                                                catch(Exception ex)
                                                    {
                                                         log.Error("Erro ao falar pro servidor que o registro foi sincronizado. - ","\n", ex);
                                                         throw ex;
                                                    }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_ESTOQUE e/ou suas dependências: ", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_ESTOQUE
                        Sync_Delete_TB_FORNECEDOR(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_FORNECEDOR
                        /*
                      try
                      {
                          var drFornecDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FORNECEDOR'");

                          if (drFornecDeletesPendentes.Length > 0)
                          {
                              using (var taFornecPdv = new TB_FORNECEDORTableAdapter())
                              {
                                  taFornecPdv.Connection = fbConnPdv;

                                  foreach (var drFornecDeletePendente in drFornecDeletesPendentes)
                                  {
                                      taFornecPdv.Delete(drFornecDeletePendente["ID_REG"].Safeint());

                                      // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                      ConfirmarAuxSync(drFornecDeletePendente["ID_REG"].Safeint(),
                                                       drFornecDeletePendente["TABELA"].Safestring(),
                                                       "D", //drFornecDeletePendente["OPERACAO"].Safestring(),
                                                       shtNumCaixa);
                                  }
                              }
                          }
                      }
                      catch (Exception ex)
                      {
                          log.Error("Erro ao deletar registro de TB_FORNECEDOR",  ex);
                          throw ex;
                      }
                  */
                        #endregion TB_FORNECEDOR
                        Sync_Delete_TB_EST_GRUPO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_EST_GRUPO
                        /*
                            try
                            {
                                var drEstGrupoDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_GRUPO'");

                                if (drEstGrupoDeletesPendentes.Length > 0)
                                {
                                    using (var taEstGrupoPdv = new TB_EST_GRUPOTableAdapter())
                                    {
                                        taEstGrupoPdv.Connection = fbConnPdv;

                                        foreach (var drEstGrupoDeletePendente in drEstGrupoDeletesPendentes)
                                        {
                                            taEstGrupoPdv.DeleteById(drEstGrupoDeletePendente["ID_REG"].Safeint());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(drEstGrupoDeletePendente["ID_REG"].Safeint(),
                                                             drEstGrupoDeletePendente["TABELA"].Safestring(),
                                                             "D", //drEstGrupoDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_GRUPO", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_EST_GRUPO
                        Sync_Delete_TB_CLIENTE(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_CLIENTE
                        /*
                            try
                            {
                                var drClienteDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_CLIENTE'");

                                if (drClienteDeletesPendentes.Length > 0)
                                {
                                    using (var taClientePdv = new TB_CLIENTETableAdapter())
                                    using (var taCliPfPdv = new TB_CLI_PFTableAdapter())
                                    using (var taCliPjPdv = new TB_CLI_PJTableAdapter())
                                    {
                                        taClientePdv.Connection = fbConnPdv;
                                        taCliPfPdv.Connection = fbConnPdv;
                                        taCliPjPdv.Connection = fbConnPdv;

                                        foreach (var drClienteDeletePendente in drClienteDeletesPendentes)
                                        {
                                            int idCliente = drClienteDeletePendente["ID_REG"].Safeint();
                                            taCliPfPdv.DeleteById(idCliente);
                                            taCliPjPdv.DeleteById(idCliente);
                                            taClientePdv.Delete(idCliente);

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(drClienteDeletePendente["ID_REG"].Safeint(),
                                                             drClienteDeletePendente["TABELA"].Safestring(),
                                                             "D",//drClienteDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_CLIENTE", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_CLIENTE
                        Sync_Delete_TB_FUNCIONARIO_TB_FUNC_PAPEL_TB_FUNC_COMISSAO(shtNumCaixa, dtAuxSyncDeletesPendentes);
                        #region TB_FUNCIONARIO / TB_FUNC_PAPEL / TB_FUNC_COMISSAO
                        /*
                            try
                            {
                                var drFuncDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FUNCIONARIO'");

                                if (drFuncDeletesPendentes.Length > 0)
                                {
                                    using (var transactionScopeFunc = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                                    using (var commFuncComissaoPdv = new FbCommand())
                                    using (var taFuncPapelPdv = new TB_FUNC_PAPELTableAdapter())
                                    using (var taFuncPdv = new TB_FUNCIONARIOTableAdapter())
                                    using (var fbConnFuncPdv = new FbConnection())
                                    {
                                        fbConnFuncPdv.ConnectionString = _strConnContingency;

                                        fbConnFuncPdv.Open();

                                        commFuncComissaoPdv.Connection = fbConnFuncPdv;
                                        commFuncComissaoPdv.CommandType = CommandType.Text;

                                        taFuncPapelPdv.Connection = fbConnFuncPdv;
                                        taFuncPdv.Connection = fbConnFuncPdv;

                                        foreach (var drFuncDeletePendente in drFuncDeletesPendentes)
                                        {
                                            commFuncComissaoPdv.CommandText = $"DELETE FROM TB_FUNC_COMISSAO WHERE ID_FUNCIONARIO = {drFuncDeletePendente["ID_REG"]}";

                                            commFuncComissaoPdv.ExecuteNonQuery();

                                            taFuncPapelPdv.DeleteByIdFunc(drFuncDeletePendente["ID_REG"].Safeint());
                                            taFuncPdv.DeleteById(drFuncDeletePendente["ID_REG"].Safeint());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(drFuncDeletePendente["ID_REG"].Safeint(),
                                                             drFuncDeletePendente["TABELA"].Safestring(),
                                                             "D", //drFuncDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa);
                                        }

                                        transactionScopeFunc.Complete();

                                        fbConnFuncPdv?.Close();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_FUNCIONARIO / TB_FUNC_PAPEL / TB_FUNC_COMISSAO ", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_FUNCIONARIO / TB_FUNC_PAPEL / TB_FUNC_COMISSAO
                        Sync_Delete_TB_UNI_MEDIDA(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_UNI_MEDIDA
                        /*
                            try
                            {
                                var drUniMedDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_UNI_MEDIDA'");

                                if (drUniMedDeletesPendentes.Length > 0)
                                {
                                    using (var taUniMedPdv = new TB_UNI_MEDIDATableAdapter())
                                    {
                                        taUniMedPdv.Connection = fbConnPdv;

                                        foreach (var drUniMedDeletePendente in drUniMedDeletesPendentes)
                                        {
                                            taUniMedPdv.DeleteByUnidade(drUniMedDeletePendente["UN_REG"].Safestring());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(0, //drUniMedDeletePendente["ID_REG"].Safeint(),
                                                             drUniMedDeletePendente["TABELA"].Safestring(),
                                                             "D",//drClienteDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa,
                                                             drUniMedDeletePendente["UN_REG"].Safestring());
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_UNI_MEDIDA", ex);
                                throw ex;
                            }
                            */
                        #endregion TB_UNI_MEDIDA
                        Sync_Delete_TB_FUNC_PAPEL(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_FUNC_PAPEL
                        /*
                            try
                            {
                                var drFuncPapelDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FUNC_PAPEL'");

                                if (drFuncPapelDeletesPendentes.Length > 0)
                                {
                                    using (var taFuncPapelPdv = new TB_FUNC_PAPELTableAdapter())
                                    {
                                        taFuncPapelPdv.Connection = fbConnPdv;

                                        foreach (var drFuncPapelDeletePendente in drFuncPapelDeletesPendentes)
                                        {
                                            taFuncPapelPdv.DeleteByIds(drFuncPapelDeletePendente["ID_REG"].Safeint(),
                                                                       drFuncPapelDeletePendente["SM_REG"].SafeShort());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(drFuncPapelDeletePendente["ID_REG"].Safeint(),
                                                             drFuncPapelDeletePendente["TABELA"].Safestring(),
                                                             "D",//drClienteDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa,
                                                             null,
                                                             drFuncPapelDeletePendente["SM_REG"].SafeShort());
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_UNI_MEDIDA", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_FUNC_PAPEL
                        Sync_Delete_TRI_PDV_USERS(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TRI_PDV_USERS
                        /*
                            try
                            {
                                var drTriUsersDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TRI_PDV_USERS'");

                                if (drTriUsersDeletesPendentes.Length > 0)
                                {
                                    using (var taTriUsersPdv = new TRI_PDV_USERSTableAdapter())
                                    {
                                        taTriUsersPdv.Connection = fbConnPdv;

                                        foreach (var drTriUsersDeletePendente in drTriUsersDeletesPendentes)
                                        {
                                            taTriUsersPdv.DeleteById(drTriUsersDeletePendente["SM_REG"].SafeShort());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(-1,
                                                             drTriUsersDeletePendente["TABELA"].Safestring(),
                                                             "D", //drEstoqueDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa,
                                                             null,
                                                             drTriUsersDeletePendente["SM_REG"].SafeShort());
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TRI_PDV_USERS", ex);
                                throw ex;
                            }
                        */
                        #endregion TRI_PDV_USERS
                        Sync_Delete_TB_TAXA_UF(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_TAXA_UF
                        /*
                            try
                            {
                                var drTaxaUfDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_TAXA_UF'");

                                if (drTaxaUfDeletesPendentes.Length > 0)
                                {
                                    using (var taTaxaUfPdv = new TB_TAXA_UFTableAdapter())
                                    {
                                        taTaxaUfPdv.Connection = fbConnPdv;

                                        foreach (var drTaxaUfDeletePendente in drTaxaUfDeletesPendentes)
                                        {
                                            taTaxaUfPdv.DeleteById(drTaxaUfDeletePendente["CH_REG"].Safestring());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(-1,
                                                             drTaxaUfDeletePendente["TABELA"].Safestring(),
                                                             "D", //drEstoqueDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa,
                                                             null,
                                                             -1,
                                                             drTaxaUfDeletePendente["CH_REG"].Safestring());
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_TAXA_UF", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_TAXA_UF
                        Sync_Delete_TB_CFOP_SIS(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_CFOP_SIS
                        /*
                            try
                            {
                                var drCfopSisDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_CFOP_SIS'");

                                if (drCfopSisDeletesPendentes.Length > 0)
                                {
                                    using (var taCfopSisPdv = new TB_CFOP_SISTableAdapter())
                                    {
                                        taCfopSisPdv.Connection = fbConnPdv;

                                        foreach (var drCfopSisDeletePendente in drCfopSisDeletesPendentes)
                                        {
                                            taCfopSisPdv.DeleteById(drCfopSisDeletePendente["UN_REG"].Safestring());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(-1,
                                                             drCfopSisDeletePendente["TABELA"].Safestring(),
                                                             "D", //drEstoqueDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa,
                                                             drCfopSisDeletePendente["UN_REG"].Safestring(),
                                                             -1,
                                                             null);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_TAXA_UF",ex);
                                throw ex;
                            }
                        */
                        #endregion TB_TAXA_UF
                        Sync_Delete_TB_NAT_OPERACAO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_NAT_OPERACAO
                        /*
                            try
                            {
                                var drNatOperDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_NAT_OPERACAO'");

                                if (drNatOperDeletesPendentes.Length > 0)
                                {
                                    using (var taNatOperPdv = new TB_NAT_OPERACAOTableAdapter())
                                    {
                                        taNatOperPdv.Connection = fbConnPdv;

                                        foreach (var drNatOperDeletePendente in drNatOperDeletesPendentes)
                                        {
                                            taNatOperPdv.DeleteById(drNatOperDeletePendente["ID_REG"].Safeint());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(drNatOperDeletePendente["ID_REG"].Safeint(),
                                                             drNatOperDeletePendente["TABELA"].Safestring(),
                                                             "D", //drEstoqueDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa,
                                                             null,
                                                             -1,
                                                             null);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_NAT_OPERACAO", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_NAT_OPERACAO                        
                        Sync_Delete_TB_FORMA_PAGTO_SIS(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_FORMA_PAGTO_SIS

                        try
                        {
                            var drFmaPgtoSisDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FORMA_PAGTO_SIS'");

                            if (drFmaPgtoSisDeletesPendentes.Length > 0)
                            {
                                using (var taFmaPgtoSisPdv = new TB_FORMA_PAGTO_SISTableAdapter())
                                {
                                    taFmaPgtoSisPdv.Connection = fbConnPdv;

                                    foreach (var drFmaPgtoSisDeletePendente in drFmaPgtoSisDeletesPendentes)
                                    {
                                        taFmaPgtoSisPdv.DeleteById(drFmaPgtoSisDeletePendente["SM_REG"].Safeshort());

                                        // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                        ConfirmarAuxSync(-1,
                                                         drFmaPgtoSisDeletePendente["TABELA"].Safestring(),
                                                         "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                                         shtNumCaixa,
                                                         null,
                                                         drFmaPgtoSisDeletePendente["SM_REG"].Safeshort(),
                                                         null);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Erro ao deletar registro de TB_FORMA_PAGTO_SIS", ex);
                            throw ex;
                        }

                        #endregion TB_FORMA_PAGTO_SIS
                        Sync_Delete_TB_PARCELAMENTO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_PARCELAMENTO
                        /*
                            try
                            {
                                var drParcelamentoDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_PARCELAMENTO'");

                                if (drParcelamentoDeletesPendentes.Length > 0)
                                {
                                    using (var taParcelamentoPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_PARCELAMENTOTableAdapter())
                                    {
                                        taParcelamentoPdv.Connection = fbConnPdv;

                                        foreach (var drParcelamentoDeletePendente in drParcelamentoDeletesPendentes)
                                        {
                                            taParcelamentoPdv.DeleteById(drParcelamentoDeletePendente["SM_REG"].SafeShort());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(-1,
                                                             drParcelamentoDeletePendente["TABELA"].Safestring(),
                                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa,
                                                             null,
                                                             drParcelamentoDeletePendente["SM_REG"].SafeShort(),
                                                             null);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_PARCELAMENTO", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_PARCELAMENTO
                        Sync_Delete_TB_FORMA_PAGTO_NFCE(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region TB_FORMA_PAGTO_NFCE
                        /*
                            try
                            {
                                var drFormaPagtoNfceDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_FORMA_PAGTO_NFCE'");

                                if (drFormaPagtoNfceDeletesPendentes.Length > 0)
                                {
                                    using (var taFormaPagtoNfcePdv = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter())
                                    {
                                        taFormaPagtoNfcePdv.Connection = fbConnPdv;

                                        foreach (var drFormaPagtoNfceDeletePendente in drFormaPagtoNfceDeletesPendentes)
                                        {
                                            taFormaPagtoNfcePdv.DeleteById(drFormaPagtoNfceDeletePendente["SM_REG"].SafeShort());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(-1,
                                                             drFormaPagtoNfceDeletePendente["TABELA"].Safestring(),
                                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa,
                                                             null,
                                                             drFormaPagtoNfceDeletePendente["SM_REG"].SafeShort(),
                                                             null);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_FORMA_PAGTO_NFCE", ex);
                                throw ex;
                            }
                        */
                        #endregion TB_FORMA_PAGTO_NFCE
                        Sync_Delete_COMPOSICAO(dtUltimaSyncPdv, fbConnServ, fbConnPdv, dtAuxSyncPendentes, dtAuxSyncDeletesPendentes, shtNumCaixa);
                        #region Composição
                        /*
                            #region TB_EST_COMP_ITEM

                            try
                            {
                                var drCompItemDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_COMP_ITEM'");

                                if (drCompItemDeletesPendentes.Length > 0)
                                {
                                    using (var taCompItemPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_ITEMTableAdapter())
                                    {
                                        taCompItemPdv.Connection = fbConnPdv;

                                        foreach (var drCompItemDeletePendente in drCompItemDeletesPendentes)
                                        {
                                            taCompItemPdv.DeleteById(drCompItemDeletePendente["ID_REG"].Safeint());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(drCompItemDeletePendente["ID_REG"].Safeint(),
                                                             drCompItemDeletePendente["TABELA"].Safestring(),
                                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_COMP_ITEM", ex);
                                throw ex;
                            }

                            #endregion TB_EST_COMP_ITEM

                            #region TB_EST_COMPOSICAO

                            try
                            {
                                var drComposicaoDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_EST_COMPOSICAO'");

                                if (drComposicaoDeletesPendentes.Length > 0)
                                {
                                    using (var taComposicaoPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMPOSICAOTableAdapter())
                                    {
                                        taComposicaoPdv.Connection = fbConnPdv;

                                        foreach (var drComposicaoDeletePendente in drComposicaoDeletesPendentes)
                                        {
                                            taComposicaoPdv.DeleteById(drComposicaoDeletePendente["ID_REG"].Safeint());

                                            // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                            ConfirmarAuxSync(drComposicaoDeletePendente["ID_REG"].Safeint(),
                                                             drComposicaoDeletePendente["TABELA"].Safestring(),
                                                             "D",//drEstProdutoDeletePendente["OPERACAO"].Safestring(),
                                                             shtNumCaixa);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao deletar registro de TB_EST_COMPOSICAO", ex);
                                throw ex;
                            }

                            #endregion TB_EST_COMPOSICAO
                        */
                        #endregion Composição

                        #region TB_IFS(DESATIVADO)
                        /*
                        try
                        {
                            var drIfsDeletesPendentes = dtAuxSyncDeletesPendentes.Select("TABELA = 'TB_IFS'");

                            if (drIfsDeletesPendentes.Length > 0)
                            {
                                using (var taIfsPdv = new TB_IFSTableAdapter())
                                {
                                    taIfsPdv.Connection = fbConnPdv;

                                    foreach (var drIfsDeletePendente in drIfsDeletesPendentes)
                                    {
                                        taIfsPdv.DeleteById(drIfsDeletePendente["SM_REG"].SafeShort());

                                        // Deletou? Tem que falar pro servidor que o registro foi sincronizado.

                                        ConfirmarAuxSync(-1,
                                                         drIfsDeletePendente["TABELA"].Safestring(),
                                                         "D", //drEstoqueDeletePendente["OPERACAO"].Safestring(),
                                                         shtNumCaixa,
                                                         null,
                                                         drIfsDeletePendente["SM_REG"].SafeShort());
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            gravarMensagemErro("Erro ao deletar registro de TB_IFS" + RetornarMensagemErro(ex, true));
                            throw ex;
                        }
                        */
                        #endregion TB_IFS(DESATIVADO)

                        //}
                    }

                    #endregion Executar deletes pendentes indicados no TRI_PDV_AUX_SYNC

                }

            }

            #endregion Cadastros

            #region Operações

            Sync_Operacoes_TRI_PDV_TERMINAL_USUARIO_INCOMPLETO(tipoSync);

            #region TRI_PDV_TERMINAL_USUARIO (PDV -> Serv) (INCOMPLETO)
            /*
             if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
             {
                 using (var tblTermarioPdv = new FDBDataSetOperSeed.TRI_PDV_TERMINAL_USUARIODataTable())
                 using (var tblTermarioServ = new FDBDataSetOperSeed.TRI_PDV_TERMINAL_USUARIODataTable())
                 {
                     try
                     {
                         using (var taTermarioPdv = new TRI_PDV_TERMINAL_USUARIOTableAdapter())
                         {
                             taTermarioPdv.Connection.ConnectionString = _strConnContingency;

                             #region (1)Consultar o primeiro registro com a maior CURRENTTIME:

                             taTermarioPdv.FillByNumCaixaAberturaLast(tblTermarioPdv, _intNoCaixa);

                             if (tblTermarioPdv != null && tblTermarioPdv.Rows.Count > 0)
                             {
                                 // deu ruim
                                 if (tblTermarioPdv.Rows.Count > 1) { throw new Exception($"Retorno não esperado: taTermarioPdv.FillByNumCaixaAberturaLast({_intNoCaixa}) retornou mais de um registro."); }

                                 using (var taTermarioServ = new TRI_PDV_TERMINAL_USUARIOTableAdapter())
                                 {
                                     taTermarioServ.Connection.ConnectionString = _strConnNetwork;

                                     #region (1.2.1)UPSERT no serv.
                                     taTermarioServ.SP_TRI_TERMARIO_UPSERT_1(//tblTermarioPdv[0].ID_OPER, // A PK não deve ser passada do PDV para o servidor....
                                                                             tblTermarioPdv[0].NUM_CAIXA,
                                                                             tblTermarioPdv[0].STATUS,
                                                                             tblTermarioPdv[0].TS_ABERTURA,
                                                                             tblTermarioPdv[0].IsTS_FECHAMENTONull() ? null : (DateTime?)tblTermarioPdv[0].TS_FECHAMENTO,
                                                                             tblTermarioPdv[0].ID_USER);
                                     #endregion (1.2.1)UPSERT no serv.

                                 }
                             }

                             #endregion (1)Consultar o primeiro registro com a maior CURRENTTIME:
                         }
                     }
                     catch (Exception ex)
                     {
                         //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                         GravarErroSync("TRI_PDV_TERMINAL_USUARIO(P->S)(PDV)", tblTermarioPdv, ex);
                         GravarErroSync("TRI_PDV_TERMINAL_USUARIO(P->S)(SERV)", tblTermarioServ, ex);
                         throw ex;
                     }
                 }
             }
             */
            #endregion TRI_PDV_TERMINAL_USUARIO (PDV -> Serv) (INCOMPLETO)

            Sync_Operacoes_TRI_PDV_SANSUP_PDV_Serv(tipoSync);
            #region TRI_PDV_SANSUP (PDV => Serv)
            /*
            if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
            {
                using (var tblSanSupPdv = new FDBDataSetOperSeed.TRI_PDV_SANSUPDataTable())
                using (var tblSanSupServ = new FDBDataSetOperSeed.TRI_PDV_SANSUPDataTable())
                {
                    try
                    {
                        using (var taSanSupPdv = new TRI_PDV_SANSUPTableAdapter())
                        {
                            taSanSupPdv.Connection.ConnectionString = _strConnContingency;

                            //Preenche a tabela com os registros não enviados para o servidor.

                            taSanSupPdv.FillByNotSynched(tblSanSupPdv);

                            //Caso haja algum registro a ser enviado para o servidor...

                            if (tblSanSupPdv != null && tblSanSupPdv.Rows.Count > 0)
                            {
                                using (var taSanSupServ = new TRI_PDV_SANSUPTableAdapter())
                                {
                                    taSanSupServ.Connection.ConnectionString = _strConnNetwork;
                                    foreach (FDBDataSetOperSeed.TRI_PDV_SANSUPRow row in tblSanSupPdv)
                                    {
                                        taSanSupServ.Insert(-1, row.ID_CAIXA, row.TS_ABERTURA, row.OPERACAO, row.VALOR, row.TS_OPERACAO, "S");
                                        row.SYNCHED = "S";
                                        taSanSupPdv.Update(row);
                                    }

                                }
                            }
                            //20205747380355 Vivo
                            //210120207984759 Vivo 10615
                            //
                        }
                    }
                    catch (Exception ex)
                    {
                        //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                        GravarErroSync("TRI_PDV_TERMINAL_USUARIO(P->S)(PDV)", tblSanSupPdv, ex);
                        GravarErroSync("TRI_PDV_TERMINAL_USUARIO(P->S)(SERV)", tblSanSupServ, ex);
                        throw ex;
                    }
                }
            }
            */

            #endregion TRI_PDV_SANSUP (PDV => Serv)

            #endregion Operações

            #region Trocas -- DESATIVADO DESDE 1.4.5.17
            //if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
            //{
            //    // Busca trocas no Serv, grava no PDV:
            //    SyncTrocas(EnmDBSync.pdv, "N"); //TODO: TERMINAR ESSA
            //    SyncTrocas(EnmDBSync.pdv, "S"); //TODO: TERMINAR ESSA
            //}
            #endregion Trocas

            #endregion Serv -> PDV

            #region Antes de incluir/excluir registros, equalizar todos os GENERATORS (Serv)
            if (tipoSyncCtrlS == EnmTipoSync.CtrlS)
            {
                try
                {
                    var atualizador = new AtualizaGeradores();
                    atualizador.Execute(EnmDBSync.pdv);
                    atualizador.Execute(EnmDBSync.serv);

                }
                #region Manipular Exception
                catch (Exception ex)
                {
                    log.Error("Erro ao sincronizar (Atualizar Geradores):", ex);
                    throw ex;
                }
                #endregion Manipular Exception
            }
            #endregion Antes de incluir/excluir registros, equalizar todos os GENERATORS (Serv)

            #region PDV -> Serv

            #region Vendas

            Sync_TRI_PDV_OPER_PDV_Serv(tipoSync);
            #region TRI_PDV_OPER (PDV -> Serv)
            /*
            // Para todos os efeitos, sangria e suprimento são considerados como venda.
            if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
            {
                using (var tblOperPdv = new FDBDataSetVenda.TRI_PDV_OPERDataTable())
                using (var tblOperServ = new FDBDataSetVenda.TRI_PDV_OPERDataTable())
                {
                    try
                    {
                        #region roteiro
                        // (1)Consultar o primeiro registro com a maior CURRENTTIME:
                        //      - Tem registro:
                        //              - Ver o campo ABERTO:
                        //                      - "S":
                        //                              - (1.1)UPSERT no serv.
                        //                      - "N":
                        //                              - (1.2)Consultar no serv um registro equivalente (ID_CAIXA e CURRENTTIME)
                        //                                      - Tem registro:
                        //                                              - Ver o campo ABERTO:
                        //                                                  - "S":
                        //                                                          - (1.2.1)UPSERT no serv.
                        //                                                  - "N":
                        //                                                          - Faz nada.
                        //                                      - Não tem registro:
                        //                                              - (1.2.1)UPSERT no serv.
                        //      - Não tem registro:
                        //              - Faz nada.
                        #endregion roteiro

                        using (var taOperPdv = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                        {
                            taOperPdv.Connection.ConnectionString = _strConnContingency;

                            #region (1)Consultar o primeiro registro com a maior CURRENTTIME:

                            taOperPdv.FillByIdCaixaLast(tblOperPdv, _intNoCaixa); // já usa sproc

                            if (tblOperPdv != null && tblOperPdv.Rows.Count > 0)
                            {
                                // deu ruim
                                if (tblOperPdv.Rows.Count > 1) { throw new Exception($"Retorno não esperado: taOperPdv.FillByIdCaixaLast({_intNoCaixa}) retornou mais de um registro."); }

                                using (var taOperServ = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                                {
                                    taOperServ.Connection.ConnectionString = _strConnNetwork;

                                    // Ver o campo ABERTO:
                                    if (tblOperPdv[0].ABERTO.Equals("S"))
                                    {
                                        #region (1.2.1)UPSERT no serv.
                                        taOperServ.SP_TRI_OPER_UPSERT_IDCX_CURT(tblOperPdv[0].ID_CAIXA,
                                                                                tblOperPdv[0].DIN,
                                                                                tblOperPdv[0].CHEQUE,
                                                                                tblOperPdv[0].CREDITO,
                                                                                tblOperPdv[0].DEBITO,
                                                                                tblOperPdv[0].LOJA,
                                                                                tblOperPdv[0].ALIMENTACAO,
                                                                                tblOperPdv[0].REFEICAO,
                                                                                tblOperPdv[0].PRESENTE,
                                                                                tblOperPdv[0].COMBUSTIVEL,
                                                                                tblOperPdv[0].OUTROS,
                                                                                tblOperPdv[0].EXTRA_1,
                                                                                tblOperPdv[0].EXTRA_2,
                                                                                tblOperPdv[0].EXTRA_3,
                                                                                tblOperPdv[0].EXTRA_4,
                                                                                tblOperPdv[0].EXTRA_5,
                                                                                tblOperPdv[0].EXTRA_6,
                                                                                tblOperPdv[0].EXTRA_7,
                                                                                tblOperPdv[0].EXTRA_8,
                                                                                tblOperPdv[0].EXTRA_9,
                                                                                tblOperPdv[0].EXTRA_10,
                                                                                tblOperPdv[0].CURRENTTIME,
                                                                                tblOperPdv[0].ABERTO,
                                                                                tblOperPdv[0].HASH,
                                                                                tblOperPdv[0].SANGRIAS,
                                                                                tblOperPdv[0].SUPRIMENTOS,
                                                                                tblOperPdv[0].TROCAS,
                                                                                (tblOperPdv[0].IsFECHADONull() ? null : (DateTime?)tblOperPdv[0].FECHADO),
                                                                                //tblOperPdv[0].ID_OPER,
                                                                                tblOperPdv[0].ID_USER,
                                                                                DateTime.Now);
                                        #endregion (1.2.1)UPSERT no serv.
                                    }
                                    else
                                    {
                                        #region (1.2)Consultar no serv um registro equivalente(ID_CAIXA e CURRENTTIME)

                                        taOperServ.FillByIdCaixaCurrentTime(tblOperServ, _intNoCaixa, tblOperPdv[0].CURRENTTIME); // já usa sproc

                                        if (tblOperServ != null && tblOperServ.Rows.Count > 0) // 1.2.19.13 - não era pra ser ||
                                        {
                                            #region E se tiver mais de 1 registro no retorno? AVISA O ARTUR

                                            if (tblOperServ.Rows.Count > 1) { throw new Exception($"Retorno não esperado: taOperServ.FillByIdCaixaCurrentTime({_intNoCaixa}, {tblOperPdv[0].CURRENTTIME}) retornou mais de um registro. \n\nLiga lá na Trilha, fala com o Artur (eu avisei)"); }

                                            #endregion E se tiver mais de 1 registro no retorno? AVISA O ARTUR

                                            #region Tem registro; Ver o campo ABERTO

                                            if (tblOperServ[0].ABERTO.Equals("S"))
                                            {
                                                #region (1.2.1)UPSERT no serv.
                                                taOperServ.SP_TRI_OPER_UPSERT_IDCX_CURT(tblOperPdv[0].ID_CAIXA,
                                                                                        tblOperPdv[0].DIN,
                                                                                        tblOperPdv[0].CHEQUE,
                                                                                        tblOperPdv[0].CREDITO,
                                                                                        tblOperPdv[0].DEBITO,
                                                                                        tblOperPdv[0].LOJA,
                                                                                        tblOperPdv[0].ALIMENTACAO,
                                                                                        tblOperPdv[0].REFEICAO,
                                                                                        tblOperPdv[0].PRESENTE,
                                                                                        tblOperPdv[0].COMBUSTIVEL,
                                                                                        tblOperPdv[0].OUTROS,
                                                                                        tblOperPdv[0].EXTRA_1,
                                                                                        tblOperPdv[0].EXTRA_2,
                                                                                        tblOperPdv[0].EXTRA_3,
                                                                                        tblOperPdv[0].EXTRA_4,
                                                                                        tblOperPdv[0].EXTRA_5,
                                                                                        tblOperPdv[0].EXTRA_6,
                                                                                        tblOperPdv[0].EXTRA_7,
                                                                                        tblOperPdv[0].EXTRA_8,
                                                                                        tblOperPdv[0].EXTRA_9,
                                                                                        tblOperPdv[0].EXTRA_10,
                                                                                        tblOperPdv[0].CURRENTTIME,
                                                                                        tblOperPdv[0].ABERTO,
                                                                                        tblOperPdv[0].HASH,
                                                                                        tblOperPdv[0].SANGRIAS,
                                                                                        tblOperPdv[0].SUPRIMENTOS,
                                                                                        tblOperPdv[0].TROCAS,
                                                                                        (tblOperPdv[0].IsFECHADONull() ? null : (DateTime?)tblOperPdv[0].FECHADO),
                                                                                        //tblOperPdv[0].ID_OPER,
                                                                                        tblOperPdv[0].ID_USER,
                                                                                        DateTime.Now);
                                                #endregion (1.2.1)UPSERT no serv.
                                            }
                                            //else if (tblOperServ[0].ABERTO.Equals("N"))
                                            //{
                                            //    // Faz nada.
                                            //}

                                            #endregion Tem registro; Ver o campo ABERTO
                                        }
                                        else
                                        {
                                            #region (1.2.1)UPSERT no serv.
                                            taOperServ.SP_TRI_OPER_UPSERT_IDCX_CURT(tblOperPdv[0].ID_CAIXA,
                                                                                    tblOperPdv[0].DIN,
                                                                                    tblOperPdv[0].CHEQUE,
                                                                                    tblOperPdv[0].CREDITO,
                                                                                    tblOperPdv[0].DEBITO,
                                                                                    tblOperPdv[0].LOJA,
                                                                                    tblOperPdv[0].ALIMENTACAO,
                                                                                    tblOperPdv[0].REFEICAO,
                                                                                    tblOperPdv[0].PRESENTE,
                                                                                    tblOperPdv[0].COMBUSTIVEL,
                                                                                    tblOperPdv[0].OUTROS,
                                                                                    tblOperPdv[0].EXTRA_1,
                                                                                    tblOperPdv[0].EXTRA_2,
                                                                                    tblOperPdv[0].EXTRA_3,
                                                                                    tblOperPdv[0].EXTRA_4,
                                                                                    tblOperPdv[0].EXTRA_5,
                                                                                    tblOperPdv[0].EXTRA_6,
                                                                                    tblOperPdv[0].EXTRA_7,
                                                                                    tblOperPdv[0].EXTRA_8,
                                                                                    tblOperPdv[0].EXTRA_9,
                                                                                    tblOperPdv[0].EXTRA_10,
                                                                                    tblOperPdv[0].CURRENTTIME,
                                                                                    tblOperPdv[0].ABERTO,
                                                                                    tblOperPdv[0].HASH,
                                                                                    tblOperPdv[0].SANGRIAS,
                                                                                    tblOperPdv[0].SUPRIMENTOS,
                                                                                    tblOperPdv[0].TROCAS,
                                                                                    (tblOperPdv[0].IsFECHADONull() ? null : (DateTime?)tblOperPdv[0].FECHADO),
                                                                                    //tblOperPdv[0].ID_OPER,
                                                                                    tblOperPdv[0].ID_USER,
                                                                                    DateTime.Now);
                                            #endregion (1.2.1)UPSERT no serv.
                                        }

                                        #endregion (1.2)Consultar no serv um registro equivalente(ID_CAIXA e CURRENTTIME)
                                    }
                                }
                            }

                            #endregion (1)Consultar o primeiro registro com a maior CURRENTTIME:
                        }
                    }
                    catch (Exception ex)
                    {
                        //gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
                        GravarErroSync("TRI_PDV_OPER(P->S)(PDV)", tblOperPdv, ex);
                        GravarErroSync("TRI_PDV_OPER(P->S)(SERV)", tblOperServ, ex);
                        throw ex;
                    }
                }
            }
            */
            #endregion TRI_PDV_OPER (PDV -> Serv)

            #region Cupons (ECF) -- deve ser desativado nesta versão

            //if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
            //{
            //    #region Padrão, unsynced

            //    /// SP_TRI_CUPOM_GETALL_UNSYNCED
            //    {
            //        #region Cria objetos da transação

            //        #region TableAdapters

            //        #region Cupons

            //        #region unsynced

            //        var taCupomUnsynced = new TB_CUPOMTableAdapter();
            //        taCupomUnsynced.Connection.ConnectionString = _strConnContingency;

            //        #endregion unsynced

            //        #region Serv

            //        //var taCupomServ = new TB_CUPOMTableAdapter();
            //        //taCupomServ.Connection.ConnectionString = _strConnNetwork;

            //        #endregion Serv

            //        #endregion Cupons

            //        #region Cupons / Forma de pagamento

            //        #region PDV

            //        var taCupomFmaPagtoPdv = new TB_CUPOM_FMAPAGTOTableAdapter();
            //        taCupomFmaPagtoPdv.Connection.ConnectionString = _strConnContingency;

            //        #endregion PDV

            //        //#region Serv

            //        //var taCupomFmaPagtoServ = new TB_CUPOM_FMAPAGTOTableAdapter();
            //        //taCupomFmaPagtoServ.Connection.ConnectionString = _strConnNetwork;

            //        //#endregion Serv

            //        #endregion Cupons / Forma de pagamento

            //        #region Produtos

            //        #region PDV

            //        var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter();
            //        taEstProdutoPdv.Connection.ConnectionString = _strConnContingency;

            //        #endregion PDV

            //        #region Serv

            //        var taEstProdutoServ = new TB_EST_PRODUTOTableAdapter();
            //        taEstProdutoServ.Connection.ConnectionString = _strConnNetwork;

            //        #endregion Serv

            //        #endregion Produtos

            //        #region Itens de cupom

            //        #region PDV

            //        var taCupomItemPdv = new TB_CUPOM_ITEMTableAdapter();
            //        taCupomItemPdv.Connection.ConnectionString = _strConnContingency;

            //        #endregion PDV

            //        //#region Serv

            //        //var taCupomItemServ = new TB_CUPOM_ITEMTableAdapter();
            //        //taCupomItemServ.Connection.ConnectionString = _strConnNetwork;

            //        //#endregion Serv

            //        #endregion Itens de cupom

            //        #endregion TableAdapters

            //        #region DataTables

            //        #region Cupons unsynced

            //        var tblCupomUnsynced = new FDBDataSet.TB_CUPOMDataTable();

            //        #endregion Cupons unsynced

            //        #region Cupom / Forma de pagamento

            //        #region PDV

            //        var tblCupomFmapagtoPdv = new FDBDataSet.TB_CUPOM_FMAPAGTODataTable();

            //        #endregion PDV

            //        #endregion Cupom / Forma de pagamento

            //        #region Contas a receber

            //        #region PDV

            //        var tblCtaRecPdv = new FDBDataSet.TB_CONTA_RECEBERDataTable();

            //        #endregion PDV

            //        #endregion Contas a receber

            //        #region Movimentações diárias

            //        #region PDV

            //        var tblMovDiarioPdv = new FDBDataSet.TB_MOVDIARIODataTable();

            //        #endregion PDV

            //        #endregion Movimentações diárias

            //        #region Itens de cupom

            //        #region PDV

            //        var tblCupomItemPdv = new FDBDataSet.TB_CUPOM_ITEMDataTable();

            //        #endregion PDV

            //        #endregion Itens de cupom

            //        #endregion DataTables

            //        #region AmbiMAITRE

            //        var tblMaitPedidoPdv = new DataSets.FDBDataSetMaitre.TRI_MAIT_PEDIDODataTable();
            //        var tblMaitPedidoItemPdv = new DataSets.FDBDataSetMaitre.TRI_MAIT_PEDIDO_ITEMDataTable();

            //        var tblMaitEstCompProdPdv = new FDBDataSetMaitre.TB_EST_COMP_PRODUCAODataTable();
            //        var tblMaitEstCompItemUsadoPdv = new FDBDataSetMaitre.TB_EST_COMP_ITEM_USADODataTable();

            //        #endregion AmbiMAITRE

            //        #endregion Cria objetos da transação

            //        try
            //        {
            //            #region Prepara o lote inicial para sincronização

            //            // Busca todos os cupons que foram finalizados mas não sincronizados (TIP_QUERY = 0):
            //            taCupomUnsynced.FillByCupomSync(tblCupomUnsynced, 0); // já usa sproc
            //            // Até o momento (23/02/2018), a quantidade de registros por lote 
            //            // fica definido na própria consulta de cupons (SP_TRI_CUPOM_GETALL_UNSYNCED).
            //            // O ideal seria que isso fosse parametrizado.

            //            // Indica quantos lotes de cupons foram processados:
            //            int contLote = 0;

            //            #endregion Prepara o lote inicial para sincronização

            //            #region Procedimento executado enquanto houver cupons para sincronizar

            //            if (tblCupomUnsynced != null && tblCupomUnsynced.Rows.Count > 0)
            //            {
            //                #region NOPE - CLIPP RULES NO MORE
            //                //taEstProdutoPdv.SP_TRI_BREAK_CLIPP_RULES();
            //                //taEstProdutoServ.SP_TRI_BREAK_CLIPP_RULES();
            //                #endregion NOPE - CLIPP RULES NO MORE

            //                //TODO: ver uma saída pro loop infinito caso estourar exceção no sync
            //                // Detalhe: a 2ª condição poderia estourar uma exception se a 1ª fosse verdadeira e o operador 
            //                // fosse OR (||). Mas com o operador AND (&&), a 2ª condição nem é verificada se a 1ª for verdadeira.
            //                while (!(tblCupomUnsynced is null) && tblCupomUnsynced.Rows.Count > 0)
            //                {
            //                    contLote++;

            //                    #region Sincroniza (manda para a retaguarda)

            //                    // Percorre pelos cupons do banco local
            //                    foreach (FDBDataSet.TB_CUPOMRow cupom in tblCupomUnsynced.Rows)
            //                    {
            //                        #region Gravar o cupom na retaguarda (transação)

            //                        #region Validações

            //                        // Foi necessário adaptar o COO como o ID_CUPOM negativo para sistema legado
            //                        if (cupom.IsCOONull()) { cupom.COO = cupom.ID_CUPOM * -1; }
            //                        if (cupom.IsNUM_CAIXANull()) { cupom.NUM_CAIXA = 0; }

            //                        #endregion Validações


            //                        //TransactionOptions to = new TransactionOptions();
            //                        //to.IsolationLevel = System.Transactions.IsolationLevel.Serializable;

            //                        // Inicia a transação:
            //                        //using (var transactionScopeCupons = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(1, 0, 0, 0)))
            //                        // Define a conexão com o banco do servidor:
            //                        // Define a conexão com o banco do PDV:
            //                        using (var fbConnServ = new FbConnection(_strConnNetwork))
            //                        using (var fbConnPdv = new FbConnection(_strConnContingency))
            //                        //using (var transactionScopeCupons = new TransactionScope(TransactionScopeOption.Required,
            //                        //                                                         new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }
            //                        //                                                         )) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
            //                        //using (var transactionScopeCupons = new TransactionScope())
            //                        {
            //                            // A função BeginTransaction() precisa de uma connection aberta... ¬¬
            //                            fbConnServ.Open();
            //                            fbConnPdv.Open();

            //                            using (var fbTransactServ = fbConnServ.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Wait, WaitTimeout = new TimeSpan(0, 0, _SyncTimeout) }))
            //                            using (var fbTransactPdv = fbConnPdv.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Wait, WaitTimeout = new TimeSpan(0, 0, _SyncTimeout) }))
            //                            {
            //                                try
            //                                {
            //                                    int newIdCupom = 0;
            //                                    int newIdMaitPedidoServ = 0;

            //                                    #region Gravar o cupom no servidor (capa)

            //                                    using (var fbCommCupomSyncInsertServ = new FbCommand())
            //                                    {
            //                                        #region Prepara o comando da SP_TRI_CUPOMSYNCINSERT

            //                                        fbCommCupomSyncInsertServ.Connection = fbConnServ;
            //                                        //fbCommCupomSyncInsertServ.Connection.ConnectionString = _strConnNetwork;

            //                                        fbCommCupomSyncInsertServ.CommandText = "SP_TRI_CUPOMSYNCINSERT";
            //                                        fbCommCupomSyncInsertServ.CommandType = CommandType.StoredProcedure;
            //                                        fbCommCupomSyncInsertServ.Transaction = fbTransactServ;

            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pCOO", cupom.COO);
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pCCF", (cupom.IsCCFNull() ? null : (int?)cupom.CCF));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pDT_CUPOM", cupom.DT_CUPOM);
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pHR_CUPOM", cupom.HR_CUPOM);
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pNUM_CAIXA", cupom.NUM_CAIXA);
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pID_CLIENTE", (cupom.IsID_CLIENTENull() ? null : (int?)cupom.ID_CLIENTE));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pID_VENDEDOR", (cupom.IsID_VENDEDORNull() ? null : (short?)cupom.ID_VENDEDOR));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pSTATUS", (cupom.IsSTATUSNull() ? null : cupom.STATUS));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pID_PARCELA", cupom.ID_PARCELA);
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pIND_CANCEL", (cupom.IsIND_CANCELNull() ? null : cupom.IND_CANCEL));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pID_IFS", cupom.ID_IFS);
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pID_NATOPE", cupom.ID_NATOPE);
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pVLR_TROCO", (cupom.IsVLR_TROCONull() ? null : (decimal?)cupom.VLR_TROCO));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pVLR_TOTAL", (cupom.IsVLR_TOTALNull() ? null : (decimal?)cupom.VLR_TOTAL));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pVLR_DESC", (cupom.IsVLR_DESCNull() ? null : (decimal?)cupom.VLR_DESC));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pTIP_DESC", (cupom.IsTIP_DESCNull() ? null : cupom.TIP_DESC));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pVLR_ACRES", (cupom.IsVLR_ACRESNull() ? null : (decimal?)cupom.VLR_ACRES));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pGNF", (cupom.IsGNFNull() ? null : (int?)cupom.GNF));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pCHAVE", (cupom.IsCHAVENull() ? null : cupom.CHAVE));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pTOTAL_TRIBUTOS_IBPT", (cupom.IsTOTAL_TRIBUTOS_IBPTNull() ? null : (decimal?)cupom.TOTAL_TRIBUTOS_IBPT));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pTOTAL_TRIB_FED", (cupom.IsTOTAL_TRIB_FEDNull() ? null : (decimal?)cupom.TOTAL_TRIB_FED));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pTOTAL_TRIB_EST", (cupom.IsTOTAL_TRIB_ESTNull() ? null : (decimal?)cupom.TOTAL_TRIB_EST));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pTOTAL_TRIB_MUN", (cupom.IsTOTAL_TRIB_MUNNull() ? null : (decimal?)cupom.TOTAL_TRIB_MUN));
            //                                        fbCommCupomSyncInsertServ.Parameters.Add("@pSYNCED", 1);

            //                                        #endregion Prepara o comando da SP_TRI_CUPOMSYNCINSERT

            //                                        // Executa a sproc
            //                                        newIdCupom = (int)fbCommCupomSyncInsertServ.ExecuteScalar();
            //                                    }

            //                                    #endregion Gravar o cupom no servidor (capa)

            //                                    #region Buscar as formas de pagamento do cupom no PDV

            //                                    tblCupomFmapagtoPdv.Clear();
            //                                    //TB_CUPOM_FMAPAGTO();
            //                                    //audit("SINCCONTNETDB>> " + "taCupomFmaPagtoPdv.FillByIdCupom(): " + taCupomFmaPagtoPdv.FillByIdCupom(tblCupomFmapagtoPdv, cupom.ID_CUPOM).ToString());
            //                                    taCupomFmaPagtoPdv.FillByIdCupom(tblCupomFmapagtoPdv, cupom.ID_CUPOM); // já usa sproc

            //                                    #endregion Buscar as formas de pagamento do cupom no PDV

            //                                    #region Gravar as formas de pagamento do cupom na retaguarda

            //                                    foreach (FDBDataSet.TB_CUPOM_FMAPAGTORow cupomFmapagtoPdv in tblCupomFmapagtoPdv)
            //                                    {
            //                                        using (var fbCommCupomFmapagtoSyncInsert = new FbCommand())
            //                                        {
            //                                            #region Prepara o comando da SP_TRI_CUPOMFMAPAGTSYNCINSERT

            //                                            fbCommCupomFmapagtoSyncInsert.Connection = fbConnServ;
            //                                            //fbCommCupomFmapagtoSyncInsert.Connection.ConnectionString = _strConnNetwork;
            //                                            fbCommCupomFmapagtoSyncInsert.Transaction = fbTransactServ;

            //                                            fbCommCupomFmapagtoSyncInsert.CommandText = "SP_TRI_CUPOMFMAPAGTSYNCINSERT";
            //                                            fbCommCupomFmapagtoSyncInsert.CommandType = CommandType.StoredProcedure;

            //                                            fbCommCupomFmapagtoSyncInsert.Parameters.Add("@pVLR_PAGTO", (cupomFmapagtoPdv.IsVLR_PAGTONull() ? null : (decimal?)cupomFmapagtoPdv.VLR_PAGTO));
            //                                            fbCommCupomFmapagtoSyncInsert.Parameters.Add("@pVLR_ESTORNO", (cupomFmapagtoPdv.IsVLR_ESTORNONull() ? null : (decimal?)cupomFmapagtoPdv.VLR_ESTORNO));
            //                                            fbCommCupomFmapagtoSyncInsert.Parameters.Add("@pIND_ESTORNO", (cupomFmapagtoPdv.IsIND_ESTORNONull() ? null : cupomFmapagtoPdv.IND_ESTORNO));
            //                                            fbCommCupomFmapagtoSyncInsert.Parameters.Add("@pID_CUPOM", newIdCupom);
            //                                            fbCommCupomFmapagtoSyncInsert.Parameters.Add("@pID_FMAPAGTO", cupomFmapagtoPdv.ID_FMAPAGTO);
            //                                            fbCommCupomFmapagtoSyncInsert.Parameters.Add("@pCHAVE", (cupomFmapagtoPdv.IsCHAVENull() ? null : cupomFmapagtoPdv.CHAVE));

            //                                            #endregion Prepara o comando da SP_TRI_CUPOMFMAPAGTSYNCINSERT

            //                                            // Executa a sproc
            //                                            /*newIdCupom = (int)*/
            //                                            fbCommCupomFmapagtoSyncInsert.ExecuteScalar();
            //                                        }
            //                                    }

            //                                    #endregion Gravar as formas de pagamento do cupom na retaguarda

            //                                    #region Pedido do cupom (AmbiMAITRE)

            //                                    if (!cupom.IsID_MAIT_PEDIDONull() && cupom.ID_MAIT_PEDIDO > 0)
            //                                    {
            //                                        #region Buscar o pedido do cupom (PDV) TRI_MAIT_PEDIDO

            //                                        tblMaitPedidoPdv.Clear();

            //                                        using (var taMaitPedidoPdv = new DataSets.FDBDataSetMaitreTableAdapters.TRI_MAIT_PEDIDOTableAdapter())
            //                                        {
            //                                            taMaitPedidoPdv.Connection.ConnectionString = _strConnContingency;
            //                                            taMaitPedidoPdv.FillById(tblMaitPedidoPdv, cupom.ID_MAIT_PEDIDO); // já usa sproc
            //                                        }

            //                                        #endregion Buscar o pedido do cupom (PDV) TRI_MAIT_PEDIDO

            //                                        #region Gravar o pedido vinculado com o cupom (capa) e retornar o novo ID no servidor (Serv) TRI_MAIT_PEDIDO

            //                                        foreach (var maitPedidoFromCupom in tblMaitPedidoPdv) // Então.... era pra ter apenas 1 pedido por cupom...
            //                                        {
            //                                            using (var fbCommMaitPedidoSyncInsert = new FbCommand())
            //                                            {
            //                                                #region Prepara o comando da SP_TRI_MAITPEDIDO_SYNCINSERT

            //                                                fbCommMaitPedidoSyncInsert.Connection = fbConnServ;
            //                                                //fbCommMaitPedidoSyncInsert.Connection.ConnectionString = _strConnNetwork;
            //                                                fbCommMaitPedidoSyncInsert.Transaction = fbTransactServ;

            //                                                fbCommMaitPedidoSyncInsert.CommandText = "SP_TRI_MAITPEDIDO_SYNCINSERT";
            //                                                fbCommMaitPedidoSyncInsert.CommandType = CommandType.StoredProcedure;

            //                                                fbCommMaitPedidoSyncInsert.Parameters.Add("@pTS_EMISSAO", DateTime.Now);
            //                                                fbCommMaitPedidoSyncInsert.Parameters.Add("@pID_USER", maitPedidoFromCupom.ID_USER);
            //                                                fbCommMaitPedidoSyncInsert.Parameters.Add("@pNR_PEDIDO", maitPedidoFromCupom.NR_PEDIDO);
            //                                                fbCommMaitPedidoSyncInsert.Parameters.Add("@pABERTO", maitPedidoFromCupom.ABERTO);
            //                                                fbCommMaitPedidoSyncInsert.Parameters.Add("@pOBSERVACAO", (maitPedidoFromCupom.IsOBSERVACAONull() ? string.Empty : maitPedidoFromCupom.OBSERVACAO));
            //                                                fbCommMaitPedidoSyncInsert.Parameters.Add("@pID_CAIXA", maitPedidoFromCupom.ID_CAIXA);

            //                                                #endregion Prepara o comando da SP_TRI_MAITPEDIDO_SYNCINSERT

            //                                                // Executa a sproc
            //                                                newIdMaitPedidoServ = (int)fbCommMaitPedidoSyncInsert.ExecuteScalar();
            //                                                audit("SINCCONTNETDB>> ", string.Format("SP_TRI_MAITPEDIDO_SYNCINSERT(pTS_EMISSAO: {0}, pID_USER: {1}, pNR_PEDIDO: {2}, pABERTO: {3}, pOBSERVACAO: {4}, pID_CAIXA: {5}): {6}",
            //                                                                    DateTime.Now,
            //                                                                    maitPedidoFromCupom.ID_USER,
            //                                                                    maitPedidoFromCupom.NR_PEDIDO,
            //                                                                    maitPedidoFromCupom.ABERTO,
            //                                                                    (maitPedidoFromCupom.IsOBSERVACAONull() ? string.Empty : maitPedidoFromCupom.OBSERVACAO),
            //                                                                    maitPedidoFromCupom.ID_CAIXA,
            //                                                                    newIdMaitPedidoServ));
            //                                            }

            //                                            #region Gravar o vínculo cupom/pedido no servidor (Serv) TRI_MAIT_PEDIDO_CUPOM

            //                                            // TRI_MAIT_PEDIDO_CUPOM
            //                                            using (var fbCommMaitPedCupomSyncInsert = new FbCommand())
            //                                            {
            //                                                #region Prepara o comando da SP_TRI_MAIT_PEDCUPOM_SYNCINSERT

            //                                                fbCommMaitPedCupomSyncInsert.Connection = fbConnServ;
            //                                                //fbCommMaitPedCupomSyncInsert.Connection.ConnectionString = _strConnNetwork;
            //                                                fbCommMaitPedCupomSyncInsert.Transaction = fbTransactServ;

            //                                                fbCommMaitPedCupomSyncInsert.CommandText = "SP_TRI_MAIT_PEDCUPOM_SYNCINSERT";
            //                                                fbCommMaitPedCupomSyncInsert.CommandType = CommandType.StoredProcedure;

            //                                                fbCommMaitPedCupomSyncInsert.Parameters.Add("@pID_MAIT_PEDIDO", newIdMaitPedidoServ);
            //                                                fbCommMaitPedCupomSyncInsert.Parameters.Add("@pID_CUPOM", newIdCupom);

            //                                                #endregion Prepara o comando da SP_TRI_MAIT_PEDCUPOM_SYNCINSERT

            //                                                // Executa a sproc
            //                                                audit("SINCCONTNETDB>> ", string.Format("SP_TRI_MAIT_PEDCUPOM_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_CUPOM: {1}): {2}",
            //                                                                    newIdMaitPedidoServ,
            //                                                                    newIdCupom,
            //                                                                    fbCommMaitPedCupomSyncInsert.ExecuteNonQuery()));
            //                                            }

            //                                            #endregion Gravar o vínculo cupom/pedido no servidor (Serv) TRI_MAIT_PEDIDO_CUPOM
            //                                        }

            //                                        #endregion Gravar o pedido vinculado com o cupom (capa) e retornar o novo ID no servidor (Serv) TRI_MAIT_PEDIDO
            //                                    }

            //                                    #endregion Pedido do cupom (AmbiMAITRE)

            //                                    #region Itens do cupom do PDV

            //                                    #region Consultar os itens do cupom do PDV

            //                                    tblCupomItemPdv.Clear();
            //                                    // Busca os itens do cupom pelo ID_CUPOM local (PDV):
            //                                    //audit("SINCCONTNETDB>> " + "taCupomItemPdv.FillByIdCupom(): " + taCupomItemPdv.FillByIdCupom(tblCupomItemPdv, cupom.ID_CUPOM).ToString());
            //                                    // SP_TRI_CUPOMITEMGET
            //                                    taCupomItemPdv.FillByIdCupom(tblCupomItemPdv, cupom.ID_CUPOM); // já usa sproc: SP_TRI_CUPOMITEMGET

            //                                    foreach (FDBDataSet.TB_CUPOM_ITEMRow cupomItem in tblCupomItemPdv.Rows)
            //                                    {
            //                                        // Os itens do cupom devem referenciar o novo ID do cupom da retaguarda

            //                                        #region Gravar os itens do cupom

            //                                        int newIdCupomItem = 0;

            //                                        using (var fbCommCupomItemSyncInsert = new FbCommand())
            //                                        {
            //                                            #region Prepara o comando da SP_TRI_CUPOMITEMSYNCINSERT

            //                                            fbCommCupomItemSyncInsert.Connection = fbConnServ;
            //                                            //fbCommCupomItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
            //                                            fbCommCupomItemSyncInsert.Transaction = fbTransactServ;

            //                                            fbCommCupomItemSyncInsert.CommandText = "SP_TRI_CUPOMITEMSYNCINSERT";
            //                                            fbCommCupomItemSyncInsert.CommandType = CommandType.StoredProcedure;

            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pID_CUPOM", newIdCupom);
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pID_IDENTIF", cupomItem.ID_IDENTIF);
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pNUM_ITEM", (cupomItem.IsNUM_ITEMNull() ? null : (int?)cupomItem.NUM_ITEM));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pQTD_ITEM", (cupomItem.IsQTD_ITEMNull() ? null : (decimal?)cupomItem.QTD_ITEM));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_UNIT", (cupomItem.IsVLR_UNITNull() ? null : (decimal?)cupomItem.VLR_UNIT));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pPRC_CUSTO", (cupomItem.IsPRC_CUSTONull() ? null : (decimal?)cupomItem.PRC_CUSTO));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pALI_ICM", (cupomItem.IsALI_ICMNull() ? null : (decimal?)cupomItem.ALI_ICM));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_ICM", (cupomItem.IsVLR_ICMNull() ? null : (decimal?)cupomItem.VLR_ICM));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pCOD_TOTALP", (cupomItem.IsCOD_TOTALPNull() ? null : cupomItem.COD_TOTALP));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pORD_APLICA", (cupomItem.IsORD_APLICANull() ? null : cupomItem.ORD_APLICA));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pITEM_CANCEL", (cupomItem.IsITEM_CANCELNull() ? null : cupomItem.ITEM_CANCEL));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pCST", (cupomItem.IsCSTNull() ? null : cupomItem.CST));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pUNI_MEDIDA", (cupomItem.IsUNI_MEDIDANull() ? null : cupomItem.UNI_MEDIDA));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pCASAS_QTD", (cupomItem.IsCASAS_QTDNull() ? null : cupomItem.CASAS_QTD));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pCASAS_VLR", (cupomItem.IsCASAS_VLRNull() ? null : cupomItem.CASAS_VLR));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pTIPO_DESC", (cupomItem.IsTIPO_DESCNull() ? null : cupomItem.TIPO_DESC));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pIAT", (cupomItem.IsIATNull() ? null : cupomItem.IAT));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pIPPT", (cupomItem.IsIPPTNull() ? null : cupomItem.IPPT));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pCOD_BARRA", (cupomItem.IsCOD_BARRANull() ? null : cupomItem.COD_BARRA));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_ACRE", (cupomItem.IsVLR_ACRENull() ? null : (decimal?)cupomItem.VLR_ACRE));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_PIS", (cupomItem.IsVLR_PISNull() ? null : (decimal?)cupomItem.VLR_PIS));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_COFINS", (cupomItem.IsVLR_COFINSNull() ? null : (decimal?)cupomItem.VLR_COFINS));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pCHAVE", (cupomItem.IsCHAVENull() ? null : cupomItem.CHAVE));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pCST_PIS", (cupomItem.IsCST_PISNull() ? null : cupomItem.CST_PIS));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pCST_COFINS", (cupomItem.IsCST_COFINSNull() ? null : cupomItem.CST_COFINS));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pCFOP", (cupomItem.IsCFOPNull() ? null : cupomItem.CFOP));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_TRIBUTOS_IBPT", (cupomItem.IsVLR_TRIBUTOS_IBPTNull() ? null : (decimal?)cupomItem.VLR_TRIBUTOS_IBPT));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pALIQ_ENCONT_IBPT", (cupomItem.IsALIQ_ENCONT_IBPTNull() ? null : cupomItem.ALIQ_ENCONT_IBPT));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pDT_ITEM", (cupomItem.IsDT_ITEMNull() ? null : (DateTime?)cupomItem.DT_ITEM));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pHR_ITEM", cupomItem.HR_ITEM);
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_TRIB_FED", (cupomItem.IsVLR_TRIB_FEDNull() ? null : (decimal?)cupomItem.VLR_TRIB_FED));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_TRIB_EST", (cupomItem.IsVLR_TRIB_ESTNull() ? null : (decimal?)cupomItem.VLR_TRIB_EST));
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_TRIB_MUN", (cupomItem.IsVLR_TRIB_MUNNull() ? null : (decimal?)cupomItem.VLR_TRIB_MUN)); // CORRETO
            //                                                                                                                                                                                    //fbCommCupomItemSyncInsert.Parameters.Add("@pVLR_TRIB_EST", (cupomItem.IsVLR_TRIB_MUNNull() ? null : (decimal?)cupomItem.VLR_TRIB_MUN)); // ERRADO, para testar transaction
            //                                            fbCommCupomItemSyncInsert.Parameters.Add("@pDESCRICAO", (cupomItem.IsDESCRICAONull() ? null : cupomItem.DESCRICAO));

            //                                            #endregion Prepara o comando da SP_TRI_CUPOMFMAPAGTSYNCINSERT

            //                                            try
            //                                            {
            //                                                // Executa a sproc
            //                                                newIdCupomItem = (int)fbCommCupomItemSyncInsert.ExecuteScalar();
            //                                            }
            //                                            catch (Exception ex)
            //                                            {
            //                                                gravarMensagemErro($"Erro ao sincronizar item de cupom. \nID_CUPOM: {newIdCupom} \nID_IDENTIF: {cupomItem.ID_IDENTIF} \n\nMensagem completa: { RetornarMensagemErro(ex, true)}");
            //                                                throw ex;
            //                                            }

            //                                            //audit("SINCCONTNETDB>> ", "SP_TRI_CUPOMITEMSYNCINSERT(): " + newIdCupomItem.ToString());
            //                                        }

            //                                        #endregion Gravar os itens do cupom

            //                                        #region Buscar os itens de pedido (AmbiMAITRE) (PDV)

            //                                        if (!cupomItem.IsID_MAIT_PEDIDO_ITEMNull() && cupomItem.ID_MAIT_PEDIDO_ITEM > 0)
            //                                        {
            //                                            #region Busca os itens do pedido (PDV) TRI_MAIT_PEDIDO_ITEM

            //                                            tblMaitPedidoItemPdv.Clear();

            //                                            using (var taMaitPedItemPdv = new DataSets.FDBDataSetMaitreTableAdapters.TRI_MAIT_PEDIDO_ITEMTableAdapter())
            //                                            {
            //                                                taMaitPedItemPdv.Connection.ConnectionString = _strConnContingency;
            //                                                // TRI_MAIT_PEDIDO_ITEM
            //                                                taMaitPedItemPdv.FillById(tblMaitPedidoItemPdv,
            //                                                                          cupomItem.ID_MAIT_PEDIDO_ITEM); // já usa sproc
            //                                            }

            //                                            #endregion Busca os itens do pedido (PDV)

            //                                            #region Procedimento de gravação os itens de pedido (AmbiMAITRE) (servidor) TRI_MAIT_PEDIDO_ITEM

            //                                            foreach (var pedItemPdv in tblMaitPedidoItemPdv) // Deve ter apenas 1 item de pedido por item de cupom
            //                                            {
            //                                                int newIdMaitPedItemServ = 0;

            //                                                #region Gravar item de pedido (serv) TRI_MAIT_PEDIDO_ITEM

            //                                                // TRI_MAIT_PEDIDO_ITEM
            //                                                using (var fbCommMaitPedItemSyncInsert = new FbCommand())
            //                                                {
            //                                                    #region Prepara o comando da SP_TRI_MAITPEDITEM_SYNCINSERT

            //                                                    fbCommMaitPedItemSyncInsert.Connection = fbConnServ;
            //                                                    //fbCommMaitPedItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
            //                                                    fbCommMaitPedItemSyncInsert.Transaction = fbTransactServ;

            //                                                    fbCommMaitPedItemSyncInsert.CommandText = "SP_TRI_MAITPEDITEM_SYNCINSERT";
            //                                                    fbCommMaitPedItemSyncInsert.CommandType = CommandType.StoredProcedure;

            //                                                    fbCommMaitPedItemSyncInsert.Parameters.Add("@pID_MAIT_PEDIDO", newIdMaitPedidoServ);
            //                                                    fbCommMaitPedItemSyncInsert.Parameters.Add("@pID_IDENTIFICADOR", pedItemPdv.ID_IDENTIFICADOR);
            //                                                    fbCommMaitPedItemSyncInsert.Parameters.Add("@pQTD_ITEM", pedItemPdv.QTD_ITEM);

            //                                                    #endregion Prepara o comando da SP_TRI_MAITPEDITEM_SYNCINSERT

            //                                                    newIdMaitPedItemServ = (int)fbCommMaitPedItemSyncInsert.ExecuteScalar();

            //                                                    // Executa a sproc
            //                                                    audit("SINCCONTNETDB >> ", string.Format("SP_TRI_MAITPEDITEM_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_IDENTIFICADOR: {1}, pQTD_ITEM: {2}): {3}",
            //                                                                        newIdMaitPedidoServ,
            //                                                                        pedItemPdv.ID_IDENTIFICADOR,
            //                                                                        pedItemPdv.QTD_ITEM,
            //                                                                        newIdMaitPedItemServ));
            //                                                }

            //                                                #endregion Gravar item de pedido (serv)

            //                                                #region Gravar o vínculo item de pedido / item de cupom TRI_MAIT_PED_ITEM_CUPOM_ITEM

            //                                                // TRI_MAIT_PED_ITEM_CUPOM_ITEM
            //                                                using (var fbCommMaitPedItemCupomItemSyncInsert = new FbCommand())
            //                                                {
            //                                                    #region Prepara o comando da SP_TRI_MTPDITM_CPITM_SYNCINSERT

            //                                                    fbCommMaitPedItemCupomItemSyncInsert.Connection = fbConnServ;
            //                                                    //fbCommMaitPedItemCupomItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
            //                                                    fbCommMaitPedItemCupomItemSyncInsert.Transaction = fbTransactServ;

            //                                                    fbCommMaitPedItemCupomItemSyncInsert.CommandText = "SP_TRI_MTPDITM_CPITM_SYNCINSERT";
            //                                                    fbCommMaitPedItemCupomItemSyncInsert.CommandType = CommandType.StoredProcedure;

            //                                                    fbCommMaitPedItemCupomItemSyncInsert.Parameters.Add("@pID_MAIT_PEDIDO_ITEM", newIdMaitPedItemServ);
            //                                                    fbCommMaitPedItemCupomItemSyncInsert.Parameters.Add("@pID_ITEMCUP", newIdCupomItem);

            //                                                    #endregion Prepara o comando da SP_TRI_MTPDITM_CPITM_SYNCINSERT

            //                                                    // Executa a sproc
            //                                                    audit("SINCCONTNETDB>> ", string.Format("SP_TRI_MTPDITM_CPITM_SYNCINSERT(pID_MAIT_PEDIDO_ITEM: {0}, pID_ITEMCUP: {1}): {2}",
            //                                                                        newIdMaitPedItemServ,
            //                                                                        newIdCupomItem,
            //                                                                        fbCommMaitPedItemCupomItemSyncInsert.ExecuteNonQuery()));
            //                                                }

            //                                                #endregion Gravar o vínculo item de pedido / item de cupom

            //                                                #region Procedimento de gravação de itens compostos

            //                                                if (!cupomItem.IsID_COMPPRONull())
            //                                                {
            //                                                    #region Consulta a ordem de produção de compostos (PDV) TB_EST_COMP_PRODUCAO

            //                                                    using (var taEstCompProdPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_PRODUCAOTableAdapter())
            //                                                    {
            //                                                        taEstCompProdPdv.Connection.ConnectionString = _strConnContingency;
            //                                                        tblMaitEstCompProdPdv.Clear();
            //                                                        // TB_EST_COMP_PRODUCAO
            //                                                        // SP_TRI_MAIT_COMPPROD_GETBYID
            //                                                        audit("SINCCONTNETDB>> ", string.Format("taEstCompProdPdv.FillById(ID_COMPPRO: {0}): {1}",
            //                                                                            cupomItem.ID_COMPPRO,
            //                                                                            taEstCompProdPdv.FillById(tblMaitEstCompProdPdv,
            //                                                                                                      cupomItem.ID_COMPPRO).ToString())); // já usa sproc
            //                                                    }

            //                                                    if (tblMaitEstCompProdPdv is null || tblMaitEstCompProdPdv.Rows.Count <= 0)
            //                                                    {
            //                                                        throw new Exception("A consulta de item composto não teve o retorno esperado (nulo ou nenhum registro).");
            //                                                    }

            //                                                    #endregion Consulta a ordem de produção de compostos (PDV) TB_EST_COMP_PRODUCAO

            //                                                    #region Gravar a produção de composto e seus vínculos

            //                                                    int newIdComProd = 0;

            //                                                    foreach (var compProdPdv in tblMaitEstCompProdPdv) // Deve ter apenas 1 produção de composto por item de pedido
            //                                                    {
            //                                                        #region Grava a ordem de produção de composto (Serv) TB_EST_COMP_PRODUCAO

            //                                                        // TB_EST_COMP_PRODUCAO
            //                                                        using (var fbCommMaitCompProdSyncInsert = new FbCommand())
            //                                                        {
            //                                                            #region Prepara o comando da SP_TRI_MAIT_COMPPROD_SYNCINSERT

            //                                                            fbCommMaitCompProdSyncInsert.Connection = fbConnServ;
            //                                                            fbCommMaitCompProdSyncInsert.Transaction = fbTransactServ;

            //                                                            fbCommMaitCompProdSyncInsert.CommandText = "SP_TRI_MAIT_COMPPROD_SYNCINSERT";
            //                                                            fbCommMaitCompProdSyncInsert.CommandType = CommandType.StoredProcedure;

            //                                                            fbCommMaitCompProdSyncInsert.Parameters.Add("@pID_COMPOSICAO", compProdPdv.ID_COMPOSICAO);
            //                                                            fbCommMaitCompProdSyncInsert.Parameters.Add("@pQT_COMPPRO", compProdPdv.IsQT_COMPPRONull() ? null : (decimal?)compProdPdv.QT_COMPPRO);
            //                                                            fbCommMaitCompProdSyncInsert.Parameters.Add("@pDT_COMPPRO", compProdPdv.IsDT_COMPPRONull() ? null : (DateTime?)compProdPdv.DT_COMPPRO);
            //                                                            fbCommMaitCompProdSyncInsert.Parameters.Add("@pHR_COMPPRO", compProdPdv.IsHR_COMPPRONull() ? null : (TimeSpan?)compProdPdv.HR_COMPPRO);
            //                                                            fbCommMaitCompProdSyncInsert.Parameters.Add("@pOBSERVACAO", compProdPdv.IsOBSERVACAONull() ? null : compProdPdv.OBSERVACAO);
            //                                                            fbCommMaitCompProdSyncInsert.Parameters.Add("@pGERADO", compProdPdv.IsGERADONull() ? null : compProdPdv.GERADO);
            //                                                            fbCommMaitCompProdSyncInsert.Parameters.Add("@pMONTADO", compProdPdv.IsMONTADONull() ? null : compProdPdv.MONTADO);
            //                                                            fbCommMaitCompProdSyncInsert.Parameters.Add("@pID_IDENTIFICADOR", compProdPdv.ID_IDENTIFICADOR);

            //                                                            #endregion Prepara o comando da SP_TRI_MAIT_COMPPROD_SYNCINSERT

            //                                                            newIdComProd = (int)fbCommMaitCompProdSyncInsert.ExecuteScalar();

            //                                                            // Executa a sproc
            //                                                            audit("SINCCONTNETDB>> ", string.Format("SP_TRI_MAIT_COMPPROD_SYNCINSERT(pID_COMPOSICAO: {0}, pQT_COMPPRO: {1}, pDT_COMPPRO: {2}, pHR_COMPPRO: {3}, pOBSERVACAO: {4}, pGERADO: {5}, pMONTADO: {6}, pID_IDENTIFICADOR: {7}): {8}",
            //                                                                                compProdPdv.ID_COMPOSICAO,
            //                                                                                compProdPdv.IsQT_COMPPRONull() ? string.Empty : compProdPdv.QT_COMPPRO.ToString(),
            //                                                                                compProdPdv.IsDT_COMPPRONull() ? string.Empty : compProdPdv.DT_COMPPRO.ToString(),
            //                                                                                compProdPdv.IsHR_COMPPRONull() ? string.Empty : compProdPdv.HR_COMPPRO.ToString(),
            //                                                                                compProdPdv.IsOBSERVACAONull() ? string.Empty : compProdPdv.OBSERVACAO,
            //                                                                                compProdPdv.IsGERADONull() ? string.Empty : compProdPdv.GERADO,
            //                                                                                compProdPdv.IsMONTADONull() ? string.Empty : compProdPdv.MONTADO,
            //                                                                                compProdPdv.ID_IDENTIFICADOR,
            //                                                                                newIdComProd));
            //                                                        }

            //                                                        #endregion Grava a ordem de produção de composto (Serv) TB_EST_COMP_PRODUCAO

            //                                                        #region Consulta os componentes da ordem de produção de composto (PDV) TB_EST_COMP_ITEM_USADO

            //                                                        // Buscar os componentes
            //                                                        using (var taEstCompItemUsadoPdv = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_ITEM_USADOTableAdapter())
            //                                                        {
            //                                                            taEstCompItemUsadoPdv.Connection.ConnectionString = _strConnContingency;
            //                                                            tblMaitEstCompItemUsadoPdv.Clear();
            //                                                            audit("SINCCONTNETDB>> ", string.Format("taEstCompItemUsadoPdv.FillByIdCompProd(pID_COMPPROD: {0}): {1}",
            //                                                                                compProdPdv.ID_COMPPRO,
            //                                                                                taEstCompItemUsadoPdv.FillByIdCompProd(tblMaitEstCompItemUsadoPdv, compProdPdv.ID_COMPPRO))); // já usa sproc
            //                                                        }

            //                                                        #endregion Consulta os componentes da ordem de produção de composto (PDV) TB_EST_COMP_ITEM_USADO

            //                                                        #region Grava os componentes da ordem de produção de composto (Serv) TB_EST_COMP_ITEM_USADO

            //                                                        // Percorre por cada componente para sync
            //                                                        foreach (var compItemUsado in tblMaitEstCompItemUsadoPdv)
            //                                                        {
            //                                                            // TB_EST_COMP_ITEM_USADO
            //                                                            using (var fbCommMaitCompItemUsadoSyncInsert = new FbCommand())
            //                                                            {
            //                                                                #region Prepara o comando da SP_TRI_MAIT_COMPITMUSD_SYNCNSRT

            //                                                                fbCommMaitCompItemUsadoSyncInsert.Connection = fbConnServ;
            //                                                                fbCommMaitCompItemUsadoSyncInsert.Transaction = fbTransactServ;

            //                                                                fbCommMaitCompItemUsadoSyncInsert.CommandText = "SP_TRI_MAIT_COMPITMUSD_SYNCNSRT";
            //                                                                fbCommMaitCompItemUsadoSyncInsert.CommandType = CommandType.StoredProcedure;

            //                                                                fbCommMaitCompItemUsadoSyncInsert.Parameters.Add("@pID_COMPPROD", newIdComProd);
            //                                                                fbCommMaitCompItemUsadoSyncInsert.Parameters.Add("@pQTD_ITEM", compItemUsado.IsQTD_ITEMNull() ? null : (decimal?)compItemUsado.QTD_ITEM);
            //                                                                fbCommMaitCompItemUsadoSyncInsert.Parameters.Add("@pDT_BAIXA", compItemUsado.IsDT_BAIXANull() ? null : (DateTime?)compItemUsado.DT_BAIXA);
            //                                                                fbCommMaitCompItemUsadoSyncInsert.Parameters.Add("@pHR_BAIXA", compItemUsado.IsHR_BAIXANull() ? null : (TimeSpan?)compItemUsado.HR_BAIXA);
            //                                                                fbCommMaitCompItemUsadoSyncInsert.Parameters.Add("@pPRC_MEDIO", compItemUsado.IsPRC_MEDIONull() ? null : (decimal?)compItemUsado.PRC_MEDIO);
            //                                                                fbCommMaitCompItemUsadoSyncInsert.Parameters.Add("@pPRC_VENDA", compItemUsado.PRC_VENDA);
            //                                                                fbCommMaitCompItemUsadoSyncInsert.Parameters.Add("@pID_IDENTIFICADOR", compItemUsado.ID_IDENTIFICADOR);

            //                                                                #endregion Prepara o comando da SP_TRI_MAIT_COMPITMUSD_SYNCNSRT

            //                                                                // Executa a sproc
            //                                                                audit("SINCCONTNETDB>> ", string.Format("SP_TRI_MAIT_COMPITMUSD_SYNCNSRT(pID_COMPPROD: {0}, pQTD_ITEM: {1}, pDT_BAIXA: {2}, pHR_BAIXA: {3}, pPRC_MEDIO: {4}, pPRC_VENDA: {5}, pID_IDENTIFICADOR: {6}): {7}",
            //                                                                                    newIdComProd,
            //                                                                                    compItemUsado.IsQTD_ITEMNull() ? null : (decimal?)compItemUsado.QTD_ITEM,
            //                                                                                    compItemUsado.IsDT_BAIXANull() ? null : (DateTime?)compItemUsado.DT_BAIXA,
            //                                                                                    compItemUsado.IsHR_BAIXANull() ? null : (TimeSpan?)compItemUsado.HR_BAIXA,
            //                                                                                    compItemUsado.IsPRC_MEDIONull() ? null : (decimal?)compItemUsado.PRC_MEDIO,
            //                                                                                    compItemUsado.PRC_VENDA,
            //                                                                                    compItemUsado.ID_IDENTIFICADOR,
            //                                                                                    fbCommMaitCompItemUsadoSyncInsert.ExecuteNonQuery()));
            //                                                            }
            //                                                        }

            //                                                        #endregion Grava os componentes da ordem de produção de composto (Serv) TB_EST_COMP_ITEM_USADO

            //                                                        #region Gravar a ligação de produção de composição com item de pedido

            //                                                        // TRI_MAIT_PED_ITEM_COMPPROD
            //                                                        using (var fbCommMaitPedItemCompProdSyncInsert = new FbCommand())
            //                                                        {
            //                                                            #region Prepara o comando da SP_TRI_MT_PDITM_COMPRD_SYNCNSRT

            //                                                            fbCommMaitPedItemCompProdSyncInsert.Connection = fbConnServ;
            //                                                            fbCommMaitPedItemCompProdSyncInsert.Transaction = fbTransactServ;

            //                                                            fbCommMaitPedItemCompProdSyncInsert.CommandText = "SP_TRI_MT_PDITM_COMPRD_SYNCNSRT";
            //                                                            fbCommMaitPedItemCompProdSyncInsert.CommandType = CommandType.StoredProcedure;

            //                                                            fbCommMaitPedItemCompProdSyncInsert.Parameters.Add("@pID_MAIT_PEDIDO_ITEM", newIdMaitPedItemServ);
            //                                                            fbCommMaitPedItemCompProdSyncInsert.Parameters.Add("@pID_COMPPRO", newIdComProd);

            //                                                            #endregion Prepara o comando da SP_TRI_MT_PDITM_COMPRD_SYNCNSRT

            //                                                            // Executa a sproc
            //                                                            audit("SINCCONTNETDB>> ", string.Format("SP_TRI_MT_PDITM_COMPRD_SYNCNSRT(pID_MAIT_PEDIDO_ITEM: {0}, pID_COMPPRO: {1}): {2}",
            //                                                                                newIdMaitPedItemServ,
            //                                                                                newIdComProd,
            //                                                                                fbCommMaitPedItemCompProdSyncInsert.ExecuteNonQuery()));
            //                                                        }

            //                                                        #endregion Gravar a ligação de produção de composição com item de pedido
            //                                                    }

            //                                                    #endregion Gravar a produção de composto e seus vínculos

            //                                                }

            //                                                #endregion Procedimento de gravação de itens compostos
            //                                            }

            //                                            #endregion Procedimento de gravação os itens de pedido (AmbiMAITRE) (servidor)
            //                                        }

            //                                        #endregion Buscar os itens de pedido (AmbiMAITRE) (PDV)

            //                                        //if (cupom.IsQTD_MAIT_PED_CUPOMNull() || cupom.QTD_MAIT_PED_CUPOM <= 0)

            //                                        #region Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)

            //                                        #region Atualizar no servidor a quantidade em estoque

            //                                        using (var fbCommEstProdutoQtdServ = new FbCommand())
            //                                        {
            //                                            #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

            //                                            fbCommEstProdutoQtdServ.Connection = fbConnServ;
            //                                            //fbCommEstProdutoQtdServ.Connection.ConnectionString = _strConnNetwork;
            //                                            fbCommEstProdutoQtdServ.Transaction = fbTransactServ;

            //                                            fbCommEstProdutoQtdServ.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
            //                                            fbCommEstProdutoQtdServ.CommandType = CommandType.StoredProcedure;

            //                                            fbCommEstProdutoQtdServ.Parameters.Add("@pQTD_ITEM", cupomItem.QTD_ITEM);
            //                                            fbCommEstProdutoQtdServ.Parameters.Add("@pID_IDENTIF", cupomItem.ID_IDENTIF);
            //                                            fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPPRO", cupomItem.IsID_COMPPRONull() ? 0 : cupomItem.ID_COMPPRO); // AmbiMAITRE
            //                                            fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPOSICAO", cupomItem.IsID_COMPOSICAONull() ? 0 : cupomItem.ID_COMPOSICAO);

            //                                            #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

            //                                            try
            //                                            {
            //                                                // Executa a sproc
            //                                                fbCommEstProdutoQtdServ.ExecuteScalar();
            //                                            }
            //                                            catch (Exception ex)
            //                                            {
            //                                                gravarMensagemErro("Erro ao deduzir quantidade em estoque (Serv): \npQTD_ITEM=" + cupomItem.QTD_ITEM.ToString() +
            //                                                                   "\npID_IDENTIF=" + cupomItem.ID_IDENTIF.ToString() +
            //                                                                    " \nMais infos: " + RetornarMensagemErro(ex, true));
            //                                                throw ex;
            //                                            }
            //                                        }

            //                                        #endregion Atualizar no servidor a quantidade em estoque

            //                                        #region Atualizar no PDV a quantidade em estoque

            //                                        // Já que todo o cadastro de produtos foi copiado do Serv pro PDV na etapa anterior, 
            //                                        // as quantidades em estoque devem ser redefinidas
            //                                        using (var fbCommEstProdutoQtdPdv = new FbCommand())
            //                                        {
            //                                            #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

            //                                            fbCommEstProdutoQtdPdv.Connection = fbConnPdv;
            //                                            //fbCommEstProdutoQtdPdv.Connection.ConnectionString = _strConnContingency;
            //                                            fbCommEstProdutoQtdPdv.Transaction = fbTransactPdv;

            //                                            fbCommEstProdutoQtdPdv.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
            //                                            fbCommEstProdutoQtdPdv.CommandType = CommandType.StoredProcedure;

            //                                            fbCommEstProdutoQtdPdv.Parameters.Add("@pQTD_ITEM", cupomItem.QTD_ITEM);
            //                                            fbCommEstProdutoQtdPdv.Parameters.Add("@pID_IDENTIF", cupomItem.ID_IDENTIF);
            //                                            fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPPRO", cupomItem.IsID_COMPPRONull() ? 0 : cupomItem.ID_COMPPRO); // AmbiMAITRE
            //                                            fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPOSICAO", cupomItem.IsID_COMPOSICAONull() ? 0 : cupomItem.ID_COMPOSICAO);

            //                                            #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

            //                                            try
            //                                            {
            //                                                // Executa a sproc
            //                                                fbCommEstProdutoQtdPdv.ExecuteScalar();
            //                                            }
            //                                            catch (Exception ex)
            //                                            {
            //                                                gravarMensagemErro("Erro ao deduzir quantidade em estoque (PDV): \npQTD_ITEM=" + cupomItem.QTD_ITEM.ToString() +
            //                                                                   "\npID_IDENTIF=" + cupomItem.ID_IDENTIF.ToString() +
            //                                                                    " \nMais infos: " + RetornarMensagemErro(ex, true));
            //                                                throw ex;
            //                                            }
            //                                        }

            //                                        #endregion Atualizar no PDV a quantidade em estoque

            //                                        #endregion Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)
            //                                    }

            //                                    #endregion Consultar os itens do cupom do PDV

            //                                    #endregion Itens do cupom do PDV

            //                                    #region Vendas a prazo

            //                                    #region Verifica se o cupom é a prazo

            //                                    // Como verificar se o cupom é uma venda a prazo?
            //                                    if (!cupom.IsQTD_CTARECNull() && cupom.QTD_CTAREC > 0)
            //                                    {

            //                                        using (var taCtaRecPdv = new TB_CONTA_RECEBERTableAdapter())
            //                                        {
            //                                            taCtaRecPdv.Connection.ConnectionString = _strConnContingency;

            //                                            tblCtaRecPdv.Clear();
            //                                            // Consultar todas as contas a receber do cupom
            //                                            //audit("SINCCONTNETDB>> " + "taCtaRecPdv.FillByIdCupom(): " + /*taCtaRecPdv.FillByIdCupom(tblCtaRecPdv, cupom.ID_CUPOM)*/.ToString());
            //                                            taCtaRecPdv.FillByIdCupom(tblCtaRecPdv, cupom.ID_CUPOM); // já usa sproc
            //                                        }

            //                                        // Percorre por cada conta a receber que o cupom possui:
            //                                        foreach (FDBDataSet.TB_CONTA_RECEBERRow ctaRecPdv in tblCtaRecPdv)
            //                                        {
            //                                            int newIdCtarec = 0;

            //                                            #region Grava conta a receber na retaguarda

            //                                            // TB_CONTA_RECEBER
            //                                            using (var fbCommCtaRecSyncInsertServ = new FbCommand())
            //                                            {
            //                                                #region Prepara o comando da SP_TRI_CTAREC_SYNC_INSERT

            //                                                fbCommCtaRecSyncInsertServ.Connection = fbConnServ;
            //                                                //fbCommCtaRecSyncInsertServ.Connection.ConnectionString = _strConnNetwork;
            //                                                fbCommCtaRecSyncInsertServ.Transaction = fbTransactServ;

            //                                                fbCommCtaRecSyncInsertServ.CommandText = "SP_TRI_CTAREC_SYNC_INSERT";
            //                                                fbCommCtaRecSyncInsertServ.CommandType = CommandType.StoredProcedure;

            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pDOCUMENTO", ctaRecPdv.DOCUMENTO);
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pHISTORICO", (ctaRecPdv.IsHISTORICONull() ? null : ctaRecPdv.HISTORICO));
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pDT_EMISSAO", ctaRecPdv.DT_EMISSAO);
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pDT_VENCTO", ctaRecPdv.DT_VENCTO);
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pVLR_CTAREC", ctaRecPdv.VLR_CTAREC);
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pTIP_CTAREC", ctaRecPdv.TIP_CTAREC);
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pID_PORTADOR", ctaRecPdv.ID_PORTADOR);
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pID_CLIENTE", ctaRecPdv.ID_CLIENTE);
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pINV_REFERENCIA", (ctaRecPdv.IsINV_REFERENCIANull() ? null : ctaRecPdv.INV_REFERENCIA));
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pDT_VENCTO_ORIG", (ctaRecPdv.IsDT_VENCTO_ORIGNull() ? null : (DateTime?)ctaRecPdv.DT_VENCTO_ORIG));
            //                                                fbCommCtaRecSyncInsertServ.Parameters.Add("@pNSU_CARTAO", (ctaRecPdv.IsNSU_CARTAONull() ? null : ctaRecPdv.NSU_CARTAO));

            //                                                #endregion Prepara o comando da SP_TRI_CTAREC_SYNC_INSERT

            //                                                try
            //                                                {
            //                                                    // Executa a sproc
            //                                                    newIdCtarec = (int)fbCommCtaRecSyncInsertServ.ExecuteScalar();
            //                                                }
            //                                                catch (Exception ex)
            //                                                {
            //                                                    gravarMensagemErro("Erro ABSURDO ao gravar conta a receber. Eis os parâmetros da gravação: \npINV_REFERENCIA=" +
            //                                                        (ctaRecPdv.IsINV_REFERENCIANull() ? "null" : ctaRecPdv.INV_REFERENCIA.ToString()) + "\n" +
            //                                                        "pDOCUMENTO=" + ctaRecPdv.DOCUMENTO.ToString() + "\n" +
            //                                                        "pHISTORICO=" + (ctaRecPdv.IsHISTORICONull() ? "null" : ctaRecPdv.HISTORICO.ToString()) + "\n" +
            //                                                        "cupom.COO=" + cupom.COO.ToString() + "\n" +
            //                                                        "cupom.NUM_CAIXA=" + cupom.NUM_CAIXA.ToString() + "\n" +
            //                                                        "newIdCupom=" + newIdCupom.ToString() + "\n" + RetornarMensagemErro(ex, true));
            //                                                    throw ex;
            //                                                }
            //                                            }

            //                                            #endregion Grava conta a receber na retaguarda

            //                                            #region Gravar a referência entre cupom e conta a receber

            //                                            using (var fbCommCupomCtarecInsertServ = new FbCommand())
            //                                            {
            //                                                #region Prepara o comando

            //                                                fbCommCupomCtarecInsertServ.Connection = fbConnServ;
            //                                                fbCommCupomCtarecInsertServ.Transaction = fbTransactServ;
            //                                                //fbCommCupomCtarecInsertServ.Connection.ConnectionString = _strConnNetwork;

            //                                                fbCommCupomCtarecInsertServ.CommandText = "INSERT INTO TB_CUPOM_CTAREC (ID_CUPOM, ID_CTAREC) VALUES(@ID_CUPOM, @ID_CTAREC); ";
            //                                                fbCommCupomCtarecInsertServ.CommandType = CommandType.Text;

            //                                                fbCommCupomCtarecInsertServ.Parameters.Add("@ID_CUPOM", newIdCupom);
            //                                                fbCommCupomCtarecInsertServ.Parameters.Add("@ID_CTAREC", newIdCtarec);

            //                                                #endregion Prepara o comando

            //                                                // Executa a sproc
            //                                                fbCommCupomCtarecInsertServ.ExecuteNonQuery();
            //                                            }

            //                                            #endregion Gravar a referência entre cupom e conta a receber

            //                                            #region Consultar as movimentações diárias da conta a receber

            //                                            using (var taMovDiarioPdv = new TB_MOVDIARIOTableAdapter())
            //                                            {
            //                                                taMovDiarioPdv.Connection.ConnectionString = _strConnContingency;

            //                                                tblMovDiarioPdv.Clear();
            //                                                //audit("SINCCONTNETDB>> " + "taMovDiarioPdv.FillByIdCtarec(): " + taMovDiarioPdv.FillByIdCtarec(tblMovDiarioPdv, ctaRecPdv.ID_CTAREC).ToString());
            //                                                taMovDiarioPdv.FillByIdCtarec(tblMovDiarioPdv, ctaRecPdv.ID_CTAREC); // já usa sproc
            //                                            }

            //                                            #endregion Consultar as movimentações diárias da conta a receber

            //                                            #region Gravar movimentação diária referente à conta a receber

            //                                            if (tblMovDiarioPdv != null && tblMovDiarioPdv.Rows.Count > 0)
            //                                            {
            //                                                foreach (FDBDataSet.TB_MOVDIARIORow movdiarioPdv in tblMovDiarioPdv)
            //                                                {
            //                                                    int newIdMovto = 0;
            //                                                    //movdiarioPdv.SYNCED = 1;
            //                                                    // TB_MOVDIARIO
            //                                                    using (var fbCommMovDiarioMovtoSyncInsertServ = new FbCommand())
            //                                                    {
            //                                                        #region Prepara o comando da SP_TRI_MOVTO_SYNC_INSERT

            //                                                        fbCommMovDiarioMovtoSyncInsertServ.Connection = fbConnServ;
            //                                                        //fbCommMovDiarioMovtoSyncInsertServ.Connection.ConnectionString = _strConnNetwork;
            //                                                        fbCommMovDiarioMovtoSyncInsertServ.Transaction = fbTransactServ;

            //                                                        fbCommMovDiarioMovtoSyncInsertServ.CommandText = "SP_TRI_MOVTO_SYNC_INSERT";
            //                                                        fbCommMovDiarioMovtoSyncInsertServ.CommandType = CommandType.StoredProcedure;

            //                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pDT_MOVTO", (movdiarioPdv.IsDT_MOVTONull() ? null : (DateTime?)movdiarioPdv.DT_MOVTO));
            //                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pHR_MOVTO", (movdiarioPdv.IsHR_MOVTONull() ? null : (TimeSpan?)movdiarioPdv.HR_MOVTO));
            //                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pHISTORICO", (movdiarioPdv.IsHISTORICONull() ? null : movdiarioPdv.HISTORICO));
            //                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pTIP_MOVTO", (movdiarioPdv.IsTIP_MOVTONull() ? null : movdiarioPdv.TIP_MOVTO));
            //                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pVLR_MOVTO", (movdiarioPdv.IsVLR_MOVTONull() ? null : (decimal?)movdiarioPdv.VLR_MOVTO));
            //                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pID_CTAPLA", (movdiarioPdv.IsID_CTAPLANull() ? null : (short?)movdiarioPdv.ID_CTAPLA));
            //                                                        fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pSYNCED", 1);

            //                                                        #endregion Prepara o comando da SP_TRI_MOVTO_SYNC_INSERT

            //                                                        try
            //                                                        {
            //                                                            // Executa a sproc
            //                                                            newIdMovto = (int)fbCommMovDiarioMovtoSyncInsertServ.ExecuteScalar(); //TODO: esse trecho é problemático para a Estilo K, às vezes apresenta dead-lock
            //                                                        }
            //                                                        catch (Exception ex)
            //                                                        {
            //                                                            gravarMensagemErro("Erro ao sync movdiario (Serv): \npDT_MOVTO=" + (movdiarioPdv.IsDT_MOVTONull() ? "null" : movdiarioPdv.DT_MOVTO.ToString()) +
            //                                                                               "\npHR_MOVTO=" + (movdiarioPdv.IsHR_MOVTONull() ? "null" : movdiarioPdv.HR_MOVTO.ToString()) +
            //                                                                               "\npHISTORICO=" + (movdiarioPdv.IsHISTORICONull() ? "null" : movdiarioPdv.HISTORICO.ToString()) +
            //                                                                               "\npTIP_MOVTO=" + (movdiarioPdv.IsTIP_MOVTONull() ? "null" : movdiarioPdv.TIP_MOVTO.ToString()) +
            //                                                                               "\npVLR_MOVTO=" + (movdiarioPdv.IsVLR_MOVTONull() ? "null" : movdiarioPdv.VLR_MOVTO.ToString()) +
            //                                                                               "\npID_CTAPLA=" + (movdiarioPdv.IsID_CTAPLANull() ? "null" : movdiarioPdv.ID_CTAPLA.ToString()) +
            //                                                                                " \nMais infos: " + RetornarMensagemErro(ex, true));
            //                                                            throw ex;
            //                                                        }
            //                                                    }

            //                                                    #region Gravar a referência entre a conta a receber e a movimentação diária

            //                                                    using (var fbCommCtarecMovtoInsertServ = new FbCommand())
            //                                                    {
            //                                                        #region Prepara o comando

            //                                                        fbCommCtarecMovtoInsertServ.Connection = fbConnServ;
            //                                                        fbCommCtarecMovtoInsertServ.Transaction = fbTransactServ;
            //                                                        //fbCommCtarecMovtoInsertServ.Connection.ConnectionString = _strConnNetwork;

            //                                                        fbCommCtarecMovtoInsertServ.CommandText = "INSERT INTO TB_CTAREC_MOVTO (ID_MOVTO, ID_CTAREC) VALUES(@ID_MOVTO, @ID_CTAREC); ";
            //                                                        fbCommCtarecMovtoInsertServ.CommandType = CommandType.Text;

            //                                                        fbCommCtarecMovtoInsertServ.Parameters.Add("@ID_MOVTO", newIdMovto);
            //                                                        fbCommCtarecMovtoInsertServ.Parameters.Add("@ID_CTAREC", newIdCtarec);

            //                                                        #endregion Prepara o comando

            //                                                        // Executa a sproc
            //                                                        fbCommCtarecMovtoInsertServ.ExecuteNonQuery();
            //                                                    }

            //                                                    #endregion Gravar a referência entre a conta a receber e a movimentação diária

            //                                                    #region Indicar que o fechamento de caixa foi sincronizado

            //                                                    using (var taMovDiarioPdv = new TB_MOVDIARIOTableAdapter())
            //                                                    {
            //                                                        taMovDiarioPdv.Connection = fbConnPdv;
            //                                                        taMovDiarioPdv.Transaction = fbTransactPdv;
            //                                                        //taMovDiarioPdv.Connection.ConnectionString = _strConnContingency;

            //                                                        taMovDiarioPdv.SP_TRI_MOVTOSETSYNCED(movdiarioPdv.ID_MOVTO, 1);
            //                                                    }

            //                                                    #endregion Indicar que o fechamento de caixa foi sincronizado
            //                                                }
            //                                            }

            //                                            #endregion Gravar movimentação diária referente à conta a receber
            //                                        }
            //                                    }

            //                                    #endregion Verifica se o cupom é a prazo

            //                                    #endregion Vendas a prazo

            //                                    #region Verificar se o cupom foi cancelado: reativar o orçamento vinculado, se houver

            //                                    if (cupom.STATUS == "C" || cupom.IND_CANCEL == "S")
            //                                    {
            //                                        // TRI_PDV_ORCA_CUPOM_REL
            //                                        using (var taOrcaServ = new DataSets.FDBDataSetOrcamTableAdapters.TRI_PDV_ORCA_CUPOM_RELTableAdapter())
            //                                        {
            //                                            taOrcaServ.Connection = fbConnServ;
            //                                            taOrcaServ.Transaction = fbTransactServ;
            //                                            //taOrcaServ.Connection.ConnectionString = _strConnNetwork;

            //                                            audit("SINCCONTNETDB>> ", string.Format("(cupom cancelado antes de sync) taOrcaServ.SP_TRI_ORCA_REATIVAORCA({1}): {0}",
            //                                                                taOrcaServ.SP_TRI_ORCA_REATIVAORCA(cupom.ID_CUPOM).Safestring(),
            //                                                                cupom.ID_CUPOM.Safestring()));
            //                                        }
            //                                    }

            //                                    #endregion Verificar se o cupom foi cancelado: reativar o orçamento vinculado, se houver

            //                                    #region Indicar que o cupom foi synced

            //                                    using (var fbCommCupomSetSynced = new FbCommand())
            //                                    {
            //                                        #region Prepara o comando da SP_TRI_CUPOMSETSYNCED

            //                                        fbCommCupomSetSynced.Connection = fbConnPdv;
            //                                        fbCommCupomSetSynced.Transaction = fbTransactPdv;
            //                                        //fbCommCupomUnsyncedSetSynced.Connection.ConnectionString = _strConnContingency;

            //                                        fbCommCupomSetSynced.CommandText = "SP_TRI_CUPOMSETSYNCED";
            //                                        fbCommCupomSetSynced.CommandType = CommandType.StoredProcedure;

            //                                        fbCommCupomSetSynced.Parameters.Add("@pID_CUPOM", cupom.ID_CUPOM);
            //                                        fbCommCupomSetSynced.Parameters.Add("@pSYNCED", 1);

            //                                        #endregion Prepara o comando da SP_TRI_CUPOMSETSYNCED

            //                                        // Executa a sproc
            //                                        fbCommCupomSetSynced.ExecuteScalar();
            //                                    }

            //                                    #endregion Indicar que o cupom foi synced

            //                                    //fbConnPdv.Close();
            //                                    //fbConnServ.Close();

            //                                    // Finaliza a transação:
            //                                    //transactionScopeCupons.Complete();
            //                                    fbTransactServ.Commit();
            //                                    fbTransactPdv.Commit();
            //                                }
            //                                catch (TransactionAbortedException taEx)
            //                                {
            //                                    gravarMensagemErro("TransactionAbortedException: \n\n" + RetornarMensagemErro(taEx, true));
            //                                    fbTransactServ.Rollback();
            //                                    fbTransactPdv.Rollback();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    gravarMensagemErro("Erro durante a transação de cupons: \n\n" + RetornarMensagemErro(ex, true));
            //                                    fbTransactServ.Rollback();
            //                                    fbTransactPdv.Rollback();
            //                                }
            //                            }
            //                        }
            //                        #endregion Gravar o cupom na retaguarda (transação)
            //                    }

            //                    audit("SINCCONTNETDB>> ", string.Format("Lote {0} de cupons processado!", contLote.ToString()));

            //                    #endregion Sincroniza (manda para a retaguarda)

            //                    #region Prepara o próximo lote

            //                    // Limpa a tabela para pegar o próximo lote (é necessário limpar mesmo? o comando seguinte deveria sobrescrevê-lo):
            //                    tblCupomUnsynced.Clear();
            //                    audit("SINCCONTNETDB>> ", "taCupomUnsynced.Fill(): " + taCupomUnsynced.FillByCupomSync(tblCupomUnsynced, 0).ToString()); // já usa sproc

            //                    #endregion Prepara o próximo lote
            //                }

            //                #region NOPE - CLIPP RULES NO MORE
            //                //taEstProdutoPdv.SP_TRI_FIX_CLIPP_RULES();
            //                //taEstProdutoServ.SP_TRI_FIX_CLIPP_RULES();
            //                #endregion NOPE - CLIPP RULES NO MORE
            //            }
            //            #endregion Procedimento executado enquanto houver cupons para sincronizar
            //        }
            //        #region Manipular Exception
            //        catch (Exception ex)
            //        {
            //            gravarMensagemErro("Erro ao sincronizar: \n\n" + RetornarMensagemErro(ex, true));
            //            GravarErroSync("Erro ao sincronizar cupons", tblCtaRecPdv, ex);
            //            GravarErroSync("Erro ao sincronizar cupons", tblCupomFmapagtoPdv, ex);
            //            GravarErroSync("Erro ao sincronizar cupons", tblCupomItemPdv, ex);
            //            GravarErroSync("Erro ao sincronizar cupons", tblCupomUnsynced, ex);
            //            GravarErroSync("Erro ao sincronizar cupons", tblMovDiarioPdv, ex);
            //            throw ex;
            //        }
            //        #endregion Manipular Exception
            //        #region Limpeza da transação
            //        finally
            //        {
            //            #region Trata disposable objects

            //            #region(cupons, contas a receber e movimentação diária)

            //            if (taCupomFmaPagtoPdv != null) { taCupomFmaPagtoPdv.Dispose(); }
            //            //if (taCupomFmaPagtoServ != null) { taCupomFmaPagtoServ.Dispose(); }

            //            //if (taTrocaPdv != null) { taTrocaPdv.Dispose(); }
            //            //if (taTrocaServ != null) { taTrocaServ.Dispose(); }
            //            //if (tblTrocaPdv != null) { tblTrocaPdv.Dispose(); }

            //            //if (taCupomServ != null) { taCupomServ.Dispose(); }
            //            //if (taCtaRecServ != null) { taCtaRecServ.Dispose(); }
            //            //if (taCupomCtarecServ != null) { taCupomCtarecServ.Dispose(); }
            //            //if (taMovDiarioServ != null) { taMovDiarioServ.Dispose(); }
            //            //if (taCtarecMovtoServ != null) { taCtarecMovtoServ.Dispose(); }

            //            //if (taCtaRecPdv != null) { taCtaRecPdv.Dispose(); }
            //            //if (taMovDiarioPdv != null) { taMovDiarioPdv.Dispose(); }

            //            if (tblCupomFmapagtoPdv != null) { tblCupomFmapagtoPdv.Dispose(); }
            //            if (tblCtaRecPdv != null) { tblCtaRecPdv.Dispose(); }
            //            if (tblMovDiarioPdv != null) { tblMovDiarioPdv.Dispose(); }

            //            #endregion (cupons, contas a receber e movimentação diária)

            //            #region (item de cupom)

            //            if (tblCupomItemPdv != null) { tblCupomItemPdv.Dispose(); }
            //            if (taCupomItemPdv != null) { taCupomItemPdv.Dispose(); }
            //            //if (taCupomItemServ != null) { taCupomItemServ.Dispose(); }

            //            #endregion (item de cupom)

            //            #region (cupons unsynced, produtos)

            //            if (taCupomUnsynced != null) { taCupomUnsynced.Dispose(); }
            //            if (tblCupomUnsynced != null) { tblCupomUnsynced.Dispose(); }

            //            if (taEstProdutoServ != null) { taEstProdutoServ.Dispose(); }
            //            if (taEstProdutoPdv != null) { taEstProdutoPdv.Dispose(); }

            //            #endregion (cupons unsynced, produtos)

            //            #region (AmbiMAITRE)

            //            tblMaitPedidoPdv?.Dispose();
            //            tblMaitPedidoItemPdv?.Dispose();

            //            tblMaitEstCompProdPdv?.Dispose();
            //            tblMaitEstCompItemUsadoPdv?.Dispose();

            //            ////taMaitPedidoPdv?.Dispose();
            //            ////taMaitPedItemPdv?.Dispose();

            //            #endregion (AmbiMAITRE)

            //            #endregion Trata disposable objects
            //        }
            //        #endregion Limpeza da transação
            //    }

            //    #endregion Padrão, unsynced

            //    #region Cupons sincronizados e posteriormente cancelados
            //    {
            //        #region Cria os TableAdapters, DataTables e variáveis

            //        var taCupomSyncedCancelPdv = new TB_CUPOMTableAdapter();
            //        taCupomSyncedCancelPdv.Connection.ConnectionString = _strConnContingency;
            //        var tblCupomSyncedCancelPdv = new FDBDataSet.TB_CUPOMDataTable();

            //        var taEstProdutoServ = new TB_EST_PRODUTOTableAdapter();
            //        taEstProdutoServ.Connection.ConnectionString = _strConnNetwork;

            //        var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter();
            //        taEstProdutoPdv.Connection.ConnectionString = _strConnContingency;

            //        int intCountLoteCupomSyncedCancel = 0;

            //        #endregion Cria os TableAdapters, DataTables e variáveis

            //        try
            //        {
            //            // Busca todos os cupons que foram synced e posteriormente cancelados (TIP_QUERY = 1)
            //            // Lembrando que a sproc executada abaixo retorna até 200 registros por vez.
            //            audit("SINCCONTNETDB>> ", "taCupomSyncedCancelPdv.FillByCupomSync(): " + taCupomSyncedCancelPdv.FillByCupomSync(tblCupomSyncedCancelPdv, 1).ToString()); // já usa sproc

            //            while (tblCupomSyncedCancelPdv != null && tblCupomSyncedCancelPdv.Rows.Count > 0)
            //            {
            //                intCountLoteCupomSyncedCancel++;

            //                #region NOPE - Break Clipp rules agora é permanente
            //                //// Para repor quantidade em estoque sem dar problemas
            //                //taEstProdutoServ.SP_TRI_BREAK_CLIPP_RULES();
            //                //taEstProdutoPdv.SP_TRI_BREAK_CLIPP_RULES();
            //                #endregion NOPE - Break Clipp rules agora é permanente

            //                // Percorre por cada cupom cancelado:
            //                foreach (FDBDataSet.TB_CUPOMRow cupomCancelPdv in tblCupomSyncedCancelPdv)
            //                {
            //                    #region Validações

            //                    // Foi necessário adaptar o COO como o ID_CUPOM negativo para sistema legado
            //                    if (cupomCancelPdv.IsCOONull()) { cupomCancelPdv.COO = cupomCancelPdv.ID_CUPOM * -1; }
            //                    if (cupomCancelPdv.IsNUM_CAIXANull()) { cupomCancelPdv.NUM_CAIXA = 0; }

            //                    #endregion Validações

            //                    #region Iniciar o procedimento de cancelamento na retaguarda
            //                    // Será necessário apenas o COO e o NUM_CAIXA.
            //                    // Objetos de banco envolvidos:
            //                    // TB_CUPOM, 
            //                    // TB_CUPOM_ITEM, 
            //                    // TB_ESTOQUE, 
            //                    // TB_EST_PRODUTO, 
            //                    // TB_EST_IDENTIFICADOR,
            //                    // TB_CONTA_RECEBER, 
            //                    // TB_CUPOM_CTAREC, 
            //                    // TB_MOVDIARIO, 
            //                    // TB_CTAREC_MOVTO,
            //                    // TRI_ORCA_ORCAMENTOS,
            //                    // TRI_PDV_ORCA_CUPOM_REL
            //                    //using (var transactionScopeCupomResync = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(1, 0, 0, 0)))
            //                    using (var transactionScopeCupomResync = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
            //                    {
            //                        // Define a conexão com o banco do servidor:
            //                        using (var fbConnServ = new FbConnection(_strConnNetwork))
            //                        // Define a conexão com o banco do PDV:
            //                        using (var fbConnPdv = new FbConnection(_strConnContingency))
            //                        using (var tblCupomItemSyncedCancelPdv = new FDBDataSet.TB_CUPOM_ITEMDataTable())
            //                        using (var tblCtarecServ = new FDBDataSet.TB_CONTA_RECEBERDataTable())
            //                        using (var tblMovtoServ = new FDBDataSet.TB_MOVDIARIODataTable())
            //                        {
            //                            fbConnServ.Open();
            //                            fbConnPdv.Open();

            //                            // Verificar se o cupom tem conta a receber:
            //                            if (!cupomCancelPdv.IsQTD_CTARECNull() && cupomCancelPdv.QTD_CTAREC > 0)
            //                            {
            //                                #region Busca as contas a receber do cupom no serv
            //                                try
            //                                {
            //                                    using (var taCtarecServ = new TB_CONTA_RECEBERTableAdapter())
            //                                    {
            //                                        taCtarecServ.Connection = fbConnServ;
            //                                        //audit("SINCCONTNETDB>> " + "taCtarecServ.FillByCooNumcaixa(): " + taCtarecServ.FillByCooNumcaixa(tblCtarecServ, cupomCancelPdv.COO, cupomCancelPdv.NUM_CAIXA).ToString());
            //                                        taCtarecServ.FillByCooNumcaixa(tblCtarecServ, cupomCancelPdv.COO, cupomCancelPdv.NUM_CAIXA).ToString(); // já usa sproc
            //                                    }
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    gravarMensagemErro("Erro ao consultar contas a receber no servidor (" +
            //                                                       " / COO = " + cupomCancelPdv.COO + " / NUM_CAIXA = " + cupomCancelPdv.NUM_CAIXA + "): " + RetornarMensagemErro(ex, true));
            //                                    throw ex;
            //                                }
            //                                #endregion Busca as contas a receber do cupom no serv

            //                                #region Percorre por cada conta a receber no servidor
            //                                foreach (FDBDataSet.TB_CONTA_RECEBERRow ctarecServ in tblCtarecServ)
            //                                {
            //                                    #region Busca os movimentos diários da conta a receber
            //                                    using (var taMovtoServ = new TB_MOVDIARIOTableAdapter())
            //                                    {
            //                                        taMovtoServ.Connection = fbConnServ;
            //                                        tblMovtoServ.Clear();
            //                                        //audit("SINCCONTNETDB>> " + "taMovtoServ.FillByIdCtarec(): " + taMovtoServ.FillByIdCtarec(tblMovtoServ, ctarecServ.ID_CTAREC).ToString());
            //                                        taMovtoServ.FillByIdCtarec(tblMovtoServ, ctarecServ.ID_CTAREC); // já usa sproc
            //                                    }
            //                                    #endregion Busca os movimentos diários da conta a receber

            //                                    #region Percorre por cada movimentação diária do servidor
            //                                    foreach (FDBDataSet.TB_MOVDIARIORow movtoServ in tblMovtoServ)
            //                                    {
            //                                        #region Apagar TB_CTAREC_MOVTO
            //                                        //taCtarecMovtoServ.SP_TRI_CTARECMOVTO_SYNC_DEL(movtoServ.ID_MOVTO, ctarecServ.ID_CTAREC);
            //                                        using (var fbCommCtarecMovtoSyncDelServ = new FbCommand())
            //                                        {
            //                                            #region Prepara o comando da SP_TRI_CTARECMOVTO_SYNC_DEL

            //                                            fbCommCtarecMovtoSyncDelServ.Connection = fbConnServ;

            //                                            fbCommCtarecMovtoSyncDelServ.CommandText = "SP_TRI_CTARECMOVTO_SYNC_DEL";
            //                                            fbCommCtarecMovtoSyncDelServ.CommandType = CommandType.StoredProcedure;

            //                                            fbCommCtarecMovtoSyncDelServ.Parameters.Add("@pID_MOVTO", movtoServ.ID_MOVTO);
            //                                            fbCommCtarecMovtoSyncDelServ.Parameters.Add("@pID_CTAREC", ctarecServ.ID_CTAREC);

            //                                            #endregion Prepara o comando da SP_TRI_CTARECMOVTO_SYNC_DEL

            //                                            // Executa a sproc
            //                                            fbCommCtarecMovtoSyncDelServ.ExecuteScalar();
            //                                        }
            //                                        #endregion Apagar TB_CTAREC_MOVTO

            //                                        #region Apagar TB_MOVDIARIO
            //                                        //taMovtoServ.SP_TRI_MOVTO_SYNC_DEL(movtoServ.ID_MOVTO);
            //                                        using (var fbCommMovtoSyncDelServ = new FbCommand())
            //                                        {
            //                                            #region Prepara o comando da SP_TRI_MOVTO_SYNC_DEL

            //                                            fbCommMovtoSyncDelServ.Connection = fbConnServ;

            //                                            fbCommMovtoSyncDelServ.CommandText = "SP_TRI_MOVTO_SYNC_DEL";
            //                                            fbCommMovtoSyncDelServ.CommandType = CommandType.StoredProcedure;

            //                                            fbCommMovtoSyncDelServ.Parameters.Add("@pID_MOVTO", movtoServ.ID_MOVTO);

            //                                            #endregion Prepara o comando da SP_TRI_MOVTO_SYNC_DEL

            //                                            // Executa a sproc
            //                                            fbCommMovtoSyncDelServ.ExecuteScalar();
            //                                        }
            //                                        #endregion Apagar TB_MOVDIARIO
            //                                    }
            //                                    #endregion Percorre por cada movimentação diária do servidor

            //                                    #region Apagar o vínculo TB_CUPOM_CTAREC
            //                                    //taCupomCtarecServ.SP_TRI_CUPOMCTAREC_SYNC_DEL(cupomCancelPdv.COO, cupomCancelPdv.NUM_CAIXA, ctarecServ.ID_CTAREC);
            //                                    using (var fbCommCupomCtarecSyncDelServ = new FbCommand())
            //                                    {
            //                                        #region Prepara o comando da SP_TRI_CUPOMCTAREC_SYNC_DEL

            //                                        fbCommCupomCtarecSyncDelServ.Connection = fbConnServ;

            //                                        fbCommCupomCtarecSyncDelServ.CommandText = "SP_TRI_CUPOMCTAREC_SYNC_DEL";
            //                                        fbCommCupomCtarecSyncDelServ.CommandType = CommandType.StoredProcedure;

            //                                        fbCommCupomCtarecSyncDelServ.Parameters.Add("@pCOO", cupomCancelPdv.COO);
            //                                        fbCommCupomCtarecSyncDelServ.Parameters.Add("@pNUM_CAIXA", cupomCancelPdv.NUM_CAIXA);
            //                                        fbCommCupomCtarecSyncDelServ.Parameters.Add("@pID_CTAREC", ctarecServ.ID_CTAREC);

            //                                        #endregion Prepara o comando da SP_TRI_CUPOMCTAREC_SYNC_DEL

            //                                        // Executa a sproc
            //                                        try
            //                                        {
            //                                            fbCommCupomCtarecSyncDelServ.ExecuteScalar();
            //                                        }
            //                                        catch (Exception ex)
            //                                        {
            //                                            gravarMensagemErro(String.Format("{0} - \n COO: {1}, NUMCAIXA: {2}, ID_CTAREC {3}",
            //                                                                             RetornarMensagemErro(ex, true),
            //                                                                             cupomCancelPdv.COO,
            //                                                                             cupomCancelPdv.NUM_CAIXA,
            //                                                                             ctarecServ.ID_CTAREC));
            //                                            throw ex;
            //                                        }

            //                                    }
            //                                    #endregion Apagar o vínculo TB_CUPOM_CTAREC

            //                                    #region Apagar conta a receber (TB_CONTA_RECEBER)
            //                                    //{
            //                                    //    // Criar procedure para deletar?
            //                                    //}
            //                                    using (var fbCommCtarecSyncDelServ = new FbCommand())
            //                                    {
            //                                        #region Prepara o comando para deletar conta a receber

            //                                        fbCommCtarecSyncDelServ.Connection = fbConnServ;

            //                                        fbCommCtarecSyncDelServ.CommandText = "DELETE FROM TB_CONTA_RECEBER WHERE ID_CTAREC = @pID_CTAREC;";
            //                                        fbCommCtarecSyncDelServ.CommandType = CommandType.Text;

            //                                        fbCommCtarecSyncDelServ.Parameters.Add("@pID_CTAREC", ctarecServ.ID_CTAREC);

            //                                        #endregion Prepara o comando para deletar conta a receber

            //                                        try
            //                                        {
            //                                            // Executa a sproc
            //                                            fbCommCtarecSyncDelServ.ExecuteScalar();
            //                                        }
            //                                        catch (Exception ex)
            //                                        {
            //                                            gravarMensagemErro($"Erro ao cancelar (excluir) conta a receber no servidor (ID_CTAREC { ctarecServ.ID_CTAREC }).");
            //                                            throw ex;
            //                                        }
            //                                    }
            //                                    #endregion Apagar conta a receber (TB_CONTA_RECEBER)
            //                                }
            //                                #endregion Percorre por cada conta a receber no servidor
            //                            }

            //                            #region Atualizar no servidor a quantidade em estoque

            //                            using (var taCupomItemSyncedCancelPdv = new TB_CUPOM_ITEMTableAdapter())
            //                            {
            //                                taCupomItemSyncedCancelPdv.Connection = fbConnPdv;
            //                                tblCupomItemSyncedCancelPdv.Clear();
            //                                taCupomItemSyncedCancelPdv.FillByIdCupom(tblCupomItemSyncedCancelPdv, cupomCancelPdv.ID_CUPOM); // já usa sproc
            //                            }

            //                            // Percorrer por cada item de cupom para repor as quantidades em estoque:
            //                            foreach (FDBDataSet.TB_CUPOM_ITEMRow cupomItemSyncedCancel in tblCupomItemSyncedCancelPdv)
            //                            {
            //                                #region Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)

            //                                #region Atualizar no servidor a quantidade em estoque

            //                                using (var fbCommEstProdutoQtdServ = new FbCommand())
            //                                {
            //                                    #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

            //                                    fbCommEstProdutoQtdServ.Connection = fbConnServ;

            //                                    fbCommEstProdutoQtdServ.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
            //                                    fbCommEstProdutoQtdServ.CommandType = CommandType.StoredProcedure;

            //                                    fbCommEstProdutoQtdServ.Parameters.Add("@pQTD_ITEM", cupomItemSyncedCancel.QTD_ITEM * -1);
            //                                    fbCommEstProdutoQtdServ.Parameters.Add("@pID_IDENTIF", cupomItemSyncedCancel.ID_IDENTIF);
            //                                    fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPPRO", cupomItemSyncedCancel.IsID_COMPPRONull() ? 0 : cupomItemSyncedCancel.ID_COMPPRO); // AmbiMAITRE
            //                                    fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPOSICAO", cupomItemSyncedCancel.IsID_COMPOSICAONull() ? 0 : cupomItemSyncedCancel.ID_COMPOSICAO);

            //                                    #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

            //                                    // Executa a sproc
            //                                    fbCommEstProdutoQtdServ.ExecuteScalar();
            //                                }

            //                                #endregion Atualizar no servidor a quantidade em estoque

            //                                #region Atualizar no PDV a quantidade em estoque

            //                                // Já que todo o cadastro de produtos foi copiado do Serv pro PDV na etapa anterior, 
            //                                // as quantidades em estoque devem ser redefinidas
            //                                using (var fbCommEstProdutoQtdPdv = new FbCommand())
            //                                {
            //                                    #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

            //                                    fbCommEstProdutoQtdPdv.Connection = fbConnPdv;

            //                                    fbCommEstProdutoQtdPdv.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
            //                                    fbCommEstProdutoQtdPdv.CommandType = CommandType.StoredProcedure;

            //                                    fbCommEstProdutoQtdPdv.Parameters.Add("@pQTD_ITEM", cupomItemSyncedCancel.QTD_ITEM * -1);
            //                                    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_IDENTIF", cupomItemSyncedCancel.ID_IDENTIF);
            //                                    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPPRO", cupomItemSyncedCancel.IsID_COMPPRONull() ? 0 : cupomItemSyncedCancel.ID_COMPPRO); // AmbiMAITRE
            //                                    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPOSICAO", cupomItemSyncedCancel.IsID_COMPOSICAONull() ? 0 : cupomItemSyncedCancel.ID_COMPOSICAO);

            //                                    #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

            //                                    // Executa a sproc
            //                                    fbCommEstProdutoQtdPdv.ExecuteScalar();
            //                                }

            //                                #endregion Atualizar no PDV a quantidade em estoque

            //                                #endregion Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)
            //                            }

            //                            #endregion Atualizar no servidor a quantidade em estoque

            //                            #region Indicar que o cupom foi synced depois de cancelado (Serv)

            //                            using (var fbCommCupomUpdtByCooNumcaixaServ = new FbCommand())
            //                            {
            //                                #region Prepara o comando da SP_TRI_CUPOM_UPDT_BYCOONUMCAIX

            //                                fbCommCupomUpdtByCooNumcaixaServ.Connection = fbConnServ;

            //                                fbCommCupomUpdtByCooNumcaixaServ.CommandText = "SP_TRI_CUPOM_UPDT_BYCOONUMCAIX";
            //                                fbCommCupomUpdtByCooNumcaixaServ.CommandType = CommandType.StoredProcedure;

            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pCOO", cupomCancelPdv.COO);
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pCCF", (cupomCancelPdv.IsCCFNull() ? null : (int?)cupomCancelPdv.CCF));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pDT_CUPOM", cupomCancelPdv.DT_CUPOM);
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pHR_CUPOM", cupomCancelPdv.HR_CUPOM);
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pNUM_CAIXA", cupomCancelPdv.NUM_CAIXA);
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pID_CLIENTE", (cupomCancelPdv.IsID_CLIENTENull() ? null : (int?)cupomCancelPdv.ID_CLIENTE));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pID_VENDEDOR", (cupomCancelPdv.IsID_VENDEDORNull() ? null : (short?)cupomCancelPdv.ID_VENDEDOR));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pSTATUS", (cupomCancelPdv.IsSTATUSNull() ? null : cupomCancelPdv.STATUS));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pID_PARCELA", (short?)cupomCancelPdv.ID_PARCELA);
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pIND_CANCEL", (cupomCancelPdv.IsIND_CANCELNull() ? null : cupomCancelPdv.IND_CANCEL));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pID_IFS", (short?)cupomCancelPdv.ID_IFS);
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pID_NATOPE", (int?)cupomCancelPdv.ID_NATOPE);
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pVLR_TROCO", (cupomCancelPdv.IsVLR_TROCONull() ? null : (decimal?)cupomCancelPdv.VLR_TROCO));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pVLR_TOTAL", (cupomCancelPdv.IsVLR_TOTALNull() ? null : (decimal?)cupomCancelPdv.VLR_TOTAL));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pVLR_DESC", (cupomCancelPdv.IsVLR_DESCNull() ? null : (decimal?)cupomCancelPdv.VLR_DESC));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pTIP_DESC", (cupomCancelPdv.IsTIP_DESCNull() ? null : cupomCancelPdv.TIP_DESC));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pVLR_ACRES", (cupomCancelPdv.IsVLR_ACRESNull() ? null : (decimal?)cupomCancelPdv.VLR_ACRES));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pGNF", (cupomCancelPdv.IsGNFNull() ? null : (int?)cupomCancelPdv.GNF));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pCHAVE", (cupomCancelPdv.IsCHAVENull() ? null : cupomCancelPdv.CHAVE));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pTOTAL_TRIBUTOS_IBPT", (cupomCancelPdv.IsTOTAL_TRIBUTOS_IBPTNull() ? null : (decimal?)cupomCancelPdv.TOTAL_TRIBUTOS_IBPT));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pTOTAL_TRIB_FED", (cupomCancelPdv.IsTOTAL_TRIB_FEDNull() ? null : (decimal?)cupomCancelPdv.TOTAL_TRIB_FED));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pTOTAL_TRIB_EST", (cupomCancelPdv.IsTOTAL_TRIB_ESTNull() ? null : (decimal?)cupomCancelPdv.TOTAL_TRIB_EST));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pTOTAL_TRIB_MUN", (cupomCancelPdv.IsTOTAL_TRIB_MUNNull() ? null : (decimal?)cupomCancelPdv.TOTAL_TRIB_MUN));
            //                                fbCommCupomUpdtByCooNumcaixaServ.Parameters.Add("@pSYNCED", 2);

            //                                #endregion Prepara o comando da SP_TRI_CUPOM_UPDT_BYCOONUMCAIX

            //                                // Executa a sproc
            //                                fbCommCupomUpdtByCooNumcaixaServ.ExecuteScalar();
            //                            }

            //                            #endregion Indicar que o cupom foi synced depois de cancelado (Serv)

            //                            #region Indicar que os itens do cupom foram cancelados (Serv)

            //                            //ATENCAO: A operação do PDV já faz essa indicação localmente.

            //                            using (var fbCommCupomItemSetCancelByCooNumcaixaServ = new FbCommand())
            //                            {
            //                                #region Prepara o comando da SP_TRI_CUPOM_ITEM_SET_CANCEL

            //                                fbCommCupomItemSetCancelByCooNumcaixaServ.Connection = fbConnServ;

            //                                fbCommCupomItemSetCancelByCooNumcaixaServ.CommandText = "SP_TRI_CUPOM_ITEM_SET_CANCEL";
            //                                fbCommCupomItemSetCancelByCooNumcaixaServ.CommandType = CommandType.StoredProcedure;

            //                                fbCommCupomItemSetCancelByCooNumcaixaServ.Parameters.Add("@pCOO", cupomCancelPdv.COO);
            //                                fbCommCupomItemSetCancelByCooNumcaixaServ.Parameters.Add("@pNUM_CAIXA", cupomCancelPdv.NUM_CAIXA);

            //                                #endregion Prepara o comando da SP_TRI_CUPOM_ITEM_SET_CANCEL

            //                                // Executa a sproc
            //                                fbCommCupomItemSetCancelByCooNumcaixaServ.ExecuteScalar();
            //                            }

            //                            #endregion Indicar que os itens do cupom foram cancelados (Serv)

            //                            #region Indicar que o cupom foi synced depois de cancelado (PDV)

            //                            //// Define a conexão com o banco do servidor:
            //                            //using (var fbConnPdv = new FbConnection(_strConnContingency))
            //                            //{
            //                            //    fbConnPdv.Open();

            //                            using (var fbCommCupomUnsyncedSetSynced = new FbCommand())
            //                            {
            //                                #region Prepara o comando da SP_TRI_CUPOMSETSYNCED

            //                                fbCommCupomUnsyncedSetSynced.Connection = fbConnPdv;

            //                                fbCommCupomUnsyncedSetSynced.CommandText = "SP_TRI_CUPOMSETSYNCED";
            //                                fbCommCupomUnsyncedSetSynced.CommandType = CommandType.StoredProcedure;

            //                                fbCommCupomUnsyncedSetSynced.Parameters.Add("@pID_CUPOM", cupomCancelPdv.ID_CUPOM);
            //                                fbCommCupomUnsyncedSetSynced.Parameters.Add("@pSYNCED", 2);

            //                                #endregion Prepara o comando da SP_TRI_CUPOMSETSYNCED

            //                                // Executa a sproc
            //                                fbCommCupomUnsyncedSetSynced.ExecuteScalar();
            //                            }
            //                            //}
            //                            #endregion Indicar que o cupom foi synced depois de cancelado (PDV)

            //                            #region Desfazer vínculo de cupom com orçamento e setar status do orçamento para "SALVO"

            //                            #region Verificar se o cupom foi cancelado: reativar o orçamento vinculado, se houver

            //                            using (var taOrcaServ = new DataSets.FDBDataSetOrcamTableAdapters.TRI_PDV_ORCA_CUPOM_RELTableAdapter())
            //                            {
            //                                taOrcaServ.Connection = fbConnServ;

            //                                audit("SINCCONTNETDB>> ", string.Format("(cupom cancelado depois de synced) taOrcaServ.SP_TRI_ORCA_REATIVAORCA({1}): {0}",
            //                                                    taOrcaServ.SP_TRI_ORCA_REATIVAORCA(cupomCancelPdv.ID_CUPOM).Safestring(),
            //                                                    cupomCancelPdv.ID_CUPOM.Safestring()));
            //                            }

            //                            #endregion Verificar se o cupom foi cancelado: reativar o orçamento vinculado, se houver

            //                            #endregion Desfazer vínculo de cupom com orçamento e setar status do orçamento para "SALVO"

            //                            // Teste de transação:
            //                            //int minibomba = 0;
            //                            //decimal bomba = 100 / minibomba;

            //                            //fbConnServ.Close();
            //                            //fbConnPdv.Close();
            //                        }
            //                        // Finaliza a transação:
            //                        transactionScopeCupomResync.Complete();
            //                    }
            //                    #endregion Iniciar o procedimento de cancelamento na retaguarda
            //                }

            //                audit("SINCCONTNETDB>> ", string.Format("Lote {0} de cupons sincronizados e cancelados processado!", intCountLoteCupomSyncedCancel.ToString()));

            //                // Busca todos os cupons que foram synced e posteriormente cancelados (TIP_QUERY = 1)
            //                // Lembrando que a sproc executada abaixo retorna até 200 registros por vez (lote).
            //                tblCupomSyncedCancelPdv.Clear();
            //                audit("SINCCONTNETDB>> ", "taCupomSyncedCancelPdv.FillByCupomSync(): " + taCupomSyncedCancelPdv.FillByCupomSync(tblCupomSyncedCancelPdv, 1).ToString()); // já usa sproc

            //                #region NOPE - Não haverá fix Clipp rules
            //                //taEstProdutoServ.SP_TRI_FIX_CLIPP_RULES();
            //                //taEstProdutoPdv.SP_TRI_FIX_CLIPP_RULES();
            //                #endregion NOPE - Não haverá fix Clipp rules
            //            }
            //        }
            //        #region Manipular Exception
            //        catch (Exception ex)
            //        {
            //            gravarMensagemErro("Erro ao sincronizar (synced e cancelado depois): \n" + RetornarMensagemErro(ex, true));
            //            GravarErroSync("Erro ao sincronizar", tblCupomSyncedCancelPdv, ex);
            //            throw ex;
            //        }
            //        #endregion Manipular Exception
            //        #region Limpeza dos objetos Disposable
            //        finally
            //        {
            //            if (taCupomSyncedCancelPdv != null) { taCupomSyncedCancelPdv.Dispose(); }
            //            if (tblCupomSyncedCancelPdv != null) { tblCupomSyncedCancelPdv.Dispose(); }
            //            if (taEstProdutoServ != null) { taEstProdutoServ.Dispose(); }
            //            if (taEstProdutoPdv != null) { taEstProdutoPdv.Dispose(); }
            //        }
            //        #endregion Limpeza dos objetos Disposable
            //    }
            //    #endregion Cupons sincronizados e posteriormente cancelados
            //}

            #endregion Cupons (ECF)

            Sync_Cupons_NFVVENDA(tipoSync);
            #region Cupons (NFVenda)
            /*
            if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
            {
                #region Padrão, unsynced

                /// SP_TRI_CUPOM_GETALL_UNSYNCED
                {
                    #region Cria objetos da transação

                    var taNfvendaUnsynced = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter();
                    taNfvendaUnsynced.Connection.ConnectionString = _strConnContingency;

                    var taSatPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_SATTableAdapter();
                    taSatPdv.Connection.ConnectionString = _strConnContingency;

                    var taSatCancPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_SAT_CANCTableAdapter();
                    taSatCancPdv.Connection.ConnectionString = _strConnContingency;

                    var taNfvendaFmaPagtoNfcePdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDA_FMAPAGTO_NFCETableAdapter();
                    taNfvendaFmaPagtoNfcePdv.Connection.ConnectionString = _strConnContingency;

                    var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter();
                    taEstProdutoPdv.Connection.ConnectionString = _strConnContingency;

                    var taEstProdutoServ = new TB_EST_PRODUTOTableAdapter();
                    taEstProdutoServ.Connection.ConnectionString = _strConnNetwork;

                    var taNfvItemPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEMTableAdapter();
                    taNfvItemPdv.Connection.ConnectionString = _strConnContingency;

                    var tblNfvendaUnsynced = new FDBDataSetVenda.TB_NFVENDADataTable();

                    var tblNfvendaFmapagtoNfcePdv = new FDBDataSetVenda.TB_NFVENDA_FMAPAGTO_NFCEDataTable();

                    var tblSatPdv = new FDBDataSetVenda.TB_SATDataTable();

                    var tblSatCancPdv = new FDBDataSetVenda.TB_SAT_CANCDataTable();

                    var tblCtaRecPdv = new FDBDataSet.TB_CONTA_RECEBERDataTable();

                    var tblMovDiarioPdv = new FDBDataSet.TB_MOVDIARIODataTable();

                    var tblNfvItemPdv = new FDBDataSetVenda.TB_NFV_ITEMDataTable();

                    var tblNfvItemCofinsPdv = new FDBDataSetVenda.TB_NFV_ITEM_COFINSDataTable();

                    var tblNfvItemPisPdv = new FDBDataSetVenda.TB_NFV_ITEM_PISDataTable();

                    var tblNfvItemIcmsPdv = new FDBDataSetVenda.TB_NFV_ITEM_ICMSDataTable();

                    var tblNfvItemStPdv = new FDBDataSetVenda.TB_NFV_ITEM_STDataTable();

                    #endregion Cria objetos da transação

                    List<AuxNfvFmaPgtoCtaRec> lstAuxNfvFmaPgtoCtaRec = new List<AuxNfvFmaPgtoCtaRec>();

                    try
                    {
                        #region Prepara o lote inicial para sincronização

                        // Busca todos os cupons que foram finalizados mas não sincronizados (TIP_QUERY = 0):
                        taNfvendaUnsynced.FillByNfvendaSync(tblNfvendaUnsynced, 0); // já usa sproc
                        // Até o momento (23/02/2018), a quantidade de registros por lote 
                        // fica definido na própria consulta de cupons (SP_TRI_CUPOM_GETALL_UNSYNCED).
                        // O ideal seria que isso fosse parametrizado.

                        // Indica quantos lotes de cupons foram processados:
                        int contLote = 0;

                        #endregion Prepara o lote inicial para sincronização

                        #region Procedimento executado enquanto houver cupons para sincronizar

                        if (tblNfvendaUnsynced != null && tblNfvendaUnsynced.Rows.Count > 0)
                        {
                            #region NOPE - CLIPP RULES NO MORE
                            //taEstProdutoPdv.SP_TRI_BREAK_CLIPP_RULES();
                            //taEstProdutoServ.SP_TRI_BREAK_CLIPP_RULES();
                            #endregion NOPE - CLIPP RULES NO MORE

                            //TODO: ver uma saída pro loop infinito caso estourar exceção no sync
                            // Detalhe: a 2ª condição poderia estourar uma exception se a 1ª fosse verdadeira e o operador 
                            // fosse OR (||). Mas com o operador AND (&&), a 2ª condição nem é verificada se a 1ª for verdadeira.
                            while (!(tblNfvendaUnsynced is null) && tblNfvendaUnsynced.Rows.Count > 0)
                            {
                                contLote++;

                                #region Sincroniza (manda para a retaguarda)

                                // Percorre pelos cupons do banco local
                                foreach (FDBDataSetVenda.TB_NFVENDARow nfvenda in tblNfvendaUnsynced.Rows)
                                {
                                    int newIdNfvenda = 0;

                                    #region Gravar a nfvenda na retaguarda (transação)

                                    #region Validações

                                    //// Foi necessário adaptar o COO como o ID_CUPOM negativo para sistema legado
                                    //// Será que tem um equivalente para a NFVENDA?
                                    //if (cupom.IsCOONull()) { cupom.COO = cupom.ID_CUPOM * -1; }
                                    //if (cupom.IsNUM_CAIXANull()) { cupom.NUM_CAIXA = 0; }

                                    #endregion Validações


                                    //TransactionOptions to = new TransactionOptions();
                                    //to.IsolationLevel = System.Transactions.IsolationLevel.Serializable;

                                    // Inicia a transação:
                                    //using (var transactionScopeCupons = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(1, 0, 0, 0)))
                                    // Define a conexão com o banco do servidor:
                                    // Define a conexão com o banco do PDV:
                                    using (var fbConnServ = new FbConnection(_strConnNetwork))
                                    using (var fbConnPdv = new FbConnection(_strConnContingency))
                                    //using (var transactionScopeCupons = new TransactionScope(TransactionScopeOption.Required,
                                    //                                                         new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }
                                    //                                                         )) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                                    //using (var transactionScopeCupons = new TransactionScope())
                                    {
                                        // A função BeginTransaction() precisa de uma connection aberta... ¬¬
                                        fbConnServ.Open();
                                        fbConnPdv.Open();

                                        using (var fbTransactServ = fbConnServ.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Wait, WaitTimeout = new TimeSpan(0, 0, _SyncTimeout) }))
                                        using (var fbTransactPdv = fbConnPdv.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Wait, WaitTimeout = new TimeSpan(0, 0, _SyncTimeout) }))
                                        {
                                            try
                                            {
                                                //int newIdNfvenda = 0;
                                                //int newIdMaitPedidoServ = 0;
                                                lstAuxNfvFmaPgtoCtaRec = null;
                                                lstAuxNfvFmaPgtoCtaRec = new List<AuxNfvFmaPgtoCtaRec>();

                                                #region Gravar a nfvenda no servidor (capa)

                                                using (var fbCommNfvendaSyncInsertServ = new FbCommand())
                                                {
                                                    #region Prepara o comando da SP_TRI_NFVENDA_SYNC_INSERT

                                                    fbCommNfvendaSyncInsertServ.Connection = fbConnServ;
                                                    //fbCommCupomSyncInsertServ.Connection.ConnectionString = _strConnNetwork;

                                                    fbCommNfvendaSyncInsertServ.CommandText = "SP_TRI_NFVENDA_SYNC_INSERT";
                                                    fbCommNfvendaSyncInsertServ.CommandType = CommandType.StoredProcedure;
                                                    fbCommNfvendaSyncInsertServ.Transaction = fbTransactServ;

                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_NATOPE", nfvenda.ID_NATOPE);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_VENDEDOR", nfvenda.IsID_VENDEDORNull() ? null : (short?)nfvenda.ID_VENDEDOR);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_CLIENTE", nfvenda.ID_CLIENTE);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pNF_NUMERO", nfvenda.NF_NUMERO);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pNF_SERIE", nfvenda.NF_SERIE);

                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pNF_MODELO", nfvenda.NF_MODELO);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pDT_EMISSAO", nfvenda.DT_EMISSAO);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pDT_SAIDA", nfvenda.IsDT_SAIDANull() ? null : (DateTime?)nfvenda.DT_SAIDA);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pHR_SAIDA", nfvenda.IsHR_SAIDANull() ? null : (TimeSpan?)nfvenda.HR_SAIDA);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pESPECIE", nfvenda.IsESPECIENull() ? null : nfvenda.ESPECIE);

                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pTIPO_FRETE", nfvenda.TIPO_FRETE);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pPES_LIQUID", nfvenda.IsPES_LIQUIDNull() ? null : (decimal?)nfvenda.PES_LIQUID);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pPES_BRUTO", nfvenda.IsPES_BRUTONull() ? null : (decimal?)nfvenda.PES_BRUTO);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pSTATUS", nfvenda.STATUS);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pENT_SAI", nfvenda.ENT_SAI);

                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_FMAPGTO", nfvenda.ID_FMAPGTO);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pID_PARCELA", nfvenda.ID_PARCELA);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pMARCA", nfvenda.IsMARCANull() ? null : nfvenda.MARCA);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pQTD_VOLUM", nfvenda.IsQTD_VOLUMNull() ? null : (decimal?)nfvenda.QTD_VOLUM);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pNUM_VOLUM", nfvenda.IsNUM_VOLUMNull() ? null : nfvenda.NUM_VOLUM);

                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pPROD_REV", nfvenda.IsPROD_REVNull() ? null : nfvenda.PROD_REV);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pSOMA_FRETE", nfvenda.IsSOMA_FRETENull() ? null : nfvenda.SOMA_FRETE);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pVLR_TROCO", nfvenda.IsVLR_TROCONull() ? null : (decimal?)nfvenda.VLR_TROCO);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pIND_PRES", nfvenda.IsIND_PRESNull() ? null : nfvenda.IND_PRES);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pIND_IE_DEST", nfvenda.IsIND_IE_DESTNull() ? null : nfvenda.IND_IE_DEST);

                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pDESCONTO_CONDICIONAL", nfvenda.DESCONTO_CONDICIONAL);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pINF_COMP_FIXA", nfvenda.IsINF_COMP_FIXANull() ? null : nfvenda.INF_COMP_FIXA);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pINF_COMP_EDIT", nfvenda.IsINF_COMP_EDITNull() ? null : nfvenda.INF_COMP_EDIT);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pENDERECO_ENTREGA", nfvenda.ENDERECO_ENTREGA);
                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pENVIO_API", nfvenda.IsENVIO_APINull() ? null : (DateTime?)nfvenda.ENVIO_API);

                                                    fbCommNfvendaSyncInsertServ.Parameters.Add("@pSYNCED", 1);

                                                    #endregion Prepara o comando da SP_TRI_NFVENDA_SYNC_INSERT

                                                    // Executa a sproc
                                                    newIdNfvenda = (int)fbCommNfvendaSyncInsertServ.ExecuteScalar();
                                                }

                                                #endregion Gravar a nfvenda no servidor (capa)

                                                #region Gravar TB_SAT, se houver

                                                tblSatPdv.Clear();
                                                //NOME_DA_PROCEDURE_AQUI
                                                taSatPdv.FillByIdNfvenda(tblSatPdv, nfvenda.ID_NFVENDA);

                                                foreach (FDBDataSetVenda.TB_SATRow satPdv in tblSatPdv)
                                                {
                                                    int newIdRegistro = 0;

                                                    using (var fbCommSatSyncInsert = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_SAT_SYNC_INSERT

                                                        fbCommSatSyncInsert.Connection = fbConnServ;
                                                        //fbCommCupomFmapagtoSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                        fbCommSatSyncInsert.Transaction = fbTransactServ;

                                                        fbCommSatSyncInsert.CommandText = "SP_TRI_SAT_SYNC_INSERT";
                                                        fbCommSatSyncInsert.CommandType = CommandType.StoredProcedure;

                                                        fbCommSatSyncInsert.Parameters.Add("@pID_NFVENDA", newIdNfvenda);
                                                        fbCommSatSyncInsert.Parameters.Add("@pCHAVE", satPdv.IsCHAVENull() ? null : satPdv.CHAVE);
                                                        fbCommSatSyncInsert.Parameters.Add("@pDT_EMISSAO", satPdv.IsDT_EMISSAONull() ? null : (DateTime?)satPdv.DT_EMISSAO);
                                                        fbCommSatSyncInsert.Parameters.Add("@pHR_EMISSAO", satPdv.IsHR_EMISSAONull() ? null : (TimeSpan?)satPdv.HR_EMISSAO);

                                                        fbCommSatSyncInsert.Parameters.Add("@pSTATUS", satPdv.IsSTATUSNull() ? null : satPdv.STATUS);
                                                        fbCommSatSyncInsert.Parameters.Add("@pSTATUS_DES", satPdv.IsSTATUS_DESNull() ? null : satPdv.STATUS_DES);
                                                        fbCommSatSyncInsert.Parameters.Add("@pNUMERO_CFE", satPdv.IsNUMERO_CFENull() ? null : (int?)satPdv.NUMERO_CFE);
                                                        fbCommSatSyncInsert.Parameters.Add("@pNUM_SERIE_SAT", satPdv.IsNUM_SERIE_SATNull() ? null : satPdv.NUM_SERIE_SAT);

                                                        #endregion Prepara o comando da SP_TRI_SAT_SYNC_INSERT

                                                        // Executa a sproc
                                                        newIdRegistro = (int)fbCommSatSyncInsert.ExecuteScalar();
                                                    }

                                                    #region Gravar TB_SAT_CANC, se houver

                                                    tblSatCancPdv.Clear();
                                                    //NOME_DA_PROCEDURE_AQUI
                                                    taSatCancPdv.FillByIdRegistro(tblSatCancPdv, satPdv.ID_REGISTRO);

                                                    foreach (FDBDataSetVenda.TB_SAT_CANCRow satCancPdv in tblSatCancPdv)
                                                    {
                                                        //int newIdCancela = 0;

                                                        using (var fbCommSatCancSyncInsert = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_SAT_CANC_SYNC_INSERT

                                                            fbCommSatCancSyncInsert.Connection = fbConnServ;
                                                            //fbCommCupomFmapagtoSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommSatCancSyncInsert.Transaction = fbTransactServ;

                                                            fbCommSatCancSyncInsert.CommandText = "SP_TRI_SAT_CANC_SYNC_INSERT";
                                                            fbCommSatCancSyncInsert.CommandType = CommandType.StoredProcedure;

                                                            fbCommSatCancSyncInsert.Parameters.Add("@pID_REGISTRO", newIdRegistro);
                                                            fbCommSatCancSyncInsert.Parameters.Add("@pDT_EMISSAO", satCancPdv.IsDT_EMISSAONull() ? null : (DateTime?)satCancPdv.DT_EMISSAO);
                                                            fbCommSatCancSyncInsert.Parameters.Add("@pHR_EMISSAO", satCancPdv.IsHR_EMISSAONull() ? null : (TimeSpan?)satCancPdv.HR_EMISSAO);
                                                            fbCommSatCancSyncInsert.Parameters.Add("@pNUMERO_CFE", satCancPdv.IsNUMERO_CFENull() ? null : (int?)satCancPdv.NUMERO_CFE);
                                                            fbCommSatCancSyncInsert.Parameters.Add("@pCHAVE", satCancPdv.IsCHAVENull() ? null : satCancPdv.CHAVE);
                                                            fbCommSatCancSyncInsert.Parameters.Add("@pNUM_SERIE_SAT", satCancPdv.IsNUM_SERIE_SATNull() ? null : satCancPdv.NUM_SERIE_SAT);
                                                            fbCommSatCancSyncInsert.Parameters.Add("@pENVIO_API", satCancPdv.IsENVIO_APINull() ? null : (DateTime?)satCancPdv.ENVIO_API);

                                                            #endregion Prepara o comando da SP_TRI_SAT_CANC_SYNC_INSERT

                                                            // Executa a sproc
                                                            //newIdCancela = (int)
                                                            fbCommSatCancSyncInsert.ExecuteScalar();
                                                        }

                                                        #region Gravar TB_SAT_CANC, se houver

                                                        //TODO: gravar TB_SAT_CANC também

                                                        #endregion Gravar TB_SAT_CANC, se houver
                                                    }

                                                    #endregion Gravar TB_SAT_CANC, se houver
                                                }

                                                #endregion Gravar TB_SAT, se houver

                                                #region Buscar as formas de pagamento da nfvenda no PDV

                                                tblNfvendaFmapagtoNfcePdv.Clear();
                                                //TB_NFVENDA_FMAPAGTO_NFCE();
                                                taNfvendaFmaPagtoNfcePdv.FillByIdNfvenda(tblNfvendaFmapagtoNfcePdv, nfvenda.ID_NFVENDA);

                                                #endregion Buscar as formas de pagamento da nfvenda no PDV

                                                #region Gravar as formas de pagamento da nfvenda na retaguarda

                                                foreach (FDBDataSetVenda.TB_NFVENDA_FMAPAGTO_NFCERow nfvendaFmapagtoNfcePdv in tblNfvendaFmapagtoNfcePdv)
                                                {
                                                    int newIdnumpag = 0;

                                                    using (var fbCommNfvendaFmapagtoNfceSyncInsert = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_NFV_FMAPAGT_SYNC_INSERT

                                                        fbCommNfvendaFmapagtoNfceSyncInsert.Connection = fbConnServ;
                                                        //fbCommCupomFmapagtoSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                        fbCommNfvendaFmapagtoNfceSyncInsert.Transaction = fbTransactServ;

                                                        fbCommNfvendaFmapagtoNfceSyncInsert.CommandText = "SP_TRI_NFV_FMAPAGT_SYNC_INSERT";
                                                        fbCommNfvendaFmapagtoNfceSyncInsert.CommandType = CommandType.StoredProcedure;

                                                        fbCommNfvendaFmapagtoNfceSyncInsert.Parameters.Add("@pVLR_PAGTO", nfvendaFmapagtoNfcePdv.VLR_PAGTO);
                                                        fbCommNfvendaFmapagtoNfceSyncInsert.Parameters.Add("@pID_NFVENDA", newIdNfvenda);
                                                        fbCommNfvendaFmapagtoNfceSyncInsert.Parameters.Add("@pID_FMANFCE", nfvendaFmapagtoNfcePdv.ID_FMANFCE);
                                                        fbCommNfvendaFmapagtoNfceSyncInsert.Parameters.Add("@pID_PARCELA", nfvendaFmapagtoNfcePdv.ID_PARCELA);

                                                        #endregion Prepara o comando da SP_TRI_NFV_FMAPAGT_SYNC_INSERT

                                                        // Executa a sproc
                                                        newIdnumpag = (int)fbCommNfvendaFmapagtoNfceSyncInsert.ExecuteScalar();
                                                    }

                                                    //TODO: o que fazer com newIdnumpag?
                                                    // montar uma relação com o ID original pra gravar depois na relação forma de pagamento / conta a receber.
                                                    AuxNfvFmaPgtoCtaRec itemAux = new AuxNfvFmaPgtoCtaRec
                                                    {
                                                        //PdvIdNfvenda = nfvenda.ID_NFVENDA,
                                                        PdvIdNumPag = nfvendaFmapagtoNfcePdv.ID_NUMPAG,
                                                        //ServIdNfvenda = newIdNfvenda,
                                                        ServIdNumPag = newIdnumpag
                                                    };
                                                    lstAuxNfvFmaPgtoCtaRec.Add(itemAux);
                                                }

                                                #endregion Gravar as formas de pagamento da nfvenda na retaguarda

                                                #region Pedido da nfvenda (AmbiMAITRE)

                                                //TODO: não há (por enquanto?)

                                                #endregion Pedido da nfvenda (AmbiMAITRE)

                                                #region Itens de nfvenda do PDV

                                                #region Consultar os itens da nfvenda do PDV

                                                tblNfvItemPdv.Clear();
                                                // Busca os itens do cupom pelo ID_CUPOM local (PDV):
                                                //audit("SINCCONTNETDB>> " + "taCupomItemPdv.FillByIdCupom(): " + taCupomItemPdv.FillByIdCupom(tblCupomItemPdv, cupom.ID_CUPOM).ToString());
                                                // SP_TRI_CUPOMITEMGET
                                                taNfvItemPdv.FillByIdNfvenda(tblNfvItemPdv, nfvenda.ID_NFVENDA); // já usa sproc: SP_TRI_CUPOMITEMGET

                                                foreach (FDBDataSetVenda.TB_NFV_ITEMRow nfvItem in tblNfvItemPdv.Rows)
                                                {
                                                    // Os itens do cupom devem referenciar o novo ID do cupom da retaguarda

                                                    #region Gravar os itens da nfvenda

                                                    int newIdNfvItem = 0;

                                                    using (var fbCommNfvItemSyncInsert = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_NFVITEM_SYNC_INSERT

                                                        fbCommNfvItemSyncInsert.Connection = fbConnServ;
                                                        //fbCommNfvItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                        fbCommNfvItemSyncInsert.Transaction = fbTransactServ;

                                                        fbCommNfvItemSyncInsert.CommandText = "SP_TRI_NFVITEM_SYNC_INSERT";
                                                        fbCommNfvItemSyncInsert.CommandType = CommandType.StoredProcedure;

                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pID_NFVENDA", newIdNfvenda);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pID_IDENTIFICADOR", nfvItem.ID_IDENTIFICADOR);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pCFOP", nfvItem.CFOP);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pNUM_ITEM", nfvItem.NUM_ITEM);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pQTD_ITEM", nfvItem.QTD_ITEM);

                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pUNI_MEDIDA", nfvItem.IsUNI_MEDIDANull() ? null : nfvItem.UNI_MEDIDA);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TOTAL", nfvItem.IsVLR_TOTALNull() ? null : (decimal?)nfvItem.VLR_TOTAL);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_DESC", nfvItem.IsVLR_DESCNull() ? null : (decimal?)nfvItem.VLR_DESC);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_CUSTO", nfvItem.IsVLR_CUSTONull() ? null : (decimal?)nfvItem.VLR_CUSTO);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pPRC_LISTA", nfvItem.IsPRC_LISTANull() ? null : (decimal?)nfvItem.PRC_LISTA);

                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pCF", nfvItem.IsCFNull() ? null : nfvItem.CF);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_FRETE", nfvItem.VLR_FRETE);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_SEGURO", nfvItem.IsVLR_SEGURONull() ? null : (decimal?)nfvItem.VLR_SEGURO);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_DESPESA", nfvItem.IsVLR_DESPESANull() ? null : (decimal?)nfvItem.VLR_DESPESA);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pRET_PIS_COF_CSLL", nfvItem.IsRET_PIS_COF_CSLLNull() ? null : (decimal?)nfvItem.RET_PIS_COF_CSLL);

                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pRET_IRRF", nfvItem.IsRET_IRRFNull() ? null : (decimal?)nfvItem.RET_IRRF);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pCOD_ENQ", nfvItem.IsCOD_ENQNull() ? null : nfvItem.COD_ENQ);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pCOD_BASE", nfvItem.IsCOD_BASENull() ? null : nfvItem.COD_BASE);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pCSOSN", nfvItem.IsCSOSNNull() ? null : nfvItem.CSOSN);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pNPED_COMPRA", nfvItem.IsNPED_COMPRANull() ? null : nfvItem.NPED_COMPRA);

                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pITEM_COMPRA", nfvItem.IsITEM_COMPRANull() ? null : (int?)nfvItem.ITEM_COMPRA);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TOTTRIB", nfvItem.IsVLR_TOTTRIBNull() ? null : (decimal?)nfvItem.VLR_TOTTRIB);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pFCI", nfvItem.IsFCINull() ? null : nfvItem.FCI);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_ICM_DESO", nfvItem.IsVLR_ICM_DESONull() ? null : (decimal?)nfvItem.VLR_ICM_DESO);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pID_MOTIVO_DESO", nfvItem.IsID_MOTIVO_DESONull() ? null : (int?)nfvItem.ID_MOTIVO_DESO);

                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pEST_BX", nfvItem.IsEST_BXNull() ? null : nfvItem.EST_BX);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TRIB_FED", nfvItem.IsVLR_TRIB_FEDNull() ? null : (decimal?)nfvItem.VLR_TRIB_FED);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TRIB_EST", nfvItem.IsVLR_TRIB_ESTNull() ? null : (decimal?)nfvItem.VLR_TRIB_EST);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_TRIB_MUN", nfvItem.IsVLR_TRIB_MUNNull() ? null : (decimal?)nfvItem.VLR_TRIB_MUN);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pINCLUIR_FATURA", nfvItem.INCLUIR_FATURA);

                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_UNIT", nfvItem.VLR_UNIT);
                                                        //fbCommNfvItemSyncInsert.Parameters.Add("@pIMP_MANUAL", nfvItem.IsIMP_MANUALNull() ? null : nfvItem.IMP_MANUAL);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pVLR_RETENCAO", nfvItem.VLR_RETENCAO);
                                                        fbCommNfvItemSyncInsert.Parameters.Add("@pREFERENCIA", nfvItem.IsREFERENCIANull() ? null : nfvItem.REFERENCIA);
                                                        //fbCommNfvItemSyncInsert.Parameters.Add("@pCODPROMOSCANNTECH", nfvItem.IsCODPROMOSCANNTECHNull() ? null : (int?)nfvItem.CODPROMOSCANNTECH);


                                                        #endregion Prepara o comando da SP_TRI_NFVITEM_SYNC_INSERT

                                                        try
                                                        {
                                                            // Executa a sproc
                                                            newIdNfvItem = (int)fbCommNfvItemSyncInsert.ExecuteScalar();
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            log.Error($"Erro ao sincronizar item de nfvenda. \npID_NFVENDA: {newIdNfvenda} \nID_IDENTIFICADOR: {nfvItem.ID_IDENTIFICADOR}", ex);
                                                            throw ex;
                                                        }

                                                        //audit("SINCCONTNETDB>> ", "SP_TRI_NFVITEM_SYNC_INSERT(): " + newIdCupomItem.ToString());
                                                    }

                                                    #endregion Gravar os itens da nfvenda

                                                    #region Gravar TB_NFV_ITEM_COFINS

                                                    #region Busca TB_NFV_ITEM_COFINS (PDV)

                                                    tblNfvItemCofinsPdv.Clear();

                                                    using (var taNfvItemCofinsPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_COFINSTableAdapter())
                                                    {
                                                        taNfvItemCofinsPdv.Connection.ConnectionString = _strConnContingency;
                                                        // TRI_MAIT_PEDIDO_ITEM
                                                        taNfvItemCofinsPdv.FillById(tblNfvItemCofinsPdv, nfvItem.ID_NFVITEM); // já usa sproc
                                                    }

                                                    #endregion Busca TB_NFV_ITEM_COFINS (PDV)

                                                    #region Procedimento de gravação de TB_NFV_ITEM_COFINS (servidor)

                                                    foreach (var nfvItemCofinsPdv in tblNfvItemCofinsPdv) // Deve ter apenas 1 item de pedido por item de cupom
                                                    {
                                                        #region Gravar TB_NFV_ITEM_COFINS (serv)

                                                        //int newIdMaitPedItemServ = 0;

                                                        using (var fbCommNfvItemCofinsSyncInsert = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_NFVITEMCOFINS_SYNCINSERT

                                                            fbCommNfvItemCofinsSyncInsert.Connection = fbConnServ;
                                                            //fbCommMaitPedItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommNfvItemCofinsSyncInsert.Transaction = fbTransactServ;

                                                            fbCommNfvItemCofinsSyncInsert.CommandText = "SP_TRI_NFVITEMCOFINS_SYNCINSERT";
                                                            fbCommNfvItemCofinsSyncInsert.CommandType = CommandType.StoredProcedure;

                                                            fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pID_NFVITEM", newIdNfvItem);
                                                            fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pPOR_BC_COFINS", nfvItemCofinsPdv.IsPOR_BC_COFINSNull() ? null : (decimal?)nfvItemCofinsPdv.POR_BC_COFINS);
                                                            fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pCST_COFINS", nfvItemCofinsPdv.IsCST_COFINSNull() ? null : nfvItemCofinsPdv.CST_COFINS);
                                                            fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pALIQ_COFINS", nfvItemCofinsPdv.IsALIQ_COFINSNull() ? null : (decimal?)nfvItemCofinsPdv.ALIQ_COFINS);
                                                            fbCommNfvItemCofinsSyncInsert.Parameters.Add("@pVLR_COFINS", nfvItemCofinsPdv.IsVLR_COFINSNull() ? null : (decimal?)nfvItemCofinsPdv.VLR_COFINS);

                                                            #endregion Prepara o comando da SP_TRI_NFVITEMCOFINS_SYNCINSERT

                                                            //newIdMaitPedItemServ = (int)
                                                            fbCommNfvItemCofinsSyncInsert.ExecuteScalar();

                                                            //// Executa a sproc
                                                            //audit("SINCCONTNETDB >> ", string.Format("SP_TRI_NFVITEMCOFINS_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_IDENTIFICADOR: {1}, pQTD_ITEM: {2}): {3}",
                                                            //                    newIdMaitPedidoServ,
                                                            //                    pedItemPdv.ID_IDENTIFICADOR,
                                                            //                    pedItemPdv.QTD_ITEM,
                                                            //                    newIdMaitPedItemServ));
                                                        }

                                                        #endregion Gravar TB_NFV_ITEM_COFINS (serv)
                                                    }

                                                    #endregion Procedimento de gravação de TB_NFV_ITEM_COFINS (servidor)

                                                    #endregion Gravar TB_NFV_ITEM_COFINS

                                                    #region Gravar TB_NFV_ITEM_PIS

                                                    #region Busca TB_NFV_ITEM_PIS (PDV)

                                                    tblNfvItemPisPdv.Clear();

                                                    using (var taNfvItemPisPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_PISTableAdapter())
                                                    {
                                                        taNfvItemPisPdv.Connection.ConnectionString = _strConnContingency;
                                                        // TRI_MAIT_PEDIDO_ITEM
                                                        taNfvItemPisPdv.FillById(tblNfvItemPisPdv, nfvItem.ID_NFVITEM); // já usa sproc
                                                    }

                                                    #endregion Busca TB_NFV_ITEM_PIS (PDV)

                                                    #region Procedimento de gravação de TB_NFV_ITEM_PIS (servidor)

                                                    foreach (var nfvItemPisPdv in tblNfvItemPisPdv)
                                                    {
                                                        #region Gravar TB_NFV_ITEM_PIS (serv)

                                                        using (var fbCommNfvItemPisSyncInsert = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_NFVITEMPIS_SYNCINSERT

                                                            fbCommNfvItemPisSyncInsert.Connection = fbConnServ;
                                                            //fbCommMaitPedItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommNfvItemPisSyncInsert.Transaction = fbTransactServ;

                                                            fbCommNfvItemPisSyncInsert.CommandText = "SP_TRI_NFVITEMPIS_SYNCINSERT";
                                                            fbCommNfvItemPisSyncInsert.CommandType = CommandType.StoredProcedure;

                                                            fbCommNfvItemPisSyncInsert.Parameters.Add("@pID_NFVITEM", newIdNfvItem);
                                                            fbCommNfvItemPisSyncInsert.Parameters.Add("@pPOR_BC_PIS", nfvItemPisPdv.IsPOR_BC_PISNull() ? null : (decimal?)nfvItemPisPdv.POR_BC_PIS);
                                                            fbCommNfvItemPisSyncInsert.Parameters.Add("@pCST_PIS", nfvItemPisPdv.IsCST_PISNull() ? null : nfvItemPisPdv.CST_PIS);
                                                            fbCommNfvItemPisSyncInsert.Parameters.Add("@pALIQ_PIS", nfvItemPisPdv.IsALIQ_PISNull() ? null : (decimal?)nfvItemPisPdv.ALIQ_PIS);
                                                            fbCommNfvItemPisSyncInsert.Parameters.Add("@pVLR_PIS", nfvItemPisPdv.IsVLR_PISNull() ? null : (decimal?)nfvItemPisPdv.VLR_PIS);

                                                            #endregion Prepara o comando da SP_TRI_NFVITEMPIS_SYNCINSERT

                                                            //newIdMaitPedItemServ = (int)
                                                            fbCommNfvItemPisSyncInsert.ExecuteScalar();

                                                            //// Executa a sproc
                                                            //audit("SINCCONTNETDB >> ", string.Format("SP_TRI_NFVITEMPIS_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_IDENTIFICADOR: {1}, pQTD_ITEM: {2}): {3}",
                                                            //                    newIdMaitPedidoServ,
                                                            //                    pedItemPdv.ID_IDENTIFICADOR,
                                                            //                    pedItemPdv.QTD_ITEM,
                                                            //                    newIdMaitPedItemServ));
                                                        }

                                                        #endregion Gravar TB_NFV_ITEM_PIS (serv)
                                                    }

                                                    #endregion Procedimento de gravação de TB_NFV_ITEM_PIS (servidor)

                                                    #endregion Gravar TB_NFV_ITEM_PIS

                                                    #region Gravar TB_NFV_ITEM_ICMS

                                                    #region Busca TB_NFV_ITEM_ICMS (PDV)

                                                    tblNfvItemIcmsPdv.Clear();

                                                    using (var taNfvItemIcmsPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_ICMSTableAdapter())
                                                    {
                                                        taNfvItemIcmsPdv.Connection.ConnectionString = _strConnContingency;
                                                        // TRI_MAIT_PEDIDO_ITEM
                                                        taNfvItemIcmsPdv.FillById(tblNfvItemIcmsPdv, nfvItem.ID_NFVITEM); // já usa sproc
                                                    }

                                                    #endregion Busca TB_NFV_ITEM_ICMS (PDV)

                                                    #region Procedimento de gravação de TB_NFV_ITEM_ICMS (servidor)

                                                    foreach (var nfvItemIcmsPdv in tblNfvItemIcmsPdv)
                                                    {
                                                        #region Gravar TB_NFV_ITEM_ICMS (serv)

                                                        using (var fbCommNfvItemIcmsSyncInsert = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_NFVITEMICMS_SYNCINSERT

                                                            fbCommNfvItemIcmsSyncInsert.Connection = fbConnServ;
                                                            //fbCommMaitPedItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommNfvItemIcmsSyncInsert.Transaction = fbTransactServ;

                                                            fbCommNfvItemIcmsSyncInsert.CommandText = "SP_TRI_NFVITEMICMS_SYNCINSERT";
                                                            fbCommNfvItemIcmsSyncInsert.CommandType = CommandType.StoredProcedure;

                                                            fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pID_NFVITEM", newIdNfvItem);
                                                            fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pVLR_BC_ICMS", nfvItemIcmsPdv.IsVLR_BC_ICMSNull() ? null : (decimal?)nfvItemIcmsPdv.VLR_BC_ICMS);
                                                            fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pPOR_BC_ICMS", nfvItemIcmsPdv.IsPOR_BC_ICMSNull() ? null : (decimal?)nfvItemIcmsPdv.POR_BC_ICMS);
                                                            fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pCST_ICMS", nfvItemIcmsPdv.IsCST_ICMSNull() ? null : nfvItemIcmsPdv.CST_ICMS);
                                                            fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pALIQ_ICMS", nfvItemIcmsPdv.IsALIQ_ICMSNull() ? null : (decimal?)nfvItemIcmsPdv.ALIQ_ICMS);
                                                            fbCommNfvItemIcmsSyncInsert.Parameters.Add("@pVLR_ICMS", nfvItemIcmsPdv.IsVLR_ICMSNull() ? null : (decimal?)nfvItemIcmsPdv.VLR_ICMS);

                                                            #endregion Prepara o comando da SP_TRI_NFVITEMICMS_SYNCINSERT

                                                            //newIdMaitPedItemServ = (int)
                                                            fbCommNfvItemIcmsSyncInsert.ExecuteScalar();

                                                            //// Executa a sproc
                                                            //audit("SINCCONTNETDB >> ", string.Format("SP_TRI_NFVITEMICMS_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_IDENTIFICADOR: {1}, pQTD_ITEM: {2}): {3}",
                                                            //                    newIdMaitPedidoServ,
                                                            //                    pedItemPdv.ID_IDENTIFICADOR,
                                                            //                    pedItemPdv.QTD_ITEM,
                                                            //                    newIdMaitPedItemServ));
                                                        }

                                                        #endregion Gravar TB_NFV_ITEM_ICMS (serv)
                                                    }

                                                    #endregion Procedimento de gravação de TB_NFV_ITEM_ICMS (servidor)

                                                    #endregion Gravar TB_NFV_ITEM_ICMS

                                                    #region Gravar TB_NFV_ITEM_ST

                                                    #region Busca TB_NFV_ITEM_ST (PDV)

                                                    tblNfvItemStPdv.Clear();

                                                    using (var taNfvItemStPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_STTableAdapter())
                                                    {
                                                        taNfvItemStPdv.Connection.ConnectionString = _strConnContingency;
                                                        // TRI_MAIT_PEDIDO_ITEM
                                                        taNfvItemStPdv.FillById(tblNfvItemStPdv, nfvItem.ID_NFVITEM); // já usa sproc
                                                    }

                                                    #endregion Busca TB_NFV_ITEM_ST (PDV)

                                                    #region Procedimento de gravação de TB_NFV_ITEM_ST (servidor)

                                                    foreach (var nfvItemStPdv in tblNfvItemStPdv)
                                                    {
                                                        #region Gravar TB_NFV_ITEM_ST (serv)

                                                        using (var fbCommNfvItemStSyncInsert = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_NFVITEMST_SYNCINSERT

                                                            fbCommNfvItemStSyncInsert.Connection = fbConnServ;
                                                            //fbCommMaitPedItemSyncInsert.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommNfvItemStSyncInsert.Transaction = fbTransactServ;

                                                            fbCommNfvItemStSyncInsert.CommandText = "SP_TRI_NFVITEMST_SYNCINSERT";
                                                            fbCommNfvItemStSyncInsert.CommandType = CommandType.StoredProcedure;

                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pID_NFVITEM", newIdNfvItem);
                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pPOR_BC_ICMS_ST", nfvItemStPdv.IsPOR_BC_ICMS_STNull() ? null : (decimal?)nfvItemStPdv.POR_BC_ICMS_ST);
                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pVLR_BC_ICMS_ST", nfvItemStPdv.IsVLR_BC_ICMS_STNull() ? null : (decimal?)nfvItemStPdv.VLR_BC_ICMS_ST);
                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pVLR_ST", nfvItemStPdv.IsVLR_STNull() ? null : (decimal?)nfvItemStPdv.VLR_ST);
                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pMVA", nfvItemStPdv.IsMVANull() ? null : (decimal?)nfvItemStPdv.MVA);

                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pALIQ_ST_ORIG", nfvItemStPdv.IsALIQ_ST_ORIGNull() ? null : (decimal?)nfvItemStPdv.ALIQ_ST_ORIG);
                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pALIQ_ST_DEST", nfvItemStPdv.IsALIQ_ST_DESTNull() ? null : (decimal?)nfvItemStPdv.ALIQ_ST_DEST);
                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pINFORMA_ST", nfvItemStPdv.IsINFORMA_STNull() ? null : nfvItemStPdv.INFORMA_ST);
                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pICMS_EFETIVO", nfvItemStPdv.ICMS_EFETIVO);
                                                            fbCommNfvItemStSyncInsert.Parameters.Add("@pVLR_ICMS_SUBSTITUTO", nfvItemStPdv.IsVLR_ICMS_SUBSTITUTONull() ? null : (decimal?)nfvItemStPdv.VLR_ICMS_SUBSTITUTO);

                                                            #endregion Prepara o comando da SP_TRI_NFVITEMST_SYNCINSERT

                                                            //newIdMaitPedItemServ = (int)
                                                            fbCommNfvItemStSyncInsert.ExecuteScalar();

                                                            //// Executa a sproc
                                                            //audit("SINCCONTNETDB >> ", string.Format("SP_TRI_NFVITEMST_SYNCINSERT(pID_MAIT_PEDIDO: {0}, pID_IDENTIFICADOR: {1}, pQTD_ITEM: {2}): {3}",
                                                            //                    newIdMaitPedidoServ,
                                                            //                    pedItemPdv.ID_IDENTIFICADOR,
                                                            //                    pedItemPdv.QTD_ITEM,
                                                            //                    newIdMaitPedItemServ));
                                                        }

                                                        #endregion Gravar TB_NFV_ITEM_ST (serv)
                                                    }

                                                    #endregion Procedimento de gravação de TB_NFV_ITEM_ST (servidor)

                                                    #endregion Gravar TB_NFV_ITEM_ST

                                                    //TODO: sync TB_NFVENDA_TOT -- TALVEZ não seja necessário. Há uma trigger em TB_NFVENDA que atualiza o TOT. Testar no servidor.

                                                    #region Buscar os itens de pedido (AmbiMAITRE) (PDV)

                                                    //TODO: não há (por enquanto?)

                                                    #endregion Buscar os itens de pedido (AmbiMAITRE) (PDV)

                                                    //if (cupom.IsQTD_MAIT_PED_CUPOMNull() || cupom.QTD_MAIT_PED_CUPOM <= 0)

                                                    #region Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)

                                                    #region Atualizar no servidor a quantidade em estoque

                                                    using (var fbCommEstProdutoQtdServ = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                        fbCommEstProdutoQtdServ.Connection = fbConnServ;
                                                        //fbCommEstProdutoQtdServ.Connection.ConnectionString = _strConnNetwork;
                                                        fbCommEstProdutoQtdServ.Transaction = fbTransactServ;

                                                        fbCommEstProdutoQtdServ.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
                                                        fbCommEstProdutoQtdServ.CommandType = CommandType.StoredProcedure;

                                                        fbCommEstProdutoQtdServ.Parameters.Add("@pQTD_ITEM", nfvItem.QTD_ITEM);
                                                        fbCommEstProdutoQtdServ.Parameters.Add("@pID_IDENTIF", nfvItem.ID_IDENTIFICADOR);
                                                        fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPPRO", 0); // cupomItem.IsID_COMPPRONull() ? 0 : cupomItem.ID_COMPPRO); // AmbiMAITRE
                                                        fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPOSICAO", nfvItem.IsID_COMPOSICAONull() ? 0 : nfvItem.ID_COMPOSICAO);

                                                        #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                        try
                                                        {
                                                            // Executa a sproc
                                                            fbCommEstProdutoQtdServ.ExecuteScalar();
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            log.Error("Erro ao deduzir quantidade em estoque (Serv): \npQTD_ITEM=" + nfvItem.QTD_ITEM.ToString() +
                                                                               "\npID_IDENTIF=" + nfvItem.ID_IDENTIFICADOR.ToString(), ex);
                                                            throw ex;
                                                        }
                                                    }

                                                    #endregion Atualizar no servidor a quantidade em estoque

                                                    #region Atualizar no PDV a quantidade em estoque

                                                    // Já que todo o cadastro de produtos foi copiado do Serv pro PDV na etapa anterior, 
                                                    // as quantidades em estoque devem ser redefinidas
                                                    //using (var fbCommEstProdutoQtdPdv = new FbCommand())
                                                    //{
                                                    //    #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                    //    fbCommEstProdutoQtdPdv.Connection = fbConnPdv;
                                                    //    //fbCommEstProdutoQtdPdv.Connection.ConnectionString = _strConnContingency;
                                                    //    fbCommEstProdutoQtdPdv.Transaction = fbTransactPdv;

                                                    //    fbCommEstProdutoQtdPdv.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
                                                    //    fbCommEstProdutoQtdPdv.CommandType = CommandType.StoredProcedure;

                                                    //    fbCommEstProdutoQtdPdv.Parameters.Add("@pQTD_ITEM", nfvItem.QTD_ITEM);
                                                    //    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_IDENTIF", nfvItem.ID_IDENTIFICADOR);
                                                    //    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPPRO", 0); // nfvItem.IsID_COMPPRONull() ? 0 : nfvItem.ID_COMPPRO); // AmbiMAITRE
                                                    //    fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPOSICAO", nfvItem.IsID_COMPOSICAONull() ? 0 : nfvItem.ID_COMPOSICAO);

                                                    //    #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                    //    try
                                                    //    {
                                                    //        // Executa a sproc
                                                    //        fbCommEstProdutoQtdPdv.ExecuteScalar();
                                                    //    }
                                                    //    catch (Exception ex)
                                                    //    {
                                                    //        gravarMensagemErro("Erro ao deduzir quantidade em estoque (PDV): \npQTD_ITEM=" + nfvItem.QTD_ITEM.ToString() +
                                                    //                           "\npID_IDENTIF=" + nfvItem.ID_IDENTIFICADOR.ToString() +
                                                    //                            " \nMais infos: " + RetornarMensagemErro(ex, true));
                                                    //        throw ex;
                                                    //    }
                                                    //}

                                                    #endregion Atualizar no PDV a quantidade em estoque

                                                    #endregion Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)
                                                }

                                                #endregion Consultar os itens da nfvenda do PDV

                                                #endregion Itens de nfvenda do PDV

                                                #region Vendas a prazo

                                                #region Verifica se a nfvenda é a prazo

                                                // Como verificar se o cupom é uma venda a prazo?
                                                if (!nfvenda.IsQTD_CTARECNull() && nfvenda.QTD_CTAREC > 0)
                                                {

                                                    using (var taCtaRecPdv = new TB_CONTA_RECEBERTableAdapter())
                                                    {
                                                        taCtaRecPdv.Connection.ConnectionString = _strConnContingency;

                                                        tblCtaRecPdv.Clear();
                                                        // Consultar todas as contas a receber do cupom
                                                        //audit("SINCCONTNETDB>> " + "taCtaRecPdv.FillByIdCupom(): " + /*taCtaRecPdv.FillByIdCupom(tblCtaRecPdv, cupom.ID_CUPOM)*//*.ToString());
                                                        taCtaRecPdv.FillByIdNfvenda(tblCtaRecPdv, nfvenda.ID_NFVENDA); // já usa sproc
                                                    }

                                                    // Percorre por cada conta a receber que o cupom possui:
                                                    foreach (FDBDataSet.TB_CONTA_RECEBERRow ctaRecPdv in tblCtaRecPdv)
                                                    {
                                                        int newIdCtarec = 0;

                                                        #region Grava conta a receber na retaguarda

                                                        // TB_CONTA_RECEBER
                                                        using (var fbCommCtaRecSyncInsertServ = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_CTAREC_SYNC_INSERT

                                                            fbCommCtaRecSyncInsertServ.Connection = fbConnServ;
                                                            //fbCommCtaRecSyncInsertServ.Connection.ConnectionString = _strConnNetwork;
                                                            fbCommCtaRecSyncInsertServ.Transaction = fbTransactServ;

                                                            fbCommCtaRecSyncInsertServ.CommandText = "SP_TRI_CTAREC_SYNC_INSERT";
                                                            fbCommCtaRecSyncInsertServ.CommandType = CommandType.StoredProcedure;

                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pDOCUMENTO", ctaRecPdv.DOCUMENTO);
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pHISTORICO", (ctaRecPdv.IsHISTORICONull() ? null : ctaRecPdv.HISTORICO));
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pDT_EMISSAO", ctaRecPdv.DT_EMISSAO);
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pDT_VENCTO", ctaRecPdv.DT_VENCTO);
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pVLR_CTAREC", ctaRecPdv.VLR_CTAREC);
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pTIP_CTAREC", ctaRecPdv.TIP_CTAREC);
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pID_PORTADOR", ctaRecPdv.ID_PORTADOR);
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pID_CLIENTE", ctaRecPdv.ID_CLIENTE);
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pINV_REFERENCIA", (ctaRecPdv.IsINV_REFERENCIANull() ? null : ctaRecPdv.INV_REFERENCIA));
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pDT_VENCTO_ORIG", (ctaRecPdv.IsDT_VENCTO_ORIGNull() ? null : (DateTime?)ctaRecPdv.DT_VENCTO_ORIG));
                                                            fbCommCtaRecSyncInsertServ.Parameters.Add("@pNSU_CARTAO", (ctaRecPdv.IsNSU_CARTAONull() ? null : ctaRecPdv.NSU_CARTAO));

                                                            #endregion Prepara o comando da SP_TRI_CTAREC_SYNC_INSERT

                                                            try
                                                            {
                                                                // Executa a sproc
                                                                newIdCtarec = (int)fbCommCtaRecSyncInsertServ.ExecuteScalar();
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                log.Error("Erro ABSURDO ao gravar conta a receber. Eis os parâmetros da gravação: \npINV_REFERENCIA=" +
                                                                    (ctaRecPdv.IsINV_REFERENCIANull() ? "null" : ctaRecPdv.INV_REFERENCIA.ToString()) + "\n" +
                                                                    "pDOCUMENTO=" + ctaRecPdv.DOCUMENTO.ToString() + "\n" +
                                                                    "pHISTORICO=" + (ctaRecPdv.IsHISTORICONull() ? "null" : ctaRecPdv.HISTORICO.ToString()) + "\n" +
                                                                    //"cupom.COO=" + cupom.COO.ToString() + "\n" +
                                                                    "nfvenda.NF_NUMERO=" + nfvenda.NF_NUMERO.ToString() + "\n" +
                                                                    "newIdNfvenda=" + newIdNfvenda.ToString(), ex);
                                                                throw ex;
                                                            }
                                                        }

                                                        #endregion Grava conta a receber na retaguarda

                                                        #region Gravar a referência entre cupom e conta a receber

                                                        using (var fbCommNfvCtarecInsertServ = new FbCommand())
                                                        {
                                                            #region Prepara o comando

                                                            fbCommNfvCtarecInsertServ.Connection = fbConnServ;
                                                            fbCommNfvCtarecInsertServ.Transaction = fbTransactServ;
                                                            //fbCommNfvCtarecInsertServ.Connection.ConnectionString = _strConnNetwork;

                                                            fbCommNfvCtarecInsertServ.CommandText = "INSERT INTO TB_NFV_CTAREC (ID_NFVENDA, ID_CTAREC, ID_NUMPAG) VALUES(@ID_NFVENDA, @ID_CTAREC, @ID_NUMPAG); ";
                                                            fbCommNfvCtarecInsertServ.CommandType = CommandType.Text;

                                                            fbCommNfvCtarecInsertServ.Parameters.Add("@ID_NFVENDA", newIdNfvenda);
                                                            fbCommNfvCtarecInsertServ.Parameters.Add("@ID_CTAREC", newIdCtarec);

                                                            int? newIdNumPag = null;

                                                            if (!ctaRecPdv.IsID_NUMPAGNull())
                                                            {
                                                                newIdNumPag = (lstAuxNfvFmaPgtoCtaRec.Find(t => t.PdvIdNumPag.Equals(ctaRecPdv.ID_NUMPAG))).ServIdNumPag;
                                                            }

                                                            fbCommNfvCtarecInsertServ.Parameters.Add("@ID_NUMPAG", newIdNumPag);

                                                            #endregion Prepara o comando

                                                            // Executa a sproc
                                                            fbCommNfvCtarecInsertServ.ExecuteNonQuery();
                                                        }

                                                        #endregion Gravar a referência entre cupom e conta a receber

                                                        #region Consultar as movimentações diárias da conta a receber

                                                        using (var taMovDiarioPdv = new TB_MOVDIARIOTableAdapter())
                                                        {
                                                            taMovDiarioPdv.Connection.ConnectionString = _strConnContingency;

                                                            tblMovDiarioPdv.Clear();
                                                            //audit("SINCCONTNETDB>> " + "taMovDiarioPdv.FillByIdCtarec(): " + taMovDiarioPdv.FillByIdCtarec(tblMovDiarioPdv, ctaRecPdv.ID_CTAREC).ToString());
                                                            taMovDiarioPdv.FillByIdCtarec(tblMovDiarioPdv, ctaRecPdv.ID_CTAREC); // já usa sproc
                                                        }

                                                        #endregion Consultar as movimentações diárias da conta a receber

                                                        #region Gravar movimentação diária referente à conta a receber

                                                        if (tblMovDiarioPdv != null && tblMovDiarioPdv.Rows.Count > 0)
                                                        {
                                                            foreach (FDBDataSet.TB_MOVDIARIORow movdiarioPdv in tblMovDiarioPdv)
                                                            {
                                                                int newIdMovto = 0;
                                                                //movdiarioPdv.SYNCED = 1;
                                                                // TB_MOVDIARIO
                                                                using (var fbCommMovDiarioMovtoSyncInsertServ = new FbCommand())
                                                                {
                                                                    #region Prepara o comando da SP_TRI_MOVTO_SYNC_INSERT

                                                                    fbCommMovDiarioMovtoSyncInsertServ.Connection = fbConnServ;
                                                                    //fbCommMovDiarioMovtoSyncInsertServ.Connection.ConnectionString = _strConnNetwork;
                                                                    fbCommMovDiarioMovtoSyncInsertServ.Transaction = fbTransactServ;

                                                                    fbCommMovDiarioMovtoSyncInsertServ.CommandText = "SP_TRI_MOVTO_SYNC_INSERT";
                                                                    fbCommMovDiarioMovtoSyncInsertServ.CommandType = CommandType.StoredProcedure;

                                                                    fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pDT_MOVTO", (movdiarioPdv.IsDT_MOVTONull() ? null : (DateTime?)movdiarioPdv.DT_MOVTO));
                                                                    fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pHR_MOVTO", (movdiarioPdv.IsHR_MOVTONull() ? null : (TimeSpan?)movdiarioPdv.HR_MOVTO));
                                                                    fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pHISTORICO", (movdiarioPdv.IsHISTORICONull() ? null : movdiarioPdv.HISTORICO));
                                                                    fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pTIP_MOVTO", (movdiarioPdv.IsTIP_MOVTONull() ? null : movdiarioPdv.TIP_MOVTO));
                                                                    fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pVLR_MOVTO", (movdiarioPdv.IsVLR_MOVTONull() ? null : (decimal?)movdiarioPdv.VLR_MOVTO));
                                                                    fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pID_CTAPLA", (movdiarioPdv.IsID_CTAPLANull() ? null : (short?)movdiarioPdv.ID_CTAPLA));
                                                                    fbCommMovDiarioMovtoSyncInsertServ.Parameters.Add("@pSYNCED", 1);

                                                                    #endregion Prepara o comando da SP_TRI_MOVTO_SYNC_INSERT

                                                                    try
                                                                    {
                                                                        // Executa a sproc
                                                                        newIdMovto = (int)fbCommMovDiarioMovtoSyncInsertServ.ExecuteScalar(); //TODO: esse trecho é problemático para a Estilo K, às vezes apresenta dead-lock
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        log.Error("Erro ao sync movdiario (Serv): \npDT_MOVTO=" + (movdiarioPdv.IsDT_MOVTONull() ? "null" : movdiarioPdv.DT_MOVTO.ToString()) +
                                                                                           "\npHR_MOVTO=" + (movdiarioPdv.IsHR_MOVTONull() ? "null" : movdiarioPdv.HR_MOVTO.ToString()) +
                                                                                           "\npHISTORICO=" + (movdiarioPdv.IsHISTORICONull() ? "null" : movdiarioPdv.HISTORICO.ToString()) +
                                                                                           "\npTIP_MOVTO=" + (movdiarioPdv.IsTIP_MOVTONull() ? "null" : movdiarioPdv.TIP_MOVTO.ToString()) +
                                                                                           "\npVLR_MOVTO=" + (movdiarioPdv.IsVLR_MOVTONull() ? "null" : movdiarioPdv.VLR_MOVTO.ToString()) +
                                                                                           "\npID_CTAPLA=" + (movdiarioPdv.IsID_CTAPLANull() ? "null" : movdiarioPdv.ID_CTAPLA.ToString()), ex);
                                                                        throw ex;
                                                                    }
                                                                }

                                                                #region Gravar a referência entre a conta a receber e a movimentação diária

                                                                using (var fbCommCtarecMovtoInsertServ = new FbCommand())
                                                                {
                                                                    #region Prepara o comando

                                                                    fbCommCtarecMovtoInsertServ.Connection = fbConnServ;
                                                                    fbCommCtarecMovtoInsertServ.Transaction = fbTransactServ;
                                                                    //fbCommCtarecMovtoInsertServ.Connection.ConnectionString = _strConnNetwork;

                                                                    fbCommCtarecMovtoInsertServ.CommandText = "INSERT INTO TB_CTAREC_MOVTO (ID_MOVTO, ID_CTAREC) VALUES(@ID_MOVTO, @ID_CTAREC); ";
                                                                    fbCommCtarecMovtoInsertServ.CommandType = CommandType.Text;

                                                                    fbCommCtarecMovtoInsertServ.Parameters.Add("@ID_MOVTO", newIdMovto);
                                                                    fbCommCtarecMovtoInsertServ.Parameters.Add("@ID_CTAREC", newIdCtarec);

                                                                    #endregion Prepara o comando

                                                                    // Executa a sproc
                                                                    fbCommCtarecMovtoInsertServ.ExecuteNonQuery();
                                                                }

                                                                #endregion Gravar a referência entre a conta a receber e a movimentação diária

                                                                #region Indicar que o fechamento de caixa foi sincronizado

                                                                using (var taMovDiarioPdv = new TB_MOVDIARIOTableAdapter())
                                                                {
                                                                    taMovDiarioPdv.Connection = fbConnPdv;
                                                                    taMovDiarioPdv.Transaction = fbTransactPdv;
                                                                    //taMovDiarioPdv.Connection.ConnectionString = _strConnContingency;

                                                                    taMovDiarioPdv.SP_TRI_MOVTOSETSYNCED(movdiarioPdv.ID_MOVTO, 1);
                                                                }

                                                                #endregion Indicar que o fechamento de caixa foi sincronizado
                                                            }
                                                        }

                                                        #endregion Gravar movimentação diária referente à conta a receber
                                                    }
                                                }

                                                #endregion Verifica se a nfvenda é a prazo

                                                #endregion Vendas a prazo

                                                #region Verificar se a nfvenda foi cancelado: reativar o orçamento vinculado, se houver

                                                //TODO: completar no orçamento

                                                //if (nfvenda.STATUS == "C" || nfvenda.STATUS == "X")
                                                //{
                                                //    // TRI_PDV_ORCA_NFVENDA_REL
                                                //    using (var taOrcaServ = new DataSets.FDBDataSetOrcamTableAdapters.TRI_PDV_ORCA_NFVENDA_RELTableAdapter())
                                                //    {
                                                //        taOrcaServ.Connection = fbConnServ;
                                                //        taOrcaServ.Transaction = fbTransactServ;
                                                //        //taOrcaServ.Connection.ConnectionString = _strConnNetwork;

                                                //        audit("SINCCONTNETDB>> ", string.Format("(nfvenda cancelado antes de sync) taOrcaServ.SP_TRI_ORCA_REATIVAORCA({1}): {0}",
                                                //                            taOrcaServ.SP_TRI_ORCA_REATIVAORCA(nfvenda.ID_NFVENDA).Safestring(),
                                                //                            nfvenda.ID_NFVENDA.Safestring()));
                                                //    }
                                                //}

                                                #endregion Verificar se a nfvenda foi cancelado: reativar o orçamento vinculado, se houver

                                                #region Indicar que a nfvenda foi synced

                                                using (var fbCommNfvendaSetSynced = new FbCommand())
                                                {
                                                    #region Prepara o comando da SP_TRI_NFVENDASETSYNCED

                                                    fbCommNfvendaSetSynced.Connection = fbConnPdv;
                                                    fbCommNfvendaSetSynced.Transaction = fbTransactPdv;

                                                    //fbCommNfvendaSetSynced.CommandText = "SP_TRI_CUPOMSETSYNCED";
                                                    fbCommNfvendaSetSynced.CommandText = "SP_TRI_NFVENDA_SETSYNCED";
                                                    fbCommNfvendaSetSynced.CommandType = CommandType.StoredProcedure;

                                                    fbCommNfvendaSetSynced.Parameters.Add("@pIdNfvenda", nfvenda.ID_NFVENDA);
                                                    fbCommNfvendaSetSynced.Parameters.Add("@pSynced", 1);

                                                    #endregion Prepara o comando da SP_TRI_NFVENDASETSYNCED

                                                    // Executa a sproc
                                                    fbCommNfvendaSetSynced.ExecuteScalar();
                                                }

                                                #endregion Indicar que a nfvenda foi synced

                                                //fbConnPdv.Close();
                                                //fbConnServ.Close();

                                                // Finaliza a transação:
                                                //transactionScopeCupons.Complete();
                                                fbTransactServ.Commit();
                                                fbTransactPdv.Commit();
                                            }
                                            catch (TransactionAbortedException taEx)
                                            {
                                                log.Error("TransactionAbortedException", taEx);
                                                fbTransactServ.Rollback();
                                                fbTransactPdv.Rollback();
                                            }
                                            catch (Exception ex)
                                            {
                                                log.Error("Erro durante a transação de nfvendas:", ex);
                                                fbTransactServ.Rollback();
                                                fbTransactPdv.Rollback();
                                            }
                                        }
                                        #region Forçar a execução da trigger ref. a tabela TB_NFVENDA_TOT

                                        using (var fbCommNfvendaForceTriggerUpdate = new FbCommand())
                                        {
                                            #region Prepara o comando da SP_TRI_NFVENDASETSYNCED

                                            fbCommNfvendaForceTriggerUpdate.Connection = fbConnServ;

                                            fbCommNfvendaForceTriggerUpdate.CommandText = "UPDATE TB_NFVENDA SET STATUS = 'I' WHERE ID_NFVENDA = @Param1 AND STATUS = 'I'";
                                            fbCommNfvendaForceTriggerUpdate.CommandType = CommandType.Text;

                                            fbCommNfvendaForceTriggerUpdate.Parameters.Add("@Param1", newIdNfvenda);


                                            #endregion Prepara o comando da SP_TRI_NFVENDASETSYNCED

                                            // Executa o ad-hoc
                                            fbCommNfvendaForceTriggerUpdate.ExecuteScalar();
                                        }


                                        #endregion

                                    }
                                    #endregion Gravar a nfvenda na retaguarda (transação)


                                }

                                log.Debug(string.Format("Lote {0} de nfvendas processado!", contLote.ToString()));

                                #endregion Sincroniza (manda para a retaguarda)

                                #region Prepara o próximo lote

                                // Limpa a tabela para pegar o próximo lote (é necessário limpar mesmo? o comando seguinte deveria sobrescrevê-lo):
                                tblNfvendaUnsynced.Clear();
                                log.Debug("taNfvendaUnsynced.FillByNfvendaSync(): " + taNfvendaUnsynced.FillByNfvendaSync(tblNfvendaUnsynced, 0).ToString()); // já usa sproc

                                #endregion Prepara o próximo lote
                            }

                            #region NOPE - CLIPP RULES NO MORE
                            //taEstProdutoPdv.SP_TRI_FIX_CLIPP_RULES();
                            //taEstProdutoServ.SP_TRI_FIX_CLIPP_RULES();
                            #endregion NOPE - CLIPP RULES NO MORE
                        }
                        #endregion Procedimento executado enquanto houver cupons para sincronizar
                    }
                    #region Manipular Exception
                    catch (Exception ex)
                    {
                        log.Error("Erro ao sincronizar",  ex);
                        GravarErroSync("Erro ao sincronizar nfvendas", tblCtaRecPdv, ex);
                        GravarErroSync("Erro ao sincronizar nfvendas", tblNfvendaFmapagtoNfcePdv, ex);
                        GravarErroSync("Erro ao sincronizar nfvendas", tblNfvItemPdv, ex);
                        GravarErroSync("Erro ao sincronizar nfvendas", tblNfvendaUnsynced, ex);
                        GravarErroSync("Erro ao sincronizar nfvendas", tblMovDiarioPdv, ex);
                        throw ex;
                    }
                    #endregion Manipular Exception
                    #region Limpeza da transação
                    finally
                    {
                        #region Trata disposable objects

                        #region(cupons, contas a receber e movimentação diária)

                        if (taNfvendaFmaPagtoNfcePdv != null) { taNfvendaFmaPagtoNfcePdv.Dispose(); }
                        //if (taCupomFmaPagtoServ != null) { taCupomFmaPagtoServ.Dispose(); }

                        //if (taTrocaPdv != null) { taTrocaPdv.Dispose(); }
                        //if (taTrocaServ != null) { taTrocaServ.Dispose(); }
                        //if (tblTrocaPdv != null) { tblTrocaPdv.Dispose(); }

                        //if (taCupomServ != null) { taCupomServ.Dispose(); }
                        //if (taCtaRecServ != null) { taCtaRecServ.Dispose(); }
                        //if (taCupomCtarecServ != null) { taCupomCtarecServ.Dispose(); }
                        //if (taMovDiarioServ != null) { taMovDiarioServ.Dispose(); }
                        //if (taCtarecMovtoServ != null) { taCtarecMovtoServ.Dispose(); }

                        //if (taCtaRecPdv != null) { taCtaRecPdv.Dispose(); }
                        //if (taMovDiarioPdv != null) { taMovDiarioPdv.Dispose(); }

                        if (tblNfvendaFmapagtoNfcePdv != null) { tblNfvendaFmapagtoNfcePdv.Dispose(); }
                        if (tblCtaRecPdv != null) { tblCtaRecPdv.Dispose(); }
                        if (tblMovDiarioPdv != null) { tblMovDiarioPdv.Dispose(); }

                        #endregion (cupons, contas a receber e movimentação diária)

                        #region (item de cupom)

                        if (tblNfvItemPdv != null) { tblNfvItemPdv.Dispose(); }
                        if (taNfvItemPdv != null) { taNfvItemPdv.Dispose(); }
                        //if (taCupomItemServ != null) { taCupomItemServ.Dispose(); }

                        #endregion (item de cupom)

                        #region (cupons unsynced, produtos)

                        if (taNfvendaUnsynced != null) { taNfvendaUnsynced.Dispose(); }
                        if (tblNfvendaUnsynced != null) { tblNfvendaUnsynced.Dispose(); }

                        if (taEstProdutoServ != null) { taEstProdutoServ.Dispose(); }
                        if (taEstProdutoPdv != null) { taEstProdutoPdv.Dispose(); }

                        #endregion (cupons unsynced, produtos)

                        taSatPdv?.Dispose();
                        tblSatPdv?.Dispose();
                        taSatCancPdv?.Dispose();
                        tblSatCancPdv?.Dispose();

                        tblNfvItemCofinsPdv?.Dispose();
                        tblNfvItemPisPdv?.Dispose();
                        tblNfvItemIcmsPdv?.Dispose();
                        tblNfvItemStPdv?.Dispose();

                        #endregion Trata disposable objects
                    }
                    #endregion Limpeza da transação
                }

                #endregion Padrão, unsynced

                #region Nfvendas sincronizadas e posteriormente canceladas
                {
                    #region Cria os TableAdapters, DataTables e variáveis

                    var taNfvendaSyncedCancelPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDATableAdapter();
                    taNfvendaSyncedCancelPdv.Connection.ConnectionString = _strConnContingency;
                    var tblNfvendaSyncedCancelPdv = new FDBDataSetVenda.TB_NFVENDADataTable();

                    var taEstProdutoServ = new TB_EST_PRODUTOTableAdapter();
                    taEstProdutoServ.Connection.ConnectionString = _strConnNetwork;

                    var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter();
                    taEstProdutoPdv.Connection.ConnectionString = _strConnContingency;

                    int intCountLoteNfvendaSyncedCancel = 0;

                    #endregion Cria os TableAdapters, DataTables e variáveis

                    try
                    {
                        // Busca todos os cupons que foram synced e posteriormente cancelados (TIP_QUERY = 1)
                        // Lembrando que a sproc executada abaixo retorna até 200 registros por vez.
                        log.Debug("taNfvendaSyncedCancelPdv.FillByNfvendaSync(): " + taNfvendaSyncedCancelPdv.FillByNfvendaSync(tblNfvendaSyncedCancelPdv, 1).ToString()); // já usa sproc

                        while (tblNfvendaSyncedCancelPdv != null && tblNfvendaSyncedCancelPdv.Rows.Count > 0)
                        {
                            intCountLoteNfvendaSyncedCancel++;

                            #region NOPE - Break Clipp rules agora é permanente
                            //// Para repor quantidade em estoque sem dar problemas
                            //taEstProdutoServ.SP_TRI_BREAK_CLIPP_RULES();
                            //taEstProdutoPdv.SP_TRI_BREAK_CLIPP_RULES();
                            #endregion NOPE - Break Clipp rules agora é permanente

                            // Percorre por cada cupom cancelado:
                            foreach (FDBDataSetVenda.TB_NFVENDARow nfvendaCancelPdv in tblNfvendaSyncedCancelPdv)
                            {
                                #region Validações

                                //// Foi necessário adaptar o COO como o ID_CUPOM negativo para sistema legado
                                //if (cupomCancelPdv.IsCOONull()) { cupomCancelPdv.COO = cupomCancelPdv.ID_CUPOM * -1; }
                                //if (cupomCancelPdv.IsNUM_CAIXANull()) { cupomCancelPdv.NUM_CAIXA = 0; }

                                #endregion Validações

                                #region Iniciar o procedimento de cancelamento na retaguarda
                                //using (var transactionScopeCupomResync = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(1, 0, 0, 0)))
                                using (var transactionScopeNfvendaResync = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                                {
                                    // Define a conexão com o banco do servidor:
                                    using (var fbConnServ = new FbConnection(_strConnNetwork))
                                    // Define a conexão com o banco do PDV:
                                    using (var fbConnPdv = new FbConnection(_strConnContingency))
                                    using (var tblNfvendaItemSyncedCancelPdv = new FDBDataSetVenda.TB_NFV_ITEMDataTable())
                                    using (var tblCtarecServ = new FDBDataSet.TB_CONTA_RECEBERDataTable())
                                    using (var tblMovtoServ = new FDBDataSet.TB_MOVDIARIODataTable())
                                    {
                                        fbConnServ.Open();
                                        fbConnPdv.Open();

                                        // Verificar se a nfvenda tem conta a receber:
                                        if (!nfvendaCancelPdv.IsQTD_CTARECNull() && nfvendaCancelPdv.QTD_CTAREC > 0)
                                        {
                                            #region Busca as contas a receber da nfvenda no serv
                                            try
                                            {
                                                using (var taCtarecServ = new TB_CONTA_RECEBERTableAdapter())
                                                {
                                                    taCtarecServ.Connection = fbConnServ;
                                                    //audit("SINCCONTNETDB>> " + "taCtarecServ.FillByCooNumcaixa(): " + taCtarecServ.FillByCooNumcaixa(tblCtarecServ, cupomCancelPdv.COO, cupomCancelPdv.NUM_CAIXA).ToString());

                                                    //TODO -- DONE --: qual é a chave de identificação de uma nfvenda equivalente para ambas as bases de dados (cliente/servidor)?
                                                    // Deve ser NF_NUMERO e NF_SERIE

                                                    taCtarecServ.FillByNfNumeroSerie(tblCtarecServ, nfvendaCancelPdv.NF_NUMERO, nfvendaCancelPdv.NF_SERIE).ToString(); // já usa sproc
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                log.Error("Erro ao consultar contas a receber no servidor ( / NF_NUMERO = " + nfvendaCancelPdv.NF_NUMERO + " / NF_SERIE = " + nfvendaCancelPdv.NF_SERIE + "): ", ex);
                                                throw ex;
                                            }
                                            #endregion Busca as contas a receber da nfvenda no serv

                                            #region Percorre por cada conta a receber no servidor
                                            foreach (FDBDataSet.TB_CONTA_RECEBERRow ctarecServ in tblCtarecServ)
                                            {
                                                #region Busca os movimentos diários da conta a receber
                                                using (var taMovtoServ = new TB_MOVDIARIOTableAdapter())
                                                {
                                                    taMovtoServ.Connection = fbConnServ;
                                                    tblMovtoServ.Clear();
                                                    //audit("SINCCONTNETDB>> " + "taMovtoServ.FillByIdCtarec(): " + taMovtoServ.FillByIdCtarec(tblMovtoServ, ctarecServ.ID_CTAREC).ToString());
                                                    taMovtoServ.FillByIdCtarec(tblMovtoServ, ctarecServ.ID_CTAREC); // já usa sproc
                                                }
                                                #endregion Busca os movimentos diários da conta a receber

                                                #region Percorre por cada movimentação diária do servidor
                                                foreach (FDBDataSet.TB_MOVDIARIORow movtoServ in tblMovtoServ)
                                                {
                                                    #region Apagar TB_CTAREC_MOVTO
                                                    //taCtarecMovtoServ.SP_TRI_CTARECMOVTO_SYNC_DEL(movtoServ.ID_MOVTO, ctarecServ.ID_CTAREC);
                                                    using (var fbCommCtarecMovtoSyncDelServ = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_CTARECMOVTO_SYNC_DEL

                                                        fbCommCtarecMovtoSyncDelServ.Connection = fbConnServ;

                                                        fbCommCtarecMovtoSyncDelServ.CommandText = "SP_TRI_CTARECMOVTO_SYNC_DEL";
                                                        fbCommCtarecMovtoSyncDelServ.CommandType = CommandType.StoredProcedure;

                                                        fbCommCtarecMovtoSyncDelServ.Parameters.Add("@pID_MOVTO", movtoServ.ID_MOVTO);
                                                        fbCommCtarecMovtoSyncDelServ.Parameters.Add("@pID_CTAREC", ctarecServ.ID_CTAREC);

                                                        #endregion Prepara o comando da SP_TRI_CTARECMOVTO_SYNC_DEL

                                                        // Executa a sproc
                                                        fbCommCtarecMovtoSyncDelServ.ExecuteScalar();
                                                    }
                                                    #endregion Apagar TB_CTAREC_MOVTO

                                                    #region Apagar TB_MOVDIARIO
                                                    //taMovtoServ.SP_TRI_MOVTO_SYNC_DEL(movtoServ.ID_MOVTO);
                                                    using (var fbCommMovtoSyncDelServ = new FbCommand())
                                                    {
                                                        #region Prepara o comando da SP_TRI_MOVTO_SYNC_DEL

                                                        fbCommMovtoSyncDelServ.Connection = fbConnServ;

                                                        fbCommMovtoSyncDelServ.CommandText = "SP_TRI_MOVTO_SYNC_DEL";
                                                        fbCommMovtoSyncDelServ.CommandType = CommandType.StoredProcedure;

                                                        fbCommMovtoSyncDelServ.Parameters.Add("@pID_MOVTO", movtoServ.ID_MOVTO);

                                                        #endregion Prepara o comando da SP_TRI_MOVTO_SYNC_DEL

                                                        // Executa a sproc
                                                        fbCommMovtoSyncDelServ.ExecuteScalar();
                                                    }
                                                    #endregion Apagar TB_MOVDIARIO
                                                }
                                                #endregion Percorre por cada movimentação diária do servidor

                                                #region Apagar o vínculo TB_NFV_CTAREC
                                                //taCupomCtarecServ.SP_TRI_CUPOMCTAREC_SYNC_DEL(cupomCancelPdv.COO, cupomCancelPdv.NUM_CAIXA, ctarecServ.ID_CTAREC);
                                                using (var fbCommNfvCtarecSyncDelServ = new FbCommand())
                                                {
                                                    #region Prepara o comando da SP_TRI_NFV_CTAREC_SYNC_DEL

                                                    fbCommNfvCtarecSyncDelServ.Connection = fbConnServ;

                                                    //TODO: talvez seja necessário adaptar a sproc para usar a ID_NUMPAG...

                                                    fbCommNfvCtarecSyncDelServ.CommandText = "SP_TRI_NFV_CTAREC_SYNC_DEL";
                                                    fbCommNfvCtarecSyncDelServ.CommandType = CommandType.StoredProcedure;

                                                    fbCommNfvCtarecSyncDelServ.Parameters.Add("@pNfNumero", nfvendaCancelPdv.NF_NUMERO);
                                                    fbCommNfvCtarecSyncDelServ.Parameters.Add("@pNfSerie", nfvendaCancelPdv.NF_SERIE);
                                                    fbCommNfvCtarecSyncDelServ.Parameters.Add("@pIdCtarec", ctarecServ.ID_CTAREC);

                                                    #endregion Prepara o comando da SP_TRI_NFV_CTAREC_SYNC_DEL

                                                    // Executa a sproc
                                                    try
                                                    {
                                                        fbCommNfvCtarecSyncDelServ.ExecuteScalar();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        log.Error(String.Format("NF_NUMERO: {1}, NF_SERIE: {2}, ID_CTAREC {3}",
                                                                                         nfvendaCancelPdv.NF_NUMERO,
                                                                                         nfvendaCancelPdv.NF_SERIE,
                                                                                         ctarecServ.ID_CTAREC), ex);
                                                        throw ex;
                                                    }

                                                }
                                                #endregion Apagar o vínculo TB_CUPOM_CTAREC

                                                #region Apagar conta a receber (TB_CONTA_RECEBER)

                                                using (var fbCommCtarecSyncDelServ = new FbCommand())
                                                {
                                                    #region Prepara o comando para deletar conta a receber

                                                    fbCommCtarecSyncDelServ.Connection = fbConnServ;

                                                    fbCommCtarecSyncDelServ.CommandText = "DELETE FROM TB_CONTA_RECEBER WHERE ID_CTAREC = @pID_CTAREC;";
                                                    fbCommCtarecSyncDelServ.CommandType = CommandType.Text;

                                                    fbCommCtarecSyncDelServ.Parameters.Add("@pID_CTAREC", ctarecServ.ID_CTAREC);

                                                    #endregion Prepara o comando para deletar conta a receber

                                                    try
                                                    {
                                                        // Executa a sproc
                                                        fbCommCtarecSyncDelServ.ExecuteScalar();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        log.Error($"Erro ao cancelar (excluir) conta a receber no servidor (ID_CTAREC { ctarecServ.ID_CTAREC }).", ex);
                                                        throw ex;
                                                    }
                                                }
                                                #endregion Apagar conta a receber (TB_CONTA_RECEBER)
                                            }
                                            #endregion Percorre por cada conta a receber no servidor
                                        }

                                        #region Atualizar TB_SAT no servidor

                                        using (var tblSatPdv = new FDBDataSetVenda.TB_SATDataTable())
                                        using (var taSatPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_SATTableAdapter())
                                        using (var taSatServ = new DataSets.FDBDataSetVendaTableAdapters.TB_SATTableAdapter())
                                        {
                                            taSatPdv.Connection = fbConnPdv;
                                            taSatPdv.FillByIdNfvenda(tblSatPdv, nfvendaCancelPdv.ID_NFVENDA);

                                            taSatServ.Connection = fbConnServ;

                                            foreach (var itemSatPdv in tblSatPdv)
                                            {
                                                #region Atualizar item SAT equivalente no servidor

                                                using (var fbCommSatUpsertServ = new FbCommand())
                                                {
                                                    #region Prepara o comando da SP_TRI_SAT_UPSERT_BY_CHAVE

                                                    fbCommSatUpsertServ.Connection = fbConnServ;

                                                    fbCommSatUpsertServ.CommandText = "SP_TRI_SAT_UPSERT_BY_CHAVE";
                                                    fbCommSatUpsertServ.CommandType = CommandType.StoredProcedure;

                                                    //fbCommSatUpsertServ.Parameters.Add("@pID_NFVENDA", itemSatPdv.ID_NFVENDA);

                                                    fbCommSatUpsertServ.Parameters.Add("@pNF_NUMERO", nfvendaCancelPdv.NF_NUMERO);
                                                    fbCommSatUpsertServ.Parameters.Add("@pNF_SERIE", nfvendaCancelPdv.NF_SERIE);

                                                    fbCommSatUpsertServ.Parameters.Add("@pCHAVE", itemSatPdv.CHAVE); //TODO: NÃO PODE SER NULL! VAI DAR RUIM
                                                    fbCommSatUpsertServ.Parameters.Add("@pDT_EMISSAO", itemSatPdv.IsDT_EMISSAONull() ? null : (DateTime?)itemSatPdv.DT_EMISSAO);
                                                    fbCommSatUpsertServ.Parameters.Add("@pHR_EMISSAO", itemSatPdv.IsHR_EMISSAONull() ? null : (TimeSpan?)itemSatPdv.HR_EMISSAO);

                                                    fbCommSatUpsertServ.Parameters.Add("@pSTATUS", itemSatPdv.IsSTATUSNull() ? null : itemSatPdv.STATUS);
                                                    fbCommSatUpsertServ.Parameters.Add("@pSTATUS_DES", itemSatPdv.IsSTATUS_DESNull() ? null : itemSatPdv.STATUS_DES);
                                                    fbCommSatUpsertServ.Parameters.Add("@pNUMERO_CFE", itemSatPdv.IsNUMERO_CFENull() ? null : (int?)itemSatPdv.NUMERO_CFE);
                                                    fbCommSatUpsertServ.Parameters.Add("@pNUM_SERIE_SAT", itemSatPdv.IsNUM_SERIE_SATNull() ? null : itemSatPdv.NUM_SERIE_SAT);

                                                    #endregion Prepara o comando da SP_TRI_SAT_UPSERT_BY_CHAVE

                                                    // Executa a sproc
                                                    try
                                                    {
                                                        log.Debug($"pNF_NUMERO: {fbCommSatUpsertServ.Parameters[0]}");
                                                        log.Debug($"pNF_SERIE: {fbCommSatUpsertServ.Parameters[1]}");
                                                        log.Debug($"pCHAVE: {fbCommSatUpsertServ.Parameters[2]}");
                                                        log.Debug($"pDT_EMISSAO: {fbCommSatUpsertServ.Parameters[3]}");
                                                        log.Debug($"pHR_EMISSAO: {fbCommSatUpsertServ.Parameters[4]}");
                                                        log.Debug($"pSTATUS: {fbCommSatUpsertServ.Parameters[5]}");
                                                        log.Debug($"pSTATUS_DES: {fbCommSatUpsertServ.Parameters[6]}");
                                                        log.Debug($"pNUMERO_CFE: {fbCommSatUpsertServ.Parameters[7]}");
                                                        log.Debug($"pNUM_SERIE_SAT: {fbCommSatUpsertServ.Parameters[8]}");

                                                        fbCommSatUpsertServ.ExecuteScalar();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        log.Error($"pNF_NUMERO: {nfvendaCancelPdv.NF_NUMERO}, NF_SERIE: {nfvendaCancelPdv.NF_SERIE}, pCHAVE {itemSatPdv.CHAVE} (PDV)", ex);
                                                        throw ex;
                                                    }

                                                }

                                                #endregion Atualizar item SAT equivalente no servidor

                                                #region Buscar os itens cancelados SAT do item SAT atual

                                                using (var tblSatCancPdv = new FDBDataSetVenda.TB_SAT_CANCDataTable())
                                                using (var taSatCancPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_SAT_CANCTableAdapter())
                                                using (var taSatCancServ = new DataSets.FDBDataSetVendaTableAdapters.TB_SAT_CANCTableAdapter())
                                                {
                                                    taSatCancPdv.Connection = fbConnPdv;
                                                    taSatCancServ.Connection = fbConnServ;

                                                    taSatCancPdv.FillByIdRegistro(tblSatCancPdv, itemSatPdv.ID_REGISTRO);

                                                    foreach (var itemSatCancPdv in tblSatCancPdv)
                                                    {
                                                        #region Atualizar item cancelado SAT equivalente no servidor:

                                                        using (var fbCommSatCancUpsertServ = new FbCommand())
                                                        {
                                                            #region Prepara o comando da SP_TRI_SATCANC_UPSERT_BY_CHAVE

                                                            fbCommSatCancUpsertServ.Connection = fbConnServ;

                                                            fbCommSatCancUpsertServ.CommandText = "SP_TRI_SATCANC_UPSERT_BY_CHAVE";
                                                            fbCommSatCancUpsertServ.CommandType = CommandType.StoredProcedure;

                                                            fbCommSatCancUpsertServ.Parameters.Add("@pCHAVE_SAT", itemSatPdv.CHAVE); //TODO: NÃO PODE SER NULO!!
                                                            fbCommSatCancUpsertServ.Parameters.Add("@pDT_EMISSAO", itemSatCancPdv.IsDT_EMISSAONull() ? null : (DateTime?)itemSatCancPdv.DT_EMISSAO);
                                                            fbCommSatCancUpsertServ.Parameters.Add("@pHR_EMISSAO", itemSatCancPdv.IsHR_EMISSAONull() ? null : (TimeSpan?)itemSatCancPdv.HR_EMISSAO);
                                                            fbCommSatCancUpsertServ.Parameters.Add("@pNUMERO_CFE", itemSatCancPdv.IsNUMERO_CFENull() ? null : (int?)itemSatCancPdv.NUMERO_CFE);

                                                            fbCommSatCancUpsertServ.Parameters.Add("@pCHAVE", itemSatCancPdv.CHAVE); //TODO: NÃO PODE SER NULO!!
                                                            fbCommSatCancUpsertServ.Parameters.Add("@pNUM_SERIE_SAT", itemSatCancPdv.IsNUM_SERIE_SATNull() ? null : itemSatCancPdv.NUM_SERIE_SAT);
                                                            fbCommSatCancUpsertServ.Parameters.Add("@pENVIO_API", itemSatCancPdv.IsENVIO_APINull() ? null : (DateTime?)itemSatCancPdv.ENVIO_API);

                                                            #endregion Prepara o comando da SP_TRI_SATCANC_UPSERT_BY_CHAVE

                                                            // Executa a sproc
                                                            try
                                                            {
                                                                fbCommSatCancUpsertServ.ExecuteScalar();
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                log.Error($"pNF_NUMERO: {nfvendaCancelPdv.NF_NUMERO}, NF_SERIE: {nfvendaCancelPdv.NF_SERIE}, pCHAVE {itemSatPdv.CHAVE} (PDV)", ex);
                                                                throw ex;
                                                            }

                                                        }

                                                        #endregion Atualizar item cancelado SAT equivalente no servidor:
                                                    }
                                                }

                                                #endregion Buscar os itens cancelados SAT do item SAT atual
                                            }
                                        }

                                        #endregion Atualizar TB_SAT no servidor

                                        #region Atualizar no servidor a quantidade em estoque

                                        using (var taNfvItemSyncedCancelPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEMTableAdapter())
                                        {
                                            taNfvItemSyncedCancelPdv.Connection = fbConnPdv;
                                            tblNfvendaItemSyncedCancelPdv.Clear();
                                            taNfvItemSyncedCancelPdv.FillByIdNfvenda(tblNfvendaItemSyncedCancelPdv, nfvendaCancelPdv.ID_NFVENDA); // já usa sproc
                                        }

                                        // Percorrer por cada item de cupom para repor as quantidades em estoque:
                                        foreach (FDBDataSetVenda.TB_NFV_ITEMRow nfvendaItemSyncedCancel in tblNfvendaItemSyncedCancelPdv)
                                        {
                                            #region Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)

                                            #region Atualizar no servidor a quantidade em estoque

                                            using (var fbCommEstProdutoQtdServ = new FbCommand())
                                            {
                                                #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                fbCommEstProdutoQtdServ.Connection = fbConnServ;

                                                fbCommEstProdutoQtdServ.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
                                                fbCommEstProdutoQtdServ.CommandType = CommandType.StoredProcedure;

                                                fbCommEstProdutoQtdServ.Parameters.Add("@pQTD_ITEM", nfvendaItemSyncedCancel.QTD_ITEM * -1);
                                                fbCommEstProdutoQtdServ.Parameters.Add("@pID_IDENTIF", nfvendaItemSyncedCancel.ID_IDENTIFICADOR);
                                                fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPPRO", 0);// cupomItemSyncedCancel.IsID_COMPPRONull() ? 0 : cupomItemSyncedCancel.ID_COMPPRO); // AmbiMAITRE
                                                fbCommEstProdutoQtdServ.Parameters.Add("@pID_COMPOSICAO", nfvendaItemSyncedCancel.IsID_COMPOSICAONull() ? 0 : nfvendaItemSyncedCancel.ID_COMPOSICAO);

                                                #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                // Executa a sproc
                                                fbCommEstProdutoQtdServ.ExecuteScalar();
                                            }

                                            #endregion Atualizar no servidor a quantidade em estoque

                                            #region Atualizar no PDV a quantidade em estoque

                                            // Já que todo o cadastro de produtos foi copiado do Serv pro PDV na etapa anterior, 
                                            // as quantidades em estoque devem ser redefinidas
                                            using (var fbCommEstProdutoQtdPdv = new FbCommand())
                                            {
                                                #region Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                fbCommEstProdutoQtdPdv.Connection = fbConnPdv;

                                                fbCommEstProdutoQtdPdv.CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE";
                                                fbCommEstProdutoQtdPdv.CommandType = CommandType.StoredProcedure;

                                                fbCommEstProdutoQtdPdv.Parameters.Add("@pQTD_ITEM", nfvendaItemSyncedCancel.QTD_ITEM * -1);
                                                fbCommEstProdutoQtdPdv.Parameters.Add("@pID_IDENTIF", nfvendaItemSyncedCancel.ID_IDENTIFICADOR);
                                                fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPPRO", 0);// ItemSyncedCancel.IsID_COMPPRONull() ? 0 : cupomItemSyncedCancel.ID_COMPPRO); // AmbiMAITRE
                                                fbCommEstProdutoQtdPdv.Parameters.Add("@pID_COMPOSICAO", nfvendaItemSyncedCancel.IsID_COMPOSICAONull() ? 0 : nfvendaItemSyncedCancel.ID_COMPOSICAO);

                                                #endregion Prepara o comando da SP_TRI_PRODUTO_RETIRAESTOQUE

                                                // Executa a sproc
                                                fbCommEstProdutoQtdPdv.ExecuteScalar();
                                            }

                                            #endregion Atualizar no PDV a quantidade em estoque

                                            #endregion Atualizar a quantidade em estoque (E DATA DA ÚLTIMA VENDA na procedure TAMBÉM, DESDE 2018-08-01)
                                        }

                                        #endregion Atualizar no servidor a quantidade em estoque

                                        #region Indicar que a nfvenda foi synced depois de cancelado (Serv)

                                        using (var fbCommNfvendaUpdtByNumeroSerieServ = new FbCommand())
                                        {
                                            #region Prepara o comando da SP_TRI_NFV_UPDT_BYNFNUMSERIE

                                            fbCommNfvendaUpdtByNumeroSerieServ.Connection = fbConnServ;

                                            //fbCommNfvendaUpdtByCooNumcaixaServ.CommandText = "SP_TRI_CUPOM_UPDT_BYCOONUMCAIX";

                                            //TODO: o que seria um COO e NUM_CAIXA do TB_CUPOM para a TB_NFVENDA?
                                            // Seria a NF_NUMERO e a NF_SERIE

                                            fbCommNfvendaUpdtByNumeroSerieServ.CommandText = "SP_TRI_NFV_UPDT_BYNFNUMSERIE";
                                            fbCommNfvendaUpdtByNumeroSerieServ.CommandType = CommandType.StoredProcedure;

                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_NATOPE", nfvendaCancelPdv.ID_NATOPE);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_VENDEDOR", nfvendaCancelPdv.IsID_VENDEDORNull() ? null : (short?)nfvendaCancelPdv.ID_VENDEDOR);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_CLIENTE", nfvendaCancelPdv.ID_CLIENTE);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pNF_NUMERO", nfvendaCancelPdv.NF_NUMERO);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pNF_SERIE", nfvendaCancelPdv.NF_SERIE);

                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pNF_MODELO", nfvendaCancelPdv.NF_MODELO);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pDT_EMISSAO", nfvendaCancelPdv.DT_EMISSAO);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pDT_SAIDA", nfvendaCancelPdv.IsDT_SAIDANull() ? null : (DateTime?)nfvendaCancelPdv.DT_SAIDA);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pHR_SAIDA", nfvendaCancelPdv.IsHR_SAIDANull() ? null : (TimeSpan?)nfvendaCancelPdv.HR_SAIDA);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pESPECIE", nfvendaCancelPdv.IsESPECIENull() ? null : nfvendaCancelPdv.ESPECIE);

                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pTIPO_FRETE", nfvendaCancelPdv.TIPO_FRETE);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pPES_LIQUID", nfvendaCancelPdv.IsPES_LIQUIDNull() ? null : (decimal?)nfvendaCancelPdv.PES_LIQUID);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pPES_BRUTO", nfvendaCancelPdv.IsPES_BRUTONull() ? null : (decimal?)nfvendaCancelPdv.PES_BRUTO);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pSTATUS", nfvendaCancelPdv.STATUS);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pENT_SAI", nfvendaCancelPdv.ENT_SAI);

                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_FMAPGTO", nfvendaCancelPdv.ID_FMAPGTO);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pID_PARCELA", nfvendaCancelPdv.ID_PARCELA);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pMARCA", nfvendaCancelPdv.IsMARCANull() ? null : nfvendaCancelPdv.MARCA);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pQTD_VOLUM", nfvendaCancelPdv.IsQTD_VOLUMNull() ? null : (decimal?)nfvendaCancelPdv.QTD_VOLUM);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pNUM_VOLUM", nfvendaCancelPdv.IsNUM_VOLUMNull() ? null : nfvendaCancelPdv.NUM_VOLUM);

                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pPROD_REV", nfvendaCancelPdv.IsPROD_REVNull() ? null : nfvendaCancelPdv.PROD_REV);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pSOMA_FRETE", nfvendaCancelPdv.IsSOMA_FRETENull() ? null : nfvendaCancelPdv.SOMA_FRETE);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pVLR_TROCO", nfvendaCancelPdv.IsVLR_TROCONull() ? null : (decimal?)nfvendaCancelPdv.VLR_TROCO);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pIND_PRES", nfvendaCancelPdv.IsIND_PRESNull() ? null : nfvendaCancelPdv.IND_PRES);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pIND_IE_DEST", nfvendaCancelPdv.IsIND_IE_DESTNull() ? null : nfvendaCancelPdv.IND_IE_DEST);

                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pDESCONTO_CONDICIONAL", nfvendaCancelPdv.DESCONTO_CONDICIONAL);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pINF_COMP_FIXA", nfvendaCancelPdv.IsINF_COMP_FIXANull() ? null : nfvendaCancelPdv.INF_COMP_FIXA);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pINF_COMP_EDIT", nfvendaCancelPdv.IsINF_COMP_EDITNull() ? null : nfvendaCancelPdv.INF_COMP_EDIT);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pENDERECO_ENTREGA", nfvendaCancelPdv.ENDERECO_ENTREGA);
                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pENVIO_API", nfvendaCancelPdv.IsENVIO_APINull() ? null : (DateTime?)nfvendaCancelPdv.ENVIO_API);

                                            fbCommNfvendaUpdtByNumeroSerieServ.Parameters.Add("@pSYNCED", 2);

                                            #endregion Prepara o comando da SP_TRI_NFV_UPDT_BYNFNUMSERIE

                                            // Executa a sproc
                                            fbCommNfvendaUpdtByNumeroSerieServ.ExecuteScalar();
                                        }

                                        #endregion Indicar que a nfvenda foi synced depois de cancelado (Serv)

                                        #region Indicar que os itens da nfvenda foram cancelados (Serv) -- DESATIVADO

                                        ////TODO: parece que esse procedimento não é necessário em TB_NFV_ITEM

                                        //using (var fbCommNfvendaItemSetCancelByNumeroSerieServ = new FbCommand())
                                        //{
                                        //    #region Prepara o comando da SP_TRI_CUPOM_ITEM_SET_CANCEL

                                        //    fbCommNfvendaItemSetCancelByNumeroSerieServ.Connection = fbConnServ;

                                        //    fbCommNfvendaItemSetCancelByNumeroSerieServ.CommandText = "SP_TRI_CUPOM_ITEM_SET_CANCEL";
                                        //    fbCommNfvendaItemSetCancelByNumeroSerieServ.CommandType = CommandType.StoredProcedure;

                                        //    fbCommNfvendaItemSetCancelByNumeroSerieServ.Parameters.Add("@pCOO", cupomCancelPdv.COO);
                                        //    fbCommNfvendaItemSetCancelByNumeroSerieServ.Parameters.Add("@pNUM_CAIXA", cupomCancelPdv.NUM_CAIXA);

                                        //    #endregion Prepara o comando da SP_TRI_CUPOM_ITEM_SET_CANCEL

                                        //    // Executa a sproc
                                        //    fbCommNfvendaItemSetCancelByNumeroSerieServ.ExecuteScalar();
                                        //}

                                        #endregion Indicar que os itens da nfvenda foram cancelados (Serv) -- DESATIVADO

                                        #region Indicar que a nfvenda foi synced depois de cancelado (PDV)

                                        using (var fbCommNfvendaUnsyncedSetSynced = new FbCommand())
                                        {
                                            #region Prepara o comando da SP_TRI_NFVENDA_SETSYNCED

                                            fbCommNfvendaUnsyncedSetSynced.Connection = fbConnPdv;

                                            fbCommNfvendaUnsyncedSetSynced.CommandText = "SP_TRI_NFVENDA_SETSYNCED";
                                            fbCommNfvendaUnsyncedSetSynced.CommandType = CommandType.StoredProcedure;

                                            fbCommNfvendaUnsyncedSetSynced.Parameters.Add("@pIdNfvenda", nfvendaCancelPdv.ID_NFVENDA);
                                            fbCommNfvendaUnsyncedSetSynced.Parameters.Add("@pSynced", 2);

                                            #endregion Prepara o comando da SP_TRI_NFVENDA_SETSYNCED

                                            // Executa a sproc
                                            fbCommNfvendaUnsyncedSetSynced.ExecuteScalar();
                                        }
                                        //}
                                        #endregion Indicar que a nfvenda foi synced depois de cancelado (PDV)

                                        #region Desfazer vínculo de nfvenda com orçamento e setar status do orçamento para "SALVO"

                                        #region Verificar se a nfvenda foi cancelada: reativar o orçamento vinculado, se houver

                                        //TODO: adaptar vínculo do orçamento com NFVENDA

                                        //using (var taOrcaServ = new DataSets.FDBDataSetOrcamTableAdapters.TRI_PDV_ORCA_NFVENDA_RELTableAdapter())
                                        //{
                                        //    taOrcaServ.Connection = fbConnServ;

                                        //    audit("SINCCONTNETDB>> ", string.Format("(nfvenda cancelada depois de synced) taOrcaServ.SP_TRI_ORCA_REATIVAORCA({1}): {0}",
                                        //                        taOrcaServ.SP_TRI_ORCA_REATIVAORCA(nfvendaCancelPdv.ID_NFVENDA).Safestring(),
                                        //                        nfvendaCancelPdv.ID_NFVENDA.Safestring()));
                                        //}

                                        #endregion Verificar se a nfvenda foi cancelada: reativar o orçamento vinculado, se houver

                                        #endregion Desfazer vínculo de nfvenda com orçamento e setar status do orçamento para "SALVO"

                                        // Teste de transação:
                                        //int minibomba = 0;
                                        //decimal bomba = 100 / minibomba;

                                        //fbConnServ.Close();
                                        //fbConnPdv.Close();
                                    }
                                    // Finaliza a transação:
                                    transactionScopeNfvendaResync.Complete();
                                }
                                #endregion Iniciar o procedimento de cancelamento na retaguarda
                            }

                            log.Debug(string.Format("Lote {0} de nfvendas sincronizadas e canceladas processado!", intCountLoteNfvendaSyncedCancel.ToString()));

                            // Busca todas as nfvendas que foram synced e posteriormente canceladas (TIP_QUERY = 1)
                            // Lembrando que a sproc executada abaixo retorna até 200 registros por vez (lote).
                            tblNfvendaSyncedCancelPdv.Clear();
                            log.Error("taNfvendaSyncedCancelPdv.FillByNfvendaSync(1): " + taNfvendaSyncedCancelPdv.FillByNfvendaSync(tblNfvendaSyncedCancelPdv, 1).ToString()); // já usa sproc

                            #region NOPE - Não haverá fix Clipp rules
                            //taEstProdutoServ.SP_TRI_FIX_CLIPP_RULES();
                            //taEstProdutoPdv.SP_TRI_FIX_CLIPP_RULES();
                            #endregion NOPE - Não haverá fix Clipp rules
                        }
                    }
                    #region Manipular Exception
                    catch (Exception ex)
                    {
                        log.Error("Erro ao sincronizar (synced e cancelado depois):", ex);
                        GravarErroSync("Erro ao sincronizar", tblNfvendaSyncedCancelPdv, ex);
                        throw ex;
                    }
                    #endregion Manipular Exception
                    #region Limpeza dos objetos Disposable
                    finally
                    {
                        if (taNfvendaSyncedCancelPdv != null) { taNfvendaSyncedCancelPdv.Dispose(); }
                        if (tblNfvendaSyncedCancelPdv != null) { tblNfvendaSyncedCancelPdv.Dispose(); }
                        if (taEstProdutoServ != null) { taEstProdutoServ.Dispose(); }
                        if (taEstProdutoPdv != null) { taEstProdutoPdv.Dispose(); }
                    }
                    #endregion Limpeza dos objetos Disposable
                }
                #endregion Nfvendas sincronizadas e posteriormente canceladas
            }
            */
            #endregion Cupons (NFVenda)

            #endregion Vendas

            #region Fechamentos de caixa
            Sync_TB_MOVTODIARIO(tipoSync);
            #region TB_MOVTODIARIO
            /*
            if (tipoSync == EnmTipoSync.fechamentos || tipoSync == EnmTipoSync.tudo)
            {
                #region Abre transação (fech. caixa)

                var taMovtoUnsynced = new TB_MOVDIARIOTableAdapter();
                taMovtoUnsynced.Connection.ConnectionString = _strConnContingency;
                var tblMovtoUnsynced = new FDBDataSet.TB_MOVDIARIODataTable();

                var taMovtoServ = new TB_MOVDIARIOTableAdapter();
                taMovtoServ.Connection.ConnectionString = _strConnNetwork;

                int intCounLoteFechCaixa = 0;

                #endregion Abre transação (fech. caixa)

                try
                {
                    log.Debug("taMovtoUnsynced.FillByMovtoUnsynced(): " + taMovtoUnsynced.FillByMovtoUnsynced(tblMovtoUnsynced).ToString()); // já usa sproc

                    // Executa enquanto houver movimentos pra sincronizar:
                    while (tblMovtoUnsynced != null && tblMovtoUnsynced.Rows.Count > 0)
                    {
                        intCounLoteFechCaixa++;

                        #region Percorre por cada fechamento de caixa não sincronizado

                        foreach (FDBDataSet.TB_MOVDIARIORow movtoUnsynced in tblMovtoUnsynced)
                        {
                            using (var transactionScopeFechCaixa = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                            {
                                movtoUnsynced.SYNCED = 1;
                                taMovtoServ.SP_TRI_MOVTO_SYNC_INSERT((movtoUnsynced.IsDT_MOVTONull() ? null : (DateTime?)movtoUnsynced.DT_MOVTO),
                                                                     (movtoUnsynced.IsHR_MOVTONull() ? null : (TimeSpan?)movtoUnsynced.HR_MOVTO),
                                                                     (movtoUnsynced.IsHISTORICONull() ? null : movtoUnsynced.HISTORICO),
                                                                     (movtoUnsynced.IsTIP_MOVTONull() ? null : movtoUnsynced.TIP_MOVTO),
                                                                     (movtoUnsynced.IsVLR_MOVTONull() ? null : (decimal?)movtoUnsynced.VLR_MOVTO),
                                                                     (movtoUnsynced.IsID_CTAPLANull() ? null : (short?)movtoUnsynced.ID_CTAPLA),
                                                                     movtoUnsynced.SYNCED);

                                #region Indicar que o fechamento de caixa foi sincronizado

                                taMovtoUnsynced.SP_TRI_MOVTOSETSYNCED(movtoUnsynced.ID_MOVTO, movtoUnsynced.SYNCED);

                                #endregion Indicar que o fechamento de caixa foi sincronizado

                                // Finaliza a transação:
                                transactionScopeFechCaixa.Complete();
                            }
                        }

                        log.Debug(string.Format("Lote {0} de fechamentos processado!", intCounLoteFechCaixa.ToString()));

                        #endregion Percorre por cada fechamento de caixa não sincronizado

                        #region Prepara o próximo lote

                        tblMovtoUnsynced.Clear();
                        log.Debug("taMovtoUnsynced.FillByMovtoUnsynced(): " + taMovtoUnsynced.FillByMovtoUnsynced(tblMovtoUnsynced).ToString()); // já usa sproc

                        #endregion Prepara o próximo lote
                    }
                }
                #region Manipular Exception
                catch (Exception ex)
                {
                    log.Error("Erro ao sincronizar fechamentos de caixa",ex);
                    GravarErroSync("", tblMovtoUnsynced, ex);
                    throw ex;
                }
                #endregion Manipular Exception
                #region Limpeza dos objetos Disposable
                finally
                {
                    if (taMovtoUnsynced != null) { taMovtoUnsynced.Dispose(); }
                    if (tblMovtoUnsynced != null) { tblMovtoUnsynced.Dispose(); }
                    if (taMovtoServ != null) { taMovtoServ.Dispose(); }
                }
                #endregion Limpeza dos objetos Disposable
            }
            */
            #endregion TB_MOVTODIARIO
            Sync_TRI_PDV_FECHAMENTOS(tipoSync);
            #region TRI_PDV_FECHAMENTOS

            if (tipoSync == EnmTipoSync.fechamentos || tipoSync == EnmTipoSync.tudo)
            {
                #region Abre transação (fech. caixa)

                var taFechamentosUnsynced = new TRI_PDV_FECHAMENTOSTableAdapter();
                taFechamentosUnsynced.Connection.ConnectionString = _strConnContingency;
                var tblFechamentosUnsynced = new FDBDataSet.TRI_PDV_FECHAMENTOSDataTable();

                var taFechamentosServ = new TRI_PDV_FECHAMENTOSTableAdapter();
                taFechamentosServ.Connection.ConnectionString = _strConnNetwork;

                int intCounLoteFechCaixa = 0;

                #endregion Abre transação (fech. caixa)

                try
                {
                    log.Debug("taFechamentosUnsynced.FillByMovtoUnsynced(): " + taFechamentosUnsynced.FillByMovtoUnsynced(tblFechamentosUnsynced).ToString()); // já usa sproc

                    // Executa enquanto houver movimentos pra sincronizar:
                    while (tblFechamentosUnsynced != null && tblFechamentosUnsynced.Rows.Count > 0)
                    {
                        intCounLoteFechCaixa++;

                        #region Percorre por cada fechamento de caixa não sincronizado

                        foreach (FDBDataSet.TRI_PDV_FECHAMENTOSRow fechamentosUnsynced in tblFechamentosUnsynced)
                        {
                            using (var transactionScopeFechCaixa = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                            {
                                fechamentosUnsynced.SYNCED = 1;
                                taFechamentosServ.SP_TRI_FECH_SYNC_INSERT((fechamentosUnsynced.IsDINNull() ? null : (decimal?)fechamentosUnsynced.DIN),
                                                                          (fechamentosUnsynced.IsCHEQUENull() ? null : (decimal?)fechamentosUnsynced.CHEQUE),
                                                                          (fechamentosUnsynced.IsCREDITONull() ? null : (decimal?)fechamentosUnsynced.CREDITO),
                                                                          (fechamentosUnsynced.IsDEBITONull() ? null : (decimal?)fechamentosUnsynced.DEBITO),
                                                                          (fechamentosUnsynced.IsLOJANull() ? null : (decimal?)fechamentosUnsynced.LOJA),
                                                                          (fechamentosUnsynced.IsALIMENTACAONull() ? null : (decimal?)fechamentosUnsynced.ALIMENTACAO),
                                                                          (fechamentosUnsynced.IsREFEICAONull() ? null : (decimal?)fechamentosUnsynced.REFEICAO),
                                                                          (fechamentosUnsynced.IsPRESENTENull() ? null : (decimal?)fechamentosUnsynced.PRESENTE),
                                                                          (fechamentosUnsynced.IsCOMBUSTIVELNull() ? null : (decimal?)fechamentosUnsynced.COMBUSTIVEL),
                                                                          (fechamentosUnsynced.IsOUTROSNull() ? null : (decimal?)fechamentosUnsynced.OUTROS),
                                                                          (fechamentosUnsynced.IsEXTRA_1Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_1),
                                                                          (fechamentosUnsynced.IsEXTRA_2Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_2),
                                                                          (fechamentosUnsynced.IsEXTRA_3Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_3),
                                                                          (fechamentosUnsynced.IsEXTRA_4Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_4),
                                                                          (fechamentosUnsynced.IsEXTRA_5Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_5),
                                                                          (fechamentosUnsynced.IsEXTRA_6Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_6),
                                                                          (fechamentosUnsynced.IsEXTRA_7Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_7),
                                                                          (fechamentosUnsynced.IsEXTRA_8Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_8),
                                                                          (fechamentosUnsynced.IsEXTRA_9Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_9),
                                                                          (fechamentosUnsynced.IsEXTRA_10Null() ? null : (decimal?)fechamentosUnsynced.EXTRA_10),
                                                                          fechamentosUnsynced.OPERADOR,
                                                                          fechamentosUnsynced.ID_CAIXA,
                                                                          fechamentosUnsynced.FECHADO,
                                                                          (fechamentosUnsynced.IsSANGRIASNull() ? null : (decimal?)fechamentosUnsynced.SANGRIAS),
                                                                          (fechamentosUnsynced.IsSUPRIMENTOSNull() ? null : (decimal?)fechamentosUnsynced.SUPRIMENTOS),
                                                                          (fechamentosUnsynced.IsTROCASNull() ? null : (decimal?)fechamentosUnsynced.TROCAS),
                                                                          fechamentosUnsynced.SYNCED);

                                #region Indicar que o fechamento de caixa foi sincronizado

                                taFechamentosUnsynced.SP_TRI_FECH_SETSYNCED(fechamentosUnsynced.ID_CAIXA, fechamentosUnsynced.FECHADO, 1);

                                #endregion Indicar que o fechamento de caixa foi sincronizado

                                // Finaliza a transação:
                                transactionScopeFechCaixa.Complete();
                            }
                        }

                        log.Debug(string.Format("Lote {0} de fechamentos processado!", intCounLoteFechCaixa.ToString()));

                        #endregion Percorre por cada fechamento de caixa não sincronizado

                        #region Prepara o próximo lote

                        tblFechamentosUnsynced.Clear();
                        log.Debug("taFechamentosUnsynced.FillByMovtoUnsynced(): " + taFechamentosUnsynced.FillByMovtoUnsynced(tblFechamentosUnsynced).ToString()); // já usa sproc

                        #endregion Prepara o próximo lote
                    }
                }
                #region Manipular Exception
                catch (Exception ex)
                {
                    log.Error("Erro ao sincronizar fechamentos de caixa:", ex);
                    GravarErroSync("", tblFechamentosUnsynced, ex);
                    throw ex;
                }
                #endregion Manipular Exception
                #region Limpeza dos objetos Disposable
                finally
                {
                    taFechamentosUnsynced?.Dispose();
                    tblFechamentosUnsynced?.Dispose();
                    taFechamentosServ?.Dispose();
                }
                #endregion Limpeza dos objetos Disposable
            }

            #endregion TRI_PDV_FECHAMENTOS

            #endregion Fechamentos de caixa

            #region Trocas (novamente, sentido contrário)

            #region Trocas -- DESATIVADO DESDE 1.4.5.17
            //if (tipoSync == EnmTipoSync.vendas || tipoSync == EnmTipoSync.tudo)
            //{
            //    // Busca trocas no PDV, grava no Serv:
            //    SyncTrocas(EnmDBSync.serv, "N"); //HACK: TERMINAR ESSA Update: Tá funcionando, não mexe.
            //    SyncTrocas(EnmDBSync.serv, "S"); //HACK: TERMINAR ESSA Update: Tá funcionando, não mexe.
            //}
            #endregion Trocas

            #endregion Trocas (novamente, sentido contrário)

            #endregion PDV -> Serv

            #region Gravar o timestamp da última sync

            //TODO: talvez a tolerância deva ser aplicada na consulta, e não na gravação...
            SetSetupUltimaSync(EnmDBSync.pdv, segundosTolerancia); //TODO: validar essa setting, já que está exposto no app.config e podem alterar como quiserem.

            #endregion Gravar o timestamp da última sync

            #region Contagem de tempo de execução do método

            dtSyncFim = DateTime.Now;

            var tsSyncDiff = dtSyncFim - dtSyncInicio;

            switch (tipoSync)
            {
                case EnmTipoSync.cadastros:
                    strSyncTipo = "cadastros";
                    break;
                case EnmTipoSync.vendas:
                    strSyncTipo = "vendas";
                    break;
                case EnmTipoSync.fechamentos:
                    strSyncTipo = "fechamentos";
                    break;
                case EnmTipoSync.tudo:
                    strSyncTipo = "tudo";
                    break;
                case EnmTipoSync.exCadastrosInexistentes:
                    strSyncTipo = "exclusão de cadastros inexistentes";
                    break;
                case EnmTipoSync.CtrlS:
                    strSyncTipo = "Ctrl + S";
                    break;
                case EnmTipoSync.dummy:
                    strSyncTipo = "dummy";
                    break;
                default:
                    strSyncTipo = "deu chabu";
                    break;
            }

            log.Debug($"Tempo total: {tsSyncDiff.TotalMilliseconds} ms ; Operação: {strSyncTipo}");

            #endregion Contagem de tempo de execução do método

            return retornoProdutosAlterados;
        }

        /// <summary>
        /// Remove a entrada na tabela auxiliar de sincronização.
        /// Informar -1 (tipos numéricos) ou null (nullables) para os parâmetros não utilizados.
        /// </summary>
        /// <param name="iD_REG"></param>
        /// <param name="tABELA"></param>
        /// <param name="oPERACAO"></param>
        /// <param name="nO_CAIXA"></param>
        /// <param name="uN_REG"></param>
        /// <param name="sM_REG"></param>
        private void ConfirmarAuxSync(int iD_REG, string tABELA, string oPERACAO, short nO_CAIXA, string uN_REG = null, short sM_REG = -1, string cH_REG = null)
        {
            try
            {
                using (var fbConnServ = new FbConnection(_strConnNetwork))
                {
                    fbConnServ.Open();

                    using (var fbCommAuxSyncEstoque = new FbCommand())
                    {
                        fbCommAuxSyncEstoque.CommandType = CommandType.Text;
                        fbCommAuxSyncEstoque.Connection = fbConnServ;

                        // Apagar as entradas para insert e update também se a operação for delete:
                        if (!string.IsNullOrWhiteSpace(uN_REG))
                        {
                            fbCommAuxSyncEstoque.CommandText = oPERACAO.Equals("D") ?
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE uN_REG = @unReg AND TABELA = @tabela AND NO_CAIXA = @noCaixa;" :
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE uN_REG = @unReg AND TABELA = @tabela AND OPERACAO = @operacao AND NO_CAIXA = @noCaixa;";

                            fbCommAuxSyncEstoque.Parameters.Add("@unReg", uN_REG);
                        }
                        else if (iD_REG >= 0 && sM_REG >= 0)
                        {
                            fbCommAuxSyncEstoque.CommandText = oPERACAO.Equals("D") ?
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE ID_REG = @idReg AND SM_REG = @smReg AND TABELA = @tabela AND NO_CAIXA = @noCaixa;" :
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE ID_REG = @idReg AND SM_REG = @smReg AND TABELA = @tabela AND OPERACAO = @operacao AND NO_CAIXA = @noCaixa;";

                            fbCommAuxSyncEstoque.Parameters.Add("@idReg", iD_REG);
                            fbCommAuxSyncEstoque.Parameters.Add("@smReg", sM_REG);
                        }
                        else if (iD_REG < 0 && sM_REG >= 0)
                        {
                            fbCommAuxSyncEstoque.CommandText = oPERACAO.Equals("D") ?
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE SM_REG = @smReg AND TABELA = @tabela AND NO_CAIXA = @noCaixa;" :
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE SM_REG = @smReg AND TABELA = @tabela AND OPERACAO = @operacao AND NO_CAIXA = @noCaixa;";

                            fbCommAuxSyncEstoque.Parameters.Add("@smReg", sM_REG);
                        }
                        else if (iD_REG < 0 && string.IsNullOrWhiteSpace(uN_REG) && sM_REG < 0 && !string.IsNullOrWhiteSpace(cH_REG))
                        {
                            fbCommAuxSyncEstoque.CommandText = oPERACAO.Equals("D") ?
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE CH_REG = @chReg AND TABELA = @tabela AND NO_CAIXA = @noCaixa;" :
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE CH_REG = @chReg AND TABELA = @tabela AND OPERACAO = @operacao AND NO_CAIXA = @noCaixa;";

                            fbCommAuxSyncEstoque.Parameters.Add("@chReg", cH_REG);
                        }
                        else
                        {
                            fbCommAuxSyncEstoque.CommandText = oPERACAO.Equals("D") ?
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE ID_REG = @idReg AND TABELA = @tabela AND NO_CAIXA = @noCaixa;" :
                                                                "DELETE FROM TRI_PDV_AUX_SYNC WHERE ID_REG = @idReg AND TABELA = @tabela AND OPERACAO = @operacao AND NO_CAIXA = @noCaixa;";

                            fbCommAuxSyncEstoque.Parameters.Add("@idReg", iD_REG);
                        }

                        fbCommAuxSyncEstoque.Parameters.Add("@tabela", tABELA);
                        fbCommAuxSyncEstoque.Parameters.Add("@operacao", oPERACAO);
                        fbCommAuxSyncEstoque.Parameters.Add("@noCaixa", nO_CAIXA);

                        fbCommAuxSyncEstoque.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Erro ao confirmar auxílio de sincronização: \niD_REG: {iD_REG} \ntABELA: {tABELA} \noPERACAO: {oPERACAO} \nnO_CAIXA: {nO_CAIXA} \nuN_REG: {uN_REG.Safestring()} \nsM_REG: {sM_REG} \ncH_REG: {cH_REG.Safestring()} \nMais detalhes: ", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Controla a sync de trocas entre servidor e PDV.
        /// Todos os PDVs devem estar cientes de todas as trocas pendentes.
        /// Trocas que já foram resgatadas devem estar marcadas tanto no servidor quanto nos PDVs e
        /// não devem mais ser processados (DONT_SYNC).
        /// </summary>
        /// <param name="pBdParaGravar">Indicação do banco a ser gravado</param>
        /// <param name="pFillByRedeemed">Indicação do status da troca a ser synced (S/N)</param>
        private void SyncTrocas(EnmDBSync pBdParaGravar, string pFillByRedeemed)
        {
            using (var tblTroca = new FDBDataSet.TRI_PDV_TROCASDataTable())
                try
                {
                    using (var taTrocaConsulta = new TRI_PDV_TROCASTableAdapter())

                    using (var fbConnServ = new FbConnection(_strConnNetwork))
                    using (var fbConnPdv = new FbConnection(_strConnContingency))
                    {
                        fbConnPdv.Open();
                        fbConnServ.Open();

                        switch (pBdParaGravar)
                        {
                            case EnmDBSync.pdv:
                                // Foi indicado gravação para o PDV...
                                // ... então a consulta será no serv:
                                taTrocaConsulta.Connection = fbConnServ;
                                break;
                            case EnmDBSync.serv:
                                // Foi indicado gravação para o serv...
                                // ... então a consulta será no PDV:
                                taTrocaConsulta.Connection = fbConnPdv;
                                break;
                            default:
                                throw new NotImplementedException("Tipo de banco de dados não esperado!");
                        }

                        log.Debug("taTrocaConsulta.FillByRedeemed(): " + taTrocaConsulta.FillByRedeemed(tblTroca, pFillByRedeemed, _intNoCaixa).ToString()); // já usa sproc

                        foreach (FDBDataSet.TRI_PDV_TROCASRow troca in tblTroca)
                        {
                            try
                            {
                                using (var taTrocaGravacao = new TRI_PDV_TROCASTableAdapter())
                                using (var tsEscopo = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted })) //TODO: verificar se esse tipo de IsolationLevel é o mais indicado para essa situação
                                {
                                    // Verificar se a troca está redeemed = 'S' nos dois bancos.
                                    // Neste caso, gravar dont_sync = '1' em ambos.

                                    switch (pBdParaGravar)
                                    {
                                        case EnmDBSync.pdv:
                                            // Foi indicado gravação para o PDV:
                                            taTrocaGravacao.Connection = fbConnPdv;
                                            break;
                                        case EnmDBSync.serv:
                                            // Foi indicado gravação para o serv:
                                            taTrocaGravacao.Connection = fbConnServ;
                                            break;
                                    }

                                    // Retorna redeemed, pois algum outro caixa pode já ter recuperado esse cupom.
                                    string strRedeemedRetorno = taTrocaGravacao.SP_TRI_TROCASYNC(null,
                                                                                        troca.REDEEMED,
                                                                                        troca.COO,
                                                                                        troca.NUM_CAIXA,
                                                                                        DateTime.Now,
                                                                                        troca.VLR_CUPOM).Safestring();

                                    if ((troca.REDEEMED == strRedeemedRetorno) && strRedeemedRetorno == "S")
                                    {
                                        // A indicação de redeemed no bd de consulta é diferente da indicação está no bd de gravação.
                                        // Será necessário atualizar essa indicação no banco de consulta (cupom já foi resgatado).

                                        // Verificar se a mesma troca está redeemed em ambos os bancos, e então setar DONT_SYNC.

                                        using (var taTrocaGravacaoSetRedeemed = new TRI_PDV_TROCASTableAdapter())
                                        {
                                            switch (pBdParaGravar)
                                            {
                                                case EnmDBSync.pdv:
                                                    // A operação de gravação inicial era para ser no PDV,
                                                    // mas o status deve ser atualizado agora no serv.
                                                    taTrocaGravacaoSetRedeemed.Connection = fbConnServ;
                                                    break;
                                                case EnmDBSync.serv:
                                                    // A operação de gravação inicial era para ser no serv,
                                                    // mas o status deve ser atualizado agora no PDV.
                                                    taTrocaGravacaoSetRedeemed.Connection = fbConnPdv;
                                                    break;
                                            }

                                            taTrocaGravacaoSetRedeemed.SP_TRI_TROCASYNC(null,
                                                                                "S",
                                                                                troca.COO,
                                                                                troca.NUM_CAIXA,
                                                                                DateTime.Now,
                                                                                troca.VLR_CUPOM);
                                        }

                                        // Setar DONT_SYNC nos dois BDs, para não processar mais essa troca:
                                        using (var taTrocaAuxServ = new TRI_PDV_TROCAS_AUXTableAdapter())
                                        {
                                            taTrocaAuxServ.Connection = fbConnServ;

                                            log.Debug("taTrocaAuxServ.SP_TRI_TROCAAUX_UPSERT(): " + taTrocaAuxServ.SP_TRI_TROCAAUX_UPSERT(troca.COO, troca.NUM_CAIXA, _intNoCaixa, "1").ToString());
                                        }
                                        using (var taTrocaAuxPdv = new TRI_PDV_TROCAS_AUXTableAdapter())
                                        {
                                            taTrocaAuxPdv.Connection = fbConnPdv;

                                            log.Debug("taTrocaAuxServ.SP_TRI_TROCAAUX_UPSERT(): " + taTrocaAuxPdv.SP_TRI_TROCAAUX_UPSERT(troca.COO, troca.NUM_CAIXA, _intNoCaixa, "1").ToString());
                                        }
                                    }
                                    log.Debug("COMPLETE!");
                                    tsEscopo.Complete();
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao sincronizar (trocas, PDV -> Serv): " +
                                                   " / VLR_CUPOM = " + troca.VLR_CUPOM.ToString() + " / REDEEMED = " + troca.REDEEMED.ToString() +
                                                   " / COO = " + troca.COO.ToString() + " / NUM_CAIXA = " + troca.NUM_CAIXA.ToString() +
                                                   " / NUM_CAIXA_SYNC = " + _intNoCaixa.ToString(), ex);
                                throw ex;
                            }
                        }
                        //fbConnPdv.Close(); // dispose já executa o .Close()
                        //fbConnServ.Close();
                    }
                }
                #region Manipular Exception
                catch (Exception ex)
                {
                    log.Error("Erro ao sincronizar (trocas)", ex);
                    GravarErroSync("", tblTroca, ex);
                    throw ex;
                }
            #endregion Manipular Exception
        }

        private void GravarErroSync(string secao, DataTable dataTable, Exception ex)
        {
            if (dataTable.Rows.Count > 0)
            {
                string erro = RetornarMensagemErro(ex, true);
                try
                {
                    log.Error("Erro ao sincronizar " + secao + ":\nRegistro " + dataTable.GetErrors()?[0]?[0] + " retornou um erro: " + dataTable.GetErrors()[0].RowError + erro, ex);
                }
                catch (IndexOutOfRangeException)
                {
                    log.Error(erro);
                }

            }
            else
            {
                log.Error("Erro ao gravar erro sync", ex);
            }
        }

        #endregion Methods
    }
}
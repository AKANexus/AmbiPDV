using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using MySql.Data.MySqlClient;
using PDV_WPF.DataSets.FDBDataSetOrcamTableAdapters;
using PDV_WPF.DataSets.FDBDataSetVendaTableAdapters;
using PDV_WPF.FDBDataSetTableAdapters;
using PDV_WPF.Funcoes;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF
{
    public class funcoesClass
    {
        Logger log = new Logger("Funções Públicas");
        #region métodos
        //public decimal CalculaValorEmCaixa(DateTime abertura, DateTime fechamento)
        //{
        //    decimal _valor = 0;

        //    using (var SVF_TA = new SomaValoresFmapagtoTableAdapter())
        //    using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
        //    {
        //        SVF_TA.Connection = LOCAL_FB_CONN;
        //        decimal totalRecebidoEmDinheiro = ((decimal?)SVF_TA.SomaDeValores(abertura, 1, NO_CAIXA.ToString(), fechamento) ?? 0M) +
        //        ((decimal?)SVF_TA.SomaDeValores(abertura, 1, "N" + NO_CAIXA.ToString(), fechamento) ?? 0M) +
        //        ((decimal?)SVF_TA.SomaDeValores(abertura, 1, "E" + NO_CAIXA.ToString(), fechamento) ?? 0M);

        //        //decimal totalDadoEmTroco = (SVF_TA.SomaDeTrocos(abertura, 1, NO_CAIXA.ToString(), fechamento) ?? 0M +
        //        //SVF_TA.SomaDeTrocos(abertura, 1, "N" + NO_CAIXA.ToString(), fechamento) ?? 0M +
        //        //SVF_TA.SomaDeTrocos(abertura, 1, "E" + NO_CAIXA.ToString(), fechamento) ?? 0M);

        //        decimal suprimentos = SVF_TA.GetSuprimentosByCaixa(abertura, NO_CAIXA) ?? 0M;
        //        decimal sangrias = SVF_TA.GetSangriasByCaixa(abertura, NO_CAIXA) ?? 0M;

        //        _valor = totalRecebidoEmDinheiro /*- totalDadoEmTroco*/ + suprimentos - sangrias;
        //    }
        //    return _valor;
        //}
        public decimal CalculaValorEmCaixa(int ID_CAIXA)
        {
            decimal _valor = 0;

            using (var SVF_TA = new SomaValoresFmapagtoTableAdapter())
            using (var PDV_OperTA = new TRI_PDV_OPERTableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {

                SVF_TA.Connection = PDV_OperTA.Connection = LOCAL_FB_CONN;
                DateTime abertura = (DateTime)PDV_OperTA.GetByCaixaAberto(NO_CAIXA)[0]["CURRENTTIME"];
                decimal vendasFiscais = ((decimal?)SVF_TA.SomaDeValores(abertura, 1, NO_CAIXA.ToString(), DateTime.Now) ?? 0M);
                decimal vendasNaoFiscais = ((decimal?)SVF_TA.SomaDeValores(abertura, 1, "N" + NO_CAIXA.ToString(), DateTime.Now) ?? 0M);
                decimal vendasECF = ((decimal?)SVF_TA.SomaDeValores(abertura, 1, "E" + NO_CAIXA.ToString(), DateTime.Now) ?? 0M);

                decimal totalRecebidoEmDinheiro = (vendasFiscais + vendasNaoFiscais + vendasECF);

                log.Debug($"totalRecebidoEmDinheiro (Fiscais + NF + ECF): ({vendasFiscais} + {vendasNaoFiscais} + {vendasECF}) = {totalRecebidoEmDinheiro}");

                //decimal totalDadoEmTroco = (SVF_TA.SomaDeTrocos(abertura, 1, NO_CAIXA.ToString(), fechamento) ?? 0M +
                //SVF_TA.SomaDeTrocos(abertura, 1, "N" + NO_CAIXA.ToString(), fechamento) ?? 0M +
                //SVF_TA.SomaDeTrocos(abertura, 1, "E" + NO_CAIXA.ToString(), fechamento) ?? 0M);

                decimal suprimentos = (decimal?)SVF_TA.GetSuprimentosByCaixa(abertura, NO_CAIXA, DateTime.Now) ?? 0M;
                decimal sangrias = (decimal?)SVF_TA.GetSangriasByCaixa(abertura, NO_CAIXA, DateTime.Now) ?? 0M;

                log.Debug($"suprimentos: {suprimentos}");
                log.Debug($"sangrias: {sangrias}");

                _valor = totalRecebidoEmDinheiro /*- totalDadoEmTroco*/ + suprimentos - sangrias;
            }
            return _valor;
        }

        //public DataSets.FDBDataSetOperSeed.TB_CLIENTEDataTable dt_cli = new DataSets.FDBDataSetOperSeed.TB_CLIENTEDataTable();
        public string ObtemIDAnyDesk()
        {
            using Process proc = new Process();
            if (!File.Exists(@"C:\Program Files (x86)\AnyDesk\AnyDesk.exe"))
            {
                MessageBox.Show("AnyDesk não está instalado, ou não foi instalado na pasta padrão.");
                return "ID Inválida ou Inexistente";
            }

            ProcessStartInfo start_service = new ProcessStartInfo
            {
                Arguments = "--start-service",
                FileName = @"C:\Program Files (x86)\AnyDesk\AnyDesk.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            ProcessStartInfo get_id = new ProcessStartInfo
            {
                Arguments = "--get-id",
                FileName = @"C:\Program Files (x86)\AnyDesk\AnyDesk.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            proc.StartInfo = start_service;
            proc.Start();
            proc.StartInfo = get_id;
            proc.Start();
            string resultado = proc.StandardOutput.ReadToEnd();
            return resultado;
        }

        /// <summary>
        /// Testa a conectividade com o servidor em rede através de uma conexão Socket/TCP. True caso a conexão seja bem feita, senão false.
        /// </summary>
        /// <param name="ipaddress">Nome ou IP do servidor a ser testado.</param>
        /// <param name="timetout">Timeout para a conexão.</param>
        /// <returns>Verdadeiro se houve uma conexão</returns>
        public bool TestaConexaoComServidor(string ipaddress, string catalog, int timeout = 1000)
        {
            var conexao = false;
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    IAsyncResult asyncResult = socket.BeginConnect(ipaddress, 3050, null, null);
                    conexao = asyncResult.AsyncWaitHandle.WaitOne(timeout, true);
                    socket.Close();
                }
                //using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                //{
                //    socket.Connect(ipaddress, 3050);
                //    socket.Close();
                //}
            }
            catch (Exception)
            {
                return false;
                //throw;
            }
            if (conexao)
            {
                try
                {
                    using var FBConn1 = new FbConnection(MontaStringDeConexao(ipaddress, catalog));
                    FBConn1.Open();
                    using (var FBComm = new FbCommand())
                    {
                        using var FBTransact1 = FBConn1.BeginTransaction();
                        FBComm.Connection = FBConn1;
                        FBComm.Transaction = FBTransact1;
                    }
                    FbConnection.ClearPool(FBConn1);
                    FBConn1.Close();
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    return false;
                }
                return true;
            }
            return false;
        }

        public string GetSerialHexNumberFromExecDisk()
        {
            //uint uintSerialNum, uintDummy1, uintDummy2;
            GetVolumeInformation(Path.GetPathRoot(Environment.CurrentDirectory), null, 0, out uint uintSerialNum, out _, out _, null, 0);
            return uintSerialNum.ToString("X");
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetVolumeInformation(string rootPathName,
                                               StringBuilder volumeNameBuffer,
                                               int volumeNameSize,
                                               out uint volumeSerialNumber,
                                               out uint maximumComponentLength,
                                               out uint fileSystemFlags,
                                               StringBuilder fileSystemNameBuffer,
                                               int nFileSystemNameSize);

        /// <summary>
        /// Altera a string de conexão FDBConnString, dos datasets, para a string informada.
        /// </summary>
        /// <param name="connectionstring">ConnectionString a ser utilizada.</param>
        public void ChangeConnectionString(string connectionstring)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
            connectionStringsSection.ConnectionStrings[0].ConnectionString = connectionstring;
            var connectionStrings = config.ConnectionStrings;
            foreach (ConnectionStringSettings conString in connectionStrings.ConnectionStrings)
            {
                if (!conString.ConnectionString.Contains("InfoPad"))
                { conString.ConnectionString = connectionstring; }

            }
            config.Save();
            ConfigurationManager.RefreshSection("connectionStrings");
            Settings.Default.Save();
            Settings.Default.Reload();
            FbConnection.ClearAllPools();
        }

        /// <summary>
        /// Aplica atualizações da base de dados no servidor, e, caso exista, na base local. Caso ela não exista, pede-se a sua cópia.
        /// </summary>
        /// <returns></returns>
        public bool StartupSequence()
        {
            // Aplicar as atualizações no servidor primeiro.
            // Depois, no PDV. Se não existir, executar ConfigurarPrimeiraSync().
            bool AplicaAtualizacao_Servidor = AplicarAtualizacaoBancoDeDados(EnmDBSync.serv);
            //audit("STARTUPSEQ", "AplicarAtualizacaoBancoDeDados(servidor) retornou " + AplicaAtualizacao_Servidor);
            if (!AplicaAtualizacao_Servidor) { return false; }

            bool AplicaAtualizacao_PDV = AplicarAtualizacaoBancoDeDados(EnmDBSync.pdv);
            //audit("STARTUPSEQ", "AplicarAtualizacaoBancoDeDados(PDV) retornou " + AplicaAtualizacao_PDV);
            if (!AplicaAtualizacao_PDV) { return false; }

            bool AplicaAtualizacao_Orca_Servidor = AplicarAtualizacaoBancoDeDados_Orca();
            //audit("STARTUPSEQ_ORCA", "AplicarAtualizacaoBancoDeDados_Orca(servidor) retornou " + AplicaAtualizacao_Orca_Servidor);
            if (!AplicaAtualizacao_Orca_Servidor) { return false; }

            return true;
        }

        /// <summary>
        /// Roda os scripts de atualização para o banco de dados informado caso sejam necessários.
        /// </summary>
        /// <param name="origemBd">Base de dados a ser atualizada</param>
        /// <returns></returns>
        private static bool AplicarAtualizacaoBancoDeDados(EnmDBSync origemBd)
        {
            string versao_banco = "0.0.0.0";
            using (var SETUP_TA = new TRI_PDV_SETUPTableAdapter())
            {
                #region Define qual banco será atualizado
                switch (origemBd)
                {
                    case EnmDBSync.pdv:
                        //audit("APLICAATUAL", "Atualizando base de contingência");
                        SETUP_TA.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath);
                        //audit("APLICAATUAL", "ConnectionString definido para " + SETUP_TA.Connection.ConnectionString);
                        break;
                    case EnmDBSync.serv:
                        //audit("APLICAATUAL", "Atualizando base do servidor");
                        SETUP_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
                        //audit("APLICAATUAL", "ConnectionString definido para " + SETUP_TA.Connection.ConnectionString);
                        break;
                    default:
                        throw new NotImplementedException("Origem de banco de dados não esperado!");
                }
                #endregion Define qual banco será atualizado

                #region Se o banco local não existir, pede pra fazer uma cópia do servidor e adequa alguns registros
                if (origemBd.Equals(EnmDBSync.pdv))
                {
                    //throw new NotImplementedException("Verificar se o banco local existe.");
                    // Lembrando que na etapa anterior (Login()) resolve o caminho do banco do servidor.
                    var _arquivoexiste = File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB");
                    // File.exists vai verificar se um arquivo existe, 
                    //O caminho dele é informado pelo Path.GetDirectoryName concatenado com @"\LocalDB\CLIPP.FDB",
                    //Assembly.GetExecutingAssembly().Location obtém o assembly que contém o código executado no momento, e o .location o caminho desse codigo executado 	
                    //-------------------------------------------------------------------------------------------------------------------------------------------------//
                    //audit("APLICAATUAL", $"O arquivo {Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB"} existe?: {_arquivoexiste}");
                    if (!_arquivoexiste)
                    {
                        //audit("APLICAATUAL", " ***Banco de dados local não foi encontrado***");
                        var _resultado = new SincronizadorDB().ConfigurarPrimeiraSync();
                        //audit("APLICAATUAL", " ConfigurarPrimeiraSync retornou " + _resultado);
                        if (!_resultado)
                        {
                            return false;
                        }
                    }
                }
                #endregion Se o banco local não existir, pede pra fazer uma cópia do servidor e adequa alguns registros

                try
                {
                    versao_banco = ((string)SETUP_TA.PegaVersaoDoBanco());
                }
                catch (Exception exp) when (exp is IndexOutOfRangeException || exp is InvalidCastException || exp.Message.Contains("Table unknown"))
                {
                    // Não conseguiu buscar a versão do banco de dados.
                    // Se origemBd = pdv, o normal é não cair aqui, pois a etapa anterior já deveria ter passado por aqui e então o banco já teria sido copiado.

                    // É recomendado evitar exceções para definir o fluxo de um programa.
                    // Como essa exceção é engatilhada apenas 1 vez (em CNTP...), não há problema em manter o fluxo assim.
                    try
                    {
                        if (!UpdateDB(origemBd))
                        {
                            throw new Exception("VersionCheck retornou falso.");
                        }
                        if (!OneTimeSetup(/*origemBd*/))
                        {
                            throw new Exception("OneTimeSetup retornou falso.");
                        }
                        //// Deve ser executado apenas 1 vez.
                        //if (!new SincronizadorDB().ConfigurarPrimeiraSync())
                        //{
                        //    // TA NO LIMBO, NÃO VAI PASSAR MAIS AQUI, depende do arquivo "FirstSyncIncomplete"
                        //    return false;
                        //}
                    }
                    catch (Exception ex)
                    {
                        logErroAntigo("Falha ao aplicar atualização no BD");
                        throw ex;
                    }
                }

                DiffVer versionamento = ComparaVersao(versao_banco, Assembly.GetExecutingAssembly().GetName().Version.ToString());
                switch (versionamento)
                {
                    case DiffVer.desatualizado:
                        Settings.Default.Upgrade();
                        // Este UpdateDB() é executado mesmo depois de cair no catch acima. Ou seja, está rodando 2 vezes.
                        if (!UpdateDB(origemBd))
                        {
                            throw new Exception("VersionCheck retornou falso.");
                        }
                        if (!LimparRegistros(origemBd))
                        {
                            throw new Exception("LimparRegistrosSujos retornou falso.");
                        }
                        SETUP_TA.AlteraVersao(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                        break;
                    case DiffVer.atualizado:
                        break;
                    case DiffVer.compativel:
                        MessageBox.Show("A versão do banco é maior que a do software PDV. Atualize assim que possível.");
                        break;
                    case DiffVer.incompativel:
                        MessageBox.Show("O progama está desatualizado e não é compatível com a nova versão. É necessário atualizar o software antes de continuar.");
                        Environment.Exit(10);
                        break;
                    default:
                        break;
                }

            }

            return true;
        }

        /// <summary>
        /// Roda os scripts do orçamento de atualização para o banco de dados do servidor.
        /// </summary>
        /// <returns></returns>
        private static bool AplicarAtualizacaoBancoDeDados_Orca()
        {
            string versao_banco_orca = "0.0.0.0";
            using (var SETUP_TA = new TRI_PDV_SETUPTableAdapter())
            {
                #region Define qual banco será atualizado (Servidor)

                //audit("APLICAATUAL_ORCA", "Atualizando base do servidor (orçamento)");
                SETUP_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
                //audit("APLICAATUAL_ORCA", "ConnectionString definido para " + SETUP_TA.Connection.ConnectionString);

                #endregion Define qual banco será atualizado (Servidor)

                try
                {
                    versao_banco_orca = ((string)SETUP_TA.PegaVersaoDoBancoOrca());
                    //audit("APLICAATUAL_ORCA", "Versão do banco encontrada: " + versao_banco_orca);
                }
                catch (Exception exp) when (exp is IndexOutOfRangeException ||
                                            exp is InvalidCastException ||
                                            exp.Message.Contains("Table unknown") ||
                                            exp.Message.Contains("Column unknown") ||
                                            exp.Message.ToUpper().Contains("COLUNA DESCONHECIDA"))
                {
                    //audit("APLICAATUAL_ORCA", "Não foi possivel obter a versão do banco");
                    logErroAntigo(RetornarMensagemErro(exp, true));

                    //audit("APLICAATUAL_ORCA", "Não foi possível obter a versão do banco. Ver o log de erro.");
                    // Não conseguiu buscar a versão do banco de dados.
                    // Se origemBd = pdv, o normal é não cair aqui, pois a etapa anterior já deveria ter passado por aqui e então o banco já teria sido copiado.

                    // É recomendado evitar exceções para definir o fluxo de um programa.
                    // Como essa exceção é engatilhada apenas 1 vez (em CNTP...), não há problema em manter o fluxo assim.
                    try
                    {
                        //audit("APLICAATUAL_ORCA", "Rodando UpdateDB() em servidor");
                        if (!UpdateDB_Orca())
                        {
                            throw new Exception("VersionCheck retornou falso (orçamento).");
                        }
                        //audit("APLICAATUAL_ORCA", "Atualização concluida.");
                    }
                    catch (Exception ex)
                    {
                        logErroAntigo(RetornarMensagemErro(ex, true));
                        throw ex;
                    }
                }
                DiffVer versionamento = ComparaVersao(versao_banco_orca,
                                                      _versaoOrcamento);// Assembly.GetExecutingAssembly().GetName().Version.ToString()); //TODO: infelizmente a versão do banco deverá ser "hard-coded"... e essa versão deve acompanhar a versão do assembly do orçamento.
                switch (versionamento)
                {
                    case DiffVer.desatualizado:
                        Settings.Default.Upgrade();
                        // Este UpdateDB() é executado mesmo depois de cair no catch acima. Ou seja, está rodando 2 vezes.
                        if (!UpdateDB_Orca())
                        {
                            throw new Exception("VersionCheck retornou falso (orçamento).");
                        }
                        //if (!LimparRegistros(origemBd))
                        //{
                        //    throw new Exception("LimparRegistrosSujos retornou falso.");
                        //}
                        SETUP_TA.AlteraVersaoOrca(_versaoOrcamento);// Assembly.GetExecutingAssembly().GetName().Version.ToString());
                        break;
                    case DiffVer.atualizado:
                        break;
                    case DiffVer.compativel:
                        MessageBox.Show("A versão do banco é maior que a do software de Orçamento. Atualize assim que possível.");
                        break;
                    case DiffVer.incompativel:
                        MessageBox.Show("O progama está desatualizado e não é compatível com a nova versão. É necessário atualizar o software antes de continuar.");
                        Environment.Exit(10);
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Executa scripts para limpar registros sujos e adaptar retaguardas mais antigas para o sistema.
        /// </summary>
        /// <param name="origemBd">Base de dados a ser atualizada</param>
        /// <returns>RETORNA NADA, CABRA DA PESTE!</returns>
        private static bool LimparRegistros(EnmDBSync origemBd)
        {
            bool blnRetorno = true;

            #region Define qual banco será atualizado

            using (var fbConn = new FbConnection())
            {
                fbConn.ConnectionString = origemBd switch
                {
                    EnmDBSync.pdv => MontaStringDeConexao("localhost", localpath),
                    EnmDBSync.serv => MontaStringDeConexao(SERVERNAME, SERVERCATALOG),
                    _ => throw new NotImplementedException("Origem de banco de dados não esperado!"),
                };
                #endregion Define qual banco será atualizado

                #region TB_CONTA_RECEBER.INV_REFERENCIA não deve ser nulo
                {
                    try
                    {
                        using var taCtarec = new FDBDataSetTableAdapters.TB_CONTA_RECEBERTableAdapter
                        {
                            Connection = fbConn
                        };
                        //audit("LIMPAREGISTROS", "Saída taCtarec.SP_TRI_CTAREC_SET_INVREF_N(): " + taCtarec.SP_TRI_CTAREC_SET_INVREF_N().ToString());
                    }
                    catch (Exception ex)
                    {
                        blnRetorno = false;
                        logErroAntigo("Erro durante gravação de INV_REFERENCIA nulos: " + RetornarMensagemErro(ex, true));
                    }
                }
                #endregion TB_CONTA_RECEBER.INV_REFERENCIA não deve ser nulo

                #region TRI_PDV_TROCAS.COO ou NUM_CAIXA não devem ser nulos
                {
                    //try
                    //{
                    //    using var taTrocas = new TRI_PDV_TROCASTableAdapter();
                    //    taTrocas.Connection = fbConn;
                    //    audit("LIMPAREGISTROS", "Saída taTrocas.SP_TRI_TROCAS_DEL_FILTHYROWS(): " + taTrocas.SP_TRI_TROCAS_DEL_FILTHYROWS().ToString());
                    //}
                    //catch (Exception ex)
                    //{
                    //    blnRetorno = false;
                    //    gravarMensagemErro("Erro durante SP_TRI_TROCAS_DEL_FILTHYROWS: " + RetornarMensagemErro(ex, true));
                    //}
                }
                #endregion TRI_PDV_TROCAS.COO ou NUM_CAIXA não devem ser nulos

                #region TB_EST_PRODUTO.CONTROLA_LOTE_VENDA não pode ser nulo
                {
                    try
                    {
                        // tem que executar o break_clipp_rules
                        using (var taBREAKONLY = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EST_PRODUTOTableAdapter())
                        {
                            taBREAKONLY.Connection = fbConn;
                            taBREAKONLY.SP_TRI_BREAK_CLIPP_RULES();
                        }

                        using var taEstProduto = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EST_PRODUTOTableAdapter
                        {
                            Connection = fbConn
                        };
                        taEstProduto.SetControlaLoteVenda().ToString();
                        //audit("LIMPAREGISTROS", "Saída taEstProduto.SetControlaLoteVenda(): " + );

                        #region NOPE
                        //// tem que executar o fix_clipp_rules
                        //using (var taFIXONLY = new PDV_WPF.DataSets.FDBDataSetOperSeedTableAdapters.TB_EST_PRODUTOTableAdapter())
                        //{
                        //    taFIXONLY.Connection = fbConn;
                        //    taFIXONLY.SP_TRI_FIX_CLIPP_RULES();
                        //}
                        #endregion NOPE
                    }
                    catch (Exception ex)
                    {
                        blnRetorno = false;
                        logErroAntigo("Erro durante taEstProduto.SetControlaLoteVenda: " + RetornarMensagemErro(ex, true));
                    }
                }
                #endregion TB_EST_PRODUTO.CONTROLA_LOTE_VENDA não pode ser nulo

                #region TB_IFS.CAIXA não deve ser nulo ou vazio

                // Preencher com 0000
                {
                    try
                    {
                        using FbCommand fbCommSetCaixaByWhiteOrNull = new FbCommand();
                        if (fbConn.State != ConnectionState.Open) { fbConn.Open(); }

                        fbCommSetCaixaByWhiteOrNull.Connection = fbConn;

                        fbCommSetCaixaByWhiteOrNull.CommandText = "UPDATE TB_IFS SET CAIXA = '0000' WHERE TRIM(COALESCE(CAIXA, '')) = ''";
                        fbCommSetCaixaByWhiteOrNull.CommandType = CommandType.Text;

                        fbCommSetCaixaByWhiteOrNull.ExecuteNonQuery();

                        if (fbConn.State != ConnectionState.Closed) { fbConn.Close(); }
                    }
                    catch (Exception ex)
                    {
                        blnRetorno = false;
                        logErroAntigo("Erro durante TB_IFS.SetCaixaByWhiteOrNull: " + RetornarMensagemErro(ex, true));
                    }
                }
            }
            #endregion TB_IFS.CAIXA não deve ser nulo ou vazio

            return blnRetorno;
        }

        /// <summary>
        /// Executa as atualizações de tabelas e procedures no banco informado.
        /// </summary>
        /// <param name="origemBd">Base de dados a ser configurado</param>
        /// <returns></returns>
        private static bool UpdateDB(EnmDBSync origemBd)
        {
            using (var Config_TA = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter())
            using (var OrcasQueries_TA = new TRI_PDV_ORCA_CUPOM_RELTableAdapter())
            {
                switch (origemBd)
                {
                    case EnmDBSync.pdv:
                        {
                            string strConn = MontaStringDeConexao("localhost", localpath);
                            Config_TA.Connection.ConnectionString = strConn;
                            //OrcasQueries_TA.Connection.ConnectionString = strConn;
                            break;
                        }
                    case EnmDBSync.serv:
                        {
                            string strConn = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
                            Config_TA.Connection.ConnectionString = strConn;
                            //OrcasQueries_TA.Connection.ConnectionString = strConn;
                            break;
                        }
                    default:
                        throw new NotImplementedException("Origem de banco de dados não esperado!");
                }

                Config_TA.A_CRIATABELAS();
                Config_TA.A_CRIATABELAS_AUX_SYNC();
                Config_TA.B_ATUALIZATABELAS();
                Config_TA.B_ATUALIZATABELAS2();
                Config_TA.B_ATUALIZA_TB_AUX_SYNC();
                Config_TA.B_GERATRIGGERS_AUX_SYNC();
                Config_TA.B_GERATRIGGERS_AUX_SYNC2();
                Config_TA.B_GERATRIGGERS_AUX_SYNC3();
                /*if (origemBd == EnmDBSync.serv) { */
                Config_TA.B_ENABLE_SERV_TRIGGERS(); /*}*/
                /*if (origemBd == EnmDBSync.pdv) { */
                Config_TA.B_DSBL_SERV_TRGGR_ON_PDV(); /*}*/
                Config_TA.D_PROCEDURES();
                Config_TA.D_PROCEDURES2();
                Config_TA.D_PROCEDURES3();
                Config_TA.D_PROCEDURES4();
                Config_TA.D_PROCEDURES5();
                Config_TA.D_PROCEDURES6();
                Config_TA.D_PROCEDURES7();
                Config_TA.D_PROCEDURES8();

                #region DDL Orçamento 1
                OrcasQueries_TA.A_ORCA_CRIATABELAS();
                OrcasQueries_TA.B_ORCA_ATUALIZATABELAS();
                OrcasQueries_TA.D_ORCA_PROCEDURES_1();
                OrcasQueries_TA.D_ORCA_PROCEDURES_2();
                #endregion DDL Orçamento 1

                string CRIATABELAS = (string)Config_TA.SP_TRI_CRIATABELAS();
                if (CRIATABELAS != "deu certo")
                {
                    //mensagem = "Erro ao Criar tabelas";
                    throw new Exception(CRIATABELAS);
                }

                string CRIATABELAS_AUX_SYNC = (string)Config_TA.SP_TRI_CRIATABELAS_AUX_SYNC();
                if (CRIATABELAS_AUX_SYNC != "deu certo")
                {
                    //mensagem = "Erro ao Criar tabelas (SERV)";
                    throw new Exception(CRIATABELAS_AUX_SYNC);
                }

                string ATUALIZA_TB_AUX_SYNC = (string)Config_TA.SP_TRI_ATUALIZA_TB_AUX_SYNC();
                if (ATUALIZA_TB_AUX_SYNC != "deu certo") { throw new Exception(ATUALIZA_TB_AUX_SYNC); }

                string ATUALIZATABELAS = (string)Config_TA.SP_TRI_ATUALIZATABELAS();
                if (ATUALIZATABELAS != "deu certo")
                {
                    //mensagem = "Erro ao Atualizar tabelas";
                    throw new Exception(ATUALIZATABELAS);
                }

                string ATUALIZATABELAS2 = (string)Config_TA.SP_TRI_ATUALIZATABELAS2();
                if (ATUALIZATABELAS2 != "deu certo")
                {
                    //mensagem = "Erro ao Atualizar tabelas";
                    throw new Exception(ATUALIZATABELAS2);
                }

                string GERATRIGGERS_AUX_SYNC = (string)Config_TA.SP_TRI_GERATRIGGERS_AUX_SYNC();
                if (GERATRIGGERS_AUX_SYNC != "deu certo") { throw new Exception(GERATRIGGERS_AUX_SYNC); }

                string GERATRIGGERS_AUX_SYNC2 = (string)Config_TA.SP_TRI_GERATRIGGERS_AUX_SYNC2();
                if (GERATRIGGERS_AUX_SYNC2 != "deu certo") { throw new Exception(GERATRIGGERS_AUX_SYNC2); }

                string GERATRIGGERS_AUX_SYNC3 = (string)Config_TA.SP_TRI_GERATRIGGERS_AUX_SYNC3();
                if (GERATRIGGERS_AUX_SYNC3 != "deu certo") { throw new Exception(GERATRIGGERS_AUX_SYNC3); }

                if (origemBd == EnmDBSync.serv)
                {
                    string ENABLETRIGGERS_SERV = (string)Config_TA.SP_TRI_ENABLE_SERV_TRIGGERS();
                    if (ENABLETRIGGERS_SERV != "deu certo")
                    {
                        //mensagem = "Erro ao ativar triggers de servidor";
                        throw new Exception(ENABLETRIGGERS_SERV);
                    }
                }

                if (origemBd == EnmDBSync.pdv)
                {
                    string DSBL_SERV_TRGGR_ON_PDV = (string)Config_TA.SP_TRI_DSBL_SERV_TRGGR_ON_PDV();
                    if (DSBL_SERV_TRGGR_ON_PDV != "deu certo")
                    {
                        //mensagem = "Erro ao desativar triggers de servidor no PDV";
                        throw new Exception(DSBL_SERV_TRGGR_ON_PDV);
                    }
                }

                string PROCEDURES = (string)Config_TA.SP_TRI_PROCEDURES();
                if (PROCEDURES != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(PROCEDURES);
                }
                string PROCEDURES2 = (string)Config_TA.SP_TRI_PROCEDURES2();
                if (PROCEDURES2 != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(PROCEDURES2);
                }
                string PROCEDURES3 = (string)Config_TA.SP_TRI_PROCEDURES3();
                if (PROCEDURES3 != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(PROCEDURES3);
                }
                string PROCEDURES4 = (string)Config_TA.SP_TRI_PROCEDURES4();
                if (PROCEDURES4 != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(PROCEDURES4);
                }
                string PROCEDURES5 = (string)Config_TA.SP_TRI_PROCEDURES5();
                if (PROCEDURES5 != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(PROCEDURES5);
                }
                string PROCEDURES6 = (string)Config_TA.SP_TRI_PROCEDURES6();
                if (PROCEDURES6 != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(PROCEDURES6);
                }
                string PROCEDURES7 = (string)Config_TA.SP_TRI_PROCEDURES7();
                if (PROCEDURES7 != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(PROCEDURES7);
                }
                string PROCEDURES8 = (string)Config_TA.SP_TRI_PROCEDURES8();
                if (PROCEDURES8 != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(PROCEDURES8);
                }
                try
                {
                    Config_TA.SP_TRI_TERMARIO_CHECKSEQ().Safestring();
                    //audit("SP_TRI_TERMARIO_CHECKSEQ>> ", );
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao executar SP_TRI_TERMARIO_CHECKSEQ", ex);
                }

                #region DDL Orçamento 2
                string ORCACRIATABELAS = (string)OrcasQueries_TA.SP_TRI_ORCA_CRIATABELAS();
                if (ORCACRIATABELAS != "deu certo")
                {
                    //mensagem = "Erro ao Criar tabelas";
                    throw new Exception(ORCACRIATABELAS);
                }
                string ORCAATUALIZATABELAS = (string)OrcasQueries_TA.SP_TRI_ORCA_ATUALIZATABELAS();
                if (ORCAATUALIZATABELAS != "deu certo")
                {
                    //mensagem = "Erro ao Criar tabelas";
                    throw new Exception(ORCAATUALIZATABELAS);
                }
                string ORCA_PROCEDURES1 = (string)OrcasQueries_TA.SP_TRI_ORCA_PROCEDURES();
                if (ORCA_PROCEDURES1 != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(ORCA_PROCEDURES1);
                }
                string ORCA_PROCEDURES2 = (string)OrcasQueries_TA.SP_TRI_ORCA_PROCEDURES_2();
                if (ORCA_PROCEDURES2 != "deu certo")
                {
                    //mensagem = "Erro ao Gerar Procedures";
                    throw new Exception(ORCA_PROCEDURES2);
                }
                #endregion DDL Orçamento 2

                try
                {
                    (new AtualizaGeradores()).Execute(origemBd);
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    MessageBox.Show("erro em " + ex.Message);
                }
            }
            return true;
        }

        /// <summary>
        /// Igual ao UpdateDB padrão, mas para orçamento.
        /// Não deve receber EnmDBSync, pois deve atualizar apenas o servidor.
        /// </summary>
        /// <returns></returns>
        public static bool UpdateDB_Orca()
        {
            using (var Config_TA = new TRI_PDV_ORCA_CUPOM_RELTableAdapter())
            {
                Config_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);

                Config_TA.A_ORCA_CRIATABELAS();
                Config_TA.B_ORCA_ATUALIZATABELAS();
                Config_TA.D_ORCA_PROCEDURES_1();
                Config_TA.D_ORCA_PROCEDURES_2();
                string CRIATABELAS = (string)Config_TA.SP_TRI_ORCA_CRIATABELAS();
                if (CRIATABELAS != "deu certo")
                {
                    throw new Exception(CRIATABELAS);
                }
                string ATUALIZATABELAS = (string)Config_TA.SP_TRI_ORCA_ATUALIZATABELAS();
                if (ATUALIZATABELAS != "deu certo")
                {
                    throw new Exception(ATUALIZATABELAS);
                }
                string PROCEDURES = (string)Config_TA.SP_TRI_ORCA_PROCEDURES();
                if (PROCEDURES != "deu certo")
                {
                    throw new Exception(PROCEDURES);
                }
                string PROCEDURES_2 = (string)Config_TA.SP_TRI_ORCA_PROCEDURES_2();
                if (PROCEDURES_2 != "deu certo")
                {
                    throw new Exception(PROCEDURES_2);
                }
            }
            return true;
        }

        /// <summary>
        /// Executa operações de configuração de base de dados que só devem ser executadas uma ver por base de dados.
        /// </summary>
        /// <returns></returns>
        public static bool OneTimeSetup()
        {
            using var FBComm = new FbCommand();
            using var FBConn = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
            using var Config_TA = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter();
            using var Setup_TA = new TRI_PDV_SETUPTableAdapter();

            //TODO: verificar se o método deve ser rodado no banco local também.

            #region Define qual banco será atualizado

            //string strConn = string.Empty;

            //switch (origemBd)
            //{
            //    case EnmDBSync.pdv:
            //        audit("OneTimeSetup>> Atualizando base de contingência");
            //        strConn = MontaStringDeConexao("localhost", localpath);
            //        audit("OneTimeSetup>> ConnectionString definido para " + strConn);
            //        break;
            //    case EnmDBSync.serv:
            //        audit("OneTimeSetup>> Atualizando base do servidor");
            //        strConn = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
            //        audit("OneTimeSetup>> ConnectionString definido para " + strConn);
            //        break;
            //    default:
            //        throw new NotImplementedException("Origem de banco de dados não esperado!");
            //}

            #endregion Define qual banco será atualizado

            //Config_TA.Connection.ConnectionString = strConn; // ?
            Config_TA.C_DADOSINICIAIS();
            //Config_TA.E_ULTIMOPASSO();
            string DADOSINICIAIS = (string)Config_TA.SP_TRI_DADOSINICIAIS();
            if (DADOSINICIAIS != "deu certo")
            {
                //mensagem = "Erro ao Criar Dados Inciais";
                throw new Exception(DADOSINICIAIS);
            }
            //Config_TA.COPIAMETODOS(); Não será mais utilizado com o uso da TB_NFVENDA.
            FBComm.CommandText = "UPDATE OR INSERT INTO TRI_PDV_SETUP (ID_DUMMY, EXECUCAO, VERSAO, ULTIMA_AT, DT_INSTALACAO, TIPO_LICENCA) " +
                                 "VALUES (1, 100, '0.0.0.0', '2000-01-01', '2000-01-01', 1) MATCHING(ID_DUMMY);";
            FBComm.Connection = FBConn;
            FBComm.CommandType = CommandType.Text;
            FBConn.Open();
            FBComm.ExecuteNonQuery();
            FBConn.Close();
            //Setup_TA.Connection.ConnectionString = strConn; // ?
            Setup_TA.FinalizaConfiguracao(Assembly.GetExecutingAssembly().GetName().Version.ToString());

            //if (Settings.Default.FDBConnString.Equals(MontaStringDeConexao("localhost", localpath)))
            //{
            //    Setup_TA.SP_TRI_SETUP_SET_ORIGEM("SERVIDOR");
            //}
            return true;
        }

        /// <summary>
        /// Consulta taxas na tabela IBPT pelo Código NCM fornecido
        /// </summary>
        /// <param name="pNCMProduto">Código NCM a ser pesquisado</param>
        /// <param name="rTaxaFed">Taxa Federal</param>
        /// <param name="rTaxaEst">Taxa Estadual</param>
        /// <param name="rTaxaMun">Taxa Municipal</param>
        public bool ConsultarTaxasPorNCM(string pNCMProduto, out decimal rTaxaFed, out decimal rTaxaEst, out decimal rTaxaMun)
        {
            rTaxaFed = 0m;
            rTaxaEst = 0m;
            rTaxaMun = 0m;

            try
            {
                var porcent_fed_est_mun = from row in IBPTDataTable.AsEnumerable()
                                          where row.Field<string>("codigo") == pNCMProduto
                                          select row;

                if (porcent_fed_est_mun is null | porcent_fed_est_mun.ToList().Count == 0)
                {
                    log.Debug("Não foi encontrado o registro na tabela IBPT contendo o seguinte NCM: " + pNCMProduto);
                    return false;
                }

                Decimal.TryParse(porcent_fed_est_mun.ToList()[0]["nacionalfederal"].Safestring().Replace('.', ','), out rTaxaFed);
                Decimal.TryParse(porcent_fed_est_mun.ToList()[0]["estadual"].Safestring().Replace('.', ','), out rTaxaEst);
                Decimal.TryParse(porcent_fed_est_mun.ToList()[0]["municipal"].Safestring().Replace('.', ','), out rTaxaMun);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Falha ao obter taxas", ex);
            }
            return false;
        }

        /// <summary>
        /// Atualiza a tabela IBPT
        /// </summary>
        private void AtualizarIBPT()
        {
            if (ChecarPorInternet())
            {
                using var client = new WebClient();
                var row_IBPT = from row in IBPTDataTable.AsEnumerable()
                               select row.Field<string>("versao");
                string versao_IBPT_local = row_IBPT.First();
                string versao_IBPT_server = "";
                try
                {
                    versao_IBPT_server = client.DownloadString("http://www.ambisoft.com.br/AmbiPDV/VER_IBPT");
                }
                catch (Exception ex)
                {
                    versao_IBPT_server = versao_IBPT_local;
                }
                if (versao_IBPT_server != versao_IBPT_local)
                {
                    client.DownloadFile("http://www.ambisoft.com.br/AmbiPDV/IBPT.csv", AppDomain.CurrentDomain.BaseDirectory + "\\IBPT.csv");
                    CarregarIBPT();
                }
            }
            else
            {
                var row_IBPT = from row in IBPTDataTable.AsEnumerable()
                               select row.Field<string>("vigenciafim");
                DateTime.TryParse(row_IBPT.First(), out DateTime validade_IBPT);
                if (DateTime.Today > validade_IBPT)
                {
                    DialogBox.Show(strings.TABELA_IBPT, DialogBoxButtons.No, DialogBoxIcons.Warn, false, strings.SUA_TABELA_IBPT_ESTA_DESATUALIZADA, strings.CASO_DEIXE_DE_ATUALIZA_LA_SUAS_VENDAS_PODEM_SER_MAL_TRIBUTADAS);
                }
            }
        }

        /// <summary>
        /// Carrega a tabela IBPT na memória.
        /// </summary>
        public void CarregarIBPT()
        {
            IBPTDataTable.Clear();
            IBPTDataTable.Columns.Clear();
            //get all lines of csv file
            string[] str;
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\IBPT.csv";
            if (!File.Exists(path))
            {
                AtualizarIBPT();
            }
            try
            {
                str = File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                log.Error("Falha ao carregar IBPT", ex);
                DialogBox.Show(strings.TABELA_IBPT, DialogBoxButtons.No, DialogBoxIcons.Error, false, strings.FALHA_AO_ACESSAR_TABELA_IBPT, "Verifique os logs");
                System.Windows.Application.Current.Shutdown(); //deuruim();
                return;
            }

            // create new datatable

            // get the column header means first line
            string[] temp = str[0].Split(';');
            // creates columns of gridview as per the header name
            foreach (string t in temp)
            {
                IBPTDataTable.Columns.Add(t, typeof(string));
            }
            // now retrive the record from second line and add it to datatable
            for (int i = 1; i < str.Length; i++)
            {
                string[] t = str[i].Split(';');
                IBPTDataTable.Rows.Add(t);

            }
            AtualizarIBPT();
        }

        /// <summary>
        /// Checa se há conectividade à Internet
        /// </summary>
        /// <returns></returns>
        private bool ChecarPorInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExecutaSQLRemotoAsync(string serial)
        {
            using var CerealConn = new MySqlConnection("server=turkeyshit.mysql.dbaas.com.br;user id=turkeyshit;password=Pfes2018;persistsecurityinfo=True;database=turkeyshit;ConvertZeroDateTime=True");
            using var CerealDataAdapter = new MySqlDataAdapter($"SELECT * FROM REMOTESQL WHERE PK_SERIAL = {serial} AND STATUS = \"PENDING\"", CerealConn);
            using var CerealComm = new MySqlCommand() { Connection = CerealConn };
            using var FbComm = new FbCommand();
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
            string exceptionMessage;
            DataTable resultsTable = new DataTable();
            await CerealDataAdapter.FillAsync(resultsTable);
            if (resultsTable.Rows.Count > 0)
            {
                CerealConn.Open();
                foreach (DataRow row in resultsTable.Rows)
                {
                    if ((string)row["BaseKind"] == "SERVER" || (string)row["BaseKind"] == "BOTH")
                    {
                        FbComm.Connection = SERVER_FB_CONN;
                        FbComm.CommandText = (string)row["Command"];
                        try
                        {
                            await FbComm.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            exceptionMessage = ex.Message;
                        }
                    }
                    if ((string)row["BaseKind"] == "LOCAL" || (string)row["BaseKind"] == "BOTH")
                    {
                        FbComm.Connection = LOCAL_FB_CONN;
                        FbComm.CommandText = (string)row["Command"];
                        try
                        {
                            await FbComm.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            exceptionMessage = ex.Message;
                        }
                    }
                    CerealComm.CommandText = $"UPDATE REMOTESQL SET STATUS = \"PROCESSED\" WHERE ID = {(int)row["ID"]}";
                    await CerealComm.ExecuteNonQueryAsync();
                }
                CerealConn.Close();
            }
            return true;
        }
        #endregion


        /// <summary>
        /// Carrega as configurações da base de dados e as guarda na memória runtime
        /// </summary>
        /// <param name="pContingencia">A contingência estava ativada previamente?</param>
        //public void CarregarConfiguracoes(bool pContingencia)
        //{
        //    using (var ta_config = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter())
        //    using (var dt_config = new PDV_WPF.DataSets.FDBDataSetConfig.TRI_PDV_CONFIGDataTable())
        //    using (var ta_setup = new TRI_PDV_SETUPTableAdapter())
        //    using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
        //    using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
        //    {
        //        var macAddr = (new Funcoes.LicencaDeUsoOffline(90, 15)).GetSerialHexNumberFromExecDisk();

        //        if (pContingencia)
        //        {
        //            ta_config.Connection = LOCAL_FB_CONN;
        //            ta_setup.Connection = LOCAL_FB_CONN;
        //        }
        //        else
        //        {
        //            ta_config.Connection = SERVER_FB_CONN;
        //            ta_setup.Connection = SERVER_FB_CONN;
        //        }

        //        ta_config.FillByMacAdress(dt_config, macAddr);

        //        sangriaObrigatoria = Convert.ToBoolean(Convert.ToInt16((string)dt_config.Rows[0]["EXIGE_SANGRIA"]));
        //        caixaMaximo = Convert.ToDecimal(dt_config.Rows[0]["VALOR_MAX_CAIXA"]);
        //        bloqueiaNoLimite = Convert.ToBoolean((Convert.ToInt16((string)dt_config.Rows[0]["BLOQUEIA_NO_LIMITE"])));
        //        permiteFolga = Convert.ToBoolean((Convert.ToInt16((string)dt_config.Rows[0]["PERMITE_FOLGA_SANGRIA"])));
        //        valorDeFolga = Convert.ToDecimal(dt_config.Rows[0]["VALOR_DE_FOLGA"]);
        //        descontoMaximo = (double)ta_setup.GetData().Rows[0]["DESC_MAX_OP"];
        //        interrompeNaoEncontrado = Convert.ToBoolean((Convert.ToInt16((string)dt_config.Rows[0]["INTERROMPE_NAO_ENCONTRADO"])));
        //        cortesia = (string)dt_config.Rows[0]["MENSAGEM_CORTESIA"];
        //        rodape = dt_config.Rows[0]["MENSAGEM_RODAPE"].Safestring();
        //        no_caixa = Convert.ToInt32(dt_config.Rows[0]["NO_CAIXA"]);
        //        Pede_CPF = (int)dt_config.Rows[0]["PEDE_CPF"];

        //        configurado = true;

        //        modeloCupom = (short)dt_config.Rows[0]["MODELO_CUPOM"];
        //        switch (dt_config.Rows[0]["PERMITE_ESTOQUE_NEGATIVO"])
        //        {
        //            case -1:
        //                permiteNegativo = null;
        //                break;
        //            case 0:
        //                permiteNegativo = false;
        //                break;
        //            case 1:
        //                permiteNegativo = true;
        //                break;
        //        }
        //    }
        //    //Local_OPER_TA.Connection.ConnectionString = Local_CTAREC_TA.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath);
        //}


        /// <summary>
        /// Lista de coisas que Tim, the Enchanter precisa comprar quando for ao mercado.
        /// </summary>
        public List<string> eegg = new List<string>
        {
            "\u004D\u0041\u0043\u0047\u0055\u0059\u0056\u0045\u0052",
            "\u004D\u0041\u004C\u0050\u0041\u0052\u0049\u0044\u004F",
            "\u0048\u0041\u0052\u004C\u0045\u0059\u0020\u0051\u0055\u0049\u004E\u004E",
            "\u0043\u004F\u004D\u002E\u0020\u0053\u0048\u0045\u0050\u0041\u0052\u0044",
            "\u004D\u0041\u004C\u004C\u0041\u004E\u0044\u0052\u004F",
            "\u0048\u0049\u0042\u0041\u004E\u0041",
            "\u0053\u004B\u0059\u0057\u0041\u004C\u004B\u0045\u0052",
            "\u004F\u0042\u0049\u002D\u0057\u0041\u004E",
            "\u004D\u0045\u0053\u0054\u0052\u0045\u0020\u0053\u0050\u004C\u0049\u004E\u0054\u0045\u0052",
            "\u006E\u0075\u006C\u006C",
            "\u0044\u004F\u004B\u004B\u0041\u0045\u0042\u0049",
            "\u0044\u002E\u0056\u0061",
            "\u004D\u0049\u0053\u0053\u0049\u004E\u0047\u004E\u004F",
            "\u0042\u004F\u0057\u0053\u0045\u0052",
            "\u0048\u0045\u0052\u004D\u0041\u004E\u004F\u0054\u0045\u0055",
            "\u004F\u0075\u0074\u004F\u0066\u0049\u006E\u0064\u0065\u0078\u0045\u0078\u0063\u0065\u0070\u0074\u0069\u006F\u006E",
            "\u0053\u0041\u0059\u004F\u0052\u0049",
            "\u0044\u0052\u002E\u0020\u005A\u0049\u0045\u0047\u004C\u0045\u0052",
            "\u0037\u0020\u0044\u0049\u0041\u0053",
            "\u0050\u004F\u0044\u0045\u0052\u004F\u0053\u004F\u0020\u0043\u0041\u0053\u0054\u0049\u0047\u0041",
            "\u0042\u004F\u004E\u0044\u002C\u0020\u004A\u0041\u004D\u0045\u0053\u0020\u0042\u004F\u004E\u0044"
        };

        /// <summary>
        /// Avalia qual a versão da retaguarda está instalada e avisa se for antiga demais.
        /// </summary>
        /// <param name="strConnDb">String de conexão da base da retaguarda</param>
        /// <returns></returns>
        internal bool DetectarVersaoClippAntigoDemais(string strConnDb)
        {
            bool blnClippAntigoDemais = false;

            try
            {
                string strVersaoClipp = GetRdbSupVersao(strConnDb);
                int intVersaoClippAno = 2016;
                if (!string.IsNullOrWhiteSpace(strVersaoClipp))
                {
                    intVersaoClippAno = Convert.ToInt32(strVersaoClipp.Substring(0, 4));
                }
                if (intVersaoClippAno <= 2016)
                {
                    blnClippAntigoDemais = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao detectar versão de Clipp", ex);
            }

            return blnClippAntigoDemais;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strConnDb"></param>
        /// <returns></returns>
        private static string GetRdbSupVersao(string strConnDb)
        {
            string strVersao = string.Empty;

            using (var taSetupPdv = new TRI_PDV_SETUPTableAdapter())
            {
                taSetupPdv.Connection.ConnectionString = strConnDb;
                strVersao = (string)taSetupPdv.GetRdbSupVersao();
            }

            return strVersao;
        }

        public udx_pdv_oper_class VerificaPDVOper(string operador_atual)
        {
            using (var TERMARIO_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TRI_PDV_TERMINAL_USUARIOTableAdapter())
            using (var USERS_TA = new TRI_PDV_USERSTableAdapter())
            using (var OPER_TA = new TRI_PDV_OPERTableAdapter())
            using (var LOCAL_FB_CON = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                TERMARIO_TA.Connection = LOCAL_FB_CON;
                USERS_TA.Connection = LOCAL_FB_CON;
                OPER_TA.Connection = LOCAL_FB_CON;

                try
                {
                    int _id_user = USERS_TA.PegaIdPorUser(operador_atual).Safeint();
                    int _count = (int)TERMARIO_TA.ContaCaixaAberto(NO_CAIXA, _id_user);
                    if (_count == 1)
                    {
                        var udxPdvOper = new udx_pdv_oper_class
                        {
                            timestamp = (DateTime)OPER_TA.SP_TRI_INSELECT_PDV_OPER(NO_CAIXA, _id_user),
                            numcaixa = NO_CAIXA
                        };
                        return udxPdvOper;
                    }
                    else if (_count > 1)
                    {
                        log.Error("Mais de uma entrada em TRI_PDV_TERMINAL_USUARIO foi encontrada. Favor rever os fechamentos.");
                        using (var Conn = new FbConnection(LOCAL_FB_CON.ConnectionString))
                        using (var Comm = new FbCommand())
                        {
                            Comm.Connection = Conn;
                            Comm.CommandType = CommandType.Text;
                            Comm.CommandText = "UPDATE TRI_PDV_TERMINAL_USUARIO SET STATUS = 'F' WHERE ID_USER = @Param1 ORDER BY ID_OPER ROWS @Param2;";
                            Comm.Parameters.Add("@Param1", _id_user);
                            Comm.Parameters.Add("@Param2", _count - 1);
                            Conn.Open();
                            Comm.ExecuteNonQuery();
                            Conn.Close();
                        }

                        return VerificaPDVOper(operador_atual);
                    }
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    MessageBox.Show("Erro ao verificar abertura de caixa. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                    System.Windows.Application.Current.Shutdown();
                    return null;
                    //throw ex; DEURUIM();
                }
            }
            return null;
        }


        //public static void Contingencia()
        //{
        //    bool? ConexaoServidorOk = null;
        //    // Quando o app é inicializado, ele não responde por alguns segundos.
        //    // Não seria interessante deixar um splash ou mostrar que o app está executando?
        //    // Do jeito como está, deixa a impressão que o app não iniciou.
        //    var task = Task.Run(() => TestaConexaoComServidor());
        //    if (task.Wait(TimeSpan.FromSeconds(5)))
        //    {
        //        ConexaoServidorOk = task.Result;
        //    }
        //    else
        //    {
        //        ConexaoServidorOk = false;
        //    }
        //    switch (ConexaoServidorOk)
        //    {
        //        case true:
        //            ChangeConnectionString(MontaStringDeConexao(SERVERNAME, SERVERCATALOG));
        //            audit("FDBConnString definido para DB na rede:");
        //            audit(Settings.Default.FDBConnString);
        //            break;
        //        case false:
        //            ChangeConnectionString(MontaStringDeConexao("localhost", localpath));
        //            audit("FDBConnString definido para DB de contingência:");
        //            DialogBox db = new DialogBox("Erro de conexão", "Não foi possível estabelecer uma conexão com o servidor.", "O sistema será iniciado em modo de contingência.", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Warn);
        //            db.ShowDialog();
        //            break;
        //        case null:
        //            break;
        //    }
        //}
    }
    public class Configs
    {
        public string cortesia { get; set; }
    }
    public class IBPT
    {
        public string codigo { get; set; }
        public string ex { get; set; }
        public string tipo { get; set; }
        public string descricao { get; set; }
        public string nacionalfederal { get; set; }
        public string importadosfederal { get; set; }
        public string estadual { get; set; }
        public string municipal { get; set; }
        public string vigenciainicio { get; set; }
        public string vigenciafim { get; set; }
        public string chave { get; set; }
        public string versao { get; set; }
        public string fonte { get; set; }
    }
    public class udx_pdv_oper_class
    {
        public int numcaixa { get; set; }
        public DateTime timestamp { get; set; }
    }

    //public class MyLabel : Label
    //{
    //    protected override void OnPaint(PaintEventArgs e)
    //    {
    //        SizeF CapitalSize = e.Graphics.MeasureString(Text.Substring(0, 1).ToUpper(), Font);
    //        SizeF SmallerSize = e.Graphics.MeasureString(Text.Substring(1, Text.Length - 1).ToUpper(), new Font(Font.FontFamily, Font.Size - 2, Font.Style));
    //        e.Graphics.DrawString(Text.Substring(0, 1).ToUpper(), Font, new SolidBrush(ForeColor), 0, 0);
    //        e.Graphics.DrawString(Text.Substring(1, Text.Length - 1).ToUpper(), new Font(Font.FontFamily, Font.Size - 6, Font.Style), new SolidBrush(ForeColor), CapitalSize.Width - 8, CapitalSize.Height - SmallerSize.Height + 5);
    //    }
    //}
    //public class READIBPT
    //{
    //    private readonly static FileHelperEngine<IBPT> engine = new FileHelperEngine<IBPT>();
    //    private readonly IBPT[] result = engine.ReadFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\IBPT\ibpt.csv");

    //    //public string retorna_estadual(string ncm)
    //    //{
    //    //    int index = Array.IndexOf(result, ncm);
    //    //    return result[index].estadual;
    //    //}
    //}

    //public class PrinterSettings
    //{
    //    public setupstatus status_naofiscal { get; set; }
    //    public setupstatus status_sat { get; set; }
    //}

    public class AtualizaGeradores
    {

        public void Execute(EnmDBSync origemBd)
        {

            var strConn = origemBd switch
            {
                EnmDBSync.pdv => MontaStringDeConexao("localhost", localpath),
                EnmDBSync.serv => MontaStringDeConexao(SERVERNAME, SERVERCATALOG),
                _ => throw new NotImplementedException("Origem de banco de dados não esperado!"),
            };
            using var FBConn1 = new FbConnection(strConn);
            FBConn1.Open();
            using FbTransaction FBTransact1 = FBConn1.BeginTransaction();
            using var FBComm = new FbCommand
            {
                Connection = FBConn1,
                Transaction = FBTransact1,
                CommandType = CommandType.Text,
                CommandText = @"EXECUTE BLOCK AS DECLARE VARIABLE VAR INTEGER; BEGIN SELECT COALESCE(MAX(ID_CUPOM),1) FROM TB_CUPOM INTO VAR; EXECUTE STATEMENT 'ALTER SEQUENCE GEN_TB_CUPOM_ID RESTART WITH ' || VAR; END"
            };
            FBComm.ExecuteNonQuery();
            FBComm.CommandText = @"EXECUTE BLOCK AS DECLARE VARIABLE VAR INTEGER; BEGIN SELECT COALESCE(MAX(ID_ITEMCUP),1) FROM TB_CUPOM_ITEM INTO VAR; EXECUTE STATEMENT 'ALTER SEQUENCE GEN_TB_CUPOM_ITEM_ID RESTART WITH ' || VAR; END";
            FBComm.ExecuteNonQuery();
            FBComm.CommandText = @"EXECUTE BLOCK AS DECLARE VARIABLE VAR INTEGER; BEGIN SELECT COALESCE(MAX(ID_CTAREC),1) FROM TB_CONTA_RECEBER INTO VAR; EXECUTE STATEMENT 'ALTER SEQUENCE GEN_TB_CTAREC_ID RESTART WITH ' || VAR; END";
            FBComm.ExecuteNonQuery();
            FBComm.CommandText = @"EXECUTE BLOCK AS DECLARE VARIABLE VAR INTEGER; BEGIN SELECT COALESCE(MAX(ID_MOVTO),1) FROM TB_MOVDIARIO INTO VAR; EXECUTE STATEMENT 'ALTER SEQUENCE GEN_TB_MOVDIARIO_ID RESTART WITH ' || VAR; END";
            FBComm.ExecuteNonQuery();
            FBTransact1.Commit();
        }
    }

    // Fonte: https://weblog.west-wind.com/posts/2017/Jul/02/Debouncing-and-Throttling-Dispatcher-Events#Search-Text-Box-Filter
    /// <summary>
    /// Provides Debounce() and Throttle() methods.
    /// Use these methods to ensure that events aren't handled too frequently.
    /// 
    /// Throttle() ensures that events are throttled by the interval specified.
    /// Only the last event in the interval sequence of events fires.
    /// 
    /// Debounce() fires an event only after the specified interval has passed
    /// in which no other pending event has fired. Only the last event in the
    /// sequence is fired.
    /// </summary>
    public class DebounceDispatcher
    {
        private DispatcherTimer timer;
        private DateTime timerStarted { get; set; } = DateTime.UtcNow.AddYears(-1);

        /// <summary>
        /// Debounce an event by resetting the event timeout every time the event is 
        /// fired. The behavior is that the Action passed is fired only after events
        /// stop firing for the given timeout period.
        /// 
        /// Use Debounce when you want events to fire only after events stop firing
        /// after the given interval timeout period.
        /// 
        /// Wrap the logic you would normally use in your event code into
        /// the  Action you pass to this method to debounce the event.
        /// Example: https://gist.github.com/RickStrahl/0519b678f3294e27891f4d4f0608519a
        /// </summary>
        /// <param name="interval">Timeout in Milliseconds</param>
        /// <param name="action">Action<object> to fire when debounced event fires</object></param>
        /// <param name="param">optional parameter</param>
        /// <param name="priority">optional priorty for the dispatcher</param>
        /// <param name="disp">optional dispatcher. If not passed or null CurrentDispatcher is used.</param>        
        public void Debounce(int interval, Action<object> action,
            object param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher disp = null)
        {
            // kill pending timer and pending ticks
            timer?.Stop();
            timer = null;

            if (disp == null)
            {
                disp = Dispatcher.CurrentDispatcher;
            }

            // timer is recreated for each event and effectively
            // resets the timeout. Action only fires after timeout has fully
            // elapsed without other events firing in between
            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (timer == null)
                {
                    return;
                }

                timer?.Stop();
                timer = null;
                action.Invoke(param);
            }, disp);

            timer.Start();
        }

        /// <summary>
        /// This method throttles events by allowing only 1 event to fire for the given
        /// timeout period. Only the last event fired is handled - all others are ignored.
        /// Throttle will fire events every timeout ms even if additional events are pending.
        /// 
        /// Use Throttle where you need to ensure that events fire at given intervals.
        /// </summary>
        /// <param name="interval">Timeout in Milliseconds</param>
        /// <param name="action">Action<object> to fire when debounced event fires</object></param>
        /// <param name="param">optional parameter</param>
        /// <param name="priority">optional priorty for the dispatcher</param>
        /// <param name="disp">optional dispatcher. If not passed or null CurrentDispatcher is used.</param>
        public void Throttle(int interval, Action<object> action,
            object param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher disp = null)
        {
            // kill pending timer and pending ticks
            timer?.Stop();
            timer = null;

            if (disp == null)
            {
                disp = Dispatcher.CurrentDispatcher;
            }

            var curTime = DateTime.UtcNow;

            // if timeout is not up yet - adjust timeout to fire 
            // with potentially new Action parameters           
            if (curTime.Subtract(timerStarted).TotalMilliseconds < interval)
            {
                interval -= (int)curTime.Subtract(timerStarted).TotalMilliseconds;
            }

            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (timer == null)
                {
                    return;
                }

                timer?.Stop();
                timer = null;
                action.Invoke(param);
            }, disp);

            timer.Start();
            timerStarted = curTime;
        }
    }
}
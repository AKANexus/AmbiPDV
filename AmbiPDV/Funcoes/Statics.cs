using Clearcove.Logging;
using DeclaracoesDllSat;
using PDV_WPF.Configuracoes;
using PDV_WPF.FDBDataSetTableAdapters;
using PDV_WPF.Objetos;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.ViewModels;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using System.Threading.Tasks;

namespace PDV_WPF.Funcoes
{

    public static class ValidaCNPJ
    {
        public static bool IsCnpj(string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;
            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
            if (cnpj.Length != 14)
            {
                return false;
            }

            tempCnpj = cnpj.Substring(0, 12);
            soma = 0;
            for (int i = 0; i < 12; i++)
            {
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            }

            resto = (soma % 11);
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
            {
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            }

            resto = (soma % 11);
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito += resto.ToString();
            return cnpj.EndsWith(digito);
        }
    }//Testa se o CNPJ é válido.
    public static class ValidaCPF
    {
        public static bool IsCpf(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
            {
                return false;
            }

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = resto.ToString();
            tempCpf += digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito += resto.ToString();
            return cpf.EndsWith(digito);
        }
    }//Teste se o CPF é válido.
    public static class Statics
    {
        public static readonly CultureInfo ptBR = CultureInfo.GetCultureInfo("pt-BR");
        public static readonly System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
        public static readonly System.Text.RegularExpressions.Regex _regex_1 = new System.Text.RegularExpressions.Regex("[^a-zA-Z\u00C0-\u017F]+");
        public static readonly DataTable IBPTDataTable = new DataTable();

        public static void CarregaXML()
        {
            var configsXML = new ConfiguracoesXML();
            var xmlLido = configsXML.Deserializa();
            LOGO = xmlLido.LOGO;
            NOMESOFTWARE = xmlLido.NOMESOFTWARE;
            FBTIMEOUT = xmlLido.FBTIMEOUT;
            SERVERNAME = xmlLido.SERVERNAME;
            COMANDASCATALOG = xmlLido.COMANDASCATALOG;
            SERVERCATALOG = xmlLido.SERVERCATALOG;
            PERMITE_CANCELAR_VENDA_EM_CURSO = xmlLido.AUTORIZADO switch
            {
                0 => false,
                _ => true
            };
            FECHAMENTO_EXTENDIDO = xmlLido.FECHAMENTO_EXTENDIDO switch
            {
                0 => false,
                _ => true
            };
            FORÇA_GAVETA = xmlLido.FORCA_GAVETA switch
            {
                1 => true,
                _ => false
            };
            USA_ORÇAMENTO = xmlLido.USAORCAMENTO switch
            {
                1 => true,
                _ => false
            };
            SATTIMEOUT = xmlLido.SATTIMEOUT;
            EXIBEFOTO = xmlLido.EXIBEFOTO switch
            {
                1 => true,
                _ => false
            };
            SENHA_PRAZO = xmlLido.SENHA_PRAZO switch
            {
                1 => true,
                _ => false
            };
            SENHA_CONSULTA = xmlLido.SENHA_CONSULTA switch
            {
                1 => true,
                _ => false
            };
            SCANNTECH = xmlLido.SCANNTECH switch
            {
                1 => true,
                _ => false
            };
        }
        public static bool ContemSoNumeros(string texto)
        {
            return !_regex.IsMatch(texto);
        }
        public static bool ContemSoLetras(string texto)
        {
            return !_regex_1.IsMatch(texto);
        }

        /// <summary>
        /// Verifica se a resolução é menor que 1024x768. Caso verdadeiro, fecha o programa com um aviso.
        /// </summary>
        public static void ChecaTamanhoDaTela()
        {
            var tela = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            if (tela.Width < 1024 && tela.Height < 768)
            {
                DialogBox.Show(strings.AMBIPDV, DialogBoxButtons.No, DialogBoxIcons.Dolan, false, strings.NECESSARIA_RESOLUCAO_MINIMA);
                Application.Current.Shutdown();
            }
        }

        public static List<string> clientesOC = new List<string>();        
        public static void CarregarClientesOC()
        {
            FbConnection fbConnection = new() { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var cLIENTETableAdapter = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter()
                { Connection = fbConnection };
            using var dt_cli = new DataSets.FDBDataSetOperSeed.TB_CLIENTEDataTable();
            cLIENTETableAdapter.FillOrderByName(dt_cli);
            foreach (DataSets.FDBDataSetOperSeed.TB_CLIENTERow row in dt_cli)
            {
                if (row.STATUS == "A")
                clientesOC.Add(row.NOME);
            }   
        }
        public static List<string> administradoraOC = new List<string>();
        public static void CarregaAdministradoras()
        {
            FbConnection fbConnection = new() { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var aDIMINISTRADORATableAdapter = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CARTAO_ADMINISTRADORATableAdapter() { Connection = fbConnection };
            using var dt_admin = new DataSets.FDBDataSetOperSeed.TB_CARTAO_ADMINISTRADORADataTable();
            aDIMINISTRADORATableAdapter.FillPegaAdmins(dt_admin);
            foreach(DataSets.FDBDataSetOperSeed.TB_CARTAO_ADMINISTRADORARow row in dt_admin)
            {
                administradoraOC.Add(row.DESCRICAO);
            }
        }
        public static string RetornaCPF_CNPJSat(string nomeCli)
        {
            try
            {               
                FbConnection fbConnection = new() { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                using var cLIENTETableAdapter = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter() { Connection = fbConnection };
                using var cLIENTETableAdapterCPF = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PFTableAdapter() { Connection = fbConnection };
                using var cLIENTETableAdapterCNPJ = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PJTableAdapter() { Connection = fbConnection };
                using var cnjpcpf_cli = new DataSets.FDBDataSetOperSeed.TB_CLI_PFDataTable();
                var idObjetc = cLIENTETableAdapter.PegaIDPorCliente(nomeCli);
                int idInt = Convert.ToInt32(idObjetc);                
                string CPF = cLIENTETableAdapterCPF.PegaCPFPorID(idInt); string CNPJ = cLIENTETableAdapterCNPJ.PegaCNPJPorID(idInt);
                if(CPF is not null)
                {
                    CPF = CPF.Replace(".", ""); CPF = CPF.Replace("-", "");                    
                }
                if(CNPJ is not null)
                {
                    CNPJ = CNPJ.Replace(".", ""); CNPJ = CNPJ.Replace("/", ""); CNPJ = CNPJ.Replace("-", "");
                }
                string retorno = CPF == null ? CNPJ : CPF;     
                if(retorno is null)
                {
                    retorno = "";
                }                
                return retorno;
            }
            catch
            {                                
                MessageBox.Show("Não foi possivel capturar o CPF/CNPJ do cliente pelo cadastro, favor digitar manualmente no campo acima!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }

        #region declarações

        public static CaixaViewModel mvm = new();
        public static string localpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB";
        public enum DiffVer { desatualizado, atualizado, compativel, incompativel };
        public enum TipoLicenca { offline, online };
        public enum TipoTEF { Undefined = 0, Credito = 3, Debito = 2, Administrativo = 110, PendenciasTerminal = 130, Pix = 122 };
        public enum Impressora { SAT, ECF }
        public enum DecisaoWhats { Whats, ImpressaoNormal, NaoImprime }
        public static TipoDeEmpresa tipoDeEmpresa = TipoDeEmpresa.SN;
        public enum StatusTEF { Aberto, EmAndamento, Confirmado, Cancelado, Erro, NaoAutorizado }
        public enum TipoDeEmpresa { RPA, SN }
        public enum DialogBoxButtons { Yes, No, YesNo, None }
        public enum DialogBoxIcons { None, Info, Warn, Error, Dolan, Sangria, Suprimento }

        public static List<string> args = new List<string>();
        public static bool homologaSAT = false, homologaDEVOL = false, eLGINStdCall;

        public static bool Conectividade = true;
        public static List<string> argumentosPermitidos = new List<string>() { "/auditoria", "/developer", "/verbose" };



        public class TEFEventArgs : EventArgs
        {
            public decimal Valor { get; set; }
            public TipoTEF TipoDoTEF { get; set; }
            public string GetStrPgCfe
            {
                get
                {
                    return TipoDoTEF switch
                    {
                        TipoTEF.Credito => "03",
                        TipoTEF.Debito => "04",
                        TipoTEF.Pix => "17",
                        _ => "99"
                    };
                }
                set
                {
                    if (value == "03") TipoDoTEF = TipoTEF.Credito;
                    else TipoDoTEF = TipoTEF.Debito;
                }
            }
            public int idMetodo { get; set; }
            public StatusTEF status { get; set; }
            public List<string> viaCliente { get; set; }
            public List<string> viaLoja { get; set; }
            public string NoCupom { get; set; }
            public List<Pendencia> pendenciasXML { get; set; }
        }


        /// <summary>
        /// Sempre que houver atualização no projeto do orçamento, alterar essa
        /// constante para a mesma versão que está no assembly.
        /// </summary>
        public const string _versaoOrcamento = "1.0.3.0";

        #endregion
        public static string operador;


        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        public static char GetCharFromKey(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }

        /// <summary>
        /// Retorna as informações (stack) de uma exceção fornecida de um modo legível.
        /// </summary>
        /// <param name="ex">Exceção a ser tratada</param>
        /// <param name="blnExibirStackTrace">Verdadeiro caso deseje exibir o stacktrace da exceção.</param>
        /// <param name="nivel">Não deve ser informado.</param>
        /// <returns></returns>
        public static string RetornarMensagemErro(Exception ex, bool blnExibirStackTrace, int nivel = 0)
        {
            string strMensagemRetorno = string.Empty;
            try
            {
                if (ex.InnerException != null)
                {
                    strMensagemRetorno += RetornarMensagemErro(ex.InnerException, blnExibirStackTrace, nivel);
                    nivel++;
                }
                string strLineBreak = nivel == 0 ? "\n" : "\n\n";
                string strMensagem = string.Format("{0}{1}", strLineBreak, ex.Message);
                if (blnExibirStackTrace) { strMensagem = string.Format("{0}\nStackTrace: {1}", strMensagem, ex.StackTrace); }
                strMensagemRetorno += strMensagem;
            }
            catch (Exception) { }
            return strMensagemRetorno;
        }

        /// <summary>
        /// Compara duas versões e informa qual a compatibilidade entre elas.
        /// </summary>
        /// <param name="versaoreferencia">Versão a ser usada como referência. Normalmente a versão do banco de dados.</param>
        /// <param name="versaotestada">Versão a qual se deseja determinar o estado de atualização.</param>
        /// <returns>Desatualizado caso versaoreferencia seja menor, atualizado caso sejam iguais, ou compatível ou incompatível caso versaoreferencia seja maior</returns>
        public static DiffVer ComparaVersao(string versaoreferencia, string versaotestada)
        {
            if (versaoreferencia is null) return DiffVer.desatualizado;
            string[] _vrefer = versaoreferencia.Split('.');
            string[] _vteste = versaotestada.Split('.');
            if (_vrefer[0].Safeint() > _vteste[0].Safeint())
            {
                return DiffVer.incompativel;
            }
            else if (_vrefer[0].Safeint() < _vteste[0].Safeint())
            {
                return DiffVer.desatualizado;
            }
            else
            {
                if (_vrefer[1].Safeint() > _vteste[1].Safeint())
                {
                    return DiffVer.incompativel;
                }
                else if (_vrefer[1].Safeint() < _vteste[1].Safeint())
                {
                    return DiffVer.desatualizado;
                }
                else
                {
                    if (_vrefer[2].Safeint() > _vteste[2].Safeint())
                    {
                        return DiffVer.compativel;
                    }
                    else if (_vrefer[2].Safeint() < _vteste[2].Safeint())
                    {
                        return DiffVer.desatualizado;
                    }
                    else
                    {
                        if (_vrefer[3].Safeint() > _vteste[3].Safeint())
                        {
                            return DiffVer.atualizado;
                        }
                        else if (_vrefer[3].Safeint() < _vteste[3].Safeint())
                        {
                            return DiffVer.desatualizado;
                        }
                        else
                        {
                            return DiffVer.atualizado;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gera uma string codificada com um hash MD5 e um salt.
        /// </summary>
        /// <param name="senha">String a ser codificada.</param>
        /// <returns>String codificada em MD5</returns>
        public static string GenerateHash(string senha)
        {
            using var md5Hash = new HMACMD5(Encoding.UTF8.GetBytes("Mah"));
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(senha));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }//Gera um salted hash.

        /// <summary>
        /// Compara o hash MD5 de uma senha informada é igual ao hash informado.
        /// </summary>
        /// <param name="senha">Senha, sem codificação, a ser comparada.</param>
        /// <param name="hash">Hash a ser usado como referência.</param>
        /// <returns>Verdadeiro se forem iguais, senão, falso.</returns>
        public static bool ChecaHash(string senha, string hash)
        {
            string _test = GenerateHash(senha);
            return (_test == hash) switch
            {
                (true) => true,
                _ => false,
            };
        }//Checa se a senha informada tem o hash informado.

        /// <summary>
        /// Retorna uma string de conexão para Firebird, baseado nos parâmetros informados.
        /// </summary>
        /// <param name="datasource">Nome ou IP do servidor</param>
        /// <param name="initialcatalog">Endereço LOCAL do arquivo *.FDB da base de dados.</param>
        /// <param name="charset">Charset da conexão.</param>
        /// <param name="password">Senha da base de dados</param>
        /// <param name="userid">Usuário da base de dados</param>
        /// <returns></returns>
        public static string MontaStringDeConexao(string datasource, string initialcatalog, string charset = "WIN1252", string password = "masterkey", string userid = "SYSDBA")
        {
            //return String.Format(@"data source={0};initial catalog={1};user id={2};Password={3};character set={4}", datasource, initialcatalog, userid, password, charset);
            return $@"initial catalog={initialcatalog};data source={datasource};user id={userid};Password={password};encoding={charset};charset={charset}";
        }

        /// <summary>
        /// Abre uma modal perguntando por uma senha de uma conta que tenha acesso gerencial.
        /// </summary>
        /// <param name="acao">Informe o motivo para pedir a senha gerencial.</param>
        /// <returns></returns>
        public static bool PedeSenhaGerencial(string acao, bool modoTeste = false)
        {
            if (modoTeste) return true;
            var senha = new perguntaSenha(acao);
            senha.ShowDialog();
            switch (senha.DialogResult)
            {
                case true:
                    if (senha.NivelAcesso == perguntaSenha.nivelDeAcesso.Gerente)
                    { return true; }
                    else
                    {
                        DialogBox.Show(strings.SENHA_DIGITADA_NAO_E_VALIDA, DialogBoxButtons.No, DialogBoxIcons.None, false, strings.USUARIO_NAO_POSSUI_PERMISSAO);
                        return false;
                    }
                case false:
                default:
                    return false;
            }
        }

        public static bool ChecaStatusSATServidor()
        {
            string retorno;
            byte[] bytes = Encoding.UTF8.GetBytes("ChecarStatus");
            using (var SAT_ENV_TA = new TRI_PDV_SAT_ENVTableAdapter())
            {
                SAT_ENV_TA.SP_TRI_ENVIA_SAT_SERVIDOR(NO_CAIXA, bytes);
            }
            try
            {
                var sb = new SATBox("Operação no SAT", "Aguarde a resposta do SAT.");
                sb.ShowDialog();
                if (sb.DialogResult is null)
                {
                    return false;
                }
                if (sb.DialogResult == false)
                {
                    return false;
                }
                else { retorno = sb.cod_retorno; }
            }
            catch (Exception ex)
            {
                DialogBox.Show("ERRO", DialogBoxButtons.No, DialogBoxIcons.Error, false, ex.Message);
                throw ex;
            }


            switch (retorno)
            {
                case "08000":
                    //MessageBox.Show("SAT configurado com sucesso.");
                    return true;
                case "08001":
                    MessageBox.Show("Código de Ativação do SAT Incorreto.");
                    return false;
                case "08002":
                    MessageBox.Show("SAT ainda não ativado");
                    return false;
                case "08098":
                    //MessageBox.Show("SAT em processamento. Tente novamente mais tarde.");
                    return true;
                case "08099":
                    MessageBox.Show("Erro desconhecido.");
                    return false;
                case "ERRO":
                    MessageBox.Show("Falha ao obter o retorno do SAT. Verifique os logs no servidor.");
                    return false;
                default:
                    return false;
            }
        }

        public static bool ChecaStatusSATLocal(bool emcontingencia)
        {
            if (!SAT_USADO || SATSERVIDOR || emcontingencia) return true;
            try
            {
                var ns = new NumSessao();
                Declaracoes_DllSat.sRetorno = Marshal.PtrToStringAnsi(Declaracoes_DllSat.ConsultarSAT(ns.GeraNumero(), MODELO_SAT));
                string[] retorno = Declaracoes_DllSat.sRetorno.Split('|');
                if (retorno.Length == 0)
                {
                    Login.stateGif = false;
                    MessageBox.Show("Falha ao obter retorno do SAT", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (retorno.Length == 1)
                {
                    Login.stateGif = false;
                    MessageBox.Show("Retorno na tentatativa de comunicação com o SAT\n" + retorno[0], "Resposta SAT", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                else
                {
                    switch (retorno[1])
                    {
                        case "08000":
                            //MessageBox.Show("SAT configurado com sucesso.");
                            return true;
                        case "08001":
                            MessageBox.Show("Código de Ativação do SAT Incorreto.");
                            return true;
                        case "08002":
                            MessageBox.Show("SAT ainda não ativado");
                            return true;
                        case "08098":
                            //MessageBox.Show("SAT em processamento. Tente novamente mais tarde.");
                            return true;
                        case "08099":
                            MessageBox.Show("Erro desconhecido.");
                            return true;
                        default:
                            throw new Exception("Erro durante Teste" +
                                " Fim a Fim. Nenhum código de retorno recebido. " + retorno[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Abre a gaveta ao imprimir uma linha em branco.
        /// </summary>
        public static void AbreGavetaSPOOLER()
        {
            try
            {
                PrintFunc.RecebePrint(" ", PrintFunc.negrito, PrintFunc.centro, 1);
                PrintFunc.PrintaSpooler();
            }
            catch (Exception ex)
            {
                DialogBox.Show("ABERTURA DE GAVETA", DialogBoxButtons.No, DialogBoxIcons.Error, true, $"Não foi possivel abrir a gaveta pois\n{ex.Message}");
                logErroAntigo(ex.Message);                
            }
        }

        readonly static object errorObj = new object();
        static StreamWriter errorwriter = new StreamWriter($@"{AppDomain.CurrentDomain.BaseDirectory}\Logs\erro-{DateTime.Today:dd-MM-yy}.txt", true) { AutoFlush = true };
        //static StreamWriter TEFwriter = new StreamWriter($@"{AppDomain.CurrentDomain.BaseDirectory}\Logs\TEFPend-{DateTime.Today.ToString("dd-MM-yy")}.txt", true) { AutoFlush = true };

        /// <summary>
        /// Grava uma string com data e hora no log de erros, separados em pastas por ano, mês e dia.
        /// </summary>
        /// <param name="texto">String a ser gravada.</param>
        public static void logErroAntigo(string texto)
        {
            lock (errorObj)
            {
                try
                {
                    errorwriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "\t" + texto + "\n\r\n\r");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao gravar mensagem de erro: " + RetornarMensagemErro(ex, false));
                }
            }
        }
        
        public async static Task AbreGavetaDLL()
        {
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        int abriuPorta = IniciaPorta("COM3");
                        int abriuGaveta = AcionaGaveta();
                        int fechouPorta = FechaPorta();                      
                    }
                    catch (Exception)
                    {
                        throw;
                    }                                              
                });                
            }
            catch(Exception ex)
            {
                logErroAntigo(ex.Message);
                AbreGavetaSPOOLER();                
            }
        }

        [DllImport(@"DLL_PRINTERS\InterfaceEpsonNF.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern int IniciaPorta(string port);

        [DllImport(@"DLL_PRINTERS\InterfaceEpsonNF.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern int AcionaGaveta();

        [DllImport(@"DLL_PRINTERS\InterfaceEpsonNF.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern int FechaPorta();       
    }
}

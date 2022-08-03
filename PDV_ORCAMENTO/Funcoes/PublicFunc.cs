using FirebirdSql.Data.FirebirdClient;
using PDV_ORCAMENTO.FDBOrcaDataSetTableAdapters;
using PDV_ORCAMENTO.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Linq;
using static PDV_WPF.staticfunc;
using System.Data;

namespace PDV_WPF
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
                return false;
            tempCnpj = cnpj.Substring(0, 12);
            soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
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
                return false;
            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }
    }//Teste se o CPF é válido.
    public static class Extensoes
    {
        public static string Safestring(this string vDado)
        {
            string vResult = string.Empty;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                vResult = vDado.ToString();
            }
            return vResult;
        }
        public static string Safestring(this object vDado)
        {
            string vResult = string.Empty;
            if (vDado != null)
            {
                if (!string.IsNullOrWhiteSpace(vDado.ToString()))
                {
                    vResult = vDado.ToString();
                }
            }
            return vResult;
        }
        public static DateTime Safedate(this DateTime vDado)
        {
            DateTime vResult = DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(Convert.ToString(vDado)))
            {
                vResult = vDado;
            }
            return vResult;
        }
        public static DateTime Safedate(this string vDado)
        {
            DateTime vResult = DateTime.MinValue;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                try
                {
                    vResult = Convert.ToDateTime(vDado);
                }
                catch
                {
                    vResult = DateTime.MinValue;
                }
            }
            return vResult;
        }
        public static double Safedouble(this double vDado)
        {
            double vResult = 0;
            try
            {
                if (!string.IsNullOrWhiteSpace(Convert.ToString(Convert.ToDouble(vDado))))
                {
                    vResult = vDado;
                }
            }
            catch
            {
                vResult = 0;
            }
            return vResult;
        }
        public static double Safedouble(this string vDado)
        {
            double vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                if (vDado != "NaN")
                {
                    try
                    {
                        vResult = Convert.ToDouble(vDado);
                    }
                    catch
                    {
                        vResult = 0;
                    }
                }
            }
            return vResult;
        }
        public static double Safedouble(this object vDado)
        {
            double vResult = 0;

            if (vDado == null) return vResult;
            if (string.IsNullOrWhiteSpace(vDado.ToString())) return vResult;
            if ((string)vDado == "NaN") return vResult;

            try
            {
                vResult = Convert.ToDouble(vDado);
            }
            catch
            {
                vResult = 0;
            }

            return vResult;
        }
        public static decimal Safedecimal(this string vDado)
        {
            decimal vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                if (vDado != "NaN")
                {
                    try
                    {
                        vResult = Convert.ToDecimal(vDado);
                    }
                    catch
                    {
                        vResult = 0;
                    }
                }
            }
            return vResult;
        }
        public static decimal Safedecimal(this object vDado)
        {
            decimal vResult = 0;

            if (vDado == null) return vResult;
            if (string.IsNullOrWhiteSpace(vDado.ToString())) return vResult;
            if ((string)vDado == "NaN") return vResult;

            try
            {
                vResult = Convert.ToDecimal(vDado);
            }
            catch
            {
                vResult = 0;
            }

            return vResult;
        }
        public static byte Safebyte(this string vDado)
        {
            byte vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                if (vDado != "NaN")
                {
                    try
                    {
                        vResult = Convert.ToByte(vDado);
                    }
                    catch
                    {
                        vResult = 0;
                    }


                }
            }
            return vResult;
        }
        public static short Safeshort(this string vDado)
        {
            short vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                if (vDado != "NaN")
                {
                    try
                    {
                        vResult = Convert.ToInt16(vDado);
                    }
                    catch
                    {
                        vResult = 0;
                    }

                }
            }
            return vResult;
        }
        public static int Safeint(this object objeto)
        {
            int vResult = 0;

            if (objeto != null && !string.IsNullOrWhiteSpace(objeto.ToString()))
            {
                if (objeto.ToString() != "NaN")
                {
                    try
                    {
                        vResult = Convert.ToInt32(objeto);
                    }
                    catch
                    {
                        vResult = 0;
                    }
                }
            }
            return vResult;
        }
        public static int Safeint(this string vDado)
        {
            int vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                if (vDado != "NaN")
                {
                    try
                    {
                        vResult = Convert.ToInt32(vDado);
                    }
                    catch
                    {
                        vResult = 0;
                    }
                }
            }
            return vResult;
        }
        public static long Safelong(this object objeto)
        {
            long vResult = 0;

            if (objeto != null && !string.IsNullOrWhiteSpace(objeto.ToString()))
            {
                if (objeto.ToString() != "NaN")
                {
                    try
                    {
                        vResult = Convert.ToInt64(objeto);
                    }
                    catch
                    {
                        vResult = 0;
                    }
                }
            }
            return vResult;
        }
        public static long Safelong(this string vDado)
        {
            long vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                if (vDado != "NaN")
                {
                    try
                    {
                        vResult = Convert.ToInt64(vDado);
                    }
                    catch
                    {
                        vResult = 0;
                    }
                }
            }
            return vResult;
        }
        public static bool Safebool(this string vDado)
        {
            bool vResult = false;

            if (!string.IsNullOrWhiteSpace(vDado))
            {
                try
                {
                    vResult = Convert.ToBoolean(vDado);
                }
                catch (Exception)
                {
                    vResult = false;
                }
            }

            return vResult;
        }


        public static string GetHashCode(this string inputString)
        {
            // Todos os disposables objects devem ser desfeitos apropriadamente com ".Dispose()"
            // ou aplicando o using, como está abaixo.
            // A não execução desse método pode causar erros aleatórios no sistema, como memory leak.
            // Fonte: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(inputString));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }//Transforma uma string em has MD5.
        public static string TiraPont(this string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }//Tira pontuação da string.
        public static string TruncateLongString(this string str)
        {
            return str.Length <= 27 ? str : str.Remove(27);
        }//Trunca strings acima de 27 chars.
        public static string Trunca(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Length <= maxLength ? str : str.Substring(0, maxLength);
        }//Trunca strings acima de maxLength.
        public static bool IsNumbersOnly(this string str)
        {
            foreach (char c in str)
            {
                if (!Char.IsDigit(c)) return false;
            }
            return true;
        }//Checa se a string possui apenas dígitos.
        public static decimal TruncateFunction(this decimal number, int digits)
        {
            decimal stepper = (decimal)(Math.Pow(10.0, (double)digits));
            int temp = (int)(stepper * number);
            return (decimal)temp / stepper;
        }
    }
    public static class staticfunc
    {
        #region declarações
        public enum TipoImpressora
        {
            /// <summary>
            /// Neste projeto, usar esse valor de enum apenas para teste de impressão quando for spooler.
            /// A principal finalidade é impressão de reports.
            /// </summary>
            officeA4 = 0 ,
            thermal80 = 1,
            nenhuma = 2
        }
        public enum PagAssist { erro = -1, bemvindo, legal, selecao, confignada, configserial, configecf, configspooler, configsat, confirmacao, fim }
        public enum ErrorLevel : int { information, warning, error, critical }
        public enum DiffVer { desatualizado, atualizado, compativel, incompativel };
        #endregion
        public static PDV_ORCAMENTO.MainViewModel.ComboBoxBindingDTO operador = new PDV_ORCAMENTO.MainViewModel.ComboBoxBindingDTO();
        public static string RetornarMensagemErro(Exception ex, bool blnExibirStackTrace, int nivel = 0)
        {
            string strMensagemRetorno = string.Empty;
            try
            {
                if (ex.InnerException != null)
                {
                    strMensagemRetorno = strMensagemRetorno + RetornarMensagemErro(ex.InnerException, blnExibirStackTrace, nivel);
                    nivel++;
                }
                string strLineBreak = nivel == 0 ? "\n" : "\n\n";
                string strMensagem = string.Format("{0}{1}", strLineBreak, ex.Message);
                if (blnExibirStackTrace) { strMensagem = string.Format("{0}\nStackTrace: {1}", strMensagem, ex.StackTrace); }
                strMensagemRetorno = strMensagemRetorno + strMensagem;
            }
            catch (Exception) { }
            //catch (Exception superEx)
            //{
            //    // Não conseguiu retornar mensagem, tenta gravar no EventLog do Windows
            //    // Poderia tentar gravar log do app mesmo, mas pode dar loop infinito
            //    GravarEventLogWin(string.Format("Erro ao retornar mensagem de erro de exception: \n{0} \n\nMensagem original: \n{1}", 
            //                                    superEx.Message, ex.Message));
            //}
            return strMensagemRetorno;
        }

        //private static void GravarEventLogWin(string message)
        //{
        //    if (string.IsNullOrWhiteSpace(message)) { return; }

        //    try
        //    {
        //        using (var eventLogger = new EventLog())
        //        {
        //            eventLogger.Source = "AmbiPDV";
        //            eventLogger.WriteEntry(message, EventLogEntryType.Information, 101, 1);
        //        }
        //    }
        //    catch (Exception exNaoTemOqueFazerDeuRuimGeral)
        //    {
        //        MessageBox.Show(string.Format("Erro ao gravar log de evento do Windows: \n{0}\n\nMensagem original: \n{1}\n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.", 
        //                                      exNaoTemOqueFazerDeuRuimGeral.Message,
        //                                      message));
        //    }
        //}

        public static void gravarMensagemErro(string texto)
        {
            try
            {
                string path = string.Format("{0}\\{1}\\{2}\\{3}\\{4}", AppDomain.CurrentDomain.BaseDirectory,
                                                                       "Erros",
                                                                       DateTime.Today.Year.ToString(),
                                                                       DateTime.Today.Month.ToString(),
                                                                       DateTime.Today.Day.ToString());
                Directory.CreateDirectory(path);
                string pathFile = string.Format("{0}\\LogErro.txt", path);
                //if (!File.Exists(pathFile)) //TODO: Comentado pra ficar mais leve
                //{
                //    using (StreamWriter sw = File.CreateText(pathFile))
                //    {
                //        sw.WriteLine("Erros AMBI Orçamento");
                //    }
                //}
                using (StreamWriter sw = File.AppendText(pathFile))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + texto + "\n\n");
                }
            }
            catch (Exception ex)
            {
                //GravarEventLogWin(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao gravar mensagem de erro: " + RetornarMensagemErro(ex, false));
            }
        }
        public static void audit(string texto)
        {
            if (Settings.Default.Auditoria > 0)
            {
                string path = string.Format("{0}\\{1}\\{2}\\{3}\\{4}", AppDomain.CurrentDomain.BaseDirectory,
                                                                       "Auditorias",
                                                                       DateTime.Today.Year.ToString(),
                                                                       DateTime.Today.Month.ToString(),
                                                                       DateTime.Today.Day.ToString());
                Directory.CreateDirectory(path);
                string pathFile = string.Format("{0}\\Auditoria.txt", path);
                //if (!File.Exists(pathFile)) //TODO: Comentado pra ficar mais leve
                //{
                //    using (StreamWriter sw = File.CreateText(pathFile))
                //    {
                //        sw.WriteLine("Auditoria AMBI Orçamento");
                //    }
                //}
                using (StreamWriter sw = File.AppendText(pathFile))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + texto);
                }
            }
        }//Escreve no arquivo da auditoria.
        public static void verbose(string texto)
        {
            if (Settings.Default.Auditoria > 1)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Auditoria.txt";
                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("Auditoria AMBI Orçamento");
                    }
                }
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\tVERB>>" + texto);
                }
            }
        }//Escreve no arquivo da auditoria.
        public static string GenerateHash(string senha)
        {
            using (var md5Hash = new HMACMD5(Encoding.UTF8.GetBytes("Mah")))
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(senha));
                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }//Gera um salted hash.
        public static bool ChecaHash(string senha, string hash)
        {
            string _test = GenerateHash(senha);
            switch (_test == hash)
            {
                case (true):
                    return true;
                default:
                    return false;
            }
        }//Checa se a senha informada tem o hash informado.
        public static string GetCurrentMacAddress()
        {
            string retorno;

            try
            {
                retorno = (
                    from nic in NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == OperationalStatus.Up
                    select nic.GetPhysicalAddress().ToString()
                    ).FirstOrDefault();
            }
            catch (Exception ex)
            {
                gravarMensagemErro(RetornarMensagemErro(ex, true));
                throw ex; //deuruim();
            }

            return retorno;
        }
        
        public static bool UpdateDB()
        {
            using (var Config_TA = new PDV_ORCAMENTO.DataSetes.FDBDataSetDDLTableAdapters.QueriesTableAdapter())
            {

                Config_TA.A_ORCA_CRIATABELAS();
                Config_TA.B_ORCA_ATUALIZATABELAS();
                Config_TA.D_ORCA_PROCEDURES1();
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
        /// Retorna mensagens e StackTraces (opcional) de Exception.
        /// </summary>
        /// <param name="ex">Exceção original</param>
        /// <param name="blnExibirStackTrace">True para exibir os StackTraces</param>
        /// <param name="nivel">Uso interno, apenas para mostrar no retorno o quão profundo foram as exceções internas.</param>
        /// <returns>Se houver erro interno do método, o retorno será vazio.</returns>
        public static bool OneTimeSetup()
        {
            //using (var Config_TA = new TRI_PDV_CONFIGTableAdapter())
            //{
            //Config_TA.C_DADOSINICIAIS();
            //Config_TA.E_ULTIMOPASSO();
            //string DADOSINICIAIS = (string)Config_TA.SP_TRI_DADOSINICIAIS();
            //if (DADOSINICIAIS != "deu certo")
            //{
            //    throw new Exception(DADOSINICIAIS);
            //}
            //Config_TA.COPIAMETODOS();
            //string ULTIMOPASSO = (string)Config_TA.SP_TRI_ULTIMOPASSO();
            //if (ULTIMOPASSO != "deu certo")
            //{
            //    throw new Exception(ULTIMOPASSO);
            //}
            //using (var Setup_TA = new TRI_PDV_SETUPTableAdapter())
            //{
            //    Setup_TA.FinalizaConfiguracao(Assembly.GetExecutingAssembly().GetName().Version.ToString());

            //if (Settings.Default.FDBOrcaConnString.Equals(Settings.Default.NetworkDB))
            //{
            //    Setup_TA.SP_TRI_SETUP_SET_ORIGEM("SERVIDOR");
            //}
            //}
            //}
            return true;
        }
        /// <summary>
        /// Compara duas versões e informa qual a compatibilidade entre elas.
        /// </summary>
        /// <param name="versaoreferencia">Versão a ser usada como referência. Normalmente a versão do banco de dados.</param>
        /// <param name="versaotestada">Versão a qual se deseja determinar o estado de atualização.</param>
        /// <returns>Desatualizado caso versaoreferencia seja menor, atualizado caso sejam iguais, ou compatível ou incompatível caso versaoreferencia seja maior</returns>
        public static DiffVer ComparaVersao(string versaoreferencia, string versaotestada)
        {
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
                            return DiffVer.compativel;
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
    }
    public class funcoes
    {
        #region declarações
        public enum NF { na = -1, nenhuma, usb, serial }
        public enum ECF { na = -1, nao, sim }
        public enum UsaSAT { na = -1, nao, sim }
        public enum PagAssist { erro = -1, bemvindo, legal, selecao, confignada, configserial, configecf, configspooler, configsat, confirmacao, fim }
        public enum ErrorLevel : int { information, warning, error, critical }
        //public static string operador;
        #endregion
        #region métodos
        
        private bool AplicarAtualizacaoBancoDeDados(/*EnmDBSync origemBd*/)
        {
            string versao_orca_banco = "0.0.0.0";
            using (var SETUP_TA = new TRI_PDV_SETUPTableAdapter())
            {
                #region Define qual banco será atualizado
                //switch (origemBd)
                //{
                //    case EnmDBSync.pdv:
                //        audit("Atualizando base de contingência");
                //        SETUP_TA.Connection.ConnectionString = Settings.Default.ContingencyDB.ToString();
                //        audit("ConnectionString definido para " + SETUP_TA.Connection.ConnectionString);
                //        break;
                //    case EnmDBSync.serv:
                audit("Atualizando base do servidor");
                SETUP_TA.Connection.ConnectionString = Settings.Default.NetworkDB.ToString();
                audit("ConnectionString definido para " + SETUP_TA.Connection.ConnectionString);
                //        break;
                //    default:
                //        throw new NotImplementedException("Origem de banco de dados não esperado!");
                //}
                #endregion Define qual banco será atualizado

                #region Se o banco local não existir, pede pra fazer uma cópia do servidor e adequa alguns registros
                //if (origemBd.Equals(EnmDBSync.pdv))
                //{
                //    //throw new NotImplementedException("Verificar se o banco local existe.");
                //    // Lembrando que na etapa anterior (Login()) resolve o caminho do banco do servidor.
                //    var _arquivoexiste = File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB");
                //    audit(String.Format("O arquivo {0} existe?: {1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB", _arquivoexiste));
                //    if (!_arquivoexiste)
                //    {
                //        audit("***Banco de dados local não foi encontrado***");
                //        var _resultado = new SincronizadorDB().ConfigurarPrimeiraSync();
                //        audit("ConfigurarPrimeiraSync retornou " + _resultado);
                //        if (!_resultado)
                //        {
                //            return false;
                //        }
                //    }
                //}
                #endregion Se o banco local não existir, pede pra fazer uma cópia do servidor e adequa alguns registros

                try
                {
                    //versao_banco = ((string)SETUP_TA.PegaVersaoDoBanco());
                    versao_orca_banco = ((string)SETUP_TA.PegaVersaoOrcaDoBanco()).Safestring();
                    audit("Versão do banco encontrada (orçamento): " + versao_orca_banco);
                }
                catch (Exception exp) when (exp is IndexOutOfRangeException ||
                                            exp is InvalidCastException || 
                                            exp.Message.Contains("Table unknown") ||
                                            exp.Message.Contains("Column unknown"))
                {
                    audit("Não foi possível obter a versão do banco.");
                    audit(exp.Message);
                    // Não conseguiu buscar a versão do banco de dados.
                    // Se origemBd = pdv, o normal é não cair aqui, pois a etapa anterior já deveria ter passado por aqui e então o banco já teria sido copiado.

                    // É recomendado evitar exceções para definir o fluxo de um programa.
                    // Como essa exceção é engatilhada apenas 1 vez (em CNTP...), não há problema em manter o fluxo assim.
                    try
                    {
                        audit("Rodando UpdateDB() em " + /*origemBd*/"servidor");
                        if (!UpdateDB(/*origemBd*/))
                        {
                            throw new Exception("VersionCheck retornou falso.");
                        }
                        audit("Rodando OnteTimeSetup() em " + /*origemBd*/"servidor");
                        if (!OneTimeSetup())
                        {
                            throw new Exception("OneTimeSetup retornou falso.");
                        }
                        audit("Atualização concluida.");
                        //// Deve ser executado apenas 1 vez.
                        //if (!new SincronizadorDB().ConfigurarPrimeiraSync())
                        //{
                        //    // TA NO LIMBO, NÃO VAI PASSAR MAIS AQUI, depende do arquivo "FirstSyncIncomplete"
                        //    return false;
                        //}
                    }
                    catch (Exception ex)
                    {
                        gravarMensagemErro(RetornarMensagemErro(ex, true));
                        throw ex; //deuruim();
                    }
                }
                DiffVer versionamento = ComparaVersao(versao_orca_banco, Assembly.GetExecutingAssembly().GetName().Version.ToString());
                switch (versionamento)
                {
                    case DiffVer.desatualizado:
                        Settings.Default.Upgrade();
                        // Este UpdateDB() é executado mesmo depois de cair no catch acima. Ou seja, está rodando 2 vezes.
                        if (!UpdateDB())
                        {
                            throw new Exception("VersionCheck retornou falso.");
                        }
                        //if (!LimparRegistros(origemBd))
                        //{
                        //    throw new Exception("LimparRegistrosSujos retornou falso.");
                        //}
                        SETUP_TA.AlteraVersaoOrca(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                        break;
                    case DiffVer.atualizado:
                        break;
                    case DiffVer.compativel:
                        MessageBox.Show("A versão do banco é maior que a do software. Atualize assim que possível.");
                        break;
                    case DiffVer.incompativel:
                        MessageBox.Show("O progama está desatualizado e não é compatível com a nova versão. É necessário atualizar o software antes de continuar.");
                        Environment.Exit(10);
                        break;
                    default:
                        break;
                }

                {
                }
            }

            return true;
        }

        /// <summary>
        /// Testa a conectividade com o servidor em rede através de uma transação sem comando com a base. True caso a conexão seja bem feita, senão false.
        /// </summary>
        /// <returns></returns>
        public bool? TestaConexaoComServidor()
        {
            try
            {
                using (var FBConn1 = new FbConnection(Settings.Default.NetworkDB))
                {
                    verbose("Checando conexão com o servidor");
                    verbose("==========Criado novo FbConnection, usando ConnectionString configurável");
                    FBConn1.Open();
                    verbose("==========FbConn aberta");
                    using (var FBComm = new FbCommand())
                    {
                        Debug.WriteLine("==========Criado novo FbCommand");
                        using (var FBTransact1 = FBConn1.BeginTransaction())
                        {
                            verbose("==========Criada nova FbTransaction, utilizando a FbConnection criada");
                            FBComm.Connection = FBConn1;
                            verbose("==========FbConnection designada ao FbCommand");
                            FBComm.Transaction = FBTransact1;
                            verbose("==========FbTransact designada o FbCommand");
                        }
                    }
                    FBConn1.Close();
                }
            }
            catch (Exception ex)
            {
                verbose("==========Rodou uma exceção durante TestaConexaoComServidor");
                verbose(RetornarMensagemErro(ex, true));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Altera a string de conexão FDBConnString para a string informada.
        /// </summary>
        /// <param name="connectionstring"></param>
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
            verbose("ChangeConnectionString salvo.");
            ConfigurationManager.RefreshSection("connectionStrings");
            Settings.Default.Save();
            Settings.Default.Reload();
            FbConnection.ClearAllPools();
            verbose("Rodou ChangeConnectionString");
            verbose("FDBConnString é:");
            verbose(Settings.Default.FDBOrcaConnString);
        }

        //public void GravarErroFromDataTable(string secao, DataTable dataTable, Exception ex)
        //{
        //    if (!(dataTable is null) | dataTable.Rows.Count > 0)
        //    {
        //        string erro = RetornarMensagemErro(ex, true);
        //        try
        //        {
        //            gravarMensagemErro("Erro ao sincronizar " + secao + 
        //                               ":\nRegistro " + dataTable.GetErrors()?[0]?[0] + " retornou um erro: " + dataTable.GetErrors()[0].RowError + erro);
        //        }
        //        catch (IndexOutOfRangeException)
        //        {
        //            gravarMensagemErro(erro);
        //        }
        //    }
        //    else
        //    {
        //        gravarMensagemErro(RetornarMensagemErro(ex, true));
        //    }

        //    #region Refatorar

        //    //if (dataTable is null | dataTable.Rows.Count == 0)
        //    //{
        //    //    gravarMensagemErro(RetornarMensagemErro(ex, true));
        //    //}
        //    //else
        //    //{
        //    //    string strErro = RetornarMensagemErro(ex, true);
        //    //    try
        //    //    {
        //    //        var oq = dataTable.GetErrors();

        //    //        if (oq.ran.Length > 0)
        //    //        {
        //    //            if (oq.)
        //    //            {

        //    //            }

        //    //            gravarMensagemErro("Erro ao preencher resultado da consulta " + secao +
        //    //                               ":\nRegistro " + oq?[0]?[0] + " retornou um erro: " + oq[0].RowError + strErro);
        //    //        }
        //    //    }
        //    //    catch (IndexOutOfRangeException ex2)
        //    //    {
        //    //        gravarMensagemErro(strErro);
        //    //    }
        //    //}

        //    #endregion Refatorar
        //}

        #endregion
        public static List<string> eegg = new List<string>
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

        public bool StartupSequence()
        {
            #region Verifica se o banco local não existe e então copia o banco do servidor para ser o novo banco de contingência. (DESATIVADO)

            //string localpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //bool blnFirstSyncIncomplete = File.Exists(localpath + @"\LocalDB\FirstSyncIncomplete");
            //if ((!File.Exists(localpath + @"\LocalDB\CLIPP.FDB") && File.Exists(localpath + @"\LocalDB\FirstSyncCompleted")) || blnFirstSyncIncomplete)
            //{
            //    // Verificar se TRI_PDV_SETUP.ORIGEM = null ou "SERVIDOR" (NÃO deve ser default)

            //    bool blnConfigurarPrimeiraSync = false;

            //    if (blnFirstSyncIncomplete)
            //    {
            //        blnConfigurarPrimeiraSync = true;
            //    }
            //    else
            //    {
            //        using (var taSetupServ = new TRI_PDV_SETUPTableAdapter())
            //        using (var tblSetupServ = new TRI_PDV_SETUPDataTable())
            //        {
            //            taSetupServ.Connection.ConnectionString = Settings.Default.NetworkDB.ToString();
            //            taSetupServ.Fill(tblSetupServ);
            //            if (tblSetupServ.Rows.Count > 0)
            //            {
            //                if (tblSetupServ[0].ORIGEM == "SERVIDOR")
            //                {
            //                    blnConfigurarPrimeiraSync = true;
            //                }
            //            }
            //        }
            //    }

            //    if (blnConfigurarPrimeiraSync)
            //    {
            //        // Deve ser executado apenas 1 vez.
            //        if (!new SincronizadorDB().ConfigurarPrimeiraSync()) { return false; }
            //    }
            //}

            #endregion Verifica se o banco local não existe e então copia o banco do servidor para ser o novo banco de contingência.

            // Aplicar as atualizações no servidor primeiro.
            // Depois, no PDV. Se não existir, executar ConfigurarPrimeiraSync().
            bool AplicaAtualizacao_Servidor = AplicarAtualizacaoBancoDeDados();
            audit("AplicarAtualizacaoBancoDeDados(servidor) retornou " + AplicaAtualizacao_Servidor);
            if (!AplicaAtualizacao_Servidor) { return false; }

            return true;
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
        //            ChangeConnectionString(Settings.Default.NetworkDB);
        //            audit("FDBOrcaConnString definido para DB na rede:");
        //            audit(Settings.Default.FDBOrcaConnString);
        //            break;
        //        case false:
        //            ChangeConnectionString(Settings.Default.ContingencyDB);
        //            audit("FDBOrcaConnString definido para DB de contingência:");
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

    public class MyLabel : Label
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            SizeF CapitalSize = e.Graphics.MeasureString(Text.Substring(0, 1).ToUpper(), Font);
            SizeF SmallerSize = e.Graphics.MeasureString(Text.Substring(1, Text.Length - 1).ToUpper(), new Font(Font.FontFamily, Font.Size - 2, Font.Style));
            e.Graphics.DrawString(Text.Substring(0, 1).ToUpper(), Font, new SolidBrush(ForeColor), 0, 0);
            e.Graphics.DrawString(Text.Substring(1, Text.Length - 1).ToUpper(), new Font(Font.FontFamily, Font.Size - 6, Font.Style), new SolidBrush(ForeColor), CapitalSize.Width - 8, CapitalSize.Height - SmallerSize.Height + 5);
        }
    }

    public class PrinterSettings
    {
        //public bool configurar_nf_usb { get; set; }
        //public bool teste { get; set; }
        public string usb_printer { get; set; }
    }

    public class SemBalanca : Exception
    {
        //        Message = "Não há balança instalada e/ou configurada no sistema";
    }

    public class AtualizaGeradores : IDisposable
    {
        FbConnection FBConn1 = new FbConnection(Settings.Default.NetworkDB);
        FbCommand FBComm = new FbCommand();
        public void Execute()
        {
            FBConn1.Open();
            using (FbTransaction FBTransact1 = FBConn1.BeginTransaction())
            {
                FBComm.Connection = FBConn1;
                FBComm.Transaction = FBTransact1;
                FBComm.CommandType = System.Data.CommandType.Text;
                FBComm.CommandText = @"EXECUTE BLOCK AS DECLARE VARIABLE VAR INTEGER; BEGIN SELECT COALESCE(MAX(ID_CUPOM),1) FROM TB_CUPOM INTO VAR; EXECUTE STATEMENT 'ALTER SEQUENCE GEN_TB_CUPOM_ID RESTART WITH ' || VAR; END";
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
        public void Dispose()
        {
            FBComm.Dispose();
            FBConn1.Dispose();
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
                disp = Dispatcher.CurrentDispatcher;

            // timer is recreated for each event and effectively
            // resets the timeout. Action only fires after timeout has fully
            // elapsed without other events firing in between
            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (timer == null)
                    return;

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
                disp = Dispatcher.CurrentDispatcher;

            var curTime = DateTime.UtcNow;

            // if timeout is not up yet - adjust timeout to fire 
            // with potentially new Action parameters           
            if (curTime.Subtract(timerStarted).TotalMilliseconds < interval)
                interval -= (int)curTime.Subtract(timerStarted).TotalMilliseconds;

            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (timer == null)
                    return;

                timer?.Stop();
                timer = null;
                action.Invoke(param);
            }, disp);

            timer.Start();
            timerStarted = curTime;
        }
    }
}
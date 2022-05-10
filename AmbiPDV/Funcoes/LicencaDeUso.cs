using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using MySql.Data.MySqlClient;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using RestSharp;
using System.Net.Http;
using System.Threading.Tasks;
using RestSharp.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PDV_WPF.Funcoes
{
    public class LicencaDeUsoOffline
    {
        Logger log = new Logger("Licenciamento offline");
        //public const int constDiasLicencaOffline = 90; // A cada liberação de licença é adicionado 90 dias de permissão de uso.
        //public const int constDiasAvisoExpLicenca = 15; // Faltando a partir de 15 dias, avisar o usuário que a licença está prestes a expirar.
        public int constDiasLicencaOffline { get; set; } // A cada liberação de licença é adicionado 90 dias de permissão de uso.
        public int constDiasAvisoExpLicenca { get; set; } // Faltando a partir de 15 dias, avisar o usuário que a licença está prestes a expirar.

        public LicencaDeUsoOffline(int intDiasLicencaOffline, int intDiasAvisoExpLicenca)
        {
            constDiasLicencaOffline = intDiasLicencaOffline;
            constDiasAvisoExpLicenca = intDiasAvisoExpLicenca;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool GetVolumeInformation(string rootPathName,
                                                       StringBuilder volumeNameBuffer,
                                                       int volumeNameSize,
                                                       out uint volumeSerialNumber,
                                                       out uint maximumComponentLength,
                                                       out uint fileSystemFlags,
                                                       StringBuilder fileSystemNameBuffer,
                                                       int nFileSystemNameSize);

        private /*static*/ DateTime Epoch2date(int epoch)
        {
            return GetEpochMinDate().AddSeconds(epoch);
        }

        private /*static*/ int Date2epoch(DateTime date)
        {
            return Convert.ToInt32((date - GetEpochMinDate()).TotalSeconds);
        }

        private /*static*/ DateTime GetEpochMinDate()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public /*static*/ string GerarSerial(string serialHexNumberFromDisk)
        {
            log.Debug($"Gerando Serial da HD: {serialHexNumberFromDisk}");
            string a, b, C, D, E, f, G, H, Dia, Mes, Ano;

            a = (serialHexNumberFromDisk.Length >= 1) ? serialHexNumberFromDisk.Substring(0, 1) : "X";
            b = (serialHexNumberFromDisk.Length >= 2) ? serialHexNumberFromDisk.Substring(1, 1) : "X";
            C = (serialHexNumberFromDisk.Length >= 3) ? serialHexNumberFromDisk.Substring(2, 1) : "X";
            D = (serialHexNumberFromDisk.Length >= 4) ? serialHexNumberFromDisk.Substring(3, 1) : "X";
            E = (serialHexNumberFromDisk.Length >= 5) ? serialHexNumberFromDisk.Substring(4, 1) : "X";
            f = (serialHexNumberFromDisk.Length >= 6) ? serialHexNumberFromDisk.Substring(5, 1) : "X";
            G = (serialHexNumberFromDisk.Length >= 7) ? serialHexNumberFromDisk.Substring(6, 1) : "X";
            H = (serialHexNumberFromDisk.Length >= 8) ? serialHexNumberFromDisk.Substring(7, 1) : "X";

            Dia = (DateTime.Today.Day * 3 - 1).ToString();
            //Mes = DateTime.Today.Month * 2 + 1 * Dia + Mes;
            Mes = (DateTime.Today.Month * 2 + 1 * Convert.ToInt32(Dia)).ToString();
            Ano = (DateTime.Today.DayOfYear + 11 * Convert.ToInt32(Dia) - Convert.ToInt32(Mes)).ToString();

            int dummy = 0;
            if (int.TryParse(a, out dummy)) { a = ((Convert.ToInt32(a) * 2) + 5).ToString(); } else { a = "S"; };
            if (int.TryParse(b, out dummy)) { b = ((Convert.ToInt32(b) * 3) - 1 + 3 * 2 + 5).ToString(); } else { b = "X5"; };
            if (int.TryParse(C, out dummy)) { C = ((Convert.ToInt32(C) * 5) - 1 + 3 * 3 + 6).ToString(); } else { C = "HW"; };
            if (int.TryParse(D, out dummy)) { D = ((Convert.ToInt32(D) * 6) - 1 + 3 * 4 + 4).ToString(); } else { D = "BS"; };
            if (int.TryParse(E, out dummy)) { E = ((Convert.ToInt32(E) * 4) - 1 + 3 * 5 + 9).ToString(); } else { E = "67"; };
            if (int.TryParse(f, out dummy)) { f = ((Convert.ToInt32(f) * 9) - 1 + 3 * 6 + 5).ToString(); } else { f = "T3"; };
            if (int.TryParse(G, out dummy)) { G = ((Convert.ToInt32(G) * 5) - 1 + 3 * 7 + 8).ToString(); } else { G = "J8"; };
            if (int.TryParse(H, out dummy)) { H = ((Convert.ToInt32(H) * 8) - 1 + 3 * 8 + 2).ToString(); } else { H = "Z1"; };

            return (a + b + C + Dia + D + E + Ano + f + G + Mes + H);
        }

        internal /*static*/ int GetDiasRestantes()
        {
            GetValidationInfo(out DateTime dummy1, out DateTime dtmUnikey, out string dummy2);
            return Convert.ToInt32(constDiasLicencaOffline - (DateTime.Today - dtmUnikey).TotalDays);
        }

        public /*static*/ string GetSerialHexNumberFromExecDisk()
        {
            //uint uintSerialNum, uintDummy1, uintDummy2;
            GetVolumeInformation(Path.GetPathRoot(Environment.CurrentDirectory), null, 0, out uint uintSerialNum, out uint uintDummy1, out uint uintDummy2, null, 0);
            return uintSerialNum.ToString("X");
        }

        public /*static*/ bool GetValidationInfo(out DateTime vDtLastLog, out DateTime vDtUnikey, out string strDiskSerialGravado)
        {
            bool retorno = false;

            DateTime dtmEpochMinDate = GetEpochMinDate();

            vDtLastLog = dtmEpochMinDate;
            vDtUnikey = dtmEpochMinDate;
            strDiskSerialGravado = string.Empty;// GetSerialHexNumberFromExecDisk();

            try
            {
                using (var dtValidOff = new FDBDataSet.TRI_PDV_VALID_OFFLINEDataTable())
                using (var taValidOffPdv = new FDBDataSetTableAdapters.TRI_PDV_VALID_OFFLINETableAdapter())
                {
                    taValidOffPdv.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath).ToString();

                    taValidOffPdv.Fill(dtValidOff);

                    if (dtValidOff?.Rows?.Count > 0)
                    {
                        vDtLastLog = dtValidOff?.Rows[0]["LASTLOG"] == null ? dtmEpochMinDate : Epoch2date((int)dtValidOff.Rows[0]["LASTLOG"]);
                        vDtUnikey = dtValidOff?.Rows[0]["UNIKEY"] == null ? dtmEpochMinDate : Epoch2date((int)dtValidOff.Rows[0]["UNIKEY"]);
                        strDiskSerialGravado = dtValidOff.Rows[0]["UNIKEYHD"] == null ? string.Empty : (string)dtValidOff.Rows[0]["UNIKEYHD"];
                    }
                    else
                    {
                        #region Insere um registro dummy em TRI_PDV_VALID_OFFLINE

                        taValidOffPdv.Insert(Date2epoch(vDtLastLog), Date2epoch(vDtUnikey), strDiskSerialGravado);

                        #endregion Insere um registro dummy em TRI_PDV_VALID_OFFLINE
                    }
                }

                retorno = true;
            }
            catch (Exception ex)
            {
                log.Error("Erro ao validar offline", ex);
                retorno = false;
            }

            return retorno;
        }

        internal /*static*/ void VerificarLicencaOffline()
        {
            DateTime dtmEpochMinDate = GetEpochMinDate();

            try
            {
                if (!GetValidationInfo(out DateTime vDtLastLog, out DateTime vData, out string strDiskSerialGravado)) { FecharAplicacao(); return; }

                if (vData == dtmEpochMinDate)
                {
                    vData = DateTime.Today;

                    using var taValidOffPdv = new FDBDataSetTableAdapters.TRI_PDV_VALID_OFFLINETableAdapter();
                    taValidOffPdv.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath).ToString();

                    taValidOffPdv.UpdateUnikey(Date2epoch(vData));
                }

                if (vData > DateTime.Today)
                {
                    vData = DateTime.Today;
                    using var taValidOffPdv = new FDBDataSetTableAdapters.TRI_PDV_VALID_OFFLINETableAdapter();
                    taValidOffPdv.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath).ToString();

                    taValidOffPdv.UpdateUnikey(Date2epoch(vData));
                }

                string strSerialHexFromDisk = GetSerialHexNumberFromExecDisk();
                if (strSerialHexFromDisk != strDiskSerialGravado)
                {
                    AplicarValidacaoOffline(DateTime.Today, strSerialHexFromDisk);
                }
                else
                {
                    if ((DateTime.Today - vData).TotalDays >= constDiasLicencaOffline)
                    {
                        AplicarValidacaoOffline(DateTime.Today, strDiskSerialGravado);
                    }
                    else
                    {
                        if ((DateTime.Today - vData).TotalDays >= (constDiasLicencaOffline - constDiasAvisoExpLicenca))
                        {
                            //vDecisaoValidacao = false;
                            //frmAvisoValidacao.Show();
                            //if (vDecisaoValidacao)
                            if ((new SerialOfflineAviso()).ShowDialog() == true)
                            {
                                AplicarValidacaoOffline(DateTime.Today, strDiskSerialGravado, true);
                            }
                        }
                    }
                }

                // Verificar se o usuário alterou a data do PC para burlar o controle de licença:
                if (vDtLastLog > DateTime.Today)
                {
                    DialogBox.Show("Licença de uso", DialogBoxButtons.No, DialogBoxIcons.Warn, true, "Houve um erro ao verificar a data e hora do sistema operacional", "Por favor verifique se o relógio do computador está ajustado e correto");
                    using (var taValidOffPdv = new FDBDataSetTableAdapters.TRI_PDV_VALID_OFFLINETableAdapter())
                    {
                        taValidOffPdv.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath).ToString();

                        taValidOffPdv.UpdateLastLog(Date2epoch(GetEpochMinDate()));
                        taValidOffPdv.UpdateUnikey(Date2epoch(GetEpochMinDate()));
                        taValidOffPdv.UpdateUnikeyHd(string.Empty);
                    }

                    // Chama o método recursivamente:
                    VerificarLicencaOffline();
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao verificar licenca offline", ex);
                MessageBox.Show("Erro ao validar licença de uso! \n\nO aplicativo será encerrado.");
                FecharAplicacao();
            }
        }

        private /*static*/ void FecharAplicacao()
        {
            Application.Current.Shutdown();
        }

        private /*static*/ void AplicarValidacaoOffline(DateTime dtmDataValid, string strDiskSerial, bool blnJaFoiValidado = false)
        {
            if (blnJaFoiValidado || (new SerialOffline()).ShowDialog() == true)
            {
                using var taValidOffPdv = new FDBDataSetTableAdapters.TRI_PDV_VALID_OFFLINETableAdapter();
                taValidOffPdv.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath).ToString();

                taValidOffPdv.UpdateUnikey(Date2epoch(dtmDataValid));
                taValidOffPdv.UpdateUnikeyHd(strDiskSerial);
            }
            else
            {
                DialogBox.Show("Licença de Uso", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Chave de ativação inválida, por favor verifique a digitação.");
                FecharAplicacao();
            }
        }

        internal /*static*/ void SetLastLog()
        {
            using var dtValidOff = new FDBDataSet.TRI_PDV_VALID_OFFLINEDataTable();
            using var taValidOffPdv = new FDBDataSetTableAdapters.TRI_PDV_VALID_OFFLINETableAdapter();
            taValidOffPdv.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath).ToString();

            taValidOffPdv.Fill(dtValidOff);

            if (dtValidOff?.Rows?.Count > 0)
            {
                #region Update LastLog

                taValidOffPdv.UpdateLastLog(Date2epoch(DateTime.Today));

                #endregion Update LastLog
            }
            else
            {
                #region Insere um registro dummy em TRI_PDV_VALID_OFFLINE

                taValidOffPdv.Insert(Date2epoch(DateTime.Today), Date2epoch(GetEpochMinDate()), string.Empty);

                #endregion Insere um registro dummy em TRI_PDV_VALID_OFFLINE
            }
        }
    }

    public class LicencaDeUsoOnline
    {
        Logger log = new Logger("Licenciamento online");
        public void VerificarLicencaOnline(string serial)
        {
            using var VALIDA_TA = new FDBDataSetTableAdapters.TRI_PDV_VALID_ONLINETableAdapter();
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            VALIDA_TA.Connection = LOCAL_FB_CONN;
            int tentativas = 0;

            //// Comentar a condição seguinte:
            //if (!(String.IsNullOrWhiteSpace(serial)))
            //{
            //    //MessageBox.Show("serial not defined or whitespace");
            //    return;
            //}
            string[] resultado = VerificarSerialOnline(serial);
            if (resultado[0] == "100")
            {
                //Passou na validação. Vai filhão!
                VALIDA_TA.FezLogin(DateTime.Today, DateTime.Today, serial);
            }
            else if (resultado[0] == "200")
            {
                DialogBox.Show("Modo de testes", DialogBoxButtons.No, DialogBoxIcons.Dolan, false, "Programa em ambiente de testes. Não publicar esta base de dados.");
                if (!(new SenhaTecnico()).ShowDialog() == true)
                {
                    Application.Current.Shutdown();
                }
            }
            else if (resultado[0].StartsWith("-5"))
            {
                switch (resultado[0])
                {
                    case "-500":
                        VALIDA_TA.SoChecou(DateTime.Today, serial);
                        return;
                    case "-520":
                        DialogBox.Show("Falha na ativação do produto.", DialogBoxButtons.No, DialogBoxIcons.Error, false, resultado[1]);
                        VALIDA_TA.SoChecou(DateTime.Today, serial);
                        return;
                    case "-510":
                        DialogBox.Show("Falha na ativação do produto.", DialogBoxButtons.No, DialogBoxIcons.Error, false, resultado[1]);
                        VALIDA_TA.SoChecou(DateTime.Today, serial);
                        Application.Current.Shutdown();
                        return;
                }
            }
            else if (resultado[0].EndsWith("5"))
            {
                VALIDA_TA.SoChecou(DateTime.Today, serial);
                DialogBox.Show("Falha na ativação", DialogBoxButtons.Yes, DialogBoxIcons.None, false, resultado[1], resultado[2]);
                Application.Current.Shutdown();
            }
            else if (resultado[0] == "-999")
            {
                new LicencaDeUsoOffline(7, 0).VerificarLicencaOffline();
            }

            else
            {
                //Não passou na validaçao
                VALIDA_TA.SoChecou(DateTime.Today, serial);
                DialogBox.Show("Ativação do produto.", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Não foi possível validar a sua licença.", resultado[1]);
                if (resultado[0] == "-100")
                { VALIDA_TA.FezLogin(DateTime.Today, DateTime.Today, serial); }
                else if (resultado[0] == "-700")
                {

                    while (resultado[0] == "-700" && tentativas < 4)
                    {
                        tentativas += 1;
                        PedeSerial();
                        serial = ChecarSerialExiste();
                        VerificarLicencaOnline(serial);
                        return;
                    }
                    if (tentativas == 4)
                    {
                        MessageBox.Show("Número de tentativas excedido.");
                        Application.Current.Shutdown();
                    }
                }
                else Application.Current.Shutdown();
            }
        }
        /*
 * Códigos de erro:
 * Primeiro dígito:
 *      1 - Vencimento
 *      2 - Bloqueio manual
 *      3 - Pendência
 *      4 - Inativo
 *      5 - Falha de conexão
 *      6 - Cadastro incorreto
 *      7 - Cadastro inexistente
 *      9 - Desconhecido
 * Segundo dígito é o erro específico.
 * Terceiro dígito:
 *      0 - Sem mensagem (não existe resultado[2])
 *      5 - Com mensagem (existe resultado[2])
 *      9 - Desconhecido
 *      
*/
        /// <summary>
        /// Retorna situação da Licença
        /// </summary>
        /// <param name="_serial"></param>
        /// <returns></returns>
        private string[] VerificarSerialOnline(string _serial)
        {
            if (_serial == "DRANGELAZIEGLERr")
            {
                var result = new string[3];
                result[0] = "200";
                result[1] = "";
                result[2] = "";
                return result;
            }
            using var EMITENTE_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EMITENTETableAdapter();
            using var EMITENTE_DT = new DataSets.FDBDataSetOperSeed.TB_EMITENTEDataTable();
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
            {
                EMITENTE_TA.Connection = LOCAL_FB_CONN;
                try
                {
                    EMITENTE_TA.Fill(EMITENTE_DT);
                }
                catch (Exception ex)
                {
                    log.Error("Falha ao preencher informações de emitente", ex);
                    MessageBox.Show("Erro ao consultar dados do emitente. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                    Environment.Exit(0);
                    return null;
                }

                var result = new string[3];

                var client = new RestClient("http://ambisoft.com.br/api/ambiserials_old/ambiserials/");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                JSONRequestBody requestBody = new JSONRequestBody()
                {
                    serial = _serial,
                    documento =  EMITENTE_DT.Rows[0]["CNPJ"].ToString().TiraPont()
                };
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(requestBody);
                IRestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    SerialValidoJSON resposta = ((JArray)JsonConvert.DeserializeObject(response.Content)).First.ToObject<SerialValidoJSON>();
                    if (DateTime.Parse(resposta.VALIDADE).AddDays(resposta.TOLERANCIA.Safeint()) < DateTime.Today)
                    {
                        result[0] = "-100";
                        result[1] = "";
                        result[2] = "";
                        return result;
                    }
                    switch (resposta.STATUS)
                    {
                        case "P":
                            result[0] = "-305";
                            result[1] = "";
                            result[2] = $"{resposta.MOTIV_BLOQUEIO}";
                            break;
                        case "I":
                            result[0] = "-400";
                            result[1] = "";
                            result[2] = $"";
                            break;
                        case "B":
                            result[0] = "-200";
                            result[1] = "";
                            result[2] = $"{resposta.MOTIV_BLOQUEIO}";
                            break;
                        case "A":
                            result[0] = "100";
                            result[1] = "";
                            result[2] = "";
                            break;
                        default:
                            break;
                    }
                    return result;
                }
                else
                {
                    result[0] = "-500";
                    result[1] = "";
                    result[2] = "";
                    return result;
                }
            }

        }
        private int ValidacaoOnlineContingencia()
        {
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var VALIDONLINE_TA = new FDBDataSetTableAdapters.TRI_PDV_VALID_ONLINETableAdapter
            {
                Connection = LOCAL_FB_CONN
            };
            DateTime lastValidCheck = VALIDONLINE_TA.BuscaSerialLocal()[0].LASTVALIDCHECK;
            string serial = VALIDONLINE_TA.BuscaSerialLocal()[0].PK_SERIAL;
            int dias = DateTime.Now.Subtract(lastValidCheck).Days;
            if (dias > 14)
            {
                return -100;
            }
            if (dias > 7)
            {
                return (14 - dias);
            }
            else return 100;
        }

        public bool PedeSerial()
        {
            return (bool)((new perguntaSerial()).ShowDialog());
        }

        public string ChecarSerialExiste()
        {
            using var Serial_TA = new FDBDataSetTableAdapters.TRI_PDV_VALID_ONLINETableAdapter();
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            Serial_TA.Connection = LOCAL_FB_CONN;
            string serial = "NaSK";
            if (Serial_TA.BuscaSerialLocal().Count == 0)
            {
                return serial;
            }
            try
            {
                serial = Serial_TA.BuscaSerialLocal()[0].PK_SERIAL;
            }
            catch (Exception ex)
            {
                log.Error("Erro ao buscar serial local", ex);
                return "Error";
                throw;
            }
            return serial;
        }

    }

    public class JSONRequestBody
    {
        public string serial { get; set; }
        public string documento { get; set; }
    }
    public class SerialValidoJSON
    {
        public string PK_SERIAL { get; set; }
        public string CNPJ_CPF { get; set; }
        public string RAZAO_SOCIAL { get; set; }
        public string STATUS { get; set; }
        public string VALIDADE { get; set; }
        public string TOLERANCIA { get; set; }
        public string MOTIV_BLOQUEIO { get; set; }
        public string NUM_TERMINAIS { get; set; }
        public string OBSERVACOES { get; set; }
        public string ULTIMA_VALID { get; set; }
        public string VERSAO_PDV { get; set; }
        public string createdAt { get; set; }
    }

    public class SerialInvalidoJSON
    {
        public string error { get; set; }
    }
}
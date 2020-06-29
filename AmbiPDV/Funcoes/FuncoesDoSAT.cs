using System;
using System.Runtime.InteropServices;

namespace SAT
{
    public class SATFuncoes
    {
        #region DLLImports
        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr AtivarSAT(int numeroSessao, int subComando, string codigoDeAtivacao, string CNPJ, int cUF);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ComunicarCertificadoICPBRASIL(int numeroSessao, string codigoDeAtivacao, string certificado);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ConsultarSAT(int numeroSessao);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr TesteFimAFim(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ConsultarNumeroSessao(int numeroSessao, string codigoDeAtivacao, int cNumeroDeSessao);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ConfigurarInterfaceDeRede(int numeroSessao, string codigoDeAtivacao, string dadosConfiguracao);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr AssociarAssinatura(int numeroSessao, string codigoDeAtivacao, string CNPJvalue, string assinaturaCNPJs);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr AtualizarSoftwareSAT(int numeroSessao, string codigoDeAtivacao);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr BloquearSAT(int numeroSessao, string codigoDeAtivacao);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DesbloquearSAT(int numeroSessao, string codigoDeAtivacao);

        [DllImport("C:\\SAT\\SAT.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr TrocarCodigoDeAtivacao(int numeroSessao, string codigoDeAtivacao, int opcao, string novoCodigo, string confNovoCodigo);
        #endregion
        /// <summary>
        /// Essa função permite ativar o SAT.
        /// </summary>
        /// <param name="subCom">Subcomando - 1 para AC-SAT/SEFAZ, 2 para ICP-BRASIL, 3 para renovação ICP-BRASIL</param>
        /// <param name="codAtiv">Código de ativação do SAT. Fornecido pelo usuário.</param>
        /// <param name="CNPJ">CNPJ do contribuinte. Somente números.</param>
        /// <param name="cUF">Código do Estado da Federação, segundo tabela do IBGE, onde o SAT será ativado. SP = 35.</param>
        /// <returns></returns>        
        public static string[] ActivateSAT(int subCom, string codAtiv, string CNPJ, int cUF)
        {
            try
            {
                string pont = Marshal.PtrToStringAnsi(AtivarSAT(GerarNumSessao(), subCom, codAtiv, CNPJ, cUF));
                string[] resultados = pont.Split('|');
                return resultados;
            }
            catch (Exception)
            {
                throw new ErrodeSAT("Erro ao ativar o SAT.");
            }
        }
        /// <summary>
        /// Essa função permite enviar o certificado ICP-BRASIL para o SAT.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <param name="certificado">Certificado Digital criado pela Autoridade Certificadora – ICPBrasil</param>
        /// <returns></returns>
        public static string[] ComunicateCertificate(string codAtiv, string certificado)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(ComunicarCertificadoICPBRASIL(GerarNumSessao(), codAtiv, certificado)).Split('|');
                return resultados;
            }
            catch (Exception)
            {
                throw new ErrodeSAT("Erro ao comunicar o certificado ICP-BRASIL.");
            }
        }
        /// <summary>
        /// Esta função faz parte do processo de envio dos dados de venda do AC para o Equipamento SAT.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <param name="dadosVenda">Refere-se aos dados de venda gerados pelo AC e utilizados para compor o CF-e-SAT. (2.1.4) </param>
        /// <returns></returns>
        public static string[] SendSaleData(string codAtiv, string dadosVenda)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(EnviarDadosVenda(GerarNumSessao(), codAtiv, dadosVenda)).Split('|');
                return resultados;
            }
            catch (Exception)
            {
                throw new ErrodeSAT("Erro ao enviar os dados para a venda.");
            }
        }
        /// <summary>
        /// Essa função permite o cancelamento da última venda feita, dentro do período de 30 minutos.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <param name="chave">Chave de acesso do CF-e-SAT a ser cancelado precedida do literal ‘CFe’ (vide 4.7)</param>
        /// <param name="dados">Refere-se aos dados da venda gerados pelo AC e utilizados para compor o CF-e-SAT de cancelamento (vide 4.2.3)</param>
        /// <returns></returns>
        public static string[] CancelLastSale(string codAtiv, string chave, string dados)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(CancelarUltimaVenda(GerarNumSessao(), codAtiv, chave, dados)).Split('|');
                return resultados;
            }
            catch (Exception)
            {
                throw new ErrodeSAT("Erro ao cancelar a última venda.");
            }
        }
        /// <summary>
        /// Esta função é usada para testes de comunicação entre o AC e o Equipamento SAT.
        /// </summary>
        /// <returns></returns>
        public static string[] QuerySAT()
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(ConsultarSAT(GerarNumSessao())).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao Consultar SAT.");
            }
        }
        /// <summary>
        /// Esta função consiste em um teste de comunicação entre o AC, o Equipamento SAT e a SEFAZ.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <param name="dadosVenda">Refere-se aos dados de venda fictícios gerados pelo AC e utilizados para compor o CF-e-SAT de teste. (vide 2.1.4).</param>
        /// <returns></returns>
        public static string[] End2End(string codAtiv, string dadosVenda)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(TesteFimAFim(GerarNumSessao(), codAtiv, dadosVenda)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao executar o Teste Fim a Fim.");
            }
        }
        /// <summary>
        /// Essa função é responsável por verificar a situação de funcionamento do Equipamento SAT.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <returns></returns>
        public static string[] QueryStatus(string codAtiv)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(ConsultarStatusOperacional(GerarNumSessao(), codAtiv)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao obter o status operacional do SAT.");
            }
        }
        /// <summary>
        /// Essa função permite consultar o andamento de uma outra solicitação, de acordo com o número da sessão.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <param name="sessaoconsultada">Número de sessão a ser consultado no SAT-CF-e</param>
        /// <returns></returns>
        public static string[] QuerySession(string codAtiv, int sessaoconsultada)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(ConsultarNumeroSessao(GerarNumSessao(), codAtiv, sessaoconsultada)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao consultar o número da sessão.");
            }
        }
        /// <summary>
        /// Essa função permite a ativação do SAT, inclusive a configuração inicial do SAT.
        /// </summary>
        /// <param name="codAtiv">Arquivo de configuração no formato XML.</param>
        /// <param name="dadosSetup"></param>
        /// <returns></returns>
        public static string[] SetupNetworkInterface(string codAtiv, string dadosSetup)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(ConfigurarInterfaceDeRede(GerarNumSessao(), codAtiv, dadosSetup)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao configurar a interface de rede.");
            }
        }
        /// <summary>
        /// Essa função associa o SAT com o Aplicativo Comercial.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <param name="CNPJ">CNPJ da empresa desenvolvedora do Aplicativo Comercial + CNPJ do Emitente, seguidos, apenas números.(vide 2.1.3)</param>
        /// <param name="assinaturaCNPJ"></param>
        /// <returns></returns>
        public static string[] LinkSignature(string codAtiv, string CNPJ, string assinaturaCNPJ)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(AssociarAssinatura(GerarNumSessao(), codAtiv, CNPJ, assinaturaCNPJ)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao associar assinatura ao SAT.");
            }
        }
        /// <summary>
        /// Essa função dá início à atualização do SAT.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <returns></returns>
        public static string[] UpdateSAT(string codAtiv)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(AtualizarSoftwareSAT(GerarNumSessao(), codAtiv)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao atualizar o software do SAT.");
            }
        }
        /// <summary>
        /// Essa função extrai, em base64, os logs do aparelho SAT.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <returns></returns>
        public static string[] ExtractLogs(string codAtiv)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(ExtrairLogs(GerarNumSessao(), codAtiv)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao extrair logs do SAT.");
            }
        }
        /// <summary>
        /// Essa função executa o bloqueio operacional do aparelho SAT.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <returns></returns>
        public static string[] LockSAT(string codAtiv)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(BloquearSAT(GerarNumSessao(), codAtiv)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao bloquear o SAT.");
            }
        }
        /// <summary>
        /// Essa função executa o desbloqueio operacional do aparelho SAT.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <returns></returns>
        public static string[] UnlockSAT(string codAtiv)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(DesbloquearSAT(GerarNumSessao(), codAtiv)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao desbloquear o SAT.");
            }
        }
        /// <summary>
        /// Essa função permite a troca do código de ativação do SAT.
        /// </summary>
        /// <param name="codAtiv"></param>
        /// <param name="opcao">1 para código de ativação normal. 2 para código de ativação de emergência.</param>
        /// <param name="novoCodigo">Novo código de ativação escolhido pelo contribuinte.</param>
        /// <param name="confNovoCod">Confirmação do novo código de ativação escolhido pelo contribuinte.</param>
        /// <returns></returns>
        public static string[] ChangeCode(string codAtiv, int opcao, string novoCodigo, string confNovoCod)
        {
            try
            {
                string[] resultados = Marshal.PtrToStringAnsi(TrocarCodigoDeAtivacao(GerarNumSessao(), codAtiv, opcao, novoCodigo, confNovoCod)).Split('|');
                return resultados;

            }
            catch (Exception)
            {

                throw new ErrodeSAT("Erro ao trocar o código de ativação. Código de ativação pode não ter sido trocado.");
            }
        }

        public static Int32 GerarNumSessao()
        {
            return GerarCodigoNumerico(int.Parse(DateTime.Now.ToString("HHmmss")));
        }
        public static Int32 GerarCodigoNumerico(Int32 numeroNF)
        {
            string s;
            Int32 i, j, k;

            // Essa função gera um código numerico atravéz de calculos realizados sobre o parametro numero
            s = numeroNF.ToString("000000");
            for (i = 0; i < 6; ++i)
            {
                k = 0;
                for (j = 0; j < 6; ++j)
                    k += Convert.ToInt32(s[j]) * (j + 1);
                s = (k % 11).ToString().Trim() + s;
            }
            return Convert.ToInt32(s.Substring(0, 6));
        }

    }
    public class ErrodeSAT : Exception
    {
        /// <summary>
        /// Just create the exception
        /// </summary>
        public ErrodeSAT()
        : base()
        {
        }

        /// <summary>
        /// Create the exception with description
        /// </summary>
        /// <param name="message">Falha ao executar um comando no SAT-Fiscal.</param>
        ///
        public ErrodeSAT(String message)
        : base(message)
        {
        }

        /// <summary>
        /// Create the exception with description and inner cause
        /// </summary>
        /// <param name="message">Falha ao executar um comando no SAT-Fiscal.</param>
        /// <param name="innerException">Exception inner cause</param>
        public ErrodeSAT(String message, Exception innerException)
        : base(message, innerException)
        {
        }

        /// <summary>
        /// Create the exception from serialized data.
        /// Usual scenario is when exception is occured somewhere on the remote workstation
        /// and we have to re-create/re-throw the exception on the local machine
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected ErrodeSAT(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
        {
        }
    }

}
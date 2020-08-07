using PDV_WPF.Exceptions;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static PDV_WPF.Funcoes.Statics;


namespace DeclaracoesDllSat
{
    public enum ModeloSAT { NENHUM = -1, DARUMA, DIMEP, BEMATECH, ELGIN, SWEDA, CONTROLID, TANCA, EMULADOR }

    internal class Declaracoes_DllSat
    {
        internal static string sRetorno;

        internal static IntPtr ConsultarSAT(int numeroSessao, ModeloSAT modelo/* = ModeloSAT.NENHUM*/)
        {
            return modelo switch
            {
                ModeloSAT.NENHUM => throw new SATNaoConfigurado(),
                ModeloSAT.DARUMA => throw new NotImplementedException(),
                ModeloSAT.DIMEP => DeclaracoesDIMEP.ConsultarSAT(numeroSessao),
                ModeloSAT.BEMATECH => DeclaracoesBEMATECH.ConsultarSAT(numeroSessao),
                ModeloSAT.ELGIN => DeclaracoesELGINStdCall.ConsultarSAT(numeroSessao),
                ModeloSAT.SWEDA => DeclaracoesSWEDA.ConsultarSAT(numeroSessao),
                ModeloSAT.CONTROLID => DeclaracoesCONTROLID.ConsultarSAT(numeroSessao),
                ModeloSAT.TANCA => DeclaracoesTANCA.ConsultarSAT(numeroSessao),
                ModeloSAT.EMULADOR => DeclaracoesEMULADOR.ConsultarSAT(numeroSessao),
                _ => throw new SATNaoConfigurado(),
            };
            throw new SATNaoConfigurado();
        }

        internal static IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao, ModeloSAT modelo/* = ModeloSAT.NENHUM*/)
        {
            return modelo switch
            {
                ModeloSAT.DARUMA => throw new NotImplementedException(),
                ModeloSAT.DIMEP => DeclaracoesDIMEP.ExtrairLogs(numeroSessao, codigoDeAtivacao),
                ModeloSAT.BEMATECH => DeclaracoesBEMATECH.ExtrairLogs(numeroSessao, codigoDeAtivacao),
                ModeloSAT.ELGIN => DeclaracoesELGINStdCall.ExtrairLogs(numeroSessao, codigoDeAtivacao),
                ModeloSAT.SWEDA => DeclaracoesSWEDA.ExtrairLogs(numeroSessao, codigoDeAtivacao),
                ModeloSAT.CONTROLID => DeclaracoesCONTROLID.ExtrairLogs(numeroSessao, codigoDeAtivacao),
                ModeloSAT.TANCA => DeclaracoesTANCA.ExtrairLogs(numeroSessao, codigoDeAtivacao),
                ModeloSAT.EMULADOR => DeclaracoesEMULADOR.ExtrairLogs(numeroSessao, codigoDeAtivacao),
                _ => throw new NotImplementedException(),
            };
            throw new NotImplementedException();
        }

        internal static IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento, ModeloSAT modelo/* = ModeloSAT.NENHUM*/)
        {
            return modelo switch
            {
                ModeloSAT.DARUMA => throw new NotImplementedException(),
                ModeloSAT.DIMEP => DeclaracoesDIMEP.CancelarUltimaVenda(numeroSessao, codigoDeAtivacao, chave, dadosCancelamento),
                ModeloSAT.BEMATECH => DeclaracoesBEMATECH.CancelarUltimaVenda(numeroSessao, codigoDeAtivacao, chave, dadosCancelamento),
                ModeloSAT.ELGIN => DeclaracoesELGINStdCall.CancelarUltimaVenda(numeroSessao, codigoDeAtivacao, chave, dadosCancelamento),
                ModeloSAT.SWEDA => DeclaracoesSWEDA.CancelarUltimaVenda(numeroSessao, codigoDeAtivacao, chave, dadosCancelamento),
                ModeloSAT.CONTROLID => DeclaracoesCONTROLID.CancelarUltimaVenda(numeroSessao, codigoDeAtivacao, chave, dadosCancelamento),
                ModeloSAT.TANCA => DeclaracoesTANCA.CancelarUltimaVenda(numeroSessao, codigoDeAtivacao, chave, dadosCancelamento),
                ModeloSAT.EMULADOR => DeclaracoesEMULADOR.CancelarUltimaVenda(numeroSessao, codigoDeAtivacao, chave, dadosCancelamento),
                _ => throw new NotImplementedException(),
            };
            throw new NotImplementedException();
        }

        internal static IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda, ModeloSAT modelo /*= ModeloSAT.NENHUM*/)
        {
            return modelo switch
            {
                ModeloSAT.DARUMA => throw new NotImplementedException(),
                ModeloSAT.DIMEP => DeclaracoesDIMEP.EnviarDadosVenda(numeroSessao, codigoDeAtivacao, dadosVenda),
                ModeloSAT.BEMATECH => DeclaracoesBEMATECH.EnviarDadosVenda(numeroSessao, codigoDeAtivacao, dadosVenda),
                ModeloSAT.ELGIN => DeclaracoesELGINStdCall.EnviarDadosVenda(numeroSessao, codigoDeAtivacao, dadosVenda),
                ModeloSAT.SWEDA => DeclaracoesSWEDA.EnviarDadosVenda(numeroSessao, codigoDeAtivacao, dadosVenda),
                ModeloSAT.CONTROLID => DeclaracoesCONTROLID.EnviarDadosVenda(numeroSessao, codigoDeAtivacao, dadosVenda),
                ModeloSAT.TANCA => DeclaracoesTANCA.EnviarDadosVenda(numeroSessao, codigoDeAtivacao, dadosVenda),
                ModeloSAT.EMULADOR => DeclaracoesEMULADOR.EnviarDadosVenda(numeroSessao, codigoDeAtivacao, dadosVenda),
                _ => throw new NotImplementedException(),
            };
            throw new NotImplementedException();
        }

        internal static IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao, ModeloSAT modelo/* = ModeloSAT.NENHUM*/)
        {
            return modelo switch
            {
                ModeloSAT.DARUMA => throw new NotImplementedException(),
                ModeloSAT.DIMEP => DeclaracoesDIMEP.ConsultarStatusOperacional(numeroSessao, codigoDeAtivacao),
                ModeloSAT.BEMATECH => DeclaracoesBEMATECH.ConsultarStatusOperacional(numeroSessao, codigoDeAtivacao),
                ModeloSAT.ELGIN => DeclaracoesELGINStdCall.ConsultarStatusOperacional(numeroSessao, codigoDeAtivacao),
                ModeloSAT.SWEDA => DeclaracoesSWEDA.ConsultarStatusOperacional(numeroSessao, codigoDeAtivacao),
                ModeloSAT.CONTROLID => DeclaracoesCONTROLID.ConsultarStatusOperacional(numeroSessao, codigoDeAtivacao),
                ModeloSAT.TANCA => DeclaracoesTANCA.ConsultarStatusOperacional(numeroSessao, codigoDeAtivacao),
                ModeloSAT.EMULADOR => DeclaracoesEMULADOR.ConsultarStatusOperacional(numeroSessao, codigoDeAtivacao),
                _ => throw new NotImplementedException(),
            };
            throw new NotImplementedException();
        }

    }

    internal /*unsafe*/ class DeclaracoesDIMEP
    {
        internal static string sRetorno;


        // [DllImport("libxml2.dll")]
        /**AtivarSAT
         * Metodo para ativar o uso do SAT
         * @param subComando
         * @param codigoDeAtivacao
         * @param CNPJ
         * @param cUF
         * @return CSR
         * DLLIMPORT char* AtivarSAT(int numeroSessao, int subComando, char *codigoDeAtivacao, char *CNPJ, int cUF);
         **/
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]

#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AtivarSAT(int numeroSessao, int subComando, string codigoDeAtivacao, string CNPJ, int cUF);

        /** ComunicarCertificadoICPBRASIL
        * @brief Comunica o certificado icp Brasil
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param Certificado
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ComunicarCertificadoICPBRASIL(int numeroSessao, char *codigoDeAtivacao, char *certificado);
        **/
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ComunicarCertificadoICPBRASIL(int numeroSessao, string codigoDeAtivacao, string certificado);

        /**EnviarDadosVenda
        * @brief Responsavel pelo comando de envio de dados de vendas
        * @param codigoDeAtivacao
        * @param numeroSessao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* EnviarDadosVenda(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
        */
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**CancelarUltimaVenda
         * @name CanclearUltimaVenda
         * @brief cancela o ultimo cupom fiscal
         * @param codigoDeAtivacao
         * @param chave
         * @param dadosCancelamento
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* CancelarUltimaVenda(int numeroSessao, char *codigoDeAtivacao, char *chave, char *dadosCancelamento);
         */
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento);

        /**ConsultarSAT
         * @name ConsultarSAT
         * @brief consultar SAT
         * @param numeroSessao
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* ConsultarSAT(int numeroSessao);
         */
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall, ThrowOnUnmappableChar = true)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall, ThrowOnUnmappableChar = true)]
#endif
        internal static extern IntPtr ConsultarSAT(int numeroSessao);

        /**TesteFimAFim
          *  @name TesteFimAFim
          *  @brief Esta funcao consiste em um teste de comunicacao entre o AC, o Equipamento SAT e a SEFAZ
          *  @param numeroSessao
          *  @param codigoAtivacao
          *  @param dadosVenda
          *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
          *  DLLIMPORT char* TesteFimAFim(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
          */
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr TesteFimAFim(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**ConsultarStatusOperacional
        *  @brief Essa funcao responsavel por verificar a situacao de funcionamento do Equipamento SAT
        *  @param numeroSessao
        *  @param codigoAtivacao
        *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        *  DLLIMPORT char* ConsultarStatusOperacional(int numeroSessao, char *codigoDeAtivacao);
        */
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao);

        /**ConsultarNumeroSessao
        * @brief O AC podera verificar se a Ultima sessao requisitada foi processada em caso de nao
        * @brief recebimento do retorno da operacao. O equipamento SAT-CF-e retornara exatamente o resultado da
        * @brief sessao consultada
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConsultarNumeroSessao(int numeroSessao, char *codigoDeAtivacao, int cNumeroDeSessao);
        */
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarNumeroSessao(int numeroSessao, string codigoDeAtivacao, int cNumeroDeSessao);

        /**ConfigurarInterfaceDeRede
        * @brief Responsavel pela configuracao da interface de rede do SAT (Ver espec:2.6.10)
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConfigurarInterfaceDeRede(int numeroSessao, char *codigoDeAtivacao, char *dadosConfiguracao);
        */
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConfigurarInterfaceDeRede(int numeroSessao, string codigoDeAtivacao, string dadosConfiguracao);

        /**AssociarAssinatura
        * @brief Responsavel pelo comando de associar o AC ao SAT
        * @param numeroSessao
        * @param CodigoAtivacao
        * @param CNPJ
        * @param assinaturaCNPJs
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* AssociarAssinatura(int numeroSessao, char *codigoDeAtivacao, char *CNPJvalue, char *assinaturaCNPJs);
        */
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AssociarAssinatura(int numeroSessao, string codigoDeAtivacao, string CNPJvalue, string assinaturaCNPJs);

        /**ExtrairLogs
        * @name ExtrairLogs
        * @brief O Aplicativo Comercial poderÃ¡ extrair os arquivos de registro do
        * @brief Equipamento SAT por meio da funÃ§Ã£o ExtrairLogs
        * @param numeroSessao
        * @param codigoAtivacao
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ExtrairLogs(int numeroSessao, char *codigoDeAtivacao);
        */
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao);

        /**BloquearSAT
        * @brief O Aplicativo Comercial ou outro software fornecido pelo Fabricante podera
        * @brief realizar o bloqueio operacional do Equipamento SAT
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* BloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        */
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr BloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* DesbloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr DesbloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //TrocarCodigoDeAtivacao
        //O Aplicativo Comercial ou outro software fornecido pelo Fabricante
        //poderá realizar a troca do codigo de ativacao a qualquer momento
        //DLLIMPORT char* TrocarCodigoDeAtivacao(int numeroSessao, char *codigoDeAtivacao, int opcao, char *novoCodigo, char *confNovoCodigo);
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr TrocarCodigoDeAtivacao(int numeroSessao, string codigoDeAtivacao, int opcao, string novoCodigo, string confNovoCodigo);

        //DLLIMPORT char* AtualizarSoftwareSAT(int numeroSessao, char *codigoDeAtivacao);
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AtualizarSoftwareSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* GetMapaPortasSAT(char  codatv);
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern String GetMapaPortasSAT(string codigoDeAtivacao);

        //DLLIMPORT char* GetPortaSAT(char serial, int sessao, char * atv);
        //[DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#if ANYCPU
        [DllImport(@"DLL\dllsat_x32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\dllsat_x64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern String GetPortaSAT(string numeroSerie, int numeroSessao, string codigoDeAtivacao);


        //Tratamento de Retornos SAT
        internal static void Trata_RetornoSAT(string Retorno)
        {

            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[2];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);
            MessageBox.Show(String.Format("Retorno SAT: {0} {1}", CodRetornoSAT + " - ", RetornoSAT), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        internal static void Trata_Alerta(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[3];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);

            var CodAlerta = retornoSplit[2];

            var CodMsgSefaz = retornoSplit[4];
            var MsgSefaz = retornoSplit[5];
            var chars2 = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars2);

            MessageBox.Show(String.Format("Retorno SAT: {0}{1} \nAlerta: {2} \nRetorno Sefaz: {3} {4} ",
            CodRetornoSAT + " - ", RetornoSAT, CodAlerta, CodMsgSefaz + " - ", MsgSefaz),
            "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Trata_MsgSefaz(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodMsgSefaz = retornoSplit[3];
            var MsgSefaz = retornoSplit[4];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars);

            MessageBox.Show(String.Format("Retorno Sefaz: {0} {1}", CodMsgSefaz + " - ", MsgSefaz), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




    }

    internal /*unsafe*/ class DeclaracoesBEMATECH
    {
        internal static string sRetorno;

        /**AtivarSAT
         * Metodo para ativar o uso do SAT
         * @param subComando
         * @param codigoDeAtivacao
         * @param CNPJ
         * @param cUF
         * @return CSR
         * DLLIMPORT char* AtivarSAT(int numeroSessao, int subComando, char *codigoDeAtivacao, char *CNPJ, int cUF);
         **/
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AtivarSAT(int numeroSessao, int subComando, string codigoDeAtivacao, string CNPJ, int cUF);

        /** ComunicarCertificadoICPBRASIL
        * @brief Comunica o certificado icp Brasil
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param Certificado
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ComunicarCertificadoICPBRASIL(int numeroSessao, char *codigoDeAtivacao, char *certificado);
        **/
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ComunicarCertificadoICPBRASIL(int numeroSessao, string codigoDeAtivacao, string certificado);

        /**EnviarDadosVenda
        * @brief Responsavel pelo comando de envio de dados de vendas
        * @param codigoDeAtivacao
        * @param numeroSessao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* EnviarDadosVenda(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
        */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**CancelarUltimaVenda
         * @name CanclearUltimaVenda
         * @brief cancela o ultimo cupom fiscal
         * @param codigoDeAtivacao
         * @param chave
         * @param dadosCancelamento
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* CancelarUltimaVenda(int numeroSessao, char *codigoDeAtivacao, char *chave, char *dadosCancelamento);
         */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento);

        /**ConsultarSAT
         * @name ConsultarSAT
         * @brief consultar SAT
         * @param numeroSessao
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* ConsultarSAT(int numeroSessao);
         */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarSAT(int numeroSessao);

        /**TesteFimAFim
          *  @name TesteFimAFim
          *  @brief Esta funcao consiste em um teste de comunicacao entre o AC, o Equipamento SAT e a SEFAZ
          *  @param numeroSessao
          *  @param codigoAtivacao
          *  @param dadosVenda
          *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
          *  DLLIMPORT char* TesteFimAFim(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
          */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr TesteFimAFim(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**ConsultarStatusOperacional
        *  @brief Essa funcao responsavel por verificar a situacao de funcionamento do Equipamento SAT
        *  @param numeroSessao
        *  @param codigoAtivacao
        *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        *  DLLIMPORT char* ConsultarStatusOperacional(int numeroSessao, char *codigoDeAtivacao);
        */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao);

        /**ConsultarNumeroSessao
        * @brief O AC podera verificar se a Ultima sessao requisitada foi processada em caso de nao
        * @brief recebimento do retorno da operacao. O equipamento SAT-CF-e retornara exatamente o resultado da
        * @brief sessao consultada
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConsultarNumeroSessao(int numeroSessao, char *codigoDeAtivacao, int cNumeroDeSessao);
        */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarNumeroSessao(int numeroSessao, string codigoDeAtivacao, int cNumeroDeSessao);

        /**ConfigurarInterfaceDeRede
        * @brief Responsavel pela configuracao da interface de rede do SAT (Ver espec:2.6.10)
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConfigurarInterfaceDeRede(int numeroSessao, char *codigoDeAtivacao, char *dadosConfiguracao);
        */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConfigurarInterfaceDeRede(int numeroSessao, string codigoDeAtivacao, string dadosConfiguracao);

        /**AssociarAssinatura
        * @brief Responsavel pelo comando de associar o AC ao SAT
        * @param numeroSessao
        * @param CodigoAtivacao
        * @param CNPJ
        * @param assinaturaCNPJs
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* AssociarAssinatura(int numeroSessao, char *codigoDeAtivacao, char *CNPJvalue, char *assinaturaCNPJs);
        */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AssociarAssinatura(int numeroSessao, string codigoDeAtivacao, string CNPJvalue, string assinaturaCNPJs);

        /**ExtrairLogs
        * @name ExtrairLogs
        * @brief O Aplicativo Comercial poderÃ¡ extrair os arquivos de registro do
        * @brief Equipamento SAT por meio da funÃ§Ã£o ExtrairLogs
        * @param numeroSessao
        * @param codigoAtivacao
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ExtrairLogs(int numeroSessao, char *codigoDeAtivacao);
        */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao);

        /**BloquearSAT
        * @brief O Aplicativo Comercial ou outro software fornecido pelo Fabricante podera
        * @brief realizar o bloqueio operacional do Equipamento SAT
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* BloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        */
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr BloquearSAT(int numeroSessao, string codigoDeAtivacao);

#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr DesbloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //TrocarCodigoDeAtivacao
        //O Aplicativo Comercial ou outro software fornecido pelo Fabricante
        //poderá realizar a troca do codigo de ativacao a qualquer momento
#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr TrocarCodigoDeAtivacao(int numeroSessao, string codigoDeAtivacao, int opcao, string novoCodigo, string confNovoCodigo);

#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AtualizarSoftwareSAT(int numeroSessao, string codigoDeAtivacao);

#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern String GetMapaPortasSAT(string codigoDeAtivacao);

#if ANYCPU
        [DllImport(@"DLL\BemaSAT32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\BemaSAT64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern String GetPortaSAT(string numeroSerie, int numeroSessao, string codigoDeAtivacao);


        //Tratamento de Retornos SAT
        internal static void Trata_RetornoSAT(string Retorno)
        {

            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[2];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);
            MessageBox.Show(String.Format("Retorno SAT: {0} {1}", CodRetornoSAT + " - ", RetornoSAT), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        internal static void Trata_Alerta(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[3];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);

            var CodAlerta = retornoSplit[2];

            var CodMsgSefaz = retornoSplit[4];
            var MsgSefaz = retornoSplit[5];
            var chars2 = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars2);

            MessageBox.Show(String.Format("Retorno SAT: {0}{1} \nAlerta: {2} \nRetorno Sefaz: {3} {4} ",
            CodRetornoSAT + " - ", RetornoSAT, CodAlerta, CodMsgSefaz + " - ", MsgSefaz),
            "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Trata_MsgSefaz(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodMsgSefaz = retornoSplit[3];
            var MsgSefaz = retornoSplit[4];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars);

            MessageBox.Show(String.Format("Retorno Sefaz: {0} {1}", CodMsgSefaz + " - ", MsgSefaz), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




    }

    internal /*unsafe*/ class DeclaracoesSWEDA
    {
        internal static string sRetorno;

        // [DllImport("libxml2.dll")]
        /**AtivarSAT
         * Metodo para ativar o uso do SAT
         * @param subComando
         * @param codigoDeAtivacao
         * @param CNPJ
         * @param cUF
         * @return CSR
         * DLLIMPORT char* AtivarSAT(int numeroSessao, int subComando, char *codigoDeAtivacao, char *CNPJ, int cUF);
         **/
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AtivarSAT(int numeroSessao, int subComando, string codigoDeAtivacao, string CNPJ, int cUF);

        /** ComunicarCertificadoICPBRASIL
        * @brief Comunica o certificado icp Brasil
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param Certificado
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ComunicarCertificadoICPBRASIL(int numeroSessao, char *codigoDeAtivacao, char *certificado);
        **/
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ComunicarCertificadoICPBRASIL(int numeroSessao, string codigoDeAtivacao, string certificado);

        /**EnviarDadosVenda
        * @brief Responsavel pelo comando de envio de dados de vendas
        * @param codigoDeAtivacao
        * @param numeroSessao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* EnviarDadosVenda(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
        */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**CancelarUltimaVenda
         * @name CanclearUltimaVenda
         * @brief cancela o ultimo cupom fiscal
         * @param codigoDeAtivacao
         * @param chave
         * @param dadosCancelamento
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* CancelarUltimaVenda(int numeroSessao, char *codigoDeAtivacao, char *chave, char *dadosCancelamento);
         */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento);

        /**ConsultarSAT
         * @name ConsultarSAT
         * @brief consultar SAT
         * @param numeroSessao
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* ConsultarSAT(int numeroSessao);
         */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarSAT(int numeroSessao);

        /**TesteFimAFim
          *  @name TesteFimAFim
          *  @brief Esta funcao consiste em um teste de comunicacao entre o AC, o Equipamento SAT e a SEFAZ
          *  @param numeroSessao
          *  @param codigoAtivacao
          *  @param dadosVenda
          *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
          *  DLLIMPORT char* TesteFimAFim(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
          */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr TesteFimAFim(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**ConsultarStatusOperacional
        *  @brief Essa funcao responsavel por verificar a situacao de funcionamento do Equipamento SAT
        *  @param numeroSessao
        *  @param codigoAtivacao
        *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        *  DLLIMPORT char* ConsultarStatusOperacional(int numeroSessao, char *codigoDeAtivacao);
        */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao);

        /**ConsultarNumeroSessao
        * @brief O AC podera verificar se a Ultima sessao requisitada foi processada em caso de nao
        * @brief recebimento do retorno da operacao. O equipamento SAT-CF-e retornara exatamente o resultado da
        * @brief sessao consultada
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConsultarNumeroSessao(int numeroSessao, char *codigoDeAtivacao, int cNumeroDeSessao);
        */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarNumeroSessao(int numeroSessao, string codigoDeAtivacao, int cNumeroDeSessao);

        /**ConfigurarInterfaceDeRede
        * @brief Responsavel pela configuracao da interface de rede do SAT (Ver espec:2.6.10)
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConfigurarInterfaceDeRede(int numeroSessao, char *codigoDeAtivacao, char *dadosConfiguracao);
        */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConfigurarInterfaceDeRede(int numeroSessao, string codigoDeAtivacao, string dadosConfiguracao);

        /**AssociarAssinatura
        * @brief Responsavel pelo comando de associar o AC ao SAT
        * @param numeroSessao
        * @param CodigoAtivacao
        * @param CNPJ
        * @param assinaturaCNPJs
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* AssociarAssinatura(int numeroSessao, char *codigoDeAtivacao, char *CNPJvalue, char *assinaturaCNPJs);
        */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AssociarAssinatura(int numeroSessao, string codigoDeAtivacao, string CNPJvalue, string assinaturaCNPJs);

        /**ExtrairLogs
        * @name ExtrairLogs
        * @brief O Aplicativo Comercial poderÃ¡ extrair os arquivos de registro do
        * @brief Equipamento SAT por meio da funÃ§Ã£o ExtrairLogs
        * @param numeroSessao
        * @param codigoAtivacao
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ExtrairLogs(int numeroSessao, char *codigoDeAtivacao);
        */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao);

        /**BloquearSAT
        * @brief O Aplicativo Comercial ou outro software fornecido pelo Fabricante podera
        * @brief realizar o bloqueio operacional do Equipamento SAT
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* BloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        */
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr BloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* DesbloquearSAT(int numeroSessao, char *codigoDeAtivacao);
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr DesbloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //TrocarCodigoDeAtivacao
        //O Aplicativo Comercial ou outro software fornecido pelo Fabricante
        //poderá realizar a troca do codigo de ativacao a qualquer momento
        //DLLIMPORT char* TrocarCodigoDeAtivacao(int numeroSessao, char *codigoDeAtivacao, int opcao, char *novoCodigo, char *confNovoCodigo);
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr TrocarCodigoDeAtivacao(int numeroSessao, string codigoDeAtivacao, int opcao, string novoCodigo, string confNovoCodigo);

        //DLLIMPORT char* AtualizarSoftwareSAT(int numeroSessao, char *codigoDeAtivacao);
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AtualizarSoftwareSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* GetMapaPortasSAT(char  codatv);
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern String GetMapaPortasSAT(string codigoDeAtivacao);

        //DLLIMPORT char* GetPortaSAT(char serial, int sessao, char * atv);
#if ANYCPU
        [DllImport(@"DLL\SATDLL32.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern String GetPortaSAT(string numeroSerie, int numeroSessao, string codigoDeAtivacao);


        //Tratamento de Retornos SAT
        internal static void Trata_RetornoSAT(string Retorno)
        {

            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[2];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);
            MessageBox.Show(String.Format("Retorno SAT: {0} {1}", CodRetornoSAT + " - ", RetornoSAT), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        internal static void Trata_Alerta(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[3];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);

            var CodAlerta = retornoSplit[2];

            var CodMsgSefaz = retornoSplit[4];
            var MsgSefaz = retornoSplit[5];
            var chars2 = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars2);

            MessageBox.Show(String.Format("Retorno SAT: {0}{1} \nAlerta: {2} \nRetorno Sefaz: {3} {4} ",
            CodRetornoSAT + " - ", RetornoSAT, CodAlerta, CodMsgSefaz + " - ", MsgSefaz),
            "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Trata_MsgSefaz(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodMsgSefaz = retornoSplit[3];
            var MsgSefaz = retornoSplit[4];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars);

            MessageBox.Show(String.Format("Retorno Sefaz: {0} {1}", CodMsgSefaz + " - ", MsgSefaz), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




    }

    internal /*unsafe*/ class DeclaracoesELGINStdCall
    {
        internal static string sRetorno;

        // [DllImport("libxml2.dll")]
        /**AtivarSAT
         * Metodo para ativar o uso do SAT
         * @param subComando
         * @param codigoDeAtivacao
         * @param CNPJ
         * @param cUF
         * @return CSR
         * DLLIMPORT char* AtivarSAT(int numeroSessao, int subComando, char *codigoDeAtivacao, char *CNPJ, int cUF);
         **/
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AtivarSAT(int numeroSessao, int subComando, string codigoDeAtivacao, string CNPJ, int cUF);

        /** ComunicarCertificadoICPBRASIL
        * @brief Comunica o certificado icp Brasil
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param Certificado
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ComunicarCertificadoICPBRASIL(int numeroSessao, char *codigoDeAtivacao, char *certificado);
        **/
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ComunicarCertificadoICPBRASIL(int numeroSessao, string codigoDeAtivacao, string certificado);

        /**EnviarDadosVenda
        * @brief Responsavel pelo comando de envio de dados de vendas
        * @param codigoDeAtivacao
        * @param numeroSessao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* EnviarDadosVenda(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
        */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**CancelarUltimaVenda
         * @name CanclearUltimaVenda
         * @brief cancela o ultimo cupom fiscal
         * @param codigoDeAtivacao
         * @param chave
         * @param dadosCancelamento
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* CancelarUltimaVenda(int numeroSessao, char *codigoDeAtivacao, char *chave, char *dadosCancelamento);
         */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento);

        /**ConsultarSAT
         * @name ConsultarSAT
         * @brief consultar SAT
         * @param numeroSessao
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* ConsultarSAT(int numeroSessao);
         */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarSAT(int numeroSessao);

        /**TesteFimAFim
          *  @name TesteFimAFim
          *  @brief Esta funcao consiste em um teste de comunicacao entre o AC, o Equipamento SAT e a SEFAZ
          *  @param numeroSessao
          *  @param codigoAtivacao
          *  @param dadosVenda
          *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
          *  DLLIMPORT char* TesteFimAFim(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
          */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr TesteFimAFim(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**ConsultarStatusOperacional
        *  @brief Essa funcao responsavel por verificar a situacao de funcionamento do Equipamento SAT
        *  @param numeroSessao
        *  @param codigoAtivacao
        *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        *  DLLIMPORT char* ConsultarStatusOperacional(int numeroSessao, char *codigoDeAtivacao);
        */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao);

        /**ConsultarNumeroSessao
        * @brief O AC podera verificar se a Ultima sessao requisitada foi processada em caso de nao
        * @brief recebimento do retorno da operacao. O equipamento SAT-CF-e retornara exatamente o resultado da
        * @brief sessao consultada
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConsultarNumeroSessao(int numeroSessao, char *codigoDeAtivacao, int cNumeroDeSessao);
        */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConsultarNumeroSessao(int numeroSessao, string codigoDeAtivacao, int cNumeroDeSessao);

        /**ConfigurarInterfaceDeRede
        * @brief Responsavel pela configuracao da interface de rede do SAT (Ver espec:2.6.10)
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConfigurarInterfaceDeRede(int numeroSessao, char *codigoDeAtivacao, char *dadosConfiguracao);
        */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ConfigurarInterfaceDeRede(int numeroSessao, string codigoDeAtivacao, string dadosConfiguracao);

        /**AssociarAssinatura
        * @brief Responsavel pelo comando de associar o AC ao SAT
        * @param numeroSessao
        * @param CodigoAtivacao
        * @param CNPJ
        * @param assinaturaCNPJs
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* AssociarAssinatura(int numeroSessao, char *codigoDeAtivacao, char *CNPJvalue, char *assinaturaCNPJs);
        */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AssociarAssinatura(int numeroSessao, string codigoDeAtivacao, string CNPJvalue, string assinaturaCNPJs);

        /**ExtrairLogs
        * @name ExtrairLogs
        * @brief O Aplicativo Comercial poderÃ¡ extrair os arquivos de registro do
        * @brief Equipamento SAT por meio da funÃ§Ã£o ExtrairLogs
        * @param numeroSessao
        * @param codigoAtivacao
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ExtrairLogs(int numeroSessao, char *codigoDeAtivacao);
        */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao);

        /**BloquearSAT
        * @brief O Aplicativo Comercial ou outro software fornecido pelo Fabricante podera
        * @brief realizar o bloqueio operacional do Equipamento SAT
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* BloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        */
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr BloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* DesbloquearSAT(int numeroSessao, char *codigoDeAtivacao);
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr DesbloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //TrocarCodigoDeAtivacao
        //O Aplicativo Comercial ou outro software fornecido pelo Fabricante
        //poderá realizar a troca do codigo de ativacao a qualquer momento
        //DLLIMPORT char* TrocarCodigoDeAtivacao(int numeroSessao, char *codigoDeAtivacao, int opcao, char *novoCodigo, char *confNovoCodigo);
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr TrocarCodigoDeAtivacao(int numeroSessao, string codigoDeAtivacao, int opcao, string novoCodigo, string confNovoCodigo);

        //DLLIMPORT char* AtualizarSoftwareSAT(int numeroSessao, char *codigoDeAtivacao);
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern IntPtr AtualizarSoftwareSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* GetMapaPortasSAT(char  codatv);
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern String GetMapaPortasSAT(string codigoDeAtivacao);

        //DLLIMPORT char* GetPortaSAT(char serial, int sessao, char * atv);
#if ANYCPU
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"DLL\SATDLL64.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern String GetPortaSAT(string numeroSerie, int numeroSessao, string codigoDeAtivacao);


        //Tratamento de Retornos SAT
        internal static void Trata_RetornoSAT(string Retorno)
        {

            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[2];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);
            MessageBox.Show(String.Format("Retorno SAT: {0} {1}", CodRetornoSAT + " - ", RetornoSAT), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        internal static void Trata_Alerta(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[3];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);

            var CodAlerta = retornoSplit[2];

            var CodMsgSefaz = retornoSplit[4];
            var MsgSefaz = retornoSplit[5];
            var chars2 = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars2);

            MessageBox.Show(String.Format("Retorno SAT: {0}{1} \nAlerta: {2} \nRetorno Sefaz: {3} {4} ",
            CodRetornoSAT + " - ", RetornoSAT, CodAlerta, CodMsgSefaz + " - ", MsgSefaz),
            "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Trata_MsgSefaz(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodMsgSefaz = retornoSplit[3];
            var MsgSefaz = retornoSplit[4];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars);

            MessageBox.Show(String.Format("Retorno Sefaz: {0} {1}", CodMsgSefaz + " - ", MsgSefaz), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




    }

    internal /*unsafe*/ class DeclaracoesELGINCdecl
    {
        internal static string sRetorno;

        // [DllImport("libxml2.dll")]
        /**AtivarSAT
         * Metodo para ativar o uso do SAT
         * @param subComando
         * @param codigoDeAtivacao
         * @param CNPJ
         * @param cUF
         * @return CSR
         * DLLIMPORT char* AtivarSAT(int numeroSessao, int subComando, char *codigoDeAtivacao, char *CNPJ, int cUF);
         **/
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AtivarSAT(int numeroSessao, int subComando, string codigoDeAtivacao, string CNPJ, int cUF);

        /** ComunicarCertificadoICPBRASIL
        * @brief Comunica o certificado icp Brasil
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param Certificado
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ComunicarCertificadoICPBRASIL(int numeroSessao, char *codigoDeAtivacao, char *certificado);
        **/
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ComunicarCertificadoICPBRASIL(int numeroSessao, string codigoDeAtivacao, string certificado);

        /**EnviarDadosVenda
        * @brief Responsavel pelo comando de envio de dados de vendas
        * @param codigoDeAtivacao
        * @param numeroSessao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* EnviarDadosVenda(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
        */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**CancelarUltimaVenda
         * @name CanclearUltimaVenda
         * @brief cancela o ultimo cupom fiscal
         * @param codigoDeAtivacao
         * @param chave
         * @param dadosCancelamento
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* CancelarUltimaVenda(int numeroSessao, char *codigoDeAtivacao, char *chave, char *dadosCancelamento);
         */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento);

        /**ConsultarSAT
         * @name ConsultarSAT
         * @brief consultar SAT
         * @param numeroSessao
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* ConsultarSAT(int numeroSessao);
         */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr ConsultarSAT(int numeroSessao);

        /**TesteFimAFim
          *  @name TesteFimAFim
          *  @brief Esta funcao consiste em um teste de comunicacao entre o AC, o Equipamento SAT e a SEFAZ
          *  @param numeroSessao
          *  @param codigoAtivacao
          *  @param dadosVenda
          *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
          *  DLLIMPORT char* TesteFimAFim(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
          */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr TesteFimAFim(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**ConsultarStatusOperacional
        *  @brief Essa funcao responsavel por verificar a situacao de funcionamento do Equipamento SAT
        *  @param numeroSessao
        *  @param codigoAtivacao
        *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        *  DLLIMPORT char* ConsultarStatusOperacional(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao);

        /**ConsultarNumeroSessao
        * @brief O AC podera verificar se a Ultima sessao requisitada foi processada em caso de nao
        * @brief recebimento do retorno da operacao. O equipamento SAT-CF-e retornara exatamente o resultado da
        * @brief sessao consultada
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConsultarNumeroSessao(int numeroSessao, char *codigoDeAtivacao, int cNumeroDeSessao);
        */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ConsultarNumeroSessao(int numeroSessao, string codigoDeAtivacao, int cNumeroDeSessao);

        /**ConfigurarInterfaceDeRede
        * @brief Responsavel pela configuracao da interface de rede do SAT (Ver espec:2.6.10)
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConfigurarInterfaceDeRede(int numeroSessao, char *codigoDeAtivacao, char *dadosConfiguracao);
        */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ConfigurarInterfaceDeRede(int numeroSessao, string codigoDeAtivacao, string dadosConfiguracao);

        /**AssociarAssinatura
        * @brief Responsavel pelo comando de associar o AC ao SAT
        * @param numeroSessao
        * @param CodigoAtivacao
        * @param CNPJ
        * @param assinaturaCNPJs
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* AssociarAssinatura(int numeroSessao, char *codigoDeAtivacao, char *CNPJvalue, char *assinaturaCNPJs);
        */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AssociarAssinatura(int numeroSessao, string codigoDeAtivacao, string CNPJvalue, string assinaturaCNPJs);

        /**ExtrairLogs
        * @name ExtrairLogs
        * @brief O Aplicativo Comercial poderÃ¡ extrair os arquivos de registro do
        * @brief Equipamento SAT por meio da funÃ§Ã£o ExtrairLogs
        * @param numeroSessao
        * @param codigoAtivacao
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ExtrairLogs(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao);

        /**BloquearSAT
        * @brief O Aplicativo Comercial ou outro software fornecido pelo Fabricante podera
        * @brief realizar o bloqueio operacional do Equipamento SAT
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* BloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr BloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* DesbloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr DesbloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //TrocarCodigoDeAtivacao
        //O Aplicativo Comercial ou outro software fornecido pelo Fabricante
        //poderá realizar a troca do codigo de ativacao a qualquer momento
        //DLLIMPORT char* TrocarCodigoDeAtivacao(int numeroSessao, char *codigoDeAtivacao, int opcao, char *novoCodigo, char *confNovoCodigo);
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr TrocarCodigoDeAtivacao(int numeroSessao, string codigoDeAtivacao, int opcao, string novoCodigo, string confNovoCodigo);

        //DLLIMPORT char* AtualizarSoftwareSAT(int numeroSessao, char *codigoDeAtivacao);
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AtualizarSoftwareSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* GetMapaPortasSAT(char  codatv);
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern String GetMapaPortasSAT(string codigoDeAtivacao);

        //DLLIMPORT char* GetPortaSAT(char serial, int sessao, char * atv);
        [DllImport(@"DLL\ElginSAT.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern String GetPortaSAT(string numeroSerie, int numeroSessao, string codigoDeAtivacao);


        //Tratamento de Retornos SAT
        internal static void Trata_RetornoSAT(string Retorno)
        {

            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[2];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);
            MessageBox.Show(String.Format("Retorno SAT: {0} {1}", CodRetornoSAT + " - ", RetornoSAT), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        internal static void Trata_Alerta(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[3];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);

            var CodAlerta = retornoSplit[2];

            var CodMsgSefaz = retornoSplit[4];
            var MsgSefaz = retornoSplit[5];
            var chars2 = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars2);

            MessageBox.Show(String.Format("Retorno SAT: {0}{1} \nAlerta: {2} \nRetorno Sefaz: {3} {4} ",
            CodRetornoSAT + " - ", RetornoSAT, CodAlerta, CodMsgSefaz + " - ", MsgSefaz),
            "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Trata_MsgSefaz(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodMsgSefaz = retornoSplit[3];
            var MsgSefaz = retornoSplit[4];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars);

            MessageBox.Show(String.Format("Retorno Sefaz: {0} {1}", CodMsgSefaz + " - ", MsgSefaz), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




    }
    
    internal /*unsafe*/ class DeclaracoesCONTROLID
    {
        internal static string sRetorno;

        // [DllImport("libxml2.dll")]
        /**AtivarSAT
         * Metodo para ativar o uso do SAT
         * @param subComando
         * @param codigoDeAtivacao
         * @param CNPJ
         * @param cUF
         * @return CSR
         * DLLIMPORT char* AtivarSAT(int numeroSessao, int subComando, char *codigoDeAtivacao, char *CNPJ, int cUF);
         **/
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AtivarSAT(int numeroSessao, int subComando, string codigoDeAtivacao, string CNPJ, int cUF);

        /** ComunicarCertificadoICPBRASIL
        * @brief Comunica o certificado icp Brasil
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param Certificado
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ComunicarCertificadoICPBRASIL(int numeroSessao, char *codigoDeAtivacao, char *certificado);
        **/
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ComunicarCertificadoICPBRASIL(int numeroSessao, string codigoDeAtivacao, string certificado);

        /**EnviarDadosVenda
        * @brief Responsavel pelo comando de envio de dados de vendas
        * @param codigoDeAtivacao
        * @param numeroSessao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* EnviarDadosVenda(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
        */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**CancelarUltimaVenda
         * @name CanclearUltimaVenda
         * @brief cancela o ultimo cupom fiscal
         * @param codigoDeAtivacao
         * @param chave
         * @param dadosCancelamento
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* CancelarUltimaVenda(int numeroSessao, char *codigoDeAtivacao, char *chave, char *dadosCancelamento);
         */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento);

        /**ConsultarSAT
         * @name ConsultarSAT
         * @brief consultar SAT
         * @param numeroSessao
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* ConsultarSAT(int numeroSessao);
         */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr ConsultarSAT(int numeroSessao);

        /**TesteFimAFim
          *  @name TesteFimAFim
          *  @brief Esta funcao consiste em um teste de comunicacao entre o AC, o Equipamento SAT e a SEFAZ
          *  @param numeroSessao
          *  @param codigoAtivacao
          *  @param dadosVenda
          *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
          *  DLLIMPORT char* TesteFimAFim(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
          */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr TesteFimAFim(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**ConsultarStatusOperacional
        *  @brief Essa funcao responsavel por verificar a situacao de funcionamento do Equipamento SAT
        *  @param numeroSessao
        *  @param codigoAtivacao
        *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        *  DLLIMPORT char* ConsultarStatusOperacional(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao);

        /**ConsultarNumeroSessao
        * @brief O AC podera verificar se a Ultima sessao requisitada foi processada em caso de nao
        * @brief recebimento do retorno da operacao. O equipamento SAT-CF-e retornara exatamente o resultado da
        * @brief sessao consultada
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConsultarNumeroSessao(int numeroSessao, char *codigoDeAtivacao, int cNumeroDeSessao);
        */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ConsultarNumeroSessao(int numeroSessao, string codigoDeAtivacao, int cNumeroDeSessao);

        /**ConfigurarInterfaceDeRede
        * @brief Responsavel pela configuracao da interface de rede do SAT (Ver espec:2.6.10)
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConfigurarInterfaceDeRede(int numeroSessao, char *codigoDeAtivacao, char *dadosConfiguracao);
        */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ConfigurarInterfaceDeRede(int numeroSessao, string codigoDeAtivacao, string dadosConfiguracao);

        /**AssociarAssinatura
        * @brief Responsavel pelo comando de associar o AC ao SAT
        * @param numeroSessao
        * @param CodigoAtivacao
        * @param CNPJ
        * @param assinaturaCNPJs
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* AssociarAssinatura(int numeroSessao, char *codigoDeAtivacao, char *CNPJvalue, char *assinaturaCNPJs);
        */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AssociarAssinatura(int numeroSessao, string codigoDeAtivacao, string CNPJvalue, string assinaturaCNPJs);

        /**ExtrairLogs
        * @name ExtrairLogs
        * @brief O Aplicativo Comercial poderÃ¡ extrair os arquivos de registro do
        * @brief Equipamento SAT por meio da funÃ§Ã£o ExtrairLogs
        * @param numeroSessao
        * @param codigoAtivacao
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ExtrairLogs(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao);

        /**BloquearSAT
        * @brief O Aplicativo Comercial ou outro software fornecido pelo Fabricante podera
        * @brief realizar o bloqueio operacional do Equipamento SAT
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* BloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr BloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* DesbloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr DesbloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //TrocarCodigoDeAtivacao
        //O Aplicativo Comercial ou outro software fornecido pelo Fabricante
        //poderá realizar a troca do codigo de ativacao a qualquer momento
        //DLLIMPORT char* TrocarCodigoDeAtivacao(int numeroSessao, char *codigoDeAtivacao, int opcao, char *novoCodigo, char *confNovoCodigo);
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr TrocarCodigoDeAtivacao(int numeroSessao, string codigoDeAtivacao, int opcao, string novoCodigo, string confNovoCodigo);

        //DLLIMPORT char* AtualizarSoftwareSAT(int numeroSessao, char *codigoDeAtivacao);
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AtualizarSoftwareSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* GetMapaPortasSAT(char  codatv);
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern String GetMapaPortasSAT(string codigoDeAtivacao);

        //DLLIMPORT char* GetPortaSAT(char serial, int sessao, char * atv);
        [DllImport(@"DLL\libsatid32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern String GetPortaSAT(string numeroSerie, int numeroSessao, string codigoDeAtivacao);


        //Tratamento de Retornos SAT
        internal static void Trata_RetornoSAT(string Retorno)
        {

            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[2];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);
            MessageBox.Show(String.Format("Retorno SAT: {0} {1}", CodRetornoSAT + " - ", RetornoSAT), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        internal static void Trata_Alerta(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[3];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);

            var CodAlerta = retornoSplit[2];

            var CodMsgSefaz = retornoSplit[4];
            var MsgSefaz = retornoSplit[5];
            var chars2 = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars2);

            MessageBox.Show(String.Format("Retorno SAT: {0}{1} \nAlerta: {2} \nRetorno Sefaz: {3} {4} ",
            CodRetornoSAT + " - ", RetornoSAT, CodAlerta, CodMsgSefaz + " - ", MsgSefaz),
            "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Trata_MsgSefaz(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodMsgSefaz = retornoSplit[3];
            var MsgSefaz = retornoSplit[4];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars);

            MessageBox.Show(String.Format("Retorno Sefaz: {0} {1}", CodMsgSefaz + " - ", MsgSefaz), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




    }

    internal /*unsafe*/ class DeclaracoesTANCA
    {
        internal static string sRetorno;

        // [DllImport("libxml2.dll")]
        /**AtivarSAT
         * Metodo para ativar o uso do SAT
         * @param subComando
         * @param codigoDeAtivacao
         * @param CNPJ
         * @param cUF
         * @return CSR
         * DLLIMPORT char* AtivarSAT(int numeroSessao, int subComando, char *codigoDeAtivacao, char *CNPJ, int cUF);
         **/
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr AtivarSAT(int numeroSessao, int subComando, string codigoDeAtivacao, string CNPJ, int cUF);

        /** ComunicarCertificadoICPBRASIL
        * @brief Comunica o certificado icp Brasil
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param Certificado
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ComunicarCertificadoICPBRASIL(int numeroSessao, char *codigoDeAtivacao, char *certificado);
        **/
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr ComunicarCertificadoICPBRASIL(int numeroSessao, string codigoDeAtivacao, string certificado);

        /**EnviarDadosVenda
        * @brief Responsavel pelo comando de envio de dados de vendas
        * @param codigoDeAtivacao
        * @param numeroSessao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* EnviarDadosVenda(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
        */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**CancelarUltimaVenda
         * @name CanclearUltimaVenda
         * @brief cancela o ultimo cupom fiscal
         * @param codigoDeAtivacao
         * @param chave
         * @param dadosCancelamento
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* CancelarUltimaVenda(int numeroSessao, char *codigoDeAtivacao, char *chave, char *dadosCancelamento);
         */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento);

        /**ConsultarSAT
         * @name ConsultarSAT
         * @brief consultar SAT
         * @param numeroSessao
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* ConsultarSAT(int numeroSessao);
         */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr ConsultarSAT(int numeroSessao);

        /**TesteFimAFim
          *  @name TesteFimAFim
          *  @brief Esta funcao consiste em um teste de comunicacao entre o AC, o Equipamento SAT e a SEFAZ
          *  @param numeroSessao
          *  @param codigoAtivacao
          *  @param dadosVenda
          *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
          *  DLLIMPORT char* TesteFimAFim(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
          */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr TesteFimAFim(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**ConsultarStatusOperacional
        *  @brief Essa funcao responsavel por verificar a situacao de funcionamento do Equipamento SAT
        *  @param numeroSessao
        *  @param codigoAtivacao
        *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        *  DLLIMPORT char* ConsultarStatusOperacional(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao);

        /**ConsultarNumeroSessao
        * @brief O AC podera verificar se a Ultima sessao requisitada foi processada em caso de nao
        * @brief recebimento do retorno da operacao. O equipamento SAT-CF-e retornara exatamente o resultado da
        * @brief sessao consultada
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConsultarNumeroSessao(int numeroSessao, char *codigoDeAtivacao, int cNumeroDeSessao);
        */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr ConsultarNumeroSessao(int numeroSessao, string codigoDeAtivacao, int cNumeroDeSessao);

        /**ConfigurarInterfaceDeRede
        * @brief Responsavel pela configuracao da interface de rede do SAT (Ver espec:2.6.10)
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConfigurarInterfaceDeRede(int numeroSessao, char *codigoDeAtivacao, char *dadosConfiguracao);
        */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr ConfigurarInterfaceDeRede(int numeroSessao, string codigoDeAtivacao, string dadosConfiguracao);

        /**AssociarAssinatura
        * @brief Responsavel pelo comando de associar o AC ao SAT
        * @param numeroSessao
        * @param CodigoAtivacao
        * @param CNPJ
        * @param assinaturaCNPJs
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* AssociarAssinatura(int numeroSessao, char *codigoDeAtivacao, char *CNPJvalue, char *assinaturaCNPJs);
        */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr AssociarAssinatura(int numeroSessao, string codigoDeAtivacao, string CNPJvalue, string assinaturaCNPJs);

        /**ExtrairLogs
        * @name ExtrairLogs
        * @brief O Aplicativo Comercial poderÃ¡ extrair os arquivos de registro do
        * @brief Equipamento SAT por meio da funÃ§Ã£o ExtrairLogs
        * @param numeroSessao
        * @param codigoAtivacao
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ExtrairLogs(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao);

        /**BloquearSAT
        * @brief O Aplicativo Comercial ou outro software fornecido pelo Fabricante podera
        * @brief realizar o bloqueio operacional do Equipamento SAT
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* BloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr BloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* DesbloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr DesbloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //TrocarCodigoDeAtivacao
        //O Aplicativo Comercial ou outro software fornecido pelo Fabricante
        //poderá realizar a troca do codigo de ativacao a qualquer momento
        //DLLIMPORT char* TrocarCodigoDeAtivacao(int numeroSessao, char *codigoDeAtivacao, int opcao, char *novoCodigo, char *confNovoCodigo);
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr TrocarCodigoDeAtivacao(int numeroSessao, string codigoDeAtivacao, int opcao, string novoCodigo, string confNovoCodigo);

        //DLLIMPORT char* AtualizarSoftwareSAT(int numeroSessao, char *codigoDeAtivacao);
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr AtualizarSoftwareSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* GetMapaPortasSAT(char  codatv);
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern String GetMapaPortasSAT(string codigoDeAtivacao);

        //DLLIMPORT char* GetPortaSAT(char serial, int sessao, char * atv);
        [DllImport(@"DLL\SAT_x64.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern String GetPortaSAT(string numeroSerie, int numeroSessao, string codigoDeAtivacao);


        //Tratamento de Retornos SAT
        internal static void Trata_RetornoSAT(string Retorno)
        {

            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[2];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);
            MessageBox.Show(String.Format("Retorno SAT: {0} {1}", CodRetornoSAT + " - ", RetornoSAT), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        internal static void Trata_Alerta(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[3];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);

            var CodAlerta = retornoSplit[2];

            var CodMsgSefaz = retornoSplit[4];
            var MsgSefaz = retornoSplit[5];
            var chars2 = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars2);

            MessageBox.Show(String.Format("Retorno SAT: {0}{1} \nAlerta: {2} \nRetorno Sefaz: {3} {4} ",
            CodRetornoSAT + " - ", RetornoSAT, CodAlerta, CodMsgSefaz + " - ", MsgSefaz),
            "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Trata_MsgSefaz(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodMsgSefaz = retornoSplit[3];
            var MsgSefaz = retornoSplit[4];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars);

            MessageBox.Show(String.Format("Retorno Sefaz: {0} {1}", CodMsgSefaz + " - ", MsgSefaz), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




    }

    internal /*unsafe*/ class DeclaracoesEMULADOR
    {
        internal static string sRetorno;

        // [DllImport("libxml2.dll")]
        /**AtivarSAT
         * Metodo para ativar o uso do SAT
         * @param subComando
         * @param codigoDeAtivacao
         * @param CNPJ
         * @param cUF
         * @return CSR
         * DLLIMPORT char* AtivarSAT(int numeroSessao, int subComando, char *codigoDeAtivacao, char *CNPJ, int cUF);
         **/
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AtivarSAT(int numeroSessao, int subComando, string codigoDeAtivacao, string CNPJ, int cUF);

        /** ComunicarCertificadoICPBRASIL
        * @brief Comunica o certificado icp Brasil
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param Certificado
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ComunicarCertificadoICPBRASIL(int numeroSessao, char *codigoDeAtivacao, char *certificado);
        **/
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ComunicarCertificadoICPBRASIL(int numeroSessao, string codigoDeAtivacao, string certificado);

        /**EnviarDadosVenda
        * @brief Responsavel pelo comando de envio de dados de vendas
        * @param codigoDeAtivacao
        * @param numeroSessao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* EnviarDadosVenda(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
        */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr EnviarDadosVenda(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**CancelarUltimaVenda
         * @name CanclearUltimaVenda
         * @brief cancela o ultimo cupom fiscal
         * @param codigoDeAtivacao
         * @param chave
         * @param dadosCancelamento
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* CancelarUltimaVenda(int numeroSessao, char *codigoDeAtivacao, char *chave, char *dadosCancelamento);
         */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr CancelarUltimaVenda(int numeroSessao, string codigoDeAtivacao, string chave, string dadosCancelamento);

        /**ConsultarSAT
         * @name ConsultarSAT
         * @brief consultar SAT
         * @param numeroSessao
         * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
         * DLLIMPORT char* ConsultarSAT(int numeroSessao);
         */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr ConsultarSAT(int numeroSessao);

        /**TesteFimAFim
          *  @name TesteFimAFim
          *  @brief Esta funcao consiste em um teste de comunicacao entre o AC, o Equipamento SAT e a SEFAZ
          *  @param numeroSessao
          *  @param codigoAtivacao
          *  @param dadosVenda
          *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
          *  DLLIMPORT char* TesteFimAFim(int numeroSessao, char *codigoDeAtivacao, char *dadosVenda);
          */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr TesteFimAFim(int numeroSessao, string codigoDeAtivacao, string dadosVenda);

        /**ConsultarStatusOperacional
        *  @brief Essa funcao responsavel por verificar a situacao de funcionamento do Equipamento SAT
        *  @param numeroSessao
        *  @param codigoAtivacao
        *  @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        *  DLLIMPORT char* ConsultarStatusOperacional(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ConsultarStatusOperacional(int numeroSessao, string codigoDeAtivacao);

        /**ConsultarNumeroSessao
        * @brief O AC podera verificar se a Ultima sessao requisitada foi processada em caso de nao
        * @brief recebimento do retorno da operacao. O equipamento SAT-CF-e retornara exatamente o resultado da
        * @brief sessao consultada
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConsultarNumeroSessao(int numeroSessao, char *codigoDeAtivacao, int cNumeroDeSessao);
        */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ConsultarNumeroSessao(int numeroSessao, string codigoDeAtivacao, int cNumeroDeSessao);

        /**ConfigurarInterfaceDeRede
        * @brief Responsavel pela configuracao da interface de rede do SAT (Ver espec:2.6.10)
        * @param numeroSessao
        * @param codigoDeAtivacao
        * @param dadosVenda
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ConfigurarInterfaceDeRede(int numeroSessao, char *codigoDeAtivacao, char *dadosConfiguracao);
        */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ConfigurarInterfaceDeRede(int numeroSessao, string codigoDeAtivacao, string dadosConfiguracao);

        /**AssociarAssinatura
        * @brief Responsavel pelo comando de associar o AC ao SAT
        * @param numeroSessao
        * @param CodigoAtivacao
        * @param CNPJ
        * @param assinaturaCNPJs
        * @return pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* AssociarAssinatura(int numeroSessao, char *codigoDeAtivacao, char *CNPJvalue, char *assinaturaCNPJs);
        */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AssociarAssinatura(int numeroSessao, string codigoDeAtivacao, string CNPJvalue, string assinaturaCNPJs);

        /**ExtrairLogs
        * @name ExtrairLogs
        * @brief O Aplicativo Comercial poderÃ¡ extrair os arquivos de registro do
        * @brief Equipamento SAT por meio da funÃ§Ã£o ExtrairLogs
        * @param numeroSessao
        * @param codigoAtivacao
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* ExtrairLogs(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ExtrairLogs(int numeroSessao, string codigoDeAtivacao);

        /**BloquearSAT
        * @brief O Aplicativo Comercial ou outro software fornecido pelo Fabricante podera
        * @brief realizar o bloqueio operacional do Equipamento SAT
        * @return  pointer para area com retorno do comando enviado pelo dispositivo SAT
        * DLLIMPORT char* BloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        */
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr BloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* DesbloquearSAT(int numeroSessao, char *codigoDeAtivacao);
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr DesbloquearSAT(int numeroSessao, string codigoDeAtivacao);

        //TrocarCodigoDeAtivacao
        //O Aplicativo Comercial ou outro software fornecido pelo Fabricante
        //poderá realizar a troca do codigo de ativacao a qualquer momento
        //DLLIMPORT char* TrocarCodigoDeAtivacao(int numeroSessao, char *codigoDeAtivacao, int opcao, char *novoCodigo, char *confNovoCodigo);
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr TrocarCodigoDeAtivacao(int numeroSessao, string codigoDeAtivacao, int opcao, string novoCodigo, string confNovoCodigo);

        //DLLIMPORT char* AtualizarSoftwareSAT(int numeroSessao, char *codigoDeAtivacao);
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AtualizarSoftwareSAT(int numeroSessao, string codigoDeAtivacao);

        //DLLIMPORT char* GetMapaPortasSAT(char  codatv);
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern String GetMapaPortasSAT(string codigoDeAtivacao);

        //DLLIMPORT char* GetPortaSAT(char serial, int sessao, char * atv);
        [DllImport("SATEMU.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern String GetPortaSAT(string numeroSerie, int numeroSessao, string codigoDeAtivacao);


        //Tratamento de Retornos SAT
        internal static void Trata_RetornoSAT(string Retorno)
        {

            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[2];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);
            MessageBox.Show(String.Format("Retorno SAT: {0} {1}", CodRetornoSAT + " - ", RetornoSAT), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        internal static void Trata_Alerta(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodRetornoSAT = retornoSplit[1];
            var RetornoSAT = retornoSplit[3];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(RetornoSAT));
            RetornoSAT = new string(chars);

            var CodAlerta = retornoSplit[2];

            var CodMsgSefaz = retornoSplit[4];
            var MsgSefaz = retornoSplit[5];
            var chars2 = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars2);

            MessageBox.Show(String.Format("Retorno SAT: {0}{1} \nAlerta: {2} \nRetorno Sefaz: {3} {4} ",
            CodRetornoSAT + " - ", RetornoSAT, CodAlerta, CodMsgSefaz + " - ", MsgSefaz),
            "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void Trata_MsgSefaz(string Retorno)
        {
            string[] retornoSplit = Retorno.Split('|');
            var CodMsgSefaz = retornoSplit[3];
            var MsgSefaz = retornoSplit[4];
            var chars = Encoding.GetEncoding("UTF-8").GetChars(Encoding.Default.GetBytes(MsgSefaz));
            MsgSefaz = new string(chars);

            MessageBox.Show(String.Format("Retorno Sefaz: {0} {1}", CodMsgSefaz + " - ", MsgSefaz), "D-SAT DIMEP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




    }

    internal class NumSessao
    {
        internal int GeraNumero()
        {
            Random rdn = new Random();
            return rdn.Next(999999);
        }
    }
}

using System.Runtime.InteropServices;

namespace PDV_WPF.Funcoes
{
    public static class SiTEFDLL
    {
#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int ConfiguraIntSiTefInterativo(string IPSiTef, string IdLoja, string IdTerminal, string Reservado);



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int ConfiguraIntSiTefInterativoEx(string IPSiTef, string IdLoja, string IdTerminal, string Reservado, string ParametrosAdicionais);



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int IniciaFuncaoSiTefInterativo(int Funcao, string Valor, string CupomFiscal, string DataFiscal, string HoraFiscal, string Operador, string ParamAdic);



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int ContinuaFuncaoSiTefInterativo(
            ref int Comando,
            ref long TipoCampo,
            ref short TamMinimo,
            ref short TamMaximo,
            byte[] Buffer,
            int TamBuffer,
            int Continua);



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern void FinalizaFuncaoSiTefInterativo(short Confirma, string CupomFiscal, string DataFiscal, string HoraFiscal, string ParamAdic);



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int ObtemQuantidadeTransacoesPendentes(string DataFiscal, string CupomFiscal);



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int VerificaPresencaPinPad();



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int KeepAlivePinPad();



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int EscreveMensagemPermanentePinPad(string Mensagem);



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int LeTrilha3(string Mensagem);



#if HOMOLOGATEF
        [DllImport(@"SiTEF\Homologa\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#else
        [DllImport(@"SiTEF\CliSiTef32I.dll", CallingConvention = CallingConvention.StdCall)]
#endif
        internal static extern int LeCartaoSeguro(string Mensagem);
    }
}

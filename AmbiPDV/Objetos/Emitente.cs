using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.DataSets.FDBDataSetOperSeedTableAdapters;
using PDV_WPF.Exceptions;
using PDV_WPF.Funcoes;
using System;
using System.Collections.Generic;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Objetos
{
    public static class Emitente
    {
        private static bool _infoCarregada = false;
        private static string _razaoSocial;
        public static string RazaoSocial { get { if (_razaoSocial is null) { _listaErros.Add("Razão Social"); return null; } else return _razaoSocial; } }
        private static string _nomeFantasia;
        public static string NomeFantasia { get { if (_nomeFantasia == "") { _listaErros.Add("Nome Fantasia"); return null; } else return _nomeFantasia; } }
        private static string _cEP;
        public static string CEP { get { if (_cEP == "") { _listaErros.Add("CEP"); return null; } else return _cEP; } }
        private static string _end_Tipo;
        public static string End_Tipo { get { if (_end_Tipo == "") { _listaErros.Add("Tipo do Logradouro"); return null; } else return _end_Tipo; } }
        private static string _endereco;
        public static string Endereco { get { if (_endereco == "Não Identificado") { _listaErros.Add("Endereço"); return null; } else return _endereco; } }
        private static string _end_Numero;
        public static string End_Numero { get { return _end_Numero; } }
        private static string _end_Bairro;
        public static string End_Bairro { get { if (_end_Bairro == "") { _listaErros.Add("Bairro"); return null; } else return _end_Bairro; } }
        public static string EnderecoCompleto
        {
            get
            {
                if (!_infoCarregada)
                {
                    throw new DataNotLoadedException();
                }
                else
                {
                    return $"{End_Tipo} {Endereco}, {End_Numero} - {End_Bairro}, São Paulo";
                }
            }
        }
        private static string _cNPJ;
        public static string CNPJ { get { return _cNPJ; } }
        private static string _iE;
        public static string IE { get { return _iE; } }
        private static string _iM;
        public static string IM { get { if (_iM == "") { _listaErros.Add("IM"); return null; } else return _iM; } }
        private static string _dDD_Comer;
        public static string DDD_Comer { get { if (_dDD_Comer == "") { _listaErros.Add("DDD Comercial"); return null; } else return _dDD_Comer; } }
        private static string _tel_Comer;
        public static string Tel_Comer { get { if (_tel_Comer == "") { _listaErros.Add("Telefone Comercial"); return null; } else return _tel_Comer; } }
        private static string _email;
        public static string Email { get { if (_email == "") { _listaErros.Add("Email"); return null; } else return _email; } }
        private static bool _simples;
        public static bool? BoolSimples
        {
            get
            {
                if (!_infoCarregada)
                {
                    _listaErros.Add("Simples Nacional");
                    return null;
                }
                else return _simples;
            }
        }
        private static string StrSimples
        {
            set
            {
                switch (value)
                {
                    case "N":
                        _simples = false;
                        break;
                    default:
                    case "S":
                        _simples = true;
                        break;
                }
            }
        }
        public static string Cidade { get; private set; }

        private static List<string> _listaErros = new List<string>();
        
        public static List<string> ListaErro
        {
            get
            { return _listaErros; }
        }
        public static bool CarregaInfo()
        {
            using var EMITENTE_DT = new DataSets.FDBDataSetOperSeed.TB_EMITENTEDataTable();
            using var EMITENTE_TA = new TB_EMITENTETableAdapter();
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            try
            {
                EMITENTE_TA.Connection = LOCAL_FB_CONN;
                EMITENTE_TA.Fill(EMITENTE_DT);
            }
            catch (Exception ex)
            {
                throw new DataNotLoadedException("Dados de emitente não puderam ser carregados", ex);
            }

            DataSets.FDBDataSetOperSeed.TB_EMITENTERow row = EMITENTE_DT[0];

            _razaoSocial = row.NOME;
            _nomeFantasia = row.IsNOME_FANTANull() ? "" : row.NOME_FANTA;
            _cEP = row.IsEND_CEPNull() ? "" : row.END_CEP;
            _end_Tipo = row.IsEND_TIPONull() ? "" : row.END_TIPO;
            _endereco = row.IsEND_LOGRADNull() ? "Não Identificado" : row.END_LOGRAD;
            _end_Numero = row.IsEND_NUMERONull() ? "S/N" : row.END_NUMERO;
            _end_Bairro = row.IsEND_BAIRRONull() ? "" : row.END_BAIRRO;
            _cNPJ = row.CNPJ;
            _iE = row.IsINSC_ESTADNull() ? "Isento" : row.INSC_ESTAD.TiraPont();
            _iM = row.IsINSC_MUNICNull() ? "" : row.INSC_MUNIC;
            _dDD_Comer = row.IsDDD_COMERNull() ? "" : row.DDD_COMER;
            _tel_Comer = row.IsFONE_COMERNull() ? "" : row.FONE_COMER;
            _email = row.IsEMAIL_CONTNull() ? "" : row.EMAIL_CONT;
            StrSimples = row.SIMPLES;
            _infoCarregada = true;
            return true;
        }
    }
}

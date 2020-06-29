using PDV_WPF.Exceptions;

namespace PDV_WPF.Objetos
{
    public class ParamsDeConfig
    {
        internal string multiplosCupons;
        public bool MultiplosCupons
        {
            set
            {
                switch (value)
                {
                    case true:
                        multiplosCupons = "1";
                        break;
                    default:
                        multiplosCupons = "0";
                        break;
                }
            }
        }

        internal string portaPinPad;
        public int PortaPinPad
        {
            set
            {
                if (value > 0)
                {
                    portaPinPad = value.ToString();
                }
                else
                {
                    throw new ErroDeValidacaoTEF("Porta informada era 0 ou negativa.");
                }
            }
        }

        internal string lojaECF;
        public string LojaECF
        {
            set
            {
                if (value.Length > 20) throw new ErroDeValidacaoTEF("Número da LojaECF contém mais que 20 caracteres.");
                lojaECF = value;
            }
        }

        internal string caixaECF;
        public string CaixaECF
        {
            set
            {
                if (value.Length > 20) throw new ErroDeValidacaoTEF("Número do CaixaECF contém mais que 20 caracteres.");
                caixaECF = value;
            }
        }

        internal string numeroSerieECF;
        public string NumeroSerieECF
        {
            set
            {
                if (value.Length > 20) throw new ErroDeValidacaoTEF("Numero de série da ECF contém mais que 20 caracteres.");
                numeroSerieECF = value;
            }
        }

        internal string cnpjEstabelecimento;
        public string CNPJ
        {
            set
            {
                if (value.Length == 14) cnpjEstabelecimento = value;
                else if (value.Length == 18) cnpjEstabelecimento = value.Replace(".", "").Replace("-", "").Replace("/", "");
            }
        }

        internal string cpfEstabelecimento;
        public string CPF
        {
            set
            {
                if (value.Length == 11) cpfEstabelecimento = value;
                else if (value.Length == 14) cpfEstabelecimento = value.Replace(".", "").Replace("-", "");
            }
        }

        internal string cnpjFacilitador;
        public string CNPJFacilitador
        {
            set
            {
                if (value.Length == 14) cnpjFacilitador = value;
                else if (value.Length == 18) cnpjFacilitador = value.Replace(".", "").Replace("-", "").Replace("/", "");
            }
        }
    }

}

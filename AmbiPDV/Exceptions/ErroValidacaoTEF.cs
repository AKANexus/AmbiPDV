using System;


namespace PDV_WPF.Exceptions
{
    [Serializable]
    internal class ErroDeValidacaoTEF : ApplicationException
    {
        public ErroDeValidacaoTEF(string message) : base(message)
        {

        }
        public ErroDeValidacaoTEF() : base()
        {

        }
    }
}

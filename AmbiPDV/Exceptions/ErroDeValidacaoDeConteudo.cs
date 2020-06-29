using System;


namespace PDV_WPF.Exceptions
{
 [Serializable]
    internal class ErroDeValidacaoDeConteudo : ApplicationException
    {
        public ErroDeValidacaoDeConteudo(string message) : base(message)
        {

        }
        public ErroDeValidacaoDeConteudo() : base()
        {

        }
    }
}

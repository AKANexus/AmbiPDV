using System;

namespace PDV_WPF.Exceptions
{
[Serializable]
    internal class SATNaoConfigurado : ApplicationException
    {
        public SATNaoConfigurado() : base($"Nenhum SAT foi configurado para uso no sistema.")
        {

        }
    }
}


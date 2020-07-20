using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDV_WPF.Exceptions
{
    [Serializable]
    internal class SynchException : ApplicationException
    {
        public SynchException(string message, Exception innerException) : base(message, innerException)
        {

        }
        public SynchException(string message) : base(message)
        {

        }
    }

}

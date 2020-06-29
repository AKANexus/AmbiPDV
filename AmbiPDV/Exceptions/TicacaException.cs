using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDV_WPF.Exceptions
{
    public class TicacaException : ApplicationException
    {
        public TicacaException() : base("TICACA!!!")
        {

        }
    }
}

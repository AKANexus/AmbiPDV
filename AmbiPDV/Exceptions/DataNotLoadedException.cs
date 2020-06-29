using System;


namespace PDV_WPF.Exceptions
{
    [Serializable]
    internal class DataNotLoadedException : ApplicationException
    {
        public DataNotLoadedException(string message, Exception innerException) : base(message, innerException)
        {

        }
        public DataNotLoadedException(string message) : base(message)
        {

        }
        public DataNotLoadedException() : base()
        {

        }
    }
}

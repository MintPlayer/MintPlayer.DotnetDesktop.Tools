using System;

namespace KarnaughMap.Exceptions
{
    public class MinificationException : Exception
    {
        public MinificationException(string message) : base(message)
        {
        }

        public MinificationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

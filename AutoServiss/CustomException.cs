using System;

namespace AutoServiss
{
    public class CustomException : Exception
    {
        public CustomException()
            : base() { }

        public CustomException(string message)
            : base(message) { }
    }
}

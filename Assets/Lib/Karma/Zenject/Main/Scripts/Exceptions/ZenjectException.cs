using System;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class ZenjectException : Exception
    {
        public ZenjectException(string message)
            : base(message)
        {
        }

        public ZenjectException(
            string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    public class ZenjectResolveException : ZenjectException
    {
        public ZenjectResolveException(string message)
            : base(message)
        {
        }

        public ZenjectResolveException(
            string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    public class ZenjectBindException : ZenjectException
    {
        public ZenjectBindException(string message)
            : base(message)
        {
        }

        public ZenjectBindException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


using System;
using System.Runtime.Serialization;

namespace slingn.circuits.Exceptions
{
    public class CircuitExecutionException : Exception
    {
        public CircuitExecutionException()
        {
        }

        public CircuitExecutionException(string message) : base(message)
        {
        }

        public CircuitExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CircuitExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
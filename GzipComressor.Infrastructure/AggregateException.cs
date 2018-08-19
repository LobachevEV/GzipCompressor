using System;
using System.Collections.Generic;
using System.Text;

namespace GzipComressor.Infrastructure
{
    public class AggregateException : Exception
    {
        private readonly IEnumerable<Exception> exceptions;

        public AggregateException(IEnumerable<Exception> exceptions) : base("")
        {
            this.exceptions = exceptions;
        }

        public override string ToString()
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine(base.ToString());
            foreach (var exception in exceptions) strBuilder.AppendLine(exception.Message);

            return strBuilder.ToString();
        }
    }
}
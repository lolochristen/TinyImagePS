using System;
using System.Management.Automation;

namespace TinyImagePS
{
    public class TinifyException : Exception
    {
        public TinifyException(string message) : base(message)
        {
        }

        public TinifyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ErrorRecord CreateErrorRecord(string errorId = null,
            ErrorCategory errorCategory = ErrorCategory.NotSpecified, object targetObject = null)
        {
            var errorRecord = new ErrorRecord(this, errorId, errorCategory, targetObject);
            errorRecord.ErrorDetails = new ErrorDetails("TinyImage Error: " + Message);
            return errorRecord;
        }
    }
}
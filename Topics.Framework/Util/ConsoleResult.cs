using System;
using System.Text;

namespace Topics.Framework.Util
{
    public class ConsoleResult
    {
        public bool HasError { get; set; }

        public string ErrorCode { get; set; }

        public string Description { get; set; }

        public string StackTrace { get; set; }

        public ConsoleResult()
        {
            HasError = false;
        }

        public ConsoleResult(Exception exception)
        {
            HasError = true;
            ErrorCode = exception.HResult.ToString();
            Description = exception.Message;
            StackTrace = exception.StackTrace;
        }

        public override String ToString()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("\nErrorCode: " + ErrorCode);
            str.AppendLine("\nMessage: " + Description);
            str.AppendLine("\nStackTrace: " + StackTrace);
            return str.ToString();
        }
    }
}
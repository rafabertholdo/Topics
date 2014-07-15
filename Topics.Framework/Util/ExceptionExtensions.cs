using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topics.Framework.Util
{
    public static class ExceptionExtensions
    {
        public static string GetRecursiveDetail(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            BuildExceptionDetail(ex, sb);
            return sb.ToString();
        }

        private static StringBuilder BuildExceptionDetail(Exception ex, StringBuilder sb)
        {
            sb.AppendLine("\nMessage: " + ex.Message);
            sb.AppendLine("\nStackTrace: " + ex.StackTrace);

            // loop recursivly through the inner exception if there are any.
            if (ex.InnerException != null)
            {
                sb.AppendLine("InnerException: ");
                BuildExceptionDetail(ex.InnerException, sb);
            }

            return sb;
        }
    }
}

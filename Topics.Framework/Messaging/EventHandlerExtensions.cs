using Topics.Framework.Messaging.Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topics.Framework.Util;

namespace Topics.Framework.Messaging
{
    /// <summary>
    /// Classe utilizada pelo IoC(inversor de controle) para registrar os subscriptions
    /// ex: public class FinanceiroEventHandler : BaseEventHandler, IEventHandler<SalarioDepositado>
    ///{
    ///    public FinanceiroEventHandler()
    ///        : base("Financeiro")
    ///    {
    ///    }        
    ///    
    ///    public void Handle(SalarioDepositado @event)
    ///    {
    ///        throw new NotImplementedException();
    ///    }
    ///}
    /// </summary>

    public static class EventHandlerExtensions
    {   
        public static string MountFilter<T>() where T : Type
        {
            return MountFilter(typeof(T));
        }

        public static string MountFilter(this Type type)
        {
            List<KeyValuePair<string, string>> eventNames = new List<KeyValuePair<string, string>>();

            foreach (var interfaceType in type.GetInterfaces().Where(e => typeof(IEventHandler).IsAssignableFrom(e)))
            {
                var genericArguments = interfaceType.GetGenericArguments();
                if (genericArguments.Length > 0)
                {
                    var genericArgument = interfaceType.GetGenericArguments()[0];
                    eventNames.Add(new KeyValuePair<string,string>(genericArgument.Name,genericArgument.Assembly.GetName().Version.ToString()));
                }
            }
            if (eventNames.Count == 0)
                return null;
            var filter = new StringBuilder();
            foreach (var kvp in eventNames)
            {
                if (filter.Length > 0)
                    filter.Append(" or ");

                filter.Append(string.Format("({0} = '{1}' and {2} = '{3}')", StandardMetadata.TypeName, kvp.Key, StandardMetadata.Version, kvp.Value));
            }
            return filter.ToString(); //string.Format("TypeName IN ('{0}')", string.Join("','", eventNames.ToArray()));
        }
    }
}

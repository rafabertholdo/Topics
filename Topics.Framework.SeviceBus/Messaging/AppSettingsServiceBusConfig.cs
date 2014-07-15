using Topics.Framework.Messaging;
using Topics.Framework.Messaging.Handling;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Topics.Framework.ServiceBus.Messaging
{
    public class AppSettingsServiceBusConfig
    {
        public ServiceBusSettings Settings { get; set; }
        public string TenantName { get; set; }
        private List<TopicSettings> topicos { get; set; }
        IUnityContainer _container;

        public AppSettingsServiceBusConfig(string tanantName, IUnityContainer unityContainer)
        {
            this.Settings = new ServiceBusSettings
            {
                ConnectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"],                
            };
            this._container = unityContainer;

            this.TenantName = tanantName;
            this.topicos = new List<TopicSettings>();
        }

        //public void RegistarSubscricoes(IEnumerable<Assinatura> assinaturas, string nomeTopico)
        //{
        //    var topico = (from e in this.topicos
        //                  where e.Path.Equals(nomeTopico)
        //                  select e).FirstOrDefault();
        //    if (topico != null)
        //    {
        //        foreach (var assinatura in assinaturas)
        //        {
        //            var subscription = new SubscriptionSettings
        //            {
        //                Name = assinatura.ObterNomeAssinatura(),
        //                RequiresSession = assinatura.RequerSessao,
        //                SqlFilter = assinatura.Filtro
        //            };
        //            topico.Subscriptions.Add(subscription);
        //        }                
        //    }
        //}

        //public void RegistarSubscricoes(Assembly assembly)
        //{
        //    foreach (var eventHandlerType in assembly.ExportedTypes.Where(e => typeof(IEventHandler).IsAssignableFrom(e)))
        //    {
        //        var eventHandlerInstance = (BaseEventHandler)Activator.CreateInstance(eventHandlerType, this.TenantName);
                
        //        var topico = (from e in this.topicos
        //                     where e.Path.Equals(eventHandlerInstance.TopicName)
        //                     select e).FirstOrDefault();
        //        if(topico != null)

        //        { 
        //            string filter = BaseEventHandler.MountFilter(eventHandlerType);

        //            var subscription = new SubscriptionSettings
        //            {
        //                Name = eventHandlerType.AssemblyQualifiedName.GetHashCode().ToString(),
        //                RequiresSession = false,
        //                SqlFilter = filter
        //            };
        //            topico.Subscriptions.Add(subscription);
        //        }
        //    }
        //}

        public void RegistrarTopico(string nomeTopico)
        {
            var topico = new TopicSettings
            {
                IsEventBus = true,
                DuplicateDetectionHistoryTimeWindow = new TimeSpan(0, 30, 00)
            };

            topico.Path = nomeTopico;
            topico.Subscriptions.Add(new SubscriptionSettings
             {
                 Name = "Log",
                 RequiresSession = false,
             });

            this.topicos.Add(topico);            
        }

        public void AplicarRegistroTopicos()
        {
            if (topicos.Count > 0)
            {
                var config = this._container.Resolve<IServiceBusConfig>(new ParameterOverride("settings", this.Settings));
                config.RegisterTopicsAndSubscriptions(topicos);
            }
        }

        //public ManifestoBase RegistrarTopicos(Assembly assembly)
        //{
        //    var topicos = new List<TopicSettings>();
        //    var manifesto = ManifestoBase.GetInstance(assembly);
        //    RegistrarTopico(manifesto);
        //    return manifesto;
        //}

        //public void RegistrarTopicos(List<Assembly> assemblies)
        //{
        //    foreach (var assembly in assemblies)
        //    {
        //        RegistrarTopicos(assembly);
        //    }
        //}
    }
}

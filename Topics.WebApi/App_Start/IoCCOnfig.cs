using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Topics.Framework.Messaging;
using Topics.Framework.Serialization;
using Topics.Framework.ServiceBus.Messaging;
using Topics.WebApi.Repositories;
using Unity.WebApi;

namespace Topics.WebApi
{
    public class IoCConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var container = new UnityContainer();

            container.RegisterType<IMessageSender, TopicSender>();
            container.RegisterType<IServiceBusConfig, ServiceBusConfig>();
            var metadata = new StandardMetadataProvider();
            var serializer = new JsonTextSerializer();

            AppSettingsServiceBusConfig serviceBusConfig = new AppSettingsServiceBusConfig("", container);
            container.RegisterInstance(serviceBusConfig.Settings);


            container.RegisterInstance(new TopicRepository());
            config.DependencyResolver = new UnityDependencyResolver(container);

            Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(
                new Microsoft.Practices.ServiceLocation.ServiceLocatorProvider(() => new Microsoft.Practices.Unity.ServiceLocatorAdapter.UnityServiceLocator(container)));
        }
    }
}
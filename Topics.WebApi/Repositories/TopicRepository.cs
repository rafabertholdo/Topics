using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Topics.Framework.Messaging;
using Topics.Framework.Serialization;
using Topics.Framework.ServiceBus.Messaging;
using Topics.WebApi.Models;

namespace Topics.WebApi.Repositories
{
    public class TopicRepository
    {
        public IEnumerable<Topic> GetTrendingTopics()
        {
            IServiceBusConfig config = null;
            if (ServiceLocator.Current != null)
            {
                config = ServiceLocator.Current.GetInstance<IServiceBusConfig>();                
            }

            if (config != null)
            {
                return from e in config.GetTopics()
                       select new Topic
                       {
                           Name = e.Path
                       };
            }
            else
                return new List<Topic>();
        }

        public IEnumerable<Message> GetMessages(string topicName, int iteration)
        {
            IServiceBusConfig config = null;
            if (ServiceLocator.Current != null)
            {
                config = ServiceLocator.Current.GetInstance<IServiceBusConfig>();                
            }

            if (config != null)
            {
                var serializer = new JsonTextSerializer();                
                return from e in config.GetTopicMessages(topicName, iteration)
                       select (Message)serializer.Deserialize(new StreamReader(e.GetBody<Stream>()));
            }

            return new List<Message>();
        }

        public void CreateTopicIfNotExits(string topicName)
        {
            IServiceBusConfig config = null;
            if (ServiceLocator.Current != null)
            {
                config = ServiceLocator.Current.GetInstance<IServiceBusConfig>();
            }

            if (config != null)
            {
                config.CreateTopicIfNotExists(new TopicSettings
                {
                    Path = topicName,
                    DuplicateDetectionHistoryTimeWindow = new TimeSpan(0, 30, 0)
                });
            }
        }

        public void PostMessage(string topicName, Message message)
        {
            ServiceBusSettings settings = null;
            if (ServiceLocator.Current != null)
            {
                settings = ServiceLocator.Current.GetInstance<ServiceBusSettings>();                
                EventBus eventBus = new EventBus(new TopicSender(settings, topicName),
                    new StandardMetadataProvider(),
                    new JsonTextSerializer());

                eventBus.Publish(message);
            }            
        }
    }
}
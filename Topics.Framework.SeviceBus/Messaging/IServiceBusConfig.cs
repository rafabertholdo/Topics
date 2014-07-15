using Topics.Framework.Messaging;
using Topics.Framework.Messaging.Handling;
using Topics.Framework.Serialization;
using Topics.Framework.ServiceBus.Messaging.Handling;
using System;
using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;
namespace Topics.Framework.ServiceBus.Messaging
{
    public interface IServiceBusConfig
    {
        EventProcessor CreateEventProcessor(string topic, string subscription, SubscriptionSettings subscriptionSettings, IEventHandler handler, ITextSerializer serializer, bool instrumentationEnabled = false);
        
        //Topics.Framework.ServiceBus.Messaging.Handling.EventProcessor CreateEventProcessor(string topic, BaseEventHandler eventHandler, Topics.Framework.Messaging.Handling.IEventHandler handler, Topics.Framework.Serialization.ITextSerializer serializer, bool instrumentationEnabled = false);
        void RegisterTopicsAndSubscriptions(System.Collections.Generic.List<TopicSettings> topics);

        IEnumerable<TopicDescription> GetTopics();
        IEnumerable<BrokeredMessage> GetTopicMessages(string topicName, int iteration);
        void CreateTopicIfNotExists(TopicSettings topic);
    }
}

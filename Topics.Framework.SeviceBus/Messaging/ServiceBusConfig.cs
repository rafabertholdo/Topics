// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://go.microsoft.com/fwlink/p/?LinkID=258575
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

namespace Topics.Framework.ServiceBus.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.Practices.TransientFaultHandling;
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
    using Topics.Framework.Messaging.Handling;
    using Topics.Framework.Serialization;
    using Topics.Framework.ServiceBus.Messaging.Intrumentation;
    using Topics.Framework.ServiceBus.Messaging.Handling;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using System.Text;
    using Topics.Framework.Messaging;



    public class ServiceBusConfig : Topics.Framework.ServiceBus.Messaging.IServiceBusConfig
    {
        private const string RuleName = "Custom";
        private ServiceBusSettings settings;

        public ServiceBusConfig(ServiceBusSettings settings)
        {
            this.settings = settings;
        }

        public void RegisterTopicsAndSubscriptions(List<TopicSettings> topics)
        {

            var namespaceManager = settings.GetNamespaceManager();

            var retryStrategy = new Incremental(3, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            var retryPolicy = new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(retryStrategy);
            topics.AsParallel().ForAll(topic =>
            {
                retryPolicy.ExecuteAction(() => CreateTopicIfNotExists(namespaceManager, topic));
                topic.Subscriptions.AsParallel().ForAll(subscription =>
                {
                    retryPolicy.ExecuteAction(() => CreateSubscriptionIfNotExists(namespaceManager, topic, subscription));
                    retryPolicy.ExecuteAction(() => UpdateRules(namespaceManager, topic, subscription, settings.GetMessagingFactory()));
                });
            });

            // Execute migration support actions only after all the previous ones have been completed.
            foreach (var topic in topics)
            {
                foreach (var action in topic.MigrationSupport)
                {
                    retryPolicy.ExecuteAction(() => UpdateSubscriptionIfExists(namespaceManager, topic, action, settings.GetMessagingFactory()));
                }
            }
        }

        public IEnumerable<TopicDescription> GetTopics()
        {
            var namespaceManager = settings.GetNamespaceManager();
            return namespaceManager.GetTopics();
        }

        public IEnumerable<BrokeredMessage> GetTopicMessages(string topicName, int iteration)
        {
            var namespaceManager = settings.GetNamespaceManager();
            var subscriptionDesc = namespaceManager.GetSubscription(topicName, "log");
            long messageCount = subscriptionDesc.MessageCount;

            var messagingFactory = settings.GetMessagingFactory();
            var brokeredMessages = new List<BrokeredMessage>();
            var subscriptionClient = messagingFactory.CreateSubscriptionClient(topicName, "log", ReceiveMode.PeekLock);

            var start = messageCount - (10 * iteration);
            var count = (int)(start > 0 ? 10 : start < -10 ? 0 : start + 10);
            if (start < 0)
                start = 0;
            var messageEnumerable = subscriptionClient.PeekBatch(start, count);
            if (messageEnumerable == null)
            {
                return brokeredMessages;
            }
            var messageArray = messageEnumerable as BrokeredMessage[] ?? messageEnumerable.ToArray();
            brokeredMessages = new List<BrokeredMessage>(messageArray);

            return brokeredMessages.Reverse<BrokeredMessage>();
        }

        public EventProcessor CreateEventProcessor(string topic, string subscription, SubscriptionSettings subscriptionSettings, IEventHandler handler, ITextSerializer serializer, bool instrumentationEnabled = false)
        {
            var retryStrategy = new Incremental(3, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            var retryPolicy = new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(retryStrategy);
            var namespaceManager = settings.GetNamespaceManager();
            var topicSettings = new TopicSettings
            {
                IsEventBus = true,
                DuplicateDetectionHistoryTimeWindow = new TimeSpan(0, 30, 0),
                Path = topic
            };
            retryPolicy.ExecuteAction(() => CreateSubscriptionIfNotExists(namespaceManager, topicSettings, subscriptionSettings));
            retryPolicy.ExecuteAction(() => UpdateRules(namespaceManager, topicSettings, subscriptionSettings, settings.GetMessagingFactory()));

            IMessageReceiver receiver;

            if (subscriptionSettings.RequiresSession)
            {
                var instrumentation = new SessionSubscriptionReceiverInstrumentation(subscription, instrumentationEnabled);
                try
                {
                    receiver = (IMessageReceiver)new SessionSubscriptionReceiver(this.settings, topic, subscription, true, instrumentation);
                }
                catch
                {
                    instrumentation.Dispose();
                    throw;
                }
            }
            else
            {
                var instrumentation = new SubscriptionReceiverInstrumentation(subscription, instrumentationEnabled);
                try
                {
                    receiver = (IMessageReceiver)new SubscriptionReceiver(this.settings, topic, subscription, true, instrumentation);
                }
                catch
                {
                    instrumentation.Dispose();
                    throw;
                }
            }

            EventProcessor processor;
            try
            {
                processor = new EventProcessor(receiver, serializer);
            }
            catch
            {
                using (receiver as IDisposable) { }
                throw;
            }

            try
            {
                processor.Register(handler);

                return processor;
            }
            catch
            {
                processor.Dispose();
                throw;
            }
        }

        public void CreateTopicIfNotExists(TopicSettings topic)
        {
            var topicDescription =
                new TopicDescription(topic.Path)
                {
                    RequiresDuplicateDetection = true,
                    DuplicateDetectionHistoryTimeWindow = topic.DuplicateDetectionHistoryTimeWindow,
                };
            
            var namespaceManager = settings.GetNamespaceManager();
            namespaceManager.CreateTopic(topicDescription);
            CreateSubscriptionIfNotExists(namespaceManager, topic, new SubscriptionSettings
            {
                Name="log",
                RequiresSession=false                
            });
        }

        private void CreateTopicIfNotExists(NamespaceManager namespaceManager, TopicSettings topic)
        {
            var topicDescription =
                new TopicDescription(topic.Path)
                {
                    RequiresDuplicateDetection = true,
                    DuplicateDetectionHistoryTimeWindow = topic.DuplicateDetectionHistoryTimeWindow,
                };

            try
            {
                namespaceManager.CreateTopic(topicDescription);
            }
            catch (MessagingEntityAlreadyExistsException) { }
        }

        private void CreateSubscriptionIfNotExists(NamespaceManager namespaceManager, TopicSettings topic, SubscriptionSettings subscription)
        {
            var subscriptionDescription =
                new SubscriptionDescription(topic.Path, subscription.Name)
                {
                    RequiresSession = subscription.RequiresSession,
                    LockDuration = TimeSpan.FromSeconds(150),
                };

            try
            {
                namespaceManager.CreateSubscription(subscriptionDescription);
            }
            catch (MessagingEntityAlreadyExistsException) { }
        }

        private static void UpdateSubscriptionIfExists(NamespaceManager namespaceManager, TopicSettings topic, UpdateSubscriptionIfExists action, MessagingFactory messagingFactory)
        {
            if (string.IsNullOrWhiteSpace(action.Name)) throw new ArgumentException("action");
            if (string.IsNullOrWhiteSpace(action.SqlFilter)) throw new ArgumentException("action");

            UpdateSqlFilter(namespaceManager, action.SqlFilter, action.Name, topic.Path, messagingFactory);
        }

        private static void UpdateRules(NamespaceManager namespaceManager, TopicSettings topic, SubscriptionSettings subscription, MessagingFactory messagingFactory)
        {
            string sqlExpression = null;
            if (!string.IsNullOrWhiteSpace(subscription.SqlFilter))
            {
                sqlExpression = subscription.SqlFilter;
            }

            UpdateSqlFilter(namespaceManager, sqlExpression, subscription.Name, topic.Path, messagingFactory);
        }

        private static void UpdateSqlFilter(NamespaceManager namespaceManager, string sqlExpression, string subscriptionName, string topicPath, MessagingFactory messagingFactory)
        {
            bool needsReset = false;
            List<RuleDescription> existingRules;
            try
            {
                existingRules = namespaceManager.GetRules(topicPath, subscriptionName).ToList();
            }
            catch (MessagingEntityNotFoundException)
            {
                // the subscription does not exist, no need to update rules.
                return;
            }
            if (existingRules.Count != 1)
            {
                needsReset = true;
            }
            else
            {
                var existingRule = existingRules.First();
                if (sqlExpression != null && existingRule.Name == RuleDescription.DefaultRuleName)
                {
                    needsReset = true;
                }
                else if (sqlExpression == null && existingRule.Name != RuleDescription.DefaultRuleName)
                {
                    needsReset = true;
                }
                else if (sqlExpression != null && existingRule.Name != RuleName)
                {
                    needsReset = true;
                }
                else if (sqlExpression != null && existingRule.Name == RuleName)
                {
                    var filter = existingRule.Filter as SqlFilter;
                    if (filter == null || filter.SqlExpression != sqlExpression)
                    {
                        needsReset = true;
                    }
                }
            }

            if (needsReset)
            {
                MessagingFactory factory = messagingFactory;
                try
                {
                    //factory = MessagingFactory.Create(namespaceManager.Address, namespaceManager.Settings.TokenProvider);
                    SubscriptionClient client = null;
                    try
                    {
                        client = factory.CreateSubscriptionClient(topicPath, subscriptionName);

                        // first add the default rule, so no new messages are lost while we are updating the subscription
                        TryAddRule(client, new RuleDescription(RuleDescription.DefaultRuleName, new TrueFilter()));

                        // then delete every rule but the Default one
                        foreach (var existing in existingRules.Where(x => x.Name != RuleDescription.DefaultRuleName))
                        {
                            TryRemoveRule(client, existing.Name);
                        }

                        if (sqlExpression != null)
                        {
                            // Add the desired rule.
                            TryAddRule(client, new RuleDescription(RuleName, new SqlFilter(sqlExpression)));

                            // once the desired rule was added, delete the default rule.
                            TryRemoveRule(client, RuleDescription.DefaultRuleName);
                        }
                    }
                    finally
                    {
                        if (client != null) client.Close();
                    }
                }
                finally
                {
                    if (factory != null) factory.Close();
                }
            }
        }

        private static void TryAddRule(SubscriptionClient client, RuleDescription rule)
        {
            // try / catch is because there could be other processes initializing at the same time.
            try
            {
                client.AddRule(rule);
            }
            catch (MessagingEntityAlreadyExistsException) { }
        }

        private static void TryRemoveRule(SubscriptionClient client, string ruleName)
        {
            // try / catch is because there could be other processes initializing at the same time.
            try
            {
                client.RemoveRule(ruleName);
            }
            catch (MessagingEntityNotFoundException) { }
        }
    }
}

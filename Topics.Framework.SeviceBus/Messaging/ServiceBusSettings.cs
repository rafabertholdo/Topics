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
    using Microsoft.ServiceBus;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Simple settings class to configure the connection to the Windows Azure Service Bus.
    /// </summary>
    [XmlRoot("ServiceBus", Namespace = InfrastructureSettings.XmlNamespace)]
    public class ServiceBusSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusSettings"/> class.
        /// </summary>
        public ServiceBusSettings()
        {
            this.ServiceUriScheme = string.Empty;
            this.ServiceNamespace = string.Empty;
            this.ServicePath = string.Empty;
            this.ConnectionString = string.Empty;
            this.TokenIssuer = string.Empty;
            this.TokenAccessKey = string.Empty;
            this.IsAzure = false;

            //this.Topics = new List<TopicSettings>();
        }

        /// <summary>
        /// Gets or sets the service URI scheme.
        /// </summary>
        public string ServiceUriScheme { get; set; }
        /// <summary>
        /// Gets or sets the service namespace.
        /// </summary>
        public string ServiceNamespace { get; set; }
        /// <summary>
        /// Gets or sets the service path.
        /// </summary>
        public string ServicePath { get; set; }
        /// <summary>
        /// Gets or sets the token issuer.
        /// </summary>
        public string TokenIssuer { get; set; }
        /// <summary>
        /// Gets or sets the token access key.
        /// </summary>
        public string TokenAccessKey { get; set; }

        /// <summary>
        /// Gets or sets the connection string to create the On Premisses Service Bus
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the flag if the information provided in other properties applies to Azure or Windows Service Bus
        /// </summary>
        public bool IsAzure { get; set; }

        /// <summary>
        /// Gets or sets the UserName to connect to the Service Bus STS endpoint
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Password to connect to the Service Bus STS endpoint
        /// </summary>
        public string Password { get; set; }

        //[XmlArray(ElementName = "Topics", Namespace = InfrastructureSettings.XmlNamespace)]
        //[XmlArrayItem(ElementName = "Topic", Namespace = InfrastructureSettings.XmlNamespace)]
        //public List<TopicSettings> Topics { get; set; }

        public NamespaceManager GetNamespaceManager()
        {
            if (this.IsAzure)
            {
                var serviceUri = ServiceBusEnvironment.CreateServiceUri(this.ServiceUriScheme, this.ServiceNamespace, this.ServicePath);
                var tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(this.TokenIssuer, this.TokenAccessKey);
                return new NamespaceManager(serviceUri, tokenProvider);
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                return NamespaceManager.CreateFromConnectionString(this.ConnectionString);
            }
        }

        public MessagingFactory GetMessagingFactory()
        {
            if (this.IsAzure)
            {
                var serviceUri = ServiceBusEnvironment.CreateServiceUri(this.ServiceUriScheme, this.ServiceNamespace, this.ServicePath);
                var tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(this.TokenIssuer, this.TokenAccessKey);
                return MessagingFactory.Create(serviceUri, tokenProvider);
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                return MessagingFactory.CreateFromConnectionString(this.ConnectionString);
            }
        }
    }

    [XmlRoot("Topic", Namespace = InfrastructureSettings.XmlNamespace)]
    public class TopicSettings
    {
        public TopicSettings()
        {
            this.Subscriptions = new List<SubscriptionSettings>();
            this.MigrationSupport = new List<UpdateSubscriptionIfExists>();
        }

        [XmlAttribute]
        public bool IsEventBus { get; set; }

        [XmlAttribute]
        public string Path { get; set; }

        [XmlIgnore]
        public TimeSpan DuplicateDetectionHistoryTimeWindow { get; set; }

        [XmlElement("Subscription", Namespace = InfrastructureSettings.XmlNamespace)]
        public List<SubscriptionSettings> Subscriptions { get; set; }

        [XmlArray(ElementName = "MigrationSupport", Namespace = InfrastructureSettings.XmlNamespace)]
        [XmlArrayItem(ElementName = "UpdateSubscriptionIfExists", Namespace = InfrastructureSettings.XmlNamespace)]
        public List<UpdateSubscriptionIfExists> MigrationSupport { get; set; }

        /// <summary>
        /// Don't access this property directly. Use the properly typed 
        /// <see cref="DuplicateDetectionHistoryTimeWindow"/> instead.
        /// </summary>
        /// <remarks>
        /// XmlSerializer still doesn't know how to convert TimeSpan... 
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlAttribute("DuplicateDetectionHistoryTimeWindow")]
        public string XmlDuplicateDetectionHistoryTimeWindow
        {
            get { return this.DuplicateDetectionHistoryTimeWindow.ToString("hh:mm:ss"); }
            set { this.DuplicateDetectionHistoryTimeWindow = TimeSpan.Parse(value); }
        }
    }

    [XmlRoot("Subscription", Namespace = InfrastructureSettings.XmlNamespace)]
    public class SubscriptionSettings
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool RequiresSession { get; set; }

        [XmlAttribute]
        public string SqlFilter { get; set; }
    }

    [XmlRoot("UpdateSubscriptionIfExists", Namespace = InfrastructureSettings.XmlNamespace)]
    public class UpdateSubscriptionIfExists
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string SqlFilter { get; set; }
    }

    public class NamespaceSettings
    {
        public IEnumerable<Uri> ServiceUris { get; set; }
        public TokenProvider TokenProvider { get; set; }
    }
}

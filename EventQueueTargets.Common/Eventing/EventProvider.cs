using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace EventQueueTargets.Common.Eventing
{
    /// <summary>Base provider class for eventing operations.</summary>
    public class EventProvider : Sitecore.Eventing.EventProvider
    {
        public List<string> SystemDatabases { get; set; }

        /// <summary>Initializes the provider.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The name of the provider is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The name of the provider has a length of zero.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)" /> on a provider after the provider has already been initialized.
        /// </exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            SystemDatabases = GetValue("systemDatabaseName", config)
                                       .Split('|')
                                       .Where(x => !string.IsNullOrWhiteSpace(x))
                                       .ToList();
        }

        /// <summary>Queues the event.</summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The @event.</param>
        public new void QueueEvent<TEvent>(TEvent @event)
        {
            QueueEvent(@event, true, false);
        }

        /// <summary>Queues the event.</summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The @event.</param>
        /// <param name="addToGlobalQueue">if set to <c>true</c> add the event to the global queue.</param>
        /// <param name="addToLocalQueue">if set to <c>true</c> add the event to the local queue.</param>
        public new void QueueEvent<TEvent>(TEvent @event, bool addToGlobalQueue, bool addToLocalQueue)
        {
            Assert.ArgumentNotNull(@event, "event");
            foreach (var systemDatabase in SystemDatabases)
            {
                Assert.IsNotNullOrEmpty(systemDatabase, "Cannot find appropriate database for an event: " + @event);
                Factory.GetDatabase(systemDatabase).RemoteEvents.Queue.QueueEvent(@event, addToGlobalQueue, addToLocalQueue);
            }
        }
    }
}

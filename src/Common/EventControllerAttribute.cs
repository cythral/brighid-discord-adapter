using System;

namespace Brighid.Discord
{
    /// <summary>
    /// Annotates an <see iref="IEventController" />.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventControllerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventControllerAttribute" /> class.
        /// </summary>
        /// <param name="eventType">The event type to control.</param>
        public EventControllerAttribute(Type eventType)
        {
            EventType = eventType;
        }

        /// <summary>
        /// Gets or sets the event type to control.
        /// </summary>
        public Type EventType { get; set; }
    }
}

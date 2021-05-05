using System;

namespace Brighid.Discord
{
    /// <summary>
    /// Attribute used to denote a Gateway Event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class GatewayEventAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayEventAttribute" /> class.
        /// </summary>
        /// <param name="opCode">The op code for this event.</param>
        public GatewayEventAttribute(GatewayOpCode opCode)
        {
            OpCode = opCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayEventAttribute" /> class.
        /// </summary>
        /// <param name="opCode">The op code for this event.</param>
        /// <param name="eventName">The name of the event.</param>
        public GatewayEventAttribute(GatewayOpCode opCode, string eventName)
        {
            OpCode = opCode;
            EventName = eventName;
        }

        /// <summary>
        /// Gets or sets the gateway op code.
        /// </summary>
        public GatewayOpCode OpCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        public string? EventName { get; set; }
    }
}

using System.Text.Json.Serialization;

using Brighid.Discord.Adapter.Serialization;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Sent on connection to the websocket. Defines the heartbeat interval that the client should heartbeat to.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Heartbeat)]
    [JsonConverter(typeof(HeartbeatEventConverter))]
    public struct HeartbeatEvent : IGatewayEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeartbeatEvent" /> struct.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the heartbeat.</param>
        public HeartbeatEvent(int sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }

        /// <summary>
        /// Gets or sets the sequence number of the heartbeat.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Converts an integer value to a HeartbeatEvent.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the heartbeat.</param>
        public static implicit operator HeartbeatEvent(int sequenceNumber)
        {
            return new HeartbeatEvent(sequenceNumber);
        }

        /// <summary>
        /// Converts a Heartbeat Event to a decimal.
        /// </summary>
        /// <param name="event">The heartbeat event to cast.</param>
        public static implicit operator decimal(HeartbeatEvent @event)
        {
            return @event.SequenceNumber;
        }

        /// <summary>
        /// Equals operator for Heartbeat Events.
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <returns>true if the events are equal, or false if not.</returns>
        public static bool operator ==(HeartbeatEvent a, HeartbeatEvent b)
        {
            return a.SequenceNumber == b.SequenceNumber;
        }

        /// <summary>
        /// Not equals operator for Heartbeat Events.
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <returns>true if the events are not equal, or false if they are.</returns>
        public static bool operator !=(HeartbeatEvent a, HeartbeatEvent b)
        {
            return a.SequenceNumber != b.SequenceNumber;
        }

        /// <summary>
        /// Gets a hash code of the Heartbeat Event.
        /// </summary>
        /// <returns>The resulting hash code.</returns>
        public override int GetHashCode()
        {
            return new { SequenceNumber }.GetHashCode();
        }

        /// <summary>
        /// Determines if this heartbeat event equals another one.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>True if <paramref name="obj"/> is equal to this heartbeat event, or false if not.</returns>
        public override bool Equals(object? obj)
        {
            return obj is HeartbeatEvent heartbeatEvent &&
                this == heartbeatEvent;
        }
    }
}

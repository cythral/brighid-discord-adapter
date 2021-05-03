using Brighid.Discord.Models;

namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <summary>
    /// An event that occurred with a Discord message.
    /// </summary>
    public interface IMessageEvent
    {
        /// <summary>
        /// Gets or sets the event's inner message.
        /// </summary>
        Message Message { get; set; }
    }
}

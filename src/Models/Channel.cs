using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Represents a guild or DM channel within Discord.
    /// </summary>
    public struct Channel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> struct.
        /// </summary>
        /// <param name="id">Id of the channel.</param>
        public Channel(Snowflake id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets the id of this channel.
        /// </summary>
        [JsonPropertyName("id")]
        public Snowflake Id { get; set; }
    }
}

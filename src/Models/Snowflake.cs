using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Snowflake ID.
    /// </summary>
    [JsonConverter(typeof(SnowflakeConverter))]
    public readonly struct Snowflake
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Snowflake" /> struct.
        /// </summary>
        /// <param name="value">The value of the snowflake.</param>
        public Snowflake(ulong value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the snowflake value.
        /// </summary>
        public ulong Value { get; init; }
    }
}

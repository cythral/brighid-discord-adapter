using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A message interaction.
    /// </summary>
    public struct Interaction
    {
        /// <summary>
        /// Gets or sets id of the interaction.
        /// </summary>
        [JsonPropertyName("id")]
        public Snowflake Id { get; set; }

        /// <summary>
        /// Gets or sets the type of interaction.
        /// </summary>
        [JsonPropertyName("type")]
        public InteractionType Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the ApplicationCommand.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user who invoked the interaction.
        /// </summary>
        [JsonPropertyName("user")]
        public User User { get; set; }
    }
}

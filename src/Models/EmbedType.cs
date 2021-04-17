using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A type of message embed.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmbedType
    {
        /// <summary>
        /// Generic embed rendered from embed attributes.
        /// </summary>
        [EnumMember(Value = "rich")]
        Rich,

        /// <summary>
        /// Image embed.
        /// </summary>
        [EnumMember(Value = "image")]
        Image,

        /// <summary>
        /// Video embed.
        /// </summary>
        [EnumMember(Value = "video")]
        Video,

        /// <summary>
        /// Animated gif image embed rendered as a video embed.
        /// </summary>
        [EnumMember(Value = "gifv")]
        AnimatedGif,

        /// <summary>
        /// Article embed.
        /// </summary>
        [EnumMember(Value = "article")]
        Article,

        /// <summary>
        /// Link embed.
        /// </summary>
        [EnumMember(Value = "link")]
        Link,
    }
}

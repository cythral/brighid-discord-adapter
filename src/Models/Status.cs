using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// The status of a user on discord.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Status
    {
        /// <summary>
        /// Status indicating a user is online.
        /// </summary>
        [EnumMember(Value = "online")]
        Online,

        /// <summary>
        /// Status indicating a user has do not disturb mode on.
        /// </summary>
        [EnumMember(Value = "dnd")]
        DoNotDisturb,

        /// <summary>
        /// Status indicating a user is AFK.
        /// </summary>
        [EnumMember(Value = "idle")]
        Idle,

        /// <summary>
        /// Status indicating a user is invisible and shown as offline.
        /// </summary>
        [EnumMember(Value = "invisible")]
        Invisible,

        /// <summary>
        /// Status indicating a user is offline.
        /// </summary>
        [EnumMember(Value = "offline")]
        Offline,
    }
}

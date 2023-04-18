using System.Text.Json.Serialization;

namespace Brighid.Discord.Models.Payloads
{
    /// <summary>
    /// Payload sent to create a new message.
    /// </summary>
    public struct CreateMessagePayload
    {
        /// <summary>
        /// Gets or sets the message contents (up to 2000 characters).
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this message is a text to speech message.
        /// </summary>
        [JsonPropertyName("tts")]
        public bool? IsTextToSpeech { get; set; }

        /// <summary>
        /// Gets or sets the contents of the file being sent.
        /// </summary>
        [JsonPropertyName("file")]
        public string? FileContents { get; set; }

        /// <summary>
        /// Gets or sets embedded rich content.
        /// </summary>
        [JsonPropertyName("embeds")]
        public Embed[]? Embeds { get; set; }

        /// <summary>
        /// Gets or sets JSON encoded body of non-file params.
        /// </summary>
        [JsonPropertyName("payload_json")]
        public string? PayloadJson { get; set; }

        /// <summary>
        /// Gets or sets a reference to a message being replied to.
        /// </summary>
        public MessageReference? MessageReference { get; set; }
    }
}

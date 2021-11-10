using System.Diagnostics.CodeAnalysis;

namespace Brighid.Discord.Adapter.Messages
{
    /// <summary>
    /// Options to use for the <see cref="SnsMessageEmitter" />.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class SnsMessageEmitterOptions
    {
        /// <summary>
        /// Gets or sets the ARN of the topic to send messages to.
        /// </summary>
        public string TopicArn { get; set; } = string.Empty;
    }
}

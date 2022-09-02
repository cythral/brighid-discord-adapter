using Lambdajection.Attributes;
using Lambdajection.Encryption;

namespace Brighid.Discord.AlarmNotifier
{
    [LambdaOptions(typeof(Handler), "AlarmNotifier")]
    public class Config
    {
        [Encrypted]
        public string DiscordWebhook { get; set; } = string.Empty;

        public string Environment { get; set; } = string.Empty;
    }
}

using Lambdajection.Attributes;

namespace Brighid.Discord.CacheExpirer
{
    [LambdaOptions(typeof(Handler), "Expirer")]
    public class CacheExpirerOptions
    {
        public string AdapterUrl { get; set; } = "discord.brigh.id";
    }
}

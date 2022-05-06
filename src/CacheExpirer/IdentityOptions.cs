using Lambdajection.Attributes;
using Lambdajection.Encryption;

namespace Brighid.Discord.CacheExpirer
{
    [LambdaOptions(typeof(Handler), "Identity")]
    public class IdentityOptions : Identity.Client.IdentityConfig
    {
        [Encrypted]
        public override string ClientSecret { get; set; } = string.Empty;
    }
}

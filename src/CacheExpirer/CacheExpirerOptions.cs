using System;

using Lambdajection.Attributes;

namespace Brighid.Discord.CacheExpirer
{
    [LambdaOptions(typeof(Handler), "Expirer")]
    public class CacheExpirerOptions
    {
        public Uri AdapterUrl { get; set; } = new Uri("http://discord.brigh.id");
    }
}

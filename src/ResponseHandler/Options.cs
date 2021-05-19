using System;

using Lambdajection.Attributes;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    [LambdaOptions(typeof(Handler), "ResponseHandler")]
    public class Options
    {
        public Uri QueueUrl { get; set; } = new Uri("http://localhost/");
    }
}

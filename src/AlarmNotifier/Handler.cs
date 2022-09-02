using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.ResponseHandler;
using Brighid.Discord.Models;

using Lambdajection.Attributes;
using Lambdajection.Sns;

using Microsoft.Extensions.Options;

namespace Brighid.Discord.AlarmNotifier
{
    [SnsEventHandler(typeof(Startup))]
    public partial class Handler
    {
        private readonly HttpClient httpClient;
        private readonly Config config;

        public Handler(
            HttpClient httpClient,
            IOptions<Config> config
        )
        {
            this.httpClient = httpClient;
            this.config = config.Value;
        }

        public async Task<string> Handle(SnsMessage<CloudWatchAlarm> request, CancellationToken cancellationToken)
        {
            var alarmStatus = request.Message.NewStateValue;
            var alarmName = request.Message.AlarmName;
            var metricNamespace = request.Message.Trigger.Namespace;
            var metricName = request.Message.Trigger.MetricName;
            var payload = new ExecuteWebhookPayload
            {
                Embeds = new Embed[]
                {
                    new Embed
                    {
                        Title = $"{alarmStatus}: {alarmName}",
                        Description = request.Message.NewStateReason,
                        Color = alarmStatus == "ALARM" ? 15548997 : 5763719,
                        Fields = new EmbedField[]
                        {
                            new EmbedField { Name = "Description", Value = request.Message.AlarmDescription },
                            new EmbedField { Name = "Metric", Value = $"{metricNamespace}/{metricName}" },
                            new EmbedField { Name = "Environment", Value = config.Environment },
                        },
                    },
                },
            };

            Console.WriteLine(JsonSerializer.Serialize(payload, JsonContext.Default.ExecuteWebhookPayload));

            var response = await httpClient.PostAsJsonAsync(config.DiscordWebhook, payload, JsonContext.Default.ExecuteWebhookPayload, cancellationToken);
            response.EnsureSuccessStatusCode();
            return string.Empty;
        }
    }
}

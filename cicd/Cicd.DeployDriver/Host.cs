using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;

using Brighid.Discord.Cicd.Utils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Cicd.DeployDriver
{
    /// <inheritdoc />
    public class Host : IHost
    {
        private readonly StackDeployer deployer;
        private readonly EcrUtils ecrUtils;
        private readonly CommandLineOptions options;
        private readonly IHostApplicationLifetime lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Host" /> class.
        /// </summary>
        /// <param name="deployer">Service for deploying cloudformation stacks.</param>
        /// <param name="ecrUtils">Utilities for interacting with ECR.</param>
        /// <param name="options">Command line options.</param>
        /// <param name="lifetime">Service that controls the application lifetime.</param>
        /// <param name="serviceProvider">Object that provides access to the program's services.</param>
        public Host(
            StackDeployer deployer,
            EcrUtils ecrUtils,
            IOptions<CommandLineOptions> options,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider
        )
        {
            this.deployer = deployer;
            this.ecrUtils = ecrUtils;
            this.options = options.Value;
            this.lifetime = lifetime;
            Services = serviceProvider;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Config? config = null;

            await Step($"Pull {options.Environment} config", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var key = $"{options.ArtifactsLocation!.AbsolutePath.TrimStart('/')}/config.{options.Environment}.json";
                var s3 = new AmazonS3Client();
                var request = new GetObjectRequest
                {
                    BucketName = options.ArtifactsLocation!.Host,
                    Key = key,
                };

                var response = await s3.GetObjectAsync(request, cancellationToken);
                config = await JsonSerializer.DeserializeAsync<Config>(response.ResponseStream, cancellationToken: cancellationToken);

                Console.WriteLine("Loaded configuration from S3.");
            });

            var image = config!.Parameters!["Image"]!;
            var repository = image.Split(':')[0];
            var environmentTag = repository + ':' + options.Environment;

            await Step($"Deploy template to {options.Environment}", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var context = new DeployContext
                {
                    StackName = "brighid-discord-adapter",
                    TemplateURL = $"https://{options.ArtifactsLocation!.Host}.s3.amazonaws.com{options.ArtifactsLocation!.AbsolutePath}/template.yml",
                    Parameters = config?.Parameters ?? new(),
                    Capabilities = { "CAPABILITY_IAM", "CAPABILITY_AUTO_EXPAND" },
                    Tags = config?.Tags ?? new(),
                };

                await deployer.Deploy(context, cancellationToken);
            });

            await Step("Login to ECR", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ecrUtils.DockerLogin(repository, cancellationToken);
            });

            await Step("[Tag Image] Pulling Existing Image", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pullCommand = new Command("docker pull", arguments: new[] { image });
                await pullCommand.RunOrThrowError("Could not pull image from ECR.");
            });

            await Step("[Tag Image] Retag Image", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tagCommand = new Command("docker tag", arguments: new[] { image, environmentTag });
                await tagCommand.RunOrThrowError("Could not retag image with environment.");
            });

            await Step("[Tag Image] Push Retagged Image", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pushCommand = new Command("docker push", arguments: new[] { environmentTag });
                await pushCommand.RunOrThrowError("Could not push retagged image.");
            });

            await Step("[Cleanup] Logout of ECR", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command("docker logout", arguments: new[] { repository });
                await command.RunOrThrowError("Could not logout of ECR.");
            });

            lifetime.StopApplication();
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private static async Task Step(string title, Func<Task> action)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{title} ==========\n");
            Console.ResetColor();

            await action();
        }
    }
}

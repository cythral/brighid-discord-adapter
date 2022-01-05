using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.ECR;
using Amazon.SecurityToken;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Cicd.BuildDriver
{
    /// <inheritdoc />
    public class Host : IHost
    {
        private static readonly string ParametersDirectory = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "cicd/Parameters/";
        private static readonly string IntermediateOutputDirectory = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "obj/Cicd.Driver/";
        private static readonly string ToolkitStack = "cdk-toolkit";
        private static readonly string OutputsFile = IntermediateOutputDirectory + "cdk.outputs.json";
        private readonly CommandLineOptions options;
        private readonly IHostApplicationLifetime lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Host" /> class.
        /// </summary>
        /// <param name="options">Command line options.</param>
        /// <param name="lifetime">Service that controls the application lifetime.</param>
        /// <param name="serviceProvider">Object that provides access to the program's services.</param>
        public Host(
            IOptions<CommandLineOptions> options,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider
        )
        {
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
            Directory.SetCurrentDirectory(ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "cicd/Cicd.Artifacts");
            Directory.CreateDirectory(ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "bin/Cicd");
            var accountNumber = await GetCurrentAccountNumber(cancellationToken);

            await Step("Bootstrapping CDK", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command("cdk bootstrap", new Dictionary<string, object>
                {
                    ["--toolkit-stack-name"] = ToolkitStack,
                });

                await command.RunOrThrowError(
                    errorMessage: "Could not bootstrap CDK.",
                    cancellationToken: cancellationToken
                );
            });

            await Step("Deploying Artifacts Stack", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command("cdk deploy", new Dictionary<string, object>
                {
                    ["--toolkit-stack-name"] = ToolkitStack,
                    ["--require-approval"] = "never",
                    ["--outputs-file"] = OutputsFile,
                });

                await command.RunOrThrowError(
                    errorMessage: "Failed to deploy Artifacts Stack.",
                    cancellationToken: cancellationToken
                );
            });

            var outputs = await GetOutputs(cancellationToken);
            var tag = $"{outputs.ImageRepositoryUri}:{options.Version}";

            await Step("Logging into ECR", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var ecr = new AmazonECRClient();
                var response = await ecr.GetAuthorizationTokenAsync(new(), cancellationToken);
                var token = response.AuthorizationData.ElementAt(0);

                var command = new Command(
                    command: "docker login",
                    options: new Dictionary<string, object>
                    {
                        ["--username"] = "AWS",
                        ["--password-stdin"] = true,
                    },
                    arguments: new[] { outputs.ImageRepositoryUri }
                );

                var passwordBytes = Convert.FromBase64String(token.AuthorizationToken);
                var password = Encoding.ASCII.GetString(passwordBytes)[4..];

                await command.RunOrThrowError(
                    errorMessage: "Failed to login to ECR.",
                    input: password,
                    cancellationToken: cancellationToken
                );
            });

            await Step("Building Docker Image", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "docker build",
                    options: new Dictionary<string, object>
                    {
                        ["--tag"] = tag,
                        ["--file"] = $"{ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory}Dockerfile",
                    },
                    arguments: new[] { ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory }
                );

                await command.RunOrThrowError(
                    errorMessage: "Failed to build Docker Image.",
                    cancellationToken: cancellationToken
                );
            });

            await Step("Pushing Docker Image", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "docker push",
                    arguments: new[] { tag }
                );

                await command.RunOrThrowError(
                    errorMessage: "Failed to push Docker Image.",
                    cancellationToken: cancellationToken
                );
            });

            await Step("Create Config Files", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                await CreateConfigFile("dev", tag, cancellationToken);
                await CreateConfigFile("prod", tag, cancellationToken);
            });

            await Step("Package Template", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "aws cloudformation package",
                    options: new Dictionary<string, object>
                    {
                        ["--template-file"] = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "template.yml",
                        ["--s3-bucket"] = outputs.BucketName,
                        ["--s3-prefix"] = options.Version,
                        ["--output-template-file"] = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "bin/Cicd/template.yml",
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not package CloudFormation template.",
                    cancellationToken: cancellationToken
                );
            });

            await Step("Upload Artifacts to S3", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "aws s3 cp",
                    options: new Dictionary<string, object>
                    {
                        ["--recursive"] = true,
                    },
                    arguments: new[]
                    {
                        $"{ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory}bin/Cicd",
                        $"s3://{outputs.BucketName}/{options.Version}",
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not upload artifacts to S3.",
                    cancellationToken: cancellationToken
                );

                Console.WriteLine($"::set-output name=artifacts-location::s3://{outputs.BucketName}/{options.Version}");
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

        private static async Task<string> GetCurrentAccountNumber(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sts = new AmazonSecurityTokenServiceClient();
            var response = await sts.GetCallerIdentityAsync(new(), cancellationToken);
            return response.Account;
        }

        private static async Task<Outputs> GetOutputs(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var outputsFileStream = File.OpenRead(OutputsFile);
            var contents = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(outputsFileStream, cancellationToken: cancellationToken);
            var outputsText = contents!["ArtifactsStack"].GetRawText();

            return JsonSerializer.Deserialize<Outputs>(outputsText)!;
        }

        private static async Task CreateConfigFile(string environment, string imageTag, CancellationToken cancellationToken)
        {
            var parametersFile = File.OpenRead(ParametersDirectory + environment + ".json");
            var parameters = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(parametersFile, cancellationToken: cancellationToken) ?? throw new Exception("Could not read parameters from file.");
            parameters["Image"] = imageTag;
            parameters["DotnetVersion"] = DotnetSdkVersionAttribute.ThisAssemblyDotnetSdkVersion;
            parameters["LambdajectionVersion"] = LambdajectionVersionAttribute.ThisAssemblyLambdajectionVersion;

            var config = new Config
            {
                Parameters = parameters,
                Tags = new Dictionary<string, string>
                {
                    ["Name"] = "Brighid Discord Adapter",
                    ["Owner"] = "Cythral",
                    ["Contact:Name"] = "Talen Fisher",
                    ["Contact:Email"] = "talen.fisher@cythral.com",
                },
            };

            var destinationFilePath = $"{ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory}bin/Cicd/config.{environment}.json";
            using var destinationFile = File.OpenWrite(destinationFilePath);
            await JsonSerializer.SerializeAsync(destinationFile, config, cancellationToken: cancellationToken);
            Console.WriteLine($"Created config file for {environment} at {destinationFilePath}.");
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

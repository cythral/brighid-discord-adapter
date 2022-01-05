using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Amazon.ECR;

namespace Brighid.Discord.Cicd.Utils
{
    /// <summary>
    /// Utilities for interacting with ECR.
    /// </summary>
    public class EcrUtils
    {
        private readonly IAmazonECR ecr = new AmazonECRClient();

        /// <summary>
        /// Logs into the given ECR repository.
        /// </summary>
        /// <param name="repository">The repository to login to.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task DockerLogin(string repository, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await ecr.GetAuthorizationTokenAsync(new(), cancellationToken);
            var token = response.AuthorizationData.ElementAt(0);

            var command = new Command(
                command: "docker login",
                options: new Dictionary<string, object>
                {
                    ["--username"] = "AWS",
                    ["--password-stdin"] = true,
                },
                arguments: new[] { repository }
            );

            var passwordBytes = Convert.FromBase64String(token.AuthorizationToken);
            var password = Encoding.ASCII.GetString(passwordBytes)[4..];

            await command.RunOrThrowError(
                errorMessage: "Failed to login to ECR.",
                input: password,
                cancellationToken: cancellationToken
            );
        }
    }
}

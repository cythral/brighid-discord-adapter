using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Organizations;
using Amazon.Organizations.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.SimpleSystemsManagement;

using FluentAssertions;

using Lambdajection.Core;

using NUnit.Framework;

namespace Brighid.Identity.Resources.ContractTests
{
    [Category("Contract")]
    public class LambdaLayerContractTests
    {
        public static readonly object[] TestCases =
        {
            new object[] { $"/lambdajection/{GetLambdajectionVersion()}/layer-arn", "Development" },
            new object[] { $"/lambdajection/{GetLambdajectionVersion()}/layer-arn", "Production" },
            new object[] { $"/dotnet/{GetDotnetSdkVersion()}/layer-arn", "Development" },
            new object[] { $"/dotnet/{GetDotnetSdkVersion()}/layer-arn", "Production" },
        };

        private static readonly Dictionary<string, Credentials> CredentialStore = new();

        public static async Task<string> GetAccountId(string env)
        {
            var organizationsClient = new AmazonOrganizationsClient();
            var listAccountsResponse = await organizationsClient.ListAccountsAsync(new ListAccountsRequest());
            var query = from account in listAccountsResponse.Accounts
                        where account.Name == $"Cythral {env}"
                        select account.Id;

            return query.First();
        }

        public static async Task<Credentials> GetCredentials(string env)
        {
            if (!CredentialStore.TryGetValue(env, out var credentials))
            {
                var accountId = await GetAccountId(env);
                var securityTokenClient = new AmazonSecurityTokenServiceClient();
                var assumeRoleResponse = await securityTokenClient.AssumeRoleAsync(new AssumeRoleRequest
                {
                    RoleArn = $"arn:aws:iam::{accountId}:role/ContractTester",
                    RoleSessionName = "identity-resources",
                });

                credentials = assumeRoleResponse.Credentials;
                CredentialStore[env] = credentials;
            }

            return credentials;
        }

        public static async Task<IAmazonSimpleSystemsManagement> GetSSMClient(string env)
        {
            var credentials = await GetCredentials(env);
            return new AmazonSimpleSystemsManagementClient(credentials);
        }

        public static async Task<IAmazonLambda> GetLambdaClient(string env)
        {
            var credentials = await GetCredentials(env);
            return new AmazonLambdaClient(credentials);
        }

        public static async Task<string> GetParameterValue(string parameterName, string environment)
        {
            var ssmClient = await GetSSMClient(environment);
            var getParameterResponse = await ssmClient.GetParameterAsync(new() { Name = parameterName });
            return getParameterResponse.Parameter.Value;
        }

        public static string GetLambdajectionVersion()
        {
            var informationVersionAttribute = typeof(ILambda<,>).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!;
            var version = informationVersionAttribute.InformationalVersion;
            return version[0..version.IndexOf('+')];
        }

        public static string GetDotnetSdkVersion()
        {
            var dotnetSdkAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<DotnetSdkAttribute>()!;
            return dotnetSdkAttribute.Version;
        }

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task ParameterShouldExist(string parameterName, string environment)
        {
            var layerArn = await GetParameterValue(parameterName, environment);

            layerArn.Should().NotBeNull();
        }

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task LayerShouldExist(string parameterName, string environment)
        {
            var layerArn = await GetParameterValue(parameterName, environment);
            var lambdaClient = await GetLambdaClient(environment);
            var getLayerResponse = await lambdaClient.GetLayerVersionByArnAsync(new GetLayerVersionByArnRequest { Arn = layerArn });

            getLayerResponse.Should().NotBeNull();
        }
    }
}

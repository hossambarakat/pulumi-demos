using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Serialization;
using TeamTimeZonesInfrastructure.Complete;

namespace TeamTimeZonesInfrastructure.Step2
{
    public class Step2Stack: Stack
    {
        [Output()]
        public Output<string> WebContainer { get; private set; }
        [Output()]
        public Output<string> StaticWebsiteConnection { get; private set; }

        public Step2Stack()
        {
            const string prefix = "teamtimezones";
            var config = new Config();
            var location = config.Get("location") ?? "westus";

            var resourceGroup = new ResourceGroup($"{prefix}-{Deployment.Instance.StackName}", new ResourceGroupArgs()
            {
                Name = $"{prefix}-{Deployment.Instance.StackName}",
                Location = location
            });

            //Static Website
            var staticWebsiteOutput = new StaticWebsite($"{prefix}{Deployment.Instance.StackName}web",
                new StaticWebsiteArgs()
                {
                    StorageAccountName = resourceGroup.Name
                });
            WebContainer = staticWebsiteOutput.WebContainer;
            StaticWebsiteConnection = staticWebsiteOutput.StaticWebsiteConnection;

            // // Cosmos DB 
            var cosmosDatabaseOutput = CosmosDatabase.Run(
                resourceGroup.Name, prefix, resourceGroup.Location);
        }
    }
}
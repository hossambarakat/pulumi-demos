using System.Collections.Generic;
using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Serialization;

namespace TeamTimeZonesInfrastructure.Complete
{
    public class CompleteStack : Stack
    {
        [Output]
        public Output<string> StaticWebsiteConnection { get; private set; }
        
        [Output]
        public Output<string> FunctionAppEndPoint { get; private set; }
        

        public CompleteStack()
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
            var staticWebsiteOutput = new StaticWebsite($"{prefix}{Deployment.Instance.StackName}web", new StaticWebsiteArgs()
            {
                StorageAccountName = resourceGroup.Name
            });
            StaticWebsiteConnection = staticWebsiteOutput.StaticWebsiteConnection;

            //Cosmos DB
            var cosmosDatabaseOutput = CosmosDatabase.Run(
                resourceGroup.Name, prefix, resourceGroup.Location);
            
            //Azure Function
            var archiveFunction = new ArchiveFunctionApp($"{prefix}-{Deployment.Instance.StackName}",
                new ArchiveFunctionAppArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    Prefix = prefix,
                    FunctionAppLocation = location,
                    FunctionAppFileLocation = "../TeamTimeZones/bin/Debug/netcoreapp3.1/publish/",
                    AppSettings = new InputMap<string>
                    {
                        {"db-account-endpoint", cosmosDatabaseOutput["db-account-endpoint"].Apply(x => x.ToString())},
                        {"db-account-key", cosmosDatabaseOutput["db-account-key"].Apply(x => x.ToString())}
                    }
                });
            FunctionAppEndPoint = archiveFunction.DefaultHostname;
        }
        
    }
}
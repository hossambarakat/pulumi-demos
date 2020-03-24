using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Shared.Protocol;
using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.CosmosDB;
using Pulumi.Azure.CosmosDB.Inputs;
using Pulumi.Serialization;

namespace TeamTimeZonesInfrastructure.Step2
{
    public class Step2Start : Stack
    {
        [Output]
        public Output<string> WebContainer { get; private set; }
        
        public Step2Start()
        {
            const string prefix = Common.Prefix;
            var config = new Config();
            var location = config.Get("location") ?? "southeastasia";
            
            var resourceGroup = new ResourceGroup($"{prefix}-{Deployment.Instance.StackName}", new ResourceGroupArgs()
            {
                Name = $"{prefix}-{Deployment.Instance.StackName}",
                Location = location
            });
            var name = $"{prefix}{Deployment.Instance.StackName}web";
            
            var staticWebsiteStorageAccount = new Pulumi.Azure.Storage.Account(
                name,
                new Pulumi.Azure.Storage.AccountArgs
                {
                    Name = name,
                    ResourceGroupName = resourceGroup.Name,
                    EnableHttpsTrafficOnly = true,
                    AccountReplicationType = "LRS",
                    AccountTier = "Standard",
                    AccountKind = "StorageV2",
                    AccessTier = "Hot"
                });
            
            WebContainer =
                staticWebsiteStorageAccount.PrimaryBlobConnectionString.Apply(async v => await EnableStaticSites(v));
            
            string accountName = $"{prefix}-{Deployment.Instance.StackName}";
            string databaseName = "TeamTimeZones";
            string containerName = "TeamMember";
            var cosmosAccount = new Account(accountName,
                new AccountArgs
                {
                    Name = accountName,
                    ResourceGroupName = resourceGroup.Name,
                    GeoLocations = new List<AccountGeoLocationsArgs>
                    {
                        new AccountGeoLocationsArgs {Location = location, FailoverPriority = 0}
                    },
                    OfferType = "Standard",
                    ConsistencyPolicy = new AccountConsistencyPolicyArgs {ConsistencyLevel = "Session"},
                });

            var database = new SqlDatabase(databaseName,
                new SqlDatabaseArgs
                {
                    Name = databaseName,
                    ResourceGroupName = resourceGroup.Name,
                    AccountName = cosmosAccount.Name
                    
                });

            var container = new SqlContainer($"{prefix}-{Deployment.Instance.StackName}",
                new SqlContainerArgs
                {
                    Name = containerName,
                    ResourceGroupName = resourceGroup.Name,
                    AccountName = cosmosAccount.Name,
                    DatabaseName = database.Name,
                    PartitionKeyPath = "/TimeZone"
                });
        }
        
        static async Task<string> EnableStaticSites(string connectionString)
        {
            if (!Deployment.Instance.IsDryRun)
            {
                var sa = CloudStorageAccount.Parse(connectionString);

                var blobClient = sa.CreateCloudBlobClient();
                var blobServiceProperties = new ServiceProperties
                {
                    StaticWebsite = new StaticWebsiteProperties
                    {
                        Enabled = true,
                        IndexDocument = "index.html",
                        ErrorDocument404Path = "404.html"
                    }
                };
                await blobClient.SetServicePropertiesAsync(blobServiceProperties);
            }

            return "$web";
        }
    }
}
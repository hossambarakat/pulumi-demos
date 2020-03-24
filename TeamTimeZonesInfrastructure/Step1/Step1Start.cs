using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Shared.Protocol;
using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Serialization;

namespace TeamTimeZonesInfrastructure.Step1
{
    public class Step1Start : Stack
    {
        [Output]
        public Output<string> WebContainer { get; private set; }
        
        public Step1Start()
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
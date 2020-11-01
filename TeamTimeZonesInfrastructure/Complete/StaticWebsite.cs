using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Shared.Protocol;
using Pulumi;
using Pulumi.Serialization;

namespace TeamTimeZonesInfrastructure.Complete
{
    public class StaticWebsiteArgs
    {
        public Input<string> StorageAccountName { get; set; }
    }
    public class StaticWebsite : ComponentResource
    {
        [Output]
        public Output<string> WebContainer { get; private set; }
        [Output]
        public Output<string> StaticWebsiteConnection { get; private set; }
        public StaticWebsite(string name, StaticWebsiteArgs args, ComponentResourceOptions? options = null) 
            : base("teamtimezones:azure:staticwebsite", name, options)
        {
            var opts =  new CustomResourceOptions { Parent = this };
            var staticWebsiteStorageAccount = new Pulumi.Azure.Storage.Account(
                name,
                new Pulumi.Azure.Storage.AccountArgs
                {
                    Name = name,
                    ResourceGroupName = args.StorageAccountName,
                    EnableHttpsTrafficOnly = true,
                    AccountReplicationType = "LRS",
                    AccountTier = "Standard",
                    AccountKind = "StorageV2",
                    AccessTier = "Hot"
                }, opts);
            
            WebContainer =
                staticWebsiteStorageAccount.PrimaryBlobConnectionString.Apply(async v => await EnableStaticSites(v));
            StaticWebsiteConnection = staticWebsiteStorageAccount.PrimaryConnectionString;
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
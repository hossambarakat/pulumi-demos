using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Shared.Protocol;
using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;
using Pulumi.Serialization;
using TeamTimeZonesInfrastructure.Complete;

namespace TeamTimeZonesInfrastructure.Step3
{
    public class Step3Start : Stack
    {
        [Output()]
        public Output<string> WebContainer { get; private set; }
        [Output()]
        public Output<string> StaticWebsiteConnection { get; private set; }
        
        [Output()]
        public Output<string> FunctionAppEndPoint { get; private set; }
        
        public Step3Start()
        {
            const string prefix = Common.Prefix;
            var config = new Config();
            var location = config.Get("location") ?? "southeastasia";

            #region Resource Group
            var resourceGroup = new ResourceGroup($"{prefix}-{Deployment.Instance.StackName}", new ResourceGroupArgs()
            {
                Name = $"{prefix}-{Deployment.Instance.StackName}",
                Location = location
            });
            var name = $"{prefix}{Deployment.Instance.StackName}web";
            #endregion

            #region Static Website
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
            StaticWebsiteConnection = staticWebsiteStorageAccount.PrimaryBlobConnectionString;
            #endregion

            #region Cosmos DB
            var cosmosDatabaseOutput = CosmosDatabase.Run(
                resourceGroup.Name, prefix, resourceGroup.Location);
            #endregion

            #region Azure Function
            #region Storage Account
            var storageAccount = new Account($"sa{prefix}{Deployment.Instance.StackName}",
                new AccountArgs
                {
                    Name = $"sa{prefix}{Deployment.Instance.StackName}",
                    ResourceGroupName = resourceGroup.Name,
                    Location = resourceGroup.Location,
                    AccountReplicationType = "LRS",
                    AccountTier = "Standard"
                });
            #endregion

            #region App Service Plan
            var appServicePlan = new Plan($"asp-{prefix}{Deployment.Instance.StackName}",
                new PlanArgs
                {
                    Name = $"asp-{prefix}{Deployment.Instance.StackName}",
                    ResourceGroupName = resourceGroup.Name,
                    Location = resourceGroup.Location,
                    Kind = "FunctionApp",
                    Sku = new PlanSkuArgs
                    {
                        Tier = "Dynamic",
                        Size = "Y1"
                    }
                });
            #endregion

            #region Storage Container
            var container = new Container($"func-code", new ContainerArgs
            {
                StorageAccountName = storageAccount.Name,
                ContainerAccessType = "private",
            });
            #endregion

            #region Function Zip Blob
            var functionAppFileLocation = "../TeamTimeZones/bin/Debug/netcoreapp3.1/publish/";
            var blob = new ZipBlob($"func", new ZipBlobArgs
            {
                StorageAccountName = storageAccount.Name,
                StorageContainerName = container.Name,
                Type = "block",
                Content = new FileArchive(functionAppFileLocation),
            });
            #endregion

            #region Function App
            var codeBlobUrl = SharedAccessSignature.SignedBlobReadUrl(blob, storageAccount);
            var app = new FunctionApp($"app-{prefix}",
                new FunctionAppArgs
                {
                    Name = $"app-{prefix}",
                    ResourceGroupName = resourceGroup.Name,
                    Location = resourceGroup.Location,
                    AppServicePlanId = appServicePlan.Id,
                    StorageConnectionString = storageAccount.PrimaryConnectionString,
                    Version = "~3",
                    AppSettings = new InputMap<string>
                    {
                        {"runtime", "dotnet"},
                        {"WEBSITE_RUN_FROM_PACKAGE", codeBlobUrl},
                        {"db-account-endpoint", cosmosDatabaseOutput["db-account-endpoint"].Apply(x => x.ToString())},
                        {"db-account-key", cosmosDatabaseOutput["db-account-key"].Apply(x => x.ToString())}
                    },
                    SiteConfig = new FunctionAppSiteConfigArgs
                    {
                        Cors = new FunctionAppSiteConfigCorsArgs
                        {
                            AllowedOrigins = "*"
                        }
                    }
                });

            this.FunctionAppEndPoint = app.DefaultHostname;
            #endregion
            #endregion
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
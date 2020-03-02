using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Storage;

namespace serverless_app
{
    public class ArchiveFunctionApp : ComponentResource
    {
        public Output<string> AppId {get; private set;} = null;

        public ArchiveFunctionApp(string name, ArchiveFunctionAppArgs args, ResourceOptions? options = null) 
            : base("myteam:azure:ArchiveFunctionApp", name, options)
        {
            var prefix=args.Prefix;
            //Create storage account
            var storageAccount = new Account($"sa{prefix}",
            new AccountArgs
            {
                ResourceGroupName=args.ResourceGroupName,
                Location = "westus",
                AccountReplicationType="LRS",
                AccountTier="Standard"
            });
            //Create an app server plan
            var appServicePlan = new Plan($"asp-{prefix}",
            new PlanArgs
            {
                ResourceGroupName = args.ResourceGroupName,
                Location = "westus",
                Kind = "FunctionApp",
                Sku = new PlanSkuArgs
                {
                    Tier = "Dynamic",
                    Size="Y1"
                }
            });

            var container = new Container($"zips-func", new ContainerArgs
            {
                StorageAccountName = storageAccount.Name,
                ContainerAccessType = "private",
            });

            var blob = new ZipBlob($"zip-func", new ZipBlobArgs
            {
                StorageAccountName = storageAccount.Name,
                StorageContainerName = container.Name,
                Type = "block",
                Content = new FileArchive(args.FunctionAppLocation),
            });
            var codeBlobUrl = SharedAccessSignature.SignedBlobReadUrl(blob, storageAccount);
            //Create Function Application
            var appSettings = new InputMap<string>
            {
                //{"runtime", "dotnet"},
                {"FUNCTIONS_WORKER_RUNTIME", "node"},
                {"WEBSITE_NODE_DEFAULT_VERSION", "10.14.1"},

                //{"WEBSITE_RUN_FROM_PACKAGE", "https://mikhailworkshop.blob.core.windows.net/zips/app.zip"}
                {"WEBSITE_RUN_FROM_PACKAGE",codeBlobUrl}
            };

            var app = new FunctionApp($"app-{prefix}",
            new FunctionAppArgs
            {
                ResourceGroupName = args.ResourceGroupName,
                Location = "westus",
                AppServicePlanId = appServicePlan.Id,
                StorageConnectionString = storageAccount.PrimaryConnectionString,
                Version="~2",
                AppSettings = appSettings
            });
            this.AppId = app.Id;
        }
    }
    public class ArchiveFunctionAppArgs
    {
        public Input<string> ResourceGroupName {get;set;}
        public string Prefix {get;set;}
        public string FunctionAppLocation {get;set;}
    }
}
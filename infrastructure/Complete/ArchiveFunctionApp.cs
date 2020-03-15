using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Storage;

namespace TeamTimeZonesInfrastructure
{
    public class ArchiveFunctionApp : ComponentResource
    {
        public Output<string> AppId { get; private set; } = null;

        public ArchiveFunctionApp(string name, ArchiveFunctionAppArgs args, ResourceOptions? options = null)
            : base("myteam:azure:ArchiveFunctionApp", name, options)
        {
            var opts = CustomResourceOptions.Merge(options, new CustomResourceOptions { Parent = this });
            var prefix = args.Prefix;

            //Create storage account
            var storageAccount = new Account($"sa{prefix}{Deployment.Instance.StackName}",
                new AccountArgs
                {
                    Name = $"sa{prefix}{Deployment.Instance.StackName}",
                    ResourceGroupName = args.ResourceGroupName,
                    Location = args.FunctionAppLocation,
                    AccountReplicationType = "LRS",
                    AccountTier = "Standard"
                },opts);

            //Create an app server plan
            var appServicePlan = new Plan($"asp-{prefix}{Deployment.Instance.StackName}",
                new PlanArgs
                {
                    Name = $"asp-{prefix}{Deployment.Instance.StackName}",
                    ResourceGroupName = args.ResourceGroupName,
                    Location = args.FunctionAppLocation,
                    Kind = "FunctionApp",
                    Sku = new PlanSkuArgs
                    {
                        Tier = "Dynamic",
                        Size = "Y1"
                    }
                },opts);

            var container = new Container($"func-code", new ContainerArgs
            {
                StorageAccountName = storageAccount.Name,
                ContainerAccessType = "private",
            },opts);

            var blob = new ZipBlob($"func", new ZipBlobArgs
            {
                StorageAccountName = storageAccount.Name,
                StorageContainerName = container.Name,
                Type = "block",
                Content = new FileArchive(args.FunctionAppFileLocation),
            },opts);

            var codeBlobUrl = SharedAccessSignature.SignedBlobReadUrl(blob, storageAccount);

            //Create Function Application
            args.AppSettings.Add("runtime", "dotnet");
            args.AppSettings.Add("WEBSITE_RUN_FROM_PACKAGE", codeBlobUrl);

            var app = new FunctionApp($"app-{prefix}",
                new FunctionAppArgs
                {
                    Name = $"app-{prefix}",
                    ResourceGroupName = args.ResourceGroupName,
                    Location = args.FunctionAppLocation,
                    AppServicePlanId = appServicePlan.Id,
                    StorageConnectionString = storageAccount.PrimaryConnectionString,
                    Version = "~2",
                    AppSettings = args.AppSettings
                }, opts);
            
            this.AppId = app.Id;
        }
    }

    public class ArchiveFunctionAppArgs
    {
        public Input<string> ResourceGroupName { get; set; }
        public string Prefix { get; set; }
        public string FunctionAppFileLocation { get; set; }
        public string FunctionAppLocation { get; set; }
        private InputMap<string>? _appSettings;

        public InputMap<string> AppSettings
        {
            get => _appSettings ?? (_appSettings = new InputMap<string>());
            set => _appSettings = value;
        }
    }
}
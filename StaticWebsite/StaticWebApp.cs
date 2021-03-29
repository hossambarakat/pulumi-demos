using Pulumi;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

namespace StaticWebsite
{
    public class StaticWebApp : ComponentResource
    {
        public StaticWebApp(string name, Input<string> resourceGroupName, ComponentResourceOptions? options = null) 
            : base("mycompany:azure:StaticWebApp", name, options)
        {
            var parentOptions =  new CustomResourceOptions { Parent = this };
            // Create an Azure Storage Account
            var storageAccount = new StorageAccount(name, new StorageAccountArgs
            {
                ResourceGroupName = resourceGroupName,
                EnableHttpsTrafficOnly = true,
                Sku =  new SkuArgs
                {
                    Name = SkuName.Standard_LRS
                },
                AccessTier = AccessTier.Hot,
                Kind = Kind.StorageV2,
            }, parentOptions);

            var staticWebsite = new StorageAccountStaticWebsite(name, new StorageAccountStaticWebsiteArgs()
            {
                ResourceGroupName = resourceGroupName,
                AccountName = storageAccount.Name,
                IndexDocument = "index.html",
                Error404Document = "404.html"
            }, parentOptions);
            
            //Upload the files
            var files =  new[]{"index.html", "404.html"};
            foreach (var file in files)
            {
                var uploadedFile = new Blob($"{name}-{file}", new BlobArgs
                {
                    ResourceGroupName = resourceGroupName,
                    AccountName = storageAccount.Name,
                    ContainerName = staticWebsite.ContainerName,
                    Type = BlobType.Block,
                    Source =  new FileAsset($"./wwwroot/{file}"),
                    ContentType = "text/html"
                }, parentOptions);
            }
            // Export the Web address string for the storage account
            PrimaryWebEndpoint = storageAccount.PrimaryEndpoints.Apply(x=>x.Web);
        }
        [Output]
        public Output<string> PrimaryWebEndpoint { get; private set; }
    }
}
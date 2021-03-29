using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

namespace StaticWebsite
{
    public class WebsiteStack : Stack
    {
        public WebsiteStack()
        {
            // Create an Azure Resource Group
            var resourceGroup = new ResourceGroup("mystaticsite", new ResourceGroupArgs
            {
                Tags = {
                    {"Environment", "Dev"}
                }
            });

            // Create an Azure Storage Account
            var storageAccount = new StorageAccount("mysite", new StorageAccountArgs
            {
                ResourceGroupName = resourceGroup.Name,
                EnableHttpsTrafficOnly = true,
                Sku = new SkuArgs
                {
                    Name = SkuName.Standard_LRS
                },
                AccessTier = AccessTier.Hot,
                Kind = Kind.StorageV2,
            });

            var storageAccountStaticWebsite = new StorageAccountStaticWebsite("mysite",
                new StorageAccountStaticWebsiteArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    AccountName = storageAccount.Name,
                    IndexDocument = "index.html",
                    Error404Document = "404.html"
                });
            
            //Upload the files
            var files =  new[]{"index.html", "404.html"};
            foreach (var file in files)
            {
                var uploadedFile = new Blob(file, new BlobArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    AccountName = storageAccount.Name,
                    ContainerName = storageAccountStaticWebsite.ContainerName,
                    Type = BlobType.Block,
                    Source =  new FileAsset($"./wwwroot/{file}"),
                    ContentType = "text/html"
                });
            }

            // Export the Web address string for the storage account
            PrimaryWebEndpoint = storageAccount.PrimaryEndpoints.Apply(x=>x.Web);
        }
        [Output]
        public Output<string> PrimaryWebEndpoint { get; private set; }
    }
}
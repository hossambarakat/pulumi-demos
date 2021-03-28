using Pulumi;
using Pulumi.Azure.Storage;
using Pulumi.Azure.Storage.Inputs;

namespace StaticWebsite
{
    public class StaticWebApp : ComponentResource
    {
        public StaticWebApp(string name, Input<string> resourceGroupName, ComponentResourceOptions? options = null) 
            : base("mycompany:azure:StaticWebApp", name, options)
        {
            var parentOptions =  new CustomResourceOptions { Parent = this };
            // Create an Azure Storage Account
            var storageAccount = new Account(name, new AccountArgs
            {
                ResourceGroupName = resourceGroupName,
                EnableHttpsTrafficOnly = true,
                AccountReplicationType = "LRS",
                AccountTier = "Standard",
                AccountKind = "StorageV2",
                AccessTier = "Hot",
                StaticWebsite = new AccountStaticWebsiteArgs
                {
                    IndexDocument = "index.html",
                    Error404Document = "404.html"
                }
            }, parentOptions);
            //Upload the files
            var files =  new[]{"index.html", "404.html"};
            foreach (var file in files)
            {
                var uploadedFile = new Blob($"{name}-{file}", new BlobArgs
                {
                    Name = file,
                    StorageAccountName = storageAccount.Name,
                    StorageContainerName = "$web",
                    Type = "Block",
                    Source =  new FileAsset($"./wwwroot/{file}"),
                    ContentType = "text/html"
                }, parentOptions);
            }
            // Export the Web address string for the storage account
            PrimaryWebEndpoint = storageAccount.PrimaryWebEndpoint;
        }
        [Output]
        public Output<string> PrimaryWebEndpoint { get; private set; }
    }
}
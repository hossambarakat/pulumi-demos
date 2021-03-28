using System.Collections.Generic;
using System.Collections.Immutable;
using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;
using Pulumi.Azure.Storage.Inputs;

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
            var storageAccount = new Account("mysite", new AccountArgs
            {
                ResourceGroupName = resourceGroup.Name,
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
            });
            //Upload the files
            var files =  new[]{"index.html", "404.html"};
            foreach (var file in files)
            {
                var uploadedFile = new Blob(file, new BlobArgs
                {
                    Name = file,
                    StorageAccountName = storageAccount.Name,
                    StorageContainerName = "$web",
                    Type = "Block",
                    Source =  new FileAsset($"./wwwroot/{file}"),
                    ContentType = "text/html"
                });
            }
            // Export the Web address string for the storage account
            PrimaryWebEndpoint = storageAccount.PrimaryWebEndpoint;
        }
        [Output]
        public Output<string> PrimaryWebEndpoint { get; private set; }
    }
}
﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;
using Pulumi.Azure.Storage.Inputs;

class Program
{
    static Task<int> Main()
    {
        return Deployment.RunAsync(() => {

            // Create an Azure Resource Group
            var resourceGroup = new ResourceGroup("mystaticsite");

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
            return new Dictionary<string, object?>
            {
                { "staticEndpoint", storageAccount.PrimaryWebEndpoint }
            };
        });
    }
}

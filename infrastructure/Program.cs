using System.Collections.Generic;
using System.Threading.Tasks;

using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.CosmosDB.Inputs;
using serverless_app;
using Pulumi.Azure.CosmosDB;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Shared.Protocol;
using System;

class Program
{
// Creating a Storage Account and a Blob Container
// Zipping up the binaries and uploading them to the blob container
// Calculating a SAS token
// Preparing a Consumption Plan and a Function App using this Consumption Plan
// Configuring the required application settings, including a reference to the zip archive with the SAS token


    static Task<int> Main()
    {
        return Deployment.RunAsync(() => {
            const string prefix="myteam";
            //create resource group
            var resourceGroup = new ResourceGroup($"rg-{prefix}");
            var name="myteam";

            var staticWebsiteStorageAccount = new Pulumi.Azure.Storage.Account("mysite", new Pulumi.Azure.Storage.AccountArgs
            {
                ResourceGroupName = resourceGroup.Name,
                EnableHttpsTrafficOnly = true,
                AccountReplicationType = "LRS",
                AccountTier = "Standard",
                AccountKind = "StorageV2",
                AccessTier = "Hot",
            });

            // We can't enable static sites using Pulumi (it's not exposed in the ARM API).
            // Therefore we have to invoke the Azure SDK from within the Pulumi code to enable the static sites 
            // The code in the Apply method must be idempotent.
            var containerName  = staticWebsiteStorageAccount.PrimaryBlobConnectionString.Apply(async v => await mEnableStaticSites(v) );


        // Cosmos DB Account with multiple replicas
        var cosmosAccount = new Account($"{prefix}-cosmos",
            new AccountArgs
            {
                ResourceGroupName = resourceGroup.Name,
                GeoLocations = new List<AccountGeoLocationsArgs>
                { 
                    new AccountGeoLocationsArgs { Location = "westus", FailoverPriority = 0 }
                },
                OfferType = "Standard",
                ConsistencyPolicy = new AccountConsistencyPolicyArgs { ConsistencyLevel = "Session" },
            });

        var database = new SqlDatabase($"db-{name}",
            new SqlDatabaseArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AccountName = cosmosAccount.Name,
                Name = "myteam",
            });

        var container = new SqlContainer($"sql-{name}",
            new SqlContainerArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AccountName = cosmosAccount.Name,
                DatabaseName = database.Name,
                Name = "timezones",
            });

            var archiveFunction = new ArchiveFunctionApp($"asp-{prefix}",
            new ArchiveFunctionAppArgs
            {
                ResourceGroupName = resourceGroup.Name,
                Prefix = prefix,
                FunctionAppLocation = "./app"
            });
            return new Dictionary<string, object?>
            {
                { "app-id", archiveFunction.AppId},
                { "connection", staticWebsiteStorageAccount.PrimaryBlobConnectionString},
                { "url" , staticWebsiteStorageAccount.PrimaryWebEndpoint},
                {"containerName", containerName}
            };
            

            // //Create storage account
            // var storageAccount = new Account($"sa{prefix}",
            // new AccountArgs
            // {
            //     ResourceGroupName=resourceGroup.Name,
            //     Location = "westus",
            //     AccountReplicationType="LRS",
            //     AccountTier="Standard"
            // });

            // //Create an app server plan
            // var appServicePlan = new Plan($"asp-{prefix}",
            // new PlanArgs
            // {
            //     ResourceGroupName = resourceGroup.Name,
            //     Location = "westus",
            //     Kind = "FunctionApp",
            //     Sku = new PlanSkuArgs
            //     {
            //         Tier = "Dynamic",
            //         Size="Y1"
            //     }
            // });

            // var container = new Container($"zips-func", new ContainerArgs
            // {
            //     StorageAccountName = storageAccount.Name,
            //     ContainerAccessType = "private",
            // });

            // var blob = new ZipBlob($"zip-func", new ZipBlobArgs
            // {
            //     StorageAccountName = storageAccount.Name,
            //     StorageContainerName = container.Name,
            //     Type = "block",
            //     Content = new FileArchive("./app"),
            // });
            // //Create Function Application
            // var appSettings = new InputMap<string>
            // {
            //     //{"runtime", "dotnet"},
            //     {"FUNCTIONS_WORKER_RUNTIME", "node"},
            //     {"WEBSITE_NODE_DEFAULT_VERSION", "10.14.1"},

            //     {"WEBSITE_RUN_FROM_PACKAGE", "https://mikhailworkshop.blob.core.windows.net/zips/app.zip"}
            // };

            

            // var app = new FunctionApp($"app-{prefix}",
            // new FunctionAppArgs
            // {
            //     ResourceGroupName = resourceGroup.Name,
            //     Location = "westus",
            //     AppServicePlanId = appServicePlan.Id,
            //     StorageConnectionString = storageAccount.PrimaryConnectionString,
            //     Version="~2",
            //     AppSettings = appSettings
            // });

            // //Create CosmosDB
            
            // Export the connection string for the storage account
            // return new Dictionary<string, object?>
            // {
            //     { "connectionString", storageAccount.PrimaryConnectionString },
            //     { "app-id", app.Id}
            // };
        });
    }
    static async Task<string> mEnableStaticSites(string connectionString)
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

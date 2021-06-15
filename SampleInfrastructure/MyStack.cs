using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

namespace SampleInfrastructure
{
    public class MyStack : Stack
    {
        public MyStack()
        {
            // Create an Azure Resource Group
            var resourceGroup = new ResourceGroup("resourceGroup", new ResourceGroupArgs()
            {
                Location = "EastUs",
                Tags = new InputMap<string>()
                {
                    {"productname","yes"}
                }
            });

            // Create an Azure Storage Account
            var storageAccount = new StorageAccount("storage", new StorageAccountArgs
            {
                ResourceGroupName = resourceGroup.Name,
                Sku = new SkuArgs
                {
                    Name = SkuName.Standard_LRS
                },
                AccessTier = AccessTier.Hot,
                Kind = Kind.StorageV2,
            });
            Output.Tuple(storageAccount.Name, resourceGroup.Name).Apply(async values =>
            {
                // Export the connection string for the storage account
                var storageAccountKeysResult = await ListStorageAccountKeys.InvokeAsync(new ListStorageAccountKeysArgs()
                {
                    AccountName = values.Item1,
                    ResourceGroupName = values.Item2
                });
                this.PrimaryStorageKey = Output.CreateSecret(storageAccountKeysResult.Keys[0].Value);
            });

        }

        [Output]
        public Output<string> PrimaryStorageKey { get; set; }
        
    }

}
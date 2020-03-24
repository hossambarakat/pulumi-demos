using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;

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
            var storageAccount = new Account("storage", new AccountArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AccountReplicationType = "LRS",
                AccountTier = "Standard"
            });

            // Export the connection string for the storage account
            this.ConnectionString = storageAccount.PrimaryConnectionString;
        }

        [Output]
        public Output<string> ConnectionString { get; set; }
    }

}
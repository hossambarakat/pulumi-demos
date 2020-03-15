using System.Collections.Generic;
using Pulumi;
using Pulumi.Azure.CosmosDB;
using Pulumi.Azure.CosmosDB.Inputs;
using Pulumi.Serialization;

namespace TeamTimeZonesInfrastructure.Complete
{
    public class CosmosDatabase
    {
        // [Output]
        // public Output<string> CosmosEndPoint { get; private set; }
        //
        // [Output]
        // public Output<string> CosmosPrimaryMasterKey { get; private set; }
        public static IDictionary<string, Output<string>> Run(
            Input<string> resourceGroupName, 
            string prefix, 
            Input<string> location
        )
        {
            string accountName = $"{prefix}-{Deployment.Instance.StackName}";
            string databaseName = "TeamTimeZones";
            string containerName = "TeamMember";
            var cosmosAccount = new Account(accountName,
                new AccountArgs
                {
                    Name = accountName,
                    ResourceGroupName = resourceGroupName,
                    GeoLocations = new List<AccountGeoLocationsArgs>
                    {
                        new AccountGeoLocationsArgs {Location = location, FailoverPriority = 0}
                    },
                    OfferType = "Standard",
                    ConsistencyPolicy = new AccountConsistencyPolicyArgs {ConsistencyLevel = "Session"},
                });

            var database = new SqlDatabase(databaseName,
                new SqlDatabaseArgs
                {
                    Name = databaseName,
                    ResourceGroupName = resourceGroupName,
                    AccountName = cosmosAccount.Name
                    
                });

            var container = new SqlContainer($"{prefix}-{Deployment.Instance.StackName}",
                new SqlContainerArgs
                {
                    Name = containerName,
                    ResourceGroupName = resourceGroupName,
                    AccountName = cosmosAccount.Name,
                    DatabaseName = database.Name,
                });
            // CosmosEndPoint = cosmosAccount.Endpoint;
            // CosmosPrimaryMasterKey = cosmosAccount.PrimaryMasterKey;
            
            return new Dictionary<string, Output<string>>
            {
                {"db-account-endpoint", cosmosAccount.Endpoint},
                {"db-account-key", cosmosAccount.PrimaryMasterKey}
            };
        }
    }
}
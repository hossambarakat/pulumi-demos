using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Serialization;
using TeamTimeZonesInfrastructure.Complete;

namespace TeamTimeZonesInfrastructure.Step1
{
    //Demo 1 - Static Website
    public class Step1Stack: Stack
    {
        [Output()]
        public Output<string> WebContainer { get; private set; }
        [Output()]
        public Output<string> StaticWebsiteConnection { get; private set; }
        
        public Step1Stack()
        {
            const string prefix = Common.Prefix;
            var config = new Config();
            var location = config.Get("location") ?? "australiaeast";

            var resourceGroup = new ResourceGroup($"{prefix}-{Deployment.Instance.StackName}", new ResourceGroupArgs()
            {
                Name = $"{prefix}-{Deployment.Instance.StackName}",
                Location = location
            });
            
            //Static Website
            var staticWebsiteOutput = new StaticWebsite($"{prefix}{Deployment.Instance.StackName}web", new StaticWebsiteArgs()
            {
                StorageAccountName = resourceGroup.Name
            });
            WebContainer = staticWebsiteOutput.WebContainer;
            StaticWebsiteConnection = staticWebsiteOutput.StaticWebsiteConnection;
        }
    }
}
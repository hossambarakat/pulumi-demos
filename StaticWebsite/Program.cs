using System.Threading.Tasks;
using Pulumi;
using StaticWebsite;

class Program
{
    static Task<int> Main() => Deployment.RunAsync<WebsiteStack>();
}

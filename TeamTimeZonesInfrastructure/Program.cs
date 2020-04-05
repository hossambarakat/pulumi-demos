using System.Threading.Tasks;
using Pulumi;
using TeamTimeZonesInfrastructure.Complete;

class Program
{
    static Task<int> Main()
    {
        return Deployment.RunAsync<CompleteStack>();
    }
}

using System.Threading.Tasks;
using Pulumi;

namespace SampleInfrastructure
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return Deployment.RunAsync<MyStack>();
        }
    }
}
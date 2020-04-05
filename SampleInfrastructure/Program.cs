using System;
using System.Threading.Tasks;
using Pulumi;

namespace SampleInfrastructure
{
    class Program
    {
        static void Main(string[] args)
        {
            static Task<int> Main() => Deployment.RunAsync<MyStack>();
        }
    }
}
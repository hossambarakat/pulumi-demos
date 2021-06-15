using System.Collections.Immutable;
using System.Threading.Tasks;
using Moq;
using Pulumi;
using Pulumi.Testing;

namespace SampleInfrastructure.Tests
{
    public static class Testing
    {
        public static Task<ImmutableArray<Resource>> TestAsync<T>() where T : Stack, new()
        {
            var mocks = new Mock<IMocks>();
            mocks.Setup(m => m.NewResourceAsync(It.IsAny<MockResourceArgs>()))
                .ReturnsAsync((MockResourceArgs args) => (args.Id ?? "", args.Inputs));
            mocks.Setup(m => m.CallAsync(It.IsAny<MockCallArgs>()))
                .ReturnsAsync((MockCallArgs args) => args.Args);
            return Deployment.TestAsync<T>(mocks.Object, new TestOptions { IsPreview = false });
        }
        
        public static async Task<TProperty> TestProperty<TProperty>(string targetResourceName, string propertyName)
        {
            var mocks = new Mock<IMocks>();

            // Capture the value of the target property of the target type
            TProperty result = default;
            mocks.Setup(m => m.NewResourceAsync(It.IsAny<MockResourceArgs>()))
                .ReturnsAsync((MockResourceArgs args) =>
                {
                    if (args.Name == targetResourceName && args.Inputs.TryGetValue(propertyName, out var value))
                    {
                        result = (TProperty)value;
                    }

                    var outputs = args.Inputs.Add("name", "test");
                    return (args.Name + "-test", outputs);
                });

            var statusCode = await Deployment.TestAsync<MyStack>(mocks.Object);
            return result;
        }
    }
}
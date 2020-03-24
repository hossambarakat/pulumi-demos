using System.Collections.Immutable;
using System.Threading.Tasks;
using Moq;
using Pulumi;
using Pulumi.Testing;
using Xunit;
using System.Linq;
using Shouldly;

namespace SampleInfrastructure.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task ResourceGroupLocation_ShouldBeEastUs_HardWay()
        {
            var mock = new Mock<IMocks>();

            mock.Setup(m => m.NewResourceAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<ImmutableDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string resource, string name, ImmutableDictionary<string, object> inputs, string provider,
                    string id) => (name + "-test", inputs));
            var resources = await Deployment.TestAsync<MyStack>(mock.Object);
            var resourceGroup = resources.OfType<Pulumi.Azure.Core.ResourceGroup>().Single();
            var location = resourceGroup.Location.GetValue();
            Assert.Equal("EastUs", location);
        }
        
        [Fact]
        public async Task ResourceGroup_WithName_ShouldBeDeployedToEastUs()
        {
            var property = await TestProperty<string>("resourceGroup", "location");
            Assert.Equal("EastUs", property);
        }
        
        [Fact]
        public async Task ResourceGroupLocation_ShouldBeEastUs()
        {
            var resources = await TestAsync<MyStack>();
            var resourceGroup = resources.OfType<Pulumi.Azure.Core.ResourceGroup>().Single();
            var location = resourceGroup.Location.GetValue();
            Assert.Equal("EastUs", location);
        }
        [Fact]
        public async Task AllResourceGroups_Should_Have_ProductName_Tag()
        {
            var resources = await TestAsync<MyStack>();
            var resourceGroups = resources.OfType<Pulumi.Azure.Core.ResourceGroup>();
            resourceGroups.ShouldAllBe(rg =>rg.Tags.GetValue().ContainsKey("productname"));
        }

        public static async Task<ImmutableArray<Resource>> TestAsync<TStack>(
            TestOptions options = null)
            where TStack : Stack, new()
        {
            var mock = new Mock<IMocks>();

            mock.Setup(m => m.NewResourceAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<ImmutableDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string resource, string name, ImmutableDictionary<string, object> inputs, string provider,
                    string id) => (name + "-test", inputs));
            var resources = await Deployment.TestAsync<MyStack>(mock.Object);
            return resources;
        }
        
        private async Task<TProperty> TestProperty<TProperty>(string targetResourceName, string propertyName)
        {
            var mock = new Mock<IMocks>();

            // Capture the value of the target property of the target type
            TProperty result = default;
            mock.Setup(m => m.NewResourceAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<ImmutableDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string resource, string name, ImmutableDictionary<string, object> inputs, string provider,
                    string id) =>
                {
                    if (name == targetResourceName && inputs.TryGetValue(propertyName, out var value))
                    {
                        result = (TProperty)value;
                    }

                    var outputs = inputs.Add("name", "test");
                    return (name + "-test", outputs);
                });

            var statusCode = await Deployment.TestAsync<MyStack>(mock.Object);
            return result;
        }
    }
}


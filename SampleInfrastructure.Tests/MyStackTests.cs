using System.Threading.Tasks;
using Xunit;
using System.Linq;
using Shouldly;
using Pulumi.AzureNative.Resources;

namespace SampleInfrastructure.Tests
{
    public class MyStackTests
    {
        [Fact]
        public async Task ResourceGroupLocation_ShouldBeEastUs_HardWay()
        {
            var resources = await Testing.TestAsync<MyStack>();
            var resourceGroup = resources.OfType<ResourceGroup>().Single();
            var location = resourceGroup.Location.GetValue();
            Assert.Equal("EastUs", location);
        }
        
        [Fact]
        public async Task ResourceGroup_WithName_ShouldBeDeployedToEastUs()
        {
            var property = await Testing.TestProperty<string>("resourceGroup", "location");
            Assert.Equal("EastUs", property);
        }
        
        [Fact]
        public async Task ResourceGroupLocation_ShouldBeEastUs()
        {
            var resources = await Testing.TestAsync<MyStack>();
            var resourceGroup = resources.OfType<ResourceGroup>().Single();
            var location = resourceGroup.Location.GetValue();
            Assert.Equal("EastUs", location);
        }
        [Fact]
        public async Task AllResourceGroups_Should_Have_ProductName_Tag()
        {
            var resources = await Testing.TestAsync<MyStack>();
            var resourceGroups = resources.OfType<ResourceGroup>();
            resourceGroups.ShouldAllBe(rg =>rg.Tags.GetValue().ContainsKey("productname"));
        }
        
    }
}


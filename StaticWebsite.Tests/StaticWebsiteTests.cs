using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.Testing;
using Shouldly;
using Xunit;

namespace StaticWebsite.Tests
{
    public class StaticWebsiteTests
    {
        private static Task<ImmutableArray<Pulumi.Resource>> TestAsync()
        {
            return Pulumi.Deployment.TestAsync<WebsiteStack>(new Mocks(), new TestOptions {IsPreview = false});
        }
        
        [Fact]
        public async Task ResourceGroup_ShouldExist()
        {
            var resources = await TestAsync();

            var resourceGroups = resources.OfType<ResourceGroup>().ToList();

            resourceGroups.Count.ShouldBe(1, "website resource group expected");
        }
        
        [Fact]
        public async Task ResourceGroup_ShouldHasEnvironmentTag()
        {
            var resources = await TestAsync();
            var resourceGroup = resources.OfType<ResourceGroup>().First();

            var tags = resourceGroup.Tags.GetValue();
            tags.ShouldNotBeNull("Tags must be defined");
            tags.ShouldContainKey("Environment");
        }
        
        [Fact]
        public async Task ShouldUploadsDefaultFiles()
        {
            var resources = await TestAsync();
            
            var files = resources.OfType<Blob>().ToList();
            files.Count.ShouldBe(2, "Should have uploaded files from `wwwroot`");
        }
        
        [Fact]
        public async Task ShouldExportsWebsiteUrl()
        {
            var resources = await TestAsync();
            var stack = resources.OfType<WebsiteStack>().First();

            var endpoint = stack.PrimaryWebEndpoint.GetValue();
            endpoint.ShouldBe("https://mysite.web.core.windows.net");
        }
    }
}
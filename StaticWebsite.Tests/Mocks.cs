using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Pulumi.Testing;

namespace StaticWebsite.Tests
{
    class Mocks : IMocks
    {
        public Task<(string id, object state)> NewResourceAsync(
            string type, string name, ImmutableDictionary<string, object> inputs, string? provider, string? id)
        {
            var outputs = ImmutableDictionary.CreateBuilder<string, object>();

            // Forward all input parameters as resource outputs, so that we could test them.
            outputs.AddRange(inputs);

            // <-- We'll customize the mocks here
            // Set the name to resource name if it's not set explicitly in inputs.
            if (!inputs.ContainsKey("name"))
                outputs.Add("name", name);
            
            if (type == "azure-native:storage:StorageAccount")
            {
                // ... set its web endpoint property.
                // Normally this would be calculated by Azure, so we have to mock it.
                outputs.Add("primaryEndpoints", new Dictionary<string, object?>
                {
                    {"blob", "BlobEndpoint"},
                    {"dfs", "DfsEndpoint"},
                    {"file", "FileEndpoint"},
                    {"internetEndpoints", null},
                    {"microsoftEndpoints", null},
                    {"queue", "QueueEndpoint"},
                    {"table", "TableEndpoint"},
                    {"web", $"https://{name}.web.core.windows.net"},
                });
            }
            
            // Default the resource ID to `{name}_id`.
            id ??= $"{name}_id";
            return Task.FromResult((id, (object)outputs));
        }

        public Task<object> CallAsync(string token, ImmutableDictionary<string, object> inputs, string? provider)
        {
            // We don't use this method in this particular test suite.
            // Default to returning whatever we got as input.
            return Task.FromResult((object)inputs);
        }
    }
}
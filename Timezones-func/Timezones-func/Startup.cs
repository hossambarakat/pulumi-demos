using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using TeamTimeZones;

[assembly: FunctionsStartup(typeof(Startup))]

namespace TeamTimeZones
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string accountEndpoint = Environment.GetEnvironmentVariable("db-account-endpoint");
            string accountKey = Environment.GetEnvironmentVariable("db-account-key");

            builder.Services.AddDbContext<TeamContext>(
                options => options.UseCosmos(accountEndpoint, accountKey, databaseName: "TeamTimeZones"));
        }
    }
}

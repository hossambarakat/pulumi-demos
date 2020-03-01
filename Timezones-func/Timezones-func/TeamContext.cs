using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Timezones_func
{
    public class TeamContext: DbContext
    {
        public DbSet<TeamMember> TeamMembers {get;set;}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(accountEndpoint: "https://timezonesdemo.documents.azure.com:443/", accountKey:"odKFs0qGxm7SYLSjRFpUtvFJkvYaBcL4uUoMW23u0NwwC1SrchjgX2XUMSuv2jmFGap3qN4jyfdqBRGTqLp2EQ==",
             databaseName:"TeamTimezones");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamMember>(x=>x.ToContainer("Team"));
            modelBuilder.Entity<TeamMember>()
                .HasNoDiscriminator()
                .HasPartitionKey(x=>x.TimeZone);
        }
    }
    public class TeamMember
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string TimeZone { get; set; }
    }
}
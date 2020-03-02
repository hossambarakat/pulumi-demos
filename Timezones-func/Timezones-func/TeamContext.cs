using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace TeamTimeZones
{
    public class TeamContext: DbContext
    {
        public TeamContext(DbContextOptions<TeamContext> options)
            : base(options)
        {

        }
        public DbSet<TeamMember> TeamMembers {get;set;}
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamMember>(x=>x.ToContainer("TeamMember"));
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
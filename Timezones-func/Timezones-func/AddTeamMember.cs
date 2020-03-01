using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace Timezones_func
{
    public static class Functions
    {
        [FunctionName("AddTeamMember")]
        public static async Task<IActionResult> RunAddTeamMemberAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "member")]
            HttpRequest req, 
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var member = JsonConvert.DeserializeObject<TeamMember>(requestBody);

            using (var context = new TeamContext())
            {
                context.Add(member);

                await context.SaveChangesAsync();
            }

            return new  OkObjectResult($"Created, {member.Name}");
        }
        
        [FunctionName("ListTeamMembers")]
        public static IActionResult RunListTeamMembersAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "member")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            List<TeamMember> members;
            using(var context = new TeamContext())
            {
                members = context.TeamMembers.ToList();
            }

            return (ActionResult) new JsonResult(members);
        }
        
        [FunctionName("DeleteTeamMember")]
        public static async Task<IActionResult> RunDeleteTeamMemberAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "member/{id}")]
            HttpRequest req,
            string id,
             ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            using(var context = new TeamContext())
            {
                var member = await context.TeamMembers.FindAsync(id);
                context.TeamMembers.Remove(member);
                await context.SaveChangesAsync();
            }

            return (ActionResult) new OkObjectResult($"Hello, {id}");
        }
        [FunctionName("UpdateTeamMember")]
        public static async Task<IActionResult> RunUpdateTeamMemberAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "member/{id}")]
            HttpRequest req, 
            string id,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var member = JsonConvert.DeserializeObject<TeamMember>(requestBody);

            using (var context = new TeamContext())
            {
                var existingMember = await context.TeamMembers.FindAsync(id);
                existingMember.Name = member.Name;
                existingMember.TimeZone = member.TimeZone;
                existingMember.Country = member.Country;

                context.TeamMembers.Update(existingMember);

                await context.SaveChangesAsync();
            }

            return (ActionResult) new OkObjectResult($"Hello, {id}");
            
        }
    }
}
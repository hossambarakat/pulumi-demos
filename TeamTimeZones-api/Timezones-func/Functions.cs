using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TeamTimeZones
{
    public class Functions
    {
        private readonly TeamContext context;

        public Functions(TeamContext teamContext)
        {
            context = teamContext;
        }
        [FunctionName("AddTeamMember")]
        public async Task<IActionResult> RunAddTeamMemberAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "members")]
            HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var member = JsonConvert.DeserializeObject<TeamMember>(requestBody);
            member.Id = Guid.NewGuid().ToString();
            context.TeamMembers.Add(member);

            await context.SaveChangesAsync();

            return new CreatedResult("member/{Name}", "");
        }

        [FunctionName("ListTeamMembers")]
        public IActionResult RunListTeamMembersAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "members")]
            HttpRequest req,
            ILogger log)
        {
            List<TeamMember> members;
            members = context.TeamMembers.ToList();

            return new JsonResult(members);
        }

        [FunctionName("DeleteTeamMember")]
        public async Task<IActionResult> RunDeleteTeamMemberAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "members/{id}")]
            HttpRequest req,
            string id,
            ILogger log)
        {
            var member = await context.TeamMembers.FindAsync(id);
            context.TeamMembers.Remove(member);
            await context.SaveChangesAsync();

            return new OkResult();
        }
        //[FunctionName("UpdateTeamMember")]
        //public async Task<IActionResult> RunUpdateTeamMemberAsync(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "members/{id}")]
        //    HttpRequest req,
        //    string id,
        //    ILogger log)
        //{
        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    var member = JsonConvert.DeserializeObject<TeamMember>(requestBody);

        //    var existingMember = await context.TeamMembers.FindAsync(id);
        //    existingMember.Name = member.Name;
        //    existingMember.TimeZone = member.TimeZone;
        //    existingMember.Country = member.Country;

        //    context.TeamMembers.Update(existingMember);

        //    await context.SaveChangesAsync();

        //    return new OkResult();
        //}
    }
}
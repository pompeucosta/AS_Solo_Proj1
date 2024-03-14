using AS_Solo_Proj1.Server.Data;
using AS_Solo_Proj1.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;

namespace AS_Solo_Proj1.Server.Controllers
{
    [Route("")]
    [ApiController]
    public class AccountDetails : ControllerBase
    {
        private ApplicationDbContext dbContext;
        private ILogger<AccountDetails> logger;

        public AccountDetails(ApplicationDbContext dbContext, ILogger<AccountDetails> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        [Authorize]
        [HttpPost("details")]
        public async Task<IResult> Details(ClientDetailsRequestModel details)
        {
            using var myActivity = OpenTelemetryData.MyActivitySource.StartActivity("Get Client details");
            var u = User.FindFirstValue(ClaimTypes.Email);
            var id = dbContext.Users.Where(_u => _u.UserName == u).First().Id;
            var requester = dbContext.MyUsers.Where(_u => _u.BaseUser.Id == id).First();

            var cId = details.ClientID;

            if (cId < 0)
            {
#if DEBUG
                var client = dbContext.Clients.Include(c => c.User).FirstOrDefault(c => c.User.UserID == requester.UserID);
                if (client == null)
                    return Results.BadRequest(new { Message = "User not found" });

                cId = client.ClientID;
#else
                return Results.BadRequest(new { Message = "Invalid Client" });
#endif
            }
            else
            {
                if (!details.AccessCode.IsNullOrEmpty())
                    logger.LogInformation($"User with id {requester.UserID} tried to validate access code of client with id {cId}");

                if (!IsRequesterValid(dbContext, requester, cId))
                    return Results.BadRequest(new { Message = "User not found" });
            }

            try
            {
                var c = dbContext.GetClientDetails(requester.UserID, cId, details.AccessCode);

                if (c == null)
                {
                    if (!details.AccessCode.IsNullOrEmpty())
                        logger.LogInformation($"User with id {requester.UserID} tried to access masked fields of client {cId} but didn't have authorization");

                    return Results.BadRequest(new { Message = "User not found" });
                }

                return Results.Ok(new { Details = c });
            }
            catch (Exception ex)
            {
#if DEBUG
                return Results.BadRequest(new {Message = ex.Message});
#else
                return Results.BadRequest(new { Message = "User not found" });
#endif
            }
        }

        [Authorize]
        [HttpPost("edit")]
        public async Task<IResult> Edit(EditUserDetailsRequestModel data)
        {
            using var myActivity = OpenTelemetryData.MyActivitySource.StartActivity("Edit Client details");
            var u = User.FindFirstValue(ClaimTypes.Email);
            var id = dbContext.Users.Where(_u => _u.UserName == u).First().Id;
            var requester = dbContext.MyUsers.Where(_u => _u.BaseUser.Id == id).First();

            if (data.ClientID < 0)
                return Results.BadRequest(new { Message = "Invalid Client" });

            if (!data.AccessCode.IsNullOrEmpty())
                logger.LogInformation($"User with id {requester.UserID} tried to validate access code of client with id {data.ClientID}");

            if (!IsRequesterValid(dbContext, requester, data.ClientID))
                return Results.Forbid();


            var rows = dbContext.UpdateClientDetails(new ClientDetails
            {
                FullName = data.FullName,
                DiagnosisDetails = data.DiagnosisDetails,
                MedicalRecordNumber = data.MedicalRecordNumber,
                PhoneNumber = data.PhoneNumber,
                TreatmentPlan = data.TreatmentPlan
            }, requester.UserID,
                data.ClientID, data.AccessCode);

            if (rows == 0)
                return Results.BadRequest(new { Message = "Error" });
#if DEBUG
            if (rows > 1)
                return Results.BadRequest(new { Message = "more than 1?" });
#endif
            return Results.Ok(new { Message = "Updated successfully" });

        }

        private bool IsRequesterValid(ApplicationDbContext dbContext, User requester, int clientID)
        {
            var client = dbContext.Clients.Include(c => c.User).FirstOrDefault(c => c.ClientID == clientID);
            if (client == null)
                return false;

            if (requester.Role == Roles.Client)
            {
                var clientUserID = client.User?.UserID ?? -1;
                if (clientUserID != requester.UserID)
                    return false;
            }

            return true;
        }

    }
}

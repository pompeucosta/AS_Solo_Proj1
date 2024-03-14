using AS_Solo_Proj1.Server.Data;
using AS_Solo_Proj1.Server.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AS_Solo_Proj1.Server.Controllers
{
    [Route("")]
    [ApiController]
    public class AccountEntry : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IResult> Login(SignInManager<ApplicationUser> signInManager, ILogger<AccountEntry> logger, LoginModel loginModel)
        {
            var result = await signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, loginModel.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                logger.LogInformation($"User with email {loginModel.Email} logged in");
                OpenTelemetryData.SuccessfulLoginsCounter.Add(1);
                return Results.Ok(new { Message = "Login successful" });
            }
            else
            {
                logger.LogInformation($"User with email {loginModel.Email} failed to logged in");
                OpenTelemetryData.FailedLoginsCounter.Add(1);
                return Results.BadRequest(new { Message = "Login failed" });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IResult> Logout(SignInManager<ApplicationUser> signInManager)
        {
            await signInManager.SignOutAsync();
            return Results.Ok();
        }

        [HttpPost("register")]
        public async Task<IResult> Register(UserManager<ApplicationUser> userManager, UserRegisterModel user, ApplicationDbContext _dbContext)
        {
            return await RegisterUserAsync(userManager, user, _dbContext);
        }

#if DEBUG
        [HttpPost("registerHelpDesk")]
        public async Task<IResult> RegisterHelpDesk(UserManager<ApplicationUser> userManager, UserRegisterModel user, ApplicationDbContext _dbContext)
        {
            return await RegisterUserAsync(userManager, user, _dbContext, Roles.Helpdesk);
        }
#endif

        private async Task<IResult> RegisterUserAsync(UserManager<ApplicationUser> userManager, UserRegisterModel user, ApplicationDbContext _dbContext, Roles userRole = Roles.Client)
        {
            var identityUser = new ApplicationUser { Email = user.Email, UserName = user.Email };
            var identityResult = await userManager.CreateAsync(identityUser, user.Password);

            if (!identityResult.Succeeded)
            {
                return Results.BadRequest(new { Errors = identityResult.Errors }); ;
            }

            var newUser = new User { BaseUser = identityUser, Role = userRole };

            if (userRole == Roles.Client)
            {
                var newClient = new Client { User = newUser, DiagnosisDetails = "", TreatmentPlan = "", PhoneNumber = user.PhoneNumber, MedicalRecordNumber = -1, FullName = user.Name, AccessCode = user.AccessCode };
                try
                {
                    _dbContext.Clients.Add(newClient);
                    _dbContext.SaveChanges();

                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { Message = ex.Message });
                }
            }
            else
            {
                try
                {
                    _dbContext.MyUsers.Add(newUser);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { Message = ex.Message });
                }
            }

            return Results.Ok(new { Message = "Success", });
        }
    }
}

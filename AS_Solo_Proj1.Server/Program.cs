
using AS_Solo_Proj1.Server.Data;
using AS_Solo_Proj1.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AS_Solo_Proj1.Server
{
    public class Program
    {
        private static Meter _meter;
        private static string meterName = "LoginMetrics";
        private static Counter<long> _successfulLoginsCounter;
        private static Counter<long> _failedLoginsCounter;
        private static string serviceName = "AS.Proj1";
        private static string serviceVersion = "1.0";
        private static readonly ActivitySource MyActivitySource = new(serviceName,serviceVersion);

        private static bool IsRequesterValid(ApplicationDbContext dbContext,User requester,int clientID)
        {
            var client = dbContext.Clients.Include(c => c.User).FirstOrDefault(c => c.ClientID == clientID);
            if(client == null)
                return false;
            
            if (requester.Role == Roles.Client)
            {
                if (client == null)
                    return false;

                var clientUserID = client.User?.UserID ?? -1;
                if (clientUserID != requester.UserID)
                    return false;
            }

            return true;
        }

        private static async Task<IResult> RegisterUserAsync(UserManager<ApplicationUser> userManager, UserRegisterModel user, ApplicationDbContext _dbContext,Roles userRole = Roles.Client)
        {
            var identityUser = new ApplicationUser { Email = user.Email, UserName = user.Email };
            var identityResult = await userManager.CreateAsync(identityUser, user.Password);

            if (!identityResult.Succeeded)
            {
                return Results.BadRequest(new { Errors = identityResult.Errors }); ;
            }

            var newUser = new User { BaseUser = identityUser, Role = userRole };

            if(userRole == Roles.Client)
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
                catch(Exception ex)
                {
                    return Results.BadRequest(new { Message = ex.Message });
                }
            }

            return Results.Ok(new { Message = "Success", });
        }

        public static void Main(string[] args)
        {
            _meter = new Meter(meterName);
            _successfulLoginsCounter = _meter.CreateCounter<long>("successful_logins");
            _failedLoginsCounter = _meter.CreateCounter<long>("failed_logins");


            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddAuthorization();
            builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
            {
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Logging.AddOpenTelemetry(x =>
            {
                x.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:3100");
                });
            });


            builder.Services.AddOpenTelemetry()
                .WithMetrics(x =>
                {
                    x.AddRuntimeInstrumentation()
                    .AddPrometheusExporter()
                    .AddMeter(meterName);
                })
                .WithTracing(x =>
                {
                    x.AddSource(serviceName)
                    .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion: serviceVersion))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            app.MapIdentityApi<ApplicationUser>();
            app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Ok();
            }).RequireAuthorization();

            app.MapGet("/pingauth", (ClaimsPrincipal user) =>
            {
                var email = user.FindFirstValue(ClaimTypes.Email);
                return Results.Json(new { Email = email });
            }).RequireAuthorization();
            
            app.MapPost("/my_register", async (UserManager < ApplicationUser > userManager,UserRegisterModel user, ApplicationDbContext _dbContext) =>
            {
                return await RegisterUserAsync(userManager, user, _dbContext);
            });

            app.MapPost("/my_login", async (SignInManager<ApplicationUser> signInManager,ILogger<Program> logger, LoginModel loginModel) =>
            {
                var result = await signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, loginModel.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    logger.LogInformation($"User with email {loginModel.Email} logged in");
                    _successfulLoginsCounter.Add(1);
                    return Results.Ok(new { Message = "Login successful" });
                }
                else
                {
                    logger.LogInformation($"User with email {loginModel.Email} failed to logged in");
                    _failedLoginsCounter.Add(1);
                    return Results.BadRequest(new { Message = "Login failed" });
                }
            });

            app.MapPost("/details", (ClaimsPrincipal user, ILogger<Program> logger, ApplicationDbContext dbContext, ClientDetailsRequestModel details) =>
            {
                using var myActivity = MyActivitySource.StartActivity("Get Client details");
                var u = user.FindFirstValue(ClaimTypes.Email);
                var id = dbContext.Users.Where(_u => _u.UserName == u).First().Id;
                var requester = dbContext.MyUsers.Where(_u => _u.BaseUser.Id == id).First();

                var cId = details.ClientID;


                if (cId == -1)
                {
                    var client = dbContext.Clients.Include(c => c.User).FirstOrDefault(c => c.User.UserID == requester.UserID);
                    if (client == null)
                        return Results.BadRequest(new { Message = "User not found" });

                    cId = client.ClientID;
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
                    return Results.BadRequest(new { Message = "User not found" });
                }
            }).RequireAuthorization();

            app.MapPost("/edit", (ClaimsPrincipal user,ILogger<Program> logger, ApplicationDbContext dbContext,EditUserDetailsRequestModel data) =>
            {
                using var myActivity = MyActivitySource.StartActivity("Edit Client details");
                var u = user.FindFirstValue(ClaimTypes.Email);
                var id = dbContext.Users.Where(_u => _u.UserName == u).First().Id;
                var requester = dbContext.MyUsers.Where(_u => _u.BaseUser.Id == id).First();

                if (!data.AccessCode.IsNullOrEmpty())
                    logger.LogInformation($"User with id {requester.UserID} tried to validate access code of client with id {data.ClientID}");
                
                if (!IsRequesterValid(dbContext, requester, data.ClientID))
                    return Results.Forbid();


                var rows = dbContext.UpdateClientDetails(new ClientDetails {FullName = data.FullName,
                    DiagnosisDetails = data.DiagnosisDetails,MedicalRecordNumber = data.MedicalRecordNumber,
                    PhoneNumber = data.PhoneNumber,TreatmentPlan = data.TreatmentPlan}, requester.UserID,
                    data.ClientID, data.AccessCode);

                if (rows == 0)
                    return Results.BadRequest(new { Message = "Error" });

                if(rows > 1)
                    return Results.BadRequest(new {Message = "more than 1?"});

                return Results.Ok(new {Message = "Updated successfully"});

            }).RequireAuthorization();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();

                app.MapPost("/register_helpdesk", async (UserManager<ApplicationUser> userManager, UserRegisterModel user, ApplicationDbContext _dbContext) =>
                {
                    return await RegisterUserAsync(userManager, user, _dbContext, Roles.Helpdesk);
                });
            }

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}

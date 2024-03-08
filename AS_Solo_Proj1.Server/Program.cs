
using AS_Solo_Proj1.Server.Data;
using AS_Solo_Proj1.Server.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

namespace AS_Solo_Proj1.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
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

            app.MapPost("/details", (ClaimsPrincipal user,ApplicationDbContext dbContext, ClientDetailsRequestModel details) =>
            {
                var u = user.FindFirstValue(ClaimTypes.Email);
                var id = dbContext.Users.Where(_u => _u.UserName == u).First().Id;
                var requester = dbContext.MyUsers.Where(_u => _u.BaseUser.Id == id).First();

                if(requester.Role == Roles.Client && requester.UserID != details.ClientID)
                {
                   return Results.Forbid();
                }

                var requesterID = requester.UserID;
                try
                {
                    var c = dbContext.GetClientDetails(requesterID, details.ClientID, details.AccessCode);
                    if (c == null)
                    {
                        return Results.BadRequest(new { Message = "User not found" });
                    }

                    return Results.Ok(new { Details = c });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { Message = "Internal error" });
                }
            }).RequireAuthorization();

            app.MapPost("/my_register", async (UserManager < ApplicationUser > userManager,UserRegisterModel user, ApplicationDbContext _dbContext) =>
            {
                var identityUser = new ApplicationUser { Email = user.Email, UserName = user.Email };
                var identityResult = await userManager.CreateAsync(identityUser, user.Password);

                if (!identityResult.Succeeded)
                {
                    return Results.BadRequest(new { Errors = identityResult.Errors });
                }

                var newUser = new User { BaseUser = identityUser, Role = Roles.Client };
                _dbContext.MyUsers.Add(newUser);
                _dbContext.SaveChanges();

                var newClient = new Client { User = new Models.User { UserID = newUser.UserID }, DiagnosisDetails = "", TreatmentPlan = "", PhoneNumber = user.PhoneNumber, MedicalRecordNumber = -1, FullName = user.Name };
                try
                {
                    _dbContext.InsertClient(newClient, user.AccessCode);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { Message = ex.Message });
                }

                return Results.Ok(new { Message = "Success", });
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
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

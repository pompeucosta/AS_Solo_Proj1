using AS_Solo_Proj1.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AS_Solo_Proj1.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<User> MyUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>().ToTable(tb => tb.HasTrigger("HashAccessCode"));
            modelBuilder.Entity<User>().ToTable("Users");
        }

        public ClientDetails? GetClientDetails(int requesterID,int clientID,string? code)
        {
            try
            {
                var r = Database.SqlQuery<ClientDetails>($"EXEC GetClientDetails {requesterID},{clientID},{code}");

                var l = r.ToList();
                if (l.Count == 0)
                {
                    return null;
                }

                return l.First();
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public int UpdateClientDetails(ClientDetails details,int requesterID,int clientID,string? code)
        {
            try
            {
                var rows = Database.ExecuteSql($"EXEC UpdateUserDetails {requesterID},{clientID},{code},{details.FullName},{details.PhoneNumber},{details.MedicalRecordNumber},{details.DiagnosisDetails},{details.TreatmentPlan}");
                return rows;
            }
            catch(Exception e)
            {
                return -1;
            }
        }
    }
}

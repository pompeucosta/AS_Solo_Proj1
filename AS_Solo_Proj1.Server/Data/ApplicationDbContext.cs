using AS_Solo_Proj1.Server.Models;
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

            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<User>().ToTable("Users");
        }

        public void InsertClient(Client client,string accessCode)
        {
            var res = Database.ExecuteSql($"EXEC InsertClient {client.User.UserID},{client.FullName},{client.PhoneNumber},{client.MedicalRecordNumber},{client.DiagnosisDetails},{client.TreatmentPlan},{accessCode}");
            if(res != 1)
            {
                throw new Exception("error while executing sql");
            }
        }

        public Client? GetClientDetails(int requesterID,int clientID,string? code)
        {
            try
            {
                var r = Clients.FromSql($"EXEC GetClientDetails {requesterID},{clientID},{code}");

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
    }
}

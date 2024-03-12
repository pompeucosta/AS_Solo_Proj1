using AS_Solo_Proj1.Server.Data;
using AS_Solo_Proj1.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AS_Solo_Proj1_Tests
{
    public class TestDatabase
    {
        private static readonly string connectionString = "Server=localhost;Database=AS1;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";
        private static readonly ApplicationDbContext dbContext = new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options);
        private static int clientID;
        private static int userID;

        //id of a requester. must be different than clientID
        private static int badResquesterID = -1;

        private static bool userCreated = false;
        private static Client clientTest = new Client { User = new User { Role = Roles.Client},AccessCode = "ana", DiagnosisDetails = "teste teste", TreatmentPlan = "plano plano", FullName = "ana", MedicalRecordNumber = 0, PhoneNumber = "987654322" };

        public TestDatabase()
        {
            if(!userCreated)
            {
                dbContext.Database.EnsureCreated();
                //certificar que existe um utilizador com os mesmos dados do cliente a testar
                //verificar se o accessCode do cliente na db corresponde ao accessCode do clientTest
                var client = dbContext.Clients.Include(c => c.User).FirstOrDefault(
                    c => c.FullName == clientTest.FullName 
                && c.DiagnosisDetails == clientTest.DiagnosisDetails 
                && c.PhoneNumber == clientTest.PhoneNumber 
                && c.TreatmentPlan == clientTest.TreatmentPlan
                );
                if(client != null)
                {
                    clientID = client.ClientID;
                    userID = client.User?.UserID ?? throw new ArgumentNullException("User is null (?????)");
                }
                else
                {
                    var c = clientTest;
                    dbContext.Clients.Add(c);
                    dbContext.SaveChanges();
                    userID = c.User.UserID;
                    clientID = c.ClientID;
                }
                userCreated = true;
            }
        }

        [Fact]
        //Cliente pede os seus proprios dados
        public void GetClientDetails_ClientSameRequester_NoCode_Unmasked()
        {
            var details = dbContext.GetClientDetails(userID, clientID,null);
            Assert.NotNull(details);
            Assert.Equal(clientTest.PhoneNumber, details.PhoneNumber);
            Assert.Equal(clientTest.DiagnosisDetails, details.DiagnosisDetails);
            Assert.Equal(clientTest.TreatmentPlan, details.TreatmentPlan);
        }

        [Fact]
        //helpdesk user pede dados de cliente mas nao introduz codigo
        public void GetClientDetails_ClientDifferentRequester_NoCode_Masked()
        {
            string phoneNumberLastThreeNumbers = clientTest.PhoneNumber.ToString().Substring(clientTest.PhoneNumber.ToString().Length - 3);
            var details = dbContext.GetClientDetails(badResquesterID, clientID, null);
            Assert.NotNull(details);
            Assert.Equal("xxxx" + phoneNumberLastThreeNumbers, details.PhoneNumber);
            Assert.Equal("xxxx", details.DiagnosisDetails);
            Assert.Equal("xxxx", details.TreatmentPlan);
        }

        [Fact]
        //helpdesk user pede dados de cliente mas introduz codigo errado
        public void GetClientDetails_ClientDifferenteRequester_WrongCode_Masked()
        {
            string code = "blabla";
            string phoneNumberLastThreeNumbers = clientTest.PhoneNumber.ToString().Substring(clientTest.PhoneNumber.ToString().Length - 3);
            var details = dbContext.GetClientDetails(badResquesterID, clientID, code);
            Assert.NotNull(details);
            Assert.Equal("xxxx" + phoneNumberLastThreeNumbers, details.PhoneNumber);
            Assert.Equal("xxxx", details.DiagnosisDetails);
            Assert.Equal("xxxx", details.TreatmentPlan);
        }

        [Fact]
        //helpdesk user pede dados de cliente e introduz codigo certo
        public void GetClientDetails_ClientDifferenteRequester_RightCode_Unmasked()
        {
            string code = clientTest.AccessCode;
            var details = dbContext.GetClientDetails(badResquesterID, clientID, code);
            Assert.NotNull(details);
            Assert.Equal(clientTest.PhoneNumber, details.PhoneNumber);
            Assert.Equal(clientTest.DiagnosisDetails, details.DiagnosisDetails);
            Assert.Equal(clientTest.TreatmentPlan, details.TreatmentPlan);
        }
    }
}
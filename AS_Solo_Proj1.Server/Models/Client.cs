using AS_Solo_Proj1.Server.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AS_Solo_Proj1.Server.Models
{
    public class Client
    {
        [Key]
        [ForeignKey("ClientID")]
        public int ClientID { get; set; }
        public User User { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string PhoneNumber { get; set; } //mask
        public int MedicalRecordNumber { get; set; }
        public string DiagnosisDetails { get; set; } //mask
        public string TreatmentPlan { get; set; } //mask

    }
}

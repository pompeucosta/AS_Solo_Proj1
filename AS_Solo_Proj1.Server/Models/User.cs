using AS_Solo_Proj1.Server.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AS_Solo_Proj1.Server.Models
{
    public enum Roles
    {
        Client,
        Helpdesk
    }

    public class User
    {
        [Key]
        [ForeignKey("UserID")]
        public int UserID { get; set; }

        public Roles Role { get; set; }

        public ApplicationUser BaseUser { get; set; }
    }
}

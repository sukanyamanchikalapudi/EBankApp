using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EBankApp.Models
{
    public class LoginRequest
    {
        [Required]
        [DisplayName("UserName / AccountNumber")]
        [MinLength(6, ErrorMessage = "Invalid userame")]
        public string UserName { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password should be minium six characters long")]
        public string Password { get; set; }
    }
}
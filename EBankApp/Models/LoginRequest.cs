using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EBankApp.Models
{
    public class LoginRequest
    {
        [Required]
        [DisplayName("UserName / AccountNumber")]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
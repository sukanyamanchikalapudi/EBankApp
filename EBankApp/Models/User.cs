using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace EBankApp.Models
{
    public class User : BaseEntity
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Firstname is required")]
        [DataType(DataType.Text)]   
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [Remote("UsernameExists", "User", HttpMethod = "POST", ErrorMessage = "User name not available.")]
        [MinLength(6, ErrorMessage = "Username should be atleast six characters long")]
        public string UserName { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password should be minimum six characters long")]
        public string Password { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "PIN should be minimum six characters long")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "PIN must be numeric")]
        public string PIN { get; set; }
        public ICollection<Account> Accounts { get; set; }
        public ICollection<Notification> Nottifications { get; set; }
        public ICollection<UserActivity> userActivities { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Firstname is required")]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Username should be atleast six characters long")]
        public string UserName { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password should be minimum six characters long")]
        public string Password { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "PIN should be minimum six characters long")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "PIN must be numeric")]
        public string PIN { get; set; }
        public int RoleId { get; set; }
    }
}
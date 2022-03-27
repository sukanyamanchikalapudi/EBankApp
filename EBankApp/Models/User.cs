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
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [Remote("UsernameExists", "User", HttpMethod = "POST", ErrorMessage = "User name not available.")]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PIN { get; set; }
        public ICollection<Account> Accounts { get; set; }
        public ICollection<Notification> Nottifications { get; set; }
        public ICollection<UserActivity> userActivities { get; set; }
        public int RoleId { get; set; }
    }
}
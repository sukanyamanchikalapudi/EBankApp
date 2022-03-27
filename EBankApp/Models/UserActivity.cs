namespace EBankApp.Models
{
    public class UserActivity : BaseEntity
    {
        public UserActivityEnum Name { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
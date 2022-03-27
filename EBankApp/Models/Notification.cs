namespace EBankApp.Models
{
    public class Notification : BaseEntity
    {
        public string Message { get; set; }
        public NotificationTypeEnum NotificationType { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
namespace EBankApp.Models
{
    public class DeleteAccountRequest
    {
        public int UserId { get; set; }
        public string AccountNumber { get; set; }
        public int Currency { get; set; }
    }
}
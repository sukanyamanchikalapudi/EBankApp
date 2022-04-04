namespace EBankApp.Models
{
    public class InterBankTransfer
    {
        public string UserId { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
    }
}
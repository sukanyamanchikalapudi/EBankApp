namespace EBankApp.Controllers.Models
{
    public class PrintTransactionRequest
    {
        public string Start { get; set; }
        public string End { get; set; }
        public string ReportType { get; set; }
        public string AccountNumber { get; set; }
        public int UserId { get; set; }
    }
}
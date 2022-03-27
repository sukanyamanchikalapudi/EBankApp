using System.ComponentModel.DataAnnotations;

namespace EBankApp.Models
{
    public class QuickTransferRequest
    {
        [Required]
        public string PayerAccountNumber { get; set; }
        [Required]
        public string PayeeAccountNumber { get; set; }
        [Required]
        public int Amount { get; set; }
    }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EBankApp.Models
{
    public class QuickTransferRequest
    {
        [Required]
        [DisplayName("Payer Account Number")]
        public string PayerAccountNumber { get; set; }
        [Required]
        [DisplayName("Payee Account Number")]
        public string PayeeAccountNumber { get; set; }
        [Required]
        [Range(1, 100000)]
        public int Amount { get; set; }
    }
}
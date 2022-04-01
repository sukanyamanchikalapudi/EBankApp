using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EBankApp.Models
{
    public class ExchangeCurrencyRequest
    {
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        [Required]
        public string Amount { get; set; }
    }
}
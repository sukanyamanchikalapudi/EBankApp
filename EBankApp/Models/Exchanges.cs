using System.ComponentModel.DataAnnotations;

namespace EBankApp.Models
{
    public class Exchanges
    {
        [Key]
        public int Id { get; set; }
        public string CurrencyCode { get; set; }
        public double ExchangeValue_USD { get; set; }
        public double ExchangeValue_GBP { get; set; }
    }
}
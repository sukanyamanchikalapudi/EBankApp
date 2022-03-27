using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EBankApp.Models
{
    public class Account : BaseEntity
    {
        public string AccountNumber { get; set; }
        public int AccountBalance { get; set; }
        public AccountTypeEnum AccountType { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
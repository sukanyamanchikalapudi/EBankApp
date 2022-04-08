using System;
using System.Collections.Generic;

namespace EBankApp.Models
{
    public class TransactionStatementResult
    {
        public List<Transaction> Transactions { get; set; }
        public string AccountNumber { get; set; }
        public DateTime Date { get; set; }
    }
}
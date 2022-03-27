namespace EBankApp.Models
{
    public class Transaction : BaseEntity
    {
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public int Credited { get; set; }
        public int Debited { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
    }
}
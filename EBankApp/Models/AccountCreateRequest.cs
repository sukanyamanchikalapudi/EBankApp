namespace EBankApp.Models
{
    public class AccountCreateRequest
    {
        public AccountTypeEnum AccountType { get; set; }
        public int UserId { get; set; }
    }
}
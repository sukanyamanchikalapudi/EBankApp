namespace EBankApp.Models
{
    public class PagingOptions
    {
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string SearchKey { get; set; }
        public string SortColumn { get; set; }
        public string SortBy { get; set; }
    }
}
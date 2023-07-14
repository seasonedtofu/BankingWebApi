namespace BankingWebApi.Models
{
    public class BaseFilter
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string SortOrder { get; init; } = "Desc";
        public string SortBy { get; init; } = "CreatedDate";
        public string SearchTerm { get; init; } = "";
    }
}

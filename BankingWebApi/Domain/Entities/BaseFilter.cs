namespace BankingWebApi.Domain.Entities
{
    public class BaseFilter
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        /// <summary>
        /// Sort order, accepts the following:
        /// 
        /// Asc
        /// 
        /// Desc
        /// </summary>
        public string SortOrder { get; init; } = "Desc";
        /// <summary>
        /// Sort by, accepts the following:
        /// 
        /// CreatedDate
        /// 
        /// UpdatedDate
        /// 
        /// Name
        /// 
        /// Balance
        /// </summary>
        public string SortBy { get; init; } = "CreatedDate";
        public string SearchTerm { get; init; } = "";
    }
}

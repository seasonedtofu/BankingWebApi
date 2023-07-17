namespace BankingWebApi.Models
{
    public class AccountsFilter: BaseFilter
    {
        /// <summary>
        /// If account is active or not
        /// </summary>
        public bool? Active { get; init; } = null;
    }
}

namespace BankingWebApi.Models
{
    public class AccountsFilter: BaseFilter
    {
        public bool? Active { get; init; } = null;
    }
}

using BankingWebApi.Models;

namespace BankingWebApi.Interfaces
{
    public interface IAccountRepository
    {
        Task<(IEnumerable<Account>, PaginationMetadata)> GetAccounts(AccountsFilter filters);
        Task<Account> GetAccount(Guid id);
        Task ChangeName(Guid id, string name);
        Task Deposit(Guid id, decimal amount);
        Task Withdraw(Guid id, decimal amount);
        Task Transfer(AccountTransfer accountTransfer);
        Task<Account> CreateAccount(AccountCreate accountCreate);
        Task ReactivateAccount(Guid id);
        Task DeleteAccount(Guid id);
    }
}

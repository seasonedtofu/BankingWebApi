using BankingWebApi.Domain.Entities;

namespace BankingWebApi.Domain.Interfaces
{
    public interface IAccountRepository
    {
        Task CreateAccount(Account account);
        Task<IEnumerable<Account>> GetAccounts(AccountsFilter filters);
        Task<Account> GetAccount(Guid id);
        Task UpdateName(Guid id, string name);
        Task AddToBalance(Guid id, decimal amount);
        Task SubtractFromBalance(Guid id, decimal amount);
        Task UpdateActive (Guid id, bool active);
        Task DeleteAccount(Guid id);
    }
}

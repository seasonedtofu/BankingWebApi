using BankingWebApi.Models;

namespace BankingWebApi.Interfaces

{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAccounts();
        Task<Account> GetAccount(Guid id);
        Task<Account> ChangeName(Guid id, string name);
        void Deposit(Guid id, decimal amount);
        void Withdraw(Guid id, decimal amount);
        void Transfer(AccountTransfer accountTransfer);
        Task<Account> CreateAccount(AccountCreate accountCreate);
        void ReactivateAccount(Guid id);
        void DeleteAccount(Guid id);
    }
}

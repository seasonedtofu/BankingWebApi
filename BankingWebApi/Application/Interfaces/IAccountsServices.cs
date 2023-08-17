using BankingWebApi.Application.Models;
using BankingWebApi.Domain.Entities;

namespace BankingWebApi.Application.Interfaces
{
    public interface IAccountsServices
    {
        Task<AccountDto> CreateAccount(CreateAccountDto accountCreate);
        Task<(List<AccountDto>, PaginationMetadata)> GetAccounts(AccountsFilter filters);
        Task<AccountDto> GetAccount(Guid id);
        Task ChangeName(Guid id, string name);
        Task Deposit(Guid id, decimal amount);
        Task Withdraw(Guid id, decimal amount);
        Task Transfer(AccountTransferDto accountTransfer);
        Task DeleteAccount(Guid id);
        Task ReactivateAccount(Guid id);
        Task HardDeleteAccount(Guid id);
    }
}

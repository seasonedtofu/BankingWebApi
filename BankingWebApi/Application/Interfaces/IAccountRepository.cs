using BankingWebApi.Domain.Entities;
using BankingWebApi.Application.Models;

namespace BankingWebApi.Application.Interfaces
{
    public interface IAccountRepository
    {
        Task<(List<AccountDto>, PaginationMetadata)> GetAccounts(AccountsFilter filters);
        Task<AccountDto> GetAccountDto(Guid id);
        Task ChangeName(Guid id, string name);
        Task Deposit(Guid id, decimal amount);
        Task Withdraw(Guid id, decimal amount);
        Task Transfer(AccountTransferDto accountTransfer);
        Task<Account> CreateAccount(CreateAccountDto accountCreate);
        Task ReactivateAccount(Guid id);
        Task DeleteAccount(Guid id);
    }
}

using BankingWebApi.DataTransformationObjects;
using BankingWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingWebApi.Interfaces
{
    public interface IAccountsController
    {
        Task<List<AccountDto>> GetAccounts([FromQuery] AccountsFilter filters);
        Task<ActionResult<AccountDto>> GetAccount(Guid id);
        Task<ActionResult<object>> GetCurrencyConversion(Guid id, string currency);
        Task<IActionResult> ChangeName(Guid id, string name);
        Task<IActionResult> Deposit(Guid id, decimal amount);
        Task<IActionResult> Withdraw(Guid id, decimal amount);
        Task<IActionResult> Transfer(AccountTransferDto accountTransfer);
        Task<ActionResult<CreateAccountDto>> PostAccount(CreateAccountDto createAccount);
        Task<IActionResult> ReactivateAccount(Guid id);
        Task<IActionResult> DeleteAccount(Guid id);
    }
}

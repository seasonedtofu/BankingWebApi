using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankingWebApi.Models;
using BankingWebApi.Utils;
using BankingWebApi.Clients;
using System.Net;
using System.Text.RegularExpressions;

namespace BankingWebApi.Controllers;

/// <summary>
/// Controller for web api endpoints.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly AccountsContext _context;
    private readonly CurrencyClient _currencyClient;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Accounts controller where context is a list of accounts and errors handls errors.
    /// </summary>
    /// <param name="context">Dependency injection for local db.</param>
    /// <param name="currencyClient">Dependency injection for currency client for API calls.</param>
    /// <param name="configuration">Dependency injection for appsettings.json to get API key.</param>
    public AccountsController(AccountsContext context, CurrencyClient currencyClient, IConfiguration configuration)
    {
        _context = context;
        _currencyClient = currencyClient;
        _configuration = configuration;
    }

    /// <summary>
    /// Gets all accounts.
    /// </summary>
    /// <returns>
    /// List of all accounts or empty list if no accounts found.
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
    {
        return await _context.Accounts.ToListAsync();
    }

    /// <summary>
    /// Gets account by id.
    /// </summary>
    /// <param name="id">GUID of account you want to view.</param>
    /// <returns>
    /// Account if found or 404 if not found.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetAccount(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }

        return account;
    }

    /// <summary>
    /// Gets currency converison.
    /// </summary>
    /// <param name="id">GUID of account you want to view.</param>
    /// <param name="currency">Enter currency abbreviations you want to convert to from USD separated by a comma.</param>
    /// <returns>
    /// Returns USD exchanged to user desired currency.
    /// </returns>
    [HttpGet("{id}/Currency/Balance")]
    public async Task<ActionResult<object>> GetCurrencyConversion(Guid id, string currency)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }

        var apiKey = _configuration.GetValue<string>("CURRENCY_API_KEY");
        var response = await _currencyClient.GetCurrencyRate(currency.ToUpper(), apiKey);

        foreach (var key in response.Keys)
        {
            response[key] *= Convert.ToDouble(account.Balance);
        }

        return response;
    }

    /// <summary>
    /// Changes name property of account.
    /// </summary>
    /// <param name="id">GUID of account you want to apply name change to.</param>
    /// <param name="name">Name(string) you want to change to for account.</param>
    /// <returns>
    /// 204 if account name successfully changes or 404 if account not found.
    /// </returns>
    [HttpPut("{id}/Name")]
    public async Task<IActionResult> ChangeName(Guid id, string name)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }

        account.Name = name;
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Deposits money into an account balance.
    /// </summary>
    /// <param name="id">GUID of account you want to apply deposit to.</param>
    /// <param name="amount">Decimal amount of money you want to add to acount.</param>
    /// <returns>
    /// 204 status code if successful, 404 if account not found.
    /// </returns>
    [HttpPost("{id}/Deposit")]
    public async Task<IActionResult> Deposit(Guid id, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }

        account.Balance += amount;
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Withdraws money from account balance.
    /// </summary>
    /// <param name="id">GUID of account you want to apply withdrawal to.</param>
    /// <param name="amount">Decimal amount of money you want to withdraw from account.</param>
    /// <returns>
    /// 204 status code if successful, 404 if account not found, or 400 if account balance is less than withdrawal amount.
    /// </returns>
    [HttpPost("{id}/Withdrawal")]
    public async Task<IActionResult> Withdraw(Guid id, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }
        else if (amount > account.Balance)
        {
            return BadRequest("Amount entered is more than account balance.");
        }

        account.Balance -= amount;
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Transfers a provided amount from one user's account to another users's account.
    /// </summary>
    /// <param name="accountTransfer">
    /// Account transfer model, contains properties of:
    /// TransferFromId: GUID (account you want to transfer from)
    /// TransferToId: GUID (account you want to transfer to)
    /// Amount: Decimal amount of money you want to transfer.
    /// </param>
    /// <returns>
    /// Returns 204 if successful, 400 if any account does not exist OR if account transfer amount is more than what exists from withdrawal account.
    /// </returns>
    [HttpPost("Transfers")]
    public async Task<IActionResult> Transfer(AccountTransfer accountTransfer)
    {
        var account = await _context.Accounts.FindAsync(accountTransfer.TransferFromId);
        var accountToTransferTo = await _context.Accounts.FindAsync(accountTransfer.TransferToId);

        if (account is null)
        {
            return NotFound("Could not find transfer from account with provided GUID.");
        }
        else if (accountToTransferTo is null)
        {
            return NotFound("Could not find transfer to account with provided GUID.");
        }
        else if (accountTransfer.Amount > account.Balance)
        {
            return BadRequest("Amount entered is more than account balance.");
        }

        await Withdraw(account.Id, accountTransfer.Amount);
        await Deposit(accountToTransferTo.Id, accountTransfer.Amount);
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Creates an account.
    /// </summary>
    /// <param name="accountCreate">
    /// Model for creating account, contains:
    /// Name: Name(string) of account holder.
    /// Balance: Decimal amount of initial deposit of money.
    /// </param>
    /// <returns>
    /// If successful, returns account object.
    /// </returns>
    [HttpPost]
    public async Task<ActionResult<AccountCreate>> PostAccount(AccountCreate accountCreate)
    {
        var dateTime = DateTime.UtcNow;
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = accountCreate.Name,
            Balance = accountCreate.Balance,
            Active = true,
            CreatedDate = dateTime,
            UpdatedDate = dateTime,
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }

    /// <summary>
    /// Reactivates a deactivated account.
    /// </summary>
    /// <param name="id">GUID of account you want to reactivate.</param>
    /// <returns>
    /// 204 if successful, 400 if account is not found.
    /// </returns>
    [HttpPut("{id}/Activation")]
    public async Task<IActionResult> ReactivateAccount(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }
        else if (account.Active)
        {
            return BadRequest("Account already active.");
        }

        account.Active = true;
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Soft deletes an account.
    /// </summary>
    /// <param name="id">GUID of account you want to deactivate</param>
    /// <returns>
    /// 204 if successful, 400 if account is already active or if account balance is still greater than 0.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }
        else if (account.Active == false)
        {
            return BadRequest("Account already inactive.");
        }
        else if (account.Balance > 0)
        {
            return BadRequest("Account currently has a balance greater than 0, please withdraw first.");
        }

        account.Active = false;
        account.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

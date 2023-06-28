using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using BankingWebApi.Models;
using BankingWebApi.Utils;
using BankingWebApi.Interfaces;

namespace BankingWebApi.Controllers;

/// <summary>
/// Controller for web api endpoints.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly AccountsContext _context;
    private readonly IErrorMessages _errors;

    /// <summary>
    /// Accounts controller where context is a list of accounts and errors handls errors.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="errors"></param>
    public AccountsController(AccountsContext context, IErrorMessages errors)
    {
        _context = context;
        _errors = errors;
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
    /// <param name="id"></param>
    /// <returns>
    /// Account if found or 404 if not found.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetAccount(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return _errors.NotFound();
        }

        return account;
    }

    /// <summary>
    /// Changes name property of account.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns>
    /// 204 if account name successfully changes or 404 if account not found.
    /// </returns>
    [HttpPut("{id}/Name")]
    public async Task<IActionResult> ChangeName(Guid id, string name)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return _errors.NotFound(HttpStatusCode.BadRequest);
        }

        account.Name = name;
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Deposits money into an account balance.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="amount"></param>
    /// <returns>
    /// 204 status code if successful, 404 if account not found.
    /// </returns>
    [HttpPost("{id}/Deposit")]
    public async Task<IActionResult> Deposit(Guid id, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return _errors.NotFound(HttpStatusCode.BadRequest);
        }

        account.Balance += amount;
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Withdraws money from account balance.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="amount"></param>
    /// <returns>
    /// 204 status code if successful, 404 if account not found, or 400 if account balance is less than withdrawal amount.
    /// </returns>
    [HttpPost("{id}/Withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return _errors.NotFound(HttpStatusCode.BadRequest);
        }
        else if (amount > account.Balance)
        {
            return _errors.Response("Amount entered is more than account balance.", HttpStatusCode.BadRequest);
        }

        account.Balance -= amount;
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Transfers a provided amount from one user's account to another users's account.
    /// </summary>
    /// <param name="accountTransfer"></param>
    /// <returns>
    /// Returns 204 if successful, 400 if any account does not exist OR if account transfer amount is more than what exists from withdrawal account.
    /// </returns>
    [HttpPost("Transfer")]
    public async Task<IActionResult> Transfer(AccountTransfer accountTransfer)
    {
        var account = await _context.Accounts.FindAsync(accountTransfer.TransferFromId);
        var accountToTransferTo = await _context.Accounts.FindAsync(accountTransfer.TransferToId);

        if (account is null)
        {
            return _errors.Response("Could not find transfer from account with provided GUID.", HttpStatusCode.BadRequest);
        }
        else if (accountToTransferTo is null)
        {
            return _errors.Response("Could not find transfer to account with provided GUID.", HttpStatusCode.BadRequest);
        }
        else if (accountTransfer.Amount > account.Balance)
        {
            return _errors.Response("Amount entered is more than account balance.", HttpStatusCode.BadRequest);
        }

        await Withdraw(account.Id, accountTransfer.Amount);
        await Deposit(accountToTransferTo.Id, accountTransfer.Amount);
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Creates an account.
    /// </summary>
    /// <param name="accountCreate"></param>
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
    /// <param name="id"></param>
    /// <returns>
    /// 204 if successful, 400 if account is not found.
    /// </returns>
    [HttpPut("{id}/Reactivate")]
    public async Task<IActionResult> ReactivateAccount(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return _errors.NotFound(HttpStatusCode.BadRequest);
        }
        else if (account.Active)
        {
            return _errors.Response("Account already active.", HttpStatusCode.BadRequest);
        }

        account.Active = true;
        await AccountsContextUtils.TrySaveContext(_context, account);

        return NoContent();
    }

    /// <summary>
    /// Soft deletes an account.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>
    /// 204 if successful, 400 if account is already active or if account balance is still greater than 0.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            return _errors.NotFound(HttpStatusCode.BadRequest);
        }
        else if (account.Active == false)
        {
            return _errors.Response("Account already inactive.", HttpStatusCode.BadRequest);
        }
        else if (account.Balance > 0)
        {
            return _errors.Response("Account currently has a balance greater than 0, please withdraw first.", HttpStatusCode.BadRequest);
        }

        account.Active = false;
        account.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

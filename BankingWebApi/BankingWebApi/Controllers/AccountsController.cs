﻿using Microsoft.AspNetCore.Mvc;
using BankingWebApi.Models;
using BankingWebApi.Clients;
using BankingWebApi.Interfaces;
using System.Text.Json;

namespace BankingWebApi.Controllers;

/// <summary>
/// Controller for web api endpoints.
/// </summary>
[Route("api/[controller]")]
[ApiVersion("1.0")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly CurrencyClient _currencyClient;
    private readonly IConfiguration _configuration;
    private readonly IAccountRepository _accountRepository;

    /// <summary>
    /// Accounts controller.
    /// </summary>
    /// <param name="currencyClient">Dependency injection for currency client for API calls.</param>
    /// <param name="configuration">Dependency injection for appsettings.json to get API key.</param>
    /// <param name="accountRepository">Dependency injection for account repository.</param>
    public AccountsController(CurrencyClient currencyClient, IConfiguration configuration, IAccountRepository accountRepository)
    {
        _currencyClient = currencyClient;
        _configuration = configuration;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Gets all accounts.
    /// </summary>
    /// <returns>
    /// List of all accounts or empty list if no accounts found.
    /// </returns>
    /// <response code="200">Returns accounts that match the provided filters.</response>
    /// <response code="400">Returns bad request if wrong properties provided.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IEnumerable<Account>> GetAccounts([FromQuery] AccountsFilter filters)
    {
        var (accounts, paginationMetadata) = await _accountRepository.GetAccounts(filters);
        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

        return accounts;
    }

    /// <summary>
    /// Gets account by id.
    /// </summary>
    /// <param name="id">GUID of account you want to view.</param>
    /// <returns>
    /// Account if found.
    /// </returns>
    /// <response code="200">Returns account that match the provided id.</response>
    /// <response code="404">Returns not found if no account found with provided id.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> GetAccount(Guid id)
    {
        var account = await _accountRepository.GetAccount(id);

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
    /// <response code="200">Returns balance in requested currencies that match the provided account id.</response>
    /// <response code="400">Returns bad request if account is inactive.</response>
    /// <response code="404">Returns not found if no account found with provided id.</response>
    [HttpGet("{id}/Currency/Balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetCurrencyConversion(Guid id, string currency)
    {
        var account = await _accountRepository.GetAccount(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }
        else if (account.Active is false)
        {
            return BadRequest("Account is not active.");
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
    /// <response code="200">Returns 200 OK if name changed successfully.</response>
    /// <response code="400">Returns bad request if account is inactive.</response>
    /// <response code="404">Returns not found if no account found with provided id.</response>
    [HttpPut("{id}/Name")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeName(Guid id, string name)
    {
        var account = await _accountRepository.GetAccount(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }
        else if (account.Active is false)
        {
            return BadRequest("Account is not active.");
        }

        await _accountRepository.ChangeName(id, name);
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
    /// <response code="200">Returns 200 OK if deposit was successful.</response>
    /// <response code="400">Returns bad request if account is inactive.</response>
    /// <response code="404">Returns not found if no account found with provided id.</response>
    [HttpPost("{id}/Deposit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deposit(Guid id, decimal amount)
    {
        var account = await _accountRepository.GetAccount(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }
        else if (account.Active is false)
        {
            return BadRequest("Account is not active.");
        }

        _accountRepository.Deposit(id, amount);
        return NoContent();
    }

    /// <summary>
    /// Withdraws money from account balance.
    /// </summary>
    /// <param name="id">GUID of account you want to apply withdrawal to.</param>
    /// <param name="amount">Decimal amount of money you want to withdraw from account.</param>
    /// <returns>
    /// Returns 200 if withdrawal is successful.
    /// </returns>
    /// <response code="200">Returns 200 OK if withdrawal was successful.</response>
    /// <response code="400">Returns bad request if account is not active or balance is less than withdrawal amount.</response>
    /// <response code="404">Returns not found if no account found with provided id.</response>
    [HttpPost("{id}/Withdrawal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Withdraw(Guid id, decimal amount)
    {
        var account = await _accountRepository.GetAccount(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }
        else if (account.Active is false)
        {
            return BadRequest("Account is not active.");
        }
        else if (amount > account.Balance)
        {
            return BadRequest("Amount entered is more than account balance.");
        }

        _accountRepository.Withdraw(id, amount);
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
    /// Returns 200 if transfer successful.
    /// </returns>
    /// <response code="200">Returns 200 OK if transfer was successfull.</response>
    /// <response code="400">Returns bad request if any account is inactive or transfer amount is more than transfer from account balance.</response>
    /// <response code="404">Returns not found if any account not found with provided id.</response>
    [HttpPost("Transfers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transfer(AccountTransfer accountTransfer)
    {
        var account = await _accountRepository.GetAccount(accountTransfer.TransferFromId);
        var accountToTransferTo = await _accountRepository.GetAccount(accountTransfer.TransferToId);

        if (account is null)
        {
            return NotFound("Could not find transfer from account with provided GUID.");
        }
        else if (account.Active is false)
        {
            return BadRequest("Transfer from account is not active.");
        }
        else if (accountToTransferTo is null)
        {
            return NotFound("Could not find transfer to account with provided GUID.");
        }
        else if (accountToTransferTo.Active is false)
        {
            return BadRequest("Transfer to account is not active.");
        }
        else if (accountTransfer.Amount > account.Balance)
        {
            return BadRequest("Amount entered is more than account balance.");
        }

        _accountRepository.Transfer(accountTransfer);
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
    /// <response code="200">Returns account that was created.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountCreate>> PostAccount(AccountCreate accountCreate)
    {
        var account = await _accountRepository.CreateAccount(accountCreate);
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }

    /// <summary>
    /// Reactivates a deactivated account.
    /// </summary>
    /// <param name="id">GUID of account you want to reactivate.</param>
    /// <returns>
    /// 204 if successful, 400 if account is not found.
    /// </returns>
    /// <response code="200">Returns 200 OK if reactivation was successfull.</response>
    /// <response code="400">Returns bad request if account is still active.</response>
    /// <response code="404">Returns not found if account not found with provided id.</response>
    [HttpPut("{id}/Activation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateAccount(Guid id)
    {
        var account = await _accountRepository.GetAccount(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }
        else if (account.Active)
        {
            return BadRequest("Account already active.");
        }

        _accountRepository.ReactivateAccount(id);
        return NoContent();
    }

    /// <summary>
    /// Soft deletes an account.
    /// </summary>
    /// <param name="id">GUID of account you want to deactivate</param>
    /// <returns>
    /// 204 if successful, 400 if account is already active or if account balance is still greater than 0.
    /// </returns>
    /// <response code="200">Returns 200 OK if deactivation was successfull.</response>
    /// <response code="400">Returns bad request if account is already inactive or there is a current balance greater than 0.</response>
    /// <response code="404">Returns not found if account not found with provided id.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var account = await _accountRepository.GetAccount(id);

        if (account is null)
        {
            return NotFound("Could not find account with provided GUID.");
        }
        else if (account.Active is false)
        {
            return BadRequest("Account already inactive.");
        }
        else if (account.Balance > 0)
        {
            return BadRequest("Account currently has a balance greater than 0, please withdraw first.");
        }

        _accountRepository.DeleteAccount(id);
        return NoContent();
    }
}

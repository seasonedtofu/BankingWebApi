using Microsoft.EntityFrameworkCore;
using BankingWebApi.Interfaces;
using BankingWebApi.Models;

namespace BankingWebApi.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AccountsContext _context;

    private bool AccountExistsAndActive(Account account)
    {
        return !(account is null) && account.Active is true;
    }

    public AccountRepository(AccountsContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Account>> GetAccounts()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<Account> GetAccount(Guid id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    public async Task<Account> ChangeName(Guid id, string name)
    {
        var account = await GetAccount(id);

        if (AccountExistsAndActive(account))
        {
            account.Name = name;
            await _context.SaveChangesAsync();
        }

        return account;
    }

    public async void Deposit(Guid id, decimal amount)
    {
        var account = await GetAccount(id);

        if (AccountExistsAndActive(account))
        {
            account.Balance += amount;
            await _context.SaveChangesAsync();
        }
    }

    public async void Withdraw(Guid id, decimal amount)
    {
        var account = await GetAccount(id);

        if (AccountExistsAndActive(account) && account.Balance > amount)
        {
            account.Balance -= amount;
            await _context.SaveChangesAsync();
        }
    }

    public async void Transfer(AccountTransfer accountTransfer)
    {
        var account = await GetAccount(accountTransfer.TransferFromId);

        if (AccountExistsAndActive(account) && account.Balance > accountTransfer.Amount)
        {
            Withdraw(accountTransfer.TransferFromId, accountTransfer.Amount);
            Deposit(accountTransfer.TransferToId, accountTransfer.Amount);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Account> CreateAccount(AccountCreate accountCreate)
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
        return account;
    }

    public async void ReactivateAccount(Guid id)
    {
        var account = await GetAccount(id);
        if (account.Active is false)
        {
            account.Active = true;
            await _context.SaveChangesAsync();
        }
    }

    public async void DeleteAccount(Guid id)
    {
        var account = await GetAccount(id);
        if (account.Active is true)
        {
            account.Active = false;
            account.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

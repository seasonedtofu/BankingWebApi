using BankingWebApi.Domain.Entities;
using BankingWebApi.Domain.Interfaces;
using BankingWebApi.Infrastructure.Data;
using BankingWebApi.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BankingWebApi.Infrastructure.Repositories;
public class AccountRepository : IAccountRepository
{
    private readonly AccountsDbContext _context;

    public AccountRepository(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task CreateAccount(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Account>> GetAccounts(AccountsFilter filters)
    {
        var pageSize = filters.PageSize;
        var active = filters.Active;
        var sortBy = typeof(Account).GetProperty(filters.SortBy);
        var sortOrder = filters.SortOrder;

        var accounts =
            await _context.Accounts
                .Where(account =>
                    account.Name.ToLower().Contains(filters.SearchTerm.ToLower())
                    && (active != null ? active == account.Active : true))
                .OrderByDynamic(account => sortBy.GetValue(account), sortOrder == "Asc" ? true : false)
                .Skip(pageSize * (filters.PageNumber - 1))
                .Take(pageSize)
                .ToAsyncEnumerable()
                .ToListAsync();

        return accounts;
    }

    public async Task<Account> GetAccount(Guid id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    public async Task UpdateName(Guid id, string name)
    {
        var account = await GetAccount(id);
        account.Name = name;
        await _context.SaveChangesAsync();
    }

    public async Task AddToBalance(Guid id, decimal amount)
    {
        var account = await GetAccount(id);
        account.Balance += amount;
        await _context.SaveChangesAsync();
    }

    public async Task SubtractFromBalance(Guid id, decimal amount)
    {
        var account = await GetAccount(id);
        account.Balance -= amount;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateActive(Guid id, bool active)
    {
        var account = await GetAccount(id);
        account.Active = active;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccount(Guid id)
    {
        var account = await GetAccount(id);
        _context.Remove(account);
        await _context.SaveChangesAsync();
    }
}

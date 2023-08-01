using BankingWebApi.Interfaces;
using BankingWebApi.Models;
using BankingWebApi.Extensions;
using BankingWebApi.Context;
using BankingWebApi.DataTransformationObjects;
using AutoMapper;

namespace BankingWebApi.Repositories;
public class AccountRepository : IAccountRepository
{
    private readonly AccountsDbContext _context;
    private readonly IMapper _mapper;
    private enum _sortBy
    {
        CreatedDate,
        UpdatedDate,
        Name,
        Balance
    }
    private enum _sortOrder
    {
        Asc,
        Desc
    }
    private bool AccountExistsAndActive(Account account)
    {
        if (account is null)
        {
            throw new InvalidOperationException("Account does not exist.");
        }
        else if (account.Active is false)
        {
            throw new InvalidOperationException("Account is inactive.");
        }
        return true;
    }
    private async Task<Account> GetAccount(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
        {
            throw new InvalidOperationException("Account does not exist.");
        }

        return account;
    }

    public AccountRepository(AccountsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<(List<AccountDto>, PaginationMetadata)> GetAccounts(AccountsFilter filters)
    {
        var pageSize = filters.PageSize;
        var active = filters.Active;
        var sortBy = typeof(Account).GetProperty(filters.SortBy);
        var sortOrder = filters.SortOrder;

        var accounts = _mapper.Map<List<AccountDto>>(
            await _context.Accounts
                .Where(account =>
                    account.Name.ToLower().Contains(filters.SearchTerm.ToLower())
                    && (active != null ? active == account.Active : true))
                .OrderByDynamic(account => sortBy.GetValue(account), sortOrder == "Asc" ? true : false)
                .Skip(pageSize * (filters.PageNumber - 1))
                .Take(pageSize)
                .ToAsyncEnumerable()
                .ToListAsync());

        var paginationMetadata = new PaginationMetadata(accounts.Count(), pageSize, filters.PageNumber);

        return (accounts, paginationMetadata);
    }

    public async Task<AccountDto> GetAccountDto(Guid id)
    {
        return _mapper.Map<AccountDto>(await GetAccount(id));
    }

    public async Task ChangeName(Guid id, string name)
    {
        var account = await GetAccount(id);

        if (AccountExistsAndActive(account))
        {
            account.Name = name;
            await _context.SaveChangesAsync();
        }
    }

    public async Task Deposit(Guid id, decimal amount)
    {
        var account = await GetAccount(id);

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException("Cannot deposit a negative amount.");
        }

        if (AccountExistsAndActive(account))
        {
            account.Balance += amount;
            await _context.SaveChangesAsync();
        }
    }

    public async Task Withdraw(Guid id, decimal amount)
    {
        var account = await GetAccount(id);

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException("Cannot withdraw a negative amount.");
        }

        if (AccountExistsAndActive(account))
        {
            if (amount > account.Balance)
            {
                throw new InvalidOperationException("Requested withdrawal amount is more than account balance.");
            }
            account.Balance -= amount;
            await _context.SaveChangesAsync();
        }
    }

    public async Task Transfer(AccountTransferDto accountTransfer)
    {
        var accountFrom = await GetAccount(accountTransfer.TransferFromId);
        var accountTo = await GetAccount(accountTransfer.TransferToId);

        if (AccountExistsAndActive(accountFrom) && AccountExistsAndActive(accountTo))
        {
            await Withdraw(accountTransfer.TransferFromId, accountTransfer.Amount);
            await Deposit(accountTransfer.TransferToId, accountTransfer.Amount);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Account> CreateAccount(CreateAccountDto accountCreate)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = accountCreate.Name,
            Balance = accountCreate.Balance,
            Active = true,
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task ReactivateAccount(Guid id)
    {
        var account = await GetAccount(id);
        if (account.Active is true)
        {
            throw new InvalidOperationException("Account is already active.");
        }
        account.Active = true;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccount(Guid id)
    {
        var account = await GetAccount(id);
        if (account.Active is false)
        {
            throw new InvalidOperationException("Account is already deactivated.");
        }
        else if (account.Balance > 0)
        {
            throw new InvalidOperationException("Account still has a balance of greater than 0, please withdraw before deactivating account.");
        }
        account.Active = false;
        await _context.SaveChangesAsync();
    }
}

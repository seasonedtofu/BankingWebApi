using BankingWebApi.Application.Interfaces;
using BankingWebApi.Application.Models;
using BankingWebApi.Domain.Entities;
using BankingWebApi.Infrastructure.Interfaces;
using AutoMapper;

namespace BankingWebApi.Application.Services
{
    public class AccountsServices : IAccountsServices
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        private bool AccountExistsAndActive(AccountDto account)
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

        public AccountsServices(IAccountRepository accountRepository, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        public async Task<AccountDto> CreateAccount(CreateAccountDto accountCreate)
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Name = accountCreate.Name,
                Balance = accountCreate.Balance,
                Active = true,
            };

            await _accountRepository.CreateAccount(account);
            return _mapper.Map<AccountDto>(account);
        }

        public async Task<(List<AccountDto>, PaginationMetadata)> GetAccounts(AccountsFilter filters)
        {
            var accounts = _mapper.Map<List<AccountDto>>(await _accountRepository.GetAccounts(filters));

            var paginationMetadata = new PaginationMetadata(accounts.Count(), filters.PageSize, filters.PageNumber);

            return (accounts, paginationMetadata);
        }

        public async Task<AccountDto> GetAccount(Guid id)
        {
            var account = await _accountRepository.GetAccount(id);

            if (account is null)
            {
                throw new InvalidOperationException("Account does not exist.");
            }

            return _mapper.Map<AccountDto>(account);
        }

        public async Task ChangeName(Guid id, string name)
        {
            var account = await GetAccount(id);

            if (AccountExistsAndActive(account))
            {
                await _accountRepository.UpdateName(id, name);
            }
        }

        public async Task Deposit(Guid id, decimal amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("Cannot deposit a negative amount.");
            }

            var account = await GetAccount(id);

            if (AccountExistsAndActive(account))
            {
                await _accountRepository.AddToBalance(id, amount);
            }
        }

        public async Task Withdraw(Guid id, decimal amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("Cannot withdraw a negative amount.");
            }

            var account = await GetAccount(id);

            if (AccountExistsAndActive(account))
            {
                if (amount > account.Balance)
                {
                    throw new InvalidOperationException("Requested withdrawal amount is more than account balance.");
                }

                await _accountRepository.SubtractFromBalance(id, amount);
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
            }
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

            await _accountRepository.UpdateActive(id, false);
        }

        public async Task ReactivateAccount(Guid id)
        {
            var account = await GetAccount(id);

            if (account.Active is true)
            {
                throw new InvalidOperationException("Account is already active.");
            }

            await _accountRepository.UpdateActive(id, true);
        }

        public async Task HardDeleteAccount(Guid id)
        {
            await _accountRepository.DeleteAccount(id);
        }
    }
}

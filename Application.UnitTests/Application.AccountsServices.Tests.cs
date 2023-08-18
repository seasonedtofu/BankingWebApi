using BankingWebApi.Application.Interfaces;
using BankingWebApi.Application.Models;
using BankingWebApi.Domain.Entities;
using BankingWebApi.Domain.Interfaces;
using AutoMapper;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace BankingWebApi.Application.Tests
{
    public class TestAccountsServices
    {
        private Mock<IAccountsServices> _services;
        private Mock<IAccountRepository> _repository;
        private Mock<DbSet<Account>> _mockDbSet;
        private IMapper _mapper;

        public TestAccountsServices()
        {
            _services = new Mock<IAccountsServices>();
            _repository = new Mock<IAccountRepository>();
            _mockDbSet = GetMockDbSet(new List<Account>());

            var config = new MapperConfiguration(cfg => cfg.CreateMap<Account, AccountDto>());
            _mapper = config.CreateMapper();

            _repository.Setup(r => r.CreateAccount(It.IsAny<Account>()))
                .Callback((Account account) =>
                {
                    _mockDbSet.Object.Add(account);
                });
            _repository.Setup(r => r.GetAccounts(It.IsAny<AccountsFilter>()))
                .Returns(async (AccountsFilter filters) =>
                {
                    var pageSize = filters.PageSize;
                    var active = filters.Active;
                    var sortBy = typeof(Account).GetProperty(filters.SortBy);
                    var sortOrder = filters.SortOrder;
                    var accounts = await _mockDbSet.Object.Where(account =>
                        account.Name.ToLower().Contains(filters.SearchTerm.ToLower())
                            && (active != null ? active == account.Active : true))
                        .OrderBy(account => $"{sortBy.GetValue(account)} {sortOrder}")
                        .Skip(pageSize * (filters.PageNumber - 1))
                        .Take(pageSize)
                        .ToAsyncEnumerable()
                        .ToListAsync();
                    return accounts;
                });
            _repository.Setup(r => r.GetAccount(It.IsAny<Guid>()))
                .Returns((Guid id) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    return Task.FromResult(account);
                });
            _repository.Setup(r => r.UpdateName(It.IsAny<Guid>(), It.IsAny<string>()))
                .Callback((Guid id, string name) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    account.Name = name;
                });
            _repository.Setup(r => r.AddToBalance(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback((Guid id, decimal amount) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    account.Balance += amount;
                });
            _repository.Setup(r => r.SubtractFromBalance(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback((Guid id, decimal amount) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    account.Balance -= amount;
                });
            _repository.Setup(r => r.UpdateActive(It.IsAny<Guid>(), It.IsAny<Boolean>()))
                .Callback((Guid id, Boolean active) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    account.Active = active;
                });
            _repository.Setup(r => r.DeleteAccount(It.IsAny<Guid>()))
                .Callback((Guid id) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    _mockDbSet.Object.Remove(account);
                });

            _services.Setup(r => r.GetAccount(It.IsAny<Guid>()))
                .Returns(async (Guid id) =>
                {
                    var account = await _repository.Object.GetAccount(id);

                    if (account is null)
                    {
                        throw new InvalidOperationException("Account does not exist.");
                    }

                    return _mapper.Map<AccountDto>(account);
                });
        }

        internal static Mock<DbSet<T>> GetMockDbSet<T>(ICollection<T> entities) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(entities.AsQueryable().Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(entities.AsQueryable().Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(entities.AsQueryable().ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(entities.AsQueryable().GetEnumerator());
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(entities.Add);
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(entity => entities.Remove(entity));
            return mockSet;
        }

        [Fact]
        public async Task CreateAccountCreatesAccount()
        {
            _services.Setup(r => r.CreateAccount(It.IsAny<CreateAccountDto>()))
                .Callback((CreateAccountDto createAccountDto) =>
                {
                    var account = new Account
                    {
                        Id = Guid.NewGuid(),
                        Name = createAccountDto.Name,
                        Balance = createAccountDto.Balance,
                        Active = true,
                    };

                    _repository.Object.CreateAccount(account);
                });

            var createAccountDto = new CreateAccountDto()
            {
                Name = "test",
                Balance = 123,
            };

            var oldCount = (await _repository.Object.GetAccounts(new AccountsFilter())).Count();
            await _services.Object.CreateAccount(createAccountDto);
            var newCount = (await _repository.Object.GetAccounts(new AccountsFilter())).Count();

            Assert.True(newCount == oldCount + 1);
        }

        [Fact]
        public async Task GetAccountsReturnsTupleOfAccountsDtoAndPaginationMetadata()
        {
            _services.Setup(r => r.GetAccounts(It.IsAny<AccountsFilter>()))
                .Returns(async (AccountsFilter accountsFilter) =>
                {
                    var accounts = await _repository.Object.GetAccounts(accountsFilter);
                    var paginationMetadata = new PaginationMetadata(accounts.Count(), accountsFilter.PageSize, accountsFilter.PageNumber);
                    return (_mapper.Map<List<AccountDto>>(accounts), paginationMetadata);
                });

            var (accounts, paginationMetadata) = await _services.Object.GetAccounts(new AccountsFilter());

            Assert.IsType<List<AccountDto>>(accounts);
            Assert.IsType<PaginationMetadata>(paginationMetadata);
        }

        [Fact]
        public async Task GetAccountReturnsAccount()
        {
            var id = Guid.NewGuid();
            var newAccount = new Account
            {
                Id = id,
                Name = "test",
                Balance = 100,
                Active = true,
            };

            await _repository.Object.CreateAccount(newAccount);
            var account = await _services.Object.GetAccount(id);

            Assert.True(account.Id == id);
        }

        [Fact]
        public async Task GetAccountIfDoesNotExistThrowsInvalidOperationException()
        {
            var id = Guid.NewGuid();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _services.Object.GetAccount(id));
        }

        [Fact]
        public async Task ChangeNameUpdatesAccountName()
        {
            _services.Setup(r => r.ChangeName(It.IsAny<Guid>(), It.IsAny<string>()))
                .Callback(async (Guid id, string name) =>
                {
                    await _repository.Object.UpdateName(id, name);
                });

            var id = Guid.NewGuid();
            var newAccount = new Account
            {
                Id = id,
                Name = "test",
                Balance = 100,
                Active = true,
            };
            await _repository.Object.CreateAccount(newAccount);

            var oldName = (await _services.Object.GetAccount(id)).Name;
            await _services.Object.ChangeName(id, "changed!");
            var newName = (await _services.Object.GetAccount(id)).Name;

            Assert.False(oldName == newName);
            Assert.True(newName == "changed!");
        }

        [Fact]
        public async Task DepositAddsToAccountBalance()
        {
            _services.Setup(r => r.Deposit(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback(async (Guid id, decimal amount) =>
                {
                    await _repository.Object.AddToBalance(id, amount);
                });

            var id = Guid.NewGuid();
            var newAccount = new Account
            {
                Id = id,
                Name = "test",
                Balance = 100,
                Active = true,
            };

            await _repository.Object.CreateAccount(newAccount);

            var oldAmount = (await _services.Object.GetAccount(id)).Balance;
            var amountToAdd = 150;
            await _services.Object.Deposit(id, amountToAdd);
            var newAmount = (await _services.Object.GetAccount(id)).Balance;

            Assert.True(newAmount == oldAmount + amountToAdd);
        }

        [Fact]
        public async Task DepositAmountLessThanZeroThrowsArgumentOutOfRangeException()
        {
            _services.Setup(r => r.Deposit(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback((Guid id, decimal amount) =>
                {
                    if (amount < 0)
                    {
                        throw new ArgumentOutOfRangeException("Cannot deposit a negative amount.");
                    }
                });

            var id = Guid.NewGuid();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _services.Object.Deposit(id, -1));
        }

        [Fact]
        public async Task WithdrawSubtractsFromAccountBalance()
        {
            _services.Setup(r => r.Withdraw(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback(async (Guid id, decimal amount) =>
                {
                    await _repository.Object.SubtractFromBalance(id, amount);
                });

            var id = Guid.NewGuid();
            var newAccount = new Account
            {
                Id = id,
                Name = "test",
                Balance = 200,
                Active = true,
            };

            await _repository.Object.CreateAccount(newAccount);

            var oldAmount = (await _services.Object.GetAccount(id)).Balance;
            var amountToSubtract = 150;
            await _services.Object.Withdraw(id, amountToSubtract);
            var newAmount = (await _services.Object.GetAccount(id)).Balance;

            Assert.True(newAmount == oldAmount - amountToSubtract);
        }

        [Fact]
        public async Task WithdrawAmountLessThanZeroThrowsArgumentOutOfRangeException()
        {
            _services.Setup(r => r.Withdraw(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback((Guid id, decimal amount) =>
                {
                    if (amount < 0)
                    {
                        throw new ArgumentOutOfRangeException("Cannot withdraw a negative amount.");
                    }
                });

            var id = Guid.NewGuid();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _services.Object.Withdraw(id, -1));
        }

        [Fact]
        public async Task WithdrawAmountGreaterThanAccountBalanceThrowsInvalidOperationException()
        {
            _services.Setup(r => r.Withdraw(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback(async (Guid id, decimal amount) =>
                {
                    var account = await _services.Object.GetAccount(id);

                    Debug.WriteLine(amount);
                    Debug.WriteLine(account.Balance);

                    if (amount > account.Balance)
                    {
                        throw new InvalidOperationException("Cannot withdraw more than account balance.");
                    }
                });

            var id = Guid.NewGuid();
            var newAccount = new Account
            {
                Id = id,
                Name = "test",
                Balance = 200,
                Active = true,
            };

            await _repository.Object.CreateAccount(newAccount);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _services.Object.Withdraw(id, 999));
        }

        [Fact]
        public async Task TransferBetweenAccountsTransfersProperly()
        {
            _services.Setup(r => r.Transfer(It.IsAny<AccountTransferDto>()))
                .Callback(async (AccountTransferDto accountTransferDto) =>
                {
                    await _repository.Object.SubtractFromBalance(accountTransferDto.TransferFromId, accountTransferDto.Amount);
                    await _repository.Object.AddToBalance(accountTransferDto.TransferToId, accountTransferDto.Amount);
                });

            var accountFromId = Guid.NewGuid();
            var accountToId = Guid.NewGuid();
            var accountFrom = new Account()
            {
                Id = accountFromId,
                Name = "accountFrom",
                Balance = 150,
            };
            var accountTo = new Account()
            {
                Id = accountToId,
                Name = "accountTo",
                Balance = 0,
            };
            await _repository.Object.CreateAccount(accountFrom);
            await _repository.Object.CreateAccount(accountTo);

            var originalAccountFromBalance = accountFrom.Balance;
            var originalAccountToBalance = accountTo.Balance;
            var amountToTransfer = 50;
            var accountTransferDto = new AccountTransferDto()
            {
                TransferFromId = accountFromId,
                TransferToId = accountToId,
                Amount = amountToTransfer,
            };
            await _services.Object.Transfer(accountTransferDto);

            var accountFromBalance = (await _services.Object.GetAccount(accountFromId)).Balance;
            var accountToBalance = (await _services.Object.GetAccount(accountToId)).Balance;

            Assert.True(accountFromBalance == originalAccountFromBalance - amountToTransfer);
            Assert.True(accountToBalance == originalAccountToBalance + amountToTransfer);
        }

        [Fact]
        public async Task DeleteAccountSoftDeletesAccount()
        {
            _services.Setup(r => r.DeleteAccount(It.IsAny<Guid>()))
                .Callback(async (Guid id) =>
                {
                    await _repository.Object.UpdateActive(id, false);
                });


            var id = Guid.NewGuid();
            var newAccount = new Account
            {
                Id = id,
                Name = "test",
                Balance = 200,
                Active = true,
            };

            await _repository.Object.CreateAccount(newAccount);

            var oldActive = (await _services.Object.GetAccount(id)).Active;
            await _services.Object.DeleteAccount(id);
            var newActive = (await _services.Object.GetAccount(id)).Active;

            Assert.True(oldActive);
            Assert.False(newActive);
        }

        [Fact]
        public async Task ReactivateAccountUpdatesActiveToTrue()
        {
            _services.Setup(r => r.ReactivateAccount(It.IsAny<Guid>()))
                .Callback(async (Guid id) =>
                {
                    await _repository.Object.UpdateActive(id, true);
                });


            var id = Guid.NewGuid();
            var newAccount = new Account
            {
                Id = id,
                Name = "test",
                Balance = 200,
                Active = false,
            };

            await _repository.Object.CreateAccount(newAccount);

            var oldActive = (await _services.Object.GetAccount(id)).Active;
            await _services.Object.ReactivateAccount(id);
            var newActive = (await _services.Object.GetAccount(id)).Active;

            Assert.False(oldActive);
            Assert.True(newActive);
        }

        [Fact]
        public async Task HardDeleteAccountRemovesAccountFromDbSet()
        {
            _services.Setup(r => r.HardDeleteAccount(It.IsAny<Guid>()))
                .Callback(async (Guid id) =>
                {
                    await _repository.Object.DeleteAccount(id);
                });


            var id = Guid.NewGuid();
            var newAccount = new Account
            {
                Id = id,
                Name = "test",
                Balance = 200,
                Active = true,
            };

            await _repository.Object.CreateAccount(newAccount);

            var accountBeforeHardDelete = await _services.Object.GetAccount(id);
            Assert.IsType<AccountDto>(accountBeforeHardDelete);

            await _services.Object.HardDeleteAccount(id);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _services.Object.GetAccount(id));
        }
    }
}
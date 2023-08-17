using BankingWebApi.Domain.Entities;
using BankingWebApi.Infrastructure.Data;
using BankingWebApi.Infrastructure.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BankingWebApi.Infrastructure.Tests
{
    public class TestAccountRepository
    {
        private Mock<IAccountRepository> _repository;
        private Mock<DbSet<Account>> _mockDbSet;
        private static Guid _idOne = Guid.NewGuid();
        private static Guid _idTwo = Guid.NewGuid();

        public TestAccountRepository()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            var accounts = new List<Account>
            {
                new Account()
                {
                    Id = _idOne,
                    Name = "John Doe",
                    Balance = 100,
                    Active = true,
                },
                new Account
                {
                    Id = _idTwo,
                    Name = "Jane Doe",
                    Balance = 200,
                    Active = true,
                }
            };
            var ctx = new Mock<AccountsDbContext>(optionsBuilder.Options);
            _mockDbSet = GetMockDbSet(accounts);
            ctx.Setup(c => c.Accounts).Returns(_mockDbSet.Object);

            _repository = new Mock<IAccountRepository>();

            _repository.Setup(r => r.GetAccount(It.IsAny<Guid>())).Returns((Guid id) =>
            {
                var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();

                if (account is null)
                {
                    throw new InvalidOperationException("Account does not exist.");
                }

                return Task.FromResult(account);
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
        public async Task CreateAccountAddsAccountToDb()
        {
            _repository.Setup(r => r.CreateAccount(It.IsAny<Account>()))
                .Callback((Account account) =>
                {
                    _mockDbSet.Object.Add(account);
                });

            var originalAmountOfAccounts = _mockDbSet.Object.Count();
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "test",
                Balance = 100,
                Active = true,
            };

            await _repository.Object.CreateAccount(account);
            var updatedAmountOfAccounts = _mockDbSet.Object.Count();
            Assert.True(updatedAmountOfAccounts == originalAmountOfAccounts + 1);
        }

        [Fact]
        public async Task GetAccountsReturnsListOfAccounts()
        {
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

            var filter = new AccountsFilter();
            var accounts = await _repository.Object.GetAccounts(filter);

            Assert.IsType<List<Account>>(accounts);
            Assert.NotEmpty(accounts);
        }

        [Fact]
        public async Task GetAccountReturnsAccount()
        {
            var accountOne = await _repository.Object.GetAccount(_idOne);
            var accountTwo = await _repository.Object.GetAccount(_idTwo);

            Assert.IsType<Account>(accountOne);
            Assert.True(accountOne.Id == _idOne);
            Assert.IsType<Account>(accountTwo);
            Assert.True(accountTwo.Id == _idTwo);
        }

        [Fact]
        public async Task UpdateNameUpdatesName()
        {
            _repository.Setup(r => r.UpdateName(It.IsAny<Guid>(), It.IsAny<string>()))
                .Callback((Guid id, string name) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    if (account is not null)
                    {
                        account.Name = name;
                    }
                });

            var originalName = (await _repository.Object.GetAccount(_idOne)).Name;
            await _repository.Object.UpdateName(_idOne, "changed");
            var changedName = (await _repository.Object.GetAccount(_idOne)).Name;

            Assert.False(originalName == changedName);
            Assert.True(changedName == "changed");
        }

        [Fact]
        public async Task AddToBalanceAddsToAccountBalance()
        {
            _repository.Setup(r => r.AddToBalance(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback((Guid id, decimal amount) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    if (account is not null)
                    {
                        account.Balance += amount;
                    }
                });

            var originalBalance = (await _repository.Object.GetAccount(_idOne)).Balance;
            await _repository.Object.AddToBalance(_idOne, 200);
            var newBalance = (await _repository.Object.GetAccount(_idOne)).Balance;
            Assert.True(newBalance == originalBalance + 200);
        }

        [Fact]
        public async Task SubtractFromBalanceSubtractsFromAccountBalance()
        {
            _repository.Setup(r => r.SubtractFromBalance(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback((Guid id, decimal amount) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    if (account is not null)
                    {
                        account.Balance -= amount;
                    }
                });

            var originalBalance = (await _repository.Object.GetAccount(_idOne)).Balance;
            await _repository.Object.SubtractFromBalance(_idOne, 50);
            var newBalance = (await _repository.Object.GetAccount(_idOne)).Balance;
            Assert.True(newBalance == originalBalance - 50);
        }

        [Fact]
        public async Task UpdateActiveActuallyUpdatesAccountActiveProperty()
        {
            _repository.Setup(r => r.UpdateActive(It.IsAny<Guid>(), It.IsAny<Boolean>()))
                .Callback((Guid id, Boolean active) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    if (account is not null)
                    {
                        account.Active = active;
                    }
                });

            await _repository.Object.UpdateActive(_idOne, false);
            var activeFalse = (await _repository.Object.GetAccount(_idOne)).Active;
            Assert.False(activeFalse);

            await _repository.Object.UpdateActive(_idOne, true);
            var activeTrue = (await _repository.Object.GetAccount(_idOne)).Active;
            Assert.True(activeTrue);
        }

        [Fact]
        public async Task DeleteAccountActuallyDeletesAccount()
        {
            var account = await _repository.Object.GetAccount(_idTwo);
            Assert.NotNull(account);
            Assert.IsType<Account>(account);

            _repository.Setup(r => r.DeleteAccount(It.IsAny<Guid>()))
                .Callback((Guid id) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    if (account is not null)
                    {
                        _mockDbSet.Object.Remove(account);
                    }
                });

            await _repository.Object.DeleteAccount(account.Id);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.Object.GetAccount(_idTwo));
        }
    }
}
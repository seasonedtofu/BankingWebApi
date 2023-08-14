using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using BankingWebApi.Application.Interfaces;
using BankingWebApi.Application.Models;
using BankingWebApi.Domain.Entities;
using BankingWebApi.Infrastructure.Data;

namespace BankingWebApi.Tests
{
    public class TestAccountRepository
    {
        private Mock<IAccountRepository> _repository;
        private Mock<DbSet<Account>> _mockDbSet;
        private IMapper _mapper;
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

            var config = new MapperConfiguration(cfg => cfg.CreateMap<Account, AccountDto>());
            _mapper = config.CreateMapper();
            _repository = new Mock<IAccountRepository>();

            _repository.Setup(r => r.GetAccountDto(It.IsAny<Guid>())).Returns((Guid id) =>
            {
                var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                return Task.FromResult(_mapper.Map<AccountDto>(account));
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
            return mockSet;
        }

        [Fact]
        public async void GetAccountsReturnsListOfAccounts()
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
                    var accountsDto = _mapper.Map<List<AccountDto>>(accounts);
                    var paginationMetadata = new PaginationMetadata(1, 1, 1);
                    return (accountsDto, paginationMetadata);
                });

            var filter = new AccountsFilter();
            var (accounts, paginationMetadata) = await _repository.Object.GetAccounts(filter);

            Assert.IsType<List<AccountDto>>(accounts);
            Assert.NotEmpty(accounts);
        }

        [Fact]
        public async void GetAccountReturnsAccount()
        {
            var account = await _repository.Object.GetAccountDto(_idTwo);

            Assert.IsType<AccountDto>(account);
            Assert.True(account.Id == _idTwo);
        }

        [Fact]
        public async void ChangeNameChangesName()
        {
            _repository.Setup(r => r.ChangeName(It.IsAny<Guid>(), It.IsAny<string>()))
                .Callback((Guid id, string name) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    if (account is not null)
                    {
                        account.Name = name;
                    }
                });

            var originalName = (await _repository.Object.GetAccountDto(_idOne)).Name;
            await _repository.Object.ChangeName(_idOne, "changed");
            var changedName = (await _repository.Object.GetAccountDto(_idOne)).Name;

            Assert.False(originalName == changedName);
            Assert.True(changedName == "changed");
        }

        [Fact]
        public async void DepositIntoAccountAddsToBalance()
        {
            _repository.Setup(r => r.Deposit(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback((Guid id, decimal amount) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    if (account is not null)
                    {
                        account.Balance += amount;
                    }
                });

            var originalBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;
            await _repository.Object.Deposit(_idOne, 200);
            var newBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;
            Assert.True(newBalance == originalBalance + 200);
        }

        [Fact]
        public async void WithdrawFromAccountSubtractsFromBalance()
        {
            _repository.Setup(r => r.Withdraw(It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Callback((Guid id, decimal amount) =>
                {
                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
                    if (account is not null)
                    {
                        account.Balance -= amount;
                    }
                });

            var originalBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;
            await _repository.Object.Withdraw(_idOne, 50);
            var newBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;
            Assert.True(newBalance == originalBalance - 50);
        }
    }
}
using AutoMapper;
using BankingWebApi.Application.Interfaces;
using BankingWebApi.Application.Models;
using BankingWebApi.Domain.Entities;
using BankingWebApi.Infrastructure.Data;
using BankingWebApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace BankingWebApi.Tests
{
    public class TestAccountRepository
    {
        private AccountRepository _repository;
        private static Guid _idOne = Guid.NewGuid();
        private static Guid _idTwo = Guid.NewGuid();
        private static List<Account> GetFakeAccountsList()
        {
            return new List<Account>()
            {
                new Account
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
        }
        public TestAccountRepository()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Account, AccountDto>());
            var mapper = config.CreateMapper();
            var optionsBuilder = new DbContextOptionsBuilder();
            var mockAccountsDbContext = new Mock<AccountsDbContext>(optionsBuilder.Options);
            mockAccountsDbContext.Setup<DbSet<Account>>(x => x.Accounts).ReturnsDbSet(GetFakeAccountsList());
            _repository = new AccountRepository(mockAccountsDbContext.Object, mapper);
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
            var filter = new AccountsFilter();
            var (accounts, paginationMetadata) = await _repository.GetAccounts(filter);
            Assert.IsType<List<AccountDto>>(accounts);
            Assert.NotEmpty(accounts);
        }

        [Fact]
        public async void GetAccountReturnsAccount()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            var ctx = new Mock<AccountsDbContext>(optionsBuilder.Options);
            var accounts = new List<Account>
            {
                new Account()
                {
                    Id = _idOne,
                    Name = "John Doe",
                    Balance = 100,
                    Active = true,
                }
            };
            var mockDbSet = GetMockDbSet(accounts);
            ctx.Setup(c => c.Accounts).Returns(mockDbSet.Object);

            var config = new MapperConfiguration(cfg => cfg.CreateMap<Account, AccountDto>());
            var mapper = config.CreateMapper();
            var repository = new Mock<IAccountRepository>();
            repository.Setup(r => r.GetAccountDto(It.IsAny<Guid>())).ReturnsAsync(mapper.Map<AccountDto>(mockDbSet.Object.FirstOrDefault()));

            var account = await repository.Object.GetAccountDto(_idOne);

            Assert.IsType<AccountDto>(account);
        }
    }
}
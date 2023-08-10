using AutoMapper;
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
        private static List<Account> GetFakeAccountsList()
        {
            return new List<Account>()
            {
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Balance = 100,
                    Active = true,
                },
                new Account
                {
                    Id = Guid.NewGuid(),
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

        [Fact]
        public async void GetAccountsReturnsListOfAccounts()
        {
            var filter = new AccountsFilter();
            var (accounts, paginationMetadata) = await _repository.GetAccounts(filter);
            Assert.IsType<List<AccountDto>>(accounts);
        }

        [Fact]
        public async void GetAccountReturnsAccount()
        {
            var accounts = GetFakeAccountsList();
            var id = accounts[0].Id;
            var account = await _repository.GetAccountDto(id);
            Assert.IsType<AccountDto>(account);
        }
    }
}
//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Xunit;
//using BankingWebApi.Application.Interfaces;
//using BankingWebApi.Application.Models;
//using BankingWebApi.Domain.Entities;
//using BankingWebApi.Infrastructure.Data;

//namespace BankingWebApi.Infrastructure.Tests
//{
//    public class TestAccountRepository
//    {
//        private Mock<IAccountRepository> _repository;
//        private Mock<DbSet<Account>> _mockDbSet;
//        private IMapper _mapper;
//        private static Guid _idOne = Guid.NewGuid();
//        private static Guid _idTwo = Guid.NewGuid();

//        public TestAccountRepository()
//        {
//            var optionsBuilder = new DbContextOptionsBuilder();
//            var accounts = new List<Account>
//            {
//                new Account()
//                {
//                    Id = _idOne,
//                    Name = "John Doe",
//                    Balance = 100,
//                    Active = true,
//                },
//                new Account
//                {
//                    Id = _idTwo,
//                    Name = "Jane Doe",
//                    Balance = 200,
//                    Active = true,
//                }
//            };
//            var ctx = new Mock<AccountsDbContext>(optionsBuilder.Options);
//            _mockDbSet = GetMockDbSet(accounts);
//            ctx.Setup(c => c.Accounts).Returns(_mockDbSet.Object);

//            var config = new MapperConfiguration(cfg => cfg.CreateMap<Account, AccountDto>());
//            _mapper = config.CreateMapper();
//            _repository = new Mock<IAccountRepository>();

//            _repository.Setup(r => r.GetAccountDto(It.IsAny<Guid>())).Returns((Guid id) =>
//            {
//                var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();

//                if (account is null)
//                {
//                    throw new InvalidOperationException("Account does not exist.");
//                }

//                return Task.FromResult(_mapper.Map<AccountDto>(account));
//            });
//        }

//        internal static Mock<DbSet<T>> GetMockDbSet<T>(ICollection<T> entities) where T : class
//        {
//            var mockSet = new Mock<DbSet<T>>();
//            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(entities.AsQueryable().Provider);
//            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(entities.AsQueryable().Expression);
//            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(entities.AsQueryable().ElementType);
//            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(entities.AsQueryable().GetEnumerator());
//            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(entities.Add);
//            return mockSet;
//        }

//        [Fact]
//        public async void GetAccountsReturnsListOfAccounts()
//        {
//            _repository.Setup(r => r.GetAccounts(It.IsAny<AccountsFilter>()))
//                .Returns(async (AccountsFilter filters) =>
//                {
//                    var pageSize = filters.PageSize;
//                    var active = filters.Active;
//                    var sortBy = typeof(Account).GetProperty(filters.SortBy);
//                    var sortOrder = filters.SortOrder;

//                    var accounts = await _mockDbSet.Object.Where(account =>
//                        account.Name.ToLower().Contains(filters.SearchTerm.ToLower())
//                            && (active != null ? active == account.Active : true))
//                        .OrderBy(account => $"{sortBy.GetValue(account)} {sortOrder}")
//                        .Skip(pageSize * (filters.PageNumber - 1))
//                        .Take(pageSize)
//                        .ToAsyncEnumerable()
//                        .ToListAsync();
//                    var accountsDto = _mapper.Map<List<AccountDto>>(accounts);
//                    var paginationMetadata = new PaginationMetadata(1, 1, 1);
//                    return (accountsDto, paginationMetadata);
//                });

//            var filter = new AccountsFilter();
//            var (accounts, paginationMetadata) = await _repository.Object.GetAccounts(filter);

//            Assert.IsType<List<AccountDto>>(accounts);
//            Assert.NotEmpty(accounts);
//        }

//        [Fact]
//        public async void GetAccountReturnsAccount()
//        {
//            var account = await _repository.Object.GetAccountDto(_idTwo);

//            Assert.IsType<AccountDto>(account);
//            Assert.True(account.Id == _idTwo);
//        }


//        [Fact]
//        public async Task GetAccountInvalidGuidThrowsInvalidOperationException()
//        {
//            var newGuid = Guid.NewGuid();
//            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.Object.GetAccountDto(newGuid));
//        }

//        [Fact]
//        public async void ChangeNameChangesName()
//        {
//            _repository.Setup(r => r.ChangeName(It.IsAny<Guid>(), It.IsAny<string>()))
//                .Callback((Guid id, string name) =>
//                {
//                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
//                    if (account is not null)
//                    {
//                        account.Name = name;
//                    }
//                });

//            var originalName = (await _repository.Object.GetAccountDto(_idOne)).Name;
//            await _repository.Object.ChangeName(_idOne, "changed");
//            var changedName = (await _repository.Object.GetAccountDto(_idOne)).Name;

//            Assert.False(originalName == changedName);
//            Assert.True(changedName == "changed");
//        }

//        [Fact]
//        public async void DepositIntoAccountAddsToBalance()
//        {
//            _repository.Setup(r => r.Deposit(It.IsAny<Guid>(), It.IsAny<decimal>()))
//                .Callback((Guid id, decimal amount) =>
//                {
//                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
//                    if (account is not null)
//                    {
//                        account.Balance += amount;
//                    }
//                });

//            var originalBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;
//            await _repository.Object.Deposit(_idOne, 200);
//            var newBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;
//            Assert.True(newBalance == originalBalance + 200);
//        }

//        [Fact]
//        public async void DepositAmountLessThanZeroThrowsOutOfRangeException()
//        {
//            _repository.Setup(r => r.Deposit(It.IsAny<Guid>(), It.IsAny<decimal>()))
//                .Callback((Guid id, decimal amount) =>
//                {
//                    if (amount < 0) throw new ArgumentOutOfRangeException("Cannot deposit a negative amount.");
//                });

//            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _repository.Object.Deposit(_idOne, -100));
//        }

//        [Fact]
//        public async void WithdrawFromAccountSubtractsFromBalance()
//        {
//            _repository.Setup(r => r.Withdraw(It.IsAny<Guid>(), It.IsAny<decimal>()))
//                .Callback((Guid id, decimal amount) =>
//                {
//                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
//                    if (account is not null)
//                    {
//                        account.Balance -= amount;
//                    }
//                });

//            var originalBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;
//            await _repository.Object.Withdraw(_idOne, 50);
//            var newBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;
//            Assert.True(newBalance == originalBalance - 50);
//        }

//        [Fact]
//        public async void WithdrawAmountLessThanZeroThrowsOutOfRangeException()
//        {
//            _repository.Setup(r => r.Withdraw(It.IsAny<Guid>(), It.IsAny<decimal>()))
//                .Callback((Guid id, decimal amount) =>
//                {
//                    if (amount < 0) throw new ArgumentOutOfRangeException("Cannot withdraw a negative amount.");
//                });

//            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _repository.Object.Withdraw(_idOne, -100));
//        }

//        [Fact]
//        public async void WithdrawAmountGreaterThanAccountBalanceThrowsInvalidOperationException()
//        {
//            _repository.Setup(r => r.Withdraw(It.IsAny<Guid>(), It.IsAny<decimal>()))
//                .Callback((Guid id, decimal amount) =>
//                {
//                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
//                    if (amount > account.Balance) throw new InvalidOperationException("Requested withdrawal amount is more than account balance.");
//                });

//            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.Object.Withdraw(_idOne, 1000));
//        }

//        [Fact]
//        public async void TransferMoneyFromOneAccountToAnotherActuallyTransfers()
//        {
//            _repository.Setup(r => r.Transfer(It.IsAny<AccountTransferDto>()))
//                .Callback((AccountTransferDto accountTransferDto) =>
//                {
//                    var transferFrom = _mockDbSet.Object.Where((account) => account.Id == accountTransferDto.TransferFromId).FirstOrDefault();
//                    var transferTo = _mockDbSet.Object.Where((account) => account.Id == accountTransferDto.TransferToId).FirstOrDefault();
//                    transferFrom.Balance -= accountTransferDto.Amount;
//                    transferTo.Balance += accountTransferDto.Amount;
//                });

//            var transferFromOriginalBalance = (await _repository.Object.GetAccountDto(_idTwo)).Balance;
//            var transferToOriginalBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;
//            var accountTransferDto = new AccountTransferDto()
//            {
//                TransferFromId = _idTwo,
//                TransferToId = _idOne,
//                Amount = 50,
//            };

//            await _repository.Object.Transfer(accountTransferDto);
//            var transferFromTransferredBalance = (await _repository.Object.GetAccountDto(_idTwo)).Balance;
//            var transferToTransferredBalance = (await _repository.Object.GetAccountDto(_idOne)).Balance;

//            Assert.True(transferFromTransferredBalance == transferFromOriginalBalance - 50);
//            Assert.True(transferToTransferredBalance == transferToOriginalBalance + 50);
//        }

//        [Fact]
//        public async void IfTransferFromAmountIsLessThanZeroOrGreaterThanAccountBalanceThrowOutOfRangeException()
//        {
//            _repository.Setup(r => r.Transfer(It.IsAny<AccountTransferDto>()))
//                .Callback((AccountTransferDto accountTransferDto) =>
//                {
//                    var amount = accountTransferDto.Amount;

//                    if (amount < 0) throw new ArgumentOutOfRangeException("Cannot deposit/withdraw a negative amount.");
//                    var transferFrom = _mockDbSet.Object.Where((account) => account.Id == accountTransferDto.TransferFromId).FirstOrDefault();
//                    if (amount > transferFrom.Balance) throw new ArgumentOutOfRangeException("Cannot withdraw more than account balance.");
//                });

//            var accountTransferDto = new AccountTransferDto()
//            {
//                TransferFromId = _idTwo,
//                TransferToId = _idOne,
//                Amount = 500,
//            };
//            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _repository.Object.Transfer(accountTransferDto));
//            accountTransferDto.Amount = -100;
//            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _repository.Object.Transfer(accountTransferDto));
//        }

//        [Fact]
//        public async void CreateAccountAddsAccountToDb()
//        {
//            _repository.Setup(r => r.CreateAccount(It.IsAny<CreateAccountDto>()))
//                .Callback((CreateAccountDto createAccountDto) =>
//                {
//                    var account = new Account
//                    {
//                        Id = Guid.NewGuid(),
//                        Name = createAccountDto.Name,
//                        Balance = createAccountDto.Balance,
//                        Active = true,
//                    };

//                    _mockDbSet.Object.Add(account);
//                });

//            var originalAmountOfAccounts = _mockDbSet.Object.Count();
//            var createAccountDto = new CreateAccountDto()
//            {
//                Name = "test",
//                Balance = 20,
//            };

//            await _repository.Object.CreateAccount(createAccountDto);
//            var updatedAmountOfAccounts = _mockDbSet.Object.Count();
//            Assert.True(updatedAmountOfAccounts == originalAmountOfAccounts + 1);
//        }

//        [Fact]
//        public async void DeleteAccountSoftDeletesAccount()
//        {
//            _repository.Setup(r => r.DeleteAccount(It.IsAny<Guid>()))
//                .Callback((Guid id) =>
//                {
//                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
//                    if (account is not null)
//                    {
//                        account.Active = false;
//                    }
//                });

//            await _repository.Object.DeleteAccount(_idOne);
//            var active = (await _repository.Object.GetAccountDto(_idOne)).Active;
//            Assert.False(active);
//        }

//        [Fact]
//        public async void DeleteAccountIfAlreadySoftDeletedOrHasBalanceGreaterThanZeroThrowInvalidOperationException()
//        {
//            _repository.Setup(r => r.DeleteAccount(It.IsAny<Guid>()))
//                .Callback((Guid id) =>
//                {
//                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
//                    if (account is not null)
//                    {
//                        if (account.Active is false)
//                        {
//                            throw new InvalidOperationException("Account is already deactivated.");
//                        }
//                        if (account.Balance > 0)
//                        {
//                            throw new InvalidOperationException("Account still has a balance of greater than 0, please withdraw before deactivating account.");
//                        }
//                    }
//                });

//            var testDeletedAccountId = Guid.NewGuid();
//            var testDeletedAccount = new Account
//            {
//                Id = testDeletedAccountId,
//                Name = "test",
//                Balance = 0,
//                Active = false,
//            };
//            _mockDbSet.Object.Add(testDeletedAccount);

//            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.Object.DeleteAccount(testDeletedAccountId));

//            var testAccountWithBalanceId = Guid.NewGuid();
//            var testAccountWithBalance = new Account
//            {
//                Id = testAccountWithBalanceId,
//                Name = "test",
//                Balance = 100,
//                Active = true,
//            };
//            _mockDbSet.Object.Add(testAccountWithBalance);

//            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.Object.DeleteAccount(testAccountWithBalanceId));
//        }

//        [Fact]
//        public async void ReactivateAccountResetsActiveToTrue()
//        {
//            _repository.Setup(r => r.ReactivateAccount(It.IsAny<Guid>()))
//                .Callback((Guid id) =>
//                {
//                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
//                    if (account is not null)
//                    {
//                        account.Active = false;
//                        Assert.False(account.Active);
//                        account.Active = true;
//                    }
//                });

//            await _repository.Object.ReactivateAccount(_idOne);
//            var active = (await _repository.Object.GetAccountDto(_idOne)).Active;
//            Assert.True(active);
//        }

//        [Fact]
//        public async void ReactivateAccountIfAlreadyActiveThrowInvalidOperationException()
//        {
//            _repository.Setup(r => r.ReactivateAccount(It.IsAny<Guid>()))
//                .Callback((Guid id) =>
//                {
//                    var account = _mockDbSet.Object.Where((account) => account.Id == id).FirstOrDefault();
//                    if (account is not null && account.Active is true)
//                    {
//                        throw new InvalidOperationException("Account is already active.");
//                    }
//                });

//            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.Object.ReactivateAccount(_idOne));
//        }
//    }
//}
using Moq;
using Xunit;
using BankingWebApi.Models;
using BankingWebApi.Interfaces;
using BankingWebApi.DataTransformationObjects;
using BankingWebApi.Controllers;
using BankingWebApi.Clients;

namespace BankingWebApi.Tests
{
    public class TestAccountController
    {
        private Mock<IAccountsController> _accountsControllerMock;
        private Mock<IAccountRepository> _accountRepositoryMock;

        public TestAccountController()
        {
            _accountsControllerMock = new Mock<IAccountsController>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
        }

        [Fact]
        public async void GetAccountsReturnsListOfAccounts()
        {
            var mockAccountsController = new Mock<IAccountsController>();

            mockAccountsController.Setup(controller =>
                controller.GetAccounts(It.IsAny<AccountsFilter>()))
                .ReturnsAsync(new List<AccountDto>() { new Mock<AccountDto>().Object });

            var controller = mockAccountsController.Object;
            var output = await controller.GetAccounts(new AccountsFilter());

            mockAccountsController.Verify(controller => controller.GetAccounts(new AccountsFilter()), Times.AtMostOnce());
            Assert.IsAssignableFrom<List<AccountDto>>(output);
            Assert.Single(output);

            //var mockCurrencyClient = new Mock<ICurrencyClient>();
            //var mockConfig = new Mock<IConfiguration>();
            //var mockRepo = new Mock<IAccountRepository>();

            //mockRepo.Setup(repo => repo.GetAccounts(It.IsAny<AccountsFilter>()));

            //_accountsControllerMock = new Mock<IAccountsController>();
            //var viewResult = Assert.IsType<List<AccountDto>>(result);
            //Assert.IsAssignableFrom<List<AccountDto>>(result);

            //var accounts = _accountsControllerMock.Setup(controller =>
            //    controller.GetAccounts(It.IsAny<AccountsFilter>()));
            //.ReturnsAsync(new List<AccountDto>() { accountMock });
            //var viewResult = Assert.IsType<Task<List<AccountDto>>>(accounts);
            //var getAccounts = new GetAccounts(_accountsControllerMock.Object, _mapper);
            //var accounts = await _accountsControllerMock.GetAccounts(new AccountsFilter());
            //Assert.IsAssignableFrom<IEnumerable<Account>>(accounts);
        }
    }

    //private List<Account> GetTestAccounts()
    //{
    //    var testAccounts = new List<Account>();
    //    testAccounts.Add(new Account {  });
    //    testAccounts.Add(new Account {  });
    //    testAccounts.Add(new Account {  });
    //    testAccounts.Add(new Account {  });

    //    return testAccounts;
    //}
}

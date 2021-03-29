using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankingSolution.Api.Contracts.Request;
using BankingSolution.Api.Controllers;
using BankingSolution.Api.Services;
using BankingSolution.Logic.Implementation;
using BankingSolution.Logic.Interfaces;
using BankingSolution.Logic.Poco;
using BankingSolution.Logic.ValueTypes;
using BankingSolution.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace BankingSolution.UnitTest
{
    public class AccountTests
    {
        private readonly DbContextOptionsBuilder _dataContextOptions =
            new DbContextOptionsBuilder<DataContext>().UseInMemoryDatabase(databaseName: "BankAccounts");
        
        private Mock<ILogger<AccountService>> _mockAccountActionLogger = new Mock<ILogger<AccountService>>();
        private Mock<ILogger<AccountController>> _mockAccountControllerLogger = new Mock<ILogger<AccountController>>();
        
        [Test]
        public async Task Account_Creation_Invokes_Deposit()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();
            
            //Act
         
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);

            var createAccount = new CreateAccount("Arlef Kaba", Amount.FromInput(100m));

            AccountCreationSummary accountCreationSummary = await accountService.CreateAccount(createAccount);


            Account searchedAccount = await accountRepository.FindByIdAsync(accountCreationSummary.AccountId);

            //Assert
            Assert.That(accountCreationSummary, Is.Not.Null);
            Assert.That(accountCreationSummary.AccountId, Is.Not.Null);
            
            Assert.That(searchedAccount, Is.Not.Null);
        }
        
        [Test]
        public async Task Deposit_RatesApi_Is_Invoked_Only_If_Currency_Is_Not_Base_Currency()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();

            CancellationToken token = new CancellationToken();

            
            var mockRates = new Mock<IRatesService>();
            mockRates.Setup(a => a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Rate("GBP", "GBP", 0.79m, DateTime.Now));
            
            //Act
            RatesApiOption ratesApiOption = new RatesApiOption("someurl", "USD");
            
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);


            AccountCreationSummary accountCreationSummary = await accountService.CreateAccount(new CreateAccount("Jared", Amount.FromInput(100m)));

            AccountController accountController = new AccountController(_mockAccountControllerLogger.Object, accountService, mockRates.Object, ratesApiOption);

            CreateDepositRequest request = new CreateDepositRequest
                {AccountId = accountCreationSummary.AccountId, Amount = 30m, Currency = Currencies.EUR};


            var result = (await accountController.Deposit(request, token) as ObjectResult);
           

            Assert.That(accountCreationSummary, Is.Not.Null);
            Assert.That(accountCreationSummary.AccountId, Is.Not.Empty);
            Assert.That(result, Is.Not.Null);
            
            mockRates.Verify(a=>a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        }
        
        [Test] public async Task Withdrawal_Multiple_Times()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();
            
            var mockRates = new Mock<IRatesService>();
            mockRates.Setup(a => a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Rate("GBP", "GBP", 0.79m, DateTime.Now));
            
            //Act
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);
            
            
            var createAccount = new CreateAccount("Martin Fowler", Amount.FromInput(100m));
            
            decimal[] withdrawalAmount = new[] {30m, 50m, 10m};
            
            
            AccountCreationSummary accountCreationSummary = await accountService.CreateAccount(createAccount);

            foreach (decimal withdrawEntry in withdrawalAmount)
            {
                await accountService.WithDraw(new CreateWithdrawal(accountCreationSummary.AccountId,
                    Amount.FromInput(withdrawEntry)));
            }
            
            BalanceResult result = await accountService.GetBalance(new AccountInfo(accountCreationSummary.AccountId, Amount.None));
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Amount.Value, Is.EqualTo((createAccount.Amount.Value - withdrawalAmount.Sum())));
            Assert.That(result.Status, Is.EqualTo(AccountStatus.Success));

        }
        
        
        [Test]
        public async Task Withdrawal_Amount_Cannot_Leave_Account_In_Negative()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();

            CancellationToken token = new CancellationToken();

            
            var mockRates = new Mock<IRatesService>();
            mockRates.Setup(a => a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Rate("GBP", "GBP", 0.79m, DateTime.Now));
            
            //Act
            RatesApiOption ratesApiOption = new RatesApiOption("someurl", "USD");
            
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);


            AccountCreationSummary accountCreationSummary = await accountService.CreateAccount(new CreateAccount("Jared", Amount.FromInput(100m)));

            AccountController accountController = new AccountController(_mockAccountControllerLogger.Object, accountService, mockRates.Object, ratesApiOption);

            CreateWithdrawalRequest request = new CreateWithdrawalRequest
                {AccountId = accountCreationSummary.AccountId, Amount = 101m};

            var result = (await accountController.Withdraw(request, token) as ObjectResult);

            Assert.That(accountCreationSummary, Is.Not.Null);
            Assert.That(accountCreationSummary.AccountId, Is.Not.Empty);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Contains.Substring("amount is greater than balance"));
            

        }
        
         [Test]
        public async Task Withdrawal_Fails_If_Account_Id_Is_Invalid()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();

            CancellationToken token = new CancellationToken();

            string accountIdInvalid = "Invalid account id";

            
            var mockRates = new Mock<IRatesService>();
            mockRates.Setup(a => a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Rate("GBP", "GBP", 0.79m, DateTime.Now));
            
            //Act
            RatesApiOption ratesApiOption = new RatesApiOption("someurl", "USD");
            
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);


           
            AccountController accountController = new AccountController(_mockAccountControllerLogger.Object, accountService, mockRates.Object, ratesApiOption);

            CreateWithdrawalRequest request = new CreateWithdrawalRequest
                {AccountId = accountIdInvalid, Amount = 101m};

            var result = (await accountController.Withdraw(request, token) as ObjectResult);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Contains.Substring("Account id not be found"));
            

        }
        
        [Test] public async Task Deposit_Multiple_Times()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();
            
            var mockRates = new Mock<IRatesService>();
            mockRates.Setup(a => a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Rate("GBP", "GBP", 0.79m, DateTime.Now));
            
            //Act
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);
            
            
            var createAccount = new CreateAccount("Martin Fowler", Amount.FromInput(100m));
            
            decimal[] depositAmount = new[] {30m, 50m, 10m};
            
            
            AccountCreationSummary accountCreationSummary = await accountService.CreateAccount(createAccount);

            foreach (decimal depositEntry in depositAmount)
            {
                await accountService.Deposit(new CreateDeposit(accountCreationSummary.AccountId,
                    Amount.FromInput(depositEntry), Currency.FromInput(Currencies.GBP)));
            }
            
            BalanceResult result = await accountService.GetBalance(new AccountInfo(accountCreationSummary.AccountId, Amount.None));
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Amount.Value, Is.EqualTo((depositAmount.Sum() + createAccount.Amount.Value)));
            Assert.That(result.Status, Is.EqualTo(AccountStatus.Success));

        }
        

        [Test]
        public async Task Deposit_Fails_If_Account_Id_Is_Invalid()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();

            CancellationToken token = new CancellationToken();

            string accountIdInvalid = "Invalid account id";

            
            var mockRates = new Mock<IRatesService>();
            mockRates.Setup(a => a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Rate("GBP", "GBP", 0.79m, DateTime.Now));
            
            //Act
            RatesApiOption ratesApiOption = new RatesApiOption("someurl", "USD");
            
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);
           
            AccountController accountController = new AccountController(_mockAccountControllerLogger.Object, accountService, mockRates.Object, ratesApiOption);

            CreateDepositRequest request = new CreateDepositRequest
                {AccountId = accountIdInvalid, Amount = 101m, Currency = Currencies.EUR};

            var result = (await accountController.Deposit(request, token) as ObjectResult);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Contains.Substring("Account id not be found"));

        }
        [Test]
        public async Task Transaction_Valid_AccountId_Returns_Transactions()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();
            
            var mockRates = new Mock<IRatesService>();
            mockRates.Setup(a => a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Rate("GBP", "GBP", 0.79m, DateTime.Now));
            
            //Act
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);
            
            
            var createAccount = new CreateAccount("Robert C. Martin", Amount.FromInput(100m));
            
            decimal[] depositAmount = new[] {30m, 50m, 10m};
            
            AccountCreationSummary accountCreationSummary = await accountService.CreateAccount(createAccount);

            foreach (decimal depositEntry in depositAmount)
            {
                await accountService.Deposit(new CreateDeposit(accountCreationSummary.AccountId,
                    Amount.FromInput(depositEntry), Currency.FromInput(Currencies.GBP)));
            }
            
            BalanceResult balance = await accountService.GetBalance(new AccountInfo(accountCreationSummary.AccountId, Amount.None));

            TransactionResult result = await accountService.GetTransactions(accountCreationSummary.AccountId);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(AccountStatus.Success));

        }
        
        [Test]
        public async Task Transaction_Invalid_AccountId_Returns_No_Transactions()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();


            string accountIdInvalid = "Invalid account id";

            
            var mockRates = new Mock<IRatesService>();
            mockRates.Setup(a => a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Rate("GBP", "GBP", 0.79m, DateTime.Now));
            
            //Act
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);

            TransactionResult result = await accountService.GetTransactions(accountIdInvalid);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(AccountStatus.Error));

        }

        [Test] public async Task Balance_Valid_Account_Id_Returns_Balance()
        {
            //Arrange
            var dataContext = new DataContext(_dataContextOptions.Options);
            IRepository<Account> accountRepository = new AccountRepository(dataContext);
            IRepository<Transaction> transactionRepository = new TransactionRepository(dataContext);
            IUnitOfWork unitOfWork = new UnitOfWork(dataContext, accountRepository, transactionRepository);
            IGenerateId idGenerator =  new IdGenerator();
            
            var mockRates = new Mock<IRatesService>();
            mockRates.Setup(a => a.GetExchangeRate(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Rate("GBP", "GBP", 0.79m, DateTime.Now));
            
            //Act
            IAccountService accountService =
                new AccountService(_mockAccountActionLogger.Object, unitOfWork, idGenerator);
            
            
            var createAccount = new CreateAccount("Martin Fowler", Amount.FromInput(100m));

            AccountCreationSummary accountCreationSummary = await accountService.CreateAccount(createAccount);

            BalanceResult result = await accountService.GetBalance(new AccountInfo(accountCreationSummary.AccountId, Amount.None));
            

          
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Amount, Is.EqualTo(accountCreationSummary.Amount));
            Assert.That(result.Status, Is.EqualTo(AccountStatus.Success));

        }
        
    }
}